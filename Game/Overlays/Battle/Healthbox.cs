namespace PlatinumSharp.Game;

public static partial class Battlers
{
    // OFFSET 0x02263B08
    public static HealthBox Battler_GetHealthbox(BattlerData battler)
    {
        return battler.Healthbox;
    }
}

public static partial class Healthboxes
{
    public const int TypePlayerSolo = 0;
    public const int TypeEnemySolo = 1;
    public const int TypePlayerSlot1 = 2;
    public const int TypeEnemySlot1 = 3;
    public const int TypePlayerSlot2 = 4;
    public const int TypeEnemySlot2 = 5;
    public const int TypeSafari = 6;
    public const int TypePalPark = 7;
    public const int TypeCount = 8;

    private const int NarcBattleSubscreen = 0x1B;
    private const int HeapIDBattle = 5;
    private const int MemberPalette = 0x47;
    private const int MemberSafariPalette = 0x51;
    private const int ResourceMainPalette = 0x4E26;
    private const int ResourceEffectsPalette = 0x4E27;
    private const int ResourceSafariPalette = 0x4E28;
    private const int PaletteBufferMainObj = 2;
    private const int VRamMain = 1;
    private const int ResourceTiles = 0;
    private const int ResourceCell = 2;
    private const int ResourceAnim = 3;
    private const uint BattleTypeSafari = 0x20;
    private const uint BattleTypePalPark = 0x200;
    private const uint BattleTypeNoArrowAnim = 0x220;
    private const uint ArrowOffsetTable = 0x0226F334;
    private const uint ArrowTemplate = 0x0226F514;

    private static readonly uint[] MainTemplates =
    {
        0x0226F5B0, 0x0226F5E4, 0x0226F4AC, 0x0226F4E0, 0x0226F548, 0x0226F618, 0x0226F57C, 0x0226F57C,
    };

    private static sbyte[]? arrowOffsets;

    private static int ArrowOffset(int type)
    {
        if (arrowOffsets == null)
        {
            byte[] raw = RomData.Overlay(16, ArrowOffsetTable, TypeCount);
            arrowOffsets = new sbyte[TypeCount];
            for (int i = 0; i < TypeCount; i++)
            {
                arrowOffsets[i] = (sbyte)raw[i];
            }
        }
        return arrowOffsets[type];
    }

    // OFFSET 0x02266F1C
    private static void Healthbox_LoadMainResources(SpriteSystem spriteSys, SpriteManager spriteMan, NARC narc, PaletteData paletteData, int type)
    {
        SpriteTemplate template = Healthbox_GetMainTemplate((byte)type)!;
        SpriteSystem.SpriteSystem_LoadCharResObjFromOpenNarc(spriteSys, spriteMan, narc, template.Resources[ResourceTiles], 1, VRamMain, template.Resources[ResourceTiles]);
        SpriteSystem.SpriteSystem_LoadPaletteBufferFromOpenNarc(paletteData, PaletteBufferMainObj, spriteSys, spriteMan, narc, MemberPalette, 0, 1, VRamMain, ResourceMainPalette);
        SpriteSystem.SpriteSystem_LoadCellResObjFromOpenNarc(spriteSys, spriteMan, narc, template.Resources[ResourceCell], 1, template.Resources[ResourceCell]);
        SpriteSystem.SpriteSystem_LoadAnimResObjFromOpenNarc(spriteSys, spriteMan, narc, template.Resources[ResourceAnim], 1, template.Resources[ResourceAnim]);
        SpriteSystem.SpriteSystem_LoadPaletteBufferFromOpenNarc(paletteData, PaletteBufferMainObj, spriteSys, spriteMan, narc, MemberPalette, 0, 1, VRamMain, ResourceEffectsPalette);
        if (type == TypeSafari || type == TypePalPark)
        {
            SpriteSystem.SpriteSystem_LoadPaletteBufferFromOpenNarc(paletteData, PaletteBufferMainObj, spriteSys, spriteMan, narc, MemberSafariPalette, 0, 1, VRamMain, ResourceSafariPalette);
        }
    }

    // OFFSET 0x02266FE4
    private static void Healthbox_LoadArrowResources(SpriteSystem spriteSys, SpriteManager spriteMan, NARC narc, PaletteData paletteData, int type)
    {
        SpriteTemplate? template = Healthbox_GetArrowTemplate((byte)type);
        if (template == null)
        {
            return;
        }
        SpriteSystem.SpriteSystem_LoadCharResObjFromOpenNarc(spriteSys, spriteMan, narc, template.Resources[ResourceTiles], 1, VRamMain, template.Resources[ResourceTiles]);
        SpriteSystem.SpriteSystem_LoadPaletteBufferFromOpenNarc(paletteData, PaletteBufferMainObj, spriteSys, spriteMan, narc, MemberPalette, 0, 1, VRamMain, ResourceMainPalette);
        SpriteSystem.SpriteSystem_LoadCellResObjFromOpenNarc(spriteSys, spriteMan, narc, template.Resources[ResourceCell], 1, template.Resources[ResourceCell]);
        SpriteSystem.SpriteSystem_LoadAnimResObjFromOpenNarc(spriteSys, spriteMan, narc, template.Resources[ResourceAnim], 1, template.Resources[ResourceAnim]);
    }

    // OFFSET 0x02267060
    private static ManagedSprite Healthbox_NewMainSprite(SpriteSystem spriteSys, SpriteManager spriteMan, int type)
    {
        SpriteTemplate template = Healthbox_GetMainTemplate((byte)type)!;
        ManagedSprite sprite = SpriteSystem.SpriteSystem_NewSprite(spriteSys, spriteMan, template)!;
        SpriteSystem.Sprite_TickFrame(sprite.Sprite!);
        return sprite;
    }

    // OFFSET 0x02267220
    private static void Healthbox_DeleteMainSprite(HealthBox healthbox)
    {
        if (healthbox.BobbingAnimTask != null)
        {
            SysTask.SysTask_Done(healthbox.BobbingAnimTask);
            healthbox.BobbingAnimTask = null;
        }
        if (healthbox.MainSprite != null)
        {
            SpriteSystem.Sprite_DeleteAndFreeResources(healthbox.MainSprite);
            healthbox.MainSprite = null;
        }
    }

    // OFFSET 0x02267244
    private static void Healthbox_DeleteArrowSprite(HealthBox healthbox)
    {
        if (healthbox.ArrowSprite != null)
        {
            SpriteSystem.Sprite_DeleteAndFreeResources(healthbox.ArrowSprite);
            healthbox.ArrowSprite = null;
        }
    }

    // OFFSET 0x02267258
    private static void Healthbox_UnloadMainResources(HealthBox healthbox)
    {
        SpriteTemplate template = Healthbox_GetMainTemplate(healthbox.Type)!;
        BattleSystems.BattleSystem_GetSpriteSystem(healthbox.BattleSys!);
        SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(healthbox.BattleSys!)!;
        SpriteSystem.SpriteManager_UnloadCharObjById(spriteMan, template.Resources[ResourceTiles]);
        SpriteSystem.SpriteManager_UnloadCellObjById(spriteMan, template.Resources[ResourceCell]);
        SpriteSystem.SpriteManager_UnloadAnimObjById(spriteMan, template.Resources[ResourceAnim]);
    }

    // OFFSET 0x0226728C
    private static void Healthbox_UnloadArrowResources(HealthBox healthbox)
    {
        SpriteTemplate? template = Healthbox_GetArrowTemplate(healthbox.Type);
        if (template == null)
        {
            return;
        }
        BattleSystems.BattleSystem_GetSpriteSystem(healthbox.BattleSys!);
        SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(healthbox.BattleSys!)!;
        SpriteSystem.SpriteManager_UnloadCharObjById(spriteMan, template.Resources[ResourceTiles]);
        SpriteSystem.SpriteManager_UnloadCellObjById(spriteMan, template.Resources[ResourceCell]);
        SpriteSystem.SpriteManager_UnloadAnimObjById(spriteMan, template.Resources[ResourceAnim]);
    }

    // OFFSET 0x022672C4
    public static void Healthbox_LoadResources(HealthBox healthbox)
    {
        NARC narc = NARC.NARC_ctor(NarcBattleSubscreen, HeapIDBattle);
        SpriteSystem spriteSys = BattleSystems.BattleSystem_GetSpriteSystem(healthbox.BattleSys!)!;
        SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(healthbox.BattleSys!)!;
        PaletteData paletteData = BattleSystems.BattleSystem_GetPaletteData(healthbox.BattleSys!)!;
        SpriteTemplate template = Healthbox_GetMainTemplate(healthbox.Type)!;
        Healthbox_LoadMainResources(spriteSys, spriteMan, narc, paletteData, healthbox.Type);
        healthbox.MainSprite = Healthbox_NewMainSprite(spriteSys, spriteMan, healthbox.Type);
        Healthbox_LoadArrowResources(spriteSys, spriteMan, narc, paletteData, healthbox.Type);
        if (healthbox.ArrowSprite != null)
        {
            SpriteSystem.Sprite_SetPositionXY(healthbox.ArrowSprite.Sprite!, (short)(template.X - ArrowOffset(healthbox.Type)), template.Y);
        }
        NARC.NARC_dtor(narc);
    }

    // OFFSET 0x02267360
    public static void Healthbox_Free(HealthBox healthbox)
    {
        Healthbox_DeleteMainSprite(healthbox);
        Healthbox_UnloadMainResources(healthbox);
        Healthbox_DeleteArrowSprite(healthbox);
        Healthbox_UnloadArrowResources(healthbox);
    }

    // OFFSET 0x022675D8
    public static void Healthbox_SetExplicitPriority(HealthBox healthbox, int priority)
    {
        if (healthbox.MainSprite == null)
        {
            return;
        }
        SpriteSystem.ManagedSprite_SetExplicitPriority(healthbox.MainSprite, priority);
        if (healthbox.ArrowSprite == null)
        {
            return;
        }
        SpriteSystem.ManagedSprite_SetExplicitPriority(healthbox.ArrowSprite, priority);
    }

    // OFFSET 0x022675F8
    public static void Healthbox_EnableArrow(HealthBox healthbox, int enable)
    {
        if (healthbox.ArrowSprite == null)
        {
            return;
        }
        if ((BattleSystems.BattleSystem_GetBattleType(healthbox.BattleSys!) & BattleTypeNoArrowAnim) != 0 && enable == 1)
        {
            return;
        }
        SpriteSystem.ManagedSprite_SetDrawFlag(healthbox.ArrowSprite, enable);
    }

    // OFFSET 0x02267620
    public static void Healthbox_Enable(HealthBox healthbox, int enable)
    {
        if (healthbox.MainSprite == null)
        {
            return;
        }
        SpriteSystem.ManagedSprite_SetDrawFlag(healthbox.MainSprite, enable);
        Healthbox_EnableArrow(healthbox, enable);
    }

    // OFFSET 0x0226763C
    public static void Healthbox_OffsetPositionXY(HealthBox healthbox, int x, int y)
    {
        if (healthbox.MainSprite == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteTemplate template = Healthbox_GetMainTemplate(healthbox.Type)!;
        SpriteSystem.Sprite_SetPositionXY(healthbox.MainSprite!.Sprite!, (short)(template.X + x), (short)(template.Y + y));
        if (healthbox.ArrowSprite != null)
        {
            SpriteSystem.Sprite_SetPositionXY(healthbox.ArrowSprite.Sprite!, (short)(template.X + x - ArrowOffset(healthbox.Type)), (short)(template.Y + y));
        }
    }

    // OFFSET 0x0226825C
    public static byte Healthbox_Type(int battlerType, uint battleType)
    {
        switch (battlerType)
        {
            case 0:
                if ((battleType & BattleTypePalPark) != 0)
                {
                    return TypePalPark;
                }
                if ((battleType & BattleTypeSafari) != 0)
                {
                    return TypeSafari;
                }
                return TypePlayerSolo;
            case 1:
                return TypeEnemySolo;
            case 2:
                return TypePlayerSlot1;
            case 3:
                return TypeEnemySlot1;
            case 4:
                return TypePlayerSlot2;
            case 5:
                return TypeEnemySlot2;
            default:
                ErrorHandling.GF_AssertFail();
                return TypePlayerSolo;
        }
    }

    // OFFSET 0x022682B0
    private static SpriteTemplate? Healthbox_GetMainTemplate(byte type)
    {
        if (type >= TypeCount)
        {
            ErrorHandling.GF_AssertFail();
            return null;
        }
        return BattleSubscreens.RomSpriteTemplate(MainTemplates[type]);
    }

    // OFFSET 0x02268314
    private static SpriteTemplate? Healthbox_GetArrowTemplate(byte type)
    {
        switch (type)
        {
            case TypePlayerSolo:
            case TypePlayerSlot1:
            case TypePlayerSlot2:
            case TypeSafari:
            case TypePalPark:
                return BattleSubscreens.RomSpriteTemplate(ArrowTemplate);
            case TypeEnemySolo:
            case TypeEnemySlot1:
            case TypeEnemySlot2:
                return null;
            default:
                ErrorHandling.GF_AssertFail();
                return null;
        }
    }
}
