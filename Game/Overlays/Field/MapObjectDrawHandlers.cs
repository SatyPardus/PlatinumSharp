using System.Runtime.CompilerServices;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate void MapObjectSpriteStateFunc(MapObject mapObject, Billboard billboard, MapObjectSpriteDrawData drawData, int dir);

public sealed class MapObjectSpriteDrawData
{
    public short Step;
    public sbyte LastDir;
    public sbyte LastDrawState;
    public byte Flags;
    public sbyte AnimTimer;
    public sbyte AnimDelay;
    public readonly MapObjectBillboardSlot Slot = new();
    public readonly MapObjectBillboardState SavedState = new();
}

public static partial class MapObjectDrawHandlers
{
    private const int OverlayID = 5;
    private const int DrawDataSize = 0x10;
    private const int NpcDrawStateCount = 9;
    private const int NurseDrawStateCount = 10;
    private const int NurseDrawStateBow = 9;
    private const int NurseBowAnim = 4;
    private const int MagikarpMaxDelay = 16;
    private const int MagikarpAnimFrames = 32;
    private const int PlayerDrawStateCount = 10;
    private const int DrawStateRunning = 9;
    private const int IdleFrameCycle = 8;
    private const int RunFrameCycle = 4;
    private const uint NpcStepFrames6Address = 0x021FF35C;
    private const uint NpcStepFrames7Address = 0x021FF32C;
    private const uint NpcStepFrames8Address = 0x021FF3B4;
    private const uint PlayerStepFrames6Address = 0x021FF378;
    private const uint PlayerStepFrames7Address = 0x021FF33C;
    private const uint PlayerStepFrames8Address = 0x021FF394;
    private const int StepFrames6Count = 7;
    private const int StepFrames7Count = 4;
    private const int StepFrames8Count = 8;
    private const uint StatusHideSprite = 1 << 9;
    private const uint StatusUnderCover = 1 << 12;
    private const uint StatusCoverVisible = 1 << 13;
    private const uint StatusDrawSuspended = 1 << 21;
    private const byte FlagWasRunning = 1;

    private static readonly ConditionalWeakTable<byte[], MapObjectSpriteDrawData> sDrawData = new();

    private static readonly MapObjectSpriteStateFunc[] sNpcDrawStates =
    {
        Npc_DrawStateIdle,
        Npc_DrawStateWalkHalf,
        Npc_DrawStateWalkHalf,
        Npc_DrawStateWalk1,
        Npc_DrawStateWalk2,
        Npc_DrawStateWalk4,
        Npc_DrawStateSteps6,
        Npc_DrawStateSteps7,
        Npc_DrawStateSteps8,
    };

    private static readonly MapObjectSpriteStateFunc[] sNurseDrawStates =
    {
        Nurse_DrawStateIdle,
        Npc_DrawStateWalkHalf,
        Npc_DrawStateWalkHalf,
        Npc_DrawStateWalk1,
        Npc_DrawStateWalk2,
        Npc_DrawStateWalk4,
        Npc_DrawStateSteps6,
        Npc_DrawStateSteps7,
        Npc_DrawStateSteps8,
        Nurse_DrawStateBow,
    };

    private static readonly MapObjectSpriteStateFunc[] sPlayerDrawStates =
    {
        Player_DrawStateIdle,
        Player_DrawStateWalkHalf,
        Player_DrawStateWalkHalf,
        Player_DrawStateWalk1,
        Player_DrawStateWalk2,
        Player_DrawStateWalk4,
        Player_DrawStateSteps6,
        Player_DrawStateSteps7,
        Player_DrawStateSteps8,
        Player_DrawStateRun,
    };

    private static MapObjectSpriteDrawData InitSpriteDrawData(MapObject mapObject)
    {
        byte[] raw = MapObject.MapObject_InitDrawData(mapObject, DrawDataSize);
        sDrawData.Remove(raw);
        MapObjectSpriteDrawData drawData = new();
        sDrawData.Add(raw, drawData);
        return drawData;
    }

    private static MapObjectSpriteDrawData GetSpriteDrawData(MapObject mapObject)
    {
        byte[] raw = MapObject.MapObject_GetDrawData(mapObject);
        return sDrawData.GetValue(raw, _ => new MapObjectSpriteDrawData());
    }

    private static int[] StepFrames(uint address, int count)
    {
        uint[] words = RomData.OverlayU32(OverlayID, address, count);
        int[] frames = new int[count];
        for (int i = 0; i < count; i++)
        {
            frames[i] = (int)words[i];
        }
        return frames;
    }

    public static bool Dispatch(uint address, MapObject mapObject)
    {
        switch (address & ~1u)
        {
            case 0x021EB2EC:
                Npc_DrawInit(mapObject);
                return true;
            case 0x021EB314:
                Npc_DrawDelete(mapObject);
                return true;
            case 0x021EB328:
                Npc_DrawSuspend(mapObject);
                return true;
            case 0x021EB354:
                Npc_DrawResume(mapObject);
                return true;
            case 0x021EB398:
                Npc_Draw(mapObject);
                return true;
            case 0x021EB40C:
                Npc_DrawNoAnim(mapObject);
                return true;
            case 0x021EB438:
                Npc_DrawLoopAnim(mapObject);
                return true;
            case 0x021EB720:
                Nurse_Draw(mapObject);
                return true;
            case 0x021EB7F8:
                Magikarp_DrawInit(mapObject);
                return true;
            case 0x021EB834:
                Magikarp_DrawDelete(mapObject);
                return true;
            case 0x021EB848:
                Magikarp_DrawSuspend(mapObject);
                return true;
            case 0x021EB874:
                Magikarp_DrawResume(mapObject);
                return true;
            case 0x021EB8B8:
                Magikarp_Draw(mapObject);
                return true;
            case 0x021EB944:
                Starly_Draw(mapObject);
                return true;
            case 0x021EBA0C:
                Player_DrawInit(mapObject);
                return true;
            case 0x021EBA34:
                Player_DrawDelete(mapObject);
                return true;
            case 0x021EBA60:
                Player_DrawSuspend(mapObject);
                return true;
            case 0x021EBA8C:
                Player_DrawResume(mapObject);
                return true;
            case 0x021EBAD0:
                Player_Draw(mapObject);
                return true;
            case 0x021ECE18:
                MapObjectRender.MapObject_DrawInitInvisible(mapObject);
                return true;
            case 0x021ECE30:
                MapObjectRender.MapObject_DrawInvisible(mapObject);
                return true;
            case 0x021ECE34:
                MapObjectRender.MapObject_DrawDeleteInvisible(mapObject);
                return true;
            case 0x021ECE38:
                MapObjectRender.MapObject_DrawSuspendInvisible(mapObject);
                return true;
            case 0x021ECE3C:
                MapObjectRender.MapObject_DrawResumeInvisible(mapObject);
                return true;
            default:
                return DispatchPlayerPoses(address, mapObject) || DispatchModel(address, mapObject);
        }
    }

    // OFFSET 0x021EB1A0
    public static Billboard? MapObject_GetBillboard(MapObject mapObject)
    {
        int graphicsID = MapObject.MapObject_GetGraphicsID(mapObject);
        switch (graphicsID)
        {
            case 0x00:
            case 0x15:
            case 0x61:
            case 0x62:
            case 0xB0:
            case 0xB1:
            case 0xB2:
            case 0xB3:
            case 0xB4:
            case 0xB5:
            case 0xBA:
            case 0xBB:
            case 0xBC:
            case 0xBD:
            case 0xC3:
            case 0xC4:
            case 0xC5:
            case 0xC6:
            case 0xC7:
            case 0xC8:
            case 0xC9:
            case 0xD2:
            case 0xD3:
            case 0xD4:
            case 0x107:
            case 0x108:
            case 0x10C:
            case 0x10D:
            case 0x10E:
            case 0x10F:
            case 0x110:
            case 0x111:
            case 0x112:
            case 0x113:
                return GetSpriteDrawData(mapObject).Slot.Billboard;
            case 0x97:
            case 0x98:
            case 0x99:
                return ov5_021ECB80(mapObject);
            case 0x64:
                return null;
        }
        if ((MapObjectRender.MapObject_GetRenderDetails(mapObject)!.Flags & 0xF) == MapObjectRender.RenderTypeBillboard)
        {
            return GetSpriteDrawData(mapObject).Slot.Billboard;
        }
        return null;
    }

    // OFFSET 0x021EB2EC
    public static void Npc_DrawInit(MapObject mapObject)
    {
        MapObjectSpriteDrawData drawData = InitSpriteDrawData(mapObject);
        drawData.LastDir = -1;
        MapObjectRenderResources.MapObject_AddBillboard(mapObject, drawData.Slot);
        if (drawData.Slot.Billboard != null)
        {
            MapObject.MapObject_CallDraw(mapObject);
        }
    }

    // OFFSET 0x021EB314
    public static void Npc_DrawDelete(MapObject mapObject)
    {
        MapObjectSpriteDrawData drawData = GetSpriteDrawData(mapObject);
        MapObjectRenderResources.MapObject_DeleteBillboard(mapObject, drawData.Slot);
    }

    // OFFSET 0x021EB328
    public static void Npc_DrawSuspend(MapObject mapObject)
    {
        MapObjectSpriteDrawData drawData = GetSpriteDrawData(mapObject);
        if (drawData.Slot.Billboard != null)
        {
            MapObjectRenderResources.Billboard_SaveState(drawData.Slot.Billboard, drawData.SavedState);
        }
        MapObjectRenderResources.MapObject_DeleteBillboard(mapObject, drawData.Slot);
        MapObject.MapObject_SetStatusFlagOn(mapObject, StatusDrawSuspended);
    }

    // OFFSET 0x021EB354
    public static void Npc_DrawResume(MapObject mapObject)
    {
        MapObjectSpriteDrawData drawData = GetSpriteDrawData(mapObject);
        if (MapObjectRenderResources.MapObject_IsBillboardPending(mapObject) == 1)
        {
            return;
        }
        if (drawData.Slot.Billboard == null)
        {
            MapObjectRenderResources.MapObject_AddBillboard(mapObject, drawData.Slot);
        }
        if (drawData.Slot.Billboard != null)
        {
            MapObjectRenderResources.Billboard_RestoreState(drawData.Slot.Billboard, drawData.SavedState);
            MapObjectRenderResources.MapObject_UpdateBillboardPos(mapObject, drawData.Slot.Billboard);
            MapObject.MapObject_SetStatusFlagOff(mapObject, StatusDrawSuspended);
        }
    }

    // OFFSET 0x021EB398
    public static void Npc_Draw(MapObject mapObject)
    {
        Billboard? billboard = GetSpriteDrawData(mapObject).Slot.Billboard;
        MapObjectSpriteDrawData drawData = GetSpriteDrawData(mapObject);
        if (MapObjectRenderResources.MapObject_IsBillboardPending(mapObject) == 1)
        {
            return;
        }
        if (billboard == null)
        {
            return;
        }
        int dir = MapObject.MapObject_GetFacingDir(mapObject);
        int drawState = MapObject.MapObject_GetDrawState(mapObject);
        if (drawState >= NpcDrawStateCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        sNpcDrawStates[drawState](mapObject, billboard, drawData, dir);
        drawData.LastDir = (sbyte)dir;
        drawData.LastDrawState = (sbyte)MapObject.MapObject_GetDrawState(mapObject);
        MapObjectRenderResources.MapObject_UpdateBillboardPos(mapObject, billboard);
        MapObject_UpdateBillboardDrawFlag(mapObject, billboard);
    }

    // OFFSET 0x021EB40C
    public static void Npc_DrawNoAnim(MapObject mapObject)
    {
        MapObjectSpriteDrawData drawData = GetSpriteDrawData(mapObject);
        if (MapObjectRenderResources.MapObject_IsBillboardPending(mapObject) == 1)
        {
            return;
        }
        Billboard? billboard = drawData.Slot.Billboard;
        if (billboard == null)
        {
            return;
        }
        MapObjectRenderResources.MapObject_UpdateBillboardPos(mapObject, billboard);
        MapObject_UpdateBillboardDrawFlag(mapObject, billboard);
    }

    // OFFSET 0x021EB438
    public static void Npc_DrawLoopAnim(MapObject mapObject)
    {
        MapObjectSpriteDrawData drawData = GetSpriteDrawData(mapObject);
        if (MapObjectRenderResources.MapObject_IsBillboardPending(mapObject) == 1)
        {
            return;
        }
        Billboard? billboard = drawData.Slot.Billboard;
        if (billboard == null)
        {
            return;
        }
        if (MapObjectRender.MapObject_IsAnimationPaused(mapObject) == 0)
        {
            Billboards.Billboard_AdvanceAnim(billboard, Fx32.One);
        }
        MapObjectRenderResources.MapObject_UpdateBillboardPos(mapObject, billboard);
        MapObject_UpdateBillboardDrawFlag(mapObject, billboard);
    }

    // OFFSET 0x021EB720
    public static void Nurse_Draw(MapObject mapObject)
    {
        Billboard? billboard = GetSpriteDrawData(mapObject).Slot.Billboard;
        MapObjectSpriteDrawData drawData = GetSpriteDrawData(mapObject);
        if (MapObjectRenderResources.MapObject_IsBillboardPending(mapObject) == 1)
        {
            return;
        }
        if (billboard == null)
        {
            return;
        }
        int dir = MapObject.MapObject_GetFacingDir(mapObject);
        int drawState = MapObject.MapObject_GetDrawState(mapObject);
        if (drawState >= NurseDrawStateCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        sNurseDrawStates[drawState](mapObject, billboard, drawData, dir);
        drawData.LastDir = (sbyte)dir;
        drawData.LastDrawState = (sbyte)MapObject.MapObject_GetDrawState(mapObject);
        MapObjectRenderResources.MapObject_UpdateBillboardPos(mapObject, billboard);
        MapObject_UpdateBillboardDrawFlag(mapObject, billboard);
    }

    // OFFSET 0x021EB794
    private static void Nurse_DrawStateIdle(MapObject mapObject, Billboard billboard, MapObjectSpriteDrawData drawData, int dir)
    {
        if (dir != drawData.LastDir || drawData.LastDrawState == NurseDrawStateBow)
        {
            Billboards.Billboard_SetAnimNum(billboard, MapObjectRenderResources.GetWalkAnimForDir(dir));
            Billboards.Billboard_SetAnimFrameNum(billboard, 0);
        }
        else
        {
            Billboard_SnapToIdleFrame(billboard);
        }
        Billboards.Billboard_AdvanceAnim(billboard, 0);
    }

    // OFFSET 0x021EB7D0
    private static void Nurse_DrawStateBow(MapObject mapObject, Billboard billboard, MapObjectSpriteDrawData drawData, int dir)
    {
        if (drawData.LastDrawState != NurseDrawStateBow)
        {
            Billboards.Billboard_SetAnimNum(billboard, NurseBowAnim);
            Billboards.Billboard_SetAnimFrameNum(billboard, 0);
        }
        Billboards.Billboard_AdvanceAnim(billboard, Fx32.One);
    }

    // OFFSET 0x021EB7F8
    public static void Magikarp_DrawInit(MapObject mapObject)
    {
        MapObjectSpriteDrawData drawData = InitSpriteDrawData(mapObject);
        drawData.LastDir = -1;
        drawData.AnimDelay = (sbyte)(MathUtil.LCRNG_Next() % MagikarpMaxDelay);
        MapObjectRenderResources.MapObject_AddBillboard(mapObject, drawData.Slot);
        if (drawData.Slot.Billboard != null)
        {
            MapObject.MapObject_CallDraw(mapObject);
        }
    }

    // OFFSET 0x021EB834
    public static void Magikarp_DrawDelete(MapObject mapObject)
    {
        MapObjectSpriteDrawData drawData = GetSpriteDrawData(mapObject);
        MapObjectRenderResources.MapObject_DeleteBillboard(mapObject, drawData.Slot);
    }

    // OFFSET 0x021EB848
    public static void Magikarp_DrawSuspend(MapObject mapObject)
    {
        MapObjectSpriteDrawData drawData = GetSpriteDrawData(mapObject);
        if (drawData.Slot.Billboard != null)
        {
            MapObjectRenderResources.Billboard_SaveState(drawData.Slot.Billboard, drawData.SavedState);
        }
        MapObjectRenderResources.MapObject_DeleteBillboard(mapObject, drawData.Slot);
        MapObject.MapObject_SetStatusFlagOn(mapObject, StatusDrawSuspended);
    }

    // OFFSET 0x021EB874
    public static void Magikarp_DrawResume(MapObject mapObject)
    {
        MapObjectSpriteDrawData drawData = GetSpriteDrawData(mapObject);
        if (MapObjectRenderResources.MapObject_IsBillboardPending(mapObject) == 1)
        {
            return;
        }
        if (drawData.Slot.Billboard == null)
        {
            MapObjectRenderResources.MapObject_AddBillboard(mapObject, drawData.Slot);
        }
        if (drawData.Slot.Billboard != null)
        {
            MapObjectRenderResources.Billboard_RestoreState(drawData.Slot.Billboard, drawData.SavedState);
            MapObjectRenderResources.MapObject_UpdateBillboardPos(mapObject, drawData.Slot.Billboard);
            MapObject.MapObject_SetStatusFlagOff(mapObject, StatusDrawSuspended);
        }
    }

    // OFFSET 0x021EB8B8
    public static void Magikarp_Draw(MapObject mapObject)
    {
        Billboard? billboard = GetSpriteDrawData(mapObject).Slot.Billboard;
        MapObjectSpriteDrawData drawData = GetSpriteDrawData(mapObject);
        if (MapObjectRenderResources.MapObject_IsBillboardPending(mapObject) == 1)
        {
            return;
        }
        if (billboard == null)
        {
            return;
        }
        int dir = MapObject.MapObject_GetFacingDir(mapObject);
        MapObject.MapObject_GetDrawState(mapObject);
        if (drawData.AnimDelay != 0)
        {
            drawData.AnimDelay--;
            Billboards.Billboard_AdvanceAnim(billboard, 0);
        }
        else
        {
            Billboards.Billboard_AdvanceAnim(billboard, Fx32.One);
            drawData.AnimTimer++;
            if (drawData.AnimTimer >= MagikarpAnimFrames)
            {
                drawData.AnimTimer = 0;
                drawData.AnimDelay = (sbyte)(MathUtil.LCRNG_Next() % MagikarpMaxDelay);
            }
        }
        drawData.LastDir = (sbyte)dir;
        drawData.LastDrawState = (sbyte)MapObject.MapObject_GetDrawState(mapObject);
        MapObjectRenderResources.MapObject_UpdateBillboardPos(mapObject, billboard);
        MapObject_UpdateBillboardDrawFlag(mapObject, billboard);
    }

    // OFFSET 0x021EB944
    public static void Starly_Draw(MapObject mapObject)
    {
        Billboard? billboard = GetSpriteDrawData(mapObject).Slot.Billboard;
        MapObjectSpriteDrawData drawData = GetSpriteDrawData(mapObject);
        if (MapObjectRenderResources.MapObject_IsBillboardPending(mapObject) == 1)
        {
            return;
        }
        if (billboard == null)
        {
            return;
        }
        int dir = MapObject.MapObject_GetFacingDir(mapObject);
        int anim = dir > 1 ? 1 : 0;
        if (MapObject.MapObject_GetDrawState(mapObject) == 0)
        {
            if (anim != Billboards.Billboard_GetAnimNum(billboard) || drawData.LastDrawState != 0)
            {
                Billboards.Billboard_SetAnimNum(billboard, anim);
                Billboards.Billboard_SetAnimFrameNum(billboard, 0);
                Billboards.Billboard_AdvanceAnim(billboard, 0);
            }
            Billboards.Billboard_AdvanceAnim(billboard, 0);
        }
        else
        {
            if (anim != Billboards.Billboard_GetAnimNum(billboard))
            {
                Billboards.Billboard_SetAnimNum(billboard, anim);
                Billboards.Billboard_SetAnimFrameNum(billboard, 0);
            }
            Billboards.Billboard_AdvanceAnim(billboard, Fx32.One);
        }
        drawData.LastDir = (sbyte)dir;
        drawData.LastDrawState = (sbyte)MapObject.MapObject_GetDrawState(mapObject);
        MapObjectRenderResources.MapObject_UpdateBillboardPos(mapObject, billboard);
        MapObject_UpdateBillboardDrawFlag(mapObject, billboard);
    }

    private static void Npc_StartDirAnim(Billboard billboard, int dir)
    {
        Billboards.Billboard_SetAnimNum(billboard, MapObjectRenderResources.GetWalkAnimForDir(dir));
        Billboards.Billboard_SetAnimFrameNum(billboard, 0);
    }

    // OFFSET 0x021EB478
    private static void Npc_DrawStateIdle(MapObject mapObject, Billboard billboard, MapObjectSpriteDrawData drawData, int dir)
    {
        if (dir != drawData.LastDir)
        {
            Npc_StartDirAnim(billboard, dir);
        }
        else
        {
            Billboard_SnapToIdleFrame(billboard);
        }
        Billboards.Billboard_AdvanceAnim(billboard, 0);
    }

    private static void Npc_DrawStateWalk(MapObject mapObject, Billboard billboard, MapObjectSpriteDrawData drawData, int dir, int speed)
    {
        if (dir != drawData.LastDir)
        {
            Npc_StartDirAnim(billboard, dir);
        }
        if (MapObjectRender.MapObject_IsAnimationPaused(mapObject) == 0)
        {
            Billboards.Billboard_AdvanceAnim(billboard, speed);
        }
    }

    // OFFSET 0x021EB4AC
    private static void Npc_DrawStateWalkHalf(MapObject mapObject, Billboard billboard, MapObjectSpriteDrawData drawData, int dir)
    {
        Npc_DrawStateWalk(mapObject, billboard, drawData, dir, Fx32.One / 2);
    }

    // OFFSET 0x021EB4E8
    private static void Npc_DrawStateWalk1(MapObject mapObject, Billboard billboard, MapObjectSpriteDrawData drawData, int dir)
    {
        Npc_DrawStateWalk(mapObject, billboard, drawData, dir, Fx32.One);
    }

    // OFFSET 0x021EB524
    private static void Npc_DrawStateWalk2(MapObject mapObject, Billboard billboard, MapObjectSpriteDrawData drawData, int dir)
    {
        Npc_DrawStateWalk(mapObject, billboard, drawData, dir, 2 * Fx32.One);
    }

    // OFFSET 0x021EB560
    private static void Npc_DrawStateWalk4(MapObject mapObject, Billboard billboard, MapObjectSpriteDrawData drawData, int dir)
    {
        Npc_DrawStateWalk(mapObject, billboard, drawData, dir, 4 * Fx32.One);
    }

    private static void Npc_DrawStateSteps(MapObject mapObject, Billboard billboard, MapObjectSpriteDrawData drawData, int dir, int drawState, int[] frames)
    {
        if (dir != drawData.LastDir)
        {
            Npc_StartDirAnim(billboard, dir);
            drawData.Step = 0;
        }
        if (drawData.LastDrawState != drawState)
        {
            drawData.Step = 0;
        }
        if (MapObjectRender.MapObject_IsAnimationPaused(mapObject) == 0)
        {
            Billboards.Billboard_AdvanceAnim(billboard, frames[drawData.Step]);
            drawData.Step++;
            if (frames[drawData.Step] == 0)
            {
                drawData.Step = 0;
            }
        }
    }

    // OFFSET 0x021EB59C
    private static void Npc_DrawStateSteps6(MapObject mapObject, Billboard billboard, MapObjectSpriteDrawData drawData, int dir)
    {
        Npc_DrawStateSteps(mapObject, billboard, drawData, dir, 6, StepFrames(NpcStepFrames6Address, StepFrames6Count));
    }

    // OFFSET 0x021EB620
    private static void Npc_DrawStateSteps7(MapObject mapObject, Billboard billboard, MapObjectSpriteDrawData drawData, int dir)
    {
        Npc_DrawStateSteps(mapObject, billboard, drawData, dir, 7, StepFrames(NpcStepFrames7Address, StepFrames7Count));
    }

    // OFFSET 0x021EB69C
    private static void Npc_DrawStateSteps8(MapObject mapObject, Billboard billboard, MapObjectSpriteDrawData drawData, int dir)
    {
        Npc_DrawStateSteps(mapObject, billboard, drawData, dir, 8, StepFrames(NpcStepFrames8Address, StepFrames8Count));
    }

    // OFFSET 0x021EBA0C
    public static void Player_DrawInit(MapObject mapObject)
    {
        MapObjectSpriteDrawData drawData = InitSpriteDrawData(mapObject);
        drawData.LastDir = -1;
        MapObjectRenderResources.MapObject_AddBillboard(mapObject, drawData.Slot);
        if (drawData.Slot.Billboard != null)
        {
            MapObject.MapObject_CallDraw(mapObject);
        }
    }

    // OFFSET 0x021EBA34
    public static void Player_DrawDelete(MapObject mapObject)
    {
        VecFx32 offset = default;
        MapObjectSpriteDrawData drawData = GetSpriteDrawData(mapObject);
        MapObjectRenderResources.MapObject_DeleteBillboard(mapObject, drawData.Slot);
        MapObject.MapObject_SetSpriteJumpOffset(mapObject, in offset);
    }

    // OFFSET 0x021EBA60
    public static void Player_DrawSuspend(MapObject mapObject)
    {
        MapObjectSpriteDrawData drawData = GetSpriteDrawData(mapObject);
        if (drawData.Slot.Billboard != null)
        {
            MapObjectRenderResources.Billboard_SaveState(drawData.Slot.Billboard, drawData.SavedState);
        }
        MapObjectRenderResources.MapObject_DeleteBillboard(mapObject, drawData.Slot);
        MapObject.MapObject_SetStatusFlagOn(mapObject, StatusDrawSuspended);
    }

    // OFFSET 0x021EBA8C
    public static void Player_DrawResume(MapObject mapObject)
    {
        MapObjectSpriteDrawData drawData = GetSpriteDrawData(mapObject);
        if (MapObjectRenderResources.MapObject_IsBillboardPending(mapObject) == 1)
        {
            return;
        }
        if (drawData.Slot.Billboard == null)
        {
            MapObjectRenderResources.MapObject_AddBillboard(mapObject, drawData.Slot);
        }
        if (drawData.Slot.Billboard != null)
        {
            MapObjectRenderResources.Billboard_RestoreState(drawData.Slot.Billboard, drawData.SavedState);
            MapObjectRenderResources.MapObject_UpdateBillboardPos(mapObject, drawData.Slot.Billboard);
            MapObject.MapObject_SetStatusFlagOff(mapObject, StatusDrawSuspended);
        }
    }

    // OFFSET 0x021EBAD0
    public static void Player_Draw(MapObject mapObject)
    {
        Billboard? billboard = GetSpriteDrawData(mapObject).Slot.Billboard;
        MapObjectSpriteDrawData drawData = GetSpriteDrawData(mapObject);
        if (MapObjectRenderResources.MapObject_IsBillboardPending(mapObject) == 1)
        {
            return;
        }
        if (billboard == null)
        {
            return;
        }
        int dir = MapObject.MapObject_GetFacingDir(mapObject);
        int drawState = MapObject.MapObject_GetDrawState(mapObject);
        if (drawState >= PlayerDrawStateCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        sPlayerDrawStates[drawState](mapObject, billboard, drawData, dir);
        drawData.LastDir = (sbyte)dir;
        drawData.LastDrawState = (sbyte)MapObject.MapObject_GetDrawState(mapObject);
        MapObjectRenderResources.MapObject_UpdateBillboardPos(mapObject, billboard);
        MapObject_UpdateBillboardDrawFlag(mapObject, billboard);
    }

    // OFFSET 0x021EBB44
    private static void Player_DrawStateIdle(MapObject mapObject, Billboard billboard, MapObjectSpriteDrawData drawData, int dir)
    {
        if (dir != drawData.LastDir)
        {
            Npc_StartDirAnim(billboard, dir);
            drawData.Flags = (byte)(drawData.Flags & ~FlagWasRunning);
            return;
        }
        if (drawData.LastDrawState == DrawStateRunning)
        {
            drawData.Flags = (byte)((drawData.Flags & ~FlagWasRunning) | FlagWasRunning);
            return;
        }
        if ((drawData.Flags & FlagWasRunning) == FlagWasRunning)
        {
            Billboard_SwitchAnimAtIdleFrame(billboard, MapObjectRenderResources.GetWalkAnimForDir(dir));
            drawData.Flags = (byte)(drawData.Flags & ~FlagWasRunning);
            return;
        }
        Billboard_SnapToIdleFrame(billboard);
    }

    // OFFSET 0x021EBBB4
    private static void Player_DrawStateWalkHalf(MapObject mapObject, Billboard billboard, MapObjectSpriteDrawData drawData, int dir)
    {
        if (dir != drawData.LastDir)
        {
            Npc_StartDirAnim(billboard, dir);
        }
        else if (drawData.LastDrawState == DrawStateRunning)
        {
            drawData.Flags = (byte)((drawData.Flags & ~FlagWasRunning) | FlagWasRunning);
            return;
        }
        else if ((drawData.Flags & FlagWasRunning) == FlagWasRunning)
        {
            Billboard_SwitchAnimAtIdleFrame(billboard, MapObjectRenderResources.GetWalkAnimForDir(dir));
            drawData.Flags = (byte)(drawData.Flags & ~FlagWasRunning);
        }
        if (MapObjectRender.MapObject_IsAnimationPaused(mapObject) == 0)
        {
            Billboards.Billboard_AdvanceAnim(billboard, Fx32.One / 2);
        }
    }

    private static void Player_DrawStateWalk(MapObject mapObject, Billboard billboard, MapObjectSpriteDrawData drawData, int dir, int speed)
    {
        if (dir != drawData.LastDir)
        {
            Npc_StartDirAnim(billboard, dir);
        }
        else if (drawData.LastDrawState == DrawStateRunning)
        {
            Billboard_SwitchAnimAtIdleFrame(billboard, MapObjectRenderResources.GetWalkAnimForDir(dir));
        }
        if (MapObjectRender.MapObject_IsAnimationPaused(mapObject) == 0)
        {
            Billboards.Billboard_AdvanceAnim(billboard, speed);
        }
    }

    // OFFSET 0x021EBC28
    private static void Player_DrawStateWalk1(MapObject mapObject, Billboard billboard, MapObjectSpriteDrawData drawData, int dir)
    {
        Player_DrawStateWalk(mapObject, billboard, drawData, dir, Fx32.One);
    }

    // OFFSET 0x021EBC7C
    private static void Player_DrawStateWalk2(MapObject mapObject, Billboard billboard, MapObjectSpriteDrawData drawData, int dir)
    {
        Player_DrawStateWalk(mapObject, billboard, drawData, dir, 2 * Fx32.One);
    }

    // OFFSET 0x021EBCD0
    private static void Player_DrawStateWalk4(MapObject mapObject, Billboard billboard, MapObjectSpriteDrawData drawData, int dir)
    {
        Player_DrawStateWalk(mapObject, billboard, drawData, dir, 4 * Fx32.One);
    }

    private static void Player_DrawStateSteps(MapObject mapObject, Billboard billboard, MapObjectSpriteDrawData drawData, int dir, int drawState, int[] frames)
    {
        if (dir != drawData.LastDir)
        {
            Npc_StartDirAnim(billboard, dir);
            drawData.Step = 0;
        }
        else if (drawData.LastDrawState == DrawStateRunning)
        {
            Billboard_SwitchAnimAtIdleFrame(billboard, MapObjectRenderResources.GetWalkAnimForDir(dir));
        }
        if (drawData.LastDrawState != drawState)
        {
            drawData.Step = 0;
        }
        if (MapObjectRender.MapObject_IsAnimationPaused(mapObject) == 0)
        {
            Billboards.Billboard_AdvanceAnim(billboard, frames[drawData.Step]);
            drawData.Step++;
            if (frames[drawData.Step] == 0)
            {
                drawData.Step = 0;
            }
        }
    }

    // OFFSET 0x021EBD24
    private static void Player_DrawStateSteps6(MapObject mapObject, Billboard billboard, MapObjectSpriteDrawData drawData, int dir)
    {
        Player_DrawStateSteps(mapObject, billboard, drawData, dir, 6, StepFrames(PlayerStepFrames6Address, StepFrames6Count));
    }

    // OFFSET 0x021EBDC4
    private static void Player_DrawStateSteps7(MapObject mapObject, Billboard billboard, MapObjectSpriteDrawData drawData, int dir)
    {
        Player_DrawStateSteps(mapObject, billboard, drawData, dir, 7, StepFrames(PlayerStepFrames7Address, StepFrames7Count));
    }

    // OFFSET 0x021EBE5C
    private static void Player_DrawStateSteps8(MapObject mapObject, Billboard billboard, MapObjectSpriteDrawData drawData, int dir)
    {
        Player_DrawStateSteps(mapObject, billboard, drawData, dir, 8, StepFrames(PlayerStepFrames8Address, StepFrames8Count));
    }

    // OFFSET 0x021EBEFC
    private static void Player_DrawStateRun(MapObject mapObject, Billboard billboard, MapObjectSpriteDrawData drawData, int dir)
    {
        if (dir != drawData.LastDir)
        {
            Billboards.Billboard_SetAnimNum(billboard, MapObjectRenderResources.GetRunAnimForDir(dir));
            Billboards.Billboard_SetAnimFrameNum(billboard, 0);
        }
        else if (drawData.LastDrawState != DrawStateRunning)
        {
            Billboard_SwitchAnimAtFrameCycle(billboard, MapObjectRenderResources.GetRunAnimForDir(dir), RunFrameCycle);
        }
        if (MapObjectRender.MapObject_IsAnimationPaused(mapObject) == 0)
        {
            Billboards.Billboard_AdvanceAnim(billboard, Fx32.One);
        }
    }

    // OFFSET 0x021EC638
    private static void Billboard_SnapToFrameCycle(Billboard billboard, int cycle)
    {
        int frame = Billboards.Billboard_GetAnimFrameNum(billboard) / Fx32.One;
        frame -= frame % cycle;
        Billboards.Billboard_SetAnimFrameNum(billboard, frame << Fx32.Shift);
        Billboards.Billboard_AdvanceAnim(billboard, 0);
    }

    // OFFSET 0x021EC668
    private static void Billboard_SnapToIdleFrame(Billboard billboard)
    {
        Billboard_SnapToFrameCycle(billboard, IdleFrameCycle);
    }

    // OFFSET 0x021EC674
    private static void Billboard_SwitchAnimAtFrameCycle(Billboard billboard, int animNum, int cycle)
    {
        Billboard_SnapToFrameCycle(billboard, cycle);
        int frame = Billboards.Billboard_GetAnimFrameNum(billboard);
        Billboards.Billboard_SetAnimNum(billboard, animNum);
        Billboards.Billboard_SetAnimFrameNum(billboard, frame);
    }

    // OFFSET 0x021EC69C
    private static void Billboard_SwitchAnimAtIdleFrame(Billboard billboard, int animNum)
    {
        Billboard_SnapToIdleFrame(billboard);
        int frame = Billboards.Billboard_GetAnimFrameNum(billboard);
        Billboards.Billboard_SetAnimNum(billboard, animNum);
        Billboards.Billboard_SetAnimFrameNum(billboard, frame);
    }

    // OFFSET 0x021EC6C0
    public static void MapObject_UpdateBillboardDrawFlag(MapObject mapObject, Billboard billboard)
    {
        int draw = 1;
        if (MapObject.MapObject_CheckStatusFlag(mapObject, StatusHideSprite))
        {
            draw = 0;
        }
        if (MapObject.MapObject_CheckStatusFlag(mapObject, StatusUnderCover))
        {
            if (!MapObject.MapObject_CheckStatusFlag(mapObject, StatusCoverVisible))
            {
                draw = 0;
            }
        }
        Billboards.Billboard_SetDrawFlag(billboard, (byte)draw);
    }
}
