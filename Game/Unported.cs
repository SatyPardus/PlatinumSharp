namespace PlatinumSharp.Game;

public static partial class Unported
{
    internal static Exception Missing(uint address)
    {
        return new NotImplementedException($"Function 0x{address:X8} is not ported yet");
    }

    public static Exception Partial(uint address)
    {
        return new NotImplementedException($"Function 0x{address:X8} is only ported for the idle path");
    }

    public static int sub_02038AB8() => throw Missing(0x02038AB8);
    public static void sub_02039834(int a, int b, int c) => throw Missing(0x02039834);
    public static void sub_02039A64(int a, int b) => throw Missing(0x02039A64);
    public static void sub_0209A74C(int a) => throw Missing(0x0209A74C);
    public static void sub_020C3EE4(int a) => throw Missing(0x020C3EE4);
    public static int sub_020CD050() => throw Missing(0x020CD050);
    public static int sub_020CD9CC() => throw Missing(0x020CD9CC);
    public static int sub_020D09B8() => throw Missing(0x020D09B8);
    public static void sub_02079AF4(object pcBoxes, uint boxID, GameString dest) => throw Missing(0x02079AF4);
    public static uint sub_02099720(uint countryID) => throw Missing(0x02099720);
    public static void sub_02075EF4(PokemonSpriteTemplate template, object mon, int face) => throw Missing(0x02075EF4);
    public static bool ScreenFadeFunc(uint address, ScreenFade fade) => throw Missing(address);
    public static int sub_020334A4() => throw Missing(0x020334A4);
    public static void sub_02033478() => throw Missing(0x02033478);
    public static void sub_020334CC() => throw Missing(0x020334CC);
    public static ApplicationTemplate sub_0208BE5C(byte applicationID) => throw Missing(0x0208BE5C);
    public static WiFiLibraryHeap sub_020A5404(byte[] buffer, uint size, int option) => throw Missing(0x020A5404);
    public static void sub_020A543C(WiFiLibraryHeap heap) => throw Missing(0x020A543C);
    public static byte[]? sub_020A5448(WiFiLibraryHeap heap, uint size, int align) => throw Missing(0x020A5448);
    public static void sub_020A55D8(WiFiLibraryHeap heap, byte[] memory) => throw Missing(0x020A55D8);
    public static void ov4_021D776C(Func<int, uint, int, byte[]?> alloc, Action<int, byte[]?> free) => throw Missing(0x021D776C);
    public static void ov4_021D8018() => throw Missing(0x021D8018);
    public static int ov4_021D7EAC() => throw Missing(0x021D7EAC);
    public static void sub_0209B49C() => throw Missing(0x0209B49C);
    public static void sub_020AC28C(RenderSurface surface, CellData cell) => throw Missing(0x020AC28C);
    public static void sub_0209AA74(int heapID, int errorType) => throw Missing(0x0209AA74);
    public static void sub_02030888(byte[] data) => throw Missing(0x02030888);
    public static void sub_0202F1DC(byte[] data) => throw Missing(0x0202F1DC);
    public static void sub_02259250(object clipSprite, out int x, out int y) => throw Missing(0x02259250);
    public static int sub_022591E0(object clipSprite) => throw Missing(0x022591E0);
    public static int sub_020765B8(object mon, int face) => throw Missing(0x020765B8);
    public static int sub_020A4498(byte[] friendKey) => throw Missing(0x020A4498);
    public static uint sub_02030698(object frontier, uint recordID, int index) => throw Missing(0x02030698);
    public static void sub_0206D1B8(FieldSystem fieldSystem, CaptureAttempt? captureAttempt, int resultMask) => throw Missing(0x0206D1B8);
}
