namespace PlatinumSharp.Game;

public static class OverlayApplications
{
    private const uint GameOpeningTemplate = 0x021D788C;
    private const uint TitleScreenTemplate = 0x021D742C;
    private const uint GameStartContinueTemplate = 0x021D0F70;
    private const uint GameStartNewGameTemplate = 0x021D0F80;
    private const uint GameStartNewSaveTemplate = 0x021D0F90;
    private const uint SaveStatusScreenTemplate = 0x020F8AB4;
    private const uint MainMenuTemplate = 0x0223D674;
    private const uint RowanIntroTemplate = 0x020F6824;
    private const uint RowanIntroTvTemplate = 0x021D3820;
    private const uint NamingScreenTemplate = 0x020F2DAC;
    private const uint FieldSystemNewGameTemplate = 0x020EA10C;
    private const uint FieldSystemContinueTemplate = 0x020EA11C;
    private const uint FieldMapTemplate = 0x021F89B0;

    public static void Resolve(ApplicationTemplate template)
    {
        switch (template.Address)
        {
            case FieldSystemChildProcesses.BattleTemplate:
                template.Init = FieldSystemChildProcesses.FieldBattle_Init;
                template.Main = FieldSystemChildProcesses.FieldBattle_Main;
                template.Exit = FieldSystemChildProcesses.FieldBattle_Exit;
                template.OverlayID = -1;
                break;
            case FieldMapTemplate:
                template.Init = FieldMap.FieldMap_Init;
                template.Main = FieldMap.FieldMap_Main;
                template.Exit = FieldMap.FieldMap_Exit;
                template.OverlayID = -1;
                break;
            case FieldSystemNewGameTemplate:
                template.Init = FieldSystemCore.FieldSystem_InitNewGame;
                template.Main = FieldSystemCore.FieldSystem_Main;
                template.Exit = FieldSystemCore.FieldSystem_Exit;
                template.OverlayID = -1;
                break;
            case FieldSystemContinueTemplate:
                template.Init = FieldSystemCore.FieldSystem_InitContinue;
                template.Main = FieldSystemCore.FieldSystem_Main;
                template.Exit = FieldSystemCore.FieldSystem_Exit;
                template.OverlayID = -1;
                break;
            case NamingScreenTemplate:
                template.Init = NamingScreen.NamingScreen_Init;
                template.Main = NamingScreen.NamingScreen_Main;
                template.Exit = NamingScreen.NamingScreen_Exit;
                template.OverlayID = -1;
                break;
            case RowanIntroTemplate:
                template.Init = RowanIntro.RowanIntro_Init;
                template.Main = RowanIntro.RowanIntro_Main;
                template.Exit = RowanIntro.RowanIntro_Exit;
                template.OverlayID = -1;
                break;
            case RowanIntroTvTemplate:
                template.Init = RowanIntroTv.RowanIntroTv_Init;
                template.Main = RowanIntroTv.RowanIntroTv_Main;
                template.Exit = RowanIntroTv.RowanIntroTv_Exit;
                template.OverlayID = -1;
                break;
            case GameOpeningTemplate:
                template.Init = GameOpening.GameOpening_Init;
                template.Main = GameOpening.GameOpening_Main;
                template.Exit = GameOpening.GameOpening_Exit;
                template.OverlayID = -1;
                break;
            case SaveStatusScreenTemplate:
                template.Init = SaveStatusScreen.SaveStatusScreen_Init;
                template.Main = SaveStatusScreen.SaveStatusScreen_Main;
                template.Exit = SaveStatusScreen.SaveStatusScreen_Exit;
                template.OverlayID = -1;
                break;
            case GameStartContinueTemplate:
                template.Init = GameStart.GameStartContinue_Init;
                template.Main = GameStart.GameStartContinue_Main;
                template.Exit = GameStart.GameStartContinue_Exit;
                template.OverlayID = -1;
                break;
            case GameStartNewGameTemplate:
                template.Init = GameStart.GameStartNewGame_Init;
                template.Main = GameStart.GameStartNewGame_Main;
                template.Exit = GameStart.GameStartNewGame_Exit;
                template.OverlayID = -1;
                break;
            case GameStartNewSaveTemplate:
                template.Init = GameStart.GameStartNewSave_Init;
                template.Main = GameStart.GameStartNewSave_Main;
                template.Exit = GameStart.GameStartNewSave_Exit;
                template.OverlayID = -1;
                break;
            case MainMenuTemplate:
                template.Init = MainMenu.MainMenu_Init;
                template.Main = MainMenu.MainMenu_Main;
                template.Exit = MainMenu.MainMenu_Exit;
                template.OverlayID = -1;
                break;
            case ChooseStarter.Template:
                template.Init = ChooseStarter.ChooseStarter_Init;
                template.Main = ChooseStarter.ChooseStarter_Main;
                template.Exit = ChooseStarter.ChooseStarter_Exit;
                template.OverlayID = -1;
                break;
            case TitleScreenTemplate:
                template.Init = TitleScreen.TitleScreen_Init;
                template.Main = TitleScreen.TitleScreen_Main;
                template.Exit = TitleScreen.TitleScreen_Exit;
                template.OverlayID = -1;
                break;
        }
    }
}
