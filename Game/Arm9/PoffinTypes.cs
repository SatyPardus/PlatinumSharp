namespace PlatinumSharp.Game;

public sealed class PoffinTypeList
{
    public const int NumPoffinTypes = 29;

    private const int MessageArchive = 0x1A;
    private const int PoffinTypeNamesBank = 0x1D1;
    private const uint NameLength = 0x16;

    public MessageLoader? MsgLoader;
    public readonly GameString?[] Names = new GameString?[NumPoffinTypes];

    // OFFSET 0x020158A8
    public static PoffinTypeList PoffinTypeList_New(int heapID)
    {
        PoffinTypeList types = new();
        types.MsgLoader = MessageLoader.MessageLoader_Init(0, MessageArchive, PoffinTypeNamesBank, heapID);
        for (int i = 0; i < NumPoffinTypes; i++)
        {
            types.Names[i] = GameString.String_Init(NameLength, heapID);
            MessageLoader.MessageLoader_GetString(types.MsgLoader!, (uint)i, types.Names[i]!);
        }
        return types;
    }

    // OFFSET 0x020158F4
    public static void PoffinTypeList_Free(PoffinTypeList types)
    {
        for (int i = NumPoffinTypes - 1; i >= 0; i--)
        {
            GameString.String_Free(types.Names[i]);
        }
        MessageLoader.MessageLoader_Free(types.MsgLoader);
    }

    // OFFSET 0x02015918
    public static GameString? PoffinTypeList_GetString(PoffinTypeList types, int type)
    {
        return types.Names[type];
    }
}
