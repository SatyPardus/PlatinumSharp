using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class Party : ISaveBlock
{
    public const int Size = 0x590;
    public const int MaxSize = 6;

    private const int MonsOffset = 8;
    private const int MonDataSpecies = 5;
    private const int MonDataSpeciesExists = 0xAC;

    public int Capacity;
    public int Count;
    public readonly Pokemon[] Mons = new Pokemon[MaxSize];

    public Party()
    {
        for (int i = 0; i < MaxSize; i++)
        {
            Mons[i] = new Pokemon();
        }
    }

    public void WriteTo(byte[] body, int offset)
    {
        Bin.W32(body, offset, (uint)Capacity);
        Bin.W32(body, offset + 4, (uint)Count);
        for (int i = 0; i < MaxSize; i++)
        {
            Array.Copy(Mons[i].Data, 0, body, offset + MonsOffset + i * Pokemon.Size, Pokemon.Size);
        }
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Capacity = (int)Bin.U32(body, offset);
        Count = (int)Bin.U32(body, offset + 4);
        for (int i = 0; i < MaxSize; i++)
        {
            Array.Copy(body, offset + MonsOffset + i * Pokemon.Size, Mons[i].Data, 0, Pokemon.Size);
        }
    }

    private static void CopyMon(Pokemon source, Pokemon destination)
    {
        Array.Copy(source.Buffer, source.Offset, destination.Buffer, destination.Offset, Pokemon.Size);
    }

    private static void CheckSlot(Party party, int slot)
    {
        if (slot < 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (slot >= party.Count)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (slot >= party.Capacity)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02079FF4
    public static Party Party_New(int heapID)
    {
        Party party = new();
        Party_Init(party);
        return party;
    }

    // OFFSET 0x0207A008
    public static void Party_Init(Party party)
    {
        Party_InitWithCapacity(party, MaxSize);
    }

    // OFFSET 0x0207A014
    public static void Party_InitWithCapacity(Party party, int capacity)
    {
        if (capacity > MaxSize)
        {
            ErrorHandling.GF_AssertFail();
        }
        party.Count = 0;
        party.Capacity = capacity;
        for (int i = 0; i < MaxSize; i++)
        {
            Pokemon.Pokemon_Init(party.Mons[i]);
        }
    }

    // OFFSET 0x0207A048
    public static bool Party_AddPokemon(Party party, Pokemon mon)
    {
        if (party.Count >= party.Capacity)
        {
            return false;
        }
        CopyMon(mon, party.Mons[party.Count]);
        party.Count++;
        return true;
    }

    // OFFSET 0x0207A080
    public static bool Party_RemovePokemonBySlotIndex(Party party, int slot)
    {
        CheckSlot(party, slot);
        if (party.Count <= 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        int i;
        for (i = slot; i < party.Count - 1; i++)
        {
            CopyMon(party.Mons[i + 1], party.Mons[i]);
        }
        Pokemon.Pokemon_Init(party.Mons[i]);
        party.Count--;
        return true;
    }

    // OFFSET 0x0207A0F4
    public static int Party_GetCapacity(Party party)
    {
        return party.Capacity;
    }

    // OFFSET 0x0207A0F8
    public static int Party_GetCurrentCount(Party party)
    {
        return party.Count;
    }

    // OFFSET 0x0207A0FC
    public static Pokemon Party_GetPokemonBySlotIndex(Party party, int slot)
    {
        CheckSlot(party, slot);
        return party.Mons[slot];
    }

    // OFFSET 0x0207A128
    public static void Party_AddPokemonBySlotIndex(Party party, int slot, Pokemon mon)
    {
        CheckSlot(party, slot);
        int change = (int)Pokemon.Pokemon_GetValue(party.Mons[slot], MonDataSpeciesExists, null);
        change -= (int)Pokemon.Pokemon_GetValue(mon, MonDataSpeciesExists, null);
        CopyMon(mon, party.Mons[slot]);
        party.Count += change;
    }

    // OFFSET 0x0207A184
    public static bool Party_SwapSlots(Party party, int slotA, int slotB)
    {
        CheckSlot(party, slotA);
        CheckSlot(party, slotB);
        Pokemon held = new();
        CopyMon(party.Mons[slotA], held);
        CopyMon(party.Mons[slotB], party.Mons[slotA]);
        CopyMon(held, party.Mons[slotB]);
        return false;
    }

    // OFFSET 0x0207A21C
    public static void Party_Copy(Party source, Party destination)
    {
        destination.Capacity = source.Capacity;
        destination.Count = source.Count;
        for (int i = 0; i < MaxSize; i++)
        {
            CopyMon(source.Mons[i], destination.Mons[i]);
        }
    }

    // OFFSET 0x0207A230
    public static bool Party_HasSpecies(Party party, int species)
    {
        int i;
        for (i = 0; i < party.Count; i++)
        {
            if (species == Pokemon.Pokemon_GetValue(party.Mons[i], MonDataSpecies, null))
            {
                break;
            }
        }
        return i != party.Count;
    }

    // OFFSET 0x0207A268
    public static Party SaveData_GetParty(SaveData saveData)
    {
        return (Party)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryParty);
    }
}

public static class DexMode
{
    public const int Local = 0;
    public const int National = 1;

    // OFFSET 0x0207A274
    public static int SaveData_GetDexMode(SaveData saveData)
    {
        return Pokedex_GetDexMode(Pokedex.SaveData_GetPokedex(saveData));
    }

    // OFFSET 0x0207A280
    public static int Pokedex_GetDexMode(Pokedex pokedex)
    {
        if (Pokedex.Pokedex_IsNationalDexObtained(pokedex))
        {
            return National;
        }
        return Local;
    }

    // OFFSET 0x0207A294
    public static int Pokemon_GetDexNumber(int dexMode, int species)
    {
        if (dexMode == Local)
        {
            species = Pokemon.Species_GetBabySpecies((ushort)species);
        }
        return species;
    }
}
