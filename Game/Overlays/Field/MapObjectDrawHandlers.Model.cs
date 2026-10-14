using System.Runtime.CompilerServices;

namespace PlatinumSharp.Game;

public sealed class MapObjectModelDrawData
{
    public OverworldAnimManager? AnimManager;
}

public static partial class MapObjectDrawHandlers
{
    private const int ModelDrawDataSize = 4;
    private const uint ModelOffsetTableAddress = 0x021FB51C;
    private const int ModelOffsetEntryWords = 4;
    private const int ModelOffsetMaxEntries = 16;
    private const uint ModelOffsetTableEnd = 0xFFFF;

    private static readonly ConditionalWeakTable<byte[], MapObjectModelDrawData> sModelDrawData = new();

    private static MapObjectModelDrawData InitModelDrawData(MapObject mapObject)
    {
        byte[] raw = MapObject.MapObject_InitDrawData(mapObject, ModelDrawDataSize);
        sModelDrawData.Remove(raw);
        MapObjectModelDrawData drawData = new();
        sModelDrawData.Add(raw, drawData);
        return drawData;
    }

    private static MapObjectModelDrawData GetModelDrawData(MapObject mapObject)
    {
        byte[] raw = MapObject.MapObject_GetDrawData(mapObject);
        return sModelDrawData.GetValue(raw, _ => new MapObjectModelDrawData());
    }

    private static bool DispatchModel(uint address, MapObject mapObject)
    {
        switch (address & ~1u)
        {
            case 0x021EC734:
                Model_DrawInit(mapObject);
                return true;
            case 0x021EC75C:
                Model_Draw(mapObject);
                return true;
            case 0x021EC760:
                Model_DrawDelete(mapObject);
                return true;
            case 0x021EC778:
                Model_DrawSuspend(mapObject);
                return true;
            case 0x021EC790:
                Model_DrawResume(mapObject);
                return true;
            case 0x021EC7B8:
                AnimModel_DrawInit(mapObject);
                return true;
            case 0x021EC7D0:
                AnimModel_DrawResume(mapObject);
                return true;
            case 0x021EC7F0:
                AnimModel_DrawDelete(mapObject);
                return true;
        }
        return false;
    }

    // OFFSET 0x021EC700
    private static void MapObject_GetModelOffset(int graphicsID, ref VecFx32 offset)
    {
        uint[] table = RomData.OverlayU32(OverlayID, ModelOffsetTableAddress, ModelOffsetMaxEntries * ModelOffsetEntryWords);
        for (int i = 0; table[i] != ModelOffsetTableEnd; i += ModelOffsetEntryWords)
        {
            if (table[i] == (uint)graphicsID)
            {
                offset.X = (int)table[i + 1];
                offset.Y = (int)table[i + 2];
                offset.Z = (int)table[i + 3];
                return;
            }
        }
        ErrorHandling.GF_AssertFail();
    }

    // OFFSET 0x021EC734
    private static void Model_DrawInit(MapObject mapObject)
    {
        MapObjectModelDrawData drawData = InitModelDrawData(mapObject);
        VecFx32 offset = default;
        MapObject_GetModelOffset(MapObject.MapObject_GetEffectiveGraphicsID(mapObject), ref offset);
        drawData.AnimManager = MapObjectModelEffect.MapObjectModelEffect_New(mapObject, offset);
    }

    // OFFSET 0x021EC75C
    private static void Model_Draw(MapObject mapObject)
    {
    }

    // OFFSET 0x021EC760
    private static void Model_DrawDelete(MapObject mapObject)
    {
        MapObjectModelDrawData drawData = GetModelDrawData(mapObject);
        if (drawData.AnimManager != null)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager(drawData.AnimManager);
            drawData.AnimManager = null;
        }
    }

    // OFFSET 0x021EC778
    private static void Model_DrawSuspend(MapObject mapObject)
    {
        MapObjectModelDrawData drawData = GetModelDrawData(mapObject);
        if (drawData.AnimManager != null)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager(drawData.AnimManager);
            drawData.AnimManager = null;
        }
    }

    // OFFSET 0x021EC790
    private static void Model_DrawResume(MapObject mapObject)
    {
        MapObjectModelDrawData drawData = GetModelDrawData(mapObject);
        VecFx32 offset = default;
        MapObject_GetModelOffset(MapObject.MapObject_GetEffectiveGraphicsID(mapObject), ref offset);
        drawData.AnimManager = MapObjectModelEffect.MapObjectModelEffect_New(mapObject, offset);
    }

    // OFFSET 0x021EC7B8
    private static void AnimModel_DrawInit(MapObject mapObject)
    {
        MapObjectModelDrawData drawData = InitModelDrawData(mapObject);
        drawData.AnimManager = AttachedAnimModelEffect.AttachedAnimModelEffect_New(mapObject);
    }

    // OFFSET 0x021EC7D0
    private static void AnimModel_DrawResume(MapObject mapObject)
    {
        MapObjectModelDrawData drawData = GetModelDrawData(mapObject);
        if (drawData.AnimManager != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        drawData.AnimManager = AttachedAnimModelEffect.AttachedAnimModelEffect_New(mapObject);
    }

    // OFFSET 0x021EC7F0
    public static void AnimModel_DrawDelete(MapObject mapObject)
    {
        MapObjectModelDrawData drawData = GetModelDrawData(mapObject);
        AttachedAnimModelEffect.AttachedAnimModelEffect_StartFinishing(drawData.AnimManager!);
        drawData.AnimManager = null;
    }
}
