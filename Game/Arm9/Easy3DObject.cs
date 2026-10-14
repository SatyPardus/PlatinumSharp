using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class Easy3DModel
{
    public byte[]? Data;
    public G3dModelSet? Set;
    public G3dModel? Model;
    public G3dTexture? Texture;
}

public sealed class Easy3DAnim
{
    public byte[]? Data;
    public G3dAnim? Anim;
    public G3dAnimObj? AnimObj;
    public int Frame;
    public int DataBorrowed;
}

public sealed class Easy3DObject
{
    public const int RotationAxisX = 0;
    public const int RotationAxisY = 1;
    public const int RotationAxisZ = 2;
    public const int RotationAxisCount = 3;

    private const uint TextureUploadTaskPriority = 0x400;

    public G3dRenderObj RenderObj = new();
    public VecFx32 Position;
    public VecFx32 Scale;
    public int Visible;
    public readonly ushort[] Rotation = new ushort[RotationAxisCount];

    // OFFSET 0x020170BC
    public static void Easy3DModel_Load(Easy3DModel model, int narcID, uint memberIndex, int heapID)
    {
        model.Data = Graphics.LoadMemberFromNARC(narcID, memberIndex, 0, heapID, 0);
        Easy3DModel_BindResources(model);
    }

    // OFFSET 0x020170D8
    public static void Easy3DModel_LoadFrom(Easy3DModel model, NARC narc, uint memberIndex, int heapID)
    {
        model.Data = Graphics.LoadMemberFromOpenNARC(narc, memberIndex, 0, heapID, 0);
        Easy3DModel_BindResources(model);
    }

    // OFFSET 0x020170F4
    private static void Easy3DModel_UploadTextureTask(SysTask task, object? param)
    {
        Easy3DModel model = (Easy3DModel)param!;
        Easy3D.Easy3D_UploadTextureToVRAM(model.Texture!);
        Easy3D.Easy3D_BindTextureToResource(model.Data!, model.Texture!);
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x02017110
    public static void Easy3DModel_Release(Easy3DModel model)
    {
        if (model.Texture != null)
        {
            G3d.ReleaseTextureKeys(model.Texture, out uint texKey, out uint tex4x4Key);
            Gfd.FreeTexVram(texKey);
            Gfd.FreeTexVram(tex4x4Key);
            uint plttKey = G3d.ReleasePaletteKey(model.Texture);
            Gfd.FreePlttVram(plttKey);
        }
        model.Data = null;
        model.Set = null;
        model.Model = null;
        model.Texture = null;
    }

    // OFFSET 0x02017164
    public static void Easy3DAnim_LoadFrom(Easy3DAnim anim, Easy3DModel model, NARC narc, uint memberIndex, int heapID, G3dAllocator allocator)
    {
        byte[]? data = Graphics.LoadMemberFromOpenNARC(narc, memberIndex, 0, heapID, 0);
        Easy3DAnim_Bind(anim, model, data!, allocator);
        anim.DataBorrowed = 0;
    }

    // OFFSET 0x02017190
    public static void Easy3DAnim_LoadFromData(Easy3DAnim anim, Easy3DModel model, byte[] data, G3dAllocator allocator)
    {
        Easy3DAnim_Bind(anim, model, data, allocator);
        anim.DataBorrowed = 1;
    }

    // OFFSET 0x020171A0
    public static void Easy3DAnim_Release(Easy3DAnim anim, G3dAllocator allocator)
    {
        if (anim.Data != null)
        {
            G3d.FreeAnimObj(allocator, anim.AnimObj!);
        }
        anim.Data = null;
        anim.Anim = null;
        anim.AnimObj = null;
        anim.Frame = 0;
        anim.DataBorrowed = 0;
    }

    // OFFSET 0x020171CC
    public static void Easy3DAnim_UpdateLooped(Easy3DAnim anim, int frameDelta)
    {
        int frameCount = anim.AnimObj!.ResAnm!.NumFrames << Fx32.Shift;
        if (frameDelta > 0)
        {
            anim.Frame = (anim.Frame + frameDelta) % frameCount;
        }
        else
        {
            anim.Frame += frameDelta;
            if (anim.Frame < 0)
            {
                anim.Frame += frameCount;
            }
        }
        anim.AnimObj.Frame = anim.Frame;
    }

    // OFFSET 0x02017204
    public static bool Easy3DAnim_Update(Easy3DAnim anim, int frameDelta)
    {
        int frameCount = anim.AnimObj!.ResAnm!.NumFrames << Fx32.Shift;
        bool finished = false;
        if (frameDelta > 0)
        {
            if (anim.Frame + frameDelta < frameCount)
            {
                anim.Frame += frameDelta;
            }
            else
            {
                anim.Frame = frameCount;
                finished = true;
            }
        }
        else
        {
            if (anim.Frame + frameDelta >= 0)
            {
                anim.Frame += frameDelta;
            }
            else
            {
                anim.Frame = 0;
                finished = true;
            }
        }
        anim.AnimObj.Frame = anim.Frame;
        return finished;
    }

    // OFFSET 0x02017240
    public static void Easy3DAnim_SetFrame(Easy3DAnim anim, int frame)
    {
        anim.Frame = frame;
        anim.AnimObj!.Frame = frame;
    }

    // OFFSET 0x02017248
    public static int Easy3DAnim_GetFrame(Easy3DAnim anim)
    {
        return anim.Frame;
    }

    // OFFSET 0x0201724C
    public static int Easy3DAnim_GetFrameCount(Easy3DAnim anim)
    {
        return anim.AnimObj!.ResAnm!.NumFrames << Fx32.Shift;
    }

    // OFFSET 0x02017258
    public static void Easy3DObject_Init(Easy3DObject obj, Easy3DModel model)
    {
        obj.RenderObj = new G3dRenderObj();
        obj.Position = default;
        Array.Clear(obj.Rotation);
        G3d.InitRenderObj(obj.RenderObj, model.Model!);
        obj.Visible = 1;
        obj.Scale.X = Fx32.One;
        obj.Scale.Y = Fx32.One;
        obj.Scale.Z = Fx32.One;
    }

    // OFFSET 0x0201727C
    public static void Easy3DObject_AddAnim(Easy3DObject obj, Easy3DAnim anim)
    {
        G3d.AddAnimObj(obj.RenderObj, anim.AnimObj!);
    }

    // OFFSET 0x02017288
    public static void Easy3DObject_RemoveAnim(Easy3DObject obj, Easy3DAnim anim)
    {
        G3d.RemoveAnimObj(obj.RenderObj, anim.AnimObj!);
    }

    // OFFSET 0x02017294
    public static void Easy3DObject_Draw(Easy3DObject obj)
    {
        if (obj.Visible == 0)
        {
            return;
        }
        Mtx.Identity33(out MtxFx33 rotation);
        int angle = obj.Rotation[RotationAxisX] >> 4;
        Mtx.RotX33(out MtxFx33 axisRotation, Render3D.Sin(angle), Render3D.Cos(angle));
        Mtx.Concat33(axisRotation, rotation, out rotation);
        angle = obj.Rotation[RotationAxisZ] >> 4;
        Mtx.RotZ33(out axisRotation, Render3D.Sin(angle), Render3D.Cos(angle));
        Mtx.Concat33(axisRotation, rotation, out rotation);
        angle = obj.Rotation[RotationAxisY] >> 4;
        Mtx.RotY33(out axisRotation, Render3D.Sin(angle), Render3D.Cos(angle));
        Mtx.Concat33(axisRotation, rotation, out rotation);
        Easy3D.Easy3D_DrawRenderObj(obj.RenderObj, obj.Position, rotation, obj.Scale);
    }

    // OFFSET 0x02017330
    public static void Easy3DObject_DrawRotated(Easy3DObject obj, in MtxFx33 rotation)
    {
        if (obj.Visible != 0)
        {
            Easy3D.Easy3D_DrawRenderObj(obj.RenderObj, obj.Position, rotation, obj.Scale);
        }
    }

    // OFFSET 0x02017348
    public static void Easy3DObject_SetVisible(Easy3DObject obj, int visible)
    {
        obj.Visible = visible;
    }

    // OFFSET 0x0201734C
    public static int Easy3DObject_IsVisible(Easy3DObject obj)
    {
        return obj.Visible;
    }

    // OFFSET 0x02017350
    public static void Easy3DObject_SetPosition(Easy3DObject obj, int x, int y, int z)
    {
        obj.Position.X = x;
        obj.Position.Y = y;
        obj.Position.Z = z;
    }

    // OFFSET 0x02017358
    public static void Easy3DObject_GetPosition(Easy3DObject obj, out int x, out int y, out int z)
    {
        x = obj.Position.X;
        y = obj.Position.Y;
        z = obj.Position.Z;
    }

    // OFFSET 0x0201736C
    public static void Easy3DObject_SetScale(Easy3DObject obj, int x, int y, int z)
    {
        obj.Scale.X = x;
        obj.Scale.Y = y;
        obj.Scale.Z = z;
    }

    // OFFSET 0x02017374
    public static void Easy3DObject_GetScale(Easy3DObject obj, out int x, out int y, out int z)
    {
        x = obj.Scale.X;
        y = obj.Scale.Y;
        z = obj.Scale.Z;
    }

    // OFFSET 0x02017388
    public static void Easy3DObject_SetRotation(Easy3DObject obj, ushort angle, int axis)
    {
        obj.Rotation[axis] = angle;
    }

    // OFFSET 0x02017394
    public static ushort Easy3DObject_GetRotation(Easy3DObject obj, int axis)
    {
        return obj.Rotation[axis];
    }

    // OFFSET 0x020173A0
    private static void Easy3DAnim_Bind(Easy3DAnim anim, Easy3DModel model, byte[] data, G3dAllocator allocator)
    {
        anim.Data = data;
        anim.Anim = G3d.GetAnimByIndex(data, 0);
        anim.AnimObj = G3d.AllocAnimObj(allocator, anim.Anim!, model.Model!);
        G3d.InitAnimObj(anim.AnimObj, anim.Anim!, model.Model!, model.Texture);
    }

    // OFFSET 0x020173CC
    private static void Easy3DModel_BindResources(Easy3DModel model)
    {
        if (model.Data == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        model.Set = G3d.GetModelSet(model.Data!);
        model.Model = G3dModelSet_GetModelByIndex(model.Set, 0);
        model.Texture = G3d.GetTexture(model.Data!);
        if (model.Texture != null)
        {
            SysTask.SysTask_ExecuteAfterVBlank(Easy3DModel_UploadTextureTask, model, TextureUploadTaskPriority);
        }
    }

    private static G3dModel? G3dModelSet_GetModelByIndex(G3dModelSet? set, int index)
    {
        if (set == null)
        {
            return null;
        }
        int dictionary = set.Offset + 8;
        int entryCount = set.File[dictionary + 1];
        if (index >= entryCount)
        {
            return null;
        }
        int entries = dictionary + (set.File[dictionary + 6] | (set.File[dictionary + 7] << 8)) + 4;
        int modelOffset = BitConverter.ToInt32(set.File, entries + index * 4);
        return new G3dModel { File = set.File, Offset = set.Offset + modelOffset };
    }
}
