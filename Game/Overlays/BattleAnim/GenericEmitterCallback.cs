namespace PlatinumSharp.Game;

public sealed class GenericEmitterCallbackData
{
    public const int ParamCount = 6;
    public const int ParamPriority = 0;
    public const int ParamTargetMode = 1;
    public const int ParamPosition = 2;
    public const int ParamAxis = 3;
    public const int ParamBehavior = 4;
    public const int ParamCamera = 5;

    public BattleAnimSystem? BattleAnimSys;
    public ParticleSystem? ParticleSys;
    public sbyte Dir;
    public readonly int[] Params = new int[ParamCount];
    public int StartBattler;
    public int EndBattler;
    public VecFx32 Position;
}

public static partial class BattleParticleUtil
{
    private const uint BubblePositionsAddress = 0x02238EAC;
    private const uint BatonPassPositionsAddress = 0x02238F44;
    private const uint DragonBreathPositionsAddress = 0x02238F0C;
    private const uint AxisConfigs1Address = 0x022396AC;
    private const uint AxisConfigs2Address = 0x02238FEC;
    private const uint AxisConfigs3Address = 0x0223919C;
    private const uint AxisConfigs4Address = 0x022394FC;
    private const uint AxisConfigs5Address = 0x0223985C;
    private const uint AxisConfigs6Address = 0x0223934C;
    private const uint AxisConfigs7Address = 0x02239A0C;
    private const uint AxisConfigs8Address = 0x02239BBC;
    private const uint SpecialAxisConfigsAddress = 0x02238F7C;
    private const uint SpecialAxisContestConfigAddress = 0x02238E9C;
    private const uint AxisPerTypeAddress = 0x02238EDC;
    private const int FlipDisable = 1;
    private const int EmitterPosNone = 0;
    private const int EmitterPosNormalOffsetExStart = 100;
    private const int EmitterPosNormalOffsetExEnd = 101;
    private const int TargetModeGenericDefender = 2;

    private static int[] ReadOverlayInts(uint address, int count)
    {
        uint[] raw = RomData.OverlayU32(Overlay, address, count);
        int[] values = new int[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = (int)raw[i];
        }
        return values;
    }

    private static VecFx32 ReadAxisConfig(uint tableAddress, int startType, int endType)
    {
        int[] v = ReadOverlayInts((uint)(tableAddress + startType * 0x48 + endType * 0xC), 3);
        VecFx32 result;
        result.X = v[0];
        result.Y = v[1];
        result.Z = v[2];
        return result;
    }

    private static VecFx16 Fx16(int x, int y, int z)
    {
        VecFx16 v;
        v.X = (short)x;
        v.Y = (short)y;
        v.Z = (short)z;
        return v;
    }

    private static VecFx32 ReadTablePosition(uint tableAddress, int count, int index)
    {
        int[] table = ReadOverlayInts(tableAddress, count * 2);
        VecFx32 pos;
        pos.X = table[index * 2];
        pos.Y = table[index * 2 + 1];
        pos.Z = 0;
        return pos;
    }

    // OFFSET 0x0222421C
    private static void GenericEmitterCallback_ApplyParams(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        if (data.Params[GenericEmitterCallbackData.ParamPosition] != EmitterPosNone)
        {
            GenericEmitterCallback_ApplyPosition(emitter, data);
        }
        if (data.Params[GenericEmitterCallbackData.ParamAxis] != 0)
        {
            GenericEmitterCallback_ApplyAxis(emitter, data);
        }
        if (data.Params[GenericEmitterCallbackData.ParamBehavior] != 0)
        {
            GenericEmitterCallback_ApplyBehavior(emitter, data);
        }
        GenericEmitterCallback_ApplyCamera(emitter, data);
        if (data.Params[GenericEmitterCallbackData.ParamPriority] != 0)
        {
            GenericEmitterCallback_ApplyPriority(emitter, data);
        }
    }

    // OFFSET 0x02224260
    private static void GenericEmitterCallback_GetDefenderSidePosition(SplEmitter emitter, GenericEmitterCallbackData data, out VecFx32 pos)
    {
        BattleAnimSystem system = data.BattleAnimSys!;
        int projection = ParticleSystem.ParticleSystem_GetCameraProjection(BattleAnimSystems.BattleAnimSystem_GetCurrentParticleSystem(system)!);
        int battler = BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, BattleAnimSystems.BattleAnimSystem_GetDefender(system)) == 3 ? 0 : 1;
        BallEffects.BattleParticle_GetDefaultPosition(battler, out pos, BattleAnimSystems.BattleAnimSystem_IsContest(system), projection);
    }

    // OFFSET 0x022242A0
    private static void GenericEmitterCallback_GetAttackerSidePosition(SplEmitter emitter, GenericEmitterCallbackData data, out VecFx32 pos)
    {
        BattleAnimSystem system = data.BattleAnimSys!;
        int projection = ParticleSystem.ParticleSystem_GetCameraProjection(BattleAnimSystems.BattleAnimSystem_GetCurrentParticleSystem(system)!);
        int battler = BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system)) == 3 ? 0 : 1;
        BallEffects.BattleParticle_GetDefaultPosition(battler, out pos, BattleAnimSystems.BattleAnimSystem_IsContest(system), projection);
    }

    // OFFSET 0x022242E0
    private static void GenericEmitterCallback_ApplyPosition(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        BattleAnimSystem system = data.BattleAnimSys!;
        int start = data.StartBattler;
        int end = data.EndBattler;
        VecFx32 pos = default;
        switch (data.Params[GenericEmitterCallbackData.ParamPosition])
        {
            case 1:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(system, start, out pos);
                break;
            case 2:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(system, end, out pos);
                break;
            case 3:
            {
                int[] parameters = new int[4];
                BattleAnimSystems.BattleAnimSystem_GetExtraParams(system, parameters, 4);
                data.Dir = GenericEmitterCallback_GetDirectionForBattlers(data);
                pos.X = parameters[1];
                pos.Y = parameters[2];
                pos.Z = parameters[3];
                if (parameters[0] == FlipDisable)
                {
                    data.Dir = 1;
                }
                pos.X *= data.Dir;
                pos.Y *= data.Dir;
                break;
            }
            case 4:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(system, start, out pos);
                GenericEmitterCallback_OffsetPosition(data, ref pos);
                break;
            case 5:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(system, end, out pos);
                GenericEmitterCallback_OffsetPosition(data, ref pos);
                break;
            case 6:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Beam1(system, start, out pos);
                break;
            case 7:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Beam1(system, end, out pos);
                break;
            case 8:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_HydroPump(system, start, out pos);
                break;
            case 9:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_HydroPump(system, end, out pos);
                break;
            case 10:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Beam2(system, start, out pos);
                break;
            case 11:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Beam2(system, end, out pos);
                break;
            case 12:
                GenericEmitterCallback_GetAttackerSidePosition(emitter, data, out pos);
                GenericEmitterCallback_OffsetPosition(data, ref pos);
                break;
            case 13:
                GenericEmitterCallback_GetDefenderSidePosition(emitter, data, out pos);
                GenericEmitterCallback_OffsetPosition(data, ref pos);
                break;
            case 14:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Beam3(system, start, out pos);
                break;
            case 15:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Beam3(system, end, out pos);
                break;
            case 16:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Hypnosis(system, start, out pos);
                break;
            case 17:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Hypnosis(system, end, out pos);
                break;
            case 18:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_TriAttack(system, start, out pos);
                break;
            case 19:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_TriAttack(system, end, out pos);
                break;
            case 20:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_HydroCannon(system, start, out pos);
                break;
            case 21:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_HydroCannon(system, end, out pos);
                break;
            case 22:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_HyperVoice(system, start, out pos);
                break;
            case 23:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_HyperVoice(system, end, out pos);
                break;
            case 24:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_GrassWhistle(system, start, out pos);
                break;
            case 25:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_GrassWhistle(system, end, out pos);
                break;
            case 26:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_DragonPulse(system, start, out pos);
                break;
            case 27:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_DragonPulse(system, end, out pos);
                break;
            case 28:
                pos.X = 0x2CE0;
                pos.Y = 0;
                pos.Z = 0;
                break;
            case 30:
            {
                int type = BattleAnimSystems.BattleAnimSystem_IsContest(system) == 1 ? 6 : BattleAnimUtil.BattleAnimUtil_GetBattlerType(system, start);
                pos = ReadTablePosition(BatonPassPositionsAddress, 7, type);
                break;
            }
            case 31:
                pos = ReadTablePosition(BubblePositionsAddress, 6, BattleAnimUtil.BattleAnimUtil_GetBattlerType(system, start));
                break;
            case 32:
            {
                int type = BattleAnimSystems.BattleAnimSystem_IsContest(system) == 1 ? 6 : BattleAnimUtil.BattleAnimUtil_GetBattlerType(system, start);
                pos = ReadTablePosition(DragonBreathPositionsAddress, 7, type);
                break;
            }
            case 33:
                pos.X = -0x1388;
                pos.Y = -0x1770;
                pos.Z = 0;
                break;
            case 34:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(system, start, out pos);
                break;
            case EmitterPosNormalOffsetExStart:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(system, start, out pos);
                GenericEmitterCallback_OffsetPositionForBattler(data, start, ref pos);
                break;
            case EmitterPosNormalOffsetExEnd:
                BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(system, end, out pos);
                GenericEmitterCallback_OffsetPositionForBattler(data, end, ref pos);
                break;
        }
        data.Position = pos;
        VecFx32 basePos = emitter.Resource!.Header.EmitterBasePos;
        emitter.Position.X = pos.X + basePos.X;
        emitter.Position.Y = pos.Y + basePos.Y;
        emitter.Position.Z = pos.Z + basePos.Z;
    }

    private static VecFx16 AxisFromConfig(uint tableAddress, int startType, int endType, bool halveZ)
    {
        VecFx32 config = ReadAxisConfig(tableAddress, startType, endType);
        return Fx16(config.X, config.Y, halveZ ? config.Z / 2 : config.Z);
    }

    private static VecFx16 AxisFromSide(GenericEmitterCallbackData data, int startType, int endType)
    {
        BattleAnimSystem system = data.BattleAnimSys!;
        if (BattleAnimSystems.BattleAnimSystem_IsContest(system) == 1)
        {
            VecFx32 config = ReadAxisConfig(AxisConfigs1Address, startType, endType);
            return Fx16(-config.X, config.Y, config.Z / 2);
        }
        if (BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, data.StartBattler) == BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, data.EndBattler))
        {
            return AxisFromConfig(AxisConfigs1Address, startType, endType, true);
        }
        if (BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, data.StartBattler) == 3)
        {
            return Fx16(0xEC0, 0x840, 0x5FC);
        }
        return Fx16(-0x1084, -0xAA8, 0x5FC);
    }

    private static VecFx16 Negate(int[] v)
    {
        return Fx16(-v[0], -v[1], -v[2]);
    }

    private static VecFx16 Plain(int[] v)
    {
        return Fx16(v[0], v[1], v[2]);
    }

    private static VecFx16 AxisSpecial(GenericEmitterCallbackData data, int startType, int endType)
    {
        int[] configs = ReadOverlayInts(SpecialAxisConfigsAddress, 28);
        int[] contestConfig = ReadOverlayInts(SpecialAxisContestConfigAddress, 4);
        int[] pE = new int[4];
        int[] p1E1 = new int[4];
        int[] p1P2 = new int[4];
        int[] p1E2 = new int[4];
        int[] p2P1 = new int[4];
        int[] p2E1 = new int[4];
        int[] p2E2 = new int[4];
        for (int i = 0; i < 4; i++)
        {
            pE[i] = configs[i];
            p1E1[i] = configs[4 + i];
            p1P2[i] = configs[8 + i];
            p1E2[i] = configs[12 + i];
            p2P1[i] = configs[16 + i];
            p2E1[i] = configs[20 + i];
            p2E2[i] = configs[24 + i];
        }
        if (BattleAnimSystems.BattleAnimSystem_IsContest(data.BattleAnimSys!) == 1)
        {
            for (int i = 0; i < 4; i++)
            {
                pE[i] = contestConfig[i];
                p1E1[i] = contestConfig[i];
                p1P2[i] = contestConfig[i];
                p1E2[i] = contestConfig[i];
                p2P1[i] = contestConfig[i];
                p2E1[i] = contestConfig[i];
                p2E2[i] = contestConfig[i];
            }
        }
        switch (startType)
        {
            case 1:
                return pE[3] == FlipDisable ? Plain(pE) : Negate(pE);
            case 2:
                if (endType == 3)
                {
                    return Plain(p1E1);
                }
                if (endType == 5)
                {
                    return Plain(p1E2);
                }
                return Plain(p1P2);
            case 3:
                if (endType == 2)
                {
                    return p1E1[3] == FlipDisable ? Plain(p1E1) : Negate(p1E1);
                }
                if (endType == 5)
                {
                    return p1P2[3] == FlipDisable ? Negate(p2P1) : Plain(p2P1);
                }
                return p2E1[3] == FlipDisable ? Plain(p2E1) : Negate(p2E1);
            case 4:
                if (endType == 3)
                {
                    return Plain(p2E1);
                }
                if (endType == 5)
                {
                    return Plain(p2E2);
                }
                return Plain(p2P1);
            case 5:
                if (endType == 3)
                {
                    return p1E1[3] == FlipDisable ? Plain(p2P1) : Negate(p2P1);
                }
                if (endType == 2)
                {
                    return p1E1[3] == FlipDisable ? Plain(p1E2) : Negate(p1E2);
                }
                return p1E1[3] == FlipDisable ? Plain(p2E2) : Negate(p2E2);
            default:
                return Plain(pE);
        }
    }

    // OFFSET 0x02224614
    private static void GenericEmitterCallback_ApplyAxis(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        BattleAnimSystem system = data.BattleAnimSys!;
        int st = BattleAnimUtil.BattleAnimUtil_GetBattlerType(system, data.StartBattler);
        int et = BattleAnimUtil.BattleAnimUtil_GetBattlerType(system, data.EndBattler);
        bool contest = BattleAnimSystems.BattleAnimSystem_IsContest(system) == 1;
        bool defenderTarget = data.Params[GenericEmitterCallbackData.ParamTargetMode] == TargetModeGenericDefender;
        VecFx16 axis = default;
        switch (data.Params[GenericEmitterCallbackData.ParamAxis])
        {
            case 0:
                axis = Fx16(0, 0, 0);
                break;
            case 1:
            case 2:
                if (contest)
                {
                    axis = defenderTarget ? Fx16(-0xD08, 0x730, 0x2E0) : Fx16(0x920, -0x5A0, 0x2E0);
                }
                else
                {
                    axis = AxisFromConfig(AxisConfigs1Address, st, et, true);
                }
                break;
            case 4:
            case 5:
                axis = AxisFromSide(data, st, et);
                break;
            case 8:
            case 9:
                if (contest)
                {
                    axis = defenderTarget ? Fx16(-0x920, 0x5A0, 0x2E0) : Fx16(0x920, -0x5A0, 0x2E0);
                }
                else
                {
                    axis = AxisFromConfig(AxisConfigs2Address, st, et, true);
                }
                break;
            case 10:
            case 11:
                if (contest)
                {
                    axis = Fx16(-0x920, 0x5A0, 0x2E0);
                }
                else
                {
                    VecFx32 x = ReadAxisConfig(AxisConfigs3Address, st, et);
                    VecFx32 yz = ReadAxisConfig(AxisConfigs2Address, st, et);
                    axis = Fx16(x.X, yz.Y, yz.Z / 2);
                }
                break;
            case 12:
            case 13:
                axis = contest ? Fx16(-0xB78, 0x5A0, 0x2E0) : AxisFromConfig(AxisConfigs4Address, st, et, false);
                break;
            case 14:
            case 15:
                axis = contest ? Fx16(-0x920, 0x5A0, 0x2E0) : AxisFromConfig(AxisConfigs5Address, st, et, false);
                break;
            case 16:
            case 17:
                axis = contest ? Fx16(-0x920, 0x5A0, 0x2E0) : AxisFromConfig(AxisConfigs6Address, st, et, false);
                break;
            case 18:
            case 19:
                axis = contest ? Fx16(-0x920, 0x5A0, 0x2E0) : AxisFromConfig(AxisConfigs7Address, st, et, false);
                break;
            case 20:
            case 21:
                axis = contest ? Fx16(-0x10F0, 0x5A0, 0x2E0) : AxisFromConfig(AxisConfigs8Address, st, et, false);
                break;
            case 6:
            case 7:
                axis = AxisSpecial(data, st, et);
                break;
            case 3:
                axis = Fx16(-0x320, 0x4B0, 0x1F4);
                break;
            case 22:
                axis = Fx16(-0xD52, -0xA54, 0);
                break;
            case 25:
                axis = Fx16(-0xD70, 0x7A0, 0);
                break;
            case 26:
                if (BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, data.StartBattler) == 3)
                {
                    axis = Fx16(0xEC0, 0x840, 0);
                }
                else
                {
                    axis = Fx16(-0x1770, -0x898, 0);
                }
                break;
            case 24:
            {
                int[] configs = ReadOverlayInts(AxisPerTypeAddress, 12);
                int startType = BattleAnimUtil.BattleAnimUtil_GetBattlerType(system, data.StartBattler);
                axis = Fx16(configs[startType * 2], configs[startType * 2 + 1], 0);
                break;
            }
        }
        emitter.Axis = axis;
    }

    // OFFSET 0x02224E74
    private static void GenericEmitterCallback_ApplyBehavior(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        int valueCount = GenericEmitterCallback_GetBehaviorValueCount();
        for (int value = 0; value < valueCount; value++)
        {
            int flag = GenericEmitterCallback_GetBehaviorFlag(value);
            if ((data.Params[GenericEmitterCallbackData.ParamBehavior] & flag) != flag)
            {
                continue;
            }
            GenericEmitterCallback_CallBehaviorFunction(value, emitter, data);
        }
    }

    // OFFSET 0x02224EA8
    private static void GenericEmitterCallback_ApplyCamera(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        GenericEmitterCallback_ApplyCameraFunc(data.Params[GenericEmitterCallbackData.ParamCamera], emitter, data);
    }

    // OFFSET 0x02224EB8
    private static void GenericEmitterCallback_ApplyPriority(SplEmitter emitter, GenericEmitterCallbackData data)
    {
        BattleAnimSystem system = data.BattleAnimSys!;
        BattleAnimUtil.BattleAnimUtil_GetBattlerType(system, data.StartBattler);
        int endBattlerType = BattleAnimUtil.BattleAnimUtil_GetBattlerType(system, data.EndBattler);
        int z = 0;
        switch (data.Params[GenericEmitterCallbackData.ParamPriority])
        {
            case 1:
                z = endBattlerType == 0 ? 0x100 : -0x1400;
                break;
            case 2:
                z = endBattlerType == 0 ? 0 : -0x1500;
                break;
        }
        emitter.Position.Z = z + emitter.Resource!.Header.EmitterBasePos.Z;
    }

    // OFFSET 0x02224F14
    private static void BattleAnimEmitterCb_Generic(SplEmitter emitter)
    {
        BattleAnimSystem system = CallbackSystem();
        GenericEmitterCallbackData data = new();
        data.BattleAnimSys = system;
        data.ParticleSys = BattleAnimSystems.BattleAnimSystem_GetCurrentParticleSystem(system);
        BattleAnimSystems.BattleAnimSystem_GetExtraParams(system, data.Params, GenericEmitterCallbackData.ParamCount);
        switch (data.Params[GenericEmitterCallbackData.ParamTargetMode])
        {
            case 1:
            case 3:
                data.StartBattler = BattleAnimSystems.BattleAnimSystem_GetDefender(system);
                data.EndBattler = BattleAnimSystems.BattleAnimSystem_GetAttacker(system);
                break;
            default:
                data.StartBattler = BattleAnimSystems.BattleAnimSystem_GetAttacker(system);
                data.EndBattler = BattleAnimSystems.BattleAnimSystem_GetDefender(system);
                break;
        }
        GenericEmitterCallback_ApplyParams(emitter, data);
    }

    // OFFSET 0x02224FC4
    public static sbyte GenericEmitterCallback_GetDirectionForBattlers(GenericEmitterCallbackData data)
    {
        sbyte dir = 1;
        int startType = BattleAnimUtil.BattleAnimUtil_GetBattlerType(data.BattleAnimSys!, data.StartBattler);
        BattleAnimUtil.BattleAnimUtil_GetBattlerType(data.BattleAnimSys!, data.EndBattler);
        switch (startType)
        {
            case 1:
            case 3:
            case 5:
                dir = -1;
                break;
        }
        return dir;
    }

    // OFFSET 0x02225008
    private static void GenericEmitterCallback_OffsetPositionForBattler(GenericEmitterCallbackData data, int battler, ref VecFx32 pos)
    {
        int[] parameters = new int[4];
        int[] parametersUnused = new int[4];
        for (int battlerType = 0; battlerType < 6; battlerType++)
        {
            if (battlerType == BattleAnimUtil.BattleAnimUtil_GetBattlerType(data.BattleAnimSys!, battler))
            {
                BattleAnimSystems.BattleAnimSystem_GetExtraParams(data.BattleAnimSys!, parameters, 4);
            }
            else
            {
                BattleAnimSystems.BattleAnimSystem_GetExtraParams(data.BattleAnimSys!, parametersUnused, 4);
            }
        }
        data.Dir = GenericEmitterCallback_GetDirectionForBattlers(data);
        if (parameters[0] == FlipDisable)
        {
            data.Dir = 1;
        }
        pos.X += parameters[1] * data.Dir;
        pos.Y += parameters[2] * data.Dir;
        pos.Z += parameters[3] * data.Dir;
    }

    // OFFSET 0x02225094
    private static void GenericEmitterCallback_OffsetPosition(GenericEmitterCallbackData data, ref VecFx32 pos)
    {
        int[] parameters = new int[4];
        BattleAnimSystems.BattleAnimSystem_GetExtraParams(data.BattleAnimSys!, parameters, 4);
        data.Dir = GenericEmitterCallback_GetDirectionForBattlers(data);
        if (parameters[0] == FlipDisable)
        {
            data.Dir = 1;
        }
        pos.X += parameters[1] * data.Dir;
        pos.Y += parameters[2] * data.Dir;
        pos.Z += parameters[3] * data.Dir;
    }
}
