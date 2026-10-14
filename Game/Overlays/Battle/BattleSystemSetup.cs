using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static partial class Battle
{
    private const int BattleContextParamSelectedPartySlot = 2;
    private const int MonDataPokeball = 0x9B;
    private const int NetworkStrengthMax = 3;

    // OFFSET 0x0223C210
    public static void BattleSystem_InitBattlerSprites(BattleSystem battleSys)
    {
        int terrain = BattleSystems.BattleSystem_GetTerrain(battleSys);
        BattleTerrainSprites.TerrainSprite_Init(battleSys.Terrains[0], battleSys, BattleTerrainSprites.SidePlayer, terrain);
        BattleTerrainSprites.TerrainSprite_Init(battleSys.Terrains[1], battleSys, BattleTerrainSprites.SideEnemy, terrain);
        BattleContext battleCtx = BattleSystems.BattleSystem_GetBattleContext(battleSys)!;
        for (int i = 0; i < battleSys.MaxBattlers; i++)
        {
            int slot = BattleContexts.BattleContext_Get(battleSys, battleCtx, BattleContextParamSelectedPartySlot, i);
            Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, i, slot);
            int ball = (int)Pokemon.Pokemon_GetValue(mon, MonDataPokeball, null);
            Battlers.Battler_InitBallRotation(battleSys, battleSys.Battlers[i]!, ball, slot);
        }
        BattleSystem_InitNetworkIcon();
    }

    // OFFSET 0x0223DECC
    public static void BattleSystem_InitNetworkIcon()
    {
        NetworkIcon.NetworkIcon_Init();
        if (CommManager.CommMan_IsWiFi() != 0)
        {
            NetworkIcon.NetworkIcon_SetStrength(NetworkStrengthMax - ov4_021D7EAC());
        }
        else if (WirelessManager.WirelessMan_IsInitialized())
        {
            throw Partial(0x0223DECC);
        }
    }
}

public static partial class Battlers
{
    private const uint BattleTypeDoubles = 1;
    private const uint BattleTypeNoBallRotation = 0x220;
    private const uint BallRotationTypeTable = 0x0226F174;
    private const int BallRotationPriority = 100;
    private const int BallRotationExplicitPriority = 2;
    private const int HeapIDBattlerBall = 5;

    // OFFSET 0x0225C038
    public static void Battler_InitBallRotation(BattleSystem battleSys, BattlerData battler, int ball, int slot)
    {
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeNoBallRotation) != 0)
        {
            return;
        }
        if ((battler.BattlerType & 1) != 0 && (BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeDoubles) == 0)
        {
            return;
        }
        BallThrow template = new();
        template.Type = RomData.Overlay(16, BallRotationTypeTable + battler.BattlerType, 1)[0];
        template.HeapID = HeapIDBattlerBall;
        template.Mode = BallRotations.ModeHold;
        template.Target = battler.Battler;
        template.BallID = ball;
        template.SpriteSys = BattleSystems.BattleSystem_GetSpriteSystem(battleSys);
        template.PaletteData = BattleSystems.BattleSystem_GetPaletteData(battleSys);
        template.Surface = 0;
        template.BgPrio = 1;
        battler.BallRotation = BallRotations.BallRotation_New(template);
        BallRotations.BallRotation_SetPriority(battler.BallRotation, BallRotationPriority);
        BallRotations.BallRotation_SetExplicitPriority(battler.BallRotation, BallRotationExplicitPriority);
        BallRotations.BallRotation_SetDrawFlag(battler.BallRotation, 0);
        BallRotations.BallRotation_SetTickAnimation(battler.BallRotation, 0);
    }
}
