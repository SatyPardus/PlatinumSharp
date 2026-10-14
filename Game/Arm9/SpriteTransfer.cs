using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class CharTransferTaskTemplate
{
    public CharacterData? Data;
    public int VramType;
    public int ResourceID;
    public int AtEnd;
}

public sealed class PlttTransferTaskTemplate
{
    public PaletteFile? Data;
    public int VramType;
    public int PaletteCount;
    public int ResourceID;
}

public static class SpriteTransfer
{
    // OFFSET 0x0200A328
    public static bool SpriteTransfer_RequestChar(SpriteResource? resource)
    {
        if (resource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (SpriteResource.SpriteResource_GetType(resource) != SpriteResource.TypeChar)
        {
            ErrorHandling.GF_AssertFail();
        }
        CharTransferTaskTemplate template = new()
        {
            Data = SpriteResource.SpriteResource_GetTileData(resource!),
            VramType = SpriteResource.SpriteResource_GetVRAMType(resource),
            ResourceID = SpriteResource.SpriteResource_GetID(resource),
            AtEnd = 0,
        };
        return CharTransfer.CharTransfer_Request(template);
    }

    // OFFSET 0x0200A368
    public static void SpriteTransfer_RequestCharList(SpriteResourceList? list)
    {
        if (list == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < list!.Capacity; i++)
        {
            if (list.Resources[i] != null && !SpriteTransfer_RequestChar(list.Resources[i]))
            {
                ErrorHandling.GF_AssertFail();
            }
        }
    }

    // OFFSET 0x0200A39C
    public static bool SpriteTransfer_RequestCharWithHardwareMappingType(SpriteResource? resource)
    {
        if (resource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (SpriteResource.SpriteResource_GetType(resource) != SpriteResource.TypeChar)
        {
            ErrorHandling.GF_AssertFail();
        }
        CharTransferTaskTemplate template = new()
        {
            Data = SpriteResource.SpriteResource_GetTileData(resource!),
            VramType = SpriteResource.SpriteResource_GetVRAMType(resource),
            ResourceID = SpriteResource.SpriteResource_GetID(resource),
            AtEnd = 0,
        };
        return CharTransfer.CharTransfer_RequestWithHardwareMappingType(template);
    }

    // OFFSET 0x0200A3DC
    public static bool SpriteTransfer_RequestCharAtEnd(SpriteResource? resource)
    {
        if (resource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (SpriteResource.SpriteResource_GetType(resource) != SpriteResource.TypeChar)
        {
            ErrorHandling.GF_AssertFail();
        }
        CharTransferTaskTemplate template = new()
        {
            Data = SpriteResource.SpriteResource_GetTileData(resource!),
            VramType = SpriteResource.SpriteResource_GetVRAMType(resource),
            ResourceID = SpriteResource.SpriteResource_GetID(resource),
            AtEnd = 1,
        };
        return CharTransfer.CharTransfer_Request(template);
    }

    // OFFSET 0x0200A41C
    public static void SpriteTransfer_RequestCharListAtEnd(SpriteResourceList? list)
    {
        if (list == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < list!.Capacity; i++)
        {
            if (list.Resources[i] != null && !SpriteTransfer_RequestCharAtEnd(list.Resources[i]))
            {
                ErrorHandling.GF_AssertFail();
            }
        }
    }

    // OFFSET 0x0200A450
    public static bool SpriteTransfer_RequestCharAtEndWithHardwareMappingType(SpriteResource? resource)
    {
        if (resource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (SpriteResource.SpriteResource_GetType(resource) != SpriteResource.TypeChar)
        {
            ErrorHandling.GF_AssertFail();
        }
        CharTransferTaskTemplate template = new()
        {
            Data = SpriteResource.SpriteResource_GetTileData(resource!),
            VramType = SpriteResource.SpriteResource_GetVRAMType(resource),
            ResourceID = SpriteResource.SpriteResource_GetID(resource),
            AtEnd = 1,
        };
        return CharTransfer.CharTransfer_RequestWithHardwareMappingType(template);
    }

    // OFFSET 0x0200A490
    public static void SpriteTransfer_ReplaceCharData(SpriteResource? destination, SpriteResource? source)
    {
        if (destination == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (source == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        int id = SpriteResource.SpriteResource_GetID(destination);
        CharTransfer.CharTransfer_ReplaceCharData(id, SpriteResource.SpriteResource_GetTileData(source!));
    }

    // OFFSET 0x0200A4C0
    public static void SpriteTransfer_RetransferCharData(SpriteResource? resource)
    {
        if (resource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        int id = SpriteResource.SpriteResource_GetID(resource);
        CharTransfer.CharTransfer_ReplaceCharData(id, SpriteResource.SpriteResource_GetTileData(resource!));
    }

    // OFFSET 0x0200A4E4
    public static void SpriteTransfer_ResetCharTransfer(SpriteResource? resource)
    {
        if (resource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (SpriteResource.SpriteResource_GetType(resource) != SpriteResource.TypeChar)
        {
            ErrorHandling.GF_AssertFail();
        }
        CharTransfer.CharTransfer_ResetTask(SpriteResource.SpriteResource_GetID(resource));
    }

    // OFFSET 0x0200A508
    public static void SpriteTransfer_ResetCharTransferList(SpriteResourceList? list)
    {
        if (list == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < list!.Capacity; i++)
        {
            if (list.Resources[i] != null)
            {
                SpriteTransfer_ResetCharTransfer(list.Resources[i]);
            }
        }
    }

    // OFFSET 0x0200A534
    public static ImageProxy? SpriteTransfer_GetImageProxy(SpriteResource? resource)
    {
        if (resource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (SpriteResource.SpriteResource_GetType(resource) != SpriteResource.TypeChar)
        {
            ErrorHandling.GF_AssertFail();
        }
        return CharTransfer.CharTransfer_GetImageProxy(SpriteResource.SpriteResource_GetID(resource));
    }

    // OFFSET 0x0200A558
    public static ImageProxy? SpriteTransfer_GetCellTransferProxy(SpriteResource? charResource, SpriteResource? cellResource)
    {
        if (charResource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (SpriteResource.SpriteResource_GetType(charResource) != SpriteResource.TypeChar)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (cellResource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (SpriteResource.SpriteResource_GetType(cellResource) != SpriteResource.TypeCell)
        {
            ErrorHandling.GF_AssertFail();
        }
        int id = SpriteResource.SpriteResource_GetID(charResource);
        CellDataBank cellBank = SpriteResource.SpriteResource_GetSpriteData(cellResource!)!;
        ImageProxy? imageProxy = CharTransfer.CharTransfer_ResizeTaskRange(id, G2d.GetCellBankTransferSize(cellBank));
        if (imageProxy == null)
        {
            imageProxy = CharTransfer.CharTransfer_CopyTask(CharTransfer.CharTransfer_GetImageProxy(id));
        }
        return imageProxy;
    }

    // OFFSET 0x0200A5B4
    public static void SpriteTransfer_DeleteCharTransfer(ImageProxy? imageProxy)
    {
        if (imageProxy == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        CharTransfer.CharTransfer_DeleteTask(imageProxy!);
    }

    // OFFSET 0x0200A5C8
    public static bool SpriteTransfer_RequestPlttWholeRange(SpriteResource? resource)
    {
        if (resource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (SpriteResource.SpriteResource_GetType(resource) != SpriteResource.TypePltt)
        {
            ErrorHandling.GF_AssertFail();
        }
        PlttTransferTaskTemplate template = new()
        {
            Data = SpriteResource.SpriteResource_GetPaletteFade(resource!),
            VramType = SpriteResource.SpriteResource_GetVRAMType(resource),
            ResourceID = SpriteResource.SpriteResource_GetID(resource),
            PaletteCount = SpriteResource.SpriteResource_GetPaletteIndex(resource),
        };
        return PlttTransfer.PlttTransfer_RequestWholeRange(template);
    }

    // OFFSET 0x0200A60C
    public static void SpriteTransfer_RequestPlttWholeRangeList(SpriteResourceList? list)
    {
        if (list == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < list!.Capacity; i++)
        {
            if (list.Resources[i] != null && !SpriteTransfer_RequestPlttWholeRange(list.Resources[i]))
            {
                ErrorHandling.GF_AssertFail();
            }
        }
    }

    // OFFSET 0x0200A640
    public static bool SpriteTransfer_RequestPlttFreeSpace(SpriteResource? resource)
    {
        if (resource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (SpriteResource.SpriteResource_GetType(resource) != SpriteResource.TypePltt)
        {
            ErrorHandling.GF_AssertFail();
        }
        PlttTransferTaskTemplate template = new()
        {
            Data = SpriteResource.SpriteResource_GetPaletteFade(resource!),
            VramType = SpriteResource.SpriteResource_GetVRAMType(resource),
            ResourceID = SpriteResource.SpriteResource_GetID(resource),
            PaletteCount = SpriteResource.SpriteResource_GetPaletteIndex(resource),
        };
        return PlttTransfer.PlttTransfer_RequestFreeSpace(template);
    }

    // OFFSET 0x0200A684
    public static void SpriteTransfer_RequestPlttFreeSpaceList(SpriteResourceList? list)
    {
        if (list == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < list!.Capacity; i++)
        {
            if (list.Resources[i] != null && !SpriteTransfer_RequestPlttFreeSpace(list.Resources[i]))
            {
                ErrorHandling.GF_AssertFail();
            }
        }
    }

    // OFFSET 0x0200A6B8
    public static void SpriteTransfer_ReplacePlttData(SpriteResource? resource)
    {
        if (resource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        int id = SpriteResource.SpriteResource_GetID(resource);
        PlttTransfer.PlttTransfer_ReplacePlttData(id, SpriteResource.SpriteResource_GetPaletteFade(resource!));
    }

    // OFFSET 0x0200A6DC
    public static void SpriteTransfer_ResetPlttTransfer(SpriteResource? resource)
    {
        if (resource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (SpriteResource.SpriteResource_GetType(resource) != SpriteResource.TypePltt)
        {
            ErrorHandling.GF_AssertFail();
        }
        PlttTransfer.PlttTransfer_ResetTask(SpriteResource.SpriteResource_GetID(resource));
    }

    // OFFSET 0x0200A700
    public static void SpriteTransfer_ResetPlttTransferList(SpriteResourceList? list)
    {
        if (list == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < list!.Capacity; i++)
        {
            if (list.Resources[i] != null)
            {
                SpriteTransfer_ResetPlttTransfer(list.Resources[i]);
            }
        }
    }

    // OFFSET 0x0200A72C
    public static ImagePaletteProxy? SpriteTransfer_GetPaletteProxy(SpriteResource? resource, ImageProxy? imageProxy)
    {
        if (resource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (SpriteResource.SpriteResource_GetType(resource) != SpriteResource.TypePltt)
        {
            ErrorHandling.GF_AssertFail();
        }
        int id = SpriteResource.SpriteResource_GetID(resource);
        if (imageProxy != null)
        {
            return PlttTransfer.PlttTransfer_ToggleExtPalette(id, imageProxy);
        }
        return PlttTransfer.PlttTransfer_GetPaletteProxy(id);
    }

    // OFFSET 0x0200A760
    public static int SpriteTransfer_GetPlttOffset(SpriteResource? resource, int vramType)
    {
        ImagePaletteProxy? paletteProxy = SpriteTransfer_GetPaletteProxy(resource, null);
        int location = G2d.GetImagePaletteLocation(paletteProxy!, vramType);
        if (location != -1)
        {
            location /= 32;
        }
        return location;
    }
}
