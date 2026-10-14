namespace PlatinumSharp.Game;

public struct OamAttributes
{
    public ushort Attr0;
    public ushort Attr1;
    public ushort Attr2;
}

public struct MtxFx22
{
    public int M00;
    public int M01;
    public int M10;
    public int M11;
}

public struct OamAffine
{
    public short PA;
    public short PB;
    public short PC;
    public short PD;
}

public sealed class OamManager
{
    public const int Entries = 128;
    public const int AffineEntries = 32;
    public const ushort AffineNone = 0xFFFE;

    private const ushort Hidden = 0x0200;
    private const int AffineEnable = 0x0100;
    private const int AffineIndexShift = 9;
    private const int AffineIndexMask = 0x1F << AffineIndexShift;

    private static readonly OamAttributes[][] screenOam = { CreateHidden(), CreateHidden() };
    private static readonly OamAffine[][] screenAffine = { new OamAffine[AffineEntries], new OamAffine[AffineEntries] };

    private OamAttributes[] buffer = Array.Empty<OamAttributes>();
    private OamAffine[] affineBuffer = Array.Empty<OamAffine>();
    private int oamStart;
    private int affineStart;
    private int used;
    private int affineUsed;

    public int Screen;

    public static event Action<int>? OamChanged;

    private static OamAttributes[] CreateHidden()
    {
        OamAttributes[] entries = new OamAttributes[Entries];
        for (int i = 0; i < Entries; i++)
        {
            entries[i].Attr0 = Hidden;
        }
        return entries;
    }

    public static OamAttributes[] GetScreenOam(int screen)
    {
        return screenOam[screen];
    }

    public static OamAffine[] GetScreenAffine(int screen)
    {
        return screenAffine[screen];
    }

    public static void FillScreen(int screen, uint fillWord)
    {
        ushort low = (ushort)fillWord;
        ushort high = (ushort)(fillWord >> 16);
        OamAttributes[] oam = screenOam[screen];
        OamAffine[] affine = screenAffine[screen];
        for (int i = 0; i < Entries; i++)
        {
            oam[i].Attr0 = low;
            oam[i].Attr1 = high;
            oam[i].Attr2 = low;
        }
        for (int i = 0; i < AffineEntries; i++)
        {
            affine[i].PA = (short)high;
            affine[i].PB = (short)high;
            affine[i].PC = (short)high;
            affine[i].PD = (short)high;
        }
        OamChanged?.Invoke(screen);
    }

    public static void InitModule()
    {
    }

    public static bool Create(OamManager manager, ushort oamStart, ushort oamCount, ushort affineStart, ushort affineCount, int screen)
    {
        if (oamStart + oamCount > Entries || affineStart + affineCount > AffineEntries)
        {
            return false;
        }
        manager.buffer = new OamAttributes[oamCount];
        manager.affineBuffer = new OamAffine[affineCount];
        manager.oamStart = oamStart;
        manager.affineStart = affineStart;
        manager.used = 0;
        manager.affineUsed = 0;
        manager.Screen = screen;
        return true;
    }

    public bool Entry(OamAttributes oam, ushort affineIndex)
    {
        if (used >= buffer.Length)
        {
            return false;
        }
        if (affineIndex != AffineNone)
        {
            oam.Attr0 |= AffineEnable;
            oam.Attr1 = (ushort)((oam.Attr1 & ~AffineIndexMask) | (((affineStart + affineIndex) << AffineIndexShift) & AffineIndexMask));
        }
        buffer[used++] = oam;
        return true;
    }

    public ushort EntryAffine(MtxFx22 matrix)
    {
        if (affineUsed >= affineBuffer.Length)
        {
            return AffineNone;
        }
        affineBuffer[affineUsed] = new OamAffine
        {
            PA = (short)(matrix.M00 >> 4),
            PB = (short)(matrix.M01 >> 4),
            PC = (short)(matrix.M10 >> 4),
            PD = (short)(matrix.M11 >> 4),
        };
        return (ushort)affineUsed++;
    }

    public void ApplyAndReset()
    {
        OamAttributes[] oam = screenOam[Screen];
        for (int i = 0; i < buffer.Length; i++)
        {
            oam[oamStart + i] = i < used ? buffer[i] : new OamAttributes { Attr0 = Hidden };
        }
        Array.Copy(affineBuffer, 0, screenAffine[Screen], affineStart, affineUsed);
        used = 0;
        affineUsed = 0;
        OamChanged?.Invoke(Screen);
    }
}
