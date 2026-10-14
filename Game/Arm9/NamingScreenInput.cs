using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public static partial class NamingScreen
{
    // OFFSET 0x02088288
    private static int NamingScreen_WrapAroundWithinInterval(int value, int lower, int upper)
    {
        if (value >= upper)
        {
            value = lower;
        }
        if (value < lower)
        {
            value = upper - 1;
        }
        return value;
    }

    // OFFSET 0x02088298
    private static void NamingScreen_MoveCursor(NamingScreenData data, int dpadMovement)
    {
        if (dpadMovement == 0)
        {
            return;
        }
        NamingScreenCursor cursor = data.KeyboardCursor;
        int deltaX = DpadMovement[dpadMovement * 2];
        int deltaY = DpadMovement[dpadMovement * 2 + 1];
        ushort original = data.KeyboardChars[cursor.Y * NamingScreenData.Columns + cursor.X];
        int x = NamingScreen_WrapAroundWithinInterval(cursor.X + deltaX, 0, NamingScreenData.Columns);
        int y = NamingScreen_WrapAroundWithinInterval(cursor.Y + deltaY, 0, NamingScreenData.Rows);
        while (true)
        {
            ushort c = data.KeyboardChars[y * NamingScreenData.Columns + x];
            if (c != ControlSkip && !(original == c && c > ButtonStart))
            {
                break;
            }
            if (cursor.PrevY == 0 && c == ControlSkip && deltaY != 0)
            {
                x = NamingScreen_WrapAroundWithinInterval(x + cursor.DeltaX, 0, NamingScreenData.Columns);
            }
            else
            {
                x = NamingScreen_WrapAroundWithinInterval(x + deltaX, 0, NamingScreenData.Columns);
                y = NamingScreen_WrapAroundWithinInterval(y + deltaY, 0, NamingScreenData.Rows);
            }
        }
        cursor.X = x;
        cursor.Y = y;
    }

    // OFFSET 0x02088350
    private static void NamingScreen_ProcessDirectionInputs(NamingScreenData data)
    {
        Sprite cursorSprite = data.UiSprites[SpriteCursor]!;
        int inputs = 0;
        int dpadMovement = 0;
        int cursorWasHidden = 0;
        if (Sprite.Sprite_GetDrawFlag(cursorSprite) == 0)
        {
            cursorWasHidden = 1;
        }
        if ((gSystem.PressedKeysRepeatable & KeyUp) != 0)
        {
            SoundPlayback.Sound_PlayEffect(SeqCursorMove);
            Sprite.Sprite_SetDrawFlag(cursorSprite, 1);
            dpadMovement = 1;
            inputs++;
        }
        if ((gSystem.PressedKeysRepeatable & KeyDown) != 0)
        {
            SoundPlayback.Sound_PlayEffect(SeqCursorMove);
            Sprite.Sprite_SetDrawFlag(cursorSprite, 1);
            dpadMovement = 2;
            inputs++;
        }
        if ((gSystem.PressedKeysRepeatable & KeyLeft) != 0)
        {
            SoundPlayback.Sound_PlayEffect(SeqCursorMove);
            Sprite.Sprite_SetDrawFlag(cursorSprite, 1);
            dpadMovement = 3;
            inputs++;
        }
        if ((gSystem.PressedKeysRepeatable & KeyRight) != 0)
        {
            SoundPlayback.Sound_PlayEffect(SeqCursorMove);
            Sprite.Sprite_SetDrawFlag(cursorSprite, 1);
            dpadMovement = 4;
            inputs++;
        }
        if ((gSystem.PressedKeys & KeyStart) != 0)
        {
            SoundPlayback.Sound_PlayEffect(SeqCursorMove);
            Sprite.Sprite_SetDrawFlag(cursorSprite, 1);
            data.KeyboardCursor.X = 0xC;
            data.KeyboardCursor.Y = 0;
            inputs++;
        }
        data.IsTouchInput = NamingScreen_ProcessTouchInputs(data);
        if (data.IsTouchInput == 1)
        {
            dpadMovement = 0;
            inputs++;
        }
        if (cursorWasHidden == 1)
        {
            inputs = 0;
            NamingScreen_UpdateCursorSpritePosition(data, dpadMovement);
        }
        if (inputs != 0)
        {
            NamingScreen_MoveCursor(data, dpadMovement);
            NamingScreen_UpdateCursorSpritePosition(data, dpadMovement);
        }
    }

    // OFFSET 0x02088454
    private static void NamingScreen_UpdateCursorSpritePosition(NamingScreenData data, int dpadMovement)
    {
        NamingScreenCursor cursor = data.KeyboardCursor;
        Sprite cursorSprite = data.UiSprites[SpriteCursor]!;
        if (cursor.Y != 0)
        {
            if (cursor.PrevY == 0 && cursor.PrevY != cursor.Y)
            {
                Sprite.Sprite_SetAnim(cursorSprite, 0x27);
            }
            VecFx32 position = new()
            {
                X = (cursor.X * 0x10 + 0x1A) << 12,
                Y = ((cursor.Y - 1) * 0x13 + 0x5B) << 12,
            };
            Sprite.Sprite_SetPosition(cursorSprite, position);
        }
        else
        {
            int button = data.KeyboardChars[cursor.Y * NamingScreenData.Columns + cursor.X] - ButtonPageUpper;
            VecFx32 position = new()
            {
                X = RomData.Arm9U16(HomeRowCursorXCoordsAddress + (uint)(button * 2), 1)[0] << 12,
                Y = 0x44000,
            };
            Sprite.Sprite_SetAnim(cursorSprite, RomData.Arm9(HomeRowCursorAnimIDsAddress + (uint)button, 1)[0]);
            Sprite.Sprite_SetPosition(cursorSprite, position);
        }
        data.PlttGlowEffectAngle = 0xB4;
        Sprite.Sprite_SetAnimFrame(cursorSprite, 0);
        cursor.PrevX = cursor.X;
        cursor.PrevY = cursor.Y;
        if (DpadMovement[dpadMovement * 2] != 0)
        {
            cursor.DeltaX = DpadMovement[dpadMovement * 2];
        }
    }

    // OFFSET 0x02088898
    private static int NamingScreen_ProcessCharacterInput(NamingScreenData data, ushort charCode, int isButtonInput)
    {
        Sprite cursorSprite = data.UiSprites[SpriteCursor]!;
        Window entryWindow = data.Windows[WindowEnteredText];
        if (charCode == ControlSpace || charCode == ControlSkip)
        {
            charCode = CharSpace;
        }
        if (data.Type == TypeFriendCode && charCode >= ButtonPageUpper && charCode <= ButtonPageJapanese)
        {
            charCode = CharSpace;
        }
        if (Sprite.Sprite_GetDrawFlag(cursorSprite) == 0 && gSystem.TouchPressed == 0)
        {
            Sprite.Sprite_SetDrawFlag(cursorSprite, 1);
            return AppStateRunning;
        }
        switch (charCode)
        {
            case ControlDakuten:
                if (NamingScreen_JPFlipDiacritic(0x2A, ConversionTableRows, 1, ControlDakuten, data.EntryBuf, data.TextCursorPos))
                {
                    Bg.Window_FillTilemap(entryWindow, 1);
                    NamingScreen_PrintChars(entryWindow, data.EntryBuf, 0, 0, 0xC, 0, EntryTextColor, null);
                    SoundPlayback.Sound_PlayEffect(SeqCharEntered);
                }
                break;
            case ControlHandakuten:
                if (NamingScreen_JPFlipDiacritic(0x48, ConversionTableRows, 2, ControlHandakuten, data.EntryBuf, data.TextCursorPos))
                {
                    Bg.Window_FillTilemap(entryWindow, 1);
                    NamingScreen_PrintChars(entryWindow, data.EntryBuf, 0, 0, 0xC, 0, EntryTextColor, null);
                    SoundPlayback.Sound_PlayEffect(SeqCharEntered);
                }
                break;
            case ButtonPageJapanese2:
                if (NamingScreen_JPFlipAlphaCase(0, ConversionTableRows, data.EntryBuf, data.TextCursorPos))
                {
                    Bg.Window_FillTilemap(entryWindow, 1);
                    NamingScreen_PrintChars(entryWindow, data.EntryBuf, 0, 0, 0xC, 0, EntryTextColor, null);
                    data.SpritesToUpdate[4]++;
                    SoundPlayback.Sound_PlayEffect(SeqCharEntered);
                }
                break;
            case ButtonPageUpper:
            case ButtonPageLower:
            case ButtonPageOthers:
            case ButtonPageJapanese:
            {
                int charsIdx = charCode - ButtonPageUpper;
                if (data.CurrentCharsIdx != charsIdx)
                {
                    data.State = StateLoadGraphics;
                    data.CurrentCharsIdx = charsIdx;
                    NamingScreen_LoadKeyboardLayout(data.KeyboardChars, data.CurrentCharsIdx);
                    data.SpritesToUpdate[charsIdx]++;
                    SoundPlayback.Sound_PlayEffect(SeqPageChange);
                    Sprite.Sprite_SetDrawFlag(cursorSprite, isButtonInput);
                }
                break;
            }
            case ButtonBack:
                if (data.TextCursorPos == 0)
                {
                    break;
                }
                data.EntryBuf[data.TextCursorPos - 1] = CharEOS;
                data.TextCursorPos--;
                Bg.Window_FillTilemap(entryWindow, 1);
                if (data.TextCursorPos == 0)
                {
                    Bg.Window_CopyToVRAM(entryWindow);
                }
                else
                {
                    NamingScreen_PrintChars(entryWindow, data.EntryBuf, 0, 0, 0xC, 0, EntryTextColor, null);
                }
                NamingScreen_PrintLastCharOfEntryBuf(data.Windows, WindowLastChar, data.EntryBuf, data.TextCursorPos, data.TmpBuf, data.PixelBuf, data.LastCharString!);
                NamingScreen_UpdateTextCursors(data.TextCursorSprites, data.TextCursorPos, data.MaxChars);
                data.SpritesToUpdate[SpriteBackButton]++;
                SoundPlayback.Sound_PlayEffect(SeqCursorMove);
                Sprite.Sprite_SetDrawFlag(cursorSprite, isButtonInput);
                break;
            case ButtonOk:
                PaletteAnimator.PaletteAnimator_Free(data.TouchscreenIconBlinker!);
                Sprite.Sprite_SetDrawFlag(cursorSprite, isButtonInput);
                if (data.PrintedFromBattleGMM == 0)
                {
                    SoundPlayback.Sound_PlayEffect(SeqConfirm);
                    data.SpritesToUpdate[SpriteOkButton]++;
                    ScreenFadeControl.StartScreenFade(2, 0, 0, 0, 0x10, 1, HeapID);
                    return AppStateWaitFadeOut;
                }
                data.State = StatePrintBattleMessage;
                break;
            default:
                if (data.CurrentCharsIdx == 4 && charCode == CharSpace)
                {
                    return AppStateRunning;
                }
                if (data.TextCursorPos == data.MaxChars)
                {
                    break;
                }
                data.EntryBuf[data.TextCursorPos] = charCode;
                Bg.Window_FillTilemap(entryWindow, 1);
                NamingScreen_PrintChars(entryWindow, data.EntryBuf, 0, 0, 0xC, 0, EntryTextColor, null);
                data.TextCursorPos++;
                NamingScreen_UpdateTextCursors(data.TextCursorSprites, data.TextCursorPos, data.MaxChars);
                SoundPlayback.Sound_PlayEffect(SeqCharEntered);
                Sprite.Sprite_SetDrawFlag(cursorSprite, 1);
                Sprite.Sprite_SetExplicitOAMMode(cursorSprite, 1);
                Video.SetBlendAlpha(DisplayMain, 0, 7, 8, 8);
                Sprite.Sprite_SetAnim(cursorSprite, 0x3C);
                data.KeyboardCursor.IgnoreInput = 1;
                NamingScreen_PrintLastCharOfEntryBuf(data.Windows, WindowLastChar, data.EntryBuf, data.TextCursorPos, data.TmpBuf, data.PixelBuf, data.LastCharString!);
                break;
        }
        return AppStateRunning;
    }

    // OFFSET 0x02088C7C
    private static ushort NamingScreen_SearchJPConvTableForNonSpace(ushort[] table, int row, int pos)
    {
        do
        {
            pos = NamingScreen_WrapAroundWithinInterval(pos + 1, 0, 3);
        }
        while (table[row * 3 + pos] == CharSpace);
        return table[row * 3 + pos];
    }

    // OFFSET 0x02088C9C
    private static bool NamingScreen_JPFlipAlphaCase(int tableStart, int tableEnd, ushort[] charBuf, int cursorPos)
    {
        if (cursorPos == 0)
        {
            return false;
        }
        ushort[] table = ConversionTable;
        ushort c = charBuf[cursorPos - 1];
        for (int row = tableStart; row < tableEnd; row++)
        {
            for (int i = 0; i < 3; i++)
            {
                if (c == table[row * 3 + i] && c != CharSpace)
                {
                    charBuf[cursorPos - 1] = NamingScreen_SearchJPConvTableForNonSpace(table, row, i);
                    return true;
                }
            }
        }
        return false;
    }

    // OFFSET 0x02088D08
    private static bool NamingScreen_JPFlipDiacritic(int tableStart, int tableEnd, int column, ushort mode, ushort[] charBuf, int cursorPos)
    {
        if (cursorPos == 0 || charBuf[cursorPos - 1] == CharSpace)
        {
            return false;
        }
        ushort[] table = ConversionTable;
        ushort c = charBuf[cursorPos - 1];
        for (int row = tableStart; row < tableEnd; row++)
        {
            if (c == table[row * 3])
            {
                charBuf[cursorPos - 1] = table[row * 3 + column];
                return true;
            }
        }
        for (int row = tableStart; row < tableEnd; row++)
        {
            if (c == table[row * 3 + column])
            {
                charBuf[cursorPos - 1] = table[row * 3];
                return true;
            }
        }
        if (mode == ControlDakuten)
        {
            ushort[] dakuten = DakutenTable;
            for (int i = 0; i < DakutenTableRows; i++)
            {
                if (c == dakuten[i * 2])
                {
                    charBuf[cursorPos - 1] = dakuten[i * 2 + 1];
                    return true;
                }
            }
        }
        else if (mode == ControlHandakuten)
        {
            ushort[] handakuten = HandakutenTable;
            for (int i = 0; i < HandakutenTableRows; i++)
            {
                if (c == handakuten[i * 2])
                {
                    charBuf[cursorPos - 1] = handakuten[i * 2 + 1];
                    return true;
                }
            }
        }
        else if (mode == ButtonPageJapanese2)
        {
            if (c == 0x26)
            {
                charBuf[cursorPos - 1] = 0x24;
                return true;
            }
            if (c == 0x76)
            {
                charBuf[cursorPos - 1] = 0x74;
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x02088FD0
    private static void NamingScreen_PlaceCursorSprite(NamingScreenData data)
    {
        Sprite cursorSprite = data.UiSprites[SpriteCursor]!;
        if (Sprite.Sprite_IsAnimated(cursorSprite))
        {
            return;
        }
        if (data.TextCursorPos == data.MaxChars)
        {
            data.KeyboardCursor.X = 0xC;
            data.KeyboardCursor.Y = 0;
            Sprite.Sprite_SetAnim(cursorSprite, 0x27);
        }
        else
        {
            Sprite.Sprite_SetAnim(cursorSprite, 0x27);
        }
        if (data.KeyboardCursor.HasCharacterBeenEntered == 0)
        {
            Sprite.Sprite_SetDrawFlag(cursorSprite, 0);
        }
        else
        {
            NamingScreen_UpdateCursorSpritePosition(data, 0);
        }
        data.KeyboardCursor.IgnoreInput = 0;
        Sprite.Sprite_SetExplicitOAMMode(cursorSprite, 0);
    }

    // OFFSET 0x0208903C
    private static int NamingScreen_ProcessTouchInputs(NamingScreenData data)
    {
        int start = 0;
        if (data.Type == TypeFriendCode)
        {
            start = 4;
        }
        if (gSystem.TouchPressed == 0)
        {
            return 0;
        }
        byte[] hitboxes = TouchHitboxes;
        int touchX = (byte)gSystem.TouchX;
        int touchY = (byte)gSystem.TouchY;
        int width = 0;
        int height = 0;
        for (int i = start; i < TouchHitboxCount; i++)
        {
            int entry = i * TouchHitboxSize;
            int x = hitboxes[entry];
            int y = hitboxes[entry + 1];
            switch (hitboxes[entry + 2] & 3)
            {
                case 0:
                    width = 0x1F;
                    height = 0x16;
                    break;
                case 1:
                    width = 0x20;
                    height = 0x16;
                    break;
                case 2:
                    width = 0x10;
                    height = 0x13;
                    break;
            }
            if (touchX >= x && touchY >= y && touchX <= x + width && touchY <= y + height)
            {
                data.KeyboardCursor.X = hitboxes[entry + 4] & 0x1F;
                data.KeyboardCursor.Y = hitboxes[entry + 5] & 0x1F;
                return 1;
            }
        }
        return 0;
    }
}
