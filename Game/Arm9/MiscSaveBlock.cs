using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed class PersistedMapFeatures
{
    public const int Size = 0x24;
    public const int BufferSize = 0x20;

    public int ID;
    public readonly byte[] Buffer = new byte[BufferSize];

    public void WriteTo(byte[] body, int offset)
    {
        Bin.W32(body, offset, (uint)ID);
        Array.Copy(Buffer, 0, body, offset + 4, BufferSize);
    }

    public void ReadFrom(byte[] body, int offset)
    {
        ID = (int)Bin.U32(body, offset);
        Array.Copy(body, offset + 4, Buffer, 0, BufferSize);
    }

    // OFFSET 0x02027F50
    public static void PersistedMapFeatures_Init(PersistedMapFeatures features)
    {
        features.ID = 0;
        Array.Clear(features.Buffer);
    }

    // OFFSET 0x02027F5C
    public static byte[] PersistedMapFeatures_InitWithID(PersistedMapFeatures features, int id)
    {
        PersistedMapFeatures_Init(features);
        features.ID = id;
        return features.Buffer;
    }

    // OFFSET 0x02027F6C
    public static byte[] PersistedMapFeatures_GetBuffer(PersistedMapFeatures features, int id)
    {
        if (features.ID != id)
        {
            ErrorHandling.GF_AssertFail();
        }
        return features.Buffer;
    }

    // OFFSET 0x02027F80
    public static int PersistedMapFeatures_GetID(PersistedMapFeatures features)
    {
        return features.ID;
    }
}

public sealed class MiscSaveBlock : ISaveBlock
{
    public const int Size = 0x78C;
    public const int RivalNameLength = 8;
    public const int TabletNameLength = 11;
    public const int ExtraSaveCount = 5;

    private const int MapFeaturesOffset = 0x700;
    private const int RivalNameOffset = 0x724;
    private const int TabletNameOffset = 0x734;
    private const int FavoriteSpeciesOffset = 0x74A;
    private const int FavoriteFormOffset = 0x74C;
    private const int FlagsOffset = 0x74D;
    private const int IntroMessageOffset = 0x754;
    private const int ExtraKeysOffset = 0x75C;
    private const int ExtraOldKeysOffset = 0x770;
    private const int ExtraMirrorFlagsOffset = 0x784;
    private const ushort EndOfString = 0xFFFF;
    private const uint NoExtraKey = 0xFFFFFFFF;
    private const uint IntroWordBank = 0x1B9;
    private const uint IntroWordEntry = 0x63;
    private const byte InitFlagMask = 1;
    private const byte VsRecorderColorMask = 0x1E;
    private const byte FavoriteFormMask = 0x7F;
    private const byte FavoriteEggMask = 0x80;

    public readonly BerryPatch[] BerryPatches = new BerryPatch[Game.BerryPatches.MaxPatches];
    public readonly PersistedMapFeatures MapFeatures = new();
    public readonly ushort[] RivalName = new ushort[RivalNameLength];
    public readonly ushort[] TabletName = new ushort[TabletNameLength];
    public ushort FavoriteSpecies;
    public byte FavoriteForm;
    public byte Flags;
    public readonly EasyChatSentence IntroMessage = new();
    public readonly uint[] ExtraKeys = new uint[ExtraSaveCount];
    public readonly uint[] ExtraOldKeys = new uint[ExtraSaveCount];
    public readonly byte[] ExtraMirrorFlags = new byte[ExtraSaveCount];

    public MiscSaveBlock()
    {
        for (int i = 0; i < BerryPatches.Length; i++)
        {
            BerryPatches[i] = new BerryPatch();
        }
    }

    public void WriteTo(byte[] body, int offset)
    {
        for (int i = 0; i < BerryPatches.Length; i++)
        {
            BerryPatches[i].WriteTo(body, offset + i * BerryPatch.Size);
        }
        MapFeatures.WriteTo(body, offset + MapFeaturesOffset);
        for (int i = 0; i < RivalNameLength; i++)
        {
            Bin.W16(body, offset + RivalNameOffset + i * 2, RivalName[i]);
        }
        for (int i = 0; i < TabletNameLength; i++)
        {
            Bin.W16(body, offset + TabletNameOffset + i * 2, TabletName[i]);
        }
        Bin.W16(body, offset + FavoriteSpeciesOffset, FavoriteSpecies);
        body[offset + FavoriteFormOffset] = FavoriteForm;
        body[offset + FlagsOffset] = Flags;
        Bin.W16(body, offset + IntroMessageOffset, IntroMessage.Type);
        Bin.W16(body, offset + IntroMessageOffset + 2, IntroMessage.ID);
        Bin.W16(body, offset + IntroMessageOffset + 4, IntroMessage.Words[0]);
        Bin.W16(body, offset + IntroMessageOffset + 6, IntroMessage.Words[1]);
        for (int i = 0; i < ExtraSaveCount; i++)
        {
            Bin.W32(body, offset + ExtraKeysOffset + i * 4, ExtraKeys[i]);
            Bin.W32(body, offset + ExtraOldKeysOffset + i * 4, ExtraOldKeys[i]);
            body[offset + ExtraMirrorFlagsOffset + i] = ExtraMirrorFlags[i];
        }
    }

    public void ReadFrom(byte[] body, int offset)
    {
        for (int i = 0; i < BerryPatches.Length; i++)
        {
            BerryPatches[i].ReadFrom(body, offset + i * BerryPatch.Size);
        }
        MapFeatures.ReadFrom(body, offset + MapFeaturesOffset);
        for (int i = 0; i < RivalNameLength; i++)
        {
            RivalName[i] = (ushort)Bin.U16(body, offset + RivalNameOffset + i * 2);
        }
        for (int i = 0; i < TabletNameLength; i++)
        {
            TabletName[i] = (ushort)Bin.U16(body, offset + TabletNameOffset + i * 2);
        }
        FavoriteSpecies = (ushort)Bin.U16(body, offset + FavoriteSpeciesOffset);
        FavoriteForm = body[offset + FavoriteFormOffset];
        Flags = body[offset + FlagsOffset];
        IntroMessage.Type = (ushort)Bin.U16(body, offset + IntroMessageOffset);
        IntroMessage.ID = (ushort)Bin.U16(body, offset + IntroMessageOffset + 2);
        IntroMessage.Words[0] = (ushort)Bin.U16(body, offset + IntroMessageOffset + 4);
        IntroMessage.Words[1] = (ushort)Bin.U16(body, offset + IntroMessageOffset + 6);
        for (int i = 0; i < ExtraSaveCount; i++)
        {
            ExtraKeys[i] = Bin.U32(body, offset + ExtraKeysOffset + i * 4);
            ExtraOldKeys[i] = Bin.U32(body, offset + ExtraOldKeysOffset + i * 4);
            ExtraMirrorFlags[i] = body[offset + ExtraMirrorFlagsOffset + i];
        }
    }

    // OFFSET 0x020277AC
    public static void MiscSaveBlock_Init(MiscSaveBlock misc)
    {
        misc.FavoriteSpecies = 0;
        misc.FavoriteForm = 0;
        misc.Flags = 0;
        Array.Clear(misc.ExtraMirrorFlags);
        Game.BerryPatches.BerryPatches_Clear(misc.BerryPatches);
        PersistedMapFeatures.PersistedMapFeatures_Init(misc.MapFeatures);
        Array.Fill(misc.RivalName, EndOfString);
        Array.Fill(misc.TabletName, EndOfString);
        EasyChatSentence.EasyChatSentence_InitWithType(misc.IntroMessage, EasyChatSentence.TypeUnionRoom);
        misc.IntroMessage.ID = 0;
        misc.IntroMessage.Words[0] = EasyChatWords.EasyChatWord_FromBankAndEntry(IntroWordBank, IntroWordEntry);
        misc.IntroMessage.Words[1] = EasyChatSentence.WordNone;
        for (int i = 0; i < ExtraSaveCount; i++)
        {
            misc.ExtraKeys[i] = NoExtraKey;
            misc.ExtraOldKeys[i] = NoExtraKey;
        }
    }

    // OFFSET 0x0202783C
    public static MiscSaveBlock SaveData_MiscSaveBlock(SaveData saveData)
    {
        return (MiscSaveBlock)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryMisc);
    }

    // OFFSET 0x02027848
    public static MiscSaveBlock SaveData_MiscSaveBlockConst(SaveData saveData)
    {
        return (MiscSaveBlock)SaveData.SaveData_SaveTableConst(saveData, SaveDataTable.EntryMisc);
    }

    // OFFSET 0x02027854
    public static BerryPatch[] SaveData_GetBerryPatches(SaveData saveData)
    {
        return ((MiscSaveBlock)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryMisc)).BerryPatches;
    }

    // OFFSET 0x02027860
    public static PersistedMapFeatures SaveData_GetPersistedMapFeatures(SaveData saveData)
    {
        return ((MiscSaveBlock)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryMisc)).MapFeatures;
    }

    // OFFSET 0x02027870
    public static ushort[] MiscSaveBlock_RivalName(MiscSaveBlock misc)
    {
        return misc.RivalName;
    }

    // OFFSET 0x0202787C
    public static void MiscSaveBlock_SetRivalName(MiscSaveBlock misc, GameString name)
    {
        GameString.String_ToChars(name, misc.RivalName, RivalNameLength);
    }

    // OFFSET 0x02027894
    public static ushort[] MiscSaveBlock_TabletName(MiscSaveBlock misc)
    {
        return misc.TabletName;
    }

    // OFFSET 0x020278A0
    public static void MiscSaveBlock_SetTabletName(MiscSaveBlock misc, GameString name)
    {
        GameString.String_ToChars(name, misc.TabletName, TabletNameLength);
    }

    // OFFSET 0x020278B8
    public static void MiscSaveBlock_SetInitFlag(MiscSaveBlock misc)
    {
        misc.Flags = (byte)((misc.Flags & ~InitFlagMask) | InitFlagMask);
    }

    // OFFSET 0x020278CC
    public static int MiscSaveBlock_InitFlag(MiscSaveBlock misc)
    {
        return misc.Flags & InitFlagMask;
    }

    // OFFSET 0x020278DC
    public static void MiscSaveBlock_SetFavoriteMon(MiscSaveBlock misc, int species, int form, int isEgg)
    {
        misc.FavoriteSpecies = (ushort)species;
        misc.FavoriteForm = (byte)((misc.FavoriteForm & ~FavoriteFormMask) | ((byte)form & FavoriteFormMask));
        misc.FavoriteForm = (byte)((misc.FavoriteForm & ~FavoriteEggMask) | (((byte)isEgg & 1) << 7));
    }

    // OFFSET 0x02027914
    public static void MiscSaveBlock_FavoriteMon(MiscSaveBlock misc, out int species, out int form, out int isEgg)
    {
        species = misc.FavoriteSpecies;
        form = misc.FavoriteForm & FavoriteFormMask;
        isEgg = (misc.FavoriteForm >> 7) & 1;
    }

    // OFFSET 0x02027938
    public static void MiscSaveBlock_IntroMsg(MiscSaveBlock misc, EasyChatSentence sentence)
    {
        sentence.Type = misc.IntroMessage.Type;
        sentence.ID = misc.IntroMessage.ID;
        sentence.Words[0] = misc.IntroMessage.Words[0];
        sentence.Words[1] = misc.IntroMessage.Words[1];
    }

    // OFFSET 0x02027958
    public static void MiscSaveBlock_SetIntroMsg(MiscSaveBlock misc, EasyChatSentence sentence)
    {
        misc.IntroMessage.Type = sentence.Type;
        misc.IntroMessage.ID = sentence.ID;
        misc.IntroMessage.Words[0] = sentence.Words[0];
        misc.IntroMessage.Words[1] = sentence.Words[1];
    }

    // OFFSET 0x0202797C
    public static void MiscSaveBlock_VsRecorderColor(MiscSaveBlock misc, out byte color)
    {
        color = (byte)((misc.Flags & VsRecorderColorMask) >> 1);
    }

    // OFFSET 0x0202798C
    public static void MiscSaveBlock_SetVsRecorderColor(MiscSaveBlock misc, byte color)
    {
        misc.Flags = (byte)((misc.Flags & ~VsRecorderColorMask) | ((color & 0xF) << 1));
    }

    // OFFSET 0x020279A8
    public static void MiscSaveBlock_ExtraSaveKey(MiscSaveBlock misc, int extraSaveID, out uint currentKey, out uint oldKey, out byte mirrorFlag)
    {
        currentKey = misc.ExtraKeys[extraSaveID - 1];
        oldKey = misc.ExtraOldKeys[extraSaveID - 1];
        mirrorFlag = misc.ExtraMirrorFlags[extraSaveID - 1];
    }

    // OFFSET 0x020279D0
    public static void MiscSaveBlock_SetExtraSaveKey(MiscSaveBlock misc, int extraSaveID, uint currentKey, uint oldKey, byte mirrorFlag)
    {
        misc.ExtraKeys[extraSaveID - 1] = currentKey;
        misc.ExtraOldKeys[extraSaveID - 1] = oldKey;
        misc.ExtraMirrorFlags[extraSaveID - 1] = mirrorFlag;
    }
}
