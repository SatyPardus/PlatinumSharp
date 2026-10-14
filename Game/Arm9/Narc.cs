using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.FileSystem;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class NARC
{
    private const uint PathTableAddress = 0x02100498;

    private static readonly Dictionary<int, string> sPaths = new();

    public readonly FsFile File = new();
    public uint BtafStart;
    public uint GmifStart;
    public ushort NumFiles;

    private static string PathFor(int narcID)
    {
        if (!sPaths.TryGetValue(narcID, out string? path))
        {
            uint address = RomData.Arm9U32(PathTableAddress + (uint)narcID * 4, 1)[0];
            path = Bin.CString(RomData.Arm9(address, 64), 0);
            sPaths[narcID] = path;
        }
        return path;
    }

    private static uint LocateMember(FsFile file, string path, int memberIndex, out uint gmifStart, out uint fileEnd)
    {
        InitFile(file);
        OpenFile(file, path);
        SeekFile(file, 0xC, SeekSet);
        uint btafStart = ReadU16(file);
        SeekFile(file, (int)btafStart + 4, SeekSet);
        uint btafSize = ReadU32(file);
        ushort numFiles = ReadU16(file);
        if (numFiles <= memberIndex)
        {
            ErrorHandling.GF_AssertFail();
        }
        uint btnfStart = btafStart + btafSize;
        SeekFile(file, (int)btnfStart + 4, SeekSet);
        uint btnfSize = ReadU32(file);
        SeekFile(file, (int)btafStart + 0xC + memberIndex * 8, SeekSet);
        uint fileStart = ReadU32(file);
        fileEnd = ReadU32(file);
        gmifStart = btnfStart + btnfSize;
        return fileStart;
    }

    // OFFSET 0x020068C8
    private static void ReadFromNarcMemberByPathAndIndex(byte[] dest, string path, int memberIndex, int offset, int bytesToRead)
    {
        FsFile file = new();
        uint fileStart = LocateMember(file, path, memberIndex, out uint gmifStart, out uint fileEnd);
        SeekFile(file, (int)(gmifStart + 8 + fileStart) + offset, SeekSet);
        uint size = bytesToRead != 0 ? (uint)bytesToRead : fileEnd - fileStart;
        if (size == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        ReadFile(file, dest, 0, (int)size);
        CloseFile(file);
    }

    // OFFSET 0x020069A8
    private static byte[] AllocAndReadFromNarcMemberByPathAndIndex(string path, int memberIndex, int heapID, int offset, int bytesToRead, int allocAtEnd)
    {
        FsFile file = new();
        uint fileStart = LocateMember(file, path, memberIndex, out uint gmifStart, out uint fileEnd);
        SeekFile(file, (int)(gmifStart + 8 + fileStart) + offset, SeekSet);
        uint size = bytesToRead != 0 ? (uint)bytesToRead : fileEnd - fileStart;
        if (size == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        byte[] dest = (allocAtEnd == 0 ? Heap.Heap_Alloc(heapID, size) : Heap.Heap_AllocAtEnd(heapID, size))!;
        ReadFile(file, dest, 0, (int)size);
        CloseFile(file);
        return dest;
    }

    // OFFSET 0x02006AA4
    public static void NARC_ReadWholeMemberByIndexPair(byte[] dest, int narcID, int memberIndex)
    {
        ReadFromNarcMemberByPathAndIndex(dest, PathFor(narcID), memberIndex, 0, 0);
    }

    // OFFSET 0x02006AC0
    public static byte[] NARC_AllocAndReadWholeMemberByIndexPair(int narcID, int memberIndex, int heapID)
    {
        return AllocAndReadFromNarcMemberByPathAndIndex(PathFor(narcID), memberIndex, heapID, 0, 0, 0);
    }

    // OFFSET 0x02006ADC
    public static byte[] NARC_AllocAtEndAndReadWholeMemberByIndexPair(int narcID, int memberIndex, int heapID)
    {
        return AllocAndReadFromNarcMemberByPathAndIndex(PathFor(narcID), memberIndex, heapID, 0, 0, 1);
    }

    // OFFSET 0x02006AFC
    public static void NARC_ReadFromMemberByIndexPair(byte[] dest, int narcID, int memberIndex, int offset, int bytesToRead)
    {
        ReadFromNarcMemberByPathAndIndex(dest, PathFor(narcID), memberIndex, offset, bytesToRead);
    }

    // OFFSET 0x02006B18
    public static byte[] NARC_AllocAndReadFromMemberByIndexPair(int narcID, int memberIndex, int heapID, int offset, int bytesToRead)
    {
        return AllocAndReadFromNarcMemberByPathAndIndex(PathFor(narcID), memberIndex, heapID, offset, bytesToRead, 0);
    }

    // OFFSET 0x02006B38
    public static byte[] NARC_AllocAtEndAndReadFromMemberByIndexPair(int narcID, int memberIndex, int heapID, int offset, int bytesToRead)
    {
        return AllocAndReadFromNarcMemberByPathAndIndex(PathFor(narcID), memberIndex, heapID, offset, bytesToRead, 1);
    }

    // OFFSET 0x02006B58
    public static uint NARC_GetMemberSizeByIndexPair(int narcID, int memberIndex)
    {
        FsFile file = new();
        uint fileStart = LocateMember(file, PathFor(narcID), memberIndex, out uint gmifStart, out uint fileEnd);
        SeekFile(file, (int)(gmifStart + 8 + fileStart), SeekSet);
        uint size = fileEnd - fileStart;
        if (size == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        return size;
    }

    // OFFSET 0x02006C24
    public static NARC NARC_ctor(int narcID, int heapID)
    {
        NARC narc = new();
        narc.BtafStart = 0;
        InitFile(narc.File);
        OpenFile(narc.File, PathFor(narcID));
        SeekFile(narc.File, 0xC, SeekSet);
        narc.BtafStart = ReadU16(narc.File);
        SeekFile(narc.File, (int)narc.BtafStart + 4, SeekSet);
        uint chunkSize = ReadU32(narc.File);
        narc.NumFiles = ReadU16(narc.File);
        uint btnfStart = narc.BtafStart + chunkSize;
        SeekFile(narc.File, (int)btnfStart + 4, SeekSet);
        chunkSize = ReadU32(narc.File);
        narc.GmifStart = btnfStart + chunkSize;
        return narc;
    }

    // OFFSET 0x02006CA8
    public static void NARC_dtor(NARC narc)
    {
        CloseFile(narc.File);
    }

    private static uint SeekToMember(NARC narc, uint memberIndex, out uint fileEnd)
    {
        if (narc.NumFiles <= memberIndex)
        {
            ErrorHandling.GF_AssertFail();
        }
        SeekFile(narc.File, (int)(narc.BtafStart + 0xC + memberIndex * 8), SeekSet);
        uint fileStart = ReadU32(narc.File);
        fileEnd = ReadU32(narc.File);
        return fileStart;
    }

    // OFFSET 0x02006CB8
    public static byte[] NARC_AllocAndReadWholeMember(NARC narc, uint memberIndex, int heapID)
    {
        uint fileStart = SeekToMember(narc, memberIndex, out uint fileEnd);
        SeekFile(narc.File, (int)(narc.GmifStart + 8 + fileStart), SeekSet);
        byte[] dest = Heap.Heap_Alloc(heapID, fileEnd - fileStart)!;
        ReadFile(narc.File, dest, 0, dest.Length);
        return dest;
    }

    // OFFSET 0x02006D28
    public static void NARC_ReadWholeMember(NARC narc, uint memberIndex, byte[] dest)
    {
        uint fileStart = SeekToMember(narc, memberIndex, out uint fileEnd);
        SeekFile(narc.File, (int)(narc.GmifStart + 8 + fileStart), SeekSet);
        ReadFile(narc.File, dest, 0, (int)(fileEnd - fileStart));
    }

    // OFFSET 0x02006D84
    public static uint NARC_GetMemberSize(NARC narc, uint memberIndex)
    {
        uint fileStart = SeekToMember(narc, memberIndex, out uint fileEnd);
        return fileEnd - fileStart;
    }

    // OFFSET 0x02006DC8
    public static void NARC_ReadFromMember(NARC narc, uint memberIndex, uint offset, uint bytesToRead, byte[] dest)
    {
        if (narc.NumFiles <= memberIndex)
        {
            ErrorHandling.GF_AssertFail();
        }
        SeekFile(narc.File, (int)(narc.BtafStart + 0xC + memberIndex * 8), SeekSet);
        uint fileStart = ReadU32(narc.File);
        SeekFile(narc.File, (int)(offset + (narc.GmifStart + 8 + fileStart)), SeekSet);
        ReadFile(narc.File, dest, 0, (int)bytesToRead);
    }

    // OFFSET 0x02006E18
    public static void NARC_ReadFile(NARC narc, uint bytesToRead, byte[] dest)
    {
        ReadFile(narc.File, dest, 0, (int)bytesToRead);
    }

    // OFFSET 0x02006E28
    public static void NARC_Seek(NARC narc, uint offset)
    {
        SeekFile(narc.File, (int)offset, SeekCurrent);
    }

    // OFFSET 0x02006E34
    public static ushort NARC_GetFileCount(NARC narc)
    {
        return narc.NumFiles;
    }
}
