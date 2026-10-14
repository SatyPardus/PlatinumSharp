using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class FieldPoketch
{
    private const int OverlayPoketchUnavailable = 0x18;
    private const int OverlayPoketch = 0x19;
    private const int OverlayLoadTypeAsync = 2;
    private const int SubScreen = 1;
    private const int PoketchFlagEnabled = 1;

    // OFFSET 0x020567D0
    public static void Poketch_Enable(Poketch poketch)
    {
        poketch.W8(0, (poketch.U8(0) & ~PoketchFlagEnabled) | PoketchFlagEnabled);
    }

    // OFFSET 0x020567E0
    public static int Poketch_IsEnabled(Poketch poketch)
    {
        return poketch.U8(0) & PoketchFlagEnabled;
    }

    // OFFSET 0x020567E8
    public static byte Poketch_IsAppRegistered(Poketch poketch, int appID)
    {
        return poketch.U8(3 + appID);
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
            PoketchSystems.PoketchSystem_SendEvent((PoketchSystem)fieldSystem.FieldMapContext.PoketchSystem, eventID, (uint)value);
        }
    }

    // OFFSET 0x021EA728
    public static void FieldSystem_InitPoketch(FieldSystem fieldSystem)
    {
        if (IsPoketchAvailable(fieldSystem))
        {
            GameOverlay.Overlay_LoadByID(OverlayPoketch, OverlayLoadTypeAsync);
            PoketchSystems.PoketchSystem_Create(fieldSystem, fieldSystem.FieldMapContext!, fieldSystem.SaveData!, fieldSystem.BgConfig!, RenderOam.RenderOam_GetScreenOam(SubScreen));
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
            PoketchSystems.PoketchSystem_StartShutdown((PoketchSystem)fieldSystem.FieldMapContext!.PoketchSystem!);
            return;
        }
        PoketchUnavailableScreen.PoketchUnavailableScreen_Exit(fieldSystem.BgConfig!);
    }

    // OFFSET 0x021EA7CC
    public static int FieldSystem_IsPoketchDone(FieldSystem fieldSystem)
    {
        if (IsPoketchAvailable(fieldSystem))
        {
            if (PoketchSystems.PoketchSystem_IsSystemShutdown((PoketchSystem?)fieldSystem.FieldMapContext!.PoketchSystem))
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

    // OFFSET 0x021DDAE4
    private static bool FieldTask_EnablePoketch(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        int[] state = (int[])FieldTask.FieldTask_GetEnv(task)!;
        switch (state[0])
        {
            case 0:
                BrightnessController.BrightnessController_StartTransition(2, -16, 0, 0x3F, 2);
                state[0]++;
                break;
            case 1:
                if (BrightnessController.BrightnessController_IsTransitionComplete(2))
                {
                    PoketchUnavailableScreen.PoketchUnavailableScreen_Exit(fieldSystem.BgConfig!);
                    state[0]++;
                }
                break;
            case 2:
                if (PoketchUnavailableScreen.PoketchUnavailableScreen_IsDone(fieldSystem.BgConfig!) != 0)
                {
                    Poketch poketch = SaveData_GetPoketch(fieldSystem.SaveData!);
                    GameOverlay.Overlay_UnloadByID(OverlayPoketchUnavailable);
                    GameOverlay.Overlay_LoadByID(OverlayPoketch, OverlayLoadTypeAsync);
                    Poketch_Enable(poketch);
                    PoketchSystems.PoketchSystem_Create(fieldSystem, fieldSystem.FieldMapContext!, fieldSystem.SaveData!, fieldSystem.BgConfig!, RenderOam.RenderOam_GetScreenOam(SubScreen));
                    state[0]++;
                }
                break;
            case 3:
                BrightnessController.BrightnessController_StartTransition(4, 0, -16, 0x3F, 2);
                state[0]++;
                break;
            case 4:
                if (BrightnessController.BrightnessController_IsTransitionComplete(2))
                {
                    return true;
                }
                break;
        }
        return false;
    }

    // OFFSET 0x021DDBC8
    public static void FieldTask_StartEnablePoketch(FieldTask task)
    {
        FieldTask.FieldTask_InitCall(task, FieldTask_EnablePoketch, new int[1]);
    }
}
