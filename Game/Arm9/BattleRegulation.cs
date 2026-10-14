using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed class BattleRegulation
{
    public const int Size = 0x20;
    public const int NameLength = 12;
    public const int RuleNone = 0;
    public const int RuleTeamSize = 1;
    public const int RuleMaxLevel = 2;
    public const int RuleTotalLevel = 3;
    public const int RuleFlagBit0 = 4;
    public const int RuleHeightLimit = 5;
    public const int RuleHeightLimitSign = 6;
    public const int RuleWeightLimit = 7;
    public const int RuleWeightLimitSign = 8;
    public const int RuleFlagBit3 = 9;
    public const int RuleFlagBit2 = 10;
    public const int RuleFlagBit1 = 11;
    public const int RuleFlagBit4 = 12;
    public const int DefaultCount = 5;
    public const int NoIndex = 0xFF;

    private const int NameStringSize = 0x18;
    private const uint DefaultRegulation = 0x02100860;
    private const uint DefaultRegulations = 0x02100880;
    private const int MessageArchive = 0x1A;
    private const int RegulationNamesBank = 0x161;
    private const int FirstRegulationName = 0x46;
    private const int CustomRegulationCount = 1;

    public readonly ushort[] Name = new ushort[NameLength];
    public ushort TotalLevel;
    public byte TeamSize;
    public byte MaxLevel;
    public sbyte HeightLimit;
    public sbyte WeightLimit;
    public byte Flags;

    private static BattleRegulation[]? sDefaults;
    private static BattleRegulation? sDefault;

    public void WriteTo(byte[] body, int offset)
    {
        for (int i = 0; i < NameLength; i++)
        {
            Bin.W16(body, offset + i * 2, Name[i]);
        }
        Bin.W16(body, offset + 0x18, TotalLevel);
        body[offset + 0x1A] = TeamSize;
        body[offset + 0x1B] = MaxLevel;
        body[offset + 0x1C] = (byte)HeightLimit;
        body[offset + 0x1D] = (byte)WeightLimit;
        body[offset + 0x1E] = Flags;
    }

    public void ReadFrom(byte[] body, int offset)
    {
        for (int i = 0; i < NameLength; i++)
        {
            Name[i] = (ushort)Bin.U16(body, offset + i * 2);
        }
        TotalLevel = (ushort)Bin.U16(body, offset + 0x18);
        TeamSize = body[offset + 0x1A];
        MaxLevel = body[offset + 0x1B];
        HeightLimit = (sbyte)body[offset + 0x1C];
        WeightLimit = (sbyte)body[offset + 0x1D];
        Flags = body[offset + 0x1E];
    }

    private static BattleRegulation FromRom(uint address)
    {
        BattleRegulation regulation = new();
        regulation.ReadFrom(RomData.Arm9(address, Size), 0);
        return regulation;
    }

    private static BattleRegulation[] Defaults()
    {
        if (sDefaults == null)
        {
            sDefaults = new BattleRegulation[DefaultCount];
            for (int i = 0; i < DefaultCount; i++)
            {
                sDefaults[i] = FromRom(DefaultRegulations + (uint)(i * Size));
            }
        }
        return sDefaults;
    }

    private bool SameAs(BattleRegulation other)
    {
        byte[] mine = new byte[Size];
        byte[] theirs = new byte[Size];
        WriteTo(mine, 0);
        other.WriteTo(theirs, 0);
        return mine.AsSpan().SequenceEqual(theirs);
    }

    // OFFSET 0x0202602C
    public static int BattleRegulation_Size()
    {
        return Size;
    }

    // OFFSET 0x02026034
    public static void BattleRegulation_Copy(BattleRegulation source, BattleRegulation destination)
    {
        Array.Copy(source.Name, destination.Name, NameLength);
        destination.TotalLevel = source.TotalLevel;
        destination.TeamSize = source.TeamSize;
        destination.MaxLevel = source.MaxLevel;
        destination.HeightLimit = source.HeightLimit;
        destination.WeightLimit = source.WeightLimit;
        destination.Flags = source.Flags;
    }

    // OFFSET 0x02026040
    public static void RegulationBattles_Init(BattleRegulationData regulationData)
    {
        BattleRegulation regulation = regulationData.Regulations[0];
        Array.Clear(regulation.Name);
        regulation.TotalLevel = 0;
        regulation.TeamSize = 0;
        regulation.MaxLevel = 0;
        regulation.HeightLimit = 0;
        regulation.WeightLimit = 0;
        regulation.Flags = 0;
    }

    // OFFSET 0x0202604C
    public static void BattleRegulation_GetName(BattleRegulation regulation, GameString name)
    {
        GameString.String_CopyNumChars(name, regulation.Name, NameLength);
    }

    // OFFSET 0x0202605C
    public static GameString BattleRegulation_GetNameString(BattleRegulation regulation, int heapID)
    {
        GameString name = GameString.String_Init(NameStringSize, heapID);
        GameString.String_CopyChars(name, regulation.Name);
        return name;
    }

    // OFFSET 0x02026074
    public static int BattleRegulation_GetRuleValue(BattleRegulation regulation, int rule)
    {
        int value = 0;
        switch (rule)
        {
            case RuleTeamSize:
                value = regulation.TeamSize;
                break;
            case RuleMaxLevel:
                value = regulation.MaxLevel;
                break;
            case RuleTotalLevel:
                value = regulation.TotalLevel;
                break;
            case RuleFlagBit0:
                value = regulation.Flags & 1;
                break;
            case RuleHeightLimit:
                value = regulation.HeightLimit;
                break;
            case RuleHeightLimitSign:
                if (regulation.HeightLimit > 0)
                {
                    value = 1;
                }
                else if (regulation.HeightLimit < 0)
                {
                    value = -1;
                }
                break;
            case RuleWeightLimit:
                value = regulation.WeightLimit;
                break;
            case RuleWeightLimitSign:
                if (regulation.WeightLimit > 0)
                {
                    value = 1;
                }
                else if (regulation.WeightLimit < 0)
                {
                    value = -1;
                }
                break;
            case RuleFlagBit3:
                value = (regulation.Flags >> 3) & 1;
                break;
            case RuleFlagBit2:
                value = (regulation.Flags >> 2) & 1;
                break;
            case RuleFlagBit1:
                value = (regulation.Flags >> 1) & 1;
                break;
            case RuleFlagBit4:
                value = (regulation.Flags >> 4) & 1;
                break;
        }
        return value;
    }

    // OFFSET 0x0202610C
    public static BattleRegulation? BattleRegulation_Load(SaveData saveData, int index)
    {
        if (index >= CustomRegulationCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        BattleRegulationData data = (BattleRegulationData)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryRegulationBattles);
        BattleRegulation regulation = data.Regulations[index];
        if (BattleRegulation_GetRuleValue(regulation, RuleTeamSize) != 0)
        {
            return regulation;
        }
        return null;
    }

    // OFFSET 0x0202613C
    public static void BattleRegulation_Save(SaveData saveData, BattleRegulation regulation)
    {
        BattleRegulationData data = (BattleRegulationData)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryRegulationBattles);
        BattleRegulation_Copy(regulation, data.Regulations[0]);
    }

    // OFFSET 0x02026150
    public static BattleRegulation BattleRegulation_GetByIndex(SaveData saveData, int index)
    {
        if ((uint)index < DefaultCount)
        {
            return Defaults()[index];
        }
        BattleRegulationData data = (BattleRegulationData)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryRegulationBattles);
        return data.Regulations[0];
    }

    // OFFSET 0x0202616C
    public static void BattleRegulation_GetNameByIndex(SaveData saveData, int index, GameString name, int heapID)
    {
        if ((uint)index < DefaultCount)
        {
            MessageLoader loader = MessageLoader.MessageLoader_Init(0, MessageArchive, RegulationNamesBank, heapID)!;
            MessageLoader.MessageLoader_GetString(loader, (uint)(index + FirstRegulationName), name);
            MessageLoader.MessageLoader_Free(loader);
            return;
        }
        BattleRegulation regulation = BattleRegulation_Load(saveData, 0)!;
        BattleRegulation_GetName(regulation, name);
    }

    // OFFSET 0x020261A8
    public static BattleRegulation BattleRegulation_GetDefault()
    {
        sDefault ??= FromRom(DefaultRegulation);
        return sDefault;
    }

    // OFFSET 0x020261B0
    public static int BattleRegulation_GetIndex(BattleRegulation? regulation)
    {
        if (regulation == null)
        {
            return NoIndex;
        }
        BattleRegulation[] defaults = Defaults();
        for (int i = 0; i < DefaultCount; i++)
        {
            if (regulation.SameAs(defaults[i]))
            {
                return i;
            }
        }
        return DefaultCount;
    }
}

public sealed class BattleRegulationData : ISaveBlock
{
    public readonly BattleRegulation[] Regulations = { new BattleRegulation() };

    public void WriteTo(byte[] body, int offset)
    {
        Regulations[0].WriteTo(body, offset);
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Regulations[0].ReadFrom(body, offset);
    }
}
