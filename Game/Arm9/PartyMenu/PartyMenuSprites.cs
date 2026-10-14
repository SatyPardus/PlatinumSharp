namespace PlatinumSharp.Game;

public static partial class PartyMenus
{
    private const uint RenderOamTemplateAddress = 0x020F20A0;
    private const uint CharTransferTemplateAddress = 0x020F2070;
    private const uint ResourcePathsAddress = 0x020F2084;
    private const int ResourcePathCount = 7;
    private const uint SpriteTemplates = 0x020F20C0;
    private const int SpriteTemplateSize = 0x28;
    private const uint ContextActionTable = 0x020F2458;
    private const int SpriteTouchButtonEffect = 28;
    private const int SpriteStatusIconBase = 10;
    private const int SpriteHeldItemBase = 16;
    private const int SpriteBallSealBase = 22;
    private const int IconResourceBase = 4;

    private static RenderOamTemplate ReadRenderOamTemplate(uint address)
    {
        int[] v = Array.ConvertAll(RomData.Arm9U32(address, 8), x => (int)x);
        return new RenderOamTemplate { MainOamStart = v[0], MainOamCount = v[1], MainAffineOamStart = v[2], MainAffineOamCount = v[3], SubOamStart = v[4], SubOamCount = v[5], SubAffineOamStart = v[6], SubAffineOamCount = v[7] };
    }

    private static CharTransferTemplateWithModes ReadCharTransferTemplate(uint address)
    {
        int[] v = Array.ConvertAll(RomData.Arm9U32(address, 5), x => (int)x);
        return new CharTransferTemplateWithModes { MaxTasks = v[0], SizeMain = v[1], SizeSub = v[2], ModeMain = v[3], ModeSub = v[4] };
    }

    private static SpriteTemplateFromResourceHeader ReadSpriteTemplate(uint address)
    {
        byte[] raw = RomData.Arm9(address, SpriteTemplateSize);
        return new SpriteTemplateFromResourceHeader
        {
            ResourceHeaderID = BitConverter.ToInt32(raw, 0),
            X = BitConverter.ToInt16(raw, 4),
            Y = BitConverter.ToInt16(raw, 6),
            Z = BitConverter.ToInt16(raw, 8),
            AnimIdx = BitConverter.ToUInt16(raw, 0xA),
            Priority = BitConverter.ToInt32(raw, 0xC),
            PlttIdx = BitConverter.ToInt32(raw, 0x10),
            VramType = BitConverter.ToInt32(raw, 0x14),
            Unused18 = BitConverter.ToInt32(raw, 0x18),
            Unused1C = BitConverter.ToInt32(raw, 0x1C),
            Unused20 = BitConverter.ToInt32(raw, 0x20),
            Unused24 = BitConverter.ToInt32(raw, 0x24),
        };
    }

    private static Sprite NewSprite(PartyMenuApplication app, SpriteTemplateFromResourceHeader template)
    {
        return SpriteSystem.SpriteSystem_NewSpriteFromResourceHeader(app.SpriteSystem!, app.SpriteMan!, template)!;
    }

    // OFFSET 0x02082C2C
    private static void PartyMenu_InitSpriteResources(PartyMenuApplication app)
    {
        GXLayers.GXLayers_EngineAToggleLayers(0x10, 1);
        GXLayers.GXLayers_EngineBToggleLayers(0x10, 1);
        VramTransfer.VramTransfer_New(0x20, HeapID);
        app.SpriteSystem = SpriteSystem.SpriteSystem_Alloc(HeapID);
        app.SpriteMan = SpriteSystem.SpriteManager_New(app.SpriteSystem);
        SpriteSystem.SpriteSystem_Init(app.SpriteSystem, ReadRenderOamTemplate(RenderOamTemplateAddress), ReadCharTransferTemplate(CharTransferTemplateAddress), 0x20);
        SpriteSystem.SpriteSystem_InitSprites(app.SpriteSystem, app.SpriteMan, 0x23);
        SpriteUtil.SetSubScreenViewRect(SpriteSystem.SpriteSystem_GetRenderer(app.SpriteSystem), 0, 0x100000);
        uint[] pointers = RomData.Arm9U32(ResourcePathsAddress, ResourcePathCount);
        string?[] paths = new string?[ResourcePathCount];
        for (int i = 0; i < ResourcePathCount; i++)
        {
            paths[i] = pointers[i] == 0 ? null : RomData.Arm9String(pointers[i]);
        }
        SpriteSystem.SpriteSystem_LoadResourceDataFromFilepaths(app.SpriteSystem, app.SpriteMan, paths);
    }

    // OFFSET 0x02082CEC
    private static void PartyMenu_DrawMemberSpeciesIcon(PartyMenuApplication app, int slot, ushort x, ushort y, NARC narc)
    {
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(app.PartyMenu!.Party!, slot);
        PartyMenuMember member = app.PartyMembers[slot];
        member.SpriteXDelta = (short)x;
        member.SpriteYDelta = (short)y;
        SpriteSystem.SpriteSystem_ReplaceCharResObjFromOpenNarc(app.SpriteSystem!, app.SpriteMan!, narc, (int)PokemonIcon.Pokemon_IconSpriteIndex(mon), 0, slot + IconResourceBase);
        uint isEgg = Pokemon.Pokemon_GetValue(mon, MonDataIsEgg, null);
        SpriteTemplateFromResourceHeader template = new()
        {
            ResourceHeaderID = slot + IconResourceBase,
            X = (short)x,
            Y = (short)y,
            Z = 0,
            AnimIdx = 0,
            Priority = 0,
            PlttIdx = (int)PokemonIcon.PokeIconPaletteIndex(member.Species, member.Form, isEgg) + 3,
            VramType = 1,
        };
        member.Sprite = NewSprite(app, template);
    }

    // OFFSET 0x02082DA8
    public static void PartyMenu_LoadMemberSpeciesIcon(PartyMenuApplication app, int slot)
    {
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(app.PartyMenu!.Party!, slot);
        uint species = Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null);
        uint form = Pokemon.Pokemon_GetValue(mon, MonDataForm, null);
        NARC narc = NARC.NARC_ctor(PokemonIconArchive, HeapID);
        Sprite sprite = app.PartyMembers[slot].Sprite!;
        int location = G2d.GetImageLocation(Sprite.Sprite_GetImageProxy(sprite), G2d.VramMain);
        byte[] file = Graphics.LoadMemberFromOpenNARC(narc, PokemonIcon.Pokemon_IconSpriteIndex(mon), 0, HeapID, 1)!;
        if (G2d.GetUnpackedCharacterData(file, out CharacterData? charData))
        {
            Video.LoadObjectTiles(DisplayMain, charData!.RawData, (uint)location, charData.SizeBytes);
        }
        SpriteSystem.Sprite_SetExplicitPalette2(sprite, PokemonIcon.PokeIconPaletteIndex(species, form, 0) + 3);
        NARC.NARC_dtor(narc);
    }

    // OFFSET 0x02082E58
    private static void PartyMenu_InitSprites(PartyMenuApplication app)
    {
        app.Sprites[SpriteCursorNormal] = NewSprite(app, ReadSpriteTemplate(SpriteTemplates));
        app.Sprites[SpriteCursorSwitch] = NewSprite(app, ReadSpriteTemplate(SpriteTemplates + 0x28));
        app.Sprites[SpriteConfirmButton] = NewSprite(app, ReadSpriteTemplate(SpriteTemplates + 0x50));
        app.Sprites[SpriteCancelButton] = NewSprite(app, ReadSpriteTemplate(SpriteTemplates + 0x78));
        app.Sprites[SpriteTouchButtonEffect] = NewSprite(app, ReadSpriteTemplate(0x020F2430));
        for (int i = 0; i < MaxPartySize; i++)
        {
            uint offset = (uint)(i * SpriteTemplateSize);
            SpriteTemplateFromResourceHeader status = ReadSpriteTemplate(SpriteTemplates + 0xA0 + offset);
            app.Sprites[SpriteStatusIconBase + i] = NewSprite(app, status);
            app.PartyMembers[i].StatusXPos = status.X;
            app.PartyMembers[i].StatusYPos = status.Y;
            Sprite.Sprite_SetDrawFlag(app.Sprites[SpriteStatusIconBase + i]!, 0);
            app.Sprites[SpriteHeldItemBase + i] = NewSprite(app, ReadSpriteTemplate(SpriteTemplates + 0x190 + offset));
            app.Sprites[SpriteBallSealBase + i] = NewSprite(app, ReadSpriteTemplate(SpriteTemplates + 0x280 + offset));
        }
        Sprite.Sprite_SetDrawFlag(app.Sprites[SpriteCursorSwitch]!, 0);
        Sprite.Sprite_SetDrawFlag(app.Sprites[SpriteTouchButtonEffect]!, 0);
    }

    // OFFSET 0x02082FAC
    private static void PartyMenu_DrawMemberPokeBall(PartyMenuApplication app, int slot, ushort x, ushort y)
    {
        SpriteTemplateFromResourceHeader template = new()
        {
            ResourceHeaderID = 0,
            X = (short)x,
            Y = (short)y,
            Z = 0,
            AnimIdx = 0,
            Priority = 1,
            PlttIdx = 0,
            VramType = 1,
        };
        app.Sprites[slot] = NewSprite(app, template);
    }

    // OFFSET 0x02082FF4
    private static void PartyMenu_CleanupSprites(PartyMenuApplication app)
    {
        SpriteSystem.SpriteSystem_DestroySpriteManager(app.SpriteSystem!, app.SpriteMan!);
        SpriteSystem.SpriteSystem_Free(app.SpriteSystem!);
    }

    // OFFSET 0x02083014
    private static void PartyMenu_DrawMemberStatusCondition(PartyMenuApplication app, int slot, byte status)
    {
        Sprite sprite = app.Sprites[SpriteStatusIconBase + slot]!;
        if (status == StatusNone)
        {
            Sprite.Sprite_SetDrawFlag(sprite, 0);
            return;
        }
        Sprite.Sprite_SetAnim(sprite, status);
        Sprite.Sprite_SetDrawFlag(sprite, 1);
    }

    // OFFSET 0x02083040
    public static void PartyMenu_DrawMemberHeldItem(PartyMenuApplication app, int slot, ushort item)
    {
        Sprite sprite = app.Sprites[SpriteHeldItemBase + slot]!;
        if (item == 0)
        {
            Sprite.Sprite_SetDrawFlag(sprite, 0);
            return;
        }
        Sprite.Sprite_SetAnim(sprite, Item.Item_IsMail(item) == 1 ? 1u : 0u);
        Sprite.Sprite_SetDrawFlag(sprite, 1);
    }

    // OFFSET 0x02083080
    public static void PartyMenu_DrawMemberMail(PartyMenuApplication app, int slot)
    {
        Sprite sprite = app.Sprites[SpriteHeldItemBase + slot]!;
        Sprite.Sprite_SetAnim(sprite, 1);
        Sprite.Sprite_SetDrawFlag(sprite, 1);
    }

    // OFFSET 0x020830A0
    private static void PartyMenu_AlignMemberHeldItem(PartyMenuApplication app, int slot, short x, short y)
    {
        PartyMenuMember member = app.PartyMembers[slot];
        member.ItemXPos = (short)(x + 8);
        member.ItemYPos = (short)(y + 8);
        SpriteSystem.Sprite_SetPositionXY(app.Sprites[SpriteHeldItemBase + slot]!, member.ItemXPos, member.ItemYPos);
    }

    // OFFSET 0x020830D4
    private static void PartyMenu_AlignMemberBallSeal(PartyMenuApplication app, int slot)
    {
        PartyMenuMember member = app.PartyMembers[slot];
        SpriteSystem.Sprite_SetPositionXY(app.Sprites[SpriteBallSealBase + slot]!, (short)(member.ItemXPos + 8), member.ItemYPos);
    }

    // OFFSET 0x02083104
    public static void PartyMenu_DrawMemberBallSeal(PartyMenuApplication app, int slot)
    {
        Sprite sprite = app.Sprites[SpriteBallSealBase + slot]!;
        Sprite.Sprite_SetDrawFlag(sprite, app.PartyMembers[slot].BallSeal == 0 ? 0 : 1);
    }

    // OFFSET 0x02083138
    private static void StartSpriteAnim(Sprite sprite, byte anim)
    {
        if (anim != Sprite.Sprite_GetActiveAnim(sprite))
        {
            Sprite.Sprite_SetAnimFrame(sprite, 0);
            Sprite.Sprite_SetAnim(sprite, anim);
        }
    }

    // OFFSET 0x02083158
    private static byte CalcMemberIconAnim(PartyMenuMember member)
    {
        if (member.CurHP == 0)
        {
            return 0;
        }
        int status = member.StatusIcon & 0xFFF;
        if (status != StatusNone && status != 0 && status != 6)
        {
            return 5;
        }
        switch (HealthBar.HealthBar_Color(member.CurHP, member.MaxHP, 0x30))
        {
            case HealthBar.ColorFull:
                return 1;
            case HealthBar.ColorGreen:
                return 2;
            case HealthBar.ColorYellow:
                return 3;
            case HealthBar.ColorRed:
                return 4;
        }
        return 0;
    }

    // OFFSET 0x020831B4
    private static void PartyMenu_UpdateMemberIcons(PartyMenuApplication app)
    {
        for (int i = 0; i < MaxPartySize; i++)
        {
            PartyMenuMember member = app.PartyMembers[i];
            if (member.IsPresent == 0)
            {
                continue;
            }
            byte anim;
            if (app.OrderSwitch.InProgress == 1 && (app.OrderSwitch.Slots[0] == i || app.OrderSwitch.Slots[1] == i))
            {
                anim = 0;
            }
            else
            {
                anim = CalcMemberIconAnim(member);
            }
            StartSpriteAnim(member.Sprite!, anim);
            Sprite.Sprite_UpdateAnim(member.Sprite!, Fx32.One);
            if (app.CurrPartySlot == i && anim != 0 && anim != 5)
            {
                if (Sprite.Sprite_GetAnimFrame(member.Sprite!) == 0)
                {
                    SpriteSystem.Sprite_SetPositionXY(member.Sprite!, member.SpriteXDelta, (short)(member.SpriteYDelta - 3));
                }
                else
                {
                    SpriteSystem.Sprite_SetPositionXY(member.Sprite!, member.SpriteXDelta, (short)(member.SpriteYDelta + 1));
                }
            }
            else
            {
                SpriteSystem.Sprite_SetPositionXY(member.Sprite!, member.SpriteXDelta, member.SpriteYDelta);
            }
        }
    }

    // OFFSET 0x0208327C
    public static void PartyMenu_UpdateCursor(PartyMenuApplication app, byte slot, uint palette)
    {
        GridMenuCursor.GridMenuCursor_GetFirstCoords(app.CursorPosTable![slot], out byte x, out byte y);
        Sprite.Sprite_SetAnim(app.Sprites[SpriteCursorNormal]!, GetCursorAnim(app.PartyMenu!.Type, slot));
        Sprite.Sprite_SetDrawFlag(app.Sprites[SpriteCursorNormal]!, 1);
        SpriteSystem.Sprite_SetPositionXY(app.Sprites[SpriteCursorNormal]!, x, y);
        SpriteSystem.Sprite_SetExplicitPalette2(app.Sprites[SpriteCursorNormal]!, palette);
    }

    // OFFSET 0x020832E4
    private static void PartyMenu_InitTouchButtonEffect(PartyMenuApplication app, short x, short y)
    {
        VecFx32 position = new() { X = x << 12, Y = (y << 12) + 0x100000, Z = 0 };
        Sprite sprite = app.Sprites[SpriteTouchButtonEffect]!;
        Sprite.Sprite_SetPosition(sprite, position);
        Sprite.Sprite_SetDrawFlag(sprite, 1);
        Sprite.Sprite_SetAnimFrame(sprite, 0);
        Sprite.Sprite_SetAnim(sprite, 0);
    }

    // OFFSET 0x02083334
    private static void PartyMenu_UpdateTouchButtonEffect(PartyMenuApplication app)
    {
        Sprite sprite = app.Sprites[SpriteTouchButtonEffect]!;
        if (Sprite.Sprite_GetDrawFlag(sprite) == 1)
        {
            Sprite.Sprite_UpdateAnim(sprite, Fx32.One);
            if (Sprite.Sprite_GetAnimFrame(sprite) == 2)
            {
                Sprite.Sprite_SetDrawFlag(sprite, 0);
            }
        }
    }

    // OFFSET 0x02083370
    private static uint PartyMenu_GetAction(int entry)
    {
        return RomData.Arm9U32((uint)(ContextActionTable + entry * 4), 1)[0];
    }

    // OFFSET 0x0208337C
    private static void PartyMenu_ClearContextWindow(PartyMenuApplication app)
    {
        RenderWindow.Window_EraseStandardFrame(app.MenuWindows[0], 1);
        Bg.Window_ClearAndScheduleCopyToVRAM(app.MenuWindows[0]);
        Menu.Menu_Free(app.ContextMenu!, out _);
        StringList.StringList_Free(app.ContextMenuChoices!);
        Bg.Window_Remove(app.MenuWindows[0]);
    }
}
