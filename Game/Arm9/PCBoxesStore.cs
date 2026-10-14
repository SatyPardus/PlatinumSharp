namespace PlatinumSharp.Game;

public sealed partial class Pokemon
{
    private const int ParamMove1MaxPP = 0x42;

    // OFFSET 0x0207896C
    public static void BoxPokemon_RestorePP(BoxPokemon boxMon)
    {
        int entered = BoxPokemon_EnterDecryptionContext(boxMon);
        for (int i = 0; i < MaxMoves; i++)
        {
            if (BoxPokemon_GetValue(boxMon, ParamMove1 + i, null) != 0)
            {
                byte maxPP = (byte)BoxPokemon_GetValue(boxMon, ParamMove1MaxPP + i, null);
                BoxPokemon_SetValue(boxMon, ParamMove1PP + i, maxPP);
            }
        }
        BoxPokemon_ExitDecryptionContext(boxMon, entered);
    }
}

public sealed partial class PCBoxes
{
    public const int CurrentBox = -1;

    private const int ParamSpecies = 5;
    private const int ParamSpeciesExists = 0xAC;
    private const int ParamIsEgg = 0x4C;

    private static BoxPokemon PCBoxes_MonAt(PCBoxes boxes, int box, int slot)
    {
        return new BoxPokemon(boxes.Buffer, MonsOffset + box * BoxSize + slot * BoxPokemon.Size);
    }

    // OFFSET 0x02079868
    public static bool PCBoxes_TryStoreBoxMon(PCBoxes boxes, BoxPokemon boxMon)
    {
        int box = (int)boxes.U32(0);
        do
        {
            Pokemon.BoxPokemon_RestorePP(boxMon);
            if (PCBoxes_TryStoreBoxMonInBox(boxes, box, boxMon))
            {
                SaveData.SaveData_SetFullSaveRequired();
                return true;
            }
            box++;
            if (box >= BoxCount)
            {
                box = 0;
            }
        }
        while (box != boxes.U32(0));
        return false;
    }

    // OFFSET 0x020798A0
    public static bool PCBoxes_TryStoreBoxMonInBox(PCBoxes boxes, int box, BoxPokemon boxMon)
    {
        Pokemon.BoxPokemon_RestorePP(boxMon);
        Pokemon.BoxPokemon_SetShayminForm(boxMon, 0);
        if (box == CurrentBox)
        {
            box = (int)boxes.U32(0);
        }
        for (int slot = 0; slot < MonsPerBox; slot++)
        {
            BoxPokemon destination = PCBoxes_MonAt(boxes, box, slot);
            if (Pokemon.BoxPokemon_GetValue(destination, ParamSpecies, null) == 0)
            {
                Array.Copy(boxMon.Buffer, boxMon.Offset, destination.Buffer, destination.Offset, BoxPokemon.Size);
                SaveData.SaveData_SetFullSaveRequired();
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x02079914
    public static bool PCBoxes_TryStoreBoxMonAt(PCBoxes boxes, int box, int slot, BoxPokemon boxMon)
    {
        Pokemon.BoxPokemon_RestorePP(boxMon);
        Pokemon.BoxPokemon_SetShayminForm(boxMon, 0);
        if (box == CurrentBox)
        {
            box = (int)boxes.U32(0);
        }
        if ((uint)box < BoxCount && (uint)slot < MonsPerBox)
        {
            BoxPokemon destination = PCBoxes_MonAt(boxes, box, slot);
            Array.Copy(boxMon.Buffer, boxMon.Offset, destination.Buffer, destination.Offset, BoxPokemon.Size);
            SaveData.SaveData_SetFullSaveRequired();
            return true;
        }
        ErrorHandling.GF_AssertFail();
        return false;
    }

    // OFFSET 0x02079968
    public static void PCBoxes_InitBoxMonAt(PCBoxes boxes, int box, int slot)
    {
        if (box == CurrentBox)
        {
            box = (int)boxes.U32(0);
        }
        if ((uint)slot < MonsPerBox && (uint)box < BoxCount)
        {
            Pokemon.BoxPokemon_Init(PCBoxes_MonAt(boxes, box, slot));
            SaveData.SaveData_SetFullSaveRequired();
            return;
        }
        ErrorHandling.GF_AssertFail();
    }

    // OFFSET 0x0207999C
    public static int PCBoxes_GetCurrentBoxID(PCBoxes boxes)
    {
        return (int)boxes.U32(0);
    }

    // OFFSET 0x02079B98
    public static int PCBoxes_CountNonEggMonsInBox(PCBoxes boxes, int box)
    {
        if (box == CurrentBox)
        {
            box = (int)boxes.U32(0);
        }
        if ((uint)box >= BoxCount)
        {
            ErrorHandling.GF_AssertFail();
            return 0;
        }
        int count = 0;
        for (int slot = 0; slot < MonsPerBox; slot++)
        {
            BoxPokemon boxMon = PCBoxes_MonAt(boxes, box, slot);
            if (Pokemon.BoxPokemon_GetValue(boxMon, ParamSpeciesExists, null) != 0 && Pokemon.BoxPokemon_GetValue(boxMon, ParamIsEgg, null) == 0)
            {
                count++;
            }
        }
        return count;
    }

    // OFFSET 0x02079BEC
    public static int PCBoxes_CountAllNonEggMons(PCBoxes boxes)
    {
        int count = 0;
        for (int box = 0; box < BoxCount; box++)
        {
            count += PCBoxes_CountNonEggMonsInBox(boxes, box);
        }
        return count;
    }

    // OFFSET 0x02079C9C
    public static BoxPokemon PCBoxes_GetBoxMonAt(PCBoxes boxes, int box, int slot)
    {
        if ((uint)box >= BoxCount && box != CurrentBox)
        {
            ErrorHandling.GF_AssertFail();
        }
        if ((uint)slot >= MonsPerBox)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (box == CurrentBox)
        {
            box = (int)boxes.U32(0);
        }
        return PCBoxes_MonAt(boxes, box, slot);
    }

    // OFFSET 0x02079A94
    public static void PCBoxes_SetCurrentBox(PCBoxes boxes, int box)
    {
        if ((uint)box < BoxCount)
        {
            boxes.W32(0, (uint)box);
            SaveData.SaveData_SetFullSaveRequired();
            return;
        }
        ErrorHandling.GF_AssertFail();
    }
}
