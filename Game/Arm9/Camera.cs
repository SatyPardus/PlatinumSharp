using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public struct CameraAngle
{
    public ushort X;
    public ushort Y;
    public ushort Z;
    public ushort Padding;
}

public sealed class CameraPositionHistory
{
    public int HistorySize;
    public int CurrentHistoryIndex;
    public int NextHistoryIndex;
    public int Delay;
    public int DelayReached;
    public int DelayX;
    public int DelayY;
    public int DelayZ;
    public VecFx32[] Positions = Array.Empty<VecFx32>();
}

public sealed class Camera
{
    public const int ProjectionPerspective = 0;
    public const int ProjectionOrthographic = 1;
    public const int DelayMaskX = 1;
    public const int DelayMaskY = 2;
    public const int DelayMaskZ = 4;

    private const int DefaultAspectRatio = 0x1555;
    private const int DefaultNearClip = 150 * Fx32.One;
    private const int DefaultFarClip = 900 * Fx32.One;

    public int SinFovY;
    public int CosFovY;
    public int AspectRatio;
    public int NearClip;
    public int FarClip;
    public VecFx32 Position;
    public VecFx32 Target;
    public VecFx32 Up;
    public int Distance;
    public CameraAngle Angle;
    public byte Projection;
    public ushort FovY;
    public VecFx32 PrevTargetPos;
    public Func<VecFx32>? TargetPos;
    public int TrackTargetX;
    public int TrackTargetY;
    public int TrackTargetZ;
    public CameraPositionHistory? History;

    private static Camera? sActiveCamera;

    // OFFSET 0x02020020
    private static void Camera_UpdatePositionAroundTarget(Camera camera)
    {
        ushort angleX = camera.Angle.X;
        camera.Position.X = Fx32.Mul(Fx32.Mul(Render3D.Sin(camera.Angle.Y >> 4), camera.Distance), Render3D.Cos(angleX >> 4));
        camera.Position.Z = Fx32.Mul(Fx32.Mul(Render3D.Cos(camera.Angle.Y >> 4), camera.Distance), Render3D.Cos(camera.Angle.X >> 4));
        camera.Position.Y = Fx32.Mul(Render3D.Sin((ushort)-angleX >> 4), camera.Distance);
        Mtx.VecAdd(camera.Position, camera.Target, out camera.Position);
    }

    // OFFSET 0x02020100
    private static void Camera_UpdateTargetAroundPosition(Camera camera)
    {
        ushort angleX = camera.Angle.X;
        camera.Target.X = -Fx32.Mul(Fx32.Mul(Render3D.Sin(camera.Angle.Y >> 4), camera.Distance), Render3D.Cos(angleX >> 4));
        camera.Target.Z = -Fx32.Mul(Fx32.Mul(Render3D.Cos(camera.Angle.Y >> 4), camera.Distance), Render3D.Cos(camera.Angle.X >> 4));
        camera.Target.Y = -Fx32.Mul(Render3D.Sin((ushort)-angleX >> 4), camera.Distance);
        Mtx.VecAdd(camera.Target, camera.Position, out camera.Target);
    }

    // OFFSET 0x020201E4
    private static void Camera_InitPerspective(ushort fovY, Camera camera)
    {
        camera.FovY = fovY;
        camera.SinFovY = Render3D.Sin(fovY >> 4);
        camera.CosFovY = Render3D.Cos(fovY >> 4);
        camera.AspectRatio = DefaultAspectRatio;
        camera.NearClip = DefaultNearClip;
        camera.FarClip = DefaultFarClip;
        camera.Up.X = 0;
        camera.Up.Y = Fx32.One;
        camera.Up.Z = 0;
        camera.TargetPos = null;
        camera.TrackTargetX = 0;
        camera.TrackTargetY = 0;
        camera.TrackTargetZ = 0;
        camera.History = null;
    }

    // OFFSET 0x02020230
    private static void Camera_ApplyTrackingMask(Camera camera, ref VecFx32 delta)
    {
        if (camera.TrackTargetX == 0)
        {
            delta.X = 0;
        }
        if (camera.TrackTargetY == 0)
        {
            delta.Y = 0;
        }
        if (camera.TrackTargetZ == 0)
        {
            delta.Z = 0;
        }
    }

    // OFFSET 0x02020250
    private static void Camera_UpdateHistory(Camera camera, in VecFx32 delta, out VecFx32 result)
    {
        CameraPositionHistory? history = camera.History;
        if (history == null)
        {
            result = delta;
            return;
        }
        if (history.DelayReached == 0)
        {
            result = delta;
            if (history.CurrentHistoryIndex == history.Delay)
            {
                history.DelayReached = 1;
            }
        }
        else
        {
            result = history.Positions[history.CurrentHistoryIndex];
        }
        history.CurrentHistoryIndex = (history.CurrentHistoryIndex + 1) % history.HistorySize;
        history.Positions[history.NextHistoryIndex] = delta;
        history.NextHistoryIndex = (history.NextHistoryIndex + 1) % history.HistorySize;
        if (history.DelayX == 0)
        {
            result.X = delta.X;
        }
        if (history.DelayY == 0)
        {
            result.Y = delta.Y;
        }
        if (history.DelayZ == 0)
        {
            result.Z = delta.Z;
        }
    }

    // OFFSET 0x02020304
    public static void Camera_InitHistory(int historySize, int delay, int delayMask, int heapID, Camera camera)
    {
        if (camera.TargetPos == null)
        {
            return;
        }
        if (delay + 1 > historySize)
        {
            ErrorHandling.GF_AssertFail();
        }
        CameraPositionHistory history = new();
        history.Positions = new VecFx32[historySize];
        history.HistorySize = historySize;
        history.CurrentHistoryIndex = 0;
        history.NextHistoryIndex = delay;
        history.Delay = delay;
        history.DelayReached = 0;
        history.DelayX = 0;
        history.DelayY = 0;
        history.DelayZ = 0;
        if ((delayMask & DelayMaskX) != 0)
        {
            history.DelayX = 1;
        }
        if ((delayMask & DelayMaskY) != 0)
        {
            history.DelayY = 1;
        }
        if ((delayMask & DelayMaskZ) != 0)
        {
            history.DelayZ = 1;
        }
        camera.History = history;
    }

    // OFFSET 0x02020390
    public static void Camera_DeleteHistory(Camera camera)
    {
        if (camera.History != null)
        {
            camera.History = null;
        }
    }

    // OFFSET 0x020203AC
    public static Camera Camera_Alloc(int heapID)
    {
        return new Camera();
    }

    // OFFSET 0x020203B8
    public static void Camera_Delete(Camera camera)
    {
    }

    // OFFSET 0x020203C0
    public static void Camera_Copy(Camera source, Camera destination)
    {
        destination.SinFovY = source.SinFovY;
        destination.CosFovY = source.CosFovY;
        destination.AspectRatio = source.AspectRatio;
        destination.NearClip = source.NearClip;
        destination.FarClip = source.FarClip;
        destination.Position = source.Position;
        destination.Target = source.Target;
        destination.Up = source.Up;
        destination.Distance = source.Distance;
        destination.Angle = source.Angle;
        destination.Projection = source.Projection;
        destination.FovY = source.FovY;
        destination.PrevTargetPos = source.PrevTargetPos;
        destination.TargetPos = source.TargetPos;
        destination.TrackTargetX = source.TrackTargetX;
        destination.TrackTargetY = source.TrackTargetY;
        destination.TrackTargetZ = source.TrackTargetZ;
        destination.History = source.History;
    }

    // OFFSET 0x020203D4
    public static void Camera_SetAsActive(Camera camera)
    {
        sActiveCamera = camera;
    }

    // OFFSET 0x020203E0
    public static void Camera_ClearActive()
    {
        sActiveCamera = null;
    }

    private static void Camera_FollowTarget(Camera camera)
    {
        VecFx32 targetPos = camera.TargetPos!();
        Mtx.VecSubtract(targetPos, camera.PrevTargetPos, out VecFx32 delta);
        Camera_ApplyTrackingMask(camera, ref delta);
        Camera_UpdateHistory(camera, delta, out VecFx32 movement);
        Camera_Move(movement, camera);
        camera.PrevTargetPos = targetPos;
    }

    // OFFSET 0x020203EC
    public static void Camera_ComputeViewMatrix()
    {
        if (sActiveCamera == null)
        {
            return;
        }
        if (sActiveCamera.TargetPos != null)
        {
            Camera_FollowTarget(sActiveCamera);
        }
        Render3D.SetLookAt(sActiveCamera.Position, sActiveCamera.Up, sActiveCamera.Target);
    }

    // OFFSET 0x0202049C
    public static void Camera_ComputeViewMatrixWithRoll()
    {
        if (sActiveCamera == null)
        {
            return;
        }
        if (sActiveCamera.TargetPos == null)
        {
            Render3D.SetLookAt(sActiveCamera.Position, sActiveCamera.Up, sActiveCamera.Target);
            return;
        }
        Camera camera = sActiveCamera;
        Camera_FollowTarget(camera);
        VecFx32 position = camera.Position;
        VecFx32 up = camera.Up;
        VecFx32 target = camera.TargetPos!();
        int cosX = Render3D.Cos(camera.Angle.X >> 4);
        int sinY = Render3D.Sin(camera.Angle.Y >> 4);
        int cosY = Render3D.Cos(camera.Angle.Y >> 4);
        int sinZ = Render3D.Sin(camera.Angle.Z >> 4);
        int cosZ = Render3D.Cos(camera.Angle.Z >> 4);
        position.Z = Fx32.Mul(cosY, camera.Distance);
        position.Z = Fx32.Mul(position.Z, cosX);
        position.Z += target.Z;
        up.X += Fx32.Mul(sinZ, cosY);
        up.Y += cosZ;
        up.Z -= Fx32.Mul(sinZ, sinY);
        Render3D.SetLookAt(position, up, target);
    }

    // OFFSET 0x02020680
    public static void Camera_SetUp(in VecFx32 up, Camera camera)
    {
        camera.Up = up;
    }

    // OFFSET 0x02020690
    public static void Camera_TrackTarget(Func<VecFx32> target, Camera camera)
    {
        camera.TargetPos = target;
        camera.PrevTargetPos = target();
        camera.TrackTargetX = 1;
        camera.TrackTargetY = 1;
        camera.TrackTargetZ = 1;
    }

    // OFFSET 0x020206B0
    public static void Camera_ReleaseTarget(Camera camera)
    {
        camera.TargetPos = null;
        camera.TrackTargetX = 0;
        camera.TrackTargetY = 0;
        camera.TrackTargetZ = 0;
    }

    // OFFSET 0x020206BC
    public static void Camera_SetClipping(int nearClip, int farClip, Camera camera)
    {
        camera.NearClip = nearClip;
        camera.FarClip = farClip;
        Camera_ComputeProjectionMatrix(camera.Projection, camera);
    }

    // OFFSET 0x020206D0
    public static void Camera_InitWithTarget(Func<VecFx32> target, int distance, in CameraAngle angle, ushort fovY, byte projection, bool trackTarget, Camera camera)
    {
        Camera_InitPerspective(fovY, camera);
        camera.Target = target();
        camera.Distance = distance;
        camera.Angle = angle;
        Camera_UpdatePositionAroundTarget(camera);
        Camera_ComputeProjectionMatrix(projection, camera);
        if (trackTarget)
        {
            camera.TargetPos = target;
            camera.PrevTargetPos = target();
            camera.TrackTargetX = 1;
            camera.TrackTargetY = 1;
            camera.TrackTargetZ = 1;
        }
    }

    // OFFSET 0x02020738
    public static void Camera_InitWithPosition(in VecFx32 position, int distance, in CameraAngle angle, ushort fovY, byte projection, Camera camera)
    {
        Camera_InitPerspective(fovY, camera);
        camera.Position = position;
        camera.Distance = distance;
        camera.Angle = angle;
        Camera_UpdateTargetAroundPosition(camera);
        Camera_ComputeProjectionMatrix(projection, camera);
    }

    // OFFSET 0x02020784
    public static void Camera_InitWithTargetAndPosition(Func<VecFx32> target, in VecFx32 position, ushort fovY, byte projection, bool trackTarget, Camera camera)
    {
        Camera_InitPerspective(fovY, camera);
        camera.Target = target();
        camera.Position = position;
        Mtx.VecSubtract(position, camera.Target, out VecFx32 offset);
        camera.Distance = Mtx.VecMag(offset);
        VecFx32 reference = default;
        reference.Z = Fx32.One;
        VecFx32 projected = offset;
        projected.Y = 0;
        camera.Angle.Y = (ushort)FxUtil.VEC_AngleBetween(reference, projected);
        reference.X = Fx32.One;
        reference.Y = 0;
        reference.Z = 0;
        projected.X = offset.Z;
        projected.Z = offset.Y;
        projected.Y = 0;
        camera.Angle.X = (ushort)FxUtil.VEC_AngleBetween(reference, projected);
        camera.Angle.Z = 0;
        Camera_ComputeProjectionMatrix(projection, camera);
        if (trackTarget)
        {
            camera.TargetPos = target;
            camera.PrevTargetPos = target();
            camera.TrackTargetX = 1;
            camera.TrackTargetY = 1;
            camera.TrackTargetZ = 1;
        }
    }

    // OFFSET 0x02020854
    public static void Camera_ComputeProjectionMatrix(byte projection, Camera camera)
    {
        if (projection == ProjectionPerspective)
        {
            Render3D.SetPerspective(camera.SinFovY, camera.CosFovY, camera.AspectRatio, camera.NearClip, camera.FarClip, Fx32.One);
            camera.Projection = ProjectionPerspective;
            Render3D.ProjectionCache = 0;
            return;
        }
        int top = Fx32.Mul(Fx32.Div(camera.SinFovY, camera.CosFovY), camera.Distance);
        int right = Fx32.Mul(top, camera.AspectRatio);
        Render3D.SetOrtho(top, -top, -right, right, camera.NearClip, camera.FarClip, Fx32.One);
        camera.Projection = ProjectionOrthographic;
        Render3D.ProjectionCache = 0;
    }

    // OFFSET 0x02020910
    public static void Camera_SetFOV(ushort fovY, Camera camera)
    {
        camera.FovY = fovY;
        camera.SinFovY = Render3D.Sin(camera.FovY >> 4);
        camera.CosFovY = Render3D.Cos(camera.FovY >> 4);
        Camera_ComputeProjectionMatrix(camera.Projection, camera);
    }

    // OFFSET 0x0202094C
    public static void Camera_AdjustFOV(ushort amount, Camera camera)
    {
        camera.FovY += amount;
        camera.SinFovY = Render3D.Sin(camera.FovY >> 4);
        camera.CosFovY = Render3D.Cos(camera.FovY >> 4);
        Camera_ComputeProjectionMatrix(camera.Projection, camera);
    }

    // OFFSET 0x02020990
    public static void Camera_Move(in VecFx32 delta, Camera camera)
    {
        Mtx.VecAdd(camera.Position, delta, out camera.Position);
        Mtx.VecAdd(camera.Target, delta, out camera.Target);
    }

    // OFFSET 0x020209B0
    public static void Camera_SetAngleAroundSelf(in CameraAngle angle, Camera camera)
    {
        camera.Angle = angle;
        Camera_UpdateTargetAroundPosition(camera);
    }

    // OFFSET 0x020209D4
    public static void Camera_SetAngleAroundTarget(in CameraAngle angle, Camera camera)
    {
        camera.Angle = angle;
        Camera_UpdatePositionAroundTarget(camera);
    }

    // OFFSET 0x020209F8
    public static void Camera_AdjustAngleAroundSelf(in CameraAngle amount, Camera camera)
    {
        camera.Angle.X += amount.X;
        camera.Angle.Y += amount.Y;
        camera.Angle.Z += amount.Z;
        Camera_UpdateTargetAroundPosition(camera);
    }

    // OFFSET 0x02020A24
    public static void Camera_AdjustAngleAroundTarget(in CameraAngle amount, Camera camera)
    {
        camera.Angle.X += amount.X;
        camera.Angle.Y += amount.Y;
        camera.Angle.Z += amount.Z;
        Camera_UpdatePositionAroundTarget(camera);
    }

    // OFFSET 0x02020A50
    public static void Camera_SetDistance(int distance, Camera camera)
    {
        camera.Distance = distance;
        Camera_UpdatePositionAroundTarget(camera);
    }

    // OFFSET 0x02020A5C
    public static void Camera_SetTargetAndUpdatePosition(in VecFx32 target, Camera camera)
    {
        camera.Target = target;
        Camera_UpdatePositionAroundTarget(camera);
    }

    // OFFSET 0x02020A78
    public static void Camera_AdjustDistance(int amount, Camera camera)
    {
        camera.Distance += amount;
        Camera_UpdatePositionAroundTarget(camera);
    }

    // OFFSET 0x02020A88
    public static ushort Camera_GetFOV(Camera camera)
    {
        return camera.FovY;
    }

    // OFFSET 0x02020A90
    public static int Camera_GetDistance(Camera camera)
    {
        return camera.Distance;
    }

    // OFFSET 0x02020A94
    public static CameraAngle Camera_GetAngle(Camera camera)
    {
        return camera.Angle;
    }

    // OFFSET 0x02020AAC
    public static VecFx32 Camera_GetTarget(Camera camera)
    {
        return camera.Target;
    }

    // OFFSET 0x02020ABC
    public static VecFx32 Camera_GetPosition(Camera camera)
    {
        return camera.Position;
    }

    // OFFSET 0x02020ACC
    public static void Camera_SetTarget(in VecFx32 target, Camera camera)
    {
        camera.Target = target;
    }

    // OFFSET 0x02020ADC
    public static void Camera_SetPosition(in VecFx32 position, Camera camera)
    {
        camera.Position = position;
    }
}
