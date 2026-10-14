namespace PlatinumSharp.Game;

public sealed class Bag : RawSaveBlock
{
    public const int Size = 0x774;

    public Bag()
        : base(Size)
    {
    }

    // OFFSET 0x0207D3D8
    public static void Bag_Init(Bag bag)
    {
        bag.Clear(0, Size);
    }
}

public sealed class VarsFlags : RawSaveBlock
{
    public const int Size = 0x3AC;

    public VarsFlags()
        : base(Size)
    {
    }

    // OFFSET 0x020507D4
    public static void VarsFlags_Init(VarsFlags varsFlags)
    {
        varsFlags.Clear(0, Size);
    }

    private const int SaveTableID = 4;
    private const int FlagsOffset = 0x240;
    private const int FlagsSize = 0x16C;
    private const int TemporaryFlagBase = 0x4000;
    private const int TemporaryFlagsSize = 8;
    private const int VarBase = 0x4000;
    private const int VarCount = 0x120;

    private static readonly byte[] sTemporaryFlags = new byte[TemporaryFlagsSize];

    // OFFSET 0x020507E4
    public static VarsFlags SaveData_GetVarsFlags(SaveData saveData)
    {
        return (VarsFlags)SaveData.SaveData_SaveTable(saveData, SaveTableID);
    }

    // OFFSET 0x020507F0
    public static bool VarsFlags_CheckFlag(VarsFlags varsFlags, ushort flagID)
    {
        if (!VarsFlags_GetFlagChunk(varsFlags, flagID, out byte[] chunk, out int index))
        {
            return false;
        }
        bool set = (chunk[index] & (1 << (flagID % 8))) != 0;
        ScriptTrace.OnFlagRead(flagID, set);
        return set;
    }

    // OFFSET 0x0205081C
    public static void VarsFlags_SetFlag(VarsFlags varsFlags, ushort flagID)
    {
        if (!VarsFlags_GetFlagChunk(varsFlags, flagID, out byte[] chunk, out int index))
        {
            return;
        }
        chunk[index] |= (byte)(1 << (flagID % 8));
    }

    // OFFSET 0x02050844
    public static void VarsFlags_ClearFlag(VarsFlags varsFlags, ushort flagID)
    {
        if (!VarsFlags_GetFlagChunk(varsFlags, flagID, out byte[] chunk, out int index))
        {
            return;
        }
        chunk[index] &= (byte)(0xFF ^ (1 << (flagID % 8)));
    }

    // OFFSET 0x02050870
    public static bool VarsFlags_GetFlagChunk(VarsFlags varsFlags, ushort flagID, out byte[] chunk, out int index)
    {
        chunk = sTemporaryFlags;
        index = 0;
        if (flagID == 0)
        {
            return false;
        }
        if (flagID < TemporaryFlagBase)
        {
            if (flagID / 8 >= FlagsSize)
            {
                ErrorHandling.GF_AssertFail();
            }
            chunk = varsFlags.Buffer;
            index = varsFlags.Offset + FlagsOffset + flagID / 8;
            return true;
        }
        if ((flagID - TemporaryFlagBase) / 8 >= TemporaryFlagsSize)
        {
            ErrorHandling.GF_AssertFail();
        }
        index = (flagID - TemporaryFlagBase) / 8;
        return true;
    }

    // OFFSET 0x020508B8
    public static int VarsFlags_GetVarOffset(VarsFlags varsFlags, ushort varID)
    {
        int index = varID - VarBase;
        if (index >= VarCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return index * 2;
    }
}

public sealed partial class Poketch : RawSaveBlock
{
    public const int Size = 0x118;
    public const int AppCount = 25;
    public const int AppDigitalClock = 0;
    public const int AppPedometer = 3;

    private const int RegisteredCountOffset = 1;
    private const int CurrentAppOffset = 2;
    private const int RegisteredAppsOffset = 3;
    private const int RegisteredAppsSize = 0x20;
    private const int StepCountOffset = 0x24;
    private const int AlarmOffset = 0x28;
    private const int CalendarMonthOffset = 0xA8;
    private const int CalendarMarksOffset = 0xA4;
    private const int MapMarkersOffset = 0xA9;
    private const int MapMarkerCount = 6;
    private const uint MapMarkerDefaults = 0x020EC538;
    private const int HistoryOffset = 0xB8;
    private const int HistoryCount = 12;
    private const int HistoryEntrySize = 8;
    private const int FlagEnabled = 1;
    private const int FlagPedometerRegistered = 2;
    private const int FlagDotArtModified = 4;
    private const int FlagScreenColor = 0x38;
    private const int AlarmSetMask = 1;
    private const int AlarmHourMask = 0x3E;
    private const int AlarmMinuteMask = 0xFC0;

    public Poketch()
        : base(Size)
    {
    }

    // OFFSET 0x02056728
    public static void Poketch_Init(Poketch poketch)
    {
        poketch.Clear(RegisteredAppsOffset, RegisteredAppsSize);
        poketch.W8(RegisteredCountOffset, 0);
        poketch.W8(CurrentAppOffset, 0);
        poketch.W8(0, poketch.U8(0) & ~FlagEnabled);
        poketch.W8(0, poketch.U8(0) & ~FlagScreenColor);
        poketch.W8(0, poketch.U8(0) & ~FlagPedometerRegistered);
        poketch.W32(StepCountOffset, 0);
        poketch.W16(AlarmOffset, poketch.U16(AlarmOffset) & ~AlarmSetMask);
        poketch.W16(AlarmOffset, poketch.U16(AlarmOffset) & ~AlarmHourMask);
        poketch.W16(AlarmOffset, poketch.U16(AlarmOffset) & ~AlarmMinuteMask);
        poketch.W8(CalendarMonthOffset, 1);
        poketch.W32(CalendarMarksOffset, 0);
        byte[] markers = RomData.Arm9(MapMarkerDefaults, MapMarkerCount * 2);
        for (int i = 0; i < MapMarkerCount; i++)
        {
            poketch.W8(MapMarkersOffset + i * 2, markers[i * 2]);
            poketch.W8(MapMarkersOffset + i * 2 + 1, markers[i * 2 + 1]);
        }
        for (int i = 0; i < HistoryCount; i++)
        {
            poketch.W16(HistoryOffset + i * HistoryEntrySize, 0);
            poketch.W16(HistoryOffset + i * HistoryEntrySize + 2, 0);
            poketch.W32(HistoryOffset + i * HistoryEntrySize + 4, 0);
        }
        poketch.W8(0, poketch.U8(0) & ~FlagDotArtModified);
        Poketch_RegisterApp(poketch, AppDigitalClock);
    }

    // OFFSET 0x020567F0
    public static bool Poketch_RegisterApp(Poketch poketch, int appID)
    {
        bool registered = false;
        if (appID < 0 || appID >= AppCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if ((sbyte)poketch.U8(RegisteredCountOffset) < AppCount && poketch.U8(RegisteredAppsOffset + appID) == 0)
        {
            poketch.W8(RegisteredAppsOffset + appID, 1);
            poketch.W8(RegisteredCountOffset, (sbyte)poketch.U8(RegisteredCountOffset) + 1);
            if (appID == AppPedometer)
            {
                poketch.W8(0, poketch.U8(0) | FlagPedometerRegistered);
            }
            registered = true;
        }
        return registered;
    }
}

public sealed class FieldOverworldState : RawSaveBlock
{
    public const int Size = 0x1400;

    public FieldOverworldState()
        : base(Size)
    {
    }

    // OFFSET 0x0203A6E8
    public static void FieldOverworldState_Init(FieldOverworldState state)
    {
        state.Clear(0, Size);
    }

    public const int SavedMapObjectCount = 0x40;

    // OFFSET 0x0203A79C
    public static FieldOverworldState SaveData_GetFieldOverworldState(SaveData saveData)
    {
        return (FieldOverworldState)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryFieldOverworldState);
    }
}

public sealed class FieldPlayerState : RawSaveBlock
{
    public const int Size = 0xA0;

    private const int WarpOffset = 0x6C;
    private const int WarpEntryCount = 6;
    private const int PlayerDataOffset = 0x8C;

    public FieldPlayerState()
        : base(Size)
    {
    }

    // OFFSET 0x0203A6F8
    public static void FieldPlayerState_Init(FieldPlayerState state)
    {
        state.Clear(0, Size);
        FieldPlayerState_InitWarps(state, WarpOffset);
        FieldPlayerState_InitPlayerData(state, PlayerDataOffset);
        state.W16(BlackOutWarpIdOffset, FieldPlayerState_DefaultBlackOutWarpId());
    }

    public const int LocationSize = 0x14;

    private const int SaveTableID = 6;
    private const uint NewGameLocationAddress = 0x020EA12C;

    // OFFSET 0x0203A720
    public static BufferView FieldPlayerState_GetPlayerLocation(FieldPlayerState state)
    {
        return new BufferView(state.Buffer, state.Offset);
    }

    // OFFSET 0x0203A790
    public static FieldPlayerState SaveData_GetFieldPlayerState(SaveData saveData)
    {
        return (FieldPlayerState)SaveData.SaveData_SaveTable(saveData, SaveTableID);
    }

    // OFFSET 0x0203D178
    public static void Location_SetNewGameStart(BufferView location)
    {
        location.CopyFrom(0, RomData.Arm9(NewGameLocationAddress, LocationSize), 0, LocationSize);
    }

    // OFFSET 0x0203D1A8
    public static void FieldPlayerState_InitNewGameLocation(SaveData saveData)
    {
        Location_SetNewGameStart(FieldPlayerState_GetPlayerLocation(SaveData_GetFieldPlayerState(saveData)));
    }

    public const int EntranceLocationOffset = 0x14;
    public const int PreviousLocationOffset = 0x28;
    public const int SpecialLocationOffset = 0x3C;
    public const int ExitLocationOffset = 0x50;
    public const int SpecialBGMOffset = 0x64;
    public const int OverworldWeatherOffset = 0x66;
    public const int BlackOutWarpIdOffset = 0x68;
    public const int CameraTypeOffset = 0x6A;
    public const int MapHistoryOffset = 0x6C;
    public const int PlayerDataBlockOffset = 0x8C;
    public const int PoisonStepCountOffset = 0x94;
    public const int SafariStepCountOffset = 0x96;
    public const int SafariBallCountOffset = 0x98;

    // OFFSET 0x0203A724
    public static BufferView FieldPlayerState_GetEntranceLocation(FieldPlayerState state)
    {
        return new BufferView(state.Buffer, state.Offset + EntranceLocationOffset);
    }

    // OFFSET 0x0203A728
    public static BufferView FieldPlayerState_GetPreviousLocation(FieldPlayerState state)
    {
        return new BufferView(state.Buffer, state.Offset + PreviousLocationOffset);
    }

    // OFFSET 0x0203A72C
    public static BufferView FieldPlayerState_GetExitLocation(FieldPlayerState state)
    {
        return new BufferView(state.Buffer, state.Offset + ExitLocationOffset);
    }

    // OFFSET 0x0203A730
    public static BufferView FieldPlayerState_GetSpecialLocation(FieldPlayerState state)
    {
        return new BufferView(state.Buffer, state.Offset + SpecialLocationOffset);
    }

    // OFFSET 0x0203A734
    public static void FieldPlayerState_SetSpecialLocation(FieldPlayerState state, BufferView location)
    {
        state.CopyFrom(SpecialLocationOffset, location.Buffer, location.Offset, LocationSize);
    }

    // OFFSET 0x0203A748
    public static BufferView FieldPlayerState_GetSpecialBGM(FieldPlayerState state)
    {
        return new BufferView(state.Buffer, state.Offset + SpecialBGMOffset);
    }

    // OFFSET 0x0203A74C
    public static int FieldPlayerState_GetWeather(FieldPlayerState state)
    {
        return state.U16(OverworldWeatherOffset);
    }

    // OFFSET 0x0203A754
    public static void FieldPlayerState_SetWeather(FieldPlayerState state, int weather)
    {
        state.W16(OverworldWeatherOffset, weather);
    }

    // OFFSET 0x0203A75C
    public static int FieldPlayerState_GetBlackOutWarpId(FieldPlayerState state)
    {
        return state.U16(BlackOutWarpIdOffset);
    }

    // OFFSET 0x0203A764
    public static void FieldPlayerState_SetBlackOutWarpId(FieldPlayerState state, int blackOutWarpId)
    {
        state.W16(BlackOutWarpIdOffset, blackOutWarpId);
    }

    // OFFSET 0x0203A76C
    public static BufferView FieldPlayerState_GetMapHistory(FieldPlayerState state)
    {
        return new BufferView(state.Buffer, state.Offset + MapHistoryOffset);
    }

    // OFFSET 0x0203A770
    public static int FieldPlayerState_GetCameraType(FieldPlayerState state)
    {
        return state.U8(CameraTypeOffset);
    }

    // OFFSET 0x0203A778
    public static void FieldPlayerState_SetCameraType(FieldPlayerState state, int cameraType)
    {
        state.W8(CameraTypeOffset, cameraType);
    }

    // OFFSET 0x0203A780
    public static BufferView FieldPlayerState_GetPlayerData(FieldPlayerState state)
    {
        return new BufferView(state.Buffer, state.Offset + PlayerDataBlockOffset);
    }

    // OFFSET 0x0203A784
    public static BufferView FieldPlayerState_GetSafariBallCount(FieldPlayerState state)
    {
        return new BufferView(state.Buffer, state.Offset + SafariBallCountOffset);
    }

    // OFFSET 0x0203A788
    public static BufferView FieldPlayerState_GetSafariStepCount(FieldPlayerState state)
    {
        return new BufferView(state.Buffer, state.Offset + SafariStepCountOffset);
    }

    // OFFSET 0x0203A78C
    public static BufferView FieldPlayerState_GetPoisonStepCount(FieldPlayerState state)
    {
        return new BufferView(state.Buffer, state.Offset + PoisonStepCountOffset);
    }

    // OFFSET 0x0203A7EC
    public static int FieldPlayerState_DefaultBlackOutWarpId()
    {
        return 1;
    }

    // OFFSET 0x020556C4
    private static void FieldPlayerState_InitWarps(BufferView view, int position)
    {
        for (byte i = 0; i < WarpEntryCount; i++)
        {
            view.W8(position + 6 + i * 4, 0);
            view.W8(position + 7 + i * 4, 0);
            view.W8(position + 8 + i * 4, 0);
            view.W8(position + 9 + i * 4, 0);
        }
        view.W32(position, 0);
        view.W8(position + 4, 0);
        view.W8(position + 5, 0);
    }

    // OFFSET 0x0205EC34
    private static void FieldPlayerState_InitPlayerData(BufferView view, int position)
    {
        view.W16(position, 0);
        view.W16(position + 2, 0);
        view.W32(position + 4, 0);
    }
}

public sealed class Email : RawSaveBlock
{
    public const int Size = 0x40;

    private const int AddressSize = 0x33;
    private const int StateOffset = 0x34;

    public Email()
        : base(Size)
    {
    }

    // OFFSET 0x02030CEC
    public static void Email_Init(Email email)
    {
        email.Clear(0, Size);
        email.Clear(0, AddressSize);
        email.W32(StateOffset, 1);
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryEmail);
    }
}

public sealed class WiFiPlazaQuestions : RawSaveBlock
{
    public const int Size = 8;

    public WiFiPlazaQuestions()
        : base(Size)
    {
    }

    // OFFSET 0x02030EA8
    public static void WiFiPlazaQuestions_Init(WiFiPlazaQuestions questions)
    {
        questions.Clear(0, SaveDataTable.WiFiPlazaQuestions_SaveSize());
        questions.W32(0, 0xFFFFFFFF);
    }
}

public sealed partial class PCBoxes : RawSaveBlock
{
    public const int Size = 0x121C8;
    public const int BoxCount = 18;
    public const int MonsPerBox = 30;
    public const int WallpaperCount = 16;
    public const int BoxNameLength = 20;

    private const int MonsOffset = 4;
    private const int BoxSize = MonsPerBox * BoxPokemon.Size;
    private const int NamesOffset = 0x11EE4;
    private const int NameSize = BoxNameLength * 2;
    private const int WallpapersOffset = 0x121B4;
    private const int UnlockedWallpapersOffset = 0x121C6;
    private const int MessageArchive = 0x1A;
    private const int BoxNamesBank = 0x12;
    private const int FirstBoxName = 6;

    public PCBoxes()
        : base(Size)
    {
    }

    // OFFSET 0x020797C8
    public static void PCBoxes_Init(PCBoxes boxes)
    {
        PCBoxes_InitInternal(boxes);
        SaveData.SaveData_SetFullSaveRequired();
    }

    // OFFSET 0x020797DC
    private static void PCBoxes_InitInternal(PCBoxes boxes)
    {
        for (uint box = 0; box < BoxCount; box++)
        {
            for (uint slot = 0; slot < MonsPerBox; slot++)
            {
                Pokemon.BoxPokemon_Init(new BoxPokemon(boxes.Buffer, MonsOffset + (int)box * BoxSize + (int)slot * BoxPokemon.Size));
            }
        }
        uint wallpaper = 0;
        for (uint box = 0; box < BoxCount; box++)
        {
            boxes.W8(WallpapersOffset + (int)box, (int)wallpaper);
            wallpaper++;
            if (wallpaper >= WallpaperCount)
            {
                wallpaper = 0;
            }
        }
        boxes.W8(UnlockedWallpapersOffset, 0);
        MessageLoader? loader = MessageLoader.MessageLoader_Init(1, MessageArchive, BoxNamesBank, 0);
        if (loader != null)
        {
            ushort[] name = new ushort[BoxNameLength];
            for (uint box = 0; box < BoxCount; box++)
            {
                MessageLoader.MessageLoader_Get(loader, box + FirstBoxName, name);
                for (int i = 0; i < BoxNameLength; i++)
                {
                    boxes.W16(NamesOffset + (int)box * NameSize + i * 2, name[i]);
                }
            }
            MessageLoader.MessageLoader_Free(loader);
        }
        boxes.W32(0, 0);
    }
}
