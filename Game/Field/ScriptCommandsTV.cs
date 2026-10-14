namespace PlatinumSharp.Game;

public static partial class ScriptCommands
{
    private const uint TVInterviewTable = 0x020EBB50;
    private const int TVInterviewEntrySize = 0x10;
    private const int TVProgramTypeInterview = 1;
    private const ushort TVBroadcastGetState = 0;
    private const ushort TVBroadcastGetProgramMessage = 1;
    private const ushort TVBroadcastFinishProgram = 2;
    private const ushort TVBroadcastPrintSegment = 3;
    private const ushort TVBroadcastGetCommercial = 4;
    private const ushort TVBroadcastFindSegment = 5;
    private const ushort TVBroadcastFindAnySegment = 6;
    private const ushort MessageBankTVPrograms = 0x26E;
    private const ushort MessageBankTVCommercials = 0x26F;

    private static void RegisterTV(Dictionary<uint, ScrCmdFunc> ported)
    {
        ported[0x020490F0] = ScrCmd_CheckTVInterviewEligible;
        ported[0x02048F74] = ScrCmd_SaveTVSegmentHiddenItem;
        ported[0x02048DD8] = ScrCmd_CallTVBroadcast;
    }

    private static TVBroadcast FieldSystem_GetTVBroadcast(FieldSystem fieldSystem)
    {
        return (TVBroadcast)SaveDataTable.SaveData_GetTVBroadcast(fieldSystem.SaveData!);
    }

    // OFFSET 0x02048DD8
    private static bool ScrCmd_CallTVBroadcast(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        switch (ScriptContext.ScriptContext_ReadHalfWord(ctx))
        {
            case TVBroadcastGetState:
            {
                BufferView destination = VarPointer(ctx);
                destination.W16(0, TVPrograms.FieldSystem_GetTVProgramState(fieldSystem));
                break;
            }
            case TVBroadcastGetProgramMessage:
            {
                ushort which = ScriptContext.ScriptContext_ReadHalfWord(ctx);
                BufferView bankDest = VarPointer(ctx);
                BufferView messageDest = VarPointer(ctx);
                bankDest.W16(0, MessageBankTVPrograms);
                messageDest.W16(0, TVPrograms.FieldSystem_GetTVProgramMessage(fieldSystem, which));
                break;
            }
            case TVBroadcastFinishProgram:
                TVPrograms.FieldSystem_SetTVProgramFinished(fieldSystem);
                break;
            case TVBroadcastPrintSegment:
            {
                StringTemplate template = Manager(ctx).StringTemplate!;
                ushort segmentID = VarValue(ctx);
                BufferView bankDest = VarPointer(ctx);
                BufferView messageDest = VarPointer(ctx);
                TVPrograms.FieldSystem_PrintTVSegment(fieldSystem, template, segmentID, bankDest, messageDest);
                break;
            }
            case TVBroadcastGetCommercial:
            {
                BufferView bankDest = VarPointer(ctx);
                BufferView messageDest = VarPointer(ctx);
                bankDest.W16(0, MessageBankTVCommercials);
                messageDest.W16(0, TVPrograms.FieldSystem_PickTVCommercial(fieldSystem));
                break;
            }
            case TVBroadcastFindSegment:
            {
                ushort own = VarValue(ctx);
                ushort played = VarValue(ctx);
                BufferView destination = VarPointer(ctx);
                destination.W16(0, TVPrograms.FieldSystem_FindTVSegment(fieldSystem, own != 0, played != 0));
                break;
            }
            case TVBroadcastFindAnySegment:
            {
                BufferView destination = VarPointer(ctx);
                destination.W16(0, TVPrograms.FieldSystem_FindAnyTVSegment(fieldSystem));
                break;
            }
        }
        return false;
    }

    // OFFSET 0x02048F74
    private static bool ScrCmd_SaveTVSegmentHiddenItem(ScriptContext ctx)
    {
        FieldTimeEvents.FieldSystem_SaveTVSegment_FoundHiddenItem(ctx.FieldSystem, VarValue(ctx));
        return false;
    }

    // OFFSET 0x020490F0
    private static bool ScrCmd_CheckTVInterviewEligible(ScriptContext ctx)
    {
        ushort interviewID = VarValue(ctx);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, TVInterview_IsEligible(ctx.FieldSystem, interviewID) ? 1 : 0);
        return false;
    }

    // OFFSET 0x020491F4
    private static bool TVInterview_IsEligible(FieldSystem fieldSystem, int interviewID)
    {
        if (!TVBroadcast.TVBroadcast_CanSaveSegment(FieldSystem_GetTVBroadcast(fieldSystem), TVProgramTypeInterview, interviewID))
        {
            return false;
        }
        uint eligibleFunc = RomData.Arm9U32((uint)(TVInterviewTable + (interviewID - 1) * TVInterviewEntrySize), 1)[0] & ~1u;
        if (eligibleFunc == 0)
        {
            return true;
        }
        switch (eligibleFunc)
        {
            case 0x02049348:
                return TVInterview_IsEligibleSection0B7C(fieldSystem);
            case 0x02049358:
                return TVInterview_IsEligiblePoketch(fieldSystem);
            case 0x02049368:
                return TVInterview_IsEligibleShowcasedPokemon(fieldSystem);
            case 0x02049378:
                return TVInterview_IsEligibleDressUpPhoto(fieldSystem);
            case 0x02049388:
                return TVInterview_IsEligibleSection0B7A(fieldSystem);
            case 0x02049398:
                return TVInterview_IsEligibleSection0B56(fieldSystem);
            case 0x020493A8:
                return TVInterview_IsEligibleSection0B88(fieldSystem);
        }
        throw Unported.Missing(eligibleFunc);
    }

    // OFFSET 0x02049348
    private static bool TVInterview_IsEligibleSection0B7C(FieldSystem fieldSystem)
    {
        return TVBroadcast.TVBroadcast_GetSection0B7C(FieldSystem_GetTVBroadcast(fieldSystem))[0] != 0;
    }

    // OFFSET 0x02049358
    private static bool TVInterview_IsEligiblePoketch(FieldSystem fieldSystem)
    {
        return FieldPoketch.Poketch_IsEnabled(FieldPoketch.SaveData_GetPoketch(fieldSystem.SaveData!)) != 0;
    }

    // OFFSET 0x02049368
    private static bool TVInterview_IsEligibleShowcasedPokemon(FieldSystem fieldSystem)
    {
        return TVBroadcast.TVBroadcast_GetShowcasedPokemon(FieldSystem_GetTVBroadcast(fieldSystem))[0] != 0;
    }

    // OFFSET 0x02049378
    private static bool TVInterview_IsEligibleDressUpPhoto(FieldSystem fieldSystem)
    {
        return ImageClips.ImageClips_PhotoHasData(ImageClips.SaveData_GetImageClips(fieldSystem.SaveData!), 0);
    }

    // OFFSET 0x02049388
    private static bool TVInterview_IsEligibleSection0B7A(FieldSystem fieldSystem)
    {
        return TVBroadcast.TVBroadcast_GetSection0B7A(FieldSystem_GetTVBroadcast(fieldSystem))[0] != 0;
    }

    // OFFSET 0x02049398
    private static bool TVInterview_IsEligibleSection0B56(FieldSystem fieldSystem)
    {
        return TVBroadcast.TVBroadcast_GetSection0B56(FieldSystem_GetTVBroadcast(fieldSystem))[0] != 0;
    }

    // OFFSET 0x020493A8
    private static bool TVInterview_IsEligibleSection0B88(FieldSystem fieldSystem)
    {
        return TVBroadcast.TVBroadcast_GetSection0B88(FieldSystem_GetTVBroadcast(fieldSystem))[0] != 0;
    }
}
