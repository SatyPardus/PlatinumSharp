namespace PlatinumSharp.Game;

public static class PokedexLanguage
{
    public const int Count = 6;

    private const uint LanguageTableAddress = 0x020F681C;

    private static byte[]? languages;

    // OFFSET 0x020986CC
    public static int PokedexLanguage_GetIndex(int language)
    {
        languages ??= RomData.Arm9(LanguageTableAddress, Count);
        int index;
        for (index = 0; index < Count; index++)
        {
            if (language == languages[index])
            {
                break;
            }
        }
        return index;
    }

    // OFFSET 0x020986E8
    public static int PokedexLanguage_GetLanguage(int index)
    {
        if (index >= Count)
        {
            ErrorHandling.GF_AssertFail();
        }
        languages ??= RomData.Arm9(LanguageTableAddress, Count);
        return languages[index];
    }
}
