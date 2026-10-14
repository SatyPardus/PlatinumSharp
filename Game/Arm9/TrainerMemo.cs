namespace PlatinumSharp.Game;

public sealed class TrainerMemoLine
{
    public int LineNum;
    public GameString? Text;
}

public sealed class TrainerMemo
{
    public const int MaxLines = 5;

    public int HeapID;
    public MessageLoader? Loader;
    public StringTemplate? Template;
    public Pokemon? Mon;
    public bool IsOTPlayer;
    public readonly TrainerMemoLine[] Lines = NewLines();

    private const int MessageArchive = 0x1A;
    private const int MemoTextBank = 0x1C7;
    private const uint CharacteristicTable = 0x020F5578;
    private const uint FlavorTable = 0x020F556C;
    private const int NatureEntryBase = 0x18;
    private const int MaxNature = 0x18;
    private const int NumStats = 6;
    private const int NumFlavors = 5;
    private const int LineNature = 0;
    private const int LineMet = 1;
    private const int LineCharacteristic = 2;
    private const int LineFlavor = 3;
    private const int LineEggStatus = 4;
    private const int ParamPersonality = 0;
    private const int ParamFriendship = 9;
    private const int ParamHPIV = 0x46;
    private const int ParamIsEgg = 0x4C;
    private const int ParamFatefulEncounter = 0x6E;
    private const int ParamMetGame = 0x7A;
    private const int ParamEggYear = 0x92;
    private const int ParamEggMonth = 0x93;
    private const int ParamEggDay = 0x94;
    private const int ParamMetYear = 0x95;
    private const int ParamMetMonth = 0x96;
    private const int ParamMetDay = 0x97;
    private const int ParamEggLocation = 0x98;
    private const int ParamMetLocation = 0x99;
    private const int ParamMetLevel = 0x9C;
    private const int LocationTypeRegular = 0;
    private const int LocationTypeSpecial = 1;
    private const int LocationTypeEvent = 2;

    private static TrainerMemoLine[] NewLines()
    {
        TrainerMemoLine[] lines = new TrainerMemoLine[MaxLines];
        for (int i = 0; i < MaxLines; i++)
        {
            lines[i] = new TrainerMemoLine();
        }
        return lines;
    }

    private static uint MonValue(TrainerMemo memo, int param)
    {
        return Pokemon.Pokemon_GetValue(memo.Mon!, param, null);
    }

    private static void SetMemo(TrainerMemo memo, int nature, int met, int metEntry, int characteristic, int flavor)
    {
        memo.Lines[LineNature].LineNum = nature;
        PrintNature(memo);
        memo.Lines[LineMet].LineNum = met;
        PrintMetInfo(memo, metEntry);
        memo.Lines[LineCharacteristic].LineNum = characteristic;
        PrintCharacteristic(memo);
        if (flavor == 0)
        {
            return;
        }
        memo.Lines[LineFlavor].LineNum = flavor;
        PrintFlavor(memo);
    }

    private static void SetEggMemo(TrainerMemo memo, int entry, int useMetDate)
    {
        memo.Lines[LineMet].LineNum = 1;
        PrintEggInfo(memo, entry, useMetDate);
        memo.Lines[LineEggStatus].LineNum = 6;
        PrintEggStatus(memo);
    }

    // OFFSET 0x02092494
    public static TrainerMemo TrainerMemo_New(Pokemon mon, bool isOTPlayer, int heapID)
    {
        TrainerMemo memo = new();
        memo.HeapID = heapID;
        memo.Loader = MessageLoader.MessageLoader_Init(1, MessageArchive, MemoTextBank, heapID);
        memo.Template = StringTemplate.StringTemplate_New(9, 0x20, memo.HeapID);
        memo.Mon = mon;
        memo.IsOTPlayer = isOTPlayer;
        for (int i = 0; i < MaxLines; i++)
        {
            memo.Lines[i].LineNum = 0;
            memo.Lines[i].Text = null;
        }
        switch (TrainerMemo_GetType(memo.Mon, memo.IsOTPlayer ? 1 : 0, memo.HeapID))
        {
            case 0:
                SetMemo(memo, 1, 2, 0x31, 6, 7);
                break;
            case 1:
                SetMemo(memo, 1, 2, 0x32, 6, 7);
                break;
            case 2:
                SetMemo(memo, 1, 2, 0x33, 6, 7);
                break;
            case 3:
                SetMemo(memo, 1, 2, 0x34, 8, 9);
                break;
            case 4:
                SetMemo(memo, 1, 2, 0x35, 8, 9);
                break;
            case 5:
                SetMemo(memo, 1, 2, 0x36, 8, 9);
                break;
            case 6:
                SetMemo(memo, 1, 2, 0x37, 8, 9);
                break;
            case 7:
                SetMemo(memo, 1, 2, 0x38, 7, 8);
                break;
            case 8:
                SetMemo(memo, 1, 2, 0x39, 7, 8);
                break;
            case 9:
                SetMemo(memo, 1, 2, 0x3A, 9, 0);
                break;
            case 10:
                SetMemo(memo, 1, 2, 0x3B, 9, 0);
                break;
            case 11:
                SetMemo(memo, 1, 2, 0x3C, 9, 0);
                break;
            case 12:
                SetMemo(memo, 1, 2, 0x3D, 9, 0);
                break;
            case 13:
                SetMemo(memo, 1, 2, 0x3E, 9, 0);
                break;
            case 14:
                SetMemo(memo, 1, 2, 0x3F, 9, 0);
                break;
            case 15:
                memo.Lines[LineNature].LineNum = 1;
                PrintNature(memo);
                memo.Lines[LineMet].LineNum = 2;
                PrintMetInfoSpecial(memo, 0x40);
                memo.Lines[LineCharacteristic].LineNum = 6;
                PrintCharacteristic(memo);
                memo.Lines[LineFlavor].LineNum = 7;
                PrintFlavor(memo);
                break;
            case 16:
                SetEggMemo(memo, 0x65, 0);
                break;
            case 17:
                SetEggMemo(memo, 0x66, 1);
                break;
            case 18:
                SetEggMemo(memo, 0x67, 0);
                break;
            case 19:
                SetEggMemo(memo, 0x67, 1);
                break;
            case 20:
                SetEggMemo(memo, 0x68, 0);
                break;
        }
        return memo;
    }

    // OFFSET 0x0209282C
    public static void TrainerMemo_Free(TrainerMemo memo)
    {
        for (int i = 0; i < MaxLines; i++)
        {
            if (memo.Lines[i].Text != null)
            {
                GameString.String_Free(memo.Lines[i].Text);
            }
        }
        StringTemplate.StringTemplate_Free(memo.Template!);
        MessageLoader.MessageLoader_Free(memo.Loader);
    }

    // OFFSET 0x02092878
    private static void PrintNature(TrainerMemo memo)
    {
        byte nature = Pokemon.Pokemon_GetNature(memo.Mon!);
        if (nature > MaxNature)
        {
            return;
        }
        memo.Lines[LineNature].Text = GameString.String_Init(0x48, memo.HeapID);
        MessageLoader.MessageLoader_GetString(memo.Loader!, (uint)(nature + NatureEntryBase), memo.Lines[LineNature].Text!);
    }

    private static void SetMetDate(TrainerMemo memo)
    {
        StringTemplate template = memo.Template!;
        StringTemplate.StringTemplate_SetNumber(template, 0, (int)MonValue(memo, ParamMetYear), 2, 2, 1);
        StringTemplate.StringTemplate_SetMonthName(template, 1, MonValue(memo, ParamMetMonth));
        StringTemplate.StringTemplate_SetNumber(template, 2, (int)MonValue(memo, ParamMetDay), 2, 0, 1);
        StringTemplate.StringTemplate_SetNumber(template, 3, (int)MonValue(memo, ParamMetLevel), 3, 0, 1);
    }

    // OFFSET 0x020928A0
    private static void PrintMetInfo(TrainerMemo memo, int entry)
    {
        GameString format = GameString.String_Init(0x240, memo.HeapID);
        memo.Lines[LineMet].Text = GameString.String_Init(0x240, memo.HeapID);
        MessageLoader.MessageLoader_GetString(memo.Loader!, (uint)entry, format);
        StringTemplate template = memo.Template!;
        SetMetDate(memo);
        StringTemplate.StringTemplate_SetMetLocationName(template, 4, MonValue(memo, ParamMetLocation));
        StringTemplate.StringTemplate_SetNumber(template, 5, (int)MonValue(memo, ParamEggYear), 2, 2, 1);
        StringTemplate.StringTemplate_SetMonthName(template, 6, MonValue(memo, ParamEggMonth));
        StringTemplate.StringTemplate_SetNumber(template, 7, (int)MonValue(memo, ParamEggDay), 2, 0, 1);
        StringTemplate.StringTemplate_SetMetLocationName(template, 8, MonValue(memo, ParamEggLocation));
        StringTemplate.StringTemplate_Format(template, memo.Lines[LineMet].Text!, format);
        GameString.String_Free(format);
    }

    // OFFSET 0x020929C0
    private static void PrintMetInfoSpecial(TrainerMemo memo, int entry)
    {
        GameString format = GameString.String_Init(0x120, memo.HeapID);
        memo.Lines[LineMet].Text = GameString.String_Init(0x120, memo.HeapID);
        MessageLoader.MessageLoader_GetString(memo.Loader!, (uint)entry, format);
        SetMetDate(memo);
        int location;
        switch (MonValue(memo, ParamMetGame))
        {
            case 1:
            case 2:
            case 3:
                location = SpecialMetLoc.SpecialMetLoc_GetId(LocationTypeSpecial, 5);
                break;
            case 4:
            case 5:
                location = SpecialMetLoc.SpecialMetLoc_GetId(LocationTypeSpecial, 3);
                break;
            case 7:
            case 8:
                location = SpecialMetLoc.SpecialMetLoc_GetId(LocationTypeSpecial, 4);
                break;
            case 10:
            case 11:
            case 12:
                location = SpecialMetLoc.SpecialMetLoc_GetId(LocationTypeSpecial, 7);
                break;
            case 15:
                location = SpecialMetLoc.SpecialMetLoc_GetId(LocationTypeSpecial, 8);
                break;
            default:
                location = SpecialMetLoc.SpecialMetLoc_GetId(LocationTypeSpecial, 7);
                break;
        }
        StringTemplate.StringTemplate_SetMetLocationName(memo.Template!, 4, (uint)location);
        StringTemplate.StringTemplate_Format(memo.Template!, memo.Lines[LineMet].Text!, format);
        GameString.String_Free(format);
    }

    // OFFSET 0x02092B1C
    private static void PrintEggInfo(TrainerMemo memo, int entry, int useMetDate)
    {
        GameString format = GameString.String_Init(0x168, memo.HeapID);
        memo.Lines[LineMet].Text = GameString.String_Init(0x168, memo.HeapID);
        MessageLoader.MessageLoader_GetString(memo.Loader!, (uint)entry, format);
        StringTemplate template = memo.Template!;
        if (useMetDate == 0)
        {
            StringTemplate.StringTemplate_SetNumber(template, 5, (int)MonValue(memo, ParamEggYear), 2, 2, 1);
            StringTemplate.StringTemplate_SetMonthName(template, 6, MonValue(memo, ParamEggMonth));
            StringTemplate.StringTemplate_SetNumber(template, 7, (int)MonValue(memo, ParamEggDay), 2, 0, 1);
            StringTemplate.StringTemplate_SetMetLocationName(template, 8, MonValue(memo, ParamEggLocation));
        }
        else
        {
            StringTemplate.StringTemplate_SetNumber(template, 5, (int)MonValue(memo, ParamMetYear), 2, 2, 1);
            StringTemplate.StringTemplate_SetMonthName(template, 6, MonValue(memo, ParamMetMonth));
            StringTemplate.StringTemplate_SetNumber(template, 7, (int)MonValue(memo, ParamMetDay), 2, 0, 1);
            StringTemplate.StringTemplate_SetMetLocationName(template, 8, MonValue(memo, ParamMetLocation));
        }
        StringTemplate.StringTemplate_Format(template, memo.Lines[LineMet].Text!, format);
        GameString.String_Free(format);
    }

    // OFFSET 0x02092C24
    private static void PrintCharacteristic(TrainerMemo memo)
    {
        memo.Lines[LineCharacteristic].Text = GameString.String_Init(0x48, memo.HeapID);
        int[] ivs = new int[NumStats];
        for (int i = 0; i < NumStats; i++)
        {
            ivs[i] = (int)MonValue(memo, ParamHPIV + i);
        }
        int start = (int)(MonValue(memo, ParamPersonality) % NumStats);
        int best = start;
        int bestIV = ivs[start];
        for (int i = 1; i < NumStats; i++)
        {
            int stat = (start + i) % NumStats;
            if (bestIV < ivs[stat])
            {
                best = stat;
                bestIV = ivs[stat];
            }
        }
        ushort entry = RomData.Arm9U16((uint)(CharacteristicTable + best * 10 + (bestIV % 5) * 2), 1)[0];
        MessageLoader.MessageLoader_GetString(memo.Loader!, entry, memo.Lines[LineCharacteristic].Text!);
    }

    // OFFSET 0x02092E10
    private static void PrintFlavor(TrainerMemo memo)
    {
        memo.Lines[LineFlavor].Text = GameString.String_Init(0x48, memo.HeapID);
        int flavor = 0;
        for (int i = 0; i < NumFlavors; i++)
        {
            if (Pokemon.Pokemon_GetFlavorAffinity(memo.Mon!, i) == 1)
            {
                flavor = i + 1;
            }
        }
        ushort entry = RomData.Arm9U16((uint)(FlavorTable + flavor * 2), 1)[0];
        MessageLoader.MessageLoader_GetString(memo.Loader!, entry, memo.Lines[LineFlavor].Text!);
    }

    // OFFSET 0x02092E4C
    private static void PrintEggStatus(TrainerMemo memo)
    {
        int friendship = (int)MonValue(memo, ParamFriendship);
        memo.Lines[LineEggStatus].Text = GameString.String_Init(0x120, memo.HeapID);
        uint entry;
        if (friendship <= 5)
        {
            entry = 0x69;
        }
        else if (friendship <= 10)
        {
            entry = 0x6A;
        }
        else if (friendship <= 40)
        {
            entry = 0x6B;
        }
        else
        {
            entry = 0x6C;
        }
        MessageLoader.MessageLoader_GetString(memo.Loader!, entry, memo.Lines[LineEggStatus].Text!);
    }

    private static uint OTVariant(int isOTPlayer, uint ownType, uint otherType)
    {
        return isOTPlayer == 1 ? ownType : otherType;
    }

    // OFFSET 0x02092E8C
    public static uint TrainerMemo_GetType(Pokemon mon, int isOTPlayer, int heapID)
    {
        if (Pokemon.Pokemon_GetValue(mon, ParamIsEgg, null) == 0)
        {
            if (Pokemon.Pokemon_GetValue(mon, ParamEggLocation, null) == 0)
            {
                uint metLocation = Pokemon.Pokemon_GetValue(mon, ParamMetLocation, null);
                if (metLocation == SpecialMetLoc.SpecialMetLoc_GetId(LocationTypeRegular, 0x37))
                {
                    return 15;
                }
                if (Pokemon.Pokemon_GetValue(mon, ParamFatefulEncounter, null) == 1)
                {
                    return OTVariant(isOTPlayer, 7, 8);
                }
                if (Pokemon.Pokemon_GetValue(mon, ParamMetLocation, null) == SpecialMetLoc.SpecialMetLoc_GetId(LocationTypeSpecial, 1))
                {
                    return 2;
                }
                return OTVariant(isOTPlayer, 0, 1);
            }
            if (Pokemon.Pokemon_GetValue(mon, ParamFatefulEncounter, null) == 1)
            {
                if (Pokemon.Pokemon_GetValue(mon, ParamEggLocation, null) == SpecialMetLoc.SpecialMetLoc_GetId(LocationTypeSpecial, 2))
                {
                    return OTVariant(isOTPlayer, 13, 14);
                }
                if (Pokemon.Pokemon_GetValue(mon, ParamEggLocation, null) == SpecialMetLoc.SpecialMetLoc_GetId(LocationTypeEvent, 1))
                {
                    return OTVariant(isOTPlayer, 11, 12);
                }
                return OTVariant(isOTPlayer, 9, 10);
            }
            if (Pokemon.Pokemon_GetValue(mon, ParamEggLocation, null) == SpecialMetLoc.SpecialMetLoc_GetId(LocationTypeSpecial, 1)
                || Pokemon.Pokemon_GetValue(mon, ParamEggLocation, null) == SpecialMetLoc.SpecialMetLoc_GetId(LocationTypeSpecial, 0)
                || Pokemon.Pokemon_GetValue(mon, ParamEggLocation, null) == SpecialMetLoc.SpecialMetLoc_GetId(LocationTypeSpecial, 9)
                || Pokemon.Pokemon_GetValue(mon, ParamEggLocation, null) == SpecialMetLoc.SpecialMetLoc_GetId(LocationTypeSpecial, 10)
                || Pokemon.Pokemon_GetValue(mon, ParamEggLocation, null) == SpecialMetLoc.SpecialMetLoc_GetId(LocationTypeSpecial, 11))
            {
                return OTVariant(isOTPlayer, 5, 6);
            }
            return OTVariant(isOTPlayer, 3, 4);
        }
        if (isOTPlayer == 1)
        {
            if (Pokemon.Pokemon_GetValue(mon, ParamFatefulEncounter, null) != 1)
            {
                return 16;
            }
            if (Pokemon.Pokemon_GetValue(mon, ParamEggLocation, null) == SpecialMetLoc.SpecialMetLoc_GetId(LocationTypeEvent, 1))
            {
                return 20;
            }
            return 18;
        }
        if (Pokemon.Pokemon_GetValue(mon, ParamFatefulEncounter, null) == 1)
        {
            return 19;
        }
        return 17;
    }
}
