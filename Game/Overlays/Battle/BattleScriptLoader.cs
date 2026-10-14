namespace PlatinumSharp.Game;

public static partial class BattleContexts
{
    public const int NarcBattleSubSeq = 1;
    public const int NarcBattleMoveSeq = 0;
    public const int NarcBattleEffectSeq = 0x1E;

    private const int BattleScriptWords = 400;
    private const int ScriptStackDepth = 4;

    private static void BattleSystem_ReadScript(BattleContext battleCtx, int narcID, int file)
    {
        uint size = NARC.NARC_GetMemberSizeByIndexPair(narcID, file);
        byte[] raw = new byte[size];
        NARC.NARC_ReadWholeMemberByIndexPair(raw, narcID, file);
        for (int i = 0; i < raw.Length / 4; i++)
        {
            battleCtx.BattleScript[i] = (int)Nitro.Bin.U32(raw, i * 4);
        }
    }

    // OFFSET 0x02251E1C
    public static void BattleSystem_LoadScript(BattleContext battleCtx, int narcID, int file)
    {
        if (NARC.NARC_GetMemberSizeByIndexPair(narcID, file) >= BattleScriptWords * 4)
        {
            ErrorHandling.GF_AssertFail();
        }
        battleCtx.ScriptNarc = narcID;
        battleCtx.ScriptFile = file;
        battleCtx.ScriptCursor = 0;
        BattleSystem_ReadScript(battleCtx, narcID, file);
    }

    // OFFSET 0x02251E5C
    public static void BattleSystem_CallScript(BattleContext battleCtx, int narcID, int file)
    {
        if (NARC.NARC_GetMemberSizeByIndexPair(narcID, file) >= BattleScriptWords * 4)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (battleCtx.ScriptStackPointer >= ScriptStackDepth)
        {
            ErrorHandling.GF_AssertFail();
        }
        battleCtx.ScriptStackNarc[battleCtx.ScriptStackPointer] = battleCtx.ScriptNarc;
        battleCtx.ScriptStackFile[battleCtx.ScriptStackPointer] = battleCtx.ScriptFile;
        battleCtx.ScriptStackCursor[battleCtx.ScriptStackPointer] = battleCtx.ScriptCursor;
        battleCtx.ScriptStackPointer++;
        battleCtx.ScriptNarc = narcID;
        battleCtx.ScriptFile = file;
        battleCtx.ScriptCursor = 0;
        BattleSystem_ReadScript(battleCtx, narcID, file);
    }

    // OFFSET 0x02251EF4
    public static int BattleSystem_PopScript(BattleContext battleCtx)
    {
        if (battleCtx.ScriptStackPointer == 0)
        {
            return 1;
        }
        battleCtx.ScriptStackPointer--;
        BattleSystem_LoadScript(battleCtx, battleCtx.ScriptStackNarc[battleCtx.ScriptStackPointer], battleCtx.ScriptStackFile[battleCtx.ScriptStackPointer]);
        battleCtx.ScriptCursor = battleCtx.ScriptStackCursor[battleCtx.ScriptStackPointer];
        return 0;
    }
}
