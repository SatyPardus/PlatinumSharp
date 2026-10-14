using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public sealed partial class Options
{
    public const int TextSpeedSlow = 0;
    public const int TextSpeedMid = 1;
    public const int TextSpeedFast = 2;
    public const int ButtonModeNormal = 0;
    public const int ButtonModeStartIsX = 1;
    public const int ButtonModeLIsA = 2;
    public const int SystemButtonModeNormal = 0;
    public const int SystemButtonModeStartIsX = 1;
    public const int SystemButtonModeLIsA = 3;

    private const int TextDelaySlow = 8;
    private const int TextDelayMid = 4;
    private const int TextDelayFast = 1;
    private const int SoundModeShift = 4;
    private const int BattleStyleShift = 6;
    private const int BattleSceneShift = 7;
    private const int ButtonModeShift = 8;
    private const int FrameShift = 10;

    // OFFSET 0x020279FC
    public static Options Options_New(int heapID)
    {
        Options options = new();
        Options_Init(options);
        return options;
    }

    // OFFSET 0x02027A10
    public static void Options_Copy(Options source, Options destination)
    {
        destination.Flags = source.Flags;
    }

    // OFFSET 0x02027A68
    public static void Options_SetSystemButtonMode(SaveData? saveData, int buttonMode)
    {
        if (saveData != null)
        {
            buttonMode = Options_ButtonMode(PlayerSave.SaveData_GetOptions(saveData));
        }
        switch (buttonMode)
        {
            case ButtonModeStartIsX:
                gSystem.ButtonMode = SystemButtonModeStartIsX;
                break;
            case ButtonModeLIsA:
                gSystem.ButtonMode = SystemButtonModeLIsA;
                break;
            default:
                gSystem.ButtonMode = SystemButtonModeNormal;
                break;
        }
    }

    // OFFSET 0x02027AA4
    public static int Options_TextSpeed(Options options)
    {
        return options.Flags & TextSpeedMask;
    }

    // OFFSET 0x02027AAC
    public static void Options_SetTextSpeed(Options options, int textSpeed)
    {
        options.Flags = (ushort)((options.Flags & ~TextSpeedMask) | ((ushort)textSpeed & TextSpeedMask));
    }

    // OFFSET 0x02027AC0
    public static int Options_TextFrameDelay(Options options)
    {
        int textSpeed = Options_TextSpeed(options);
        if (textSpeed == TextSpeedSlow)
        {
            return TextDelaySlow;
        }
        if (textSpeed == TextSpeedMid)
        {
            return TextDelayMid;
        }
        return TextDelayFast;
    }

    // OFFSET 0x02027ADC
    public static int Options_SoundMode(Options options)
    {
        return (options.Flags & SoundModeMask) >> SoundModeShift;
    }

    // OFFSET 0x02027AE4
    public static void Options_SetSoundMode(Options options, int soundMode)
    {
        options.Flags = (ushort)((options.Flags & ~SoundModeMask) | ((soundMode << SoundModeShift) & SoundModeMask));
    }

    // OFFSET 0x02027AF8
    public static int Options_BattleScene(Options options)
    {
        return (options.Flags & BattleSceneMask) >> BattleSceneShift;
    }

    // OFFSET 0x02027B00
    public static void Options_SetBattleScene(Options options, int battleScene)
    {
        options.Flags = (ushort)((options.Flags & ~BattleSceneMask) | ((battleScene << BattleSceneShift) & BattleSceneMask));
    }

    // OFFSET 0x02027B14
    public static int Options_BattleStyle(Options options)
    {
        return (options.Flags & BattleStyleMask) >> BattleStyleShift;
    }

    // OFFSET 0x02027B1C
    public static void Options_SetBattleStyle(Options options, int battleStyle)
    {
        options.Flags = (ushort)((options.Flags & ~BattleStyleMask) | ((battleStyle << BattleStyleShift) & BattleStyleMask));
    }

    // OFFSET 0x02027B30
    public static int Options_ButtonMode(Options options)
    {
        return (options.Flags & ButtonModeMask) >> ButtonModeShift;
    }

    // OFFSET 0x02027B38
    public static void Options_SetButtonMode(Options options, int buttonMode)
    {
        options.Flags = (ushort)((options.Flags & ~ButtonModeMask) | ((buttonMode << ButtonModeShift) & ButtonModeMask));
    }

    // OFFSET 0x02027B50
    public static int Options_Frame(Options options)
    {
        return (options.Flags & FrameMask) >> FrameShift;
    }

    // OFFSET 0x02027B58
    public static void Options_SetFrame(Options options, int frame)
    {
        options.Flags = (ushort)((options.Flags & ~FrameMask) | ((frame << FrameShift) & FrameMask));
    }
}
