namespace PlatinumSharp.Game;

public sealed class HealingMachineAnimation
{
    public VecFx32 Position;
    public byte PokeballCount;
    public byte PokeballIndex;
    public byte Ticks;
    public byte State;
    public readonly byte[] PokeballPropIndices = new byte[6];
}

public static class HealingMachineAnimations
{
    private const int OverlayID = 6;
    private const int PokecenterMachineModelID = 0x7B;
    private const int PokeballModelID = 0x205;
    private const int ScreenModelID = 0x7C;
    private const int PokeballTag = 0x10;
    private const int ScreenTag = 0x20;
    private const int PokeballMaxTicks = 15;
    private const uint PokeballOffsetsAddress = 0x0224989C;
    private const int PokeballOffsetSize = 3;
    private const int SeqSePokeballPlaced = 0x5FE;
    private const ushort SeqFanfareHealed = 0x48E;
    private const int StateStart = 0;
    private const int StateAddPokeball = 1;
    private const int StateWaitForPokeball = 2;
    private const int StatePlayFinalAnimation = 3;
    private const int StateWaitForFinalAnimation = 4;
    private const int StateCleanUp = 5;

    // OFFSET 0x02246C24
    public static void FieldSystem_PlayHealingAnimation_Pokecenter(FieldSystem fieldSystem, byte pokeballCount)
    {
        object? mapProp = null;
        int mapMatrixIndex = 0;
        if (!TerrainCollisionManager.FieldSystem_FindLoadedMapPropByModelID(fieldSystem, PokecenterMachineModelID, ref mapProp, ref mapMatrixIndex))
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        HealingMachineAnimation animation = new HealingMachineAnimation();
        animation.PokeballCount = pokeballCount;
        animation.PokeballIndex = 0;
        animation.Ticks = 0;
        animation.State = StateStart;
        VecFx32 mapAbsoluteOrigin = default;
        TerrainCollisionManager.TerrainCollisionManager_GetMapAbsoluteOrigin(mapMatrixIndex, MapMatrix.MapMatrix_GetWidth(fieldSystem.MapMatrix), ref mapAbsoluteOrigin);
        animation.Position = MapProps.MapProp_GetPosition((MapProp)mapProp!);
        animation.Position.X += mapAbsoluteOrigin.X;
        animation.Position.Z += mapAbsoluteOrigin.Z;
        FieldTask.FieldTask_InitCall(fieldSystem.Task!, FieldTask_PlayHealingAnimation_Pokecenter, animation);
    }

    private static G3dModel? FirstModel(byte[]? modelFile)
    {
        G3dModelSet? modelSet = modelFile == null ? null : G3d.GetModelSet(modelFile);
        return modelSet == null ? null : G3d.GetModelByIndex(modelSet, 0);
    }

    // OFFSET 0x02246C9C
    private static bool FieldTask_PlayHealingAnimation_Pokecenter(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        HealingMachineAnimation animation = (HealingMachineAnimation)FieldTask.FieldTask_GetEnv(task)!;
        switch (animation.State)
        {
            case StateStart:
            {
                byte[]? pokeballFile = AreaData.AreaDataManager_GetMapPropModelFile(PokeballModelID, fieldSystem.AreaDataManager);
                byte[]? screenFile = AreaData.AreaDataManager_GetMapPropModelFile(ScreenModelID, fieldSystem.AreaDataManager);
                G3dModel? pokeballModel = FirstModel(pokeballFile);
                G3dModel? screenModel = FirstModel(screenFile);
                MapPropAnimations.MapPropOneShotAnimationManager_LoadPropAnimations(fieldSystem.MapPropAnimationManager!, fieldSystem.MapPropOneShotAnimationManager!, PokeballTag, PokeballModelID, null, pokeballModel, AreaData.AreaDataManager_GetMapPropTexture(fieldSystem.AreaDataManager), 1, 1, false);
                object? screenProp = null;
                int unused = 0;
                if (!TerrainCollisionManager.FieldSystem_FindLoadedMapPropByModelID(fieldSystem, ScreenModelID, ref screenProp, ref unused))
                {
                    ErrorHandling.GF_AssertFail();
                }
                G3dRenderObj screenRenderObj = MapProps.MapProp_GetRenderObj((MapProp)screenProp!);
                MapPropAnimations.MapPropOneShotAnimationManager_LoadPropAnimations(fieldSystem.MapPropAnimationManager!, fieldSystem.MapPropOneShotAnimationManager!, ScreenTag, ScreenModelID, screenRenderObj, screenModel, AreaData.AreaDataManager_GetMapPropTexture(fieldSystem.AreaDataManager), 1, 1, false);
                animation.State++;
                break;
            }
            case StateAddPokeball:
            {
                VecFx32 rotation = default;
                int[] offset = RomData.OverlayU32(OverlayID, (uint)(PokeballOffsetsAddress + animation.PokeballIndex * PokeballOffsetSize * 4), PokeballOffsetSize).Select(word => (int)word).ToArray();
                VecFx32 position = new VecFx32 { X = animation.Position.X + offset[0], Y = animation.Position.Y + offset[1], Z = animation.Position.Z + offset[2] };
                SoundPlayback.Sound_PlayEffect(SeqSePokeballPlaced);
                animation.PokeballPropIndices[animation.PokeballIndex] = MapProps.MapPropManager_LoadOne(fieldSystem.MapPropManager!, fieldSystem.AreaDataManager!, PokeballModelID, position, rotation, fieldSystem.MapPropAnimationManager);
                G3dRenderObj renderObj = MapProps.MapProp_GetRenderObj(MapProps.MapPropManager_GetLoadedPropSafely(fieldSystem.MapPropManager!, animation.PokeballPropIndices[animation.PokeballIndex]));
                MapPropAnimations.MapPropOneShotAnimationManager_SetAnimationRenderObj(fieldSystem.MapPropOneShotAnimationManager!, PokeballTag, animation.PokeballIndex, renderObj);
                animation.State++;
                break;
            }
            case StateWaitForPokeball:
                if (animation.Ticks < PokeballMaxTicks)
                {
                    animation.Ticks++;
                    break;
                }
                animation.Ticks = 0;
                animation.PokeballIndex++;
                if (animation.PokeballIndex < animation.PokeballCount)
                {
                    animation.State = StateAddPokeball;
                }
                else
                {
                    animation.State++;
                }
                break;
            case StatePlayFinalAnimation:
                MapPropAnimations.MapPropOneShotAnimationManager_PlayAnimation(fieldSystem.MapPropOneShotAnimationManager!, PokeballTag, 0);
                MapPropAnimations.MapPropOneShotAnimationManager_PlayAnimation(fieldSystem.MapPropOneShotAnimationManager!, ScreenTag, 0);
                SoundPlayback.Sound_PlayFanfare(SeqFanfareHealed);
                animation.State++;
                break;
            case StateWaitForFinalAnimation:
                if (!MapPropAnimations.MapPropOneShotAnimationManager_IsAnimationLoopFinished(fieldSystem.MapPropOneShotAnimationManager!, PokeballTag)
                    || !MapPropAnimations.MapPropOneShotAnimationManager_IsAnimationLoopFinished(fieldSystem.MapPropOneShotAnimationManager!, ScreenTag)
                    || SoundPlayback.Sound_IsBGMPausedByFanfare() != 0)
                {
                    break;
                }
                MapPropAnimations.MapPropOneShotAnimationManager_UnloadAnimation(fieldSystem.MapPropAnimationManager, fieldSystem.MapPropOneShotAnimationManager!, ScreenTag);
                MapPropAnimations.MapPropOneShotAnimationManager_UnloadAnimation(fieldSystem.MapPropAnimationManager, fieldSystem.MapPropOneShotAnimationManager!, PokeballTag);
                for (byte i = 0; i < animation.PokeballCount; i++)
                {
                    MapProps.MapPropManager_ClearOne(animation.PokeballPropIndices[i], fieldSystem.MapPropManager!);
                }
                animation.State++;
                break;
            case StateCleanUp:
                return true;
        }
        return false;
    }
}
