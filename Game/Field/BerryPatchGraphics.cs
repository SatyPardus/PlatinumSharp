using static PlatinumSharp.Game.MapObject;
using static PlatinumSharp.Game.MapObjectBuffer;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class BerryPatchGraphics
{
    public const int GraphicsIDBerryPatch = 0x64;
    public const int GraphicsResourceNone = 0xFFFF;
    public const int GraphicsResourceSprout = 0x1000;

    private const int OverlayID = 5;
    private const uint BerryGraphicsTableAddress = 0x021FB67C;
    private const int BerryGraphicsEntrySize = 0xC;

    private const int DataSize = 4;
    private const int DataGrowthStageOffset = 0x0;
    private const int DataKeepEffectOffset = 0x2;

    private const int DrawDataSize = 0x14;
    private const int DrawResourceOffset = 0x0;
    private const int DrawGrowthStageOffset = 0x4;

    private const int GrowthStageNone = 0;
    private const int GrowthStagePlanted = 1;
    private const int GrowthStageSprouted = 2;
    private const int GrowthStageGrowing = 3;
    private const int GrowthStageBlooming = 4;
    private const int GrowthStageFruit = 5;
    private const int AnimFrameStep = 0x1000;

    // OFFSET 0x020677F4
    public static int BerryPatchGraphics_IsBerryPatch(int graphicsID)
    {
        if (graphicsID == GraphicsIDBerryPatch)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02067800
    public static int BerryPatchGraphics_GetCurrentGraphicsResourceID(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementTypeData(mapObject);
        if (U16(data, DataGrowthStageOffset) == GrowthStageNone)
        {
            return GraphicsResourceNone;
        }
        if (MapObject_IsDrawActive(mapObject) == 1)
        {
            byte[] drawData = MapObject_GetDrawData(mapObject);
            if (S32(drawData, DrawGrowthStageOffset) != GrowthStageNone)
            {
                return S32(drawData, DrawResourceOffset);
            }
        }
        return GraphicsResourceNone;
    }

    // OFFSET 0x02067834
    public static void BerryPatchGraphics_MarkForUpdate(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementTypeData(mapObject);
        W16(data, DataKeepEffectOffset, 1);
    }

    // OFFSET 0x02067840
    public static void BerryPatchGraphics_NewData(MapObject mapObject)
    {
        byte[] data = MapObject_InitMovementTypeData(mapObject, DataSize);
        W16(data, DataGrowthStageOffset, GrowthStageNone);
    }

    // OFFSET 0x02067850
    public static void BerryPatchGraphics_UpdateGrowthStage(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementTypeData(mapObject);
        W16(data, DataGrowthStageOffset, BerryPatchManager.BerryPatches_GetGrowthStage(MapObject_FieldSystem(mapObject)!, mapObject));
    }

    // OFFSET 0x0206786C
    public static void BerryPatchGraphics_NoOp(MapObject mapObject)
    {
    }

    // OFFSET 0x02067870
    public static void BerryPatchGraphics_NewGraphics(MapObject mapObject)
    {
        byte[] drawData = MapObject_InitDrawData(mapObject, DrawDataSize);
        W32(drawData, DrawResourceOffset, GraphicsResourceNone);
        W32(drawData, DrawGrowthStageOffset, GrowthStageNone);
        mapObject.BerryPatchBillboardSlot.Billboard = null;
        FootprintEffect.TerrainModelEffect_New(mapObject);
    }

    // OFFSET 0x02067890
    public static void BerryPatchGraphics_UpdateGraphics(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementTypeData(mapObject);
        byte[] drawData = MapObject_GetDrawData(mapObject);
        MapObjectBillboardSlot slot = mapObject.BerryPatchBillboardSlot;
        int growthStage = BerryPatchManager.BerryPatches_GetGrowthStage(MapObject_FieldSystem(mapObject)!, mapObject);
        if (MapObjectRenderResources.MapObject_IsBillboardPending(mapObject) == 1)
        {
            return;
        }
        if (growthStage != S32(drawData, DrawGrowthStageOffset))
        {
            MapObjectRenderResources.MapObject_DeleteBillboardAndRelease(mapObject, slot, S32(drawData, DrawResourceOffset));
            int berryID = BerryPatchManager.BerryPatches_GetBerryID(MapObject_FieldSystem(mapObject)!, mapObject);
            int resourceID = BerryPatchGraphics_GetGraphicsResourceID(berryID, growthStage);
            W32(drawData, DrawResourceOffset, resourceID);
            if (resourceID != GraphicsResourceNone)
            {
                if (S32(drawData, DrawGrowthStageOffset) != GrowthStageNone)
                {
                    FootprintEffect.SpriteBurstEffect_New(mapObject);
                }
                MapObjectRenderResources.MapObject_AddBillboardForGraphics(mapObject, slot, S32(drawData, DrawResourceOffset));
            }
            else if (S32(drawData, DrawGrowthStageOffset) != GrowthStageNone)
            {
                if (U16(data, DataKeepEffectOffset) == 0)
                {
                    FootprintEffect.SpriteBurstEffect_New(mapObject);
                }
            }
            W16(data, DataKeepEffectOffset, 0);
        }
        W32(drawData, DrawGrowthStageOffset, growthStage);
        if (MapObjectRenderResources.MapObject_IsBillboardPending(mapObject) == 1)
        {
            return;
        }
        Billboard? billboard = slot.Billboard;
        if (billboard != null)
        {
            MapObjectRenderResources.MapObject_UpdateBillboardPos(mapObject, billboard);
            if (MapObjectRender.MapObject_IsAnimationPaused(mapObject) == 0)
            {
                Billboards.Billboard_AdvanceAnim(slot.Billboard, AnimFrameStep);
            }
            MapObjectRenderResources.MapObject_UpdateBillboardDrawFlag(mapObject, slot.Billboard!);
        }
    }

    // OFFSET 0x02067950
    public static void BerryPatchGraphics_FreeGraphics(MapObject mapObject)
    {
        byte[] drawData = MapObject_GetDrawData(mapObject);
        MapObjectRenderResources.MapObject_DeleteBillboardAndRelease(mapObject, mapObject.BerryPatchBillboardSlot, S32(drawData, DrawResourceOffset));
    }

    // OFFSET 0x02067968
    public static void BerryPatchGraphics_PauseGraphics(MapObject mapObject)
    {
        byte[] drawData = MapObject_GetDrawData(mapObject);
        MapObjectBillboardSlot slot = mapObject.BerryPatchBillboardSlot;
        if (slot.Billboard != null)
        {
            MapObjectRenderResources.Billboard_SaveState(slot.Billboard, mapObject.BerryPatchBillboardState);
        }
        MapObjectRenderResources.MapObject_DeleteBillboardAndRelease(mapObject, slot, S32(drawData, DrawResourceOffset));
        MapObject_SetStatusFlagOn(mapObject, Status21);
    }

    // OFFSET 0x02067998
    public static void BerryPatchGraphics_ResumeGraphics(MapObject mapObject)
    {
        byte[] drawData = MapObject_GetDrawData(mapObject);
        MapObjectBillboardSlot slot = mapObject.BerryPatchBillboardSlot;
        if (MapObjectRenderResources.MapObject_IsBillboardPending(mapObject) == 1)
        {
            return;
        }
        if (slot.Billboard == null)
        {
            if (S32(drawData, DrawResourceOffset) != GraphicsResourceNone)
            {
                MapObjectRenderResources.MapObject_AddBillboardForGraphics(mapObject, slot, S32(drawData, DrawResourceOffset));
            }
            else
            {
                MapObject_SetStatusFlagOff(mapObject, Status21);
            }
            FootprintEffect.TerrainModelEffect_New(mapObject);
        }
        if (slot.Billboard != null)
        {
            MapObjectRenderResources.Billboard_RestoreState(slot.Billboard, mapObject.BerryPatchBillboardState);
            MapObjectRenderResources.MapObject_UpdateBillboardPos(mapObject, slot.Billboard);
            MapObject_SetStatusFlagOff(mapObject, Status21);
        }
    }

    // OFFSET 0x020679FC
    public static int BerryPatchGraphics_GetGraphicsResourceID(int berryID, int growthStage)
    {
        if (growthStage == GrowthStageNone)
        {
            return GraphicsResourceNone;
        }
        if (growthStage == GrowthStagePlanted)
        {
            return GraphicsResourceNone;
        }
        uint entry = BerryGraphicsTableAddress + (uint)((berryID - 1) * BerryGraphicsEntrySize);
        switch (growthStage)
        {
            case GrowthStageSprouted:
                return GraphicsResourceSprout;
            case GrowthStageGrowing:
                return (int)RomData.OverlayU32(OverlayID, entry, 1)[0];
            case GrowthStageBlooming:
                return (int)RomData.OverlayU32(OverlayID, entry + 4, 1)[0];
            case GrowthStageFruit:
                return (int)RomData.OverlayU32(OverlayID, entry + 8, 1)[0];
            default:
                ErrorHandling.GF_AssertFail();
                return GraphicsResourceNone;
        }
    }

    // OFFSET 0x02067A58
    public static Billboard? BerryPatchGraphics_GetGraphicsObject(MapObject mapObject)
    {
        Billboard? billboard = null;
        if (BerryPatchGraphics_IsBerryPatch(MapObject_GetGraphicsID(mapObject)) == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (MapObject_IsDrawInitialized(mapObject) == 1)
        {
            billboard = mapObject.BerryPatchBillboardSlot.Billboard;
        }
        return billboard;
    }
}
