using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class PCHallOfFamePokemon
{
    public const int MoveCount = 4;

    public GameString? Nickname;
    public GameString? OTName;
    public uint Personality;
    public uint OTID;
    public ushort Species;
    public byte Level;
    public byte Form;
    public readonly ushort[] Moves = new ushort[MoveCount];
}

public static class HallOfFame
{
    public const int Size = 0x2AB0;
    public const int MaxEntries = 30;
    public const int MaxPokemon = 6;
    public const int MaxTotalEntries = 9999;

    private const int EntrySize = 0x16C;
    private const int PokemonSize = 0x3C;
    private const int EntryPokemonBytes = MaxPokemon * PokemonSize;
    private const int EntryYearOffset = 0x168;
    private const int EntryMonthOffset = 0x16A;
    private const int EntryDayOffset = 0x16B;
    private const int NextIndexOffset = 0x2AA8;
    private const int TotalEntriesOffset = 0x2AAC;
    private const int MonSpeciesOffset = 0;
    private const int MonLevelOffset = 2;
    private const int MonFormOffset = 3;
    private const int MonPersonalityOffset = 4;
    private const int MonOTIDOffset = 8;
    private const int MonNicknameOffset = 0xC;
    private const int MonOTNameOffset = 0x22;
    private const int MonMovesOffset = 0x32;
    private const int NicknameLength = 11;
    private const int OTNameLength = 8;
    private const ushort EndOfString = 0xFFFF;
    private const int MonDataPersonality = 0;
    private const int MonDataSpecies = 5;
    private const int MonDataOTID = 7;
    private const int MonDataMove1 = 0x36;
    private const int MonDataIsEgg = 0x4C;
    private const int MonDataForm = 0x70;
    private const int MonDataNicknameString = 0x77;
    private const int MonDataOTNameString = 0x91;
    private const int MonDataLevel = 0xA1;

    private static void WriteString(byte[] hallOfFame, int offset, GameString text, uint length)
    {
        ushort[] buffer = new ushort[length];
        GameString.String_ToChars(text, buffer, length);
        for (int i = 0; i < length; i++)
        {
            Bin.W16(hallOfFame, offset + i * 2, buffer[i]);
        }
    }

    private static ushort[] ReadString(byte[] hallOfFame, int offset, int length)
    {
        ushort[] buffer = new ushort[length];
        for (int i = 0; i < length; i++)
        {
            buffer[i] = (ushort)Bin.U16(hallOfFame, offset + i * 2);
        }
        return buffer;
    }

    private static int EntryOffset(byte[] hallOfFame, int entryIndex)
    {
        int entry = (int)Bin.U32(hallOfFame, NextIndexOffset) - 1 - entryIndex;
        if (entry < 0)
        {
            entry += MaxEntries;
        }
        return entry * EntrySize;
    }

    private static void CheckState(byte[]? hallOfFame)
    {
        if (hallOfFame == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (Bin.U32(hallOfFame!, NextIndexOffset) >= MaxEntries)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x0202DF94
    public static void HallOfFame_Init(byte[] hallOfFame)
    {
        Array.Clear(hallOfFame, 0, Size);
    }

    // OFFSET 0x0202DFA8
    public static void HallOfFame_AddEntry(byte[] hallOfFame, Party party, in RtcDate date)
    {
        CheckState(hallOfFame);
        if (Bin.U32(hallOfFame, TotalEntriesOffset) >= MaxTotalEntries)
        {
            return;
        }
        int entry = (int)Bin.U32(hallOfFame, NextIndexOffset) * EntrySize;
        int count = Party.Party_GetCurrentCount(party);
        GameString? text = GameString.String_Init(NicknameLength, 0);
        Array.Clear(hallOfFame, entry, EntryPokemonBytes);
        int slot = entry;
        for (int i = 0; i < count; i++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, i);
            int wasDecrypted = Pokemon.Pokemon_EnterDecryptionContext(mon);
            if (Pokemon.Pokemon_GetValue(mon, MonDataIsEgg, null) == 0)
            {
                Bin.W16(hallOfFame, slot + MonSpeciesOffset, (ushort)Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null));
                hallOfFame[slot + MonLevelOffset] = (byte)Pokemon.Pokemon_GetValue(mon, MonDataLevel, null);
                hallOfFame[slot + MonFormOffset] = (byte)Pokemon.Pokemon_GetValue(mon, MonDataForm, null);
                Bin.W32(hallOfFame, slot + MonPersonalityOffset, Pokemon.Pokemon_GetValue(mon, MonDataPersonality, null));
                Bin.W32(hallOfFame, slot + MonOTIDOffset, Pokemon.Pokemon_GetValue(mon, MonDataOTID, null));
                for (int move = 0; move < PCHallOfFamePokemon.MoveCount; move++)
                {
                    Bin.W16(hallOfFame, slot + MonMovesOffset + move * 2, (ushort)Pokemon.Pokemon_GetValue(mon, MonDataMove1 + move, null));
                }
                if (text != null)
                {
                    Pokemon.Pokemon_GetValue(mon, MonDataNicknameString, text);
                    WriteString(hallOfFame, slot + MonNicknameOffset, text, NicknameLength);
                    Pokemon.Pokemon_GetValue(mon, MonDataOTNameString, text);
                    WriteString(hallOfFame, slot + MonOTNameOffset, text, OTNameLength);
                }
                else
                {
                    Bin.W16(hallOfFame, slot + MonNicknameOffset, EndOfString);
                    Bin.W16(hallOfFame, slot + MonOTNameOffset, EndOfString);
                }
                slot += PokemonSize;
            }
            Pokemon.Pokemon_ExitDecryptionContext(mon, wasDecrypted);
        }
        Bin.W16(hallOfFame, entry + EntryYearOffset, (ushort)date.Year);
        hallOfFame[entry + EntryMonthOffset] = (byte)date.Month;
        hallOfFame[entry + EntryDayOffset] = (byte)date.Day;
        uint next = Bin.U32(hallOfFame, NextIndexOffset) + 1;
        if (next >= MaxEntries)
        {
            next = 0;
        }
        Bin.W32(hallOfFame, NextIndexOffset, next);
        Bin.W32(hallOfFame, TotalEntriesOffset, Bin.U32(hallOfFame, TotalEntriesOffset) + 1);
        if (text != null)
        {
            GameString.String_Free(text);
        }
    }

    // OFFSET 0x0202E148
    public static uint HallOfFame_GetStoredEntriesCount(byte[] hallOfFame)
    {
        CheckState(hallOfFame);
        uint total = Bin.U32(hallOfFame, TotalEntriesOffset);
        if (total < MaxEntries)
        {
            return total;
        }
        return MaxEntries;
    }

    // OFFSET 0x0202E174
    public static uint HallOfFame_GetEntryNum(byte[] hallOfFame, int entryIndex)
    {
        CheckState(hallOfFame);
        if (entryIndex >= MaxEntries)
        {
            ErrorHandling.GF_AssertFail();
        }
        return Bin.U32(hallOfFame, TotalEntriesOffset) - (uint)entryIndex;
    }

    // OFFSET 0x0202E1A8
    public static uint HallOfFame_GetEntryPokemonCount(byte[] hallOfFame, int entryIndex)
    {
        CheckState(hallOfFame);
        if (entryIndex >= MaxEntries)
        {
            ErrorHandling.GF_AssertFail();
        }
        int entry = EntryOffset(hallOfFame, entryIndex);
        uint count;
        for (count = 0; count < MaxPokemon; count++)
        {
            if (Bin.U16(hallOfFame, entry + (int)count * PokemonSize + MonSpeciesOffset) == 0)
            {
                break;
            }
        }
        return count;
    }

    // OFFSET 0x0202E1F4
    public static void HallOfFame_GetEntryPokemonData(byte[] hallOfFame, int entryIndex, int pokemonIndex, PCHallOfFamePokemon pokemon)
    {
        CheckState(hallOfFame);
        if (entryIndex >= MaxEntries)
        {
            ErrorHandling.GF_AssertFail();
        }
        int mon = EntryOffset(hallOfFame, entryIndex) + pokemonIndex * PokemonSize;
        pokemon.Species = (ushort)Bin.U16(hallOfFame, mon + MonSpeciesOffset);
        pokemon.Level = hallOfFame[mon + MonLevelOffset];
        pokemon.Personality = Bin.U32(hallOfFame, mon + MonPersonalityOffset);
        pokemon.OTID = Bin.U32(hallOfFame, mon + MonOTIDOffset);
        pokemon.Form = hallOfFame[mon + MonFormOffset];
        GameString.String_CopyChars(pokemon.Nickname, ReadString(hallOfFame, mon + MonNicknameOffset, NicknameLength));
        GameString.String_CopyChars(pokemon.OTName, ReadString(hallOfFame, mon + MonOTNameOffset, OTNameLength));
        for (int move = 0; move < PCHallOfFamePokemon.MoveCount; move++)
        {
            pokemon.Moves[move] = (ushort)Bin.U16(hallOfFame, mon + MonMovesOffset + move * 2);
        }
    }

    // OFFSET 0x0202E274
    public static void HallOfFame_GetEntryDate(byte[] hallOfFame, int entryIndex, out RtcDate date)
    {
        CheckState(hallOfFame);
        if (entryIndex >= MaxEntries)
        {
            ErrorHandling.GF_AssertFail();
        }
        int entry = EntryOffset(hallOfFame, entryIndex);
        date = new RtcDate
        {
            Year = Bin.U16(hallOfFame, entry + EntryYearOffset),
            Month = hallOfFame[entry + EntryMonthOffset],
            Day = hallOfFame[entry + EntryDayOffset],
            Week = 0,
        };
    }
}
