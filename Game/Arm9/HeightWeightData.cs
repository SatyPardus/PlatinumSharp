namespace PlatinumSharp.Game;

public sealed class HeightWeightData
{
    public int[]? Height;
    public int[]? Weight;
    public short[]? TrainerPositions;
    public short[]? PokemonPositions;
    public short[]? TrainerScales;
    public short[]? PokemonScales;
}

public static class HeightWeight
{
    // OFFSET 0x0207A2A8
    public static HeightWeightData HeightWeightData_LoadNew(int heapID)
    {
        HeightWeightData data = HeightWeightData_New(heapID);
        HeightWeightData_Load(data, 0, heapID);
        return data;
    }

    // OFFSET 0x0207A2C0
    public static void HeightWeightData_FreeLoaded(HeightWeightData data)
    {
        HeightWeightData_Release(data);
        HeightWeightData_Free(data);
    }

    private const int MemberHeight = 0;
    private const int MemberWeight = 1;
    private const int MemberMaleTrainerPositions = 9;
    private const int MemberMalePokemonPositions = 10;
    private const int MemberMaleTrainerScales = 5;
    private const int MemberMalePokemonScales = 6;
    private const int MemberFemaleTrainerPositions = 7;
    private const int MemberFemalePokemonPositions = 8;
    private const int MemberFemaleTrainerScales = 3;
    private const int MemberFemalePokemonScales = 4;
    private const int NarcDefaultForms = 0xC2;
    private const int NarcAlternateForms = 0x4B;
    private const int DefaultFormsSortNarc = 0x2C4;
    private const int AlternateFormsSortNarc = 0x2C3;

    private static int sSortNarcID = AlternateFormsSortNarc;
    private static int sDataNarcID = NarcAlternateForms;
    private static int sEncounterNarcID = AlternateFormsSortNarc + 2;

    private static int[] ToInts(byte[] raw)
    {
        int[] values = new int[raw.Length / 4];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = (int)Nitro.Bin.U32(raw, i * 4);
        }
        return values;
    }

    private static short[] ToShorts(byte[] raw)
    {
        short[] values = new short[raw.Length / 2];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = (short)Nitro.Bin.U16(raw, i * 2);
        }
        return values;
    }

    // OFFSET 0x02098700
    public static HeightWeightData HeightWeightData_New(int heapID)
    {
        return new HeightWeightData();
    }

    // OFFSET 0x02098718
    public static void HeightWeightData_Free(HeightWeightData? data)
    {
        if (data == null)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x0209872C
    public static void HeightWeightData_Load(HeightWeightData? data, int isFemale, int heapID)
    {
        if (data == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (data!.Height != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (data.Weight != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        NARC narc = NARC.NARC_ctor(HeightWeight_GetNarcID(), heapID);
        data.Height = HeightWeightData_LoadHeight(narc, heapID);
        data.Weight = HeightWeightData_LoadWeight(narc, heapID);
        if (isFemale == 0)
        {
            HeightWeightData_LoadMalePositions(narc, out data.TrainerPositions, out data.PokemonPositions, heapID);
            HeightWeightData_LoadMaleScales(narc, out data.TrainerScales, out data.PokemonScales, heapID);
        }
        else
        {
            HeightWeightData_LoadFemalePositions(narc, out data.TrainerPositions, out data.PokemonPositions, heapID);
            HeightWeightData_LoadFemaleScales(narc, out data.TrainerScales, out data.PokemonScales, heapID);
        }
        NARC.NARC_dtor(narc);
    }

    // OFFSET 0x020987BC
    public static void HeightWeightData_Release(HeightWeightData data)
    {
        if (data.Height == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (data.Weight == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        data.Height = null;
        data.Weight = null;
        data.TrainerPositions = null;
        data.PokemonPositions = null;
        data.TrainerScales = null;
        data.PokemonScales = null;
    }

    // OFFSET 0x02098808
    public static int HeightWeightData_Height(HeightWeightData? data, int species)
    {
        if (data == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (data!.Height == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return data.Height![species];
    }

    // OFFSET 0x02098828
    public static int HeightWeightData_Weight(HeightWeightData? data, int species)
    {
        if (data == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (data!.Weight == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return data.Weight![species];
    }

    // OFFSET 0x02098848
    public static short HeightWeightData_TrainerPosition(HeightWeightData? data, int species)
    {
        if (data == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (data!.TrainerPositions == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return data.TrainerPositions![species];
    }

    // OFFSET 0x02098868
    public static short HeightWeightData_PokemonPosition(HeightWeightData? data, int species)
    {
        if (data == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (data!.PokemonPositions == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return data.PokemonPositions![species];
    }

    // OFFSET 0x02098888
    public static short HeightWeightData_TrainerScale(HeightWeightData? data, int species)
    {
        if (data == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (data!.TrainerScales == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return data.TrainerScales![species];
    }

    // OFFSET 0x020988A8
    public static short HeightWeightData_PokemonScale(HeightWeightData? data, int species)
    {
        if (data == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (data!.PokemonScales == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return data.PokemonScales![species];
    }

    // OFFSET 0x020988C8
    private static int[] HeightWeightData_LoadHeight(NARC narc, int heapID)
    {
        return ToInts(Graphics.LoadMemberFromOpenNARC(narc, MemberHeight, 0, heapID, 0)!);
    }

    // OFFSET 0x020988D8
    private static int[] HeightWeightData_LoadWeight(NARC narc, int heapID)
    {
        return ToInts(Graphics.LoadMemberFromOpenNARC(narc, MemberWeight, 0, heapID, 0)!);
    }

    // OFFSET 0x020988E8
    private static void HeightWeightData_LoadMalePositions(NARC narc, out short[]? trainer, out short[]? pokemon, int heapID)
    {
        trainer = ToShorts(Graphics.LoadMemberFromOpenNARC(narc, MemberMaleTrainerPositions, 0, heapID, 0)!);
        pokemon = ToShorts(Graphics.LoadMemberFromOpenNARC(narc, MemberMalePokemonPositions, 0, heapID, 0)!);
    }

    // OFFSET 0x02098910
    private static void HeightWeightData_LoadFemalePositions(NARC narc, out short[]? trainer, out short[]? pokemon, int heapID)
    {
        trainer = ToShorts(Graphics.LoadMemberFromOpenNARC(narc, MemberFemaleTrainerPositions, 0, heapID, 0)!);
        pokemon = ToShorts(Graphics.LoadMemberFromOpenNARC(narc, MemberFemalePokemonPositions, 0, heapID, 0)!);
    }

    // OFFSET 0x02098938
    private static void HeightWeightData_LoadMaleScales(NARC narc, out short[]? trainer, out short[]? pokemon, int heapID)
    {
        trainer = ToShorts(Graphics.LoadMemberFromOpenNARC(narc, MemberMaleTrainerScales, 0, heapID, 0)!);
        pokemon = ToShorts(Graphics.LoadMemberFromOpenNARC(narc, MemberMalePokemonScales, 0, heapID, 0)!);
    }

    // OFFSET 0x02098960
    private static void HeightWeightData_LoadFemaleScales(NARC narc, out short[]? trainer, out short[]? pokemon, int heapID)
    {
        trainer = ToShorts(Graphics.LoadMemberFromOpenNARC(narc, MemberFemaleTrainerScales, 0, heapID, 0)!);
        pokemon = ToShorts(Graphics.LoadMemberFromOpenNARC(narc, MemberFemalePokemonScales, 0, heapID, 0)!);
    }

    // OFFSET 0x02098988
    public static void HeightWeight_SetFormNarcs(int form)
    {
        if (form == 0)
        {
            sDataNarcID = NarcDefaultForms;
            sSortNarcID = DefaultFormsSortNarc;
            sEncounterNarcID = DefaultFormsSortNarc + 2;
        }
        else
        {
            sDataNarcID = NarcAlternateForms;
            sSortNarcID = AlternateFormsSortNarc;
            sEncounterNarcID = AlternateFormsSortNarc + 2;
        }
    }

    // OFFSET 0x020989B8
    public static int HeightWeight_GetNarcID()
    {
        return sDataNarcID;
    }
}
