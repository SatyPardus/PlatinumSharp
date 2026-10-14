namespace PlatinumSharp.Game;

public static class BattlerSlide
{
    private const int FaceFront = 0;
    private const int FaceBack = 2;
    private const int MonSpriteYCenter = 1;
    private const int MonSpriteShadowY = 0x14;
    private const int MonSpriteShadowXOffset = 0x15;
    private const int MonSpriteShadowYOffset = 0x16;
    private const int MonSpriteShadowHeight = 0x29;
    private const int MonSpriteShadowSize = 0x2E;
    private const int ShadowBase = 0x24;
    private const int NarcPokemonSpriteOffsets = 0x93;
    private const int NarcSubstituteSprite = 0x75;
    private const int NarcSubstituteYOffsets = 0x79;
    private const ushort SubstituteCharFront = 0xF8;
    private const ushort SubstituteCharBack = 0xF9;
    private const ushort SubstitutePalette = 0xFA;
    private const int SubstituteYOffsetFront = 0x86;
    private const int SubstituteYOffsetBack = 0x87;
    private const uint SlideTrainerInCoordsAddress = 0x0223B0A0;
    private const uint EncounterCoordsAddress = 0x0223B0B8;
    private const int Overlay = 12;

    // OFFSET 0x022380BC
    public static void BattlerSlide_StartSlideIn(BattlerSpriteContext battlerSpriteCtx, int heapID)
    {
        BattlerSlide_TransformSprite(battlerSpriteCtx, battlerSpriteCtx.TargetBattler, 0, heapID);
    }

    // OFFSET 0x022380CC
    public static void BattlerSlide_StartAllySlideIn(BattlerSpriteContext battlerSpriteCtx, int heapID)
    {
        BattlerSlide_TransformSprite(battlerSpriteCtx, battlerSpriteCtx.SourceBattler, 1, heapID);
    }

    // OFFSET 0x022380DC
    private static void BattlerSlide_TransformSprite(BattlerSpriteContext ctx, int faceBattler, int invertFace, int heapID)
    {
        int target = ctx.TargetBattler;
        int source = ctx.SourceBattler;
        PokemonSprite sprite = ctx.Sprites[target]!;
        PokemonSprite.PokemonSprite_Push(sprite);
        ushort species = ctx.Species[source];
        byte gender = ctx.Genders[source];
        byte form = ctx.Forms[source];
        byte shiny = ctx.ShinyFlags[source];
        uint personality = ctx.Personalities[source];
        int face;
        if (invertFace != 0)
        {
            face = (ctx.Types[faceBattler] & 1) != 0 ? FaceFront : FaceBack;
        }
        else
        {
            face = (ctx.Types[faceBattler] & 1) != 0 ? FaceBack : FaceFront;
        }
        PokemonSpriteTemplate template = new();
        PokemonSpriteTemplates.BuildPokemonSpriteTemplate(template, species, gender, face, shiny, form, personality);
        PokemonSprite.PokemonSprite_GetTemplate(sprite).CopyFrom(template);
        PokemonSprite.PokemonSprite_ScheduleReloadFromNARC(sprite);
        PokemonSpriteTemplate current = PokemonSprite.PokemonSprite_GetTemplate(sprite);
        int narcID = (int)(current.Word0 & 0xFFFF);
        int character = (int)(current.Word0 >> 16);
        int palette = (int)(current.Word1 & 0xFFFF);
        int templateSpecies = (int)(current.Word1 >> 16);
        PokemonSpriteData spriteData = ctx.PokemonSpriteData[target]!;
        CharacterSprite.CharacterSprite_LoadPokemonSprite(narcID, character, heapID, spriteData.Tiles!, personality, false, face, templateSpecies);
        spriteData.NarcID = narcID;
        spriteData.Palette = palette;
        spriteData.YOffset = PokemonSpriteMetadata.Pokemon_LoadSpriteYOffset(species, gender, (byte)face, form, personality);
        sbyte yOffset = (sbyte)spriteData.YOffset;
        int baseY = BattlerSlide_GetCoord(ctx.Types[target], 1);
        PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteYCenter, baseY + yOffset);
        if (face != FaceBack)
        {
            return;
        }
        NARC narc = NARC.NARC_ctor(NarcPokemonSpriteOffsets, heapID);
        PokemonSpriteMetadata.PokemonSprite_LoadYOffset(narc, out sbyte shadowYOffset, species);
        PokemonSpriteMetadata.PokemonSprite_LoadXOffsetShadow(narc, out sbyte shadowXOffset, species);
        PokemonSpriteMetadata.PokemonSprite_LoadShadowSize(narc, out byte shadowSize, species);
        NARC.NARC_dtor(narc);
        PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteShadowSize, shadowSize);
        PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteShadowY, baseY + ShadowBase);
        PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteShadowXOffset, shadowXOffset);
        PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteShadowYOffset, ShadowBase - yOffset);
        PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteShadowHeight, shadowYOffset);
    }

    // OFFSET 0x022382BC
    public static void BattlerSlide_StartSlideOut(BattlerSpriteContext ctx, int heapID)
    {
        int target = ctx.TargetBattler;
        PokemonSprite sprite = ctx.Sprites[target]!;
        PokemonSprite.PokemonSprite_Pop(sprite);
        PokemonSpriteTemplate template = PokemonSprite.PokemonSprite_GetTemplate(sprite);
        ushort species = ctx.Species[target];
        byte gender = ctx.Genders[target];
        byte form = ctx.Forms[target];
        uint personality = ctx.Personalities[target];
        int face = (ctx.Types[target] & 1) != 0 ? FaceBack : FaceFront;
        PokemonSpriteData spriteData = ctx.PokemonSpriteData[target]!;
        CharacterSprite.CharacterSprite_LoadPokemonSprite((int)(template.Word0 & 0xFFFF), (int)(template.Word0 >> 16), heapID, spriteData.Tiles!, personality, false, face, (int)(template.Word1 >> 16));
        spriteData.NarcID = (int)(template.Word0 & 0xFFFF);
        spriteData.Palette = (int)(template.Word1 & 0xFFFF);
        spriteData.YOffset = PokemonSpriteMetadata.Pokemon_LoadSpriteYOffset(species, gender, (byte)face, form, personality);
        PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteYCenter, spriteData.YOffset + BattlerSlide_GetCoord(ctx.Types[target], 1));
    }

    // OFFSET 0x02238390
    public static void BattlerSlide_StartPartnerSlideIn(BattlerSpriteContext ctx, int heapID)
    {
        int target = ctx.TargetBattler;
        PokemonSprite sprite = ctx.Sprites[target]!;
        PokemonSprite.PokemonSprite_Push(sprite);
        ushort character;
        int yOffsetMember;
        if ((ctx.Types[target] & 1) != 0)
        {
            character = SubstituteCharBack;
            yOffsetMember = SubstituteYOffsetBack;
        }
        else
        {
            character = SubstituteCharFront;
            yOffsetMember = SubstituteYOffsetFront;
        }
        PokemonSpriteTemplate template = PokemonSprite.PokemonSprite_GetTemplate(sprite);
        template.Word0 = (uint)(NarcSubstituteSprite | (character << 16));
        template.Word1 = SubstitutePalette;
        template.Word2 = 0;
        template.Word3 = 0;
        PokemonSprite.PokemonSprite_ScheduleReloadFromNARC(sprite);
        PokemonSpriteData spriteData = ctx.PokemonSpriteData[target]!;
        CharacterSprite.CharacterSprite_LoadSpriteFrame0(NarcSubstituteSprite, character, heapID, spriteData.Tiles!);
        spriteData.NarcID = NarcSubstituteSprite;
        spriteData.Palette = SubstitutePalette;
        byte[] yOffsetData = new byte[1];
        NARC.NARC_ReadWholeMemberByIndexPair(yOffsetData, NarcSubstituteYOffsets, yOffsetMember);
        spriteData.YOffset = yOffsetData[0];
        int y = spriteData.YOffset + BattlerSlide_GetCoord(ctx.Types[target], 1);
        PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteYCenter, y);
        if ((ctx.Types[target] & 1) == 0)
        {
            return;
        }
        PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteShadowSize, 0);
        PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteShadowY, y + (ShadowBase - yOffsetData[0]));
        PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteShadowXOffset, 0);
        PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteShadowYOffset, ShadowBase - yOffsetData[0]);
        PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteShadowHeight, 0);
    }

    // OFFSET 0x022384CC
    public static short BattlerSlide_GetCoord(int battlerType, int coordIndex)
    {
        if (coordIndex == 0)
        {
            return (short)RomData.OverlayU16(Overlay, (uint)(SlideTrainerInCoordsAddress + battlerType * 4), 1)[0];
        }
        return (short)RomData.OverlayU16(Overlay, (uint)(EncounterCoordsAddress + battlerType * 6 + coordIndex * 2), 1)[0];
    }
}

public static partial class PokemonSpriteMetadata
{
    private const ushort SpeciesChatotCry = 0x1B9;

    // OFFSET 0x02077D3C
    public static void Pokemon_PlayCry(object? chatotCry, int cryMod, ushort species, int form, int pan, int volume, int forceDefaultChatot, int heapID)
    {
        if (species == SpeciesChatotCry)
        {
            if (!SoundChatot.Sound_CanPlayChatotCry(cryMod))
            {
                SoundChatot.Sound_SetUsingDefaultChatotCry(1);
                SoundPlayback.Sound_PlayPokemonCryEx(cryMod, species, pan, volume, heapID, (byte)form);
                return;
            }
            if (forceDefaultChatot != 0)
            {
                SoundChatot.Sound_SetUsingDefaultChatotCry(1);
            }
            SoundChatot.Sound_PlayChatotCry(chatotCry, 0, volume, pan);
            return;
        }
        SoundPlayback.Sound_PlayPokemonCryEx(cryMod, species, pan, volume, heapID, (byte)form);
    }
}
