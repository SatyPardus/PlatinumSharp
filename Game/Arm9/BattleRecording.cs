namespace PlatinumSharp.Game;

public static partial class BattleRecording
{
    public const int CommandBufferOffset = 0x238;
    public const int CommandBufferSize = 0x400;

    public static byte[]? sBattleRecording;

    // OFFSET 0x0202F868
    public static void BattleRecording_WriteCommand(int battler, int position, byte data)
    {
        if (sBattleRecording != null)
        {
            sBattleRecording[CommandBufferOffset + battler * CommandBufferSize + position] = data;
        }
    }

    // OFFSET 0x0202F884
    public static byte BattleRecording_ReadCommand(int battler, int position)
    {
        if (sBattleRecording == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return sBattleRecording![CommandBufferOffset + battler * CommandBufferSize + position];
    }
}

public static partial class BattleRecording
{
    private const int ParamsOffset = 0xE8;
    private const int PartiesOffset = 0x1238;
    private const int RecordedPartySize = 0x2A4;
    private const int TrainerInfosOffset = 0x1CC8;
    private const int OptionsOffset = 0x1D48;
    private const uint DefaultSystemVersion = 0x140;

    // OFFSET 0x0202F8AC
    public static void BattleRecording_SaveBattleParams(FieldBattleDTO dto)
    {
        byte[]? rec = sBattleRecording;
        if (rec == null)
        {
            return;
        }
        Nitro.Bin.W32(rec, ParamsOffset, dto.BattleType);
        Nitro.Bin.W32(rec, ParamsOffset + 4, (uint)dto.ResultMask);
        Nitro.Bin.W32(rec, 0x1D0, (uint)dto.Background);
        Nitro.Bin.W32(rec, 0x1D4, (uint)dto.Terrain);
        Nitro.Bin.W32(rec, 0x1D8, (uint)dto.MapLabelTextID);
        Nitro.Bin.W32(rec, 0x1DC, (uint)dto.MapHeaderID);
        Nitro.Bin.W32(rec, 0x1E0, (uint)dto.TimeOfDay);
        Nitro.Bin.W32(rec, 0x1E4, (uint)dto.MapEvolutionMethod);
        Nitro.Bin.W32(rec, 0x1E8, (uint)dto.VisitedContestHall);
        Nitro.Bin.W32(rec, 0x1EC, (uint)dto.MetBebe);
        Nitro.Bin.W32(rec, 0x1F0, (uint)dto.CaughtBattlerIndex);
        Nitro.Bin.W32(rec, 0x1F4, (uint)dto.FieldWeather);
        Nitro.Bin.W32(rec, 0x1F8, dto.LeveledUpMonsMask);
        Nitro.Bin.W32(rec, 0x20C, dto.BattleStatusMask);
        Nitro.Bin.W32(rec, 0x210, (uint)dto.CountSafariBalls);
        Nitro.Bin.W32(rec, 0x214, dto.RulesetMask);
        Nitro.Bin.W32(rec, 0x218, dto.Seed);
        Nitro.Bin.W16(rec, 0x22C, dto.NetworkID);
        Nitro.Bin.W16(rec, 0x22E, dto.Unknown1B3);
        Nitro.Bin.W32(rec, 0x230, (uint)dto.TotalTurnsElapsed);
        for (int i = 0; i < FieldBattleDTO.MaxBattlers; i++)
        {
            Nitro.Bin.W32(rec, 0xF0 + i * 4, (uint)dto.TrainerIDs[i]);
            Array.Copy(dto.Trainers[i], 0, rec, 0x100 + i * FieldBattleDTO.TrainerSize, FieldBattleDTO.TrainerSize);
            Nitro.Bin.W32(rec, 0x1FC + i * 4, dto.SystemVersion[i] == 0 ? DefaultSystemVersion : dto.SystemVersion[i]);
            Nitro.Bin.W32(rec, 0x21C + i * 4, (uint)dto.NetworkIDs[i]);
            rec[0x234 + i] = dto.Unknown1BC[i];
        }
        for (int i = 0; i < FieldBattleDTO.MaxBattlers; i++)
        {
            BattleRecording_StoreParty(dto.Parties[i]!, rec, PartiesOffset + i * RecordedPartySize);
            dto.TrainerInfos[i]!.WriteTo(rec, TrainerInfosOffset + i * TrainerInfo.Size);
            rec[0x234 + i] = (byte)SoundChatot.Sound_GetChatterActivationParameter(dto.ChatotCries[i]);
        }
        Nitro.Bin.W16(rec, OptionsOffset, dto.Options!.Flags);
    }

    private static void BattleRecording_StoreParty(Party party, byte[] rec, int offset)
    {
        throw Unported.Missing(0x0202FCE8);
    }
}
