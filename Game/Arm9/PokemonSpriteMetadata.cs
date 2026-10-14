namespace PlatinumSharp.Game;

public static partial class PokemonSpriteMetadata
{
    public const int FaceBack = 0;
    public const int FaceFront = 2;
    public const int AnimFrameCount = 10;

    private const int RecordSize = 0x59;
    private const int FaceRecordSize = 0x2B;
    private const int FaceCryDelay = 0;
    private const int FaceAnimation = 1;
    private const int FaceStartDelay = 2;
    private const int FaceFrames = 3;
    private const int RecordYOffset = 0x56;
    private const int RecordShadowXOffset = 0x57;
    private const int RecordShadowSize = 0x58;
    private const int NarcHeight = 5;
    private const int NarcOtherHeight = 0x79;
    private const int SpeciesUnown = 0xC9;
    private const int SpeciesCastform = 0x15F;
    private const int SpeciesDeoxys = 0x182;
    private const int SpeciesBurmy = 0x19C;
    private const int SpeciesWormadam = 0x19D;
    private const int SpeciesCherrim = 0x1A5;
    private const int SpeciesShellos = 0x1A6;
    private const int SpeciesGastrodon = 0x1A7;
    private const int SpeciesRotom = 0x1DF;
    private const int SpeciesGiratina = 0x1E7;
    private const int SpeciesShaymin = 0x1EC;
    private const int SpeciesArceus = 0x1ED;
    private const int SpeciesEgg = 0x1EE;
    private const int SpeciesManaphyEgg = 0x1EF;
    private const int SpeciesChatot = 0x1B9;

    private static byte[] ReadRecord(NARC narc, int species)
    {
        byte[] record = new byte[RecordSize];
        NARC.NARC_ReadFromMember(narc, 0, (uint)(species * RecordSize), RecordSize, record);
        return record;
    }

    // OFFSET 0x02076648
    public static byte Pokemon_LoadSpriteYOffset(ushort species, byte gender, byte face, byte form, uint personality)
    {
        int sanitized = PokemonSpriteTemplates.Pokemon_SanitizeFormId(species, form);
        int narcID = NarcOtherHeight;
        int member;
        switch (species)
        {
            case SpeciesBurmy:
                member = (face >> 1) + 0x48 + sanitized * 2;
                break;
            case SpeciesWormadam:
                member = (face >> 1) + 0x4E + sanitized * 2;
                break;
            case SpeciesShellos:
                member = face + 0x54 + sanitized;
                break;
            case SpeciesGastrodon:
                member = face + 0x58 + sanitized;
                break;
            case SpeciesCherrim:
                member = face + 0x5C + sanitized;
                break;
            case SpeciesArceus:
                member = (face >> 1) + 0x60 + sanitized * 2;
                break;
            case SpeciesCastform:
                member = face * 2 + 0x40 + sanitized;
                break;
            case SpeciesDeoxys:
                member = (face >> 1) + sanitized * 2;
                break;
            case SpeciesUnown:
                member = (face >> 1) + 8 + sanitized * 2;
                break;
            case SpeciesEgg:
                member = sanitized + 0x84;
                break;
            case SpeciesManaphyEgg:
                member = 0x84;
                break;
            case SpeciesShaymin:
                member = (face >> 1) + 0x88 + sanitized * 2;
                break;
            case SpeciesRotom:
                member = (face >> 1) + 0x8C + sanitized * 2;
                break;
            case SpeciesGiratina:
                member = (face >> 1) + 0x98 + sanitized * 2;
                break;
            default:
                narcID = NarcHeight;
                member = species * 4 + face + (gender == 1 ? 0 : 1);
                break;
        }
        byte[] value = new byte[1];
        NARC.NARC_ReadWholeMemberByIndexPair(value, narcID, member);
        return value[0];
    }

    // OFFSET 0x02077DB4
    public static void Pokemon_PlayDelayedCry(object? chatotCry, int cryMod, ushort species, int form, int pan, int volume, int forceDefaultChatot, int heapID, byte delay)
    {
        if (species == SpeciesChatot)
        {
            if (!SoundChatot.Sound_CanPlayChatotCry(cryMod))
            {
                SoundChatot.Sound_SetUsingDefaultChatotCry(1);
                SoundPlayback.Sound_PlayDelayedPokemonCryEx(cryMod, species, pan, volume, heapID, delay, (byte)form);
                return;
            }
            if (forceDefaultChatot != 0)
            {
                SoundChatot.Sound_SetUsingDefaultChatotCry(1);
            }
            SoundChatot.Sound_PlayDelayedChatotCry(chatotCry, 0, volume, pan, delay);
            return;
        }
        SoundPlayback.Sound_PlayDelayedPokemonCryEx(cryMod, species, pan, volume, heapID, delay, (byte)form);
    }

    // OFFSET 0x020789BC
    public static void PokemonSprite_LoadAnimFrames(NARC narc, SpriteAnimFrame[] frames, ushort species, int clientType)
    {
        int face = (clientType & 1) != 0 ? 0 : 1;
        byte[] record = ReadRecord(narc, species);
        int offset = face * FaceRecordSize + FaceFrames;
        for (int i = 0; i < AnimFrameCount; i++)
        {
            frames[i].SpriteFrame = (sbyte)record[offset + i * 4];
            frames[i].FrameDelay = record[offset + i * 4 + 1];
            frames[i].XOffset = (sbyte)record[offset + i * 4 + 2];
            frames[i].Padding = (sbyte)record[offset + i * 4 + 3];
        }
    }

    // OFFSET 0x020789F4
    public static void PokemonSprite_LoadAnim(NARC narc, PokemonAnimManager animMan, PokemonSprite sprite, ushort species, int face, int flipSprite, int index)
    {
        int faceIndex = face == FaceFront ? 0 : 1;
        byte[] record = ReadRecord(narc, species);
        PokemonAnimTemplate template = new();
        template.Animation = record[faceIndex * FaceRecordSize + FaceAnimation];
        template.StartDelay = record[faceIndex * FaceRecordSize + FaceStartDelay];
        template.FlipSprite = (byte)flipSprite;
        PokemonAnimManager.PokemonAnimManager_InitAnim(animMan, sprite, template, (byte)index);
    }

    // OFFSET 0x02078A4C
    public static void PokemonSprite_LoadCryDelay(NARC narc, out byte cryDelay, ushort species, int clientType)
    {
        int face = (clientType & 1) != 0 ? 0 : 1;
        byte[] record = ReadRecord(narc, species);
        cryDelay = record[face * FaceRecordSize + FaceCryDelay];
    }

    // OFFSET 0x02078A80
    public static void PokemonSprite_LoadYOffset(NARC narc, out sbyte yOffset, ushort species)
    {
        yOffset = (sbyte)ReadRecord(narc, species)[RecordYOffset];
    }

    // OFFSET 0x02078AA4
    public static void PokemonSprite_LoadXOffsetShadow(NARC narc, out sbyte xOffset, ushort species)
    {
        xOffset = (sbyte)ReadRecord(narc, species)[RecordShadowXOffset];
    }

    // OFFSET 0x02078AC8
    public static void PokemonSprite_LoadShadowSize(NARC narc, out byte shadowSize, ushort species)
    {
        shadowSize = ReadRecord(narc, species)[RecordShadowSize];
    }
}
