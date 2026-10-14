using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public struct TouchScreenHitTable
{
    public byte Top;
    public byte Bottom;
    public byte Left;
    public byte Right;
}

public delegate void TouchScreenHitTableAction(uint index, int state, object? context);

public sealed class YesNoButtonAnimData
{
    public int[]? FrameThresholds;
    public uint FrameCount;
    public int FrameProgress;
}

public sealed class YesNoButtonSprite
{
    public object? BgConfig;
    public int BgLayer;
    public readonly byte[]?[] NscrBuffers = new byte[]?[YesNoTouchMenu.ButtonFrameCount];
    public readonly ScreenData?[] ScreenData = new ScreenData?[YesNoTouchMenu.ButtonFrameCount];
    public readonly YesNoButtonAnimData AnimData = new();
    public byte TilemapLeft;
    public byte TilemapTop;
    public byte AnimFrame;
}

public sealed class YesNoButtonSpriteTemplate
{
    public object? BgConfig;
    public int BgLayer;
    public int NarcID;
    public readonly int[] NarcMembers = new int[YesNoTouchMenu.ButtonFrameCount];
    public uint BaseTile;
    public uint Palette;
    public int[]? FrameThresholds;
    public uint FrameCount;
    public byte TilemapLeft;
    public byte TilemapTop;
}

public sealed class YesNoTouchMenuParams
{
    public object? BgConfig;
    public int BgLayer;
    public uint BaseTile;
    public uint Palette;
    public byte TilemapLeft;
    public byte TilemapTop;
}

public sealed class YesNoTilesLoadTask
{
    public object? BgConfig;
    public CharacterData? CharData;
    public byte[]? Buffer;
    public int BgLayer;
    public uint BaseTile;
}

public sealed class YesNoPaletteLoadTask
{
    public PaletteFile? PaletteData;
    public byte[]? Buffer;
    public int LoadLocation;
    public uint Offset;
    public uint Size;
}

public sealed class YesNoTouchMenu
{
    public const int ButtonYes = 0;
    public const int ButtonNo = 1;
    public const int ButtonCount = 2;
    public const int ButtonNone = 3;
    public const int ButtonFrameCount = 4;
    public const uint ResultNone = 0;
    public const uint ResultYes = 1;
    public const uint ResultNo = 2;
    public const uint ResultYesPressed = 3;
    public const uint ResultNoPressed = 4;

    private const int MenuNarcID = 0x54;
    private const int TilesMember = 1;
    private const int PaletteMember = 0;
    private const int FirstScreenMember = 2;
    private const int ButtonRows = 5;
    private const int ButtonHeightPixels = 0x20;
    private const int ButtonSpacingPixels = 0x28;
    private const int ButtonWidthPixels = 0x30;
    private const uint PaletteLoadSize = 0x40;
    private const int PaletteSlotBytes = 0x20;
    private const int SubScreenBgLayer = 4;
    private const int LoadLocationMainBg = 0;
    private const int LoadLocationSubBg = 4;
    private const uint LoadTaskPriority = 0x80;
    private const int AnimStep = 0x800;
    private const int StateIdle = 8;
    private const int PressSoundEffect = 0x5E4;

    public TouchScreenActions? ButtonAction;
    public readonly TouchScreenHitTable[] HitTable = new TouchScreenHitTable[ButtonCount];
    public readonly YesNoButtonSprite[] ButtonSprites = { new(), new() };
    public object? BgConfig;
    public int BgLayer;
    public int HeapID;
    public readonly int[] FrameThresholds = new int[ButtonFrameCount];
    public byte TilemapLeft;
    public byte TilemapTop;
    public byte PressedButton;
    public byte ParamsApplied;
    public byte ButtonState;

    // OFFSET 0x02015920
    public static YesNoTouchMenu YesNoTouchMenu_New(int heapID)
    {
        YesNoTouchMenu menu = new();
        YesNoTouchMenu_Init(menu, heapID);
        return menu;
    }

    // OFFSET 0x02015938
    public static void YesNoTouchMenu_Free(YesNoTouchMenu menu)
    {
        if (menu.ParamsApplied == 1)
        {
            YesNoTouchMenu_Reset(menu);
        }
    }

    // OFFSET 0x02015958
    public static void YesNoTouchMenu_InitWithParams(YesNoTouchMenu menu, YesNoTouchMenuParams menuParams)
    {
        menu.BgConfig = menuParams.BgConfig;
        menu.BgLayer = menuParams.BgLayer;
        menu.TilemapLeft = menuParams.TilemapLeft;
        menu.TilemapTop = menuParams.TilemapTop;
        YesNoTouchMenu_LoadTiles(menu, menuParams);
        YesNoTouchMenu_LoadPalette(menu, menuParams);
        YesNoTouchMenu_InitButtonSprites(menu, menuParams);
        YesNoTouchMenu_RegisterTouchActions(menu, menuParams);
        menu.ParamsApplied = 1;
        menu.ButtonState = StateIdle;
    }

    // OFFSET 0x020159C0
    public static uint YesNoTouchMenu_ProcessInputInstant(YesNoTouchMenu menu)
    {
        menu.ButtonState = StateIdle;
        uint result = YesNoTouchMenu_ProcessInput(menu);
        if (result == ResultNone && menu.ButtonState == 0)
        {
            if (menu.PressedButton == ButtonYes)
            {
                return ResultYesPressed;
            }
            return ResultNoPressed;
        }
        return result;
    }

    // OFFSET 0x020159FC
    public static uint YesNoTouchMenu_ProcessInput(YesNoTouchMenu menu)
    {
        if (menu.ParamsApplied != 1)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (menu.PressedButton == ButtonNone)
        {
            TouchScreenActions.TouchScreenActions_HandleAction(menu.ButtonAction!);
        }
        bool animationDone = false;
        if (menu.PressedButton != ButtonNone)
        {
            animationDone = YesNoButtonSprite_Animate(menu.ButtonSprites[menu.PressedButton]);
        }
        if (animationDone)
        {
            if (menu.PressedButton == ButtonYes)
            {
                return ResultYes;
            }
            return ResultNo;
        }
        return ResultNone;
    }

    // OFFSET 0x02015A54
    public static void YesNoTouchMenu_Reset(YesNoTouchMenu menu)
    {
        TouchScreenActions.TouchScreenActions_Free(menu.ButtonAction!);
        for (int i = 0; i < ButtonCount; i++)
        {
            YesNoButtonSprite_Free(menu.ButtonSprites[i]);
        }
        YesNoTouchMenu_Init(menu, menu.HeapID);
    }

    // OFFSET 0x02015A80
    private static void YesNoTouchMenu_Init(YesNoTouchMenu menu, int heapID)
    {
        menu.ButtonAction = null;
        for (int i = 0; i < ButtonCount; i++)
        {
            menu.HitTable[i] = default;
            YesNoButtonSprite_Clear(menu.ButtonSprites[i]);
        }
        menu.BgConfig = null;
        menu.BgLayer = 0;
        Array.Clear(menu.FrameThresholds);
        menu.TilemapLeft = 0;
        menu.TilemapTop = 0;
        menu.ParamsApplied = 0;
        menu.HeapID = heapID;
        menu.PressedButton = ButtonNone;
        menu.ButtonState = StateIdle;
    }

    private static void YesNoButtonSprite_Clear(YesNoButtonSprite sprite)
    {
        sprite.BgConfig = null;
        sprite.BgLayer = 0;
        for (int i = 0; i < ButtonFrameCount; i++)
        {
            sprite.NscrBuffers[i] = null;
            sprite.ScreenData[i] = null;
        }
        sprite.AnimData.FrameThresholds = null;
        sprite.AnimData.FrameCount = 0;
        sprite.AnimData.FrameProgress = 0;
        sprite.TilemapLeft = 0;
        sprite.TilemapTop = 0;
        sprite.AnimFrame = 0;
    }

    // OFFSET 0x02015AC0
    private static void YesNoTouchMenu_LoadTiles(YesNoTouchMenu menu, YesNoTouchMenuParams menuParams)
    {
        YesNoTouchMenu_LoadTilesAfterVBlank(MenuNarcID, TilesMember, menu.BgConfig, menu.BgLayer, menuParams.BaseTile, menu.HeapID);
    }

    // OFFSET 0x02015AE4
    private static void YesNoTouchMenu_LoadPalette(YesNoTouchMenu menu, YesNoTouchMenuParams menuParams)
    {
        int loadLocation;
        if ((uint)menu.BgLayer < SubScreenBgLayer)
        {
            loadLocation = LoadLocationMainBg;
        }
        else
        {
            loadLocation = LoadLocationSubBg;
        }
        YesNoTouchMenu_LoadPaletteAfterVBlank(MenuNarcID, PaletteMember, loadLocation, menuParams.Palette * PaletteSlotBytes, PaletteLoadSize, menu.HeapID);
    }

    // OFFSET 0x02015B14
    private static void YesNoTouchMenu_InitButtonSprites(YesNoTouchMenu menu, YesNoTouchMenuParams menuParams)
    {
        for (int i = 0; i < ButtonFrameCount; i++)
        {
            menu.FrameThresholds[i] = (i + 1) << Fx32.Shift;
        }
        YesNoButtonSpriteTemplate template = new()
        {
            BgConfig = menu.BgConfig,
            BgLayer = menu.BgLayer,
            NarcID = MenuNarcID,
            BaseTile = menuParams.BaseTile,
            Palette = menuParams.Palette,
            FrameThresholds = menu.FrameThresholds,
            FrameCount = ButtonFrameCount,
            TilemapLeft = menu.TilemapLeft,
        };
        for (int i = 0; i < ButtonCount; i++)
        {
            for (int frame = 0; frame < ButtonFrameCount; frame++)
            {
                template.NarcMembers[frame] = i + FirstScreenMember + frame * 2;
            }
            template.TilemapTop = (byte)(menu.TilemapTop + i * ButtonRows);
            template.Palette += (uint)i;
            YesNoButtonSprite_Init(menu.ButtonSprites[i], template, menu.HeapID);
        }
    }

    // OFFSET 0x02015BA4
    private static void YesNoTouchMenu_RegisterTouchActions(YesNoTouchMenu menu, YesNoTouchMenuParams menuParams)
    {
        for (int i = 0; i < ButtonCount; i++)
        {
            menu.HitTable[i].Top = (byte)(menu.TilemapTop * 8 + i * ButtonSpacingPixels);
            menu.HitTable[i].Bottom = (byte)(menu.TilemapTop * 8 + i * ButtonSpacingPixels + ButtonHeightPixels);
            menu.HitTable[i].Left = (byte)(menu.TilemapLeft * 8);
            menu.HitTable[i].Right = (byte)(menu.TilemapLeft * 8 + ButtonWidthPixels);
        }
        menu.ButtonAction = TouchScreenActions.TouchScreenActions_RegisterHandler(menu.HitTable, ButtonCount, YesNoTouchMenu_OnButtonTouched, menu, menu.HeapID);
    }

    // OFFSET 0x02015C08
    private static void YesNoTouchMenu_OnButtonTouched(uint index, int state, object? context)
    {
        YesNoTouchMenu menu = (YesNoTouchMenu)context!;
        menu.ButtonState = (byte)(state & 0xF);
        if (state == 0)
        {
            menu.PressedButton = (byte)index;
            SoundPlayback.Sound_PlayEffect(PressSoundEffect);
        }
    }

    // OFFSET 0x02015C38
    private static void YesNoButtonSprite_Init(YesNoButtonSprite sprite, YesNoButtonSpriteTemplate template, int heapID)
    {
        YesNoButtonAnimData_Init(sprite.AnimData, template.FrameThresholds, template.FrameCount);
        sprite.BgConfig = template.BgConfig;
        sprite.BgLayer = template.BgLayer;
        sprite.TilemapLeft = template.TilemapLeft;
        sprite.TilemapTop = template.TilemapTop;
        for (int i = 0; i < ButtonFrameCount; i++)
        {
            sprite.NscrBuffers[i] = Graphics.Graphics_GetScrnData(template.NarcID, (uint)template.NarcMembers[i], 1, out sprite.ScreenData[i], heapID);
            YesNoTouchMenu_AddTileOffset(sprite.ScreenData[i]!, (int)template.BaseTile);
            YesNoTouchMenu_SetScreenPalette(sprite.ScreenData[i]!, (int)template.Palette);
        }
        YesNoButtonSprite_Draw(sprite);
        sprite.AnimFrame = 0;
    }

    // OFFSET 0x02015CC0
    private static bool YesNoButtonSprite_Animate(YesNoButtonSprite sprite)
    {
        uint frame = YesNoButtonAnimData_GetFrame(sprite.AnimData);
        if (sprite.AnimFrame != frame)
        {
            YesNoButtonSprite_Draw(sprite);
            sprite.AnimFrame = (byte)frame;
        }
        if (frame == sprite.AnimData.FrameCount - 1)
        {
            return true;
        }
        YesNoButtonAnimData_Advance(sprite.AnimData, AnimStep);
        return false;
    }

    // OFFSET 0x02015D00
    private static void YesNoButtonSprite_Free(YesNoButtonSprite sprite)
    {
        ScreenData screenData = sprite.ScreenData[0]!;
        Bg.Bg_FillTilemapRect(sprite.BgConfig!, (byte)sprite.BgLayer, 0, sprite.TilemapLeft, sprite.TilemapTop,
            (screenData.Width >> 3) & 0xFF, (screenData.Height >> 3) & 0xFF, 0);
        Bg.Bg_ScheduleTilemapTransfer(sprite.BgConfig!, (byte)sprite.BgLayer);
        YesNoButtonSprite_Clear(sprite);
    }

    // OFFSET 0x02015D60
    private static void YesNoButtonSprite_Draw(YesNoButtonSprite sprite)
    {
        uint frame = YesNoButtonAnimData_GetFrame(sprite.AnimData);
        YesNoTouchMenu_DrawScreenData(sprite.BgConfig, sprite.BgLayer, sprite.ScreenData[frame]!, sprite.TilemapLeft, sprite.TilemapTop);
    }

    // OFFSET 0x02015D8C
    private static void YesNoButtonAnimData_Init(YesNoButtonAnimData animData, int[]? frameThresholds, uint frameCount)
    {
        animData.FrameThresholds = frameThresholds;
        animData.FrameCount = frameCount;
        animData.FrameProgress = 0;
    }

    // OFFSET 0x02015D98
    private static void YesNoButtonAnimData_Advance(YesNoButtonAnimData animData, int amount)
    {
        animData.FrameProgress += amount;
    }

    // OFFSET 0x02015DA0
    private static uint YesNoButtonAnimData_GetFrame(YesNoButtonAnimData animData)
    {
        for (uint i = 0; i < animData.FrameCount; i++)
        {
            if (animData.FrameThresholds![i] >= animData.FrameProgress)
            {
                return i;
            }
        }
        return animData.FrameCount - 1;
    }

    // OFFSET 0x02015DCC
    private static void YesNoTouchMenu_LoadTilesAfterVBlank(int narcID, int narcMemberIdx, object? bgConfig, int bgLayer, uint baseTile, int heapID)
    {
        YesNoTilesLoadTask data = new();
        data.Buffer = Graphics.Graphics_GetCharData(narcID, (uint)narcMemberIdx, 1, out data.CharData, heapID);
        data.BgConfig = bgConfig;
        data.BgLayer = bgLayer;
        data.BaseTile = baseTile;
        SysTask.SysTask_ExecuteAfterVBlank(YesNoTouchMenu_LoadTilesTask, data, LoadTaskPriority);
    }

    // OFFSET 0x02015E1C
    private static void YesNoTouchMenu_LoadPaletteAfterVBlank(int narcID, int narcMemberIdx, int loadLocation, uint offset, uint size, int heapID)
    {
        YesNoPaletteLoadTask data = new();
        data.Buffer = Graphics.Graphics_GetPlttData(narcID, (uint)narcMemberIdx, out data.PaletteData, heapID);
        data.LoadLocation = loadLocation;
        data.Offset = offset;
        data.Size = size;
        SysTask.SysTask_ExecuteAfterVBlank(YesNoTouchMenu_LoadPaletteTask, data, LoadTaskPriority);
    }

    // OFFSET 0x02015E64
    private static void YesNoTouchMenu_LoadTilesTask(SysTask task, object? param)
    {
        YesNoTilesLoadTask data = (YesNoTilesLoadTask)param!;
        Bg.Bg_LoadTiles(data.BgConfig!, (byte)data.BgLayer, data.CharData!.RawData, data.CharData.SizeBytes, data.BaseTile);
        SysTask.SysTask_Done(task);
        data.Buffer = null;
    }

    // OFFSET 0x02015EA0
    private static void YesNoTouchMenu_LoadPaletteTask(SysTask task, object? param)
    {
        YesNoPaletteLoadTask data = (YesNoPaletteLoadTask)param!;
        if (data.LoadLocation == LoadLocationMainBg)
        {
            Video.LoadPaletteRange(Video.MainBackground, data.PaletteData!.Colors, 0, data.Offset, data.Size);
        }
        else if (data.LoadLocation == LoadLocationSubBg)
        {
            Video.LoadPaletteRange(Video.SubBackground, data.PaletteData!.Colors, 0, data.Offset, data.Size);
        }
        SysTask.SysTask_Done(task);
        data.Buffer = null;
    }

    // OFFSET 0x02015EE8
    private static void YesNoTouchMenu_DrawScreenData(object? bgConfig, int bgLayer, ScreenData screenData, byte x, byte y)
    {
        int width = (screenData.Width >> 3) & 0xFF;
        int height = (screenData.Height >> 3) & 0xFF;
        Bg.Bg_CopyToTilemapRect(bgConfig!, (byte)bgLayer, x, y, width, height, screenData.RawData, 0, 0, width, height);
        Bg.Bg_ScheduleTilemapTransfer(bgConfig!, (byte)bgLayer);
    }

    // OFFSET 0x02015F34
    private static void YesNoTouchMenu_AddTileOffset(ScreenData screenData, int offset)
    {
        int count = (int)(screenData.SizeBytes >> 1);
        for (int i = 0; i < count; i++)
        {
            Bin.W16(screenData.RawData, i * 2, (ushort)(Bin.U16(screenData.RawData, i * 2) + offset));
        }
    }

    // OFFSET 0x02015F54
    private static void YesNoTouchMenu_SetScreenPalette(ScreenData screenData, int palette)
    {
        int count = (int)(screenData.SizeBytes >> 1);
        for (int i = 0; i < count; i++)
        {
            int tile = Bin.U16(screenData.RawData, i * 2) & 0xFFF;
            Bin.W16(screenData.RawData, i * 2, (ushort)(tile | (palette << 12)));
        }
    }
}
