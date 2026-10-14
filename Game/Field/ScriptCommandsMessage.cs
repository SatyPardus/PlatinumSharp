using static PlatinumSharp.Game.Globals;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static partial class ScriptCommands
{
    private const int HeapIDField = 4;
    private const int HeapIDFieldTaskTemporary = 0x20;
    private const int NarcMessages = 0x1A;
    private const int MessageLoaderTypeNarcHandle = 1;
    private const int KeyAOrB = 3;
    private const int KeyRight = 0x10;
    private const int KeyLeft = 0x20;
    private const int KeyUp = 0x40;
    private const int KeyDown = 0x80;
    private const int KeyDirections = 0xF0;
    private const int KeyX = 0x400;
    private const int DirNorth = 0;
    private const int DirSouth = 1;
    private const int DirWest = 2;
    private const int DirEast = 3;
    private const int DirNone = 0xFFFF;
    private const int BgLayerMain3 = 3;
    private const int FontMessage = 1;
    private const int FontUnown = 3;
    private const int InstantMarker = 0xFF;
    private const int MessageBoxTile = 0x3E2;
    private const int YesNoTile = 0x3D9;
    private const int YesNoPalette = 0xB;
    private const uint YesNoWindowTemplateAddress = 0x020EAB84;
    private const uint MenuNoChoice = 0xFFFFFFFF;
    private const int BgOffsetAddX = 1;
    private const int BgOffsetSubX = 2;
    private const int BgOffsetAddY = 4;
    private const int BgOffsetSubY = 5;
    private const int SignpostCommandShow = 1;

    private static void RegisterMessage(Dictionary<uint, ScrCmdFunc> ported)
    {
        ported[0x0203FD5C] = ScrCmd_MessageInstant;
        ported[0x0203FD70] = ScrCmd_MessageInstantFromBank;
        ported[0x0203FDBC] = ScrCmd_MessageFromBank;
        ported[0x0203FE20] = ScrCmd_MessageSentenceInstant;
        ported[0x0203FE60] = ScrCmd_MessageSentence;
        ported[0x0203FFAC] = ScrCmd_MessageUnown;
        ported[0x0203FFE8] = ScrCmd_Message;
        ported[0x02040028] = ScrCmd_MessageVar;
        ported[0x02040064] = ScrCmd_MessageVarAutoScroll;
        ported[0x020400AC] = ScrCmd_MessageVarNoSkip;
        ported[0x020400E8] = ScrCmd_MessageTrainerType;
        ported[0x02040124] = ScrCmd_MessageLinked;
        ported[0x02040180] = ScrCmd_WaitABPress;
        ported[0x020401A8] = ScrCmd_WaitABPressTime;
        ported[0x020401F4] = ScrCmd_WaitButton;
        ported[0x02040284] = ScrCmd_WaitABPadPress;
        ported[0x020402B4] = ScrCmd_OpenMessage;
        ported[0x020402FC] = ScrCmd_CloseMessage;
        ported[0x0204032C] = ScrCmd_FreeMessageWindow;
        ported[0x02040354] = ScrCmd_ScrollBg3;
        ported[0x020404A4] = ScrCmd_DrawSignpostInstantMessage;
        ported[0x02040554] = ScrCmd_DrawSignpostTextBox;
        ported[0x02040580] = ScrCmd_SetSignpostCommand;
        ported[0x0204059C] = ScrCmd_WaitForSignpostDone;
        ported[0x020405DC] = ScrCmd_DrawSignpostScrollingMessage;
        ported[0x02040714] = ScrCmd_WaitForSignpostInput;
        ported[0x020407B4] = ScrCmd_ShowStartMenu;
        ported[0x020407C4] = ScrCmd_ShowYesNoMenu;
        ported[0x02040868] = ScrCmd_ShowSavingIcon;
        ported[0x02040898] = ScrCmd_HideSavingIcon;
    }

    // OFFSET 0x0203FD5C
    private static bool ScrCmd_MessageInstant(ScriptContext ctx)
    {
        byte messageID = ScriptContext.ScriptContext_ReadByte(ctx);
        ScriptMessage.ScriptMessage_ShowInstant(ctx, ctx.Loader!, messageID);
        return false;
    }

    // OFFSET 0x0203FD70
    private static bool ScrCmd_MessageInstantFromBank(ScriptContext ctx)
    {
        ushort bankID = VarValue(ctx);
        ushort messageID = VarValue(ctx);
        MessageLoader loader = MessageLoader.MessageLoader_Init(MessageLoaderTypeNarcHandle, NarcMessages, bankID, HeapIDFieldTaskTemporary)!;
        ScriptMessage.ScriptMessage_ShowInstant(ctx, loader, messageID);
        MessageLoader.MessageLoader_Free(loader);
        return false;
    }

    // OFFSET 0x0203FDBC
    private static bool ScrCmd_MessageFromBank(ScriptContext ctx)
    {
        ushort bankID = VarValue(ctx);
        ushort messageID = VarValue(ctx);
        MessageLoader loader = MessageLoader.MessageLoader_Init(MessageLoaderTypeNarcHandle, NarcMessages, bankID, HeapIDFieldTaskTemporary)!;
        ScriptMessage.ScriptMessage_Show(ctx, loader, messageID, 1, null);
        MessageLoader.MessageLoader_Free(loader);
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForMessage);
        return true;
    }

    // OFFSET 0x0203FE20
    private static bool ScrCmd_MessageSentenceInstant(ScriptContext ctx)
    {
        ushort sentenceType = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ushort sentenceID = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ushort word1 = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ushort word2 = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ScriptMessage.ScriptMessage_ShowSentence(ctx, sentenceType, sentenceID, word1, (short)word2, InstantMarker);
        return false;
    }

    // OFFSET 0x0203FE60
    private static bool ScrCmd_MessageSentence(ScriptContext ctx)
    {
        ushort sentenceType = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ushort sentenceID = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ushort word1 = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ushort word2 = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ScriptMessage.ScriptMessage_ShowSentence(ctx, sentenceType, sentenceID, word1, (short)word2, 1);
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForMessage);
        return true;
    }

    // OFFSET 0x0203FFAC
    private static bool ScrCmd_MessageUnown(ScriptContext ctx)
    {
        ushort messageID = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ScriptMessageOptions options = new();
        ScriptMessage.ScriptMessageOptions_Init(options, ctx);
        options.FontID = FontUnown;
        ScriptMessage.ScriptMessage_Show(ctx, ctx.Loader!, messageID, 0, options);
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForMessage);
        return true;
    }

    // OFFSET 0x0203FFE8
    private static bool ScrCmd_Message(ScriptContext ctx)
    {
        byte messageID = ScriptContext.ScriptContext_ReadByte(ctx);
        ScriptMessage.ScriptMessage_Show(ctx, ctx.Loader!, messageID, 1, null);
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForMessage);
        return true;
    }

    // OFFSET 0x02040014
    private static bool ScriptContext_WaitForMessage(ScriptContext ctx)
    {
        return FieldMessage.FieldMessage_FinishedPrinting(Manager(ctx).MessageID) != 0;
    }

    // OFFSET 0x02040028
    private static bool ScrCmd_MessageVar(ScriptContext ctx)
    {
        ushort messageID = VarValue(ctx);
        ScriptMessage.ScriptMessage_Show(ctx, ctx.Loader!, (byte)messageID, 1, null);
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForMessage);
        return true;
    }

    // OFFSET 0x02040064
    private static bool ScrCmd_MessageVarAutoScroll(ScriptContext ctx)
    {
        ushort messageID = VarValue(ctx);
        ScriptMessageOptions options = new();
        ScriptMessage.ScriptMessageOptions_Init(options, ctx);
        options.AutoScroll = 1;
        ScriptMessage.ScriptMessage_Show(ctx, ctx.Loader!, (byte)messageID, 1, options);
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForMessage);
        return true;
    }

    // OFFSET 0x020400AC
    private static bool ScrCmd_MessageVarNoSkip(ScriptContext ctx)
    {
        ushort messageID = VarValue(ctx);
        ScriptMessage.ScriptMessage_Show(ctx, ctx.Loader!, (byte)messageID, 0, null);
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForMessage);
        return true;
    }

    // OFFSET 0x020400E8
    private static bool ScrCmd_MessageTrainerType(ScriptContext ctx)
    {
        MapObject target = Manager(ctx).TargetObject!;
        byte messageID = (byte)MapObject.MapObject_GetTrainerType(target);
        ScriptMessage.ScriptMessage_Show(ctx, ctx.Loader!, messageID, 1, null);
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForMessage);
        return true;
    }

    // OFFSET 0x02040124
    private static bool ScrCmd_MessageLinked(ScriptContext ctx)
    {
        byte messageID = ScriptContext.ScriptContext_ReadByte(ctx);
        if (CommSystem.CommSys_IsInitialized() == 0)
        {
            ScriptMessage.ScriptMessage_Show(ctx, ctx.Loader!, messageID, 1, null);
        }
        else
        {
            ScriptMessageOptions options = new();
            ScriptMessage.ScriptMessageOptions_Init(options, ctx);
            options.RenderDelay = 1;
            options.AutoScroll = 1;
            ScriptMessage.ScriptMessage_Show(ctx, ctx.Loader!, messageID, 0, options);
        }
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForMessage);
        return true;
    }

    // OFFSET 0x02040180
    private static bool ScrCmd_WaitABPress(ScriptContext ctx)
    {
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForABPress);
        return true;
    }

    // OFFSET 0x02040190
    private static bool ScriptContext_WaitForABPress(ScriptContext ctx)
    {
        if ((gSystem.PressedKeys & KeyAOrB) != 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x020401A8
    private static bool ScrCmd_WaitABPressTime(ScriptContext ctx)
    {
        ctx.Data[0] = VarValue(ctx);
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForABPressTime);
        return true;
    }

    // OFFSET 0x020401D0
    private static bool ScriptContext_WaitForABPressTime(ScriptContext ctx)
    {
        if ((gSystem.PressedKeys & KeyAOrB) != 0)
        {
            return true;
        }
        ctx.Data[0]--;
        if (ctx.Data[0] == 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x020401F4
    private static bool ScrCmd_WaitButton(ScriptContext ctx)
    {
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForButton);
        return true;
    }

    // OFFSET 0x02040204
    private static bool ScriptContext_WaitForButton(ScriptContext ctx)
    {
        uint keys = (uint)gSystem.PressedKeys;
        if ((keys & KeyAOrB) != 0)
        {
            return true;
        }
        if ((keys & KeyUp) != 0)
        {
            PlayerAvatar.PlayerAvatar_TryFace(ctx.FieldSystem.PlayerAvatar!, DirNorth);
        }
        else if ((keys & KeyDown) != 0)
        {
            PlayerAvatar.PlayerAvatar_TryFace(ctx.FieldSystem.PlayerAvatar!, DirSouth);
        }
        else if ((keys & KeyLeft) != 0)
        {
            PlayerAvatar.PlayerAvatar_TryFace(ctx.FieldSystem.PlayerAvatar!, DirWest);
        }
        else if ((keys & KeyRight) != 0)
        {
            PlayerAvatar.PlayerAvatar_TryFace(ctx.FieldSystem.PlayerAvatar!, DirEast);
        }
        else if ((keys & KeyX) != 0)
        {
            ScriptManager.FieldSystem_ShowStartMenu(ctx.FieldSystem);
        }
        else
        {
            return false;
        }
        return true;
    }

    // OFFSET 0x02040284
    private static bool ScrCmd_WaitABPadPress(ScriptContext ctx)
    {
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForABPadPress);
        return true;
    }

    // OFFSET 0x02040294
    private static bool ScriptContext_WaitForABPadPress(ScriptContext ctx)
    {
        if ((gSystem.PressedKeys & KeyAOrB) != 0)
        {
            return true;
        }
        if ((gSystem.PressedKeys & KeyDirections) != 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x020402B4
    private static bool ScrCmd_OpenMessage(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ScriptManager scriptManager = Manager(ctx);
        FieldMessage.FieldMessage_AddWindow(fieldSystem.BgConfig!, scriptManager.Window, BgLayerMain3);
        FieldMessage.FieldMessage_DrawWindow(scriptManager.Window, PlayerSave.SaveData_GetOptions(fieldSystem.SaveData!));
        scriptManager.IsMessageBoxOpen = 1;
        return false;
    }

    // OFFSET 0x020402FC
    private static bool ScrCmd_CloseMessage(ScriptContext ctx)
    {
        ScriptManager scriptManager = Manager(ctx);
        RenderWindow.Window_EraseMessageBox(scriptManager.Window, 0);
        Bg.Window_Remove(scriptManager.Window);
        scriptManager.IsMessageBoxOpen = 0;
        return false;
    }

    // OFFSET 0x0204032C
    private static bool ScrCmd_FreeMessageWindow(ScriptContext ctx)
    {
        ScriptManager scriptManager = Manager(ctx);
        Bg.Window_Remove(scriptManager.Window);
        scriptManager.IsMessageBoxOpen = 0;
        return false;
    }

    // OFFSET 0x02040354
    private static bool ScrCmd_ScrollBg3(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        BufferView distX = ScriptManager.FieldSystem_GetScriptMemberPtr(fieldSystem, ScriptManager.MemberBg3ScrollDistX);
        BufferView countX = ScriptManager.FieldSystem_GetScriptMemberPtr(fieldSystem, ScriptManager.MemberBg3ScrollCountX);
        BufferView dirX = ScriptManager.FieldSystem_GetScriptMemberPtr(fieldSystem, ScriptManager.MemberBg3ScrollDirX);
        BufferView distY = ScriptManager.FieldSystem_GetScriptMemberPtr(fieldSystem, ScriptManager.MemberBg3ScrollDistY);
        BufferView countY = ScriptManager.FieldSystem_GetScriptMemberPtr(fieldSystem, ScriptManager.MemberBg3ScrollCountY);
        BufferView dirY = ScriptManager.FieldSystem_GetScriptMemberPtr(fieldSystem, ScriptManager.MemberBg3ScrollDirY);
        distX.W16(0, ScriptContext.ScriptContext_ReadByte(ctx));
        countX.W16(0, ScriptContext.ScriptContext_ReadByte(ctx));
        dirX.W16(0, ScriptContext.ScriptContext_ReadByte(ctx));
        distY.W16(0, ScriptContext.ScriptContext_ReadByte(ctx));
        countY.W16(0, ScriptContext.ScriptContext_ReadByte(ctx));
        dirY.W16(0, ScriptContext.ScriptContext_ReadByte(ctx));
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_ScrollBg3);
        return true;
    }

    // OFFSET 0x020403EC
    private static bool ScriptContext_ScrollBg3(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        BufferView distX = ScriptManager.FieldSystem_GetScriptMemberPtr(fieldSystem, ScriptManager.MemberBg3ScrollDistX);
        BufferView dirX = ScriptManager.FieldSystem_GetScriptMemberPtr(fieldSystem, ScriptManager.MemberBg3ScrollDirX);
        BufferView distY = ScriptManager.FieldSystem_GetScriptMemberPtr(fieldSystem, ScriptManager.MemberBg3ScrollDistY);
        BufferView dirY = ScriptManager.FieldSystem_GetScriptMemberPtr(fieldSystem, ScriptManager.MemberBg3ScrollDirY);
        BufferView countX = ScriptManager.FieldSystem_GetScriptMemberPtr(fieldSystem, ScriptManager.MemberBg3ScrollCountX);
        BufferView countY = ScriptManager.FieldSystem_GetScriptMemberPtr(fieldSystem, ScriptManager.MemberBg3ScrollCountY);
        if (countX.U16(0) == 0 && countY.U16(0) == 0)
        {
            return true;
        }
        if (distX.U16(0) != 0)
        {
            if (dirX.U16(0) == 0)
            {
                Bg.Bg_SetOffset(fieldSystem.BgConfig!, BgLayerMain3, BgOffsetAddX, distX.U16(0));
            }
            else
            {
                Bg.Bg_SetOffset(fieldSystem.BgConfig!, BgLayerMain3, BgOffsetSubX, distX.U16(0));
            }
        }
        if (distY.U16(0) != 0)
        {
            if (dirY.U16(0) == 0)
            {
                Bg.Bg_SetOffset(fieldSystem.BgConfig!, BgLayerMain3, BgOffsetAddY, distY.U16(0));
            }
            else
            {
                Bg.Bg_SetOffset(fieldSystem.BgConfig!, BgLayerMain3, BgOffsetSubY, distY.U16(0));
            }
        }
        if (countX.U16(0) != 0)
        {
            countX.W16(0, countX.U16(0) - 1);
        }
        if (countY.U16(0) != 0)
        {
            countY.W16(0, countY.U16(0) - 1);
        }
        return false;
    }

    // OFFSET 0x020404A4
    private static bool ScrCmd_DrawSignpostInstantMessage(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ScriptManager scriptManager = Manager(ctx);
        byte messageID = ScriptContext.ScriptContext_ReadByte(ctx);
        byte signpostType = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort narcMemberIdx = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ScriptContext.ScriptContext_ReadHalfWord(ctx);
        if (narcMemberIdx == 0)
        {
            narcMemberIdx = (ushort)MapObject.MapObject_GetDataAt(scriptManager.TargetObject!, 0);
        }
        Signposts.Signpost_SetTypeAndNARCMemberIdx(fieldSystem.Signpost!, signpostType, narcMemberIdx);
        Signposts.Signpost_SetCommand(fieldSystem.Signpost!, SignpostCommandShow);
        Signposts.Signpost_DoCurrentCommand(fieldSystem);
        MessageLoader.MessageLoader_GetString(ctx.Loader!, messageID, scriptManager.TemporaryBuffer!);
        StringTemplate.StringTemplate_Format(scriptManager.StringTemplate!, scriptManager.MessageBuffer!, scriptManager.TemporaryBuffer!);
        Window window = Signposts.Signpost_GetWindow(fieldSystem.Signpost!);
        Text.Text_AddPrinterWithParams(window, FontMessage, scriptManager.MessageBuffer, 0, 0, 0, null);
        return true;
    }

    // OFFSET 0x02040554
    private static bool ScrCmd_DrawSignpostTextBox(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        byte signpostType = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort narcMemberIdx = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        Signposts.Signpost_SetTypeAndNARCMemberIdx(fieldSystem.Signpost!, signpostType, narcMemberIdx);
        Signposts.Signpost_SetCommand(fieldSystem.Signpost!, SignpostCommandShow);
        return true;
    }

    // OFFSET 0x02040580
    private static bool ScrCmd_SetSignpostCommand(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        byte command = ScriptContext.ScriptContext_ReadByte(ctx);
        Signposts.Signpost_SetCommand(fieldSystem.Signpost!, command);
        return true;
    }

    // OFFSET 0x0204059C
    private static bool ScrCmd_WaitForSignpostDone(ScriptContext ctx)
    {
        if (Signposts.Signpost_IsDone(ctx.FieldSystem.Signpost!))
        {
            return false;
        }
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForSignpostDone);
        return true;
    }

    // OFFSET 0x020405C4
    private static bool ScriptContext_WaitForSignpostDone(ScriptContext ctx)
    {
        if (Signposts.Signpost_IsDone(ctx.FieldSystem.Signpost!))
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x020405DC
    private static bool ScrCmd_DrawSignpostScrollingMessage(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ScriptManager scriptManager = Manager(ctx);
        byte messageID = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort destVarID = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        MessageLoader.MessageLoader_GetString(ctx.Loader!, messageID, scriptManager.TemporaryBuffer!);
        StringTemplate.StringTemplate_Format(scriptManager.StringTemplate!, scriptManager.MessageBuffer!, scriptManager.TemporaryBuffer!);
        Window window = Signposts.Signpost_GetWindow(fieldSystem.Signpost!);
        scriptManager.MessageID = FieldMessage.FieldMessage_Print(window, scriptManager.MessageBuffer!, PlayerSave.SaveData_GetOptions(fieldSystem.SaveData!), 1);
        ctx.Data[0] = destVarID;
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForSignpostMessage);
        return true;
    }

    private static int PressedDirection(uint keys)
    {
        if ((keys & KeyUp) != 0)
        {
            return DirNorth;
        }
        if ((keys & KeyDown) != 0)
        {
            return DirSouth;
        }
        if ((keys & KeyLeft) != 0)
        {
            return DirWest;
        }
        if ((keys & KeyRight) != 0)
        {
            return DirEast;
        }
        return DirNone;
    }

    // OFFSET 0x02040670
    private static bool ScriptContext_WaitForSignpostMessage(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ScriptManager scriptManager = Manager(ctx);
        BufferView destination = FieldSystemVarsFlags.FieldSystem_GetVarPointer(fieldSystem, (ushort)ctx.Data[0])!;
        Signposts.Signpost_GetType(fieldSystem.Signpost!);
        if (FieldMessage.FieldMessage_FinishedPrinting(scriptManager.MessageID) == 1)
        {
            destination.W16(0, 2);
            return true;
        }
        uint keys = (uint)gSystem.PressedKeys;
        int direction = PressedDirection(keys);
        if (direction != DirNone)
        {
            Text.Text_RemovePrinter(scriptManager.MessageID);
            PlayerAvatar.PlayerAvatar_TryFace(fieldSystem.PlayerAvatar!, direction);
            destination.W16(0, 0);
            return true;
        }
        if ((keys & KeyX) != 0)
        {
            Text.Text_RemovePrinter(scriptManager.MessageID);
            destination.W16(0, 1);
            return true;
        }
        return false;
    }

    // OFFSET 0x02040714
    private static bool ScrCmd_WaitForSignpostInput(ScriptContext ctx)
    {
        ctx.Data[0] = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForSignpostInput);
        return true;
    }

    // OFFSET 0x02040730
    private static bool ScriptContext_WaitForSignpostInput(ScriptContext ctx)
    {
        BufferView destination = FieldSystemVarsFlags.FieldSystem_GetVarPointer(ctx.FieldSystem, (ushort)ctx.Data[0])!;
        uint keys = (uint)gSystem.PressedKeys;
        if ((keys & KeyAOrB) != 0)
        {
            destination.W16(0, 0);
            return true;
        }
        int direction = PressedDirection(keys);
        if (direction != DirNone)
        {
            PlayerAvatar.PlayerAvatar_TryFace(ctx.FieldSystem.PlayerAvatar!, direction);
            destination.W16(0, 0);
            return true;
        }
        if ((keys & KeyX) != 0)
        {
            destination.W16(0, 1);
            return true;
        }
        return false;
    }

    // OFFSET 0x020407B4
    private static bool ScrCmd_ShowStartMenu(ScriptContext ctx)
    {
        ScriptManager.FieldSystem_ShowStartMenu(ctx.FieldSystem);
        return false;
    }

    // OFFSET 0x020407C4
    private static bool ScrCmd_ShowYesNoMenu(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ScriptManager scriptManager = Manager(ctx);
        ushort destVarID = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        RenderWindow.LoadStandardWindowGraphics(fieldSystem.BgConfig!, BgLayerMain3, YesNoTile, YesNoPalette, 0, HeapIDField);
        scriptManager.ControlUI = Menu.Menu_MakeYesNoChoice(fieldSystem.BgConfig!, RomStructs.Arm9WindowTemplate(YesNoWindowTemplateAddress), YesNoTile, YesNoPalette, HeapIDField);
        ctx.Data[0] = destVarID;
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForYesNoResult);
        return true;
    }

    // OFFSET 0x02040824
    private static bool ScriptContext_WaitForYesNoResult(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ScriptManager scriptManager = Manager(ctx);
        BufferView destination = FieldSystemVarsFlags.FieldSystem_GetVarPointer(fieldSystem, (ushort)ctx.Data[0])!;
        uint result = Menu.Menu_ProcessInputAndHandleExit(scriptManager.ControlUI!, HeapIDField);
        if (result == MenuNoChoice)
        {
            return false;
        }
        if (result == 0)
        {
            destination.W16(0, 0);
        }
        else
        {
            destination.W16(0, 1);
        }
        return true;
    }

    // OFFSET 0x02040868
    private static bool ScrCmd_ShowSavingIcon(ScriptContext ctx)
    {
        ScriptManager scriptManager = Manager(ctx);
        scriptManager.SavingIcon = RenderWindow.Window_AddWaitDial(scriptManager.Window, MessageBoxTile);
        return false;
    }

    // OFFSET 0x02040898
    private static bool ScrCmd_HideSavingIcon(ScriptContext ctx)
    {
        RenderWindow.DestroyWaitDial(Manager(ctx).SavingIcon!);
        return false;
    }
}
