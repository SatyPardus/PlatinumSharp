namespace PlatinumSharp.Game;

public sealed class BattleBgSwitch
{
    public int Unread00;
    public byte State;
    public byte BlendCoeffA;
    public byte BlendCoeffB;
    public byte TargetBlendCoeffA;
    public byte TargetBlendCoeffB;
    public byte FadeType;
    public int BgID;
    public int Mode;
    public int Flags;
    public readonly int[] ScriptVars = new int[BattleAnimSystem.ScriptVarCount];
    public int BgMoveAnimActive;
    public int BgWaveAnimActive;
    public BattleAnimSystem? BattleAnimSystem;
}

public sealed class BgWaveStrip
{
    public short StartScanline;
    public short EndScanline;
    public short ScrollSpeed;
    public short ScrollAccum;
    public int BaseOffset;
}

public sealed class WaveAnimContext
{
    public readonly BgWaveStrip[] Strips = new BgWaveStrip[16];
    public CustomBgScrollContext? BgScrollCtx;

    public WaveAnimContext()
    {
        for (int i = 0; i < Strips.Length; i++)
        {
            Strips[i] = new BgWaveStrip();
        }
    }
}

public sealed class BattleBgAnim
{
    public BgConfig? BgConfig;
    public short OffsetX;
    public short OffsetY;
    public short StepX;
    public short StepY;
    public int Bg;
    public int UnusedBg;
    public int Unused;
    public int Cancel;
    public WaveAnimContext? WaveAnimCtx;
}

public static partial class BattleAnimSystems
{
    private const uint BgNarcIndicesAddress = 0x02238928;
    private const uint BgSwitchFlagsAddress = 0x02238594;
    private const uint BgWaveScrollSpeedsAddress = 0x02238660;
    private const int NarcBattBg = 7;
    private const int BgNarcMemberNcgr = 0;
    private const int BgNarcMemberNclr = 1;
    private const int BgNarcMemberNscr1 = 2;
    private const int BgNarcMemberNscr2 = 3;
    private const int BgNarcMemberNscr3 = 4;
    private const int BgVarMoveStepX = 0;
    private const int BgVarMoveStepY = 1;
    private const int BgVarMoveStartX = 2;
    private const int BgVarMoveStartY = 3;
    private const int BgVarFadeType = 4;
    private const int BgVarBlendType = 5;
    private const int BgVarAnimMode = 6;
    private const int BgVarScreenMode = 7;
    private const int BgReverseNever = 0;
    private const int BgReverseDefault = 2;
    private const int BgBlendPartial = 1;
    private const int BgBlendInversePartial = 2;
    private const int BgSwitchStateNone = 0;
    private const int BgSwitchStateRunning = 1;
    private const int BgSwitchStatePartial = 2;
    private const int BgSwitchModeCount = 3;
    private const uint BgSwitchTaskPriority = 0x44C;
    private const int BgControlParamColorMode = 0;
    private const int BgControlParamCharBase = 2;
    private const byte CharBase0C000 = 3;
    private const byte CharBase10000 = 4;
    private const byte ColorMode16 = 0;
    private const byte ColorMode256 = 1;
    private const int BlendMaskBase = 4;
    private const int BlendMaskEffect = 8;
    private const ushort PaletteFlagEffect = 0x200;
    private const ushort PlttBufMainBgF = 1;
    private const ushort FadeColorBlack = 0;
    private const ushort FadeColorWhite = 0xFFFF;
    private const int WaveStripCount = 0x10;

    private static readonly Func<SysTask, BattleBgSwitch, int>[] BgSwitchFuncs =
    {
        BattleBgSwitch_Blend,
        BattleBgSwitch_Fade,
        BattleBgSwitch_FlagsOnly,
        BattleBgRestore_Blend,
        BattleBgRestore_Fade,
        BattleBgRestore_FlagsOnly,
    };

    private static readonly Func<BattleBgSwitch, int>[] BgAnimFuncs =
    {
        BattleBgSwitch_AnimNone,
        BattleBgSwitch_AnimMoveStart,
        BattleBgSwitch_AnimStop,
        BattleBgSwitch_AnimCancel,
        BattleBgSwitch_AnimNone,
        BattleBgSwitch_AnimStartWave,
        BattleBgSwitch_AnimRegisterWave,
    };

    // OFFSET 0x02221A54
    private static int BattleBgSwitch_ShouldBeReversed(BattleBgSwitch bgSwitch, BattleAnimSystem system, int var)
    {
        int attackerSide = BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, system.Context!.Attacker);
        int defenderSide = BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, system.Context.Defender);
        if (bgSwitch.ScriptVars[var] == BgReverseDefault)
        {
            if (attackerSide == defenderSide)
            {
                return defenderSide == BattlerSidePlayer ? 0 : 1;
            }
            if (defenderSide == BattlerSidePlayer)
            {
                return 1;
            }
        }
        else if (bgSwitch.ScriptVars[var] != BgReverseNever && defenderSide == BattlerSidePlayer)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02221AA8
    private static void BattleBgSwitch_SetBg(BattleBgSwitch bgSwitch, BattleAnimSystem system, int bgLayer, int bgID)
    {
        Graphics.Graphics_LoadTilesToBgLayer(NarcBattBg, (uint)BattleAnimSystem_GetBgNarcMemberIndex(bgID, BgNarcMemberNcgr), system.BgConfig!, (uint)bgLayer, 0, 0, 1, system.HeapID);
        PaletteData.PaletteData_LoadBufferFromFileStart(system.PaletteData!, NarcBattBg, BattleAnimSystem_GetBgNarcMemberIndex(bgID, BgNarcMemberNclr), system.HeapID, PlttBufMainBg, PaletteSizeBytes, 0x90);
        Bg.Bg_ClearTilemap(system.BgConfig!, bgLayer);
        int memberType = BgNarcMemberNscr1;
        if (BattleAnimSystem_IsContest(system) == 1)
        {
            memberType = BgNarcMemberNscr3;
        }
        else if (BattleBgSwitch_ShouldBeReversed(bgSwitch, system, BgVarScreenMode) == 1)
        {
            memberType = BgNarcMemberNscr2;
        }
        Graphics.Graphics_LoadTilemapToBgLayer(NarcBattBg, (uint)BattleAnimSystem_GetBgNarcMemberIndex(bgID, memberType), system.BgConfig!, (uint)bgLayer, 0, 0, 1, system.HeapID);
    }

    // OFFSET 0x02221B54
    private static int BattleBgSwitch_IsFlagSet(int flags, uint flag)
    {
        return (((uint)flags << 16) & flag) == flag ? 1 : 0;
    }

    // OFFSET 0x02221B64
    private static void BattleBgSwitch_ApplyFlags(BattleBgSwitch bgSwitch)
    {
        uint[] flags = RomData.OverlayU32(Overlay, BgSwitchFlagsAddress, 4);
        for (int i = 0; i < 4; i++)
        {
            if (BattleBgSwitch_IsFlagSet(bgSwitch.Flags, flags[i]) == 0)
            {
                continue;
            }
            int id = 0;
            int flag = (int)(flags[i] >> 16);
            while (flag >= 2)
            {
                flag /= 2;
                id++;
            }
            BgAnimFuncs[id](bgSwitch);
        }
    }

    // OFFSET 0x02221BBC
    private static BattleBgSwitch BattleAnimSystem_CreateBgSwitch(BattleAnimSystem system)
    {
        BattleBgSwitch bgSwitch = new();
        bgSwitch.Unread00 = 0;
        bgSwitch.State = 0;
        bgSwitch.BattleAnimSystem = system;
        bgSwitch.BlendCoeffA = 0;
        bgSwitch.BlendCoeffB = 31;
        bgSwitch.TargetBlendCoeffA = 29;
        bgSwitch.TargetBlendCoeffB = 2;
        if (BattleAnimSystem_GetScriptVar(system, BgVarBlendType) == BgBlendPartial)
        {
            bgSwitch.BlendCoeffA = 0;
            bgSwitch.BlendCoeffB = 31;
            bgSwitch.TargetBlendCoeffA = 15;
            bgSwitch.TargetBlendCoeffB = 7;
        }
        if (BattleAnimSystem_GetScriptVar(system, BgVarBlendType) == BgBlendInversePartial)
        {
            bgSwitch.BlendCoeffA = 7;
            bgSwitch.BlendCoeffB = 15;
            bgSwitch.TargetBlendCoeffA = 29;
            bgSwitch.TargetBlendCoeffB = 2;
        }
        for (int i = 0; i < BattleAnimSystem.ScriptVarCount; i++)
        {
            bgSwitch.ScriptVars[i] = system.ScriptVars[i];
        }
        system.BgSwitchState = BgSwitchStateRunning;
        return bgSwitch;
    }

    private static int StepBlendCoefficients(BattleBgSwitch bgSwitch)
    {
        int done = 0;
        if (bgSwitch.BlendCoeffA < bgSwitch.TargetBlendCoeffA)
        {
            bgSwitch.BlendCoeffA += 2;
        }
        else
        {
            done++;
        }
        if (bgSwitch.BlendCoeffB > bgSwitch.TargetBlendCoeffB)
        {
            bgSwitch.BlendCoeffB -= 2;
        }
        else
        {
            done++;
        }
        return done;
    }

    // OFFSET 0x02221C50
    private static int BattleBgSwitch_Blend(SysTask task, BattleBgSwitch bgSwitch)
    {
        BattleAnimSystem system = bgSwitch.BattleAnimSystem!;
        switch (bgSwitch.State)
        {
            case 0:
            {
                BattleAnimSystem_LoadBaseBg(system, BattleBgBase);
                int effectPrio = BattleAnimSystem_GetBgPriority(system, 2);
                BattleAnimSystem_GetBgPriority(system, 1);
                Bg.Bg_SetPriority(BattleBgEffect, (byte)effectPrio);
                Bg.Bg_SetPriority(BattleBgBase, (byte)effectPrio);
                Bg.Bg_ToggleLayer(BattleBgBase, 1);
                bgSwitch.State++;
                return 1;
            }
            case 1:
                Bg.Bg_SetControlParam(system.BgConfig!, BattleBgEffect, BgControlParamCharBase, CharBase0C000);
                if (BattleAnimSystem_IsContest(system) != 1)
                {
                    Bg.Bg_SetControlParam(system.BgConfig!, BattleBgEffect, BgControlParamColorMode, ColorMode16);
                }
                BattleBgSwitch_SetBg(bgSwitch, system, BattleBgEffect, bgSwitch.BgID);
                Video.SetBlendAlpha(0, BlendMaskBase, BlendMaskEffect, bgSwitch.BlendCoeffB, bgSwitch.BlendCoeffA);
                BattleBgSwitch_ApplyFlags(bgSwitch);
                bgSwitch.State++;
                goto case 2;
            case 2:
                if (StepBlendCoefficients(bgSwitch) == 2)
                {
                    bgSwitch.BlendCoeffA = bgSwitch.TargetBlendCoeffA;
                    bgSwitch.BlendCoeffB = bgSwitch.TargetBlendCoeffB;
                    bgSwitch.State++;
                }
                Video.SetBlendAlphaCoefficients(0, (ushort)((bgSwitch.BlendCoeffA << 8) | bgSwitch.BlendCoeffB));
                return bgSwitch.State != 2 ? 0 : 1;
            default:
                return 0;
        }
    }

    // OFFSET 0x02221D50
    private static int BattleBgRestore_Blend(SysTask task, BattleBgSwitch bgSwitch)
    {
        BattleAnimSystem system = bgSwitch.BattleAnimSystem!;
        switch (bgSwitch.State)
        {
            case 0:
                Bg.Bg_ToggleLayer(BattleBgBase, 1);
                bgSwitch.State++;
                goto case 1;
            case 1:
            {
                int effectPrio = BattleAnimSystem_GetBgPriority(system, 2);
                BattleAnimSystem_GetBgPriority(system, 1);
                Bg.Bg_SetPriority(BattleBgEffect, (byte)effectPrio);
                Bg.Bg_SetPriority(BattleBgBase, (byte)effectPrio);
                Video.SetBlendAlpha(0, BlendMaskBase, BlendMaskEffect, bgSwitch.BlendCoeffA, bgSwitch.BlendCoeffB);
                BattleBgSwitch_ApplyFlags(bgSwitch);
                bgSwitch.State++;
                goto case 2;
            }
            case 2:
                if (StepBlendCoefficients(bgSwitch) == 2)
                {
                    bgSwitch.BlendCoeffA = (byte)(bgSwitch.TargetBlendCoeffA + 2);
                    bgSwitch.BlendCoeffB = (byte)(bgSwitch.TargetBlendCoeffB - 2);
                    bgSwitch.State++;
                }
                Video.SetBlendAlphaCoefficients(0, (ushort)((bgSwitch.BlendCoeffB << 8) | bgSwitch.BlendCoeffA));
                return 1;
            case 3:
                if (bgSwitch.BgMoveAnimActive == 1)
                {
                    BattleAnimSystem_CancelBgAnim(system);
                }
                if (bgSwitch.BgWaveAnimActive == 1)
                {
                    BattleAnimSystem_CancelBgAnim(system);
                }
                Bg.Bg_SetOffset(system.BgConfig!, BattleBgEffect, 0, 0);
                Bg.Bg_SetOffset(system.BgConfig!, BattleBgEffect, 3, 0);
                Bg.Bg_SetControlParam(system.BgConfig!, BattleBgEffect, BgControlParamCharBase, CharBase10000);
                if (BattleAnimSystem_IsContest(system) == 0)
                {
                    Bg.Bg_SetControlParam(system.BgConfig!, BattleBgEffect, BgControlParamColorMode, ColorMode256);
                    BattleAnimSystem_LoadBattleBgTiles(system, BattleBgEffect);
                    BattleAnimSystem_LoadBattleBgPaletteBuffer(system);
                }
                else
                {
                    LoadContestBaseBg(system, BattleBgEffect);
                }
                Graphics.Graphics_LoadTilemapToBgLayer(system.BattleBgRefs.NarcID, (uint)system.BattleBgRefs.TilemapNarcMemberIdx, system.BgConfig!, BattleBgEffect, 0, 0, 1, system.HeapID);
                bgSwitch.State++;
                return 1;
            case 4:
                if (BattleAnimSystem_IsContest(system) == 0)
                {
                    Battle.Battle_ClearBlendControl();
                }
                else
                {
                    throw Unported.Partial(0x022413D8);
                }
                BattleAnimSystem_UnloadBaseBg(system, BattleBgBase);
                bgSwitch.State++;
                return 1;
            default:
                return 0;
        }
    }

    private static void LoadContestBaseBg(BattleAnimSystem system, int layer)
    {
        Graphics.Graphics_LoadTilesToBgLayer(system.BattleBgRefs.NarcID, (uint)system.BattleBgRefs.TilesNarcMemberIdx, system.BgConfig!, (uint)layer, 0, 0, 1, system.HeapID);
        PaletteData.PaletteData_LoadBufferFromFileStart(system.PaletteData!, system.BattleBgRefs.NarcID, system.BattleBgRefs.PaletteNarcMemberIdx, system.HeapID, PlttBufMainBg, (uint)(system.BattleBgRefs.PaletteSrcSize * PaletteSizeBytes), (ushort)system.BattleBgRefs.PaletteDestStart);
    }

    private static ushort FadeColor(BattleBgSwitch bgSwitch)
    {
        return bgSwitch.FadeType == 0 ? FadeColorBlack : FadeColorWhite;
    }

    // OFFSET 0x02221F44
    private static int BattleBgSwitch_Fade(SysTask task, BattleBgSwitch bgSwitch)
    {
        BattleAnimSystem system = bgSwitch.BattleAnimSystem!;
        switch (bgSwitch.State)
        {
            case 0:
                PaletteData.PaletteData_StartFade(system.PaletteData!, PlttBufMainBgF, (ushort)system.BaseBgPalettes, 0, 0, 16, FadeColor(bgSwitch));
                PaletteData.PaletteData_BlendMulti(system.PaletteData!, PlttBufMainBg, PaletteFlagEffect, 16, FadeColor(bgSwitch));
                bgSwitch.State++;
                goto case 1;
            case 1:
                if (PaletteData.PaletteData_GetSelectedBuffersMask(system.PaletteData!) != 0)
                {
                    return 1;
                }
                Bg.Bg_SetControlParam(system.BgConfig!, BattleBgEffect, BgControlParamColorMode, ColorMode16);
                BattleBgSwitch_SetBg(bgSwitch, system, BattleBgEffect, bgSwitch.BgID);
                PaletteData.PaletteData_StartFade(system.PaletteData!, PlttBufMainBgF, PaletteFlagEffect, 0, 16, 0, FadeColor(bgSwitch));
                BattleBgSwitch_ApplyFlags(bgSwitch);
                system.BgSwitchState = BgSwitchStatePartial;
                bgSwitch.State++;
                return 1;
            default:
                if (PaletteData.PaletteData_GetSelectedBuffersMask(system.PaletteData!) != 0)
                {
                    return 1;
                }
                return 0;
        }
    }

    // OFFSET 0x0222206C
    private static int BattleBgRestore_Fade(SysTask task, BattleBgSwitch bgSwitch)
    {
        BattleAnimSystem system = bgSwitch.BattleAnimSystem!;
        switch (bgSwitch.State)
        {
            case 0:
                BattleBgSwitch_ApplyFlags(bgSwitch);
                bgSwitch.State++;
                goto case 1;
            case 1:
                PaletteData.PaletteData_StartFade(system.PaletteData!, PlttBufMainBgF, PaletteFlagEffect, 0, 0, 16, FadeColor(bgSwitch));
                PaletteData.PaletteData_BlendMulti(system.PaletteData!, PlttBufMainBg, (ushort)system.BaseBgPalettes, 16, FadeColor(bgSwitch));
                bgSwitch.State++;
                goto case 2;
            case 2:
                if (PaletteData.PaletteData_GetSelectedBuffersMask(system.PaletteData!) != 0)
                {
                    return 1;
                }
                if (bgSwitch.BgMoveAnimActive == 1)
                {
                    BattleAnimSystem_CancelBgAnim(system);
                }
                if (bgSwitch.BgWaveAnimActive == 1)
                {
                    BattleAnimSystem_CancelBgAnim(system);
                }
                Bg.Bg_ToggleLayer(BattleBgEffect, 0);
                if (BattleAnimSystem_IsContest(system) == 0)
                {
                    Bg.Bg_SetControlParam(system.BgConfig!, BattleBgEffect, BgControlParamColorMode, ColorMode256);
                    BattleAnimSystem_LoadBattleBgTiles(system, BattleBgEffect);
                    BattleAnimSystem_LoadBattleBgPaletteBuffer(system);
                }
                else
                {
                    LoadContestBaseBg(system, BattleBgEffect);
                }
                Graphics.Graphics_LoadTilemapToBgLayer(system.BattleBgRefs.NarcID, (uint)system.BattleBgRefs.TilemapNarcMemberIdx, system.BgConfig!, BattleBgEffect, 0, 0, 1, system.HeapID);
                Bg.Bg_ToggleLayer(BattleBgEffect, 1);
                bgSwitch.State++;
                goto case 3;
            case 3:
                Bg.Bg_SetOffset(system.BgConfig!, BattleBgEffect, 0, 0);
                Bg.Bg_SetOffset(system.BgConfig!, BattleBgEffect, 3, 0);
                PaletteData.PaletteData_StartFade(system.PaletteData!, PlttBufMainBgF, (ushort)system.BaseBgPalettes, 0, 16, 0, FadeColor(bgSwitch));
                bgSwitch.State++;
                goto default;
            default:
                if (PaletteData.PaletteData_GetSelectedBuffersMask(system.PaletteData!) != 0)
                {
                    return 1;
                }
                system.BgSwitchState = BgSwitchStatePartial;
                bgSwitch.State++;
                return 0;
        }
    }

    // OFFSET 0x022222A4
    private static int BattleBgSwitch_FlagsOnly(SysTask task, BattleBgSwitch bgSwitch)
    {
        BattleBgSwitch_ApplyFlags(bgSwitch);
        return 0;
    }

    // OFFSET 0x022222B0
    private static int BattleBgRestore_FlagsOnly(SysTask task, BattleBgSwitch bgSwitch)
    {
        BattleBgSwitch_ApplyFlags(bgSwitch);
        if (bgSwitch.BgMoveAnimActive == 1)
        {
            BattleAnimSystem_CancelBgAnim(bgSwitch.BattleAnimSystem!);
        }
        return 0;
    }

    // OFFSET 0x022222D4
    private static void BattleBgAnimTask_Move(SysTask task, object? param)
    {
        BattleBgAnim bgAnim = (BattleBgAnim)param!;
        if (bgAnim.Cancel == 1)
        {
            SysTask.SysTask_Done(task);
            return;
        }
        bgAnim.OffsetX += bgAnim.StepX;
        bgAnim.OffsetY += bgAnim.StepY;
        if (bgAnim.StepX != 0)
        {
            Bg.Bg_SetOffset(bgAnim.BgConfig!, (byte)bgAnim.Bg, 0, bgAnim.OffsetX);
        }
        if (bgAnim.StepY != 0)
        {
            Bg.Bg_SetOffset(bgAnim.BgConfig!, (byte)bgAnim.Bg, 3, bgAnim.OffsetY);
        }
    }

    // OFFSET 0x02222338
    public static void BattleAnimSystem_CancelBgAnim(BattleAnimSystem system)
    {
        if (system == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (system!.BgAnim == null)
        {
            return;
        }
        system.BgAnim.Cancel = 1;
    }

    // OFFSET 0x02222354
    public static int BattleAnimSystem_GetBaseBgPalettes(BattleAnimSystem system)
    {
        return system.BaseBgPalettes;
    }

    // OFFSET 0x0222235C
    private static int BattleBgSwitch_AnimNone(BattleBgSwitch bgSwitch)
    {
        return 0;
    }

    // OFFSET 0x02222360
    private static int BattleBgSwitch_AnimMoveStart(BattleBgSwitch bgSwitch)
    {
        BattleAnimSystem system = bgSwitch.BattleAnimSystem!;
        BattleBgAnim bgAnim = new();
        bgAnim.BgConfig = system.BgConfig;
        bgAnim.OffsetX = (short)system.ScriptVars[BgVarMoveStartX];
        bgAnim.OffsetY = (short)system.ScriptVars[BgVarMoveStartY];
        bgAnim.StepX = (short)system.ScriptVars[BgVarMoveStepX];
        bgAnim.StepY = (short)system.ScriptVars[BgVarMoveStepY];
        bgAnim.Bg = BattleBgEffect;
        bgAnim.UnusedBg = BattleBgEffect;
        if (BattleBgSwitch_ShouldBeReversed(bgSwitch, system, BgVarAnimMode) == 1)
        {
            bgAnim.StepX = (short)-bgAnim.StepX;
            bgAnim.StepY = (short)-bgAnim.StepY;
            bgAnim.OffsetX = (short)-bgAnim.OffsetX;
            bgAnim.OffsetY = (short)-bgAnim.OffsetY;
        }
        bgAnim.Unused = 1;
        bgAnim.Cancel = 0;
        system.BgAnim = bgAnim;
        bgSwitch.BgMoveAnimActive = 1;
        SysTask.SysTask_Start(BattleBgAnimTask_Move, bgAnim, TrackingTaskPriority);
        return 0;
    }

    // OFFSET 0x0222240C
    private static int BattleBgSwitch_AnimStartWave(BattleBgSwitch bgSwitch)
    {
        BattleAnimSystem system = bgSwitch.BattleAnimSystem!;
        BattleBgAnim battleBgAnim = new();
        battleBgAnim.WaveAnimCtx = new WaveAnimContext();
        system.BgAnim = battleBgAnim;
        bgSwitch.BgWaveAnimActive = 1;
        battleBgAnim.Cancel = 0;
        battleBgAnim.WaveAnimCtx.BgScrollCtx = BattleAnimUtil.CustomBgScrollContext_New(BattleAnimUtil.BattleAnimUtil_GetHOffsetRegisterForBg(BattleAnimSystem_GetBgID(system, 2)), (uint)BattleAnimUtil.BattleAnimUtil_MakeBgOffsetValue(0, 0), system.HeapID);
        ushort[] speeds = RomData.OverlayU16(Overlay, BgWaveScrollSpeedsAddress, WaveStripCount);
        for (int i = 0; i < WaveStripCount; i++)
        {
            BgWaveStrip strip = battleBgAnim.WaveAnimCtx.Strips[i];
            strip.StartScanline = (short)(i * 8);
            strip.EndScanline = (short)(strip.StartScanline + 8);
            strip.ScrollSpeed = (short)speeds[i];
            strip.ScrollAccum = 0;
            strip.BaseOffset = BattleAnimUtil.BattleAnimUtil_MakeBgOffsetValue(0, 0);
            if (BattleBgSwitch_ShouldBeReversed(bgSwitch, system, BgVarAnimMode) == 1)
            {
                strip.ScrollSpeed = (short)-strip.ScrollSpeed;
            }
        }
        SysTask.SysTask_Start(BattleBgAnim_WaveTask, battleBgAnim, DrawTaskPriority);
        return 0;
    }

    // OFFSET 0x022224E4
    private static int BattleBgSwitch_AnimRegisterWave(BattleBgSwitch bgSwitch)
    {
        bgSwitch.BgWaveAnimActive = 1;
        return 0;
    }

    // OFFSET 0x022224F8
    private static void BattleBgAnim_WaveTask(SysTask task, object? param)
    {
        BattleBgAnim bgAnim = (BattleBgAnim)param!;
        WaveAnimContext waveCtx = bgAnim.WaveAnimCtx!;
        if (bgAnim.Cancel == 1)
        {
            BattleAnimUtil.CustomBgScrollContext_Free(waveCtx.BgScrollCtx!);
            SysTask.SysTask_Done(task);
            return;
        }
        uint[] buffer = BattleAnimUtil.CustomBgScrollContext_GetWriteBuffer(waveCtx.BgScrollCtx!);
        for (int stripIdx = 0; stripIdx < WaveStripCount; stripIdx++)
        {
            BgWaveStrip strip = waveCtx.Strips[stripIdx];
            strip.ScrollAccum += strip.ScrollSpeed;
            for (int scanline = strip.StartScanline; scanline < strip.EndScanline; scanline++)
            {
                short baseX = (short)(strip.BaseOffset & 0xFFFF);
                short baseY = (short)(strip.BaseOffset >> 16);
                buffer[scanline] = (uint)BattleAnimUtil.BattleAnimUtil_MakeBgOffsetValue((ushort)(baseX + strip.ScrollAccum), (ushort)baseY);
            }
        }
    }

    // OFFSET 0x02222590
    public static void BattleAnimSystem_LoadBaseBg(BattleAnimSystem system, int bgLayer)
    {
        int effectPrio = BattleAnimSystem_GetBgPriority(system, 2);
        Bg.Bg_SetPriority((byte)bgLayer, (byte)effectPrio);
        Bg.Bg_ToggleLayer((byte)bgLayer, 0);
        if (BattleAnimSystem_IsContest(system) == 1)
        {
            Bg.Bg_SetControlParam(system.BgConfig!, bgLayer, BgControlParamCharBase, CharBase10000);
        }
        else
        {
            Bg.Bg_SetControlParam(system.BgConfig!, bgLayer, BgControlParamColorMode, ColorMode256);
            Bg.Bg_SetControlParam(system.BgConfig!, bgLayer, BgControlParamCharBase, CharBase10000);
        }
        Bg.Bg_ClearTilemap(system.BgConfig!, bgLayer);
        if (BattleAnimSystem_IsContest(system) == 1)
        {
            Graphics.Graphics_LoadTilesToBgLayer(system.BattleBgRefs.NarcID, (uint)system.BattleBgRefs.TilesNarcMemberIdx, system.BgConfig!, (uint)bgLayer, 0, 0, 1, system.HeapID);
        }
        else
        {
            BattleAnimSystem_LoadBattleBgTiles(system, bgLayer);
        }
        Graphics.Graphics_LoadTilemapToBgLayer(system.BattleBgRefs.NarcID, (uint)system.BattleBgRefs.TilemapNarcMemberIdx, system.BgConfig!, (uint)bgLayer, 0, 0, 1, system.HeapID);
    }

    // OFFSET 0x02222664
    public static void BattleAnimSystem_UnloadBaseBg(BattleAnimSystem system, int bgLayer)
    {
        int basePrio = BattleAnimSystem_GetBgPriority(system, 1);
        Bg.Bg_SetPriority((byte)bgLayer, (byte)basePrio);
        if (BattleAnimSystem_IsContest(system) == 1)
        {
            Bg.Bg_SetControlParam(system.BgConfig!, bgLayer, BgControlParamCharBase, CharBase0C000);
        }
        else
        {
            Bg.Bg_SetControlParam(system.BgConfig!, bgLayer, BgControlParamColorMode, ColorMode16);
            Bg.Bg_SetControlParam(system.BgConfig!, bgLayer, BgControlParamCharBase, CharBase0C000);
        }
        Bg.Bg_ClearTilemap(system.BgConfig!, bgLayer);
    }

    // OFFSET 0x022226D0
    private static int BattleBgSwitch_AnimStop(BattleBgSwitch bgSwitch)
    {
        bgSwitch.BgMoveAnimActive = 1;
        return 0;
    }

    // OFFSET 0x022226E8
    private static int BattleBgSwitch_AnimCancel(BattleBgSwitch bgSwitch)
    {
        BattleAnimSystem_CancelBgAnim(bgSwitch.BattleAnimSystem!);
        return 0;
    }

    // OFFSET 0x022226F4
    private static void BattleBgSwitchTask_Start(SysTask task, object? param)
    {
        BattleBgSwitch bgSwitch = (BattleBgSwitch)param!;
        int active = BgSwitchFuncs[bgSwitch.Mode](task, bgSwitch);
        if (active == 0)
        {
            bgSwitch.BattleAnimSystem!.BgSwitchState = BgSwitchStateNone;
            SysTask.SysTask_Done(task);
        }
    }

    // OFFSET 0x02222724
    private static void BattleAnimScriptCmd_SwitchBg(BattleAnimSystem system)
    {
        BattleBgSwitch bgSwitch = BattleAnimSystem_CreateBgSwitch(system);
        bgSwitch.FadeType = (byte)BattleAnimSystem_GetScriptVar(system, BgVarFadeType);
        Next(system);
        bgSwitch.BgID = (int)ReadWord(system);
        Next(system);
        uint param = ReadWord(system);
        Next(system);
        bgSwitch.Mode = (int)(param & 0xFFFF);
        bgSwitch.Flags = (int)(param >> 16);
        SysTask.SysTask_Start(BattleBgSwitchTask_Start, bgSwitch, BgSwitchTaskPriority);
    }

    // OFFSET 0x02222774
    private static void BattleAnimScriptCmd_SetBgSwitchVar(BattleAnimSystem system)
    {
        Next(system);
        int var = (int)ReadWord(system);
        Next(system);
        short value = (short)ReadWord(system);
        Next(system);
        switch (var)
        {
            case BgVarMoveStepX:
                system.BgAnim!.StepX = value;
                break;
            case BgVarMoveStepY:
                system.BgAnim!.StepY = value;
                break;
            case BgVarMoveStartX:
            case BgVarMoveStartY:
                system.BgAnim!.OffsetX = value;
                break;
        }
    }

    // OFFSET 0x022227CC
    private static void BattleAnimScriptCmd_RestoreBg(BattleAnimSystem system)
    {
        BattleBgSwitch bgSwitch = BattleAnimSystem_CreateBgSwitch(system);
        bgSwitch.FadeType = (byte)BattleAnimSystem_GetScriptVar(system, BgVarFadeType);
        Next(system);
        bgSwitch.BgID = (int)ReadWord(system);
        Next(system);
        uint param = ReadWord(system);
        Next(system);
        bgSwitch.Mode = (int)(param & 0xFFFF) + BgSwitchModeCount;
        bgSwitch.Flags = (int)(param >> 16);
        SysTask.SysTask_Start(BattleBgSwitchTask_Start, bgSwitch, BgSwitchTaskPriority);
    }

    // OFFSET 0x02222820
    private static void BattleAnimScriptCmd_WaitForPartialBgSwitch(BattleAnimSystem system)
    {
        if (system.BgSwitchState == BgSwitchStatePartial)
        {
            Next(system);
            system.ScriptDelay = 0;
        }
        else
        {
            system.ScriptDelay = 1;
        }
    }

    // OFFSET 0x02222840
    private static void BattleAnimScriptCmd_WaitForBgSwitch(BattleAnimSystem system)
    {
        if (system.BgSwitchState == BgSwitchStateNone)
        {
            Next(system);
            system.ScriptDelay = 0;
        }
        else
        {
            system.ScriptDelay = 1;
        }
    }

    // OFFSET 0x02222860
    private static void BattleAnimScriptCmd_SetBg(BattleAnimSystem system)
    {
        Next(system);
        int bgID = (int)ReadWord(system);
        Next(system);
        Graphics.Graphics_LoadTilesToBgLayer(NarcBattBg, (uint)BattleAnimSystem_GetBgNarcMemberIndex(bgID, BgNarcMemberNcgr), system.BgConfig!, BattleBgEffect, 0, 0, 1, system.HeapID);
        Graphics.Graphics_LoadPalette(NarcBattBg, (uint)BattleAnimSystem_GetBgNarcMemberIndex(bgID, BgNarcMemberNclr), 0, 0, 0, system.HeapID);
        Graphics.Graphics_LoadTilemapToBgLayer(NarcBattBg, (uint)BattleAnimSystem_GetBgNarcMemberIndex(bgID, BgNarcMemberNscr1), system.BgConfig!, BattleBgEffect, 0, 0, 1, system.HeapID);
    }

    // OFFSET 0x022228DC
    private static void BattleAnimScriptCmd_SwitchBgEx(BattleAnimSystem system)
    {
        BattleBgSwitch bgSwitch = BattleAnimSystem_CreateBgSwitch(system);
        Next(system);
        int battleBgPlayerAttack = (int)ReadWord(system);
        Next(system);
        int battleBgEnemyAttack = (int)ReadWord(system);
        Next(system);
        int contestBg = (int)ReadWord(system);
        Next(system);
        if (BattleAnimSystem_IsContest(system) == 1)
        {
            bgSwitch.BgID = contestBg;
        }
        else if (BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, system.Context!.Defender) == BattlerSidePlayer)
        {
            bgSwitch.BgID = battleBgEnemyAttack;
        }
        else
        {
            bgSwitch.BgID = battleBgPlayerAttack;
        }
        SysTask.SysTask_Start(BattleBgSwitchTask_Start, bgSwitch, BgSwitchTaskPriority);
    }

    // OFFSET 0x022234E4
    public static int BattleAnimSystem_GetBgNarcMemberIndex(int bgID, int type)
    {
        uint[] row = RomData.OverlayU32(Overlay, (uint)(BgNarcIndicesAddress + bgID * 0x14), 5);
        return (int)row[type];
    }
}
