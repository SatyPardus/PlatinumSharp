using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class MapObjectRenderDetails
{
    public const int Size = 8;

    public int GraphicsID;
    public uint Flags;
}

public static class MapObjectRender
{
    public const int HeapIDFieldMap = 4;
    public const int GraphicsIDNone = 0xFFFF;
    public const int RenderTypeBillboard = 1;

    private const int NarcIDMapObjectModels = 0x51;
    private const int OverlayID = 5;
    private const uint RenderDetailsTableAddress = 0x021FC194;
    private const uint ManagerStatusDrawInitialized = 1 << 0;
    private const uint ManagerStatusPauseMovement = 1 << 2;
    private const uint StatusPauseAnimation = 1 << 8;
    private const uint StatusDrawRequested = 1 << 14;
    private const uint StatusHiddenByDraw = 1 << 20;

    private static MapObjectRenderDetails[]? sRenderDetails;

    private static MapObjectRenderDetails[] RenderDetails()
    {
        if (sRenderDetails == null)
        {
            List<MapObjectRenderDetails> entries = new();
            uint address = RenderDetailsTableAddress;
            while (true)
            {
                uint[] words = RomData.OverlayU32(OverlayID, address, 2);
                entries.Add(new MapObjectRenderDetails { GraphicsID = (int)words[0], Flags = words[1] });
                if (words[0] == GraphicsIDNone)
                {
                    break;
                }
                address += MapObjectRenderDetails.Size;
            }
            sRenderDetails = entries.ToArray();
        }
        return sRenderDetails;
    }

    public static MapObjectRenderData RenderDataOf(MapObjectManager mapObjectManager)
    {
        if (mapObjectManager.RenderData == null)
        {
            mapObjectManager.RenderData = new MapObjectRenderData();
        }
        return (MapObjectRenderData)MapObjectManager.MapObjectMan_GetRenderData(mapObjectManager)!;
    }

    // OFFSET 0x021ECC20
    public static void MapObjectMan_InitRender(MapObjectManager mapObjectManager, int maxTextures, int preloadTextureCount, int[] preloadIDs, int maxLoadsPerFrame)
    {
        if (MapObjectManager.MapObjectMan_CheckStatus(mapObjectManager, ManagerStatusDrawInitialized) != 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapObjectMan_OpenRenderNARC(mapObjectManager);
        int maxObjects = MapObjectManager.MapObjectMan_GetMaxObjects(mapObjectManager);
        int priority = MapObjectManager.MapObjectMan_GetTaskBasePriority(mapObjectManager) - 1;
        MapObjectRenderData renderData = RenderDataOf(mapObjectManager);
        MapObjectRenderResources.MapObjectRenderData_Init(renderData, mapObjectManager, maxObjects, priority, maxTextures, preloadTextureCount, preloadIDs, maxLoadsPerFrame);
        MapObjectManager.MapObjectMan_SetStatusFlagOn(mapObjectManager, ManagerStatusDrawInitialized);
    }

    // OFFSET 0x021ECC78
    public static void MapObjectMan_FreeRender(MapObjectManager mapObjectManager)
    {
        if (MapObjectManager.MapObjectMan_IsDrawInitialized(mapObjectManager) != 1)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapObjectRenderResources.MapObjectRenderData_Free(RenderDataOf(mapObjectManager));
        MapObjectManager.MapObjectMan_SetStatusFlagOff(mapObjectManager, ManagerStatusDrawInitialized);
        MapObjectMan_CloseRenderNARC(mapObjectManager);
    }

    // OFFSET 0x021ECCA4
    private static void MapObjectMan_OpenRenderNARC(MapObjectManager mapObjectManager)
    {
        NARC narc = NARC.NARC_ctor(NarcIDMapObjectModels, HeapIDFieldMap);
        MapObjectManager.MapObjectMan_SetNARC(mapObjectManager, narc);
    }

    // OFFSET 0x021ECCBC
    private static void MapObjectMan_CloseRenderNARC(MapObjectManager mapObjectManager)
    {
        NARC.NARC_dtor(MapObjectManager.MapObjectMan_GetNARC(mapObjectManager)!);
    }

    // OFFSET 0x021ECCC8
    public static void MapObject_Draw(MapObject mapObject)
    {
        MapObjectManager mapObjectManager = MapObject.MapObject_MapObjectManager(mapObject)!;
        if (MapObjectManager.MapObjectMan_CheckStatus(mapObjectManager, ManagerStatusPauseMovement) != 0)
        {
            return;
        }
        if (MapObject.MapObject_CheckStatus(mapObject, StatusDrawRequested) == 0)
        {
            return;
        }
        if (MapObject.MapObject_IsMovementPaused(mapObject) == 0 || MapObject.MapObject_HasMovementAction(mapObject) != 0)
        {
            MapObject.MapObject_CallDraw(mapObject);
        }
    }

    // OFFSET 0x021ECD04
    public static MapObjectRenderDetails? MapObject_GetRenderDetails(MapObject mapObject)
    {
        return GetRenderDetailsForGraphics(MapObject.MapObject_GetGraphicsID(mapObject));
    }

    // OFFSET 0x021ECD10
    private static MapObjectRenderDetails? GetRenderDetailsForGraphics(int graphicsID)
    {
        MapObjectRenderDetails[] table = RenderDetails();
        int index = 0;
        do
        {
            if (table[index].GraphicsID == graphicsID)
            {
                return table[index];
            }
            index++;
        }
        while (table[index].GraphicsID != GraphicsIDNone);
        ErrorHandling.GF_AssertFail();
        return null;
    }

    // OFFSET 0x021ECD38
    public static int MapObject_IsAnimationPaused(MapObject mapObject)
    {
        if (MapObject.MapObject_IsMovementPaused(mapObject) == 1 && MapObject.MapObject_HasMovementAction(mapObject) == 0)
        {
            return 1;
        }
        if (MapObject.MapObject_CheckStatus(mapObject, StatusPauseAnimation) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x021ECD68
    public static byte[] MapObjectMan_LoadRenderResource(MapObjectManager mapObjectManager, uint memberIndex, int allocAtStart)
    {
        NARC narc = MapObjectManager.MapObjectMan_GetNARC(mapObjectManager)!;
        uint size = NARC.NARC_GetMemberSize(narc, memberIndex);
        byte[] data;
        if (allocAtStart == 1)
        {
            data = Heap.Heap_Alloc(HeapIDFieldMap, size)!;
        }
        else
        {
            data = Heap.Heap_AllocAtEnd(HeapIDFieldMap, size)!;
        }
        NARC.NARC_ReadWholeMember(narc, memberIndex, data);
        return data;
    }

    // OFFSET 0x021ECDA0
    public static void MapObject_GetSpritePos(MapObject mapObject, out VecFx32 spritePos)
    {
        MapObject.MapObject_CopyPos(mapObject, out VecFx32 position);
        MapObject.MapObject_CopySpriteJumpOffset(mapObject, out VecFx32 jumpOffset);
        MapObject.MapObject_CopySpritePosOffset(mapObject, out VecFx32 posOffset);
        MapObject.MapObject_CopySpriteTerrainOffset(mapObject, out VecFx32 terrainOffset);
        spritePos.X = terrainOffset.X + (posOffset.X + (position.X + jumpOffset.X));
        spritePos.Y = terrainOffset.Y + (posOffset.Y + (position.Y + jumpOffset.Y));
        spritePos.Z = terrainOffset.Z + (posOffset.Z + (position.Z + jumpOffset.Z));
    }

    // OFFSET 0x021ECDFC
    public static void MapObject_FaceAndDraw(MapObject mapObject, int dir)
    {
        MapObject.MapObject_TryFace(mapObject, dir);
        if (MapObject.MapObject_IsDrawInitialized(mapObject) == 1)
        {
            MapObject.MapObject_CallDraw(mapObject);
        }
    }

    // OFFSET 0x021ECE18
    public static void MapObject_DrawInitInvisible(MapObject mapObject)
    {
        MapObject.MapObject_SetHidden(mapObject, 1);
        MapObject.MapObject_SetStatusFlagOn(mapObject, StatusHiddenByDraw);
    }

    // OFFSET 0x021ECE30
    public static void MapObject_DrawInvisible(MapObject mapObject)
    {
    }

    // OFFSET 0x021ECE34
    public static void MapObject_DrawDeleteInvisible(MapObject mapObject)
    {
    }

    // OFFSET 0x021ECE38
    public static void MapObject_DrawSuspendInvisible(MapObject mapObject)
    {
    }

    // OFFSET 0x021ECE3C
    public static void MapObject_DrawResumeInvisible(MapObject mapObject)
    {
    }
}
