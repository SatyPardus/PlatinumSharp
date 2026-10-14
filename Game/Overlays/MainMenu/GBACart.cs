using System.Buffers.Binary;

namespace PlatinumSharp.Game;

public static class GBACart
{
    private const int Overlay = 97;
    private const uint CartInfoAddress = 0x0223D92C;
    private const int CartInfoCount = 0x1E;
    private const int CartInfoSize = 8;
    private const uint LogoAddress = 0x0223EC04;
    private const int LogoLength = 0x9C;
    private const uint RomHeaderAddress = 0x08000000;
    private const int RomHeaderLength = 0xC0;
    private const uint GameHeaderAddress = 0x08000100;
    private const int GameHeaderLength = 0xFC;
    private const int HeaderFlagsOffset = 0x50;
    private const int HeaderSaveBlock2Size = 0x88;
    private const int HeaderSaveBlock1Size = 0x8C;
    private const int HeaderWarpFlagsOffset = 0x98;
    private const int BackupTypeFlash1M = 1;
    private const int NoCartInfo = -1;

    private static readonly byte[] sGBAPokemonGameHeader = new byte[GameHeaderLength];
    private static int sLoadedGBACartInfo = NoCartInfo;
    private static object? sGBASaveSlot;

    // OFFSET 0x02235D18
    private static bool GBACart_HasFlashBackup()
    {
        if (Cartridge.IdentifyAgbBackup(BackupTypeFlash1M) != 0)
        {
            return false;
        }
        return true;
    }

    // OFFSET 0x02235D2C
    public static int GBACart_Load(object? saveSlot)
    {
        sGBASaveSlot = saveSlot;
        if (sLoadedGBACartInfo != NoCartInfo)
        {
            return 0xC;
        }
        int result = GBACart_Identify(CartInfoAddress, CartInfoCount);
        if (result != 0)
        {
            return result;
        }
        if (CartInfo(sLoadedGBACartInfo)[4] == 0)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(sGBAPokemonGameHeader.AsSpan(HeaderSaveBlock2Size), 0x890);
            BinaryPrimitives.WriteUInt32LittleEndian(sGBAPokemonGameHeader.AsSpan(HeaderSaveBlock1Size), 0x3AC0);
            BinaryPrimitives.WriteUInt32LittleEndian(sGBAPokemonGameHeader.AsSpan(HeaderFlagsOffset), 0x1220);
            BinaryPrimitives.WriteUInt32LittleEndian(sGBAPokemonGameHeader.AsSpan(HeaderWarpFlagsOffset), 9);
        }
        else
        {
            Cartridge.Read(GameHeaderAddress, sGBAPokemonGameHeader, GameHeaderLength);
        }
        if (!GBACart_HasFlashBackup())
        {
            return 3;
        }
        return 0;
    }

    // OFFSET 0x02235DB0
    public static int GBACart_GetAGBGameType()
    {
        return CartInfo(sLoadedGBACartInfo)[5];
    }

    // OFFSET 0x02235DBC
    public static int GBACart_GetLanguage()
    {
        return BinaryPrimitives.ReadUInt16LittleEndian(CartInfo(sLoadedGBACartInfo).AsSpan(6));
    }

    // OFFSET 0x02235DC8
    private static int GBACart_Identify(uint cartInfoAddress, int count)
    {
        sLoadedGBACartInfo = NoCartInfo;
        if (!Cartridge.IsAgbCartridge())
        {
            return 1;
        }
        uint gameCode = Cartridge.GetAgbGameCode();
        uint[] cartInfo = RomData.OverlayU32(Overlay, cartInfoAddress, count * 2);
        for (int i = 0; i < count; i++)
        {
            if (gameCode == cartInfo[i * 2])
            {
                sLoadedGBACartInfo = i;
                break;
            }
        }
        if (sLoadedGBACartInfo == NoCartInfo)
        {
            return 2;
        }
        Cartridge.Enable(true);
        byte[] header = new byte[RomHeaderLength];
        Cartridge.Read(RomHeaderAddress, header, RomHeaderLength);
        byte[] logo = RomData.Overlay(Overlay, LogoAddress, LogoLength);
        for (int i = 0; i < LogoLength; i++)
        {
            if (logo[i] != header[4 + i])
            {
                return 4;
            }
        }
        byte sum = 0;
        for (int i = 0xA0; i <= 0xBC; i++)
        {
            sum += header[i];
        }
        if (header[0xBD] != (byte)-(sum + 0x19))
        {
            return 4;
        }
        return 0;
    }

    private static byte[] CartInfo(int index)
    {
        return RomData.Overlay(Overlay, CartInfoAddress + (uint)(index * CartInfoSize), CartInfoSize);
    }
}
