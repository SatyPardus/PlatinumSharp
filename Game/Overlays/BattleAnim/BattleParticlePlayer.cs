namespace PlatinumSharp.Game;

public sealed class BattleParticlePlayerParams
{
    public int Member;
    public int CameraProjection;
}

public sealed class BattleParticlePlayer
{
    public int Index;
    public int HeapID;
    public int Member;
    public int CameraProjection;
    public ParticleSystem? ParticleSys;
    public SysTask? Task;
    public int EmitterCount;
    public int Active;
}

public sealed class BattleParticleHolder
{
    public const int PlayerCount = 16;

    public int HeapID;
    public int Unknown04;
    public readonly BattleParticlePlayer?[] Players = new BattleParticlePlayer?[PlayerCount];
}

public static class BattleParticlePlayers
{
    private const int NarcBattleParticles = 0x1D;
    private const int ResourceAutoRelease = 0xA;
    private const uint EmitterCountTable = 0x0223B048;
    private const int FirstMember = 5;
    private const int MemberCount = 0x16;
    private const uint TaskPriority = 1000;

    // OFFSET 0x022237F0
    public static ParticleSystem BattleParticleUtil_CreateFromMember(int heapID, int member, int uploadImmediately)
    {
        ParticleSystem particleSystem = BattleParticleUtil.BattleParticleUtil_CreateParticleSystem(heapID)!;
        byte[] resource = ParticleSystem.ParticleSystem_LoadResourceFromNARC(NarcBattleParticles, member, heapID);
        ParticleSystem.ParticleSystem_SetResource(particleSystem, resource, ResourceAutoRelease, uploadImmediately);
        return particleSystem;
    }

    // OFFSET 0x02223818
    public static ParticleSystem BattleParticleUtil_CreateFromNarc(int heapID, int narcID, int member, int uploadImmediately)
    {
        ParticleSystem particleSystem = BattleParticleUtil.BattleParticleUtil_CreateParticleSystem(heapID)!;
        byte[] resource = ParticleSystem.ParticleSystem_LoadResourceFromNARC(narcID, member, heapID);
        ParticleSystem.ParticleSystem_SetResource(particleSystem, resource, ResourceAutoRelease, uploadImmediately);
        return particleSystem;
    }

    // OFFSET 0x02223894
    public static void BattleParticleUtil_Free(ParticleSystem particleSystem)
    {
        BattleParticleUtil.BattleParticleUtil_FreeParticleSystem(particleSystem);
    }

    // OFFSET 0x02237EFC
    public static BattleParticleHolder BattleParticleHolder_New(int heapID, int[]? unknown)
    {
        BattleParticleHolder holder = new();
        BallRotations.BattleAnim_SetDefaultAlphaBlending();
        holder.HeapID = heapID;
        if (unknown != null)
        {
            holder.Unknown04 = unknown[0];
        }
        for (int i = 0; i < BattleParticleHolder.PlayerCount; i++)
        {
            holder.Players[i] = null;
        }
        return holder;
    }

    // OFFSET 0x02237F38
    public static BattleParticlePlayer? BattleParticleHolder_AddPlayer(BattleParticleHolder holder, BattleParticlePlayerParams playerParams)
    {
        BattleParticlePlayer? player = null;
        for (int i = 0; i < BattleParticleHolder.PlayerCount; i++)
        {
            if (holder.Players[i] == null)
            {
                holder.Players[i] = BattleParticlePlayer_New(holder.HeapID, playerParams);
                holder.Players[i]!.Task = null;
                player = holder.Players[i];
                player!.Index = i;
                break;
            }
        }
        if (player == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return player;
    }

    // OFFSET 0x02237F74
    public static void BattleParticleHolder_Free(BattleParticleHolder holder)
    {
        for (int i = 0; i < BattleParticleHolder.PlayerCount; i++)
        {
            holder.Players[i] = null;
        }
    }

    // OFFSET 0x02237F98
    public static void BattleParticleHolder_StopAll(BattleParticleHolder holder)
    {
        for (int i = 0; i < BattleParticleHolder.PlayerCount; i++)
        {
            BattleParticlePlayer? player = holder.Players[i];
            if (player == null)
            {
                continue;
            }
            BattleParticleUtil_Free(player.ParticleSys!);
            if (player.Active != 0 && player.Task != null)
            {
                SysTask.SysTask_Done(player.Task);
            }
        }
    }

    // OFFSET 0x02237FC8
    private static BattleParticlePlayer BattleParticlePlayer_New(int heapID, BattleParticlePlayerParams playerParams)
    {
        BattleParticlePlayer player = new();
        player.HeapID = heapID;
        player.Member = playerParams.Member;
        player.CameraProjection = playerParams.CameraProjection;
        player.EmitterCount = BattleParticlePlayer_GetEmitterCount(player.Member);
        player.ParticleSys = BattleParticleUtil_CreateFromMember(player.HeapID, player.Member, 1);
        return player;
    }

    // OFFSET 0x02238004
    public static void BattleParticlePlayer_Start(BattleParticlePlayer player)
    {
        player.Active = 1;
        BattleParticlePlayer_CreateEmitters(player, BattleParticlePlayer_EmitterCallback);
        player.Task = SysTask.SysTask_Start(BattleParticlePlayer_UpdateTask, player, TaskPriority);
    }

    // OFFSET 0x0223802C
    public static int BattleParticlePlayer_IsActive(BattleParticlePlayer player)
    {
        return player.Active;
    }

    // OFFSET 0x02238030
    private static void BattleParticlePlayer_UpdateTask(SysTask task, object? param)
    {
        BattleParticlePlayer player = (BattleParticlePlayer)param!;
        if (ParticleSystem.ParticleSystem_GetActiveEmitterCount(player.ParticleSys!) == 0)
        {
            player.Active = 0;
            BattleParticleUtil_Free(player.ParticleSys!);
            SysTask.SysTask_Done(task);
        }
    }

    // OFFSET 0x02238054
    private static void BattleParticlePlayer_CreateEmitters(BattleParticlePlayer player, SplEmitterCallback callback)
    {
        for (int i = 0; i < player.EmitterCount; i++)
        {
            ParticleSystem.ParticleSystem_CreateEmitterWithCallback(player.ParticleSys!, i, callback, player);
        }
        ParticleSystem.ParticleSystem_SetCameraProjection(player.ParticleSys!, player.CameraProjection);
    }

    // OFFSET 0x02238080
    private static void BattleParticlePlayer_EmitterCallback(SplEmitter emitter)
    {
        ParticleSystem.ParticleSystem_GetEmitterCallbackParam();
    }

    // OFFSET 0x02238088
    private static int BattleParticlePlayer_GetEmitterCount(int member)
    {
        if ((uint)(member - FirstMember) >= MemberCount)
        {
            ErrorHandling.GF_AssertFail();
            return 0;
        }
        return (int)Nitro.Bin.U32(RomData.Overlay(12, (uint)(EmitterCountTable + (member - FirstMember) * 4), 4), 0);
    }
}
