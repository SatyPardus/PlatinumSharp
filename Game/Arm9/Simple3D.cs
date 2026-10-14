namespace PlatinumSharp.Game;

public sealed class Simple3DModel
{
    public const int Size = 0x14;

    public int TextureBound;
    public byte[]? Data;
    public G3dModelSet? Set;
    public G3dModel? Model;
    public G3dTexture? Texture;
}

public sealed class Simple3DAnimation
{
    public const int Size = 0x24;

    public uint Flags;
    public int Frame;
    public byte[]? Data;
    public G3dAnim? Anim;
    public G3dAnimObj? AnimObj;
    public readonly HeapAllocator Allocator = new();
    public readonly G3dAllocator AnimAllocator = new();
}

public static class Simple3D
{
    private const uint AnimationOwnsData = 1;
    private const uint AnimationFinished = 2;
    private const uint BindTextureTaskPriority = 0xFFFF;

    private static readonly VecFx32 sDefaultScale = new() { X = Fx32.One, Y = Fx32.One, Z = Fx32.One };

    private static readonly MtxFx33 sDefaultRotation = new()
    {
        M00 = Fx32.One,
        M11 = Fx32.One,
        M22 = Fx32.One,
    };

    // OFFSET 0x02073838
    private static void Simple3DModel_Clear(Simple3DModel model)
    {
        model.TextureBound = 0;
        model.Data = null;
        model.Set = null;
        model.Model = null;
        model.Texture = null;
    }

    // OFFSET 0x02073848
    public static void Simple3DModel_Init(Simple3DModel model, byte[] data, int modelIndex)
    {
        Simple3DModel_Clear(model);
        model.Data = data;
        model.TextureBound = 0;
        model.Set = G3d.GetModelSet(data);
        model.Model = model.Set != null ? G3d.GetModelByIndex(model.Set, modelIndex) : null;
        model.Texture = G3d.GetTexture(data);
    }

    // OFFSET 0x0207389C
    public static void Simple3DModel_LoadFromNARC(Simple3DModel model, int modelIndex, NARC narc, int memberIndex, int heapID, int allocAtEnd)
    {
        byte[]? data = Graphics.LoadMemberFromOpenNARC(narc, (uint)memberIndex, 0, heapID, allocAtEnd);
        Simple3DModel_Init(model, data!, modelIndex);
    }

    // OFFSET 0x020738C0
    public static void Simple3DModel_BindTexture(Simple3DModel model)
    {
        Easy3D.Easy3D_UploadTextureToVRAM(model.Texture);
        G3d.BindModelSet(model.Set!, model.Texture!);
        model.TextureBound = 1;
    }

    // OFFSET 0x020738D8
    private static void Simple3DModel_BindTextureTask(SysTask task, object? param)
    {
        Simple3DModel_BindTexture((Simple3DModel)param!);
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x020738EC
    public static void Simple3DModel_BindTextureOnVBlank(Simple3DModel model)
    {
        SysTask? task = SysTask.SysTask_ExecuteOnVBlank(Simple3DModel_BindTextureTask, model, BindTextureTaskPriority);
        if (task == null)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x0207390C
    private static void Simple3DModel_FreeData(Simple3DModel model)
    {
        if (model.Data != null)
        {
            model.Data = null;
        }
    }

    // OFFSET 0x0207391C
    private static void Simple3DModel_ReleaseTexture(Simple3DModel model)
    {
        if (model.Texture != null)
        {
            G3d.ReleaseTextureKeys(model.Texture, out uint texKey, out uint tex4x4Key);
            Gfd.FreeTexVram(texKey);
            Gfd.FreeTexVram(tex4x4Key);
            uint plttKey = G3d.ReleasePaletteKey(model.Texture);
            Gfd.FreePlttVram(plttKey);
            model.Texture = null;
        }
    }

    // OFFSET 0x0207395C
    public static void Simple3DModel_Free(Simple3DModel model)
    {
        Simple3DModel_ReleaseTexture(model);
        Simple3DModel_FreeData(model);
        Simple3DModel_Clear(model);
    }

    // OFFSET 0x02073974
    private static void Simple3DAnimation_Clear(Simple3DAnimation animation)
    {
        animation.Flags = 0;
        animation.Frame = 0;
        animation.Data = null;
        animation.Anim = null;
        animation.AnimObj = null;
        animation.Allocator.Heap = null;
        animation.Allocator.Alignment = 0;
    }

    // OFFSET 0x02073980
    public static void Simple3DAnimation_InitOwned(Simple3DAnimation animation, byte[] data, int unused)
    {
        Simple3DAnimation_SetData(animation, data, unused);
        animation.Flags |= AnimationOwnsData;
    }

    // OFFSET 0x02073994
    public static void Simple3DAnimation_InitBorrowed(Simple3DAnimation animation, byte[] data, int unused)
    {
        Simple3DAnimation_SetData(animation, data, unused);
        animation.Flags &= ~AnimationOwnsData;
    }

    // OFFSET 0x020739A8
    public static void Simple3DAnimation_InitFromAnimation(Simple3DAnimation animation, Simple3DAnimation source, int unused)
    {
        Simple3DAnimation_InitBorrowed(animation, source.Data!, unused);
    }

    // OFFSET 0x020739B4
    public static void Simple3DAnimation_LoadFromNARC(Simple3DAnimation animation, int unused, NARC narc, int memberIndex, int heapID, int allocAtEnd)
    {
        byte[]? data = Graphics.LoadMemberFromOpenNARC(narc, (uint)memberIndex, 0, heapID, allocAtEnd);
        Simple3DAnimation_InitOwned(animation, data!, unused);
    }

    // OFFSET 0x020739D8
    private static void Simple3DAnimation_SetData(Simple3DAnimation animation, byte[] data, int unused)
    {
        Simple3DAnimation_Clear(animation);
        animation.Data = data;
        animation.Anim = G3d.GetAnimByIndex(data, 0);
    }

    // OFFSET 0x020739F0
    private static void Simple3DAnimation_FreeData(Simple3DAnimation animation)
    {
        if ((animation.Flags & AnimationOwnsData) != 0)
        {
            animation.Data = null;
            animation.Flags &= ~AnimationOwnsData;
        }
        animation.Data = null;
        animation.Anim = null;
    }

    // OFFSET 0x02073A14
    private static void Simple3DAnimation_AllocAnimObjForModel(Simple3DAnimation animation, G3dModel model, int heapID)
    {
        Heap.HeapExp_FndInitAllocator(animation.Allocator, heapID, 4);
        animation.AnimObj = G3d.AllocAnimObj(animation.AnimAllocator, animation.Anim!, model);
        if (animation.AnimObj == null)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02073A3C
    public static void Simple3DAnimation_AllocAnimObj(Simple3DAnimation animation, Simple3DModel model, int heapID)
    {
        Simple3DAnimation_AllocAnimObjForModel(animation, model.Model!, heapID);
    }

    // OFFSET 0x02073A48
    private static void Simple3DAnimation_BindToModelAndTexture(Simple3DAnimation animation, G3dModel model, G3dTexture? texture)
    {
        G3d.InitAnimObj(animation.AnimObj!, animation.Anim!, model, texture);
    }

    // OFFSET 0x02073A5C
    public static void Simple3DAnimation_BindToModel(Simple3DAnimation animation, Simple3DModel model)
    {
        Simple3DAnimation_BindToModelAndTexture(animation, model.Model!, model.Texture);
    }

    // OFFSET 0x02073A6C
    public static void Simple3DAnimation_CopyAndBindToModel(Simple3DAnimation animation, Simple3DModel model, Simple3DAnimation source, int unused, int heapID)
    {
        Simple3DAnimation_InitFromAnimation(animation, source, unused);
        Simple3DAnimation_AllocAnimObj(animation, model, heapID);
        Simple3DAnimation_BindToModel(animation, model);
    }

    // OFFSET 0x02073A90
    private static void Simple3DAnimation_FreeAnimObj(Simple3DAnimation animation)
    {
        if (animation.AnimObj != null)
        {
            G3d.FreeAnimObj(animation.AnimAllocator, animation.AnimObj);
            animation.AnimObj = null;
        }
    }

    // OFFSET 0x02073AA8
    public static void Simple3DAnimation_Free(Simple3DAnimation animation)
    {
        Simple3DAnimation_FreeAnimObj(animation);
        Simple3DAnimation_FreeData(animation);
        Simple3DAnimation_Clear(animation);
    }

    // OFFSET 0x02073AC0
    public static int Simple3DAnimation_Advance(Simple3DAnimation animation, int frameDelta, int loop)
    {
        int frameCount = animation.AnimObj!.ResAnm!.NumFrames << Fx32.Shift;
        int finished = 0;
        animation.Frame += frameDelta;
        if (frameDelta > 0)
        {
            if (animation.Frame >= frameCount)
            {
                finished = 1;
                if (loop == 1)
                {
                    animation.Frame -= frameCount;
                }
                else
                {
                    animation.Frame = frameCount;
                }
            }
        }
        else if (animation.Frame <= 0)
        {
            finished = 1;
            if (loop == 1)
            {
                animation.Frame += frameCount;
            }
            else
            {
                animation.Frame = 0;
            }
        }
        animation.AnimObj.Frame = animation.Frame;
        if (finished == 1)
        {
            animation.Flags |= AnimationFinished;
        }
        else
        {
            animation.Flags &= ~AnimationFinished;
        }
        return finished;
    }

    // OFFSET 0x02073B20
    public static void Simple3DAnimation_SetFrame(Simple3DAnimation animation, int frame)
    {
        animation.Frame = frame;
    }

    // OFFSET 0x02073B24
    public static int Simple3DAnimation_GetFrame(Simple3DAnimation animation)
    {
        return animation.Frame;
    }

    // OFFSET 0x02073B28
    public static int Simple3DAnimation_GetFrameCount(Simple3DAnimation animation)
    {
        if (animation.AnimObj == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return animation.AnimObj!.ResAnm!.NumFrames << Fx32.Shift;
    }

    // OFFSET 0x02073B40
    public static int Simple3DAnimation_IsFinished(Simple3DAnimation animation)
    {
        if ((animation.Flags & AnimationFinished) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02073B50
    private static void Simple3DRenderObj_Clear(G3dRenderObj renderObj)
    {
        renderObj.Model = null;
        renderObj.AnimObjs.Clear();
    }

    // OFFSET 0x02073B5C
    private static void Simple3DRenderObj_InitFromG3dModel(G3dRenderObj renderObj, G3dModel model)
    {
        Simple3DRenderObj_Clear(renderObj);
        G3d.InitRenderObj(renderObj, model);
    }

    // OFFSET 0x02073B70
    public static void Simple3DRenderObj_Init(G3dRenderObj renderObj, Simple3DModel model)
    {
        Simple3DRenderObj_InitFromG3dModel(renderObj, model.Model!);
    }

    // OFFSET 0x02073B7C
    private static void Simple3DRenderObj_AddAnimObj(G3dRenderObj renderObj, G3dAnimObj animObj)
    {
        G3d.AddAnimObj(renderObj, animObj);
    }

    // OFFSET 0x02073B84
    public static void Simple3DRenderObj_AddAnimation(G3dRenderObj renderObj, Simple3DAnimation animation)
    {
        Simple3DRenderObj_AddAnimObj(renderObj, animation.AnimObj!);
    }

    // OFFSET 0x02073B90
    public static void Simple3DRenderObj_InitWithAnimation(G3dRenderObj renderObj, Simple3DModel model, Simple3DAnimation animation)
    {
        Simple3DRenderObj_Init(renderObj, model);
        Simple3DRenderObj_AddAnimation(renderObj, animation);
    }

    // OFFSET 0x02073BA4
    public static void Simple3DRenderObj_Draw(G3dRenderObj renderObj, in VecFx32 position, in VecFx32 scale, in MtxFx33 rotation)
    {
        Easy3D.Easy3D_DrawRenderObj(renderObj, position, rotation, scale);
    }

    // OFFSET 0x02073BB4
    public static void Simple3DRenderObj_DrawAtPosition(G3dRenderObj renderObj, in VecFx32 position)
    {
        Simple3DRenderObj_Draw(renderObj, position, sDefaultScale, sDefaultRotation);
    }

    // OFFSET 0x02073BC8
    public static void Simple3DRenderObj_DrawWithAngles(G3dRenderObj renderObj, in VecFx32 position, in VecFx32 scale, ushort[] angles)
    {
        FxUtil.MTX_Rot33Angles(out MtxFx33 rotation, angles[0], angles[1], angles[2]);
        Simple3DRenderObj_Draw(renderObj, position, scale, rotation);
    }

    // OFFSET 0x02073BF0
    public static void Simple3DRenderObj_DrawAtPositionWithAngles(G3dRenderObj renderObj, in VecFx32 position, ushort[] angles)
    {
        FxUtil.MTX_Rot33Angles(out MtxFx33 rotation, angles[0], angles[1], angles[2]);
        Simple3DRenderObj_Draw(renderObj, position, sDefaultScale, rotation);
    }

    // OFFSET 0x02073C1C
    public static void Simple3DRenderObj_DrawSimple(G3dRenderObj renderObj, in VecFx32 position, in VecFx32 scale, in MtxFx33 rotation)
    {
        Easy3D.Easy3D_DrawRenderObjSimple(renderObj, position, rotation, scale);
    }
}
