using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static partial class Battle
{
    private const int FontSubscreen = 2;
    private const uint BattleTypeNoRedHPSound = 0x220;
    private const int RedHPSoundPlaying = 1;
    private const int RedHPSoundSuspended = 2;
    private const int RedHPSoundRepeatDelay = 4;
    private const ushort SeqRedHP = 0x704;
    private const uint HealthBarSize = 0x30;
    private const int NarcBattleGraphics = 7;
    private const int NarcBattleSubscreen = 0x1B;
    private const int NarcMessageBanks = 0x1A;
    private const int MessageBankBattle = 0x170;
    private const int MessageBankMoveNames = 0;
    private const int MessageBufferSize = 0x140;
    private const int SpriteTileBufferSize = 0xC80;
    private const int VramTransferTasks = 0x40;
    private const int BattleWindowCount = 3;
    private const int MaxSprites = 0x80;
    private const int SpritePalettes = 0x20;
    private const int PaletteBufferMainBg = 0;
    private const int PaletteBufferSubBg = 1;
    private const int PaletteBufferMainObj = 2;
    private const int PaletteBufferSubObj = 3;
    private const int SavedPaletteColors = 112;
    private const uint BackgroundColorTable = 0x0226E44C;
    private const int BackgroundColorVariants = 3;
    private const int MessageBoxStartOffset = -51;
    private const int MessageBoxFlyInSpeed = 3;
    private const int BgLayerMain1 = 1;
    private const int BgOffsetUpdateSetY = 3;
    private const uint TaskPriorityDrawSprites = 60000;
    private const uint TaskPriorityRedHPSound = 50000;
    private const uint TaskPriorityFlyIn = 0x4B0;
    private const int RenderModeNormal = 0;
    private const int RenderModeSubMenu = 3;
    private const int PokemonAnimCount = 4;
    private const int CellTransferCapacity = 4;
    private const int TouchPadSamplingFrequency = 4;

    private static readonly RenderOamTemplate oamTemplate = new() { MainOamStart = 0, MainOamCount = 128, MainAffineOamStart = 0, MainAffineOamCount = 32, SubOamStart = 0, SubOamCount = 128, SubAffineOamStart = 0, SubAffineOamCount = 32 };
    private static readonly CharTransferTemplateWithModes transferTemplate = new() { MaxTasks = 96, SizeMain = 0x10000, SizeSub = 0x4000, ModeMain = 0x100010, ModeSub = 0x10 };
    private static readonly int[] spriteCapacities = { 96, 32, 64, 64, 8, 8 };

    // OFFSET 0x0223B790
    private static void BattleSystem_Init(ApplicationManager appMan)
    {
        BattleSystem battleSys = (BattleSystem)ApplicationManager.ApplicationManager_Data(appMan)!;
        FieldBattleDTO dto = (FieldBattleDTO)ApplicationManager.ApplicationManager_Args(appMan)!;
        Array.Clear(HardwarePalette.GetHardwareMainBgPaletteAddress(), 0, (int)HardwarePalette.GetHardwareMainBgPaletteSize() / 2);
        Array.Clear(HardwarePalette.GetHardwareSubBgPaletteAddress(), 0, (int)HardwarePalette.GetHardwareSubBgPaletteSize() / 2);
        battleSys.PipelineBuffers = BattleSystem_Init3D();
        GameSystem.DisableHBlank();
        Font.Font_InitManager(FontSubscreen, HeapIDBattle);
        battleSys.SpecialCharsHP = FontSpecialCharsContext.FontSpecialChars_Init(0xE, 2, 0xF, HeapIDBattle);
        battleSys.SpecialCharsLevel = battleSys.SpecialCharsHP;
        battleSys.PaletteData = PaletteData.PaletteData_New(HeapIDBattle);
        PaletteData.PaletteData_SetAutoTransparent(battleSys.PaletteData, 1);
        PaletteData.PaletteData_AllocBuffer(battleSys.PaletteData, PaletteBufferMainBg, 0x200, HeapIDBattle);
        PaletteData.PaletteData_AllocBuffer(battleSys.PaletteData, PaletteBufferSubBg, 0x200, HeapIDBattle);
        PaletteData.PaletteData_AllocBuffer(battleSys.PaletteData, PaletteBufferMainObj, 0x1C0, HeapIDBattle);
        PaletteData.PaletteData_AllocBuffer(battleSys.PaletteData, PaletteBufferSubObj, 0x200, HeapIDBattle);
        battleSys.BgConfig = Bg.BgConfig_New(HeapIDBattle);
        battleSys.Windows = Bg.Window_New(HeapIDBattle, BattleWindowCount);
        for (int i = 0; i < BattleSystem.MaxBattlerSlots; i++)
        {
            battleSys.PokemonSpriteDataArray[i].Tiles = Heap.Heap_Alloc(HeapIDBattle, SpriteTileBufferSize);
        }
        VramTransfer.VramTransfer_New(VramTransferTasks, HeapIDBattle);
        NARC battleNarc = NARC.NARC_ctor(NarcBattleGraphics, HeapIDBattle);
        NARC subscreenNarc = NARC.NARC_ctor(NarcBattleSubscreen, HeapIDBattle);
        battleSys.BtlSubscreen = BattleSubscreens.BattleSubscreen_New(battleNarc, subscreenNarc, battleSys, BattleSystems.BattleSystem_GetTrainerGender(battleSys, BattleSystems.BattleSystem_GetNetworkID(battleSys)), battleSys.SubscreenCursorOn);
        NARC.NARC_dtor(battleNarc);
        NARC.NARC_dtor(subscreenNarc);
        BattleSystem_InitBackgrounds(battleSys, battleSys.BgConfig);
        Bg.Window_Add(battleSys.BgConfig, battleSys.Windows[0], 1, 2, 0x13, 0x1B, 4, 0xB, 0x1F);
        Bg.Window_FillTilemap(battleSys.Windows[0], 0xFF);
        RenderWindow.Window_DrawMessageBoxWithScrollCursor(battleSys.Windows[0], 0, 1, 0xA);
        battleSys.SpriteSys = SpriteSystem.SpriteSystem_Alloc(HeapIDBattle);
        SpriteSystem.SpriteSystem_Init(battleSys.SpriteSys, oamTemplate, transferTemplate, SpritePalettes);
        SpriteUtil.ReserveVramForWirelessIconChars(1, 0x100010);
        SpriteUtil.ReserveSlotsForWirelessIconPalette(1);
        battleSys.SpriteMan = SpriteSystem.SpriteManager_New(battleSys.SpriteSys);
        SpriteSystem.SpriteSystem_InitSprites(battleSys.SpriteSys, battleSys.SpriteMan, MaxSprites);
        SpriteSystem.SpriteSystem_InitManagerWithCapacities(battleSys.SpriteSys, battleSys.SpriteMan, spriteCapacities);
        SpriteUtil.SetSubScreenViewRect(SpriteSystem.SpriteSystem_GetRenderer(battleSys.SpriteSys), 0, 0x110000);
        BattleSubscreens.BattleSubscreen_LoadGraphics(battleSys.BtlSubscreen);
        battleNarc = NARC.NARC_ctor(NarcBattleGraphics, HeapIDBattle);
        subscreenNarc = NARC.NARC_ctor(NarcBattleSubscreen, HeapIDBattle);
        BattleSubscreens.BattleSubscreen_LoadMenu(battleNarc, subscreenNarc, battleSys.BtlSubscreen, 0, 1, null);
        BattleSubscreens.BattleSubscreen_CreatePartyBalls(subscreenNarc, battleSys.BtlSubscreen);
        NARC.NARC_dtor(battleNarc);
        NARC.NARC_dtor(subscreenNarc);
        battleSys.MonSpriteMan = PokemonSpriteManager.PokemonSpriteManager_New(HeapIDBattle);
        PokemonSpriteManager.PokemonSpriteManager_SetPlttBaseAddrAndSize(battleSys.MonSpriteMan, 0, 0xC0);
        BattleSystem_LoadHealthboxes(battleSys);
        BattleSystem_InitParticleVram();
        battleSys.BattleAnimSys = BattleAnimSystems.BattleAnimSystem_New(HeapIDBattle);
        BattleSystem_InitBattlerSprites(battleSys);
        TouchPad.EnableTouchPad();
        TouchPad.InitializeTouchPad(TouchPadSamplingFrequency);
        battleSys.MsgLoader = MessageLoader.MessageLoader_Init(1, NarcMessageBanks, MessageBankBattle, HeapIDBattle);
        battleSys.AttackMsgLoader = MessageLoader.MessageLoader_Init(1, NarcMessageBanks, MessageBankMoveNames, HeapIDBattle);
        battleSys.StrFormatter = StringTemplate.StringTemplate_Default(HeapIDBattle);
        battleSys.MsgBuffer = GameString.String_Init(MessageBufferSize, HeapIDBattle);
        Array.Copy(PaletteData.PaletteData_GetUnfadedBuffer(battleSys.PaletteData, PaletteBufferMainBg)!, battleSys.SavedBgPalettes, SavedPaletteColors);
        Array.Copy(PaletteData.PaletteData_GetUnfadedBuffer(battleSys.PaletteData, PaletteBufferMainObj)!, battleSys.SavedObjPalettes, SavedPaletteColors);
        int variant = BattleSystems.BattleSystem_GetBackgroundTimeOffset(battleSys);
        ushort backgroundColor = (ushort)Nitro.Bin.U32(RomData.Overlay(16, BackgroundColorTable + (uint)(battleSys.Background * BackgroundColorVariants + variant) * 4, 4), 0);
        PaletteData.PaletteData_FillBufferRange(battleSys.PaletteData, PaletteBufferMainBg, 2, backgroundColor, 0, 0x70);
        PaletteData.PaletteData_FillBufferRange(battleSys.PaletteData, PaletteBufferMainBg, 2, backgroundColor, 0xC0, 0x100);
        PaletteData.PaletteData_FillBufferRange(battleSys.PaletteData, PaletteBufferMainObj, 2, backgroundColor, 0, 0xDF);
        PaletteData.PaletteData_FillBufferRange(battleSys.PaletteData, PaletteBufferMainBg, 0, 0, 0xA0, 0xC0);
        PaletteData.PaletteData_FillBufferRange(battleSys.PaletteData, PaletteBufferSubBg, 0, 0, 0, 0xFF);
        PaletteData.PaletteData_FillBufferRange(battleSys.PaletteData, PaletteBufferSubObj, 0, 0xFFFF, 0, 0xFF);
        battleSys.PaletteAnimator = PaletteAnimator.PaletteAnimator_New(battleSys.PaletteData, 0, 0xB, HeapIDBattle);
        PaletteAnimator.PaletteAnimator_SetMode(battleSys.PaletteAnimator, 1);
        battleSys.TaskDrawSprites = SysTask.SysTask_Start(BattleSystem_DrawSpritesTask, battleSys, TaskPriorityDrawSprites);
        battleSys.TaskUpdateRedHPSound = SysTask.SysTask_Start(BattleSystem_UpdateRedHPSoundTask, battleSys, TaskPriorityRedHPSound);
        battleSys.TaskFlyInMessageBox = SysTask.SysTask_ExecuteOnVBlank(BattleSystem_FlyInMessageBoxTask, battleSys, TaskPriorityFlyIn);
        battleSys.YOffset = MessageBoxStartOffset;
        BattleSystems.BattleSystem_SetTextRenderFlags(battleSys);
        BagCursor.BagCursor_ResetBattle(BattleSystems.BattleSystem_GetBagCursor(battleSys)!);
        battleSys.MonAnimMan = PokemonAnimManager.PokemonAnimManager_New(HeapIDBattle, PokemonAnimCount, 0);
        battleSys.CellTransferState = CellTransfer.CellTransfer_New(CellTransferCapacity, HeapIDBattle);
        if ((battleSys.BattleStatusMask & BattleStatusRecording) != 0)
        {
            for (int i = 0; i < BattleSystem.MaxBattlerSlots; i++)
            {
                battleSys.RecordedChatter[i] = dto.Unknown1BC[i];
            }
        }
    }

    // OFFSET 0x0223CD7C
    private static G3DPipelineBuffers BattleSystem_Init3D()
    {
        return G3DPipeline.G3DPipeline_InitEx(HeapIDBattle, 0, 2, 0, 2, BattleSystem_Setup3D);
    }

    // OFFSET 0x0223CD9C
    private static void BattleSystem_Setup3D()
    {
        GXLayers.GXLayers_EngineAToggleLayers(1, 1);
        Video.SetBgControl(0, (ushort)((Video.GetBgControl(0) & ~3) | 1));
        Render3D.Display3DControl &= 0xCFFD;
        Render3D.Display3DControl = (ushort)((Render3D.Display3DControl & 0xCFFF) | 0x10);
        Render3D.Display3DControl &= 0xCFFB;
        Render3D.Display3DControl = (ushort)((Render3D.Display3DControl & 0xCFFF) | 8);
        Render3D.Display3DControl &= 0xCFDF;
        Render3D.SetFog(false, 0, 0, 0);
        Render3D.SetClearColor(0, 0, 0x7FFF, 0x3F, false);
        Render3D.SetViewport(0, 0, 255, 191);
    }

    // OFFSET 0x0223CF48
    private static void BattleSystem_DrawSpritesTask(SysTask task, object? param)
    {
        BattleSystem battleSys = (BattleSystem)param!;
        CommManager.CommMan_DrawErrorHook(HeapIDBattle, battleSys.BgConfig);
        if (battleSys.RenderMode == RenderModeNormal || battleSys.RenderMode == RenderModeSubMenu)
        {
            if (battleSys.RenderMode == RenderModeNormal)
            {
                BattleParticleUtil.BattleParticleUtil_UpdateAndDraw();
            }
            PokemonSpriteManager.PokemonSpriteManager_DrawSprites(battleSys.MonSpriteMan!);
            SpriteSystem.SpriteSystem_DrawSprites(battleSys.SpriteMan);
            SpriteSystem.SpriteSystem_UpdateTransfer();
            G3SwapRequest.G3_RequestSwapBuffers(1, 0);
        }
    }

    // OFFSET 0x0223CF8C
    private static void BattleSystem_UpdateRedHPSoundTask(SysTask task, object? param)
    {
        BattleSystem battleSys = (BattleSystem)param!;
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        uint lowHPMask = 0;
        int flag = BattleSystem_GetRedHPSoundFlag(battleSys);
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeNoRedHPSound) != 0)
        {
            return;
        }
        if ((flag & RedHPSoundSuspended) != 0)
        {
            if ((flag & RedHPSoundPlaying) != 0)
            {
                SoundPlayback.Sound_StopEffect(SeqRedHP, 0);
                BattleSystem_SetRedHPSoundFlag(battleSys, RedHPSoundSuspended);
            }
            return;
        }
        for (int i = 0; i < maxBattlers; i++)
        {
            BattlerData battler = BattleSystems.BattleSystem_GetBattlerData(battleSys, i)!;
            bool check;
            if (Battlers.Battler_GetBootState(battler) == 0 && (BattleSystems.BattleSystem_GetBattleStatusMask(battleSys) & BattleStatusRecording) == 0)
            {
                check = true;
            }
            else
            {
                check = BattleSystems.BattleSystem_GetBattlerSide(battleSys, i) == 0 && (BattleSystems.BattleSystem_GetBattleStatusMask(battleSys) & BattleStatusRecording) != 0;
            }
            if (!check)
            {
                continue;
            }
            HealthBox? healthbox = Battlers.Battler_GetHealthbox(battler);
            if (healthbox == null)
            {
                continue;
            }
            if (HealthBar.HealthBar_Color((ushort)healthbox.CurHP, (ushort)healthbox.MaxHP, HealthBarSize) == HealthBar.ColorRed)
            {
                lowHPMask |= HealthBar.FlagIndex(i);
            }
        }
        if (lowHPMask != 0 && BattleSystem_GetRedHPSoundFlag(battleSys) == 0)
        {
            SoundPlayback.Sound_PlayEffect(SeqRedHP);
            BattleSystem_SetRedHPSoundFlag(battleSys, RedHPSoundPlaying);
            BattleSystem_SetRedHPSoundEffectDelay(battleSys, RedHPSoundRepeatDelay);
        }
        else if (lowHPMask == 0 && BattleSystem_GetRedHPSoundFlag(battleSys) != 0)
        {
            SoundPlayback.Sound_StopEffect(SeqRedHP, 0);
            BattleSystem_SetRedHPSoundFlag(battleSys, 0);
        }
        if (BattleSystem_GetRedHPSoundFlag(battleSys) == 0)
        {
            return;
        }
        int delay = BattleSystem_GetRedHPSoundEffectDelay(battleSys);
        if (SoundPlayback.Sound_IsEffectPlaying(SeqRedHP) != 0)
        {
            return;
        }
        delay--;
        if (delay == 0)
        {
            SoundPlayback.Sound_PlayEffect(SeqRedHP);
            BattleSystem_SetRedHPSoundEffectDelay(battleSys, RedHPSoundRepeatDelay);
        }
        else
        {
            BattleSystem_SetRedHPSoundEffectDelay(battleSys, (byte)delay);
        }
    }

    // OFFSET 0x0223D0C4
    private static void BattleSystem_FlyInMessageBoxTask(SysTask task, object? param)
    {
        BattleSystem battleSys = (BattleSystem)param!;
        battleSys.YOffset += MessageBoxFlyInSpeed;
        Bg.Bg_SetOffset(battleSys.BgConfig!, BgLayerMain1, BgOffsetUpdateSetY, battleSys.YOffset);
        if (battleSys.YOffset == 0)
        {
            SysTask.SysTask_Done(task);
        }
    }
}
