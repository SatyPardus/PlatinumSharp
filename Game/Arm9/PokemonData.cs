using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed partial class Pokemon
{
    public const int OTIDTypeZero = 0;
    public const int OTIDTypePreset = 1;
    public const int OTIDTypeNotShiny = 2;
    public const int RandomIVs = 0x20;
    public const int NatureCount = 25;
    public const int MaxMoves = 4;
    public const ushort MoveNoSlot = 0xFFFF;
    public const ushort MoveAlreadyKnown = 0xFFFE;

    private const int Language = 2;
    private const int VersionPlatinum = 12;
    private const int ItemPokeBall = 4;
    private const int ItemLuxuryBall = 11;
    private const int ItemHealBall = 14;
    private const int SpeciesShedinja = 292;
    private const int SpeciesShaymin = 492;
    private const int ShinyThreshold = 8;
    private const int UnownLetterCount = 28;
    private const int MaxLevel = 100;
    private const int GrowthRateCount = 8;
    private const int NarcGrowthTables = 3;
    private const int NarcLevelUpMoves = 33;
    private const int NarcBabySpecies = 0x6D;
    private const int LevelUpTableSize = 0x2C;
    private const ushort LevelUpTableEnd = 0xFFFF;
    private const int LevelUpLevelMask = 0xFE00;
    private const int LevelUpLevelShift = 9;
    private const int LevelUpMoveMask = 0x1FF;
    private const int HoldEffectFriendshipUp = 0x35;
    private const int FriendshipEventWalking = 5;
    private const int FriendshipTiers = 3;
    private const int FriendshipEventCount = 10;
    private const int StatCount = 5;
    private const uint NatureStatTableAddress = 0x020F0618;
    private const uint FriendshipTableAddress = 0x020F05A0;
    private const int PersonalBaseFriendship = 0x14;
    private const int PersonalGrowthRate = 0x15;
    private const int PersonalAbility1 = 0x18;
    private const int PersonalAbility2 = 0x19;
    private const int ParamPersonality = 0;
    private const int ShinyPersonalityBits = 13;
    private const uint BattleTypeNoWildItems = 0x81;
    private const uint HeldItemRates = 0x020F0574;
    private const int SpeciesParamItem1 = 0x10;
    private const int SpeciesParamItem2 = 0x11;
    private const int ParamSpecies = 5;
    private const int ParamHeldItem = 6;
    private const int ParamOTID = 7;
    private const int ParamExp = 8;
    private const int ParamFriendship = 9;
    private const int ParamAbility = 10;
    private const int ParamLanguage = 12;
    private const int ParamHPEV = 13;
    private const int ParamMove1 = 54;
    private const int ParamMove1PP = 58;
    private const int ParamMove1PPUps = 62;
    private const int ParamHPIV = 70;
    private const int ParamGender = 111;
    private const int ParamForm = 112;
    private const int ParamGameVersion = 122;
    private const int ParamOTName = 145;
    private const int ParamMetLocation = 152;
    private const int ParamPokeball = 155;
    private const int ParamMetLevel = 156;
    private const int ParamOTGender = 157;
    private const int ParamMetTerrain = 158;
    private const int ParamStatus = 160;
    private const int ParamLevel = 161;
    private const int ParamBallCapsuleID = 162;
    private const int ParamHP = 163;
    private const int ParamMaxHP = 164;
    private const int ParamAttack = 165;
    private const int ParamSpeciesEgg = 174;
    private const int ParamCombinedIVs = 175;
    private const int ParamSpeciesName = 179;

    private static readonly byte[] sGrowthTable = new byte[(MaxLevel + 1) * 4];
    private static byte[]? natureStatModifiers;
    private static byte[]? friendshipChanges;

    private static uint Number(object? value)
    {
        switch (value)
        {
            case uint number:
                return number;
            case int number:
                return (uint)number;
            case ushort number:
                return number;
            case short number:
                return (uint)number;
            case byte number:
                return number;
            case sbyte number:
                return (uint)number;
            case long number:
                return (uint)number;
            case ulong number:
                return (uint)number;
            case bool flag:
                return flag ? 1u : 0u;
            case Enum number:
                return Convert.ToUInt32(number);
        }
        throw new ArgumentException($"Pokemon value {value?.GetType().Name ?? "null"} is not a number");
    }

    private static uint RandomU32()
    {
        uint low = MathUtil.LCRNG_Next();
        return low | ((uint)MathUtil.LCRNG_Next() << 16);
    }

    private static void WriteChars(BufferView view, int position, ushort[] chars, int count)
    {
        for (int i = 0; i < count; i++)
        {
            view.W16(position + i * 2, chars[i]);
        }
    }

    // OFFSET 0x02073D80
    public static void Pokemon_InitWith(Pokemon mon, int species, int level, int ivs, int useParamPersonality, uint personality, int otIDType, uint otID)
    {
        Pokemon_Init(mon);
        BoxPokemon_InitWith(mon.Box, species, level, ivs, useParamPersonality, personality, otIDType, otID);
        Pokemon_EncryptData(mon, PartyOffset, PartySize, 0);
        Pokemon_EncryptData(mon, PartyOffset, PartySize, mon.U32(0));
        Pokemon_SetValue(mon, ParamLevel, level);
        Mail mail = Mail.Mail_New(0);
        Pokemon_SetValue(mon, PartyParamMail, mail);
        Pokemon_SetValue(mon, ParamBallCapsuleID, 0);
        Pokemon_SetValue(mon, PartyParamBallCapsule, new BallCapsule());
        Pokemon_CalcLevelAndStats(mon);
    }

    // OFFSET 0x02073E18
    public static void BoxPokemon_InitWith(BoxPokemon boxMon, int species, int level, int ivs, int useParamPersonality, uint personality, int otIDType, uint otID)
    {
        BoxPokemon_Init(boxMon);
        int entered = BoxPokemon_EnterDecryptionContext(boxMon);
        if (useParamPersonality == 0)
        {
            personality = RandomU32();
        }
        BoxPokemon_SetValue(boxMon, ParamPersonality, personality);
        if (otIDType == OTIDTypeNotShiny)
        {
            do
            {
                otID = RandomU32();
            }
            while (((otID >> 16) ^ (otID & 0xFFFF) ^ (personality >> 16) ^ (personality & 0xFFFF)) < ShinyThreshold);
        }
        else if (otIDType != OTIDTypePreset)
        {
            otID = 0;
        }
        BoxPokemon_SetValue(boxMon, ParamOTID, otID);
        BoxPokemon_SetValue(boxMon, ParamLanguage, Language);
        BoxPokemon_SetValue(boxMon, ParamSpecies, species);
        BoxPokemon_SetValue(boxMon, ParamSpeciesName, null);
        BoxPokemon_SetValue(boxMon, ParamExp, Species_GetExpForLevel(species, level));
        BoxPokemon_SetValue(boxMon, ParamFriendship, SpeciesData.SpeciesData_GetSpeciesValue(species, PersonalBaseFriendship));
        BoxPokemon_SetValue(boxMon, ParamMetLevel, level);
        BoxPokemon_SetValue(boxMon, ParamGameVersion, VersionPlatinum);
        BoxPokemon_SetValue(boxMon, ParamPokeball, ItemPokeBall);
        if (ivs < RandomIVs)
        {
            for (int i = 0; i < 6; i++)
            {
                BoxPokemon_SetValue(boxMon, ParamHPIV + i, ivs);
            }
        }
        else
        {
            uint random = MathUtil.LCRNG_Next();
            BoxPokemon_SetValue(boxMon, ParamHPIV, random & 0x1F);
            BoxPokemon_SetValue(boxMon, ParamHPIV + 1, (random & 0x3E0) >> 5);
            BoxPokemon_SetValue(boxMon, ParamHPIV + 2, (random & 0x7C00) >> 10);
            random = MathUtil.LCRNG_Next();
            BoxPokemon_SetValue(boxMon, ParamHPIV + 3, random & 0x1F);
            BoxPokemon_SetValue(boxMon, ParamHPIV + 4, (random & 0x3E0) >> 5);
            BoxPokemon_SetValue(boxMon, ParamHPIV + 5, (random & 0x7C00) >> 10);
        }
        uint ability1 = SpeciesData.SpeciesData_GetSpeciesValue(species, PersonalAbility1);
        uint ability2 = SpeciesData.SpeciesData_GetSpeciesValue(species, PersonalAbility2);
        if (ability2 != 0 && (personality & 1) != 0)
        {
            BoxPokemon_SetValue(boxMon, ParamAbility, ability2);
        }
        else
        {
            BoxPokemon_SetValue(boxMon, ParamAbility, ability1);
        }
        BoxPokemon_SetValue(boxMon, ParamGender, BoxPokemon_GetGender(boxMon));
        BoxPokemon_SetDefaultMoves(boxMon);
        BoxPokemon_ExitDecryptionContext(boxMon, entered);
    }

    // OFFSET 0x02074044
    public static void Pokemon_InitWithNature(Pokemon mon, int species, int level, int ivs, uint nature)
    {
        uint personality;
        do
        {
            personality = RandomU32();
        }
        while (nature != Personality_GetNature(personality));
        Pokemon_InitWith(mon, species, level, ivs, 1, personality, OTIDTypeZero, 0);
    }

    // OFFSET 0x02074088
    public static void Pokemon_InitWithGenderNatureLetter(Pokemon mon, int species, int level, int ivs, uint gender, uint nature, byte unownLetter)
    {
        uint personality;
        if (unownLetter != 0 && unownLetter < UnownLetterCount + 1)
        {
            int letter = unownLetter - 1;
            ushort personalityLetter;
            do
            {
                personality = RandomU32();
                personalityLetter = (ushort)((((personality & 0x03000000) >> 18) | ((personality & 0x00030000) >> 12) | ((personality & 0x00000300) >> 6) | (personality & 3)) % UnownLetterCount);
            }
            while (nature != Personality_GetNature(personality) || gender != SpeciesData.Species_GetGender((ushort)species, personality) || personalityLetter != letter);
        }
        else
        {
            personality = Species_GetPersonalityForGenderAndNature(species, gender, nature);
        }
        Pokemon_InitWith(mon, species, level, ivs, 1, personality, OTIDTypeZero, 0);
    }

    // OFFSET 0x02074128
    public static uint Species_GetPersonalityForGenderAndNature(int species, uint gender, uint nature)
    {
        byte ratio = (byte)SpeciesData.SpeciesData_GetSpeciesValue(species, SpeciesData.ParamGenderRatio);
        switch (ratio)
        {
            case 0:
            case 0xFE:
            case 0xFF:
                return nature;
        }
        if (gender == SpeciesData.GenderMale)
        {
            nature += (uint)(NatureCount * (ratio / NatureCount + 1));
        }
        return nature;
    }

    // OFFSET 0x02074158
    public static void Pokemon_InitWithCombinedIVs(Pokemon mon, int species, int level, uint combinedIVs, uint personality)
    {
        Pokemon_InitWith(mon, species, level, 0, 1, personality, OTIDTypeZero, 0);
        Pokemon_SetValue(mon, ParamCombinedIVs, combinedIVs);
        Pokemon_CalcLevelAndStats(mon);
    }

    // OFFSET 0x0207418C
    public static void Pokemon_CalcLevelAndStats(Pokemon mon)
    {
        int entered = Pokemon_EnterDecryptionContext(mon);
        Pokemon_SetValue(mon, ParamLevel, Pokemon_GetLevel(mon));
        Pokemon_CalcStats(mon);
        Pokemon_ExitDecryptionContext(mon, entered);
    }

    // OFFSET 0x020741B8
    public static void Pokemon_CalcStats(Pokemon mon)
    {
        int entered = Pokemon_EnterDecryptionContext(mon);
        int level = (int)Pokemon_GetValue(mon, ParamLevel, null);
        int oldMaxHP = (int)Pokemon_GetValue(mon, ParamMaxHP, null);
        int hp = (int)Pokemon_GetValue(mon, ParamHP, null);
        int[] ivs = new int[6];
        int[] evs = new int[6];
        for (int i = 0; i < 6; i++)
        {
            ivs[i] = (int)Pokemon_GetValue(mon, ParamHPIV + i, null);
            evs[i] = (int)Pokemon_GetValue(mon, ParamHPEV + i, null);
        }
        int form = (int)Pokemon_GetValue(mon, ParamForm, null);
        int species = (int)Pokemon_GetValue(mon, ParamSpecies, null);
        byte[] speciesData = Heap.Heap_Alloc(0, SpeciesData.Size)!;
        SpeciesData.SpeciesData_LoadForm(species, form, speciesData);
        int maxHP;
        if (species == SpeciesShedinja)
        {
            maxHP = 1;
        }
        else
        {
            maxHP = (speciesData[0] * 2 + ivs[0] + evs[0] / 4) * level / 100 + level + 10;
        }
        Pokemon_SetValue(mon, ParamMaxHP, maxHP);
        for (int stat = 1; stat <= StatCount; stat++)
        {
            int value = (speciesData[stat] * 2 + ivs[stat] + evs[stat] / 4) * level / 100 + 5;
            value = Nature_ModifyStat(Pokemon_GetNature(mon), (ushort)value, stat);
            Pokemon_SetValue(mon, ParamAttack + stat - 1, value);
        }
        Heap.Heap_Free(speciesData);
        if (hp != 0 || oldMaxHP == 0)
        {
            if (species == SpeciesShedinja)
            {
                hp = 1;
            }
            else if (hp == 0)
            {
                hp = maxHP;
            }
            else
            {
                hp += maxHP - oldMaxHP;
            }
        }
        if (hp != 0)
        {
            Pokemon_SetValue(mon, ParamHP, hp);
        }
        Pokemon_ExitDecryptionContext(mon, entered);
    }

    // OFFSET 0x02074B30
    public static void Pokemon_SetValue(Pokemon mon, int param, object? value)
    {
        if ((mon.U16(FlagsOffset) & FlagPartyDecrypted) == 0)
        {
            Pokemon_DecryptData(mon, PartyOffset, PartySize, mon.U32(0));
            Pokemon_DecryptData(mon, BlocksOffset, BlocksSize, mon.U16(ChecksumOffset));
            ushort checksum = Pokemon_GetDataChecksum(mon, BlocksOffset, BlocksSize);
            if (checksum != mon.U16(ChecksumOffset))
            {
                ErrorHandling.GF_AssertFail();
                mon.W16(FlagsOffset, mon.U16(FlagsOffset) | FlagInvalid);
                Pokemon_EncryptData(mon, BlocksOffset, BlocksSize, mon.U16(ChecksumOffset));
                return;
            }
        }
        Pokemon_SetValueInternal(mon, param, value);
        if ((mon.U16(FlagsOffset) & FlagPartyDecrypted) == 0)
        {
            Pokemon_EncryptData(mon, PartyOffset, PartySize, mon.U32(0));
            mon.W16(ChecksumOffset, Pokemon_GetDataChecksum(mon, BlocksOffset, BlocksSize));
            Pokemon_EncryptData(mon, BlocksOffset, BlocksSize, mon.U16(ChecksumOffset));
        }
    }

    // OFFSET 0x02074BC0
    private static void Pokemon_SetValueInternal(Pokemon mon, int param, object? value)
    {
        switch (param - FirstPartyParam)
        {
            case 0:
                mon.W32(PartyStatusOffset, Number(value));
                return;
            case 1:
                mon.W8(PartyLevelOffset, (byte)Number(value));
                return;
            case 2:
                mon.W8(PartyCapsuleOffset, (byte)Number(value));
                return;
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
                mon.W16(PartyHPOffset + (param - FirstPartyParam - 3) * 2, (ushort)Number(value));
                return;
            case PartyParamMail - FirstPartyParam:
                ((Mail)value!).WriteTo(mon.Buffer, mon.Offset + PartyMailOffset);
                return;
            case PartyParamBallCapsule - FirstPartyParam:
                ((BallCapsule)value!).WriteTo(mon.Buffer, mon.Offset + PartyBallCapsuleOffset);
                return;
        }
        BoxPokemon_SetValueInternal(mon.Box, param, value);
    }

    // OFFSET 0x02074C60
    public static void BoxPokemon_SetValue(BoxPokemon boxMon, int param, object? value)
    {
        if ((boxMon.U16(FlagsOffset) & FlagBoxDecrypted) == 0)
        {
            Pokemon_DecryptData(boxMon, BlocksOffset, BlocksSize, boxMon.U16(ChecksumOffset));
            ushort checksum = Pokemon_GetDataChecksum(boxMon, BlocksOffset, BlocksSize);
            if (checksum != boxMon.U16(ChecksumOffset))
            {
                ErrorHandling.GF_AssertFail();
                boxMon.W16(FlagsOffset, boxMon.U16(FlagsOffset) | FlagInvalid);
                Pokemon_EncryptData(boxMon, BlocksOffset, BlocksSize, boxMon.U16(ChecksumOffset));
                return;
            }
        }
        BoxPokemon_SetValueInternal(boxMon, param, value);
        if ((boxMon.U16(FlagsOffset) & FlagBoxDecrypted) == 0)
        {
            boxMon.W16(ChecksumOffset, Pokemon_GetDataChecksum(boxMon, BlocksOffset, BlocksSize));
            Pokemon_EncryptData(boxMon, BlocksOffset, BlocksSize, boxMon.U16(ChecksumOffset));
        }
    }

    // OFFSET 0x02074CD8
    private static void BoxPokemon_SetValueInternal(BoxPokemon boxMon, int param, object? value)
    {
        uint personality = boxMon.U32(0);
        int a = BoxPokemon_GetDataBlock(boxMon, personality, BlockA);
        int b = BoxPokemon_GetDataBlock(boxMon, personality, BlockB);
        int c = BoxPokemon_GetDataBlock(boxMon, personality, BlockC);
        int d = BoxPokemon_GetDataBlock(boxMon, personality, BlockD);
        if (param >= 9 && param <= 24)
        {
            boxMon.W8(a + 0xC + (param - 9), (byte)Number(value));
            return;
        }
        if (param >= 25 && param <= 53)
        {
            uint mask = 1u << (param - 25);
            if ((byte)Number(value) != 0)
            {
                boxMon.W32(a + 0x1C, boxMon.U32(a + 0x1C) | mask);
            }
            else
            {
                boxMon.W32(a + 0x1C, boxMon.U32(a + 0x1C) & ~mask);
            }
            return;
        }
        if (param >= 54 && param <= 57)
        {
            boxMon.W16(b + (param - 54) * 2, (ushort)Number(value));
            return;
        }
        if (param >= 58 && param <= 61)
        {
            boxMon.W8(b + 8 + (param - 58), (byte)Number(value));
            return;
        }
        if (param >= 62 && param <= 65)
        {
            boxMon.W8(b + 0xC + (param - 62), (byte)Number(value));
            return;
        }
        if (param >= 70 && param <= 75)
        {
            int shift = (param - 70) * 5;
            boxMon.W32(b + 0x10, (boxMon.U32(b + 0x10) & ~(0x1Fu << shift)) | ((Number(value) & 0x1F) << shift));
            return;
        }
        if (param >= 78 && param <= 109)
        {
            uint mask = 1u << (param - 78);
            if ((byte)Number(value) != 0)
            {
                boxMon.W32(b + 0x14, boxMon.U32(b + 0x14) | mask);
            }
            else
            {
                boxMon.W32(b + 0x14, boxMon.U32(b + 0x14) & ~mask);
            }
            return;
        }
        if (param >= 123 && param <= 143)
        {
            ulong mask = 1UL << (param - 123);
            ulong flags = boxMon.U32(c + 0x18) | ((ulong)boxMon.U32(c + 0x1C) << 32);
            if ((byte)Number(value) != 0)
            {
                flags |= mask;
            }
            else
            {
                flags &= ~mask;
            }
            boxMon.W32(c + 0x18, (uint)flags);
            boxMon.W32(c + 0x1C, (uint)(flags >> 32));
            return;
        }
        if (param >= 146 && param <= 151)
        {
            boxMon.W8(d + 0x10 + (param - 146), (byte)Number(value));
            return;
        }
        switch (param)
        {
            case 0:
                boxMon.W32(0, Number(value));
                break;
            case 1:
                ErrorHandling.GF_AssertFail();
                boxMon.W16(FlagsOffset, (boxMon.U16(FlagsOffset) & ~FlagPartyDecrypted) | (int)(Number(value) & 1));
                break;
            case 2:
                ErrorHandling.GF_AssertFail();
                boxMon.W16(FlagsOffset, (boxMon.U16(FlagsOffset) & ~FlagBoxDecrypted) | (int)((Number(value) & 1) << 1));
                break;
            case 3:
                boxMon.W16(FlagsOffset, (boxMon.U16(FlagsOffset) & ~FlagInvalid) | (int)((Number(value) & 1) << 2));
                break;
            case 4:
                boxMon.W16(ChecksumOffset, (ushort)Number(value));
                break;
            case 5:
                boxMon.W16(a, (ushort)Number(value));
                break;
            case 6:
                boxMon.W16(a + 2, (ushort)Number(value));
                break;
            case 7:
                boxMon.W32(a + 4, Number(value));
                break;
            case 8:
                boxMon.W32(a + 8, Number(value));
                break;
            case 76:
                boxMon.W32(b + 0x10, (boxMon.U32(b + 0x10) & 0xBFFFFFFF) | ((Number(value) & 1) << 30));
                break;
            case 77:
                boxMon.W32(b + 0x10, (boxMon.U32(b + 0x10) & 0x7FFFFFFF) | ((Number(value) & 1) << 31));
                break;
            case 110:
                boxMon.W8(b + 0x18, (boxMon.U8(b + 0x18) & ~1) | (int)(Number(value) & 1));
                break;
            case 111:
            {
                uint gender = SpeciesData.Species_GetGender(boxMon.U16(a), personality);
                boxMon.W8(b + 0x18, (boxMon.U8(b + 0x18) & ~6) | (int)((gender & 3) << 1));
                break;
            }
            case 112:
                boxMon.W8(b + 0x18, (boxMon.U8(b + 0x18) & ~0xF8) | (int)((Number(value) & 0x1F) << 3));
                break;
            case 113:
                boxMon.W8(b + 0x19, (byte)Number(value));
                break;
            case 114:
                boxMon.W16(b + 0x1A, (ushort)Number(value));
                break;
            case 115:
            case 152:
            {
                ushort location = (ushort)Number(value);
                if (location == 0 || SpecialMetLoc.SpecialMetLoc_IsValid(location))
                {
                    boxMon.W16(d + 0x16, location);
                }
                else
                {
                    boxMon.W16(d + 0x16, SpecialMetLocation);
                }
                boxMon.W16(b + 0x1C, location);
                break;
            }
            case 116:
            case 153:
            {
                ushort location = (ushort)Number(value);
                if (location == 0 || SpecialMetLoc.SpecialMetLoc_IsValid(location))
                {
                    boxMon.W16(d + 0x18, location);
                }
                else
                {
                    boxMon.W16(d + 0x18, SpecialMetLocation);
                }
                boxMon.W16(b + 0x1E, location);
                break;
            }
            case 117:
            case 118:
            {
                ushort[] chars = (ushort[])value!;
                if (param == 118)
                {
                    ushort[] speciesName = new ushort[NicknameLength + 1];
                    MessageLoader.MessageLoader_GetSpeciesName(boxMon.U16(a), 0, speciesName);
                    uint differs = (uint)CharCode.CharCode_Compare(speciesName, 0, chars, 0);
                    boxMon.W32(b + 0x10, (boxMon.U32(b + 0x10) & 0x7FFFFFFF) | (differs << 31));
                }
                WriteChars(boxMon, c, chars, NicknameLength + 1);
                break;
            }
            case 119:
            case 120:
            {
                ushort[] chars = new ushort[NicknameLength + 1];
                if (param == 120)
                {
                    ushort[] speciesName = new ushort[NicknameLength + 1];
                    MessageLoader.MessageLoader_GetSpeciesName(boxMon.U16(a), 0, speciesName);
                    GameString.String_ToChars((GameString?)value, chars, NicknameLength + 1);
                    uint differs = (uint)CharCode.CharCode_Compare(speciesName, 0, chars, 0);
                    boxMon.W32(b + 0x10, (boxMon.U32(b + 0x10) & 0x7FFFFFFF) | (differs << 31));
                }
                GameString.String_ToChars((GameString?)value, chars, NicknameLength + 1);
                WriteChars(boxMon, c, chars, NicknameLength + 1);
                break;
            }
            case 121:
                boxMon.W8(c + 0x16, (byte)Number(value));
                break;
            case 122:
                boxMon.W8(c + 0x17, (byte)Number(value));
                break;
            case 144:
                WriteChars(boxMon, d, (ushort[])value!, OTNameLength + 1);
                break;
            case 145:
            {
                ushort[] chars = new ushort[OTNameLength + 1];
                GameString.String_ToChars((GameString?)value, chars, OTNameLength + 1);
                WriteChars(boxMon, d, chars, OTNameLength + 1);
                break;
            }
            case 154:
                boxMon.W8(d + 0x1A, (byte)Number(value));
                break;
            case 155:
                boxMon.W8(d + 0x1B, (byte)Number(value));
                break;
            case 156:
                boxMon.W8(d + 0x1C, (boxMon.U8(d + 0x1C) & ~0x7F) | (int)(Number(value) & 0x7F));
                break;
            case 157:
                boxMon.W8(d + 0x1C, (boxMon.U8(d + 0x1C) & ~0x80) | (int)((Number(value) & 1) << 7));
                break;
            case 158:
                boxMon.W8(d + 0x1D, (byte)Number(value));
                break;
            case 159:
                boxMon.W16(d + 0x1E, (ushort)Number(value));
                break;
            case 175:
            {
                uint combined = Number(value);
                uint ivs = boxMon.U32(b + 0x10);
                ivs = (ivs & ~0x1Fu) | (combined & 0x1F);
                ivs = (ivs & 0xFFFFFC1F) | (((combined >> 5) & 0x1F) << 5);
                ivs = (ivs & 0xFFFF83FF) | (((combined >> 10) & 0x1F) << 10);
                ivs = (ivs & 0xFFF07FFF) | (((combined >> 15) & 0x1F) << 15);
                ivs = (ivs & 0xFE0FFFFF) | (((combined >> 20) & 0x1F) << 20);
                ivs = (ivs & 0xC1FFFFFF) | (((combined >> 25) & 0x1F) << 25);
                boxMon.W32(b + 0x10, ivs);
                break;
            }
            case 179:
            {
                GameString? speciesName = MessageUtil.MessageUtil_SpeciesName(boxMon.U16(a), 0);
                ushort[] chars = new ushort[NicknameLength + 1];
                GameString.String_ToChars(speciesName, chars, NicknameLength + 1);
                WriteChars(boxMon, c, chars, NicknameLength + 1);
                GameString.String_Free(speciesName);
                break;
            }
        }
    }

    // OFFSET 0x02075AD0
    public static uint Species_GetExpForLevel(int species, int level)
    {
        return GrowthRate_GetExpForLevel((int)SpeciesData.SpeciesData_GetSpeciesValue(species, PersonalGrowthRate), level);
    }

    // OFFSET 0x02075AE4
    public static void GrowthRate_LoadTable(int growthRate, byte[] table)
    {
        if (growthRate >= GrowthRateCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        NARC.NARC_ReadWholeMemberByIndexPair(table, NarcGrowthTables, growthRate);
    }

    // OFFSET 0x02075B00
    public static uint GrowthRate_GetExpForLevel(int growthRate, int level)
    {
        if (growthRate >= GrowthRateCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (level > MaxLevel + 1)
        {
            ErrorHandling.GF_AssertFail();
        }
        byte[] table = Heap.Heap_Alloc(0, (MaxLevel + 1) * 4)!;
        GrowthRate_LoadTable(growthRate, table);
        uint exp = Bin.U32(table, level * 4);
        Heap.Heap_Free(table);
        return exp;
    }

    // OFFSET 0x02075B38
    public static uint Pokemon_GetLevel(Pokemon mon)
    {
        return BoxPokemon_GetLevel(mon.Box);
    }

    // OFFSET 0x02075B40
    public static uint BoxPokemon_GetLevel(BoxPokemon boxMon)
    {
        int entered = BoxPokemon_EnterDecryptionContext(boxMon);
        uint species = BoxPokemon_GetValue(boxMon, ParamSpecies, null);
        uint exp = BoxPokemon_GetValue(boxMon, ParamExp, null);
        BoxPokemon_ExitDecryptionContext(boxMon, entered);
        return Species_GetLevelFromExp((ushort)species, exp);
    }

    // OFFSET 0x02075B78
    public static uint Species_GetLevelFromExp(ushort species, uint exp)
    {
        byte[] speciesData = SpeciesData.SpeciesData_FromMonSpecies(species, 0);
        uint level = SpeciesData_GetLevelFromExp(speciesData, species, exp);
        SpeciesData.SpeciesData_Free(speciesData);
        return level;
    }

    // OFFSET 0x02075B9C
    public static uint SpeciesData_GetLevelFromExp(byte[] speciesData, ushort species, uint exp)
    {
        GrowthRate_LoadTable((int)SpeciesData.SpeciesData_GetValue(speciesData, PersonalGrowthRate), sGrowthTable);
        int level;
        for (level = 1; level <= MaxLevel; level++)
        {
            if (Bin.U32(sGrowthTable, level * 4) > exp)
            {
                break;
            }
        }
        return (uint)(level - 1);
    }

    // OFFSET 0x02075BCC
    public static byte Pokemon_GetNature(Pokemon mon)
    {
        return BoxPokemon_GetNature(mon.Box);
    }

    // OFFSET 0x02075BD4
    public static byte BoxPokemon_GetNature(BoxPokemon boxMon)
    {
        int entered = BoxPokemon_EnterDecryptionContext(boxMon);
        uint personality = BoxPokemon_GetValue(boxMon, ParamPersonality, null);
        BoxPokemon_ExitDecryptionContext(boxMon, entered);
        return Personality_GetNature(personality);
    }

    // OFFSET 0x02075BFC
    public static byte Personality_GetNature(uint personality)
    {
        return (byte)(personality % NatureCount);
    }

    // OFFSET 0x02075C0C
    public static ushort Nature_ModifyStat(byte nature, ushort value, int stat)
    {
        if (stat < 1 || stat > StatCount)
        {
            return value;
        }
        natureStatModifiers ??= RomData.Arm9(NatureStatTableAddress, NatureCount * StatCount);
        switch ((sbyte)natureStatModifiers[nature * StatCount + stat - 1])
        {
            case 1:
                value = (ushort)((ushort)(value * 110) / 100);
                break;
            case -1:
                value = (ushort)((ushort)(value * 90) / 100);
                break;
        }
        return value;
    }

    // OFFSET 0x02075C60
    public static sbyte Nature_GetStatModifier(byte nature, int stat)
    {
        natureStatModifiers ??= RomData.Arm9(NatureStatTableAddress, NatureCount * StatCount);
        return (sbyte)natureStatModifiers[nature * StatCount + stat - 1];
    }

    // OFFSET 0x02075C74
    public static void Pokemon_UpdateFriendship(Pokemon mon, int friendshipEvent, ushort location)
    {
        if (friendshipEvent == FriendshipEventWalking && (MathUtil.LCRNG_Next() & 1) != 0)
        {
            return;
        }
        ushort species = (ushort)Pokemon_GetValue(mon, ParamSpeciesEgg, null);
        if (species == 0 || species == SpeciesEgg)
        {
            return;
        }
        byte holdEffect = (byte)Item.Item_LoadParam((ushort)Pokemon_GetValue(mon, ParamHeldItem, null), ItemParamHoldEffect, 0);
        byte tier = 0;
        short friendship = (short)Pokemon_GetValue(mon, ParamFriendship, null);
        if (friendship >= 100)
        {
            tier++;
        }
        if (friendship >= 200)
        {
            tier++;
        }
        friendshipChanges ??= RomData.Arm9(FriendshipTableAddress, FriendshipEventCount * FriendshipTiers);
        sbyte change = (sbyte)friendshipChanges[friendshipEvent * FriendshipTiers + tier];
        if (change > 0 && Pokemon_GetValue(mon, ParamPokeball, null) == ItemLuxuryBall)
        {
            change++;
        }
        if (change > 0 && Pokemon_GetValue(mon, ParamMetLocation, null) == location)
        {
            change++;
        }
        if (change > 0 && holdEffect == HoldEffectFriendshipUp)
        {
            change = (sbyte)(change * 150 / 100);
        }
        friendship = (short)(friendship + change);
        if (friendship < 0)
        {
            friendship = 0;
        }
        if (friendship > 255)
        {
            friendship = 255;
        }
        Pokemon_SetValue(mon, ParamFriendship, friendship);
    }

    // OFFSET 0x02075D6C
    public static byte Pokemon_GetGender(Pokemon mon)
    {
        return BoxPokemon_GetGender(mon.Box);
    }

    // OFFSET 0x02075E0C
    public static byte Pokemon_IsShiny(Pokemon mon)
    {
        return BoxPokemon_IsShiny(mon.Box);
    }

    // OFFSET 0x02075E14
    public static byte BoxPokemon_IsShiny(BoxPokemon boxMon)
    {
        uint otID = BoxPokemon_GetValue(boxMon, ParamOTID, null);
        uint personality = BoxPokemon_GetValue(boxMon, ParamPersonality, null);
        return Pokemon_IsPersonalityShiny(otID, personality);
    }

    // OFFSET 0x02075E38
    public static byte Pokemon_IsPersonalityShiny(uint otID, uint personality)
    {
        uint value = ((otID & 0xFFFF0000) >> 16) ^ (otID & 0xFFFF) ^ ((personality & 0xFFFF0000) >> 16) ^ (personality & 0xFFFF);
        return (byte)(value < ShinyThreshold ? 1 : 0);
    }

    // OFFSET 0x02075E64
    public static uint Pokemon_FindShinyPersonality(uint otID)
    {
        uint shinyValue = (((otID & 0xFFFF0000) >> 16) ^ (otID & 0xFFFF)) >> 3;
        ushort low = (ushort)(MathUtil.LCRNG_Next() & 7);
        ushort high = (ushort)(MathUtil.LCRNG_Next() & 7);
        for (int i = 0; i < ShinyPersonalityBits; i++)
        {
            if ((HealthBar.FlagIndex(i) & shinyValue) != 0)
            {
                if ((MathUtil.LCRNG_Next() & 1) != 0)
                {
                    low = (ushort)(low | HealthBar.FlagIndex(i + 3));
                }
                else
                {
                    high = (ushort)(high | HealthBar.FlagIndex(i + 3));
                }
            }
            else if ((MathUtil.LCRNG_Next() & 1) != 0)
            {
                low = (ushort)(low | HealthBar.FlagIndex(i + 3));
                high = (ushort)(high | HealthBar.FlagIndex(i + 3));
            }
        }
        return ((uint)high << 16) | low;
    }

    // OFFSET 0x02075D74
    public static byte BoxPokemon_GetGender(BoxPokemon boxMon)
    {
        int entered = BoxPokemon_EnterDecryptionContext(boxMon);
        ushort species = (ushort)BoxPokemon_GetValue(boxMon, ParamSpecies, null);
        uint personality = BoxPokemon_GetValue(boxMon, ParamPersonality, null);
        BoxPokemon_ExitDecryptionContext(boxMon, entered);
        return (byte)SpeciesData.Species_GetGender(species, personality);
    }

    // OFFSET 0x02076AF8
    public static byte Pokemon_GetForm(Pokemon mon)
    {
        return BoxPokemon_GetForm(mon.Box);
    }

    // OFFSET 0x02076B00
    public static byte BoxPokemon_GetForm(BoxPokemon boxMon)
    {
        return (byte)BoxPokemon_GetValue(boxMon, ParamForm, null);
    }

    // OFFSET 0x02076B10
    public static BoxPokemon Pokemon_GetBoxPokemon(Pokemon mon)
    {
        return mon.Box;
    }

    // OFFSET 0x02077020
    public static void BoxPokemon_SetDefaultMoves(BoxPokemon boxMon)
    {
        byte[] levelUpMoves = Heap.Heap_Alloc(0, LevelUpTableSize)!;
        int entered = BoxPokemon_EnterDecryptionContext(boxMon);
        ushort species = (ushort)BoxPokemon_GetValue(boxMon, ParamSpecies, null);
        int form = (int)BoxPokemon_GetValue(boxMon, ParamForm, null);
        byte level = (byte)BoxPokemon_GetLevel(boxMon);
        Species_LoadLevelUpMoves(species, form, levelUpMoves);
        for (int i = 0; Bin.U16(levelUpMoves, i * 2) != LevelUpTableEnd; i++)
        {
            ushort entry = (ushort)Bin.U16(levelUpMoves, i * 2);
            if ((entry & LevelUpLevelMask) > level << LevelUpLevelShift)
            {
                break;
            }
            ushort move = (ushort)(entry & LevelUpMoveMask);
            if (BoxPokemon_AddMove(boxMon, move) == MoveNoSlot)
            {
                BoxPokemon_ReplaceOldestMove(boxMon, move);
            }
        }
        Heap.Heap_Free(levelUpMoves);
        BoxPokemon_ExitDecryptionContext(boxMon, entered);
    }

    // OFFSET 0x020770C4
    public static ushort Pokemon_AddMove(Pokemon mon, ushort move)
    {
        return BoxPokemon_AddMove(Pokemon_GetBoxPokemon(mon), move);
    }

    // OFFSET 0x020770D4
    public static ushort BoxPokemon_AddMove(BoxPokemon boxMon, ushort move)
    {
        ushort result = MoveNoSlot;
        int entered = BoxPokemon_EnterDecryptionContext(boxMon);
        for (int slot = 0; slot < MaxMoves; slot++)
        {
            ushort slotMove = (ushort)BoxPokemon_GetValue(boxMon, ParamMove1 + slot, null);
            if (slotMove == 0)
            {
                BoxPokemon_SetMoveSlot(boxMon, move, (byte)slot);
                result = move;
                break;
            }
            if (slotMove == move)
            {
                result = MoveAlreadyKnown;
                break;
            }
        }
        BoxPokemon_ExitDecryptionContext(boxMon, entered);
        return result;
    }

    // OFFSET 0x02077134
    public static void Pokemon_ReplaceOldestMove(Pokemon mon, ushort move)
    {
        BoxPokemon_ReplaceOldestMove(Pokemon_GetBoxPokemon(mon), move);
    }

    // OFFSET 0x02077144
    public static void BoxPokemon_ReplaceOldestMove(BoxPokemon boxMon, ushort move)
    {
        int entered = BoxPokemon_EnterDecryptionContext(boxMon);
        ushort[] moves = new ushort[MaxMoves];
        byte[] pp = new byte[MaxMoves];
        byte[] ppUps = new byte[MaxMoves];
        for (int i = 0; i < MaxMoves - 1; i++)
        {
            moves[i] = (ushort)BoxPokemon_GetValue(boxMon, ParamMove1 + 1 + i, null);
            pp[i] = (byte)BoxPokemon_GetValue(boxMon, ParamMove1PP + 1 + i, null);
            ppUps[i] = (byte)BoxPokemon_GetValue(boxMon, ParamMove1PPUps + 1 + i, null);
        }
        moves[MaxMoves - 1] = move;
        pp[MaxMoves - 1] = (byte)MoveTable.MoveTable_LoadParam(move, MoveTable.ParamPP);
        ppUps[MaxMoves - 1] = 0;
        for (int i = 0; i < MaxMoves; i++)
        {
            BoxPokemon_SetValue(boxMon, ParamMove1 + i, moves[i]);
            BoxPokemon_SetValue(boxMon, ParamMove1PP + i, pp[i]);
            BoxPokemon_SetValue(boxMon, ParamMove1PPUps + i, ppUps[i]);
        }
        BoxPokemon_ExitDecryptionContext(boxMon, entered);
    }

    // OFFSET 0x020771F8
    public static void Pokemon_ResetMoveSlot(Pokemon mon, ushort move, byte slot)
    {
        Pokemon_SetMoveSlot(mon, move, slot);
        Pokemon_SetValue(mon, ParamMove1PPUps + slot, 0);
        Pokemon_SetValue(mon, ParamMove1PP + slot, MoveTable.MoveTable_CalcMaxPP(move, 0));
    }

    // OFFSET 0x02077230
    public static void Pokemon_SetMoveSlot(Pokemon mon, ushort move, byte slot)
    {
        BoxPokemon_SetMoveSlot(mon.Box, move, slot);
    }

    // OFFSET 0x02077238
    public static void BoxPokemon_SetMoveSlot(BoxPokemon boxMon, ushort move, byte slot)
    {
        BoxPokemon_SetValue(boxMon, ParamMove1 + slot, move);
        byte ppUps = (byte)BoxPokemon_GetValue(boxMon, ParamMove1PPUps + slot, null);
        byte pp = (byte)MoveTable.MoveTable_CalcMaxPP(move, ppUps);
        BoxPokemon_SetValue(boxMon, ParamMove1PP + slot, pp);
    }

    // OFFSET 0x02077F0C
    public static void Pokemon_GiveHeldItem(Pokemon mon, uint battleType, int itemRates)
    {
        if ((battleType & BattleTypeNoWildItems) != 0)
        {
            return;
        }
        int random = MathUtil.LCRNG_Next() % 100;
        ushort species = (ushort)Pokemon_GetValue(mon, ParamSpecies, null);
        ushort form = (ushort)Pokemon_GetValue(mon, ParamForm, null);
        ushort item1 = (ushort)SpeciesData.SpeciesData_GetFormValue(species, form, SpeciesParamItem1);
        ushort item2 = (ushort)SpeciesData.SpeciesData_GetFormValue(species, form, SpeciesParamItem2);
        if (item1 == item2 && item1 != 0)
        {
            Pokemon_SetValue(mon, ParamHeldItem, item1);
            return;
        }
        ushort[] rates = RomData.Arm9U16((uint)(HeldItemRates + itemRates * 4), 2);
        if (random < rates[0])
        {
            return;
        }
        if (random < rates[1])
        {
            Pokemon_SetValue(mon, ParamHeldItem, item1);
            return;
        }
        Pokemon_SetValue(mon, ParamHeldItem, item2);
    }

    // OFFSET 0x02077344
    public static void Pokemon_SwapMoveSlots(Pokemon mon, int slot1, int slot2)
    {
        BoxPokemon_SwapMoveSlots(mon.Box, slot1, slot2);
    }

    // OFFSET 0x0207734C
    public static void BoxPokemon_SwapMoveSlots(BoxPokemon boxMon, int slot1, int slot2)
    {
        ushort move1 = (ushort)BoxPokemon_GetValue(boxMon, ParamMove1 + slot1, null);
        byte pp1 = (byte)BoxPokemon_GetValue(boxMon, ParamMove1PP + slot1, null);
        byte ppUps1 = (byte)BoxPokemon_GetValue(boxMon, ParamMove1PPUps + slot1, null);
        ushort move2 = (ushort)BoxPokemon_GetValue(boxMon, ParamMove1 + slot2, null);
        byte pp2 = (byte)BoxPokemon_GetValue(boxMon, ParamMove1PP + slot2, null);
        byte ppUps2 = (byte)BoxPokemon_GetValue(boxMon, ParamMove1PPUps + slot2, null);
        BoxPokemon_SetValue(boxMon, ParamMove1 + slot1, move2);
        BoxPokemon_SetValue(boxMon, ParamMove1PP + slot1, pp2);
        BoxPokemon_SetValue(boxMon, ParamMove1PPUps + slot1, ppUps2);
        BoxPokemon_SetValue(boxMon, ParamMove1 + slot2, move1);
        BoxPokemon_SetValue(boxMon, ParamMove1PP + slot2, pp1);
        BoxPokemon_SetValue(boxMon, ParamMove1PPUps + slot2, ppUps1);
    }

    // OFFSET 0x020774C8
    public static void Pokemon_FromBoxPokemon(BoxPokemon boxMon, Pokemon mon)
    {
        mon.CopyFrom(0, boxMon.Buffer, boxMon.Offset, BoxPokemon.Size);
        if ((mon.U16(FlagsOffset) & FlagBoxDecrypted) != 0)
        {
            mon.W16(FlagsOffset, (mon.U16(FlagsOffset) & ~FlagPartyDecrypted) | FlagPartyDecrypted);
        }
        Pokemon_SetValue(mon, ParamStatus, 0);
        Pokemon_SetValue(mon, ParamHP, 0);
        Pokemon_SetValue(mon, ParamMaxHP, 0);
        Mail mail = Mail.Mail_New(0);
        Pokemon_SetValue(mon, PartyParamMail, mail);
        Pokemon_SetValue(mon, ParamBallCapsuleID, 0);
        Pokemon_SetValue(mon, PartyParamBallCapsule, new BallCapsule());
        Pokemon_CalcLevelAndStats(mon);
    }

    // OFFSET 0x020775A4
    public static ushort Species_GetBabySpecies(ushort species)
    {
        byte[] babySpecies = new byte[2];
        NARC.NARC_ReadFromMemberByIndexPair(babySpecies, NarcBabySpecies, 0, species * 2, 2);
        return (ushort)Bin.U16(babySpecies, 0);
    }

    // OFFSET 0x02077988
    public static uint Pokemon_GetArceusTypeOf(ushort holdEffect)
    {
        switch (holdEffect)
        {
            case 126:
                return 10;
            case 127:
                return 11;
            case 128:
                return 13;
            case 129:
                return 12;
            case 130:
                return 15;
            case 131:
                return 1;
            case 132:
                return 3;
            case 133:
                return 4;
            case 134:
                return 2;
            case 135:
                return 14;
            case 136:
                return 6;
            case 137:
                return 5;
            case 138:
                return 7;
            case 139:
                return 16;
            case 140:
                return 17;
            case 141:
                return 8;
        }
        return 0;
    }

    // OFFSET 0x02077ACC
    public static void Pokemon_SetShayminForm(Pokemon mon, int form)
    {
        BoxPokemon_SetShayminForm(mon.Box, form);
        Pokemon_CalcLevelAndStats(mon);
    }

    // OFFSET 0x02077ADC
    public static void BoxPokemon_SetShayminForm(BoxPokemon boxMon, int form)
    {
        if (BoxPokemon_GetValue(boxMon, ParamSpecies, null) == SpeciesShaymin)
        {
            if (form > 1)
            {
                ErrorHandling.GF_AssertFail();
            }
            BoxPokemon_SetValue(boxMon, ParamForm, form);
            BoxPokemon_CalcAbility(boxMon);
        }
    }

    // OFFSET 0x02077D28
    public static void Species_LoadLevelUpMoves(int species, int form, byte[] levelUpMoves)
    {
        NARC.NARC_ReadWholeMemberByIndexPair(levelUpMoves, NarcLevelUpMoves, SpeciesData.Species_GetFormDataIndex(species, form));
    }

    // OFFSET 0x02077E64
    public static void Pokemon_SetCatchData(Pokemon mon, TrainerInfo trainerInfo, int pokeball, int metLocation, int metTerrain, int heapID)
    {
        BoxPokemon_SetCatchData(mon.Box, trainerInfo, pokeball, metLocation, metTerrain, heapID);
        if (pokeball == ItemHealBall)
        {
            Pokemon_SetValue(mon, ParamHP, Pokemon_GetValue(mon, ParamMaxHP, null));
            Pokemon_SetValue(mon, ParamStatus, 0);
        }
    }

    // OFFSET 0x02077EA4
    public static void BoxPokemon_SetCatchData(BoxPokemon boxMon, TrainerInfo trainerInfo, int pokeball, int metLocation, int metTerrain, int heapID)
    {
        PokemonMetInfo.BoxPokemon_UpdateMetInfo(boxMon, trainerInfo, PokemonMetInfo.MethodCaught, metLocation, heapID);
        BoxPokemon_SetValue(boxMon, ParamGameVersion, VersionPlatinum);
        BoxPokemon_SetValue(boxMon, ParamPokeball, pokeball);
        BoxPokemon_SetValue(boxMon, ParamMetTerrain, metTerrain);
    }

    // OFFSET 0x02078044
    public static void BoxPokemon_CalcAbility(BoxPokemon boxMon)
    {
        int entered = BoxPokemon_EnterDecryptionContext(boxMon);
        int species = (int)BoxPokemon_GetValue(boxMon, ParamSpecies, null);
        uint personality = BoxPokemon_GetValue(boxMon, ParamPersonality, null);
        int form = (int)BoxPokemon_GetValue(boxMon, ParamForm, null);
        uint ability1 = SpeciesData.SpeciesData_GetFormValue(species, form, PersonalAbility1);
        uint ability2 = SpeciesData.SpeciesData_GetFormValue(species, form, PersonalAbility2);
        if (ability2 != 0 && (personality & 1) != 0)
        {
            BoxPokemon_SetValue(boxMon, ParamAbility, ability2);
        }
        else
        {
            BoxPokemon_SetValue(boxMon, ParamAbility, ability1);
        }
        BoxPokemon_ExitDecryptionContext(boxMon, entered);
    }

    // OFFSET 0x0207884C
    public static int BoxPokemon_BelongsToTrainer(BoxPokemon boxMon, TrainerInfo trainerInfo, int heapID)
    {
        uint trainerID = TrainerInfo.TrainerInfo_ID(trainerInfo);
        uint otID = BoxPokemon_GetValue(boxMon, ParamOTID, null);
        uint trainerGender = TrainerInfo.TrainerInfo_Gender(trainerInfo);
        uint otGender = BoxPokemon_GetValue(boxMon, ParamOTGender, null);
        GameString trainerName = TrainerInfo.TrainerInfo_NameNewString(trainerInfo, heapID);
        GameString otName = GameString.String_Init(OTNameLength + 1, heapID);
        int belongs = 0;
        BoxPokemon_GetValue(boxMon, ParamOTName, otName);
        if (trainerID == otID && trainerGender == otGender && GameString.String_Compare(trainerName, otName) == 0)
        {
            belongs = 1;
        }
        GameString.String_Free(otName);
        GameString.String_Free(trainerName);
        return belongs;
    }
}
