using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class FieldPoketch
{
    private const int OverlayPoketchUnavailable = 0x18;
    private const int OverlayPoketch = 0x19;
    private const int OverlayLoadTypeAsync = 2;
    private const int SubScreen = 1;
    private const int PoketchFlagEnabled = 1;

    // OFFSET 0x020567E0
    public static int Poketch_IsEnabled(Poketch poketch)
    {
        return poketch.U8(0) & PoketchFlagEnabled;
    }

    // OFFSET 0x02056B24
    public static Poketch SaveData_GetPoketch(SaveData saveData)
    {
        return (Poketch)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryPoketch);
    }

    private static bool IsPoketchAvailable(FieldSystem fieldSystem)
    {
        Poketch poketch = SaveData_GetPoketch(fieldSystem.SaveData!);
        VarsFlags varsFlags = VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!);
        return Poketch_IsEnabled(poketch) != 0 && !SystemFlags.SystemFlag_CheckPoketchHidden(varsFlags);
    }

    // OFFSET 0x021EA714
    public static void FieldSystem_SendPoketchEvent(FieldSystem fieldSystem, int eventID, int value)
    {
        if (fieldSystem.FieldMapContext != null && fieldSystem.FieldMapContext.PoketchSystem != null)
        {
            ov25_02253D7C(fieldSystem.FieldMapContext.PoketchSystem, eventID, value);
        }
    }

    // OFFSET 0x021EA728
    public static void FieldSystem_InitPoketch(FieldSystem fieldSystem)
    {
        if (IsPoketchAvailable(fieldSystem))
        {
            GameOverlay.Overlay_LoadByID(OverlayPoketch, OverlayLoadTypeAsync);
            ov25_02253CE8(fieldSystem, fieldSystem.FieldMapContext!, fieldSystem.SaveData!, fieldSystem.BgConfig!, RenderOam.RenderOam_GetScreenOam(SubScreen));
            return;
        }
        GameOverlay.Overlay_LoadByID(OverlayPoketchUnavailable, OverlayLoadTypeAsync);
        PoketchUnavailableScreen.PoketchUnavailableScreen_Init(fieldSystem.BgConfig!);
    }

    // OFFSET 0x021EA790
    public static void FieldSystem_EndPoketch(FieldSystem fieldSystem)
    {
        if (IsPoketchAvailable(fieldSystem))
        {
            ov25_02253D5C(fieldSystem.FieldMapContext!.PoketchSystem);
            return;
        }
        PoketchUnavailableScreen.PoketchUnavailableScreen_Exit(fieldSystem.BgConfig!);
    }

    // OFFSET 0x021EA7CC
    public static int FieldSystem_IsPoketchDone(FieldSystem fieldSystem)
    {
        if (IsPoketchAvailable(fieldSystem))
        {
            if (ov25_02253D70(fieldSystem.FieldMapContext!.PoketchSystem) != 0)
            {
                fieldSystem.FieldMapContext.PoketchSystem = null;
                GameOverlay.Overlay_UnloadByID(OverlayPoketch);
                return 1;
            }
            return 0;
        }
        if (PoketchUnavailableScreen.PoketchUnavailableScreen_IsDone(fieldSystem.BgConfig!) != 0)
        {
            GameOverlay.Overlay_UnloadByID(OverlayPoketchUnavailable);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x021EA830
    public static void FieldSystem_InitPoketchUnavailableScreen(FieldSystem fieldSystem)
    {
        GameOverlay.Overlay_LoadByID(OverlayPoketchUnavailable, OverlayLoadTypeAsync);
        PoketchUnavailableScreen.PoketchUnavailableScreen_Init(fieldSystem.BgConfig!);
    }

    // OFFSET 0x021EA848
    public static void FieldSystem_EndPoketchUnavailableScreen(FieldSystem fieldSystem)
    {
        PoketchUnavailableScreen.PoketchUnavailableScreen_Exit(fieldSystem.BgConfig!);
    }

    // OFFSET 0x021EA854
    public static int FieldSystem_IsPoketchUnavailableScreenDone(FieldSystem fieldSystem)
    {
        if (PoketchUnavailableScreen.PoketchUnavailableScreen_IsDone(fieldSystem.BgConfig!) != 0)
        {
            GameOverlay.Overlay_UnloadByID(OverlayPoketchUnavailable);
            return 1;
        }
        return 0;
    }
}
