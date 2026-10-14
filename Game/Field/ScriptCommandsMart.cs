namespace PlatinumSharp.Game;

public static partial class ScriptCommands
{
    private const uint CommonMartItems = 0x020EBAFC;
    private const int CommonMartItemCount = 0x13;
    private const uint SpecialtyMartTable = 0x02100B1C;
    private const uint DecorMartTable = 0x02100AF8;
    private const uint SealMartTable = 0x02100B00;
    private const ushort MartListEnd = 0xFFFF;
    private const int MartBadgeCount = 8;

    private static void RegisterMart(Dictionary<uint, ScrCmdFunc> ported)
    {
        ported[0x02046AD4] = ScrCmd_PokeMartCommon;
        ported[0x02046B9C] = ScrCmd_PokeMartSpecialties;
        ported[0x02046BF4] = ScrCmd_PokeMartDecor;
        ported[0x02046C34] = ScrCmd_PokeMartSeal;
    }

    private static ushort[] ReadMartList(uint address)
    {
        List<ushort> items = new();
        for (uint offset = 0; ; offset += 2)
        {
            ushort item = RomData.Arm9U16(address + offset, 1)[0];
            items.Add(item);
            if (item == MartListEnd)
            {
                break;
            }
        }
        return items.ToArray();
    }

    // OFFSET 0x02046AD4
    private static bool ScrCmd_PokeMartCommon(ScriptContext ctx)
    {
        VarValue(ctx);
        byte badges = 0;
        for (byte badge = 0; badge < MartBadgeCount; badge++)
        {
            if (TrainerInfo.TrainerInfo_HasBadge(PlayerSave.SaveData_GetTrainerInfo(ctx.FieldSystem.SaveData!), badge))
            {
                badges++;
            }
        }
        int martLevel;
        switch (badges)
        {
            case 0:
                martLevel = 1;
                break;
            case 1:
            case 2:
                martLevel = 2;
                break;
            case 3:
            case 4:
                martLevel = 3;
                break;
            case 5:
            case 6:
                martLevel = 4;
                break;
            case 7:
                martLevel = 5;
                break;
            case 8:
                martLevel = 6;
                break;
            default:
                martLevel = 1;
                break;
        }
        ushort[] table = RomData.Arm9U16(CommonMartItems, CommonMartItemCount * 2);
        List<ushort> items = new();
        for (byte i = 0; i < CommonMartItemCount; i++)
        {
            if (martLevel >= table[i * 2 + 1])
            {
                items.Add(table[i * 2]);
            }
        }
        items.Add(MartListEnd);
        ShopMenu.Shop_Start(ctx.Task!, ctx.FieldSystem, items.ToArray(), ShopMenu.MartTypeNormal, 0);
        return true;
    }

    // OFFSET 0x02046B9C
    private static bool ScrCmd_PokeMartSpecialties(ScriptContext ctx)
    {
        ushort martID = VarValue(ctx);
        int isDepartmentStore = (martID >= 8 && martID <= 0xD) || martID == 0x13 ? 1 : 0;
        uint list = RomData.Arm9U32((uint)(SpecialtyMartTable + martID * 4), 1)[0];
        ShopMenu.Shop_Start(ctx.Task!, ctx.FieldSystem, ReadMartList(list), ShopMenu.MartTypeNormal, isDepartmentStore);
        return true;
    }

    // OFFSET 0x02046BF4
    private static bool ScrCmd_PokeMartDecor(ScriptContext ctx)
    {
        ushort martID = VarValue(ctx);
        int isDepartmentStore = martID <= 1 ? 1 : 0;
        uint list = RomData.Arm9U32((uint)(DecorMartTable + martID * 4), 1)[0];
        ShopMenu.Shop_Start(ctx.Task!, ctx.FieldSystem, ReadMartList(list), ShopMenu.MartTypeDecor, isDepartmentStore);
        return true;
    }

    // OFFSET 0x02046C34
    private static bool ScrCmd_PokeMartSeal(ScriptContext ctx)
    {
        ushort martID = VarValue(ctx);
        uint list = RomData.Arm9U32((uint)(SealMartTable + martID * 4), 1)[0];
        ShopMenu.Shop_Start(ctx.Task!, ctx.FieldSystem, ReadMartList(list), ShopMenu.MartTypeSeal, 0);
        return true;
    }
}
