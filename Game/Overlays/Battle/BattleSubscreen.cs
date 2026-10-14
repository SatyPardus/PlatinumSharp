using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static partial class BattleSubscreens
{
    private const int HeapIDBattle = 5;
    private const int NarcBattleGraphics = 7;
    private const int TilemapBufferCount = 7;
    private const int TilemapBufferSize = 0x800;
    private const int PaletteBufferSize = 0x200;
    private const int MovePaletteBufferSize = 0x40;
    private const int MovePaletteCopySize = 0x20;
    private const int FontOAMCount = 13;
    private const int NoMenu = -1;
    private const int BackgroundCount = 0x17;
    private const uint TilemapMemberTable = 0x02270264;
    private const uint BackgroundPaletteTable = 0x022704E4;
    private const int MemberNone = 0xFFFF;
    private const int TilemapMemberCommandPanel = 0x31;
    private const int TilemapMemberFrontierCommandPanel = 0xAA;
    private const int PaletteMemberNormal = 0xF2;
    private const int PaletteMemberFrontier = 0x154;
    private const int MovePaletteMemberDefault = 0x10B;
    private const int MovePaletteMemberFrontier = 0x155;
    private const int PaletteBufferSubBg = 1;
    private const int MoveSlots = 4;
    private const int MoveIconSpriteSize = 6;
    private const uint SpriteSizeTable = 0x020F4030;
    private const uint TaskPriorityPulseCursor = 0x51E;
    private const uint TaskPrioritySpeedUpPalette = 0xD6D8;

    private static ushort[]? tilemapMembers;
    private static ushort[]? backgroundPalettes;

    // OFFSET 0x0208C098
    public static int SpriteSize_GetTileBytes(int size)
    {
        return RomData.Arm9U16(SpriteSizeTable + (uint)size * 2, 1)[0];
    }

    // OFFSET 0x0226871C
    private static BattleSubscreen BattleSubscreen_Alloc()
    {
        BattleSubscreen subscreen = new();
        subscreen.ActiveMenuConfigIndex = NoMenu;
        return subscreen;
    }

    // OFFSET 0x022687C8
    public static BattleSubscreen BattleSubscreen_New(NARC battleNarc, NARC subscreenNarc, BattleSystem battleSys, uint trainerGender, byte[]? suppressActivationSfx)
    {
        BattleSubscreen subscreen = BattleSubscreen_Alloc();
        subscreen.BattleSys = battleSys;
        subscreen.TrainerGender = (byte)trainerGender;
        subscreen.SuppressActivationSfxPtr = suppressActivationSfx;
        if (suppressActivationSfx != null)
        {
            subscreen.SuppressActivationSfx = suppressActivationSfx[0];
        }
        PaletteData paletteData = BattleSystems.BattleSystem_GetPaletteData(battleSys)!;
        int background = BattleSystems.BattleSystem_GetBackground(battleSys);
        if (background >= BackgroundCount)
        {
            background = 0;
            ErrorHandling.GF_AssertFail();
        }
        subscreen.FontOAMManager = FontOAMSystem.FontOAMManager_New(FontOAMCount, HeapIDBattle);
        subscreen.PulseCursorTask = SysTask.SysTask_Start(BattleSubscreen_PulseCursorTask, subscreen, TaskPriorityPulseCursor);
        tilemapMembers ??= RomData.OverlayU16(16, TilemapMemberTable, TilemapBufferCount);
        backgroundPalettes ??= RomData.OverlayU16(16, BackgroundPaletteTable, BackgroundCount * 2);
        for (int i = 0; i < TilemapBufferCount; i++)
        {
            subscreen.TilemapBuffers[i] = new byte[TilemapBufferSize];
            int member = tilemapMembers[i];
            if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeFrontier) != 0 && member == TilemapMemberCommandPanel)
            {
                member = TilemapMemberFrontierCommandPanel;
            }
            byte[]? file = Graphics.Graphics_GetScrnData(NarcBattleGraphics, (uint)member, 1, out ScreenData? screenData, HeapIDBattle);
            Array.Copy(screenData!.RawData, 0, subscreen.TilemapBuffers[i]!, 0, TilemapBufferSize);
            Heap.Heap_Free(file!);
        }
        int paletteMember = (BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeFrontier) != 0 ? PaletteMemberFrontier : PaletteMemberNormal;
        subscreen.SubscreenPaletteBuf = new ushort[PaletteBufferSize / 2];
        PaletteData.PaletteData_LoadBufferFromFileStart(paletteData, NarcBattleGraphics, paletteMember, HeapIDBattle, PaletteBufferSubBg, 0, 0);
        if (backgroundPalettes[background * 2] != MemberNone)
        {
            PaletteData.PaletteData_LoadBufferFromFileStart(paletteData, NarcBattleGraphics, backgroundPalettes[background * 2], HeapIDBattle, PaletteBufferSubBg, 0x20, 0);
        }
        Array.Copy(PaletteData.PaletteData_GetUnfadedBuffer(paletteData, PaletteBufferSubBg)!, subscreen.SubscreenPaletteBuf, PaletteBufferSize / 2);
        for (int i = 0; i < MoveSlots; i++)
        {
            MoveDisplayInfo info = subscreen.MoveDisplayData[i].MoveDisplayInfo;
            for (int slot = 0; slot < MoveSlots; slot++)
            {
                info.Move[slot] = 0xFFFF;
                info.CurPP[slot] = 0xFFFF;
                info.MaxPP[slot] = 0xFFFF;
            }
        }
        for (int i = 0; i < MoveSlots; i++)
        {
            for (int slot = 0; slot < MoveSlots; slot++)
            {
                subscreen.MoveDisplayData[i].MoveIcons[slot] = new byte[SpriteSize_GetTileBytes(MoveIconSpriteSize)];
            }
        }
        subscreen.NormalMovePalBuf = new ushort[MovePaletteBufferSize / 2];
        subscreen.SpeedUpMovePalBuf = new ushort[MovePaletteBufferSize / 2];
        Array.Copy(subscreen.SubscreenPaletteBuf, 0, subscreen.NormalMovePalBuf, 0, MovePaletteCopySize / 2);
        Array.Copy(subscreen.SubscreenPaletteBuf, 0xE0 / 2, subscreen.NormalMovePalBuf, MovePaletteCopySize / 2, MovePaletteCopySize / 2);
        int movePaletteMember;
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeFrontier) != 0)
        {
            movePaletteMember = MovePaletteMemberFrontier;
        }
        else
        {
            movePaletteMember = backgroundPalettes[background * 2 + 1];
            if (movePaletteMember == MemberNone)
            {
                movePaletteMember = MovePaletteMemberDefault;
            }
        }
        byte[]? paletteFile = Graphics.Graphics_GetPlttData(NarcBattleGraphics, (uint)movePaletteMember, out PaletteFile? palette, HeapIDBattle);
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeFrontier) != 0)
        {
            Array.Copy(palette!.Colors, subscreen.SpeedUpMovePalBuf, MovePaletteBufferSize / 2);
        }
        else
        {
            Array.Copy(palette!.Colors, subscreen.SpeedUpMovePalBuf, MovePaletteCopySize / 2);
        }
        Heap.Heap_Free(paletteFile!);
        RenderText.RenderControlFlags_ZeroSpeedUpBattle();
        RenderText.RenderControlFlags_ZeroWaitBattle();
        subscreen.SpeedUpPaletteTask = SysTask.SysTask_Start(BattleSubscreen_SpeedUpPaletteTask, subscreen, TaskPrioritySpeedUpPalette);
        return subscreen;
    }

    // OFFSET 0x0226BCE4
    private static void BattleSubscreen_PulseCursorTask(SysTask task, object? param)
    {
        BattleSubscreen subscreen = (BattleSubscreen)param!;
        PaletteData paletteData = BattleSystems.BattleSystem_GetPaletteData(subscreen.BattleSys!)!;
        if (PaletteData.PaletteData_GetSelectedBuffersMask(paletteData) != 0)
        {
            return;
        }
        PaletteData.PaletteData_Blend(paletteData, PaletteBufferSubBg, 1, 1, (byte)(subscreen.PulseCursorBlendValue >> 8), PulseCursorColor);
        if (subscreen.PulseCursorDirection == 0)
        {
            subscreen.PulseCursorBlendValue += PulseCursorStep;
        }
        else
        {
            subscreen.PulseCursorBlendValue -= PulseCursorStep;
        }
        if (subscreen.PulseCursorBlendValue >= PulseCursorMax)
        {
            subscreen.PulseCursorBlendValue = PulseCursorMax - PulseCursorStep;
            subscreen.PulseCursorDirection = 1;
        }
        else if (subscreen.PulseCursorBlendValue <= 0)
        {
            subscreen.PulseCursorBlendValue = PulseCursorMin;
            subscreen.PulseCursorDirection = 0;
        }
    }

    // OFFSET 0x0226BD74
    private static void BattleSubscreen_SpeedUpPaletteTask(SysTask task, object? param)
    {
        BattleSubscreen subscreen = (BattleSubscreen)param!;
        int speedUp = RenderText.RenderControlFlags_GetSpeedUpBattle();
        int wait = RenderText.RenderControlFlags_GetWaitBattle();
        RenderText.RenderControlFlags_ZeroSpeedUpBattle();
        RenderText.RenderControlFlags_ZeroWaitBattle();
        PaletteData paletteData = BattleSystems.BattleSystem_GetPaletteData(subscreen.BattleSys!)!;
        if (PaletteData.PaletteData_GetSelectedBuffersMask(paletteData) != 0)
        {
            return;
        }
        ushort[] faded = PaletteData.PaletteData_GetFadedBuffer(paletteData, PaletteBufferSubBg)!;
        ushort[] compare;
        ushort[] source;
        if (Globals.gSystem.TouchHeld != 0 && (speedUp == 1 || wait == 1))
        {
            compare = subscreen.NormalMovePalBuf!;
            source = subscreen.SpeedUpMovePalBuf!;
        }
        else
        {
            compare = subscreen.SpeedUpMovePalBuf!;
            source = subscreen.NormalMovePalBuf!;
        }
        for (int i = 0; i < MovePaletteHalfColors; i++)
        {
            if (faded[MovePaletteHalfColors + i] != compare[MovePaletteHalfColors + i])
            {
                return;
            }
        }
        if ((BattleSystems.BattleSystem_GetBattleType(subscreen.BattleSys!) & BattleSystems.BattleTypeFrontier) != 0)
        {
            Array.Copy(source, 0, faded, 0, MovePaletteCopySize / 2);
            Array.Copy(source, MovePaletteCopySize / 2, faded, 0xE0 / 2, MovePaletteCopySize / 2);
        }
        else
        {
            Array.Copy(source, MovePaletteHalfColors, faded, MovePaletteHalfColors, MovePaletteHalfColors);
        }
    }
}

public static partial class BattleSubscreens
{
    private const ushort PulseCursorColor = 0x7E37;
    private const short PulseCursorStep = 0x200;
    private const short PulseCursorMax = 0x1100;
    private const short PulseCursorMin = 0x100;
    private const int MovePaletteHalfColors = 8;
}

public static partial class BattleSystems
{
    // OFFSET 0x0223F6E4
    public static ushort BattleSystem_GetNetworkID(BattleSystem battleSys)
    {
        return battleSys.NetworkID;
    }
}
