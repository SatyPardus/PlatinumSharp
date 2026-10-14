namespace PlatinumSharp.Game;

public static class PokedexRating
{
    private static readonly (int MaxCount, int Message)[] sLocalRatings =
    {
        (15, 6),
        (30, 7),
        (45, 8),
        (60, 9),
        (80, 10),
        (100, 11),
        (120, 12),
        (140, 13),
        (160, 14),
        (180, 15),
        (200, 16),
        (209, 17),
    };

    private static readonly (int MaxCount, int Message)[] sNationalRatings =
    {
        (39, 0x16),
        (59, 0x17),
        (89, 0x18),
        (119, 0x19),
        (149, 0x1A),
        (189, 0x1B),
        (229, 0x1C),
        (269, 0x1D),
        (309, 0x1E),
        (349, 0x1F),
        (379, 0x20),
        (409, 0x21),
        (429, -1),
        (449, 0x24),
        (459, 0x25),
        (469, 0x26),
        (475, 0x27),
        (481, 0x28),
    };

    // OFFSET 0x0205E078
    public static int PokedexRating_GetLocalMessage(int seenCount, ushort visitedEterna)
    {
        foreach ((int maxCount, int message) in sLocalRatings)
        {
            if (seenCount <= maxCount)
            {
                return message;
            }
        }
        return visitedEterna != 0 ? 4 : 5;
    }

    // OFFSET 0x0205E0E4
    public static int PokedexRating_GetNationalMessage(int caughtCount, ushort gender)
    {
        foreach ((int maxCount, int message) in sNationalRatings)
        {
            if (caughtCount <= maxCount)
            {
                if (message < 0)
                {
                    return gender != 0 ? 0x23 : 0x22;
                }
                return message;
            }
        }
        return gender != 0 ? 0x2A : 0x29;
    }
}
