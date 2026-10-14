namespace PlatinumSharp.Game;

public sealed class BattleAnimScriptFuncCommon
{
    public byte State;
    public byte Counter1;
    public byte Counter2;
    public byte Unused03;
    public BattleAnimSystem? BattleAnimSys;
    public SpriteSystem? SpriteSystem;
    public SpriteManager? PokemonSpriteManager;
    public SpriteManager? PrimarySpriteManager;
    public BgConfig? BgConfig;
    public PaletteData? PaletteData;
}

public sealed class BattleAnimSpriteInfo
{
    public PokemonSprite? MonSprite;
    public ManagedSprite? HwSprite;
    public int Battler;
    public short X;
    public short Y;
}

public static partial class BattleAnimUtil
{
    private const uint BattlerTypeDefaultPositionsAddress = 0x0223A230;
    private const uint ContestantDefaultPositionsAddress = 0x0223A248;
    private const uint UnitDownAddress = 0x0223A224;
    private const int Overlay = 12;
    private const int WorldPosNormal = 0;
    private const int WorldPosBeam1 = 1;
    private const int WorldPosBeam2 = 2;
    private const int WorldPosBeam3 = 3;
    private const int WorldPosHypnosis = 4;
    private const int WorldPosTriAttack = 5;
    private const int WorldPosHydroCannon = 6;
    private const int WorldPosHyperVoice = 7;
    private const int WorldPosGrassWhistle = 8;
    private const int WorldPosDragonPulse = 9;
    private const int WorldPosCameraLookAt = 10;
    private const int WorldPosHydroPump = 11;
    private const int ParticleViewportTopY = 0x68D0;
    private const int DefaultAlpha = -1;
    private const int DefaultSpriteAlpha = 0x1A;
    private const int DefaultBgAlpha = 5;
    private const int BlendPlaneBg0Obj = 0x11;
    private const int WindowEffectsFlag = 0x20;
    private const int AnimAllBattlers = 0x40;
    private const int AnimNotAttacker = 0x20;
    private const int AnimAttacker = 2;
    private const int AnimAttackerPartner = 4;
    private const int AnimDefender = 8;
    private const int AnimDefenderPartner = 0x10;
    private const int AnimPokemonSprite0 = 2;
    private const int AnimPokemonSprite1 = 4;
    private const int AnimPokemonSprite2 = 8;
    private const int AnimPokemonSprite3 = 0x10;

    public static bool BattleAnimUtil_IsMaskSet(int value, int mask)
    {
        return (value & mask) == mask;
    }

    // OFFSET 0x02235310
    public static int BattleAnimUtil_GetOpposingBattlerType(int battlerType)
    {
        switch (battlerType)
        {
            case 0:
                return 1;
            case 1:
                return 0;
            case 2:
                return 3;
            case 3:
                return 2;
            case 4:
                return 5;
            case 5:
                return 4;
        }
        ErrorHandling.GF_AssertFail();
        return 0;
    }

    // OFFSET 0x02235350
    public static void BattleAnimUtil_GetBattlerTypeDefaultPos(int battlerType, int isContest, out short x, out short y)
    {
        uint table = isContest != 0 ? ContestantDefaultPositionsAddress : BattlerTypeDefaultPositionsAddress;
        ushort[] pos = RomData.OverlayU16(Overlay, (uint)(table + battlerType * 4), 2);
        x = (short)pos[0];
        y = (short)pos[1];
    }

    // OFFSET 0x022353AC
    public static void BattleAnimUtil_GetBattlerDefaultPos(BattleAnimSystem system, int battler, out short x, out short y)
    {
        int battlerType = BattleAnimUtil_GetBattlerType(system, battler);
        int isContest = BattleAnimSystems.BattleAnimSystem_IsContest(system);
        BattleAnimUtil_GetBattlerTypeDefaultPos(battlerType, isContest, out x, out y);
    }

    // OFFSET 0x02235458
    public static void BattleAnimUtil_GetBattlerTypeWorldPos_Beam1(int battlerType, out VecFx32 pos, int isContest, int projection)
    {
        BallEffects.BattleParticle_GetPosition(battlerType, out pos, isContest, projection, WorldPosBeam1);
    }

    // OFFSET 0x02235468
    public static void BattleAnimUtil_GetBattlerTypeWorldPos_Beam2(int battlerType, out VecFx32 pos, int isContest, int projection)
    {
        BallEffects.BattleParticle_GetPosition(battlerType, out pos, isContest, projection, WorldPosBeam2);
    }

    // OFFSET 0x02235478
    public static void BattleAnimUtil_GetBattlerTypeWorldPos_Beam3(int battlerType, out VecFx32 pos, int isContest, int projection)
    {
        BallEffects.BattleParticle_GetPosition(battlerType, out pos, isContest, projection, WorldPosBeam3);
    }

    // OFFSET 0x02235488
    public static void BattleAnimUtil_GetBattlerTypeWorldPos_Hypnosis(int battlerType, out VecFx32 pos, int isContest, int projection)
    {
        BallEffects.BattleParticle_GetPosition(battlerType, out pos, isContest, projection, WorldPosHypnosis);
    }

    // OFFSET 0x02235498
    public static void BattleAnimUtil_GetBattlerTypeWorldPos_TriAttack(int battlerType, out VecFx32 pos, int isContest, int projection)
    {
        BallEffects.BattleParticle_GetPosition(battlerType, out pos, isContest, projection, WorldPosTriAttack);
    }

    // OFFSET 0x022354A8
    public static void BattleAnimUtil_GetBattlerTypeWorldPos_HydroCannon(int battlerType, out VecFx32 pos, int isContest, int projection)
    {
        BallEffects.BattleParticle_GetPosition(battlerType, out pos, isContest, projection, WorldPosHydroCannon);
    }

    // OFFSET 0x022354B8
    public static void BattleAnimUtil_GetBattlerTypeWorldPos_HyperVoice(int battlerType, out VecFx32 pos, int isContest, int projection)
    {
        BallEffects.BattleParticle_GetPosition(battlerType, out pos, isContest, projection, WorldPosHyperVoice);
    }

    // OFFSET 0x022354C8
    public static void BattleAnimUtil_GetBattlerTypeWorldPos_GrassWhistle(int battlerType, out VecFx32 pos, int isContest, int projection)
    {
        BallEffects.BattleParticle_GetPosition(battlerType, out pos, isContest, projection, WorldPosGrassWhistle);
    }

    // OFFSET 0x022354D8
    public static void BattleAnimUtil_GetBattlerTypeWorldPos_DragonPulse(int battlerType, out VecFx32 pos, int isContest, int projection)
    {
        BallEffects.BattleParticle_GetPosition(battlerType, out pos, isContest, projection, WorldPosDragonPulse);
    }

    // OFFSET 0x022354E8
    public static void BattleAnimUtil_GetBattlerTypeWorldPos_CameraLookAt(int battlerType, out VecFx32 pos, int isContest, int projection)
    {
        BallEffects.BattleParticle_GetPosition(battlerType, out pos, isContest, projection, WorldPosCameraLookAt);
    }

    // OFFSET 0x022354F8
    public static void BattleAnimUtil_GetBattlerTypeWorldPos_HydroPump(int battlerType, out VecFx32 pos, int isContest, int projection)
    {
        BallEffects.BattleParticle_GetPosition(battlerType, out pos, isContest, projection, WorldPosHydroPump);
    }

    private static void GetBattlerWorldPos(BattleAnimSystem system, int battler, out VecFx32 pos, int posType)
    {
        ParticleSystem ps = BattleAnimSystems.BattleAnimSystem_GetCurrentParticleSystem(system)!;
        int projection = ParticleSystem.ParticleSystem_GetCameraProjection(ps);
        int battlerType = BattleAnimUtil_GetBattlerType(system, battler);
        int isContest = BattleAnimSystems.BattleAnimSystem_IsContest(system);
        BallEffects.BattleParticle_GetPosition(battlerType, out pos, isContest, projection, posType);
    }

    // OFFSET 0x02235508
    public static void BattleAnimUtil_GetBattlerWorldPos_Normal(BattleAnimSystem system, int battler, out VecFx32 pos)
    {
        GetBattlerWorldPos(system, battler, out pos, WorldPosNormal);
    }

    // OFFSET 0x02235538
    public static void BattleAnimUtil_GetBattlerWorldPos_Beam1(BattleAnimSystem system, int battler, out VecFx32 pos)
    {
        GetBattlerWorldPos(system, battler, out pos, WorldPosBeam1);
    }

    // OFFSET 0x02235568
    public static void BattleAnimUtil_GetBattlerWorldPos_Beam2(BattleAnimSystem system, int battler, out VecFx32 pos)
    {
        GetBattlerWorldPos(system, battler, out pos, WorldPosBeam2);
    }

    // OFFSET 0x02235598
    public static void BattleAnimUtil_GetBattlerWorldPos_Beam3(BattleAnimSystem system, int battler, out VecFx32 pos)
    {
        GetBattlerWorldPos(system, battler, out pos, WorldPosBeam3);
    }

    // OFFSET 0x022355C8
    public static void BattleAnimUtil_GetBattlerWorldPos_Hypnosis(BattleAnimSystem system, int battler, out VecFx32 pos)
    {
        GetBattlerWorldPos(system, battler, out pos, WorldPosHypnosis);
    }

    // OFFSET 0x022355F8
    public static void BattleAnimUtil_GetBattlerWorldPos_TriAttack(BattleAnimSystem system, int battler, out VecFx32 pos)
    {
        GetBattlerWorldPos(system, battler, out pos, WorldPosTriAttack);
    }

    // OFFSET 0x02235628
    public static void BattleAnimUtil_GetBattlerWorldPos_HydroCannon(BattleAnimSystem system, int battler, out VecFx32 pos)
    {
        GetBattlerWorldPos(system, battler, out pos, WorldPosHydroCannon);
    }

    // OFFSET 0x02235658
    public static void BattleAnimUtil_GetBattlerWorldPos_HyperVoice(BattleAnimSystem system, int battler, out VecFx32 pos)
    {
        GetBattlerWorldPos(system, battler, out pos, WorldPosHyperVoice);
    }

    // OFFSET 0x02235688
    public static void BattleAnimUtil_GetBattlerWorldPos_GrassWhistle(BattleAnimSystem system, int battler, out VecFx32 pos)
    {
        GetBattlerWorldPos(system, battler, out pos, WorldPosGrassWhistle);
    }

    // OFFSET 0x022356B8
    public static void BattleAnimUtil_GetBattlerWorldPos_DragonPulse(BattleAnimSystem system, int battler, out VecFx32 pos)
    {
        GetBattlerWorldPos(system, battler, out pos, WorldPosDragonPulse);
    }

    // OFFSET 0x022356E8
    public static void BattleAnimUtil_GetBattlerWorldPos_CameraLookAt(BattleAnimSystem system, int battler, out VecFx32 pos)
    {
        GetBattlerWorldPos(system, battler, out pos, WorldPosCameraLookAt);
    }

    // OFFSET 0x02235718
    public static void BattleAnimUtil_GetBattlerWorldPos_HydroPump(BattleAnimSystem system, int battler, out VecFx32 pos)
    {
        GetBattlerWorldPos(system, battler, out pos, WorldPosHydroPump);
    }

    // OFFSET 0x02235748
    public static void BattleAnimUtil_GetParticleViewportTopPosition(out VecFx32 pos)
    {
        pos.X = 0;
        pos.Y = ParticleViewportTopY;
        pos.Z = 0;
    }

    // OFFSET 0x02235760
    public static void BattleAnimUtil_GetUpVectorForBattler(int battler, out VecFx32 up)
    {
        if (battler == 0)
        {
            ParticleSystem.ParticleSystem_GetDefaultCameraUp(out up);
            return;
        }
        uint[] down = RomData.OverlayU32(Overlay, UnitDownAddress, 3);
        up.X = (int)down[0];
        up.Y = (int)down[1];
        up.Z = (int)down[2];
    }

    // OFFSET 0x02235780
    public static void BattleAnimUtil_SetSpriteBgBlending(BattleAnimSystem system, int spriteAlpha, int bgAlpha)
    {
        if (spriteAlpha == DefaultAlpha)
        {
            spriteAlpha = DefaultSpriteAlpha;
        }
        if (bgAlpha == DefaultAlpha)
        {
            bgAlpha = DefaultBgAlpha;
        }
        Video.SetBlendAlpha(0, 0, (1 << BattleAnimSystems.BattleAnimSystem_GetBgID(system, 2)) | 1, spriteAlpha, bgAlpha);
    }

    // OFFSET 0x022357BC
    public static void BattleAnimUtil_SetSpriteBlending(BattleAnimSystem system, int planes, int spriteAlpha, int otherAlpha)
    {
        if (spriteAlpha == DefaultAlpha)
        {
            spriteAlpha = DefaultSpriteAlpha;
        }
        if (otherAlpha == DefaultAlpha)
        {
            otherAlpha = DefaultBgAlpha;
        }
        Video.SetBlendAlpha(0, 0, planes, spriteAlpha, otherAlpha);
    }

    // OFFSET 0x022357EC
    public static void BattleAnimUtil_SetEffectBaseBgBlending(BattleAnimSystem system, int alphaBase, int alphaEffect)
    {
        if (alphaBase == DefaultAlpha)
        {
            alphaBase = DefaultSpriteAlpha;
        }
        if (alphaEffect == DefaultAlpha)
        {
            alphaEffect = DefaultBgAlpha;
        }
        int second = (1 << BattleAnimSystems.BattleAnimSystem_GetBgID(system, 2)) | BlendPlaneBg0Obj;
        Video.SetBlendAlpha(0, 1 << BattleAnimSystems.BattleAnimSystem_GetBgID(system, 1), second, alphaBase, alphaEffect);
    }

    // OFFSET 0x02235838
    public static void BattleAnimUtil_SetBackgroundWindowMask(BattleAnimSystem system, int window, int applyEffects)
    {
        int mask = (1 << BattleAnimSystems.BattleAnimSystem_GetBgID(system, 2)) | (1 << BattleAnimSystems.BattleAnimSystem_GetBgID(system, 0)) | (1 << BattleAnimSystems.BattleAnimSystem_GetBgID(system, 1)) | BlendPlaneBg0Obj;
        if (applyEffects != 0)
        {
            mask |= WindowEffectsFlag;
        }
        Video.SetWindowInside(0, window == 0 ? 0 : 1, (byte)mask);
        int outside = (1 << BattleAnimSystems.BattleAnimSystem_GetBgID(system, 1)) | (1 << BattleAnimSystems.BattleAnimSystem_GetBgID(system, 0)) | BlendPlaneBg0Obj | WindowEffectsFlag;
        Video.SetWindowOutside(0, (byte)outside);
    }

    // OFFSET 0x02235918
    public static void BattleAnimUtil_GetMonSpritePos(PokemonSprite? sprite, out short x, out short y)
    {
        if (sprite == null || !PokemonSprite.PokemonSprite_IsActive(sprite))
        {
            x = 0;
            y = 0;
            return;
        }
        x = (short)PokemonSprite.PokemonSprite_GetAttribute(sprite, 0);
        y = (short)PokemonSprite.PokemonSprite_GetAttribute(sprite, 1);
    }

    // OFFSET 0x02235950
    public static void BattleAnimUtil_GetSpritePos(ManagedSprite sprite, out short x, out short y)
    {
        SpriteSystem.ManagedSprite_GetPositionXY(sprite, out x, out y);
    }

    // OFFSET 0x0223595C
    public static void BattleAnimSystem_GetCommonData(BattleAnimSystem system, BattleAnimScriptFuncCommon common)
    {
        common.State = 0;
        common.Counter1 = 0;
        common.Counter2 = 0;
        common.Unused03 = 0;
        common.BattleAnimSys = system;
        common.SpriteSystem = BattleAnimSystems.BattleAnimSystem_GetSpriteSystem(system);
        common.PokemonSpriteManager = system.PokemonSpriteManager;
        common.PrimarySpriteManager = BattleAnimSystems.BattleAnimSystem_GetPrimarySpriteManager(system);
        common.BgConfig = BattleAnimSystems.BattleAnimSystem_GetBgConfig(system);
        common.PaletteData = BattleAnimSystems.BattleAnimSystem_GetPaletteData(system);
    }

    private static void AddBattlerSprite(BattleAnimSystem system, int battler, BattleAnimSpriteInfo[] sprites, ref int count)
    {
        sprites[count].MonSprite = BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, battler);
        if (sprites[count].MonSprite != null)
        {
            sprites[count].Battler = battler;
            BattleAnimUtil_GetMonSpritePos(sprites[count].MonSprite, out sprites[count].X, out sprites[count].Y);
            count++;
        }
    }

    // OFFSET 0x02235998
    public static void BattleAnimUtil_GetBattlerSprites(BattleAnimSystem system, int targets, BattleAnimSpriteInfo[] sprites, out int count)
    {
        count = 0;
        if (BattleAnimUtil_IsMaskSet(targets, AnimAllBattlers))
        {
            int attacker = BattleAnimSystems.BattleAnimSystem_GetAttacker(system);
            AddBattlerSprite(system, attacker, sprites, ref count);
            int battler = BattleAnimUtil_GetAlliedBattler(system, attacker);
            AddBattlerSprite(system, battler, sprites, ref count);
            int type = BattleAnimSystems.BattleAnimSystem_GetBattlerType(system, attacker);
            type = BattleAnimUtil_GetOpposingBattlerType(type);
            battler = BattleAnimUtil_GetBattlerOfType(system, type);
            AddBattlerSprite(system, battler, sprites, ref count);
            battler = BattleAnimUtil_GetAlliedBattler(system, battler);
            AddBattlerSprite(system, battler, sprites, ref count);
            return;
        }
        if (BattleAnimUtil_IsMaskSet(targets, AnimNotAttacker))
        {
            int attacker = BattleAnimSystems.BattleAnimSystem_GetAttacker(system);
            int battler = BattleAnimUtil_GetAlliedBattler(system, attacker);
            if (battler != attacker)
            {
                AddBattlerSprite(system, battler, sprites, ref count);
            }
            int type = BattleAnimSystems.BattleAnimSystem_GetBattlerType(system, attacker);
            type = BattleAnimUtil_GetOpposingBattlerType(type);
            battler = BattleAnimUtil_GetBattlerOfType(system, type);
            if (battler != attacker)
            {
                AddBattlerSprite(system, battler, sprites, ref count);
            }
            battler = BattleAnimUtil_GetAlliedBattler(system, battler);
            if (battler != attacker)
            {
                AddBattlerSprite(system, battler, sprites, ref count);
            }
            return;
        }
        if (BattleAnimUtil_IsMaskSet(targets, AnimAttacker))
        {
            AddBattlerSprite(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system), sprites, ref count);
        }
        if (BattleAnimSystems.BattleAnimSystem_IsDoubleBattle(system) == 1 && BattleAnimUtil_IsMaskSet(targets, AnimAttackerPartner))
        {
            AddBattlerSprite(system, BattleAnimUtil_GetAlliedBattler(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system)), sprites, ref count);
        }
        if (BattleAnimUtil_IsMaskSet(targets, AnimDefender))
        {
            AddBattlerSprite(system, BattleAnimSystems.BattleAnimSystem_GetDefender(system), sprites, ref count);
        }
        if (BattleAnimSystems.BattleAnimSystem_IsDoubleBattle(system) == 1 && BattleAnimUtil_IsMaskSet(targets, AnimDefenderPartner))
        {
            AddBattlerSprite(system, BattleAnimUtil_GetAlliedBattler(system, BattleAnimSystems.BattleAnimSystem_GetDefender(system)), sprites, ref count);
        }
    }

    // OFFSET 0x02235D74
    public static void BattleAnimUtil_GetPokemonSprites(BattleAnimSystem system, int targets, BattleAnimSpriteInfo[] sprites, out int count)
    {
        count = 0;
        int[] masks = { AnimPokemonSprite0, AnimPokemonSprite1, AnimPokemonSprite2, AnimPokemonSprite3 };
        foreach (int mask in masks)
        {
            if (BattleAnimUtil_IsMaskSet(targets, mask))
            {
                sprites[count].HwSprite = BattleAnimSystems.BattleAnimSystem_GetPokemonSprite(system, count);
                BattleAnimUtil_GetSpritePos(sprites[count].HwSprite!, out sprites[count].X, out sprites[count].Y);
                count++;
            }
        }
    }

    // OFFSET 0x02235E50
    public static T BattleAnimUtil_Alloc<T>(BattleAnimSystem system) where T : new()
    {
        if (system == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return new T();
    }

    // OFFSET 0x02235E80
    public static void BattleAnimUtil_Free(object? ptr)
    {
        if (ptr == null)
        {
            ErrorHandling.GF_AssertFail();
        }
    }
}
