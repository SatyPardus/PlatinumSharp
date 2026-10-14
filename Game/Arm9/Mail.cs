using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class Mail
{
    public const int Size = 0x38;
    public const int NameLength = 8;
    public const int IconCount = 3;
    public const int SentenceCount = 3;
    public const int MailTypeCount = 12;
    public const byte MailTypeNone = 0xFF;
    public const int IconDataIcon = 0;
    public const int IconDataPalette = 1;
    public const int IconDataRaw = 2;

    private const int NameOffset = 8;
    private const int IconsOffset = 0x18;
    private const int FormBitsOffset = 0x1E;
    private const int SentencesOffset = 0x20;
    private const int SentenceSize = 8;
    private const ushort IconMask = 0xFFF;
    private const int PaletteShift = 12;
    private const ushort NoIcon = 0xFFFF;
    private const int FormBitsPerIcon = 5;
    private const int FormBitsMask = 0x1F;
    private const int MaxIcon = 0x222;
    private const int FallbackIcon = 7;
    private const uint FormIconTable = 0x020E5B0C;
    private const int FormIconCount = 7;
    private const int FormIconSize = 8;
    private const int FormIconAlternate = 0;
    private const int FormIconBase = 2;
    private const int FormIconSpecies = 4;
    private const int FormIconForm = 6;
    private const int MonDataSpecies = 5;
    private const int MonDataIsEgg = 0x4C;
    private const int MonDataForm = 0x70;

    public uint TrainerID;
    public byte Gender;
    public byte Language;
    public byte Version;
    public byte MailType;
    public readonly ushort[] Name = new ushort[NameLength];
    public readonly ushort[] Icons = new ushort[IconCount];
    public ushort FormBits;
    public readonly EasyChatSentence[] Sentences = { new EasyChatSentence(), new EasyChatSentence(), new EasyChatSentence() };

    private static byte[]? sFormIcons;

    private static byte[] FormIcons => sFormIcons ??= RomData.Arm9(FormIconTable, FormIconCount * FormIconSize);

    public void WriteTo(byte[] body, int offset)
    {
        Bin.W32(body, offset, TrainerID);
        body[offset + 4] = Gender;
        body[offset + 5] = Language;
        body[offset + 6] = Version;
        body[offset + 7] = MailType;
        for (int i = 0; i < NameLength; i++)
        {
            Bin.W16(body, offset + NameOffset + i * 2, Name[i]);
        }
        for (int i = 0; i < IconCount; i++)
        {
            Bin.W16(body, offset + IconsOffset + i * 2, Icons[i]);
        }
        Bin.W16(body, offset + FormBitsOffset, FormBits);
        for (int i = 0; i < SentenceCount; i++)
        {
            int position = offset + SentencesOffset + i * SentenceSize;
            Bin.W16(body, position, Sentences[i].Type);
            Bin.W16(body, position + 2, Sentences[i].ID);
            Bin.W16(body, position + 4, Sentences[i].Words[0]);
            Bin.W16(body, position + 6, Sentences[i].Words[1]);
        }
    }

    public void ReadFrom(byte[] body, int offset)
    {
        TrainerID = Bin.U32(body, offset);
        Gender = body[offset + 4];
        Language = body[offset + 5];
        Version = body[offset + 6];
        MailType = body[offset + 7];
        for (int i = 0; i < NameLength; i++)
        {
            Name[i] = (ushort)Bin.U16(body, offset + NameOffset + i * 2);
        }
        for (int i = 0; i < IconCount; i++)
        {
            Icons[i] = (ushort)Bin.U16(body, offset + IconsOffset + i * 2);
        }
        FormBits = (ushort)Bin.U16(body, offset + FormBitsOffset);
        for (int i = 0; i < SentenceCount; i++)
        {
            int position = offset + SentencesOffset + i * SentenceSize;
            Sentences[i].Type = (ushort)Bin.U16(body, position);
            Sentences[i].ID = (ushort)Bin.U16(body, position + 2);
            Sentences[i].Words[0] = (ushort)Bin.U16(body, position + 4);
            Sentences[i].Words[1] = (ushort)Bin.U16(body, position + 6);
        }
    }

    public void Clear()
    {
        ReadFrom(new byte[Size], 0);
    }

    private static int FormIconU16(int entry, int field)
    {
        return Bin.U16(FormIcons, entry * FormIconSize + field);
    }

    private static int FormIconFormValue(int entry)
    {
        return FormIcons[entry * FormIconSize + FormIconForm];
    }

    private static ushort WithIcon(ushort value, uint icon)
    {
        return (ushort)((uint)(value & 0xF000) | (icon & IconMask));
    }

    private static ushort WithPalette(ushort value, uint palette)
    {
        return (ushort)((uint)(value & IconMask) | ((palette & 0xF) << PaletteShift));
    }

    // OFFSET 0x02028124
    public static void Mail_Init(Mail mail)
    {
        mail.TrainerID = 0;
        mail.Gender = 0;
        mail.Language = TrainerInfo.LanguageEnglish;
        mail.Version = TrainerInfo.GameCodePlatinum;
        mail.MailType = MailTypeNone;
        CharCode.CharCode_FillWithEOS(mail.Name, 0, NameLength);
        for (int i = 0; i < IconCount; i++)
        {
            mail.Icons[i] = NoIcon;
        }
        mail.FormBits = 0;
        for (int i = 0; i < SentenceCount; i++)
        {
            EasyChatSentence.EasyChatSentence_Init(mail.Sentences[i]);
        }
    }

    // OFFSET 0x0202817C
    public static bool Mail_IsUsed(Mail mail)
    {
        if (mail.MailType < MailTypeCount)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0202818C
    public static Mail Mail_New(int heapID)
    {
        Mail mail = new();
        Mail_Init(mail);
        return mail;
    }

    // OFFSET 0x020281A0
    public static void Mail_Copy(Mail source, Mail destination)
    {
        byte[] buffer = new byte[Size];
        source.WriteTo(buffer, 0);
        destination.ReadFrom(buffer, 0);
    }

    // OFFSET 0x020281AC
    public static void Mail_SetFromSaveData(Mail mail, byte mailType, byte partySlot, SaveData saveData)
    {
        Mail_Init(mail);
        mail.MailType = mailType;
        Party party = Party.SaveData_GetParty(saveData);
        TrainerInfo info = PlayerSave.SaveData_GetTrainerInfo(saveData);
        CharCode.CharCode_Copy(mail.Name, 0, TrainerInfo.TrainerInfo_Name(info), 0);
        mail.Gender = (byte)TrainerInfo.TrainerInfo_Gender(info);
        mail.TrainerID = TrainerInfo.TrainerInfo_ID(info);
        byte slot = 0;
        mail.FormBits = 0;
        for (byte i = partySlot; i < Party.Party_GetCurrentCount(party); i++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, i);
            ushort species = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null);
            uint isEgg = Pokemon.Pokemon_GetValue(mon, MonDataIsEgg, null);
            uint form = Pokemon.Pokemon_GetValue(mon, MonDataForm, null);
            uint icon = PokemonIcon.Pokemon_IconSpriteIndex(mon);
            uint palette = PokemonIcon.PokeIconPaletteIndex(species, form, isEgg);
            mail.Icons[slot] = WithIcon(mail.Icons[slot], icon);
            mail.Icons[slot] = WithPalette(mail.Icons[slot], palette);
            for (byte entry = 0; entry < FormIconCount; entry++)
            {
                if (FormIconU16(entry, FormIconBase) == (mail.Icons[slot] & IconMask) && form == FormIconFormValue(entry))
                {
                    mail.Icons[slot] = WithIcon(mail.Icons[slot], (uint)FormIconU16(entry, FormIconAlternate));
                    mail.Icons[slot] = WithPalette(mail.Icons[slot], PokemonIcon.PokeIconPaletteIndex(species, 0, isEgg));
                    mail.FormBits |= (ushort)(FormIconFormValue(entry) << (slot * FormBitsPerIcon));
                    break;
                }
            }
            slot++;
            if (slot >= IconCount)
            {
                break;
            }
        }
    }

    // OFFSET 0x02028308
    public static uint Mail_TrainerID(Mail mail)
    {
        return mail.TrainerID;
    }

    // OFFSET 0x0202830C
    public static ushort[] Mail_Name(Mail mail)
    {
        return mail.Name;
    }

    // OFFSET 0x02028310
    public static byte Mail_Gender(Mail mail)
    {
        return mail.Gender;
    }

    // OFFSET 0x02028314
    public static byte Mail_MailType(Mail mail)
    {
        return mail.MailType;
    }

    // OFFSET 0x02028318
    public static void Mail_SetMailType(Mail mail, byte mailType)
    {
        if (mailType < MailTypeCount)
        {
            mail.MailType = mailType;
        }
    }

    // OFFSET 0x02028320
    public static byte Mail_Language(Mail mail)
    {
        return mail.Language;
    }

    // OFFSET 0x02028324
    public static byte Mail_Version(Mail mail)
    {
        return mail.Version;
    }

    // OFFSET 0x02028328
    public static ushort Mail_IconData(Mail mail, byte slot, byte mode, ushort formBits)
    {
        if (slot >= IconCount)
        {
            return 0;
        }
        ushort value = mail.Icons[slot];
        int form = (formBits >> (slot * FormBitsPerIcon)) & FormBitsMask;
        for (int entry = 0; entry < FormIconCount; entry++)
        {
            if ((value & IconMask) == FormIconU16(entry, FormIconAlternate) && FormIconFormValue(entry) == form)
            {
                value = WithIcon(value, (uint)FormIconU16(entry, FormIconBase));
                value = WithPalette(value, PokemonIcon.PokeIconPaletteIndex((uint)FormIconU16(entry, FormIconSpecies), (uint)FormIconFormValue(entry), 0));
                break;
            }
        }
        if ((value & IconMask) > MaxIcon)
        {
            value = WithIcon(value, FallbackIcon);
            value = WithPalette(value, 0);
        }
        switch (mode)
        {
            case IconDataIcon:
                return (ushort)(value & IconMask);
            case IconDataPalette:
                return (ushort)(value >> PaletteShift);
            default:
                return value;
        }
    }

    // OFFSET 0x02028408
    public static ushort Mail_FormBits(Mail mail)
    {
        return mail.FormBits;
    }

    // OFFSET 0x0202840C
    public static EasyChatSentence Mail_Sentence(Mail mail, byte slot)
    {
        if (slot < SentenceCount)
        {
            return mail.Sentences[slot];
        }
        return mail.Sentences[0];
    }

    // OFFSET 0x0202841C
    public static void Mail_SetSentence(Mail mail, EasyChatSentence sentence, byte slot)
    {
        if (slot < SentenceCount)
        {
            EasyChatSentence.EasyChatSentence_Copy(mail.Sentences[slot], sentence);
        }
    }
}

public sealed class Mailbox : ISaveBlock
{
    public const int SlotCount = 20;
    public const int GroupMailbox = 0;
    public const int NoSlot = -1;

    public readonly Mail[] Mails = new Mail[SlotCount];

    public Mailbox()
    {
        for (int i = 0; i < SlotCount; i++)
        {
            Mails[i] = new Mail();
        }
    }

    public void WriteTo(byte[] body, int offset)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            Mails[i].WriteTo(body, offset + i * Mail.Size);
        }
    }

    public void ReadFrom(byte[] body, int offset)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            Mails[i].ReadFrom(body, offset + i * Mail.Size);
        }
    }

    // OFFSET 0x02028430
    public static Mailbox SaveData_GetMailbox(SaveData saveData)
    {
        return (Mailbox)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryMailbox);
    }

    // OFFSET 0x02028444
    public static void Mailbox_Init(Mailbox mailbox)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            Mail.Mail_Init(mailbox.Mails[i]);
        }
    }

    // OFFSET 0x0202845C
    public static int Mailbox_FindEmptySlot(Mailbox mailbox, int group)
    {
        if (group == GroupMailbox)
        {
            return Mailbox_FindFirstEmpty(mailbox.Mails, SlotCount);
        }
        return NoSlot;
    }

    // OFFSET 0x02028470
    public static void Mailbox_ClearSlot(Mailbox mailbox, int group, int slot)
    {
        Mail? mail = Mailbox_GetSlot(mailbox, group, slot);
        if (mail != null)
        {
            Mail.Mail_Init(mail);
        }
    }

    // OFFSET 0x02028480
    public static void Mailbox_SetSlot(Mailbox mailbox, int group, int slot, Mail source)
    {
        Mail? mail = Mailbox_GetSlot(mailbox, group, slot);
        if (mail != null)
        {
            Mail.Mail_Copy(source, mail);
        }
    }

    // OFFSET 0x02028494
    public static int Mailbox_CountUsedSlots(Mailbox mailbox, int group)
    {
        if (group == GroupMailbox)
        {
            return Mailbox_CountUsed(mailbox.Mails, SlotCount);
        }
        return 0;
    }

    // OFFSET 0x020284A8
    public static Mail Mailbox_NewMailFromSlot(Mailbox mailbox, int group, int slot, int heapID)
    {
        Mail? stored = Mailbox_GetSlot(mailbox, group, slot);
        Mail mail = Mail.Mail_New(heapID);
        if (stored != null)
        {
            Mail.Mail_Copy(stored, mail);
        }
        return mail;
    }

    // OFFSET 0x020284CC
    public static void Mailbox_CopySlot(Mailbox mailbox, int group, int slot, Mail destination)
    {
        Mail? stored = Mailbox_GetSlot(mailbox, group, slot);
        if (stored == null)
        {
            Mail.Mail_Init(destination);
        }
        else
        {
            Mail.Mail_Copy(stored, destination);
        }
    }

    // OFFSET 0x020284E8
    private static int Mailbox_FindFirstEmpty(Mail[] mails, int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (!Mail.Mail_IsUsed(mails[i]))
            {
                return i;
            }
        }
        return NoSlot;
    }

    // OFFSET 0x02028510
    private static int Mailbox_CountUsed(Mail[] mails, int count)
    {
        int used = 0;
        for (int i = 0; i < count; i++)
        {
            if (Mail.Mail_IsUsed(mails[i]))
            {
                used++;
            }
        }
        return used;
    }

    // OFFSET 0x02028538
    private static Mail? Mailbox_GetSlot(Mailbox mailbox, int group, int slot)
    {
        Mail? mail = null;
        if (group == GroupMailbox && slot < SlotCount)
        {
            mail = mailbox.Mails[slot];
        }
        return mail;
    }
}
