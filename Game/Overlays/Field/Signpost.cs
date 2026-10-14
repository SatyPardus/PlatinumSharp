using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class Signpost
{
    public const int WindowOffset = 0x00;
    public const int NarcMemberIdxOffset = 0x10;
    public const int TypeOffset = 0x12;
    public const int CommandOffset = 0x13;
    public const int Size = 0x14;

    public readonly Window Window = new();
    public ushort NarcMemberIdx;
    public byte Type;
    public byte Command;
    public bool WindowAdded;
}

public static class Signposts
{
    public const int CommandNone = 0;
    public const int CommandDraw = 1;
    public const int CommandScrollOut = 2;
    public const int CommandScrollIn = 3;
    public const int CommandRemove = 4;

    private const int CommandMask = 0x7F;
    private const int BgLayerMain3 = 3;
    private const byte BgOffsetSetY = 3;
    private const byte BgOffsetAddY = 4;
    private const byte BgOffsetSubY = 5;
    private const int HiddenYOffset = -0x30;
    private const int ScrollSpeed = 0x10;
    private const int TilemapY = 0x12;
    private const int TilemapWidth = 0x20;
    private const int TilemapHeight = 6;
    private const int TilemapPalette = 0x10;

    // OFFSET 0x021E1B08
    public static Signpost Signpost_Init(int heapID)
    {
        return new Signpost();
    }

    // OFFSET 0x021E1B20
    public static void Signpost_Free(object? signpostObject)
    {
        Signpost signpost = (Signpost)signpostObject!;
        if (signpost.WindowAdded)
        {
            Bg.Window_Remove(signpost.Window);
        }
    }

    // OFFSET 0x021E1B38
    public static void Signpost_SetTypeAndNARCMemberIdx(Signpost signpost, ushort type, ushort narcMemberIdx)
    {
        signpost.Type = (byte)type;
        signpost.NarcMemberIdx = narcMemberIdx;
    }

    // OFFSET 0x021E1B40
    public static void Signpost_SetCommand(Signpost signpost, byte command)
    {
        signpost.Command = (byte)(command & CommandMask);
    }

    // OFFSET 0x021E1B50
    public static Window Signpost_GetWindow(Signpost signpost)
    {
        return signpost.Window;
    }

    // OFFSET 0x021E1B54
    public static byte Signpost_GetType(Signpost signpost)
    {
        return signpost.Type;
    }

    // OFFSET 0x021E1B58
    public static bool Signpost_IsDone(Signpost signpost)
    {
        if (signpost.Command == CommandNone)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x021E1B68
    public static void Signpost_DoCurrentCommand(FieldSystem fieldSystem)
    {
        Signpost signpost = (Signpost)fieldSystem.Signpost!;
        switch (signpost.Command)
        {
            case CommandDraw:
                Signpost_Draw(fieldSystem);
                signpost.Command = CommandNone;
                break;
            case CommandScrollOut:
                if (Signpost_ScrollOut(fieldSystem))
                {
                    signpost.Command = CommandNone;
                }
                break;
            case CommandScrollIn:
                if (Signpost_ScrollIn(fieldSystem))
                {
                    signpost.Command = CommandNone;
                }
                break;
            case CommandRemove:
                Signpost_Remove(fieldSystem);
                signpost.Command = CommandNone;
                break;
        }
    }

    // OFFSET 0x021E1BCC
    public static void Signpost_DoCommand(FieldSystem fieldSystem, int command)
    {
        Signpost_SetCommand((Signpost)fieldSystem.Signpost!, (byte)command);
        Signpost_DoCurrentCommand(fieldSystem);
    }

    // OFFSET 0x021E1BE0
    private static void Signpost_Draw(FieldSystem fieldSystem)
    {
        Bg.Bg_SetOffset(fieldSystem.BgConfig!, BgLayerMain3, BgOffsetSetY, HiddenYOffset);
        Signpost signpost = (Signpost)fieldSystem.Signpost!;
        if (!signpost.WindowAdded)
        {
            FieldMessage.FieldMessage_AddSignpostWindow(fieldSystem.BgConfig!, signpost.Window, signpost.Type, BgLayerMain3);
            signpost.WindowAdded = true;
        }
        FieldMessage.FieldMessage_LoadAndDrawSignpost(signpost.Window, signpost.Type, signpost.NarcMemberIdx);
    }

    // OFFSET 0x021E1C1C
    private static void Signpost_Remove(FieldSystem fieldSystem)
    {
        Signpost signpost = (Signpost)fieldSystem.Signpost!;
        if (signpost.WindowAdded)
        {
            Bg.Window_Remove(signpost.Window);
            Bg.Bg_FillTilemapRect(fieldSystem.BgConfig!, BgLayerMain3, 0, 0, TilemapY, TilemapWidth, TilemapHeight, TilemapPalette);
            Bg.Bg_CopyTilemapBufferToVRAM(fieldSystem.BgConfig!, BgLayerMain3);
            Bg.Bg_SetOffset(fieldSystem.BgConfig!, BgLayerMain3, BgOffsetSetY, 0);
            signpost.WindowAdded = false;
        }
    }

    // OFFSET 0x021E1C70
    private static bool Signpost_ScrollIn(FieldSystem fieldSystem)
    {
        int yOffset = Bg.Bg_GetYOffset(fieldSystem.BgConfig!, BgLayerMain3);
        if (yOffset == 0)
        {
            return true;
        }
        if (yOffset <= HiddenYOffset || yOffset >= 0)
        {
            Bg.Bg_SetOffset(fieldSystem.BgConfig!, BgLayerMain3, BgOffsetSetY, HiddenYOffset);
        }
        Bg.Bg_SetOffset(fieldSystem.BgConfig!, BgLayerMain3, BgOffsetAddY, ScrollSpeed);
        return false;
    }

    // OFFSET 0x021E1CB0
    private static bool Signpost_ScrollOut(FieldSystem fieldSystem)
    {
        int yOffset = Bg.Bg_GetYOffset(fieldSystem.BgConfig!, BgLayerMain3);
        if (yOffset == HiddenYOffset)
        {
            Bg.Bg_FillTilemapRect(fieldSystem.BgConfig!, BgLayerMain3, 0, 0, TilemapY, TilemapWidth, TilemapHeight, TilemapPalette);
            Bg.Bg_CopyTilemapBufferToVRAM(fieldSystem.BgConfig!, BgLayerMain3);
            Bg.Bg_SetOffset(fieldSystem.BgConfig!, BgLayerMain3, BgOffsetSetY, 0);
            return true;
        }
        if (yOffset <= HiddenYOffset || yOffset >= 0)
        {
            Bg.Bg_SetOffset(fieldSystem.BgConfig!, BgLayerMain3, BgOffsetSetY, 0);
        }
        Bg.Bg_SetOffset(fieldSystem.BgConfig!, BgLayerMain3, BgOffsetSubY, ScrollSpeed);
        return false;
    }
}
