using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class Pokedex : ISaveBlock
{
    public const int Size = 0x324;
    public const int NationalDexCount = 493;
    public const int NationalDexGoal = 482;
    public const int LocalDexGoal = 210;
    public const int UnownFormCount = 28;
    public const int DeoxysFormCount = 4;
    public const int RotomFormCount = 6;
    public const int GenderMale = 0;
    public const int GenderFemale = 1;
    public const int GenderNone = 2;
    public const uint NoForm = 0xFFFFFFFF;

    private const uint MagicNumber = 0xBEEFCAFE;
    private const int DexBytes = 0x40;
    private const int LanguageBytes = 0x1F0;
    private const int SpeciesUnown = 201;
    private const int SpeciesSpinda = 327;
    private const int SpeciesDeoxys = 386;
    private const int SpeciesBurmy = 412;
    private const int SpeciesWormadam = 413;
    private const int SpeciesShellos = 422;
    private const int SpeciesGastrodon = 423;
    private const int SpeciesRotom = 479;
    private const int SpeciesGiratina = 487;
    private const int SpeciesShaymin = 492;
    private const int MonDataPersonality = 0;
    private const int MonDataSpecies = 5;
    private const int MonDataLanguage = 0xC;
    private const int MonDataForm = 0x70;
    private const int SpeciesDataGenderRatio = 0x12;
    private const int GenderRatioNone = 0xFF;
    private const byte UnusedForm = 0xFF;
    private const int DeoxysUnused = 0xF;
    private const int RotomUnused = 7;
    private const int ThreeFormUnused = 3;
    private const int LanguageIndexNone = 6;
    private const int LanguageCount = 8;
    private const uint ExcludedNationalTable = 0x020E5AF4;
    private const int ExcludedNationalCount = 11;
    private const int LastDexWord = 0x3C;

    public uint Magic;
    public readonly byte[] Caught = new byte[DexBytes];
    public readonly byte[] Seen = new byte[DexBytes];
    public readonly byte[][] Genders = { new byte[DexBytes], new byte[DexBytes] };
    public uint SpindaForm;
    public byte ShellosForms;
    public byte GastrodonForms;
    public byte BurmyForms;
    public byte WormadamForms;
    public readonly byte[] UnownForms = new byte[UnownFormCount];
    public readonly byte[] Languages = new byte[LanguageBytes];
    public byte CanDetectForms;
    public byte CanDetectLanguages;
    public byte Obtained;
    public byte NationalDexObtained;
    public uint RotomForms;
    public byte ShayminForms;
    public byte GiratinaForms;

    private static ushort[]? sExcludedNational;

    public void WriteTo(byte[] body, int offset)
    {
        Bin.W32(body, offset, Magic);
        Array.Copy(Caught, 0, body, offset + 4, DexBytes);
        Array.Copy(Seen, 0, body, offset + 0x44, DexBytes);
        Array.Copy(Genders[0], 0, body, offset + 0x84, DexBytes);
        Array.Copy(Genders[1], 0, body, offset + 0xC4, DexBytes);
        Bin.W32(body, offset + 0x104, SpindaForm);
        body[offset + 0x108] = ShellosForms;
        body[offset + 0x109] = GastrodonForms;
        body[offset + 0x10A] = BurmyForms;
        body[offset + 0x10B] = WormadamForms;
        Array.Copy(UnownForms, 0, body, offset + 0x10C, UnownFormCount);
        Array.Copy(Languages, 0, body, offset + 0x128, LanguageBytes);
        body[offset + 0x318] = CanDetectForms;
        body[offset + 0x319] = CanDetectLanguages;
        body[offset + 0x31A] = Obtained;
        body[offset + 0x31B] = NationalDexObtained;
        Bin.W32(body, offset + 0x31C, RotomForms);
        body[offset + 0x320] = ShayminForms;
        body[offset + 0x321] = GiratinaForms;
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Magic = Bin.U32(body, offset);
        Array.Copy(body, offset + 4, Caught, 0, DexBytes);
        Array.Copy(body, offset + 0x44, Seen, 0, DexBytes);
        Array.Copy(body, offset + 0x84, Genders[0], 0, DexBytes);
        Array.Copy(body, offset + 0xC4, Genders[1], 0, DexBytes);
        SpindaForm = Bin.U32(body, offset + 0x104);
        ShellosForms = body[offset + 0x108];
        GastrodonForms = body[offset + 0x109];
        BurmyForms = body[offset + 0x10A];
        WormadamForms = body[offset + 0x10B];
        Array.Copy(body, offset + 0x10C, UnownForms, 0, UnownFormCount);
        Array.Copy(body, offset + 0x128, Languages, 0, LanguageBytes);
        CanDetectForms = body[offset + 0x318];
        CanDetectLanguages = body[offset + 0x319];
        Obtained = body[offset + 0x31A];
        NationalDexObtained = body[offset + 0x31B];
        RotomForms = Bin.U32(body, offset + 0x31C);
        ShayminForms = body[offset + 0x320];
        GiratinaForms = body[offset + 0x321];
    }

    private static void CheckMagic(Pokedex pokedex)
    {
        if (pokedex.Magic != MagicNumber)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    private static bool GetBit(byte[] bits, ushort index)
    {
        return (bits[index >> 3] & (1 << (index & 7))) != 0;
    }

    private static void WriteBit(byte[] bits, ushort index, int value)
    {
        bits[index >> 3] &= (byte)~(1 << (index & 7));
        bits[index >> 3] |= (byte)(value << (index & 7));
    }

    private static void SetBit(byte[] bits, ushort index)
    {
        bits[index >> 3] |= (byte)(1 << (index & 7));
    }

    private static bool IsTwoFormSpecies(int species)
    {
        return species == SpeciesShellos || species == SpeciesGastrodon || species == SpeciesShaymin || species == SpeciesGiratina;
    }

    private ref byte TwoForms(int species)
    {
        switch (species)
        {
            case SpeciesShellos:
                return ref ShellosForms;
            case SpeciesGastrodon:
                return ref GastrodonForms;
            case SpeciesShaymin:
                return ref ShayminForms;
            default:
                return ref GiratinaForms;
        }
    }

    private ref byte ThreeForms(int species)
    {
        if (species == SpeciesBurmy)
        {
            return ref BurmyForms;
        }
        return ref WormadamForms;
    }

    // OFFSET 0x02026324
    public static Pokedex Pokedex_New(int heapID)
    {
        Pokedex pokedex = new();
        Pokedex_Init(pokedex);
        return pokedex;
    }

    // OFFSET 0x02026338
    public static void Pokedex_Copy(Pokedex source, Pokedex destination)
    {
        byte[] buffer = new byte[Size];
        source.WriteTo(buffer, 0);
        destination.ReadFrom(buffer, 0);
    }

    // OFFSET 0x02026344
    private static bool Pokedex_IsSpeciesInvalid(ushort species)
    {
        if (species == 0 || species > NationalDexCount)
        {
            ErrorHandling.GF_AssertFail();
            return true;
        }
        return false;
    }

    // OFFSET 0x02026360
    private static void Pokedex_WriteGender(Pokedex pokedex, byte gender, int isSecondary, ushort species)
    {
        if (isSecondary == 0)
        {
            if (gender >= 2)
            {
                ErrorHandling.GF_AssertFail();
            }
            WriteBit(pokedex.Genders[1], (ushort)(species - 1), gender);
        }
        if (gender >= 2)
        {
            ErrorHandling.GF_AssertFail();
        }
        WriteBit(pokedex.Genders[isSecondary], (ushort)(species - 1), gender);
    }

    // OFFSET 0x020263D8
    private static void Pokedex_UpdateGenders(Pokedex pokedex, byte gender, int isSecondary, ushort species)
    {
        if (gender > GenderNone)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (gender == GenderNone)
        {
            gender = GenderMale;
        }
        Pokedex_WriteGender(pokedex, gender, isSecondary, species);
    }

    // OFFSET 0x02026400
    private static int Pokedex_CountUnownForms(Pokedex pokedex)
    {
        int count;
        for (count = 0; count < UnownFormCount; count++)
        {
            if (pokedex.UnownForms[count] == UnusedForm)
            {
                break;
            }
        }
        return count;
    }

    // OFFSET 0x02026418
    private static bool Pokedex_HasUnownForm(Pokedex pokedex, byte form)
    {
        for (int i = 0; i < UnownFormCount; i++)
        {
            if (form == pokedex.UnownForms[i])
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x0202643C
    private static void Pokedex_AddUnownForm(Pokedex pokedex, int form)
    {
        if (Pokedex_HasUnownForm(pokedex, (byte)form))
        {
            return;
        }
        int count = Pokedex_CountUnownForms(pokedex);
        if (count < UnownFormCount)
        {
            pokedex.UnownForms[count] = (byte)form;
        }
    }

    // OFFSET 0x02026464
    private static uint Pokedex_CountTwoForms(Pokedex pokedex, int species)
    {
        if (!IsTwoFormSpecies(species))
        {
            ErrorHandling.GF_AssertFail();
        }
        if (!Pokedex_HasSeenSpecies(pokedex, (ushort)species))
        {
            return 0;
        }
        byte forms = pokedex.TwoForms(species);
        int first = forms & 1;
        int second = (forms >> 1) & 1;
        if (first == second)
        {
            return 1;
        }
        return 2;
    }

    // OFFSET 0x02026514
    private static bool Pokedex_HasTwoFormsForm(Pokedex pokedex, int species, byte form)
    {
        if (!IsTwoFormSpecies(species))
        {
            ErrorHandling.GF_AssertFail();
        }
        if (!Pokedex_HasSeenSpecies(pokedex, (ushort)species))
        {
            return false;
        }
        byte forms = pokedex.TwoForms(species);
        uint count = Pokedex_CountTwoForms(pokedex, species);
        for (int i = 0; i < count; i++)
        {
            if (((forms >> i) & 1) == form)
            {
                return true;
            }
        }
        return false;
    }

    private static void WriteFormBit(ref byte forms, int index, byte form)
    {
        if (form >= 2)
        {
            ErrorHandling.GF_AssertFail();
        }
        forms &= (byte)~(1 << index);
        forms |= (byte)(form << index);
    }

    // OFFSET 0x020265E8
    private static void Pokedex_UpdateTwoForms(Pokedex pokedex, int species, int form)
    {
        if (!IsTwoFormSpecies(species))
        {
            ErrorHandling.GF_AssertFail();
        }
        if (Pokedex_HasTwoFormsForm(pokedex, species, (byte)form))
        {
            return;
        }
        ref byte forms = ref pokedex.TwoForms(species);
        int count = (int)Pokedex_CountTwoForms(pokedex, species);
        if (count >= 2)
        {
            return;
        }
        WriteFormBit(ref forms, count, (byte)form);
        if (count == 0)
        {
            WriteFormBit(ref forms, 1, (byte)form);
        }
    }

    private static int ThreeFormSlot(byte forms, int index)
    {
        return (forms >> (index * 2)) & 3;
    }

    // OFFSET 0x020266F8
    private static uint Pokedex_CountThreeForms(Pokedex pokedex, int species)
    {
        if (species != SpeciesBurmy && species != SpeciesWormadam)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (!Pokedex_HasSeenSpecies(pokedex, (ushort)species))
        {
            return 0;
        }
        byte forms = pokedex.ThreeForms(species);
        uint count;
        for (count = 0; count < 3; count++)
        {
            if (ThreeFormSlot(forms, (int)count) == ThreeFormUnused)
            {
                break;
            }
        }
        return count;
    }

    // OFFSET 0x02026754
    private static bool Pokedex_HasThreeFormsForm(Pokedex pokedex, int species, byte form)
    {
        if (species != SpeciesBurmy && species != SpeciesWormadam)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (!Pokedex_HasSeenSpecies(pokedex, (ushort)species))
        {
            return false;
        }
        byte forms = pokedex.ThreeForms(species);
        for (int i = 0; i < 3; i++)
        {
            if (ThreeFormSlot(forms, i) == form)
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x020267B8
    private static void Pokedex_UpdateThreeForms(Pokedex pokedex, int species, int form)
    {
        if (species != SpeciesBurmy && species != SpeciesWormadam)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (Pokedex_HasThreeFormsForm(pokedex, species, (byte)form))
        {
            return;
        }
        ref byte forms = ref pokedex.ThreeForms(species);
        int count = (int)Pokedex_CountThreeForms(pokedex, species);
        if (count >= 3)
        {
            return;
        }
        if ((byte)form >= 4)
        {
            ErrorHandling.GF_AssertFail();
        }
        forms &= (byte)~(3 << (count * 2));
        forms |= (byte)((byte)form << (count * 2));
    }

    // OFFSET 0x02026834
    private static void Pokedex_WriteDeoxysNibble(byte[] dexBits, uint form, byte slot)
    {
        int shift = slot * 4 + 24;
        uint word = Bin.U32(dexBits, LastDexWord);
        word &= ~(0xFu << shift);
        word |= form << shift;
        Bin.W32(dexBits, LastDexWord, word);
    }

    // OFFSET 0x02026850
    private static void Pokedex_SetDeoxysForm(Pokedex pokedex, uint form, byte index)
    {
        if (index >= DeoxysFormCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (form > DeoxysUnused)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (index < 2)
        {
            Pokedex_WriteDeoxysNibble(pokedex.Caught, form, index);
        }
        else
        {
            Pokedex_WriteDeoxysNibble(pokedex.Seen, form, (byte)(index - 2));
        }
    }

    // OFFSET 0x0202688C
    private static uint Pokedex_GetDeoxysForm(Pokedex pokedex, byte index)
    {
        if (index < 2)
        {
            return (Bin.U32(pokedex.Caught, LastDexWord) >> (index * 4 + 24)) & 0xF;
        }
        return (Bin.U32(pokedex.Seen, LastDexWord) >> ((byte)(index - 2) * 4 + 24)) & 0xF;
    }

    // OFFSET 0x020268B8
    private static uint Pokedex_CountDeoxysForms(Pokedex pokedex)
    {
        uint count;
        for (count = 0; count < DeoxysFormCount; count++)
        {
            if (Pokedex_GetDeoxysForm(pokedex, (byte)count) == DeoxysUnused)
            {
                break;
            }
        }
        return count;
    }

    // OFFSET 0x020268D8
    private static bool Pokedex_HasDeoxysForm(Pokedex pokedex, uint form)
    {
        for (int i = 0; i < DeoxysFormCount; i++)
        {
            if (form == Pokedex_GetDeoxysForm(pokedex, (byte)i))
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x020268FC
    private static void Pokedex_UpdateDeoxysForms(Pokedex pokedex, int species, Pokemon mon)
    {
        byte form = (byte)Pokemon.Pokemon_GetValue(mon, MonDataForm, null);
        if (species == SpeciesDeoxys)
        {
            if (!Pokedex_HasDeoxysForm(pokedex, form))
            {
                Pokedex_SetDeoxysForm(pokedex, form, (byte)Pokedex_CountDeoxysForms(pokedex));
            }
        }
    }

    // OFFSET 0x0202693C
    private static void Pokedex_ResetDeoxysForms(Pokedex pokedex)
    {
        for (int i = 0; i < DeoxysFormCount; i++)
        {
            Pokedex_SetDeoxysForm(pokedex, DeoxysUnused, (byte)i);
        }
    }

    // OFFSET 0x02026958
    private static uint Pokedex_CountRotomForms(Pokedex pokedex, int species)
    {
        if (species != SpeciesRotom)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (!Pokedex_HasSeenSpecies(pokedex, (ushort)species))
        {
            return 0;
        }
        uint count = 0;
        for (int i = 0; i < RotomFormCount; i++)
        {
            if (((pokedex.RotomForms >> (i * 3)) & 7) == RotomUnused)
            {
                break;
            }
            count++;
        }
        return count;
    }

    // OFFSET 0x020269A4
    private static bool Pokedex_HasRotomForm(Pokedex pokedex, int species, byte form)
    {
        if (species != SpeciesRotom)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (!Pokedex_HasSeenSpecies(pokedex, (ushort)species))
        {
            return false;
        }
        uint count = Pokedex_CountRotomForms(pokedex, species);
        for (int i = 0; i < count; i++)
        {
            if (((pokedex.RotomForms >> (i * 3)) & 7) == form)
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x02026A00
    private static void Pokedex_UpdateRotomForms(Pokedex pokedex, int species, int form)
    {
        if (species != SpeciesRotom)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (Pokedex_HasRotomForm(pokedex, species, (byte)form))
        {
            return;
        }
        int count = (int)Pokedex_CountRotomForms(pokedex, species);
        if (count >= RotomFormCount)
        {
            return;
        }
        if (form >= RotomUnused)
        {
            ErrorHandling.GF_AssertFail();
        }
        pokedex.RotomForms &= ~(7u << (count * 3));
        pokedex.RotomForms |= (uint)form << (count * 3);
    }

    // OFFSET 0x02026A60
    private static void Pokedex_UpdateForms(Pokedex pokedex, ushort species, Pokemon mon)
    {
        switch (species)
        {
            case SpeciesUnown:
                Pokedex_AddUnownForm(pokedex, Pokemon.Pokemon_GetForm(mon));
                break;
            case SpeciesBurmy:
            case SpeciesWormadam:
                Pokedex_UpdateThreeForms(pokedex, species, (int)Pokemon.Pokemon_GetValue(mon, MonDataForm, null));
                break;
            case SpeciesShellos:
            case SpeciesGastrodon:
            case SpeciesShaymin:
            case SpeciesGiratina:
                Pokedex_UpdateTwoForms(pokedex, species, (int)Pokemon.Pokemon_GetValue(mon, MonDataForm, null));
                break;
            case SpeciesDeoxys:
                Pokedex_UpdateDeoxysForms(pokedex, species, mon);
                break;
            case SpeciesRotom:
                Pokedex_UpdateRotomForms(pokedex, species, (int)Pokemon.Pokemon_GetValue(mon, MonDataForm, null));
                break;
        }
    }

    // OFFSET 0x02026B88
    private static void Pokedex_UpdateLanguage(Pokedex pokedex, ushort species, uint language)
    {
        int index = PokedexLanguage.PokedexLanguage_GetIndex((int)language);
        if (index != LanguageIndexNone)
        {
            pokedex.Languages[species] |= (byte)(1 << index);
        }
    }

    // OFFSET 0x02026BAC
    private static uint Pokedex_ReadDisplayedGender(Pokedex pokedex, ushort species, int displaySecondary)
    {
        if (SpeciesData.SpeciesData_GetSpeciesValue(species, SpeciesDataGenderRatio) == GenderRatioNone)
        {
            if (displaySecondary == 0)
            {
                return GenderNone;
            }
            return NoForm;
        }
        byte first = (byte)(GetBit(pokedex.Genders[0], (ushort)(species - 1)) ? 1 : 0);
        if (displaySecondary == 1)
        {
            byte second = (byte)(GetBit(pokedex.Genders[1], (ushort)(species - 1)) ? 1 : 0);
            if (second == first)
            {
                return NoForm;
            }
            return second;
        }
        return first;
    }

    // OFFSET 0x02026C24
    private static uint Pokedex_GetTwoFormsForm(Pokedex pokedex, int species, int index)
    {
        if (!IsTwoFormSpecies(species))
        {
            ErrorHandling.GF_AssertFail();
        }
        if (index >= 2)
        {
            ErrorHandling.GF_AssertFail();
        }
        return (uint)((pokedex.TwoForms(species) >> index) & 1);
    }

    // OFFSET 0x02026CCC
    private static uint Pokedex_GetRotomForm(Pokedex pokedex, int species, int index)
    {
        if (species != SpeciesRotom)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (index >= RotomFormCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return (pokedex.RotomForms >> (index * 3)) & 7;
    }

    // OFFSET 0x02026CFC
    private static uint Pokedex_GetThreeFormsForm(Pokedex pokedex, int species, int index)
    {
        if (species != SpeciesBurmy && species != SpeciesWormadam)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (index >= 3)
        {
            ErrorHandling.GF_AssertFail();
        }
        return (uint)ThreeFormSlot(pokedex.ThreeForms(species), index);
    }

    // OFFSET 0x02026D44
    private static bool Pokedex_CountsForNationalDex(ushort species)
    {
        sExcludedNational ??= RomData.Arm9U16(ExcludedNationalTable, ExcludedNationalCount);
        bool counts = true;
        for (int i = 0; i < ExcludedNationalCount; i++)
        {
            if (species == sExcludedNational[i])
            {
                counts = false;
            }
        }
        return counts;
    }

    // OFFSET 0x02026D68
    private static bool Pokedex_CountsForLocalDex(ushort species)
    {
        return true;
    }

    // OFFSET 0x02026D6C
    public static void Pokedex_Init(Pokedex pokedex)
    {
        Array.Clear(pokedex.Caught);
        Array.Clear(pokedex.Seen);
        Array.Clear(pokedex.Genders[0]);
        Array.Clear(pokedex.Genders[1]);
        Array.Clear(pokedex.Languages);
        pokedex.SpindaForm = 0;
        pokedex.CanDetectForms = 0;
        pokedex.CanDetectLanguages = 0;
        pokedex.Obtained = 0;
        pokedex.Magic = MagicNumber;
        pokedex.NationalDexObtained = 0;
        Array.Fill(pokedex.UnownForms, UnusedForm);
        pokedex.ShellosForms = UnusedForm;
        pokedex.GastrodonForms = UnusedForm;
        pokedex.BurmyForms = UnusedForm;
        pokedex.WormadamForms = UnusedForm;
        pokedex.RotomForms = 0xFFFFFFFF;
        pokedex.ShayminForms = UnusedForm;
        pokedex.GiratinaForms = UnusedForm;
        Pokedex_ResetDeoxysForms(pokedex);
    }

    // OFFSET 0x02026DD0
    public static ushort Pokedex_CountCaught_National(Pokedex pokedex)
    {
        CheckMagic(pokedex);
        int count = 0;
        for (int species = 1; species <= NationalDexCount; species++)
        {
            if (Pokedex_HasCaughtSpecies(pokedex, (ushort)species))
            {
                count++;
            }
        }
        return (ushort)count;
    }

    // OFFSET 0x02026E0C
    public static ushort Pokedex_CountSeen_National(Pokedex pokedex)
    {
        CheckMagic(pokedex);
        int count = 0;
        for (int species = 1; species <= NationalDexCount; species++)
        {
            if (Pokedex_HasSeenSpecies(pokedex, (ushort)species))
            {
                count++;
            }
        }
        return (ushort)count;
    }

    // OFFSET 0x02026E48
    public static ushort Pokedex_CountSeen(Pokedex pokedex)
    {
        if (Pokedex_IsNationalDexObtained(pokedex))
        {
            return Pokedex_CountSeen_National(pokedex);
        }
        return Pokedex_CountSeen_Local(pokedex);
    }

    // OFFSET 0x02026E64
    public static ushort Pokedex_CountCaught_Local(Pokedex pokedex)
    {
        CheckMagic(pokedex);
        int count = 0;
        for (int species = 1; species <= NationalDexCount; species++)
        {
            if (Pokedex_HasCaughtSpecies(pokedex, (ushort)species))
            {
                if (Pokemon.Species_GetBabySpecies((ushort)species) != 0)
                {
                    count++;
                }
            }
        }
        return (ushort)count;
    }

    // OFFSET 0x02026EAC
    public static ushort Pokedex_CountSeen_Local(Pokedex pokedex)
    {
        CheckMagic(pokedex);
        int count = 0;
        for (int species = 1; species <= NationalDexCount; species++)
        {
            if (Pokedex_HasSeenSpecies(pokedex, (ushort)species))
            {
                if (Pokemon.Species_GetBabySpecies((ushort)species) != 0)
                {
                    count++;
                }
            }
        }
        return (ushort)count;
    }

    // OFFSET 0x02026EF4
    public static bool Pokedex_NationalDexCompleted(Pokedex pokedex)
    {
        return Pokedex_NumCaught_National(pokedex) >= NationalDexGoal;
    }

    // OFFSET 0x02026F0C
    public static bool Pokedex_LocalDexCompleted(Pokedex pokedex)
    {
        return Pokedex_NumSeen_Local(pokedex) >= LocalDexGoal;
    }

    // OFFSET 0x02026F20
    public static ushort Pokedex_NumCaught_National(Pokedex pokedex)
    {
        ushort count = 0;
        for (int species = 1; species <= NationalDexCount; species++)
        {
            if (Pokedex_HasCaughtSpecies(pokedex, (ushort)species))
            {
                if (Pokedex_CountsForNationalDex((ushort)species))
                {
                    count++;
                }
            }
        }
        return count;
    }

    // OFFSET 0x02026F58
    public static ushort Pokedex_NumSeen_Local(Pokedex pokedex)
    {
        ushort count = 0;
        for (int species = 1; species <= NationalDexCount; species++)
        {
            if (Pokedex_HasSeenSpecies(pokedex, (ushort)species))
            {
                if (Pokemon.Species_GetBabySpecies((ushort)species) != 0)
                {
                    if (Pokedex_CountsForLocalDex((ushort)species))
                    {
                        count++;
                    }
                }
            }
        }
        return count;
    }

    // OFFSET 0x02026F9C
    public static bool Pokedex_HasCaughtSpecies(Pokedex pokedex, ushort species)
    {
        CheckMagic(pokedex);
        if (Pokedex_IsSpeciesInvalid(species))
        {
            return false;
        }
        ushort index = (ushort)(species - 1);
        if (GetBit(pokedex.Caught, index) && GetBit(pokedex.Seen, index))
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02026FE8
    public static bool Pokedex_HasSeenSpecies(Pokedex pokedex, ushort species)
    {
        CheckMagic(pokedex);
        if (Pokedex_IsSpeciesInvalid(species))
        {
            return false;
        }
        return GetBit(pokedex.Seen, (ushort)(species - 1));
    }

    // OFFSET 0x0202702C
    public static uint Pokedex_GetForm_Spinda(Pokedex pokedex, byte formIndex)
    {
        uint form = 0;
        CheckMagic(pokedex);
        if (formIndex == 0)
        {
            form = pokedex.SpindaForm;
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
        return form;
    }

    // OFFSET 0x02027058
    public static uint Pokedex_DisplayedGender(Pokedex pokedex, ushort species, int displaySecondary)
    {
        CheckMagic(pokedex);
        if (Pokedex_IsSpeciesInvalid(species))
        {
            return NoForm;
        }
        if (GetBit(pokedex.Seen, (ushort)(species - 1)))
        {
            return Pokedex_ReadDisplayedGender(pokedex, species, displaySecondary);
        }
        return NoForm;
    }

    // OFFSET 0x020270AC
    public static uint Pokedex_GetForm_Unown(Pokedex pokedex, int formIndex)
    {
        CheckMagic(pokedex);
        if (Pokedex_CountUnownForms(pokedex) <= formIndex)
        {
            return NoForm;
        }
        return pokedex.UnownForms[formIndex];
    }

    // OFFSET 0x020270DC
    public static uint Pokedex_NumFormsSeen_Unown(Pokedex pokedex)
    {
        CheckMagic(pokedex);
        return (uint)Pokedex_CountUnownForms(pokedex);
    }

    // OFFSET 0x020270F8
    public static uint Pokedex_GetForm_Shellos(Pokedex pokedex, int formIndex)
    {
        CheckMagic(pokedex);
        if ((int)Pokedex_CountTwoForms(pokedex, SpeciesShellos) <= formIndex)
        {
            return NoForm;
        }
        return Pokedex_GetTwoFormsForm(pokedex, SpeciesShellos, formIndex);
    }

    // OFFSET 0x02027130
    public static uint Pokedex_NumFormsSeen_Shellos(Pokedex pokedex)
    {
        CheckMagic(pokedex);
        return Pokedex_CountTwoForms(pokedex, SpeciesShellos);
    }

    // OFFSET 0x02027154
    public static uint Pokedex_GetForm_Gastrodon(Pokedex pokedex, int formIndex)
    {
        CheckMagic(pokedex);
        if ((int)Pokedex_CountTwoForms(pokedex, SpeciesGastrodon) <= formIndex)
        {
            return NoForm;
        }
        return Pokedex_GetTwoFormsForm(pokedex, SpeciesGastrodon, formIndex);
    }

    // OFFSET 0x0202718C
    public static uint Pokedex_NumFormsSeen_Gastrodon(Pokedex pokedex)
    {
        CheckMagic(pokedex);
        return Pokedex_CountTwoForms(pokedex, SpeciesGastrodon);
    }

    // OFFSET 0x020271B0
    public static uint Pokedex_GetForm_Burmy(Pokedex pokedex, int formIndex)
    {
        CheckMagic(pokedex);
        if ((int)Pokedex_CountThreeForms(pokedex, SpeciesBurmy) <= formIndex)
        {
            return NoForm;
        }
        return Pokedex_GetThreeFormsForm(pokedex, SpeciesBurmy, formIndex);
    }

    // OFFSET 0x020271E8
    public static uint Pokedex_NumFormsSeen_Burmy(Pokedex pokedex)
    {
        CheckMagic(pokedex);
        return Pokedex_CountThreeForms(pokedex, SpeciesBurmy);
    }

    // OFFSET 0x02027208
    public static uint Pokedex_GetForm_Wormadam(Pokedex pokedex, int formIndex)
    {
        CheckMagic(pokedex);
        if ((int)Pokedex_CountThreeForms(pokedex, SpeciesWormadam) <= formIndex)
        {
            return NoForm;
        }
        return Pokedex_GetThreeFormsForm(pokedex, SpeciesWormadam, formIndex);
    }

    // OFFSET 0x02027240
    public static uint Pokedex_NumFormsSeen_Wormadam(Pokedex pokedex)
    {
        CheckMagic(pokedex);
        return Pokedex_CountThreeForms(pokedex, SpeciesWormadam);
    }

    // OFFSET 0x02027264
    public static uint Pokedex_GetForm_Deoxys(Pokedex pokedex, int formIndex)
    {
        CheckMagic(pokedex);
        return Pokedex_GetDeoxysForm(pokedex, (byte)formIndex);
    }

    // OFFSET 0x02027288
    public static uint Pokedex_NumFormsSeen_Deoxys(Pokedex pokedex)
    {
        CheckMagic(pokedex);
        return Pokedex_CountDeoxysForms(pokedex);
    }

    private static void Pokedex_RecordGender(Pokedex pokedex, ushort species, uint personality, int gender)
    {
        ushort index = (ushort)(species - 1);
        if (!GetBit(pokedex.Seen, index))
        {
            if (species == SpeciesSpinda)
            {
                pokedex.SpindaForm = personality;
            }
            Pokedex_UpdateGenders(pokedex, (byte)gender, 0, species);
            return;
        }
        byte first = (byte)(GetBit(pokedex.Genders[0], index) ? 1 : 0);
        if (first != gender)
        {
            Pokedex_UpdateGenders(pokedex, (byte)gender, 1, species);
        }
    }

    // OFFSET 0x020272A4
    public static void Pokedex_Encounter(Pokedex pokedex, Pokemon mon)
    {
        ushort species = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null);
        uint personality = Pokemon.Pokemon_GetValue(mon, MonDataPersonality, null);
        int gender = Pokemon.Pokemon_GetGender(mon);
        CheckMagic(pokedex);
        if (Pokedex_IsSpeciesInvalid(species))
        {
            return;
        }
        Pokedex_RecordGender(pokedex, species, personality, gender);
        Pokedex_UpdateForms(pokedex, species, mon);
        SetBit(pokedex.Seen, (ushort)(species - 1));
    }

    // OFFSET 0x0202736C
    public static void Pokedex_Capture(Pokedex pokedex, Pokemon mon)
    {
        ushort species = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null);
        uint language = Pokemon.Pokemon_GetValue(mon, MonDataLanguage, null);
        uint personality = Pokemon.Pokemon_GetValue(mon, MonDataPersonality, null);
        int gender = Pokemon.Pokemon_GetGender(mon);
        CheckMagic(pokedex);
        if (Pokedex_IsSpeciesInvalid(species))
        {
            return;
        }
        Pokedex_RecordGender(pokedex, species, personality, gender);
        Pokedex_UpdateForms(pokedex, species, mon);
        Pokedex_UpdateLanguage(pokedex, species, language);
        SetBit(pokedex.Caught, (ushort)(species - 1));
        SetBit(pokedex.Seen, (ushort)(species - 1));
    }

    // OFFSET 0x02027454
    public static void Pokedex_ObtainNationalDex(Pokedex pokedex)
    {
        CheckMagic(pokedex);
        pokedex.NationalDexObtained = 1;
    }

    // OFFSET 0x02027474
    public static bool Pokedex_IsNationalDexObtained(Pokedex pokedex)
    {
        CheckMagic(pokedex);
        return pokedex.NationalDexObtained != 0;
    }

    // OFFSET 0x02027494
    public static bool Pokedex_CanDetectForms(Pokedex pokedex)
    {
        CheckMagic(pokedex);
        return pokedex.CanDetectForms != 0;
    }

    // OFFSET 0x020274B0
    public static void Pokedex_TurnOnFormDetection(Pokedex pokedex)
    {
        CheckMagic(pokedex);
        pokedex.CanDetectForms = 1;
    }

    // OFFSET 0x020274D0
    public static bool Pokedex_IsLanguageObtained(Pokedex pokedex, ushort species, uint languageIndex)
    {
        if (languageIndex >= LanguageCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        CheckMagic(pokedex);
        int index = PokedexLanguage.PokedexLanguage_GetIndex((int)languageIndex);
        return (pokedex.Languages[species] & (1 << index)) != 0;
    }

    // OFFSET 0x02027508
    public static void Pokedex_TurnOnLanguageDetection(Pokedex pokedex)
    {
        pokedex.CanDetectLanguages = 1;
    }

    // OFFSET 0x02027514
    public static bool Pokedex_CanDetectLanguages(Pokedex pokedex)
    {
        return pokedex.CanDetectLanguages != 0;
    }

    // OFFSET 0x02027520
    public static bool Pokedex_IsObtained(Pokedex pokedex)
    {
        CheckMagic(pokedex);
        return pokedex.Obtained != 0;
    }

    // OFFSET 0x02027540
    public static void Pokedex_ObtainPokedex(Pokedex pokedex)
    {
        CheckMagic(pokedex);
        pokedex.Obtained = 1;
    }

    // OFFSET 0x02027560
    public static Pokedex SaveData_GetPokedex(SaveData saveData)
    {
        return (Pokedex)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryPokedex);
    }

    // OFFSET 0x0202756C
    public static uint Pokedex_GetDisplayForm(Pokedex pokedex, int species, int formIndex)
    {
        CheckMagic(pokedex);
        switch (species)
        {
            case SpeciesUnown:
                if ((uint)formIndex < Pokedex_NumFormsSeen_Unown(pokedex))
                {
                    return Pokedex_GetForm_Unown(pokedex, formIndex);
                }
                break;
            case SpeciesShellos:
                if ((uint)formIndex < Pokedex_NumFormsSeen_Shellos(pokedex))
                {
                    return Pokedex_GetForm_Shellos(pokedex, formIndex);
                }
                break;
            case SpeciesGastrodon:
                if ((uint)formIndex < Pokedex_NumFormsSeen_Gastrodon(pokedex))
                {
                    return Pokedex_GetForm_Gastrodon(pokedex, formIndex);
                }
                break;
            case SpeciesBurmy:
                if ((uint)formIndex < Pokedex_NumFormsSeen_Burmy(pokedex))
                {
                    return Pokedex_GetForm_Burmy(pokedex, formIndex);
                }
                break;
            case SpeciesWormadam:
                if ((uint)formIndex < Pokedex_NumFormsSeen_Wormadam(pokedex))
                {
                    return Pokedex_GetForm_Wormadam(pokedex, formIndex);
                }
                break;
            case SpeciesDeoxys:
                if ((uint)formIndex < Pokedex_NumFormsSeen_Deoxys(pokedex))
                {
                    return Pokedex_GetForm_Deoxys(pokedex, formIndex);
                }
                break;
            case SpeciesShaymin:
                if (formIndex < (int)Pokedex_CountTwoForms(pokedex, SpeciesShaymin))
                {
                    return Pokedex_GetTwoFormsForm(pokedex, SpeciesShaymin, formIndex);
                }
                break;
            case SpeciesGiratina:
                if (formIndex < (int)Pokedex_CountTwoForms(pokedex, SpeciesGiratina))
                {
                    return Pokedex_GetTwoFormsForm(pokedex, SpeciesGiratina, formIndex);
                }
                break;
            case SpeciesRotom:
                if (formIndex < (int)Pokedex_CountRotomForms(pokedex, SpeciesRotom))
                {
                    return Pokedex_GetRotomForm(pokedex, SpeciesRotom, formIndex);
                }
                break;
        }
        return 0;
    }

    // OFFSET 0x020276C8
    public static uint Pokedex_NumFormsSeen(Pokedex pokedex, int species)
    {
        CheckMagic(pokedex);
        switch (species)
        {
            case SpeciesUnown:
                return Pokedex_NumFormsSeen_Unown(pokedex);
            case SpeciesShellos:
                return Pokedex_NumFormsSeen_Shellos(pokedex);
            case SpeciesGastrodon:
                return Pokedex_NumFormsSeen_Gastrodon(pokedex);
            case SpeciesBurmy:
                return Pokedex_NumFormsSeen_Burmy(pokedex);
            case SpeciesWormadam:
                return Pokedex_NumFormsSeen_Wormadam(pokedex);
            case SpeciesDeoxys:
                return Pokedex_NumFormsSeen_Deoxys(pokedex);
            case SpeciesShaymin:
                return Pokedex_CountTwoForms(pokedex, SpeciesShaymin);
            case SpeciesGiratina:
                return Pokedex_CountTwoForms(pokedex, SpeciesGiratina);
            case SpeciesRotom:
                return Pokedex_CountRotomForms(pokedex, SpeciesRotom);
        }
        return 1;
    }
}
