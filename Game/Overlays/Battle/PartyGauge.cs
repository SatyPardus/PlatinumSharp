namespace PlatinumSharp.Game;

public sealed class PartyGaugeAlpha
{
    public short Value;
}

public sealed class PartyGaugeCounter
{
    public sbyte Value;
}

public sealed class PartyGaugeArrow
{
    public ManagedSprite? Sprite;
    public SysTask? Task;
    public int HideMode;
    public int Side;
    public int Position;
    public int PosX;
    public readonly PartyGaugeAlpha Alpha = new();
    public byte State;
    public byte Delay;
}

public sealed class PartyGaugeBall
{
    public ManagedSprite? Sprite;
    public SysTask? Task;
    public int Side;
    public int Position;
    public PartyGaugeCounter? Counter;
    public PartyGaugeAlpha? Alpha;
    public int PosX;
    public int RestX;
    public int ArriveX;
    public short Delay;
    public ushort Sound;
    public byte State;
    public byte Slot;
    public byte FinalAnim;
    public byte Wait;
}

public sealed class PartyGauge
{
    public const int BallCount = 6;

    public readonly PartyGaugeArrow Arrow = new();
    public readonly PartyGaugeBall[] Balls = { new(), new(), new(), new(), new(), new() };
    public readonly PartyGaugeCounter Counter = new();
}

public static partial class PartyGauges
{
    public const int SidePlayer = 0;
    public const int SideEnemy = 1;
    public const int StatusNone = 0;
    public const int StatusHealthy = 1;
    public const int StatusFainted = 2;
    public const int StatusAilment = 3;

    private const int NarcBattleSubscreen = 0x1B;
    private const int HeapIDBattle = 5;
    private const int MemberPalette = 0x6E;
    private const int MemberTiles = 0x154;
    private const int MemberCell = 0x155;
    private const int MemberAnim = 0x156;
    private const int ResourcePalette = 0x4E45;
    private const int ResourceTiles = 0x4FBD;
    private const int ResourceCell = 0x4FB7;
    private const int ResourceAnim = 0x4FAC;
    private const int PaletteBufferMainObj = 2;
    private const int VRamMain = 1;
    private const int DisplayMain = 0;
    private const uint EnemyBallYTable = 0x02270A1C;
    private const uint EnemyArrowYTable = 0x02270A24;
    private const uint PlayerArrowYTable = 0x02270A2C;
    private const uint PlayerBallYTable = 0x02270A34;
    private const uint ArrowTemplate = 0x02270A3C;
    private const uint BallTemplate = 0x02270A70;
    private const uint ArrowTaskPriority = 500;
    private const uint BallTaskPriority = 501;
    private const int SeqArrowSlide = 0x711;
    private const int SeqBallArrive = 0x712;
    private const int SeqBallEmpty = 0x713;
    private const int AnimEmpty = 6;
    private const int SlideSpeed = 0x1200;
    private const int SettleSpeed = 0x600;
    private const int ArrowHideSpeed = 0x400;
    private const int BallHideSpeed = 0xC00;
    private const int StateAbort = 100;

    private static short TableY(uint table, int position)
    {
        return (short)Nitro.Bin.U16(RomData.Overlay(16, (uint)(table + position * 2), 2), 0);
    }

    // OFFSET 0x0226D094
    public static void PartyGauge_LoadGraphics(SpriteSystem spriteSys, SpriteManager spriteMan, PaletteData paletteData)
    {
        NARC narc = NARC.NARC_ctor(NarcBattleSubscreen, HeapIDBattle);
        SpriteSystem.SpriteSystem_LoadPaletteBufferFromOpenNarc(paletteData, PaletteBufferMainObj, spriteSys, spriteMan, narc, MemberPalette, 0, 1, VRamMain, ResourcePalette);
        SpriteSystem.SpriteSystem_LoadCharResObjFromOpenNarc(spriteSys, spriteMan, narc, MemberTiles, 1, VRamMain, ResourceTiles);
        SpriteSystem.SpriteSystem_LoadCellResObjFromOpenNarc(spriteSys, spriteMan, narc, MemberCell, 1, ResourceCell);
        SpriteSystem.SpriteSystem_LoadAnimResObjFromOpenNarc(spriteSys, spriteMan, narc, MemberAnim, 1, ResourceAnim);
        NARC.NARC_dtor(narc);
    }

    // OFFSET 0x0226D12C
    public static void PartyGauge_FreeGraphics(SpriteManager spriteMan)
    {
        SpriteSystem.SpriteManager_UnloadCharObjById(spriteMan, ResourceTiles);
        SpriteSystem.SpriteManager_UnloadPlttObjById(spriteMan, ResourcePalette);
        SpriteSystem.SpriteManager_UnloadCellObjById(spriteMan, ResourceCell);
        SpriteSystem.SpriteManager_UnloadAnimObjById(spriteMan, ResourceAnim);
    }

    // OFFSET 0x0226D160
    private static PartyGauge PartyGauge_Alloc()
    {
        return new PartyGauge();
    }

    // OFFSET 0x0226D17C
    private static void PartyGauge_FreeMemory(PartyGauge gauge)
    {
        if (gauge.Arrow.Task != null)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x0226D194
    public static PartyGauge PartyGauge_Show(byte[] status, int statusOffset, int side, int mode, int position, SpriteSystem spriteSys, SpriteManager spriteMan)
    {
        PartyGauge gauge = PartyGauge_Alloc();
        PartyGaugeArrow_Show(gauge.Arrow, side, position, spriteSys, spriteMan);
        for (int i = 0; i < PartyGauge.BallCount; i++)
        {
            int anim = PartyGauge_GetStatusAnim(status[statusOffset + i], side);
            PartyGaugeBall_Show(gauge.Balls[i], gauge.Counter, side, mode, position, i, anim, spriteSys, spriteMan);
        }
        return gauge;
    }

    // OFFSET 0x0226D1FC
    public static int PartyGauge_ShowIsDone(PartyGauge gauge)
    {
        if (gauge.Arrow.Task != null)
        {
            return 0;
        }
        int i;
        for (i = 0; i < PartyGauge.BallCount; i++)
        {
            if (gauge.Balls[i].Task != null)
            {
                break;
            }
        }
        return i == PartyGauge.BallCount ? 1 : 0;
    }

    // OFFSET 0x0226D220
    public static void PartyGauge_Hide(PartyGauge? gauge, int arrowMode, int ballMode)
    {
        if (gauge == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        PartyGaugeArrow_Hide(gauge!.Arrow, arrowMode);
        for (int i = 0; i < PartyGauge.BallCount; i++)
        {
            PartyGaugeBall_Hide(gauge.Balls[i], i, ballMode, gauge.Arrow.Alpha);
        }
    }

    // OFFSET 0x0226D258
    public static int PartyGauge_HideIsDone(PartyGauge gauge)
    {
        if (gauge.Arrow.Task != null)
        {
            return 0;
        }
        int i;
        for (i = 0; i < PartyGauge.BallCount; i++)
        {
            if (gauge.Balls[i].Task != null)
            {
                break;
            }
        }
        return i == PartyGauge.BallCount ? 1 : 0;
    }

    // OFFSET 0x0226D27C
    public static void PartyGauge_Free(PartyGauge gauge)
    {
        SpriteSystem.Sprite_DeleteAndFreeResources(gauge.Arrow.Sprite!);
        for (int i = 0; i < PartyGauge.BallCount; i++)
        {
            SpriteSystem.Sprite_DeleteAndFreeResources(gauge.Balls[i].Sprite!);
        }
        PartyGauge_FreeMemory(gauge);
    }

    // OFFSET 0x0226D2A0
    private static void PartyGaugeArrow_Show(PartyGaugeArrow arrow, int side, int position, SpriteSystem spriteSys, SpriteManager spriteMan)
    {
        if (!(arrow.Sprite == null && arrow.Task == null))
        {
            ErrorHandling.GF_AssertFail();
        }
        arrow.HideMode = 0;
        arrow.PosX = 0;
        arrow.Alpha.Value = 0;
        arrow.Delay = 0;
        arrow.Sprite = SpriteSystem.SpriteSystem_NewSprite(spriteSys, spriteMan, BattleSubscreens.RomSpriteTemplate(ArrowTemplate));
        if (side == SidePlayer)
        {
            SpriteSystem.ManagedSprite_SetPositionXY(arrow.Sprite!, 0x160, TableY(PlayerArrowYTable, position));
            Sprite.Sprite_SetAnim(arrow.Sprite!.Sprite!, 8);
        }
        else
        {
            SpriteSystem.ManagedSprite_SetPositionXY(arrow.Sprite!, -0x60, TableY(EnemyArrowYTable, position));
            Sprite.Sprite_SetAnim(arrow.Sprite!.Sprite!, 7);
        }
        SpriteSystem.Sprite_TickFrame(arrow.Sprite!.Sprite!);
        arrow.Side = side;
        arrow.Position = position;
        arrow.State = 0;
        arrow.Task = SysTask.SysTask_Start(PartyGaugeArrow_ShowTask, arrow, ArrowTaskPriority);
        SoundPlayback.Sound_PlayEffect(SeqArrowSlide);
    }

    // OFFSET 0x0226D34C
    private static void PartyGaugeArrow_ShowTask(SysTask task, object? param)
    {
        PartyGaugeArrow arrow = (PartyGaugeArrow)param!;
        switch (arrow.State)
        {
            case 0:
                SpriteSystem.ManagedSprite_GetPositionXY(arrow.Sprite!, out short x, out short y);
                arrow.PosX = x << 8;
                arrow.State++;
                goto case 1;
            case 1:
                if (arrow.Side == SidePlayer)
                {
                    arrow.PosX -= SlideSpeed;
                    if (arrow.PosX <= 0xE000)
                    {
                        arrow.PosX = 0xE000;
                        arrow.State++;
                    }
                    SpriteSystem.ManagedSprite_SetPositionXY(arrow.Sprite!, (short)(arrow.PosX >> 8), TableY(PlayerArrowYTable, arrow.Position));
                }
                else
                {
                    arrow.PosX += SlideSpeed;
                    if (arrow.PosX >= 0x2000)
                    {
                        arrow.PosX = 0x2000;
                        arrow.State++;
                    }
                    SpriteSystem.ManagedSprite_SetPositionXY(arrow.Sprite!, (short)(arrow.PosX >> 8), TableY(EnemyArrowYTable, arrow.Position));
                }
                break;
            default:
                SysTask.SysTask_Done(task);
                arrow.Task = null;
                break;
        }
    }

    // OFFSET 0x0226D3F8
    private static void PartyGaugeArrow_Hide(PartyGaugeArrow arrow, int hideMode)
    {
        if (arrow.Sprite == null || arrow.Task != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        arrow.State = 0;
        arrow.HideMode = hideMode;
        arrow.Delay = (byte)(hideMode == 0 ? 4 : 0);
        arrow.Task = SysTask.SysTask_Start(PartyGaugeArrow_HideTask, arrow, ArrowTaskPriority);
    }

    // OFFSET 0x0226D434
    private static void PartyGaugeArrow_HideTask(SysTask task, object? param)
    {
        PartyGaugeArrow arrow = (PartyGaugeArrow)param!;
        switch (arrow.State)
        {
            case 0:
                SpriteSystem.ManagedSprite_GetPositionXY(arrow.Sprite!, out short x, out short y);
                arrow.PosX = x << 8;
                SpriteSystem.ManagedSprite_SetExplicitOamMode(arrow.Sprite!, 1);
                arrow.Alpha.Value = 0x1000;
                Video.SetBlendAlpha(DisplayMain, 0, 0x3F, arrow.Alpha.Value >> 8, 16 - (arrow.Alpha.Value >> 8));
                arrow.State++;
                goto case 1;
            case 1:
                if (arrow.Delay != 0)
                {
                    arrow.Delay--;
                    break;
                }
                arrow.State++;
                goto case 2;
            case 2:
                if (arrow.HideMode == 0)
                {
                    if (arrow.Side == SidePlayer)
                    {
                        arrow.PosX -= ArrowHideSpeed;
                        SpriteSystem.ManagedSprite_SetPositionXY(arrow.Sprite!, (short)(arrow.PosX >> 8), TableY(PlayerArrowYTable, arrow.Position));
                    }
                    else
                    {
                        arrow.PosX += ArrowHideSpeed;
                        SpriteSystem.ManagedSprite_SetPositionXY(arrow.Sprite!, (short)(arrow.PosX >> 8), TableY(EnemyArrowYTable, arrow.Position));
                    }
                }
                arrow.Alpha.Value -= 0x100;
                if (arrow.Alpha.Value <= 0)
                {
                    arrow.Alpha.Value = 0;
                    SpriteSystem.Sprite_SetDrawFlag2(arrow.Sprite!.Sprite!, 0);
                    arrow.State++;
                }
                Video.SetBlendAlphaCoefficients(DisplayMain, (ushort)((arrow.Alpha.Value >> 8) | ((16 - (arrow.Alpha.Value >> 8)) << 8)));
                break;
            default:
                Battle.Battle_ClearBlendControl();
                SysTask.SysTask_Done(task);
                arrow.Task = null;
                break;
        }
    }

    // OFFSET 0x0226D540
    private static void PartyGaugeBall_Show(PartyGaugeBall ball, PartyGaugeCounter counter, int side, int mode, int position, int slot, int anim, SpriteSystem spriteSys, SpriteManager spriteMan)
    {
        if (!(ball.Sprite == null && ball.Task == null))
        {
            ErrorHandling.GF_AssertFail();
        }
        ball.Alpha = null;
        ball.PosX = 0;
        ball.Wait = 0;
        ball.Sprite = SpriteSystem.SpriteSystem_NewSprite(spriteSys, spriteMan, BattleSubscreens.RomSpriteTemplate(BallTemplate));
        if (side == SidePlayer)
        {
            SpriteSystem.ManagedSprite_SetPositionXY(ball.Sprite!, 0x114, TableY(PlayerBallYTable, position));
        }
        else
        {
            SpriteSystem.ManagedSprite_SetPositionXY(ball.Sprite!, -20, TableY(EnemyBallYTable, position));
        }
        Sprite.Sprite_SetAnim(ball.Sprite!.Sprite!, (uint)anim);
        SpriteSystem.Sprite_TickFrame(ball.Sprite!.Sprite!);
        ball.Side = side;
        ball.Slot = (byte)slot;
        ball.Position = position;
        ball.FinalAnim = (byte)PartyGauge_GetFinalAnim(anim);
        ball.Counter = counter;
        ball.Sound = (ushort)(anim == AnimEmpty ? SeqBallEmpty : SeqBallArrive);
        if (side == SidePlayer)
        {
            ball.RestX = (slot << 4) + 0xA2;
            ball.ArriveX = 15 * slot + 0x9C;
        }
        else
        {
            ball.RestX = 0x5E - (slot << 4);
            ball.ArriveX = 0x64 - 15 * slot;
        }
        ball.State = 0;
        if (mode == 0)
        {
            ball.Delay = (short)(slot * 3 + 5);
            ball.Task = SysTask.SysTask_Start(PartyGaugeBall_ShowTask, ball, BallTaskPriority);
        }
        else
        {
            ball.Delay = 0;
            ball.Task = SysTask.SysTask_Start(PartyGaugeBall_ShowQuickTask, ball, BallTaskPriority);
        }
    }

    // OFFSET 0x0226D654
    private static void PartyGaugeBall_ShowTask(SysTask task, object? param)
    {
        PartyGaugeBall ball = (PartyGaugeBall)param!;
        switch (ball.State)
        {
            case 0:
                SpriteSystem.ManagedSprite_GetPositionXY(ball.Sprite!, out short x, out short y);
                ball.PosX = x << 8;
                ball.State++;
                goto case 1;
            case 1:
                if (ball.Delay > 0)
                {
                    ball.Delay--;
                    break;
                }
                goto case 2;
            case 2:
                if (ball.Side == SidePlayer)
                {
                    ball.PosX -= SlideSpeed;
                    if (ball.PosX <= ball.ArriveX << 8)
                    {
                        ball.PosX = ball.ArriveX << 8;
                        SoundPlayback.Sound_PlayEffect(ball.Sound);
                        ball.State++;
                    }
                    SpriteSystem.ManagedSprite_SetPositionXY(ball.Sprite!, (short)(ball.PosX >> 8), TableY(PlayerBallYTable, ball.Position));
                }
                else
                {
                    ball.PosX += SlideSpeed;
                    if (ball.PosX >= ball.ArriveX << 8)
                    {
                        ball.PosX = ball.ArriveX << 8;
                        ball.State++;
                    }
                    SpriteSystem.ManagedSprite_SetPositionXY(ball.Sprite!, (short)(ball.PosX >> 8), TableY(EnemyBallYTable, ball.Position));
                }
                SpriteSystem.Sprite_TickFrame(ball.Sprite!.Sprite!);
                break;
            case 3:
                ball.Counter!.Value++;
                ball.State++;
                goto case 4;
            case 4:
                if (ball.Counter!.Value != PartyGauge.BallCount)
                {
                    SpriteSystem.Sprite_TickFrame(ball.Sprite!.Sprite!);
                    break;
                }
                Sprite.Sprite_SetAnimFrame(ball.Sprite!.Sprite!, 1);
                ball.Delay = 0;
                ball.State++;
                goto case 5;
            case 5:
                ball.Delay++;
                if (ball.Delay < 0)
                {
                    break;
                }
                Sprite.Sprite_SetAnim(ball.Sprite!.Sprite!, ball.FinalAnim);
                ball.Delay = 0;
                ball.State++;
                goto case 6;
            case 6:
                if (ball.Side == SidePlayer)
                {
                    ball.PosX += SettleSpeed;
                    if (ball.PosX >= ball.RestX << 8)
                    {
                        ball.PosX = ball.RestX << 8;
                        ball.State++;
                    }
                    SpriteSystem.ManagedSprite_SetPositionXY(ball.Sprite!, (short)(ball.PosX >> 8), TableY(PlayerBallYTable, ball.Position));
                }
                else
                {
                    ball.PosX -= SettleSpeed;
                    if (ball.PosX <= ball.RestX << 8)
                    {
                        ball.PosX = ball.RestX << 8;
                        ball.State++;
                    }
                    SpriteSystem.ManagedSprite_SetPositionXY(ball.Sprite!, (short)(ball.PosX >> 8), TableY(EnemyBallYTable, ball.Position));
                }
                SpriteSystem.Sprite_TickFrame(ball.Sprite!.Sprite!);
                break;
            default:
                Sprite.Sprite_SetAnimFrame(ball.Sprite!.Sprite!, 0);
                SysTask.SysTask_Done(task);
                ball.Task = null;
                break;
        }
    }

    // OFFSET 0x0226D854
    private static void PartyGaugeBall_ShowQuickTask(SysTask task, object? param)
    {
        PartyGaugeBall ball = (PartyGaugeBall)param!;
        switch (ball.State)
        {
            case 0:
                SpriteSystem.ManagedSprite_GetPositionXY(ball.Sprite!, out short x, out short y);
                ball.PosX = x << 8;
                Sprite.Sprite_SetAnimFrame(ball.Sprite!.Sprite!, 0);
                ball.State++;
                goto case 1;
            case 1:
                if (ball.Delay > 0)
                {
                    ball.Delay--;
                    break;
                }
                goto case 2;
            case 2:
                if (ball.Side == SidePlayer)
                {
                    ball.PosX -= SlideSpeed;
                    if (ball.PosX <= ball.RestX << 8)
                    {
                        ball.PosX = ball.RestX << 8;
                        ball.State++;
                    }
                    SpriteSystem.ManagedSprite_SetPositionXY(ball.Sprite!, (short)(ball.PosX >> 8), TableY(PlayerBallYTable, ball.Position));
                }
                else
                {
                    ball.PosX += SlideSpeed;
                    if (ball.PosX >= ball.RestX << 8)
                    {
                        ball.PosX = ball.RestX << 8;
                        ball.State++;
                    }
                    SpriteSystem.ManagedSprite_SetPositionXY(ball.Sprite!, (short)(ball.PosX >> 8), TableY(EnemyBallYTable, ball.Position));
                }
                break;
            default:
                SysTask.SysTask_Done(task);
                ball.Task = null;
                break;
        }
    }

    // OFFSET 0x0226D938
    private static void PartyGaugeBall_Hide(PartyGaugeBall ball, int slot, int mode, PartyGaugeAlpha alpha)
    {
        if (ball.Sprite == null || ball.Task != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        ball.State = 0;
        ball.Alpha = alpha;
        if (mode == 0)
        {
            ball.Delay = (short)(slot * 3);
            ball.Wait = 4;
            ball.Task = SysTask.SysTask_Start(PartyGaugeBall_HideTask, ball, BallTaskPriority);
        }
        else
        {
            ball.Delay = 0;
            ball.Wait = 0;
            ball.Task = SysTask.SysTask_Start(PartyGaugeBall_HideQuickTask, ball, BallTaskPriority);
        }
    }

    // OFFSET 0x0226D99C
    private static void PartyGaugeBall_HideTask(SysTask task, object? param)
    {
        PartyGaugeBall ball = (PartyGaugeBall)param!;
        if (ball.Alpha!.Value == 0)
        {
            ball.State = StateAbort;
        }
        switch (ball.State)
        {
            case 0:
                SpriteSystem.ManagedSprite_GetPositionXY(ball.Sprite!, out short x, out short y);
                ball.PosX = x << 8;
                SpriteSystem.ManagedSprite_SetExplicitOamMode(ball.Sprite!, 1);
                ball.State++;
                goto case 1;
            case 1:
                if (ball.Wait != 0)
                {
                    ball.Wait--;
                    break;
                }
                if (ball.Delay > 0)
                {
                    ball.Delay--;
                    break;
                }
                goto case 2;
            case 2:
                if (ball.Side == SidePlayer)
                {
                    ball.PosX -= BallHideSpeed;
                    SpriteSystem.ManagedSprite_SetPositionXY(ball.Sprite!, (short)(ball.PosX >> 8), TableY(PlayerBallYTable, ball.Position));
                }
                else
                {
                    ball.PosX += BallHideSpeed;
                    SpriteSystem.ManagedSprite_SetPositionXY(ball.Sprite!, (short)(ball.PosX >> 8), TableY(EnemyBallYTable, ball.Position));
                }
                if (ball.PosX < -0x1000 || ball.PosX > 0x11000)
                {
                    ball.State++;
                }
                SpriteSystem.Sprite_TickFrame(ball.Sprite!.Sprite!);
                break;
            default:
                SpriteSystem.Sprite_SetDrawFlag2(ball.Sprite!.Sprite!, 0);
                SysTask.SysTask_Done(task);
                ball.Task = null;
                break;
        }
    }

    // OFFSET 0x0226DAAC
    private static void PartyGaugeBall_HideQuickTask(SysTask task, object? param)
    {
        PartyGaugeBall ball = (PartyGaugeBall)param!;
        if (ball.Alpha!.Value == 0)
        {
            ball.State = StateAbort;
        }
        switch (ball.State)
        {
            case 0:
                SpriteSystem.ManagedSprite_SetExplicitOamMode(ball.Sprite!, 1);
                ball.State++;
                break;
            case 1:
                break;
            default:
                SpriteSystem.Sprite_SetDrawFlag2(ball.Sprite!.Sprite!, 0);
                SysTask.SysTask_Done(task);
                ball.Task = null;
                break;
        }
    }

    // OFFSET 0x0226DB04
    private static int PartyGauge_GetStatusAnim(int status, int side)
    {
        switch (status)
        {
            case StatusNone:
                return AnimEmpty;
            case StatusHealthy:
                return side == SidePlayer ? 3 : 0;
            case StatusFainted:
                return side == SidePlayer ? 5 : 2;
            case StatusAilment:
                return side == SidePlayer ? 4 : 1;
        }
        return AnimEmpty;
    }

    // OFFSET 0x0226DB44
    private static int PartyGauge_GetFinalAnim(int anim)
    {
        switch (anim)
        {
            case 0:
                return 3;
            case 1:
                return 4;
            case 2:
                return 5;
            case 3:
                return 0;
            case 4:
                return 1;
            case 5:
                return 2;
        }
        return anim;
    }
}

public static partial class Battle
{
    // OFFSET 0x0223F9F0
    public static void Battle_ClearBlendControl()
    {
        Video.SetBlendControl(0, 0);
    }
}
