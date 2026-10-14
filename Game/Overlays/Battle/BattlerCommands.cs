using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static partial class Battlers
{
    private static readonly Action<BattleSystem, BattlerData>[] CommandFuncs =
    {
        Battler_CommandNone,
        Battler_CommandSetupBattleUI,
        Battler_CommandPokemonEncounter,
        Battler_CommandPokemonSendOut,
        Battler_CommandShowPokemon,
        Battler_CommandReturnPokemon,
        Battler_CommandOpenCaptureBall,
        Battler_CommandDeleteMonSprite,
        Battler_CommandTrainerEncounter,
        Battler_CommandThrowTrainerBall,
        Battler_CommandSlideTrainerOut,
        Battler_CommandSlideTrainerIn,
        Battler_CommandSlideHealthboxIn,
        Battler_CommandSlideHealthboxOut,
        Battler_CommandSetCommandSelection,
        Battler_CommandShowMoveSelectMenu,
        Battler_CommandShowTargetSelectMenu,
        Battler_CommandShowBagMenu,
        Battler_CommandShowPartyMenu,
        Battler_CommandShowYesNoMenu,
        Battler_CommandPrintAttackMessage,
        Battler_CommandPrintMessage,
        Battler_CommandSetMoveAnimation,
        Battler_CommandFlickerBattler,
        Battler_CommandUpdateHPGauge,
        Battler_CommandUpdateExpGauge,
        Battler_CommandPlayFaintingSequence,
        Battler_CommandPlaySound,
        Battler_CommandFadeOut,
        Battler_CommandToggleVanish,
        Battler_CommandSetStatusIcon,
        Battler_CommandPrintTrainerMessage,
        Battler_CommandPrintRecallMessage,
        Battler_CommandPrintSwitchInMessage,
        Battler_CommandEncounterMessage,
        Battler_CommandSendOutMessage,
        Battler_CommandPlayLevelUpAnimation,
        Battler_CommandAlertMessage,
        Battler_CommandRefreshHPGauge,
        Battler_CommandUpdatePartyMon,
        Battler_CommandSlidePartyGauge,
        Battler_CommandStopCommandIdle,
        Battler_CommandCurePartyStatus,
        Battler_CommandForgetMove,
        Battler_CommandSetMosaic,
        Battler_CommandChangeWeatherForm,
        Battler_CommandBakeTerrain,
        Battler_CommandStopCommandSelection,
        Battler_CommandShowPartyGaugeAtStart,
        Battler_CommandHidePartyGaugeAtStart,
        Battler_CommandShowPartyGauge,
        Battler_CommandHidePartyGauge,
        Battler_CommandLoadPartyGaugeGraphics,
        Battler_CommandFreePartyGaugeGraphics,
        Battler_CommandIncrementRecord,
        Battler_CommandLinkWaitMessage,
        Battler_CommandRestoreSprite,
        Battler_CommandSpriteToOAM,
        Battler_CommandOAMToSprite,
        Battler_CommandPrintResultMessage,
        Battler_CommandPrintEscapeMessage,
        Battler_CommandPrintForfeitMessage,
        Battler_CommandRefreshSprite,
        Battler_CommandPlayMoveHitSound,
        Battler_CommandPlayMusic,
        Battler_CommandSetLinkBattleResult,
        Battler_CommandClearMessageBox,
    };

    // OFFSET 0x0225C0DC
    public static void Battler_Update(BattleSystem battleSys, BattlerData battler)
    {
        if (battler.Data[0] != 0)
        {
            battler.MsgPending = 0;
            CommandFuncs[battler.Data[0]](battleSys, battler);
        }
    }
}
