using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public delegate void FieldSysFunc(FieldSystem fieldSystem);

public sealed class HiddenItemTilePosition
{
    public const int Size = 6;

    public ushort ScreenTileX;
    public ushort ScreenTileZ;
    public byte Range;
}

public sealed class ApproachingTrainer
{
    public const int Size = 0x1C;

    public int SightRange;
    public int Direction;
    public int ScriptID;
    public int TrainerID;
    public int ApproachType;
    public MapObject? Object;
    public SysTask? Task;
}

public sealed class ScriptManager
{
    public const int MagicOffset = 0x00;
    public const int StateOffset = 0x04;
    public const int MessageIDOffset = 0x05;
    public const int MovementCountOffset = 0x06;
    public const int SubContextActiveOffset = 0x07;
    public const int IsMessageBoxOpenOffset = 0x08;
    public const int NumActiveContextsOffset = 0x09;
    public const int ScriptIDOffset = 0x0A;
    public const int BattleResultOffset = 0x0C;
    public const int FieldMenuManagerOffset = 0x10;
    public const int WindowOffset = 0x14;
    public const int ControlUIOffset = 0x24;
    public const int PlayerDirOffset = 0x28;
    public const int TargetObjectOffset = 0x2C;
    public const int CameraObjectOffset = 0x30;
    public const int SaveTypeOffset = 0x34;
    public const int ContextsOffset = 0x38;
    public const int StringTemplateOffset = 0x40;
    public const int MessageBufferOffset = 0x44;
    public const int TemporaryBufferOffset = 0x48;
    public const int SavingIconOffset = 0x4C;
    public const int TrainersOffset = 0x50;
    public const int DataOffset = 0x88;
    public const int FunctionOffset = 0xA4;
    public const int PartyManagementDataOffset = 0xA8;
    public const int DataPtrOffset = 0xAC;
    public const int OverworldAnimManagerOffset = 0xB0;
    public const int PlayerTaskOffset = 0xB4;
    public const int SpecialCurrencyWindowOffset = 0xB8;
    public const int MoneyWindowOffset = 0xC8;
    public const int SaveInfoWindowOffset = 0xD8;
    public const int Size = 0xDC;

    public const uint MagicNumber = 0x0003643F;

    public const int ContextMain = 0;
    public const int ContextSub = 1;
    public const int NumContexts = 2;
    public const int NumApproachingTrainers = 2;

    public const int MemberFieldMenuManager = 0;
    public const int MemberWindow = 1;
    public const int MemberControlUI = 2;
    public const int MemberMessageID = 3;
    public const int MemberMovementCount = 4;
    public const int MemberSubContextActive = 5;
    public const int MemberIsMessageBoxOpen = 6;
    public const int MemberNumActiveContexts = 7;
    public const int MemberScriptID = 8;
    public const int MemberPlayerDir = 9;
    public const int MemberTargetObject = 10;
    public const int MemberCameraObject = 11;
    public const int MemberSaveType = 12;
    public const int MemberMainContext = 13;
    public const int MemberSubContext = 14;
    public const int MemberStringTemplate = 15;
    public const int MemberMessageBuffer = 16;
    public const int MemberTemporaryBuffer = 17;
    public const int MemberSavingIcon = 18;
    public const int MemberPartyManagementData = 19;
    public const int MemberDataPtr = 20;
    public const int MemberOverworldAnimManager = 21;
    public const int MemberPlayerTask = 22;
    public const int MemberBattleResult = 23;
    public const int MemberTrainer0SightRange = 24;
    public const int MemberTrainer1SightRange = 31;
    public const int MemberSpecialCurrencyWindow = 38;
    public const int MemberMoneyWindow = 39;
    public const int MemberSaveInfoWindow = 40;
    public const int MemberDataStart = 41;
    public const int MemberParameter0 = 41;
    public const int MemberParameter1 = 42;
    public const int MemberParameter2 = 43;
    public const int MemberParameter3 = 44;
    public const int MemberBg3ScrollCountX = 45;
    public const int MemberBg3ScrollCountY = 46;
    public const int MemberBg3ScrollDistX = 49;
    public const int MemberBg3ScrollDirX = 50;
    public const int MemberBg3ScrollDistY = 51;
    public const int MemberBg3ScrollDirY = 52;
    public const int MemberTargetObjectID = 54;
    public const int MemberMax = 55;
    public const int DataCount = MemberMax - MemberDataStart;

    private const int HeapIDFieldTask = 0x0B;
    private const int NarcScripts = 10;
    private const int NarcMessages = 0x1A;
    private const int MessageLoaderTypeNarcHandle = 1;
    private const int StringTemplateArgs = 8;
    private const int StringTemplateMaxLength = 0x40;
    private const int BufferSize = 0x400;
    private const int ScriptNewGameInit = 9600;
    private const int HiddenItemScriptFirst = 8000;
    private const int HiddenItemScriptLast = 8799;
    private const uint HiddenItemTableAddress = 0x020EA378;
    private const int HiddenItemEntrySize = 8;
    private const int HiddenItemCount = 0x101;
    private const int HiddenItemEntryItem = 0;
    private const int HiddenItemEntryQuantity = 2;
    private const int HiddenItemEntryRange = 3;
    private const int HiddenItemEntryIndex = 6;
    private const int BgEventTypeHiddenItem = 2;
    private const int InitScriptFrame = 1;
    private const int InitScriptEntrySize = 5;
    private const int InitScriptConditionSize = 6;
    private const int NoScript = 0xFFFF;
    private const int LocationMapIdOffset = 0x00;

    public uint Magic;
    public byte State;
    public byte MessageID;
    public byte MovementCount;
    public byte SubContextActive;
    public byte IsMessageBoxOpen;
    public byte NumActiveContexts;
    public ushort ScriptID;
    public int BattleResult;
    public object? FieldMenuManager;
    public readonly Window Window = new();
    public Menu? ControlUI;
    public int PlayerDir;
    public MapObject? TargetObject;
    public MapObject? CameraObject;
    public BufferView? SaveType;
    public readonly ScriptContext?[] Contexts = new ScriptContext?[NumContexts];
    public StringTemplate? StringTemplate;
    public GameString? MessageBuffer;
    public GameString? TemporaryBuffer;
    public WaitDial? SavingIcon;
    public readonly ApproachingTrainer[] Trainers = { new(), new() };
    public readonly byte[] Data = new byte[DataCount * 2];
    public FieldSysFunc? Function;
    public object? PartyManagementData;
    public object? DataPtr;
    public object? OverworldAnimManager;
    public SysTask? PlayerTask;
    public readonly Window SpecialCurrencyWindow = new();
    public Window? MoneyWindow;
    public object? SaveInfoWindow;

    private static byte[]? sHiddenItems;

    // OFFSET 0x0203E880
    public static void ScriptManager_Set(FieldSystem fieldSystem, ushort scriptID, MapObject? mapObject)
    {
        ScriptManager scriptManager = ScriptManager_New();
        ScriptManager_Init(fieldSystem, scriptManager, scriptID, mapObject, null);
        FieldTask.FieldSystem_CreateTask(fieldSystem, FieldTask_RunScript, scriptManager);
    }

    // OFFSET 0x0203E8B0
    public static void ScriptManager_SetApproachingTrainer(FieldSystem fieldSystem, MapObject? mapObject, int sightRange, int direction, int scriptID, int trainerID, int approachType, int approachNum)
    {
        ScriptManager scriptManager = (ScriptManager)FieldTask.FieldTask_GetEnv(fieldSystem.Task!)!;
        ApproachingTrainer trainer = scriptManager.Trainers[approachNum];
        trainer.SightRange = sightRange;
        trainer.Direction = direction;
        trainer.ScriptID = scriptID;
        trainer.TrainerID = trainerID;
        trainer.ApproachType = approachType;
        trainer.Object = mapObject;
    }

    // OFFSET 0x0203E8E0
    public static void ScriptManager_Start(FieldTask task, ushort scriptID, MapObject? mapObject, BufferView? saveType)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        ScriptManager scriptManager = ScriptManager_New();
        ScriptManager_Init(fieldSystem, scriptManager, scriptID, mapObject, saveType);
        FieldTask.FieldTask_InitCall(task, FieldTask_RunScript, scriptManager);
    }

    // OFFSET 0x0203E918
    public static void ScriptManager_Change(FieldTask task, ushort scriptID, MapObject? mapObject)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        ScriptManager scriptManager = ScriptManager_New();
        ScriptManager_Init(fieldSystem, scriptManager, scriptID, mapObject, null);
        FieldTask.FieldTask_InitJump(task, FieldTask_RunScript, scriptManager);
    }

    // OFFSET 0x0203E950
    private static bool FieldTask_RunScript(FieldTask task)
    {
        ScriptManager scriptManager = (ScriptManager)FieldTask.FieldTask_GetEnv(task)!;
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        switch (scriptManager.State)
        {
            case 0:
                scriptManager.Contexts[ContextMain] = ScriptContext_CreateAndStart(fieldSystem, scriptManager.ScriptID);
                scriptManager.NumActiveContexts = 1;
                scriptManager.StringTemplate = StringTemplate.StringTemplate_New(StringTemplateArgs, StringTemplateMaxLength, HeapIDFieldTask);
                scriptManager.MessageBuffer = GameString.String_Init(BufferSize, HeapIDFieldTask);
                scriptManager.TemporaryBuffer = GameString.String_Init(BufferSize, HeapIDFieldTask);
                scriptManager.State++;
                goto case 1;
            case 1:
                for (int i = 0; i < NumContexts; i++)
                {
                    ScriptContext? ctx = scriptManager.Contexts[i];
                    if (ctx != null && !ScriptContext.ScriptContext_Run(ctx))
                    {
                        ScriptManager_FreeContext(ctx);
                        if (scriptManager.NumActiveContexts == 0)
                        {
                            ErrorHandling.GF_AssertFail();
                        }
                        scriptManager.Contexts[i] = null;
                        scriptManager.NumActiveContexts--;
                    }
                }
                if (scriptManager.NumActiveContexts == 0)
                {
                    FieldSysFunc? function = scriptManager.Function;
                    StringTemplate.StringTemplate_Free(scriptManager.StringTemplate!);
                    GameString.String_Free(scriptManager.MessageBuffer);
                    GameString.String_Free(scriptManager.TemporaryBuffer);
                    scriptManager.Magic = 0;
                    if (function != null)
                    {
                        function(fieldSystem);
                        return false;
                    }
                    return true;
                }
                break;
        }
        return false;
    }

    // OFFSET 0x0203EA28
    private static ScriptManager ScriptManager_New()
    {
        ScriptManager scriptManager = new();
        scriptManager.Magic = MagicNumber;
        return scriptManager;
    }

    // OFFSET 0x0203EA50
    private static void ScriptManager_FreeContext(ScriptContext ctx)
    {
        MessageLoader.MessageLoader_Free(ctx.Loader);
        Heap.Heap_Free(ctx.Scripts!);
    }

    // OFFSET 0x0203EA68
    private static void ScriptManager_Init(FieldSystem fieldSystem, ScriptManager scriptManager, ushort scriptID, MapObject? mapObject, BufferView? saveType)
    {
        BufferView targetObjectID = ScriptManager_GetMemberPtr(scriptManager, MemberTargetObjectID)!;
        scriptManager.PlayerDir = PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar!);
        scriptManager.TargetObject = mapObject;
        scriptManager.ScriptID = scriptID;
        scriptManager.SaveType = saveType;
        if (mapObject != null)
        {
            targetObjectID.W16(0, MapObject.MapObject_GetLocalID(mapObject));
        }
        if (scriptID >= HiddenItemScriptFirst && scriptID <= HiddenItemScriptLast)
        {
            ScriptManager_SetHiddenItemParameters(scriptManager, scriptID);
        }
    }

    // OFFSET 0x0203EAB8
    public static ScriptContext ScriptContext_CreateAndStart(FieldSystem fieldSystem, ushort scriptID)
    {
        ScriptContext ctx = new();
        ScriptContext.ScriptContext_Init(ctx, ScriptCommands.Table, ScriptCommands.TableSize);
        ScriptContext_Setup(fieldSystem, ctx, scriptID, 0);
        return ctx;
    }

    // OFFSET 0x0203EAF4
    private static void ScriptContext_Setup(FieldSystem fieldSystem, ScriptContext ctx, ushort scriptID, int unused)
    {
        ctx.FieldSystem = fieldSystem;
        ushort offsetID = ScriptContext_LoadAndOffsetID(fieldSystem, ctx, scriptID);
        ScriptContext.ScriptContext_Start(ctx, 0);
        ScriptContext_JumpToOffsetID(ctx, offsetID);
        ScriptContext.ScriptContext_SetTask(ctx, fieldSystem.Task);
    }

    // OFFSET 0x0203EB20
    private static ushort ScriptContext_LoadAndOffsetID(FieldSystem fieldSystem, ScriptContext ctx, ushort scriptID)
    {
        if (scriptID >= 10490)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x1F3, 0x21D);
            return (ushort)(scriptID - 10490);
        }
        if (scriptID >= 10450)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x1F4, 0x10);
            return (ushort)(scriptID - 10450);
        }
        if (scriptID >= 10400)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x190, 0xCB);
            return (ushort)(scriptID - 10400);
        }
        if (scriptID >= 10300)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x41B, 0x228);
            return (ushort)(scriptID - 10300);
        }
        if (scriptID >= 10200)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x197, 0x17B);
            return (ushort)(scriptID - 10200);
        }
        if (scriptID >= 10150)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x45C, 0x26D);
            return (ushort)(scriptID - 10150);
        }
        if (scriptID >= 10100)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x45B, 0x26E);
            return (ushort)(scriptID - 10100);
        }
        if (scriptID >= 10000)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x199, 0x17D);
            return (ushort)(scriptID - 10000);
        }
        if (scriptID >= 9950)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x19B, 0x17F);
            return (ushort)(scriptID - 9950);
        }
        if (scriptID >= 9900)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x18D, 0xD5);
            return (ushort)(scriptID - 9900);
        }
        if (scriptID >= 9800)
        {
            ScriptContext_Load(fieldSystem, ctx, 0xD4, 0xD9);
            return (ushort)(scriptID - 9800);
        }
        if (scriptID >= 9700)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x1A6, 0x1AD);
            return (ushort)(scriptID - 9700);
        }
        if (scriptID >= 9600)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x19C, 0xD5);
            return (ushort)(scriptID - 9600);
        }
        if (scriptID >= 9500)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x1F5, 0x223);
            return (ushort)(scriptID - 9500);
        }
        if (scriptID >= 9400)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x1AA, 0x1B0);
            return (ushort)(scriptID - 9400);
        }
        if (scriptID >= 9300)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x196, 0x176);
            return (ushort)(scriptID - 9300);
        }
        if (scriptID >= 9200)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x1A7, 0x1AE);
            return (ushort)(scriptID - 9200);
        }
        if (scriptID >= 9100)
        {
            ScriptContext_Load(fieldSystem, ctx, 0, 0xB);
            return (ushort)(scriptID - 9100);
        }
        if (scriptID >= 9000)
        {
            ScriptContext_Load(fieldSystem, ctx, 0xD5, 0xDD);
            return (ushort)(scriptID - 9000);
        }
        if (scriptID >= 8970)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x1A9, 7);
            return (ushort)(scriptID - 8970);
        }
        if (scriptID >= 8950)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x1F2, 0x21B);
            return (ushort)(scriptID - 8950);
        }
        if (scriptID >= 8900)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x1A8, 0x1AF);
            return (ushort)(scriptID - 8900);
        }
        if (scriptID >= 8800)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x1F1, 0x21A);
            return (ushort)(scriptID - 8800);
        }
        if (scriptID >= 8000)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x198, 0x17C);
            return (ushort)(scriptID - 8000);
        }
        if (scriptID >= 7000)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x194, 0x171);
            return (ushort)(scriptID - 7000);
        }
        if (scriptID >= 5000)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x45A, 0xD5);
            return (ushort)(scriptID - 5000);
        }
        if (scriptID >= 3000)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x45A, 0xD5);
            return (ushort)(scriptID - 3000);
        }
        if (scriptID >= 2800)
        {
            ScriptContext_Load(fieldSystem, ctx, 0x19D, 0x18D);
            return (ushort)(scriptID - 2800);
        }
        if (scriptID >= 2500)
        {
            ScriptContext_Load(fieldSystem, ctx, 1, 0x11);
            return (ushort)(scriptID - 2500);
        }
        if (scriptID >= 2000)
        {
            ScriptContext_Load(fieldSystem, ctx, 0xD3, 0xD5);
            return (ushort)(scriptID - 2000);
        }
        if (scriptID >= 1)
        {
            ScriptContext_LoadFromCurrentMap(fieldSystem, ctx);
            return (ushort)(scriptID - 1);
        }
        ScriptContext_Load(fieldSystem, ctx, 0x192, 0x163);
        return 0;
    }

    // OFFSET 0x0203EF14
    private static void ScriptContext_Load(FieldSystem fieldSystem, ScriptContext ctx, int scriptFile, uint textBank)
    {
        ctx.Scripts = NARC.NARC_AllocAndReadWholeMemberByIndexPair(NarcScripts, scriptFile, HeapIDFieldTask);
        ctx.ScriptFile = scriptFile;
        ctx.Loader = MessageLoader.MessageLoader_Init(MessageLoaderTypeNarcHandle, NarcMessages, (int)textBank, HeapIDFieldTask);
    }

    // OFFSET 0x0203EF38
    private static void ScriptContext_LoadFromCurrentMap(FieldSystem fieldSystem, ScriptContext ctx)
    {
        ctx.Scripts = ScriptContext_LoadMapScripts((int)fieldSystem.Location!.U32(LocationMapIdOffset));
        ctx.ScriptFile = MapHeader.MapHeader_GetScriptsArchiveID((int)fieldSystem.Location!.U32(LocationMapIdOffset));
        ctx.Loader = MessageLoader.MessageLoader_Init(MessageLoaderTypeNarcHandle, NarcMessages, ScriptContext_GetMapMessageBank((int)fieldSystem.Location!.U32(LocationMapIdOffset)), HeapIDFieldTask);
    }

    // OFFSET 0x0203EF60
    public static BufferView? ScriptManager_GetMemberPtr(ScriptManager scriptManager, int member)
    {
        if ((uint)member >= MemberMax)
        {
            ErrorHandling.GF_AssertFail();
            return null;
        }
        if (member >= MemberDataStart)
        {
            return new BufferView(scriptManager.Data, (member - MemberDataStart) * 2);
        }
        throw new InvalidOperationException($"Script manager member {member} is a typed field");
    }

    // OFFSET 0x0203F098
    public static BufferView FieldSystem_GetScriptMemberPtr(FieldSystem fieldSystem, int member)
    {
        return ScriptManager_GetMemberPtr(FieldSystem_GetScriptManager(fieldSystem), member)!;
    }

    public static ScriptManager FieldSystem_GetScriptManager(FieldSystem fieldSystem)
    {
        ScriptManager scriptManager = (ScriptManager)FieldTask.FieldTask_GetEnv(fieldSystem.Task!)!;
        if (scriptManager.Magic != MagicNumber)
        {
            ErrorHandling.GF_AssertFail();
        }
        return scriptManager;
    }

    // OFFSET 0x0203F0C0
    public static void FieldSystem_ShowStartMenu(FieldSystem fieldSystem)
    {
        ScriptManager scriptManager = (ScriptManager)FieldTask.FieldTask_GetEnv(fieldSystem.Task!)!;
        if (StartMenu.FieldSystem_IsInValidLocation(fieldSystem))
        {
            scriptManager.Function = StartMenu.StartMenu_OpenFromScript;
        }
    }

    // OFFSET 0x0203F0E4
    private static void ScriptContext_JumpToOffsetID(ScriptContext ctx, int id)
    {
        ctx.ScriptPtr += id * 4;
        uint offset = ScriptContext.ScriptContext_ReadWord(ctx);
        ctx.ScriptPtr += (int)offset;
    }

    // OFFSET 0x0203F0FC
    private static byte[] ScriptContext_LoadMapScripts(int mapHeaderID)
    {
        return NARC.NARC_AllocAndReadWholeMemberByIndexPair(NarcScripts, MapHeader.MapHeader_GetScriptsArchiveID(mapHeaderID), HeapIDFieldTask);
    }

    // OFFSET 0x0203F110
    private static int ScriptContext_GetMapMessageBank(int mapHeaderID)
    {
        return MapHeader.MapHeader_GetMsgArchiveID(mapHeaderID);
    }

    // OFFSET 0x0203F21C
    public static void FieldSystem_SetScriptParameters(FieldSystem fieldSystem, ushort parameter0, ushort parameter1, ushort parameter2, ushort parameter3)
    {
        FieldSystem_GetScriptMemberPtr(fieldSystem, MemberParameter0).W16(0, parameter0);
        FieldSystem_GetScriptMemberPtr(fieldSystem, MemberParameter1).W16(0, parameter1);
        FieldSystem_GetScriptMemberPtr(fieldSystem, MemberParameter2).W16(0, parameter2);
        FieldSystem_GetScriptMemberPtr(fieldSystem, MemberParameter3).W16(0, parameter3);
    }

    private static int FindHiddenItem(ushort scriptID)
    {
        sHiddenItems ??= RomData.Arm9(HiddenItemTableAddress, HiddenItemCount * HiddenItemEntrySize);
        ushort index = FieldSystemVarsFlags.Script_GetHiddenItemScript(scriptID);
        int entry = 0;
        while (entry < HiddenItemCount)
        {
            if (index == Bin.U16(sHiddenItems, entry * HiddenItemEntrySize + HiddenItemEntryIndex))
            {
                break;
            }
            entry++;
        }
        return entry;
    }

    // OFFSET 0x0203F3C8
    public static byte Script_GetHiddenItemRange(ushort scriptID)
    {
        int entry = FindHiddenItem(scriptID);
        if (entry >= HiddenItemCount)
        {
            ErrorHandling.GF_AssertFail();
            return 0;
        }
        return sHiddenItems![entry * HiddenItemEntrySize + HiddenItemEntryRange];
    }

    // OFFSET 0x0203F404
    private static bool ScriptManager_SetHiddenItemParameters(ScriptManager scriptManager, ushort scriptID)
    {
        BufferView item = ScriptManager_GetMemberPtr(scriptManager, MemberParameter0)!;
        BufferView quantity = ScriptManager_GetMemberPtr(scriptManager, MemberParameter1)!;
        BufferView flag = ScriptManager_GetMemberPtr(scriptManager, MemberParameter2)!;
        int entry = FindHiddenItem(scriptID);
        if (entry >= HiddenItemCount)
        {
            ErrorHandling.GF_AssertFail();
            return false;
        }
        item.W16(0, Bin.U16(sHiddenItems!, entry * HiddenItemEntrySize + HiddenItemEntryItem));
        quantity.W16(0, sHiddenItems![entry * HiddenItemEntrySize + HiddenItemEntryQuantity]);
        flag.W16(0, FieldSystemVarsFlags.Script_GetHiddenItemFlag(scriptID));
        return true;
    }

    // OFFSET 0x0203F478
    public static HiddenItemTilePosition[] FieldSystem_GetNearbyHiddenItems(FieldSystem fieldSystem, int heapID)
    {
        int found = 0;
        int numBgEvents = MapHeaderData.MapHeaderData_GetNumBgEvents(fieldSystem);
        HiddenItemTilePosition[] positions = new HiddenItemTilePosition[numBgEvents + 1];
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = new HiddenItemTilePosition();
        }
        if (numBgEvents == 0)
        {
            positions[0].Range = 0xFF;
            positions[0].ScreenTileX = 0xFFFF;
            positions[0].ScreenTileZ = 0xFFFF;
            return positions;
        }
        BgEvent[]? bgEvents = MapHeaderData.MapHeaderData_GetBgEvents(fieldSystem);
        if (bgEvents == null)
        {
            positions[0].Range = 0xFF;
            positions[0].ScreenTileX = 0xFFFF;
            positions[0].ScreenTileZ = 0xFFFF;
            return positions;
        }
        int playerX = PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar!);
        int playerZ = PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar!);
        int minX = playerX - 7;
        int maxX = playerX + 7;
        int minZ = playerZ - 7;
        int maxZ = playerZ + 6;
        if (minX < 0)
        {
            minX = 0;
        }
        if (minZ < 0)
        {
            minZ = 0;
        }
        for (int i = 0; i < numBgEvents + 1 && i < bgEvents.Length; i++)
        {
            BgEvent bgEvent = bgEvents[i];
            if (bgEvent.Type == BgEventTypeHiddenItem
                && !FieldSystemVarsFlags.FieldSystem_CheckFlag(fieldSystem, FieldSystemVarsFlags.Script_GetHiddenItemFlag(bgEvent.Script))
                && bgEvent.X >= minX
                && bgEvent.X <= maxX
                && bgEvent.Z >= minZ
                && bgEvent.Z <= maxZ)
            {
                positions[found].Range = Script_GetHiddenItemRange(bgEvent.Script);
                positions[found].ScreenTileX = (ushort)Math.Abs(7 - (playerX - bgEvent.X));
                positions[found].ScreenTileZ = (ushort)Math.Abs(7 - (playerZ - bgEvent.Z));
                found++;
            }
        }
        positions[found].Range = 0xFF;
        positions[found].ScreenTileX = 0xFFFF;
        positions[found].ScreenTileZ = 0xFFFF;
        return positions;
    }

    // OFFSET 0x0203F598
    public static void FieldSystem_InitNewGameState(FieldSystem fieldSystem)
    {
        FieldSystem_RunScript(fieldSystem, ScriptNewGameInit);
    }

    // OFFSET 0x0203F5A4
    public static void FieldSystem_RunScript(FieldSystem fieldSystem, ushort scriptID)
    {
        ScriptContext ctx = ScriptContext_CreateAndStart(fieldSystem, scriptID);
        while (ScriptContext.ScriptContext_Run(ctx))
        {
        }
        ScriptManager_FreeContext(ctx);
    }

    // OFFSET 0x0203F5C0
    public static bool FieldSystem_RunInitScript(FieldSystem fieldSystem, int initScriptType)
    {
        byte[]? initScripts = MapHeaderData.MapHeaderData_GetInitScriptBytes(fieldSystem);
        if (initScripts == null)
        {
            return false;
        }
        ushort scriptID;
        if (initScriptType == InitScriptFrame)
        {
            scriptID = InitScripts_FindFrameScript(fieldSystem, initScripts, (byte)initScriptType);
        }
        else
        {
            scriptID = InitScripts_FindScript(initScripts, (byte)initScriptType);
        }
        if (scriptID == NoScript)
        {
            return false;
        }
        if (initScriptType == InitScriptFrame)
        {
            ScriptManager_Set(fieldSystem, scriptID, null);
        }
        else
        {
            FieldSystem_RunScript(fieldSystem, scriptID);
        }
        return true;
    }

    // OFFSET 0x0203F610
    private static ushort InitScripts_FindScript(byte[] initScripts, byte initScriptType)
    {
        int position = 0;
        while (true)
        {
            if (initScripts[position] == 0)
            {
                return NoScript;
            }
            if (initScripts[position] == initScriptType)
            {
                return (ushort)(initScripts[position + 1] + (initScripts[position + 2] << 8));
            }
            position += InitScriptEntrySize;
        }
    }

    // OFFSET 0x0203F638
    private static ushort InitScripts_FindFrameScript(FieldSystem fieldSystem, byte[] initScripts, byte initScriptType)
    {
        int position = 0;
        uint tableOffset;
        while (true)
        {
            if (initScripts[position] == 0)
            {
                return NoScript;
            }
            if (initScripts[position] == initScriptType)
            {
                tableOffset = ((uint)initScripts[position + 4] << 24) + ((uint)initScripts[position + 3] << 16) + (uint)(initScripts[position + 1] + (initScripts[position + 2] << 8));
                position += InitScriptEntrySize;
                break;
            }
            position += InitScriptEntrySize;
        }
        if (tableOffset == 0)
        {
            return NoScript;
        }
        position += (int)tableOffset;
        while (true)
        {
            if (initScripts[position] == 0)
            {
                return NoScript;
            }
            ushort varID = (ushort)(initScripts[position] + (initScripts[position + 1] << 8));
            if (varID == 0)
            {
                return NoScript;
            }
            ushort compareID = (ushort)(initScripts[position + 2] + (initScripts[position + 3] << 8));
            ushort value = FieldSystemVarsFlags.FieldSystem_TryGetVar(fieldSystem, varID);
            ushort compare = FieldSystemVarsFlags.FieldSystem_TryGetVar(fieldSystem, compareID);
            if (value == compare)
            {
                return (ushort)(initScripts[position + 4] + (initScripts[position + 5] << 8));
            }
            position += InitScriptConditionSize;
        }
    }
}
