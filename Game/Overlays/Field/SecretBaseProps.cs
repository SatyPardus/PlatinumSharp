using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class SecretBaseProps
{
    private const uint GoodsTableAddress = 0x020EC598;
    private const int GoodsEntrySize = 0x1C;
    private const int GoodWidthOffset = 0;
    private const int GoodDepthOffset = 1;
    private const int MaxGoods = 32;
    private const int PropHeight = 0x1000;

    // OFFSET 0x020573FC
    private static int Good_GetWidth(int goodID)
    {
        return RomData.Arm9(GoodsTableAddress + (uint)(goodID * GoodsEntrySize) + GoodWidthOffset, 1)[0];
    }

    // OFFSET 0x0205740C
    private static int Good_GetDepth(int goodID)
    {
        return RomData.Arm9(GoodsTableAddress + (uint)(goodID * GoodsEntrySize) + GoodDepthOffset, 1)[0];
    }

    // OFFSET 0x021F0824
    public static void FieldSystem_LoadSecretBaseGoodProps(FieldSystem fieldSystem)
    {
        SecretBase? secretBase = UndergroundManager.UndergroundMan_GetOccupiedSecretBase(fieldSystem.SaveData!);
        if (secretBase == null)
        {
            return;
        }
        int baseX = ov23_02242E28() << 21;
        int baseZ = ov23_02242E40() << 21;
        for (int i = 0; i < MaxGoods; i++)
        {
            int goodID = SecretBase.SecretBase_GetGoodIDAtIndex(secretBase, i);
            int width = Good_GetWidth(goodID);
            int depth = Good_GetDepth(goodID);
            if (goodID == 0)
            {
                continue;
            }
            int x = SecretBase.SecretBase_GetGoodXCoordAtIndex(secretBase, i);
            int z = SecretBase.SecretBase_GetGoodZCoordAtIndex(secretBase, i);
            VecFx32 position;
            position.Y = PropHeight;
            position.X = (x << 16) + baseX + (width << 15);
            position.Z = (z << 16) + baseZ + (depth << 15);
            int modelID = AreaData.AreaDataManager_GetMapPropModelID(fieldSystem.AreaDataManager!, goodID);
            MapProps.MapPropManager_LoadOne(fieldSystem.MapPropManager!, fieldSystem.AreaDataManager!, modelID, position, null, fieldSystem.MapPropAnimationManager);
        }
    }
}
