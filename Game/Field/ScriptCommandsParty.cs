namespace PlatinumSharp.Game;

public static partial class ScriptCommands
{
    private const int PartyParamContestStatBase = 0x13;
    private const int PartyParamIsEgg = 0x4C;
    private const int PartyParamCurrentHP = 0xA3;
    private const int PartyParamType1 = 0xB1;
    private const int PartyParamType2 = 0xB2;

    private const int MonDataPokerus = 0x9A;

    private static void RegisterParty(Dictionary<uint, ScrCmdFunc> ported)
    {
        ported[0x0204D74C] = ScrCmd_GetPartyMonContestStat;
        ported[0x02044FA4] = ScrCmd_PlayPokecenterHealingAnimation;
        ported[0x0204DA20] = ScrCmd_CheckPartyPokerus;
        ported[0x0204D7AC] = ScrCmd_GetFirstNonEggInParty;
        ported[0x0204D7D4] = ScrCmd_GetPartyMonType;
        ported[0x0204D840] = ScrCmd_GetPartyCount;
        ported[0x0204D86C] = ScrCmd_CountPartyNonEggs;
        ported[0x0204D8C4] = ScrCmd_CountAliveMonsExcept;
        ported[0x0204D94C] = ScrCmd_CountAliveMonsAndBoxMons;
        ported[0x0204D9C8] = ScrCmd_CountPartyEggs;
    }

    private static Pokemon PartyMon(ScriptContext ctx, int slot)
    {
        return Party.Party_GetPokemonBySlotIndex(Party.SaveData_GetParty(ctx.FieldSystem.SaveData!), slot);
    }

    private static int PartyCount(ScriptContext ctx)
    {
        return Party.Party_GetCurrentCount(Party.SaveData_GetParty(ctx.FieldSystem.SaveData!));
    }

    // OFFSET 0x0204D74C
    private static bool ScrCmd_GetPartyMonContestStat(ScriptContext ctx)
    {
        ushort slot = VarValue(ctx);
        ushort stat = VarValue(ctx);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, (int)Pokemon.Pokemon_GetValue(PartyMon(ctx, slot), PartyParamContestStatBase + stat, null));
        return false;
    }

    // OFFSET 0x0204D7AC
    private static bool ScrCmd_GetFirstNonEggInParty(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        destination.W16(0, FieldParty.SaveData_GetFirstNonEggInParty(ctx.FieldSystem.SaveData!));
        return false;
    }

    // OFFSET 0x0204D7D4
    private static bool ScrCmd_GetPartyMonType(ScriptContext ctx)
    {
        BufferView type1 = VarPointer(ctx);
        BufferView type2 = VarPointer(ctx);
        ushort slot = VarValue(ctx);
        Pokemon mon = PartyMon(ctx, slot);
        type1.W16(0, (int)Pokemon.Pokemon_GetValue(mon, PartyParamType1, null));
        type2.W16(0, (int)Pokemon.Pokemon_GetValue(mon, PartyParamType2, null));
        return false;
    }

    // OFFSET 0x0204D840
    private static bool ScrCmd_GetPartyCount(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        destination.W16(0, PartyCount(ctx));
        return false;
    }

    // OFFSET 0x0204D86C
    private static bool ScrCmd_CountPartyNonEggs(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        int count = PartyCount(ctx);
        int nonEggs = 0;
        for (int slot = 0; slot < count; slot++)
        {
            if (Pokemon.Pokemon_GetValue(PartyMon(ctx, slot), PartyParamIsEgg, null) == 0)
            {
                nonEggs++;
            }
        }
        destination.W16(0, nonEggs);
        return false;
    }

    // OFFSET 0x0204D8C4
    private static bool ScrCmd_CountAliveMonsExcept(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        ushort excluded = VarValue(ctx);
        int count = PartyCount(ctx);
        int alive = 0;
        for (int slot = 0; slot < count; slot++)
        {
            if (slot == excluded)
            {
                continue;
            }
            Pokemon mon = PartyMon(ctx, slot);
            if (Pokemon.Pokemon_GetValue(mon, PartyParamIsEgg, null) == 0 && Pokemon.Pokemon_GetValue(mon, PartyParamCurrentHP, null) != 0)
            {
                alive++;
            }
        }
        destination.W16(0, alive);
        return false;
    }

    // OFFSET 0x0204D94C
    private static bool ScrCmd_CountAliveMonsAndBoxMons(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        PCBoxes boxes = (PCBoxes)SaveDataTable.SaveData_GetPCBoxes(ctx.FieldSystem.SaveData!);
        int count = PartyCount(ctx);
        int alive = 0;
        for (int slot = 0; slot < count; slot++)
        {
            Pokemon mon = PartyMon(ctx, slot);
            if (Pokemon.Pokemon_GetValue(mon, PartyParamIsEgg, null) == 0 && Pokemon.Pokemon_GetValue(mon, PartyParamCurrentHP, null) != 0)
            {
                alive++;
            }
        }
        destination.W16(0, alive + PCBoxes.PCBoxes_CountAllNonEggMons(boxes));
        return false;
    }

    // OFFSET 0x0204D9C8
    private static bool ScrCmd_CountPartyEggs(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        int count = PartyCount(ctx);
        int eggs = 0;
        for (int slot = 0; slot < count; slot++)
        {
            if (Pokemon.Pokemon_GetValue(PartyMon(ctx, slot), PartyParamIsEgg, null) != 0)
            {
                eggs++;
            }
        }
        destination.W16(0, eggs);
        return false;
    }

    // OFFSET 0x02044FA4
    private static bool ScrCmd_PlayPokecenterHealingAnimation(ScriptContext ctx)
    {
        byte pokeballCount = (byte)VarValue(ctx);
        HealingMachineAnimations.FieldSystem_PlayHealingAnimation_Pokecenter(ctx.FieldSystem, pokeballCount);
        return true;
    }

    // OFFSET 0x0204DA20
    private static bool ScrCmd_CheckPartyPokerus(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        BufferView destination = VarPointer(ctx);
        ushort partyCount = (ushort)Party.Party_GetCurrentCount(Party.SaveData_GetParty(fieldSystem.SaveData!));
        destination.W16(0, 0);
        for (ushort slot = 0; slot < partyCount; slot++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(Party.SaveData_GetParty(fieldSystem.SaveData!), slot);
            if (Pokemon.Pokemon_GetValue(mon, MonDataPokerus, null) != 0)
            {
                destination.W16(0, 1);
                break;
            }
        }
        return false;
    }
}
