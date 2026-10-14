namespace PlatinumSharp.Game;

public static partial class MapObjectDrawHandlers
{
    private static bool DispatchPlayerPoses(uint address, MapObject mapObject)
    {
        switch (address & ~1u)
        {
            case 0x021EC3F0:
                Player_DrawFacingPose(mapObject);
                return true;
            case 0x021EC454:
                Player_DrawLoopFirstAnim(mapObject);
                return true;
            case 0x021EC4BC:
                Player_DrawStateSelectedAnim(mapObject);
                return true;
            case 0x021EC554:
                Player_DrawWalkInPlace(mapObject);
                return true;
        }
        return false;
    }

    private static void FinishPlayerPose(MapObject mapObject, Billboard billboard)
    {
        MapObjectRenderResources.MapObject_UpdateBillboardPos(mapObject, billboard);
        MapObject_UpdateBillboardDrawFlag(mapObject, billboard);
    }

    private static void SelectAnimIfChanged(Billboard billboard, int animNum)
    {
        if (Billboards.Billboard_GetAnimNum(billboard) != animNum)
        {
            Billboards.Billboard_SetAnimNum(billboard, animNum);
            Billboards.Billboard_SetAnimFrameNum(billboard, 0);
        }
    }

    // OFFSET 0x021EC3F0
    public static void Player_DrawFacingPose(MapObject mapObject)
    {
        Billboard? billboard = GetSpriteDrawData(mapObject).Slot.Billboard;
        MapObjectSpriteDrawData drawData = GetSpriteDrawData(mapObject);
        if (MapObjectRenderResources.MapObject_IsBillboardPending(mapObject) == 1 || billboard == null)
        {
            return;
        }
        int dir = MapObject.MapObject_GetFacingDir(mapObject);
        drawData.LastDir = (sbyte)dir;
        drawData.LastDrawState = (sbyte)MapObject.MapObject_GetDrawState(mapObject);
        Billboards.Billboard_SetAnimNum(billboard, MapObjectRenderResources.GetWalkAnimForDir(dir));
        Billboards.Billboard_SetAnimFrameNum(billboard, 0);
        Billboards.Billboard_AdvanceAnim(billboard, 0);
        FinishPlayerPose(mapObject, billboard);
    }

    // OFFSET 0x021EC454
    public static void Player_DrawLoopFirstAnim(MapObject mapObject)
    {
        Billboard? billboard = GetSpriteDrawData(mapObject).Slot.Billboard;
        MapObjectSpriteDrawData drawData = GetSpriteDrawData(mapObject);
        if (MapObjectRenderResources.MapObject_IsBillboardPending(mapObject) == 1 || billboard == null)
        {
            return;
        }
        drawData.LastDir = (sbyte)MapObject.MapObject_GetFacingDir(mapObject);
        drawData.LastDrawState = (sbyte)MapObject.MapObject_GetDrawState(mapObject);
        SelectAnimIfChanged(billboard, 0);
        Billboards.Billboard_AdvanceAnim(billboard, Fx32.One);
        FinishPlayerPose(mapObject, billboard);
    }

    // OFFSET 0x021EC4BC
    public static void Player_DrawStateSelectedAnim(MapObject mapObject)
    {
        Billboard? billboard = GetSpriteDrawData(mapObject).Slot.Billboard;
        MapObjectSpriteDrawData drawData = GetSpriteDrawData(mapObject);
        if (MapObjectRenderResources.MapObject_IsBillboardPending(mapObject) == 1 || billboard == null)
        {
            return;
        }
        int dir = MapObject.MapObject_GetFacingDir(mapObject);
        if (MapObject.MapObject_GetDrawState(mapObject) == 0)
        {
            SelectAnimIfChanged(billboard, 1);
        }
        else
        {
            SelectAnimIfChanged(billboard, 0);
        }
        Billboards.Billboard_AdvanceAnim(billboard, Fx32.One);
        drawData.LastDir = (sbyte)dir;
        drawData.LastDrawState = (sbyte)MapObject.MapObject_GetDrawState(mapObject);
        FinishPlayerPose(mapObject, billboard);
    }

    // OFFSET 0x021EC554
    public static void Player_DrawWalkInPlace(MapObject mapObject)
    {
        Billboard? billboard = GetSpriteDrawData(mapObject).Slot.Billboard;
        MapObjectSpriteDrawData drawData = GetSpriteDrawData(mapObject);
        if (MapObjectRenderResources.MapObject_IsBillboardPending(mapObject) == 1 || billboard == null)
        {
            return;
        }
        int dir = MapObject.MapObject_GetFacingDir(mapObject);
        if (dir != drawData.LastDir)
        {
            Billboards.Billboard_SetAnimNum(billboard, MapObjectRenderResources.GetWalkAnimForDir(dir));
            Billboards.Billboard_SetAnimFrameNum(billboard, 0);
        }
        Billboards.Billboard_AdvanceAnim(billboard, Fx32.One);
        FinishPlayerPose(mapObject, billboard);
        drawData.LastDir = (sbyte)dir;
        drawData.LastDrawState = (sbyte)MapObject.MapObject_GetDrawState(mapObject);
    }
}
