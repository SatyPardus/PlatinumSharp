using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed class Pgt
{
    public const int Size = 0x104;
    public const int DataSize = 0x100;

    private const int DataOffset = 4;

    public ushort Type;
    public ushort Flags;
    public readonly byte[] Data = new byte[DataSize];

    public void WriteTo(byte[] body, int offset)
    {
        Bin.W16(body, offset, Type);
        Bin.W16(body, offset + 2, Flags);
        Array.Copy(Data, 0, body, offset + DataOffset, DataSize);
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Type = (ushort)Bin.U16(body, offset);
        Flags = (ushort)Bin.U16(body, offset + 2);
        Array.Copy(body, offset + DataOffset, Data, 0, DataSize);
    }

    public void CopyFrom(Pgt source)
    {
        Type = source.Type;
        Flags = source.Flags;
        Array.Copy(source.Data, Data, DataSize);
    }
}

public sealed class WonderCard
{
    public const int Size = 0x358;
    public const int ExtraSize = Size - Pgt.Size;

    private const int IDOffset = 0x150 - Pgt.Size;
    private const int FlagsOffset = 0x152 - Pgt.Size;
    private const byte SavePgtFlag = 8;

    public readonly Pgt Pgt = new();
    public readonly byte[] Extra = new byte[ExtraSize];

    public ushort ID => (ushort)Bin.U16(Extra, IDOffset);

    public bool SavesPgt => (Extra[FlagsOffset] & SavePgtFlag) != 0;

    public void WriteTo(byte[] body, int offset)
    {
        Pgt.WriteTo(body, offset);
        Array.Copy(Extra, 0, body, offset + Pgt.Size, ExtraSize);
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Pgt.ReadFrom(body, offset);
        Array.Copy(body, offset + Pgt.Size, Extra, 0, ExtraSize);
    }

    public void CopyFrom(WonderCard source)
    {
        Pgt.CopyFrom(source.Pgt);
        Array.Copy(source.Extra, Extra, ExtraSize);
    }
}

public sealed class MysteryGift : ISaveBlock
{
    public const int Size = 0x1328;
    public const int PgtSlotCount = 8;
    public const int WonderCardSlotCount = 3;
    public const int WonderCardFlagCount = 2048;
    public const int MysteryGiftUnlockedFlag = WonderCardFlagCount - 1;
    public const int GiftTypeCount = 14;
    public const int NoSlot = -1;

    private const int ReceivedBytes = 0x100;
    private const int PgtsOffset = 0x100;
    private const int WonderCardsOffset = 0x920;
    private const ushort WonderCardSlotMask = 3;

    public readonly byte[] Received = new byte[ReceivedBytes];
    public readonly Pgt[] Pgts = new Pgt[PgtSlotCount];
    public readonly WonderCard[] WonderCards = new WonderCard[WonderCardSlotCount];

    private static MysteryGift? sMysteryGift;

    public MysteryGift()
    {
        for (int i = 0; i < PgtSlotCount; i++)
        {
            Pgts[i] = new Pgt();
        }
        for (int i = 0; i < WonderCardSlotCount; i++)
        {
            WonderCards[i] = new WonderCard();
        }
    }

    public void WriteTo(byte[] body, int offset)
    {
        Array.Copy(Received, 0, body, offset, ReceivedBytes);
        for (int i = 0; i < PgtSlotCount; i++)
        {
            Pgts[i].WriteTo(body, offset + PgtsOffset + i * Pgt.Size);
        }
        for (int i = 0; i < WonderCardSlotCount; i++)
        {
            WonderCards[i].WriteTo(body, offset + WonderCardsOffset + i * WonderCard.Size);
        }
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Array.Copy(body, offset, Received, 0, ReceivedBytes);
        for (int i = 0; i < PgtSlotCount; i++)
        {
            Pgts[i].ReadFrom(body, offset + PgtsOffset + i * Pgt.Size);
        }
        for (int i = 0; i < WonderCardSlotCount; i++)
        {
            WonderCards[i].ReadFrom(body, offset + WonderCardsOffset + i * WonderCard.Size);
        }
    }

    // OFFSET 0x0202DABC
    public static void MysteryGift_Init(MysteryGift mysteryGift)
    {
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryMysteryGift);
    }

    // OFFSET 0x0202DAC8
    private static bool MysteryGift_IsValidGiftType(int type)
    {
        if (type != 0 && (uint)type < GiftTypeCount)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0202DAD8
    private static Pgt? MysteryGift_TryGetPgt(MysteryGift mysteryGift, int pgtSlot)
    {
        if (pgtSlot >= 0 && pgtSlot < PgtSlotCount)
        {
            if (MysteryGift_IsValidGiftType(mysteryGift.Pgts[pgtSlot].Type))
            {
                return mysteryGift.Pgts[pgtSlot];
            }
        }
        return null;
    }

    // OFFSET 0x0202DB00
    public static WonderCard? MysteryGift_TryGetWonderCard(MysteryGift mysteryGift, int wonderCardSlot)
    {
        if (wonderCardSlot >= 0 && wonderCardSlot < WonderCardSlotCount)
        {
            if (MysteryGift_IsValidGiftType(mysteryGift.WonderCards[wonderCardSlot].Pgt.Type))
            {
                return mysteryGift.WonderCards[wonderCardSlot];
            }
        }
        return null;
    }

    // OFFSET 0x0202DB2C
    public static bool MysteryGift_TrySavePgt(MysteryGift mysteryGift, Pgt pgt, int wonderCardSlot)
    {
        bool saved = false;
        if (!MysteryGift_CheckFreePgtSlotExists(mysteryGift))
        {
            return false;
        }
        for (int i = 0; i < PgtSlotCount; i++)
        {
            if (!MysteryGift_IsValidGiftType(mysteryGift.Pgts[i].Type))
            {
                mysteryGift.Pgts[i].CopyFrom(pgt);
                mysteryGift.Pgts[i].Flags = (ushort)((mysteryGift.Pgts[i].Flags & ~WonderCardSlotMask) | ((ushort)wonderCardSlot & WonderCardSlotMask));
                saved = true;
                break;
            }
        }
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryMysteryGift);
        return saved;
    }

    // OFFSET 0x0202DBAC
    public static bool MysteryGift_TrySaveWondercard(MysteryGift mysteryGift, WonderCard wonderCard)
    {
        bool saved = false;
        if (!MysteryGift_CheckFreeWcSlotExists(mysteryGift))
        {
            return false;
        }
        if (wonderCard.SavesPgt && !MysteryGift_CheckFreePgtSlotExists(mysteryGift))
        {
            return false;
        }
        for (int i = 0; i < WonderCardSlotCount; i++)
        {
            if (!MysteryGift_IsValidGiftType(mysteryGift.WonderCards[i].Pgt.Type))
            {
                mysteryGift.WonderCards[i].CopyFrom(wonderCard);
                if (wonderCard.SavesPgt)
                {
                    MysteryGift_TrySavePgt(mysteryGift, wonderCard.Pgt, i);
                }
                saved = true;
                break;
            }
        }
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryMysteryGift);
        return saved;
    }

    // OFFSET 0x0202DC48
    public static bool MysteryGift_FreePgtSlotIn(MysteryGift mysteryGift, int pgtSlot)
    {
        if (pgtSlot >= PgtSlotCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        mysteryGift.Pgts[pgtSlot].Type = 0;
        mysteryGift.Pgts[pgtSlot].Flags = (ushort)(mysteryGift.Pgts[pgtSlot].Flags & ~WonderCardSlotMask);
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryMysteryGift);
        return true;
    }

    // OFFSET 0x0202DC7C
    public static bool MysteryGift_FreeWcErasePgt(MysteryGift mysteryGift, int wonderCardSlot)
    {
        if (wonderCardSlot >= WonderCardSlotCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        mysteryGift.WonderCards[wonderCardSlot].Pgt.Type = 0;
        MysteryGift_ClearWcIDReceived(mysteryGift, mysteryGift.WonderCards[wonderCardSlot].ID);
        MysteryGift_FreePgtOfWonderCard(mysteryGift, wonderCardSlot);
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryMysteryGift);
        return true;
    }

    // OFFSET 0x0202DCB8
    public static bool MysteryGift_FreeWcSlot(MysteryGift mysteryGift, int wonderCardSlot)
    {
        if (wonderCardSlot >= WonderCardSlotCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        mysteryGift.WonderCards[wonderCardSlot].Pgt.Type = 0;
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryMysteryGift);
        return true;
    }

    // OFFSET 0x0202DCE0
    public static bool MysteryGift_CheckFreePgtSlotExists(MysteryGift mysteryGift)
    {
        for (int i = 0; i < PgtSlotCount; i++)
        {
            if (!MysteryGift_IsValidGiftType(mysteryGift.Pgts[i].Type))
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x0202DD08
    public static bool MysteryGift_CheckFreeWcSlotExists(MysteryGift mysteryGift)
    {
        for (int i = 0; i < WonderCardSlotCount; i++)
        {
            if (!MysteryGift_IsValidGiftType(mysteryGift.WonderCards[i].Pgt.Type))
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x0202DD30
    public static bool MysteryGift_CheckIsPgtSlotOccupied(MysteryGift mysteryGift, int pgtSlot)
    {
        if (pgtSlot >= PgtSlotCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (MysteryGift_IsValidGiftType(mysteryGift.Pgts[pgtSlot].Type))
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0202DD5C
    public static bool MysteryGift_CheckIsWcSlotOccupied(MysteryGift mysteryGift, int wonderCardSlot)
    {
        if (wonderCardSlot >= WonderCardSlotCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (MysteryGift_IsValidGiftType(mysteryGift.WonderCards[wonderCardSlot].Pgt.Type))
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0202DD88
    public static bool MysteryGift_CheckHasWonderCards(MysteryGift mysteryGift)
    {
        for (int i = 0; i < WonderCardSlotCount; i++)
        {
            if (MysteryGift_CheckIsWcSlotOccupied(mysteryGift, i))
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x0202DDA8
    public static bool MysteryGift_CheckWcHasPgtSaved(MysteryGift mysteryGift, int wonderCardSlot)
    {
        for (int i = 0; i < PgtSlotCount; i++)
        {
            if (MysteryGift_IsValidGiftType(mysteryGift.Pgts[i].Type) && (mysteryGift.Pgts[i].Flags & WonderCardSlotMask) == wonderCardSlot)
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x0202DDE8
    private static void MysteryGift_FreePgtOfWonderCard(MysteryGift mysteryGift, int wonderCardSlot)
    {
        for (int i = 0; i < PgtSlotCount; i++)
        {
            if (MysteryGift_IsValidGiftType(mysteryGift.Pgts[i].Type) && (mysteryGift.Pgts[i].Flags & WonderCardSlotMask) == wonderCardSlot)
            {
                MysteryGift_FreePgtSlotIn(mysteryGift, i);
                return;
            }
        }
    }

    // OFFSET 0x0202DE2C
    public static bool MysteryGift_GetWcIDReceived(MysteryGift mysteryGift, int wonderCardID)
    {
        if (wonderCardID >= WonderCardFlagCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if ((mysteryGift.Received[wonderCardID / 8] & (1 << (wonderCardID & 7))) != 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0202DE5C
    public static void MysteryGift_SetWcIDReceived(MysteryGift mysteryGift, int wonderCardID)
    {
        if (wonderCardID >= WonderCardFlagCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        mysteryGift.Received[wonderCardID / 8] |= (byte)(1 << (wonderCardID & 7));
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryMysteryGift);
    }

    // OFFSET 0x0202DE8C
    private static void MysteryGift_ClearWcIDReceived(MysteryGift mysteryGift, int wonderCardID)
    {
        byte mask = (byte)~(1 << (wonderCardID & 7));
        if (wonderCardID >= WonderCardFlagCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        mysteryGift.Received[wonderCardID / 8] &= mask;
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryMysteryGift);
    }

    // OFFSET 0x0202DEC4
    public static bool MysteryGift_GetMysteryGiftUnlockedFlag(MysteryGift mysteryGift)
    {
        return MysteryGift_GetWcIDReceived(mysteryGift, MysteryGiftUnlockedFlag);
    }

    // OFFSET 0x0202DED4
    public static void MysteryGift_SetMysteryGiftUnlockedFlag(MysteryGift mysteryGift)
    {
        MysteryGift_SetWcIDReceived(mysteryGift, MysteryGiftUnlockedFlag);
    }

    // OFFSET 0x0202DEE4
    public static void MysteryGift_Load(SaveData saveData, int unused)
    {
        if (sMysteryGift == null)
        {
            sMysteryGift = (MysteryGift)SaveDataTable.SaveData_GetMysteryGift(saveData);
        }
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryMysteryGift);
    }

    // OFFSET 0x0202DF04
    public static void MysteryGift_Unload(SaveData saveData, int unused)
    {
        if (sMysteryGift != null)
        {
            sMysteryGift = null;
        }
    }

    // OFFSET 0x0202DF18
    public static int MysteryGift_TryGetFirstValidPgtSlot()
    {
        for (int i = 0; i < PgtSlotCount; i++)
        {
            if (MysteryGift_CheckIsPgtSlotOccupied(sMysteryGift!, i))
            {
                return i;
            }
        }
        return NoSlot;
    }

    // OFFSET 0x0202DF40
    public static int MysteryGift_TryGetPgtType(int pgtSlot)
    {
        Pgt? pgt = MysteryGift_TryGetPgt(sMysteryGift!, pgtSlot);
        if (pgt != null)
        {
            return pgt.Type;
        }
        return 0;
    }

    // OFFSET 0x0202DF5C
    public static byte[]? MysteryGift_TryGetPgtData(int pgtSlot)
    {
        Pgt? pgt = MysteryGift_TryGetPgt(sMysteryGift!, pgtSlot);
        if (pgt != null)
        {
            return pgt.Data;
        }
        return null;
    }

    // OFFSET 0x0202DF78
    public static void MysteryGift_FreePgtSlot(int pgtSlot)
    {
        MysteryGift_FreePgtSlotIn(sMysteryGift!, pgtSlot);
    }
}
