using System.Buffers.Binary;

namespace PlatinumSharp.Game;

public static class DistributionCartridge
{
    private const int Overlay = 97;
    private const uint GameCodesAddress = 0x0223EF9C;
    private const uint PublicKeyAddress = 0x0223DA1C;
    private const uint DataAddress = 0x08100000;
    private const uint SignatureAddress = 0x08020000;
    private const int DataLength = 0x4A8;
    private const int SignatureLength = 0x80;
    private const ushort NintendoMakerCode = 0x3130;

    private static int sHeapID;

    // OFFSET 0x02237520
    public static void DistributionCartridge_UseHeap(int heapID)
    {
        sHeapID = heapID;
    }

    // OFFSET 0x0223752C
    private static byte[] DistributionCartridge_Alloc(uint size)
    {
        return Heap.Heap_Alloc(sHeapID, size)!;
    }

    // OFFSET 0x02237540
    private static bool DistributionCartridge_VerifySignature()
    {
        byte[] signature = new byte[SignatureLength];
        byte[] data = new byte[DataLength];
        uint gameCode = BinaryPrimitives.ReverseEndianness(Cartridge.GetAgbGameCode());
        uint index = 0;
        uint count = 0;
        if (GameCodeAt(0) == 0)
        {
            return false;
        }
        do
        {
            if (gameCode == GameCodeAt(index))
            {
                Cartridge.Enable(true);
                Cartridge.Read(DataAddress, data, DataLength);
                Cartridge.Read(SignatureAddress, signature, SignatureLength);
                Cartridge.Enable(false);
                Unported.ov97_0223D5C8(DistributionCartridge_Alloc, Heap.Heap_Free);
                if (Unported.ov97_02239498(data, DataLength, signature, PublicKeyAddress) != 0)
                {
                    return true;
                }
            }
            index++;
            count++;
        }
        while (count < GameCodeAt(index));
        return false;
    }

    // OFFSET 0x022375F0
    private static bool DistributionCartridge_IsValid()
    {
        Cartridge.Init();
        if (!Cartridge.IsAgbCartridge())
        {
            return false;
        }
        if (Cartridge.GetAgbMakerCode() != NintendoMakerCode)
        {
            return false;
        }
        if (DistributionCartridge_VerifySignature())
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02237624
    public static int DistributionCartridge_ReadLength()
    {
        if (!DistributionCartridge_IsValid())
        {
            return 0;
        }
        Cartridge.Enable(true);
        Cartridge.Read32(DataAddress, out uint length);
        Cartridge.Enable(false);
        return (int)length;
    }

    private static uint GameCodeAt(uint index)
    {
        return RomData.OverlayU32(Overlay, GameCodesAddress + index * 4, 1)[0];
    }
}
