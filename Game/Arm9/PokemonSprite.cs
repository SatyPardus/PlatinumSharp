using System.Buffers.Binary;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public struct SpriteAnimFrame
{
    public sbyte SpriteFrame;
    public byte FrameDelay;
    public sbyte XOffset;
    public sbyte Padding;
}

public delegate void PokemonSpriteCallback(PokemonSprite sprite);

public sealed class PokemonSpriteTemplate
{
    public uint Word0;
    public uint Word1;
    public uint Word2;
    public uint Word3;

    public void CopyFrom(PokemonSpriteTemplate other)
    {
        Word0 = other.Word0;
        Word1 = other.Word1;
        Word2 = other.Word2;
        Word3 = other.Word3;
    }

    public void Clear()
    {
        Word0 = 0;
        Word1 = 0;
        Word2 = 0;
        Word3 = 0;
    }
}

public sealed class PokemonSprite
{
    public const int AnimFrameCount = 10;

    public bool Active;
    public int PolygonID;
    public bool NeedReloadChar;
    public bool NeedReloadPltt;
    public readonly PokemonSpriteTemplate Template = new();
    public readonly PokemonSpriteTemplate TemplateBackup = new();
    public byte FadeInitAlpha;
    public byte FadeTargetAlpha;
    public byte FadeDelayCounter;
    public byte FadeDelayLength;
    public int FadeTargetColor;
    public bool FadeActive;
    public bool FlipH;
    public bool FlipV;
    public int Mosaic;
    public short X;
    public short Y;
    public int Z;
    public short XOffset;
    public short YOffset;
    public int ZOffset;
    public short ScaleX;
    public short ScaleY;
    public ushort RotationX;
    public ushort RotationY;
    public ushort RotationZ;
    public short PivotX;
    public short PivotY;
    public byte DrawXOffset;
    public byte DrawYOffset;
    public byte DrawWidth;
    public byte DrawHeight;
    public int DiffuseR;
    public int DiffuseG;
    public int DiffuseB;
    public int AmbientR;
    public int AmbientG;
    public int AmbientB;
    public bool Hide;
    public bool PartialDraw;
    public int Alpha;
    public bool ForceHide;
    public byte AnimActive;
    public byte CurrAnimFrame;
    public byte FrameDelay;
    public byte CurrSpriteFrame;
    public readonly byte[] LoopTimers = new byte[AnimFrameCount];
    public PokemonSpriteCallback? Callback;
    public int ShadowPaletteSlot;
    public bool ShadowFollowX;
    public bool ShadowFollowY;
    public bool ShadowIsAffine;
    public int ShadowSize;
    public sbyte ShadowHeight;
    public short ShadowX;
    public short ShadowY;
    public short ShadowXOffset;
    public short ShadowYOffset;
    public readonly SpriteAnimFrame[] AnimFrames = new SpriteAnimFrame[AnimFrameCount];

    public void Clear()
    {
        Active = false;
        PolygonID = 0;
        NeedReloadChar = false;
        NeedReloadPltt = false;
        Template.Clear();
        TemplateBackup.Clear();
        FadeInitAlpha = 0;
        FadeTargetAlpha = 0;
        FadeDelayCounter = 0;
        FadeDelayLength = 0;
        FadeTargetColor = 0;
        FadeActive = false;
        FlipH = false;
        FlipV = false;
        Mosaic = 0;
        X = 0;
        Y = 0;
        Z = 0;
        XOffset = 0;
        YOffset = 0;
        ZOffset = 0;
        ScaleX = 0;
        ScaleY = 0;
        RotationX = 0;
        RotationY = 0;
        RotationZ = 0;
        PivotX = 0;
        PivotY = 0;
        DrawXOffset = 0;
        DrawYOffset = 0;
        DrawWidth = 0;
        DrawHeight = 0;
        DiffuseR = 0;
        DiffuseG = 0;
        DiffuseB = 0;
        AmbientR = 0;
        AmbientG = 0;
        AmbientB = 0;
        Hide = false;
        PartialDraw = false;
        Alpha = 0;
        ForceHide = false;
        AnimActive = 0;
        CurrAnimFrame = 0;
        FrameDelay = 0;
        CurrSpriteFrame = 0;
        Array.Clear(LoopTimers);
        Callback = null;
        ShadowPaletteSlot = 0;
        ShadowFollowX = false;
        ShadowFollowY = false;
        ShadowIsAffine = false;
        ShadowSize = 0;
        ShadowHeight = 0;
        ShadowX = 0;
        ShadowY = 0;
        ShadowXOffset = 0;
        ShadowYOffset = 0;
        Array.Clear(AnimFrames);
        ShadowBackup = default;
    }

    // OFFSET 0x02007B98
    public static void PokemonSprite_InitAnim(PokemonSprite monSprite, int dummy)
    {
        monSprite.CurrAnimFrame = 0;
        for (int i = 0; i < AnimFrameCount; i++)
        {
            monSprite.LoopTimers[i] = 0;
        }
        SpriteAnimFrame frame = monSprite.AnimFrames[monSprite.CurrAnimFrame];
        if (frame.SpriteFrame == -1)
        {
            monSprite.CurrSpriteFrame = 0;
            return;
        }
        monSprite.AnimActive = 1;
        monSprite.CurrSpriteFrame = (byte)frame.SpriteFrame;
        monSprite.FrameDelay = frame.FrameDelay;
        monSprite.XOffset = frame.XOffset;
    }

    // OFFSET 0x02007C10
    public static void PokemonSprite_SetAnimFrames(PokemonSprite monSprite, SpriteAnimFrame[] animFrames)
    {
        Array.Copy(animFrames, monSprite.AnimFrames, AnimFrameCount);
    }

    // OFFSET 0x02007C24
    public static bool PokemonSprite_IsAnimActive(PokemonSprite monSprite)
    {
        return monSprite.AnimActive != 0;
    }

    // OFFSET 0x02007DC8
    public static void PokemonSprite_Delete(PokemonSprite monSprite)
    {
        monSprite.Active = false;
    }

    // OFFSET 0x02007DEC
    public static void PokemonSprite_SetAttribute(PokemonSprite monSprite, int attribute, int value)
    {
        switch (attribute)
        {
            case 0:
                monSprite.X = (short)value;
                break;
            case 1:
                monSprite.Y = (short)value;
                break;
            case 2:
                monSprite.Z = value;
                break;
            case 3:
                monSprite.XOffset = (short)value;
                break;
            case 4:
                monSprite.YOffset = (short)value;
                break;
            case 5:
                monSprite.ZOffset = value;
                break;
            case 6:
                monSprite.Hide = (value & 1) != 0;
                break;
            case 7:
                monSprite.RotationX = (ushort)value;
                break;
            case 8:
                monSprite.RotationY = (ushort)value;
                break;
            case 9:
                monSprite.RotationZ = (ushort)value;
                break;
            case 10:
                monSprite.PivotX = (short)value;
                break;
            case 11:
                monSprite.PivotY = (short)value;
                break;
            case 12:
                monSprite.ScaleX = (short)value;
                break;
            case 13:
                monSprite.ScaleY = (short)value;
                break;
            case 14:
                monSprite.PartialDraw = (value & 1) != 0;
                break;
            case 15:
                monSprite.DrawXOffset = (byte)value;
                break;
            case 16:
                monSprite.DrawYOffset = (byte)value;
                break;
            case 17:
                monSprite.DrawWidth = (byte)value;
                break;
            case 18:
                monSprite.DrawHeight = (byte)value;
                break;
            case 19:
                monSprite.ShadowX = (short)value;
                break;
            case 20:
                monSprite.ShadowY = (short)value;
                break;
            case 21:
                monSprite.ShadowXOffset = (short)value;
                break;
            case 22:
                monSprite.ShadowYOffset = (short)value;
                break;
            case 23:
                monSprite.Alpha = value & 0x1F;
                break;
            case 24:
                monSprite.DiffuseR = value & 0x1F;
                break;
            case 25:
                monSprite.DiffuseG = value & 0x1F;
                break;
            case 26:
                monSprite.DiffuseB = value & 0x1F;
                break;
            case 27:
                monSprite.AmbientR = value & 0x1F;
                break;
            case 28:
                monSprite.AmbientG = value & 0x1F;
                break;
            case 29:
                monSprite.AmbientB = value & 0x1F;
                break;
            case 30:
                monSprite.FadeActive = (value & 1) != 0;
                monSprite.NeedReloadPltt = true;
                break;
            case 31:
                monSprite.FadeTargetColor = value;
                monSprite.NeedReloadPltt = true;
                break;
            case 32:
                monSprite.FadeInitAlpha = (byte)value;
                monSprite.NeedReloadPltt = true;
                break;
            case 33:
                monSprite.FadeTargetAlpha = (byte)value;
                monSprite.NeedReloadPltt = true;
                break;
            case 34:
                monSprite.FadeDelayCounter = (byte)value;
                break;
            case 35:
                monSprite.FlipH = (value & 1) != 0;
                monSprite.NeedReloadChar = true;
                break;
            case 36:
                monSprite.FlipV = (value & 1) != 0;
                monSprite.NeedReloadChar = true;
                break;
            case 37:
                monSprite.ForceHide = (value & 1) != 0;
                break;
            case 38:
                monSprite.CurrSpriteFrame = (byte)value;
                break;
            case 40:
                monSprite.Mosaic = value & 0xF;
                monSprite.NeedReloadChar = true;
                break;
            case 41:
                monSprite.ShadowHeight = (sbyte)value;
                break;
            case 42:
                monSprite.ShadowPaletteSlot = value & 3;
                monSprite.NeedReloadPltt = true;
                break;
            case 43:
                monSprite.ShadowFollowX = (value & 1) != 0;
                break;
            case 44:
                monSprite.ShadowFollowY = (value & 1) != 0;
                break;
            case 45:
                monSprite.ShadowIsAffine = (value & 1) != 0;
                break;
            case 46:
                monSprite.ShadowSize = value & 3;
                break;
        }
    }

    // OFFSET 0x020080C0
    public static int PokemonSprite_GetAttribute(PokemonSprite monSprite, int attribute)
    {
        switch (attribute)
        {
            case 0:
                return monSprite.X;
            case 1:
                return monSprite.Y;
            case 2:
                return monSprite.Z;
            case 3:
                return monSprite.XOffset;
            case 4:
                return monSprite.YOffset;
            case 5:
                return monSprite.ZOffset;
            case 6:
                return monSprite.Hide ? 1 : 0;
            case 7:
                return monSprite.RotationX;
            case 8:
                return monSprite.RotationY;
            case 9:
                return monSprite.RotationZ;
            case 10:
                return monSprite.PivotX;
            case 11:
                return monSprite.PivotY;
            case 12:
                return monSprite.ScaleX;
            case 13:
                return monSprite.ScaleY;
            case 14:
                return monSprite.PartialDraw ? 1 : 0;
            case 15:
                return monSprite.DrawXOffset;
            case 16:
                return monSprite.DrawYOffset;
            case 17:
                return monSprite.DrawWidth;
            case 18:
                return monSprite.DrawHeight;
            case 19:
                return monSprite.ShadowX;
            case 20:
                return monSprite.ShadowY;
            case 21:
                return monSprite.ShadowXOffset;
            case 22:
                return monSprite.ShadowYOffset;
            case 23:
                return monSprite.Alpha;
            case 24:
                return monSprite.DiffuseR;
            case 25:
                return monSprite.DiffuseG;
            case 26:
                return monSprite.DiffuseB;
            case 27:
                return monSprite.AmbientR;
            case 28:
                return monSprite.AmbientG;
            case 29:
                return monSprite.AmbientB;
            case 30:
                return monSprite.FadeActive ? 1 : 0;
            case 31:
                return monSprite.FadeTargetColor;
            case 32:
                return monSprite.FadeInitAlpha;
            case 33:
                return monSprite.FadeTargetAlpha;
            case 34:
                return monSprite.FadeDelayCounter;
            case 35:
                return monSprite.FlipH ? 1 : 0;
            case 36:
                return monSprite.FlipV ? 1 : 0;
            case 37:
                return monSprite.ForceHide ? 1 : 0;
            case 38:
                return monSprite.CurrSpriteFrame;
            case 40:
                return monSprite.Mosaic;
            case 41:
                return monSprite.ShadowHeight;
            case 42:
                return monSprite.ShadowPaletteSlot;
            case 43:
                return monSprite.ShadowFollowX ? 1 : 0;
            case 44:
                return monSprite.ShadowFollowY ? 1 : 0;
            case 45:
                return monSprite.ShadowIsAffine ? 1 : 0;
            case 46:
                return monSprite.ShadowSize;
            default:
                ErrorHandling.GF_AssertFail();
                return 0;
        }
    }

    // OFFSET 0x02008274
    public static void PokemonSprite_AddAttribute(PokemonSprite monSprite, int attribute, int delta)
    {
        switch (attribute)
        {
            case 0:
                monSprite.X = (short)(monSprite.X + delta);
                break;
            case 1:
                monSprite.Y = (short)(monSprite.Y + delta);
                break;
            case 2:
                monSprite.Z += delta;
                break;
            case 3:
                monSprite.XOffset = (short)(monSprite.XOffset + delta);
                break;
            case 4:
                monSprite.YOffset = (short)(monSprite.YOffset + delta);
                break;
            case 5:
                monSprite.ZOffset += delta;
                break;
            case 6:
                monSprite.Hide = (((monSprite.Hide ? 1 : 0) + delta) & 1) != 0;
                break;
            case 7:
                monSprite.RotationX = (ushort)(monSprite.RotationX + delta);
                break;
            case 8:
                monSprite.RotationY = (ushort)(monSprite.RotationY + delta);
                break;
            case 9:
                monSprite.RotationZ = (ushort)(monSprite.RotationZ + delta);
                break;
            case 10:
                monSprite.PivotX = (short)(monSprite.PivotX + delta);
                break;
            case 11:
                monSprite.PivotY = (short)(monSprite.PivotY + delta);
                break;
            case 12:
                monSprite.ScaleX = (short)(monSprite.ScaleX + delta);
                break;
            case 13:
                monSprite.ScaleY = (short)(monSprite.ScaleY + delta);
                break;
            case 14:
                monSprite.PartialDraw = (((monSprite.PartialDraw ? 1 : 0) + delta) & 1) != 0;
                break;
            case 15:
                monSprite.DrawXOffset = (byte)(monSprite.DrawXOffset + delta);
                break;
            case 16:
                monSprite.DrawYOffset = (byte)(monSprite.DrawYOffset + delta);
                break;
            case 17:
                monSprite.DrawWidth = (byte)(monSprite.DrawWidth + delta);
                break;
            case 18:
                monSprite.DrawHeight = (byte)(monSprite.DrawHeight + delta);
                break;
            case 19:
                monSprite.ShadowX = (short)(monSprite.ShadowX + delta);
                break;
            case 20:
                monSprite.ShadowY = (short)(monSprite.ShadowY + delta);
                break;
            case 21:
                monSprite.ShadowXOffset = (short)(monSprite.ShadowXOffset + delta);
                break;
            case 22:
                monSprite.ShadowYOffset = (short)(monSprite.ShadowYOffset + delta);
                break;
            case 23:
                monSprite.Alpha = (monSprite.Alpha + delta) & 0x1F;
                break;
            case 24:
                monSprite.DiffuseR = (monSprite.DiffuseR + delta) & 0x1F;
                break;
            case 25:
                monSprite.DiffuseG = (monSprite.DiffuseG + delta) & 0x1F;
                break;
            case 26:
                monSprite.DiffuseB = (monSprite.DiffuseB + delta) & 0x1F;
                break;
            case 27:
                monSprite.AmbientR = (monSprite.AmbientR + delta) & 0x1F;
                break;
            case 28:
                monSprite.AmbientG = (monSprite.AmbientG + delta) & 0x1F;
                break;
            case 29:
                monSprite.AmbientB = (monSprite.AmbientB + delta) & 0x1F;
                break;
            case 30:
                monSprite.FadeActive = (((monSprite.FadeActive ? 1 : 0) + delta) & 1) != 0;
                monSprite.NeedReloadPltt = true;
                break;
            case 31:
                monSprite.FadeTargetColor += delta;
                monSprite.NeedReloadPltt = true;
                break;
            case 32:
                monSprite.FadeInitAlpha = (byte)(monSprite.FadeInitAlpha + delta);
                monSprite.NeedReloadPltt = true;
                break;
            case 33:
                monSprite.FadeTargetAlpha = (byte)(monSprite.FadeTargetAlpha + delta);
                monSprite.NeedReloadPltt = true;
                break;
            case 34:
                monSprite.FadeDelayCounter = (byte)(monSprite.FadeDelayCounter + delta);
                break;
            case 35:
                monSprite.FlipH = (((monSprite.FlipH ? 1 : 0) + delta) & 1) != 0;
                monSprite.NeedReloadChar = true;
                break;
            case 36:
                monSprite.FlipV = (((monSprite.FlipV ? 1 : 0) + delta) & 1) != 0;
                monSprite.NeedReloadChar = true;
                break;
            case 37:
                monSprite.ForceHide = (((monSprite.ForceHide ? 1 : 0) + delta) & 1) != 0;
                break;
            case 38:
                monSprite.CurrSpriteFrame = (byte)(monSprite.CurrSpriteFrame + delta);
                break;
            case 40:
                monSprite.Mosaic = (monSprite.Mosaic + delta) & 0xF;
                monSprite.NeedReloadChar = true;
                break;
            case 41:
                monSprite.ShadowHeight = (sbyte)(monSprite.ShadowHeight + delta);
                break;
            case 42:
                monSprite.ShadowPaletteSlot = (monSprite.ShadowPaletteSlot + delta) & 3;
                monSprite.NeedReloadPltt = true;
                break;
            case 43:
                monSprite.ShadowFollowX = (((monSprite.ShadowFollowX ? 1 : 0) + delta) & 1) != 0;
                break;
            case 44:
                monSprite.ShadowFollowY = (((monSprite.ShadowFollowY ? 1 : 0) + delta) & 1) != 0;
                break;
            case 45:
                monSprite.ShadowIsAffine = (((monSprite.ShadowIsAffine ? 1 : 0) + delta) & 1) != 0;
                break;
            case 46:
                monSprite.ShadowSize = (monSprite.ShadowSize + delta) & 3;
                break;
        }
    }

    // OFFSET 0x020086D4
    public static void PokemonSprite_SetPartialDraw(PokemonSprite monSprite, int xOffset, int yOffset, int width, int height)
    {
        monSprite.PartialDraw = true;
        monSprite.DrawXOffset = (byte)xOffset;
        monSprite.DrawYOffset = (byte)yOffset;
        monSprite.DrawWidth = (byte)width;
        monSprite.DrawHeight = (byte)height;
    }

    public struct ShadowState
    {
        public int PaletteSlot;
        public bool FollowX;
        public bool FollowY;
        public bool IsAffine;
        public int Size;
        public sbyte Height;
        public short X;
        public short Y;
        public short XOffset;
        public short YOffset;
    }

    public ShadowState ShadowBackup;

    // OFFSET 0x020086FC
    public static void PokemonSprite_StartFade(PokemonSprite monSprite, int initAlpha, int targetAlpha, int delay, int color)
    {
        monSprite.FadeActive = true;
        monSprite.FadeInitAlpha = (byte)initAlpha;
        monSprite.FadeTargetAlpha = (byte)targetAlpha;
        monSprite.FadeDelayCounter = 0;
        monSprite.FadeDelayLength = (byte)delay;
        monSprite.FadeTargetColor = color;
    }

    // OFFSET 0x02008780
    public static void PokemonSprite_ClearFade(PokemonSprite monSprite)
    {
        monSprite.FadeActive = false;
        monSprite.FadeInitAlpha = 0;
        monSprite.FadeTargetAlpha = 0;
        monSprite.FadeDelayCounter = 0;
        monSprite.FadeDelayLength = 0;
        monSprite.FadeTargetColor = 0;
        monSprite.NeedReloadPltt = true;
    }

    // OFFSET 0x020087B4
    public static bool PokemonSprite_IsFadeActive(PokemonSprite monSprite)
    {
        return monSprite.FadeActive;
    }

    // OFFSET 0x020087C8
    public static void PokemonSprite_CalcScaledYOffset(PokemonSprite monSprite, int height)
    {
        int distance = 40 - height;
        monSprite.YOffset = (short)(distance - ((distance * monSprite.ScaleY) >> 8));
    }

    // OFFSET 0x020087DC
    private static void TickPokemonSpriteAnim(PokemonSprite monSprite)
    {
        if (monSprite.AnimActive == 0)
        {
            return;
        }
        if (monSprite.FrameDelay != 0)
        {
            monSprite.FrameDelay--;
            return;
        }
        monSprite.CurrAnimFrame++;
        while (monSprite.AnimFrames[monSprite.CurrAnimFrame].SpriteFrame < -1)
        {
            byte current = monSprite.CurrAnimFrame;
            monSprite.LoopTimers[current]++;
            SpriteAnimFrame command = monSprite.AnimFrames[current];
            if (command.FrameDelay == monSprite.LoopTimers[current] || command.FrameDelay == 0)
            {
                monSprite.LoopTimers[current] = 0;
                monSprite.CurrAnimFrame++;
            }
            else
            {
                monSprite.CurrAnimFrame = (byte)(-2 - command.SpriteFrame);
            }
        }
        if (monSprite.CurrAnimFrame >= AnimFrameCount || monSprite.AnimFrames[monSprite.CurrAnimFrame].SpriteFrame == -1)
        {
            monSprite.CurrSpriteFrame = 0;
            monSprite.AnimActive = 0;
            monSprite.XOffset = 0;
            return;
        }
        SpriteAnimFrame frame = monSprite.AnimFrames[monSprite.CurrAnimFrame];
        monSprite.CurrSpriteFrame = (byte)frame.SpriteFrame;
        monSprite.FrameDelay = frame.FrameDelay;
        monSprite.XOffset = frame.XOffset;
    }

    // OFFSET 0x020088D8
    internal static void PokemonSprite_TickAnim(PokemonSprite monSprite)
    {
        TickPokemonSpriteAnim(monSprite);
    }

    // OFFSET 0x020089A0
    public static void PokemonSprite_ScheduleReloadFromNARC(PokemonSprite monSprite)
    {
        monSprite.NeedReloadChar = true;
        monSprite.NeedReloadPltt = true;
    }

    // OFFSET 0x020089B0
    public static void PokemonSprite_Push(PokemonSprite monSprite)
    {
        monSprite.TemplateBackup.CopyFrom(monSprite.Template);
        monSprite.ShadowBackup = new ShadowState
        {
            PaletteSlot = monSprite.ShadowPaletteSlot,
            FollowX = monSprite.ShadowFollowX,
            FollowY = monSprite.ShadowFollowY,
            IsAffine = monSprite.ShadowIsAffine,
            Size = monSprite.ShadowSize,
            Height = monSprite.ShadowHeight,
            X = monSprite.ShadowX,
            Y = monSprite.ShadowY,
            XOffset = monSprite.ShadowXOffset,
            YOffset = monSprite.ShadowYOffset,
        };
    }

    // OFFSET 0x02008A0C
    public static void PokemonSprite_Pop(PokemonSprite monSprite)
    {
        monSprite.Template.CopyFrom(monSprite.TemplateBackup);
        ShadowState backup = monSprite.ShadowBackup;
        monSprite.ShadowPaletteSlot = backup.PaletteSlot;
        monSprite.ShadowFollowX = backup.FollowX;
        monSprite.ShadowFollowY = backup.FollowY;
        monSprite.ShadowIsAffine = backup.IsAffine;
        monSprite.ShadowSize = backup.Size;
        monSprite.ShadowHeight = backup.Height;
        monSprite.ShadowX = backup.X;
        monSprite.ShadowY = backup.Y;
        monSprite.ShadowXOffset = backup.XOffset;
        monSprite.ShadowYOffset = backup.YOffset;
        monSprite.NeedReloadChar = true;
        monSprite.NeedReloadPltt = true;
    }

    // OFFSET 0x02008A90
    public static PokemonSpriteTemplate PokemonSprite_GetTemplate(PokemonSprite monSprite)
    {
        return monSprite.Template;
    }

    // OFFSET 0x02008B38
    public static bool PokemonSprite_IsActive(PokemonSprite? monSprite)
    {
        if (monSprite == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return monSprite!.Active;
    }

    private const int FrameRowBytes = 0x50;
    private const int SpindaSpotLists = 4;
    private const int EncryptedWords = 0xC80;

    private static byte[][]? sSpindaSpots;

    private static byte[][] LoadSpindaSpots()
    {
        uint[] addresses = RomData.Arm9U32(0x021007A4, SpindaSpotLists);
        byte[][] lists = new byte[SpindaSpotLists][];
        for (int i = 0; i < SpindaSpotLists; i++)
        {
            List<byte> list = new();
            uint address = addresses[i];
            while (true)
            {
                byte[] pair = RomData.Arm9(address, 2);
                list.Add(pair[0]);
                if (pair[0] == 0xFF)
                {
                    break;
                }
                list.Add(pair[1]);
                address += 2;
            }
            lists[i] = list.ToArray();
        }
        return lists;
    }

    private static void RecolorSpindaPixel(byte[] rawCharData, byte x, byte y)
    {
        int index = (x >> 1) + y * FrameRowBytes;
        byte value = rawCharData[index];
        if ((x & 1) != 0)
        {
            int high = value & 0xF0;
            if (high >= 0x10 && high <= 0x30)
            {
                rawCharData[index] = (byte)(value + 0x50);
            }
        }
        else
        {
            int low = value & 0xF;
            if (low >= 1 && low <= 3)
            {
                rawCharData[index] = (byte)(value + 5);
            }
        }
    }

    // OFFSET 0x020091AC
    internal static byte SwapNybbles(byte value)
    {
        return (byte)(((value & 0xF0) >> 4) | ((value << 4) & 0xF0));
    }

    // OFFSET 0x020091C0
    internal static void TryDrawSpindaSpots(PokemonSprite monSprite, byte[] rawCharData)
    {
        if ((monSprite.Template.Word1 >> 16) != 0)
        {
            PokemonSprite_DrawSpindaSpots(rawCharData, monSprite.Template.Word3, true);
        }
    }

    // OFFSET 0x020091D8
    public static void PokemonSprite_DrawSpindaSpots(byte[] rawCharData, uint personality, bool isAnimated)
    {
        sSpindaSpots ??= LoadSpindaSpots();
        uint value = personality;
        for (int i = 0; i < SpindaSpotLists; i++)
        {
            byte[] spots = sSpindaSpots[i];
            int baseX = (int)(value & 0xF) - 8;
            int baseY = (int)((value & 0xF0) >> 4) - 8;
            for (byte j = 0; spots[j * 2] != 0xFF; j++)
            {
                RecolorSpindaPixel(rawCharData, (byte)(baseX + spots[j * 2]), (byte)(baseY + spots[j * 2 + 1]));
            }
            value >>= 8;
        }
        if (!isAnimated)
        {
            return;
        }
        value = personality;
        for (int i = 0; i < SpindaSpotLists; i++)
        {
            byte[] spots = sSpindaSpots[i];
            int baseX = (int)(value & 0xF) + 0x48;
            int baseY = (int)((value & 0xF0) >> 4) - 8;
            for (byte j = 0; spots[j * 2] != 0xFF; j++)
            {
                RecolorSpindaPixel(rawCharData, (byte)(baseX + spots[j * 2] - 0xE), (byte)(baseY + spots[j * 2 + 1]));
            }
            value >>= 8;
        }
    }

    // OFFSET 0x02009328
    private static ushort PokemonSprite_LCRNGNext(ref uint seed)
    {
        seed = seed * 0x41C64E6D + 0x6073;
        return (ushort)(seed >> 16);
    }

    // OFFSET 0x02009348
    public static void PokemonSprite_DecryptPt(byte[] rawCharData)
    {
        uint seed = BinaryPrimitives.ReadUInt16LittleEndian(rawCharData);
        for (int i = 0; i < EncryptedWords; i++)
        {
            Span<byte> word = rawCharData.AsSpan(i * 2, 2);
            BinaryPrimitives.WriteUInt16LittleEndian(word, (ushort)(BinaryPrimitives.ReadUInt16LittleEndian(word) ^ seed));
            PokemonSprite_LCRNGNext(ref seed);
        }
    }

    // OFFSET 0x02009370
    public static void PokemonSprite_DecryptDP(byte[] rawCharData)
    {
        uint seed = BinaryPrimitives.ReadUInt16LittleEndian(rawCharData.AsSpan((EncryptedWords - 1) * 2, 2));
        for (int i = EncryptedWords - 1; i > -1; i--)
        {
            Span<byte> word = rawCharData.AsSpan(i * 2, 2);
            BinaryPrimitives.WriteUInt16LittleEndian(word, (ushort)(BinaryPrimitives.ReadUInt16LittleEndian(word) ^ seed));
            PokemonSprite_LCRNGNext(ref seed);
        }
    }

    // OFFSET 0x020093A0
    public static void PokemonSprite_Decrypt(byte[] rawCharData, int narcID)
    {
        if ((uint)(narcID - 0xA5) <= 1)
        {
            PokemonSprite_DecryptDP(rawCharData);
        }
        else
        {
            PokemonSprite_DecryptPt(rawCharData);
        }
    }
}

public sealed class PokemonSpriteTaskAnim
{
    public byte Active;
    public byte CurrSpriteFrame;
    public byte CurrAnimFrame;
    public byte FrameDelay;
    public readonly byte[] LoopTimers = new byte[PokemonSprite.AnimFrameCount];
    public SpriteAnimFrame[] AnimFrames = Array.Empty<SpriteAnimFrame>();

    // OFFSET 0x020088E0
    public static void PokemonSpriteTaskAnim_Init(PokemonSpriteTaskAnim anim, SpriteAnimFrame[] animFrames)
    {
        anim.Active = 1;
        anim.CurrAnimFrame = 0;
        anim.CurrSpriteFrame = (byte)animFrames[0].SpriteFrame;
        anim.FrameDelay = animFrames[0].FrameDelay;
        anim.AnimFrames = animFrames;
        for (int i = 0; i < PokemonSprite.AnimFrameCount; i++)
        {
            anim.LoopTimers[i] = 0;
        }
    }

    // OFFSET 0x02008900
    public static int PokemonSpriteTaskAnim_Tick(PokemonSpriteTaskAnim anim)
    {
        if (anim.Active == 0)
        {
            return -1;
        }
        SpriteAnimFrame[] frames = anim.AnimFrames;
        if (anim.FrameDelay != 0)
        {
            anim.FrameDelay--;
            return anim.CurrSpriteFrame;
        }
        anim.CurrAnimFrame++;
        while (frames[anim.CurrAnimFrame].SpriteFrame < -1)
        {
            byte current = anim.CurrAnimFrame;
            anim.LoopTimers[current]++;
            if (frames[current].FrameDelay == anim.LoopTimers[current] || frames[current].FrameDelay == 0)
            {
                anim.LoopTimers[current] = 0;
                anim.CurrAnimFrame++;
            }
            else
            {
                anim.CurrAnimFrame = (byte)(-2 - frames[current].SpriteFrame);
            }
        }
        if (frames[anim.CurrAnimFrame].SpriteFrame == -1 || anim.CurrAnimFrame >= PokemonSprite.AnimFrameCount)
        {
            anim.CurrSpriteFrame = 0;
            anim.Active = 0;
        }
        else
        {
            anim.CurrSpriteFrame = (byte)frames[anim.CurrAnimFrame].SpriteFrame;
            anim.FrameDelay = frames[anim.CurrAnimFrame].FrameDelay;
        }
        return anim.CurrSpriteFrame;
    }
}

public sealed class PokemonSpriteManager
{
    public const int SpriteCount = 4;

    private const int ShadowArchive = 0x75;
    private const int ShadowMember = 0xFB;
    private const int CharBufferSize = 0x8000;
    private const int PlttBufferColors = 0x60;
    private const int TextureRowBytes = 0x80;
    private const int ShadowTextureOffset = 0x5050;
    private const int ShadowRows = 0x50;
    private const int ShadowRowBytes = 0x28;
    private const int FormatPalette16 = 2;

    private static int[]? sSpriteFrames;
    private static int[]? sShadowFrames;

    public readonly PokemonSprite[] Sprites = CreateSprites();
    public readonly ImageProxy ImageProxy = new();
    public readonly ImagePaletteProxy PaletteProxy = new();
    public readonly CharacterData CharData = new();
    public readonly PaletteFile PlttData = new();
    public int HeapID;
    public uint CharBaseAddr;
    public uint CharSize;
    public uint PlttBaseAddr;
    public uint PlttSize;
    public byte[] CharRawData = Array.Empty<byte>();
    public ushort[] PlttRawData = Array.Empty<ushort>();
    public ushort[] PlttRawDataUnfaded = Array.Empty<ushort>();
    public byte Unused330;
    public byte NeedCharUpload;
    public byte NeedPlttUpload;
    public byte ExcludeIdentity;
    public uint HideShadows;

    private static PokemonSprite[] CreateSprites()
    {
        PokemonSprite[] sprites = new PokemonSprite[SpriteCount];
        for (int i = 0; i < SpriteCount; i++)
        {
            sprites[i] = new PokemonSprite();
        }
        return sprites;
    }

    private static int[] LoadFrameTable(uint address, int entries)
    {
        uint[] raw = RomData.Arm9U32(address, entries * 4);
        int[] table = new int[raw.Length];
        for (int i = 0; i < raw.Length; i++)
        {
            table[i] = (int)raw[i];
        }
        return table;
    }

    // OFFSET 0x0200762C
    public static PokemonSpriteManager PokemonSpriteManager_New(int heapID)
    {
        PokemonSpriteManager monSpriteMan = new();
        monSpriteMan.HeapID = heapID;
        monSpriteMan.Unused330 = 0;
        monSpriteMan.CharBaseAddr = 0;
        monSpriteMan.CharSize = CharBufferSize;
        monSpriteMan.PlttBaseAddr = 0;
        monSpriteMan.PlttSize = 0x80;
        monSpriteMan.CharRawData = new byte[CharBufferSize];
        monSpriteMan.PlttRawData = new ushort[PlttBufferColors];
        monSpriteMan.PlttRawDataUnfaded = new ushort[PlttBufferColors];
        for (int i = 0; i < SpriteCount; i++)
        {
            monSpriteMan.Sprites[i].Clear();
        }
        Render3D.SetupSoftwareSpriteCamera();
        monSpriteMan.ExcludeIdentity = 0;
        byte[] file = NARC.NARC_AllocAndReadWholeMemberByIndexPair(ShadowArchive, ShadowMember, monSpriteMan.HeapID);
        G2d.GetUnpackedCharacterData(file, out CharacterData? charData);
        monSpriteMan.CharData.PixelFormat = charData!.PixelFormat;
        monSpriteMan.CharData.MappingType = charData.MappingType;
        monSpriteMan.CharData.CharacterFormat = charData.CharacterFormat;
        byte[] rawData = charData.RawData;
        PokemonSprite.PokemonSprite_DecryptPt(rawData);
        Array.Fill(monSpriteMan.CharRawData, rawData[0]);
        for (int row = 0; row < ShadowRows; row++)
        {
            for (int column = 0; column < ShadowRowBytes; column++)
            {
                monSpriteMan.CharRawData[ShadowTextureOffset + row * TextureRowBytes + column] = rawData[row * ShadowRows + column];
            }
        }
        monSpriteMan.NeedCharUpload = 1;
        monSpriteMan.NeedPlttUpload = 1;
        return monSpriteMan;
    }

    // OFFSET 0x02007768
    public static void PokemonSpriteManager_DrawSprites(PokemonSpriteManager monSpriteMan)
    {
        sSpriteFrames ??= LoadFrameTable(0x020E4ECC, SpriteCount * 2);
        sShadowFrames ??= LoadFrameTable(0x020E4D50, 4);
        BufferPokemonSpriteCharData(monSpriteMan);
        BufferPokemonSpritePlttData(monSpriteMan);
        Render3D.FlushBuffer();
        Render3D.PushMatrix();
        Render3D.SetTexImageParam(
            (monSpriteMan.CharBaseAddr >> 3)
            | (monSpriteMan.ImageProxy.Format << 26)
            | (1u << 30)
            | (monSpriteMan.ImageProxy.SizeS << 20)
            | (monSpriteMan.ImageProxy.SizeT << 23)
            | (monSpriteMan.ImageProxy.Color0Transparent << 29));
        int paletteShift = 4 - (monSpriteMan.ImageProxy.Format == FormatPalette16 ? 1 : 0);
        for (int i = 0; i < SpriteCount; i++)
        {
            PokemonSprite sprite = monSpriteMan.Sprites[i];
            if (!sprite.Active || sprite.Hide || sprite.ForceHide)
            {
                continue;
            }
            sprite.Callback?.Invoke(sprite);
            Render3D.FlushBuffer();
            if (monSpriteMan.ExcludeIdentity != 1)
            {
                Render3D.Identity();
            }
            PokemonSprite.PokemonSprite_TickAnim(sprite);
            Render3D.SetPaletteBase((monSpriteMan.PlttBaseAddr + (uint)i * 0x20) >> paletteShift);
            int pivotX = sprite.X + sprite.PivotX;
            int pivotY = sprite.Y + sprite.PivotY;
            Render3D.Translate(pivotX << 12, pivotY << 12, sprite.Z << 12);
            Render3D.RotateX(Render3D.Sin(sprite.RotationX >> 4), Render3D.Cos(sprite.RotationX >> 4));
            Render3D.RotateY(Render3D.Sin(sprite.RotationY >> 4), Render3D.Cos(sprite.RotationY >> 4));
            Render3D.RotateZ(Render3D.Sin(sprite.RotationZ >> 4), Render3D.Cos(sprite.RotationZ >> 4));
            Render3D.Translate(-(pivotX << 12), -(pivotY << 12), -(sprite.Z << 12));
            uint diffuse = (uint)(sprite.DiffuseR | (sprite.DiffuseG << 5) | (sprite.DiffuseB << 10));
            uint ambient = (uint)(sprite.AmbientR | (sprite.AmbientG << 5) | (sprite.AmbientB << 10));
            Render3D.SetDiffuseAmbient(diffuse | (ambient << 16) | 0x8000);
            Render3D.SetSpecularEmission(0x4210);
            Render3D.SetPolygonAttr(0xC0 | ((uint)sprite.PolygonID << 24) | ((uint)sprite.Alpha << 16));
            int frame = (i * 2 + sprite.CurrSpriteFrame) * 4;
            if (sprite.PartialDraw)
            {
                int u0 = sprite.DrawXOffset + sSpriteFrames[frame];
                int v0 = sprite.DrawYOffset + sSpriteFrames[frame + 1];
                short x = (short)(sprite.XOffset + (sprite.X - 40 + sprite.DrawXOffset));
                short y = (short)(sprite.YOffset + (sprite.Y - 40 + sprite.DrawYOffset) - sprite.ShadowHeight);
                Render3D.DrawSpriteFast(x, y, sprite.Z + sprite.ZOffset, sprite.DrawWidth, sprite.DrawHeight, u0, v0, sprite.DrawWidth + u0, sprite.DrawHeight + v0);
            }
            else
            {
                int width = (sprite.ScaleX * 80) >> 8;
                int height = (sprite.ScaleY * 80) >> 8;
                short x = (short)(sprite.X - width / 2 + sprite.XOffset);
                short y = (short)(sprite.Y - height / 2 + sprite.YOffset - sprite.ShadowHeight);
                Render3D.DrawSpriteFast(x, y, sprite.Z + sprite.ZOffset, width, height, sSpriteFrames[frame], sSpriteFrames[frame + 1], sSpriteFrames[frame + 2], sSpriteFrames[frame + 3]);
            }
            if (sprite.ShadowPaletteSlot == 0 || sprite.ShadowSize == 0 || sprite.PartialDraw || (monSpriteMan.HideShadows & 1) != 0)
            {
                continue;
            }
            if (monSpriteMan.ExcludeIdentity != 1)
            {
                Render3D.Identity();
            }
            Render3D.SetPaletteBase((monSpriteMan.PlttBaseAddr + (uint)(sprite.ShadowPaletteSlot + 3) * 0x20) >> paletteShift);
            int shadowWidth;
            int shadowHeight;
            if (sprite.ShadowIsAffine)
            {
                shadowWidth = (sprite.ScaleX << 6) >> 8;
                shadowHeight = (sprite.ScaleY << 4) >> 8;
            }
            else
            {
                shadowWidth = 64;
                shadowHeight = 16;
            }
            if (sprite.ShadowFollowX)
            {
                sprite.ShadowX = (short)(sprite.ShadowXOffset + (sprite.X + sprite.XOffset));
            }
            if (sprite.ShadowFollowY)
            {
                sprite.ShadowY = (short)(sprite.ShadowYOffset + (sprite.Y + sprite.YOffset));
            }
            int shadow = sprite.ShadowSize * 4;
            Render3D.DrawSpriteFast(
                (short)(sprite.ShadowX - shadowWidth / 2),
                (short)(sprite.ShadowY - shadowHeight / 2),
                -1000,
                shadowWidth,
                shadowHeight,
                sShadowFrames[shadow],
                sShadowFrames[shadow + 1],
                sShadowFrames[shadow + 2],
                sShadowFrames[shadow + 3]);
        }
        Render3D.PopMatrix(1);
    }

    // OFFSET 0x02007B6C
    public static void PokemonSpriteManager_Free(PokemonSpriteManager monSpriteMan)
    {
        monSpriteMan.CharRawData = Array.Empty<byte>();
        monSpriteMan.PlttRawData = Array.Empty<ushort>();
        monSpriteMan.PlttRawDataUnfaded = Array.Empty<ushort>();
    }

    // OFFSET 0x02007C34
    public static PokemonSprite PokemonSpriteManager_CreateSprite(PokemonSpriteManager monSpriteMan, PokemonSpriteTemplate spriteTemplate, int x, int y, int z, int polygonID, SpriteAnimFrame[]? animFrames, PokemonSpriteCallback? callback)
    {
        int index;
        for (index = 0; index < SpriteCount; index++)
        {
            if (!monSpriteMan.Sprites[index].Active)
            {
                break;
            }
        }
        if (index == SpriteCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return PokemonSpriteManager_CreateSpriteAtIndex(monSpriteMan, spriteTemplate, x, y, z, polygonID, index, animFrames, callback);
    }

    // OFFSET 0x02007C7C
    public static PokemonSprite PokemonSpriteManager_CreateSpriteAtIndex(PokemonSpriteManager monSpriteMan, PokemonSpriteTemplate spriteTemplate, int x, int y, int z, int polygonID, int index, SpriteAnimFrame[]? animFrames, PokemonSpriteCallback? callback)
    {
        PokemonSprite monSprite = monSpriteMan.Sprites[index];
        if (monSprite.Active)
        {
            ErrorHandling.GF_AssertFail();
        }
        monSprite.Clear();
        monSprite.Active = true;
        monSprite.NeedReloadChar = true;
        monSprite.NeedReloadPltt = true;
        monSprite.PolygonID = polygonID & 0x3F;
        monSprite.Template.CopyFrom(spriteTemplate);
        monSprite.TemplateBackup.CopyFrom(spriteTemplate);
        monSprite.X = (short)x;
        monSprite.Y = (short)y;
        monSprite.Z = z;
        monSprite.ScaleX = 0x100;
        monSprite.ScaleY = 0x100;
        monSprite.Alpha = 31;
        monSprite.DiffuseR = 31;
        monSprite.DiffuseG = 31;
        monSprite.DiffuseB = 31;
        monSprite.AmbientR = 16;
        monSprite.AmbientG = 16;
        monSprite.AmbientB = 16;
        monSprite.Callback = callback;
        monSprite.ShadowX = (short)x;
        monSprite.ShadowY = (short)y;
        monSprite.ShadowFollowX = true;
        monSprite.ShadowFollowY = true;
        monSprite.ShadowIsAffine = true;
        if (animFrames != null)
        {
            Array.Copy(animFrames, monSprite.AnimFrames, PokemonSprite.AnimFrameCount);
        }
        return monSprite;
    }

    // OFFSET 0x02007DD4
    public static void PokemonSpriteManager_DeleteAll(PokemonSpriteManager monSpriteMan)
    {
        for (int i = 0; i < SpriteCount; i++)
        {
            PokemonSprite.PokemonSprite_Delete(monSpriteMan.Sprites[i]);
        }
    }

    // OFFSET 0x0200872C
    public static void PokemonSpriteManager_StartFadeAll(PokemonSpriteManager monSpriteMan, int initAlpha, int targetAlpha, int delay, int color)
    {
        for (int i = 0; i < SpriteCount; i++)
        {
            PokemonSprite monSprite = monSpriteMan.Sprites[i];
            if (!monSprite.Active)
            {
                continue;
            }
            monSprite.FadeActive = true;
            monSprite.FadeInitAlpha = (byte)initAlpha;
            monSprite.FadeTargetAlpha = (byte)targetAlpha;
            monSprite.FadeDelayCounter = 0;
            monSprite.FadeDelayLength = (byte)delay;
            monSprite.FadeTargetColor = color;
        }
    }

    // OFFSET 0x02008A78
    public static void PokemonSpriteManager_SetCharBaseAddrAndSize(PokemonSpriteManager monSpriteMan, uint addr, uint size)
    {
        monSpriteMan.CharBaseAddr = addr;
        monSpriteMan.CharSize = size;
    }

    // OFFSET 0x02008A84
    public static void PokemonSpriteManager_SetPlttBaseAddrAndSize(PokemonSpriteManager monSpriteMan, uint addr, uint size)
    {
        monSpriteMan.PlttBaseAddr = addr;
        monSpriteMan.PlttSize = size;
    }

    // OFFSET 0x02008A94
    public static void PokemonSpriteManager_UpdateCharAndPltt(PokemonSpriteManager monSpriteMan)
    {
        if (monSpriteMan.NeedCharUpload != 0)
        {
            monSpriteMan.NeedCharUpload = 0;
            monSpriteMan.ImageProxy.Init();
            monSpriteMan.CharData.Height = 0x20;
            monSpriteMan.CharData.Width = 0x20;
            monSpriteMan.CharData.SizeBytes = monSpriteMan.CharSize;
            monSpriteMan.CharData.RawData = monSpriteMan.CharRawData;
            G2d.LoadImageMapping(1, monSpriteMan.CharData, monSpriteMan.CharBaseAddr, G2d.Vram3D, monSpriteMan.ImageProxy);
        }
        if (monSpriteMan.NeedPlttUpload != 0)
        {
            monSpriteMan.NeedPlttUpload = 0;
            monSpriteMan.PaletteProxy.Init();
            monSpriteMan.PlttData.ByteSize = monSpriteMan.PlttSize;
            monSpriteMan.PlttData.Colors = monSpriteMan.PlttRawData;
            G2d.LoadPalette(monSpriteMan.PlttData, null, monSpriteMan.PlttBaseAddr, G2d.Vram3D, monSpriteMan.PaletteProxy);
        }
    }

    // OFFSET 0x02008B2C
    public static void PokemonSpriteManager_SetExcludeIdentity(PokemonSpriteManager monSpriteMan, int value)
    {
        monSpriteMan.ExcludeIdentity = (byte)value;
    }

    // OFFSET 0x02008B54
    public static void PokemonSpriteManager_SetHideShadows(PokemonSpriteManager monSpriteMan, uint value)
    {
        monSpriteMan.HideShadows |= value;
    }

    // OFFSET 0x02008B60
    public static void PokemonSpriteManager_ClearHideShadows(PokemonSpriteManager monSpriteMan, uint value)
    {
        monSpriteMan.HideShadows &= ~value;
    }

    private const int ShadowPaletteMember = 0xFC;
    private const int SpriteCharBytes = 0x2800;
    private const int FrameBytes = 0x50;
    private const int HalfFrameBytes = 0x28;
    private const int SpritePaletteColors = 0x10;

    // OFFSET 0x02008B78
    private static void BufferPokemonSpriteCharData(PokemonSpriteManager monSpriteMan)
    {
        byte needUpload = 0;
        for (int i = 0; i < SpriteCount; i++)
        {
            PokemonSprite monSprite = monSpriteMan.Sprites[i];
            if (!monSprite.Active || !monSprite.NeedReloadChar)
            {
                continue;
            }
            monSprite.NeedReloadChar = false;
            needUpload = 1;
            int narcID = (int)(monSprite.Template.Word0 & 0xFFFF);
            int character = (int)(monSprite.Template.Word0 >> 16);
            byte[] file = NARC.NARC_AllocAndReadWholeMemberByIndexPair(narcID, character, monSpriteMan.HeapID);
            G2d.GetUnpackedCharacterData(file, out CharacterData? charData);
            monSpriteMan.CharData.PixelFormat = charData!.PixelFormat;
            monSpriteMan.CharData.MappingType = charData.MappingType;
            monSpriteMan.CharData.CharacterFormat = charData.CharacterFormat;
            byte[] rawData = charData.RawData;
            PokemonSprite.PokemonSprite_Decrypt(rawData, narcID);
            PokemonSprite.TryDrawSpindaSpots(monSprite, rawData);
            byte[] dest = monSpriteMan.CharRawData;
            for (int y = 0; y < FrameBytes; y++)
            {
                int sourceRow = y * FrameBytes;
                int flippedRow = (FrameBytes - 1 - y) * FrameBytes;
                for (int x = 0; x < FrameBytes; x++)
                {
                    int index;
                    if (i != 3)
                    {
                        index = i * SpriteCharBytes + y * TextureRowBytes + x;
                    }
                    else if (x < HalfFrameBytes)
                    {
                        index = y * TextureRowBytes + x + 0x50;
                    }
                    else
                    {
                        index = y * TextureRowBytes + x + 0x2828;
                    }
                    int mirrored = x < HalfFrameBytes ? HalfFrameBytes - 1 - x : FrameBytes - 1 - (x - HalfFrameBytes);
                    if (monSprite.FlipH && monSprite.FlipV)
                    {
                        dest[index] = PokemonSprite.SwapNybbles(rawData[flippedRow + mirrored]);
                    }
                    else if (monSprite.FlipH)
                    {
                        dest[index] = PokemonSprite.SwapNybbles(rawData[sourceRow + mirrored]);
                    }
                    else if (monSprite.FlipV)
                    {
                        dest[index] = rawData[flippedRow + x];
                    }
                    else if (monSprite.Mosaic != 0)
                    {
                        if (y % (monSprite.Mosaic * 2) != 0)
                        {
                            dest[index] = dest[index - TextureRowBytes];
                        }
                        else if (x % monSprite.Mosaic != 0)
                        {
                            dest[index] = dest[index - 1];
                        }
                        else
                        {
                            int nybble = rawData[sourceRow + x] & 0xF;
                            dest[index] = (byte)((nybble << 4) | nybble);
                        }
                    }
                    else
                    {
                        dest[index] = rawData[sourceRow + x];
                    }
                }
            }
        }
        monSpriteMan.NeedCharUpload = needUpload;
    }

    // OFFSET 0x02008FC8
    private static void BufferPokemonSpritePlttData(PokemonSpriteManager monSpriteMan)
    {
        byte needUpload = 0;
        for (int i = 0; i < SpriteCount; i++)
        {
            PokemonSprite monSprite = monSpriteMan.Sprites[i];
            int offset = i * SpritePaletteColors;
            if (monSprite.Active && monSprite.NeedReloadPltt)
            {
                monSprite.NeedReloadPltt = false;
                needUpload = 1;
                int narcID = (int)(monSprite.Template.Word0 & 0xFFFF);
                int palette = (int)(monSprite.Template.Word1 & 0xFFFF);
                byte[] file = NARC.NARC_AllocAndReadWholeMemberByIndexPair(narcID, palette, monSpriteMan.HeapID);
                G2d.GetUnpackedPaletteData(file, out PaletteFile? plttData);
                monSpriteMan.PlttData.Format = plttData!.Format;
                for (int j = 0; j < SpritePaletteColors; j++)
                {
                    monSpriteMan.PlttRawData[offset + j] = plttData.Colors[j];
                    monSpriteMan.PlttRawDataUnfaded[offset + j] = plttData.Colors[j];
                }
                if (monSprite.ShadowPaletteSlot != 0)
                {
                    file = NARC.NARC_AllocAndReadWholeMemberByIndexPair(ShadowArchive, ShadowPaletteMember, monSpriteMan.HeapID);
                    G2d.GetUnpackedPaletteData(file, out plttData);
                    for (int j = 0; j < SpritePaletteColors; j++)
                    {
                        int shadowIndex = j + (monSprite.ShadowPaletteSlot + 3) * SpritePaletteColors;
                        monSpriteMan.PlttRawData[shadowIndex] = plttData!.Colors[j];
                        monSpriteMan.PlttRawDataUnfaded[shadowIndex] = plttData.Colors[j];
                    }
                }
            }
            if (!monSprite.Active || !monSprite.FadeActive)
            {
                continue;
            }
            if (monSprite.FadeDelayCounter != 0)
            {
                monSprite.FadeDelayCounter--;
                continue;
            }
            needUpload = 1;
            monSprite.FadeDelayCounter = monSprite.FadeDelayLength;
            PaletteData.BlendPalette(monSpriteMan.PlttRawDataUnfaded, offset, monSpriteMan.PlttRawData, offset, SpritePaletteColors, monSprite.FadeInitAlpha, (ushort)monSprite.FadeTargetColor);
            if (monSprite.ShadowPaletteSlot != 0)
            {
                int shadowOffset = (monSprite.ShadowPaletteSlot + 3) * SpritePaletteColors;
                PaletteData.BlendPalette(monSpriteMan.PlttRawDataUnfaded, shadowOffset, monSpriteMan.PlttRawData, shadowOffset, SpritePaletteColors, monSprite.FadeInitAlpha, (ushort)monSprite.FadeTargetColor);
            }
            if (monSprite.FadeInitAlpha == monSprite.FadeTargetAlpha)
            {
                monSprite.FadeActive = false;
            }
            else if (monSprite.FadeInitAlpha > monSprite.FadeTargetAlpha)
            {
                monSprite.FadeInitAlpha--;
            }
            else
            {
                monSprite.FadeInitAlpha++;
            }
        }
        monSpriteMan.NeedPlttUpload = needUpload;
    }
}
