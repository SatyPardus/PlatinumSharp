using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class DaycareMail
{
    public const int Size = 0x60;
    public const int TrainerNameLength = 8;
    public const int MonNameLength = 11;

    private const int TrainerNameOffset = 0x38;
    private const int MonNameOffset = 0x48;

    public readonly Mail Mail = new();
    public readonly ushort[] TrainerName = new ushort[TrainerNameLength];
    public readonly ushort[] MonName = new ushort[MonNameLength];

    public void WriteTo(byte[] body, int offset)
    {
        Mail.WriteTo(body, offset);
        for (int i = 0; i < TrainerNameLength; i++)
        {
            Bin.W16(body, offset + TrainerNameOffset + i * 2, TrainerName[i]);
        }
        for (int i = 0; i < MonNameLength; i++)
        {
            Bin.W16(body, offset + MonNameOffset + i * 2, MonName[i]);
        }
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Mail.ReadFrom(body, offset);
        for (int i = 0; i < TrainerNameLength; i++)
        {
            TrainerName[i] = (ushort)Bin.U16(body, offset + TrainerNameOffset + i * 2);
        }
        for (int i = 0; i < MonNameLength; i++)
        {
            MonName[i] = (ushort)Bin.U16(body, offset + MonNameOffset + i * 2);
        }
    }
}

public sealed class DaycareMon
{
    public const int Size = 0xEC;

    public BoxPokemon BoxMon = new();
    public readonly DaycareMail DaycareMail = new();
    public uint Steps;

    public void WriteTo(byte[] body, int offset)
    {
        BoxMon.CopyTo(0, body, offset, BoxPokemon.Size);
        DaycareMail.WriteTo(body, offset + BoxPokemon.Size);
        Bin.W32(body, offset + BoxPokemon.Size + DaycareMail.Size, Steps);
    }

    public void ReadFrom(byte[] body, int offset)
    {
        BoxMon.CopyFrom(0, body, offset, BoxPokemon.Size);
        DaycareMail.ReadFrom(body, offset + BoxPokemon.Size);
        Steps = Bin.U32(body, offset + BoxPokemon.Size + DaycareMail.Size);
    }
}

public sealed class Daycare : ISaveBlock
{
    public const int SlotCount = 2;

    private const int OffspringPersonalityOffset = 0x1D8;
    private const int StepCounterOffset = 0x1DC;
    private const int MonDataLanguage = 0xC;
    private const ushort EndOfString = 0xFFFF;

    public readonly DaycareMon[] Mons = { new DaycareMon(), new DaycareMon() };
    public int OffspringPersonality;
    public byte StepCounter;

    public void WriteTo(byte[] body, int offset)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            Mons[i].WriteTo(body, offset + i * DaycareMon.Size);
        }
        Bin.W32(body, offset + OffspringPersonalityOffset, (uint)OffspringPersonality);
        body[offset + StepCounterOffset] = StepCounter;
    }

    public void ReadFrom(byte[] body, int offset)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            Mons[i].ReadFrom(body, offset + i * DaycareMon.Size);
        }
        OffspringPersonality = (int)Bin.U32(body, offset + OffspringPersonalityOffset);
        StepCounter = body[offset + StepCounterOffset];
    }

    // OFFSET 0x020261EC
    public static void Daycare_Init(Daycare daycare)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            DaycareMon mon = daycare.Mons[i];
            mon.BoxMon.Clear(0, BoxPokemon.Size);
            mon.DaycareMail.Mail.Clear();
            Array.Clear(mon.DaycareMail.TrainerName);
            Array.Clear(mon.DaycareMail.MonName);
            mon.Steps = 0;
        }
        Pokemon.BoxPokemon_Init(daycare.Mons[0].BoxMon);
        Pokemon.BoxPokemon_Init(daycare.Mons[1].BoxMon);
        daycare.OffspringPersonality = 0;
        daycare.StepCounter = 0;
    }

    // OFFSET 0x02026218
    public static DaycareMon Daycare_GetDaycareMon(Daycare daycare, int slot)
    {
        return daycare.Mons[slot];
    }

    // OFFSET 0x02026220
    public static BoxPokemon DaycareMon_GetBoxMon(DaycareMon daycareMon)
    {
        return daycareMon.BoxMon;
    }

    // OFFSET 0x02026224
    public static DaycareMail DaycareMon_GetDaycareMail(DaycareMon daycareMon)
    {
        return daycareMon.DaycareMail;
    }

    // OFFSET 0x02026228
    public static uint DaycareMon_GetSteps(DaycareMon daycareMon)
    {
        return daycareMon.Steps;
    }

    // OFFSET 0x02026230
    public static Mail DaycareMail_GetMail(DaycareMail daycareMail)
    {
        return daycareMail.Mail;
    }

    // OFFSET 0x02026234
    public static bool Daycare_HasEgg(Daycare daycare)
    {
        return daycare.OffspringPersonality != 0;
    }

    // OFFSET 0x02026248
    public static int Daycare_GetOffspringPersonality(Daycare daycare)
    {
        return daycare.OffspringPersonality;
    }

    // OFFSET 0x02026250
    public static int Daycare_GetStepCounter(Daycare daycare)
    {
        return daycare.StepCounter;
    }

    // OFFSET 0x02026258
    public static void DaycareMon_SetSteps(DaycareMon daycareMon, int steps)
    {
        daycareMon.Steps = (uint)steps;
    }

    // OFFSET 0x02026260
    public static void DaycareMon_AddSteps(DaycareMon daycareMon, int steps)
    {
        daycareMon.Steps += (uint)steps;
    }

    // OFFSET 0x02026270
    public static void Daycare_SetOffspringPersonality(Daycare daycare, int personality)
    {
        daycare.OffspringPersonality = personality;
    }

    // OFFSET 0x02026278
    public static void Daycare_SetStepCounter(Daycare daycare, int steps)
    {
        daycare.StepCounter = (byte)steps;
    }

    // OFFSET 0x02026280
    public static bool Daycare_AreParentLanguagesDifferent(Daycare daycare)
    {
        uint first = Pokemon.BoxPokemon_GetValue(daycare.Mons[0].BoxMon, MonDataLanguage, null);
        uint second = Pokemon.BoxPokemon_GetValue(daycare.Mons[1].BoxMon, MonDataLanguage, null);
        return first != second;
    }

    // OFFSET 0x020262A8
    public static void DaycareMon_CopyToDaycareMon(DaycareMon destination, DaycareMon source)
    {
        byte[] buffer = new byte[DaycareMon.Size];
        source.WriteTo(buffer, 0);
        destination.ReadFrom(buffer, 0);
    }

    // OFFSET 0x020262C0
    private static void DaycareMail_Init(DaycareMail daycareMail)
    {
        Array.Clear(daycareMail.TrainerName);
        Array.Clear(daycareMail.MonName);
        daycareMail.TrainerName[0] = EndOfString;
        daycareMail.MonName[0] = EndOfString;
    }

    // OFFSET 0x020262F4
    public static void DaycareMon_Init(DaycareMon daycareMon)
    {
        Pokemon.BoxPokemon_Init(daycareMon.BoxMon);
        daycareMon.Steps = 0;
        DaycareMail_Init(daycareMon.DaycareMail);
    }

    // OFFSET 0x02026310
    public static Daycare SaveData_GetDaycare(SaveData saveData)
    {
        return (Daycare)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryDaycare);
    }
}
