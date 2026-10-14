namespace PlatinumSharp.Game;

public static partial class BattleParticleUtil
{
    private const int Overlay = 12;
    private const uint PosTableXAddress = 0x02238E0C;
    private const uint PosTableYAddress = 0x02238E54;
    private const int ParticlePixelFactor = 172;
    private const int Center3DX = 0;
    private const int Center3DY = 0xC80;
    private const int Center3DZ = 0;
    private const int SignModeX = 1;
    private const int SignModeY = 2;
    private const int SignModeZ = 3;
    private const int SignModeXY = 4;
    private const int SignModeXZ = 5;
    private const int SignModeYZ = 6;
    private const int SignModeXYZ = 7;
    private const int BehaviorModeNone = 0;
    private const int BehaviorModeConvergence = 1;
    private const int BehaviorModeMagnet = 2;
    private const int TargetModeOrigin = 0;
    private const int TargetModeSign = 1;
    private const int TargetModeCenter = 2;
    private const int TargetModeDefender = 3;
    private const int TargetModeAttacker = 4;
    private const int TargetModeExplicit = 5;

    private static BattleAnimSystem CallbackSystem()
    {
        return (BattleAnimSystem)ParticleSystem.ParticleSystem_GetEmitterCallbackParam()!;
    }

    private static void SetEmitterPos(SplEmitter emitter, in VecFx32 pos)
    {
        emitter.Position.X = pos.X;
        emitter.Position.Y = pos.Y;
        emitter.Position.Z = pos.Z;
    }

    // OFFSET 0x0222389C
    private static sbyte BattleParticleUtil_GetSignFromBattler(BattleAnimSystem system, int startBattler, int endBattler)
    {
        sbyte sign = 1;
        int startType = BattleAnimUtil.BattleAnimUtil_GetBattlerType(system, startBattler);
        BattleAnimUtil.BattleAnimUtil_GetBattlerType(system, endBattler);
        switch (startType)
        {
            case 1:
            case 3:
            case 5:
                sign = -1;
                break;
        }
        return sign;
    }

    // OFFSET 0x022238DC
    private static void BattleAnimEmitterCb_Nop2(SplEmitter emitter)
    {
    }

    // OFFSET 0x022238E0
    private static void BattleAnimEmitterCb_SetPosToEnemy1(SplEmitter emitter)
    {
        BattleAnimSystem system = CallbackSystem();
        BattleAnimSystems.BattleAnimSystem_GetAttacker(system);
        BattleAnimSystems.BattleAnimSystem_GetDefender(system);
        int attacker = 1;
        ParticleSystem ps = BattleAnimSystems.BattleAnimSystem_GetCurrentParticleSystem(system)!;
        VecFx32 destination;
        if (BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, attacker) == 3)
        {
            BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(system, attacker, out destination);
        }
        else
        {
            int attackerType = BattleAnimUtil.BattleAnimUtil_GetBattlerType(system, attacker);
            int opposingType = BattleAnimUtil.BattleAnimUtil_GetOpposingBattlerType(attackerType);
            int projection = ParticleSystem.ParticleSystem_GetCameraProjection(ps);
            int isContest = BattleAnimSystems.BattleAnimSystem_IsContest(system);
            BallEffects.BattleParticle_GetDefaultPosition(opposingType, out destination, isContest, projection);
            BallEffects.BattleParticle_GetDefaultPosition(attackerType, out _, isContest, projection);
        }
        SetEmitterPos(emitter, destination);
        BattleAnimUtil.BattleAnimUtil_GetUpVectorForBattler(attacker, out _);
        Camera camera = ParticleSystem.ParticleSystem_GetCamera(ps)!;
        CameraAngle angle = Camera.Camera_GetAngle(camera);
        Camera.Camera_SetAngleAroundTarget(in angle, camera);
    }

    // OFFSET 0x02223998
    private static void BattleAnimEmitterCb_SetPosToPlayer1(SplEmitter emitter)
    {
        BattleAnimSystem system = CallbackSystem();
        int defender = BattleAnimSystems.BattleAnimSystem_GetDefender(system);
        ParticleSystem ps = BattleAnimSystems.BattleAnimSystem_GetCurrentParticleSystem(system)!;
        BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(system, 0, out VecFx32 player1Pos);
        SetEmitterPos(emitter, player1Pos);
        BattleAnimUtil.BattleAnimUtil_GetUpVectorForBattler(defender, out VecFx32 up);
        ParticleSystem.ParticleSystem_SetCameraUp(ps, in up);
    }

    // OFFSET 0x022239F4
    private static void BattleAnimEmitterCb_SetPosToDefender(SplEmitter emitter)
    {
        BattleAnimSystem system = CallbackSystem();
        BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(system, BattleAnimSystems.BattleAnimSystem_GetDefender(system), out VecFx32 pos);
        SetEmitterPos(emitter, pos);
    }

    // OFFSET 0x02223A38
    private static void BattleAnimEmitterCb_SetPosToAttacker(SplEmitter emitter)
    {
        BattleAnimSystem system = CallbackSystem();
        BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system), out VecFx32 pos);
        SetEmitterPos(emitter, pos);
    }

    // OFFSET 0x02223A7C
    private static void BattleAnimEmitterCb_SetPosToAttacker2(SplEmitter emitter)
    {
        BattleAnimSystem system = CallbackSystem();
        int attacker = BattleAnimSystems.BattleAnimSystem_GetAttacker(system);
        BattleAnimSystems.BattleAnimSystem_GetDefender(system);
        BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(system, attacker, out VecFx32 pos);
        SetEmitterPos(emitter, pos);
    }

    // OFFSET 0x02223AC8
    private static void BattleAnimEmitterCb_SetPosToAttackerSide(SplEmitter emitter)
    {
        BattleAnimSystem system = CallbackSystem();
        ParticleSystem ps = BattleAnimSystems.BattleAnimSystem_GetCurrentParticleSystem(system)!;
        int projection = ParticleSystem.ParticleSystem_GetCameraProjection(ps);
        int attacker = BattleAnimSystems.BattleAnimSystem_GetAttacker(system);
        int battler = BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, attacker) == 3 ? 0 : 1;
        BallEffects.BattleParticle_GetDefaultPosition(battler, out VecFx32 pos, BattleAnimSystems.BattleAnimSystem_IsContest(system), projection);
        SetEmitterPos(emitter, pos);
    }

    // OFFSET 0x02223B30
    private static void BattleAnimEmitterCb_SetPosToDefenderSide(SplEmitter emitter)
    {
        BattleAnimSystem system = CallbackSystem();
        ParticleSystem ps = BattleAnimSystems.BattleAnimSystem_GetCurrentParticleSystem(system)!;
        int projection = ParticleSystem.ParticleSystem_GetCameraProjection(ps);
        int defender = BattleAnimSystems.BattleAnimSystem_GetDefender(system);
        int battler = BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, defender) == 3 ? 0 : 1;
        BallEffects.BattleParticle_GetDefaultPosition(battler, out VecFx32 pos, BattleAnimSystems.BattleAnimSystem_IsContest(system), projection);
        SetEmitterPos(emitter, pos);
    }

    // OFFSET 0x02223B98
    private static void BattleAnimEmitterCb_SetPosBasedOnBattlers(SplEmitter emitter)
    {
        BattleAnimSystem system = CallbackSystem();
        int attacker = BattleAnimSystems.BattleAnimSystem_GetAttacker(system);
        int defender = BattleAnimSystems.BattleAnimSystem_GetDefender(system);
        int attackerType = BattleAnimUtil.BattleAnimUtil_GetBattlerType(system, attacker);
        int defenderType = BattleAnimUtil.BattleAnimUtil_GetBattlerType(system, defender);
        int index = attackerType * 6 + defenderType;
        short tableX = (short)RomData.OverlayU16(Overlay, (uint)(PosTableXAddress + index * 2), 1)[0];
        short tableY = (short)RomData.OverlayU16(Overlay, (uint)(PosTableYAddress + index * 2), 1)[0];
        VecFx32 pos;
        pos.X = tableX * ParticlePixelFactor;
        pos.Y = tableY * ParticlePixelFactor;
        pos.Z = 0;
        SetEmitterPos(emitter, pos);
    }

    // OFFSET 0x02223C40
    private static void BattleParticleUtil_ApplySignToVector(int mode, sbyte sign, ref VecFx32 vec)
    {
        switch (mode)
        {
            case SignModeX:
                vec.X *= sign;
                break;
            case SignModeY:
                vec.Y *= sign;
                break;
            case SignModeZ:
                vec.Z *= sign;
                break;
            case SignModeXY:
                vec.X *= sign;
                vec.Y *= sign;
                break;
            case SignModeXZ:
                vec.X *= sign;
                vec.Z *= sign;
                break;
            case SignModeYZ:
                vec.Y *= sign;
                vec.Z *= sign;
                break;
            case SignModeXYZ:
                vec.X *= sign;
                vec.Y *= sign;
                vec.Z *= sign;
                break;
        }
    }

    private static VecFx32 ResolveTarget(BattleAnimSystem system, SplEmitter emitter, int attacker, int defender, int mode, sbyte sign, bool magnet)
    {
        VecFx32 target = default;
        switch (mode)
        {
            case TargetModeSign:
                if (magnet)
                {
                    ParticleSystem.ParticleSystem_GetEmitterMagnetTarget(emitter, out target);
                }
                else
                {
                    ParticleSystem.ParticleSystem_GetEmitterConvergenceTarget(emitter, out target);
                }
                target.X *= sign;
                target.Y *= sign;
                target.Z *= sign;
                break;
            case TargetModeCenter:
                target.X = Center3DX * sign;
                target.Y = Center3DY * sign;
                target.Z = Center3DZ * sign;
                break;
            case TargetModeDefender:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(system, defender, out target);
                break;
            case TargetModeAttacker:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(system, attacker, out target);
                break;
            case TargetModeExplicit:
            {
                int[] parameters = new int[4];
                BattleAnimSystems.BattleAnimSystem_GetExtraParams(system, parameters, 4);
                target.X = parameters[0];
                target.Y = parameters[1];
                target.Z = parameters[2];
                BattleParticleUtil_ApplySignToVector(parameters[3], sign, ref target);
                break;
            }
        }
        return target;
    }

    // OFFSET 0x02223CD4
    private static void BattleParticleUtil_SetConvergenceTarget(BattleAnimSystem system, SplEmitter emitter, int attacker, int defender, int mode, sbyte sign, in VecFx32 emitterPos)
    {
        VecFx32 target = ResolveTarget(system, emitter, attacker, defender, mode, sign, false);
        target.X -= emitterPos.X;
        target.Y -= emitterPos.Y;
        target.Z -= emitterPos.Z;
        ParticleSystem.ParticleSystem_SetEmitterConvergenceTarget(emitter, in target);
    }

    // OFFSET 0x02223DA4
    private static void BattleParticleUtil_SetMagnetTarget(BattleAnimSystem system, SplEmitter emitter, int attacker, int defender, int mode, sbyte sign, in VecFx32 emitterPos)
    {
        VecFx32 target = ResolveTarget(system, emitter, attacker, defender, mode, sign, true);
        target.X -= emitterPos.X;
        target.Y -= emitterPos.Y;
        target.Z -= emitterPos.Z;
        ParticleSystem.ParticleSystem_SetEmitterMagnetTarget(emitter, in target);
    }

    private static VecFx32 GetExtraParamVec3(BattleAnimSystem system)
    {
        int[] values = new int[3];
        BattleAnimSystems.BattleAnimSystem_GetExtraParams(system, values, 3);
        VecFx32 vec;
        vec.X = values[0];
        vec.Y = values[1];
        vec.Z = values[2];
        return vec;
    }

    private static VecFx16 ToFx16(in VecFx32 v, int sign)
    {
        VecFx16 result;
        result.X = (short)(v.X * sign);
        result.Y = (short)(v.Y * sign);
        result.Z = (short)(v.Z * sign);
        return result;
    }

    // OFFSET 0x02223E74
    private static void BattleParticleUtil_SetEmitterAxisPosEx(BattleAnimSystem system, SplEmitter emitter, int attacker, int defender, int behaviorMode, int targetMode)
    {
        BattleAnimSystems.BattleAnimSystem_GetCurrentParticleSystem(system);
        int attackerType = BattleAnimUtil.BattleAnimUtil_GetBattlerType(system, attacker);
        int defenderType = BattleAnimUtil.BattleAnimUtil_GetBattlerType(system, defender);
        VecFx32 dirPlToEm = GetExtraParamVec3(system);
        VecFx32 dirP1ToE1 = GetExtraParamVec3(system);
        VecFx32 dirP1ToE2 = GetExtraParamVec3(system);
        VecFx32 dirP1ToP2 = GetExtraParamVec3(system);
        VecFx32 dirP2ToE1 = GetExtraParamVec3(system);
        VecFx32 dirP2ToE2 = GetExtraParamVec3(system);
        VecFx32 dirP2ToP1 = GetExtraParamVec3(system);
        VecFx16 dir;
        switch (attackerType)
        {
            case 1:
                dir = ToFx16(dirPlToEm, -1);
                break;
            case 2:
                dir = defenderType == 3 ? ToFx16(dirP1ToE1, 1) : defenderType == 5 ? ToFx16(dirP1ToE2, 1) : ToFx16(dirP1ToP2, 1);
                break;
            case 3:
                dir = defenderType == 2 ? ToFx16(dirP1ToE1, -1) : defenderType == 5 ? ToFx16(dirP1ToP2, -1) : ToFx16(dirP2ToE1, -1);
                break;
            case 4:
                dir = defenderType == 3 ? ToFx16(dirP2ToE1, 1) : defenderType == 5 ? ToFx16(dirP2ToE2, 1) : ToFx16(dirP2ToP1, 1);
                break;
            case 5:
                dir = defenderType == 3 ? ToFx16(dirP2ToP1, -1) : defenderType == 2 ? ToFx16(dirP1ToE2, -1) : ToFx16(dirP2ToE2, -1);
                break;
            default:
                dir = ToFx16(dirPlToEm, 1);
                break;
        }
        sbyte sign = BattleParticleUtil_GetSignFromBattler(system, attacker, defender);
        VecFx32 explicitPos = GetExtraParamVec3(system);
        BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(system, attacker, out VecFx32 attackerPos);
        VecFx32 pos;
        if (explicitPos.X == 0 && explicitPos.Y == 0 && explicitPos.Z == 0)
        {
            pos = attackerPos;
            SetEmitterPos(emitter, attackerPos);
        }
        else
        {
            pos = explicitPos;
            if (explicitPos.X == Center3DX && explicitPos.Y == Center3DY)
            {
                sign = 1;
            }
            pos.X *= sign;
            pos.Y *= sign;
            pos.Z = attackerPos.Z * sign;
            SetEmitterPos(emitter, pos);
        }
        switch (behaviorMode)
        {
            case BehaviorModeConvergence:
                BattleParticleUtil_SetConvergenceTarget(system, emitter, attacker, defender, targetMode, sign, in pos);
                break;
            case BehaviorModeMagnet:
                BattleParticleUtil_SetMagnetTarget(system, emitter, attacker, defender, targetMode, sign, in pos);
                break;
        }
        emitter.Axis = dir;
    }

    // OFFSET 0x02224138
    private static void BattleParticleUtil_SetEmitterAxisPos(SplEmitter emitter, int behaviorMode, int targetMode)
    {
        BattleAnimSystem system = CallbackSystem();
        int attacker = BattleAnimSystems.BattleAnimSystem_GetAttacker(system);
        int defender = BattleAnimSystems.BattleAnimSystem_GetDefender(system);
        BattleParticleUtil_SetEmitterAxisPosEx(system, emitter, attacker, defender, behaviorMode, targetMode);
    }

    // OFFSET 0x02224168
    private static void BattleAnimEmitterCb_SetAxisAndPos(SplEmitter emitter)
    {
        BattleParticleUtil_SetEmitterAxisPos(emitter, BehaviorModeNone, TargetModeOrigin);
    }

    // OFFSET 0x02224174
    private static void BattleAnimEmitterCb_SetAxisAndPos_ConvergeDefault(SplEmitter emitter)
    {
        BattleParticleUtil_SetEmitterAxisPos(emitter, BehaviorModeConvergence, TargetModeSign);
    }

    // OFFSET 0x02224180
    private static void BattleAnimEmitterCb_SetAxisAndPos_ConvergeCenter(SplEmitter emitter)
    {
        BattleParticleUtil_SetEmitterAxisPos(emitter, BehaviorModeConvergence, TargetModeCenter);
    }

    // OFFSET 0x0222418C
    private static void BattleAnimEmitterCb_SetAxisAndPos_ConvergeDefender(SplEmitter emitter)
    {
        BattleParticleUtil_SetEmitterAxisPos(emitter, BehaviorModeConvergence, TargetModeDefender);
    }

    // OFFSET 0x02224198
    private static void BattleAnimEmitterCb_SetAxisAndPos_ConvergeAttacker(SplEmitter emitter)
    {
        BattleParticleUtil_SetEmitterAxisPos(emitter, BehaviorModeConvergence, TargetModeAttacker);
    }

    // OFFSET 0x022241A4
    private static void BattleAnimEmitterCb_SetAxisAndPos_ConvergeExplicit(SplEmitter emitter)
    {
        BattleParticleUtil_SetEmitterAxisPos(emitter, BehaviorModeConvergence, TargetModeExplicit);
    }

    // OFFSET 0x022241B0
    private static void BattleAnimEmitterCb_SetAxisAndPos_MagnetDefault(SplEmitter emitter)
    {
        BattleParticleUtil_SetEmitterAxisPos(emitter, BehaviorModeMagnet, TargetModeSign);
    }

    // OFFSET 0x022241BC
    private static void BattleAnimEmitterCb_SetAxisAndPos_MagnetCenter(SplEmitter emitter)
    {
        BattleParticleUtil_SetEmitterAxisPos(emitter, BehaviorModeMagnet, TargetModeCenter);
    }

    // OFFSET 0x022241C8
    private static void BattleAnimEmitterCb_SetAxisAndPos_MagnetDefender(SplEmitter emitter)
    {
        BattleParticleUtil_SetEmitterAxisPos(emitter, BehaviorModeMagnet, TargetModeDefender);
    }

    // OFFSET 0x022241D4
    private static void BattleAnimEmitterCb_SetAxisAndPos_MagnetAttacker(SplEmitter emitter)
    {
        BattleParticleUtil_SetEmitterAxisPos(emitter, BehaviorModeMagnet, TargetModeAttacker);
    }

    // OFFSET 0x022241E0
    private static void BattleAnimEmitterCb_SetAxisAndPos_MagnetExplicit(SplEmitter emitter)
    {
        BattleParticleUtil_SetEmitterAxisPos(emitter, BehaviorModeMagnet, TargetModeExplicit);
    }

    // OFFSET 0x022241EC
    private static void BattleAnimEmitterCb_SetAxisAndPosReverse(SplEmitter emitter)
    {
        BattleAnimSystem system = CallbackSystem();
        int attacker = BattleAnimSystems.BattleAnimSystem_GetAttacker(system);
        int defender = BattleAnimSystems.BattleAnimSystem_GetDefender(system);
        BattleParticleUtil_SetEmitterAxisPosEx(system, emitter, defender, attacker, BehaviorModeNone, TargetModeOrigin);
    }

    // OFFSET 0x022237EC
    private static void BattleAnimEmitterCb_Nop(SplEmitter emitter)
    {
    }
}
