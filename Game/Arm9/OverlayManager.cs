using static PlatinumSharp.Game.GameOverlay;

namespace PlatinumSharp.Game;

public delegate int ApplicationFunc(ApplicationManager appMan, ref int state);

public sealed class ApplicationManager
{
    private const int NoOverlay = -1;

    public ApplicationFunc? Init;
    public ApplicationFunc? Main;
    public ApplicationFunc? Exit;
    public int OverlayID;
    public uint TemplateAddress;
    public int State;
    public int SubState;
    public object? Args;
    public object? Data;
    public int Unused20;
    public int Unused24;

    private static int Run(ApplicationFunc? function, ApplicationManager appMan, string phase)
    {
        if (function == null)
        {
            throw new NotImplementedException($"Application 0x{appMan.TemplateAddress:X8} has no ported {phase} function");
        }
        return function(appMan, ref appMan.SubState);
    }

    // OFFSET 0x020067E8
    public static ApplicationManager ApplicationManager_New(ApplicationTemplate template, object? args, int heapID)
    {
        ApplicationManager appMan = new()
        {
            Init = template.Init,
            Main = template.Main,
            Exit = template.Exit,
            OverlayID = template.OverlayID,
            TemplateAddress = template.Address,
        };
        appMan.State = 0;
        appMan.SubState = 0;
        appMan.Args = args;
        appMan.Data = null;
        appMan.Unused20 = 0;
        appMan.Unused24 = 0;
        return appMan;
    }

    // OFFSET 0x02006814
    public static void ApplicationManager_Free(ApplicationManager appMan)
    {
    }

    // OFFSET 0x0200681C
    public static object ApplicationManager_NewData(ApplicationManager appMan, uint size, int heapID)
    {
        appMan.Data = new byte[size];
        return appMan.Data;
    }

    // OFFSET 0x0200682C
    public static object? ApplicationManager_Data(ApplicationManager appMan)
    {
        return appMan.Data;
    }

    // OFFSET 0x02006830
    public static void ApplicationManager_FreeData(ApplicationManager appMan)
    {
        appMan.Data = null;
    }

    // OFFSET 0x02006840
    public static object? ApplicationManager_Args(ApplicationManager appMan)
    {
        return appMan.Args;
    }

    // OFFSET 0x02006844
    public static bool ApplicationManager_Exec(ApplicationManager appMan)
    {
        switch (appMan.State)
        {
            case 0:
                if (appMan.OverlayID != NoOverlay)
                {
                    Overlay_LoadByID(appMan.OverlayID, LoadNoInitAsync);
                }
                appMan.State = 1;
                goto case 1;
            case 1:
                if (Run(appMan.Init, appMan, "init") == 1)
                {
                    appMan.State = 2;
                    appMan.SubState = 0;
                }
                break;
            case 2:
                if (Run(appMan.Main, appMan, "main") == 1)
                {
                    appMan.State = 3;
                    appMan.SubState = 0;
                }
                break;
            case 3:
                if (Run(appMan.Exit, appMan, "exit") == 1)
                {
                    if (appMan.OverlayID != NoOverlay)
                    {
                        Overlay_UnloadByID(appMan.OverlayID);
                    }
                    return true;
                }
                break;
        }
        return false;
    }
}
