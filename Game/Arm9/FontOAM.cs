using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class FontOAMPart
{
    public Sprite? Sprite;
    public int X;
    public int Y;
}

public sealed class FontOAM
{
    public FontOAMPart[]? Parts;
    public int NumParts;
    public Sprite? Parent;
    public int X;
    public int Y;
}

public sealed class FontOAMManager
{
    public readonly byte[]?[] CellFiles = new byte[]?[FontOAMSystem.SizeCount];
    public readonly CellDataBank?[] CellBanks = new CellDataBank?[FontOAMSystem.SizeCount];
    public FontOAM[] FontOAMs = Array.Empty<FontOAM>();
    public int Count;
}

public sealed class FontOAMInitData
{
    public FontOAMManager Manager = null!;
    public Window Window = null!;
    public SpriteList? SpriteList;
    public ImagePaletteProxy? PaletteProxy;
    public Sprite? Parent;
    public uint CharOffset;
    public int X;
    public int Y;
    public byte BgPriority;
    public int Priority;
    public int VramType;
    public int HeapID;
}

public sealed class FontOAMSizeNode
{
    public int X;
    public int Y;
    public int SizeIndex;
    public FontOAMSizeNode? Next;
    public FontOAMSizeNode? Prev;
}

public sealed class FontOAMSizeList
{
    public readonly FontOAMSizeNode Head = new();
    public int Count;
}

public sealed class FontOAMSplitRegion
{
    public int Y;
    public int X;
    public int Width;
    public int Height;

    public void CopyFrom(FontOAMSplitRegion other)
    {
        Y = other.Y;
        X = other.X;
        Width = other.Width;
        Height = other.Height;
    }
}

public sealed class FontOAMSplitState
{
    public readonly FontOAMSplitRegion Current = new();
    public readonly FontOAMSplitRegion Pending = new();
    public byte HasPending;
}

public static class FontOAMSystem
{
    public const int SizeCount = 12;

    private const int CellNarcID = 0x23;
    private const int TileBytes = 0x20;
    private const int VramMain = 1;
    private const int DisplayMain = 0;
    private const int DisplaySub = 1;
    private const uint ObjMappingMask = 0x00300010;

    private static byte[]? sSizeTable;

    private static byte[] SizeTable => sSizeTable ??= RomData.Arm9(0x020E52E4, SizeCount * 2);

    private static int SizeWidth(int sizeIndex)
    {
        return SizeTable[sizeIndex * 2];
    }

    private static int SizeHeight(int sizeIndex)
    {
        return SizeTable[sizeIndex * 2 + 1];
    }

    private static uint GetObjMapping(int vramType)
    {
        return Video.GetDisplayControl(vramType == VramMain ? DisplayMain : DisplaySub) & ObjMappingMask;
    }

    private static void LoadObjectTiles(int vramType, byte[] buffer, uint offset, int size)
    {
        if (vramType == VramMain)
        {
            Video.LoadObjectTiles(DisplayMain, buffer, offset, (uint)size);
        }
        else
        {
            Video.LoadObjectTiles(DisplaySub, buffer, offset, (uint)size);
        }
    }

    // OFFSET 0x02012744
    public static FontOAMManager FontOAMManager_New(int count, int heapID)
    {
        FontOAMManager manager = new();
        for (int i = 0; i < SizeCount; i++)
        {
            manager.CellFiles[i] = Graphics.Graphics_GetCellBank(CellNarcID, (uint)i, 0, out manager.CellBanks[i], heapID);
            if (manager.CellFiles[i] == null)
            {
                ErrorHandling.GF_AssertFail();
            }
        }
        manager.FontOAMs = new FontOAM[count];
        for (int i = 0; i < count; i++)
        {
            manager.FontOAMs[i] = new FontOAM();
        }
        manager.Count = count;
        return manager;
    }

    // OFFSET 0x020127BC
    public static void FontOAMManager_Free(FontOAMManager? manager)
    {
        if (manager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < SizeCount; i++)
        {
            manager!.CellFiles[i] = null;
        }
        manager!.FontOAMs = Array.Empty<FontOAM>();
    }

    // OFFSET 0x020127E8
    public static FontOAM FontOAM_Create(FontOAMInitData? init)
    {
        if (init == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        FontOAM? fontOAM = FontOAMManager_FindFreeFontOAM(init!.Manager);
        if (fontOAM == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        fontOAM!.Parent = init.Parent;
        fontOAM.X = init.X;
        fontOAM.Y = init.Y;
        FontOAMSizeNode head = new();
        head.Next = head;
        head.Prev = head;
        int numParts = FontOAMSizeList_Split(init.Window.Width, init.Window.Height, init.HeapID, head);
        ImageProxy[] proxies = new ImageProxy[numParts];
        fontOAM.Parts = new FontOAMPart[numParts];
        for (int i = 0; i < numParts; i++)
        {
            proxies[i] = new ImageProxy();
            fontOAM.Parts[i] = new FontOAMPart();
        }
        fontOAM.NumParts = numParts;
        FontOAM_LoadTiles(init.Window, head, proxies, init.CharOffset, init.VramType, init.HeapID);
        FontOAM_CreateSprites(init, head, proxies, fontOAM);
        FontOAMSizeList_FreeNodes(head);
        return fontOAM;
    }

    // OFFSET 0x02012870
    public static void FontOAM_Delete(FontOAM? fontOAM)
    {
        if (fontOAM == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (fontOAM!.Parts == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        FontOAM_DeleteSprites(fontOAM);
        FontOAM_Clear(fontOAM);
    }

    // OFFSET 0x02012898
    public static int FontOAM_CalcCharSize(Window window, int vramType, int heapID)
    {
        FontOAMSizeNode head = new();
        head.Next = head;
        head.Prev = head;
        FontOAMSizeList_Split(window.Width, window.Height, heapID, head);
        int size = FontOAMSizeList_CalcCharSize(head, vramType);
        FontOAMSizeList_FreeNodes(head);
        return size;
    }

    // OFFSET 0x020128C4
    public static void FontOAM_SetXY(FontOAM? fontOAM, int x, int y)
    {
        if (fontOAM == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        fontOAM!.X = x;
        fontOAM.Y = y;
        int baseX = x << Fx32.Shift;
        int baseY = y << Fx32.Shift;
        if (fontOAM.Parent != null)
        {
            VecFx32 parentPosition = Sprite.Sprite_GetPosition(fontOAM.Parent);
            baseX += parentPosition.X;
            baseY += parentPosition.Y;
        }
        VecFx32 position = default;
        for (int i = 0; i < fontOAM.NumParts; i++)
        {
            FontOAMPart part = fontOAM.Parts![i];
            position.X = baseX + (part.X << Fx32.Shift);
            position.Y = baseY + (part.Y << Fx32.Shift);
            Sprite.Sprite_SetPosition(part.Sprite!, position);
        }
    }

    // OFFSET 0x02012938
    public static void FontOAM_UpdatePosition(FontOAM? fontOAM)
    {
        if (fontOAM == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (fontOAM!.Parent == null)
        {
            return;
        }
        int baseX = fontOAM.X << Fx32.Shift;
        int baseY = fontOAM.Y << Fx32.Shift;
        VecFx32 parentPosition = Sprite.Sprite_GetPosition(fontOAM.Parent);
        baseX += parentPosition.X;
        baseY += parentPosition.Y;
        VecFx32 position = default;
        for (int i = 0; i < fontOAM.NumParts; i++)
        {
            FontOAMPart part = fontOAM.Parts![i];
            position.X = baseX + (part.X << Fx32.Shift);
            position.Y = baseY + (part.Y << Fx32.Shift);
            Sprite.Sprite_SetPosition(part.Sprite!, position);
        }
    }

    // OFFSET 0x020129A4
    public static void FontOAM_GetXY(FontOAM? fontOAM, out int x, out int y)
    {
        if (fontOAM == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        x = fontOAM!.X;
        y = fontOAM.Y;
    }

    // OFFSET 0x020129D0
    public static void FontOAM_SetDrawFlag(FontOAM? fontOAM, int draw)
    {
        if (fontOAM == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < fontOAM!.NumParts; i++)
        {
            Sprite.Sprite_SetDrawFlag(fontOAM.Parts![i].Sprite!, draw);
        }
    }

    // OFFSET 0x02012A00
    public static void FontOAM_SetExplicitPriority(FontOAM? fontOAM, byte priority)
    {
        if (fontOAM == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < fontOAM!.NumParts; i++)
        {
            Sprite.Sprite_SetExplicitPriority(fontOAM.Parts![i].Sprite!, priority);
        }
    }

    // OFFSET 0x02012A30
    public static void FontOAM_SetPriority(FontOAM? fontOAM, uint priority)
    {
        if (fontOAM == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < fontOAM!.NumParts; i++)
        {
            Sprite.Sprite_SetPriority(fontOAM.Parts![i].Sprite!, priority);
        }
    }

    // OFFSET 0x02012A60
    public static void FontOAM_SetExplicitPalette(FontOAM? fontOAM, uint palette)
    {
        if (fontOAM == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < fontOAM!.NumParts; i++)
        {
            Sprite.Sprite_SetExplicitPalette(fontOAM.Parts![i].Sprite!, palette);
        }
    }

    // OFFSET 0x02012A90
    public static void FontOAM_SetExplicitPaletteOffset(FontOAM? fontOAM, uint paletteOffset)
    {
        if (fontOAM == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < fontOAM!.NumParts; i++)
        {
            Sprite.Sprite_SetExplicitPaletteOffset(fontOAM.Parts![i].Sprite!, paletteOffset);
        }
    }

    // OFFSET 0x02012AC0
    public static void FontOAM_SetExplicitPaletteOffsetAutoAdjust(FontOAM? fontOAM, uint paletteOffset)
    {
        if (fontOAM == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < fontOAM!.NumParts; i++)
        {
            Sprite.Sprite_SetExplicitPaletteOffsetAutoAdjust(fontOAM.Parts![i].Sprite!, paletteOffset);
        }
    }

    // OFFSET 0x02012AF0
    public static void FontOAM_SetExplicitOamMode(FontOAM? fontOAM, int mode)
    {
        if (fontOAM == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < fontOAM!.NumParts; i++)
        {
            Sprite.Sprite_SetExplicitOAMMode(fontOAM.Parts![i].Sprite!, mode);
        }
    }

    // OFFSET 0x02012B20
    public static FontOAMSizeList FontOAMSizeList_New(Window window, int heapID)
    {
        FontOAMSizeList list = new();
        list.Head.Next = list.Head;
        list.Head.Prev = list.Head;
        list.Count = FontOAMSizeList_Split(window.Width, window.Height, heapID, list.Head);
        return list;
    }

    // OFFSET 0x02012B48
    public static void FontOAMSizeList_Free(FontOAMSizeList list)
    {
        FontOAMSizeList_FreeNodes(list.Head);
    }

    // OFFSET 0x02012B58
    public static int FontOAMSizeList_GetCharSize(FontOAMSizeList list, int vramType)
    {
        return FontOAMSizeList_CalcCharSize(list.Head, vramType);
    }

    // OFFSET 0x02012B60
    public static FontOAM FontOAM_CreateWithSizeList(FontOAMInitData? init, FontOAMSizeList list)
    {
        if (init == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        FontOAM? fontOAM = FontOAMManager_FindFreeFontOAM(init!.Manager);
        if (fontOAM == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        fontOAM!.Parent = init.Parent;
        fontOAM.X = init.X;
        fontOAM.Y = init.Y;
        ImageProxy[] proxies = new ImageProxy[list.Count];
        fontOAM.Parts = new FontOAMPart[list.Count];
        for (int i = 0; i < list.Count; i++)
        {
            proxies[i] = new ImageProxy();
            fontOAM.Parts[i] = new FontOAMPart();
        }
        fontOAM.NumParts = list.Count;
        FontOAM_LoadTiles(init.Window, list.Head, proxies, init.CharOffset, init.VramType, init.HeapID);
        FontOAM_CreateSprites(init, list.Head, proxies, fontOAM);
        return fontOAM;
    }

    // OFFSET 0x02012BD8
    public static void FontOAM_DeleteWithSizeList(FontOAM? fontOAM)
    {
        FontOAM_Delete(fontOAM);
    }

    // OFFSET 0x02012BE0
    public static void FontOAM_RedrawWindow(FontOAM fontOAM, FontOAMSizeList list, Window window, int heapID)
    {
        Sprite sprite = fontOAM.Parts![0].Sprite!;
        int vramType = Sprite.Sprite_GetVRamType(sprite);
        int size = FontOAMSizeList_GetCharSize(list, vramType);
        byte[] buffer = new byte[size];
        FontOAM_CopyTilesToBuffer(window, buffer, list.Head, vramType, heapID);
        ImageProxy proxy = Sprite.Sprite_GetImageProxy(sprite);
        if (vramType == VramMain)
        {
            uint location = (uint)G2d.GetImageLocation(proxy, G2d.VramMain);
            Video.LoadObjectTiles(DisplayMain, buffer, location, (uint)size);
        }
        else
        {
            uint location = (uint)G2d.GetImageLocation(proxy, G2d.VramSub);
            Video.LoadObjectTiles(DisplaySub, buffer, location, (uint)size);
        }
    }

    // OFFSET 0x02012C60
    public static void FontOAM_CopyWindowTiles(Window window, int width, int height, int x, int y, byte[] output, int outputOffset)
    {
        if (window.Width < width + x)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (window.Height < height + y)
        {
            ErrorHandling.GF_AssertFail();
        }
        int rowBytes = width * TileBytes;
        int outputTile = 0;
        for (int row = 0; row < height; row++)
        {
            int source = ((row + y) * window.Width + x) * TileBytes;
            Array.Copy(window.Pixels!, source, output, outputOffset + outputTile * TileBytes, rowBytes);
            outputTile += width;
        }
    }

    // OFFSET 0x02012CD0
    private static void FontOAM_Clear(FontOAM fontOAM)
    {
        fontOAM.Parts = null;
        fontOAM.NumParts = 0;
        fontOAM.Parent = null;
        fontOAM.X = 0;
        fontOAM.Y = 0;
    }

    // OFFSET 0x02012CE0
    private static FontOAM? FontOAMManager_FindFreeFontOAM(FontOAMManager manager)
    {
        for (int i = 0; i < manager.Count; i++)
        {
            if (manager.FontOAMs[i].Parts == null)
            {
                return manager.FontOAMs[i];
            }
        }
        return null;
    }

    // OFFSET 0x02012D08
    private static int FontOAM_FindSizeIndex(int width, int height)
    {
        for (int i = 0; i < SizeCount; i++)
        {
            if (SizeWidth(i) <= width && SizeHeight(i) <= height)
            {
                return i;
            }
        }
        return SizeCount;
    }

    // OFFSET 0x02012D34
    private static bool FontOAMSizeList_SplitStep(FontOAMSplitState state, FontOAMSizeNode head, int heapID)
    {
        FontOAMSizeNode node = FontOAMSizeNode_New(heapID);
        FontOAMSizeNode_InsertAfter(node, head.Prev!);
        FontOAMSplitRegion current = state.Current;
        node.SizeIndex = FontOAM_FindSizeIndex(current.Width, current.Height);
        node.X = current.X;
        node.Y = current.Y;
        int remainingWidth = current.Width - SizeWidth(node.SizeIndex);
        int remainingHeight = current.Height - SizeHeight(node.SizeIndex);
        if (remainingWidth > 0)
        {
            state.Pending.Height = current.Height;
            state.Pending.Width = remainingWidth;
            state.Pending.Y = current.Y;
            state.Pending.X = current.X + SizeWidth(node.SizeIndex);
            if (state.HasPending == 1)
            {
                ErrorHandling.GF_AssertFail();
            }
            state.HasPending = 1;
        }
        if (remainingHeight > 0)
        {
            current.Y += SizeHeight(node.SizeIndex);
            current.Height = remainingHeight;
        }
        else if (state.HasPending == 1)
        {
            current.CopyFrom(state.Pending);
            state.HasPending = 0;
        }
        else
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02012DE4
    private static int FontOAMSizeList_Split(int width, int height, int heapID, FontOAMSizeNode head)
    {
        if (width == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (height == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        FontOAMSplitState state = new();
        FontOAMSplitRegion nextRow = new();
        int count = 0;
        int remaining = height;
        state.Current.Height = height;
        state.Current.Y = 0;
        state.Current.X = 0;
        state.Current.Width = width;
        state.HasPending = 0;
        nextRow.X = 0;
        nextRow.Width = width;
        while (remaining != 0)
        {
            int rowHeight = SizeHeight(FontOAM_FindSizeIndex(state.Current.Width, remaining));
            nextRow.Y = state.Current.Y + rowHeight;
            nextRow.Height = state.Current.Height - rowHeight;
            state.Current.Height = rowHeight;
            do
            {
                count++;
            } while (!FontOAMSizeList_SplitStep(state, head, heapID));
            state.Current.CopyFrom(nextRow);
            remaining = state.Current.Height;
        }
        return count;
    }

    // OFFSET 0x02012E6C
    private static void FontOAM_LoadTiles(Window window, FontOAMSizeNode head, ImageProxy[] proxies, uint charOffset, int vramType, int heapID)
    {
        uint mapping = GetObjMapping(vramType);
        int blockSize = CharTransfer.CharTransfer_GetBlockSize(mapping);
        int index = 0;
        for (FontOAMSizeNode node = head.Next!; node != head; node = node.Next!)
        {
            ImageProxy proxy = proxies[index++];
            proxy.Init();
            charOffset = FontOAM_LoadNodeTiles(window, node, proxy, blockSize, mapping, charOffset, vramType, heapID);
        }
    }

    // OFFSET 0x02012EE0
    private static uint FontOAM_LoadNodeTiles(Window window, FontOAMSizeNode node, ImageProxy proxy, int blockSize, uint mapping, uint charOffset, int vramType, int heapID)
    {
        int width = SizeWidth(node.SizeIndex);
        int height = SizeHeight(node.SizeIndex);
        int tiles = width * height;
        if (tiles < blockSize)
        {
            tiles = blockSize;
        }
        int size = tiles * TileBytes;
        byte[] buffer = new byte[size];
        FontOAM_CopyWindowTiles(window, width, height, node.X, node.Y, buffer, 0);
        LoadObjectTiles(vramType, buffer, charOffset, size);
        proxy.BaseAddress = charOffset;
        proxy.VramType = vramType == VramMain ? G2d.VramMain : G2d.VramSub;
        proxy.Located = true;
        proxy.Locations[proxy.VramType] = (int)charOffset;
        proxy.SizeS = 0xFFFF;
        proxy.SizeT = 0xFFFF;
        proxy.Format = 3;
        proxy.Color0Transparent = 1;
        proxy.Layout = (int)mapping;
        return charOffset + (uint)size;
    }

    // OFFSET 0x02012F98
    private static void FontOAM_CopyTilesToBuffer(Window window, byte[] buffer, FontOAMSizeNode head, int vramType, int heapID)
    {
        uint mapping = GetObjMapping(vramType);
        int blockSize = CharTransfer.CharTransfer_GetBlockSize(mapping);
        int offset = 0;
        for (FontOAMSizeNode node = head.Next!; node != head; node = node.Next!)
        {
            offset = FontOAM_CopyNodeTilesToBuffer(window, node, buffer, offset, blockSize, mapping, heapID);
        }
    }

    // OFFSET 0x02012FF0
    private static int FontOAM_CopyNodeTilesToBuffer(Window window, FontOAMSizeNode node, byte[] buffer, int offset, int blockSize, uint mapping, int heapID)
    {
        int width = SizeWidth(node.SizeIndex);
        int height = SizeHeight(node.SizeIndex);
        int tiles = width * height;
        if (tiles < blockSize)
        {
            tiles = blockSize;
        }
        FontOAM_CopyWindowTiles(window, width, height, node.X, node.Y, buffer, offset);
        return offset + tiles * TileBytes;
    }

    // OFFSET 0x02013034
    private static int FontOAMSizeList_CalcCharSize(FontOAMSizeNode head, int vramType)
    {
        int blockSize = CharTransfer.CharTransfer_GetBlockSize(GetObjMapping(vramType));
        int size = 0;
        for (FontOAMSizeNode node = head.Next!; node != head; node = node.Next!)
        {
            int tiles = SizeWidth(node.SizeIndex) * SizeHeight(node.SizeIndex);
            if (tiles < blockSize)
            {
                tiles = blockSize;
            }
            size += tiles * TileBytes;
        }
        return size;
    }

    // OFFSET 0x02013088
    private static void FontOAM_CreateSprites(FontOAMInitData init, FontOAMSizeNode head, ImageProxy[] proxies, FontOAM fontOAM)
    {
        int index = 0;
        for (FontOAMSizeNode node = head.Next!; node != head; node = node.Next!)
        {
            FontOAMPart part = fontOAM.Parts![index];
            part.Sprite = FontOAM_CreatePartSprite(init, node, proxies[index]);
            if (part.Sprite == null)
            {
                ErrorHandling.GF_AssertFail();
            }
            part.X = node.X * 8;
            part.Y = node.Y * 8;
            index++;
        }
    }

    // OFFSET 0x020130DC
    private static void FontOAM_DeleteSprites(FontOAM fontOAM)
    {
        for (int i = 0; i < fontOAM.NumParts; i++)
        {
            Sprite.Sprite_Delete(fontOAM.Parts![i].Sprite!);
        }
    }

    // OFFSET 0x02013100
    private static Sprite? FontOAM_CreatePartSprite(FontOAMInitData init, FontOAMSizeNode node, ImageProxy proxy)
    {
        SpriteResourcesHeader resourceData = new()
        {
            ImageProxy = proxy,
            CharData = null,
            PaletteProxy = init.PaletteProxy,
            CellBank = init.Manager.CellBanks[node.SizeIndex],
            CellAnimBank = null,
            MultiCellBank = null,
            MultiCellAnimBank = null,
            IsVRamTransfer = 0,
            Priority = init.BgPriority,
        };
        SpriteListTemplate template = new()
        {
            List = init.SpriteList,
            ResourceData = resourceData,
            Priority = init.Priority,
            VramType = init.VramType,
            HeapID = init.HeapID,
        };
        if (init.Parent != null)
        {
            template.Position = Sprite.Sprite_GetPosition(init.Parent);
        }
        template.Position.X += (init.X + node.X * 8) << Fx32.Shift;
        template.Position.Y += (init.Y + node.Y * 8) << Fx32.Shift;
        return Sprite.SpriteList_Add(template);
    }

    // OFFSET 0x02013188
    private static FontOAMSizeNode FontOAMSizeNode_New(int heapID)
    {
        FontOAMSizeNode node = new();
        node.Next = null;
        node.Prev = null;
        return node;
    }

    // OFFSET 0x020131A4
    private static void FontOAMSizeNode_Free(FontOAMSizeNode? node)
    {
        if (node == null)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x020131B8
    private static void FontOAMSizeList_FreeNodes(FontOAMSizeNode head)
    {
        FontOAMSizeNode node = head.Next!;
        while (node != head)
        {
            FontOAMSizeNode next = node.Next!;
            FontOAMSizeNode_Free(node);
            node = next;
        }
    }

    // OFFSET 0x020131D0
    private static void FontOAMSizeNode_InsertAfter(FontOAMSizeNode node, FontOAMSizeNode previous)
    {
        node.Next = previous.Next;
        node.Prev = previous;
        previous.Next!.Prev = node;
        previous.Next = node;
    }

    // OFFSET 0x020131E0
    public static void FontOAM_SetParent(FontOAM fontOAM, Sprite? parent)
    {
        fontOAM.Parent = parent;
        FontOAM_UpdatePosition(fontOAM);
    }
}
