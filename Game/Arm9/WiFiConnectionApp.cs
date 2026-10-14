using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class WiFiConnectionAppArgs
{
    public object? ChildArgs;
    public int Unused4;
    public int Unused8;
    public byte ApplicationID;
}

public sealed class WiFiLibraryHeap
{
}

public sealed class WiFiConnectionAppData
{
    public WiFiConnectionAppArgs? Args;
    public int MonitorConnection;
    public ApplicationManager? ChildApp;
    public byte[]? HeapBuffer;
    public WiFiLibraryHeap? LibraryHeap;
    public int LibraryStarted;
    public int Connected;
}

public static class WiFiConnectionApp
{
    private const int AppHeapID = 0x74;
    private const int ParentHeapID = 3;
    private const uint AppHeapSize = 0xA << 14;
    private const uint AppDataSize = 0x80;
    private const uint LibraryHeapBufferSize = 0x20020;
    private const uint LibraryHeapSize = 0x20000;
    private const uint ConnectScreenTemplate = 0x020E5664;
    private const uint DisplayLayerMask = 0xFFFFE0FF;
    private const uint DisplayWindowMask = 0xFFFF1FFF;
    private const int DisplayMain = 0;
    private const int DisplaySub = 1;
    private const byte SoundScene = 0xB;
    private const ushort BackgroundMusic = 0x497;
    private const int MaxSignalLevel = 3;

    private const int StateStartLibrary = 0;
    private const int StateWaitLibrary = 1;
    private const int StateOpenConnectScreen = 2;
    private const int StateRunConnectScreen = 3;
    private const int StateOpenApplication = 4;
    private const int StateRunApplication = 5;
    private const int StateOpenDisconnectScreen = 6;
    private const int StateRunDisconnectScreen = 7;
    private const int StateDone = 8;

    private static WiFiConnectionAppData? sLibraryHeapOwner;

    // OFFSET 0x02017498
    public static int WiFiConnectionApp_Init(ApplicationManager appMan, ref int state)
    {
        GameSystem.SetVBlankCallback(null, null);
        GameSystem.DisableHBlank();
        GXLayers.GXLayers_DisableEngineALayers();
        GXLayers.GXLayers_DisableEngineBLayers();
        Video.SetDisplayControl(DisplayMain, Video.GetDisplayControl(DisplayMain) & DisplayLayerMask);
        Video.SetDisplayControl(DisplaySub, Video.GetDisplayControl(DisplaySub) & DisplayLayerMask);
        Video.SetDisplayControl(DisplayMain, Video.GetDisplayControl(DisplayMain) & DisplayWindowMask);
        Video.SetDisplayControl(DisplaySub, Video.GetDisplayControl(DisplaySub) & DisplayWindowMask);
        Video.SetBlendControl(DisplayMain, 0);
        Video.SetBlendControl(DisplaySub, 0);
        Heap.Heap_Create(ParentHeapID, AppHeapID, AppHeapSize);
        WiFiConnectionAppData data = new();
        appMan.Data = data;
        data.Args = (WiFiConnectionAppArgs?)ApplicationManager.ApplicationManager_Args(appMan);
        Sound.Sound_SetSceneAndPlayBGM(SoundScene, BackgroundMusic, 1);
        return 1;
    }

    // OFFSET 0x02017524
    public static int WiFiConnectionApp_Main(ApplicationManager appMan, ref int state)
    {
        WiFiConnectionAppData data = (WiFiConnectionAppData)ApplicationManager.ApplicationManager_Data(appMan)!;
        switch (state)
        {
            case StateStartLibrary:
                WiFiConnectionApp_StartLibrary(data);
                state = StateWaitLibrary;
                break;
            case StateWaitLibrary:
                if (sub_020334A4() != 0)
                {
                    sLibraryHeapOwner = data;
                    ov4_021D776C(WiFiConnectionApp_Alloc, WiFiConnectionApp_Free);
                    data.LibraryStarted = 1;
                    state++;
                }
                break;
            case StateOpenConnectScreen:
                data.ChildApp = ApplicationManager.ApplicationManager_New(ApplicationTemplate.At(ConnectScreenTemplate), data, AppHeapID);
                state++;
                break;
            case StateRunConnectScreen:
                if (ApplicationManager.ApplicationManager_Exec(data.ChildApp!))
                {
                    ApplicationManager.ApplicationManager_Free(data.ChildApp!);
                    if (data.Connected == 1)
                    {
                        data.MonitorConnection = 1;
                        state++;
                    }
                    else
                    {
                        state = StateDone;
                    }
                }
                break;
            case StateOpenApplication:
            {
                ApplicationTemplate template = sub_0208BE5C(data.Args!.ApplicationID);
                data.ChildApp = ApplicationManager.ApplicationManager_New(template, data.Args.ChildArgs, AppHeapID);
                state++;
                break;
            }
            case StateRunApplication:
                if (ApplicationManager.ApplicationManager_Exec(data.ChildApp!))
                {
                    ApplicationManager.ApplicationManager_Free(data.ChildApp!);
                    state++;
                }
                break;
            case StateOpenDisconnectScreen:
                data.ChildApp = ApplicationManager.ApplicationManager_New(ApplicationTemplate.At(ConnectScreenTemplate), data, AppHeapID);
                state++;
                break;
            case StateRunDisconnectScreen:
                if (ApplicationManager.ApplicationManager_Exec(data.ChildApp!))
                {
                    ApplicationManager.ApplicationManager_Free(data.ChildApp!);
                    data.MonitorConnection = 0;
                    state++;
                }
                break;
            case StateDone:
                return 1;
        }
        if (data.LibraryStarted == 1 && data.MonitorConnection == 1 && data.Connected == 1)
        {
            ov4_021D8018();
            NetworkIcon.NetworkIcon_SetStrength(MaxSignalLevel - ov4_021D7EAC());
        }
        return 0;
    }

    // OFFSET 0x02017658
    public static int WiFiConnectionApp_Exit(ApplicationManager appMan, ref int state)
    {
        WiFiConnectionAppData data = (WiFiConnectionAppData)ApplicationManager.ApplicationManager_Data(appMan)!;
        WiFiConnectionApp_StopLibrary(data);
        data.Args = null;
        ApplicationManager.ApplicationManager_FreeData(appMan);
        Heap.Heap_Destroy(AppHeapID);
        return 1;
    }

    // OFFSET 0x0201767C
    private static void WiFiConnectionApp_StartLibrary(WiFiConnectionAppData data)
    {
        if (data.LibraryStarted == 0)
        {
            data.HeapBuffer = new byte[LibraryHeapBufferSize];
            data.LibraryHeap = sub_020A5404(data.HeapBuffer, LibraryHeapSize, 0);
            WiFiOverlays.WiFiOverlays_LoadBase();
            WiFiOverlays.WiFiOverlays_LoadLogin();
            sub_02033478();
        }
    }

    // OFFSET 0x020176B4
    private static void WiFiConnectionApp_StopLibrary(WiFiConnectionAppData data)
    {
        if (data.LibraryStarted == 1)
        {
            sub_020A543C(data.LibraryHeap!);
            data.HeapBuffer = null;
            WiFiOverlays.WiFiOverlays_UnloadLogin();
            WiFiOverlays.WiFiOverlays_UnloadBase();
            sub_020334CC();
            data.LibraryStarted = 0;
        }
    }

    // OFFSET 0x020176DC
    private static byte[]? WiFiConnectionApp_Alloc(int name, uint size, int align)
    {
        return sub_020A5448(sLibraryHeapOwner!.LibraryHeap!, size, align);
    }

    // OFFSET 0x02017704
    private static void WiFiConnectionApp_Free(int name, byte[]? memory)
    {
        if (memory != null)
        {
            sub_020A55D8(sLibraryHeapOwner!.LibraryHeap!, memory);
        }
    }
}
