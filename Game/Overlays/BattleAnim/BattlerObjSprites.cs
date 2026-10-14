namespace PlatinumSharp.Game;

public sealed class BattlerObjSprites
{
    public const int MaxBattlers = 4;

    public int HeapID;
    public int Battler;
    public SpriteSystem? SpriteSys;
    public SpriteManager? SpriteMan;
    public PaletteData? PaletteData;
    public readonly int[] ResourceIDs = new int[MaxBattlers];
    public readonly ManagedSprite?[] Sprites = new ManagedSprite?[MaxBattlers];
    public readonly PokemonSpriteData?[] SpriteData = new PokemonSpriteData?[MaxBattlers];
    public readonly byte[] BattlerTypes = new byte[MaxBattlers];
    public readonly PokemonSprite?[] MonSprites = new PokemonSprite?[MaxBattlers];
}

public static partial class BattleSystems
{
    private const byte BattlerTypeNone = 0xFF;

    // OFFSET 0x0223F2AC
    public static PokemonSpriteData BattleSystem_GetBattlerSpriteData(BattleSystem battleSys, int battler)
    {
        return battleSys.PokemonSpriteDataArray[battler];
    }

    // OFFSET 0x0223F87C
    public static void BattleSystem_GetBattlerTypes(BattleSystem battleSys, byte[] types)
    {
        for (int i = 0; i < BattlerObjSprites.MaxBattlers; i++)
        {
            types[i] = BattlerTypeNone;
        }
        for (int i = 0; i < battleSys.MaxBattlers; i++)
        {
            types[i] = Battlers.Battler_GetType(battleSys.Battlers[i]!);
        }
    }

    // OFFSET 0x0223F8AC
    public static void BattleSystem_GetBattlerMonSprites(BattleSystem battleSys, PokemonSprite?[] monSprites)
    {
        for (int i = 0; i < BattlerObjSprites.MaxBattlers; i++)
        {
            monSprites[i] = null;
        }
        for (int i = 0; i < battleSys.MaxBattlers; i++)
        {
            monSprites[i] = Battlers.Battler_GetMonSprite(battleSys.Battlers[i]!);
        }
    }
}

public static partial class Battlers
{
    // OFFSET 0x02263AFC
    public static PokemonSprite? Battler_GetMonSprite(BattlerData battler)
    {
        return battler.MonSprite;
    }
}

public static partial class BattlerObjSpritesFuncs
{
    public const int AllBattlers = 0xFF;

    private const int NarcBattleSubscreen = 0x1B;
    private const int MemberPalette = 0x4B;
    private const int MemberTiles = 0x4C;
    private const int MemberCell = 0x4D;
    private const int MemberAnim = 0x4E;
    private const int ResourceIDBase = 0xD903;
    private const int ResourceIDStep = 5000;
    private const int PaletteBufferMainObj = 2;
    private const int VRamMain = 1;
    private const int MonAttrX = 0;
    private const int MonAttrY = 1;
    private const int MonAttrHide = 6;
    private const int MonAttrShadowHeight = 0x29;
    private const int TransferObjCharMain = 0x13;
    private const uint MonTilesSize = 0xC80;
    private const uint PaletteSize = 0x20;

    private static readonly int[] Priorities = { 0, 0, 0x14, 0xA, 0xA, 0x14 };

    // OFFSET 0x022234F8
    public static BattlerObjSprites BattlerObjSprites_New(BattleSystem battleSys, int heapID, int battler)
    {
        BattlerObjSprites sprites = new();
        sprites.HeapID = heapID;
        sprites.Battler = battler;
        sprites.SpriteSys = BattleSystems.BattleSystem_GetSpriteSystem(battleSys);
        sprites.SpriteMan = BattleSystems.BattleSystem_GetSpriteManager(battleSys);
        sprites.PaletteData = BattleSystems.BattleSystem_GetPaletteData(battleSys);
        for (int i = 0; i < BattlerObjSprites.MaxBattlers; i++)
        {
            sprites.Sprites[i] = null;
            sprites.SpriteData[i] = BattleSystems.BattleSystem_GetBattlerSpriteData(battleSys, i);
        }
        BattleSystems.BattleSystem_GetBattlerTypes(battleSys, sprites.BattlerTypes);
        BattleSystems.BattleSystem_GetBattlerMonSprites(battleSys, sprites.MonSprites);
        NARC narc = NARC.NARC_ctor(NarcBattleSubscreen, heapID);
        for (int i = 0; i < BattlerObjSprites.MaxBattlers; i++)
        {
            if ((i == sprites.Battler || sprites.Battler == AllBattlers) && sprites.MonSprites[i] != null)
            {
                int id = ResourceIDBase + i + sprites.Battler * ResourceIDStep;
                SpriteSystem.SpriteSystem_LoadCharResObjFromOpenNarc(sprites.SpriteSys!, sprites.SpriteMan!, narc, MemberTiles, 0, VRamMain, id);
                SpriteSystem.SpriteSystem_LoadPaletteBufferFromOpenNarc(sprites.PaletteData!, PaletteBufferMainObj, sprites.SpriteSys!, sprites.SpriteMan!, narc, MemberPalette, 0, 1, VRamMain, id);
                SpriteSystem.SpriteSystem_LoadCellResObjFromOpenNarc(sprites.SpriteSys!, sprites.SpriteMan!, narc, MemberCell, 0, id);
                SpriteSystem.SpriteSystem_LoadAnimResObjFromOpenNarc(sprites.SpriteSys!, sprites.SpriteMan!, narc, MemberAnim, 0, id);
            }
        }
        NARC.NARC_dtor(narc);
        for (int i = 0; i < BattlerObjSprites.MaxBattlers; i++)
        {
            if (i != sprites.Battler && sprites.Battler != AllBattlers)
            {
                continue;
            }
            int id = ResourceIDBase + i + sprites.Battler * ResourceIDStep;
            sprites.ResourceIDs[i] = id;
            PokemonSpriteData data = sprites.SpriteData[i]!;
            PokemonSprite? monSprite = sprites.MonSprites[i];
            int narcID = data.NarcID;
            int palette = data.Palette;
            byte[]? tiles = data.Tiles;
            if (monSprite == null)
            {
                continue;
            }
            SpriteTemplate template = new();
            template.X = (short)PokemonSprite.PokemonSprite_GetAttribute(monSprite, MonAttrX);
            template.Y = (short)((short)PokemonSprite.PokemonSprite_GetAttribute(monSprite, MonAttrY) - PokemonSprite.PokemonSprite_GetAttribute(monSprite, MonAttrShadowHeight));
            template.Z = 0;
            template.AnimIdx = 0;
            template.Priority = Priorities[sprites.BattlerTypes[i]];
            template.PlttIdx = 0;
            template.VramType = VRamMain;
            template.Resources[0] = id;
            template.Resources[1] = id;
            template.Resources[2] = id;
            template.Resources[3] = id;
            template.Resources[4] = 0;
            template.Resources[5] = 0;
            template.BgPriority = 1;
            template.VramTransfer = 0;
            ManagedSprite sprite = SpriteSystem.SpriteSystem_NewSprite(sprites.SpriteSys!, sprites.SpriteMan!, template)!;
            SpriteSystem.ManagedSprite_TickFrame(sprite);
            sprites.Sprites[i] = sprite;
            if (!PokemonSprite.PokemonSprite_IsActive(monSprite))
            {
                SpriteSystem.ManagedSprite_SetDrawFlag(sprite, 0);
            }
            else if (PokemonSprite.PokemonSprite_GetAttribute(monSprite, MonAttrHide) == 1)
            {
                SpriteSystem.ManagedSprite_SetDrawFlag(sprite, 0);
            }
            ImageProxy imageProxy = Sprite.Sprite_GetImageProxy(sprite.Sprite!);
            VramTransfer.VramTransfer_Request(TransferObjCharMain, (uint)G2d.GetImageLocation(imageProxy, VRamMain), tiles!, MonTilesSize);
            ImagePaletteProxy paletteProxy = Sprite.Sprite_GetPaletteProxy(sprite.Sprite!);
            uint offset = PlttTransfer.PlttTransfer_GetPlttOffset(paletteProxy, VRamMain);
            PaletteData.PaletteData_LoadBufferFromFileStart(sprites.PaletteData!, narcID, palette, sprites.HeapID, PaletteBufferMainObj, PaletteSize, (ushort)(offset << 4));
        }
        return sprites;
    }

    // OFFSET 0x02223764
    public static BattlerObjSprites BattlerObjSprites_NewAll(BattleSystem battleSys, int heapID)
    {
        return BattlerObjSprites_New(battleSys, heapID, AllBattlers);
    }

    // OFFSET 0x02223770
    public static void BattlerObjSprites_Free(BattlerObjSprites sprites)
    {
        for (int i = 0; i < BattlerObjSprites.MaxBattlers; i++)
        {
            if (sprites.Sprites[i] != null)
            {
                SpriteSystem.SpriteManager_UnloadCharObjById(sprites.SpriteMan!, sprites.ResourceIDs[i]);
                SpriteSystem.SpriteManager_UnloadPlttObjById(sprites.SpriteMan!, sprites.ResourceIDs[i]);
                SpriteSystem.Sprite_DeleteAndFreeResources(sprites.Sprites[i]!);
            }
        }
    }
}
