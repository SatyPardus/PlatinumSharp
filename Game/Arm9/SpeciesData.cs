using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public static class SpeciesData
{
    public const int Size = 0x2C;
    public const int ParamGenderRatio = 0x12;
    public const int GenderMale = 0;
    public const int GenderFemale = 1;
    public const int GenderNone = 2;

    private const int NarcPersonal = 2;
    private const int SpeciesDeoxys = 0x182;
    private const int SpeciesWormadam = 0x19D;
    private const int SpeciesRotom = 0x1DF;
    private const int SpeciesGiratina = 0x1E7;
    private const int SpeciesShaymin = 0x1EC;
    private const int FormIndexDeoxys = 0x1EF;
    private const int FormIndexWormadam = 0x1F2;
    private const int FormIndexGiratina = 0x1F4;
    private const int FormIndexShaymin = 0x1F5;
    private const int FormIndexRotom = 0x1F6;
    private const int GenderRatioMaleOnly = 0;
    private const int GenderRatioFemaleOnly = 0xFE;
    private const int GenderRatioNone = 0xFF;

    // OFFSET 0x02075874
    public static byte[] SpeciesData_FromMonForm(int species, int form, int heapID)
    {
        byte[] speciesData = Heap.Heap_Alloc(heapID, Size)!;
        SpeciesData_LoadForm(species, form, speciesData);
        return speciesData;
    }

    // OFFSET 0x02075894
    public static byte[] SpeciesData_FromMonSpecies(int species, int heapID)
    {
        byte[] speciesData = Heap.Heap_Alloc(heapID, Size)!;
        SpeciesData_LoadSpecies(species, speciesData);
        return speciesData;
    }

    // OFFSET 0x020758B0
    public static uint SpeciesData_GetValue(byte[]? speciesData, int param)
    {
        if (speciesData == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        byte[] data = speciesData!;
        if ((uint)param > 32)
        {
            return 0;
        }
        if (param < 10)
        {
            return data[param];
        }
        if (param < 16)
        {
            return (uint)(Bin.U16(data, 0xA) >> ((param - 10) * 2)) & 3;
        }
        if (param == 16)
        {
            return (uint)Bin.U16(data, 0xC);
        }
        if (param == 17)
        {
            return (uint)Bin.U16(data, 0xE);
        }
        if (param < 27)
        {
            return data[0x10 + param - 18];
        }
        if (param == 27)
        {
            return (uint)(data[0x19] & 0x7F);
        }
        if (param == 28)
        {
            return (uint)(data[0x19] >> 7);
        }
        return Bin.U32(data, 0x1C + (param - 29) * 4);
    }

    // OFFSET 0x020759B8
    public static void SpeciesData_Free(byte[]? speciesData)
    {
        if (speciesData == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        Heap.Heap_Free(speciesData!);
    }

    // OFFSET 0x020759CC
    public static uint SpeciesData_GetFormValue(int species, int form, int param)
    {
        byte[] speciesData = SpeciesData_FromMonSpecies(Species_GetFormDataIndex(species, form), 0);
        uint value = SpeciesData_GetValue(speciesData, param);
        SpeciesData_Free(speciesData);
        return value;
    }

    // OFFSET 0x020759F0
    public static uint SpeciesData_GetSpeciesValue(int species, int param)
    {
        byte[] speciesData = SpeciesData_FromMonSpecies(species, 0);
        uint value = SpeciesData_GetValue(speciesData, param);
        SpeciesData_Free(speciesData);
        return value;
    }

    // OFFSET 0x02075DAC
    public static uint Species_GetGender(ushort species, uint personality)
    {
        byte[] speciesData = SpeciesData_FromMonSpecies(species, 0);
        byte gender = SpeciesData_GetGender(speciesData, species, personality);
        SpeciesData_Free(speciesData);
        return gender;
    }

    // OFFSET 0x02075DD0
    public static byte SpeciesData_GetGender(byte[] speciesData, ushort species, uint personality)
    {
        byte ratio = (byte)SpeciesData_GetValue(speciesData, ParamGenderRatio);
        switch (ratio)
        {
            case GenderRatioMaleOnly:
                return GenderMale;
            case GenderRatioFemaleOnly:
                return GenderFemale;
            case GenderRatioNone:
                return GenderNone;
        }
        if (ratio > (byte)personality)
        {
            return GenderFemale;
        }
        return GenderMale;
    }

    // OFFSET 0x020781F8
    public static void SpeciesData_LoadSpecies(int species, byte[] speciesData)
    {
        NARC.NARC_ReadWholeMemberByIndexPair(speciesData, NarcPersonal, species);
    }

    // OFFSET 0x02078208
    public static void SpeciesData_LoadForm(int species, int form, byte[] speciesData)
    {
        NARC.NARC_ReadWholeMemberByIndexPair(speciesData, NarcPersonal, Species_GetFormDataIndex(species, form));
    }

    // OFFSET 0x02078740
    public static int Species_GetFormDataIndex(int species, int form)
    {
        switch (species)
        {
            case SpeciesDeoxys:
                if (form != 0 && form <= 3)
                {
                    return FormIndexDeoxys + form;
                }
                break;
            case SpeciesWormadam:
                if (form != 0 && form <= 2)
                {
                    return FormIndexWormadam + form;
                }
                break;
            case SpeciesGiratina:
                if (form != 0 && form <= 1)
                {
                    return FormIndexGiratina + form;
                }
                break;
            case SpeciesShaymin:
                if (form != 0 && form <= 1)
                {
                    return FormIndexShaymin + form;
                }
                break;
            case SpeciesRotom:
                if (form != 0 && form <= 5)
                {
                    return FormIndexRotom + form;
                }
                break;
        }
        return species;
    }
}
