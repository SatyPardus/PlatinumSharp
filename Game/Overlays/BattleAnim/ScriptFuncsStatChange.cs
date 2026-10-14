namespace PlatinumSharp.Game;

public sealed class StatChangeConfig
{
    public int NarcID;
    public int MemberTableIndex;
    public int Mode;
    public short StepX;
    public short StepY;
    public short OffsetX;
    public short OffsetY;
    public ushort OverlayAlpha;
    public ushort OtherAlpha;
}

public sealed class StatChangeContext
{
    public readonly StatChangeConfig Config = new();
    public readonly BattleAnimScriptFuncCommon Common = new();
    public ManagedSprite? AttackerSprite;
    public ManagedSprite? AttackerSprite2;
    public ManagedSprite? AttackerPartnerSprite;
    public byte Timer;
}

public static partial class BattleAnimScriptFuncs
{
    private const uint StatChangeMemberTableAddress = 0x0223A1D8;
    private const int StatChangeMemberRowSize = 0x10;
    private const int StatChangeBgLayer = 2;
    private const int StatChangeNarc = 7;
    private const int StatChangeOverlayMoveFrames = 20;
    private const int StatChangeOtherTargetAlpha = 16;
    private const int StatChangeBlendFirst = 4;
    private const int StatChangeBlendSecond = 0x39;
    private const byte StatChangeObjWindowInside = 0x36;
    private const byte StatChangeWindowOutside = 0x3B;
    private const byte StatChangeWindowReset = 0x1F;
    private const int StatChangeWindowObj = 4;
    private const int BgOffsetSetX = 0;
    private const int BgOffsetSetY = 3;

    // OFFSET 0x02234E44
    private static void StatChangeConfig_SetStepSize(StatChangeConfig config, short stepX, short stepY)
    {
        config.StepX = stepX;
        config.StepY = stepY;
    }

    // OFFSET 0x02234E4C
    private static void StatChangeConfig_SetInitAlpha(StatChangeConfig config, ushort overlayAlpha, ushort otherAlpha)
    {
        config.OverlayAlpha = overlayAlpha;
        config.OtherAlpha = otherAlpha;
    }

    private static uint StatChangeMember(int row, int column)
    {
        return RomData.OverlayU32(12, (uint)(StatChangeMemberTableAddress + row * StatChangeMemberRowSize + column * 4), 1)[0];
    }

    // OFFSET 0x02234E54
    private static void StatChangeContext_LoadBg(StatChangeContext ctx, int bgLayer)
    {
        int heapID = BattleAnimSystems.BattleAnimSystem_GetHeapID(ctx.Common.BattleAnimSys!);
        Bg.Bg_SetPriority((byte)bgLayer, (byte)BattleAnimSystems.BattleAnimSystem_GetBgPriority(ctx.Common.BattleAnimSys!, 3));
        Graphics.Graphics_LoadTilesToBgLayer(ctx.Config.NarcID, StatChangeMember(ctx.Config.MemberTableIndex, 0), ctx.Common.BgConfig!, (uint)bgLayer, 0, 0, 1, heapID);
        PaletteData.PaletteData_LoadBufferFromFileStart(ctx.Common.PaletteData!, ctx.Config.NarcID, (int)StatChangeMember(ctx.Config.MemberTableIndex, 1), heapID, 0, 0x20, 0x80);
        Bg.Bg_ClearTilemap(ctx.Common.BgConfig!, (byte)bgLayer);
        Graphics.Graphics_LoadTilemapToBgLayer(ctx.Config.NarcID, StatChangeMember(ctx.Config.MemberTableIndex, 2), ctx.Common.BgConfig!, (uint)bgLayer, 0, 0, 1, heapID);
    }

    // OFFSET 0x02234EF0
    private static void BattleAnimTask_StatChange(SysTask task, object? param)
    {
        StatChangeContext ctx = (StatChangeContext)param!;
        switch (ctx.Common.State)
        {
            case 0:
                StatChangeContext_LoadBg(ctx, StatChangeBgLayer);
                ctx.Common.State++;
                break;
            case 1:
                Video.SetBlendAlpha(0, StatChangeBlendFirst, StatChangeBlendSecond, ctx.Config.OverlayAlpha, ctx.Config.OtherAlpha);
                ctx.Common.State++;
                break;
            case 2:
                if (ctx.Timer > StatChangeOverlayMoveFrames)
                {
                    ctx.Common.State++;
                }
                else
                {
                    ctx.Timer++;
                }
                break;
            case 3:
                if (ctx.Config.OverlayAlpha != 0)
                {
                    ctx.Config.OverlayAlpha--;
                }
                if (ctx.Config.OtherAlpha < StatChangeOtherTargetAlpha)
                {
                    ctx.Config.OtherAlpha++;
                }
                if (ctx.Config.OverlayAlpha == 0 && ctx.Config.OtherAlpha == StatChangeOtherTargetAlpha)
                {
                    Bg.Bg_ClearTilemap(ctx.Common.BgConfig!, StatChangeBgLayer);
                    SpriteSystem.ManagedSprite_SetDrawFlag(ctx.AttackerSprite!, 0);
                    SpriteSystem.ManagedSprite_SetDrawFlag(ctx.AttackerSprite2!, 0);
                    SpriteSystem.ManagedSprite_SetDrawFlag(ctx.AttackerPartnerSprite!, 0);
                    ctx.Common.State++;
                }
                Video.SetBlendAlphaCoefficients(0, (ushort)(ctx.Config.OverlayAlpha | (ctx.Config.OtherAlpha << 8)));
                break;
            default:
                Video.SetWindowOutside(0, StatChangeWindowReset);
                Video.SetObjWindowInside(0, StatChangeWindowReset);
                Bg.Bg_SetOffset(ctx.Common.BgConfig!, StatChangeBgLayer, BgOffsetSetX, 0);
                Bg.Bg_SetOffset(ctx.Common.BgConfig!, StatChangeBgLayer, BgOffsetSetY, 0);
                Video.SetVisibleWindows(0, 0);
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.Common.BattleAnimSys!, task);
                BattleAnimUtil.BattleAnimUtil_Free(ctx);
                return;
        }
        ctx.Config.OffsetX = (short)(ctx.Config.OffsetX + ctx.Config.StepX);
        ctx.Config.OffsetY = (short)(ctx.Config.OffsetY + ctx.Config.StepY);
        Bg.Bg_SetOffset(ctx.Common.BgConfig!, StatChangeBgLayer, BgOffsetSetX, ctx.Config.OffsetX);
        Bg.Bg_SetOffset(ctx.Common.BgConfig!, StatChangeBgLayer, BgOffsetSetY, ctx.Config.OffsetY);
        SpriteSystem.ManagedSprite_TickFrame(ctx.AttackerSprite!);
        SpriteSystem.ManagedSprite_TickFrame(ctx.AttackerSprite2!);
        SpriteSystem.ManagedSprite_TickFrame(ctx.AttackerPartnerSprite!);
        SpriteSystem.SpriteSystem_DrawSprites(ctx.Common.PokemonSpriteManager);
    }

    // OFFSET 0x0223505C
    private static void StatChangeConfig_Init(BattleAnimSystem system, StatChangeConfig config)
    {
        config.NarcID = StatChangeNarc;
        config.MemberTableIndex = BattleAnimSystems.BattleAnimSystem_GetScriptVar(system, 0);
        config.Mode = BattleAnimSystems.BattleAnimSystem_GetScriptVar(system, 1);
    }

    // OFFSET 0x0223507C
    private static void BattleAnimScriptFunc_StatChangeCommon(BattleAnimSystem system, StatChangeConfig config)
    {
        StatChangeContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<StatChangeContext>(system);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        ctx.Config.NarcID = config.NarcID;
        ctx.Config.MemberTableIndex = config.MemberTableIndex;
        ctx.Config.Mode = config.Mode;
        ctx.Config.StepX = config.StepX;
        ctx.Config.StepY = config.StepY;
        ctx.Config.OffsetX = config.OffsetX;
        ctx.Config.OffsetY = config.OffsetY;
        ctx.Config.OverlayAlpha = config.OverlayAlpha;
        ctx.Config.OtherAlpha = config.OtherAlpha;
        ctx.AttackerSprite = BattleAnimSystems.BattleAnimSystem_GetPokemonSprite(ctx.Common.BattleAnimSys!, 0);
        ctx.AttackerSprite2 = BattleAnimSystems.BattleAnimSystem_GetPokemonSprite(ctx.Common.BattleAnimSys!, 1);
        ctx.AttackerPartnerSprite = BattleAnimSystems.BattleAnimSystem_GetPokemonSprite(ctx.Common.BattleAnimSys!, 2);
        BattleAnimSystems.BattleAnimSystem_GetBgPriority(ctx.Common.BattleAnimSys!, 2);
        SpriteSystem.ManagedSprite_SetExplicitPriority(ctx.AttackerSprite, 2);
        SpriteSystem.ManagedSprite_SetExplicitPriority(ctx.AttackerSprite2, 2);
        int target = ctx.Config.Mode == 0 ? BattleAnimSystems.BattleAnimSystem_GetAttacker(system) : BattleAnimSystems.BattleAnimSystem_GetDefender(system);
        int type = BattleAnimUtil.BattleAnimUtil_GetBattlerType(system, target);
        if (BattleAnimSystems.BattleAnimSystem_IsContest(system) == 1)
        {
            SpriteSystem.ManagedSprite_SetDrawFlag(ctx.AttackerPartnerSprite, 0);
            int priority = BattleAnimSystems.BattleAnimSystem_GetBgPriority(ctx.Common.BattleAnimSys!, 2);
            SpriteSystem.ManagedSprite_SetExplicitPriority(ctx.AttackerSprite, priority);
            SpriteSystem.ManagedSprite_SetExplicitPriority(ctx.AttackerSprite2, priority);
        }
        else if ((uint)type <= 1)
        {
            SpriteSystem.ManagedSprite_SetDrawFlag(ctx.AttackerPartnerSprite, 0);
        }
        else if ((uint)(type - 3) <= 1)
        {
            SpriteSystem.ManagedSprite_SetExplicitPriority(ctx.AttackerPartnerSprite, 3);
        }
        else
        {
            SpriteSystem.ManagedSprite_SetExplicitPriority(ctx.AttackerPartnerSprite, 1);
        }
        Video.SetObjWindowInside(0, StatChangeObjWindowInside);
        Video.SetWindowOutside(0, StatChangeWindowOutside);
        Video.SetVisibleWindows(0, StatChangeWindowObj);
        SpriteSystem.ManagedSprite_SetExplicitOamMode(ctx.AttackerSprite2, 2);
        ctx.Config.OffsetX = 0;
        ctx.Config.OffsetY = 0;
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(ctx.Common.BattleAnimSys!, BattleAnimTask_StatChange, ctx);
    }

    private static void BattleAnimScriptFunc_StatChange(BattleAnimSystem system, short stepY)
    {
        StatChangeConfig config = new();
        StatChangeConfig_Init(system, config);
        StatChangeConfig_SetStepSize(config, 0, stepY);
        StatChangeConfig_SetInitAlpha(config, 12, 4);
        BattleAnimScriptFunc_StatChangeCommon(system, config);
    }

    // OFFSET 0x022351A4
    private static void BattleAnimScriptFunc_StatChangeUp(BattleAnimSystem system)
    {
        BattleAnimScriptFunc_StatChange(system, 3);
    }

    // OFFSET 0x022351D0
    private static void BattleAnimScriptFunc_StatChangeDown(BattleAnimSystem system)
    {
        BattleAnimScriptFunc_StatChange(system, -3);
    }

    // OFFSET 0x022351FC
    private static void BattleAnimScriptFunc_StatChangeHeal(BattleAnimSystem system)
    {
        BattleAnimScriptFunc_StatChange(system, 3);
    }

    // OFFSET 0x02235228
    private static void BattleAnimScriptFunc_StatChangeMetal(BattleAnimSystem system)
    {
        BattleAnimScriptFunc_StatChange(system, -6);
    }
}
