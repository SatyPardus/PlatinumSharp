using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public sealed partial class TestScript
{
    private const int MonDataMove1 = 0x36;
    private const int MonDataMove1PP = 0x3A;
    private const int MonDataIsEgg = 0x4C;
    private const int MonDataCurrentHP = 0xA3;
    private const int MonDataMaxHP = 0xA4;
    private const int NurseGraphicsID = 26;
    private const int CommandMenuFight = 1;
    private const int CommandMenuRun = 4;
    private const int MoveMenuIndex = 11;
    private const int FleeHPPercent = 25;
    private const int HealHPPercent = 50;
    private const int HealLeadHPPercent = 35;
    private const uint BattleTypeTrainerMask = 1;
    private static readonly int[] CommandMenuIndices = { 1, 2, 3, 4, 7, 8 };

    private bool battleTouching;
    private bool busyWasBattle;
    private int battleMenuFrames;
    private int lastBattleMenu = -1;

    private static BattleSystem? ActiveBattle()
    {
        return (sApplication.CurrentManager?.Data as FieldSystem)?.ProcessManager?.Child?.Data as BattleSystem;
    }

    private static bool MoveUsable(ushort move, int pp)
    {
        return move != 0 && pp > 0;
    }

    private static int MovePower(ushort move)
    {
        return move == 0 ? 0 : (int)MoveTable.MoveTable_LoadParam(move, MoveTable.ParamPower);
    }

    private int BattleKeys(BattleSystem battleSys)
    {
        BattleSubscreen? subscreen = battleSys.BtlSubscreen;
        int menu = subscreen?.ActiveMenuConfigIndex ?? -1;
        if (menu != lastBattleMenu)
        {
            lastBattleMenu = menu;
            battleMenuFrames = 0;
        }
        battleMenuFrames++;
        BattleMenuConfig? config = subscreen == null || subscreen.IsPanelSliding != 0 ? null : BattleSubscreens.BattleSubscreen_TestMenuConfig(menu);
        int wanted = -1;
        if (config?.TouchScreenRects != null && config.ButtonResults != null)
        {
            if (Array.IndexOf(CommandMenuIndices, menu) >= 0)
            {
                wanted = CommandMenuFight;
                ActionSelectData action = subscreen!.ActiveMenuData.ActionData;
                bool wild = (battleSys.BattleType & BattleTypeTrainerMask) == 0;
                BattleMon mon = battleSys.BattleCtx!.BattleMons[action.Battler & 3];
                bool hasMove = Enumerable.Range(0, 4).Any(i => MoveUsable(mon.Moves[i], mon.PpCur[i]) && MovePower(mon.Moves[i]) > 0);
                if (wild && (action.CurHp * 100 < action.MaxHp * FleeHPPercent || !hasMove))
                {
                    wanted = CommandMenuRun;
                }
            }
            else if (menu == MoveMenuIndex)
            {
                MoveSelectData moves = subscreen!.ActiveMenuData.MoveData;
                int bestSlot = -1;
                int bestPower = -1;
                for (int i = 0; i < 4; i++)
                {
                    if (!MoveUsable(moves.MoveIDs[i], moves.CurrPP[i]))
                    {
                        continue;
                    }
                    int power = MovePower(moves.MoveIDs[i]);
                    if (power > bestPower)
                    {
                        bestPower = power;
                        bestSlot = i;
                    }
                }
                wanted = bestSlot >= 0 ? bestSlot + 1 : -1;
            }
        }
        if (wanted >= 0 && battleMenuFrames > PulsePeriod)
        {
            int button = Array.IndexOf(config!.ButtonResults!, wanted);
            if (button >= 0 && button < config.TouchScreenRects!.Length)
            {
                TouchScreenHitTable rect = config.TouchScreenRects[button];
                Input.TouchX = (ushort)((rect.Left + rect.Right) / 2);
                Input.TouchY = (ushort)((rect.Top + rect.Bottom) / 2);
                Input.Touching = stepFrame % PulsePeriod < PulseHold;
                battleTouching = true;
                return 0;
            }
        }
        if (battleTouching)
        {
            Input.Touching = false;
            battleTouching = false;
        }
        return wanted >= 0 ? 0 : Pulse();
    }

    private static IEnumerable<Pokemon> PartyMons(FieldSystem field)
    {
        Party party = Party.SaveData_GetParty(field.SaveData!);
        for (int i = 0; i < Party.Party_GetCurrentCount(party); i++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, i);
            if (Pokemon.Pokemon_GetValue(mon, MonDataIsEgg, null) == 0)
            {
                yield return mon;
            }
        }
    }

    private static bool HasDamagingPP(Pokemon mon)
    {
        for (int i = 0; i < 4; i++)
        {
            ushort move = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataMove1 + i, null);
            if (MoveUsable(move, (int)Pokemon.Pokemon_GetValue(mon, MonDataMove1PP + i, null)) && MovePower(move) > 0)
            {
                return true;
            }
        }
        return false;
    }

    private static string? HealingReason(FieldSystem field)
    {
        List<Pokemon> mons = PartyMons(field).ToList();
        if (mons.Count == 0)
        {
            return null;
        }
        long hp = 0;
        long maxHP = 0;
        foreach (Pokemon mon in mons)
        {
            uint cur = Pokemon.Pokemon_GetValue(mon, MonDataCurrentHP, null);
            if (cur == 0)
            {
                return "a Pokémon fainted";
            }
            hp += cur;
            maxHP += Pokemon.Pokemon_GetValue(mon, MonDataMaxHP, null);
        }
        Pokemon lead = mons[0];
        if (Pokemon.Pokemon_GetValue(lead, MonDataCurrentHP, null) * 100 < Pokemon.Pokemon_GetValue(lead, MonDataMaxHP, null) * HealLeadHPPercent)
        {
            return "the lead Pokémon is low on HP";
        }
        if (hp * 100 < maxHP * HealHPPercent)
        {
            return "the party is low on HP";
        }
        if (!mons.Any(HasDamagingPP))
        {
            return "no attacking moves have PP left";
        }
        return null;
    }

    private void RememberNurses(FieldSystem field, int map)
    {
        foreach (MapObject mapObject in TestWorld.Objects(field))
        {
            if (MapObject.MapObject_GetGraphicsID(mapObject) == NurseGraphicsID)
            {
                int localID = MapObject.MapObject_GetLocalID(mapObject);
                if (memory.HealMaps.GetValueOrDefault(map, -1) != localID)
                {
                    memory.HealMaps[map] = localID;
                    memory.Save();
                    Log($"# remembered a Pokémon Center nurse on map {map} (object {localID})");
                }
            }
        }
    }

    private void RememberLink(int from, string action, int to)
    {
        if (from == to || to <= 0 || !(action.StartsWith("warp ") || action.StartsWith("edge ")))
        {
            return;
        }
        string key = $"{from}|{action}";
        if (memory.Links.GetValueOrDefault(key) != to)
        {
            memory.Links[key] = to;
            memory.Save();
        }
    }

    private string? HealAction(FieldSystem field, int map)
    {
        string? reason = HealingReason(field);
        if (reason == null)
        {
            return null;
        }
        if (memory.HealMaps.TryGetValue(map, out int nurse))
        {
            string talk = $"talkto {nurse}";
            if (TestWorld.ObjectByLocalID(field, nurse) != null && !GaveUpOn(talk, map))
            {
                Log($"# healing: {reason}, talking to the nurse");
                return talk;
            }
        }
        Dictionary<int, List<(string Action, int To)>> graph = new();
        foreach (var link in memory.Links)
        {
            int split = link.Key.IndexOf('|');
            int from = int.Parse(link.Key.Substring(0, split));
            if (!graph.TryGetValue(from, out List<(string, int)>? edges))
            {
                edges = new List<(string, int)>();
                graph[from] = edges;
            }
            edges.Add((link.Key.Substring(split + 1), link.Value));
        }
        Dictionary<int, string> firstHop = new() { [map] = "" };
        Queue<int> queue = new();
        queue.Enqueue(map);
        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            if (current != map && memory.HealMaps.ContainsKey(current))
            {
                string hop = firstHop[current];
                if (!GaveUpOn(hop, map))
                {
                    Log($"# healing: {reason}, heading for the Pokémon Center on map {current}");
                    return hop;
                }
            }
            if (!graph.TryGetValue(current, out List<(string Action, int To)>? edges))
            {
                continue;
            }
            foreach (var edge in edges)
            {
                if (!firstHop.ContainsKey(edge.To))
                {
                    firstHop[edge.To] = current == map ? edge.Action : firstHop[current];
                    queue.Enqueue(edge.To);
                }
            }
        }
        return null;
    }

    private static bool CanTalkFrom(FieldSystem field, int fromX, int fromZ, int objectX, int objectZ)
    {
        int dx = objectX - fromX;
        int dz = objectZ - fromZ;
        if (Math.Abs(dx) + Math.Abs(dz) == 1)
        {
            return true;
        }
        if ((Math.Abs(dx) == 2 && dz == 0) || (dx == 0 && Math.Abs(dz) == 2))
        {
            return !TestWorld.Walkable(field, fromX + dx / 2, fromZ + dz / 2);
        }
        return false;
    }
}
