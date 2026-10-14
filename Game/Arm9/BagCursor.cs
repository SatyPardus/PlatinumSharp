namespace PlatinumSharp.Game;

public sealed class BagCursor
{
    public const int Size = 0x24;
    public const int FieldPocketCount = 8;
    public const int BattleCategoryCount = 5;

    public readonly byte[] FieldScroll = new byte[FieldPocketCount];
    public readonly byte[] FieldIndex = new byte[FieldPocketCount];
    public ushort FieldPocket;
    public ushort FieldPadding;
    public readonly byte[] BattleScroll = new byte[BattleCategoryCount];
    public readonly byte[] BattleIndex = new byte[BattleCategoryCount];
    public ushort BattleLastUsedItem;
    public ushort BattleLastUsedCategory;
    public ushort BattleCurrentCategory;

    // OFFSET 0x0207D99C
    public static BagCursor BagCursor_New(int heapID)
    {
        BagCursor cursor = new();
        Array.Clear(cursor.FieldScroll);
        Array.Clear(cursor.FieldIndex);
        cursor.FieldPocket = 0;
        cursor.FieldPadding = 0;
        Array.Clear(cursor.BattleScroll);
        Array.Clear(cursor.BattleIndex);
        cursor.BattleLastUsedItem = 0;
        cursor.BattleLastUsedCategory = 0;
        cursor.BattleCurrentCategory = 0;
        return cursor;
    }

    // OFFSET 0x0207D9B4
    public static void BagCursor_GetFieldPocketPosition(BagCursor cursor, uint pocket, out byte index, out byte scroll)
    {
        index = cursor.FieldIndex[pocket];
        scroll = cursor.FieldScroll[pocket];
    }

    // OFFSET 0x0207D9C4
    public static ushort BagCursor_GetFieldPocket(BagCursor cursor)
    {
        return cursor.FieldPocket;
    }

    // OFFSET 0x0207D9C8
    public static void BagCursor_SetFieldPocketPosition(BagCursor cursor, uint pocket, byte index, byte scroll)
    {
        cursor.FieldIndex[pocket] = index;
        cursor.FieldScroll[pocket] = scroll;
    }

    // OFFSET 0x0207D9D4
    public static void BagCursor_SetFieldPocket(BagCursor cursor, ushort pocket)
    {
        cursor.FieldPocket = pocket;
    }

    // OFFSET 0x0207D9D8
    public static void BagCursor_GetBattleCategoryPosition(BagCursor cursor, uint category, out byte index, out byte scroll)
    {
        index = cursor.BattleIndex[category];
        scroll = cursor.BattleScroll[category];
    }

    // OFFSET 0x0207D9E4
    public static ushort BagCursor_GetLastUsedBattleItem(BagCursor cursor)
    {
        return cursor.BattleLastUsedItem;
    }

    // OFFSET 0x0207D9E8
    public static ushort BagCursor_GetLastUsedBattleItemCategory(BagCursor cursor)
    {
        return cursor.BattleLastUsedCategory;
    }

    // OFFSET 0x0207D9EC
    public static ushort BagCursor_GetBattleCurrentCategory(BagCursor cursor)
    {
        return cursor.BattleCurrentCategory;
    }

    // OFFSET 0x0207D9F0
    public static void BagCursor_SetBattleCategoryPosition(BagCursor cursor, uint category, byte index, byte scroll)
    {
        cursor.BattleIndex[category] = index;
        cursor.BattleScroll[category] = scroll;
    }

    // OFFSET 0x0207D9F8
    public static void BagCursor_ResetBattle(BagCursor cursor)
    {
        for (uint category = 0; category < BattleCategoryCount; category++)
        {
            BagCursor_SetBattleCategoryPosition(cursor, category, 0, 0);
        }
        BagCursor_SetBattleCurrentCategory(cursor, 0);
    }

    // OFFSET 0x0207DA1C
    public static void BagCursor_SetLastUsedBattleItem(BagCursor cursor, ushort item, ushort category)
    {
        cursor.BattleLastUsedItem = item;
        cursor.BattleLastUsedCategory = category;
    }

    // OFFSET 0x0207DA24
    public static void BagCursor_SetBattleCurrentCategory(BagCursor cursor, ushort category)
    {
        cursor.BattleCurrentCategory = category;
    }
}
