namespace PlatinumSharp.Game;

public static partial class Battlers
{
    private const int CommandStopCommandSelection = 0x2F;
    private const int NarcBattleSubscreen = 7;
    private const int NarcBattleGraphics = 0x1B;
    private const int HeapIDBattleNarc = 5;
    private const int BattlerSlideCoordY = 1;
    private const int MonAttrYPos = 1;

    // OFFSET 0x0225C684
    private static void Battler_CommandChangeWeatherForm(BattleSystem battleSys, BattlerData battler)
    {
        byte[] data = battler.Data;
        int face = (battler.BattlerType & 1) != 0 ? PokemonSpriteMetadata.FaceFront : PokemonSpriteMetadata.FaceBack;
        ushort species = (ushort)BattleIO.R16(data, 2);
        uint personality = BattleIO.R32(data, 8);
        PokemonSpriteTemplate template = new();
        PokemonSpriteTemplates.BuildPokemonSpriteTemplate(template, species, data[4], (byte)face, data[5], data[1], personality);
        PokemonSpriteTemplate spriteTemplate = PokemonSprite.PokemonSprite_GetTemplate(battler.MonSprite!);
        spriteTemplate.CopyFrom(template);
        PokemonSprite.PokemonSprite_ScheduleReloadFromNARC(battler.MonSprite!);
        byte[] tiles = BattleDisplay.PokemonSpriteData_GetTiles(BattleSystems.BattleSystem_GetPokemonSpriteData(battleSys), battler.Battler)!;
        int narcID = (int)(spriteTemplate.Word0 & 0xFFFF);
        int character = (int)(spriteTemplate.Word0 >> 16);
        int palette = (int)(spriteTemplate.Word1 & 0xFFFF);
        int spriteSpecies = (int)(spriteTemplate.Word1 >> 16);
        CharacterSprite.CharacterSprite_LoadPokemonSprite(narcID, character, HeapIDBattle, tiles, personality, false, face, spriteSpecies);
        BattleDisplay.PokemonSpriteData_SetNarcID(BattleSystems.BattleSystem_GetPokemonSpriteData(battleSys), battler.Battler, narcID);
        BattleDisplay.PokemonSpriteData_SetPalette(BattleSystems.BattleSystem_GetPokemonSpriteData(battleSys), battler.Battler, palette);
        int yOffset = PokemonSpriteMetadata.Pokemon_LoadSpriteYOffset(species, data[4], (byte)face, data[1], personality);
        BattleDisplay.PokemonSpriteData_SetYOffset(BattleSystems.BattleSystem_GetPokemonSpriteData(battleSys), battler.Battler, yOffset);
        PokemonSprite.PokemonSprite_SetAttribute(battler.MonSprite!, MonAttrYPos, yOffset + BattlerSlide.BattlerSlide_GetCoord(battler.BattlerType, BattlerSlideCoordY));
        BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, data[0]);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C79C
    private static void Battler_CommandStopCommandSelection(BattleSystem battleSys, BattlerData battler)
    {
        if (battler.BootState == BootStatePlayer)
        {
            NARC subscreenNarc = NARC.NARC_ctor(NarcBattleSubscreen, HeapIDBattleNarc);
            NARC battleNarc = NARC.NARC_ctor(NarcBattleGraphics, HeapIDBattleNarc);
            BattleSubscreen subscreen = BattleSystems.BattleSystem_GetBattleSubscreen(battleSys)!;
            BattleSubscreens.BattleSubscreen_LoadMenu(subscreenNarc, battleNarc, subscreen, 0, 0, null);
            BattleSubscreens.BattleSubscreen_SetCommandMenuActive(subscreen, 0);
            NARC.NARC_dtor(subscreenNarc);
            NARC.NARC_dtor(battleNarc);
            int partner = BattleSystems.BattleSystem_GetPartner(battleSys, battler.Battler);
            if (partner != battler.Battler)
            {
                Healthboxes.Healthbox_OnPartnerCommandEnd(BattleSystems.BattleSystem_GetHealthbox(battleSys, partner));
            }
            Healthboxes.Healthbox_StopArrowAnim(battler.Healthbox);
            BattleSubscreens.BattleSubscreen_HidePartyBalls(subscreen);
            Battler_StopIdleBob(battler);
        }
        BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, CommandStopCommandSelection);
        Battler_ClearCommand(battler);
    }
}

public sealed class SetMosaicTask
{
    public BattleSystem? BattleSys;
    public PokemonSprite? Sprite;
    public byte Command;
    public byte Battler;
    public byte State;
    public byte TargetMosaic;
    public byte Timer;
    public byte Delay;
}

public static partial class Battlers
{
    // OFFSET 0x0225C670
    private static void Battler_CommandSetMosaic(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_SetMosaic(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }
}

public static partial class BattleDisplay
{
    private const int MonAttrMosaic = 0x28;

    // OFFSET 0x0225E0F4
    public static void BattleDisplay_SetMosaic(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        SetMosaicTask task = new();
        task.State = 0;
        task.BattleSys = battleSys;
        task.Sprite = battler.MonSprite;
        task.Command = data[0];
        task.Battler = battler.Battler;
        task.TargetMosaic = data[1];
        task.Timer = 0;
        task.Delay = data[2];
        SysTask.SysTask_Start(BattleDisplay_SetMosaicTask, task, 0);
    }

    // OFFSET 0x022634DC
    private static void BattleDisplay_SetMosaicTask(SysTask sysTask, object? param)
    {
        SetMosaicTask task = (SetMosaicTask)param!;
        switch (task.State)
        {
            case 0:
                if (task.Timer != task.Delay)
                {
                    task.Timer++;
                    break;
                }
                task.Timer = 0;
                int mosaic = PokemonSprite.PokemonSprite_GetAttribute(task.Sprite!, MonAttrMosaic);
                if (mosaic > task.TargetMosaic)
                {
                    mosaic--;
                }
                else if (mosaic < task.TargetMosaic)
                {
                    mosaic++;
                }
                PokemonSprite.PokemonSprite_SetAttribute(task.Sprite!, MonAttrMosaic, mosaic);
                if (mosaic == task.TargetMosaic)
                {
                    task.State++;
                }
                break;
            case 1:
                BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
                SysTask.SysTask_Done(sysTask);
                break;
        }
    }
}
