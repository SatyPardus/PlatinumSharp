namespace PlatinumSharp.Game;

public sealed class MysteryGiftMessageArgs
{
    public FieldSystem? FieldSystem;
    public StringTemplate? Template;
    public byte[]? Data;
}

public delegate int MysteryGiftCheckFunc(FieldSystem fieldSystem, byte[]? data);

public delegate void MysteryGiftReceiveFunc(FieldSystem fieldSystem, byte[]? data);

public delegate void MysteryGiftMessageFunc(MysteryGiftMessageArgs args, BufferView bankID, BufferView messageID);

public static partial class ScriptCommands
{
    private const uint MysteryGiftHandlerTable = 0x020EBE94;
    private const int MysteryGiftHandlerSize = 0x10;
    private const int MysteryGiftHeapID = 0x20;
    private const int MysteryGiftLoad = 0;
    private const int MysteryGiftHasGift = 1;
    private const int MysteryGiftGetType = 2;
    private const int MysteryGiftCanReceive = 3;
    private const int MysteryGiftReceive = 4;
    private const int MysteryGiftSuccessMessage = 5;
    private const int MysteryGiftFailMessage = 6;
    private const int MysteryGiftUnload = 7;
    private const int MysteryGiftUnloadAndSave = 8;

    private static uint MysteryGiftHandler(int type, int function)
    {
        return RomData.Arm9U32((uint)(MysteryGiftHandlerTable + (type - 1) * MysteryGiftHandlerSize + function * 4), 1)[0] & ~1u;
    }

    // OFFSET 0x0204B830
    private static void MysteryGiftMessageArgs_Init(MysteryGiftMessageArgs args, FieldSystem fieldSystem, StringTemplate template, byte[]? data)
    {
        args.FieldSystem = fieldSystem;
        args.Template = template;
        args.Data = data;
    }

    // OFFSET 0x0204B838
    private static int MysteryGift_GetPendingGiftType(FieldSystem fieldSystem)
    {
        return MysteryGift.MysteryGift_TryGetPgtType(MysteryGift.MysteryGift_TryGetFirstValidPgtSlot());
    }

    // OFFSET 0x0204B844
    private static byte[]? MysteryGift_GetPendingGiftData(FieldSystem fieldSystem)
    {
        return MysteryGift.MysteryGift_TryGetPgtData(MysteryGift.MysteryGift_TryGetFirstValidPgtSlot());
    }

    // OFFSET 0x0204B850
    private static void MysteryGift_FreePendingGift(FieldSystem fieldSystem)
    {
        MysteryGift.MysteryGift_FreePgtSlot(MysteryGift.MysteryGift_TryGetFirstValidPgtSlot());
    }

    private static MysteryGiftCheckFunc MysteryGiftCheckHandler(uint address)
    {
        return (_, _) => throw Unported.Missing(address);
    }

    private static MysteryGiftReceiveFunc MysteryGiftReceiveHandler(uint address)
    {
        return (_, _) => throw Unported.Missing(address);
    }

    private static MysteryGiftMessageFunc MysteryGiftMessageHandler(uint address)
    {
        return (_, _, _) => throw Unported.Missing(address);
    }

    private static void MysteryGift_RunMessageHandler(ScriptContext ctx, int function)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        int type = MysteryGift_GetPendingGiftType(fieldSystem);
        StringTemplate template = Manager(ctx).StringTemplate!;
        BufferView bankID = VarPointer(ctx);
        BufferView messageID = VarPointer(ctx);
        byte[]? data = MysteryGift_GetPendingGiftData(fieldSystem);
        MysteryGiftMessageArgs args = new();
        MysteryGiftMessageArgs_Init(args, fieldSystem, template, data);
        MysteryGiftMessageHandler(MysteryGiftHandler(type, function))(args, bankID, messageID);
    }

    // OFFSET 0x0204B85C
    private static bool ScrCmd_MysteryGiftGive(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        switch (ScriptContext.ScriptContext_ReadHalfWord(ctx))
        {
            case MysteryGiftLoad:
                MysteryGift.MysteryGift_Load(fieldSystem.SaveData!, MysteryGiftHeapID);
                break;
            case MysteryGiftUnload:
                MysteryGift.MysteryGift_Unload(fieldSystem.SaveData!, 0);
                break;
            case MysteryGiftUnloadAndSave:
                MysteryGift.MysteryGift_Unload(fieldSystem.SaveData!, 1);
                break;
            case MysteryGiftHasGift:
            {
                BufferView destination = VarPointer(ctx);
                destination.W16(0, MysteryGift_GetPendingGiftType(fieldSystem) != 0 ? 1 : 0);
                break;
            }
            case MysteryGiftGetType:
            {
                BufferView destination = VarPointer(ctx);
                destination.W16(0, MysteryGift_GetPendingGiftType(fieldSystem));
                break;
            }
            case MysteryGiftCanReceive:
            {
                BufferView destination = VarPointer(ctx);
                int type = MysteryGift_GetPendingGiftType(fieldSystem);
                byte[]? data = MysteryGift_GetPendingGiftData(fieldSystem);
                destination.W16(0, MysteryGiftCheckHandler(MysteryGiftHandler(type, 0))(fieldSystem, data));
                break;
            }
            case MysteryGiftReceive:
            {
                int type = MysteryGift_GetPendingGiftType(fieldSystem);
                byte[]? data = MysteryGift_GetPendingGiftData(fieldSystem);
                MysteryGiftReceiveHandler(MysteryGiftHandler(type, 1))(fieldSystem, data);
                MysteryGift_FreePendingGift(fieldSystem);
                break;
            }
            case MysteryGiftSuccessMessage:
                MysteryGift_RunMessageHandler(ctx, 2);
                break;
            case MysteryGiftFailMessage:
                MysteryGift_RunMessageHandler(ctx, 3);
                break;
        }
        return false;
    }
}
