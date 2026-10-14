using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate void BattleMenuDrawFunc(BattleSubscreen subscreen, int menuIndex, int forceRedraw);
public delegate int BattleMenuProcessFunc(BattleSubscreen subscreen, int input, int buttonSlot);
public delegate int BattleMenuCursorFunc(BattleSubscreen subscreen, int init);
public delegate void BattleMenuSaveCursorFunc(BattleSubscreen subscreen, int input);

public sealed class BattleMenuConfig
{
    public const int Size = 0x30;
    public const int LayerCount = 4;

    public readonly ushort[] BgTilemapBufIndices = new ushort[LayerCount];
    public readonly ushort[] BgPriorities = new ushort[LayerCount];
    public TouchScreenHitTable[]? TouchScreenRects;
    public int[]? ButtonResults;
    public byte[]? ButtonSlots;
    public uint HandleCursorInput;
    public uint SaveCursorPos;
    public uint DrawMenu;
    public uint ProcessInput;
}

public static partial class BattleSubscreens
{
    public const int MenuCount = 19;

    private const uint MenuConfigTable = 0x02270670;
    private const int ResourceSubscreenPalette = 0x4E37;
    private const int SubscreenObjPaletteMember = 0x48;
    private const int SubscreenObjPaletteCount = 7;
    private const int DisplaySub = 1;
    private const uint TaskPriorityMenuTransfer = 10;
    private const ushort TilemapNone = 0xFFFF;
    private const int MenuDataSize = 0x22;
    private const int MaxTouchRects = 16;

    private static BattleMenuConfig[]? menuConfigs;

    private static TouchScreenHitTable[] RomTouchRects(uint address)
    {
        byte[] raw = RomData.Overlay(16, address, MaxTouchRects * 4);
        int count = 0;
        while (raw[count * 4] != TouchScreen.TableTerminator)
        {
            count++;
        }
        TouchScreenHitTable[] rects = new TouchScreenHitTable[count + 1];
        for (int i = 0; i <= count; i++)
        {
            rects[i].Top = raw[i * 4];
            rects[i].Bottom = raw[i * 4 + 1];
            rects[i].Left = raw[i * 4 + 2];
            rects[i].Right = raw[i * 4 + 3];
        }
        return rects;
    }

    private static short[] RomShorts(uint address, int count)
    {
        ushort[] raw = RomData.OverlayU16(16, address, count);
        short[] values = new short[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = (short)raw[i];
        }
        return values;
    }

    private static byte[] RomBytes(uint address, int count)
    {
        return RomData.Overlay(16, address, count);
    }

    private static BattleMenuConfig MenuConfig(int index)
    {
        if (menuConfigs == null)
        {
            byte[] raw = RomData.Overlay(16, MenuConfigTable, BattleMenuConfig.Size * MenuCount);
            menuConfigs = new BattleMenuConfig[MenuCount];
            for (int i = 0; i < MenuCount; i++)
            {
                int entry = i * BattleMenuConfig.Size;
                BattleMenuConfig config = new();
                for (int layer = 0; layer < BattleMenuConfig.LayerCount; layer++)
                {
                    config.BgTilemapBufIndices[layer] = (ushort)Nitro.Bin.U16(raw, entry + 4 + layer * 2);
                    config.BgPriorities[layer] = (ushort)Nitro.Bin.U16(raw, entry + 0xC + layer * 2);
                }
                uint rects = Nitro.Bin.U32(raw, entry + 0x14);
                if (rects != 0)
                {
                    config.TouchScreenRects = RomTouchRects(rects);
                    int count = config.TouchScreenRects.Length - 1;
                    uint results = Nitro.Bin.U32(raw, entry + 0x18);
                    if (results != 0)
                    {
                        uint[] values = RomData.OverlayU32(16, results, count);
                        config.ButtonResults = values.Select(v => (int)v).ToArray();
                    }
                    uint slots = Nitro.Bin.U32(raw, entry + 0x1C);
                    if (slots != 0)
                    {
                        config.ButtonSlots = RomBytes(slots, count);
                    }
                }
                config.HandleCursorInput = Nitro.Bin.U32(raw, entry + 0x20) & ~1u;
                config.SaveCursorPos = Nitro.Bin.U32(raw, entry + 0x24) & ~1u;
                config.DrawMenu = Nitro.Bin.U32(raw, entry + 0x28) & ~1u;
                config.ProcessInput = Nitro.Bin.U32(raw, entry + 0x2C) & ~1u;
                menuConfigs[i] = config;
            }
        }
        return menuConfigs[index];
    }

    private static BattleMenuDrawFunc MenuDrawFunc(uint address)
    {
        return address switch
        {
            0x02269654 => (subscreen, menuIndex, forceRedraw) => BattleSubscreen_DrawActionMenuWithCurtain(subscreen),
            0x02269668 => (subscreen, menuIndex, forceRedraw) => BattleSubscreen_DrawActionMenu(subscreen),
            0x022698A8 => (subscreen, menuIndex, forceRedraw) => BattleSubscreen_DrawFightOnlyMenuWithCurtain(subscreen),
            0x022698BC => (subscreen, menuIndex, forceRedraw) => BattleSubscreen_DrawFightOnlyMenu(subscreen),
            0x02269924 => (subscreen, menuIndex, forceRedraw) => BattleSubscreen_DrawPalParkMenuWithCurtain(subscreen),
            0x02269938 => (subscreen, menuIndex, forceRedraw) => BattleSubscreen_DrawPalParkMenu(subscreen),
            0x022699AC => (subscreen, menuIndex, forceRedraw) => BattleSubscreen_DrawMoveSelectMenu(subscreen),
            0x02269C7C => (subscreen, menuIndex, forceRedraw) => BattleSubscreen_DrawRunConfirmMenu(subscreen),
            0x02269D14 => (subscreen, menuIndex, forceRedraw) => BattleSubscreen_DrawSwitchConfirmMenu(subscreen),
            0x02269DB0 => (subscreen, menuIndex, forceRedraw) => BattleSubscreen_DrawForgetMoveMenu(subscreen),
            0x02269E94 => (subscreen, menuIndex, forceRedraw) => BattleSubscreen_DrawStopPlaybackMenu(subscreen),
            0x02269F68 => (subscreen, menuIndex, forceRedraw) => BattleSubscreen_DrawGiveUpMoveMenu(subscreen),
            0x0226A04C => (subscreen, menuIndex, forceRedraw) => BattleSubscreen_DrawLearnMoveMenu(subscreen),
            0x0226A12C => (subscreen, menuIndex, forceRedraw) => BattleSubscreen_DrawTargetSelectMenu(subscreen),
            _ => throw Missing(address),
        };
    }

    private static BattleMenuProcessFunc MenuProcessFunc(uint address)
    {
        return address switch
        {
            0x0226A318 => BattleSubscreen_ProcessActionMenu,
            0x0226A3F4 => BattleSubscreen_ProcessMoveSelectMenu,
            0x0226A4A4 => BattleSubscreen_ProcessYesNoMenu,
            0x0226A528 => BattleSubscreen_ProcessTargetSelectMenu,
            0x0226A634 => BattleSubscreen_ProcessStopPlaybackMenu,
            _ => throw Missing(address),
        };
    }

    private static BattleMenuCursorFunc MenuCursorFunc(uint address)
    {
        return address switch
        {
            0x0226BEC0 => BattleSubscreen_ActionMenuCursor,
            0x0226C100 => BattleSubscreen_TwoButtonMenuCursor,
            0x0226C1F8 => BattleSubscreen_MoveSelectCursor,
            0x0226C3C8 => BattleSubscreen_TargetSelectCursor,
            0x0226CA14 => BattleSubscreen_YesNoMenuCursor,
            _ => throw Missing(address),
        };
    }

    private static BattleMenuSaveCursorFunc MenuSaveCursorFunc(uint address)
    {
        return address switch
        {
            0x0226C0A0 => BattleSubscreen_SaveActionMenuCursor,
            0x0226C378 => BattleSubscreen_SaveMoveSelectCursor,
            0x0226C9B8 => BattleSubscreen_SaveTargetSelectCursor,
            _ => throw Missing(address),
        };
    }

    private static void BattleSubscreen_CopyMenuData(BattleSubscreen subscreen, object menuData)
    {
        ActiveMenuData active = subscreen.ActiveMenuData;
        switch (menuData)
        {
            case ActionSelectData action:
                active.ActionData.BattlerType = action.BattlerType;
                active.ActionData.Battler = action.Battler;
                active.ActionData.PartySlot = action.PartySlot;
                active.ActionData.BallStatusBattler = action.BallStatusBattler;
                active.ActionData.CurHp = action.CurHp;
                active.ActionData.MaxHp = action.MaxHp;
                active.ActionData.IsWaitingForPartner = action.IsWaitingForPartner;
                break;
            case MoveSelectData moves:
                Array.Copy(moves.MoveIDs, active.MoveData.MoveIDs, MoveSlots);
                Array.Copy(moves.CurrPP, active.MoveData.CurrPP, MoveSlots);
                Array.Copy(moves.MaxPP, active.MoveData.MaxPP, MoveSlots);
                active.MoveData.BattlerType = moves.BattlerType;
                break;
            case TargetSelectData targets:
                for (int i = 0; i < MoveSlots; i++)
                {
                    TargetMonData src = targets.TargetMonData[i];
                    TargetMonData dst = active.TargetSelectData.TargetMonData[i];
                    dst.PartySlot = src.PartySlot;
                    dst.Gender = src.Gender;
                    dst.IsPresent = src.IsPresent;
                    dst.StockStatus = src.StockStatus;
                    dst.Padding03 = src.Padding03;
                    dst.CurHP = src.CurHP;
                    dst.MaxHP = src.MaxHP;
                }
                active.TargetSelectData.BattlerType = targets.BattlerType;
                active.TargetSelectData.TargetingLayout = targets.TargetingLayout;
                break;
            case GiveUpMoveData giveUp:
                active.GiveUpMoveData.Move = giveUp.Move;
                active.GiveUpMoveData.Unused02 = giveUp.Unused02;
                break;
        }
    }

    // OFFSET 0x02268C04
    public static void BattleSubscreen_LoadMenu(NARC battleNarc, NARC subscreenNarc, BattleSubscreen subscreen, int menuIndex, int forceRedraw, object? menuData)
    {
        if (menuData != null)
        {
            BattleSubscreen_CopyMenuData(subscreen, menuData);
        }
        subscreen.IsPanelSliding = 0;
        BgConfig bgConfig = BattleSystems.BattleSystem_GetBgConfig(subscreen.BattleSys!)!;
        SpriteSystem spriteSys = BattleSystems.BattleSystem_GetSpriteSystem(subscreen.BattleSys!)!;
        SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(subscreen.BattleSys!)!;
        BattleMenuConfig? previous;
        if (subscreen.ActiveMenuConfigIndex == NoMenu)
        {
            forceRedraw = 1;
            previous = null;
        }
        else
        {
            previous = MenuConfig(subscreen.ActiveMenuConfigIndex);
        }
        BattleMenuConfig config = MenuConfig(menuIndex);
        PaletteData.PaletteData_LoadBuffer(BattleSystems.BattleSystem_GetPaletteData(subscreen.BattleSys!)!, subscreen.SubscreenPaletteBuf!, 0, PaletteBufferSubBg, 0, PaletteBufferSize);
        for (int i = 0; i < BattleMenuConfig.LayerCount; i++)
        {
            ushort tilemap = config.BgTilemapBufIndices[i];
            if (tilemap != TilemapNone && (forceRedraw == 1 || tilemap != previous!.BgTilemapBufIndices[i]))
            {
                Bg.Bg_LoadTilemapBuffer(bgConfig, SubscreenFirstLayer + i, subscreen.TilemapBuffers[tilemap]!, TilemapBufferSize);
                Bg.Bg_ScheduleTilemapTransfer(bgConfig, SubscreenFirstLayer + i);
            }
        }
        SpriteSystem.SpriteSystem_LoadPaletteBufferFromOpenNarc(BattleSystems.BattleSystem_GetPaletteData(subscreen.BattleSys!)!, PaletteBufferSubObj, spriteSys, spriteMan, subscreenNarc, SubscreenObjPaletteMember, 0, SubscreenObjPaletteCount, VRamSub, ResourceSubscreenPalette);
        subscreen.ActiveMenuConfigIndex = (sbyte)menuIndex;
        Video.SetBlendAlpha(DisplaySub, 2, 0xF, 8, 0xC);
        BattleSubscreen_DeleteMenuSprites(subscreen);
        if (config.DrawMenu != 0)
        {
            MenuDrawFunc(config.DrawMenu)(subscreen, menuIndex, forceRedraw);
        }
        SysTask.SysTask_ExecuteAfterVBlank(BattleSubscreen_TransferMenuTask, subscreen, TaskPriorityMenuTransfer);
    }

    // OFFSET 0x0226BC50
    private static void BattleSubscreen_TransferMenuTask(SysTask task, object? param)
    {
        BattleSubscreen subscreen = (BattleSubscreen)param!;
        BattleMenuConfig config = MenuConfig(subscreen.ActiveMenuConfigIndex);
        for (int i = 0; i < BattleMenuConfig.LayerCount; i++)
        {
            Bg.Bg_ToggleLayer((byte)(SubscreenFirstLayer + i), (byte)(config.BgTilemapBufIndices[i] == TilemapNone ? 0 : 1));
        }
        for (int i = 0; i < BattleMenuConfig.LayerCount; i++)
        {
            Bg.Bg_SetPriority((byte)(SubscreenFirstLayer + i), (byte)config.BgPriorities[i]);
        }
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x0226925C
    public static int BattleSubscreen_CheckTouchInput(BattleSubscreen subscreen)
    {
        int fromCursor = 0;
        if (subscreen.ActiveMenuConfigIndex == NoMenu)
        {
            ErrorHandling.GF_AssertFail();
        }
        BattleMenuConfig config = MenuConfig(subscreen.ActiveMenuConfigIndex);
        if (config.TouchScreenRects == null || subscreen.IsPanelSliding == 1)
        {
            return TouchScreen.ButtonNotPressed;
        }
        if (config.ButtonResults == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        int hit;
        if ((BattleSystems.BattleSystem_GetBattleType(subscreen.BattleSys!) & BattleSystems.BattleTypeCatchTutorial) != 0)
        {
            hit = BattleSubscreen_CatchTutorialInput(subscreen);
        }
        else
        {
            hit = TouchScreen.TouchScreen_CheckRectanglePressed(config.TouchScreenRects);
            if (hit == TouchScreen.ButtonNotPressed)
            {
                hit = BattleSubscreen_CheckCursorInput(subscreen);
                fromCursor++;
            }
        }
        int result = TouchScreen.ButtonNotPressed;
        int slot;
        if (hit == TouchScreen.ButtonNotPressed)
        {
            slot = 0xFF;
        }
        else
        {
            result = config.ButtonResults![hit];
            slot = config.ButtonSlots![hit];
        }
        if (config.ProcessInput != 0)
        {
            result = MenuProcessFunc(config.ProcessInput)(subscreen, result, slot);
            if (result != TouchScreen.ButtonNotPressed)
            {
                if (config.SaveCursorPos != 0)
                {
                    MenuSaveCursorFunc(config.SaveCursorPos)(subscreen, hit);
                }
                subscreen.Cursor.IsActive = 0;
                subscreen.Cursor.Y = 0;
                subscreen.Cursor.X = 0;
                subscreen.Cursor.Unused03 = 0;
                BattleCursors.BattleCursor_Hide(subscreen.CursorRenderer!);
                subscreen.SuppressActivationSfx = (byte)(fromCursor > 0 ? 1 : 0);
            }
        }
        return result;
    }

    // OFFSET 0x02269348
    public static int BattleSubscreen_IsMenuIdle(BattleSubscreen subscreen)
    {
        if (subscreen.MenuTransitionTask == null && subscreen.Unused10 == null && BattleSubscreen_NoPendingTask(subscreen) == 1)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02269368
    public static int BattleSubscreen_TargetingLayoutForRange(int range, int battlerType)
    {
        switch (range)
        {
            case 0:
                return battlerType == BattlerTypePlayerSlot2 ? 9 : 8;
            case 1:
            case 2:
            case 0x10:
                return battlerType == BattlerTypePlayerSlot2 ? 6 : 4;
            case 4:
            case 0x80:
                return 1;
            case 8:
                return battlerType == BattlerTypePlayerSlot2 ? 7 : 2;
            case 0x20:
                return 5;
            case 0x40:
                return 3;
            case 0x100:
                return battlerType == BattlerTypePlayerSlot2 ? 4 : 6;
            case 0x200:
                return 0xA;
            case 0x400:
                return 0xB;
        }
        ErrorHandling.GF_AssertFail();
        return 0;
    }

    // OFFSET 0x0226BCCC
    public static void BattleSubscreen_SetCommandMenuActive(BattleSubscreen subscreen, int value)
    {
    }

    // OFFSET 0x0226BCD0
    public static int BattleSubscreen_NoPendingTask(BattleSubscreen subscreen)
    {
        if (subscreen.Unused664 == null)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0226CD08
    public static byte BattleSubscreen_GetSuppressActivationSfx(BattleSubscreen subscreen)
    {
        return subscreen.SuppressActivationSfx;
    }

    // OFFSET 0x0226CD10
    public static void BattleSubscreen_SetSuppressActivationSfx(BattleSubscreen subscreen, byte value)
    {
        subscreen.SuppressActivationSfx = value;
    }

    // OFFSET 0x0226D088
    public static byte BattleSubscreen_IsWaitingForPartner(BattleSubscreen subscreen)
    {
        return subscreen.IsWaitingForPartner;
    }
}
