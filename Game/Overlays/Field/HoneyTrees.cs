using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed class HoneyTreeShake
{
    public byte AnimIndex;
    public int IsShaking;
}

public sealed class HoneyTreeShakeList
{
    public const int TreeCount = 21;

    public readonly HoneyTreeShake[] Trees = new HoneyTreeShake[TreeCount];

    public HoneyTreeShakeList()
    {
        for (int i = 0; i < TreeCount; i++)
        {
            Trees[i] = new HoneyTreeShake();
        }
    }
}

public static class HoneyTrees
{
    public const int SlatherStatusBare = 1;
    public const int SlatherStatusSlathered = 2;
    public const int SlatherStatusEncounter = 3;

    private const int OverlayID = 5;
    private const uint EncounterTableMembersAddress = 0x021FF5A8;
    private const int EncounterTableCount = 3;
    private const uint TreeMapHeadersAddress = 0x021FF5B4;
    private const int TreeCount = HoneyTreeShakeList.TreeCount;
    private const int NoTree = TreeCount;

    private const int HeapIDField = 4;
    private const int NarcEncounterData = 0x6A;
    private const int MapPropModelHoneyTree = 0x1A;
    private const int ScriptHoneyTree = 0x7D8;
    private const int LocationMapHeaderID = 0x00;
    private const int DirNorth = 0;

    private const int SlatherMinutes = 0x5A0;
    private const int EncounterWindowMinutes = 0x438;
    private const uint RandDivisor = 0x290;
    private const int RandRange = 100;
    private const int SameTreeKeepChance = 90;

    private static uint[]? sTreeMapHeaders;
    private static uint[]? sEncounterTableMembers;

    private static uint[] TreeMapHeaders => sTreeMapHeaders ??= RomData.OverlayU32(OverlayID, TreeMapHeadersAddress, TreeCount);

    private static uint[] EncounterTableMembers => sEncounterTableMembers ??= RomData.OverlayU32(OverlayID, EncounterTableMembersAddress, EncounterTableCount);

    private static ushort RandomPercent()
    {
        ushort rand = (ushort)(MathUtil.LCRNG_Next() / RandDivisor);
        if (rand >= RandRange)
        {
            ErrorHandling.GF_AssertFail();
        }
        return rand;
    }

    private static HoneyTree CurrentTree(FieldSystem fieldSystem, byte treeId)
    {
        PlayerHoneyTreeStates states = SpecialEncounter.SpecialEncounter_GetPlayerHoneyTreeStates(SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!));
        return SpecialEncounter.SpecialEncounter_GetHoneyTree(treeId, states);
    }

    // OFFSET 0x021EFB0C
    public static HoneyTreeShakeList HoneyTree_ShakeDataInit()
    {
        HoneyTreeShakeList list = new();
        for (byte i = 0; i < TreeCount; i++)
        {
            list.Trees[i].AnimIndex = 0;
            list.Trees[i].IsShaking = 0;
        }
        return list;
    }

    // OFFSET 0x021EFB30
    public static void HoneyTree_FreeShakeData(ref HoneyTreeShakeList? list)
    {
        list = null;
    }

    // OFFSET 0x021EFB40
    public static int HoneyTree_TryInteract(FieldSystem fieldSystem, out int eventId)
    {
        eventId = ScriptHoneyTree;
        int x = PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar!);
        int z = PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar!);
        if (PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar!) == DirNorth)
        {
            TerrainCollisionManager.TerrainCollisionHitbox_Init(x, z, 0, -1, 1, 1, out TerrainCollisionHitbox hitbox);
            object? mapProp = null;
            return TerrainCollisionManager.FieldSystem_FindCollidingLoadedMapPropByModelID(fieldSystem, MapPropModelHoneyTree, ref hitbox, ref mapProp) ? 1 : 0;
        }
        return 0;
    }

    // OFFSET 0x021EFB94
    public static ushort HoneyTree_GetTreeSlatherStatus(FieldSystem fieldSystem)
    {
        byte treeId = GetTreeIDFromMapHeaderID((int)fieldSystem.Location!.U32(LocationMapHeaderID));
        if (treeId == NoTree)
        {
            ErrorHandling.GF_AssertFail();
        }
        HoneyTree tree = CurrentTree(fieldSystem, treeId);
        if (IsEncounterReady(tree.Timer))
        {
            return SlatherStatusEncounter;
        }
        if (tree.Timer != 0)
        {
            return SlatherStatusSlathered;
        }
        return SlatherStatusBare;
    }

    // OFFSET 0x021EFBDC
    public static void HoneyTree_SlatherTree(FieldSystem fieldSystem)
    {
        byte treeId = GetTreeIDFromMapHeaderID((int)fieldSystem.Location!.U32(LocationMapHeaderID));
        if (treeId == NoTree)
        {
            ErrorHandling.GF_AssertFail();
        }
        PlayerHoneyTreeStates states = SpecialEncounter.SpecialEncounter_GetPlayerHoneyTreeStates(SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!));
        HoneyTree tree = SpecialEncounter.SpecialEncounter_GetHoneyTree(treeId, states);
        tree.Timer = SlatherMinutes;
        bool isMunchlaxTree = IsMunchlaxTree(TrainerInfo.TrainerInfo_ID(PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!)), treeId);
        if (treeId == SpecialEncounter.SpecialEncounter_GetLastSlatheredTreeId(states))
        {
            if (RandomPercent() < SameTreeKeepChance)
            {
                GetTreeEncounterSlot(out tree.EncounterTableIndex);
                tree.ShakeCount = (byte)GetShakesFromGroup(tree.Rarity);
                return;
            }
        }
        GetTreeEncounterGroup(isMunchlaxTree, out tree.Rarity);
        if (tree.Rarity != 0)
        {
            GetTreeEncounterSlot(out tree.EncounterTableIndex);
            tree.EncounterGroup = (byte)GetEncounterTableFromGroup(tree.Rarity);
        }
        else
        {
            tree.EncounterGroup = 0;
            tree.EncounterTableIndex = 0;
            tree.Timer = 0;
        }
        tree.ShakeCount = (byte)GetShakesFromGroup(tree.Rarity);
        SpecialEncounter.SpecialEncounter_SetLastSlatheredTreeId(treeId, states);
    }

    // OFFSET 0x021EFC90
    public static void HoneyTree_StopShaking(FieldSystem fieldSystem)
    {
        byte treeId = GetTreeIDFromMapHeaderID((int)fieldSystem.Location!.U32(LocationMapHeaderID));
        if (treeId == NoTree)
        {
            ErrorHandling.GF_AssertFail();
        }
        HoneyTreeShake shake = fieldSystem.HoneyTreeShakeList!.Trees[treeId];
        if (shake.IsShaking != 0)
        {
            byte quadrant = LandDataManager.LandDataManager_GetTrackedTargetLoadedMapsQuadrant(fieldSystem.LandDataManager!);
            LandDataManager.LandDataManager_GetLoadedMapPropManager(quadrant, fieldSystem.LandDataManager!, out MapPropManager? mapPropManager);
            MapProp? mapProp = MapProps.MapPropManager_FindLoadedPropByModelID(mapPropManager!, MapPropModelHoneyTree);
            if (mapProp != null)
            {
                G3dRenderObj renderObj = MapProps.MapProp_GetRenderObj(mapProp);
                MapPropAnimations.MapPropAnimationManager_RemoveAnimationFromRenderObj(fieldSystem.MapPropAnimationManager!, renderObj, MapPropModelHoneyTree, shake.AnimIndex);
            }
            shake.IsShaking = 0;
        }
    }

    // OFFSET 0x021EFCF8
    private static void GetTreeEncounterGroup(bool isMunchlaxTree, out byte group)
    {
        ushort rand = RandomPercent();
        if (isMunchlaxTree)
        {
            if (rand < 1)
            {
                group = 3;
            }
            else if (rand < 10)
            {
                group = 0;
            }
            else if (rand < 30)
            {
                group = 1;
            }
            else
            {
                group = 2;
            }
        }
        else if (rand < 10)
        {
            group = 0;
        }
        else if (rand < 30)
        {
            group = 2;
        }
        else
        {
            group = 1;
        }
    }

    // OFFSET 0x021EFD58
    private static void GetTreeEncounterSlot(out byte slot)
    {
        ushort rand = RandomPercent();
        if (rand < 5)
        {
            slot = 5;
        }
        else if (rand < 10)
        {
            slot = 4;
        }
        else if (rand < 20)
        {
            slot = 3;
        }
        else if (rand < 40)
        {
            slot = 2;
        }
        else if (rand < 60)
        {
            slot = 1;
        }
        else
        {
            slot = 0;
        }
    }

    // OFFSET 0x021EFDAC
    private static int GetEncounterTableFromGroup(byte group)
    {
        if (group == 3)
        {
            return 2;
        }
        if (group == 2)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x021EFDC0
    private static int GetShakesFromGroup(byte group)
    {
        ushort rand = RandomPercent();
        int twoShakes;
        int oneShake;
        int noShake;
        if (group == 3)
        {
            twoShakes = 5;
            oneShake = 6;
            noShake = 7;
        }
        else if (group == 2)
        {
            twoShakes = 75;
            oneShake = 95;
            noShake = 96;
        }
        else if (group == 1)
        {
            twoShakes = 19;
            oneShake = 79;
            noShake = 99;
        }
        else
        {
            twoShakes = 1;
            oneShake = 19;
            noShake = 99;
        }
        if (rand < twoShakes)
        {
            return 2;
        }
        if (rand < oneShake)
        {
            return 1;
        }
        if (rand < noShake)
        {
            return 0;
        }
        return 3;
    }

    // OFFSET 0x021EFE58
    private static bool GetShakingValue(int numShakes, ref byte value)
    {
        bool result = true;
        if (numShakes == 3)
        {
            value = 2;
        }
        else if (numShakes == 2)
        {
            value = 1;
        }
        else if (numShakes == 1)
        {
            value = 0;
        }
        else
        {
            result = false;
        }
        return result;
    }

    // OFFSET 0x021EFE7C
    private static void DoTreeShakingAnimation(FieldSystem fieldSystem, MapPropManager mapPropManager, int mapMatrixIndex)
    {
        byte animIndex = 0;
        byte treeId = GetTreeIDFromMapHeaderID(MapMatrix.MapMatrix_GetMapHeaderIDAtIndex(fieldSystem.MapMatrix!, mapMatrixIndex));
        if (treeId == NoTree)
        {
            return;
        }
        HoneyTree tree = CurrentTree(fieldSystem, treeId);
        if (!IsEncounterReady(tree.Timer))
        {
            return;
        }
        bool isShaking = GetShakingValue(tree.ShakeCount, ref animIndex);
        if (!isShaking)
        {
            return;
        }
        MapProp? mapProp = MapProps.MapPropManager_FindLoadedPropByModelID(mapPropManager, MapPropModelHoneyTree);
        if (mapProp == null)
        {
            return;
        }
        G3dRenderObj renderObj = MapProps.MapProp_GetRenderObj(mapProp);
        HoneyTreeShake shake = fieldSystem.HoneyTreeShakeList!.Trees[treeId];
        MapPropAnimations.MapPropAnimationManager_RemoveAnimationFromRenderObj(fieldSystem.MapPropAnimationManager!, renderObj, MapPropModelHoneyTree, shake.AnimIndex);
        shake.AnimIndex = animIndex;
        shake.IsShaking = 1;
        MapPropAnimations.MapPropAnimationManager_AddAnimationToRenderObj(MapPropModelHoneyTree, animIndex, true, renderObj, fieldSystem.MapPropAnimationManager);
    }

    // OFFSET 0x021EFF10
    private static byte GetTreeIDFromMapHeaderID(int mapHeaderID)
    {
        uint[] mapHeaders = TreeMapHeaders;
        for (byte i = 0; i < TreeCount; i++)
        {
            if (mapHeaderID == (int)mapHeaders[i])
            {
                return i;
            }
        }
        return NoTree;
    }

    // OFFSET 0x021EFF34
    private static bool IsEncounterReady(int minutesLeft)
    {
        if (minutesLeft > 0 && minutesLeft <= EncounterWindowMinutes)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x021EFF4C
    private static bool IsMunchlaxTree(uint trainerId, byte treeId)
    {
        byte[] trees = new byte[4];
        trees[0] = (byte)((byte)(trainerId >> 24) % TreeCount);
        trees[1] = (byte)((byte)(trainerId >> 16) % TreeCount);
        trees[2] = (byte)((byte)(trainerId >> 8) % TreeCount);
        trees[3] = (byte)((byte)trainerId % TreeCount);
        for (byte i = 1; i < 4; i++)
        {
            for (byte j = 0; j < i; j++)
            {
                if (trees[j] == trees[i])
                {
                    trees[i]++;
                    if (trees[i] >= TreeCount)
                    {
                        trees[i] = 0;
                    }
                }
            }
        }
        for (byte i = 0; i < 4; i++)
        {
            if (treeId == trees[i])
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x021EFFE4
    public static int HoneyTree_GetSpecies(FieldSystem fieldSystem)
    {
        byte treeId = GetTreeIDFromMapHeaderID((int)fieldSystem.Location!.U32(LocationMapHeaderID));
        if (treeId == NoTree)
        {
            ErrorHandling.GF_AssertFail();
        }
        HoneyTree tree = CurrentTree(fieldSystem, treeId);
        byte[] table = NARC.NARC_AllocAtEndAndReadWholeMemberByIndexPair(NarcEncounterData, (int)EncounterTableMembers[tree.EncounterGroup], HeapIDField);
        int species = (int)Bin.U32(table, tree.EncounterTableIndex * 4);
        Heap.Heap_Free(table);
        return species;
    }

    // OFFSET 0x021F0030
    public static void HoneyTree_OnMapLoaded(object? data, int landDataID, object? mapPropManager)
    {
        if (landDataID >= 0)
        {
            DoTreeShakingAnimation((FieldSystem)data!, (MapPropManager)mapPropManager!, landDataID);
        }
    }

    // OFFSET 0x021F0040
    public static void HoneyTree_Unslather(FieldSystem fieldSystem)
    {
        byte treeId = GetTreeIDFromMapHeaderID((int)fieldSystem.Location!.U32(LocationMapHeaderID));
        if (treeId == NoTree)
        {
            ErrorHandling.GF_AssertFail();
        }
        fieldSystem.HoneyTreeShakeList!.Trees[treeId].IsShaking = 0;
        CurrentTree(fieldSystem, treeId).Timer = 0;
    }
}
