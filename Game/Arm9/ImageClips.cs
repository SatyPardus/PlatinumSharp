using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class ClipAccessorySource
{
    public int ID;
    public SoftwareSprite? Sprite;
}

public sealed class ClipAccessory
{
    public const int Size = 4;

    public byte ID;
    public byte X;
    public byte Y;
    public byte Priority;

    public void WriteTo(byte[] body, int offset)
    {
        body[offset] = ID;
        body[offset + 1] = X;
        body[offset + 2] = Y;
        body[offset + 3] = Priority;
    }

    public void ReadFrom(byte[] body, int offset)
    {
        ID = body[offset];
        X = body[offset + 1];
        Y = body[offset + 2];
        Priority = body[offset + 3];
    }

    // OFFSET 0x02029A18
    public static void ClipAccessory_Set(ClipAccessory accessory, byte id, byte x, byte y, byte priority)
    {
        accessory.ID = id;
        accessory.X = x;
        accessory.Y = y;
        accessory.Priority = priority;
    }

    // OFFSET 0x0202A624
    public static int ClipAccessory_ID(ClipAccessory accessory)
    {
        return accessory.ID;
    }

    // OFFSET 0x0202A628
    public static int ClipAccessory_X(ClipAccessory accessory)
    {
        return accessory.X;
    }

    // OFFSET 0x0202A62C
    public static int ClipAccessory_Y(ClipAccessory accessory)
    {
        return accessory.Y;
    }

    // OFFSET 0x0202A630
    public static int ClipAccessory_Priority(ClipAccessory accessory)
    {
        return (sbyte)accessory.Priority;
    }
}

public sealed class ClipMon
{
    public const int Size = 0x38;
    public const int NicknameLength = 11;
    public const int OTNameLength = 8;

    private const int NicknameOffset = 0xA;
    private const int OTNameOffset = 0x20;
    private const int MonDataPersonality = 0;
    private const int MonDataSpecies = 5;
    private const int MonDataOTID = 7;
    private const int MonDataForm = 0x70;
    private const int MonDataNickname = 0x75;
    private const int CoordLimit = 0x100;
    private const int MinPriority = -128;

    public uint Personality;
    public uint OTID;
    public ushort Species;
    public readonly ushort[] Nickname = new ushort[NicknameLength];
    public readonly ushort[] OTName = new ushort[OTNameLength];
    public sbyte Priority;
    public byte X;
    public byte Y;
    public byte Form;
    public byte OTGender;

    public void WriteTo(byte[] body, int offset)
    {
        Bin.W32(body, offset, Personality);
        Bin.W32(body, offset + 4, OTID);
        Bin.W16(body, offset + 8, Species);
        for (int i = 0; i < NicknameLength; i++)
        {
            Bin.W16(body, offset + NicknameOffset + i * 2, Nickname[i]);
        }
        for (int i = 0; i < OTNameLength; i++)
        {
            Bin.W16(body, offset + OTNameOffset + i * 2, OTName[i]);
        }
        body[offset + 0x30] = (byte)Priority;
        body[offset + 0x31] = X;
        body[offset + 0x32] = Y;
        body[offset + 0x33] = Form;
        body[offset + 0x34] = OTGender;
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Personality = Bin.U32(body, offset);
        OTID = Bin.U32(body, offset + 4);
        Species = (ushort)Bin.U16(body, offset + 8);
        for (int i = 0; i < NicknameLength; i++)
        {
            Nickname[i] = (ushort)Bin.U16(body, offset + NicknameOffset + i * 2);
        }
        for (int i = 0; i < OTNameLength; i++)
        {
            OTName[i] = (ushort)Bin.U16(body, offset + OTNameOffset + i * 2);
        }
        Priority = (sbyte)body[offset + 0x30];
        X = body[offset + 0x31];
        Y = body[offset + 0x32];
        Form = body[offset + 0x33];
        OTGender = body[offset + 0x34];
    }

    // OFFSET 0x020298D8
    private static void ClipMon_ReadSpritePlacement(object clipSprite, out byte x, out byte y, out sbyte priority)
    {
        sub_02259250(clipSprite, out int spriteX, out int spriteY);
        int spritePriority = sub_022591E0(clipSprite);
        if (spriteX >= CoordLimit)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteY >= CoordLimit)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spritePriority <= MinPriority)
        {
            ErrorHandling.GF_AssertFail();
        }
        x = (byte)spriteX;
        y = (byte)spriteY;
        priority = (sbyte)spritePriority;
    }

    // OFFSET 0x0202992C
    public static void ClipMon_SetFromPokemon(ClipMon clipMon, Pokemon mon, byte x, byte y, sbyte priority)
    {
        clipMon.Species = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null);
        Pokemon.Pokemon_GetValue(mon, MonDataNickname, clipMon.Nickname);
        clipMon.Personality = Pokemon.Pokemon_GetValue(mon, MonDataPersonality, null);
        clipMon.OTID = Pokemon.Pokemon_GetValue(mon, MonDataOTID, null);
        clipMon.Form = (byte)Pokemon.Pokemon_GetValue(mon, MonDataForm, null);
        clipMon.X = x;
        clipMon.Y = y;
        clipMon.Priority = priority;
    }

    // OFFSET 0x02029990
    public static void ClipMon_SetFromSprite(ClipMon clipMon, Pokemon mon, object clipSprite)
    {
        ClipMon_ReadSpritePlacement(clipSprite, out byte x, out byte y, out sbyte priority);
        ClipMon_SetFromPokemon(clipMon, mon, x, y, priority);
    }

    // OFFSET 0x020299C0
    public static void ClipMon_SetOT(ClipMon clipMon, GameString name, int gender)
    {
        GameString.String_ToChars(name, clipMon.OTName, OTNameLength);
        clipMon.OTGender = (byte)gender;
    }

    // OFFSET 0x020299D8
    private static void ClipMon_InitPokemon(ClipMon clipMon, Pokemon mon)
    {
        Pokemon.Pokemon_InitWith(mon, clipMon.Species, 0, 0, 1, clipMon.Personality, 1, clipMon.OTID);
        Pokemon.Pokemon_SetValue(mon, MonDataNickname, clipMon.Nickname);
        Pokemon.Pokemon_SetValue(mon, MonDataForm, clipMon.Form);
    }

    // OFFSET 0x0202A5E8
    public static int ClipMon_Species(ClipMon clipMon)
    {
        return clipMon.Species;
    }

    // OFFSET 0x0202A5EC
    public static void ClipMon_OTName(ClipMon clipMon, GameString name)
    {
        GameString.String_CopyChars(name, clipMon.OTName);
    }

    // OFFSET 0x0202A5FC
    public static int ClipMon_OTGender(ClipMon clipMon)
    {
        return clipMon.OTGender;
    }

    // OFFSET 0x0202A604
    public static int ClipMon_Priority(ClipMon clipMon)
    {
        return clipMon.Priority;
    }

    // OFFSET 0x0202A60C
    public static int ClipMon_X(ClipMon clipMon)
    {
        return clipMon.X;
    }

    // OFFSET 0x0202A614
    public static int ClipMon_Y(ClipMon clipMon)
    {
        return clipMon.Y;
    }

    // OFFSET 0x0202A61C
    public static void ClipMon_BuildPokemon(ClipMon clipMon, Pokemon mon)
    {
        ClipMon_InitPokemon(clipMon, mon);
    }
}

public sealed class FashionCase
{
    public const int Size = 0x40;
    public const int AccessoryCount = 100;
    public const int StackableAccessoryCount = 61;
    public const int BackdropCount = 18;
    public const int MaxStack = 9;
    public const int NoBackdropSlot = 18;

    private const int CountWords = 8;
    private const int SingleWords = 2;
    private const int BackdropWords = 6;
    private const int SingleOffset = 0x20;
    private const int BackdropOffset = 0x28;

    public readonly uint[] StackCounts = new uint[CountWords];
    public readonly uint[] SingleFlags = new uint[SingleWords];
    public readonly uint[] BackdropSlots = new uint[BackdropWords];

    public void WriteTo(byte[] body, int offset)
    {
        for (int i = 0; i < CountWords; i++)
        {
            Bin.W32(body, offset + i * 4, StackCounts[i]);
        }
        for (int i = 0; i < SingleWords; i++)
        {
            Bin.W32(body, offset + SingleOffset + i * 4, SingleFlags[i]);
        }
        for (int i = 0; i < BackdropWords; i++)
        {
            Bin.W32(body, offset + BackdropOffset + i * 4, BackdropSlots[i]);
        }
    }

    public void ReadFrom(byte[] body, int offset)
    {
        for (int i = 0; i < CountWords; i++)
        {
            StackCounts[i] = Bin.U32(body, offset + i * 4);
        }
        for (int i = 0; i < SingleWords; i++)
        {
            SingleFlags[i] = Bin.U32(body, offset + SingleOffset + i * 4);
        }
        for (int i = 0; i < BackdropWords; i++)
        {
            BackdropSlots[i] = Bin.U32(body, offset + BackdropOffset + i * 4);
        }
    }

    // OFFSET 0x02029A2C
    private static void FashionCase_WriteStackCount(uint[] counts, uint count, byte accessory)
    {
        if (accessory >= StackableAccessoryCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        int shift = (accessory % 8) * 4;
        int word = accessory / 8;
        counts[word] &= ~(0xFu << shift);
        counts[word] |= count << shift;
    }

    // OFFSET 0x02029A70
    private static uint FashionCase_ReadStackCount(uint[] counts, byte accessory)
    {
        if (accessory >= StackableAccessoryCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        uint count = (counts[accessory / 8] >> ((accessory % 8) * 4)) & 0xF;
        if (count > MaxStack)
        {
            count = MaxStack;
        }
        return count;
    }

    // OFFSET 0x02029AB0
    private static void FashionCase_WriteSingleFlag(uint[] flags, uint value, byte index)
    {
        if (value >= 2)
        {
            ErrorHandling.GF_AssertFail();
        }
        int shift = index % 32;
        int word = index / 32;
        flags[word] &= ~(1u << shift);
        flags[word] |= value << shift;
    }

    // OFFSET 0x02029AF0
    private static uint FashionCase_ReadSingleFlag(FashionCase fashionCase, byte index)
    {
        int word = index / 32;
        uint flags = word < SingleWords ? fashionCase.SingleFlags[word] : fashionCase.BackdropSlots[word - SingleWords];
        return (flags >> (index % 32)) & 1;
    }

    // OFFSET 0x02029B18
    private static void FashionCase_WriteBackdropSlot(uint[] slots, uint slot, byte backdrop)
    {
        if (slot > NoBackdropSlot)
        {
            ErrorHandling.GF_AssertFail();
        }
        int shift = (backdrop % 4) * 8;
        int word = backdrop / 4;
        slots[word] &= ~(0xFFu << shift);
        slots[word] |= slot << shift;
    }

    // OFFSET 0x02029B5C
    private static uint FashionCase_ReadBackdropSlot(uint[] slots, byte backdrop)
    {
        return (slots[backdrop / 4] >> ((backdrop % 4) * 8)) & 0xFF;
    }

    // OFFSET 0x02029B80
    private static byte FashionCase_CountBackdropSlots(uint[] slots)
    {
        int count = 0;
        for (int i = 0; i < BackdropCount; i++)
        {
            if (FashionCase_ReadBackdropSlot(slots, (byte)i) != NoBackdropSlot)
            {
                count++;
            }
        }
        return (byte)count;
    }

    // OFFSET 0x02029BA4
    private static bool FashionCase_IsStackable(uint accessory)
    {
        if (accessory < StackableAccessoryCount)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02029BB0
    public static void FashionCase_Init(FashionCase fashionCase)
    {
        Array.Clear(fashionCase.StackCounts);
        Array.Clear(fashionCase.SingleFlags);
        Array.Clear(fashionCase.BackdropSlots);
        for (int i = 0; i < BackdropCount; i++)
        {
            FashionCase_WriteBackdropSlot(fashionCase.BackdropSlots, NoBackdropSlot, (byte)i);
        }
    }

    // OFFSET 0x02029D50
    public static bool FashionCase_CanFitAccessory(FashionCase fashionCase, uint accessory, uint count)
    {
        bool fits = true;
        uint owned = FashionCase_GetAccessoryCount(fashionCase, accessory);
        if (FashionCase_IsStackable(accessory))
        {
            if (owned + count > MaxStack)
            {
                fits = false;
            }
        }
        else if (owned + count > 1)
        {
            fits = false;
        }
        return fits;
    }

    // OFFSET 0x02029D80
    public static bool FashionCase_HasBackdrop(FashionCase fashionCase, uint backdrop)
    {
        if (FashionCase_GetBackdropSlot(fashionCase, backdrop) != NoBackdropSlot)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02029D94
    public static uint FashionCase_GetAccessoryCount(FashionCase fashionCase, uint accessory)
    {
        if (accessory >= AccessoryCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (FashionCase_IsStackable(accessory))
        {
            return FashionCase_ReadStackCount(fashionCase.StackCounts, (byte)accessory);
        }
        if (accessory < StackableAccessoryCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return FashionCase_ReadSingleFlag(fashionCase, (byte)(accessory - StackableAccessoryCount));
    }

    // OFFSET 0x02029DD4
    public static uint FashionCase_GetBackdropSlot(FashionCase fashionCase, uint backdrop)
    {
        if (backdrop >= BackdropCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return FashionCase_ReadBackdropSlot(fashionCase.BackdropSlots, (byte)backdrop);
    }

    // OFFSET 0x02029DF0
    public static uint FashionCase_CountAccessories(FashionCase fashionCase)
    {
        uint total = 0;
        for (uint i = 0; i < AccessoryCount; i++)
        {
            total += FashionCase_GetAccessoryCount(fashionCase, i);
        }
        return total;
    }

    // OFFSET 0x02029E0C
    public static uint FashionCase_CountBackdrops(FashionCase fashionCase)
    {
        uint total = 0;
        for (uint i = 0; i < BackdropCount; i++)
        {
            if (FashionCase_GetBackdropSlot(fashionCase, i) != NoBackdropSlot)
            {
                total++;
            }
        }
        return total;
    }

    // OFFSET 0x02029E2C
    public static void FashionCase_AddAccessory(FashionCase fashionCase, uint accessory, uint count)
    {
        if (accessory >= AccessoryCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (FashionCase_IsStackable(accessory))
        {
            byte total = (byte)(FashionCase_ReadStackCount(fashionCase.StackCounts, (byte)accessory) + count);
            if (total > MaxStack)
            {
                total = MaxStack;
            }
            FashionCase_WriteStackCount(fashionCase.StackCounts, total, (byte)accessory);
            return;
        }
        byte owned = (byte)(FashionCase_ReadSingleFlag(fashionCase, (byte)accessory) + count);
        if (owned > 1)
        {
            owned = 1;
        }
        if (accessory < StackableAccessoryCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        FashionCase_WriteSingleFlag(fashionCase.SingleFlags, owned, (byte)(accessory - StackableAccessoryCount));
    }

    // OFFSET 0x02029EA0
    public static void FashionCase_RemoveAccessory(FashionCase fashionCase, uint accessory, uint count)
    {
        if (accessory >= AccessoryCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (FashionCase_IsStackable(accessory))
        {
            uint owned = FashionCase_ReadStackCount(fashionCase.StackCounts, (byte)accessory);
            uint remaining = 0;
            if (owned > count)
            {
                remaining = (byte)(owned - count);
            }
            FashionCase_WriteStackCount(fashionCase.StackCounts, remaining, (byte)accessory);
            return;
        }
        if (accessory < StackableAccessoryCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        FashionCase_WriteSingleFlag(fashionCase.SingleFlags, 0, (byte)(accessory - StackableAccessoryCount));
    }

    // OFFSET 0x02029EFC
    public static void FashionCase_AddBackdrop(FashionCase fashionCase, uint backdrop)
    {
        if (backdrop >= BackdropCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (FashionCase_ReadBackdropSlot(fashionCase.BackdropSlots, (byte)backdrop) == NoBackdropSlot)
        {
            byte slot = FashionCase_CountBackdropSlots(fashionCase.BackdropSlots);
            FashionCase_WriteBackdropSlot(fashionCase.BackdropSlots, slot, (byte)backdrop);
        }
    }
}

public sealed class ClipPhoto
{
    public const int Size = 0x74;
    public const int AccessorySlots = 10;
    public const uint SignatureEmpty = 0x1234;
    public const uint SignatureUsed = 0x2345;
    public const int PackedSize = 0x60;

    private const int MonOffset = 4;
    private const int MaskOffset = 0x3C;
    private const int TitleOffset = 0x40;
    private const int AccessoriesOffset = 0x48;
    private const int BackdropOffset = 0x70;
    private const int LanguageOffset = 0x71;
    private const int PackedOTNameOffset = 0xE;
    private const int PackedAccessoriesOffset = 0x30;
    private const ushort EndOfString = 0xFFFF;
    private const int CoordLimit = 0x100;
    private const int MinPriority = -128;

    public uint Signature;
    public readonly ClipMon Mon = new();
    public uint AccessoryMask;
    public readonly EasyChatSentence Title = new();
    public readonly ClipAccessory[] Accessories = new ClipAccessory[AccessorySlots];
    public byte Backdrop;
    public byte Language;

    public ClipPhoto()
    {
        for (int i = 0; i < AccessorySlots; i++)
        {
            Accessories[i] = new ClipAccessory();
        }
    }

    public void WriteTo(byte[] body, int offset)
    {
        Bin.W32(body, offset, Signature);
        Mon.WriteTo(body, offset + MonOffset);
        Bin.W32(body, offset + MaskOffset, AccessoryMask);
        Bin.W16(body, offset + TitleOffset, Title.Type);
        Bin.W16(body, offset + TitleOffset + 2, Title.ID);
        Bin.W16(body, offset + TitleOffset + 4, Title.Words[0]);
        Bin.W16(body, offset + TitleOffset + 6, Title.Words[1]);
        for (int i = 0; i < AccessorySlots; i++)
        {
            Accessories[i].WriteTo(body, offset + AccessoriesOffset + i * ClipAccessory.Size);
        }
        body[offset + BackdropOffset] = Backdrop;
        body[offset + LanguageOffset] = Language;
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Signature = Bin.U32(body, offset);
        Mon.ReadFrom(body, offset + MonOffset);
        AccessoryMask = Bin.U32(body, offset + MaskOffset);
        Title.Type = (ushort)Bin.U16(body, offset + TitleOffset);
        Title.ID = (ushort)Bin.U16(body, offset + TitleOffset + 2);
        Title.Words[0] = (ushort)Bin.U16(body, offset + TitleOffset + 4);
        Title.Words[1] = (ushort)Bin.U16(body, offset + TitleOffset + 6);
        for (int i = 0; i < AccessorySlots; i++)
        {
            Accessories[i].ReadFrom(body, offset + AccessoriesOffset + i * ClipAccessory.Size);
        }
        Backdrop = body[offset + BackdropOffset];
        Language = body[offset + LanguageOffset];
    }

    public byte[] ToBytes()
    {
        byte[] buffer = new byte[Size];
        WriteTo(buffer, 0);
        return buffer;
    }

    private static void CheckSignature(ClipPhoto photo)
    {
        if (!ImageClips.ImageClips_IsValidSignature(photo.Signature))
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02029C60
    public static int ClipPhoto_Size()
    {
        return Size;
    }

    // OFFSET 0x02029C68
    public static ClipPhoto ClipPhoto_New(int heapID)
    {
        ClipPhoto photo = new();
        photo.Signature = SignatureEmpty;
        return photo;
    }

    // OFFSET 0x02029F34
    public static bool ClipPhoto_HasData(ClipPhoto photo)
    {
        CheckSignature(photo);
        if (photo.Signature == SignatureUsed)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02029F5C
    public static void ClipPhoto_SetHasData(ClipPhoto photo)
    {
        CheckSignature(photo);
        photo.Signature = SignatureUsed;
        photo.Language = TrainerInfo.LanguageEnglish;
    }

    // OFFSET 0x02029F84
    public static void ClipPhoto_Clear(ClipPhoto photo)
    {
        CheckSignature(photo);
        photo.ReadFrom(new byte[Size], 0);
        photo.Signature = SignatureEmpty;
    }

    // OFFSET 0x02029FAC
    public static void ClipPhoto_SetMon(ClipPhoto photo, Pokemon mon, object clipSprite)
    {
        CheckSignature(photo);
        ClipMon.ClipMon_SetFromSprite(photo.Mon, mon, clipSprite);
    }

    // OFFSET 0x02029FD0
    public static void ClipPhoto_SetAccessory(ClipPhoto photo, ClipAccessorySource source, int slot)
    {
        (short x, short y) = SoftwareSprite.SoftwareSprite_GetPosition(source.Sprite!);
        int priority = SoftwareSprite.SoftwareSprite_GetPriority(source.Sprite!);
        if (slot >= AccessorySlots)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (x >= CoordLimit)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (y >= CoordLimit)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (priority <= MinPriority)
        {
            ErrorHandling.GF_AssertFail();
        }
        if ((photo.AccessoryMask & (1u << slot)) != 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        CheckSignature(photo);
        ClipAccessory.ClipAccessory_Set(photo.Accessories[slot], (byte)source.ID, (byte)x, (byte)y, (byte)priority);
        photo.AccessoryMask |= 1u << slot;
    }

    // OFFSET 0x0202A084
    public static void ClipPhoto_SetBackdrop(ClipPhoto photo, byte backdrop)
    {
        CheckSignature(photo);
        photo.Backdrop = backdrop;
    }

    // OFFSET 0x0202A0A0
    public static void ClipPhoto_SetTitleWord(ClipPhoto photo, ushort word)
    {
        CheckSignature(photo);
        EasyChatSentence.EasyChatSentence_Init(photo.Title);
        EasyChatSentence.EasyChatSentence_SetWord(photo.Title, 0, word);
    }

    // OFFSET 0x0202A0CC
    public static void ClipPhoto_Copy(ClipPhoto destination, ClipPhoto source)
    {
        CheckSignature(destination);
        destination.ReadFrom(source.ToBytes(), 0);
    }

    // OFFSET 0x0202A0EC
    public static void ClipPhoto_SetOT(ClipPhoto photo, GameString name, int gender)
    {
        CheckSignature(photo);
        ClipMon.ClipMon_SetOT(photo.Mon, name, gender);
    }

    // OFFSET 0x0202A110
    public static uint ClipPhoto_HasAccessory(ClipPhoto photo, int slot)
    {
        if (slot >= AccessorySlots)
        {
            ErrorHandling.GF_AssertFail();
        }
        CheckSignature(photo);
        return photo.AccessoryMask & (1u << slot);
    }

    // OFFSET 0x0202A138
    public static ClipMon ClipPhoto_GetMon(ClipPhoto photo)
    {
        CheckSignature(photo);
        return photo.Mon;
    }

    // OFFSET 0x0202A150
    public static ClipAccessory ClipPhoto_GetAccessory(ClipPhoto photo, int slot)
    {
        if (slot >= AccessorySlots)
        {
            ErrorHandling.GF_AssertFail();
        }
        if ((photo.AccessoryMask & (1u << slot)) == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        CheckSignature(photo);
        return photo.Accessories[slot];
    }

    // OFFSET 0x0202A184
    public static int ClipPhoto_Species(ClipPhoto photo)
    {
        CheckSignature(photo);
        return ClipMon.ClipMon_Species(photo.Mon);
    }

    // OFFSET 0x0202A1A0
    public static void ClipPhoto_OTName(ClipPhoto photo, GameString name)
    {
        CheckSignature(photo);
        ClipMon.ClipMon_OTName(photo.Mon, name);
    }

    // OFFSET 0x0202A1C0
    public static int ClipPhoto_OTGender(ClipPhoto photo)
    {
        CheckSignature(photo);
        return ClipMon.ClipMon_OTGender(photo.Mon);
    }

    // OFFSET 0x0202A1DC
    public static int ClipPhoto_Backdrop(ClipPhoto photo)
    {
        CheckSignature(photo);
        return photo.Backdrop;
    }

    // OFFSET 0x0202A1F4
    public static ushort ClipPhoto_TitleWord(ClipPhoto photo)
    {
        return EasyChatSentence.EasyChatSentence_GetWord(photo.Title, 0);
    }

    // OFFSET 0x0202A200
    public static int ClipPhoto_Language(ClipPhoto photo)
    {
        CheckSignature(photo);
        return photo.Language;
    }

    // OFFSET 0x0202A75C
    public static void ClipPhoto_Pack(ClipPhoto photo, byte[] packed)
    {
        Array.Clear(packed, 0, PackedSize);
        Bin.W32(packed, 0, photo.Signature);
        Bin.W32(packed, 4, photo.Mon.Personality);
        Bin.W32(packed, 8, photo.Mon.OTID);
        Bin.W16(packed, 0xC, photo.Mon.Species);
        for (int i = 0; i < ClipMon.OTNameLength; i++)
        {
            Bin.W16(packed, PackedOTNameOffset + i * 2, photo.Mon.OTName[i]);
        }
        packed[0x1E] = (byte)photo.Mon.Priority;
        packed[0x1F] = photo.Mon.X;
        packed[0x20] = photo.Mon.Y;
        packed[0x21] = photo.Mon.Form;
        Bin.W32(packed, 0x24, photo.AccessoryMask);
        Bin.W16(packed, 0x28, photo.Title.Type);
        Bin.W16(packed, 0x2A, photo.Title.ID);
        Bin.W16(packed, 0x2C, photo.Title.Words[0]);
        Bin.W16(packed, 0x2E, photo.Title.Words[1]);
        for (int i = 0; i < AccessorySlots; i++)
        {
            photo.Accessories[i].WriteTo(packed, PackedAccessoriesOffset + i * ClipAccessory.Size);
        }
        packed[0x58] = photo.Backdrop;
        packed[0x59] = photo.Language;
    }

    // OFFSET 0x0202A824
    public static void ClipPhoto_Unpack(byte[] packed, ClipPhoto photo)
    {
        photo.ReadFrom(new byte[ClipPhoto_Size()], 0);
        photo.Signature = Bin.U32(packed, 0);
        photo.Mon.Personality = Bin.U32(packed, 4);
        photo.Mon.OTID = Bin.U32(packed, 8);
        photo.Mon.Species = (ushort)Bin.U16(packed, 0xC);
        for (int i = 0; i < ClipMon.OTNameLength; i++)
        {
            photo.Mon.OTName[i] = (ushort)Bin.U16(packed, PackedOTNameOffset + i * 2);
        }
        photo.Mon.Priority = (sbyte)packed[0x1E];
        photo.Mon.X = packed[0x1F];
        photo.Mon.Y = packed[0x20];
        photo.Mon.Form = packed[0x21];
        photo.AccessoryMask = Bin.U32(packed, 0x24);
        photo.Title.Type = (ushort)Bin.U16(packed, 0x28);
        photo.Title.ID = (ushort)Bin.U16(packed, 0x2A);
        photo.Title.Words[0] = (ushort)Bin.U16(packed, 0x2C);
        photo.Title.Words[1] = (ushort)Bin.U16(packed, 0x2E);
        for (int i = 0; i < AccessorySlots; i++)
        {
            photo.Accessories[i].ReadFrom(packed, PackedAccessoriesOffset + i * ClipAccessory.Size);
        }
        photo.Backdrop = packed[0x58];
        photo.Language = packed[0x59];
        for (int i = 0; i < ClipMon.NicknameLength; i++)
        {
            photo.Mon.Nickname[i] = EndOfString;
        }
        photo.Mon.OTGender = 0;
    }
}

public sealed class ContestClip
{
    public const int Size = 0x98;
    public const int AccessorySlots = 20;

    private const int ContestOffset = 4;
    private const int MonOffset = 8;
    private const int MaskOffset = 0x40;
    private const int AccessoriesOffset = 0x44;
    private const int BackdropOffset = 0x94;
    private const int CoordLimit = 0x100;
    private const int MinPriority = -128;
    private const int MonSpriteX = 0x80;
    private const int MonSpriteBaseY = 0x8D;
    private const int MonSpriteHalfHeight = 0x28;
    private const int MonFaceFront = 2;

    public uint Signature;
    public uint ContestType;
    public readonly ClipMon Mon = new();
    public uint AccessoryMask;
    public readonly ClipAccessory[] Accessories = new ClipAccessory[AccessorySlots];
    public byte Backdrop;

    public ContestClip()
    {
        for (int i = 0; i < AccessorySlots; i++)
        {
            Accessories[i] = new ClipAccessory();
        }
    }

    public void WriteTo(byte[] body, int offset)
    {
        Bin.W32(body, offset, Signature);
        Bin.W32(body, offset + ContestOffset, ContestType);
        Mon.WriteTo(body, offset + MonOffset);
        Bin.W32(body, offset + MaskOffset, AccessoryMask);
        for (int i = 0; i < AccessorySlots; i++)
        {
            Accessories[i].WriteTo(body, offset + AccessoriesOffset + i * ClipAccessory.Size);
        }
        body[offset + BackdropOffset] = Backdrop;
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Signature = Bin.U32(body, offset);
        ContestType = Bin.U32(body, offset + ContestOffset);
        Mon.ReadFrom(body, offset + MonOffset);
        AccessoryMask = Bin.U32(body, offset + MaskOffset);
        for (int i = 0; i < AccessorySlots; i++)
        {
            Accessories[i].ReadFrom(body, offset + AccessoriesOffset + i * ClipAccessory.Size);
        }
        Backdrop = body[offset + BackdropOffset];
    }

    private static void CheckSignature(ContestClip clip)
    {
        if (!ImageClips.ImageClips_IsValidSignature(clip.Signature))
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02029BD8
    private static void ContestClip_CalcMonPlacement(Pokemon mon, out byte x, out byte y)
    {
        int height = sub_020765B8(mon, MonFaceFront);
        x = MonSpriteX;
        y = (byte)(MonSpriteBaseY - (MonSpriteHalfHeight - height));
        y = (byte)(y + MonSpriteHalfHeight);
    }

    // OFFSET 0x02029C64
    public static int ContestClip_Size()
    {
        return Size;
    }

    // OFFSET 0x02029C88
    public static ContestClip ContestClip_New(int heapID)
    {
        ContestClip clip = new();
        clip.Signature = ClipPhoto.SignatureEmpty;
        return clip;
    }

    // OFFSET 0x0202A218
    public static bool ContestClip_HasData(ContestClip clip)
    {
        CheckSignature(clip);
        if (clip.Signature == ClipPhoto.SignatureUsed)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0202A240
    public static void ContestClip_SetHasData(ContestClip clip)
    {
        CheckSignature(clip);
        clip.Signature = ClipPhoto.SignatureUsed;
    }

    // OFFSET 0x0202A25C
    public static void ContestClip_Clear(ContestClip clip)
    {
        CheckSignature(clip);
        clip.ReadFrom(new byte[Size], 0);
        clip.Signature = ClipPhoto.SignatureEmpty;
    }

    // OFFSET 0x0202A284
    public static void ContestClip_SetMonFromSprite(ContestClip clip, Pokemon mon, object clipSprite)
    {
        CheckSignature(clip);
        ClipMon.ClipMon_SetFromSprite(clip.Mon, mon, clipSprite);
    }

    // OFFSET 0x0202A2A8
    public static void ContestClip_SetAccessoryFromSprite(ContestClip clip, ClipAccessorySource source, int slot)
    {
        (short x, short y) = SoftwareSprite.SoftwareSprite_GetPosition(source.Sprite!);
        int priority = SoftwareSprite.SoftwareSprite_GetPriority(source.Sprite!);
        if (slot >= AccessorySlots)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (x >= CoordLimit)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (y >= CoordLimit)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (priority <= MinPriority)
        {
            ErrorHandling.GF_AssertFail();
        }
        if ((clip.AccessoryMask & (1u << slot)) != 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        CheckSignature(clip);
        ClipAccessory.ClipAccessory_Set(clip.Accessories[slot], (byte)source.ID, (byte)x, (byte)y, (byte)priority);
        clip.AccessoryMask |= 1u << slot;
    }

    // OFFSET 0x0202A35C
    public static void ContestClip_SetBackdrop(ContestClip clip, byte backdrop)
    {
        CheckSignature(clip);
        clip.Backdrop = backdrop;
    }

    // OFFSET 0x0202A378
    public static void ContestClip_SetContestType(ContestClip clip, uint contestType)
    {
        CheckSignature(clip);
        clip.ContestType = contestType;
    }

    // OFFSET 0x0202A390
    public static void ContestClip_Copy(ContestClip destination, ContestClip source)
    {
        CheckSignature(destination);
        byte[] buffer = new byte[Size];
        source.WriteTo(buffer, 0);
        destination.ReadFrom(buffer, 0);
    }

    // OFFSET 0x0202A3B0
    public static void ContestClip_SetMon(ContestClip clip, Pokemon mon, int priority)
    {
        CheckSignature(clip);
        ContestClip_CalcMonPlacement(mon, out byte x, out byte y);
        ClipMon.ClipMon_SetFromPokemon(clip.Mon, mon, x, y, (sbyte)priority);
    }

    // OFFSET 0x0202A3EC
    public static void ContestClip_SetAccessory(ContestClip clip, uint slot, uint accessory, int x, int y, int priority)
    {
        if (slot >= AccessorySlots)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (accessory >= FashionCase.AccessoryCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (x >= CoordLimit)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (y >= CoordLimit)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (priority <= MinPriority)
        {
            ErrorHandling.GF_AssertFail();
        }
        if ((clip.AccessoryMask & (1u << (int)slot)) != 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        CheckSignature(clip);
        if (clip.Mon.Priority >= priority)
        {
            priority = (sbyte)(clip.Mon.Priority + 1);
        }
        ClipAccessory.ClipAccessory_Set(clip.Accessories[slot], (byte)accessory, (byte)x, (byte)y, (byte)priority);
        clip.AccessoryMask |= 1u << (int)slot;
    }

    // OFFSET 0x0202A488
    public static bool ContestClip_HasAccessory(ContestClip clip, int slot)
    {
        if (slot >= AccessorySlots)
        {
            ErrorHandling.GF_AssertFail();
        }
        CheckSignature(clip);
        if ((clip.AccessoryMask & (1u << slot)) != 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0202A4B4
    public static void ContestClip_SetOT(ContestClip clip, GameString name, int gender)
    {
        CheckSignature(clip);
        ClipMon.ClipMon_SetOT(clip.Mon, name, gender);
    }

    // OFFSET 0x0202A4D8
    public static ClipMon ContestClip_GetMon(ContestClip clip)
    {
        CheckSignature(clip);
        return clip.Mon;
    }

    // OFFSET 0x0202A4F0
    public static ClipAccessory ContestClip_GetAccessory(ContestClip clip, int slot)
    {
        if (slot >= AccessorySlots)
        {
            ErrorHandling.GF_AssertFail();
        }
        if ((clip.AccessoryMask & (1u << slot)) == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        CheckSignature(clip);
        return clip.Accessories[slot];
    }

    // OFFSET 0x0202A524
    public static void ContestClip_OTName(ContestClip clip, GameString name)
    {
        CheckSignature(clip);
        ClipMon.ClipMon_OTName(clip.Mon, name);
    }

    // OFFSET 0x0202A544
    public static int ContestClip_OTGender(ContestClip clip)
    {
        CheckSignature(clip);
        return ClipMon.ClipMon_OTGender(clip.Mon);
    }

    // OFFSET 0x0202A560
    public static void ContestClip_BuildPokemon(ContestClip clip, Pokemon mon)
    {
        CheckSignature(clip);
        ClipMon.ClipMon_BuildPokemon(clip.Mon, mon);
    }

    // OFFSET 0x0202A580
    public static int ContestClip_AccessoryID(ContestClip clip, int slot)
    {
        if (slot >= AccessorySlots)
        {
            ErrorHandling.GF_AssertFail();
        }
        if ((clip.AccessoryMask & (1u << slot)) == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        CheckSignature(clip);
        return ClipAccessory.ClipAccessory_ID(clip.Accessories[slot]);
    }

    // OFFSET 0x0202A5B8
    public static int ContestClip_Backdrop(ContestClip clip)
    {
        CheckSignature(clip);
        return clip.Backdrop;
    }

    // OFFSET 0x0202A5D0
    public static uint ContestClip_ContestType(ContestClip clip)
    {
        CheckSignature(clip);
        return clip.ContestType;
    }
}

public sealed class ImageClips : ISaveBlock
{
    public const int PhotoCount = 11;
    public const int ContestClipCount = 5;

    private const int ContestClipsOffset = 0x4FC;
    private const int FashionCaseOffset = 0x7F4;
    private const uint CrcPolynomial = 0xEDB88320;

    public readonly ClipPhoto[] Photos = new ClipPhoto[PhotoCount];
    public readonly ContestClip[] ContestClips = new ContestClip[ContestClipCount];
    public readonly FashionCase FashionCase = new();

    public ImageClips()
    {
        for (int i = 0; i < PhotoCount; i++)
        {
            Photos[i] = new ClipPhoto();
        }
        for (int i = 0; i < ContestClipCount; i++)
        {
            ContestClips[i] = new ContestClip();
        }
    }

    public void WriteTo(byte[] body, int offset)
    {
        for (int i = 0; i < PhotoCount; i++)
        {
            Photos[i].WriteTo(body, offset + i * ClipPhoto.Size);
        }
        for (int i = 0; i < ContestClipCount; i++)
        {
            ContestClips[i].WriteTo(body, offset + ContestClipsOffset + i * ContestClip.Size);
        }
        FashionCase.WriteTo(body, offset + FashionCaseOffset);
    }

    public void ReadFrom(byte[] body, int offset)
    {
        for (int i = 0; i < PhotoCount; i++)
        {
            Photos[i].ReadFrom(body, offset + i * ClipPhoto.Size);
        }
        for (int i = 0; i < ContestClipCount; i++)
        {
            ContestClips[i].ReadFrom(body, offset + ContestClipsOffset + i * ContestClip.Size);
        }
        FashionCase.ReadFrom(body, offset + FashionCaseOffset);
    }

    private static uint Crc32(byte[] data)
    {
        uint crc = 0xFFFFFFFF;
        for (int i = 0; i < data.Length; i++)
        {
            crc ^= data[i];
            for (int bit = 0; bit < 8; bit++)
            {
                crc = (crc & 1) != 0 ? (crc >> 1) ^ CrcPolynomial : crc >> 1;
            }
        }
        return ~crc;
    }

    // OFFSET 0x020298BC
    public static bool ImageClips_IsValidSignature(uint signature)
    {
        if (signature == ClipPhoto.SignatureEmpty || signature == ClipPhoto.SignatureUsed)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02029BFC
    public static void ImageClips_Init(ImageClips imageClips)
    {
        for (int i = 0; i < PhotoCount; i++)
        {
            imageClips.Photos[i].ReadFrom(new byte[ClipPhoto.Size], 0);
            imageClips.Photos[i].Signature = ClipPhoto.SignatureEmpty;
        }
        for (int i = 0; i < ContestClipCount; i++)
        {
            imageClips.ContestClips[i].ReadFrom(new byte[ContestClip.Size], 0);
            imageClips.ContestClips[i].Signature = ClipPhoto.SignatureEmpty;
        }
        FashionCase.FashionCase_Init(imageClips.FashionCase);
    }

    // OFFSET 0x02029CA8
    public static ClipPhoto ImageClips_GetPhoto(ImageClips imageClips, int index)
    {
        if (index >= PhotoCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (!ImageClips_IsValidSignature(imageClips.Photos[index].Signature))
        {
            ErrorHandling.GF_AssertFail();
        }
        return imageClips.Photos[index];
    }

    // OFFSET 0x02029CD0
    public static ContestClip ImageClips_GetContestClip(ImageClips imageClips, int index)
    {
        if (index >= ContestClipCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (!ImageClips_IsValidSignature(imageClips.ContestClips[index].Signature))
        {
            ErrorHandling.GF_AssertFail();
        }
        return imageClips.ContestClips[index];
    }

    // OFFSET 0x02029D04
    public static FashionCase ImageClips_GetFashionCase(ImageClips imageClips)
    {
        return imageClips.FashionCase;
    }

    // OFFSET 0x02029D10
    public static bool ImageClips_PhotoHasData(ImageClips imageClips, int index)
    {
        if (index >= PhotoCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return ClipPhoto.ClipPhoto_HasData(imageClips.Photos[index]);
    }

    // OFFSET 0x02029D2C
    public static bool ImageClips_ContestClipHasData(ImageClips imageClips, int index)
    {
        if (index >= ContestClipCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return ContestClip.ContestClip_HasData(imageClips.ContestClips[index]);
    }

    // OFFSET 0x0202A638
    private static bool ImageClips_IsNewPhoto(ImageClips imageClips, ClipPhoto photo)
    {
        bool isNew = true;
        if (ClipPhoto.ClipPhoto_HasData(photo))
        {
            uint checksum = Crc32(photo.ToBytes());
            for (int i = 0; i < PhotoCount; i++)
            {
                ClipPhoto stored = ImageClips_GetPhoto(imageClips, i);
                if (Crc32(stored.ToBytes()) == checksum)
                {
                    isNew = false;
                    break;
                }
            }
        }
        else
        {
            isNew = false;
        }
        return isNew;
    }

    // OFFSET 0x0202A6A8
    public static void ImageClips_ReceivePhotos(int count, int ownIndex, ImageClips imageClips, ClipPhoto?[] photos)
    {
        int newCount = 0;
        for (int i = 0; i < count; i++)
        {
            if (i != ownIndex && photos[i] != null && ImageClips_IsNewPhoto(imageClips, photos[i]!))
            {
                newCount++;
            }
        }
        for (int i = PhotoCount - 1; i >= 1; i--)
        {
            if (i + newCount < PhotoCount)
            {
                ClipPhoto destination = ImageClips_GetPhoto(imageClips, i + newCount);
                ClipPhoto.ClipPhoto_Copy(destination, ImageClips_GetPhoto(imageClips, i));
            }
        }
        int slot = 1;
        for (int i = 0; i < count; i++)
        {
            if (i != ownIndex && photos[i] != null && ImageClips_IsNewPhoto(imageClips, photos[i]!))
            {
                ClipPhoto.ClipPhoto_Copy(ImageClips_GetPhoto(imageClips, slot), photos[i]!);
                slot++;
            }
        }
    }

    // OFFSET 0x0202A750
    public static ImageClips SaveData_GetImageClips(SaveData saveData)
    {
        return (ImageClips)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryImageClips);
    }
}
