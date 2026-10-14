using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class BallThrow
{
    public int Type;
    public int HeapID;
    public int Mode;
    public int Target;
    public int BallID;
    public int BgPrio;
    public int Surface;
    public SpriteSystem? SpriteSys;
    public PaletteData? PaletteData;
    public BattleSystem? BattleSys;

    public BallThrow Clone()
    {
        return (BallThrow)MemberwiseClone();
    }
}

public sealed class BallRotation
{
    public const int StateFinished = 0xFF;

    public int Unknown00;
    public int Unknown04;
    public int State;
    public int SubState;
    public int Counter;
    public int Signal;
    public int Unknown18;
    public int Active;
    public byte Alpha;
    public byte AlphaStep;
    public sbyte SpinDirection;
    public int TickAnimation;
    public int Finished;
    public SpriteManager? SpriteMan;
    public ManagedSprite? Sprite;
    public BallThrow Template = new();
    public short X;
    public short Y;
    public short TargetX;
    public short TargetY;
    public int Frames;
    public int PathCounter;
    public int ArcHeight;
    public SysTask? Task;
    public PaletteFadeContext? Fade;
    public int Delay;
    public readonly XYTransformContext LinearContext = new();
    public readonly XYTransformContext RevolutionContext = new();
}

public static partial class BallRotations
{
    public const int ModeCount = 14;
    public const int ModeHold = 4;

    private const int NarcBattleSubscreen = 0x1B;
    private const int ResourceBase = 0x1770;
    private const int MaxSprites = 10;
    private const int PaletteBufferMainObj = 2;
    private const int VRamMain = 1;
    private const int DisplayMain = 0;
    private const int SubScreenViewY = 0x110000;
    private const uint TaskPriority = 1000;
    private const uint BallResourceTable = 0x0223AF00;
    private const uint BattlerPositionTable = 0x02239E34;
    private const int BattlerPositionRowSize = 0x18;
    private const int FirstSpecialBall = 0xFF;
    private const int LastBall = 0x113;
    private const int DefaultBallIndex = 3;
    private const int AffineOverwriteDouble = 2;

    private static readonly int[] StaticBallTypes = { 3, 5, 1 };
    private static readonly int[] ThrownBallTypes = { 6, 7, 8, 9, 10, 11, 12, 13, 15, 16, 17 };
    private static readonly int[] FadingBallTypes = { 6, 7, 8, 9, 10, 11 };

    private const uint FadeTaskPriority = 0x3EA;
    private const int SpinSpeed = 0x2000;
    private const int BallIDBait = 0x111;
    private const int SeqBaitLand = 0x7E8;

    private static readonly Func<BallRotation, int>[] ModeFuncs =
    {
        BallRotation_ThrowMode,
        rotation => throw Missing(0x022371E4),
        rotation => throw Missing(0x02237608),
        rotation => throw Missing(0x02236F10),
        BallRotation_HoldMode,
        rotation => throw Missing(0x02237474),
        rotation => throw Missing(0x02236788),
        rotation => throw Missing(0x022367B0),
        rotation => throw Missing(0x022367D8),
        rotation => throw Missing(0x02236800),
        rotation => throw Missing(0x02236828),
        rotation => throw Missing(0x02236850),
        rotation => throw Missing(0x02236878),
        rotation => throw Missing(0x022368A0),
    };

    // OFFSET 0x02220474
    public static void BattleAnim_SetDefaultAlphaBlending()
    {
        Video.SetBlendAlpha(DisplayMain, 0, 0x3F, 8, 8);
    }

    // OFFSET 0x02225864
    public static void BattleAnim_GetBattlerPosition(int isDoubles, int battlerType, out short x, out short y)
    {
        byte[] raw = RomData.Overlay(12, (uint)(BattlerPositionTable + isDoubles * BattlerPositionRowSize + battlerType * 4), 4);
        x = (short)Nitro.Bin.U16(raw, 0);
        y = (short)Nitro.Bin.U16(raw, 2);
    }

    // OFFSET 0x02235E94
    public static int Ball_GetResourceIndex(int ballID)
    {
        if (ballID < 1 || ballID > LastBall)
        {
            ballID = DefaultBallIndex + 1;
        }
        else if (ballID >= FirstSpecialBall)
        {
            ballID -= FirstSpecialBall;
        }
        return ballID - 1;
    }

    // OFFSET 0x02235EB0
    public static int BallRotation_IsThrownType(int type)
    {
        for (int i = 0; i < ThrownBallTypes.Length; i++)
        {
            if (type == ThrownBallTypes[i])
            {
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x02235EF0
    public static int BallRotation_IsFadingType(int type)
    {
        for (int i = 0; i < FadingBallTypes.Length; i++)
        {
            if (type == FadingBallTypes[i])
            {
                return 1;
            }
        }
        return 0;
    }

    private static PaletteFadeContext BallRotation_StartFade(BallRotation rotation)
    {
        ushort offset = (ushort)(SpriteSystem.ManagedSprite_GetExplicitPaletteOffset(rotation.Sprite!) << 4);
        return BattleAnimUtil.PaletteFadeContext_New(rotation.Template.PaletteData!, rotation.Template.HeapID, PaletteBufferMainObj, offset, 0x10, -2, 2, 0, 0xE, 0xFFFF, FadeTaskPriority);
    }

    // OFFSET 0x02236F24
    private static int BallRotation_ThrowMode(BallRotation rotation)
    {
        int type = rotation.Template.Type;
        switch (rotation.State)
        {
            case 0:
                BattleAnimUtil.ParabolicContext_Init(rotation.LinearContext, rotation.RevolutionContext, rotation.X, rotation.TargetX, rotation.Y, rotation.TargetY, (ushort)rotation.Frames, rotation.ArcHeight << 12);
                rotation.PathCounter = 0;
                rotation.State++;
                rotation.Fade = null;
                if (BallRotation_IsThrownType(type) == 1)
                {
                    SpriteSystem.ManagedSprite_OffsetAffineZRotation(rotation.Sprite!, (MathUtil.LCRNG_Next() % 20 + 10) << 13);
                }
                break;
            case 1:
            {
                int half = (rotation.Frames + (int)((uint)rotation.Frames >> 31)) >> 1;
                if (BallRotation_IsThrownType(type) == 1)
                {
                    SpriteSystem.ManagedSprite_OffsetAffineZRotation(rotation.Sprite!, SpinSpeed);
                    if (rotation.PathCounter > half + 10)
                    {
                        SpriteSystem.ManagedSprite_OffsetAffineZRotation(rotation.Sprite!, SpinSpeed);
                    }
                    if (BallRotation_IsFadingType(type) == 1 && rotation.PathCounter == half + 10)
                    {
                        rotation.Fade = BallRotation_StartFade(rotation);
                    }
                }
                rotation.PathCounter++;
                if (type >= 6 && type <= 8 && rotation.PathCounter > half && rotation.PathCounter < half + 5)
                {
                    break;
                }
                if (BattleAnimUtil.ParabolicContext_UpdateAndApplyToSprite(rotation.LinearContext, rotation.RevolutionContext, rotation.Sprite!) != 0)
                {
                    break;
                }
                BallRotation_SetAffineZRotation(rotation, 0);
                rotation.State++;
                break;
            }
            case 2:
                if (BallRotation_IsFadingType(type) == 1 && rotation.Fade != null)
                {
                    if (BattleAnimUtil.PaletteFadeContext_IsActive(rotation.Fade) == 1)
                    {
                        break;
                    }
                    BattleAnimUtil.PaletteFadeContext_Free(rotation.Fade);
                    rotation.Finished = 1;
                    return 0;
                }
                if (type >= 0xF)
                {
                    rotation.Finished = 1;
                    return 0;
                }
                if (rotation.Template.BallID == BallIDBait)
                {
                    SoundPlayback.Sound_PlayPannedEffect(SeqBaitLand, 0x75);
                }
                rotation.State++;
                break;
            case 3:
                SpriteSystem.ManagedSprite_SetAnim(rotation.Sprite!, 1);
                BallRotation_SetTickAnimation(rotation, 1);
                rotation.PathCounter = 0;
                rotation.State++;
                break;
            default:
                if (rotation.PathCounter == 5)
                {
                    if (type != 0xC && type != 0xD)
                    {
                        rotation.Fade = BallRotation_StartFade(rotation);
                    }
                    rotation.Finished = 1;
                }
                rotation.PathCounter++;
                if (rotation.PathCounter <= 0xF)
                {
                    break;
                }
                if (type != 0xC && type != 0xD)
                {
                    if (BattleAnimUtil.PaletteFadeContext_IsActive(rotation.Fade) == 1)
                    {
                        break;
                    }
                    BattleAnimUtil.PaletteFadeContext_Free(rotation.Fade);
                }
                return 0;
        }
        return 1;
    }

    // OFFSET 0x02235F2C
    public static int BallRotation_TypeAnimates(int type)
    {
        for (int i = 0; i < StaticBallTypes.Length; i++)
        {
            if (type == StaticBallTypes[i])
            {
                return 0;
            }
        }
        return 1;
    }

    // OFFSET 0x02235FC8
    public static int Ball_GetResourceMember(int ballID, int resource)
    {
        int index = Ball_GetResourceIndex(ballID);
        byte[] raw = RomData.Overlay(12, (uint)(BallResourceTable + index * 16 + resource * 4), 4);
        return (int)Nitro.Bin.U32(raw, 0);
    }

    // OFFSET 0x02237694
    private static int BallRotation_HoldMode(BallRotation rotation)
    {
        if (rotation.State == BallRotation.StateFinished)
        {
            return 0;
        }
        if (rotation.Sprite != null)
        {
            SpriteSystem.ManagedSprite_GetPositionXY(rotation.Sprite, out short x, out short y);
            rotation.X = x;
            rotation.Y = y;
        }
        return 1;
    }

    // OFFSET 0x022376D0
    private static void BallRotation_UpdateTask(SysTask task, object? param)
    {
        BallRotation rotation = (BallRotation)param!;
        if (rotation.Active == 0)
        {
            return;
        }
        if (rotation.Delay > 0)
        {
            rotation.Delay--;
            SpriteSystem.SpriteSystem_DrawSprites(rotation.SpriteMan);
            return;
        }
        if (ModeFuncs[rotation.Template.Mode](rotation) == 0)
        {
            rotation.Active = 0;
        }
        if (rotation.TickAnimation == 1)
        {
            SpriteSystem.ManagedSprite_TickFrame(rotation.Sprite!);
        }
        SpriteSystem.SpriteSystem_DrawSprites(rotation.SpriteMan);
    }

    // OFFSET 0x02237728
    public static BallRotation BallRotation_New(BallThrow template)
    {
        BallRotation rotation = new();
        rotation.Template = template.Clone();
        rotation.State = 0;
        rotation.Counter = 0;
        rotation.SubState = 0;
        rotation.Signal = 0;
        rotation.Unknown18 = 0;
        rotation.Unknown00 = 0;
        rotation.Unknown04 = 0;
        rotation.SpriteMan = SpriteSystem.SpriteManager_New(rotation.Template.SpriteSys);
        rotation.Delay = 0;
        rotation.TickAnimation = BallRotation_TypeAnimates(rotation.Template.Type);
        rotation.PathCounter = 0;
        rotation.Alpha = 0x10;
        rotation.AlphaStep = 0;
        BattleAnim_SetDefaultAlphaBlending();
        if (MathUtil.LCRNG_Next() % 2 != 0)
        {
            rotation.SpinDirection = 1;
        }
        else
        {
            rotation.SpinDirection = -1;
        }
        BallRotation_LoadResources(rotation);
        BallRotation_CreateSprite(rotation);
        BallRotation_InitPath(rotation);
        rotation.Active = 1;
        rotation.Finished = 0;
        rotation.Task = SysTask.SysTask_Start(BallRotation_UpdateTask, rotation, TaskPriority);
        return rotation;
    }

    // OFFSET 0x022377F8
    public static int BallRotation_IsActive(BallRotation? rotation)
    {
        if (rotation == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return rotation!.Active != 0 ? 1 : 0;
    }

    // OFFSET 0x02237810
    public static int BallRotation_IsFinished(BallRotation? rotation)
    {
        if (rotation == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return rotation!.Finished != 0 ? 1 : 0;
    }

    // OFFSET 0x02237828
    public static void BallRotation_SetSignal(BallRotation rotation, int signal)
    {
        rotation.Signal = signal;
    }

    // OFFSET 0x0223782C
    public static int BallRotation_IsSignal(BallRotation rotation, int signal)
    {
        return rotation.Signal == signal ? 1 : 0;
    }

    // OFFSET 0x0223783C
    public static void BallRotation_Free(BallRotation? rotation)
    {
        if (rotation == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteSystem.SpriteSystem_FreeResourcesAndManager(rotation!.Template.SpriteSys!, rotation.SpriteMan!);
        SpriteSystem.Sprite_DeleteAndFreeResources(rotation.Sprite!);
        SysTask.SysTask_Done(rotation.Task!);
    }

    // OFFSET 0x0223786C
    public static void BallRotation_SetMode(BallRotation rotation, int mode)
    {
        rotation.Template.Mode = mode;
        rotation.State = 0;
        rotation.SubState = 0;
        rotation.Counter = 0;
        rotation.Active = 1;
        SpriteSystem.ManagedSprite_GetPositionXY(rotation.Sprite!, out rotation.X, out rotation.Y);
    }

    // OFFSET 0x02237890
    public static int BallRotation_GetMode(BallRotation? rotation)
    {
        if (rotation == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return rotation!.Template.Mode;
    }

    // OFFSET 0x022378A0
    private static void BallRotation_InitPath(BallRotation rotation)
    {
        SpriteSystem.ManagedSprite_GetPositionXY(rotation.Sprite!, out rotation.X, out rotation.Y);
        switch (rotation.Template.Type)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
                SpriteSystem.ManagedSprite_GetPositionXY(rotation.Sprite!, out rotation.TargetX, out rotation.TargetY);
                rotation.ArcHeight = 0;
                rotation.Frames = 12;
                return;
            case 6:
                BattleAnim_GetBattlerPosition(0, 0, out rotation.TargetX, out rotation.TargetY);
                rotation.ArcHeight = 0x30;
                rotation.TargetY += 0x20;
                break;
            case 7:
                SpriteSystem.ManagedSprite_GetPositionXY(rotation.Sprite!, out rotation.X, out rotation.Y);
                BattleAnim_GetBattlerPosition(1, 4, out rotation.TargetX, out rotation.TargetY);
                rotation.ArcHeight = 0x30;
                rotation.TargetY += 0x20;
                break;
            case 8:
                BattleAnim_GetBattlerPosition(1, 2, out rotation.TargetX, out rotation.TargetY);
                rotation.ArcHeight = 0x30;
                rotation.TargetY += 0x20;
                break;
            case 9:
                BattleAnim_GetBattlerPosition(0, 1, out rotation.TargetX, out rotation.TargetY);
                rotation.ArcHeight = 0x30;
                rotation.TargetY += 0x20;
                break;
            case 10:
                BattleAnim_GetBattlerPosition(1, 3, out rotation.TargetX, out rotation.TargetY);
                rotation.ArcHeight = 0x30;
                rotation.TargetY += 0x20;
                break;
            case 11:
                BattleAnim_GetBattlerPosition(1, 5, out rotation.TargetX, out rotation.TargetY);
                rotation.ArcHeight = 0x30;
                rotation.TargetY += 0x20;
                break;
            case 12:
                BattleAnim_GetBattlerPosition(0, 1, out rotation.TargetX, out rotation.TargetY);
                rotation.ArcHeight = 0x10;
                rotation.TargetY += 0x10;
                rotation.Frames = 12;
                return;
            case 13:
                BattleAnim_GetBattlerPosition(0, 1, out rotation.TargetX, out rotation.TargetY);
                rotation.ArcHeight = 0x20;
                rotation.TargetX -= 0x14;
                rotation.TargetY += 0x26;
                rotation.Frames = 16;
                return;
            case 14:
                rotation.TargetX = 0x80;
                rotation.TargetY = 0x40;
                rotation.ArcHeight = 0x20;
                rotation.TargetY += 0x20;
                break;
            case 15:
                BattleAnim_GetBattlerPosition(0, 1, out rotation.TargetX, out rotation.TargetY);
                rotation.ArcHeight = 0x40;
                rotation.TargetY += 8;
                rotation.Frames = 16;
                return;
            case 16:
                BattleAnim_GetBattlerPosition(1, 3, out rotation.TargetX, out rotation.TargetY);
                rotation.ArcHeight = 0x40;
                rotation.TargetY += 8;
                rotation.Frames = 16;
                return;
            case 17:
                BattleAnim_GetBattlerPosition(1, 5, out rotation.TargetX, out rotation.TargetY);
                rotation.ArcHeight = 0x40;
                rotation.TargetY += 8;
                rotation.Frames = 16;
                return;
        }
        rotation.Frames = 20;
    }

    // OFFSET 0x02237B14
    private static void BallRotation_GetStartPosition(BallRotation rotation, ref short x, ref short y)
    {
        switch (rotation.Template.Type)
        {
            case 0:
                BattleAnim_GetBattlerPosition(0, 0, out x, out y);
                y += 0x28;
                break;
            case 1:
                BattleAnim_GetBattlerPosition(0, 1, out x, out y);
                y += 0x26;
                break;
            case 2:
                BattleAnim_GetBattlerPosition(1, 2, out x, out y);
                y += 0x28;
                break;
            case 3:
                BattleAnim_GetBattlerPosition(1, 3, out x, out y);
                y += 0x26;
                break;
            case 4:
                BattleAnim_GetBattlerPosition(1, 4, out x, out y);
                y += 0x28;
                break;
            case 5:
                BattleAnim_GetBattlerPosition(1, 5, out x, out y);
                y += 0x26;
                break;
            case 6:
            case 7:
            case 8:
                x = 10;
                y = 100;
                break;
            case 9:
            case 10:
            case 11:
            case 12:
            case 13:
                BattleAnim_GetBattlerPosition(0, 0, out x, out y);
                break;
            case 14:
                x = 0xE6;
                y = 0x20;
                break;
            case 15:
            case 16:
            case 17:
                x = -30;
                y = 0xA0;
                break;
        }
    }

    // OFFSET 0x02237C54
    private static void BallRotation_LoadResources(BallRotation rotation)
    {
        BallThrow template = rotation.Template;
        SpriteSystem.SpriteSystem_InitSprites(template.SpriteSys, rotation.SpriteMan, MaxSprites);
        if (template.Surface == 0)
        {
            SpriteUtil.SetSubScreenViewRect(SpriteSystem.SpriteSystem_GetRenderer(template.SpriteSys!), 0, SubScreenViewY);
        }
        int[] capacities = { MaxSprites, MaxSprites, MaxSprites, MaxSprites, 0, 0 };
        SpriteSystem.SpriteSystem_InitManagerWithCapacities(template.SpriteSys, rotation.SpriteMan, capacities);
        int tiles = Ball_GetResourceMember(template.BallID, 0);
        int palette = Ball_GetResourceMember(template.BallID, 1);
        int cell = Ball_GetResourceMember(template.BallID, 2);
        int anim = Ball_GetResourceMember(template.BallID, 3);
        NARC narc = NARC.NARC_ctor(NarcBattleSubscreen, template.HeapID);
        int resourceID = template.Target + ResourceBase;
        SpriteSystem.SpriteSystem_LoadCharResObjFromOpenNarc(template.SpriteSys!, rotation.SpriteMan!, narc, tiles, 1, VRamMain, resourceID);
        SpriteSystem.SpriteSystem_LoadPaletteBufferFromOpenNarc(template.PaletteData!, PaletteBufferMainObj, template.SpriteSys!, rotation.SpriteMan!, narc, palette, 0, 1, VRamMain, resourceID);
        SpriteSystem.SpriteSystem_LoadCellResObjFromOpenNarc(template.SpriteSys!, rotation.SpriteMan!, narc, cell, 1, resourceID);
        SpriteSystem.SpriteSystem_LoadAnimResObjFromOpenNarc(template.SpriteSys!, rotation.SpriteMan!, narc, anim, 1, resourceID);
        NARC.NARC_dtor(narc);
    }

    // OFFSET 0x02237D8C
    private static void BallRotation_CreateSprite(BallRotation rotation)
    {
        SpriteTemplate template = new();
        short x = 0;
        short y = 0;
        BallRotation_GetStartPosition(rotation, ref x, ref y);
        template.X = x;
        template.Y = y;
        template.Z = 0;
        template.AnimIdx = 0;
        template.VramType = VRamMain;
        template.Priority = 0;
        template.PlttIdx = 0;
        template.BgPriority = rotation.Template.BgPrio;
        template.VramTransfer = 0;
        for (int i = 0; i < template.Resources.Length; i++)
        {
            template.Resources[i] = rotation.Template.Target + ResourceBase;
        }
        rotation.Sprite = SpriteSystem.SpriteSystem_NewSprite(rotation.Template.SpriteSys!, rotation.SpriteMan!, template);
        SpriteSystem.ManagedSprite_SetDrawFlag(rotation.Sprite!, 1);
        SpriteSystem.ManagedSprite_SetAffineOverwriteMode(rotation.Sprite!, AffineOverwriteDouble);
        SpriteSystem.ManagedSprite_SetAnimationFrame(rotation.Sprite!, 0);
        SpriteSystem.ManagedSprite_SetAnim(rotation.Sprite!, 0);
        SpriteSystem.ManagedSprite_TickFrame(rotation.Sprite!);
        BattleAnim_SetDefaultAlphaBlending();
    }

    // OFFSET 0x02237E0C
    public static void BallRotation_SetDrawFlag(BallRotation rotation, int draw)
    {
        SpriteSystem.ManagedSprite_SetDrawFlag(rotation.Sprite!, draw);
    }

    // OFFSET 0x02237E18
    public static void BallRotation_SetPositionXY(BallRotation rotation, short x, short y)
    {
        SpriteSystem.ManagedSprite_SetPositionXY(rotation.Sprite!, x, y);
    }

    // OFFSET 0x02237E24
    public static void BallRotation_SetAffineZRotation(BallRotation rotation, ushort angle)
    {
        SpriteSystem.ManagedSprite_SetAffineZRotation(rotation.Sprite!, angle);
    }

    // OFFSET 0x02237E30
    public static void BallRotation_SetTickAnimation(BallRotation rotation, int tick)
    {
        rotation.TickAnimation = tick;
    }

    // OFFSET 0x02237E34
    public static void BallRotation_SetPriority(BallRotation rotation, int priority)
    {
        SpriteSystem.ManagedSprite_SetPriority(rotation.Sprite!, (uint)priority);
    }

    // OFFSET 0x02237E40
    public static void BallRotation_SetExplicitPriority(BallRotation rotation, int priority)
    {
        SpriteSystem.ManagedSprite_SetExplicitPriority(rotation.Sprite!, priority);
    }

    // OFFSET 0x02237E4C
    public static void BallRotation_SetDelay(BallRotation rotation, int delay)
    {
        rotation.Delay = delay;
    }
}
