using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class PasswordWordBank
{
    private const int WordBankNarcID = 0xBF;
    private const ushort WordNone = 0xFFFF;

    public uint Length;
    public uint[] Words = Array.Empty<uint>();

    // OFFSET 0x02014FB0
    public static PasswordWordBank PasswordWordBank_New(int heapID)
    {
        PasswordWordBank wordBank = new();
        byte[] file = Graphics.LoadMemberFromNARC_OutFileSize(WordBankNarcID, 0, 0, heapID, 0, out uint fileSize)!;
        wordBank.Length = fileSize / 4;
        wordBank.Words = new uint[wordBank.Length];
        for (int i = 0; i < wordBank.Length; i++)
        {
            wordBank.Words[i] = Bin.U32(file, i * 4);
        }
        return wordBank;
    }

    // OFFSET 0x02014FF0
    public static void PasswordWordBank_Free(PasswordWordBank wordBank)
    {
        wordBank.Words = Array.Empty<uint>();
    }

    // OFFSET 0x02015004
    public static uint PasswordWordBank_GetLength(PasswordWordBank wordBank)
    {
        return wordBank.Length;
    }

    // OFFSET 0x02015008
    public static ushort PasswordWordBank_GetWordAtIndex(PasswordWordBank? wordBank, uint index)
    {
        if (wordBank == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (wordBank!.Length <= index)
        {
            return WordNone;
        }
        return (ushort)wordBank.Words[index];
    }

    // OFFSET 0x02015030
    public static short PasswordWordBank_FindWord(PasswordWordBank? wordBank, ushort word)
    {
        if (wordBank == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (uint i = 0; i < wordBank!.Length; i++)
        {
            if (word == wordBank.Words[i])
            {
                return (short)i;
            }
        }
        return -1;
    }
}
