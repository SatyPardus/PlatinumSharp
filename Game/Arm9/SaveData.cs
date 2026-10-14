using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public interface ISaveBlock
{
    void WriteTo(byte[] body, int offset);

    void ReadFrom(byte[] body, int offset);
}

public sealed class SavePageInfo
{
    public int PageID;
    public uint Size;
    public uint Location;
    public ushort Checksum;
    public ushort BlockID;
}

public sealed class SaveBlockInfo
{
    public byte SaveBlockID;
    public byte SectorStartPos;
    public byte SectorsInUse;
    public uint Offset;
    public uint Size;
}

public sealed class SaveDataState
{
    public int FullSaveMode;
    public int StartBlock;
    public int CurrentBlock;
    public int EndBlock;
    public int LockID;
    public int MainSequence;
    public uint GlobalCounterBackup;
    public readonly uint[] BlockCounterBackup = new uint[SaveData.BlockCount];
    public int Locked;
}

public struct SaveCheckInfo
{
    public int Valid;
    public uint GlobalCounter;
    public uint BlockCounter;
}

public sealed class SaveData
{
    public const int BlockNormal = 0;
    public const int BlockBoxes = 1;
    public const int BlockCount = 2;
    public const int BlockAll = 2;
    public const int TableEntryCount = 38;
    public const int BodySize = 0x20000;
    public const int LoadStatusNoData = 0;
    public const int LoadStatusValid = 1;
    public const int LoadStatusBackupUsed = 2;
    public const int LoadStatusCorrupt = 3;
    public const int SaveResultProceed = 0;
    public const int SaveResultProceedFinal = 1;
    public const int SaveResultOk = 2;
    public const int SaveResultFailed = 3;
    public const int LoadResultOk = 1;
    public const int LoadResultCorrupt = 2;
    public const uint LoadCheckBackupUsed = 1;
    public const uint LoadCheckCorrupt = 2;
    public const uint LoadCheckFrontierBackupUsed = 4;
    public const uint LoadCheckFrontierCorrupt = 8;
    public const uint LoadCheckVideoBackupUsed = 0x10;
    public const uint LoadCheckVideoCorrupt = 0x20;

    private const int SectorSize = 0x1000;
    private const int SectorMax = 32;
    private const int MirrorSectorOffset = 0x40;
    private const uint MirrorOffset = 0x40000;
    private const int FooterSize = 0x14;
    private const int ExtraFooterSize = 0x10;
    private const uint Signature = 0x20060623;
    private const int BackupTypeFlash4M = 0x1302;
    private const int BackupTypeFlash2M = 0x1202;
    private const uint NoExtraKey = 0xFFFFFFFF;
    private const int HeapSaveWork = 3;
    private const int ExtraFrontier = 1;
    private const int ExtraFirstVideo = 2;
    private const int ExtraLastVideo = 5;
    private const int CardErrorTimeout = 0;
    private const int CardErrorFailure = 1;

    public int BackupExists;
    public int DataExists;
    public int IsNewGameData;
    public int FullSaveRequired;
    public uint LoadCheckStatus;
    public readonly byte[] Body = new byte[BodySize];
    public uint GlobalCounter;
    public readonly uint[] BlockCounters = new uint[BlockCount];
    public readonly byte[] BlockOffsets = new byte[BlockCount];
    public readonly SavePageInfo[] PageInfo = new SavePageInfo[TableEntryCount];
    public readonly SaveBlockInfo[] BlockInfo = new SaveBlockInfo[BlockCount];
    public readonly SaveDataState State = new();
    public int SectorSwitch;
    public uint SectorCounter;
    public readonly object?[] Tables = new object?[TableEntryCount];

    private static SaveData? sSaveData;
    private static int sCardAsyncDone;

    private static ushort Checksum(byte[] data, int offset, uint size)
    {
        byte[] slice = new byte[size];
        Array.Copy(data, offset, slice, 0, (int)size);
        return MathUtil.CalcCRC16Checksum(slice, size);
    }

    private static void FlushTable(SaveData saveData, int saveTableID)
    {
        if (saveData.Tables[saveTableID] is ISaveBlock block)
        {
            block.WriteTo(saveData.Body, (int)saveData.PageInfo[saveTableID].Location);
        }
    }

    private static void FlushBlock(SaveData saveData, int blockID)
    {
        for (int i = 0; i < TableEntryCount; i++)
        {
            if (saveData.PageInfo[i].BlockID == blockID)
            {
                FlushTable(saveData, i);
            }
        }
    }

    // OFFSET 0x020244AC
    public static SaveData SaveData_Init()
    {
        SaveData saveData = new();
        for (int i = 0; i < TableEntryCount; i++)
        {
            saveData.PageInfo[i] = new SavePageInfo();
        }
        for (int i = 0; i < BlockCount; i++)
        {
            saveData.BlockInfo[i] = new SaveBlockInfo();
        }
        sSaveData = saveData;
        saveData.BackupExists = SaveData_CardBackupType() ? 1 : 0;
        saveData.DataExists = 0;
        saveData.IsNewGameData = 1;
        saveData.FullSaveRequired = 1;
        SaveData_InitPageInfo(saveData.PageInfo);
        SaveData_InitBlockInfo(saveData.BlockInfo, saveData.PageInfo);
        saveData.BlockCounters[0] = 0;
        saveData.BlockCounters[1] = 0;
        int status = SaveData_LoadCheck(saveData);
        saveData.LoadCheckStatus = 0;
        switch (status)
        {
            case LoadStatusValid:
            case LoadStatusBackupUsed:
                if (status == LoadStatusValid)
                {
                    saveData.FullSaveRequired = 0;
                }
                SaveData_LoadBlocks(saveData);
                saveData.DataExists = 1;
                saveData.IsNewGameData = 0;
                if (status == LoadStatusBackupUsed)
                {
                    saveData.LoadCheckStatus |= LoadCheckBackupUsed;
                }
                SaveData_CheckExtraSaves(saveData, out int frontierStatus, out int videoStatus);
                if (frontierStatus == SaveResultFailed)
                {
                    saveData.LoadCheckStatus |= LoadCheckFrontierCorrupt;
                }
                else if (frontierStatus == SaveResultOk)
                {
                    saveData.LoadCheckStatus |= LoadCheckFrontierBackupUsed;
                }
                if (videoStatus == SaveResultFailed)
                {
                    saveData.LoadCheckStatus |= LoadCheckVideoCorrupt;
                }
                else if (videoStatus == SaveResultOk)
                {
                    saveData.LoadCheckStatus |= LoadCheckVideoBackupUsed;
                }
                break;
            case LoadStatusCorrupt:
                saveData.LoadCheckStatus |= LoadCheckCorrupt;
                SaveData_Clear(saveData);
                break;
            case LoadStatusNoData:
                SaveData_Clear(saveData);
                break;
        }
        return saveData;
    }

    // OFFSET 0x020245A4
    public static SaveData SaveData_Ptr()
    {
        if (sSaveData == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return sSaveData!;
    }

    // OFFSET 0x020245BC
    public static object SaveData_SaveTable(SaveData saveData, int saveTableID)
    {
        if (saveTableID >= TableEntryCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return saveData.Tables[saveTableID]!;
    }

    // OFFSET 0x020245E0
    public static object SaveData_SaveTableConst(SaveData saveData, int saveTableID)
    {
        return SaveData_SaveTable(saveData, saveTableID);
    }

    private static void SaveData_WriteBlankFooters(SaveData saveData)
    {
        SaveData_WriteBlankFooter(saveData, BlockNormal, (byte)(saveData.BlockOffsets[BlockNormal] == 0 ? 1 : 0));
        SaveData_WriteBlankFooter(saveData, BlockBoxes, (byte)(saveData.BlockOffsets[BlockBoxes] == 0 ? 1 : 0));
        SaveData_WriteBlankFooter(saveData, BlockNormal, saveData.BlockOffsets[BlockNormal]);
        SaveData_WriteBlankFooter(saveData, BlockBoxes, saveData.BlockOffsets[BlockBoxes]);
    }

    // OFFSET 0x020245E8
    public static bool SaveData_Erase(SaveData saveData)
    {
        byte[] blank = new byte[SectorSize];
        GameSystem.SleepLock(1);
        SaveData_WriteBlankFooters(saveData);
        Array.Fill(blank, (byte)0xFF);
        for (int i = 0; i < MirrorSectorOffset; i++)
        {
            SaveData_CardSave((uint)(i * SectorSize), blank, 0, SectorSize);
            SaveData_CardSave((uint)((i + MirrorSectorOffset) << 12), blank, 0, SectorSize);
        }
        SaveData_Clear(saveData);
        saveData.DataExists = 0;
        GameSystem.SleepUnlock(1);
        return true;
    }

    // OFFSET 0x020246A8
    public static bool SaveData_Load(SaveData saveData)
    {
        if (saveData.BackupExists == 0)
        {
            return false;
        }
        if (SaveData_LoadBlocks(saveData))
        {
            saveData.DataExists = 1;
            saveData.IsNewGameData = 0;
            SaveData_CheckExtraSaves(saveData, out _, out _);
            return true;
        }
        return false;
    }

    // OFFSET 0x020246E0
    public static int SaveData_Save(SaveData saveData)
    {
        if (saveData.BackupExists == 0)
        {
            return SaveResultFailed;
        }
        if (saveData.IsNewGameData != 0)
        {
            GameSystem.SleepLock(1);
            SaveData_WriteBlankFooters(saveData);
            GameSystem.SleepUnlock(1);
        }
        int result = SaveData_SaveAll(saveData);
        if (result == SaveResultOk)
        {
            saveData.DataExists = 1;
            saveData.IsNewGameData = 0;
        }
        return result;
    }

    // OFFSET 0x02024760
    public static int SaveData_SaveBlock(SaveData saveData, int saveBlockID)
    {
        if (saveBlockID >= BlockCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (saveData.IsNewGameData != 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (saveData.DataExists != 1)
        {
            ErrorHandling.GF_AssertFail();
        }
        SaveData_SaveStateInit(saveData, saveBlockID);
        int result;
        do
        {
            result = SaveData_SaveStateMain(saveData);
        } while (result == SaveResultProceed || result == SaveResultProceedFinal);
        return result;
    }

    // OFFSET 0x0202479C
    public static void SaveData_Clear(SaveData saveData)
    {
        saveData.IsNewGameData = 1;
        saveData.FullSaveRequired = 1;
        SaveData_InitBody(saveData, saveData.PageInfo);
    }

    // OFFSET 0x020247B8
    public static int SaveData_BackupExists(SaveData? saveData)
    {
        return saveData!.BackupExists;
    }

    // OFFSET 0x020247BC
    public static uint SaveData_LoadCheckStatus(SaveData saveData)
    {
        return saveData.LoadCheckStatus;
    }

    // OFFSET 0x020247C0
    public static int SaveData_DataExists(SaveData saveData)
    {
        return saveData.DataExists;
    }

    // OFFSET 0x020247C4
    public static int SaveData_IsNewGameData(SaveData saveData)
    {
        return saveData.IsNewGameData;
    }

    // OFFSET 0x020247C8
    public static int SaveData_MiscSaveBlock_InitFlag(SaveData saveData)
    {
        return MiscSaveBlock.MiscSaveBlock_InitFlag(MiscSaveBlock.SaveData_MiscSaveBlock(saveData));
    }

    // OFFSET 0x020247D4
    private static void SaveData_MiscSaveBlock_SetInitFlag(SaveData saveData)
    {
        MiscSaveBlock.MiscSaveBlock_SetInitFlag(MiscSaveBlock.SaveData_MiscSaveBlock(saveData));
    }

    // OFFSET 0x020247E0
    public static bool SaveData_OverwriteCheck(SaveData saveData)
    {
        if (SaveData_IsNewGameData(saveData) != 0 && SaveData_DataExists(saveData) != 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02024800
    public static int SaveData_FullSaveRequired(SaveData saveData)
    {
        return saveData.FullSaveRequired;
    }

    // OFFSET 0x02024804
    public static void SaveData_SetFullSaveRequired()
    {
        sSaveData!.FullSaveRequired = 1;
    }

    // OFFSET 0x02024814
    public static void SaveData_SaveStateInit(SaveData saveData, int blockID)
    {
        SaveDataState_Init(saveData, saveData.State, blockID);
    }

    // OFFSET 0x02024828
    public static int SaveData_SaveStateMain(SaveData saveData)
    {
        int result = SaveDataState_Main(saveData, saveData.State);
        if (result != SaveResultProceed && result != SaveResultProceedFinal)
        {
            SaveDataState_Finish(saveData, saveData.State, result);
        }
        return result;
    }

    // OFFSET 0x02024850
    public static void SaveData_SaveStateCancel(SaveData saveData)
    {
        SaveDataState_Cancel(saveData, saveData.State);
    }

    // OFFSET 0x02024860
    private static void SaveCheckInfo_Init(out SaveCheckInfo info)
    {
        info.Valid = 0;
        info.GlobalCounter = 0;
        info.BlockCounter = 0;
    }

    // OFFSET 0x0202486C
    public static ushort SaveData_CalculateChecksum(SaveData saveData, byte[] data, int offset, uint size)
    {
        return Checksum(data, offset, size);
    }

    // OFFSET 0x02024878
    private static ushort SaveData_CalculateFooterChecksum(SaveData saveData, byte[] data, int offset, uint size)
    {
        return Checksum(data, offset, size - FooterSize);
    }

    // OFFSET 0x02024888
    private static uint SaveData_GetCardOffset(int mirror, SaveBlockInfo blockInfo)
    {
        uint offset;
        if (mirror == 0)
        {
            offset = 0;
        }
        else
        {
            offset = MirrorOffset;
        }
        return offset + blockInfo.Offset;
    }

    // OFFSET 0x0202489C
    private static int SaveData_GetFooterOffset(SaveData saveData, int bodyOffset, int blockID)
    {
        SaveBlockInfo blockInfo = saveData.BlockInfo[blockID];
        int start = bodyOffset + (int)blockInfo.Offset;
        if (blockInfo.Size == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        return start + (int)blockInfo.Size - FooterSize;
    }

    // OFFSET 0x020248C4
    private static bool SaveData_VerifyFooter(SaveData saveData, byte[] body, int blockID)
    {
        SaveBlockInfo blockInfo = saveData.BlockInfo[blockID];
        int footer = SaveData_GetFooterOffset(saveData, 0, blockID);
        if (Bin.U32(body, footer + 8) != blockInfo.Size)
        {
            return false;
        }
        if (Bin.U32(body, footer + 0xC) != Signature)
        {
            return false;
        }
        if (body[footer + 0x10] != blockID)
        {
            return false;
        }
        ushort checksum = SaveData_CalculateFooterChecksum(saveData, body, (int)blockInfo.Offset, blockInfo.Size);
        if (Bin.U16(body, footer + 0x12) == checksum)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02024920
    private static void SaveCheckInfo_Read(out SaveCheckInfo info, SaveData saveData, byte[] body, int blockID)
    {
        int footer = SaveData_GetFooterOffset(saveData, 0, blockID);
        info.Valid = SaveData_VerifyFooter(saveData, body, blockID) ? 1 : 0;
        info.GlobalCounter = Bin.U32(body, footer);
        info.BlockCounter = Bin.U32(body, footer + 4);
    }

    // OFFSET 0x0202494C
    private static void SaveData_WriteFooter(SaveData saveData, byte[] body, int blockID)
    {
        SaveBlockInfo blockInfo = saveData.BlockInfo[blockID];
        int footer = SaveData_GetFooterOffset(saveData, 0, blockID);
        Bin.W32(body, footer, saveData.GlobalCounter);
        Bin.W32(body, footer + 4, saveData.BlockCounters[blockID]);
        Bin.W32(body, footer + 8, blockInfo.Size);
        Bin.W32(body, footer + 0xC, Signature);
        body[footer + 0x10] = (byte)blockID;
        Bin.W16(body, footer + 0x12, SaveData_CalculateFooterChecksum(saveData, body, (int)blockInfo.Offset, blockInfo.Size));
    }

    // OFFSET 0x0202499C
    private static int SaveData_CompareCounters(uint first, uint second)
    {
        if (first == 0xFFFFFFFF && second == 0)
        {
            return -1;
        }
        if (first == 0 && second == 0xFFFFFFFF)
        {
            return 1;
        }
        if (first > second)
        {
            return 1;
        }
        if (first < second)
        {
            return -1;
        }
        return 0;
    }

    // OFFSET 0x020249D4
    private static int SaveCheckInfo_Compare(in SaveCheckInfo primary, in SaveCheckInfo backup, out int current, out int stale)
    {
        int globalOrder = SaveData_CompareCounters(primary.GlobalCounter, backup.GlobalCounter);
        int blockOrder = SaveData_CompareCounters(primary.BlockCounter, backup.BlockCounter);
        if (primary.Valid != 0 && backup.Valid != 0)
        {
            if (globalOrder > 0)
            {
                if (blockOrder <= 0)
                {
                    ErrorHandling.GF_AssertFail();
                }
                current = 0;
                stale = 1;
            }
            else if (globalOrder < 0)
            {
                if (blockOrder >= 0)
                {
                    ErrorHandling.GF_AssertFail();
                }
                current = 1;
                stale = 0;
            }
            else if (blockOrder > 0)
            {
                current = 0;
                stale = 1;
            }
            else if (blockOrder < 0)
            {
                current = 1;
                stale = 0;
            }
            else
            {
                current = 0;
                stale = 1;
            }
            return 2;
        }
        if (primary.Valid != 0 && backup.Valid == 0)
        {
            current = 0;
            stale = 2;
            return 1;
        }
        if (primary.Valid == 0 && backup.Valid != 0)
        {
            current = 1;
            stale = 2;
            return 1;
        }
        current = 2;
        stale = 2;
        return 0;
    }

    // OFFSET 0x02024A84
    private static void SaveData_SetCounters(SaveData saveData, SaveCheckInfo[] normal, SaveCheckInfo[] boxes, int normalMirror, int boxesMirror)
    {
        saveData.GlobalCounter = normal[normalMirror].GlobalCounter;
        saveData.BlockCounters[BlockNormal] = normal[normalMirror].BlockCounter;
        saveData.BlockCounters[BlockBoxes] = boxes[boxesMirror].BlockCounter;
        saveData.BlockOffsets[BlockNormal] = (byte)normalMirror;
        saveData.BlockOffsets[BlockBoxes] = (byte)boxesMirror;
    }

    // OFFSET 0x02024ABC
    private static int SaveData_LoadCheck(SaveData saveData)
    {
        SaveCheckInfo[] normal = new SaveCheckInfo[2];
        SaveCheckInfo[] boxes = new SaveCheckInfo[2];
        byte[] primary = new byte[BodySize];
        byte[] backup = new byte[BodySize];
        if (SaveData_CardLoad(0, primary, 0, BodySize))
        {
            SaveCheckInfo_Read(out normal[0], saveData, primary, BlockNormal);
            SaveCheckInfo_Read(out boxes[0], saveData, primary, BlockBoxes);
        }
        else
        {
            SaveCheckInfo_Init(out normal[0]);
            SaveCheckInfo_Init(out boxes[0]);
        }
        if (SaveData_CardLoad(MirrorOffset, backup, 0, BodySize))
        {
            SaveCheckInfo_Read(out normal[1], saveData, backup, BlockNormal);
            SaveCheckInfo_Read(out boxes[1], saveData, backup, BlockBoxes);
        }
        else
        {
            SaveCheckInfo_Init(out normal[1]);
            SaveCheckInfo_Init(out boxes[1]);
        }
        int validNormal = SaveCheckInfo_Compare(normal[0], normal[1], out int currentNormal, out int staleNormal);
        int validBoxes = SaveCheckInfo_Compare(boxes[0], boxes[1], out int currentBoxes, out int staleBoxes);
        if (validNormal == 0 && validBoxes == 0)
        {
            return LoadStatusNoData;
        }
        if (validNormal == 0 || validBoxes == 0)
        {
            return LoadStatusCorrupt;
        }
        if (validNormal == 2 && validBoxes == 2)
        {
            if (normal[currentNormal].GlobalCounter == boxes[currentBoxes].GlobalCounter)
            {
                SaveData_SetCounters(saveData, normal, boxes, currentNormal, currentBoxes);
                return LoadStatusValid;
            }
            SaveData_SetCounters(saveData, normal, boxes, staleNormal, currentBoxes);
            return LoadStatusBackupUsed;
        }
        if (validNormal == 1 && validBoxes == 2)
        {
            if (normal[currentNormal].GlobalCounter == boxes[currentBoxes].GlobalCounter)
            {
                SaveData_SetCounters(saveData, normal, boxes, currentNormal, currentBoxes);
                return LoadStatusBackupUsed;
            }
            if (normal[currentNormal].GlobalCounter == boxes[staleBoxes].GlobalCounter)
            {
                SaveData_SetCounters(saveData, normal, boxes, currentNormal, staleBoxes);
                return LoadStatusBackupUsed;
            }
            return LoadStatusCorrupt;
        }
        if (validNormal == 2 && validBoxes == 1)
        {
            if (normal[currentNormal].GlobalCounter == boxes[currentBoxes].GlobalCounter)
            {
                SaveData_SetCounters(saveData, normal, boxes, currentNormal, currentBoxes);
                return LoadStatusValid;
            }
            SaveData_SetCounters(saveData, normal, boxes, staleNormal, currentBoxes);
            return LoadStatusBackupUsed;
        }
        if (validNormal == 1 && validBoxes == 1 && currentNormal == currentBoxes)
        {
            if (normal[currentNormal].GlobalCounter != boxes[currentBoxes].GlobalCounter)
            {
                ErrorHandling.GF_AssertFail();
            }
            SaveData_SetCounters(saveData, normal, boxes, currentNormal, currentBoxes);
            return LoadStatusValid;
        }
        if (normal[currentNormal].GlobalCounter != boxes[currentBoxes].GlobalCounter)
        {
            ErrorHandling.GF_AssertFail();
        }
        SaveData_SetCounters(saveData, normal, boxes, currentNormal, currentBoxes);
        return LoadStatusBackupUsed;
    }

    private static int SaveData_CheckExtraSave(SaveData saveData, MiscSaveBlock miscSaveBlock, int extraSaveID, int previous)
    {
        MiscSaveBlock.MiscSaveBlock_ExtraSaveKey(miscSaveBlock, extraSaveID, out uint currentKey, out uint oldKey, out _);
        if (currentKey == NoExtraKey && oldKey == NoExtraKey)
        {
            return previous;
        }
        SaveDataExtra_Mirror(saveData, HeapSaveWork, extraSaveID, out int loadResult, out int isOld);
        if (loadResult == LoadResultCorrupt)
        {
            return SaveResultFailed;
        }
        if (loadResult == LoadResultOk && isOld == 1 && previous != SaveResultFailed)
        {
            return SaveResultOk;
        }
        return previous;
    }

    // OFFSET 0x02024CD4
    private static void SaveData_CheckExtraSaves(SaveData saveData, out int frontierStatus, out int videoStatus)
    {
        MiscSaveBlock miscSaveBlock = MiscSaveBlock.SaveData_MiscSaveBlock(saveData);
        frontierStatus = 1;
        videoStatus = 1;
        if (SaveData_MiscSaveBlock_InitFlag(saveData) == 0)
        {
            return;
        }
        frontierStatus = SaveData_CheckExtraSave(saveData, miscSaveBlock, ExtraFrontier, frontierStatus);
        for (int extraSaveID = ExtraFirstVideo; extraSaveID <= ExtraLastVideo; extraSaveID++)
        {
            videoStatus = SaveData_CheckExtraSave(saveData, miscSaveBlock, extraSaveID, videoStatus);
        }
    }

    // OFFSET 0x02024DA4
    private static bool SaveData_LoadBlock(int mirror, SaveBlockInfo blockInfo, byte[] body)
    {
        uint address = SaveData_GetCardOffset(mirror, blockInfo);
        return SaveData_CardLoad(address, body, (int)blockInfo.Offset, blockInfo.Size);
    }

    // OFFSET 0x02024DBC
    private static bool SaveData_LoadBlocks(SaveData saveData)
    {
        for (int blockID = 0; blockID < BlockCount; blockID++)
        {
            if (!SaveData_LoadBlock(saveData.BlockOffsets[blockID], saveData.BlockInfo[blockID], saveData.Body))
            {
                return false;
            }
            if (!SaveData_VerifyFooter(saveData, saveData.Body, blockID))
            {
                return false;
            }
        }
        for (int i = 0; i < TableEntryCount; i++)
        {
            SavePageInfo page = saveData.PageInfo[i];
            saveData.Tables[i] = SaveDataTable.sSaveTable[i].CreateFunc();
            if (saveData.Tables[i] is ISaveBlock block)
            {
                block.ReadFrom(saveData.Body, (int)page.Location);
            }
            page.Checksum = Checksum(saveData.Body, (int)page.Location, page.Size);
        }
        return true;
    }

    // OFFSET 0x02024E30
    private static int SaveData_WriteBlockBody(SaveData saveData, int blockID, byte mirror)
    {
        SaveBlockInfo blockInfo = saveData.BlockInfo[blockID];
        FlushBlock(saveData, blockID);
        SaveData_WriteFooter(saveData, saveData.Body, blockID);
        uint address = SaveData_GetCardOffset(mirror, blockInfo);
        return SaveData_CardSave_Init(address, saveData.Body, (int)blockInfo.Offset, blockInfo.Size - FooterSize);
    }

    // OFFSET 0x02024E68
    private static int SaveData_WriteBlockFooter(SaveData saveData, int blockID, byte mirror)
    {
        SaveBlockInfo blockInfo = saveData.BlockInfo[blockID];
        uint size = blockInfo.Size;
        uint address = SaveData_GetCardOffset(mirror, blockInfo) + size - FooterSize;
        return SaveData_CardSave_Init(address, saveData.Body, (int)(blockInfo.Offset + size) - FooterSize, FooterSize);
    }

    // OFFSET 0x02024E98
    private static int SaveData_WriteBlockSignature(SaveData saveData, int blockID, byte mirror)
    {
        SaveBlockInfo blockInfo = saveData.BlockInfo[blockID];
        uint size = blockInfo.Size;
        uint address = SaveData_GetCardOffset(mirror, blockInfo) + size - 0xC;
        return SaveData_CardSave_Init(address, saveData.Body, (int)(blockInfo.Offset + size) - 0xC, 8);
    }

    // OFFSET 0x02024EC8
    private static void SaveDataState_Init(SaveData saveData, SaveDataState state, int blockID)
    {
        for (int i = 0; i < BlockCount; i++)
        {
            state.BlockCounterBackup[i] = saveData.BlockCounters[i];
            saveData.BlockCounters[i]++;
        }
        state.MainSequence = 0;
        state.FullSaveMode = 0;
        state.Locked = 0;
        if (blockID == BlockAll)
        {
            if (saveData.FullSaveRequired != 0)
            {
                state.FullSaveMode = 1;
                state.GlobalCounterBackup = saveData.GlobalCounter;
                saveData.GlobalCounter++;
                state.StartBlock = 0;
                state.CurrentBlock = 0;
                state.EndBlock = 2;
            }
            else
            {
                state.StartBlock = 0;
                state.CurrentBlock = 0;
                state.EndBlock = 1;
            }
        }
        else
        {
            state.StartBlock = blockID;
            state.CurrentBlock = blockID;
            state.EndBlock = blockID + 1;
        }
        GameSystem.SleepLock(1);
    }

    private static byte OtherMirror(SaveData saveData, int blockID)
    {
        return (byte)(saveData.BlockOffsets[blockID] == 0 ? 1 : 0);
    }

    // OFFSET 0x02024F44
    private static int SaveDataState_Main(SaveData saveData, SaveDataState state)
    {
        int succeeded = 0;
        switch (state.MainSequence)
        {
            case 0:
                state.LockID = SaveData_WriteBlockBody(saveData, state.CurrentBlock, OtherMirror(saveData, state.CurrentBlock));
                state.Locked = 1;
                state.MainSequence++;
                goto case 1;
            case 1:
                if (!SaveData_CardSave_Main(state.LockID, state.Locked, ref succeeded))
                {
                    break;
                }
                state.Locked = 0;
                if (succeeded == 0)
                {
                    return SaveResultFailed;
                }
                state.MainSequence++;
                goto case 2;
            case 2:
                state.LockID = SaveData_WriteBlockSignature(saveData, state.CurrentBlock, OtherMirror(saveData, state.CurrentBlock));
                state.Locked = 1;
                state.MainSequence++;
                goto case 3;
            case 3:
                if (!SaveData_CardSave_Main(state.LockID, state.Locked, ref succeeded))
                {
                    break;
                }
                state.Locked = 0;
                if (succeeded == 0)
                {
                    return SaveResultFailed;
                }
                state.MainSequence++;
                if (state.CurrentBlock + 1 == state.EndBlock)
                {
                    return SaveResultProceedFinal;
                }
                goto case 4;
            case 4:
                state.LockID = SaveData_WriteBlockFooter(saveData, state.CurrentBlock, OtherMirror(saveData, state.CurrentBlock));
                state.Locked = 1;
                state.MainSequence++;
                goto case 5;
            case 5:
                if (!SaveData_CardSave_Main(state.LockID, state.Locked, ref succeeded))
                {
                    break;
                }
                state.Locked = 0;
                if (succeeded == 0)
                {
                    return SaveResultFailed;
                }
                state.CurrentBlock++;
                if (state.CurrentBlock == state.EndBlock)
                {
                    return SaveResultOk;
                }
                state.MainSequence = 0;
                break;
        }
        return SaveResultProceed;
    }

    private static void SaveDataState_RestoreCounters(SaveData saveData, SaveDataState state)
    {
        if (state.FullSaveMode != 0)
        {
            saveData.GlobalCounter = state.GlobalCounterBackup;
        }
        for (int i = 0; i < BlockCount; i++)
        {
            saveData.BlockCounters[i] = state.BlockCounterBackup[i];
        }
    }

    // OFFSET 0x0202506C
    private static void SaveDataState_Finish(SaveData saveData, SaveDataState state, int result)
    {
        if (result == SaveResultFailed)
        {
            SaveDataState_RestoreCounters(saveData, state);
        }
        else
        {
            for (int blockID = state.StartBlock; blockID < state.EndBlock; blockID++)
            {
                saveData.BlockOffsets[blockID] = OtherMirror(saveData, blockID);
            }
            saveData.DataExists = 1;
            saveData.IsNewGameData = 0;
            saveData.FullSaveRequired = 0;
        }
        GameSystem.SleepUnlock(1);
    }

    // OFFSET 0x020250DC
    private static void SaveDataState_Cancel(SaveData saveData, SaveDataState state)
    {
        SaveDataState_RestoreCounters(saveData, state);
        if (state.Locked != 0)
        {
            state.Locked = 0;
        }
        GameSystem.SleepUnlock(1);
    }

    // OFFSET 0x0202513C
    private static int SaveData_SaveAll(SaveData saveData)
    {
        SaveDataState state = new();
        SaveDataState_Init(saveData, state, BlockAll);
        int result;
        do
        {
            result = SaveDataState_Main(saveData, state);
        } while (result == SaveResultProceed || result == SaveResultProceedFinal);
        SaveDataState_Finish(saveData, state, result);
        return result;
    }

    // OFFSET 0x0202516C
    private static void SaveData_WriteBlankFooter(SaveData saveData, int blockID, byte mirror)
    {
        SaveBlockInfo blockInfo = saveData.BlockInfo[blockID];
        byte[] blank = new byte[FooterSize];
        Array.Fill(blank, (byte)0xFF);
        uint address = SaveData_GetCardOffset(mirror, blockInfo) + blockInfo.Size - FooterSize;
        SaveData_CardSave(address, blank, 0, FooterSize);
    }

    // OFFSET 0x020251A4
    public static int SaveTableEntry_BodySize(int saveTableID)
    {
        if (saveTableID >= SaveDataTable.sSaveTable.Length)
        {
            ErrorHandling.GF_AssertFail();
        }
        int size = SaveDataTable.sSaveTable[saveTableID].SizeFunc();
        size += 4 - size % 4;
        size += 4;
        return size;
    }

    // OFFSET 0x020251DC
    private static void SaveData_InitPageInfo(SavePageInfo[] pageInfo)
    {
        SaveTableEntry[] table = SaveDataTable.sSaveTable;
        int count = table.Length;
        uint location = 0;
        if (count != TableEntryCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < count; i++)
        {
            if (i != table[i].ID)
            {
                ErrorHandling.GF_AssertFail();
            }
            pageInfo[i].PageID = table[i].ID;
            pageInfo[i].Size = (uint)SaveTableEntry_BodySize(i);
            pageInfo[i].Location = location;
            pageInfo[i].Checksum = 0;
            pageInfo[i].BlockID = (ushort)table[i].BlockID;
            location += pageInfo[i].Size;
            if (i == count - 1 || table[i].BlockID != table[i + 1].BlockID)
            {
                location += FooterSize;
            }
        }
        if (location > BodySize)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02025258
    private static void SaveData_InitBlockInfo(SaveBlockInfo[] blockInfo, SavePageInfo[] pageInfo)
    {
        int count = SaveDataTable.sSaveTable.Length;
        int sector = 0;
        uint offset = 0;
        int page = 0;
        for (int blockID = 0; blockID < BlockCount; blockID++)
        {
            SaveBlockInfo info = blockInfo[blockID];
            info.SaveBlockID = (byte)blockID;
            info.Size = 0;
            while (page < count && pageInfo[page].BlockID == blockID)
            {
                info.Size += pageInfo[page].Size;
                page++;
            }
            info.Size += FooterSize;
            info.SectorStartPos = (byte)sector;
            info.Offset = offset;
            info.SectorsInUse = (byte)((info.Size + (SectorSize - 1)) >> 12);
            sector += info.SectorsInUse;
            offset += info.Size;
        }
        if (sector != blockInfo[BlockCount - 1].SectorStartPos + blockInfo[BlockCount - 1].SectorsInUse)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (sector > SectorMax)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x020252EC
    private static void SaveData_InitBody(SaveData saveData, SavePageInfo[] pageInfo)
    {
        SaveTableEntry[] table = SaveDataTable.sSaveTable;
        Array.Clear(saveData.Body);
        for (int i = 0; i < table.Length; i++)
        {
            Array.Clear(saveData.Body, (int)pageInfo[i].Location, (int)pageInfo[i].Size);
            saveData.Tables[i] = table[i].CreateFunc();
            table[i].InitFunc?.Invoke(saveData.Tables[i]!);
        }
    }

    // OFFSET 0x02025340
    public static void SaveDataExtra_Init(SaveData saveData)
    {
        ExtraSaveTableEntry[] table = SaveDataTable.sExtraSaveTable;
        if (SaveData_MiscSaveBlock_InitFlag(saveData) == 1)
        {
            return;
        }
        for (int i = 0; i < table.Length; i++)
        {
            if (table[i].ID == 0)
            {
                continue;
            }
            byte[]? data = SaveDataExtra_Get(saveData, HeapSaveWork, table[i].ID, out _);
            if (data == null)
            {
                ErrorHandling.GF_AssertFail();
            }
            Array.Clear(data!, 0, table[i].SizeFunc());
            table[i].InitFunc(data!);
            SaveDataExtra_Save(saveData, table[i].ID, data!);
        }
        SaveData_MiscSaveBlock_SetInitFlag(saveData);
    }

    // OFFSET 0x020253B4
    private static void SaveDataExtra_WriteFooter(SaveData saveData, byte[] data, int extraSaveID, int size)
    {
        Bin.W32(data, size, Signature);
        Bin.W32(data, size + 4, saveData.SectorCounter + 1);
        Bin.W32(data, size + 8, (uint)size);
        Bin.W16(data, size + 0xC, (ushort)extraSaveID);
        Bin.W16(data, size + 0xE, Checksum(data, 0, (uint)(size + 0xE)));
    }

    // OFFSET 0x020253E0
    private static bool SaveDataExtra_VerifyFooter(SaveData saveData, byte[] data, int extraSaveID, int size)
    {
        if (Bin.U32(data, size) != Signature)
        {
            return false;
        }
        if (Bin.U32(data, size + 8) != size)
        {
            return false;
        }
        if (Bin.U16(data, size + 0xC) != extraSaveID)
        {
            return false;
        }
        if (Bin.U16(data, size + 0xE) == Checksum(data, 0, (uint)(size + 0xE)))
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02025420
    private static uint SaveDataExtra_GetFooterCounter(byte[] data, int size)
    {
        return Bin.U32(data, size + 4);
    }

    private static ExtraSaveTableEntry GetExtraEntry(int extraSaveID)
    {
        ExtraSaveTableEntry[] table = SaveDataTable.sExtraSaveTable;
        if (extraSaveID >= table.Length)
        {
            ErrorHandling.GF_AssertFail();
        }
        ExtraSaveTableEntry entry = table[extraSaveID];
        if (entry.ID != extraSaveID)
        {
            ErrorHandling.GF_AssertFail();
        }
        return entry;
    }

    private static bool SaveDataExtra_WriteSector(SaveData saveData, ExtraSaveTableEntry entry, byte[] data, int extraSaveID, bool mirror)
    {
        uint size = (uint)(entry.SizeFunc() + ExtraFooterSize);
        SaveDataExtra_WriteFooter(saveData, data, extraSaveID, entry.SizeFunc());
        uint sector = (uint)entry.Sector;
        if (mirror)
        {
            sector += MirrorSectorOffset;
        }
        bool written = SaveData_CardSave(sector << 12, data, 0, size);
        if (!SaveDataExtra_VerifyFooter(saveData, data, extraSaveID, entry.SizeFunc()))
        {
            ErrorHandling.GF_AssertFail();
        }
        return written;
    }

    // OFFSET 0x02025428
    public static int SaveDataExtra_Save(SaveData saveData, int extraSaveID, byte[] data)
    {
        GameSystem.SleepLock(1);
        ExtraSaveTableEntry entry = GetExtraEntry(extraSaveID);
        bool written;
        if (saveData.SectorSwitch == 1)
        {
            written = SaveDataExtra_WriteSector(saveData, entry, data, extraSaveID, false);
            written |= SaveDataExtra_WriteSector(saveData, entry, data, extraSaveID, true);
        }
        else
        {
            written = SaveDataExtra_WriteSector(saveData, entry, data, extraSaveID, true);
            written |= SaveDataExtra_WriteSector(saveData, entry, data, extraSaveID, false);
        }
        if (written)
        {
            GameSystem.SleepUnlock(1);
            return SaveResultOk;
        }
        GameSystem.SleepUnlock(1);
        return SaveResultFailed;
    }

    // OFFSET 0x02025574
    public static int SaveDataExtra_SaveMirror(SaveData saveData, int extraSaveID, byte[] data)
    {
        GameSystem.SleepLock(1);
        ExtraSaveTableEntry entry = GetExtraEntry(extraSaveID);
        SaveDataExtra_GetKeys(saveData, extraSaveID, out uint currentKey, out _, out byte mirrorFlag);
        uint newKey;
        do
        {
            newKey = MathUtil.ARNG_Next(currentKey);
        } while (newKey == NoExtraKey);
        SaveDataExtra_SetKeys(saveData, extraSaveID, newKey, currentKey, (byte)(mirrorFlag ^ 1));
        Bin.W32(data, 0, newKey);
        bool written;
        if (mirrorFlag == 1)
        {
            written = SaveDataExtra_WriteSector(saveData, entry, data, extraSaveID, false);
        }
        else
        {
            written = SaveDataExtra_WriteSector(saveData, entry, data, extraSaveID, true);
        }
        if (written)
        {
            GameSystem.SleepUnlock(1);
            return SaveResultOk;
        }
        GameSystem.SleepUnlock(1);
        return SaveResultFailed;
    }

    private static void SaveDataExtra_ReadSector(ExtraSaveTableEntry entry, byte[] data, bool mirror)
    {
        uint sector = (uint)entry.Sector;
        if (mirror)
        {
            sector += MirrorSectorOffset;
        }
        SaveData_CardLoad(sector << 12, data, 0, (uint)(entry.SizeFunc() + ExtraFooterSize));
    }

    // OFFSET 0x02025680
    public static byte[]? SaveDataExtra_Get(SaveData saveData, int heapID, int extraSaveID, out int loadResult)
    {
        ExtraSaveTableEntry entry = GetExtraEntry(extraSaveID);
        byte[] data = new byte[entry.SizeFunc() + ExtraFooterSize];
        SaveDataExtra_ReadSector(entry, data, false);
        bool primaryValid = SaveDataExtra_VerifyFooter(saveData, data, extraSaveID, entry.SizeFunc());
        uint primaryCounter = SaveDataExtra_GetFooterCounter(data, entry.SizeFunc());
        SaveDataExtra_ReadSector(entry, data, true);
        bool mirrorValid = SaveDataExtra_VerifyFooter(saveData, data, extraSaveID, entry.SizeFunc());
        uint mirrorCounter = SaveDataExtra_GetFooterCounter(data, entry.SizeFunc());
        loadResult = LoadResultOk;
        if (primaryValid && !mirrorValid)
        {
            saveData.SectorSwitch = 0;
            saveData.SectorCounter = primaryCounter;
            SaveDataExtra_ReadSector(entry, data, false);
            return data;
        }
        if (!primaryValid && mirrorValid)
        {
            saveData.SectorSwitch = 1;
            saveData.SectorCounter = mirrorCounter;
            SaveDataExtra_ReadSector(entry, data, true);
            return data;
        }
        if (primaryValid && mirrorValid)
        {
            if (SaveData_CompareCounters(primaryCounter, mirrorCounter) != -1)
            {
                saveData.SectorSwitch = 0;
                saveData.SectorCounter = primaryCounter;
                SaveDataExtra_ReadSector(entry, data, false);
                return data;
            }
            saveData.SectorSwitch = 1;
            saveData.SectorCounter = mirrorCounter;
            SaveDataExtra_ReadSector(entry, data, true);
            return data;
        }
        loadResult = LoadResultCorrupt;
        saveData.SectorSwitch = 0;
        saveData.SectorCounter = 0;
        return data;
    }

    // OFFSET 0x020257E8
    public static byte[]? SaveDataExtra_Mirror(SaveData saveData, int heapID, int extraSaveID, out int loadResult, out int isOld)
    {
        MiscSaveBlock miscSaveBlock = MiscSaveBlock.SaveData_MiscSaveBlock(saveData);
        if (extraSaveID == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        ExtraSaveTableEntry entry = GetExtraEntry(extraSaveID);
        byte[] data = new byte[entry.SizeFunc() + ExtraFooterSize];
        SaveDataExtra_GetKeys(saveData, extraSaveID, out uint currentKey, out uint oldKey, out byte mirrorFlag);
        SaveDataExtra_ReadSector(entry, data, false);
        bool primaryValid = SaveDataExtra_VerifyFooter(saveData, data, extraSaveID, entry.SizeFunc());
        uint primaryKey = Bin.U32(data, 0);
        SaveDataExtra_ReadSector(entry, data, true);
        bool mirrorValid = SaveDataExtra_VerifyFooter(saveData, data, extraSaveID, entry.SizeFunc());
        uint mirrorKey = Bin.U32(data, 0);
        loadResult = LoadResultOk;
        isOld = 0;
        if (primaryValid && !mirrorValid && currentKey == primaryKey)
        {
            if (mirrorFlag == 1)
            {
                SaveDataExtra_SetKeys(saveData, extraSaveID, oldKey, oldKey, 0);
                isOld = 1;
            }
            SaveDataExtra_ReadSector(entry, data, false);
            return data;
        }
        if (!primaryValid && mirrorValid && currentKey == mirrorKey)
        {
            if (mirrorFlag == 0)
            {
                SaveDataExtra_SetKeys(saveData, extraSaveID, oldKey, oldKey, 1);
                isOld = 1;
            }
            SaveDataExtra_ReadSector(entry, data, true);
            return data;
        }
        if (primaryValid && mirrorValid)
        {
            if (mirrorFlag == 0)
            {
                if (currentKey == primaryKey)
                {
                    SaveDataExtra_ReadSector(entry, data, false);
                    return data;
                }
                if (oldKey == mirrorKey)
                {
                    SaveDataExtra_SetKeys(saveData, extraSaveID, oldKey, oldKey, (byte)(mirrorFlag ^ 1));
                    isOld = 1;
                    SaveDataExtra_ReadSector(entry, data, true);
                    return data;
                }
            }
            else
            {
                if (currentKey == mirrorKey)
                {
                    SaveDataExtra_ReadSector(entry, data, true);
                    return data;
                }
                if (oldKey == primaryKey)
                {
                    SaveDataExtra_SetKeys(saveData, extraSaveID, oldKey, oldKey, (byte)(mirrorFlag ^ 1));
                    isOld = 1;
                    SaveDataExtra_ReadSector(entry, data, false);
                    return data;
                }
            }
        }
        loadResult = LoadResultCorrupt;
        MiscSaveBlock.MiscSaveBlock_SetExtraSaveKey(miscSaveBlock, entry.ID, NoExtraKey, NoExtraKey, 0);
        return data;
    }

    // OFFSET 0x020259F8
    private static void SaveDataExtra_GetKeys(SaveData saveData, int extraSaveID, out uint currentKey, out uint oldKey, out byte mirrorFlag)
    {
        MiscSaveBlock.MiscSaveBlock_ExtraSaveKey(MiscSaveBlock.SaveData_MiscSaveBlock(saveData), extraSaveID, out currentKey, out oldKey, out mirrorFlag);
    }

    // OFFSET 0x02025A18
    private static void SaveDataExtra_SetKeys(SaveData saveData, int extraSaveID, uint currentKey, uint oldKey, byte mirrorFlag)
    {
        MiscSaveBlock.MiscSaveBlock_SetExtraSaveKey(MiscSaveBlock.SaveData_MiscSaveBlock(saveData), extraSaveID, currentKey, oldKey, mirrorFlag);
    }

    // OFFSET 0x02025A3C
    public static bool SaveData_CardBackupType()
    {
        int type;
        if (CardBackup.Identify(BackupTypeFlash4M))
        {
            type = BackupTypeFlash4M;
        }
        else if (CardBackup.Identify(BackupTypeFlash2M))
        {
            type = BackupTypeFlash2M;
        }
        else
        {
            type = 0;
        }
        if (type != 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02025A9C
    public static bool SaveData_CardSave(uint address, byte[] data, int dataOffset, uint size)
    {
        int lockID = SaveData_CardSave_Init(address, data, dataOffset, size);
        int succeeded = 0;
        while (!SaveData_CardSave_Main(lockID, 1, ref succeeded))
        {
        }
        return succeeded != 0;
    }

    // OFFSET 0x02025AC0
    public static bool SaveData_CardLoad(uint address, byte[] data, int dataOffset, uint size)
    {
        bool loaded = CardBackup.Read(address, data, dataOffset, size);
        if (!loaded)
        {
            sSaveData = null;
            sub_0209A74C(1);
        }
        return loaded;
    }

    // OFFSET 0x02025B30
    private static void SaveData_OnCardWriteDone()
    {
        sCardAsyncDone = 1;
    }

    // OFFSET 0x02025B3C
    private static int SaveData_CardSave_Init(uint address, byte[] data, int dataOffset, uint size)
    {
        int lockID = 1;
        byte[] probe = new byte[4];
        if (!CardBackup.Read(0, probe, 0, 4))
        {
            SaveData_CardError(lockID, CardErrorFailure);
        }
        sCardAsyncDone = 0;
        CardBackup.Write(address, data, dataOffset, size);
        SaveData_OnCardWriteDone();
        return lockID;
    }

    // OFFSET 0x02025BB8
    private static bool SaveData_CardSave_Main(int lockID, int locked, ref int succeeded)
    {
        if (sCardAsyncDone != 1)
        {
            return false;
        }
        if (locked == 0)
        {
            return true;
        }
        switch (CardBackup.ResultCode)
        {
            case CardBackup.ResultSuccess:
                succeeded = 1;
                break;
            case CardBackup.ResultError:
                succeeded = 0;
                SaveData_CardError(lockID, CardErrorFailure);
                break;
            case CardBackup.ResultTimeout:
                succeeded = 0;
                SaveData_CardError(lockID, CardErrorTimeout);
                goto case CardBackup.ResultError;
            default:
                ErrorHandling.GF_AssertFail();
                goto case CardBackup.ResultTimeout;
        }
        return true;
    }

    // OFFSET 0x02025C1C
    private static void SaveData_CardError(int lockID, int errorType)
    {
        sSaveData = null;
        sub_0209AA74(1, errorType);
    }

    // OFFSET 0x02025C48
    public static bool SaveData_Checksum(int saveTableID)
    {
        SaveData saveData = SaveData_Ptr();
        FlushTable(saveData, saveTableID);
        int location = (int)saveData.PageInfo[saveTableID].Location;
        int size = SaveTableEntry_BodySize(saveTableID) - 4;
        ushort checksum = Checksum(saveData.Body, location, (uint)size);
        if (checksum == Bin.U16(saveData.Body, location + size / 2 * 2))
        {
            return true;
        }
        ErrorHandling.GF_AssertFail();
        return false;
    }

    // OFFSET 0x02025C84
    public static void SaveData_SetChecksum(int saveTableID)
    {
        SaveData saveData = SaveData_Ptr();
        FlushTable(saveData, saveTableID);
        int location = (int)saveData.PageInfo[saveTableID].Location;
        int size = SaveTableEntry_BodySize(saveTableID) - 4;
        ushort checksum = Checksum(saveData.Body, location, (uint)size);
        Bin.W16(saveData.Body, location + size / 2 * 2, checksum);
    }
}
