namespace PlatinumSharp.Game;

public static partial class Battlers
{
    // OFFSET 0x0225C138
    private static void Battler_CommandNone(BattleSystem battleSys, BattlerData battler)
    {
    }

    // OFFSET 0x0225C13C
    private static void Battler_CommandSetupBattleUI(BattleSystem battleSys, BattlerData battler)
    {
        BattleSystems.BattleSystem_SetSeedDTO(battleSys, BattleIO.R32(battler.Data, 4));
        BattleDisplay.BattleDisplay_StartIntro(battleSys, battler);
        BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, BattleIO.CommandSetupBattleUI);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C17C
    private static void Battler_CommandPokemonSendOut(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_PokemonSendOut(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C1F0
    private static void Battler_CommandTrainerEncounter(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_TrainerEncounter(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C204
    private static void Battler_CommandThrowTrainerBall(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_ThrowTrainerBall(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C23C
    private static void Battler_CommandSlideHealthboxIn(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_SlideHealthboxIn(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C250
    private static void Battler_CommandSlideHealthboxOut(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_SlideHealthboxOut(battleSys, battler);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C420
    private static void Battler_CommandEncounterMessage(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_PrintEncounterMessage(battleSys, battler);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C430
    private static void Battler_CommandSendOutMessage(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_PrintSendOutMessage(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C840
    private static void Battler_CommandShowPartyGaugeAtStart(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_ShowPartyGaugeAtStart(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C854
    private static void Battler_CommandHidePartyGaugeAtStart(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_HidePartyGaugeAtStart(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C868
    private static void Battler_CommandShowPartyGauge(BattleSystem battleSys, BattlerData battler)
    {
        if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler.Battler) != 0)
        {
            BattleDisplay.BattleDisplay_ShowPartyGauge(battleSys, battler, battler.Data);
        }
        else
        {
            BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, BattleIO.CommandShowPartyGauge);
        }
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C8A4
    private static void Battler_CommandHidePartyGauge(BattleSystem battleSys, BattlerData battler)
    {
        if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler.Battler) != 0)
        {
            BattleDisplay.BattleDisplay_HidePartyGauge(battleSys, battler, battler.Data);
        }
        else
        {
            BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, BattleIO.CommandHidePartyGauge);
        }
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C8E0
    private static void Battler_CommandLoadPartyGaugeGraphics(BattleSystem battleSys, BattlerData battler)
    {
        SpriteSystem spriteSys = BattleSystems.BattleSystem_GetSpriteSystem(battleSys)!;
        SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(battleSys)!;
        PaletteData paletteData = BattleSystems.BattleSystem_GetPaletteData(battleSys)!;
        PartyGauges.PartyGauge_LoadGraphics(spriteSys, spriteMan, paletteData);
        BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, BattleIO.CommandLoadPartyGaugeGraphics);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C91C
    private static void Battler_CommandFreePartyGaugeGraphics(BattleSystem battleSys, BattlerData battler)
    {
        PartyGauges.PartyGauge_FreeGraphics(BattleSystems.BattleSystem_GetSpriteManager(battleSys)!);
        BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, BattleIO.CommandFreePartyGaugeGraphics);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225CBB0
    private static void Battler_ClearCommand(BattlerData battler)
    {
        battler.Data[0] = 0;
    }
}
