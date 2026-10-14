namespace PlatinumSharp.Game;

public static class SplBehaviors
{
    private const int NoCollisionPlaneHeight = int.MinValue;
    private const int CollisionTypeKill = 0;
    private const int CollisionTypeBounce = 1;
    private const int SpinAxisX = 0;
    private const int SpinAxisY = 1;
    private const int SpinAxisZ = 2;

    public static void Apply(SplBehavior behavior, SplParticle particle, ref VecFx32 acceleration, SplEmitter emitter)
    {
        switch (behavior.ApplyFunc)
        {
            case SplBehaviorType.Gravity:
                SPLBehavior_ApplyGravity((SplGravityBehavior)behavior.Object!, particle, ref acceleration, emitter);
                break;
            case SplBehaviorType.Random:
                SPLBehavior_ApplyRandom((SplRandomBehavior)behavior.Object!, particle, ref acceleration, emitter);
                break;
            case SplBehaviorType.Magnet:
                SPLBehavior_ApplyMagnet((SplMagnetBehavior)behavior.Object!, particle, ref acceleration, emitter);
                break;
            case SplBehaviorType.Spin:
                SPLBehavior_ApplySpin((SplSpinBehavior)behavior.Object!, particle, ref acceleration, emitter);
                break;
            case SplBehaviorType.CollisionPlane:
                SPLBehavior_ApplyCollisionPlane((SplCollisionPlaneBehavior)behavior.Object!, particle, ref acceleration, emitter);
                break;
            case SplBehaviorType.Convergence:
                SPLBehavior_ApplyConvergence((SplConvergenceBehavior)behavior.Object!, particle, ref acceleration, emitter);
                break;
        }
    }

    // OFFSET 0x020A1E30
    public static void SPLBehavior_ApplyConvergence(SplConvergenceBehavior behavior, SplParticle particle, ref VecFx32 acceleration, SplEmitter emitter)
    {
        particle.Position.X += Fx32.Mul(behavior.Force, behavior.Target.X - particle.Position.X);
        particle.Position.Y += Fx32.Mul(behavior.Force, behavior.Target.Y - particle.Position.Y);
        particle.Position.Z += Fx32.Mul(behavior.Force, behavior.Target.Z - particle.Position.Z);
    }

    // OFFSET 0x020A1EC4
    public static void SPLBehavior_ApplyCollisionPlane(SplCollisionPlaneBehavior behavior, SplParticle particle, ref VecFx32 acceleration, SplEmitter emitter)
    {
        int y = behavior.Y;
        if (emitter.CollisionPlaneHeight != NoCollisionPlaneHeight)
        {
            y = emitter.CollisionPlaneHeight;
        }
        int emitterY = particle.EmitterPos.Y;
        switch (behavior.CollisionType & 3)
        {
            case CollisionTypeKill:
                if (emitterY < y)
                {
                    if (emitterY + particle.Position.Y > y)
                    {
                        particle.Position.Y = y - emitterY;
                        particle.Age = particle.LifeTime;
                    }
                    return;
                }
                if (emitterY + particle.Position.Y < y)
                {
                    particle.Position.Y = y - emitterY;
                    particle.Age = particle.LifeTime;
                }
                return;
            case CollisionTypeBounce:
                if (emitterY < y)
                {
                    if (emitterY + particle.Position.Y > y)
                    {
                        particle.Position.Y = y - emitterY;
                        particle.Velocity.Y = -Fx32.Mul(particle.Velocity.Y, behavior.Elasticity);
                    }
                    return;
                }
                if (emitterY + particle.Position.Y >= y)
                {
                    return;
                }
                particle.Position.Y = y - emitterY;
                particle.Velocity.Y = -Fx32.Mul(particle.Velocity.Y, behavior.Elasticity);
                return;
            default:
                return;
        }
    }

    // OFFSET 0x020A1FE0
    public static void SPLBehavior_ApplySpin(SplSpinBehavior behavior, SplParticle particle, ref VecFx32 acceleration, SplEmitter emitter)
    {
        MtxFx33 rotation;
        int index = behavior.Angle >> 4;
        switch (behavior.Axis)
        {
            case SpinAxisX:
                Mtx.RotX33(out rotation, Render3D.Sin(index), Render3D.Cos(index));
                break;
            case SpinAxisY:
                Mtx.RotY33(out rotation, Render3D.Sin(index), Render3D.Cos(index));
                break;
            case SpinAxisZ:
                Mtx.RotZ33(out rotation, Render3D.Sin(index), Render3D.Cos(index));
                break;
            default:
                return;
        }
        SplMath.MultVec33(particle.Position, rotation, out particle.Position);
    }

    // OFFSET 0x020A20B8
    public static void SPLBehavior_ApplyMagnet(SplMagnetBehavior behavior, SplParticle particle, ref VecFx32 acceleration, SplEmitter emitter)
    {
        acceleration.X += (behavior.Force * (behavior.Target.X - particle.Position.X - particle.Velocity.X)) >> Fx32.Shift;
        acceleration.Y += (behavior.Force * (behavior.Target.Y - particle.Position.Y - particle.Velocity.Y)) >> Fx32.Shift;
        acceleration.Z += (behavior.Force * (behavior.Target.Z - particle.Position.Z - particle.Velocity.Z)) >> Fx32.Shift;
    }

    // OFFSET 0x020A213C
    public static void SPLBehavior_ApplyRandom(SplRandomBehavior behavior, SplParticle particle, ref VecFx32 acceleration, SplEmitter emitter)
    {
        if (SplMath.Mod(particle.Age, behavior.ApplyInterval) != 0)
        {
            return;
        }
        acceleration.X += SplRandom.Range(behavior.Magnitude.X);
        acceleration.Y += SplRandom.Range(behavior.Magnitude.Y);
        acceleration.Z += SplRandom.Range(behavior.Magnitude.Z);
    }

    // OFFSET 0x020A2204
    public static void SPLBehavior_ApplyGravity(SplGravityBehavior behavior, SplParticle particle, ref VecFx32 acceleration, SplEmitter emitter)
    {
        acceleration.X += behavior.Magnitude.X;
        acceleration.Y += behavior.Magnitude.Y;
        acceleration.Z += behavior.Magnitude.Z;
    }
}
