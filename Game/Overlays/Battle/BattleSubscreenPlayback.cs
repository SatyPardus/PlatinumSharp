namespace PlatinumSharp.Game;

public sealed class PlaybackStopButtonData
{
    public BattleSystem? BattleSys;
    public short State;
    public short Counter;
}

public static partial class BattleSubscreens
{
    private const int StopFirstBattleMenuIndex = 0x12;
    private const int NarcBattleSubscreenGraphics = 0x1B;
    private const uint RecordingMessageTable = 0x02270234;
    private const uint TaskPriorityPlaybackButton = 1000;
    private const int PlaybackStopSfx = 0x5DD;
    private const int PlaybackStopDelay = 8;
    private const uint ColorRecordingMessage = 0x10200;
    private const int RecordingWindowLayer = SubscreenFirstLayer + 1;
    private const ushort RecordingFrameTile = 0x20;

    // OFFSET 0x0226CEB0
    public static void BattleSubscreen_ShowRecordingStoppedMessage(BattleSubscreen subscreen, int messageSet)
    {
        BgConfig bgConfig = BattleSystems.BattleSystem_GetBgConfig(subscreen.BattleSys!)!;
        PaletteData paletteData = BattleSystems.BattleSystem_GetPaletteData(subscreen.BattleSys!)!;
        if (subscreen.ActiveMenuConfigIndex != StopFirstBattleMenuIndex)
        {
            ErrorHandling.GF_AssertFail();
        }
        RenderWindow.LoadStandardWindowGraphics(bgConfig, RecordingWindowLayer, RecordingFrameTile, 1, 0, HeapIDBattle);
        PaletteData.PaletteData_LoadBufferFromHardware(paletteData, PaletteBufferSubBg, 0x10, 0x20);
        Bg.Bg_FillTilemapRect(bgConfig, RecordingWindowLayer, RecordingFrameTile, 1, 6, 1, 1, 1);
        Bg.Bg_FillTilemapRect(bgConfig, RecordingWindowLayer, RecordingFrameTile + 1, 2, 6, 0x1C, 1, 1);
        Bg.Bg_FillTilemapRect(bgConfig, RecordingWindowLayer, RecordingFrameTile + 2, 0x1E, 6, 1, 1, 1);
        Bg.Bg_FillTilemapRect(bgConfig, RecordingWindowLayer, RecordingFrameTile + 3, 1, 7, 1, 4, 1);
        Bg.Bg_FillTilemapRect(bgConfig, RecordingWindowLayer, RecordingFrameTile + 5, 0x1E, 7, 1, 4, 1);
        Bg.Bg_FillTilemapRect(bgConfig, RecordingWindowLayer, RecordingFrameTile + 6, 1, 0xB, 1, 1, 1);
        Bg.Bg_FillTilemapRect(bgConfig, RecordingWindowLayer, RecordingFrameTile + 7, 2, 0xB, 0x1C, 1, 1);
        Bg.Bg_FillTilemapRect(bgConfig, RecordingWindowLayer, RecordingFrameTile + 8, 0x1E, 0xB, 1, 1, 1);
        Bg.Bg_FillTilemapRect(bgConfig, RecordingWindowLayer, RecordingFrameTile + 4, 2, 7, 0x1C, 4, 1);
        Bg.Bg_ScheduleTilemapTransfer(bgConfig, RecordingWindowLayer);
        MessageLoader loader = BattleSystems.BattleSystem_GetMessageLoader(subscreen.BattleSys!)!;
        ushort[] messages = RomData.OverlayU16(16, RecordingMessageTable, 6);
        for (int i = 0; i < 2; i++)
        {
            if (subscreen.FontOAMEntry[1 + i].FontOAM != null)
            {
                ErrorHandling.GF_AssertFail();
                return;
            }
            GameString text = MessageLoader.MessageLoader_GetNewString(loader, messages[messageSet * 2 + i])!;
            BattleSubscreen_CreateFontOAM(subscreen, subscreen.FontOAMEntry[1 + i], text, FontMessage, ColorRecordingMessage, MovePPPaletteOffset, ResourceSubscreenPalette, 0x10, 0x40 + 0x10 * i, 0, null);
            GameString.String_Free(text);
        }
    }

    // OFFSET 0x0226E148
    public static SysTask BattleSubscreen_StartPlaybackStopButton(BattleSystem battleSys)
    {
        PlaybackStopButtonData data = new();
        data.BattleSys = battleSys;
        return SysTask.SysTask_Start(BattleSubscreen_PlaybackStopButtonTask, data, TaskPriorityPlaybackButton);
    }

    // OFFSET 0x0226E174
    public static void BattleSubscreen_FreePlaybackStopButton(SysTask task)
    {
        SysTaskManager.SysTask_GetParam(task);
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x0226E188
    private static void BattleSubscreen_PlaybackStopButtonTask(SysTask task, object? param)
    {
        PlaybackStopButtonData data = (PlaybackStopButtonData)param!;
        BattleSubscreen? subscreen = BattleSystems.BattleSystem_GetBattleSubscreen(data.BattleSys!);
        switch (data.State)
        {
            case 0:
            {
                if (subscreen == null)
                {
                    ErrorHandling.GF_AssertFail();
                }
                NARC battleNarc = NARC.NARC_ctor(NarcBattleGraphics, HeapIDBattle);
                NARC subscreenNarc = NARC.NARC_ctor(NarcBattleSubscreenGraphics, HeapIDBattle);
                BattleSubscreen_LoadMenu(battleNarc, subscreenNarc, subscreen!, StopFirstBattleMenuIndex, 0, null);
                NARC.NARC_dtor(battleNarc);
                NARC.NARC_dtor(subscreenNarc);
                data.State++;
                break;
            }
            case 1:
                if (BattleSubscreen_IsMenuIdle(subscreen!) == 1)
                {
                    data.State++;
                }
                break;
            case 2:
                if (BattleSystems.BattleSystem_CanStopPlayback(data.BattleSys!) == 1 && BattleSubscreen_CheckTouchInput(subscreen!) == 1)
                {
                    SoundPlayback.Sound_PlayEffect(PlaybackStopSfx);
                    data.State++;
                }
                break;
            case 3:
                data.Counter++;
                if (data.Counter > PlaybackStopDelay)
                {
                    BattleSystems.BattleSystem_SetStopRecording(data.BattleSys!, 0);
                    data.State++;
                }
                break;
        }
    }
}
