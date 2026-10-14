using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public sealed class ChooseStarterArgs
{
    public int Species;
    public Options? Options;
}

public sealed class StarterInterpolation
{
    public int Value;
    public int Start;
    public int Delta;
    public int Frames;
}

public sealed class StarterCameraMove
{
    public readonly StarterInterpolation Angle = new();
    public readonly StarterInterpolation Distance = new();
    public readonly StarterInterpolation TargetZ = new();
    public Camera? Camera;
    public ChooseStarterData? Target;
    public int Frame;
    public int Done;
    public SysTask? Task;
}

public sealed class StarterModel
{
    public readonly G3dRenderObj RenderObj = new();
    public byte[]? Resource;
    public G3dModelSet? ModelSet;
    public G3dModel? Model;
    public G3dTexture? Texture;
    public byte[]? AnimData;
    public G3dAnim? Anim;
    public G3dAnimObj? AnimObj;
    public int AnimFrame;
    public int Visible;
    public VecFx32 Position;
    public VecFx32 Scale;
    public ushort RotationX;
    public ushort RotationY;
    public ushort RotationZ;
}

public sealed class StarterSpriteAnim
{
    public PokemonSprite? Sprite;
    public readonly StarterInterpolation X = new();
    public readonly StarterInterpolation Y = new();
    public readonly StarterInterpolation Scale = new();
    public int Frame;
    public int Direction;
    public SysTask? Task;
}

public sealed class StarterBob
{
    public int Value;
    public int Amplitude;
    public int Period;
    public int Counter;
}

public sealed class StarterCursor
{
    public Sprite? Sprite;
    public SpriteResource? Tiles;
    public SpriteResource? Palette;
    public SpriteResource? Cell;
    public SpriteResource? Anim;
    public VecFx32 Position;
    public SysTask? Task;
    public readonly StarterBob Bob = new();
}

public sealed class StarterMatte
{
    public SoftwareSpriteChars? Chars;
    public SoftwareSpritePalette? Palette;
    public SoftwareSprite? Sprite;
    public byte[]? CharRaw;
    public byte[]? PaletteRaw;
    public CharacterData? CharData;
    public PaletteFile? PaletteData;
    public readonly StarterInterpolation X = new();
    public readonly StarterInterpolation Y = new();
    public readonly StarterInterpolation Scale = new();
    public int Frame;
    public int Direction;
    public SysTask? Task;
}

public sealed class ChooseStarterData
{
    public const int StarterCount = 3;
    public const int ModelCount = 6;

    public int State;
    public int SubState;
    public int InputLocked;
    public int Counter;
    public readonly StarterCameraMove CameraMove = new();
    public int CursorPosition;
    public readonly VecFx32[] BallPositions = new VecFx32[StarterCount];
    public readonly int[] CursorX = new int[StarterCount];
    public readonly int[] CursorY = new int[StarterCount];
    public BgConfig? BgConfig;
    public Window? MessageWindow;
    public readonly Window?[] LabelWindows = new Window?[StarterCount];
    public int LabelIndex;
    public GameString? MessageString;
    public readonly WindowTemplate YesNoTemplate = new();
    public Menu? YesNoMenu;
    public readonly G2dRenderer Renderer = new();
    public SpriteList? SpriteList;
    public readonly SpriteResourceCollection?[] SpriteResources = new SpriteResourceCollection?[4];
    public PokemonSpriteManager? MonSpriteManager;
    public readonly PokemonSprite?[] MonSprites = new PokemonSprite?[StarterCount];
    public readonly StarterSpriteAnim MonAnim = new();
    public readonly HeapAllocator Allocator = new();
    public readonly G3dAllocator AnimAllocator = new();
    public readonly StarterModel[] Models = { new(), new(), new(), new(), new(), new() };
    public Camera? Camera;
    public VecFx32 CameraTarget;
    public readonly StarterCursor Cursor = new();
    public readonly ushort[] EdgeColors = new ushort[8];
    public SoftwareSpriteManager? SoftSpriteManager;
    public readonly StarterMatte Matte = new();
    public int Frame;
    public int TextFrameDelay;
    public byte PrinterID;
}

public static class ChooseStarter
{
    public const uint Template = 0x020EA1F8;

    private const int Overlay = 78;
    private const int HeapID = 0x2F;
    private const uint HeapSize = 0x40000;
    private const uint DataSize = 0x6B4;
    private const int Archive = 0x52;
    private const int NarcMessages = 0x1A;
    private const int MessageBank = 0x168;
    private const uint BanksAddress = 0x021D29C0;
    private const uint Bg1Template = 0x021D2988;
    private const uint Bg2Template = 0x021D296C;
    private const uint Bg3Template = 0x021D29A4;
    private const ushort BackdropColor = 0x08C3;
    private const ushort EdgeColor = 0x1084;
    private const ushort White = 0x7FFF;
    private const ushort Disp3DToonMask = 0xCFFD;
    private const ushort Disp3DMask = 0xCFFF;
    private const ushort Disp3DAntiAlias = 0x10;
    private const ushort Disp3DAlphaTestMask = 0xCFFB;
    private const ushort Disp3DAlphaBlend = 8;
    private const ushort Disp3DEdgeMarking = 0x20;
    private const int KeyA = 0x001;
    private const int KeyRight = 0x010;
    private const int KeyLeft = 0x020;
    private const int SeqSeConfirm = 0x5DC;
    private const int SeqSeOpenBriefcase = 0x6CA;
    private const int BgmScene = 0x3C;
    private const int Fx32One = 0x1000;
    private const int StateIntroDelay = 0;
    private const int StateOpenDelay = 1;
    private const int StateOpening = 2;
    private const int StateSelect = 3;
    private const int StateConfirm = 4;
    private const int StateDone = 5;
    private const uint TextColorMessage = 0x0001020F;
    private const uint TextColorLabel = 0x0001020A;
    private const int MessageFrameTile = 0x200;
    private const int YesNoFrameTile = 0x29E;
    private const int SpeciesTurtwig = 0x183;
    private const int SpeciesChimchar = 0x186;
    private const int SpeciesPiplup = 0x189;
    private const int MonSpriteHidden = 6;
    private const int MonSpriteX = 0;
    private const int MonSpriteY = 1;
    private const int MonSpriteScaleX = 0xC;
    private const int MonSpriteScaleY = 0xD;
    private const int ModelBriefcaseClosed = 0;
    private const int ModelBriefcaseOpen = 1;
    private const int ModelFirstBall = 2;
    private const int ModelBackground = 5;
    private const int MenuYes = 0;
    private const uint MenuCancel = 0xFFFFFFFE;

    // OFFSET 0x021D0D80
    public static int ChooseStarter_Init(ApplicationManager appMan, ref int state)
    {
        Heap.Heap_Create(3, HeapID, HeapSize);
        ChooseStarterData data = new();
        appMan.Data = data;
        Heap.HeapExp_FndInitAllocator(data.Allocator, HeapID, 0x20);
        ChooseStarterArgs args = (ChooseStarterArgs)ApplicationManager.ApplicationManager_Args(appMan)!;
        data.Frame = Options.Options_Frame(args.Options!);
        data.TextFrameDelay = Options.Options_TextFrameDelay(args.Options!);
        VramTransfer.VramTransfer_New(8, HeapID);
        GameSystem.SetVBlankCallback(ChooseStarter_VBlank, data);
        GameSystem.DisableHBlank();
        TouchPad.EnableTouchPad();
        if (TouchPad.InitializeTouchPad(4) != 1)
        {
            ErrorHandling.GF_AssertFail();
        }
        RenderText.RenderControlFlags_SetCanABSpeedUpPrint(1);
        RenderText.RenderControlFlags_SetAutoScrollFlags(0);
        RenderText.RenderControlFlags_SetSpeedUpOnTouch(0);
        ChooseStarter_InitGraphics(data, HeapID);
        data.BgConfig = Bg.BgConfig_New(HeapID);
        GraphicsModes modes = new() { DisplayMode = 1, MainBgMode = 0, SubBgMode = 1, Bg0As2DOr3D = 1 };
        Bg.SetAllGraphicsModes(modes);
        ChooseStarter_InitBgs(data.BgConfig, HeapID);
        GXLayers.GXLayers_EngineAToggleLayers(1, 1);
        GXLayers.GXLayers_EngineAToggleLayers(2, 0);
        ChooseStarter_InitMessageWindow(data, HeapID);
        ChooseStarter_InitYesNoTemplate(data, HeapID);
        ChooseStarter_InitLabelWindows(data, HeapID);
        ChooseStarter_InitMonSprites(data, HeapID);
        ChooseStarter_InitSoftwareSprites(data, HeapID);
        ChooseStarter_InitSpriteSystem(data, HeapID);
        ChooseStarter_InitModels(data, HeapID);
        ChooseStarter_InitCamera(data, HeapID);
        ChooseStarter_InitPositions(data);
        ChooseStarter_PlaceBalls(data);
        StarterCursor_LoadResources(data, data.Cursor, HeapID);
        StarterCursor_Create(data, data.Cursor, HeapID);
        StarterCursor_StartBob(data.Cursor);
        StarterMatte_Init(data.Matte, data, HeapID);
        Sound.Sound_SetSceneAndPlayBGM(BgmScene, 0, 0);
        return 1;
    }

    // OFFSET 0x021D0EF4
    public static int ChooseStarter_Main(ApplicationManager appMan, ref int state)
    {
        ChooseStarterData data = (ChooseStarterData)ApplicationManager.ApplicationManager_Data(appMan)!;
        switch (state)
        {
            case 0:
                ChooseStarter_StartFadeIn();
                Video.LoadPaletteRange(Video.MainBackground, new[] { BackdropColor }, 0, 0, 2);
                state++;
                break;
            case 1:
                if (ChooseStarter_IsFadeDone())
                {
                    ScreenFadeControl.ResetVisibleHardwareWindows(0);
                    ScreenFadeControl.ResetVisibleHardwareWindows(1);
                    state++;
                }
                break;
            case 2:
            {
                int done = ChooseStarter_HandleInput(data, HeapID);
                ChooseStarter_Update(data, HeapID);
                if (done == 1)
                {
                    state++;
                }
                break;
            }
            case 3:
                ChooseStarter_StartFadeOut();
                state++;
                break;
            case 4:
                if (ChooseStarter_IsFadeDone())
                {
                    ScreenFadeControl.ResetVisibleHardwareWindows(0);
                    ScreenFadeControl.ResetVisibleHardwareWindows(1);
                    return 1;
                }
                break;
        }
        ChooseStarter_Draw(data);
        return 0;
    }

    // OFFSET 0x021D0FA8
    public static int ChooseStarter_Exit(ApplicationManager appMan, ref int state)
    {
        ChooseStarterData data = (ChooseStarterData)ApplicationManager.ApplicationManager_Data(appMan)!;
        ChooseStarterArgs args = (ChooseStarterArgs)ApplicationManager.ApplicationManager_Args(appMan)!;
        GameSystem.SetVBlankCallback(null, null);
        args.Species = ChooseStarter_GetSpecies((ushort)data.CursorPosition);
        if (TouchPad.DisableTouchPad() != 1)
        {
            ErrorHandling.GF_AssertFail();
        }
        StarterMatte_Free(data.Matte);
        StarterCursor_Delete(data.Cursor);
        StarterCursor_FreeResources(data, data.Cursor);
        StarterCursor_StopBob(data.Cursor);
        ChooseStarter_FreeCamera(data);
        ChooseStarter_FreeModels(data);
        ChooseStarter_FreeSpriteSystem(data);
        ChooseStarter_FreeMonSprites(data);
        ChooseStarter_FreeSoftwareSprites(data);
        ChooseStarter_FreeMessageWindow(data);
        ChooseStarter_FreeLabelWindows(data);
        ChooseStarter_FreeBgs(data.BgConfig!);
        data.BgConfig = null;
        ChooseStarter_FreeGraphics();
        VramTransfer.VramTransfer_Free();
        ApplicationManager.ApplicationManager_FreeData(appMan);
        Heap.Heap_Destroy(HeapID);
        return 1;
    }

    // OFFSET 0x021D1058
    private static void ChooseStarter_VBlank(object? param)
    {
        ChooseStarterData data = (ChooseStarterData)param!;
        RenderOam.RenderOam_Transfer();
        Bg.Bg_RunScheduledUpdates(data.BgConfig!);
        PokemonSpriteManager.PokemonSpriteManager_UpdateCharAndPltt(data.MonSpriteManager!);
        VramTransfer.VramTransfer_Process();
    }

    // OFFSET 0x021D107C
    private static void ChooseStarter_StartFadeIn()
    {
        ScreenFadeControl.StartScreenFade(0, 1, 1, 0, 6, 1, HeapID);
    }

    // OFFSET 0x021D109C
    private static void ChooseStarter_StartFadeOut()
    {
        ScreenFadeControl.StartScreenFade(0, 0, 0, 0, 6, 1, HeapID);
    }

    // OFFSET 0x021D10BC
    private static bool ChooseStarter_IsFadeDone()
    {
        return ScreenFadeControl.IsScreenFadeDone();
    }

    // OFFSET 0x021D10C4
    private static void ChooseStarter_InitGraphics(ChooseStarterData data, int heapID)
    {
        ChooseStarter_SetBanks();
        ChooseStarter_InitOam(heapID);
        ChooseStarter_Init3D(data);
    }

    // OFFSET 0x021D10DC
    private static void ChooseStarter_FreeGraphics()
    {
        RenderOam.RenderOam_Free();
        CharTransfer.CharTransfer_Free();
        PlttTransfer.PlttTransfer_Free();
        ChooseStarter_Reset3DVram();
    }

    // OFFSET 0x021D10F0
    private static void ChooseStarter_SetBanks()
    {
        GXLayers.GXLayers_SetBanks(RomStructs.Banks(RomData.Overlay(Overlay, BanksAddress, RomStructs.BanksSize), 0));
    }

    // OFFSET 0x021D1110
    private static void ChooseStarter_InitOam(int heapID)
    {
        RenderOam.RenderOam_Init(0, 0x80, 0, 0x20, 0, 0x80, 0, 0x20, heapID);
        CharTransferTemplate template = new() { MaxTasks = 0x20, SizeMain = 0x14000, SizeSub = 0x4000, HeapID = heapID };
        CharTransfer.CharTransfer_InitWithVramModes(template, 0x200010, 0x10);
        PlttTransfer.PlttTransfer_Init(0x20, heapID);
        CharTransfer.CharTransfer_ClearBuffers();
        PlttTransfer.PlttTransfer_Clear();
    }

    // OFFSET 0x021D1168
    private static void ChooseStarter_Init3D(ChooseStarterData data)
    {
        Render3D.Reset();
        Render3D.Display3DControl &= Disp3DToonMask;
        Render3D.Display3DControl = (ushort)((Render3D.Display3DControl & Disp3DMask) | Disp3DAntiAlias);
        Render3D.Display3DControl &= Disp3DAlphaTestMask;
        Render3D.Display3DControl = (ushort)((Render3D.Display3DControl & Disp3DMask) | Disp3DAlphaBlend);
        Render3D.Display3DControl = (ushort)((Render3D.Display3DControl & Disp3DMask) | Disp3DEdgeMarking);
        for (int i = 0; i < 8; i++)
        {
            data.EdgeColors[i] = EdgeColor;
        }
        Render3D.SetEdgeColorTable(data.EdgeColors);
        Render3D.SetClearColor(0x43FF, 0, 0x7FFF, 0x3F, false);
        Render3D.SwapBuffers(0, 1);
        Render3D.SetViewport(0, 0, 255, 191);
        NnsGfd.InitFrmTexVramManager(1, true);
        NnsGfd.InitFrmPlttVramManager(0x4000, true);
    }

    // OFFSET 0x021D1218
    private static void ChooseStarter_Reset3DVram()
    {
        NnsGfd.ResetFrmTexVramState();
        NnsGfd.ResetFrmPlttVramState();
    }

    // OFFSET 0x021D1224
    private static void ChooseStarter_InitBgs(BgConfig bgConfig, int heapID)
    {
        Video.SetBgControl(0, (ushort)((Video.GetBgControl(0) & ~3) | 1));
        Bg.Bg_InitFromTemplate(bgConfig, 1, RomStructs.BgTemplate(RomData.Overlay(Overlay, Bg1Template, RomStructs.BgTemplateSize), 0), 0);
        Bg.Bg_ClearTilesRange(1, 0x20, 0, heapID);
        Bg.Bg_ClearTilemap(bgConfig, 1);
        Bg.Bg_InitFromTemplate(bgConfig, 2, RomStructs.BgTemplate(RomData.Overlay(Overlay, Bg2Template, RomStructs.BgTemplateSize), 0), 0);
        Bg.Bg_ClearTilesRange(2, 0x20, 0, heapID);
        Bg.Bg_ClearTilemap(bgConfig, 2);
        Bg.Bg_InitFromTemplate(bgConfig, 3, RomStructs.BgTemplate(RomData.Overlay(Overlay, Bg3Template, RomStructs.BgTemplateSize), 0), 0);
        Bg.Bg_ClearTilesRange(3, 0x20, 0, heapID);
        Bg.Bg_ClearTilemap(bgConfig, 3);
    }

    // OFFSET 0x021D12EC
    private static void ChooseStarter_FreeBgs(BgConfig bgConfig)
    {
        Bg.Bg_FreeTilemapBuffer(bgConfig, 1);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 2);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 3);
    }

    // OFFSET 0x021D1308
    private static void ChooseStarter_InitMessageWindow(ChooseStarterData data, int heapID)
    {
        data.MessageWindow = Bg.Window_New(heapID, 1)[0];
        Bg.Window_Init(data.MessageWindow);
        Bg.Window_Add(data.BgConfig!, data.MessageWindow, 1, 4, 0x13, 0x17, 4, 2, 0x1F);
        Bg.Window_FillTilemap(data.MessageWindow, 0xF);
        RenderWindow.LoadMessageBoxGraphics(data.BgConfig!, 1, MessageFrameTile, 0, (byte)data.Frame, heapID);
        Graphics.Graphics_LoadPalette(Archive, 0x10, 0, 0x40, 0x20, heapID);
        RenderWindow.Window_DrawMessageBoxWithScrollCursor(data.MessageWindow, 0, MessageFrameTile, 0);
    }

    // OFFSET 0x021D13A0
    private static void ChooseStarter_FreeMessageWindow(ChooseStarterData data)
    {
        Bg.Window_Remove(data.MessageWindow!);
        data.MessageWindow = null;
    }

    // OFFSET 0x021D13B8
    private static void ChooseStarter_InitMonSprites(ChooseStarterData data, int heapID)
    {
        data.MonSpriteManager = PokemonSpriteManager.PokemonSpriteManager_New(heapID);
        uint texKey = Gfd.AllocTexVram(0x8000, 0, 0);
        uint plttKey = Gfd.AllocPlttVram(0x80, 0, 1);
        PokemonSpriteManager.PokemonSpriteManager_SetCharBaseAddrAndSize(data.MonSpriteManager, (texKey & 0xFFFF) << 3, ((texKey & 0x7FFF0000) >> 16) << 4);
        PokemonSpriteManager.PokemonSpriteManager_SetPlttBaseAddrAndSize(data.MonSpriteManager, (plttKey & 0xFFFF) << 3, ((plttKey & 0xFFFF0000) >> 16) << 3);
        ChooseStarter_CreateMonSprite(out data.MonSprites[0], data, SpeciesTurtwig);
        ChooseStarter_CreateMonSprite(out data.MonSprites[1], data, SpeciesChimchar);
        ChooseStarter_CreateMonSprite(out data.MonSprites[2], data, SpeciesPiplup);
        for (int i = 0; i < ChooseStarterData.StarterCount; i++)
        {
            PokemonSprite.PokemonSprite_SetAttribute(data.MonSprites[i]!, MonSpriteHidden, 1);
        }
    }

    // OFFSET 0x021D1468
    private static void ChooseStarter_CreateMonSprite(out PokemonSprite? sprite, ChooseStarterData data, int species)
    {
        PokemonSpriteTemplate template = new();
        uint gender = SpeciesData.Species_GetGender((ushort)species, 0);
        PokemonSpriteTemplates.BuildPokemonSpriteTemplate(template, (ushort)species, (byte)gender, 2, 0, 0, 0);
        sprite = PokemonSpriteManager.PokemonSpriteManager_CreateSprite(data.MonSpriteManager!, template, 0x80, 0x60, 0x3FF, 0, null, null);
    }

    // OFFSET 0x021D14BC
    private static void ChooseStarter_FreeMonSprites(ChooseStarterData data)
    {
        for (int i = 0; i < ChooseStarterData.StarterCount; i++)
        {
            if (data.MonSprites[i] != null)
            {
                PokemonSprite.PokemonSprite_Delete(data.MonSprites[i]!);
            }
        }
        PokemonSpriteManager.PokemonSpriteManager_Free(data.MonSpriteManager!);
    }

    // OFFSET 0x021D14E8
    private static void ChooseStarter_InitSoftwareSprites(ChooseStarterData data, int heapID)
    {
        SoftwareSpriteManagerTemplate template = new() { NumSprites = 1, NumChars = 1, NumPalettes = 1, HeapID = heapID };
        data.SoftSpriteManager = SoftwareSprite.SoftwareSpriteManager_New(template);
    }

    // OFFSET 0x021D1518
    private static void ChooseStarter_FreeSoftwareSprites(ChooseStarterData data)
    {
        SoftwareSprite.SoftwareSpriteManager_Free(data.SoftSpriteManager);
    }

    // OFFSET 0x021D1528
    private static void ChooseStarter_InitSpriteSystem(ChooseStarterData data, int heapID)
    {
        data.SpriteList = SpriteUtil.SpriteList_InitRendering(2, data.Renderer, heapID);
        for (int i = 0; i < 4; i++)
        {
            data.SpriteResources[i] = SpriteResourceCollection.SpriteResourceCollection_New(2, i, heapID);
        }
        GXLayers.GXLayers_EngineAToggleLayers(0x10, 1);
        GXLayers.GXLayers_EngineBToggleLayers(0x10, 1);
    }

    // OFFSET 0x021D1594
    private static void ChooseStarter_FreeSpriteSystem(ChooseStarterData data)
    {
        Sprite.SpriteList_Delete(data.SpriteList);
        for (int i = 0; i < 4; i++)
        {
            SpriteResourceCollection.SpriteResourceCollection_Delete(data.SpriteResources[i]);
        }
    }

    // OFFSET 0x021D15CC
    private static void StarterModel_InitWithAnim(StarterModel model, int modelMember, int animMember, int heapID, G3dAllocator allocator)
    {
        StarterModel_Clear(model);
        StarterModel_LoadModel(model, modelMember, heapID);
        StarterModel_LoadAnim(model, animMember, heapID, allocator);
        StarterModel_SetScale(model, Fx32One, Fx32One, Fx32One);
    }

    // OFFSET 0x021D1604
    private static void StarterModel_Init(StarterModel model, int modelMember, int heapID)
    {
        StarterModel_Clear(model);
        StarterModel_LoadModel(model, modelMember, heapID);
        StarterModel_SetScale(model, Fx32One, Fx32One, Fx32One);
    }

    private static void StarterModel_Clear(StarterModel model)
    {
        model.Resource = null;
        model.ModelSet = null;
        model.Model = null;
        model.Texture = null;
        model.AnimData = null;
        model.Anim = null;
        model.AnimObj = null;
        model.AnimFrame = 0;
        model.Visible = 0;
        model.Position = default;
        model.Scale = default;
        model.RotationX = 0;
        model.RotationY = 0;
        model.RotationZ = 0;
    }

    // OFFSET 0x021D1630
    private static void StarterModel_LoadModel(StarterModel model, int member, int heapID)
    {
        model.Resource = Graphics.LoadMemberFromNARC(Archive, (uint)member, 0, heapID, 0);
        model.ModelSet = G3d.GetModelSet(model.Resource!);
        model.Model = model.ModelSet != null ? G3d.GetModelByIndex(model.ModelSet, 0) : null;
        model.Texture = G3d.GetTexture(model.Resource!);
        Easy3D.Easy3D_UploadTextureToVRAM(model.Texture);
        Easy3D.Easy3D_BindTextureToResource(model.Resource!, model.Texture);
        G3d.InitRenderObj(model.RenderObj, model.Model!);
    }

    // OFFSET 0x021D1694
    private static void StarterModel_LoadAnim(StarterModel model, int member, int heapID, G3dAllocator allocator)
    {
        model.AnimData = Graphics.LoadMemberFromNARC(Archive, (uint)member, 0, heapID, 0);
        model.Anim = G3d.GetAnimByIndex(model.AnimData!, 0);
        model.AnimObj = G3d.AllocAnimObj(allocator, model.Anim!, model.Model!);
        G3d.InitAnimObj(model.AnimObj, model.Anim!, model.Model!, model.Texture);
        G3d.AddAnimObj(model.RenderObj, model.AnimObj);
    }

    // OFFSET 0x021D16D8
    private static void StarterModel_Free(StarterModel model, G3dAllocator allocator)
    {
        if (model.AnimData != null)
        {
            G3d.FreeAnimObj(allocator, model.AnimObj!);
        }
        StarterModel_Clear(model);
    }

    // OFFSET 0x021D1708
    private static void StarterModel_Draw(StarterModel model)
    {
        Mtx.Identity33(out MtxFx33 rotation);
        int index = model.RotationX >> 4;
        Mtx.RotX33(out MtxFx33 axis, Render3D.Sin(index), Render3D.Cos(index));
        Mtx.Concat33(axis, rotation, out rotation);
        index = model.RotationY >> 4;
        Mtx.RotY33(out axis, Render3D.Sin(index), Render3D.Cos(index));
        Mtx.Concat33(axis, rotation, out rotation);
        index = model.RotationZ >> 4;
        Mtx.RotZ33(out axis, Render3D.Sin(index), Render3D.Cos(index));
        Mtx.Concat33(axis, rotation, out rotation);
        if (model.Visible != 0)
        {
            Easy3D.Easy3D_DrawRenderObj(model.RenderObj, model.Position, rotation, model.Scale);
        }
    }

    // OFFSET 0x021D17A4
    private static void StarterModel_SetVisible(StarterModel model, int visible)
    {
        model.Visible = visible;
    }

    // OFFSET 0x021D17A8
    private static void StarterModel_SetPosition(StarterModel model, int x, int y, int z)
    {
        model.Position.X = x;
        model.Position.Y = y;
        model.Position.Z = z;
    }

    // OFFSET 0x021D17B4
    private static void StarterModel_SetScale(StarterModel model, int x, int y, int z)
    {
        model.Scale.X = x;
        model.Scale.Y = y;
        model.Scale.Z = z;
    }

    // OFFSET 0x021D17CC
    private static void StarterModel_SetRotation(StarterModel model, ushort x, ushort y, ushort z)
    {
        model.RotationX = x;
        model.RotationY = y;
        model.RotationZ = z;
    }

    // OFFSET 0x021D17E4
    private static int StarterModel_AdvanceAnim(StarterModel model)
    {
        int done;
        int end = model.Anim!.NumFrames << 12;
        int next = model.AnimFrame + Fx32One;
        if (next < end)
        {
            model.AnimFrame = next;
            done = 0;
        }
        else
        {
            model.AnimFrame = end;
            done = 1;
        }
        model.AnimObj!.Frame = model.AnimFrame;
        return done;
    }

    // OFFSET 0x021D180C
    private static void StarterModel_AdvanceAnimLooped(StarterModel model)
    {
        model.AnimFrame = (model.AnimFrame + Fx32One) % (model.Anim!.NumFrames << 12);
        model.AnimObj!.Frame = model.AnimFrame;
    }

    // OFFSET 0x021D182C
    private static void StarterModel_SetAnimFrame(StarterModel model, int frame)
    {
        model.AnimFrame = frame;
        model.AnimObj!.Frame = frame;
    }

    // OFFSET 0x021D1834
    private static void ChooseStarter_InitModels(ChooseStarterData data, int heapID)
    {
        StarterModel_InitWithAnim(data.Models[ModelBriefcaseClosed], 1, 0, heapID, data.AnimAllocator);
        StarterModel_SetVisible(data.Models[ModelBriefcaseClosed], 1);
        StarterModel_Init(data.Models[ModelBriefcaseOpen], 8, heapID);
        StarterModel_SetVisible(data.Models[ModelBriefcaseOpen], 0);
        for (int i = 2; i <= 4; i++)
        {
            int member = (i - 2) * 2;
            StarterModel_InitWithAnim(data.Models[i], member + 3, member + 2, heapID, data.AnimAllocator);
            StarterModel_SetVisible(data.Models[i], 0);
        }
        StarterModel background = data.Models[ModelBackground];
        StarterModel_Init(background, 9, heapID);
        StarterModel_SetVisible(background, 1);
        StarterModel_SetPosition(background, 0, unchecked((int)0xFFFE4000), 0x28000);
        StarterModel_SetScale(background, 0x3800, Fx32One, 0x3800);
        StarterModel_SetRotation(background, 0, 0x7FFF, 0);
    }

    // OFFSET 0x021D1908
    private static void ChooseStarter_FreeModels(ChooseStarterData data)
    {
        for (int i = 0; i < ChooseStarterData.ModelCount; i++)
        {
            StarterModel_Free(data.Models[i], data.AnimAllocator);
        }
    }

    // OFFSET 0x021D192C
    private static void ChooseStarter_DrawModels(ChooseStarterData data)
    {
        Render3D.SetGlobalLightVector(0, 0, -Fx32One, 0);
        Render3D.SetGlobalLightColor(0, White);
        Render3D.SetGlobalDiffuseAmbient(White, White, false);
        Render3D.SetGlobalSpecularEmission(White, White, false);
        for (int i = 0; i < ChooseStarterData.ModelCount; i++)
        {
            StarterModel_Draw(data.Models[i]);
        }
    }

    // OFFSET 0x021D1978
    private static int ChooseStarter_HandleInput(ChooseStarterData data, int heapID)
    {
        if (data.InputLocked == 1)
        {
            return 0;
        }
        switch (ChooseStarter_GetState(data))
        {
            case StateSelect:
                ChooseStarter_MoveCursor(data);
                if ((gSystem.PressedKeys & KeyA) != 0)
                {
                    ChooseStarter_AdvanceState(data, 1);
                    SoundPlayback.Sound_PlayEffect(SeqSeConfirm);
                }
                break;
            case StateDone:
                return 1;
        }
        return 0;
    }

    // OFFSET 0x021D19D4
    private static void ChooseStarter_Update(ChooseStarterData data, int heapID)
    {
        switch (ChooseStarter_GetState(data))
        {
            case StateIntroDelay:
                data.InputLocked = 1;
                data.Counter = 0x24;
                ChooseStarter_AdvanceState(data, 1);
                Video.SetBlendAlpha(0, 8, 0x15, 0xA, 6);
                break;
            case StateOpenDelay:
                data.Counter--;
                if (data.Counter < 0)
                {
                    ChooseStarter_AdvanceState(data, 1);
                    SoundPlayback.Sound_PlayEffect(SeqSeOpenBriefcase);
                }
                break;
            case StateOpening:
                if (StarterModel_AdvanceAnim(data.Models[ModelBriefcaseClosed]) != 0)
                {
                    StarterModel_SetVisible(data.Models[ModelBriefcaseClosed], 0);
                    StarterModel_SetVisible(data.Models[ModelBriefcaseOpen], 1);
                    StarterModel_SetVisible(data.Models[2], 1);
                    StarterModel_SetVisible(data.Models[3], 1);
                    StarterModel_SetVisible(data.Models[4], 1);
                    ChooseStarter_AdvanceState(data, 1);
                }
                break;
            case StateSelect:
                ChooseStarter_UpdateSelect(data, heapID);
                break;
            case StateConfirm:
                ChooseStarter_UpdateConfirm(data, heapID);
                break;
            case StateDone:
                Video.SetBlendControl(0, 0);
                break;
        }
    }

    // OFFSET 0x021D1AAC
    private static void ChooseStarter_Draw(ChooseStarterData data)
    {
        G3SwapRequest.G3_ResetG3X();
        Render3D.PushMatrix();
        Render3D.FlushBuffer();
        Render3D.SetupSoftwareSpriteCamera();
        PokemonSpriteManager.PokemonSpriteManager_DrawSprites(data.MonSpriteManager!);
        SoftwareSprite.SoftwareSpriteManager_DrawVisible(data.SoftSpriteManager!);
        Render3D.PopMatrix(1);
        Render3D.PushMatrix();
        Camera.Camera_ComputeViewMatrix();
        ChooseStarter_DrawModels(data);
        Render3D.PopMatrix(1);
        G3SwapRequest.G3_RequestSwapBuffers(0, 0);
        Sprite.SpriteList_Update(data.SpriteList);
    }

    // OFFSET 0x021D1B20
    private static void ChooseStarter_InitCamera(ChooseStarterData data, int heapID)
    {
        data.Camera = Camera.Camera_Alloc(heapID);
        ChooseStarter_SetupCamera(data.Camera, data);
    }

    // OFFSET 0x021D1B3C
    private static void ChooseStarter_SetupCamera(Camera camera, ChooseStarterData data)
    {
        data.CameraTarget = default;
        CameraAngle angle = new() { X = 0xEAAB, Y = 0, Z = 0 };
        Camera.Camera_InitWithTarget(() => data.CameraTarget, 0x12C000, angle, 0xFA4, 0, true, camera);
        VecFx32 up = new() { X = 0, Y = Fx32One, Z = 0 };
        Camera.Camera_SetUp(up, camera);
        Camera.Camera_SetAsActive(camera);
    }

    // OFFSET 0x021D1B90
    private static void ChooseStarter_FreeCamera(ChooseStarterData data)
    {
        Camera.Camera_Delete(data.Camera!);
    }

    // OFFSET 0x021D1BA0
    private static void ChooseStarter_InitPositions(ChooseStarterData data)
    {
        for (int i = 0; i < ChooseStarterData.StarterCount; i++)
        {
            switch (i)
            {
                case 0:
                    data.BallPositions[i].X = -0x2C;
                    data.BallPositions[i].Y = -4;
                    data.BallPositions[i].Z = 0x20;
                    break;
                case 1:
                    data.BallPositions[i].X = 0;
                    data.BallPositions[i].Y = -4;
                    data.BallPositions[i].Z = 0x3E;
                    break;
                case 2:
                    data.BallPositions[i].X = 0x26;
                    data.BallPositions[i].Y = -4;
                    data.BallPositions[i].Z = 0x1A;
                    break;
            }
        }
        for (int i = 0; i < ChooseStarterData.StarterCount; i++)
        {
            switch (i)
            {
                case 0:
                    data.CursorX[i] = 0x4E;
                    data.CursorY[i] = 0x37;
                    break;
                case 1:
                    data.CursorX[i] = 0x82;
                    data.CursorY[i] = 0x52;
                    break;
                case 2:
                    data.CursorX[i] = 0xAC;
                    data.CursorY[i] = 0x32;
                    break;
            }
        }
    }

    // OFFSET 0x021D1C28
    private static void ChooseStarter_PlaceBalls(ChooseStarterData data)
    {
        for (int i = 0; i < ChooseStarterData.StarterCount; i++)
        {
            StarterModel_SetPosition(data.Models[i + ModelFirstBall], data.BallPositions[i].X << 12, data.BallPositions[i].Y << 12, data.BallPositions[i].Z << 12);
        }
    }

    // OFFSET 0x021D1C58
    private static void ChooseStarter_MoveCursor(ChooseStarterData data)
    {
        if ((gSystem.PressedKeys & KeyLeft) != 0)
        {
            int position = data.CursorPosition - 1;
            if (position >= 0)
            {
                data.CursorPosition = position;
                SoundPlayback.Sound_PlayEffect(SeqSeConfirm);
            }
        }
        if ((gSystem.PressedKeys & KeyRight) != 0)
        {
            int position = data.CursorPosition + 1;
            if (position < ChooseStarterData.StarterCount)
            {
                data.CursorPosition = position;
                SoundPlayback.Sound_PlayEffect(SeqSeConfirm);
            }
        }
    }

    // OFFSET 0x021D1C98
    private static void ChooseStarter_AdvanceState(ChooseStarterData data, int amount)
    {
        data.State += amount;
        data.SubState = 0;
    }

    // OFFSET 0x021D1CA4
    private static int ChooseStarter_GetState(ChooseStarterData data)
    {
        return data.State;
    }

    // OFFSET 0x021D1CA8
    private static void ChooseStarter_UpdateSelect(ChooseStarterData data, int heapID)
    {
        switch (data.SubState)
        {
            case 0:
                StarterCameraMove_Start(data.CameraMove, data.Camera!, data);
                GXLayers.GXLayers_EngineAToggleLayers(2, 1);
                data.SubState++;
                break;
            case 1:
                if (StarterCameraMove_IsDone(data.CameraMove) != 0)
                {
                    data.Counter = 6;
                    data.SubState++;
                }
                break;
            case 2:
                data.Counter--;
                if (data.Counter < 0)
                {
                    ChooseStarter_MoveCursorSprite(data);
                    data.SubState++;
                }
                break;
            case 3:
                data.PrinterID = ChooseStarter_PrintMessageKeepString(data.MessageWindow!, heapID, MessageBank, 0, TextColorMessage, (uint)data.TextFrameDelay, ref data.MessageString);
                data.SubState++;
                break;
            case 4:
                if (Text.Text_IsPrinterActive(data.PrinterID) == 0)
                {
                    ChooseStarter_FreeMessage(data);
                    data.SubState++;
                }
                break;
            case 5:
                data.PrinterID = ChooseStarter_PrintMessageKeepString(data.MessageWindow!, heapID, MessageBank, 7, TextColorMessage, (uint)data.TextFrameDelay, ref data.MessageString);
                data.SubState++;
                break;
            case 6:
                if (Text.Text_IsPrinterActive(data.PrinterID) == 0)
                {
                    ChooseStarter_FreeMessage(data);
                    data.SubState++;
                }
                break;
            case 7:
                StarterCursor_SetDrawFlag(data.Cursor, 1);
                data.InputLocked = 0;
                data.SubState++;
                break;
            case 8:
                ChooseStarter_AnimateBalls(data);
                ChooseStarter_MoveCursorSprite(data);
                break;
        }
    }

    // OFFSET 0x021D1DF0
    private static void ChooseStarter_AnimateBalls(ChooseStarterData data)
    {
        for (int i = 0; i < ChooseStarterData.StarterCount; i++)
        {
            if (data.CursorPosition == i)
            {
                StarterModel_AdvanceAnimLooped(data.Models[i + ModelFirstBall]);
            }
            else
            {
                StarterModel_SetAnimFrame(data.Models[i + ModelFirstBall], 0);
            }
        }
    }

    // OFFSET 0x021D1E28
    private static void ChooseStarter_MoveCursorSprite(ChooseStarterData data)
    {
        StarterCursor_SetPosition(data.Cursor, data.CursorX[data.CursorPosition], data.CursorY[data.CursorPosition]);
    }

    // OFFSET 0x021D1E44
    private static void ChooseStarter_UpdateConfirm(ChooseStarterData data, int heapID)
    {
        switch (data.SubState)
        {
            case 0:
                StarterCursor_SetDrawFlag(data.Cursor, 0);
                ChooseStarter_MoveCursorSprite(data);
                ChooseStarter_ClearLabelWindow(data);
                ChooseStarter_StartZoom(data);
                data.SubState++;
                data.InputLocked = 1;
                break;
            case 1:
                StarterMatte_SetVisible(data.Matte, 1);
                PokemonSprite.PokemonSprite_SetAttribute(data.MonSprites[data.CursorPosition]!, MonSpriteHidden, 0);
                if (ChooseStarter_IsZoomDone(data))
                {
                    SoundPlayback.Sound_PlayPokemonCry((ushort)ChooseStarter_GetSpecies((ushort)data.CursorPosition), 0);
                    data.SubState++;
                }
                break;
            case 2:
                ChooseStarter_PrintMessage(data.MessageWindow!, heapID, MessageBank, data.CursorPosition + 1, TextColorMessage, 0xFF);
                data.YesNoMenu = Menu.Menu_MakeYesNoChoice(data.BgConfig!, data.YesNoTemplate, YesNoFrameTile, 1, (uint)heapID);
                data.InputLocked = 0;
                data.SubState++;
                break;
            case 3:
            {
                uint result = Menu.Menu_ProcessInputAndHandleExit(data.YesNoMenu!, (uint)heapID);
                if (result == MenuYes)
                {
                    ChooseStarter_AdvanceState(data, 1);
                }
                else if (result == MenuCancel)
                {
                    data.SubState++;
                    ChooseStarter_ReverseZoom(data);
                }
                break;
            }
            case 4:
                if (ChooseStarter_IsZoomDone(data))
                {
                    ChooseStarter_AdvanceState(data, -1);
                    data.SubState = 7;
                    StarterMatte_SetVisible(data.Matte, 0);
                    PokemonSprite.PokemonSprite_SetAttribute(data.MonSprites[data.CursorPosition]!, MonSpriteHidden, 1);
                    data.PrinterID = ChooseStarter_PrintMessage(data.MessageWindow!, heapID, MessageBank, 7, TextColorMessage, 0xFF);
                }
                break;
        }
    }

    // OFFSET 0x021D1FB4
    private static byte ChooseStarter_PrintMessage(Window window, int heapID, int bank, int messageID, uint color, uint delay)
    {
        MessageLoader? loader = MessageLoader.MessageLoader_Init(0, NarcMessages, bank, heapID);
        if (loader == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        GameString? text = MessageLoader.MessageLoader_GetNewString(loader!, (uint)messageID);
        Bg.Window_FillTilemap(window, 0xF);
        int printer = Text.Text_AddPrinterWithParamsAndColor(window, 1, text, 0, 0, delay, color, null);
        RenderWindow.Window_DrawMessageBoxWithScrollCursor(window, 0, MessageFrameTile, 0);
        GameString.String_Free(text);
        MessageLoader.MessageLoader_Free(loader);
        return (byte)printer;
    }

    // OFFSET 0x021D201C
    private static byte ChooseStarter_PrintMessageKeepString(Window window, int heapID, int bank, int messageID, uint color, uint delay, ref GameString? text)
    {
        if (text != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        MessageLoader? loader = MessageLoader.MessageLoader_Init(0, NarcMessages, bank, heapID);
        if (loader == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        text = MessageLoader.MessageLoader_GetNewString(loader!, (uint)messageID);
        Bg.Window_FillTilemap(window, 0xF);
        int printer = Text.Text_AddPrinterWithParamsAndColor(window, 1, text, 0, 0, delay, color, null);
        RenderWindow.Window_DrawMessageBoxWithScrollCursor(window, 0, MessageFrameTile, 0);
        MessageLoader.MessageLoader_Free(loader);
        return (byte)printer;
    }

    // OFFSET 0x021D2090
    private static void ChooseStarter_FreeMessage(ChooseStarterData data)
    {
        GameString.String_Free(data.MessageString);
        data.MessageString = null;
    }

    // OFFSET 0x021D20A4
    private static void ChooseStarter_InitYesNoTemplate(ChooseStarterData data, int heapID)
    {
        data.YesNoTemplate.BgLayer = 1;
        data.YesNoTemplate.TilemapLeft = 0x17;
        data.YesNoTemplate.TilemapTop = 0xC;
        data.YesNoTemplate.Width = 5;
        data.YesNoTemplate.Height = 4;
        data.YesNoTemplate.Palette = 3;
        data.YesNoTemplate.BaseTile = 0xA7;
        RenderWindow.LoadStandardWindowGraphics(data.BgConfig!, 1, YesNoFrameTile, 1, 0, heapID);
        Font.Font_LoadTextPalette(0, 0x60, heapID);
    }

    // OFFSET 0x021D2108
    private static void StarterInterpolation_Init(StarterInterpolation interpolation, int start, int end, int frames)
    {
        interpolation.Value = start;
        interpolation.Start = start;
        interpolation.Delta = end - start;
        interpolation.Frames = frames;
    }

    // OFFSET 0x021D2114
    private static int StarterInterpolation_Step(StarterInterpolation interpolation, int frame)
    {
        int done = frame >= interpolation.Frames ? 1 : 0;
        interpolation.Value = interpolation.Delta * frame / interpolation.Frames + interpolation.Start;
        return done;
    }

    // OFFSET 0x021D213C
    private static void StarterCameraMove_Start(StarterCameraMove move, Camera camera, ChooseStarterData target)
    {
        if (move.Task != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        StarterInterpolation_Init(move.Angle, unchecked((int)0xFFFFEAAB), unchecked((int)0xFFFFDC72), 6);
        StarterInterpolation_Init(move.Distance, 0x12C000, 0xC8000, 6);
        StarterInterpolation_Init(move.TargetZ, 0, 0x24000, 6);
        move.Done = 0;
        move.Camera = camera;
        move.Target = target;
        move.Frame = 0;
        SysTask.SysTask_Start(StarterCameraMove_Task, move, 0);
    }

    // OFFSET 0x021D219C
    private static void StarterCameraMove_Task(SysTask task, object? param)
    {
        StarterCameraMove move = (StarterCameraMove)param!;
        int done = StarterInterpolation_Step(move.Angle, move.Frame);
        StarterInterpolation_Step(move.Distance, move.Frame);
        StarterInterpolation_Step(move.TargetZ, move.Frame);
        CameraAngle angle = new() { X = (ushort)move.Angle.Value, Y = 0, Z = 0 };
        Camera.Camera_SetAngleAroundTarget(angle, move.Camera!);
        Camera.Camera_SetDistance(move.Distance.Value, move.Camera!);
        move.Target!.CameraTarget.Z = move.TargetZ.Value;
        move.Frame++;
        if (done == 1)
        {
            SysTask.SysTask_Done(task);
            move.Task = null;
            move.Done = 1;
        }
    }

    // OFFSET 0x021D2200
    private static int StarterCameraMove_IsDone(StarterCameraMove move)
    {
        return move.Done;
    }

    // OFFSET 0x021D2204
    private static void StarterCursor_LoadResources(ChooseStarterData data, StarterCursor cursor, int heapID)
    {
        cursor.Tiles = SpriteResourceCollection.SpriteResourceCollection_AddTiles(data.SpriteResources[0], Archive, 0xA, 0, 0xA, 1, heapID);
        SpriteTransfer.SpriteTransfer_RequestCharAtEnd(cursor.Tiles);
        SpriteResource.SpriteResource_ReleaseData(cursor.Tiles);
        cursor.Palette = SpriteResourceCollection.SpriteResourceCollection_AddPalette(data.SpriteResources[1], Archive, 0xB, 0, 0xB, 1, 1, heapID);
        SpriteTransfer.SpriteTransfer_RequestPlttFreeSpace(cursor.Palette);
        SpriteResource.SpriteResource_ReleaseData(cursor.Palette);
        cursor.Cell = SpriteResourceCollection.SpriteResourceCollection_Add(data.SpriteResources[2], Archive, 0xC, 0, 0xC, 2, heapID);
        cursor.Anim = SpriteResourceCollection.SpriteResourceCollection_Add(data.SpriteResources[3], Archive, 0xD, 0, 0xD, 3, heapID);
    }

    // OFFSET 0x021D2290
    private static void StarterCursor_FreeResources(ChooseStarterData data, StarterCursor cursor)
    {
        SpriteTransfer.SpriteTransfer_ResetCharTransfer(cursor.Tiles);
        SpriteTransfer.SpriteTransfer_ResetPlttTransfer(cursor.Palette);
        SpriteResourceCollection.SpriteResourceCollection_Remove(data.SpriteResources[0], cursor.Tiles!);
        SpriteResourceCollection.SpriteResourceCollection_Remove(data.SpriteResources[1], cursor.Palette!);
        SpriteResourceCollection.SpriteResourceCollection_Remove(data.SpriteResources[2], cursor.Cell!);
        SpriteResourceCollection.SpriteResourceCollection_Remove(data.SpriteResources[3], cursor.Anim!);
    }

    // OFFSET 0x021D22D4
    private static void StarterCursor_Create(ChooseStarterData data, StarterCursor cursor, int heapID)
    {
        SpriteResourcesHeader header = new();
        SpriteUtil.SpriteResourcesHeader_Init(header, 0xA, 0xB, 0xC, 0xD, -1, -1, 0, 1, data.SpriteResources[0], data.SpriteResources[1], data.SpriteResources[2], data.SpriteResources[3], null, null);
        SpriteListTemplate template = new()
        {
            List = data.SpriteList,
            ResourceData = header,
            Priority = 0x20,
            VramType = 1,
            HeapID = heapID,
        };
        cursor.Sprite = Sprite.SpriteList_Add(template);
        Sprite.Sprite_SetDrawFlag(cursor.Sprite!, 0);
        cursor.Position.X = 0;
        cursor.Position.Y = 0;
    }

    // OFFSET 0x021D2350
    private static void StarterCursor_Delete(StarterCursor cursor)
    {
        Sprite.Sprite_Delete(cursor.Sprite!);
    }

    // OFFSET 0x021D235C
    private static void StarterBob_Init(StarterBob bob, int amplitude, int period)
    {
        bob.Value = 0;
        bob.Amplitude = amplitude;
        bob.Period = period;
        bob.Counter = 0;
    }

    // OFFSET 0x021D2368
    private static void StarterBob_Step(StarterBob bob)
    {
        ushort angle = (ushort)(bob.Counter * 0xFFFF / bob.Period);
        int sine = Render3D.Sin(angle >> 4);
        bob.Value = (int)(((long)sine * bob.Amplitude + 0x800) >> 12);
        bob.Counter = (bob.Counter + 1) % bob.Period;
    }

    // OFFSET 0x021D23B8
    private static void StarterCursor_StartBob(StarterCursor cursor)
    {
        if (cursor.Task != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        StarterBob_Init(cursor.Bob, 0x8000, 0x20);
        cursor.Task = SysTask.SysTask_Start(StarterCursor_BobTask, cursor, 0);
    }

    // OFFSET 0x021D23E8
    private static void StarterCursor_BobTask(SysTask task, object? param)
    {
        StarterCursor cursor = (StarterCursor)param!;
        StarterBob_Step(cursor.Bob);
        VecFx32 position = cursor.Position;
        position.Y += cursor.Bob.Value;
        Sprite.Sprite_SetPosition(cursor.Sprite!, position);
    }

    // OFFSET 0x021D241C
    private static void StarterCursor_StopBob(StarterCursor cursor)
    {
        if (cursor.Task != null)
        {
            SysTask.SysTask_Done(cursor.Task);
            cursor.Task = null;
        }
    }

    // OFFSET 0x021D2430
    private static void StarterCursor_SetDrawFlag(StarterCursor cursor, int draw)
    {
        Sprite.Sprite_SetDrawFlag(cursor.Sprite!, draw);
    }

    // OFFSET 0x021D243C
    private static void StarterCursor_SetPosition(StarterCursor cursor, int x, int y)
    {
        cursor.Position.X = x << 12;
        cursor.Position.Y = y << 12;
    }

    // OFFSET 0x021D2448
    private static void StarterMatte_Init(StarterMatte matte, ChooseStarterData data, int heapID)
    {
        matte.CharRaw = Graphics.Graphics_GetCharData(Archive, 0xE, 0, out matte.CharData, heapID);
        matte.PaletteRaw = Graphics.Graphics_GetPlttData(Archive, 0xF, out matte.PaletteData, heapID);
        SoftwareSpriteCharsTemplate charsTemplate = new() { SoftSpriteMan = data.SoftSpriteManager!, CharsData = matte.CharData! };
        matte.Chars = SoftwareSprite.SoftwareSprite_LoadChars(charsTemplate);
        SoftwareSpritePaletteTemplate paletteTemplate = new() { SoftSpriteMan = data.SoftSpriteManager!, PaletteData = matte.PaletteData!, PaletteSlot = 1 };
        matte.Palette = SoftwareSprite.SoftwareSprite_LoadPalette(paletteTemplate);
        SoftwareSpriteTemplate template = new()
        {
            SoftSpriteMan = data.SoftSpriteManager!,
            Chars = matte.Chars,
            Palette = matte.Palette,
            XPos = 0,
            YPos = 0,
            Rotation = 0,
            Alpha = 0x1F,
            Priority = 0x3FE,
            PaletteSlot = 0,
        };
        matte.Sprite = SoftwareSprite.SoftwareSprite_Load(template);
        SoftwareSprite.SoftwareSprite_SetVisible(matte.Sprite, 0);
        SoftwareSprite.SoftwareSprite_SetCenter(matte.Sprite, 0x40, 0x40);
    }

    // OFFSET 0x021D24E4
    private static void StarterMatte_Free(StarterMatte matte)
    {
        SoftwareSprite.SoftwareSprite_Reset(matte.Sprite!);
        SoftwareSprite.SoftwareSprite_FreeChars(matte.Chars!);
        SoftwareSprite.SoftwareSprite_FreePalette(matte.Palette!);
        matte.CharRaw = null;
        matte.PaletteRaw = null;
    }

    // OFFSET 0x021D2508
    private static void StarterMatte_SetVisible(StarterMatte matte, int visible)
    {
        SoftwareSprite.SoftwareSprite_SetVisible(matte.Sprite, visible);
    }

    // OFFSET 0x021D2514
    private static void StarterMatte_StartAnim(StarterMatte matte, int startX, int endX, int startY, int endY, int startScale, int endScale, int frames)
    {
        if (matte.Task != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        StarterInterpolation_Init(matte.X, startX, endX, frames);
        StarterInterpolation_Init(matte.Y, startY, endY, frames);
        StarterInterpolation_Init(matte.Scale, startScale, endScale, frames);
        matte.Frame = 0;
        matte.Direction = 1;
        matte.Task = SysTask.SysTask_Start(StarterMatte_Task, matte, 0);
    }

    // OFFSET 0x021D256C
    private static void StarterMatte_ReverseAnim(StarterMatte matte)
    {
        if (matte.Task != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        matte.Direction = -2;
        if (matte.Frame >= matte.X.Frames)
        {
            matte.Frame = matte.X.Frames + matte.Direction;
        }
        matte.Task = SysTask.SysTask_Start(StarterMatte_Task, matte, 0);
    }

    // OFFSET 0x021D25A0
    private static void StarterMatte_Task(SysTask task, object? param)
    {
        StarterMatte matte = (StarterMatte)param!;
        int done = StarterInterpolation_Step(matte.X, matte.Frame);
        StarterInterpolation_Step(matte.Y, matte.Frame);
        StarterInterpolation_Step(matte.Scale, matte.Frame);
        SoftwareSprite.SoftwareSprite_SetPosition(matte.Sprite!, (short)(((matte.X.Value - 0x40000) << 4) >> 16), (short)(((matte.Y.Value - 0x40000) << 4) >> 16));
        SoftwareSprite.SoftwareSprite_SetScalingFactors(matte.Sprite!, matte.Scale.Value, matte.Scale.Value);
        if (done == 1 || matte.Frame < 0)
        {
            SysTask.SysTask_Done(task);
            matte.Task = null;
        }
        matte.Frame += matte.Direction;
    }

    // OFFSET 0x021D2608
    private static bool StarterMatte_IsAnimDone(StarterMatte matte)
    {
        if (matte.Task == null)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x021D2618
    private static void ChooseStarter_StartZoom(ChooseStarterData data)
    {
        int x = data.CursorX[data.CursorPosition] << 12;
        int y = (data.CursorY[data.CursorPosition] + 0x30) << 12;
        StarterMatte_StartAnim(data.Matte, x, 0x80000, y, 0x60000, 0x666, Fx32One, 6);
        StarterSpriteAnim_Start(data.MonAnim, data.MonSprites[data.CursorPosition]!, x, 0x80000, y, 0x60000, 0x666, Fx32One, 6);
    }

    // OFFSET 0x021D2688
    private static void ChooseStarter_ReverseZoom(ChooseStarterData data)
    {
        StarterMatte_ReverseAnim(data.Matte);
        StarterSpriteAnim_Reverse(data.MonAnim);
    }

    // OFFSET 0x021D26A4
    private static bool ChooseStarter_IsZoomDone(ChooseStarterData data)
    {
        return StarterMatte_IsAnimDone(data.Matte);
    }

    // OFFSET 0x021D26B4
    private static void StarterSpriteAnim_Start(StarterSpriteAnim anim, PokemonSprite sprite, int startX, int endX, int startY, int endY, int startScale, int endScale, int frames)
    {
        if (anim.Task != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        StarterInterpolation_Init(anim.X, startX, endX, frames);
        StarterInterpolation_Init(anim.Y, startY, endY, frames);
        StarterInterpolation_Init(anim.Scale, startScale, endScale, frames);
        anim.Sprite = sprite;
        anim.Frame = 0;
        anim.Direction = 1;
        anim.Task = SysTask.SysTask_Start(StarterSpriteAnim_Task, anim, 0);
    }

    // OFFSET 0x021D270C
    private static void StarterSpriteAnim_Reverse(StarterSpriteAnim anim)
    {
        if (anim.Task != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        anim.Direction = -2;
        if (anim.Frame >= anim.X.Frames)
        {
            anim.Frame = anim.X.Frames + anim.Direction;
        }
        anim.Task = SysTask.SysTask_Start(StarterSpriteAnim_Task, anim, 0);
    }

    // OFFSET 0x021D2740
    private static void StarterSpriteAnim_Task(SysTask task, object? param)
    {
        StarterSpriteAnim anim = (StarterSpriteAnim)param!;
        int done = StarterInterpolation_Step(anim.X, anim.Frame);
        StarterInterpolation_Step(anim.Y, anim.Frame);
        StarterInterpolation_Step(anim.Scale, anim.Frame);
        int scale = (int)((((long)anim.Scale.Value << 20) + 0x800) >> 12) >> 12;
        PokemonSprite.PokemonSprite_SetAttribute(anim.Sprite!, MonSpriteX, anim.X.Value >> 12);
        PokemonSprite.PokemonSprite_SetAttribute(anim.Sprite!, MonSpriteY, anim.Y.Value >> 12);
        PokemonSprite.PokemonSprite_SetAttribute(anim.Sprite!, MonSpriteScaleX, scale);
        PokemonSprite.PokemonSprite_SetAttribute(anim.Sprite!, MonSpriteScaleY, scale);
        if (done == 1 || anim.Frame < 0)
        {
            SysTask.SysTask_Done(task);
            anim.Task = null;
        }
        anim.Frame += anim.Direction;
    }

    // OFFSET 0x021D27CC
    private static void ChooseStarter_InitLabelWindows(ChooseStarterData data, int heapID)
    {
        Graphics.Graphics_LoadPalette(Archive, 0x11, 0, 0xA0, 0x20, heapID);
        ushort baseTile = 1;
        for (int i = 0; i < ChooseStarterData.StarterCount; i++)
        {
            Window window = Bg.Window_New(heapID, 1)[0];
            data.LabelWindows[i] = window;
            Bg.Window_Init(window);
            byte left = 0;
            byte top = 0;
            switch (i)
            {
                case 0:
                    left = 0xC;
                    top = 4;
                    break;
                case 1:
                    left = 4;
                    top = 0xE;
                    break;
                case 2:
                    left = 0x14;
                    top = 0xD;
                    break;
            }
            Bg.Window_Add(data.BgConfig!, window, 3, left, top, 0xB, 4, 5, baseTile);
            ChooseStarter_PrintLabel(window, heapID, MessageBank, i + 4, TextColorLabel);
            baseTile += 0x40;
        }
    }

    // OFFSET 0x021D2884
    private static void ChooseStarter_FreeLabelWindows(ChooseStarterData data)
    {
        for (int i = 0; i < ChooseStarterData.StarterCount; i++)
        {
            Bg.Window_Remove(data.LabelWindows[i]!);
            data.LabelWindows[i] = null;
        }
    }

    // OFFSET 0x021D28A8
    private static void ChooseStarter_PrintLabel(Window window, int heapID, int bank, int messageID, uint color)
    {
        MessageLoader? loader = MessageLoader.MessageLoader_Init(0, NarcMessages, bank, heapID);
        if (loader == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        GameString? text = MessageLoader.MessageLoader_GetNewString(loader!, (uint)messageID);
        Bg.Window_FillTilemap(window, (byte)color);
        Text.Text_AddPrinterWithParamsAndColor(window, 0, text, 1, 0, 0xFF, color, null);
        GameString.String_Free(text);
        MessageLoader.MessageLoader_Free(loader);
    }

    // OFFSET 0x021D2904
    private static void ChooseStarter_ClearLabelWindow(ChooseStarterData data)
    {
        Bg.Window_ClearAndCopyToVRAM(data.LabelWindows[data.LabelIndex]!);
    }

    // OFFSET 0x021D291C
    private static int ChooseStarter_GetSpecies(ushort index)
    {
        switch (index)
        {
            case 0:
                return SpeciesTurtwig;
            case 1:
                return SpeciesChimchar;
            case 2:
                return SpeciesPiplup;
        }
        ErrorHandling.GF_AssertFail();
        return 0;
    }
}
