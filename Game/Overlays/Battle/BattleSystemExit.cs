namespace PlatinumSharp.Game;

public static partial class Battle
{
    private const int OverlayWiFiBattle = 0x15;
    private const int ResultCaught = 4;
    private const ushort SeqRedHPWarning = 0x704;
    private const int MasterVolumeFull = 0x7F;
    private const int BattleContextParamLevelUpMons = 4;
    private const int BattleContextParamTotalTurns = 3;
    private const int BattleContextParamTotalFainted = 6;
    private const int BattleContextParamTotalDamage = 7;
    private const int RenderModeSubMenuOnly = 2;

    // OFFSET 0x0223BCB4
    private static void BattleSystem_Exit(ApplicationManager appMan)
    {
        BattleSystem battleSys = (BattleSystem)ApplicationManager.ApplicationManager_Data(appMan)!;
        FieldBattleDTO dto = (FieldBattleDTO)ApplicationManager.ApplicationManager_Args(appMan)!;
        dto.Seed = battleSys.SeedDTO;
        dto.BattleStatusMask = battleSys.BattleStatusMask;
        if ((battleSys.BattleStatusMask & BattleStatusRecording) == 0)
        {
            BattleRecording.BattleRecording_SaveBattleParams(dto);
        }
        if (battleSys.OverlayFlags != 0)
        {
            Battle_SetOverlay(battleSys, 0);
        }
        ScreenFadeControl.SetScreenColorBrightness(0, 0);
        ScreenFadeControl.SetScreenColorBrightness(1, 0);
        BattleSystems.BattleSystem_UpdateBurmyForms(battleSys);
        if (battleSys.ResultMask != ResultCaught)
        {
            BattleSystems.BattleSystem_RecordCaptureAttempt(battleSys, Party.Party_GetPokemonBySlotIndex(battleSys.Parties[1]!, 0));
        }
        for (int i = 0; i < BattleSystem.MaxBattlerSlots; i++)
        {
            Party.Party_Copy(battleSys.Parties[i]!, dto.Parties[i]!);
            battleSys.Parties[i] = null;
            TrainerInfo.TrainerInfo_Copy(battleSys.TrainerInfos[i]!, dto.TrainerInfos[i]!);
            battleSys.TrainerInfos[i] = null;
        }
        PaletteAnimator.PaletteAnimator_Free(battleSys.PaletteAnimator!);
        BagItems.Bag_Copy(battleSys.Bag!, dto.Bag!);
        battleSys.Bag = null;
        Pokedex.Pokedex_Copy(battleSys.Pokedex!, dto.Pokedex!);
        battleSys.Pokedex = null;
        dto.PCBoxes = battleSys.PcBoxes;
        dto.BagCursor = battleSys.BagCursor;
        dto.SubscreenCursorOn = battleSys.SubscreenCursorOn;
        dto.Poketch = battleSys.Poketch;
        dto.CaptureAttempt = battleSys.CaptureAttempt;
        dto.CountSafariBalls = battleSys.SafariBalls;
        dto.ResultMask = battleSys.ResultMask & 0x3F;
        dto.CaughtBattlerIndex = battleSys.CaughtBattlerIdx;
        BattleContext battleCtx = battleSys.BattleCtx!;
        dto.LeveledUpMonsMask = (uint)BattleContexts.BattleContext_Get(battleSys, battleCtx, BattleContextParamLevelUpMons, 0);
        BattleSystem_AddBattleRecord(dto, 0, BattleContexts.BattleContext_Get(battleSys, battleCtx, BattleContextParamTotalTurns, 0));
        BattleSystem_AddBattleRecord(dto, 4, BattleContexts.BattleContext_Get(battleSys, battleCtx, BattleContextParamTotalFainted, 0) + BattleContexts.BattleContext_Get(battleSys, battleCtx, BattleContextParamTotalFainted, 2));
        BattleSystem_AddBattleRecord(dto, 8, BattleContexts.BattleContext_Get(battleSys, battleCtx, BattleContextParamTotalDamage, 0) + BattleContexts.BattleContext_Get(battleSys, battleCtx, BattleContextParamTotalDamage, 2));
        dto.TotalTurnsElapsed = BattleContexts.BattleContext_Get(battleSys, battleCtx, BattleContextParamTotalTurns, 0);
        dto.RecordingStopped = (uint)(battleSys.RecordingStopped & 1);
        for (int i = 0; i < BattleSystem.MaxBattlerSlots; i++)
        {
            battleSys.PokemonSpriteDataArray[i].Tiles = null;
        }
        GameString.String_Free(battleSys.MsgBuffer);
        PaletteData.PaletteData_FreeBuffer(battleSys.PaletteData!, 0);
        PaletteData.PaletteData_FreeBuffer(battleSys.PaletteData!, 1);
        PaletteData.PaletteData_FreeBuffer(battleSys.PaletteData!, 2);
        PaletteData.PaletteData_FreeBuffer(battleSys.PaletteData!, 3);
        PaletteData.PaletteData_Free(battleSys.PaletteData!);
        MessageLoader.MessageLoader_Free(battleSys.MsgLoader);
        MessageLoader.MessageLoader_Free(battleSys.AttackMsgLoader);
        StringTemplate.StringTemplate_Free(battleSys.StrFormatter);
        PokemonAnimManager.PokemonAnimManager_Free(battleSys.MonAnimMan!);
        ParticleSystem.ParticleSystem_FreeAll();
        BattleAnimSystems.BattleAnimSystem_Delete(battleSys.BattleAnimSys);
        BattleContexts.BattleContext_Free(battleSys.BattleCtx!);
        for (int i = 0; i < battleSys.MaxBattlers; i++)
        {
            Battlers.Battler_Free(battleSys, battleSys.Battlers[i]!, battleSys.RenderMode);
        }
        PokemonSpriteManager.PokemonSpriteManager_Free(battleSys.MonSpriteMan!);
        if (battleSys.RenderMode != RenderModeSubMenuOnly)
        {
            BattleSystem_FreeGraphics(battleSys);
        }
        RenderText.RenderControlFlags_SetCanABSpeedUpPrint(0);
        RenderText.RenderControlFlags_SetAutoScrollFlags(0);
        RenderText.RenderControlFlags_SetSpeedUpOnTouch(0);
        Bg.Windows_Delete(battleSys.Windows!, BattleWindowCount);
        battleSys.BgConfig = null;
        battleSys.BgTileSnapshot = null;
        battleSys.BgPaletteSnapshot = null;
        FontSpecialCharsContext.FontSpecialChars_Free(battleSys.SpecialCharsHP);
        Font.Font_Free(FontSubscreen);
        SysTask.SysTask_Done(battleSys.TaskDrawSprites!);
        SysTask.SysTask_Done(battleSys.TaskUpdateRedHPSound!);
        TouchPad.DisableTouchPad();
        BattleSystem_Free3D(battleSys.PipelineBuffers!);
        MathUtil.LCRNG_SetSeed(battleSys.SeedLCRNG);
        if (BattleSystem_GetRedHPSoundFlag(battleSys) != 0)
        {
            SoundPlayback.Sound_StopEffect(SeqRedHPWarning, 0);
        }
        CellTransfer.CellTransfer_Free(battleSys.CellTransferState);
        if (BattleSystems.BattleSystem_RecordingStopped(battleSys) != 0)
        {
            Sound.Sound_SetMasterVolume(MasterVolumeFull);
        }
        if (battleSys.PlaybackStopButton != null)
        {
            BattleSubscreens.BattleSubscreen_FreePlaybackStopButton(battleSys.PlaybackStopButton);
        }
        GameOverlay.Overlay_UnloadByID(OverlayBattleController);
        GameOverlay.Overlay_UnloadByID(OverlayBattleAnim);
        if (CommManager.CommMan_IsWiFi() == 0)
        {
            GameOverlay.Overlay_UnloadByID(OverlayWiFiBattle);
        }
    }

    private static void BattleSystem_AddBattleRecord(FieldBattleDTO dto, int offset, int value)
    {
        Nitro.Bin.W32(dto.BattleRecords, offset, (uint)((int)Nitro.Bin.U32(dto.BattleRecords, offset) + value));
    }

    // OFFSET 0x0223B3E4
    public static void BattleSystem_FreeGraphics(BattleSystem battleSys)
    {
        GameSystem.SetVBlankCallback(null, null);
        BattleSubscreens.BattleSubscreen_Free(battleSys.BtlSubscreen!);
        Bg.Window_Remove(battleSys.Windows![0]);
        BattleSystem_FreeBackgrounds(battleSys.BgConfig!);
        BattleSystem_FreeUnused(battleSys);
        SpriteSystem.SpriteSystem_FreeResourcesAndManager(battleSys.SpriteSys!, battleSys.SpriteMan!);
        SpriteSystem.SpriteSystem_Free(battleSys.SpriteSys!);
        VramTransfer.VramTransfer_Free();
        Font.Font_Free(FontSubscreen);
    }

    // OFFSET 0x0223C288
    private static void BattleSystem_FreeBackgrounds(BgConfig bgConfig)
    {
        GXLayers.GXLayers_EngineAToggleLayers(1, 0);
        GXLayers.GXLayers_EngineAToggleLayers(2, 0);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 1);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 2);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 3);
        BattleSubscreens.BattleSubscreen_FreeBackgrounds(bgConfig);
    }

    // OFFSET 0x0223C2BC
    private static void BattleSystem_FreeUnused(BattleSystem battleSys)
    {
    }

    // OFFSET 0x0223CE20
    private static void BattleSystem_Free3D(G3DPipelineBuffers buffers)
    {
        G3DPipeline.G3DPipelineBuffers_Free(buffers);
    }
}

public static partial class BattleSystems
{
    private const uint BattleTypeNoFormChange = 0x2A4;
    private const int MonDataSpeciesOrEgg = 0xAE;
    private const int MonDataFormValue = 0x70;
    private const int SpeciesBurmy = 0x19C;
    private const int BurmyPlantCloak = 0;
    private const int BurmySandyCloak = 1;
    private const int BurmyTrashCloak = 2;
    private const int CaptureAttemptHeapID = 5;

    // OFFSET 0x0223EE70
    public static void BattleSystem_UpdateBurmyForms(BattleSystem battleSys)
    {
        if ((battleSys.BattleType & BattleTypeNoFormChange) != 0)
        {
            return;
        }
        for (int slot = 0; slot < BattleSystem_GetPartyCount(battleSys, 0); slot++)
        {
            Pokemon mon = BattleSystem_GetPartyPokemon(battleSys, 0, slot);
            if ((ushort)Pokemon.Pokemon_GetValue(mon, MonDataSpeciesOrEgg, null) != SpeciesBurmy || (HealthBar.FlagIndex(slot) & battleSys.BattleParticipantMask[0]) == 0)
            {
                continue;
            }
            int form;
            switch (BattleSystem_GetTerrain(battleSys))
            {
                case 0:
                case 1:
                case 4:
                case 5:
                case 17:
                case 23:
                    form = BurmySandyCloak;
                    break;
                case 9:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 18:
                case 19:
                case 20:
                case 21:
                case 22:
                    form = BurmyTrashCloak;
                    break;
                default:
                    form = BurmyPlantCloak;
                    break;
            }
            Pokemon.Pokemon_SetValue(mon, MonDataFormValue, form);
        }
    }

    // OFFSET 0x0223EF68
    public static void BattleSystem_RecordCaptureAttempt(BattleSystem battleSys, Pokemon mon)
    {
        CaptureAttempt.CaptureAttempt_Init(battleSys.CaptureAttempt!, mon, battleSys.ResultMask, battleSys.BallsThrown, CaptureAttemptHeapID);
    }
}

public static partial class BattleContexts
{
    // OFFSET 0x0224B9DC
    public static void BattleContext_Free(BattleContext battleCtx)
    {
        battleCtx.AiContext.ItemTable = null;
    }
}

public static partial class Battlers
{
    // OFFSET 0x0225C104
    public static void Battler_Free(BattleSystem battleSys, BattlerData battler, int renderMode)
    {
        if (renderMode != 2)
        {
            Healthboxes.Healthbox_Free(battler.Healthbox);
        }
        if (battler.ManagedSprite != null)
        {
            SpriteSystem.Sprite_DeleteAndFreeResources(battler.ManagedSprite);
        }
        Battler_StopIdleBob(battler);
        NARC.NARC_dtor(battler.Narc!);
    }
}
