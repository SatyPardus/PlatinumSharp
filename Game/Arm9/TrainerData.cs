using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public static partial class TrainerData
{
    public const int Size = 0x34;
    public const int TypeOffset = 0;
    public const int ClassOffset = 1;
    public const int SpriteOffset = 2;
    public const int PartySizeOffset = 3;
    public const int ItemsOffset = 4;
    public const int AIMaskOffset = 0xC;
    public const int BattleTypeOffset = 0x10;
    public const int NameOffset = 0x14;
    public const int ParamType = 0;
    public const int ParamClass = 1;
    public const int ParamSprite = 2;
    public const int ParamPartySize = 3;
    public const int ParamItem1 = 4;
    public const int ParamItem4 = 7;
    public const int ParamAIMask = 8;
    public const int ParamBattleType = 9;
    public const int TypeDefault = 0;
    public const int TypeWithMoves = 1;
    public const int TypeWithItem = 2;
    public const int TypeWithItemAndMoves = 3;

    private const int NarcTrainerData = 0x39;
    private const int NarcTrainerParties = 0x3A;
    private const int NarcBallSeals = 0x98;
    private const int NarcMessages = 0x1A;
    private const int MessageBankTrainerNames = 0x26A;
    private const int ClassRival = 0x3F;
    private const int NameLength = 8;
    private const int PartyBufferSize = 0x6C;
    private const int MaxPartySize = 6;
    private const int TrainerClassCount = 0x69;
    private const uint ClassGenderTable = 0x020F0714;
    private const int GenderFemale = 1;
    private const uint PersonalityMale = 0x88;
    private const uint PersonalityFemale = 0x78;
    private const int FormMask = 0xFC00;
    private const int FormShift = 10;
    private const int SpeciesMask = 0x3FF;
    private const int MoveSlots = 4;
    private const int ParamHeldItem = 6;
    private const int ParamForm = 112;
    private const int ParamBallCapsuleID = 162;
    private const int ParamBallCapsule = 171;

    private static byte[]? classGenders;

    // OFFSET 0x02078AEC
    public static int Pokemon_SetBallSeal(int sealID, Pokemon mon, int heapID)
    {
        if (sealID == 0)
        {
            return 0;
        }
        NARC narc = NARC.NARC_ctor(NarcBallSeals, heapID);
        byte[] seals = new byte[BallCapsule.Size];
        NARC.NARC_ReadFromMember(narc, 0, (uint)((sealID - 1) * BallCapsule.Size), BallCapsule.Size, seals);
        BallCapsule capsule = new();
        capsule.ReadFrom(seals, 0);
        Pokemon.Pokemon_SetValue(mon, ParamBallCapsuleID, 1);
        Pokemon.Pokemon_SetValue(mon, ParamBallCapsule, capsule);
        NARC.NARC_dtor(narc);
        return 1;
    }

    // OFFSET 0x02079170
    public static void Trainer_Encounter(FieldBattleDTO dto, SaveData saveData, int heapID)
    {
        MessageLoader? loader = MessageLoader.MessageLoader_Init(1, NarcMessages, MessageBankTrainerNames, heapID);
        ushort[] rivalName = MiscSaveBlock.MiscSaveBlock_RivalName(MiscSaveBlock.SaveData_MiscSaveBlockConst(saveData));
        byte[] trainer = new byte[Size];
        for (int battler = 0; battler < FieldBattleDTO.MaxBattlers; battler++)
        {
            if (dto.TrainerIDs[battler] == 0)
            {
                continue;
            }
            Trainer_Load(dto.TrainerIDs[battler], trainer);
            Array.Copy(trainer, dto.Trainers[battler], Size);
            ushort[] name = new ushort[NameLength];
            if (trainer[ClassOffset] == ClassRival)
            {
                CharCode.CharCode_Copy(name, 0, rivalName, 0);
            }
            else
            {
                GameString? trainerName = MessageLoader.MessageLoader_GetNewString(loader!, (uint)dto.TrainerIDs[battler]);
                GameString.String_ToChars(trainerName, name, NameLength);
                GameString.String_Free(trainerName);
            }
            for (int i = 0; i < NameLength; i++)
            {
                Bin.W16(dto.Trainers[battler], NameOffset + i * 2, name[i]);
            }
            Trainer_BuildParty(dto, battler, heapID);
        }
        dto.BattleType |= Bin.U32(trainer, BattleTypeOffset);
        MessageLoader.MessageLoader_Free(loader);
    }

    // OFFSET 0x02079220
    public static uint Trainer_LoadParam(int trainerID, int param)
    {
        byte[] trainer = new byte[Size];
        Trainer_Load(trainerID, trainer);
        switch (param)
        {
            case ParamType:
                return trainer[TypeOffset];
            case ParamClass:
                return trainer[ClassOffset];
            case ParamSprite:
                return trainer[SpriteOffset];
            case ParamPartySize:
                return trainer[PartySizeOffset];
            case ParamItem1:
            case ParamItem1 + 1:
            case ParamItem1 + 2:
            case ParamItem4:
                return (uint)Bin.U16(trainer, ItemsOffset + (param - ParamItem1) * 2);
            case ParamAIMask:
                return Bin.U32(trainer, AIMaskOffset);
            case ParamBattleType:
                return Bin.U32(trainer, BattleTypeOffset);
        }
        return 0;
    }

    // OFFSET 0x0207938C
    public static void Trainer_Load(int trainerID, byte[] trainer)
    {
        NARC.NARC_ReadWholeMemberByIndexPair(trainer, NarcTrainerData, trainerID);
    }

    // OFFSET 0x0207939C
    public static void Trainer_LoadParty(int trainerID, byte[] party)
    {
        NARC.NARC_ReadWholeMemberByIndexPair(party, NarcTrainerParties, trainerID);
    }

    // OFFSET 0x020793AC
    public static byte TrainerClass_Gender(int trainerClass)
    {
        classGenders ??= RomData.Arm9(ClassGenderTable, TrainerClassCount);
        return classGenders[trainerClass];
    }

    // OFFSET 0x020793B8
    private static void Trainer_BuildParty(FieldBattleDTO dto, int battler, int heapID)
    {
        uint savedSeed = MathUtil.LCRNG_GetSeed();
        Party party = dto.Parties[battler]!;
        Party.Party_InitWithCapacity(party, MaxPartySize);
        byte[] entries = Heap.Heap_Alloc(heapID, PartyBufferSize)!;
        Pokemon mon = Pokemon.Pokemon_New(heapID);
        Trainer_LoadParty(dto.TrainerIDs[battler], entries);
        byte[] trainer = dto.Trainers[battler];
        uint genderMod = TrainerClass_Gender(trainer[ClassOffset]) == GenderFemale ? PersonalityFemale : PersonalityMale;
        int type = trainer[TypeOffset];
        if (type <= TypeWithItemAndMoves)
        {
            int entrySize = type == TypeDefault ? 8 : type == TypeWithMoves ? 0x10 : type == TypeWithItem ? 0xA : 0x12;
            for (int i = 0; i < trainer[PartySizeOffset]; i++)
            {
                int entry = i * entrySize;
                ushort dv = (ushort)Bin.U16(entries, entry);
                ushort level = (ushort)Bin.U16(entries, entry + 2);
                ushort speciesAndForm = (ushort)Bin.U16(entries, entry + 4);
                byte form = (byte)((speciesAndForm & FormMask) >> FormShift);
                ushort species = (ushort)(speciesAndForm & SpeciesMask);
                uint random = (uint)(dto.TrainerIDs[battler] + (species + (dv + level)));
                MathUtil.LCRNG_SetSeed(random);
                for (int j = 0; j < trainer[ClassOffset]; j++)
                {
                    random = MathUtil.LCRNG_Next();
                }
                uint personality = (random << 8) + genderMod;
                byte ivs = (byte)(dv * 31 / 255);
                Pokemon.Pokemon_InitWith(mon, species, level, ivs, 1, personality, Pokemon.OTIDTypeNotShiny, 0);
                int cursor = entry + 6;
                if (type == TypeWithItem || type == TypeWithItemAndMoves)
                {
                    Pokemon.Pokemon_SetValue(mon, ParamHeldItem, (ushort)Bin.U16(entries, cursor));
                    cursor += 2;
                }
                if (type == TypeWithMoves || type == TypeWithItemAndMoves)
                {
                    for (int slot = 0; slot < MoveSlots; slot++)
                    {
                        Pokemon.Pokemon_SetMoveSlot(mon, (ushort)Bin.U16(entries, cursor + slot * 2), (byte)slot);
                    }
                    cursor += MoveSlots * 2;
                }
                Pokemon_SetBallSeal(Bin.U16(entries, cursor), mon, heapID);
                Pokemon.Pokemon_SetValue(mon, ParamForm, form);
                Party.Party_AddPokemon(party, mon);
            }
        }
        Heap.Heap_Free(entries);
        MathUtil.LCRNG_SetSeed(savedSeed);
    }
}
