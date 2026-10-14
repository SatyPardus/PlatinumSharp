namespace PlatinumSharp.Game;

public sealed class BlackOutScreen
{
    public int State;
    public FieldSystem? FieldSystem;
    public BgConfig? BgConfig;
    public readonly Window Window = new();
    public MessageLoader? MsgLoader;
    public StringTemplate? StrTemplate;
}

public static class BlackOut
{
    private const int HeapIDFieldTask = 0xB;
    private const int HeapIDFade = 0x20;
    private const uint BanksAddress = 0x020EC324;
    private const uint GraphicsModesAddress = 0x020EC2F8;
    private const uint BgTemplateAddress = 0x020EC308;
    private const uint WindowTemplateAddress = 0x020EC2F0;
    private const int NarcFontPalettes = 0xE;
    private const int FontPaletteMember = 6;
    private const int NarcMessages = 0x1A;
    private const int MessageBankBlackOut = 0x175;
    private const int MapHeaderPlayerHouse = 0x19E;
    private const int MsgBlackOutToHome = 4;
    private const int MsgBlackOutToCenter = 3;
    private const int MessageBufferSize = 0x400;
    private const uint MessageColor = 0xF0200;
    private const int PadButtonsAB = 3;
    private const ushort ScriptBlackOutHome = 0x7E4;
    private const ushort ScriptBlackOutCenter = 0x7E5;

    // OFFSET 0x020528D0
    private static void BlackOut_InitGraphics(BgConfig bgConfig)
    {
        GXLayers.GXLayers_SetBanks(RomStructs.Banks(RomData.Arm9(BanksAddress, RomStructs.BanksSize), 0));
        Bg.SetAllGraphicsModes(RomStructs.GraphicsModes(RomData.Arm9(GraphicsModesAddress, 0x10), 0));
        Bg.Bg_InitFromTemplate(bgConfig, 3, RomStructs.BgTemplate(RomData.Arm9(BgTemplateAddress, RomStructs.BgTemplateSize), 0), 0);
        Graphics.Graphics_LoadPalette(NarcFontPalettes, FontPaletteMember, 0, 0x1A0, 0x20, HeapIDFieldTask);
    }

    // OFFSET 0x02052914
    private static void BlackOut_Start(FieldSystem fieldSystem, FieldTask task)
    {
        BlackOutScreen screen = new();
        screen.State = 0;
        screen.FieldSystem = fieldSystem;
        screen.BgConfig = Bg.BgConfig_New(HeapIDFieldTask);
        BlackOut_InitGraphics(screen.BgConfig);
        screen.MsgLoader = MessageLoader.MessageLoader_Init(1, NarcMessages, MessageBankBlackOut, HeapIDFieldTask);
        screen.StrTemplate = StringTemplate.StringTemplate_Default(HeapIDFieldTask);
        Bg.Window_AddFromTemplate(screen.BgConfig, screen.Window, RomStructs.WindowTemplate(RomData.Arm9(WindowTemplateAddress, 8), 0));
        StringTemplate.StringTemplate_SetPlayerName(screen.StrTemplate, 0, PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!));
        if ((int)fieldSystem.Location!.U32(0) == MapHeaderPlayerHouse)
        {
            BlackOut_PrintMessage(screen, MsgBlackOutToHome, 0, 0);
        }
        else
        {
            BlackOut_PrintMessage(screen, MsgBlackOutToCenter, 0, 0);
        }
        Bg.Window_CopyToVRAM(screen.Window);
        FieldTask.FieldTask_InitCall(task, FieldTask_BlackOutScreen, screen);
    }

    // OFFSET 0x020529C4
    private static bool FieldTask_BlackOutScreen(FieldTask task)
    {
        BlackOutScreen screen = (BlackOutScreen)FieldTask.FieldTask_GetEnv(task)!;
        switch (screen.State)
        {
            case 0:
                ScreenFadeControl.StartScreenFade(3, 1, 0x2A, 0, 8, 1, HeapIDFade);
                screen.State++;
                break;
            case 1:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    screen.State++;
                }
                break;
            case 2:
                if ((Globals.gSystem.PressedKeys & PadButtonsAB) != 0)
                {
                    ScreenFadeControl.StartScreenFade(0, 0, 0, 0, 8, 1, HeapIDFade);
                    screen.State++;
                }
                break;
            case 3:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    Bg.Window_FillTilemap(screen.Window, 0);
                    screen.State++;
                }
                break;
            case 4:
                RenderWindow.Window_EraseMessageBox(screen.Window, 0);
                Bg.Window_Remove(screen.Window);
                StringTemplate.StringTemplate_Free(screen.StrTemplate!);
                MessageLoader.MessageLoader_Free(screen.MsgLoader!);
                Bg.Bg_FreeTilemapBuffer(screen.BgConfig!, 3);
                return true;
        }
        return false;
    }

    // OFFSET 0x02052AA4
    private static void BlackOut_PrintMessage(BlackOutScreen screen, int messageID, int unused, int y)
    {
        GameString format = GameString.String_Init(MessageBufferSize, HeapIDFieldTask);
        GameString message = GameString.String_Init(MessageBufferSize, HeapIDFieldTask);
        Bg.Window_FillTilemap(screen.Window, 0);
        MessageLoader.MessageLoader_GetString(screen.MsgLoader!, (uint)messageID, format);
        StringTemplate.StringTemplate_Format(screen.StrTemplate!, message, format);
        int width = Font.Font_CalcMaxLineWidth(0, message, 0);
        byte x = (byte)((((byte)(screen.Window.Width * 8 - width)) >> 1) - 4);
        Text.Text_AddPrinterWithParamsAndColor(screen.Window, 0, message, x, (uint)y, 0xFF, MessageColor, null);
        GameString.String_Free(format);
        GameString.String_Free(message);
    }

    // OFFSET 0x02052B2C
    public static bool FieldTask_BlackOut(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        ref int state = ref FieldTask.FieldTask_GetState(task);
        switch (state)
        {
            case 0:
            {
                if (fieldSystem != null && fieldSystem.SaveData != null)
                {
                    Pokemon.Party_SetGiratinaForm(Party.SaveData_GetParty(fieldSystem.SaveData), 0);
                }
                FieldPlayerState playerState = FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem!.SaveData!);
                int warpId = FieldPlayerState.FieldPlayerState_GetBlackOutWarpId(playerState);
                Location location = new();
                SpawnLocations.Location_InitBlackOut(warpId, location);
                SpawnLocations.Location_InitFly(warpId, FieldPlayerState.FieldPlayerState_GetExitLocation(playerState));
                FieldMapChange.FieldTask_ChangeMapByLocation(task, location);
                FieldMapChangeFlags.FieldSystem_ClearPartnerTrainer(fieldSystem);
                state++;
                break;
            }
            case 1:
                SoundPlayback.Sound_FadeOutBGM(0, 0x14);
                state++;
                break;
            case 2:
                if (SoundPlayback.Sound_IsFadeActive() == 0)
                {
                    FieldBgm.FieldBGM_Stop();
                    state++;
                }
                break;
            case 3:
                BrightnessController.BrightnessController_SetScreenBrightness(-16, 0x37, 1);
                BrightnessController.BrightnessController_SetScreenBrightness(-16, 0x3F, 2);
                BlackOut_Start(fieldSystem, task);
                state++;
                break;
            case 4:
                FieldTransition.FieldTransition_StartMap(task);
                state++;
                break;
            case 5:
                BrightnessController.BrightnessController_SetScreenBrightness(0, 0x3F, 3);
                if (FieldPlayerState.FieldPlayerState_DefaultBlackOutWarpId() == FieldPlayerState.FieldPlayerState_GetBlackOutWarpId(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!)))
                {
                    ScriptManager.ScriptManager_Start(task, ScriptBlackOutHome, null, null);
                }
                else
                {
                    ScriptManager.ScriptManager_Start(task, ScriptBlackOutCenter, null, null);
                }
                state++;
                break;
            case 6:
                return true;
        }
        return false;
    }

    // OFFSET 0x02052C5C
    public static void FieldTask_StartBlackOut(FieldTask task)
    {
        FieldTask.FieldTask_InitCall(task, FieldTask_BlackOut, null);
    }
}
