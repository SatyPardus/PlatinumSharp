using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed partial class FieldSystem
{
    public const int CommBottomScreenOffset = 0x84;

    public object? CommBottomScreen;
}

public static class FieldBottomScreen
{
    public const int BottomScreenPoketch = 1;
    public const int BottomScreenUnderground = 2;
    public const int BottomScreenComm = 3;
    public const int BottomScreenUnion = 4;
    public const int BottomScreenNone = 5;

    private const int OverlayCommBottomScreen = 0x38;
    private const int OverlayLoadTypeAsync = 2;

    private sealed class Handlers
    {
        public Action<FieldSystem>? Init;
        public Func<FieldSystem, int>? IsRunning;
        public Action<FieldSystem>? End;
        public Func<FieldSystem, int>? IsDone;
    }

    private static readonly Handlers[] sHandlers =
    {
        new() { Init = FieldBottomScreen_InitPoketch, IsRunning = null, End = FieldBottomScreen_EndPoketch, IsDone = FieldBottomScreen_IsPoketchDone },
        new() { Init = FieldBottomScreen_InitUnderground, IsRunning = null, End = FieldBottomScreen_EndUnderground, IsDone = FieldBottomScreen_IsUndergroundDone },
        new() { Init = FieldBottomScreen_InitComm, IsRunning = null, End = FieldBottomScreen_EndComm, IsDone = FieldBottomScreen_IsCommDone },
        new() { Init = FieldPoketch.FieldSystem_InitPoketchUnavailableScreen, IsRunning = null, End = FieldPoketch.FieldSystem_EndPoketchUnavailableScreen, IsDone = FieldPoketch.FieldSystem_IsPoketchUnavailableScreenDone },
    };

    // OFFSET 0x021D5BC0
    private static int FieldBottomScreen_GetHandlerIndex(FieldSystem fieldSystem)
    {
        int bottomScreen = fieldSystem.BottomScreen;
        if (bottomScreen == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (bottomScreen >= BottomScreenNone)
        {
            ErrorHandling.GF_AssertFail();
        }
        return bottomScreen - 1;
    }

    // OFFSET 0x021D5BD8
    public static void FieldSystem_InitBottomScreen(FieldSystem fieldSystem)
    {
        sHandlers[FieldBottomScreen_GetHandlerIndex(fieldSystem)].Init!(fieldSystem);
    }

    // OFFSET 0x021D5BF4
    public static int FieldSystem_IsBottomScreenRunning(FieldSystem fieldSystem)
    {
        Func<FieldSystem, int>? isRunning = sHandlers[FieldBottomScreen_GetHandlerIndex(fieldSystem)].IsRunning;
        if (isRunning == null)
        {
            return 1;
        }
        return isRunning(fieldSystem);
    }

    // OFFSET 0x021D5C14
    public static void FieldSystem_EndBottomScreen(FieldSystem fieldSystem)
    {
        sHandlers[FieldBottomScreen_GetHandlerIndex(fieldSystem)].End!(fieldSystem);
    }

    // OFFSET 0x021D5C30
    public static int FieldSystem_IsBottomScreenDone(FieldSystem fieldSystem)
    {
        return sHandlers[FieldBottomScreen_GetHandlerIndex(fieldSystem)].IsDone!(fieldSystem);
    }

    // OFFSET 0x021D5C4C
    private static void FieldBottomScreen_InitPoketch(FieldSystem fieldSystem)
    {
        FieldPoketch.FieldSystem_InitPoketch(fieldSystem);
    }

    // OFFSET 0x021D5C54
    private static void FieldBottomScreen_InitUnderground(FieldSystem fieldSystem)
    {
        UndergroundManager.UndergroundMan_InitBottomScreen(fieldSystem);
    }

    // OFFSET 0x021D5C5C
    private static void FieldBottomScreen_InitComm(FieldSystem fieldSystem)
    {
        GameOverlay.Overlay_LoadByID(OverlayCommBottomScreen, OverlayLoadTypeAsync);
        fieldSystem.CommBottomScreen = ov56_02256410(fieldSystem);
    }

    // OFFSET 0x021D5C78
    private static void FieldBottomScreen_EndPoketch(FieldSystem fieldSystem)
    {
        FieldPoketch.FieldSystem_EndPoketch(fieldSystem);
    }

    // OFFSET 0x021D5C80
    private static void FieldBottomScreen_EndUnderground(FieldSystem fieldSystem)
    {
        UndergroundManager.UndergroundMan_EndBottomScreen(fieldSystem);
    }

    // OFFSET 0x021D5C88
    private static void FieldBottomScreen_EndComm(FieldSystem fieldSystem)
    {
        ov56_02256468(fieldSystem.CommBottomScreen);
        GameOverlay.Overlay_UnloadByID(OverlayCommBottomScreen);
    }

    // OFFSET 0x021D5CA0
    private static int FieldBottomScreen_IsPoketchDone(FieldSystem fieldSystem)
    {
        return FieldPoketch.FieldSystem_IsPoketchDone(fieldSystem);
    }

    // OFFSET 0x021D5CA8
    private static int FieldBottomScreen_IsCommDone(FieldSystem fieldSystem)
    {
        return 1;
    }

    // OFFSET 0x021D5CAC
    private static int FieldBottomScreen_IsUndergroundDone(FieldSystem fieldSystem)
    {
        return 1;
    }
}
