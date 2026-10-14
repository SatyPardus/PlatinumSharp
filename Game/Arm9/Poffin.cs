namespace PlatinumSharp.Game;

public sealed class Poffin
{
    public const int Size = 8;
    public const int FlavorCount = 5;
    public const int AttributeCount = 7;
    public const int AttributeType = 0;
    public const int AttributeSpiciness = 1;
    public const int AttributeDryness = 2;
    public const int AttributeSweetness = 3;
    public const int AttributeBitterness = 4;
    public const int AttributeSourness = 5;
    public const int AttributeSmoothness = 6;
    public const byte TypeRich = 25;
    public const byte TypeOverripe = 26;
    public const byte TypeFoul = 27;
    public const byte TypeMild = 28;
    public const byte TypeNone = 30;

    private const int FoulFlavorCount = 3;
    private const byte FoulFlavorValue = 2;
    private const int MildFlavorThreshold = 50;
    private const int MaxLevel = 99;

    public byte Type;
    public readonly byte[] Flavors = new byte[FlavorCount];
    public byte Smoothness;
    public byte Padding;

    public void WriteTo(byte[] body, int offset)
    {
        body[offset] = Type;
        Array.Copy(Flavors, 0, body, offset + 1, FlavorCount);
        body[offset + 6] = Smoothness;
        body[offset + 7] = Padding;
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Type = body[offset];
        Array.Copy(body, offset + 1, Flavors, 0, FlavorCount);
        Smoothness = body[offset + 6];
        Padding = body[offset + 7];
    }

    // OFFSET 0x0202A910
    public static int Poffin_SizeOf()
    {
        return Size;
    }

    // OFFSET 0x0202A914
    public static bool Poffin_HasValidFlavor(Poffin poffin)
    {
        if (poffin.Type != TypeNone)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0202A924
    public static void Poffin_Clear(Poffin poffin)
    {
        poffin.Type = TypeNone;
        Array.Clear(poffin.Flavors);
        poffin.Smoothness = 0;
        poffin.Padding = 0;
    }

    // OFFSET 0x0202A93C
    public static Poffin Poffin_New(int heapID)
    {
        Poffin poffin = new();
        Poffin_Clear(poffin);
        return poffin;
    }

    // OFFSET 0x0202A950
    public static void Poffin_Copy(Poffin source, Poffin destination)
    {
        destination.Type = source.Type;
        Array.Copy(source.Flavors, destination.Flavors, FlavorCount);
        destination.Smoothness = source.Smoothness;
        destination.Padding = source.Padding;
    }

    // OFFSET 0x0202A974
    public static byte Poffin_GetAttribute(Poffin poffin, int attribute)
    {
        switch (attribute)
        {
            case AttributeType:
                return poffin.Type;
            case AttributeSpiciness:
            case AttributeDryness:
            case AttributeSweetness:
            case AttributeBitterness:
            case AttributeSourness:
                return poffin.Flavors[attribute - 1];
            default:
                return poffin.Smoothness;
        }
    }

    // OFFSET 0x0202A9B0
    private static void Poffin_MakeFoul(Poffin poffin, byte smoothness)
    {
        int count = 0;
        do
        {
            byte flavor = (byte)(MathUtil.LCRNG_Next() % FlavorCount);
            if (poffin.Flavors[flavor] == 0)
            {
                poffin.Flavors[flavor] = FoulFlavorValue;
                count++;
            }
        }
        while (count < FoulFlavorCount);
        poffin.Type = TypeFoul;
        poffin.Smoothness = smoothness;
    }

    // OFFSET 0x0202A9E4
    public static int Poffin_MakePoffin(Poffin poffin, byte[] flavors, byte smoothness, bool isFoul)
    {
        int count = 0;
        bool isMild = false;
        byte type = TypeFoul;
        byte[] present = new byte[FlavorCount];
        if (isFoul)
        {
            Poffin_MakeFoul(poffin, smoothness);
            return type;
        }
        for (int i = 0; i < FlavorCount; i++)
        {
            if (flavors[i] == 0)
            {
                continue;
            }
            if (flavors[i] >= MildFlavorThreshold)
            {
                isMild = true;
            }
            present[count] = (byte)i;
            count++;
        }
        switch (count)
        {
            case 0:
                Poffin_MakeFoul(poffin, smoothness);
                return TypeFoul;
            case 1:
                type = (byte)(present[0] * 6);
                break;
            case 2:
                if (flavors[present[0]] >= flavors[present[1]])
                {
                    type = (byte)(present[0] * FlavorCount + present[1]);
                }
                else
                {
                    type = (byte)(present[1] * FlavorCount + present[0]);
                }
                break;
            case 3:
                type = TypeRich;
                break;
            case 4:
            case 5:
                type = TypeOverripe;
                break;
        }
        if (isMild)
        {
            type = TypeMild;
        }
        for (int i = 0; i < FlavorCount; i++)
        {
            poffin.Flavors[i] = flavors[i];
        }
        poffin.Type = type;
        poffin.Smoothness = smoothness;
        return type;
    }

    // OFFSET 0x0202AA9C
    public static void Poffin_StoreAttributesToArray(Poffin poffin, byte[] destination)
    {
        destination[0] = poffin.Type;
        for (int i = 0; i < FlavorCount; i++)
        {
            destination[i + 1] = poffin.Flavors[i];
        }
        destination[6] = poffin.Smoothness;
    }

    // OFFSET 0x0202AABC
    public static byte Poffin_CalcLevel(Poffin poffin)
    {
        byte level;
        byte mainFlavor = (byte)(poffin.Type / FlavorCount);
        if (mainFlavor < FlavorCount)
        {
            level = poffin.Flavors[mainFlavor];
        }
        else
        {
            level = poffin.Flavors[0];
            for (int i = 1; i < FlavorCount; i++)
            {
                if (level < poffin.Flavors[i])
                {
                    level = poffin.Flavors[i];
                }
            }
        }
        if (level > MaxLevel)
        {
            level = MaxLevel;
        }
        return level;
    }
}

public sealed class PoffinCase : ISaveBlock
{
    public const int MaxPoffins = 100;
    public const ushort NoSlot = 0xFFFF;

    public readonly Poffin[] Poffins = new Poffin[MaxPoffins];

    public PoffinCase()
    {
        for (int i = 0; i < MaxPoffins; i++)
        {
            Poffins[i] = new Poffin();
        }
    }

    public void WriteTo(byte[] body, int offset)
    {
        for (int i = 0; i < MaxPoffins; i++)
        {
            Poffins[i].WriteTo(body, offset + i * Poffin.Size);
        }
    }

    public void ReadFrom(byte[] body, int offset)
    {
        for (int i = 0; i < MaxPoffins; i++)
        {
            Poffins[i].ReadFrom(body, offset + i * Poffin.Size);
        }
    }

    // OFFSET 0x0202AB28
    public static PoffinCase SaveData_GetPoffinCase(SaveData saveData)
    {
        return (PoffinCase)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryPoffins);
    }

    // OFFSET 0x0202AB3C
    public static void PoffinCase_Init(PoffinCase poffinCase)
    {
        for (int i = 0; i < MaxPoffins; i++)
        {
            Poffin.Poffin_Clear(poffinCase.Poffins[i]);
        }
    }

    // OFFSET 0x0202AB54
    public static ushort PoffinCase_GetEmptySlot(PoffinCase poffinCase)
    {
        for (ushort i = 0; i < MaxPoffins; i++)
        {
            if (poffinCase.Poffins[i].Type == Poffin.TypeNone)
            {
                return i;
            }
        }
        return NoSlot;
    }

    // OFFSET 0x0202AB74
    public static ushort PoffinCase_AddPoffin(PoffinCase poffinCase, Poffin poffin)
    {
        ushort slot = PoffinCase_GetEmptySlot(poffinCase);
        if (slot == NoSlot)
        {
            return NoSlot;
        }
        Poffin.Poffin_Copy(poffin, poffinCase.Poffins[slot]);
        return slot;
    }

    // OFFSET 0x0202AB98
    public static bool PoffinCase_ClearSlot(PoffinCase poffinCase, ushort slot)
    {
        if (slot >= MaxPoffins)
        {
            return false;
        }
        Poffin.Poffin_Clear(poffinCase.Poffins[slot]);
        return true;
    }

    // OFFSET 0x0202ABB0
    private static ushort PoffinCase_FindNextFilledSlot(PoffinCase poffinCase, ushort start)
    {
        for (ushort i = start; i < MaxPoffins; i++)
        {
            if (poffinCase.Poffins[i].Type != Poffin.TypeNone)
            {
                return i;
            }
        }
        return NoSlot;
    }

    // OFFSET 0x0202ABD4
    public static void PoffinCase_Compact(PoffinCase poffinCase)
    {
        ushort limit = MaxPoffins;
        for (ushort i = 0; i < limit; i++)
        {
            if (poffinCase.Poffins[i].Type != Poffin.TypeNone)
            {
                continue;
            }
            ushort destination = i;
            ushort source = PoffinCase_FindNextFilledSlot(poffinCase, i);
            if (source == NoSlot)
            {
                return;
            }
            ushort gap = (ushort)(source - i);
            while (source < limit)
            {
                Poffin.Poffin_Copy(poffinCase.Poffins[source], poffinCase.Poffins[destination]);
                Poffin.Poffin_Clear(poffinCase.Poffins[source]);
                destination++;
                source++;
            }
            limit = (ushort)(limit - gap);
        }
    }

    // OFFSET 0x0202AC54
    public static void PoffinCase_CopyPoffinFromSlot(PoffinCase poffinCase, ushort slot, Poffin destination)
    {
        if (slot >= MaxPoffins)
        {
            Poffin.Poffin_Clear(destination);
            return;
        }
        Poffin.Poffin_Copy(poffinCase.Poffins[slot], destination);
    }

    // OFFSET 0x0202AC70
    public static Poffin? PoffinCase_AllocateForSlot(PoffinCase poffinCase, ushort slot, int heapID)
    {
        Poffin poffin = Poffin.Poffin_New(heapID);
        if (slot >= MaxPoffins)
        {
            Poffin.Poffin_Clear(poffin);
            return null;
        }
        Poffin.Poffin_Copy(poffinCase.Poffins[slot], poffin);
        return poffin;
    }

    // OFFSET 0x0202AC98
    public static ushort PoffinCase_CountFilledSlots(PoffinCase poffinCase)
    {
        ushort count = 0;
        for (ushort i = 0; i < MaxPoffins; i++)
        {
            if (Poffin.Poffin_HasValidFlavor(poffinCase.Poffins[i]))
            {
                count++;
            }
        }
        return count;
    }

    // OFFSET 0x0202ACC0
    public static ushort PoffinCase_CountEmptySlots(PoffinCase poffinCase)
    {
        ushort count = 0;
        for (ushort i = 0; i < MaxPoffins; i++)
        {
            if (poffinCase.Poffins[i].Type == Poffin.TypeNone)
            {
                count++;
            }
        }
        return count;
    }
}
