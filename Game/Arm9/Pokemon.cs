using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class BoxPokemon : BufferView
{
    public const int Size = 0x88;

    public BoxPokemon()
        : base(new byte[Size], 0)
    {
    }

    public BoxPokemon(byte[] buffer, int offset)
        : base(buffer, offset)
    {
    }
}

public sealed partial class Pokemon : BufferView
{
    public const int Size = 0xEC;
    public const int FlagsOffset = 4;
    public const int ChecksumOffset = 6;
    public const int BlocksOffset = 8;
    public const int BlocksSize = 0x80;
    public const int BlockSize = 0x20;
    public const int PartyOffset = 0x88;
    public const int PartySize = 0x64;
    public const int FlagPartyDecrypted = 1;
    public const int FlagBoxDecrypted = 2;
    public const int FlagInvalid = 4;
    public const int BlockA = 0;
    public const int BlockB = 1;
    public const int BlockC = 2;
    public const int BlockD = 3;
    public const int SpeciesEgg = 494;
    public const int SpeciesBadEgg = 495;

    private const int PartyStatusOffset = 0x88;
    private const int PartyLevelOffset = 0x8C;
    private const int PartyCapsuleOffset = 0x8D;
    private const int PartyHPOffset = 0x8E;
    private const int PartyMailOffset = 0x9C;
    private const int PartyBallCapsuleOffset = 0xD4;
    private const int FirstPartyParam = 0xA0;
    private const int PartyParamMail = 0xAA;
    private const int PartyParamBallCapsule = 0xAB;
    private const int ShuffleMask = 0x3E000;
    private const int ShuffleShift = 13;
    private const int NicknameLength = 10;
    private const int OTNameLength = 7;
    private const ushort EndOfString = 0xFFFF;
    private const int SpecialMetLocation = 3002;
    private const int SpeciesNidoranF = 29;
    private const int SpeciesNidoranM = 32;
    private const int SpeciesArceus = 493;
    private const int AbilityMultitype = 121;
    private const int ItemParamHoldEffect = 1;
    private const int FirstTypeParam = 171;

    private static readonly byte[][] blockPositions =
    {
        new byte[] { 0, 1, 2, 3 }, new byte[] { 0, 1, 3, 2 }, new byte[] { 0, 2, 1, 3 }, new byte[] { 0, 3, 1, 2 },
        new byte[] { 0, 2, 3, 1 }, new byte[] { 0, 3, 2, 1 }, new byte[] { 1, 0, 2, 3 }, new byte[] { 1, 0, 3, 2 },
        new byte[] { 2, 0, 1, 3 }, new byte[] { 3, 0, 1, 2 }, new byte[] { 2, 0, 3, 1 }, new byte[] { 3, 0, 2, 1 },
        new byte[] { 1, 2, 0, 3 }, new byte[] { 1, 3, 0, 2 }, new byte[] { 2, 1, 0, 3 }, new byte[] { 3, 1, 0, 2 },
        new byte[] { 2, 3, 0, 1 }, new byte[] { 3, 2, 0, 1 }, new byte[] { 1, 2, 3, 0 }, new byte[] { 1, 3, 2, 0 },
        new byte[] { 2, 1, 3, 0 }, new byte[] { 3, 1, 2, 0 }, new byte[] { 2, 3, 1, 0 }, new byte[] { 3, 2, 1, 0 },
        new byte[] { 0, 1, 2, 3 }, new byte[] { 0, 1, 3, 2 }, new byte[] { 0, 2, 1, 3 }, new byte[] { 0, 3, 1, 2 },
        new byte[] { 0, 2, 3, 1 }, new byte[] { 0, 3, 2, 1 }, new byte[] { 1, 0, 2, 3 }, new byte[] { 1, 0, 3, 2 },
    };

    public Pokemon()
        : base(new byte[Size], 0)
    {
    }

    public Pokemon(byte[] buffer, int offset)
        : base(buffer, offset)
    {
    }

    public byte[] Data => Buffer;

    public BoxPokemon Box => new(Buffer, Offset);

    // OFFSET 0x02073C2C
    public static void Pokemon_Init(Pokemon mon)
    {
        mon.Clear(0, Size);
        Pokemon_EncryptData(mon, BlocksOffset, BlocksSize, mon.U16(ChecksumOffset));
        Pokemon_EncryptData(mon, PartyOffset, PartySize, mon.U32(0));
    }

    // OFFSET 0x02073C54
    public static void BoxPokemon_Init(BoxPokemon boxMon)
    {
        boxMon.Clear(0, BoxPokemon.Size);
        Pokemon_EncryptData(boxMon, BlocksOffset, BlocksSize, boxMon.U16(ChecksumOffset));
    }

    // OFFSET 0x02073C70
    public static int Pokemon_StructSize()
    {
        return Size;
    }

    // OFFSET 0x02073C74
    public static Pokemon Pokemon_New(int heapID)
    {
        Pokemon mon = new();
        Pokemon_Init(mon);
        return mon;
    }

    // OFFSET 0x02073C88
    public static int Pokemon_EnterDecryptionContext(Pokemon mon)
    {
        int entered = 0;
        if ((mon.U16(FlagsOffset) & FlagPartyDecrypted) == 0)
        {
            entered = 1;
            if ((mon.U16(FlagsOffset) & FlagBoxDecrypted) != 0)
            {
                ErrorHandling.GF_AssertFail();
            }
            mon.W16(FlagsOffset, (mon.U16(FlagsOffset) & ~FlagPartyDecrypted) | FlagPartyDecrypted);
            mon.W16(FlagsOffset, mon.U16(FlagsOffset) | FlagBoxDecrypted);
            Pokemon_DecryptData(mon, PartyOffset, PartySize, mon.U32(0));
            Pokemon_DecryptData(mon, BlocksOffset, BlocksSize, mon.U16(ChecksumOffset));
        }
        return entered;
    }

    // OFFSET 0x02073CD4
    public static int Pokemon_ExitDecryptionContext(Pokemon mon, int encrypt)
    {
        int exited = 0;
        if ((mon.U16(FlagsOffset) & FlagPartyDecrypted) != 0 && encrypt == 1)
        {
            mon.W16(FlagsOffset, mon.U16(FlagsOffset) & ~FlagPartyDecrypted);
            mon.W16(FlagsOffset, mon.U16(FlagsOffset) & ~FlagBoxDecrypted);
            exited = 1;
            Pokemon_EncryptData(mon, PartyOffset, PartySize, mon.U32(0));
            mon.W16(ChecksumOffset, Pokemon_GetDataChecksum(mon, BlocksOffset, BlocksSize));
            Pokemon_EncryptData(mon, BlocksOffset, BlocksSize, mon.U16(ChecksumOffset));
        }
        return exited;
    }

    // OFFSET 0x02073D20
    public static int BoxPokemon_EnterDecryptionContext(BoxPokemon boxMon)
    {
        int entered = 0;
        if ((boxMon.U16(FlagsOffset) & FlagBoxDecrypted) == 0)
        {
            boxMon.W16(FlagsOffset, boxMon.U16(FlagsOffset) | FlagBoxDecrypted);
            entered = 1;
            Pokemon_DecryptData(boxMon, BlocksOffset, BlocksSize, boxMon.U16(ChecksumOffset));
        }
        return entered;
    }

    // OFFSET 0x02073D48
    public static int BoxPokemon_ExitDecryptionContext(BoxPokemon boxMon, int encrypt)
    {
        int exited = 0;
        if ((boxMon.U16(FlagsOffset) & FlagBoxDecrypted) != 0 && encrypt == 1)
        {
            boxMon.W16(FlagsOffset, boxMon.U16(FlagsOffset) & ~FlagBoxDecrypted);
            exited = 1;
            boxMon.W16(ChecksumOffset, Pokemon_GetDataChecksum(boxMon, BlocksOffset, BlocksSize));
            Pokemon_EncryptData(boxMon, BlocksOffset, BlocksSize, boxMon.U16(ChecksumOffset));
        }
        return exited;
    }

    // OFFSET 0x02074470
    public static uint Pokemon_GetValue(Pokemon mon, int param, object? dest)
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
            }
        }
        uint value = Pokemon_GetValueInternal(mon, param, dest);
        if ((mon.U16(FlagsOffset) & FlagPartyDecrypted) == 0)
        {
            Pokemon_EncryptData(mon, PartyOffset, PartySize, mon.U32(0));
            Pokemon_EncryptData(mon, BlocksOffset, BlocksSize, mon.U16(ChecksumOffset));
        }
        return value;
    }

    // OFFSET 0x020744E8
    private static uint Pokemon_GetValueInternal(Pokemon mon, int param, object? dest)
    {
        switch (param - FirstPartyParam)
        {
            case 0:
                return mon.U32(PartyStatusOffset);
            case 1:
                return mon.U8(PartyLevelOffset);
            case 2:
                return mon.U8(PartyCapsuleOffset);
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
                return mon.U16(PartyHPOffset + (param - FirstPartyParam - 3) * 2);
            case PartyParamMail - FirstPartyParam:
                ((Mail)dest!).ReadFrom(mon.Buffer, mon.Offset + PartyMailOffset);
                return 1;
            case PartyParamBallCapsule - FirstPartyParam:
                ((BallCapsule)dest!).ReadFrom(mon.Buffer, mon.Offset + PartyBallCapsuleOffset);
                return 1;
        }
        return BoxPokemon_GetValueInternal(mon.Box, param, dest);
    }

    // OFFSET 0x02074570
    public static uint BoxPokemon_GetValue(BoxPokemon boxMon, int param, object? dest)
    {
        if ((boxMon.U16(FlagsOffset) & FlagBoxDecrypted) == 0)
        {
            Pokemon_DecryptData(boxMon, BlocksOffset, BlocksSize, boxMon.U16(ChecksumOffset));
            ushort checksum = Pokemon_GetDataChecksum(boxMon, BlocksOffset, BlocksSize);
            if (checksum != boxMon.U16(ChecksumOffset))
            {
                ErrorHandling.GF_AssertFail();
                boxMon.W16(FlagsOffset, boxMon.U16(FlagsOffset) | FlagInvalid);
            }
        }
        uint value = BoxPokemon_GetValueInternal(boxMon, param, dest);
        if ((boxMon.U16(FlagsOffset) & FlagBoxDecrypted) == 0)
        {
            Pokemon_EncryptData(boxMon, BlocksOffset, BlocksSize, boxMon.U16(ChecksumOffset));
        }
        return value;
    }

    // OFFSET 0x020745D0
    private static uint BoxPokemon_GetValueInternal(BoxPokemon boxMon, int param, object? dest)
    {
        uint personality = boxMon.U32(0);
        int a = BoxPokemon_GetDataBlock(boxMon, personality, BlockA);
        int b = BoxPokemon_GetDataBlock(boxMon, personality, BlockB);
        int c = BoxPokemon_GetDataBlock(boxMon, personality, BlockC);
        int d = BoxPokemon_GetDataBlock(boxMon, personality, BlockD);
        bool invalid = (boxMon.U16(FlagsOffset) & FlagInvalid) != 0;
        uint ivs = boxMon.U32(b + 0x10);
        uint isEgg = (ivs >> 30) & 1;
        ushort species = boxMon.U16(a);
        if (param >= 6 && param <= 8)
        {
            switch (param)
            {
                case 6:
                    return boxMon.U16(a + 2);
                case 7:
                    return boxMon.U32(a + 4);
                default:
                    return boxMon.U32(a + 8);
            }
        }
        if (param >= 9 && param <= 24)
        {
            return boxMon.U8(a + 0xC + (param - 9));
        }
        if (param >= 25 && param <= 53)
        {
            return (boxMon.U32(a + 0x1C) & (uint)(1UL << (param - 25))) != 0 ? 1u : 0u;
        }
        if (param >= 54 && param <= 57)
        {
            return boxMon.U16(b + (param - 54) * 2);
        }
        if (param >= 58 && param <= 61)
        {
            return boxMon.U8(b + 8 + (param - 58));
        }
        if (param >= 62 && param <= 65)
        {
            return boxMon.U8(b + 0xC + (param - 62));
        }
        if (param >= 66 && param <= 69)
        {
            return MoveTable.MoveTable_CalcMaxPP(boxMon.U16(b + (param - 66) * 2), boxMon.U8(b + 0xC + (param - 66)));
        }
        if (param >= 70 && param <= 75)
        {
            return (ivs >> ((param - 70) * 5)) & 0x1F;
        }
        if (param >= 78 && param <= 109)
        {
            return (boxMon.U32(b + 0x14) & (uint)(1UL << (param - 78))) != 0 ? 1u : 0u;
        }
        if (param >= 123 && param <= 143)
        {
            ulong flags = boxMon.U32(c + 0x18) | ((ulong)boxMon.U32(c + 0x1C) << 32);
            return (flags & (1UL << (param - 123))) != 0 ? 1u : 0u;
        }
        if (param >= 146 && param <= 151)
        {
            return boxMon.U8(d + 0x10 + (param - 146));
        }
        switch (param)
        {
            case 0:
                return personality;
            case 1:
                return (uint)(boxMon.U16(FlagsOffset) & FlagPartyDecrypted);
            case 2:
                return (uint)((boxMon.U16(FlagsOffset) >> 1) & 1);
            case 3:
                return invalid ? 1u : 0u;
            case 4:
                return boxMon.U16(ChecksumOffset);
            case 172:
                return species != 0 ? 1u : 0u;
            case 173:
                if (invalid)
                {
                    return 1;
                }
                return isEgg;
            case 174:
                if (species == 0)
                {
                    return 0;
                }
                if (isEgg != 0 || invalid)
                {
                    return SpeciesEgg;
                }
                return species;
            case 161:
                return Pokemon.Species_GetLevelFromExp(species, boxMon.U32(a + 8));
            case 5:
                if (invalid)
                {
                    return SpeciesEgg;
                }
                return species;
            case 76:
                if (invalid)
                {
                    return 1;
                }
                return isEgg;
            case 77:
                return ivs >> 31;
            case 110:
                return (uint)(boxMon.U8(b + 0x18) & 1);
            case 111:
            {
                uint gender = SpeciesData.Species_GetGender(species, personality);
                boxMon.W8(b + 0x18, (boxMon.U8(b + 0x18) & ~6) | (((byte)gender & 3) << 1));
                boxMon.W16(ChecksumOffset, Pokemon_GetDataChecksum(boxMon, BlocksOffset, BlocksSize));
                return gender;
            }
            case 112:
                return (uint)(boxMon.U8(b + 0x18) >> 3);
            case 113:
                return boxMon.U8(b + 0x19);
            case 114:
                return boxMon.U16(b + 0x1A);
            case 117:
            {
                ushort[] buffer = (ushort[])dest!;
                if (invalid)
                {
                    MessageLoader.MessageLoader_GetSpeciesName(SpeciesBadEgg, 0, buffer);
                    return 0;
                }
                for (int i = 0; i < NicknameLength; i++)
                {
                    buffer[i] = boxMon.U16(c + i * 2);
                }
                buffer[NicknameLength] = EndOfString;
                return NicknameLength;
            }
            case 119:
            case 120:
            {
                uint result = param == 120 ? ivs >> 31 : 0;
                if (invalid)
                {
                    GameString? name = MessageUtil.MessageUtil_SpeciesName(SpeciesBadEgg, 0);
                    GameString.String_Copy((GameString?)dest, name);
                    GameString.String_Free(name);
                }
                else
                {
                    GameString.String_CopyChars((GameString?)dest, ReadChars(boxMon, c, NicknameLength + 1));
                }
                return result;
            }
            case 121:
                return boxMon.U8(c + 0x16);
            case 122:
                return boxMon.U8(c + 0x17);
            case 144:
            {
                ushort[] buffer = (ushort[])dest!;
                for (int i = 0; i < OTNameLength; i++)
                {
                    buffer[i] = boxMon.U16(d + i * 2);
                }
                buffer[OTNameLength] = EndOfString;
                return OTNameLength;
            }
            case 145:
                GameString.String_CopyChars((GameString?)dest, ReadChars(boxMon, d, OTNameLength + 1));
                return 0;
            case 115:
            case 152:
            {
                ushort location = boxMon.U16(d + 0x16);
                if (location == SpecialMetLocation && boxMon.U16(b + 0x1C) != 0)
                {
                    return boxMon.U16(b + 0x1C);
                }
                return location;
            }
            case 116:
            case 153:
            {
                ushort location = boxMon.U16(d + 0x18);
                if (location == SpecialMetLocation && boxMon.U16(b + 0x1E) != 0)
                {
                    return boxMon.U16(b + 0x1E);
                }
                return location;
            }
            case 154:
                return boxMon.U8(d + 0x1A);
            case 155:
                return boxMon.U8(d + 0x1B);
            case 156:
                return (uint)(boxMon.U8(d + 0x1C) & 0x7F);
            case 157:
                return (uint)(boxMon.U8(d + 0x1C) >> 7);
            case 158:
                return boxMon.U8(d + 0x1D);
            case 159:
                return boxMon.U16(d + 0x1E);
            case 175:
                return ivs & 0x3FFFFFFF;
            case 176:
                if ((species == SpeciesNidoranF || species == SpeciesNidoranM) && (ivs >> 31) == 0)
                {
                    return 0;
                }
                return 1;
            case 177:
            case 178:
                if (species == SpeciesArceus && boxMon.U8(a + 0xD) == AbilityMultitype)
                {
                    return Pokemon.Pokemon_GetArceusTypeOf((ushort)Item.Item_LoadParam(boxMon.U16(a + 2), ItemParamHoldEffect, 0));
                }
                return SpeciesData.SpeciesData_GetFormValue(species, boxMon.U8(b + 0x18) >> 3, param - FirstTypeParam);
            case 179:
                MessageLoader.MessageLoader_GetSpeciesName(species, 0, (ushort[])dest!);
                return 0;
        }
        return 0;
    }

    private static ushort[] ReadChars(BufferView view, int position, int length)
    {
        ushort[] chars = new ushort[length + 1];
        for (int i = 0; i < length; i++)
        {
            chars[i] = view.U16(position + i * 2);
        }
        chars[length] = EndOfString;
        return chars;
    }

    // OFFSET 0x0207822C
    private static void Pokemon_EncryptData(BufferView view, int position, uint size, uint seed)
    {
        MathUtil.EncodeData(view.Buffer, view.Offset + position, size, seed);
    }

    // OFFSET 0x02078234
    private static void Pokemon_DecryptData(BufferView view, int position, uint size, uint seed)
    {
        MathUtil.EncodeData(view.Buffer, view.Offset + position, size, seed);
    }

    // OFFSET 0x0207823C
    private static ushort Pokemon_GetDataChecksum(BufferView view, int position, uint size)
    {
        ushort checksum = 0;
        for (int i = 0; i < size / 2; i++)
        {
            checksum += view.U16(position + i * 2);
        }
        return checksum;
    }

    // OFFSET 0x0207825C
    private static int BoxPokemon_GetDataBlock(BoxPokemon boxMon, uint personality, int block)
    {
        uint shuffle = (personality & ShuffleMask) >> ShuffleShift;
        return BlocksOffset + blockPositions[shuffle][block] * BlockSize;
    }
}
