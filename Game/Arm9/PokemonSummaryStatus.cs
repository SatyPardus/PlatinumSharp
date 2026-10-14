namespace PlatinumSharp.Game;

public sealed partial class Pokemon
{
    public const int SummaryStatusParalyzed = 1;
    public const int SummaryStatusFrozen = 2;
    public const int SummaryStatusAsleep = 3;
    public const int SummaryStatusPoisoned = 4;
    public const int SummaryStatusBurned = 5;
    public const int SummaryStatusFainted = 6;
    public const int SummaryStatusNone = 7;

    private const int SummaryParamStatus = 0xA0;
    private const int SummaryParamCurrentHP = 0xA3;
    private const uint StatusPoisonMask = 0x88;
    private const uint StatusSleepMask = 7;
    private const uint StatusBurn = 0x10;
    private const uint StatusFreeze = 0x20;
    private const uint StatusParalysis = 0x40;

    // OFFSET 0x0208E9F0
    public static int Pokemon_GetSummaryStatus(Pokemon mon)
    {
        uint status = Pokemon_GetValue(mon, SummaryParamStatus, null);
        if (Pokemon_GetValue(mon, SummaryParamCurrentHP, null) == 0)
        {
            return SummaryStatusFainted;
        }
        if ((status & StatusPoisonMask) != 0)
        {
            return SummaryStatusPoisoned;
        }
        if ((status & StatusSleepMask) != 0)
        {
            return SummaryStatusAsleep;
        }
        if ((status & StatusBurn) != 0)
        {
            return SummaryStatusBurned;
        }
        if ((status & StatusFreeze) != 0)
        {
            return SummaryStatusFrozen;
        }
        if ((status & StatusParalysis) != 0)
        {
            return SummaryStatusParalyzed;
        }
        return SummaryStatusNone;
    }
}

public static class ContestEffect
{
    private const uint ContestEffectTable = 0x020F568C;
    private const int ContestEffectSize = 0x1A;
    private const int ContestEffectCount = 0x18;

    // OFFSET 0x02095734
    public static int ContestEffect_GetAppeal(int effect)
    {
        if (effect >= ContestEffectCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return (sbyte)RomData.Arm9((uint)(ContestEffectTable + 4 + effect * ContestEffectSize), 1)[0];
    }

    // OFFSET 0x0209577C
    public static int ContestEffect_GetDescription(int effect)
    {
        if (effect >= ContestEffectCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return effect + 0x2D;
    }
}
