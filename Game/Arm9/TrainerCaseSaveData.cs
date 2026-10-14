using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed class TrainerCaseSaveData : ISaveBlock
{
    public const int Size = 0x620;
    public const int BadgeCount = 8;
    public const int SignatureSize = 0x600;
    public const uint InitialPolish = 140;

    private const int SignatureOffset = 0x20;

    public readonly uint[] BadgePolish = new uint[BadgeCount];
    public readonly byte[] Signature = new byte[SignatureSize];

    public void WriteTo(byte[] body, int offset)
    {
        for (int i = 0; i < BadgeCount; i++)
        {
            Bin.W32(body, offset + i * 4, BadgePolish[i]);
        }
        Array.Copy(Signature, 0, body, offset + SignatureOffset, SignatureSize);
    }

    public void ReadFrom(byte[] body, int offset)
    {
        for (int i = 0; i < BadgeCount; i++)
        {
            BadgePolish[i] = Bin.U32(body, offset + i * 4);
        }
        Array.Copy(body, offset + SignatureOffset, Signature, 0, SignatureSize);
    }

    // OFFSET 0x0202C804
    public static void TrainerCaseSaveData_Init(TrainerCaseSaveData tcSaveData)
    {
        Array.Clear(tcSaveData.BadgePolish);
        Array.Clear(tcSaveData.Signature);
        uint[] badges = TrainerCaseSaveData_GetBadges(tcSaveData);
        for (int i = 0; i < BadgeCount; i++)
        {
            TrainerCaseSaveDataBadge_SetPolish((byte)i, InitialPolish, badges);
        }
    }

    // OFFSET 0x0202C834
    public static TrainerCaseSaveData SaveData_GetTrainerCaseSaveData(SaveData saveData)
    {
        return (TrainerCaseSaveData)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryTrainerCase);
    }

    // OFFSET 0x0202C840
    public static byte[] TrainerCaseSaveData_GetSignature(TrainerCaseSaveData tcSaveData)
    {
        return tcSaveData.Signature;
    }

    // OFFSET 0x0202C844
    public static uint[] TrainerCaseSaveData_GetBadges(TrainerCaseSaveData tcSaveData)
    {
        return tcSaveData.BadgePolish;
    }

    // OFFSET 0x0202C848
    public static uint TrainerCaseSaveDataBadge_GetPolish(byte badgeID, uint[] badges)
    {
        return badges[badgeID];
    }

    // OFFSET 0x0202C850
    public static void TrainerCaseSaveDataBadge_SetPolish(byte badgeID, uint polish, uint[] badges)
    {
        badges[badgeID] = polish;
    }
}

public sealed class WiFiHistory : ISaveBlock
{
    public const int Size = 0xFF8;
    public const int CountryLimit = 256;
    public const int RegionLimit = 64;
    public const int RecencyNone = 0;
    public const int RecencyNew = 1;
    public const int RecencyOld = 2;
    public const int RecencyOwn = 3;
    public const int RecencyLimit = 4;

    private const int MapSize = 0xFF0;
    private const int MapOffset = 7;
    private const int BytesPerCountry = 16;
    private const int RegionsPerByte = 4;
    private const int CountryJapan = 103;

    public uint Reserved;
    public byte InteractedOutsideJapan;
    public byte Country;
    public byte Region;
    public readonly byte[] CommunicationMap = new byte[MapSize];

    public void WriteTo(byte[] body, int offset)
    {
        Bin.W32(body, offset, Reserved);
        body[offset + 4] = InteractedOutsideJapan;
        body[offset + 5] = Country;
        body[offset + 6] = Region;
        Array.Copy(CommunicationMap, 0, body, offset + MapOffset, MapSize);
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Reserved = Bin.U32(body, offset);
        InteractedOutsideJapan = body[offset + 4];
        Country = body[offset + 5];
        Region = body[offset + 6];
        Array.Copy(body, offset + MapOffset, CommunicationMap, 0, MapSize);
    }

    // OFFSET 0x0202C860
    public static void WiFiHistory_Init(WiFiHistory wiFiHistory)
    {
        wiFiHistory.Reserved = 0;
        wiFiHistory.InteractedOutsideJapan = 0;
        wiFiHistory.Country = 0;
        wiFiHistory.Region = 0;
        Array.Clear(wiFiHistory.CommunicationMap);
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryWiFiHistory);
    }

    // OFFSET 0x0202C878
    public static WiFiHistory SaveData_WiFiHistory(SaveData saveData)
    {
        SaveData.SaveData_Checksum(SaveDataTable.EntryWiFiHistory);
        return (WiFiHistory)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryWiFiHistory);
    }

    // OFFSET 0x0202C88C
    public static void WiFiHistory_SetCountryAndRegion(WiFiHistory wiFiHistory, int country, int region)
    {
        if (country >= CountryLimit)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (region >= RegionLimit)
        {
            ErrorHandling.GF_AssertFail();
        }
        wiFiHistory.Country = (byte)country;
        wiFiHistory.Region = (byte)region;
        WiFiHistory_SetGeonetCommunicatedWith(wiFiHistory, country, region, RecencyOwn);
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryWiFiHistory);
    }

    // OFFSET 0x0202C8C0
    public static int WiFiHistory_GetCountry(WiFiHistory wiFiHistory)
    {
        return wiFiHistory.Country;
    }

    // OFFSET 0x0202C8C4
    public static int WiFiHistory_GetRegion(WiFiHistory wiFiHistory)
    {
        return wiFiHistory.Region;
    }

    // OFFSET 0x0202C8C8
    public static int WiFiHistory_GetGeonetCommunicatedWith(WiFiHistory wiFiHistory, int country, int region)
    {
        if (country >= CountryLimit)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (region >= RegionLimit)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (country == 0)
        {
            return RecencyNone;
        }
        int index = (country - 1) * BytesPerCountry + region / RegionsPerByte;
        return (wiFiHistory.CommunicationMap[index] >> ((region % RegionsPerByte) * 2)) & 3;
    }

    // OFFSET 0x0202C918
    public static void WiFiHistory_SetGeonetCommunicatedWith(WiFiHistory wiFiHistory, int country, int region, int recency)
    {
        if (recency >= RecencyLimit)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (country >= CountryLimit)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (region >= RegionLimit)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (country == 0)
        {
            return;
        }
        int index = (country - 1) * BytesPerCountry + region / RegionsPerByte;
        int shift = (region % RegionsPerByte) * 2;
        wiFiHistory.CommunicationMap[index] &= (byte)(0xFF ^ (3 << shift));
        wiFiHistory.CommunicationMap[index] |= (byte)(recency << shift);
        if (country != CountryJapan)
        {
            WiFiHistory_SetInteractedOutsideJapan(wiFiHistory, 1);
        }
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryWiFiHistory);
    }

    // OFFSET 0x0202C990
    public static int WiFiHistory_HasInteractedOutsideJapan(WiFiHistory wiFiHistory)
    {
        return wiFiHistory.InteractedOutsideJapan;
    }

    // OFFSET 0x0202C994
    public static void WiFiHistory_SetInteractedOutsideJapan(WiFiHistory wiFiHistory, int interactedOutsideJapan)
    {
        wiFiHistory.InteractedOutsideJapan = (byte)interactedOutsideJapan;
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryWiFiHistory);
    }

    // OFFSET 0x0202C9A0
    public static void WiFiHistory_UpdateGeonetCommunicationMap(WiFiHistory wiFiHistory)
    {
        for (int i = 0; i < MapSize; i++)
        {
            byte value = wiFiHistory.CommunicationMap[i];
            for (int shift = 0; shift < 8; shift += 2)
            {
                if (((value >> shift) & 3) == RecencyNew)
                {
                    value = (byte)((value & ((3 << shift) ^ 0xFF)) | (RecencyOld << shift));
                }
            }
            wiFiHistory.CommunicationMap[i] = value;
        }
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryWiFiHistory);
    }
}

public sealed class BallSeal
{
    public const int Size = 3;

    public byte Type;
    public byte X;
    public byte Y;

    // OFFSET 0x0202CA7C
    public static byte BallSeal_GetSealType(BallSeal seal)
    {
        return seal.Type;
    }

    // OFFSET 0x0202CA80
    public static byte BallSeal_GetX(BallSeal seal)
    {
        return seal.X;
    }

    // OFFSET 0x0202CA84
    public static byte BallSeal_GetY(BallSeal seal)
    {
        return seal.Y;
    }
}

public sealed class BallCapsule
{
    public const int Size = 0x18;
    public const int SealCount = 8;

    public readonly BallSeal[] Seals = new BallSeal[SealCount];

    public BallCapsule()
    {
        for (int i = 0; i < SealCount; i++)
        {
            Seals[i] = new BallSeal();
        }
    }

    public void WriteTo(byte[] body, int offset)
    {
        for (int i = 0; i < SealCount; i++)
        {
            body[offset + i * BallSeal.Size] = Seals[i].Type;
            body[offset + i * BallSeal.Size + 1] = Seals[i].X;
            body[offset + i * BallSeal.Size + 2] = Seals[i].Y;
        }
    }

    public void ReadFrom(byte[] body, int offset)
    {
        for (int i = 0; i < SealCount; i++)
        {
            Seals[i].Type = body[offset + i * BallSeal.Size];
            Seals[i].X = body[offset + i * BallSeal.Size + 1];
            Seals[i].Y = body[offset + i * BallSeal.Size + 2];
        }
    }

    // OFFSET 0x0202CA10
    public static void BallCapsule_Copy(BallCapsule source, BallCapsule destination)
    {
        for (int i = 0; i < SealCount; i++)
        {
            destination.Seals[i].Type = source.Seals[i].Type;
            destination.Seals[i].X = source.Seals[i].X;
            destination.Seals[i].Y = source.Seals[i].Y;
        }
    }

    // OFFSET 0x0202CA64
    public static BallSeal BallCapsule_GetBallSeals(BallCapsule ballCapsule, int sealNum)
    {
        if (sealNum >= SealCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return ballCapsule.Seals[sealNum];
    }

    // OFFSET 0x0202CA94
    public static bool SealIsOnCapsule(BallCapsule ballCapsule, int sealId)
    {
        for (int i = 0; i < SealCount; i++)
        {
            if (sealId + 1 == ballCapsule.Seals[i].Type)
            {
                return true;
            }
        }
        return false;
    }
}

public sealed class SealCase : ISaveBlock
{
    public const int Size = 0x170;
    public const int CapsuleCount = 12;
    public const int SealTypeCount = 80;
    public const int MaxSealCount = 99;

    private const int CountsOffset = 0x120;

    public readonly BallCapsule[] Capsules = new BallCapsule[CapsuleCount];
    public readonly byte[] SealCounts = new byte[SealTypeCount];

    public SealCase()
    {
        for (int i = 0; i < CapsuleCount; i++)
        {
            Capsules[i] = new BallCapsule();
        }
    }

    public void WriteTo(byte[] body, int offset)
    {
        for (int i = 0; i < CapsuleCount; i++)
        {
            Capsules[i].WriteTo(body, offset + i * BallCapsule.Size);
        }
        Array.Copy(SealCounts, 0, body, offset + CountsOffset, SealTypeCount);
    }

    public void ReadFrom(byte[] body, int offset)
    {
        for (int i = 0; i < CapsuleCount; i++)
        {
            Capsules[i].ReadFrom(body, offset + i * BallCapsule.Size);
        }
        Array.Copy(body, offset + CountsOffset, SealCounts, 0, SealTypeCount);
    }

    // OFFSET 0x0202C9FC
    public static void SealCase_Init(SealCase sealCase)
    {
        sealCase.ReadFrom(new byte[SaveDataTable.SealCase_SaveSize()], 0);
    }

    // OFFSET 0x0202CA1C
    public static SealCase SaveData_GetSealCase(SaveData saveData)
    {
        return (SealCase)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntrySealCase);
    }

    // OFFSET 0x0202CA28
    public static BallCapsule SealCase_GetCapsuleById(SealCase sealCase, int capsuleId)
    {
        if (capsuleId >= CapsuleCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return sealCase.Capsules[capsuleId];
    }

    // OFFSET 0x0202CA40
    public static void SealCase_SetCapsuleById(SealCase sealCase, BallCapsule source, int capsuleId)
    {
        if (capsuleId >= CapsuleCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        BallCapsule.BallCapsule_Copy(source, SealCase_GetCapsuleById(sealCase, capsuleId));
    }

    // OFFSET 0x0202CA88
    public static byte[] SealCase_GetSealsObtained(SealCase sealCase)
    {
        return sealCase.SealCounts;
    }

    // OFFSET 0x0202CA90
    public static byte SealCase_GetSealCount(byte[] seals, int sealNum)
    {
        return seals[sealNum];
    }

    // OFFSET 0x0202CAB0
    public static int SealCase_CountSealOccurrenceInUse(SealCase sealCase, int sealId)
    {
        int count = 0;
        for (int capsule = 0; capsule < CapsuleCount; capsule++)
        {
            for (int seal = 0; seal < BallCapsule.SealCount; seal++)
            {
                if (sealId + 1 == sealCase.Capsules[capsule].Seals[seal].Type)
                {
                    count++;
                }
            }
        }
        return count;
    }

    // OFFSET 0x0202CADC
    public static void SealCase_SetSealQuantity(byte[] sealCounts, int sealId, int quantity)
    {
        sealCounts[sealId] = (byte)quantity;
    }

    // OFFSET 0x0202CAE0
    public static bool GiveOrTakeSeal(SealCase sealCase, int sealId, short quantity)
    {
        int owned = sealCase.SealCounts[sealId - 1];
        int total = SealCase_CountSealOccurrenceInUse(sealCase, sealId - 1) + owned;
        if (quantity < 0)
        {
            if (quantity + owned < 0)
            {
                return false;
            }
            sealCase.SealCounts[sealId - 1] = (byte)(quantity + owned);
        }
        else
        {
            if (total + quantity > MaxSealCount)
            {
                return false;
            }
            sealCase.SealCounts[sealId - 1] = (byte)(owned + quantity);
        }
        return true;
    }

    // OFFSET 0x0202CB20
    public static bool GiveOrTakeSeal2(SealCase sealCase, int sealId, short quantity)
    {
        int inUse = SealCase_CountSealOccurrenceInUse(sealCase, sealId - 1);
        if (quantity < 0)
        {
            int remaining = quantity + sealCase.SealCounts[sealId - 1];
            if (remaining < 0)
            {
                return false;
            }
            sealCase.SealCounts[sealId - 1] = (byte)remaining;
        }
        else
        {
            if (inUse + quantity > MaxSealCount)
            {
                return false;
            }
            sealCase.SealCounts[sealId - 1] = (byte)(sealCase.SealCounts[sealId - 1] + quantity);
            if (sealCase.SealCounts[sealId - 1] >= MaxSealCount)
            {
                sealCase.SealCounts[sealId - 1] = MaxSealCount;
            }
        }
        return true;
    }

    // OFFSET 0x0202CB70
    public static bool SealCase_CheckSealCount(SealCase sealCase, int sealId, short quantity)
    {
        int owned = sealCase.SealCounts[sealId - 1];
        int total = SealCase_CountSealOccurrenceInUse(sealCase, sealId - 1) + owned;
        if (quantity < 0)
        {
            if (quantity + owned < 0)
            {
                return false;
            }
            return true;
        }
        if (total + quantity > MaxSealCount)
        {
            return false;
        }
        return true;
    }

    // OFFSET 0x0202CBA8
    public static int SealCase_CountUniqueSeals(SealCase sealCase)
    {
        int count = 0;
        for (int sealId = 1; sealId <= SealTypeCount; sealId++)
        {
            if (SealCase_CountSealOccurrenceAnywhere(sealCase, sealId) != 0)
            {
                count++;
            }
        }
        return count;
    }

    // OFFSET 0x0202CBC8
    public static int SealCase_CountSealOccurrenceAnywhere(SealCase sealCase, int sealId)
    {
        return SealCase_CountSealOccurrenceInUse(sealCase, sealId - 1) + sealCase.SealCounts[sealId - 1];
    }
}

public sealed class ChatotCry : ISaveBlock
{
    public const int Size = 0x3EC;
    public const int SampleCount = 1000;

    private const int DataOffset = 4;

    public uint Valid;
    public readonly sbyte[] Data = new sbyte[SampleCount];

    public void WriteTo(byte[] body, int offset)
    {
        Bin.W32(body, offset, Valid);
        for (int i = 0; i < SampleCount; i++)
        {
            body[offset + DataOffset + i] = (byte)Data[i];
        }
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Valid = Bin.U32(body, offset);
        for (int i = 0; i < SampleCount; i++)
        {
            Data[i] = (sbyte)body[offset + DataOffset + i];
        }
    }

    // OFFSET 0x0202CC6C
    public static void ChatotCry_Init(ChatotCry chatotCry)
    {
        Array.Clear(chatotCry.Data);
        chatotCry.Valid = 0;
    }

    // OFFSET 0x0202CC84
    public static ChatotCry ChatotCry_New(int heapID)
    {
        ChatotCry chatotCry = new();
        ChatotCry_Init(chatotCry);
        return chatotCry;
    }

    // OFFSET 0x0202CC98
    public static ChatotCry SaveData_GetChatotCry(SaveData saveData)
    {
        return (ChatotCry)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryChatot);
    }

    // OFFSET 0x0202CCA4
    public static uint ChatotCry_IsValid(ChatotCry chatotCry)
    {
        return chatotCry.Valid;
    }

    // OFFSET 0x0202CCA8
    public static void ChatotCry_ResetStatus(ChatotCry chatotCry)
    {
        chatotCry.Valid = 0;
    }

    // OFFSET 0x0202CCB0
    public static sbyte[] ChatotCry_GetRawAudio(ChatotCry chatotCry)
    {
        return chatotCry.Data;
    }

    // OFFSET 0x0202CCB4
    public static void ChatotCry_GetUpsampledAudio(sbyte[] output, sbyte[] cryData)
    {
        for (int i = 0; i < SampleCount; i++)
        {
            int packed = cryData[i];
            output[i * 2] = (sbyte)(((packed & 0xF) - 8) << 4);
            output[i * 2 + 1] = (sbyte)((((packed >> 4) & 0xFF) - 8) << 4);
        }
    }

    // OFFSET 0x0202CCEC
    public static void ChatotCry_StoreAudio(ChatotCry chatotCry, sbyte[] input)
    {
        chatotCry.Valid = 1;
        int sample = 0;
        for (int i = 0; i < SampleCount * 2; i += 2)
        {
            chatotCry.Data[sample] = (sbyte)(input[i] / 16 + 8);
            int high = input[i + 1] / 16 + 8;
            chatotCry.Data[sample] = (sbyte)((byte)(high << 4) | (byte)chatotCry.Data[sample]);
            sample++;
        }
    }

    // OFFSET 0x0202CD3C
    public static void ChatotCry_Copy(ChatotCry destination, ChatotCry source)
    {
        destination.Valid = source.Valid;
        Array.Copy(source.Data, destination.Data, SampleCount);
    }
}
