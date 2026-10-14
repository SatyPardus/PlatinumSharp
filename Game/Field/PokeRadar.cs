using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class RadarGrassPatch
{
    public const int Size = 0x28;

    public int X;
    public int Z;
    public int ShakeType;
    public int Active;
    public int ContinueChain;
    public int Shiny;
    public OverworldAnimManager? AnimManager;
    public VecFx32 Position;
}

public sealed class RadarRefreshTaskData
{
    public int State;
}

public sealed class RadarChain
{
    public const int Size = 0xD4;
    public const int PatchCount = 4;

    public int ShakeType;
    public int Count;
    public int Species;
    public int Level;
    public int Active;
    public int AwaitingFirstEncounter;
    public int PatchEntered;
    public readonly RadarGrassPatch[] Patches = new RadarGrassPatch[PatchCount];
    public GFXTestBox GrassPatchVolume;
    public byte RecordSlot;

    private const uint PatchTablesAddress = 0x020EF8C8;
    private const int ContinueOddsAfterWinOffset = 0;
    private const int RingTileCountsOffset = 4;
    private const int ContinueOddsAfterCaptureOffset = 8;
    private const int PatchTablesSize = 12;
    private const int BattleResultWin = 1;
    private const int BattleResultCaptured = 4;
    private const int PatchVolumeWidth = 0x10000;
    private const int PatchVolumeHeight = 0x8000;
    private const int MaxChainCount = 999;
    private const int RecordCount = 3;
    private const int RecordSize = 4;
    private const int RecordSpeciesOffset = 0;
    private const int RecordChainOffset = 2;
    private const int PercentDivisor = 0x290;
    private const int MaxRadarCharge = 50;
    private const int ScriptRadarNotCharged = 0x230A;
    private const int ScriptRadarNoPatches = 0x230B;
    private const int ScriptMemberParameter0 = 0x29;
    private const int BgmPokeRadar = 0x47E;
    private const int ItemPokeRadar = 0x1AF;
    private const int HeapIDField = 4;
    private const int ShinyOddsBase = 0x2008;
    private const int ShinyOddsStep = 200;
    private const int ShakeEffectGrass = 0;
    private const int ShakeEffectRareGrass = 1;
    private const int ShakeEffectShiny = 2;
    private const int MapBlockTiles = 32;

    private static byte[]? sPatchTables;

    private static byte[] PatchTables
    {
        get
        {
            sPatchTables ??= RomData.Arm9(PatchTablesAddress, PatchTablesSize);
            return sPatchTables;
        }
    }

    private static ushort RandMod(ushort max)
    {
        if (max == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (max <= 1)
        {
            return 0;
        }
        ushort divisor = (ushort)(0xFFFF / max + 1);
        ushort result = (ushort)(MathUtil.LCRNG_Next() / divisor);
        if (result >= max)
        {
            ErrorHandling.GF_AssertFail();
        }
        return result;
    }

    private static ushort RandPercent()
    {
        ushort result = (ushort)(MathUtil.LCRNG_Next() / PercentDivisor);
        if (result >= 100)
        {
            ErrorHandling.GF_AssertFail();
        }
        return result;
    }

    // OFFSET 0x0206940C
    public static RadarChain RadarChain_Init(int heapID)
    {
        RadarChain chain = new();
        for (int i = 0; i < PatchCount; i++)
        {
            chain.Patches[i] = new RadarGrassPatch();
        }
        GfxBoxTest.GFXBoxTest_MakeBox(PatchVolumeWidth, PatchVolumeHeight, PatchVolumeWidth, out chain.GrassPatchVolume);
        return chain;
    }

    // OFFSET 0x0206942C
    public static void RadarChain_Free(RadarChain chain)
    {
    }

    // OFFSET 0x02069434
    public static void RadarChain_Clear(RadarChain chain)
    {
        chain.Count = 0;
        chain.ShakeType = 0;
        chain.Species = 0;
        chain.Level = 0;
        chain.Active = 0;
        chain.RecordSlot = 0;
        chain.AwaitingFirstEncounter = 1;
        chain.PatchEntered = 0;
        for (int i = 0; i < PatchCount; i++)
        {
            RadarGrassPatch patch = chain.Patches[i];
            patch.X = 0;
            patch.Z = 0;
            patch.ShakeType = 0;
            patch.Active = 0;
            patch.ContinueChain = 0;
            patch.Shiny = 0;
            patch.AnimManager = null;
            patch.Position = default;
        }
        for (byte i = 0; i < PatchCount; i++)
        {
            chain.Patches[i].Active = 0;
        }
    }

    // OFFSET 0x02069474
    public static int PokeRadar_SpawnPatches(FieldSystem fieldSystem, int playerX, int playerZ, RadarChain chain)
    {
        byte[] ringTileCounts = new byte[PatchCount];
        Array.Copy(PatchTables, RingTileCountsOffset, ringTileCounts, 0, PatchCount);
        VecFx32 playerPosition = (VecFx32)PlayerAvatar.PlayerAvatar_GetPos(fieldSystem.PlayerAvatar)!;
        byte spawned = 0;
        for (byte ring = 0; ring < PatchCount; ring++)
        {
            byte tile = (byte)RandMod(ringTileCounts[ring]);
            byte sideLength = (byte)(9 - ring * 2);
            byte side = (byte)(tile / sideLength);
            int column;
            int row;
            if (side == 0)
            {
                column = ring + tile % sideLength;
                row = ring;
            }
            else if (side == 1)
            {
                row = ring + sideLength - 1;
                column = ring + tile % sideLength;
            }
            else
            {
                if (tile < sideLength * 2)
                {
                    ErrorHandling.GF_AssertFail();
                }
                byte remainder = (byte)(tile - sideLength * 2);
                row = ring + (remainder >> 1) + 1;
                if (remainder % 2 == 0)
                {
                    column = ring;
                }
                else
                {
                    column = ring + sideLength - 1;
                }
            }
            if (PokeRadar_CheckTileIsGrass(fieldSystem, playerPosition.Y, playerX, playerZ, (byte)column, (byte)row, chain.Patches[ring]))
            {
                spawned++;
            }
        }
        if (spawned == 0)
        {
            RadarChain_Clear(chain);
            int bgmID = FieldBgm.FieldBGM_GetEffective(fieldSystem, (int)fieldSystem.Location!.U32(0));
            FieldBgm.FieldBGM_TryFadeOut(fieldSystem, bgmID, 1);
        }
        else
        {
            chain.Active = 1;
        }
        return chain.Active;
    }

    // OFFSET 0x020695C8
    public static void PokeRadar_SetupGrassPatches(FieldSystem fieldSystem, int battleResult, RadarChain chain)
    {
        for (byte ring = 0; ring < PatchCount; ring++)
        {
            RadarGrassPatch patch = chain.Patches[ring];
            if (patch.Active == 0)
            {
                continue;
            }
            patch.ContinueChain = PokeRadar_CheckPatchContinueChain(ring, battleResult, chain) ? 1 : 0;
            if (patch.ContinueChain == 0)
            {
                patch.ShakeType = RandPercent() < 50 ? 0 : 1;
                patch.Shiny = 0;
            }
            else
            {
                patch.ShakeType = chain.ShakeType;
                patch.Shiny = PokeRadar_CheckPatchShiny(((RadarChain)fieldSystem.RadarChain!).Count) ? 1 : 0;
            }
        }
    }

    // OFFSET 0x02069638
    public static void PokeRadar_CreateShakingPatches(FieldSystem fieldSystem, RadarChain chain)
    {
        for (byte i = 0; i < PatchCount; i++)
        {
            RadarGrassPatch patch = chain.Patches[i];
            if (patch.Active != 0)
            {
                if (patch.Shiny != 0)
                {
                    patch.AnimManager = GrassEffect.GrassShakeEffect_New(fieldSystem, patch.X, patch.Z, ShakeEffectShiny);
                }
                else if (patch.ShakeType == 0)
                {
                    patch.AnimManager = GrassEffect.GrassShakeEffect_New(fieldSystem, patch.X, patch.Z, ShakeEffectGrass);
                }
                else
                {
                    patch.AnimManager = GrassEffect.GrassShakeEffect_New(fieldSystem, patch.X, patch.Z, ShakeEffectRareGrass);
                }
            }
            else
            {
                patch.AnimManager = null;
            }
        }
    }

    // OFFSET 0x02069690
    public static bool PokeRadar_FinishPatchShakes(RadarChain chain)
    {
        byte finished = 0;
        for (byte i = 0; i < PatchCount; i++)
        {
            RadarGrassPatch patch = chain.Patches[i];
            if (patch.AnimManager != null)
            {
                if (GrassEffect.GrassShakeEffect_IsFinished(patch.AnimManager) != 0)
                {
                    OverworldAnimManagers.OverworldAnimManager_Finish(patch.AnimManager);
                    patch.AnimManager = null;
                    finished++;
                }
            }
            else
            {
                finished++;
            }
        }
        return finished >= PatchCount;
    }

    // OFFSET 0x020696DC
    public static bool PokeRadar_ShouldDoRadarEncounter(int playerX, int playerZ, FieldSystem fieldSystem, RadarChain chain, ref int shakeType, out int preserveChain, out int isShiny)
    {
        preserveChain = 0;
        isShiny = 0;
        if (!PokeRadar_PlayerStandingInPatch(chain, playerX, playerZ, out byte patchIndex))
        {
            return false;
        }
        chain.PatchEntered = 1;
        RadarGrassPatch patch = chain.Patches[patchIndex];
        int continueChain = patch.ContinueChain;
        int patchShakeType = patch.ShakeType;
        if (chain.AwaitingFirstEncounter == 0)
        {
            if (continueChain != 0)
            {
                PokeRadar_IncrementWithCap(ref chain.Count);
                shakeType = patchShakeType;
                preserveChain = 1;
                PokeRadar_TryReplaceLowestChainRecord(fieldSystem, chain);
                isShiny = chain.Patches[patchIndex].Shiny;
                return true;
            }
            shakeType = patchShakeType;
        }
        else
        {
            shakeType = patchShakeType;
            chain.AwaitingFirstEncounter = 0;
            chain.RecordSlot = PokeRadar_GetLowestChainRecordSlot(fieldSystem);
        }
        chain.ShakeType = shakeType;
        return true;
    }

    // OFFSET 0x02069774
    public static void RadarChain_SetMon(RadarChain chain, int species, int level)
    {
        if (species == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        chain.Species = species;
        chain.Level = level;
    }

    // OFFSET 0x0206978C
    public static void RadarChain_GetMon(RadarChain chain, out int species, out int level)
    {
        species = chain.Species;
        level = chain.Level;
    }

    // OFFSET 0x02069798
    public static int RadarChain_GetPatchEntered(RadarChain chain)
    {
        return chain.PatchEntered;
    }

    // OFFSET 0x0206979C
    public static void PokeRadar_ClearIfAllOutOfView(FieldSystem fieldSystem)
    {
        RadarChain chain = (RadarChain)fieldSystem.RadarChain!;
        if (chain.Active == 0)
        {
            return;
        }
        if (fieldSystem.Task != null)
        {
            return;
        }
        for (int i = 0; i < PatchCount; i++)
        {
            RadarGrassPatch patch = chain.Patches[i];
            uint inView = GfxBoxTest.GFXBoxTest_IsBoxAtPositionInView(patch.Position, chain.GrassPatchVolume);
            if (patch.Active != 0 && inView == 0)
            {
                patch.Active = 0;
            }
        }
        int inactive = 0;
        for (int i = 0; i < PatchCount; i++)
        {
            if (chain.Patches[i].Active == 0)
            {
                inactive++;
            }
        }
        if (inactive == PatchCount)
        {
            RadarChain_Clear(chain);
            int bgmID = FieldBgm.FieldBGM_GetEffective(fieldSystem, (int)fieldSystem.Location!.U32(0));
            FieldBgm.FieldBGM_TryFadeOut(fieldSystem, bgmID, 1);
        }
    }

    // OFFSET 0x0206981C
    public static int RadarChain_GetActive(RadarChain chain)
    {
        return chain.Active;
    }

    // OFFSET 0x02069820
    private static bool PokeRadar_CheckTileIsGrass(FieldSystem fieldSystem, int playerHeight, int playerX, int playerZ, byte column, byte row, RadarGrassPatch patch)
    {
        int x = playerX - 4 + column;
        int z = playerZ - 4 + row;
        patch.X = x;
        patch.Z = z;
        int tileBehavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x, z);
        if (TileBehavior.TileBehavior_IsTallGrass(tileBehavior) != 0)
        {
            patch.Position.X = x << 16;
            patch.Position.Z = z << 16;
            int height = TerrainCollisionManager.TerrainCollisionManager_GetHeight(fieldSystem, 0, patch.Position.X, patch.Position.Z, out _);
            patch.Position.Y = height;
            if (playerHeight != height)
            {
                patch.Active = 0;
                return false;
            }
            int mapHeaderID = MapMatrix.MapMatrix_GetMapHeaderIDAtCoords(fieldSystem.MapMatrix!, x / MapBlockTiles, z / MapBlockTiles);
            if ((int)fieldSystem.Location!.U32(0) != mapHeaderID)
            {
                patch.Active = 0;
                return false;
            }
            patch.Active = 1;
            return true;
        }
        patch.Active = 0;
        return false;
    }

    // OFFSET 0x020698AC
    private static bool PokeRadar_PlayerStandingInPatch(RadarChain chain, int x, int z, out byte patchIndex)
    {
        for (byte i = 0; i < PatchCount; i++)
        {
            RadarGrassPatch patch = chain.Patches[i];
            if (patch.Active != 0 && x == patch.X && z == patch.Z)
            {
                patchIndex = i;
                return true;
            }
        }
        patchIndex = 0;
        return false;
    }

    // OFFSET 0x020698E4
    private static void PokeRadar_TryReplaceLowestChainRecord(FieldSystem fieldSystem, RadarChain chain)
    {
        byte[] records = SpecialEncounter.SpecialEncounter_GetRadarChainRecords(SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!));
        if (Bin.U16(records, chain.RecordSlot * RecordSize + RecordChainOffset) >= chain.Count)
        {
            return;
        }
        Bin.W16(records, chain.RecordSlot * RecordSize + RecordChainOffset, (ushort)chain.Count);
        Bin.W16(records, chain.RecordSlot * RecordSize + RecordSpeciesOffset, (ushort)chain.Species);
        ov6_02243160(records);
        int count = chain.Count;
        if (Bin.U16(records, chain.RecordSlot * RecordSize + RecordChainOffset) > count)
        {
            return;
        }
        for (int i = 0; i < RecordCount; i++)
        {
            int slot = RecordCount - 1 - i;
            if (count == Bin.U16(records, slot * RecordSize + RecordChainOffset))
            {
                chain.RecordSlot = (byte)slot;
                return;
            }
        }
        ErrorHandling.GF_AssertFail();
    }

    // OFFSET 0x0206994C
    private static byte PokeRadar_GetLowestChainRecordSlot(FieldSystem fieldSystem)
    {
        byte[] records = SpecialEncounter.SpecialEncounter_GetRadarChainRecords(SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!));
        for (byte i = 0; i < RecordCount; i++)
        {
            if (Bin.U16(records, i * RecordSize + RecordSpeciesOffset) == 0)
            {
                return i;
            }
        }
        byte lowest;
        if (Bin.U16(records, 0 * RecordSize + RecordChainOffset) >= Bin.U16(records, 1 * RecordSize + RecordChainOffset))
        {
            lowest = 1;
        }
        else
        {
            lowest = 0;
        }
        if (Bin.U16(records, lowest * RecordSize + RecordChainOffset) >= Bin.U16(records, 2 * RecordSize + RecordChainOffset))
        {
            lowest = 2;
        }
        return lowest;
    }

    // OFFSET 0x02069994
    private static bool PokeRadar_CheckPatchContinueChain(byte patchRing, int battleResult, RadarChain chain)
    {
        int odds;
        if (battleResult == BattleResultWin)
        {
            odds = PatchTables[ContinueOddsAfterWinOffset + patchRing];
        }
        else if (battleResult == BattleResultCaptured)
        {
            odds = PatchTables[ContinueOddsAfterCaptureOffset + patchRing];
        }
        else
        {
            odds = (byte)(chain.ShakeType >> (patchRing * 8));
        }
        return RandPercent() < odds;
    }

    // OFFSET 0x02069A00
    public static bool PokeRadar_RefreshChainTask(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        RadarRefreshTaskData data = (RadarRefreshTaskData)FieldTask.FieldTask_GetEnv(task)!;
        switch (data.State)
        {
            case 0:
            {
                MapObjectManager.MapObjectMan_PauseAllMovement(fieldSystem.MapObjectManager);
                ref byte radarCharge = ref SpecialEncounter.SpecialEncounter_GetRadarCharge(SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!));
                if (radarCharge < MaxRadarCharge)
                {
                    ScriptManager.ScriptManager_Start(task, ScriptRadarNotCharged, null, null);
                    BufferView parameter = ScriptManager.FieldSystem_GetScriptMemberPtr(fieldSystem, ScriptMemberParameter0);
                    parameter.W16(0, MaxRadarCharge - radarCharge);
                    data.State = 4;
                }
                else
                {
                    radarCharge = 0;
                    int playerX = PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar);
                    int playerZ = PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar);
                    PokeRadar_SpawnPatches(fieldSystem, playerX, playerZ, (RadarChain)fieldSystem.RadarChain!);
                    if (((RadarChain)fieldSystem.RadarChain!).Active != 0)
                    {
                        PokeRadar_SetupGrassPatches(fieldSystem, BattleResultWin, (RadarChain)fieldSystem.RadarChain!);
                        PokeRadar_CreateShakingPatches(fieldSystem, (RadarChain)fieldSystem.RadarChain!);
                        data.State = 1;
                    }
                    else
                    {
                        data.State = 3;
                    }
                }
                break;
            }
            case 1:
                SoundPlayback.Sound_PlayBGM(BgmPokeRadar);
                data.State = 2;
                break;
            case 2:
                if (PokeRadar_FinishPatchShakes((RadarChain)fieldSystem.RadarChain!))
                {
                    data.State = 4;
                }
                break;
            case 3:
                ScriptManager.ScriptManager_Start(task, ScriptRadarNoPatches, null, null);
                data.State = 4;
                break;
            case 4:
                MapObjectManager.MapObjectMan_UnpauseAllMovement(fieldSystem.MapObjectManager);
                return true;
        }
        return false;
    }

    // OFFSET 0x02069B00
    private static bool PokeRadar_CheckPatchShiny(int chainCount)
    {
        if (chainCount == 0)
        {
            return false;
        }
        int odds = ShinyOddsBase - chainCount * ShinyOddsStep;
        if (odds < ShinyOddsStep)
        {
            odds = ShinyOddsStep;
        }
        return RandMod((ushort)odds) == 0;
    }

    // OFFSET 0x02069B74
    public static void RadarChain_Increment(FieldSystem fieldSystem)
    {
        RadarChain chain = (RadarChain)fieldSystem.RadarChain!;
        PokeRadar_IncrementWithCap(ref chain.Count);
        PokeRadar_TryReplaceLowestChainRecord(fieldSystem, (RadarChain)fieldSystem.RadarChain!);
    }

    // OFFSET 0x02069B90
    public static int RadarChain_GetCount(FieldSystem fieldSystem)
    {
        return ((RadarChain)fieldSystem.RadarChain!).Count;
    }

    // OFFSET 0x02069B98
    public static void PokeRadar_ChargeStep(FieldSystem fieldSystem)
    {
        Bag bag = BagItems.SaveData_GetBag(fieldSystem.SaveData!);
        if (BagItems.Bag_CanRemoveItem(bag, ItemPokeRadar, 1, HeapIDField))
        {
            ref byte radarCharge = ref SpecialEncounter.SpecialEncounter_GetRadarCharge(SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!));
            if (radarCharge < MaxRadarCharge)
            {
                radarCharge++;
            }
        }
    }

    // OFFSET 0x02069BCC
    private static void PokeRadar_IncrementWithCap(ref int value)
    {
        value++;
        if (value > MaxChainCount)
        {
            value = MaxChainCount;
        }
    }
}
