namespace PlatinumSharp.Game;

public static partial class Healthboxes
{
    public const uint InfoHPGauge = 0x1;
    public const uint InfoCurrentHP = 0x2;
    public const uint InfoMaxHP = 0x4;
    public const uint InfoLevelNumber = 0x8;
    public const uint InfoName = 0x10;
    public const uint InfoExpGauge = 0x20;
    public const uint InfoGender = 0x40;
    public const uint InfoLevelText = 0x80;
    public const uint InfoStatus = 0x100;
    public const uint InfoCaughtSpecies = 0x200;
    public const uint InfoSafariBallsLabel = 0x400;
    public const uint InfoSafariBallsCount = 0x800;
    public const uint InfoParkBallsLabel = 0x1000;
    public const uint InfoParkBallsCount = 0x2000;
    public const uint InfoAll = 0xFFFFFFFF;
    public const int GaugeDone = -1;
    public const int GaugeHP = 0;
    public const int GaugeExp = 1;

    private const uint InfoSafariMask = 0xC00;
    private const uint InfoPalParkMask = 0x3000;
    private const uint InfoNotBallsMask = 0xFFFFC3FF;
    private const uint InfoBallsLabelMask = 0x1400;
    private const uint InfoBallsCountMask = 0x2800;
    private const uint BattleTypeTrainer = 0x1;
    private const int TempUninitialized = unchecked((int)0x80000000);
    private const int HPGaugeTiles = 6;
    private const int ExpGaugeTiles = 12;
    private const int ExpGaugeFirstRowTiles = 5;
    private const int ExpGaugeFirstRowOffset = 0x660;
    private const int ExpGaugeSecondRowOffset = 0xE00;
    private const int HPGaugePixels = 0x30;
    private const int TileSize = 0x20;
    private const int HalfTileSize = 0x10;
    private const int NumberBufferSize = 0x60;
    private const int NumberDigits = 3;
    private const int MaxLevel = 100;
    private const int ScrollSpeed = 0x18;
    private const int ScrollDistance = 0xA0;
    private const uint ScrollTaskPriority = 990;
    private const uint BobTaskPriority = 1010;
    private const int BobDegreesStep = 20;
    private const int FullCircleDegrees = 360;
    private const int BobAmplitude = 0x1800;
    private const int DisplayMain = 0;
    private const uint StaticTiles = 0x0226F6AC;
    private const uint NameOffsetTable = 0x0226F64C;
    private const int NameOffsetEntrySize = 0x10;
    private const uint GenderLevelOffsetTable = 0x0226F47C;
    private const uint LevelNumberOffsetTable = 0x0226F3EC;
    private const uint CurrentHPOffsetTable = 0x0226F41C;
    private const uint MaxHPOffsetTable = 0x0226F3BC;
    private const uint CaughtOffsetTable = 0x0226F38C;
    private const uint StatusOffsetTable = 0x0226F35C;
    private const uint HPGaugeOffsetTable = 0x0226F44C;
    private const uint NumberModeLeftOffsetTable = 0x0226F374;
    private const uint NumberModeRightOffsetTable = 0x0226F3A4;
    private const uint NumberModeLabelOffsetTable = 0x0226F3D4;
    private const int TileStatusNone = 0x26;
    private const int TileStatusSleep = 0x2F;
    private const int TileStatusPoison = 0x32;
    private const int TileStatusBurn = 0x35;
    private const int TileStatusFreeze = 0x2C;
    private const int TileStatusParalysis = 0x29;
    private const int TileCaught = 0x3B;
    private const int TileBlank = 0x26;
    private const int TileHPGaugeGreen = 0x2;
    private const int TileHPGaugeYellow = 0xB;
    private const int TileHPGaugeRed = 0x14;
    private const int TileExpGauge = 0x1D;
    private const int TileMaleLevel = 0x3E;
    private const int TileMaleGender = 0x4A;
    private const int TileFemaleLevel = 0x3C;
    private const int TileFemaleGender = 0x48;
    private const int TileGenderlessLevel = 0x40;
    private const int TileGenderlessGender = 0x4C;
    private const int TileHPLabelLeft = 0x42;
    private const int TileHPLabelRight = 0x44;
    private const int TileNumberLabel = 0x45;
    private const int TileNumberLeft = 0x46;
    private const int TileNumberRight = 0x47;
    private const uint MessageNickname = 0x3C4;
    private const uint MessageSafariBalls = 0x3B6;
    private const uint MessageSafariBallsLeft = 0x3B7;
    private const uint MessageParkBalls = 0x4C4;
    private const uint MessageParkBallsLeft = 0x4C5;
    private const uint TextColor = 0x000E020F;
    private const int TextNoDelay = 0xFF;
    private const byte WindowFill = 0xF;
    private const int NameWindowWidth = 8;
    private const int BallsWindowWidth = 13;
    private const int WindowHeight = 2;
    private const int NameRowSize = 0x100;
    private const int BallsRowSize = 0x1A0;
    private const int NameStringSize = 0x16;
    private const int BallsStringSize = 0x1E;

    private static int TableU16(uint table, int offset)
    {
        return Nitro.Bin.U16(RomData.Overlay(16, (uint)(table + offset), 2), 0);
    }

    private static int VramBase(HealthBox healthbox)
    {
        return G2d.GetImageLocation(Sprite.Sprite_GetImageProxy(healthbox.MainSprite!.Sprite!), VRamMain);
    }

    private static void CopyToVram(HealthBox healthbox, byte[] source, int sourceOffset, int destOffset, int size)
    {
        byte[] slice = new byte[size];
        Array.Copy(source, sourceOffset, slice, 0, size);
        Video.LoadObjectTiles(DisplayMain, slice, (uint)(VramBase(healthbox) + destOffset), (uint)size);
    }

    private static void CopyFromVram(HealthBox healthbox, int sourceOffset, byte[] dest, int destOffset, int size)
    {
        Array.Copy(Video.GetObjectTiles(DisplayMain), VramBase(healthbox) + sourceOffset, dest, destOffset, size);
    }

    // OFFSET 0x02267084
    public static void Healthbox_DrawInfo(HealthBox healthbox, int hp, uint flags)
    {
        if (healthbox.MainSprite == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (healthbox.Type == TypeSafari)
        {
            flags &= InfoSafariMask;
        }
        else if (healthbox.Type == TypePalPark)
        {
            flags &= InfoPalParkMask;
        }
        else
        {
            flags &= InfoNotBallsMask;
        }
        switch (healthbox.Type)
        {
            case TypePlayerSolo:
                flags &= ~InfoCaughtSpecies;
                break;
            case TypeEnemySolo:
            case TypeEnemySlot1:
            case TypeEnemySlot2:
                flags &= ~(InfoCurrentHP | InfoMaxHP | InfoExpGauge);
                break;
            case TypePlayerSlot1:
            case TypePlayerSlot2:
                flags &= ~(InfoExpGauge | InfoCaughtSpecies);
                if (healthbox.NumberMode == 0)
                {
                    flags &= ~(InfoCurrentHP | InfoMaxHP);
                }
                else
                {
                    flags &= ~InfoHPGauge;
                }
                break;
        }
        if ((BattleSystems.BattleSystem_GetBattleType(healthbox.BattleSys!) & BattleTypeTrainer) != 0)
        {
            flags &= ~InfoCaughtSpecies;
        }
        if ((flags & InfoHPGauge) != 0)
        {
            Healthbox_CalcHP(healthbox, 0);
            Healthbox_UpdateGauge(healthbox, GaugeHP);
        }
        if ((flags & InfoCurrentHP) != 0)
        {
            Healthbox_DrawCurrentHP(healthbox, hp);
        }
        if ((flags & InfoMaxHP) != 0)
        {
            Healthbox_DrawMaxHP(healthbox);
        }
        if ((flags & InfoLevelText) != 0 || (flags & InfoGender) != 0)
        {
            Healthbox_DrawLevelText(healthbox);
        }
        if ((flags & InfoLevelNumber) != 0)
        {
            Healthbox_DrawLevelNumber(healthbox);
        }
        if ((flags & InfoName) != 0)
        {
            Healthbox_DrawName(healthbox);
        }
        if ((flags & InfoExpGauge) != 0)
        {
            Healthbox_CalcExp(healthbox, 0);
            Healthbox_UpdateGauge(healthbox, GaugeExp);
        }
        if ((flags & InfoCaughtSpecies) != 0)
        {
            Healthbox_DrawCaughtIcon(healthbox);
        }
        if ((flags & InfoStatus) != 0)
        {
            switch (healthbox.Status)
            {
                default:
                    Healthbox_DrawStatusIcon(healthbox, TileStatusNone);
                    break;
                case 1:
                    Healthbox_DrawStatusIcon(healthbox, TileStatusSleep);
                    break;
                case 2:
                    Healthbox_DrawStatusIcon(healthbox, TileStatusPoison);
                    break;
                case 3:
                    Healthbox_DrawStatusIcon(healthbox, TileStatusBurn);
                    break;
                case 4:
                    Healthbox_DrawStatusIcon(healthbox, TileStatusFreeze);
                    break;
                case 5:
                    Healthbox_DrawStatusIcon(healthbox, TileStatusParalysis);
                    break;
            }
        }
        if ((flags & InfoBallsLabelMask) != 0)
        {
            Healthbox_DrawBallsLabel(healthbox, flags);
        }
        if ((flags & InfoBallsCountMask) != 0)
        {
            Healthbox_DrawBallsCount(healthbox, flags);
        }
    }

    // OFFSET 0x0226737C
    public static void Healthbox_ToggleNumberMode(HealthBox healthbox)
    {
        if (healthbox.Type != TypePlayerSlot1 && healthbox.Type != TypePlayerSlot2)
        {
            return;
        }
        healthbox.NumberMode ^= 1;
        int entry = healthbox.Type * 4;
        if (healthbox.NumberMode == 1)
        {
            CopyToVram(healthbox, Healthbox_StaticTiles(TileNumberLeft, TileSize), 0, TableU16(NumberModeLeftOffsetTable, entry) + TileSize, TileSize);
            CopyToVram(healthbox, Healthbox_StaticTiles(TileNumberRight, TileSize), 0, TableU16(NumberModeRightOffsetTable, entry) + TileSize, TileSize);
            int size = TableU16(NumberModeLabelOffsetTable, entry + 2);
            CopyToVram(healthbox, Healthbox_StaticTiles(TileNumberLabel, size), 0, TableU16(NumberModeLabelOffsetTable, entry), size);
            Healthbox_DrawInfo(healthbox, healthbox.CurHP, InfoCurrentHP | InfoMaxHP);
        }
        else
        {
            int size = TableU16(NumberModeLeftOffsetTable, entry + 2);
            CopyToVram(healthbox, Healthbox_StaticTiles(TileHPLabelLeft, size), 0, TableU16(NumberModeLeftOffsetTable, entry), size);
            size = TableU16(NumberModeRightOffsetTable, entry + 2);
            CopyToVram(healthbox, Healthbox_StaticTiles(TileHPLabelRight, size), 0, TableU16(NumberModeRightOffsetTable, entry), size);
            CopyToVram(healthbox, Healthbox_StaticTiles(TileBlank, TileSize), 0, TableU16(NumberModeRightOffsetTable, entry) + TileSize, TileSize);
            Healthbox_DrawInfo(healthbox, healthbox.CurHP, InfoHPGauge);
        }
    }

    // OFFSET 0x022674C4
    public static void Healthbox_CalcHP(HealthBox healthbox, int damage)
    {
        healthbox.HpTemp = TempUninitialized;
        if (healthbox.CurHP + damage < 0)
        {
            damage -= healthbox.CurHP + damage;
        }
        if (healthbox.CurHP + damage > healthbox.MaxHP)
        {
            damage -= healthbox.CurHP + damage - healthbox.MaxHP;
        }
        healthbox.Damage = -damage;
        if (healthbox.CurHP < 0)
        {
            healthbox.CurHP = 0;
        }
        if (healthbox.CurHP > healthbox.MaxHP)
        {
            healthbox.CurHP = healthbox.MaxHP;
        }
    }

    // OFFSET 0x022674F8
    public static int Healthbox_UpdateHP(HealthBox healthbox)
    {
        int result = Healthbox_UpdateGauge(healthbox, GaugeHP);
        if (result == GaugeDone)
        {
            healthbox.CurHP -= healthbox.Damage;
            Healthbox_DrawInfo(healthbox, healthbox.CurHP, InfoCurrentHP);
        }
        else
        {
            Healthbox_DrawInfo(healthbox, result, InfoCurrentHP);
        }
        return result;
    }

    // OFFSET 0x0226752C
    public static void Healthbox_CalcExp(HealthBox healthbox, int expReward)
    {
        healthbox.ExpTemp = TempUninitialized;
        if (healthbox.CurExp + expReward < 0)
        {
            expReward -= healthbox.CurExp + expReward;
        }
        if (healthbox.CurExp + expReward > healthbox.MaxExp)
        {
            expReward -= healthbox.CurExp + expReward - healthbox.MaxExp;
        }
        healthbox.ExpReward = -expReward;
        if (healthbox.CurExp < 0)
        {
            healthbox.CurExp = 0;
        }
        if (healthbox.CurExp > healthbox.MaxExp)
        {
            healthbox.CurExp = healthbox.MaxExp;
        }
    }

    // OFFSET 0x02267560
    public static int Healthbox_UpdateExp(HealthBox healthbox)
    {
        int result = Healthbox_UpdateGauge(healthbox, GaugeExp);
        if (result == GaugeDone)
        {
            healthbox.CurExp -= healthbox.ExpReward;
        }
        return result;
    }

    // OFFSET 0x0226757C
    public static void Healthbox_StartArrowAnim(HealthBox healthbox)
    {
        if (healthbox.ArrowSprite != null)
        {
            Sprite.Sprite_SetAnimateFlag(healthbox.ArrowSprite.Sprite!, 1);
            Healthbox_EnableArrow(healthbox, 1);
        }
        if ((BattleSystems.BattleSystem_GetBattleType(healthbox.BattleSys!) & BattleTypeNoArrowAnim) == 0)
        {
            Healthbox_StartBobbing(healthbox);
        }
    }

    // OFFSET 0x022675AC
    public static void Healthbox_StopArrowAnim(HealthBox healthbox)
    {
        if (healthbox.ArrowSprite != null)
        {
            Sprite.Sprite_SetAnimateFlag(healthbox.ArrowSprite.Sprite!, 0);
            Sprite.Sprite_SetAnimFrame(healthbox.ArrowSprite.Sprite!, 0);
            Healthbox_EnableArrow(healthbox, 0);
        }
        Healthbox_StopBobbing(healthbox);
    }

    // OFFSET 0x022676A8
    public static void Healthbox_Scroll(HealthBox healthbox, int direction)
    {
        if (healthbox == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (healthbox!.MainSprite == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        healthbox.DoneScrolling = 0;
        healthbox.ScrollDirection = direction & 1;
        if (direction == 0)
        {
            switch (healthbox.Type)
            {
                case TypeEnemySolo:
                case TypeEnemySlot1:
                case TypeEnemySlot2:
                    Healthbox_OffsetPositionXY(healthbox, -ScrollDistance, 0);
                    break;
                default:
                    Healthbox_OffsetPositionXY(healthbox, ScrollDistance, 0);
                    break;
            }
        }
        else
        {
            Healthbox_OffsetPositionXY(healthbox, 0, 0);
        }
        SysTask.SysTask_Start(Healthbox_ScrollTask, healthbox, ScrollTaskPriority);
    }

    // OFFSET 0x0226774C
    private static void Healthbox_ScrollTask(SysTask task, object? param)
    {
        HealthBox healthbox = (HealthBox)param!;
        int done = 0;
        SpriteTemplate template = Healthbox_GetMainTemplate(healthbox.Type)!;
        SpriteSystem.ManagedSprite_GetPositionXY(healthbox.MainSprite!, out short x, out short y);
        switch (healthbox.Type)
        {
            case TypeEnemySolo:
            case TypeEnemySlot1:
            case TypeEnemySlot2:
                if (healthbox.ScrollDirection == 0)
                {
                    x += ScrollSpeed;
                    if (x > template.X)
                    {
                        x = template.X;
                        done++;
                    }
                }
                else
                {
                    x -= ScrollSpeed;
                    if (x < template.X - ScrollSpeed)
                    {
                        x = (short)(template.X - ScrollSpeed);
                        done++;
                    }
                }
                break;
            default:
                if (healthbox.ScrollDirection == 0)
                {
                    x -= ScrollSpeed;
                    if (x < template.X)
                    {
                        x = template.X;
                        done++;
                    }
                }
                else
                {
                    x += ScrollSpeed;
                    if (x > template.X + ScrollDistance)
                    {
                        x = (short)(template.X + ScrollDistance);
                        done++;
                    }
                }
                break;
        }
        SpriteSystem.ManagedSprite_SetPositionXY(healthbox.MainSprite!, x, y);
        if (healthbox.ArrowSprite != null)
        {
            SpriteSystem.ManagedSprite_SetPositionXY(healthbox.ArrowSprite, (short)(x - ArrowOffset(healthbox.Type)), y);
        }
        if (done > 0)
        {
            healthbox.DoneScrolling = 1;
            SysTask.SysTask_Done(task);
        }
    }

    // OFFSET 0x02267864
    private static void Healthbox_DrawName(HealthBox healthbox)
    {
        BgConfig bgConfig = BattleSystems.BattleSystem_GetBgConfig(healthbox.BattleSys!)!;
        MessageLoader msgLoader = BattleSystems.BattleSystem_GetMessageLoader(healthbox.BattleSys!)!;
        StringTemplate strTemplate = BattleSystems.BattleSystem_GetStringTemplate(healthbox.BattleSys!)!;
        GameString name = GameString.String_Init(NameStringSize, HeapIDBattle);
        GameString format = MessageLoader.MessageLoader_GetNewString(msgLoader, MessageNickname)!;
        Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(healthbox.BattleSys!, healthbox.Battler, healthbox.SelectedPartySlot);
        StringTemplate.StringTemplate_SetNickname(strTemplate, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
        StringTemplate.StringTemplate_Format(strTemplate, name, format);
        Window window = new();
        Bg.Window_AddToTopLeftCorner(bgConfig, window, NameWindowWidth, WindowHeight, 0, WindowFill);
        Text.Text_AddPrinterWithParamsColorAndSpacing(window, 0, name, 0, 0, TextNoDelay, TextColor, 0, 0, null);
        byte[] pixels = window.Pixels!;
        int entry = healthbox.Type * NameOffsetEntrySize;
        int topSize = TableU16(NameOffsetTable, entry + 2);
        int bottomSize = TableU16(NameOffsetTable, entry + 6);
        CopyToVram(healthbox, pixels, 0, TableU16(NameOffsetTable, entry), topSize);
        CopyToVram(healthbox, pixels, NameRowSize, TableU16(NameOffsetTable, entry + 4), bottomSize);
        CopyToVram(healthbox, pixels, topSize, TableU16(NameOffsetTable, entry + 8), TableU16(NameOffsetTable, entry + 0xA));
        CopyToVram(healthbox, pixels, NameRowSize + bottomSize, TableU16(NameOffsetTable, entry + 0xC), TableU16(NameOffsetTable, entry + 0xE));
        Bg.Window_Remove(window);
        GameString.String_Free(name);
        GameString.String_Free(format);
    }

    // OFFSET 0x022679C8
    private static void Healthbox_DrawLevelText(HealthBox healthbox)
    {
        int genderTile;
        int levelTile;
        if (healthbox.Gender == 0)
        {
            genderTile = TileMaleGender;
            levelTile = TileMaleLevel;
        }
        else if (healthbox.Gender == 1)
        {
            genderTile = TileFemaleGender;
            levelTile = TileFemaleLevel;
        }
        else
        {
            genderTile = TileGenderlessGender;
            levelTile = TileGenderlessLevel;
        }
        int entry = healthbox.Type * 8;
        int size = TableU16(GenderLevelOffsetTable, entry + 2);
        CopyToVram(healthbox, Healthbox_StaticTiles(levelTile, size), 0, TableU16(GenderLevelOffsetTable, entry), size);
        size = TableU16(GenderLevelOffsetTable, entry + 6);
        CopyToVram(healthbox, Healthbox_StaticTiles(genderTile, size), 0, TableU16(GenderLevelOffsetTable, entry + 4), size);
    }

    // OFFSET 0x02267A4C
    private static void Healthbox_DrawLevelNumber(HealthBox healthbox)
    {
        byte[] number = new byte[NumberBufferSize];
        byte[] tiles = new byte[NumberBufferSize * 2];
        Array.Fill(number, (byte)0xFF);
        FontSpecialCharsContext.FontSpecialChars_DrawBattleScreenText(BattleSystems.BattleSystem_GetSpecialCharsLevel(healthbox.BattleSys!)!, healthbox.Level, NumberDigits, 0, number);
        int entry = healthbox.Type * 8;
        int topOffset = TableU16(LevelNumberOffsetTable, entry);
        int topSize = TableU16(LevelNumberOffsetTable, entry + 2);
        int bottomOffset = TableU16(LevelNumberOffsetTable, entry + 4);
        int bottomSize = TableU16(LevelNumberOffsetTable, entry + 6);
        CopyFromVram(healthbox, topOffset, tiles, 0, topSize);
        CopyFromVram(healthbox, bottomOffset, tiles, NumberBufferSize, bottomSize);
        for (int tile = 0; tile < NumberBufferSize; tile += TileSize)
        {
            for (int i = 0; i < HalfTileSize; i++)
            {
                tiles[tile + HalfTileSize + i] = number[tile + i];
                tiles[NumberBufferSize + tile + i] = number[tile + HalfTileSize + i];
            }
        }
        CopyToVram(healthbox, tiles, 0, topOffset, topSize);
        CopyToVram(healthbox, tiles, NumberBufferSize, bottomOffset, bottomSize);
    }

    // OFFSET 0x02267B6C
    private static void Healthbox_DrawCurrentHP(HealthBox healthbox, int hp)
    {
        byte[] number = new byte[NumberBufferSize];
        Array.Fill(number, (byte)0xFF);
        FontSpecialCharsContext.FontSpecialChars_DrawBattleScreenText(BattleSystems.BattleSystem_GetSpecialCharsHP(healthbox.BattleSys!)!, hp, NumberDigits, 1, number);
        int entry = healthbox.Type * 8;
        int firstSize = TableU16(CurrentHPOffsetTable, entry + 2);
        CopyToVram(healthbox, number, 0, TableU16(CurrentHPOffsetTable, entry), firstSize);
        CopyToVram(healthbox, number, firstSize, TableU16(CurrentHPOffsetTable, entry + 4), TableU16(CurrentHPOffsetTable, entry + 6));
    }

    // OFFSET 0x02267BF8
    private static void Healthbox_DrawMaxHP(HealthBox healthbox)
    {
        byte[] number = new byte[NumberBufferSize];
        Array.Fill(number, (byte)0xFF);
        FontSpecialCharsContext.FontSpecialChars_DrawBattleScreenText(BattleSystems.BattleSystem_GetSpecialCharsHP(healthbox.BattleSys!)!, healthbox.MaxHP, NumberDigits, 0, number);
        int entry = healthbox.Type * 4;
        CopyToVram(healthbox, number, 0, TableU16(MaxHPOffsetTable, entry), TableU16(MaxHPOffsetTable, entry + 2));
    }

    // OFFSET 0x02267C58
    private static void Healthbox_DrawCaughtIcon(HealthBox healthbox)
    {
        int tile = healthbox.CaughtSpecies == 1 ? TileCaught : TileBlank;
        int entry = healthbox.Type * 4;
        int size = TableU16(CaughtOffsetTable, entry + 2);
        CopyToVram(healthbox, Healthbox_StaticTiles(tile, size), 0, TableU16(CaughtOffsetTable, entry), size);
    }

    // OFFSET 0x02267CA8
    private static void Healthbox_DrawStatusIcon(HealthBox healthbox, int tile)
    {
        int entry = healthbox.Type * 4;
        int size = TableU16(StatusOffsetTable, entry + 2);
        CopyToVram(healthbox, Healthbox_StaticTiles(tile, size), 0, TableU16(StatusOffsetTable, entry), size);
    }

    // OFFSET 0x02267CE8
    private static void Healthbox_DrawBallsLabel(HealthBox healthbox, uint flags)
    {
        BgConfig bgConfig = BattleSystems.BattleSystem_GetBgConfig(healthbox.BattleSys!)!;
        MessageLoader msgLoader = BattleSystems.BattleSystem_GetMessageLoader(healthbox.BattleSys!)!;
        GameString label = MessageLoader.MessageLoader_GetNewString(msgLoader, (flags & InfoSafariBallsLabel) != 0 ? MessageSafariBalls : MessageParkBalls)!;
        Window window = new();
        Bg.Window_AddToTopLeftCorner(bgConfig, window, BallsWindowWidth, WindowHeight, 0, WindowFill);
        Text.Text_AddPrinterWithParamsColorAndSpacing(window, 0, label, 0, 0, TextNoDelay, TextColor, 0, 0, null);
        byte[] pixels = window.Pixels!;
        CopyToVram(healthbox, pixels, 0, 0x240, 0xC0);
        CopyToVram(healthbox, pixels, BallsRowSize, 0x340, 0xC0);
        CopyToVram(healthbox, pixels, 0xC0, 0xA00, 0xE0);
        CopyToVram(healthbox, pixels, BallsRowSize + 0xC0, 0xB00, 0xE0);
        Bg.Window_Remove(window);
        GameString.String_Free(label);
    }

    // OFFSET 0x02267DC4
    private static void Healthbox_DrawBallsCount(HealthBox healthbox, uint flags)
    {
        BgConfig bgConfig = BattleSystems.BattleSystem_GetBgConfig(healthbox.BattleSys!)!;
        MessageLoader msgLoader = BattleSystems.BattleSystem_GetMessageLoader(healthbox.BattleSys!)!;
        StringTemplate strTemplate = BattleSystems.BattleSystem_GetStringTemplate(healthbox.BattleSys!)!;
        GameString text = GameString.String_Init(BallsStringSize, HeapIDBattle);
        GameString format = MessageLoader.MessageLoader_GetNewString(msgLoader, (flags & InfoSafariBallsLabel) != 0 ? MessageSafariBallsLeft : MessageParkBallsLeft)!;
        StringTemplate.StringTemplate_SetNumber(strTemplate, 0, healthbox.NumSafariBalls, 2, 1, 1);
        StringTemplate.StringTemplate_Format(strTemplate, text, format);
        Window window = new();
        Bg.Window_AddToTopLeftCorner(bgConfig, window, BallsWindowWidth, WindowHeight, 0, WindowFill);
        Text.Text_AddPrinterWithParamsColorAndSpacing(window, 0, text, 0, 0, TextNoDelay, TextColor, 0, 0, null);
        byte[] pixels = window.Pixels!;
        CopyToVram(healthbox, pixels, 0, 0x440, 0xC0);
        CopyToVram(healthbox, pixels, BallsRowSize, 0x540, 0xC0);
        CopyToVram(healthbox, pixels, 0xC0, 0xC00, 0xE0);
        CopyToVram(healthbox, pixels, BallsRowSize + 0xC0, 0xD00, 0xE0);
        Bg.Window_Remove(window);
        GameString.String_Free(text);
        GameString.String_Free(format);
    }

    // OFFSET 0x02267EDC
    private static int Healthbox_UpdateGauge(HealthBox healthbox, int gauge)
    {
        int result;
        if (gauge == GaugeHP)
        {
            result = Gauge_Update(healthbox.MaxHP, healthbox.CurHP, healthbox.Damage, ref healthbox.HpTemp, HPGaugeTiles, 1);
        }
        else
        {
            int step = Gauge_PixelDelta(healthbox.CurExp, healthbox.ExpReward, healthbox.MaxExp, ExpGaugeTiles);
            if (step == 0)
            {
                step = 1;
            }
            step = Math.Abs(healthbox.ExpReward / step);
            result = Gauge_Update(healthbox.MaxExp, healthbox.CurExp, healthbox.ExpReward, ref healthbox.ExpTemp, ExpGaugeTiles, (ushort)step);
        }
        if (gauge != GaugeHP || healthbox.NumberMode != 1)
        {
            Healthbox_DrawGauge(healthbox, (byte)gauge);
        }
        if (result == GaugeDone)
        {
            if (gauge == GaugeHP)
            {
                healthbox.HpTemp = 0;
            }
            else
            {
                healthbox.ExpTemp = 0;
            }
        }
        return result;
    }

    // OFFSET 0x02267F70
    private static void Healthbox_DrawGauge(HealthBox healthbox, byte gauge)
    {
        byte[] cells = new byte[ExpGaugeTiles];
        switch (gauge)
        {
            case GaugeHP:
            {
                int pixels = Gauge_FillCells(healthbox.MaxHP, healthbox.CurHP, healthbox.Damage, ref healthbox.HpTemp, cells, HPGaugeTiles);
                int tile;
                switch (HealthBar.HealthBar_FillColor((uint)pixels, HPGaugePixels))
                {
                    case HealthBar.ColorGreen:
                        tile = TileHPGaugeGreen;
                        break;
                    case HealthBar.ColorYellow:
                        tile = TileHPGaugeYellow;
                        break;
                    default:
                        tile = TileHPGaugeRed;
                        break;
                }
                int entry = healthbox.Type * 8;
                int firstRowTiles = TableU16(HPGaugeOffsetTable, entry + 2) >> 5;
                for (int i = 0; i < HPGaugeTiles; i++)
                {
                    byte[] source = Healthbox_StaticTiles(tile + cells[i], TileSize);
                    if (i < firstRowTiles)
                    {
                        CopyToVram(healthbox, source, 0, TableU16(HPGaugeOffsetTable, entry) + i * TileSize, TileSize);
                    }
                    else
                    {
                        CopyToVram(healthbox, source, 0, TableU16(HPGaugeOffsetTable, entry + 4) + (i - firstRowTiles) * TileSize, TileSize);
                    }
                }
                break;
            }
            case GaugeExp:
            {
                Gauge_FillCells(healthbox.MaxExp, healthbox.CurExp, healthbox.ExpReward, ref healthbox.ExpTemp, cells, ExpGaugeTiles);
                if (healthbox.Level == MaxLevel)
                {
                    for (int i = 0; i < ExpGaugeTiles; i++)
                    {
                        cells[i] = 0;
                    }
                }
                for (int i = 0; i < ExpGaugeTiles; i++)
                {
                    byte[] source = Healthbox_StaticTiles(TileExpGauge + cells[i], TileSize);
                    if (i < ExpGaugeFirstRowTiles)
                    {
                        CopyToVram(healthbox, source, 0, ExpGaugeFirstRowOffset + i * TileSize, TileSize);
                    }
                    else
                    {
                        CopyToVram(healthbox, source, 0, ExpGaugeSecondRowOffset + (i - ExpGaugeFirstRowTiles) * TileSize, TileSize);
                    }
                }
                break;
            }
        }
    }

    // OFFSET 0x022680D8
    private static int Gauge_Update(int max, int cur, int delta, ref int temp, byte tiles, ushort step)
    {
        int pixels = (byte)(tiles * 8);
        if (temp == TempUninitialized)
        {
            temp = max < pixels ? cur << 8 : cur;
        }
        int target = cur - delta;
        if (target < 0)
        {
            target = 0;
        }
        else if (target > max)
        {
            target = max;
        }
        if (max < pixels)
        {
            if (target == temp >> 8 && (temp & 0xFF) == 0)
            {
                return GaugeDone;
            }
        }
        else if (target == temp)
        {
            return GaugeDone;
        }
        if (max < pixels)
        {
            int increment = (max << 8) / pixels;
            int value;
            if (delta < 0)
            {
                temp += increment;
                value = temp >> 8;
                if (value >= target)
                {
                    temp = target << 8;
                    value = target;
                }
            }
            else
            {
                temp -= increment;
                value = temp >> 8;
                if ((temp & 0xFF) > 0)
                {
                    value++;
                }
                if (value <= target)
                {
                    temp = target << 8;
                    value = target;
                }
            }
            return value;
        }
        if (delta < 0)
        {
            temp += step;
            if (temp > target)
            {
                temp = target;
            }
        }
        else
        {
            temp -= step;
            if (temp < target)
            {
                temp = target;
            }
        }
        return temp;
    }

    // OFFSET 0x02268194
    private static byte Gauge_FillCells(int max, int cur, int delta, ref int temp, byte[] cells, byte tiles)
    {
        int target = cur - delta;
        if (target < 0)
        {
            target = 0;
        }
        else if (target > max)
        {
            target = max;
        }
        uint pixels = (uint)(tiles * 8);
        for (int i = 0; i < tiles; i++)
        {
            cells[i] = 0;
        }
        uint filled;
        if ((uint)max < pixels)
        {
            filled = (uint)(temp * (int)pixels) / (uint)max >> 8;
        }
        else
        {
            filled = (uint)(temp * (int)pixels) / (uint)max;
        }
        uint result = filled;
        if (filled == 0 && target > 0)
        {
            cells[0] = 1;
            result = 1;
        }
        else
        {
            for (int i = 0; i < tiles; i++)
            {
                if (filled >= 8)
                {
                    cells[i] = 8;
                    filled -= 8;
                }
                else
                {
                    cells[i] = (byte)filled;
                    break;
                }
            }
        }
        return (byte)result;
    }

    // OFFSET 0x02268210
    private static int Gauge_PixelDelta(int cur, int delta, int max, int tiles)
    {
        int pixels = (byte)(tiles << 3);
        int target = cur - delta;
        if (target < 0)
        {
            target = 0;
        }
        else if (target > max)
        {
            target = max;
        }
        sbyte before = (sbyte)(cur * pixels / max);
        sbyte after = (sbyte)(target * pixels / max);
        return Math.Abs(before - after);
    }

    // OFFSET 0x02268250
    private static byte[] Healthbox_StaticTiles(int tile, int size)
    {
        return RomData.Overlay(16, (uint)(StaticTiles + tile * TileSize), size);
    }

    // OFFSET 0x02268470
    private static void Healthbox_StartBobbing(HealthBox healthbox)
    {
        if (healthbox.BobbingAnimTask == null)
        {
            healthbox.Degrees = 0;
            healthbox.BobbingAnimTask = SysTask.SysTask_Start(Healthbox_BobTask, healthbox, BobTaskPriority);
        }
    }

    // OFFSET 0x02268498
    private static void Healthbox_StopBobbing(HealthBox healthbox)
    {
        if (healthbox.BobbingAnimTask != null)
        {
            SysTask.SysTask_Done(healthbox.BobbingAnimTask);
            healthbox.BobbingAnimTask = null;
        }
        healthbox.Degrees = 0;
        Healthbox_OffsetPositionXY(healthbox, 0, 0);
    }

    // OFFSET 0x022684BC
    private static void Healthbox_BobTask(SysTask task, object? param)
    {
        HealthBox healthbox = (HealthBox)param!;
        healthbox.Degrees += BobDegreesStep;
        if (healthbox.Degrees >= FullCircleDegrees)
        {
            healthbox.Degrees -= FullCircleDegrees;
        }
        long product = (long)MathUtil.CalcSineDegrees(healthbox.Degrees) * BobAmplitude;
        int offset = (int)((product + 0x800) >> 12);
        Healthbox_OffsetPositionXY(healthbox, 0, offset / 0x1000);
    }
}
