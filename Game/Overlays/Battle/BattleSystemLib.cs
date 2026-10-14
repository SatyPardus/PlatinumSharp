namespace PlatinumSharp.Game;

public static partial class BattleSystems
{
    public const uint BattleTypeTrainer = 1;
    public const uint BattleTypeDoubles = 2;
    public const uint BattleTypeLink = 4;
    public const uint BattleTypeMulti = 8;
    public const uint BattleTypeTag = 0x10;
    public const uint BattleTypeSafari = 0x20;
    public const uint BattleTypeFrontier = 0x80;
    public const uint BattleTypePalPark = 0x200;
    public const uint BattleTypeCatchTutorial = 0x400;
    public const uint BattleStatusRecording = 0x10;
    public const int TerrainMax = 0x18;
    public const int BattlerTypeEnemyFlag = 1;
    public const int BattlerTypeSecondSlotFlag = 2;

    private const uint RandMultiplier = 0x41C64E6D;
    private const uint RandIncrement = 0x6073;

    private static bool BattleSystem_UsesOwnPartySlot(BattleSystem battleSys, int battler)
    {
        uint battleType = battleSys.BattleType;
        return (battleType & BattleTypeMulti) != 0 || ((battleType & BattleTypeTag) != 0 && (BattleSystem_BattlerSlot(battleSys, battler) & BattlerTypeEnemyFlag) != 0);
    }

    private static int BattleSystem_OwnerIndex(BattleSystem battleSys, int battler)
    {
        if (BattleSystem_UsesOwnPartySlot(battleSys, battler))
        {
            return battler;
        }
        if ((battleSys.BattleType & BattleTypeDoubles) != 0)
        {
            return battler & 1;
        }
        return battler;
    }

    // OFFSET 0x0223DD4C
    public static void BattleSystem_SetTextRenderFlags(BattleSystem battleSys)
    {
        if ((battleSys.BattleType & (BattleTypeLink | BattleTypeCatchTutorial)) != 0 || (battleSys.BattleStatusMask & BattleStatusRecording) != 0)
        {
            RenderText.RenderControlFlags_SetCanABSpeedUpPrint(1);
            RenderText.RenderControlFlags_SetAutoScrollFlags(1);
            RenderText.RenderControlFlags_SetSpeedUpOnTouch(0);
        }
        else
        {
            RenderText.RenderControlFlags_SetCanABSpeedUpPrint(3);
            RenderText.RenderControlFlags_SetAutoScrollFlags(1);
            RenderText.RenderControlFlags_SetSpeedUpOnTouch(1);
        }
    }

    // OFFSET 0x0223DF00
    public static BgConfig? BattleSystem_GetBgConfig(BattleSystem battleSys)
    {
        return battleSys.BgConfig;
    }

    // OFFSET 0x0223DF04
    public static Window BattleSystem_GetWindow(BattleSystem battleSys, int index)
    {
        return battleSys.Windows![index];
    }

    // OFFSET 0x0223DF0C
    public static uint BattleSystem_GetBattleType(BattleSystem battleSys)
    {
        return battleSys.BattleType;
    }

    // OFFSET 0x0223DF10
    public static BattleContext? BattleSystem_GetBattleContext(BattleSystem battleSys)
    {
        return battleSys.BattleCtx;
    }

    // OFFSET 0x0223DF14
    public static BattlerData? BattleSystem_GetBattlerData(BattleSystem battleSys, int battler)
    {
        return battleSys.Battlers[battler];
    }

    // OFFSET 0x0223DF1C
    public static int BattleSystem_GetMaxBattlers(BattleSystem battleSys)
    {
        return battleSys.MaxBattlers;
    }

    // OFFSET 0x0223DF20
    public static Party BattleSystem_GetParty(BattleSystem battleSys, int battler)
    {
        return battleSys.Parties[BattleSystem_OwnerIndex(battleSys, battler)]!;
    }

    // OFFSET 0x0223DF60
    public static int BattleSystem_GetPartyCount(BattleSystem battleSys, int battler)
    {
        return Party.Party_GetCurrentCount(battleSys.Parties[BattleSystem_OwnerIndex(battleSys, battler)]!);
    }

    // OFFSET 0x0223DFAC
    public static Pokemon BattleSystem_GetPartyPokemon(BattleSystem battleSys, int battler, int slot)
    {
        return Party.Party_GetPokemonBySlotIndex(battleSys.Parties[BattleSystem_OwnerIndex(battleSys, battler)]!, slot);
    }

    // OFFSET 0x0223E000
    public static PokemonSpriteManager? BattleSystem_GetPokemonSpriteManager(BattleSystem battleSys)
    {
        return battleSys.MonSpriteMan;
    }

    // OFFSET 0x0223E008
    public static BattleAnimSystem? BattleSystem_GetBattleAnimSystem(BattleSystem battleSys)
    {
        return battleSys.BattleAnimSys;
    }

    // OFFSET 0x0223E010
    public static SpriteSystem? BattleSystem_GetSpriteSystem(BattleSystem battleSys)
    {
        return battleSys.SpriteSys;
    }

    // OFFSET 0x0223E018
    public static SpriteManager? BattleSystem_GetSpriteManager(BattleSystem battleSys)
    {
        return battleSys.SpriteMan;
    }

    // OFFSET 0x0223E020
    public static Terrain BattleSystem_GetTerrainSprite(BattleSystem battleSys, int side)
    {
        return battleSys.Terrains[side];
    }

    // OFFSET 0x0223E02C
    public static BattleSubscreen? BattleSystem_GetBattleSubscreen(BattleSystem battleSys)
    {
        return battleSys.BtlSubscreen;
    }

    // OFFSET 0x0223E034
    public static PartyGauge? BattleSystem_GetPartyGauge(BattleSystem battleSys, int side)
    {
        return battleSys.PartyGauges[side];
    }

    // OFFSET 0x0223E040
    public static void BattleSystem_SetPartyGauge(BattleSystem battleSys, int side, PartyGauge? partyGauge)
    {
        battleSys.PartyGauges[side] = partyGauge;
    }

    // OFFSET 0x0223E04C
    public static FontSpecialCharsContext? BattleSystem_GetSpecialCharsHP(BattleSystem battleSys)
    {
        return battleSys.SpecialCharsHP;
    }

    // OFFSET 0x0223E054
    public static FontSpecialCharsContext? BattleSystem_GetSpecialCharsLevel(BattleSystem battleSys)
    {
        return battleSys.SpecialCharsLevel;
    }

    // OFFSET 0x0223E05C
    public static MessageLoader? BattleSystem_GetMessageLoader(BattleSystem battleSys)
    {
        return battleSys.MsgLoader;
    }

    // OFFSET 0x0223E060
    public static MessageLoader? BattleSystem_GetAttackMessageLoader(BattleSystem battleSys)
    {
        return battleSys.AttackMsgLoader;
    }

    // OFFSET 0x0223E064
    public static PaletteData? BattleSystem_GetPaletteData(BattleSystem battleSys)
    {
        return battleSys.PaletteData;
    }

    // OFFSET 0x0223E068
    public static Pokedex? BattleSystem_GetPokedex(BattleSystem battleSys)
    {
        return battleSys.Pokedex;
    }

    // OFFSET 0x0223E06C
    public static byte[] BattleSystem_GetServerMessage(BattleSystem battleSys)
    {
        return battleSys.ServerMessage;
    }

    // OFFSET 0x0223E074
    public static byte[] BattleSystem_GetClientMessage(BattleSystem battleSys)
    {
        return battleSys.ClientMessage;
    }

    // OFFSET 0x0223E080
    public static ref ushort BattleSystem_GetServerReadIndex(BattleSystem battleSys)
    {
        return ref battleSys.ServerReadIndex;
    }

    // OFFSET 0x0223E08C
    public static ref ushort BattleSystem_GetServerWriteIndex(BattleSystem battleSys)
    {
        return ref battleSys.ServerWriteIndex;
    }

    // OFFSET 0x0223E098
    public static ref ushort BattleSystem_GetServerEndIndex(BattleSystem battleSys)
    {
        return ref battleSys.ServerEndIndex;
    }

    // OFFSET 0x0223E0A4
    public static ref ushort BattleSystem_GetClientReadIndex(BattleSystem battleSys)
    {
        return ref battleSys.ClientReadIndex;
    }

    // OFFSET 0x0223E0B0
    public static ref ushort BattleSystem_GetClientWriteIndex(BattleSystem battleSys)
    {
        return ref battleSys.ClientWriteIndex;
    }

    // OFFSET 0x0223E0BC
    public static ref ushort BattleSystem_GetClientEndIndex(BattleSystem battleSys)
    {
        return ref battleSys.ClientEndIndex;
    }

    // OFFSET 0x0223E0C8
    public static PokemonSpriteData[] BattleSystem_GetPokemonSpriteData(BattleSystem battleSys)
    {
        return battleSys.PokemonSpriteDataArray;
    }

    // OFFSET 0x0223E0D0
    public static StringTemplate? BattleSystem_GetStringTemplate(BattleSystem battleSys)
    {
        return battleSys.StrFormatter;
    }

    // OFFSET 0x0223E0D4
    public static GameString? BattleSystem_GetMessageBuffer(BattleSystem battleSys)
    {
        return battleSys.MsgBuffer;
    }

    // OFFSET 0x0223E0D8
    public static ushort BattleSystem_GetTrainerID(BattleSystem battleSys, int battler)
    {
        return battleSys.TrainerIDs[BattleSystem_OwnerIndex(battleSys, battler)];
    }

    // OFFSET 0x0223E120
    public static byte[] BattleSystem_GetTrainer(BattleSystem battleSys, int battler)
    {
        return battleSys.Trainers[BattleSystem_OwnerIndex(battleSys, battler)];
    }

    // OFFSET 0x0223E16C
    public static TrainerInfo BattleSystem_GetTrainerInfo(BattleSystem battleSys, int battler)
    {
        return battleSys.TrainerInfos[BattleSystem_OwnerIndex(battleSys, battler)]!;
    }

    // OFFSET 0x0223E1AC
    public static Bag? BattleSystem_GetBag(BattleSystem battleSys)
    {
        return battleSys.Bag;
    }

    // OFFSET 0x0223E1B0
    public static BagCursor? BattleSystem_GetBagCursor(BattleSystem battleSys)
    {
        return battleSys.BagCursor;
    }

    // OFFSET 0x0223E1B4
    public static uint BattleSystem_GetTrainerGender(BattleSystem battleSys, int battler)
    {
        return TrainerInfo.TrainerInfo_Gender(battleSys.TrainerInfos[battler]!);
    }

    // OFFSET 0x0223E1C4
    public static int BattleSystem_GetBattlerOfType(BattleSystem battleSys, int battlerType)
    {
        int battler;
        for (battler = 0; battler < battleSys.MaxBattlers; battler++)
        {
            if (battlerType == Battlers.Battler_GetType(battleSys.Battlers[battler]!))
            {
                break;
            }
        }
        if (battler >= battleSys.MaxBattlers)
        {
            ErrorHandling.GF_AssertFail();
        }
        return battler;
    }

    // OFFSET 0x0223E1F8
    public static byte BattleSystem_BattlerSlot(BattleSystem battleSys, int battler)
    {
        return Battlers.Battler_GetType(battleSys.Battlers[battler]!);
    }

    // OFFSET 0x0223E208
    public static byte BattleSystem_GetBattlerSide(BattleSystem battleSys, int battler)
    {
        return (byte)(Battlers.Battler_GetType(battleSys.Battlers[battler]!) & BattlerTypeEnemyFlag);
    }

    // OFFSET 0x0223E220
    public static PaletteAnimator? BattleSystem_GetPaletteAnimator(BattleSystem battleSys)
    {
        return battleSys.PaletteAnimator;
    }

    // OFFSET 0x0223E228
    public static PCBoxes? BattleSystem_GetPCBoxes(BattleSystem battleSys)
    {
        return battleSys.PcBoxes;
    }

    // OFFSET 0x0223E22C
    public static int BattleSystem_GetTerrain(BattleSystem battleSys)
    {
        int terrain = battleSys.BattleTerrain;
        if (terrain > TerrainMax || terrain < 0)
        {
            return TerrainMax;
        }
        return terrain;
    }

    // OFFSET 0x0223E240
    public static int BattleSystem_GetBackground(BattleSystem battleSys)
    {
        return battleSys.Background;
    }

    // OFFSET 0x0223E24C
    public static int BattleSystem_GetMapLabelTextID(BattleSystem battleSys)
    {
        return battleSys.MapLabelTextID;
    }

    // OFFSET 0x0223E258
    public static int BattleSystem_GetPartner(BattleSystem battleSys, int battler)
    {
        int maxBattlers = BattleSystem_GetMaxBattlers(battleSys);
        if ((BattleSystem_GetBattleType(battleSys) & BattleTypeDoubles) == 0)
        {
            return battler;
        }
        int partner;
        for (partner = 0; partner < maxBattlers; partner++)
        {
            if (partner != battler && BattleSystem_GetBattlerSide(battleSys, partner) == BattleSystem_GetBattlerSide(battleSys, battler))
            {
                break;
            }
        }
        return partner;
    }

    // OFFSET 0x0223E2A4
    public static int BattleSystem_GetEnemyInSlot(BattleSystem battleSys, int attacker, int slot)
    {
        int maxBattlers = BattleSystem_GetMaxBattlers(battleSys);
        if ((BattleSystem_GetBattleType(battleSys) & BattleTypeDoubles) == 0)
        {
            return attacker ^ 1;
        }
        int battler;
        for (battler = 0; battler < maxBattlers; battler++)
        {
            if (battler != attacker && (BattleSystem_BattlerSlot(battleSys, battler) & BattlerTypeSecondSlotFlag) == slot && BattleSystem_GetBattlerSide(battleSys, battler) != BattleSystem_GetBattlerSide(battleSys, attacker))
            {
                break;
            }
        }
        return battler;
    }

    // OFFSET 0x0223F4BC
    public static ushort BattleSystem_RandNext(BattleSystem battleSys)
    {
        battleSys.SeedRandNext = battleSys.SeedRandNext * RandMultiplier + RandIncrement;
        return (ushort)(battleSys.SeedRandNext >> 16);
    }

    // OFFSET 0x0223F700
    public static ushort BattleSystem_GetTrainerItem(BattleSystem battleSys, int battler, int slot)
    {
        return (ushort)Nitro.Bin.U16(battleSys.Trainers[battler], TrainerData.ItemsOffset + slot * 2);
    }
}

public static partial class Battlers
{
    // OFFSET 0x02263AE4
    public static byte Battler_GetType(BattlerData battler)
    {
        return battler.BattlerType;
    }

    // OFFSET 0x02263AF0
    public static byte Battler_GetBootState(BattlerData battler)
    {
        return battler.BootState;
    }
}

public static partial class BattleContexts
{
    private const int HeapIDBattle = 5;
    private const int NoBattler = 0xFF;
    private const int NoPartySlot = 6;
    private const uint AIScriptTableAddress = 0x022248A4;
    private const uint BattleTypeNoAITrainerItems = 0x2E4;
    private const uint BattleStatusPersistentMask = 0xFF800000;
    private const uint BattleStatus2PersistentMask = 0xFFFFFEA1;
    private const int TrainerItemSlots = 4;
    private const int SafariStageDefault = 6;

    // OFFSET 0x0224B940
    public static BattleContext BattleContext_New(BattleSystem battleSys)
    {
        BattleContext battleCtx = new();
        BattleContext_InitCounters(battleCtx);
        BattleContext_Init(battleSys, battleCtx);
        BattleAI_Init(battleSys, battleCtx);
        MoveTable.MoveTable_Load(battleCtx.AiContext.MoveTable);
        battleCtx.AiContext.ItemTable = Item.Item_LoadTable(HeapIDBattle);
        return battleCtx;
    }

    // OFFSET 0x02251604
    public static void BattleAI_Init(BattleSystem battleSys, BattleContext battleCtx)
    {
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        AIContext ai = battleCtx.AiContext;
        Array.Clear(ai.TrainerItems);
        Array.Clear(ai.TrainerItemCounts);
        if ((battleType & BattleSystems.BattleTypeTrainer) != 0 && (battleType & BattleTypeNoAITrainerItems) == 0)
        {
            for (int battler = 0; battler < BattleSystem.MaxBattlerSlots; battler++)
            {
                if ((battler & 1) == 0)
                {
                    continue;
                }
                int side = battler >> 1;
                for (int slot = 0; slot < TrainerItemSlots; slot++)
                {
                    ushort item = BattleSystems.BattleSystem_GetTrainerItem(battleSys, battler, slot);
                    if (item != 0)
                    {
                        ai.TrainerItems[side, ai.TrainerItemCounts[side]] = item;
                        ai.TrainerItemCounts[side]++;
                    }
                }
            }
        }
        battleCtx.AiScriptTempAddress = AIScriptTableAddress;
    }

    // OFFSET 0x022541C4
    public static void BattleContext_InitCounters(BattleContext battleCtx)
    {
        battleCtx.Damage = 0;
        battleCtx.CriticalMul = 1;
        battleCtx.CriticalBoosts = 0;
        battleCtx.MovePower = 0;
        battleCtx.PowerMul = 10;
        battleCtx.MoveType = 0;
        battleCtx.MoveEffectChance = 0;
        battleCtx.MoveStatusFlags = 0;
        battleCtx.FaintedMon = NoBattler;
        battleCtx.SideEffectDirectFlags = 0;
        battleCtx.SideEffectIndirectFlags = 0;
        battleCtx.SideEffectAbilityFlags = 0;
        battleCtx.SideEffectType = 0;
        battleCtx.SideEffectParam = 0;
        battleCtx.SideEffectMon = NoBattler;
        battleCtx.MultiHitCounter = 0;
        battleCtx.MultiHitNumHits = 0;
        battleCtx.BattlerCounter = 0;
        battleCtx.MultiHitLoop = 0;
        battleCtx.AfterMoveMessageType = 0;
        battleCtx.MultiHitCheckFlags = 0;
        battleCtx.MultiHitAccuracyCheck = 0;
        battleCtx.FieldConditionCheckState = 0;
        battleCtx.MonConditionCheckState = 0;
        battleCtx.SideConditionCheckState = 0;
        battleCtx.TurnStartCheckState = 0;
        battleCtx.AfterMoveHitCheckState = 0;
        battleCtx.AfterMoveMessageState = 0;
        battleCtx.AfterMoveEffectState = 0;
        battleCtx.BeforeMoveCheckState = 0;
        battleCtx.TryMoveCheckState = 0;
        battleCtx.StatusCheckState = 0;
        battleCtx.AbilityCheckState = 0;
        battleCtx.BattleStatusMask &= BattleStatusPersistentMask;
        battleCtx.BattleStatusMask2 &= BattleStatus2PersistentMask;
        battleCtx.Magnitude = 0;
        for (int i = 0; i < BattleSystem.MaxBattlerSlots; i++)
        {
            battleCtx.SelfTurnFlags[i] = new SelfTurnFlags();
            battleCtx.AiSwitchedPartySlot[i] = NoPartySlot;
        }
    }

    // OFFSET 0x022542B8
    public static void BattleContext_Init(BattleSystem battleSys, BattleContext battleCtx)
    {
        for (int i = 0; i < BattleSystem.MaxBattlerSlots; i++)
        {
            battleCtx.MoveHitBattler[i] = NoBattler;
            battleCtx.SwitchedPartySlot[i] = NoPartySlot;
            battleCtx.SpeedRand[i] = BattleSystems.BattleSystem_RandNext(battleSys);
        }
        battleCtx.PrizeMoneyMul = 1;
        battleCtx.MeFirstTurnOrder = 1;
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeDoubles) == 0)
        {
            battleCtx.BattlersSwitchingMask |= 1 << 2;
            battleCtx.BattlersSwitchingMask |= 1 << 3;
        }
        battleCtx.SafariCatchStage = SafariStageDefault;
        battleCtx.SafariEscapeCount = SafariStageDefault;
    }
}
