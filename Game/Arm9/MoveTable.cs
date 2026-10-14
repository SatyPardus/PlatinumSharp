using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public static class MoveTable
{
    public const int Size = 0x10;
    public const int ParamEffect = 0;
    public const int ParamClass = 1;
    public const int ParamPower = 2;
    public const int ParamType = 3;
    public const int ParamAccuracy = 4;
    public const int ParamPP = 5;
    public const int ParamEffectChance = 6;
    public const int ParamRange = 7;
    public const int ParamPriority = 8;
    public const int ParamFlags = 9;
    public const int ParamContestEffect = 10;
    public const int ParamContestType = 11;

    private const int NarcMoveData = 9;
    private const int MaxPPUps = 3;
    private const int PPUpPercent = 20;

    // OFFSET 0x020790B0
    public static void MoveTable_Load(MoveData[] table)
    {
        byte[] data = new byte[table.Length * Size];
        NARC.NARC_ReadFromMemberByIndexPair(data, NarcMoveData, 0, 0, data.Length);
        for (int i = 0; i < table.Length; i++)
        {
            int entry = i * Size;
            MoveData move = table[i];
            move.Effect = (ushort)Bin.U16(data, entry);
            move.Class = data[entry + 2];
            move.Power = data[entry + 3];
            move.Type = data[entry + 4];
            move.Accuracy = data[entry + 5];
            move.Pp = data[entry + 6];
            move.EffectChance = data[entry + 7];
            move.Range = (ushort)Bin.U16(data, entry + 8);
            move.Priority = (sbyte)data[entry + 0xA];
            move.Flags = data[entry + 0xB];
            move.Contest.Effect = data[entry + 0xC];
            move.Contest.Type = data[entry + 0xD];
        }
    }

    // OFFSET 0x020790C4
    public static uint MoveTable_LoadParam(int move, int param)
    {
        byte[] moveData = new byte[Size];
        MoveTable_LoadMove(move, moveData);
        return MoveTable_GetParam(moveData, param);
    }

    // OFFSET 0x020790DC
    public static uint MoveTable_CalcMaxPP(ushort move, byte ppUps)
    {
        int ups = ppUps;
        if (ups > MaxPPUps)
        {
            ups = MaxPPUps;
        }
        int pp = (byte)MoveTable_LoadParam(move, ParamPP);
        return (byte)(pp + pp * PPUpPercent * ups / 100);
    }

    // OFFSET 0x02079104
    public static uint MoveTable_GetParam(byte[] moveData, int param)
    {
        switch (param)
        {
            case ParamEffect:
                return (uint)Bin.U16(moveData, 0);
            case ParamClass:
                return moveData[2];
            case ParamPower:
                return moveData[3];
            case ParamType:
                return moveData[4];
            case ParamAccuracy:
                return moveData[5];
            case ParamPP:
                return moveData[6];
            case ParamEffectChance:
                return moveData[7];
            case ParamRange:
                return (uint)Bin.U16(moveData, 8);
            case ParamPriority:
                return (uint)(sbyte)moveData[0xA];
            case ParamFlags:
                return moveData[0xB];
            case ParamContestEffect:
                return moveData[0xC];
            case ParamContestType:
                return moveData[0xD];
        }
        return 0;
    }

    // OFFSET 0x02079160
    public static void MoveTable_LoadMove(int move, byte[] moveData)
    {
        NARC.NARC_ReadWholeMemberByIndexPair(moveData, NarcMoveData, move);
    }
}
