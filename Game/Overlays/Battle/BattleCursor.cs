namespace PlatinumSharp.Game;

public sealed class BattleCursor
{
    public const int CornerCount = 4;
    public const int SpriteCount = 5;
    public const int PointerSprite = 4;

    public readonly ManagedSprite?[] Sprites = new ManagedSprite?[SpriteCount];
    public SysTask? Task;
}

public static class BattleCursors
{
    private const int NarcBattleSubscreen = 0x1B;
    private const int MemberPalette = 0x50;
    private const int MemberTiles = 0xFA;
    private const int MemberCell = 0xFB;
    private const int MemberAnim = 0xFC;
    private const int PaletteBufferSubObj = 3;
    private const int VRamSub = 2;
    private const uint CursorTemplate = 0x02270AA4;
    private const uint TaskPriority = 40000;
    private const int DefaultSubscreenOffset = 0xC0000;

    // OFFSET 0x0226DB7C
    public static void BattleCursor_LoadResources(SpriteSystem spriteSys, SpriteManager spriteMan, PaletteData paletteData, int heapID, int tilesID, int paletteID, int cellID, int animID)
    {
        NARC narc = NARC.NARC_ctor(NarcBattleSubscreen, heapID);
        SpriteSystem.SpriteSystem_LoadPaletteBufferFromOpenNarc(paletteData, PaletteBufferSubObj, spriteSys, spriteMan, narc, MemberPalette, 0, 1, VRamSub, paletteID);
        SpriteSystem.SpriteSystem_LoadCharResObjFromOpenNarc(spriteSys, spriteMan, narc, MemberTiles, 1, VRamSub, tilesID);
        SpriteSystem.SpriteSystem_LoadCellResObjFromOpenNarc(spriteSys, spriteMan, narc, MemberCell, 1, cellID);
        SpriteSystem.SpriteSystem_LoadAnimResObjFromOpenNarc(spriteSys, spriteMan, narc, MemberAnim, 1, animID);
        NARC.NARC_dtor(narc);
    }

    // OFFSET 0x0226DBFC
    public static void BattleCursor_FreeResources(SpriteManager spriteMan, int tilesID, int paletteID, int cellID, int animID)
    {
        SpriteSystem.SpriteManager_UnloadCharObjById(spriteMan, tilesID);
        SpriteSystem.SpriteManager_UnloadPlttObjById(spriteMan, paletteID);
        SpriteSystem.SpriteManager_UnloadCellObjById(spriteMan, cellID);
        SpriteSystem.SpriteManager_UnloadAnimObjById(spriteMan, animID);
    }

    // OFFSET 0x0226DC24
    public static BattleCursor BattleCursor_New(SpriteSystem spriteSys, SpriteManager spriteMan, int heapID, int tilesID, int paletteID, int cellID, int animID, int priority, int bgPriority)
    {
        SpriteTemplate template = BattleSubscreens.RomSpriteTemplate(CursorTemplate);
        template.Resources[0] = tilesID;
        template.Resources[1] = paletteID;
        template.Resources[2] = cellID;
        template.Resources[3] = animID;
        template.Priority = priority;
        template.BgPriority = bgPriority;
        BattleCursor cursor = new();
        for (int i = 0; i < BattleCursor.SpriteCount; i++)
        {
            cursor.Sprites[i] = SpriteSystem.SpriteSystem_NewSprite(spriteSys, spriteMan, template);
            SpriteSystem.ManagedSprite_SetDrawFlag(cursor.Sprites[i]!, 0);
        }
        cursor.Task = SysTask.SysTask_Start(BattleCursor_AnimateTask, cursor, TaskPriority);
        return cursor;
    }

    // OFFSET 0x0226DCA8
    public static void BattleCursor_Free(BattleCursor cursor)
    {
        for (int i = 0; i < BattleCursor.SpriteCount; i++)
        {
            SpriteSystem.Sprite_DeleteAndFreeResources(cursor.Sprites[i]!);
        }
        SysTask.SysTask_Done(cursor.Task!);
    }

    // OFFSET 0x0226DCCC
    public static void BattleCursor_SetCornersWithOffset(BattleCursor cursor, int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3, int offset)
    {
        SpriteSystem.ManagedSprite_SetPositionXYWithSubscreenOffset(cursor.Sprites[0]!, (short)x0, (short)y0, offset);
        SpriteSystem.ManagedSprite_SetPositionXYWithSubscreenOffset(cursor.Sprites[1]!, (short)x1, (short)y1, offset);
        SpriteSystem.ManagedSprite_SetPositionXYWithSubscreenOffset(cursor.Sprites[2]!, (short)x2, (short)y2, offset);
        SpriteSystem.ManagedSprite_SetPositionXYWithSubscreenOffset(cursor.Sprites[3]!, (short)x3, (short)y3, offset);
        for (int i = 0; i < BattleCursor.CornerCount; i++)
        {
            SpriteSystem.ManagedSprite_SetAnim(cursor.Sprites[i]!, (uint)i);
        }
        for (int i = 0; i < BattleCursor.CornerCount; i++)
        {
            SpriteSystem.ManagedSprite_SetDrawFlag(cursor.Sprites[i]!, 1);
        }
    }

    // OFFSET 0x0226DD54
    public static void BattleCursor_SetCorners(BattleCursor cursor, int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3)
    {
        BattleCursor_SetCornersWithOffset(cursor, x0, y0, x1, y1, x2, y2, x3, y3, DefaultSubscreenOffset);
    }

    // OFFSET 0x0226DD7C
    public static void BattleCursor_SetRect(BattleCursor cursor, int left, int right, int top, int bottom)
    {
        BattleCursor_SetCorners(cursor, left, top, right, top, left, bottom, right, bottom);
    }

    // OFFSET 0x0226DD9C
    public static void BattleCursor_SetRectWithOffset(BattleCursor cursor, int left, int right, int top, int bottom, int offset)
    {
        BattleCursor_SetCornersWithOffset(cursor, left, top, right, top, left, bottom, right, bottom, offset);
    }

    // OFFSET 0x0226DDC0
    public static void BattleCursor_SetPointer(BattleCursor cursor, int x, int y, int anim, int offset)
    {
        SpriteSystem.ManagedSprite_SetPositionXYWithSubscreenOffset(cursor.Sprites[BattleCursor.PointerSprite]!, (short)x, (short)y, offset);
        SpriteSystem.ManagedSprite_SetAnim(cursor.Sprites[BattleCursor.PointerSprite]!, (uint)anim);
        SpriteSystem.ManagedSprite_SetDrawFlag(cursor.Sprites[BattleCursor.PointerSprite]!, 1);
    }

    // OFFSET 0x0226DDE8
    public static void BattleCursor_Hide(BattleCursor cursor)
    {
        for (int i = 0; i < BattleCursor.SpriteCount; i++)
        {
            SpriteSystem.ManagedSprite_SetDrawFlag(cursor.Sprites[i]!, 0);
        }
    }

    // OFFSET 0x0226DE04
    public static void BattleCursor_HidePointer(BattleCursor cursor)
    {
        SpriteSystem.ManagedSprite_SetDrawFlag(cursor.Sprites[BattleCursor.PointerSprite]!, 0);
    }

    // OFFSET 0x0226DE10
    private static void BattleCursor_AnimateTask(SysTask task, object? param)
    {
        BattleCursor cursor = (BattleCursor)param!;
        if (SpriteSystem.ManagedSprite_GetDrawFlag(cursor.Sprites[0]!) == 0)
        {
            return;
        }
        for (int i = 0; i < BattleCursor.CornerCount; i++)
        {
            SpriteSystem.ManagedSprite_TickFrame(cursor.Sprites[i]!);
        }
        if (SpriteSystem.ManagedSprite_GetDrawFlag(cursor.Sprites[BattleCursor.PointerSprite]!) == 1)
        {
            SpriteSystem.ManagedSprite_TickFrame(cursor.Sprites[BattleCursor.PointerSprite]!);
        }
    }
}
