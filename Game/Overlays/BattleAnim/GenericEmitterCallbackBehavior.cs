namespace PlatinumSharp.Game;

public static partial class BattleParticleUtil
{
    private const uint BehaviorFlagTableAddress = 0x02239DA4;
    private const uint CameraAnglesAddress = 0x02239DE0;
    private const int BehaviorFlagAll = 0xFEFE;
    private const int BehaviorParamCount = 5;
    private const int BehaviorModeDefault = 0;
    private const int BehaviorModeExplicit = 1;
    private const int BehaviorModeStart = 2;
    private const int BehaviorModeEnd = 3;
    private const int BehaviorModeScaled = 4;

    private delegate bool BehaviorFunction(SplEmitter emitter, GenericEmitterCallbackData data);

    private delegate bool CameraFunction(SplEmitter emitter, GenericEmitterCallbackData data);

    private static readonly BehaviorFunction[] BehaviorFunctions =
    {
        GenericEmitterCallback_ApplyBehaviorNone,
        GenericEmitterCallback_ApplyGravityMag,
        GenericEmitterCallback_ApplyRandomMag,
        GenericEmitterCallback_ApplyRandomInterval,
        GenericEmitterCallback_ApplyMagnetPos,
        GenericEmitterCallback_ApplyMagnetForce,
        GenericEmitterCallback_ApplySpinAngle,
        GenericEmitterCallback_ApplySpinAxis,
        GenericEmitterCallback_UnusedApply8,
        GenericEmitterCallback_UnusedApply9,
        GenericEmitterCallback_UnusedApply10,
        GenericEmitterCallback_UnusedApply11,
        GenericEmitterCallback_ApplyConvergenceTarget,
        GenericEmitterCallback_ApplyConvergenceForce,
    };

    private static readonly CameraFunction[] CameraFunctions =
    {
        GenericEmitterCallback_CameraFixedAngle0,
        GenericEmitterCallback_CameraFixedAngle1,
        GenericEmitterCallback_CameraExplicit,
        GenericEmitterCallback_CameraFixedAngle3,
        GenericEmitterCallback_CameraFixedAngle4,
        GenericEmitterCallback_CameraFixedAngle5,
        GenericEmitterCallback_CameraFixedAngle6,
        GenericEmitterCallback_CameraTargetStart,
        GenericEmitterCallback_CameraTargetEnd,
    };

    // OFFSET 0x02225104
    private static void GenericEmitterCallback_GetBehaviorParams(GenericEmitterCallbackData data, int[] parameters)
    {
        BattleAnimSystems.BattleAnimSystem_GetExtraParams(data.BattleAnimSys!, parameters, BehaviorParamCount);
    }

    private static int[] ReadBehaviorParams(GenericEmitterCallbackData data)
    {
        int[] parameters = new int[BehaviorParamCount];
        GenericEmitterCallback_GetBehaviorParams(data, parameters);
        data.Dir = GenericEmitterCallback_GetDirectionForBattlers(data);
        if (parameters[1] == FlipDisable)
        {
            data.Dir = 1;
        }
        return parameters;
    }

    // OFFSET 0x02225110
    private static bool GenericEmitterCallback_ApplyBehaviorNone(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        return true;
    }

    // OFFSET 0x02225114
    private static bool GenericEmitterCallback_ApplyGravityMag(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        VecFx16 mag = default;
        int[] parameters = ReadBehaviorParams(data);
        switch (parameters[0])
        {
            case BehaviorModeDefault:
                ParticleSystem.ParticleSystem_SetEmitterGravityMagnitude(emitter, in mag);
                mag.X = (short)(mag.X * data.Dir);
                mag.Y = (short)(mag.Y * data.Dir);
                mag.Z = (short)(mag.Z * data.Dir);
                break;
            case BehaviorModeExplicit:
                mag.X = (short)parameters[2];
                mag.Y = (short)parameters[3];
                mag.Z = (short)parameters[4];
                mag.X = (short)(mag.X * data.Dir);
                mag.Y = (short)(mag.Y * data.Dir);
                mag.Z = (short)(mag.Z * data.Dir);
                ParticleSystem.ParticleSystem_SetEmitterGravityMagnitude(emitter, in mag);
                return true;
        }
        mag.X = (short)(mag.X - data.Position.X);
        mag.Y = (short)(mag.Y - data.Position.Y);
        mag.Z = (short)(mag.Z - data.Position.Z);
        ParticleSystem.ParticleSystem_SetEmitterGravityMagnitude(emitter, in mag);
        return true;
    }

    // OFFSET 0x022251F8
    private static bool GenericEmitterCallback_ApplyRandomMag(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        return true;
    }

    // OFFSET 0x022251FC
    private static bool GenericEmitterCallback_ApplyRandomInterval(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        return true;
    }

    // OFFSET 0x02225200
    private static bool GenericEmitterCallback_ApplyMagnetPos(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        VecFx32 target = default;
        int[] parameters = ReadBehaviorParams(data);
        switch (parameters[0])
        {
            case BehaviorModeDefault:
                ParticleSystem.ParticleSystem_GetEmitterMagnetTarget(emitter, out target);
                target.X *= data.Dir;
                target.Y *= data.Dir;
                target.Z *= data.Dir;
                break;
            case BehaviorModeExplicit:
                target.X = parameters[2] * data.Dir;
                target.Y = parameters[3] * data.Dir;
                target.Z = parameters[4] * data.Dir;
                break;
            case BehaviorModeStart:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(data.BattleAnimSys!, data.StartBattler, out target);
                break;
            case BehaviorModeEnd:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(data.BattleAnimSys!, data.EndBattler, out target);
                break;
        }
        target.X -= data.Position.X;
        target.Y -= data.Position.Y;
        ParticleSystem.ParticleSystem_SetEmitterMagnetTarget(emitter, in target);
        return true;
    }

    // OFFSET 0x022252C8
    private static bool GenericEmitterCallback_ApplyMagnetForce(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        short force = 0;
        int[] parameters = ReadBehaviorParams(data);
        switch (parameters[0])
        {
            case BehaviorModeDefault:
                ParticleSystem.ParticleSystem_GetEmitterMagnetForce(emitter, out force);
                force = (short)(force * data.Dir);
                break;
            case BehaviorModeExplicit:
                force = (short)parameters[2];
                force = (short)(force * data.Dir);
                break;
        }
        ParticleSystem.ParticleSystem_SetEmitterMagnetForce(emitter, force);
        return true;
    }

    // OFFSET 0x0222534C
    private static bool GenericEmitterCallback_ApplySpinAngle(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        ushort angle = 0;
        int[] parameters = ReadBehaviorParams(data);
        switch (parameters[0])
        {
            case BehaviorModeDefault:
                ParticleSystem.ParticleSystem_GetEmitterSpinAngle(emitter, out angle);
                angle = (ushort)(angle * data.Dir);
                break;
            case BehaviorModeExplicit:
                angle = (ushort)parameters[2];
                angle = (ushort)(angle * data.Dir);
                break;
        }
        ParticleSystem.ParticleSystem_SetEmitterSpinAngle(emitter, angle);
        return true;
    }

    // OFFSET 0x022253CC
    private static bool GenericEmitterCallback_ApplySpinAxis(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        ushort axis = 0;
        int[] parameters = ReadBehaviorParams(data);
        switch (parameters[0])
        {
            case BehaviorModeDefault:
                ParticleSystem.ParticleSystem_GetEmitterSpinAxis(emitter, out axis);
                break;
            case BehaviorModeExplicit:
                axis = (ushort)parameters[2];
                break;
        }
        ParticleSystem.ParticleSystem_SetEmitterSpinAxis(emitter, axis);
        return true;
    }

    // OFFSET 0x02225434
    private static bool GenericEmitterCallback_UnusedApply8(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        return true;
    }

    // OFFSET 0x02225438
    private static bool GenericEmitterCallback_UnusedApply9(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        return true;
    }

    // OFFSET 0x0222543C
    private static bool GenericEmitterCallback_UnusedApply10(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        return true;
    }

    // OFFSET 0x02225440
    private static bool GenericEmitterCallback_UnusedApply11(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        return true;
    }

    // OFFSET 0x02225444
    private static bool GenericEmitterCallback_ApplyConvergenceTarget(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        VecFx32 target = default;
        int[] parameters = ReadBehaviorParams(data);
        switch (parameters[0])
        {
            case BehaviorModeDefault:
                ParticleSystem.ParticleSystem_GetEmitterConvergenceTarget(emitter, out target);
                target.X *= data.Dir;
                target.Y *= data.Dir;
                target.Z *= data.Dir;
                break;
            case BehaviorModeExplicit:
                target.X = parameters[2] * data.Dir;
                target.Y = parameters[3] * data.Dir;
                target.Z = parameters[4] * data.Dir;
                break;
            case BehaviorModeScaled:
            {
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(data.BattleAnimSys!, data.EndBattler, out target);
                int numerator = parameters[2];
                int denominator = parameters[3];
                target.X = target.X * numerator / denominator;
                target.Y = target.Y * numerator / denominator;
                target.Z = target.Z * numerator / denominator;
                break;
            }
            case BehaviorModeStart:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(data.BattleAnimSys!, data.StartBattler, out target);
                break;
            case BehaviorModeEnd:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(data.BattleAnimSys!, data.EndBattler, out target);
                break;
        }
        target.X -= data.Position.X;
        target.Y -= data.Position.Y;
        target.Z -= data.Position.Z;
        ParticleSystem.ParticleSystem_SetEmitterConvergenceTarget(emitter, in target);
        return true;
    }

    // OFFSET 0x02225550
    private static bool GenericEmitterCallback_ApplyConvergenceForce(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        short force = 0;
        int[] parameters = ReadBehaviorParams(data);
        switch (parameters[0])
        {
            case BehaviorModeDefault:
                ParticleSystem.ParticleSystem_GetEmitterConvergenceForce(emitter, out force);
                force = (short)(force * data.Dir);
                break;
            case BehaviorModeExplicit:
                force = (short)parameters[2];
                force = (short)(force * data.Dir);
                break;
        }
        ParticleSystem.ParticleSystem_SetEmitterConvergenceForce(emitter, force);
        return true;
    }

    // OFFSET 0x022255D4
    public static void GenericEmitterCallback_CallBehaviorFunction(int behaviorValue, SplEmitter emitter, GenericEmitterCallbackData data)
    {
        BehaviorFunctions[behaviorValue](emitter, data);
    }

    // OFFSET 0x022255EC
    public static int GenericEmitterCallback_GetBehaviorFlag(int behaviorValue)
    {
        int[] flagTable = ReadOverlayInts(BehaviorFlagTableAddress, 15);
        return flagTable[behaviorValue];
    }

    // OFFSET 0x02225614
    public static int GenericEmitterCallback_GetBehaviorValueCount()
    {
        int i = 0;
        int count = 0;
        do
        {
            int flag = GenericEmitterCallback_GetBehaviorFlag(i);
            i++;
            if (flag == BehaviorFlagAll)
            {
                break;
            }
            count++;
        }
        while (count < 0xFF);
        if (count >= 0xFF)
        {
            return 0;
        }
        return i;
    }

    // OFFSET 0x02225640
    private static void GenericEmitterCallback_GetCameraParams(GenericEmitterCallbackData data, int[] parameters)
    {
        BattleAnimSystems.BattleAnimSystem_GetExtraParams(data.BattleAnimSys!, parameters, 4);
    }

    private static CameraAngle ReadCameraAngle(int offset)
    {
        ushort[] raw = RomData.OverlayU16(Overlay, (uint)(CameraAnglesAddress + offset), 4);
        CameraAngle angle;
        angle.X = raw[0];
        angle.Y = raw[1];
        angle.Z = raw[2];
        angle.Padding = raw[3];
        return angle;
    }

    private static void ApplyFixedCameraAngle(GenericEmitterCallbackData data, in CameraAngle angle)
    {
        Camera camera = ParticleSystem.ParticleSystem_GetCamera(data.ParticleSys!)!;
        ParticleSystem.ParticleSystem_SetCameraProjection(data.ParticleSys!, 1);
        Camera.Camera_SetAngleAroundTarget(in angle, camera);
    }

    // OFFSET 0x0222564C
    private static bool GenericEmitterCallback_CameraFixedAngle0(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        CameraAngle angle = default;
        Camera camera = ParticleSystem.ParticleSystem_GetCamera(data.ParticleSys!)!;
        Camera.Camera_SetAngleAroundTarget(in angle, camera);
        return true;
    }

    // OFFSET 0x02225670
    private static bool GenericEmitterCallback_CameraFixedAngle1(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        CameraAngle angle = ReadCameraAngle(0x20);
        ApplyFixedCameraAngle(data, in angle);
        return true;
    }

    // OFFSET 0x022256AC
    private static bool GenericEmitterCallback_CameraExplicit(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        CameraAngle angle = default;
        int[] parameters = new int[4];
        GenericEmitterCallback_GetCameraParams(data, parameters);
        angle.X = (ushort)parameters[1];
        angle.Y = (ushort)parameters[2];
        angle.Z = (ushort)parameters[3];
        Camera camera = ParticleSystem.ParticleSystem_GetCamera(data.ParticleSys!)!;
        Camera.Camera_SetAngleAroundTarget(in angle, camera);
        return true;
    }

    // OFFSET 0x022256E8
    private static bool GenericEmitterCallback_CameraFixedAngle3(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        CameraAngle angle = ReadCameraAngle(0x18);
        ApplyFixedCameraAngle(data, in angle);
        return true;
    }

    // OFFSET 0x02225724
    private static bool GenericEmitterCallback_CameraFixedAngle4(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        CameraAngle battleAngle = ReadCameraAngle(0x28);
        CameraAngle contestAngle = ReadCameraAngle(0x10);
        Camera camera = ParticleSystem.ParticleSystem_GetCamera(data.ParticleSys!)!;
        ParticleSystem.ParticleSystem_SetCameraProjection(data.ParticleSys!, 1);
        if (BattleAnimSystems.BattleAnimSystem_IsContest(data.BattleAnimSys!) == 1)
        {
            Camera.Camera_SetAngleAroundTarget(in contestAngle, camera);
        }
        else
        {
            Camera.Camera_SetAngleAroundTarget(in battleAngle, camera);
        }
        return true;
    }

    // OFFSET 0x02225784
    private static bool GenericEmitterCallback_CameraFixedAngle5(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        CameraAngle angle = ReadCameraAngle(0x0);
        ApplyFixedCameraAngle(data, in angle);
        return true;
    }

    // OFFSET 0x022257C0
    private static bool GenericEmitterCallback_CameraFixedAngle6(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        CameraAngle angle = ReadCameraAngle(0x8);
        ApplyFixedCameraAngle(data, in angle);
        return true;
    }

    // OFFSET 0x022257FC
    private static bool GenericEmitterCallback_CameraTargetStart(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        Camera camera = ParticleSystem.ParticleSystem_GetCamera(data.ParticleSys!)!;
        BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_CameraLookAt(data.BattleAnimSys!, data.StartBattler, out VecFx32 target);
        Camera.Camera_SetTarget(in target, camera);
        return true;
    }

    // OFFSET 0x02225824
    private static bool GenericEmitterCallback_CameraTargetEnd(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        Camera camera = ParticleSystem.ParticleSystem_GetCamera(data.ParticleSys!)!;
        BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_CameraLookAt(data.BattleAnimSys!, data.EndBattler, out VecFx32 target);
        Camera.Camera_SetTarget(in target, camera);
        return true;
    }

    // OFFSET 0x0222584C
    public static void GenericEmitterCallback_ApplyCameraFunc(int mode, SplEmitter emitter, GenericEmitterCallbackData data)
    {
        CameraFunctions[mode](emitter, data);
    }
}
