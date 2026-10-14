using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class SpriteResourceData
{
    public CharacterData? Character;
    public PaletteFile? Palette;
    public CellDataBank? Cells;
    public AnimBankData? CellAnims;
    public MultiCellDataBank? MultiCells;
    public MultiCellAnimBankData? MultiCellAnims;
    public int VramType;
    public int PaletteCount;
}

public sealed class SpriteResource
{
    public const int TypeChar = 0;
    public const int TypePltt = 1;
    public const int TypeCell = 2;
    public const int TypeAnim = 3;
    public const int TypeMultiCell = 4;
    public const int TypeMultiAnim = 5;

    public Resource? Resource;
    public int Type;
    public SpriteResourceData? Data;

    // OFFSET 0x02009D4C
    public static void SpriteResource_ReleaseData(SpriteResource? spriteRes)
    {
        if (spriteRes == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResource_FreeData(spriteRes!);
        ResourceCollection.Resource_SetData(spriteRes!.Resource, null);
    }

    // OFFSET 0x02009E08
    public static int SpriteResource_GetID(SpriteResource? spriteRes)
    {
        if (spriteRes == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return ResourceCollection.Resource_GetID(spriteRes!.Resource);
    }

    // OFFSET 0x02009E1C
    public static CharacterData? SpriteResource_GetTileData(SpriteResource spriteRes)
    {
        if (spriteRes.Type != TypeChar)
        {
            ErrorHandling.GF_AssertFail();
        }
        return SpriteResource_GetData(spriteRes).Character;
    }

    // OFFSET 0x02009E34
    public static PaletteFile? SpriteResource_GetPaletteFade(SpriteResource spriteRes)
    {
        if (spriteRes.Type != TypePltt)
        {
            ErrorHandling.GF_AssertFail();
        }
        return SpriteResource_GetData(spriteRes).Palette;
    }

    // OFFSET 0x02009E4C
    public static CellDataBank? SpriteResource_GetSpriteData(SpriteResource spriteRes)
    {
        if (spriteRes.Type != TypeCell)
        {
            ErrorHandling.GF_AssertFail();
        }
        return SpriteResource_GetData(spriteRes).Cells;
    }

    // OFFSET 0x02009E64
    public static AnimBankData? SpriteResource_GetSpriteAnimData(SpriteResource spriteRes)
    {
        if (spriteRes.Type != TypeAnim)
        {
            ErrorHandling.GF_AssertFail();
        }
        return SpriteResource_GetData(spriteRes).CellAnims;
    }

    // OFFSET 0x02009E7C
    public static MultiCellDataBank? SpriteResource_GetMultiSpriteData(SpriteResource spriteRes)
    {
        if (spriteRes.Type != TypeMultiCell)
        {
            ErrorHandling.GF_AssertFail();
        }
        return SpriteResource_GetData(spriteRes).MultiCells;
    }

    // OFFSET 0x02009E94
    public static MultiCellAnimBankData? SpriteResource_GetMultiSpriteAnimData(SpriteResource spriteRes)
    {
        if (spriteRes.Type != TypeMultiAnim)
        {
            ErrorHandling.GF_AssertFail();
        }
        return SpriteResource_GetData(spriteRes).MultiCellAnims;
    }

    // OFFSET 0x02009EAC
    public static int SpriteResource_GetType(SpriteResource? spriteRes)
    {
        if (spriteRes == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return spriteRes!.Type;
    }

    // OFFSET 0x02009EBC
    public static int SpriteResource_GetVRAMType(SpriteResource? spriteRes)
    {
        if (spriteRes == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteRes!.Type == TypeChar)
        {
            return SpriteResource_GetData(spriteRes).VramType;
        }
        if (spriteRes.Type == TypePltt)
        {
            return SpriteResource_GetData(spriteRes).VramType;
        }
        return 0;
    }

    // OFFSET 0x02009EE8
    public static int SpriteResource_GetPaletteIndex(SpriteResource? spriteRes)
    {
        if (spriteRes == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteRes!.Type == TypePltt)
        {
            return SpriteResource_GetData(spriteRes).PaletteCount;
        }
        return 0;
    }

    // OFFSET 0x02009F08
    public static void SpriteResource_SetVRAMType(SpriteResource? spriteRes, int vramType)
    {
        if (spriteRes == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteRes!.Type == TypeChar)
        {
            SpriteResource_GetData(spriteRes).VramType = vramType;
        }
        if (spriteRes.Type == TypePltt)
        {
            SpriteResource_GetData(spriteRes).VramType = vramType;
        }
    }

    // OFFSET 0x0200A0D4
    internal static void SpriteResource_UnpackData(SpriteResource spriteRes, int type, int vramType, int paletteCount, int heapID)
    {
        byte[] data = ResourceCollection.Resource_GetData(spriteRes.Resource)!;
        switch (type)
        {
            case TypeChar:
                spriteRes.Data = SpriteUtil_UnpackTileResource(data, vramType, heapID);
                break;
            case TypePltt:
                spriteRes.Data = SpriteUtil_UnpackPaletteResource(data, vramType, paletteCount, heapID);
                break;
            case TypeCell:
                spriteRes.Data = SpriteUtil_UnpackSpriteResource(data, heapID);
                break;
            case TypeAnim:
                spriteRes.Data = SpriteUtil_UnpackSpriteAnimResource(data, heapID);
                break;
            case TypeMultiCell:
                spriteRes.Data = SpriteUtil_UnpackMultiSpriteResource(data, heapID);
                break;
            case TypeMultiAnim:
                spriteRes.Data = SpriteUtil_UnpackMultiSpriteAnimResource(data, heapID);
                break;
        }
    }

    // OFFSET 0x0200A144
    private static SpriteResourceData SpriteUtil_UnpackTileResource(byte[] data, int vramType, int heapID)
    {
        SpriteResourceData tileData = new();
        G2d.GetUnpackedCharacterData(data, out tileData.Character);
        tileData.VramType = vramType;
        return tileData;
    }

    // OFFSET 0x0200A164
    private static SpriteResourceData SpriteUtil_UnpackPaletteResource(byte[] data, int vramType, int paletteCount, int heapID)
    {
        SpriteResourceData paletteData = new();
        G2d.GetUnpackedPaletteData(data, out paletteData.Palette);
        paletteData.VramType = vramType;
        paletteData.PaletteCount = paletteCount;
        return paletteData;
    }

    // OFFSET 0x0200A188
    private static SpriteResourceData SpriteUtil_UnpackSpriteResource(byte[] data, int heapID)
    {
        SpriteResourceData spriteData = new();
        G2d.GetUnpackedCellBank(data, out spriteData.Cells);
        return spriteData;
    }

    // OFFSET 0x0200A1A4
    private static SpriteResourceData SpriteUtil_UnpackSpriteAnimResource(byte[] data, int heapID)
    {
        SpriteResourceData animData = new();
        G2d.GetUnpackedAnimBank(data, out animData.CellAnims);
        return animData;
    }

    // OFFSET 0x0200A1C0
    private static SpriteResourceData SpriteUtil_UnpackMultiSpriteResource(byte[] data, int heapID)
    {
        SpriteResourceData multiSpriteData = new();
        G2d.GetUnpackedMultiCellBank(data, out multiSpriteData.MultiCells);
        return multiSpriteData;
    }

    // OFFSET 0x0200A1DC
    private static SpriteResourceData SpriteUtil_UnpackMultiSpriteAnimResource(byte[] data, int heapID)
    {
        SpriteResourceData multiAnimData = new();
        G2d.GetUnpackedMultiCellAnimBank(data, out multiAnimData.MultiCellAnims);
        return multiAnimData;
    }

    // OFFSET 0x0200A1F8
    internal static void SpriteResource_FreeData(SpriteResource spriteRes)
    {
        spriteRes.Data = null;
    }

    // OFFSET 0x0200A20C
    private static SpriteResourceData SpriteResource_GetData(SpriteResource? spriteRes)
    {
        if (spriteRes == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteRes!.Data == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return spriteRes.Data!;
    }
}

public sealed class SpriteResourceList
{
    public SpriteResource?[] Resources = Array.Empty<SpriteResource?>();
    public int Capacity;
    public int Count;

    // OFFSET 0x02009CFC
    public static SpriteResourceList SpriteResourceList_New(int capacity, int heapID)
    {
        SpriteResourceList list = new();
        list.Resources = new SpriteResource?[capacity];
        list.Capacity = capacity;
        list.Count = 0;
        return list;
    }

    // OFFSET 0x02009D20
    public static void SpriteResourceList_Delete(SpriteResourceList list)
    {
        list.Resources = Array.Empty<SpriteResource?>();
    }
}

public sealed class SpriteResourceTableEntry
{
    public int NarcID;
    public int MemberIndex;
    public int Compressed;
    public int ID;
    public string Filename = string.Empty;
    public int VramType;
    public int PaletteCount;
}

public sealed class SpriteResourceTable
{
    private const int NarcEntrySize = 0x18;
    private const int EndOfTable = -2;

    public SpriteResourceTableEntry[]? Entries;
    public int Count;
    public int Type;
    public bool IsNARC;

    // OFFSET 0x02009F34
    public static int SpriteResourceTable_Size()
    {
        return 0x10;
    }

    // OFFSET 0x02009F38
    public static SpriteResourceTable SpriteResourceTable_GetArrayElement(SpriteResourceTable[] tables, int index)
    {
        return tables[index];
    }

    // OFFSET 0x02009F40
    public static void SpriteResourceTable_LoadFromBinary(byte[] data, SpriteResourceTable? table, int heapID)
    {
        if (table == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        table!.Type = Bin.I32(data, 0);
        table.IsNARC = true;
        table.Count = SpriteResourceTableEntryNARC_GetEntryCount(data, 4);
        table.Entries = table.Count > 0 ? new SpriteResourceTableEntry[table.Count] : null;
        if (table.Entries == null)
        {
            return;
        }
        for (int i = 0; i < table.Count; i++)
        {
            int offset = 4 + i * NarcEntrySize;
            table.Entries[i] = new SpriteResourceTableEntry
            {
                NarcID = Bin.I32(data, offset),
                MemberIndex = Bin.I32(data, offset + 4),
                Compressed = Bin.I32(data, offset + 8),
                ID = Bin.I32(data, offset + 0xC),
                VramType = Bin.I32(data, offset + 0x10),
                PaletteCount = Bin.I32(data, offset + 0x14),
            };
        }
    }

    // OFFSET 0x02009F8C
    public static void SpriteResourceTable_Clear(SpriteResourceTable table)
    {
        table.Entries = null;
        table.Count = 0;
    }

    // OFFSET 0x02009FA4
    public static int SpriteResourceTable_GetCount(SpriteResourceTable? table)
    {
        if (table == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return table!.Count;
    }

    // OFFSET 0x02009FB4
    public static int SpriteResourceTable_GetEntryID(SpriteResourceTable? table, int index)
    {
        if (table == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (table!.Count <= index)
        {
            ErrorHandling.GF_AssertFail();
        }
        return table.Entries![index].ID;
    }

    // OFFSET 0x02009FE8
    public static int SpriteResourceTable_GetNARCEntryMemberIndex(SpriteResourceTable? table, int index)
    {
        int memberIndex = 0;
        if (table == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (table!.Count <= index)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (table.IsNARC)
        {
            memberIndex = table.Entries![index].MemberIndex;
        }
        return memberIndex;
    }

    // OFFSET 0x0200A014
    public static int SpriteResourceTable_IsNARCEntryCompressed(SpriteResourceTable? table, int index)
    {
        int compressed = 0;
        if (table == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (table!.Count <= index)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (table.IsNARC)
        {
            compressed = table.Entries![index].Compressed;
        }
        return compressed;
    }

    // OFFSET 0x0200A040
    public static int SpriteResourceTable_GetEntryVRAMType(SpriteResourceTable? table, int index)
    {
        if (table == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (table!.Count <= index)
        {
            ErrorHandling.GF_AssertFail();
        }
        return table.Entries![index].VramType;
    }

    // OFFSET 0x0200A074
    public static int SpriteResourceTable_GetPaletteIndex(SpriteResourceTable? table, int index)
    {
        if (table == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (table!.Count <= index)
        {
            ErrorHandling.GF_AssertFail();
        }
        return table.Entries![index].PaletteCount;
    }

    // OFFSET 0x0200A2C0
    private static int SpriteResourceTableEntryNARC_GetEntryCount(byte[] data, int offset)
    {
        int count = 0;
        while (Bin.I32(data, offset + count * NarcEntrySize) != EndOfTable)
        {
            count++;
        }
        return count;
    }
}

public sealed class SpriteResourceCollection
{
    public ResourceCollection? Collection;
    public SpriteResource[]? Resources;
    public int Capacity;
    public int Count;
    public int Type;

    // OFFSET 0x02009714
    public static SpriteResourceCollection SpriteResourceCollection_New(int capacity, int type, int heapID)
    {
        SpriteResourceCollection spriteResources = new();
        spriteResources.Collection = ResourceCollection.ResourceCollection_New(capacity, heapID);
        spriteResources.Resources = new SpriteResource[capacity];
        for (int i = 0; i < capacity; i++)
        {
            spriteResources.Resources[i] = new SpriteResource();
        }
        spriteResources.Capacity = capacity;
        spriteResources.Count = 0;
        spriteResources.Type = type;
        return spriteResources;
    }

    // OFFSET 0x02009754
    public static void SpriteResourceCollection_Delete(SpriteResourceCollection? spriteResources)
    {
        if (spriteResources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteResources!.Collection == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteResources.Resources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResourceCollection_Clear(spriteResources);
        ResourceCollection.ResourceCollection_Delete(spriteResources.Collection);
        spriteResources.Collection = null;
        spriteResources.Resources = null;
    }

    // OFFSET 0x02009794
    public static SpriteResource SpriteResourceCollection_AddFromTable(SpriteResourceCollection? spriteResources, SpriteResourceTable? table, int index, int heapID)
    {
        if (spriteResources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (table == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (table!.Count <= index)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteResources!.Type != table.Type)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResource? spriteRes = SpriteResourceCollection_AllocResource(spriteResources);
        if (spriteRes == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResourceTableEntry entry = table.Entries![index];
        if (!SpriteResourceCollection_IsIDUnused(spriteResources, entry.ID))
        {
            ErrorHandling.GF_AssertFail();
        }
        if (!table.IsNARC)
        {
            SpriteResourceCollection_InitResFromFile(spriteResources, spriteRes!, entry.Filename, entry.ID, entry.VramType, entry.PaletteCount, table.Type, heapID);
        }
        else
        {
            SpriteResourceCollection_InitRes(spriteResources, spriteRes!, entry.NarcID, entry.MemberIndex, entry.Compressed, entry.ID, entry.VramType, entry.PaletteCount, table.Type, heapID, 0);
        }
        spriteResources.Count++;
        return spriteRes!;
    }

    // OFFSET 0x0200985C
    public static SpriteResource SpriteResourceCollection_AddTiles(SpriteResourceCollection? spriteResources, int narcID, int memberIndex, int compressed, int id, int vramType, int heapID)
    {
        if (spriteResources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteResources!.Type != SpriteResource.TypeChar)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResource? spriteRes = SpriteResourceCollection_AllocResource(spriteResources);
        if (spriteRes == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResourceCollection_InitRes(spriteResources, spriteRes!, narcID, memberIndex, compressed, id, vramType, 0, SpriteResource.TypeChar, heapID, 0);
        spriteResources.Count++;
        return spriteRes!;
    }

    // OFFSET 0x020098B8
    public static SpriteResource SpriteResourceCollection_AddPalette(SpriteResourceCollection? spriteResources, int narcID, int memberIndex, int compressed, int id, int vramType, int paletteCount, int heapID)
    {
        if (spriteResources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteResources!.Type != SpriteResource.TypePltt)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResource? spriteRes = SpriteResourceCollection_AllocResource(spriteResources);
        if (spriteRes == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResourceCollection_InitRes(spriteResources, spriteRes!, narcID, memberIndex, compressed, id, vramType, paletteCount, SpriteResource.TypePltt, heapID, 0);
        spriteResources.Count++;
        return spriteRes!;
    }

    // OFFSET 0x02009918
    public static SpriteResource SpriteResourceCollection_Add(SpriteResourceCollection? spriteResources, int narcID, int memberIndex, int compressed, int id, int type, int heapID)
    {
        if (spriteResources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResource? spriteRes = SpriteResourceCollection_AllocResource(spriteResources!);
        if (spriteRes == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResourceCollection_InitRes(spriteResources!, spriteRes!, narcID, memberIndex, compressed, id, 0, 0, type, heapID, 0);
        spriteResources!.Count++;
        return spriteRes!;
    }

    // OFFSET 0x02009968
    public static void SpriteResourceCollection_ModifyTiles(SpriteResourceCollection? spriteResources, SpriteResource? spriteRes, int narcID, int memberIndex, int compressed, int heapID)
    {
        if (spriteResources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteResources!.Type != SpriteResource.TypeChar)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteRes == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteRes!.Type != SpriteResource.TypeChar)
        {
            ErrorHandling.GF_AssertFail();
        }
        int id = SpriteResource.SpriteResource_GetID(spriteRes);
        int vramType = SpriteResource.SpriteResource_GetVRAMType(spriteRes);
        SpriteResourceCollection_Remove(spriteResources, spriteRes);
        SpriteResourceCollection_InitRes(spriteResources, spriteRes, narcID, memberIndex, compressed, id, vramType, 0, SpriteResource.TypeChar, heapID, 0);
    }

    // OFFSET 0x020099D4
    public static void SpriteResourceCollection_ModifyPalette(SpriteResourceCollection? spriteResources, SpriteResource? spriteRes, int narcID, int memberIndex, int compressed, int heapID)
    {
        if (spriteResources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteResources!.Type != SpriteResource.TypePltt)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteRes == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteRes!.Type != SpriteResource.TypePltt)
        {
            ErrorHandling.GF_AssertFail();
        }
        int id = SpriteResource.SpriteResource_GetID(spriteRes);
        int vramType = SpriteResource.SpriteResource_GetVRAMType(spriteRes);
        int paletteCount = SpriteResource.SpriteResource_GetPaletteIndex(spriteRes);
        SpriteResourceCollection_Remove(spriteResources, spriteRes);
        SpriteResourceCollection_InitRes(spriteResources, spriteRes, narcID, memberIndex, compressed, id, vramType, paletteCount, SpriteResource.TypePltt, heapID, 0);
    }

    // OFFSET 0x02009A4C
    public static SpriteResource SpriteResourceCollection_AddTilesFrom(SpriteResourceCollection? spriteResources, NARC narc, int memberIndex, int compressed, int id, int vramType, int heapID)
    {
        if (spriteResources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteResources!.Type != SpriteResource.TypeChar)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResource? spriteRes = SpriteResourceCollection_AllocResource(spriteResources);
        if (spriteRes == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResourceCollection_InitResFromNARC(spriteResources, spriteRes!, narc, memberIndex, compressed, id, vramType, 0, SpriteResource.TypeChar, heapID, 0);
        spriteResources.Count++;
        return spriteRes!;
    }

    // OFFSET 0x02009AA8
    public static SpriteResource SpriteResourceCollection_AddTilesFromEx(SpriteResourceCollection? spriteResources, NARC narc, int memberIndex, int compressed, int id, int vramType, int heapID, int allocAtEnd)
    {
        if (spriteResources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteResources!.Type != SpriteResource.TypeChar)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResource? spriteRes = SpriteResourceCollection_AllocResource(spriteResources);
        if (spriteRes == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResourceCollection_InitResFromNARC(spriteResources, spriteRes!, narc, memberIndex, compressed, id, vramType, 0, SpriteResource.TypeChar, heapID, allocAtEnd);
        spriteResources.Count++;
        return spriteRes!;
    }

    // OFFSET 0x02009B04
    public static SpriteResource SpriteResourceCollection_AddPaletteFrom(SpriteResourceCollection? spriteResources, NARC narc, int memberIndex, int compressed, int id, int vramType, int paletteCount, int heapID)
    {
        if (spriteResources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteResources!.Type != SpriteResource.TypePltt)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResource? spriteRes = SpriteResourceCollection_AllocResource(spriteResources);
        if (spriteRes == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResourceCollection_InitResFromNARC(spriteResources, spriteRes!, narc, memberIndex, compressed, id, vramType, paletteCount, SpriteResource.TypePltt, heapID, 0);
        spriteResources.Count++;
        return spriteRes!;
    }

    // OFFSET 0x02009B64
    public static SpriteResource SpriteResourceCollection_AddPaletteFromEx(SpriteResourceCollection? spriteResources, NARC narc, int memberIndex, int compressed, int id, int vramType, int paletteCount, int heapID, int allocAtEnd)
    {
        if (spriteResources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteResources!.Type != SpriteResource.TypePltt)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResource? spriteRes = SpriteResourceCollection_AllocResource(spriteResources);
        if (spriteRes == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResourceCollection_InitResFromNARC(spriteResources, spriteRes!, narc, memberIndex, compressed, id, vramType, paletteCount, SpriteResource.TypePltt, heapID, allocAtEnd);
        spriteResources.Count++;
        return spriteRes!;
    }

    // OFFSET 0x02009BC4
    public static SpriteResource SpriteResourceCollection_AddFrom(SpriteResourceCollection? spriteResources, NARC narc, int memberIndex, int compressed, int id, int type, int heapID)
    {
        if (spriteResources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResource? spriteRes = SpriteResourceCollection_AllocResource(spriteResources!);
        if (spriteRes == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResourceCollection_InitResFromNARC(spriteResources!, spriteRes!, narc, memberIndex, compressed, id, 0, 0, type, heapID, 0);
        spriteResources!.Count++;
        return spriteRes!;
    }

    // OFFSET 0x02009C14
    public static void SpriteResourceCollection_ModifyTilesFrom(SpriteResourceCollection? spriteResources, SpriteResource? spriteRes, NARC narc, int memberIndex, int compressed, int heapID)
    {
        if (spriteResources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteResources!.Type != SpriteResource.TypeChar)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteRes == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteRes!.Type != SpriteResource.TypeChar)
        {
            ErrorHandling.GF_AssertFail();
        }
        int id = SpriteResource.SpriteResource_GetID(spriteRes);
        int vramType = SpriteResource.SpriteResource_GetVRAMType(spriteRes);
        SpriteResourceCollection_Remove(spriteResources, spriteRes);
        SpriteResourceCollection_InitResFromNARC(spriteResources, spriteRes, narc, memberIndex, compressed, id, vramType, 0, SpriteResource.TypeChar, heapID, 0);
    }

    // OFFSET 0x02009C80
    public static int SpriteResourceCollection_Extend(SpriteResourceCollection? spriteResources, SpriteResourceTable? table, SpriteResourceList? list, int heapID)
    {
        if (spriteResources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (table == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResourceCollection_ExtendEx(spriteResources!, table!, 0, table!.Count, list, heapID);
        return table.Count;
    }

    // OFFSET 0x02009CB4
    public static void SpriteResourceCollection_ExtendEx(SpriteResourceCollection spriteResources, SpriteResourceTable table, int first, int count, SpriteResourceList? list, int heapID)
    {
        for (int i = first; i < first + count; i++)
        {
            SpriteResource spriteRes = SpriteResourceCollection_AddFromTable(spriteResources, table, i, heapID);
            if (list != null && list.Capacity > list.Count)
            {
                list.Resources[list.Count] = spriteRes;
                list.Count++;
            }
        }
    }

    // OFFSET 0x02009D34
    public static bool SpriteResourceCollection_IsIDUnused(SpriteResourceCollection? spriteResources, int id)
    {
        if (spriteResources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return ResourceCollection.ResourceCollection_IsIDUnused(spriteResources!.Collection, id);
    }

    // OFFSET 0x02009D68
    public static void SpriteResourceCollection_Remove(SpriteResourceCollection? spriteResources, SpriteResource spriteRes)
    {
        if (spriteResources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteResources!.Resources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResource.SpriteResource_FreeData(spriteRes);
        ResourceCollection.ResourceCollection_Remove(spriteResources.Collection, spriteRes.Resource);
        spriteRes.Resource = null;
        spriteResources.Count--;
    }

    // OFFSET 0x02009D9C
    public static void SpriteResourceCollection_Clear(SpriteResourceCollection spriteResources)
    {
        for (int i = 0; i < spriteResources.Capacity; i++)
        {
            if (spriteResources.Resources![i].Resource != null)
            {
                SpriteResourceCollection_Remove(spriteResources, spriteResources.Resources[i]);
            }
        }
    }

    // OFFSET 0x02009DC8
    public static SpriteResource? SpriteResourceCollection_Find(SpriteResourceCollection? spriteResources, int id)
    {
        if (spriteResources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < spriteResources!.Capacity; i++)
        {
            SpriteResource spriteRes = spriteResources.Resources![i];
            if (spriteRes.Resource != null && ResourceCollection.Resource_GetID(spriteRes.Resource) == id)
            {
                return spriteRes;
            }
        }
        return null;
    }

    // OFFSET 0x0200A0A8
    private static SpriteResource? SpriteResourceCollection_AllocResource(SpriteResourceCollection spriteResources)
    {
        for (int i = 0; i < spriteResources.Capacity; i++)
        {
            if (spriteResources.Resources![i].Resource == null)
            {
                return spriteResources.Resources[i];
            }
        }
        return null;
    }

    // OFFSET 0x0200A224
    private static void SpriteResourceCollection_InitResFromFile(SpriteResourceCollection spriteResources, SpriteResource spriteRes, string filename, int id, int vramType, int paletteCount, int type, int heapID)
    {
        spriteRes.Resource = ResourceCollection.ResourceCollection_AddFromFile(spriteResources.Collection, filename, id, heapID);
        spriteRes.Type = type;
        SpriteResource.SpriteResource_UnpackData(spriteRes, type, vramType, paletteCount, heapID);
    }

    // OFFSET 0x0200A250
    private static void SpriteResourceCollection_InitRes(SpriteResourceCollection spriteResources, SpriteResource spriteRes, int narcID, int memberIndex, int compressed, int id, int vramType, int paletteCount, int type, int heapID, int allocAtEnd)
    {
        byte[]? data = Graphics.LoadMemberFromNARC(narcID, (uint)memberIndex, compressed, heapID, allocAtEnd);
        spriteRes.Resource = ResourceCollection.ResourceCollection_Add(spriteResources.Collection, data, id);
        spriteRes.Type = type;
        SpriteResource.SpriteResource_UnpackData(spriteRes, type, vramType, paletteCount, heapID);
    }

    // OFFSET 0x0200A288
    private static void SpriteResourceCollection_InitResFromNARC(SpriteResourceCollection spriteResources, SpriteResource spriteRes, NARC narc, int memberIndex, int compressed, int id, int vramType, int paletteCount, int type, int heapID, int allocAtEnd)
    {
        byte[]? data = SpriteUtil_ReadNARCMember(narc, memberIndex, compressed, heapID, allocAtEnd);
        spriteRes.Resource = ResourceCollection.ResourceCollection_Add(spriteResources.Collection, data, id);
        spriteRes.Type = type;
        SpriteResource.SpriteResource_UnpackData(spriteRes, type, vramType, paletteCount, heapID);
    }

    // OFFSET 0x0200A2DC
    private static byte[]? SpriteUtil_ReadNARCMember(NARC narc, int memberIndex, int compressed, int heapID, int allocAtEnd)
    {
        byte[]? data = NARC.NARC_AllocAndReadWholeMember(narc, (uint)memberIndex, heapID);
        if (data != null && compressed != 0)
        {
            data = Lz.TryDecompress(data);
        }
        return data;
    }
}
