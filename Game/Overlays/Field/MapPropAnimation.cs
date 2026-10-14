using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed class MapPropAnimation
{
    public G3dAnimObj? AnimationObj;
    public bool Loaded;
    public int CurrentLoop;
    public int AnimeArchiveID;
    public bool Paused;
    public byte[]? AnimationFile;
    public int LoopCount;
    public bool Looping;
    public bool Reversed;
    public int PastoriaGymButtonGroup;
}

public sealed class BicycleSlopeAnimation
{
    public bool Loaded;
    public G3dRenderObj? RenderObj;
    public MapPropAnimation? Animation;
    public int AnimeArchiveID;
}

public sealed class MapPropAnimeListFile
{
    public const int Size = 0x14;
    public const int ArchiveIDsCount = 4;

    public byte HasAnimations;
    public byte Flags;
    public byte IsBicycleSlope;
    public readonly int[] AnimeArchiveIDs = new int[ArchiveIDsCount];
}

public sealed class MapPropAnimationManager
{
    public const int MaxAnimations = 16;
    public const int MaxBicycleSlopeAnimations = 2;

    public readonly HeapAllocator Allocator = new();
    public readonly G3dAllocator AnimationAllocator = new();
    public readonly MapPropAnimation[] Animations = new MapPropAnimation[MaxAnimations];
    public readonly BicycleSlopeAnimation[] BicycleSlopeAnimations = new BicycleSlopeAnimation[MaxBicycleSlopeAnimations];
    public NARC? AnimeNARC;
    public NARC? AnimeListNARC;

    public MapPropAnimationManager()
    {
        for (int i = 0; i < MaxAnimations; i++)
        {
            Animations[i] = new MapPropAnimation();
        }
        for (int i = 0; i < MaxBicycleSlopeAnimations; i++)
        {
            BicycleSlopeAnimations[i] = new BicycleSlopeAnimation();
        }
    }
}

public sealed class MapPropOneShotAnimation
{
    public const int MaxAnimations = 4;
    public const int MaxRenderObjs = 6;

    public readonly MapPropAnimation?[] Animations = new MapPropAnimation?[MaxAnimations];
    public int AnimationCount;
    public readonly G3dRenderObj?[] MapPropRenderObjs = new G3dRenderObj?[MaxRenderObjs];
    public MapPropAnimation? CurrentAnimation;
    public int MapPropModelID;
    public byte Tag;
}

public sealed class MapPropOneShotAnimationManager
{
    public const int MaxItems = 16;

    public readonly MapPropOneShotAnimation[] Items = new MapPropOneShotAnimation[MaxItems];

    public MapPropOneShotAnimationManager()
    {
        for (int i = 0; i < MaxItems; i++)
        {
            Items[i] = new MapPropOneShotAnimation();
        }
    }
}

public static class MapPropAnimations
{
    private const int HeapIDField = 4;
    private const int MapPropAnimeArchive = 0x6E;
    private const int MapPropAnimeListArchive = 0x6F;
    private const int AnimeArchiveIDNone = -1;
    private const int LoopCountInfinite = -1;
    private const int ListFlagDeferredLoading = 1;
    private const int ListFlagDeferredAddToRenderObjShift = 1;

    private static MapPropAnimeListFile ReadAnimeListFile(MapPropAnimationManager manager, int mapPropModelID)
    {
        byte[] data = new byte[MapPropAnimeListFile.Size];
        NARC.NARC_ReadWholeMember(manager.AnimeListNARC!, (uint)mapPropModelID, data);
        MapPropAnimeListFile listFile = new();
        listFile.HasAnimations = data[0];
        listFile.Flags = data[1];
        listFile.IsBicycleSlope = data[2];
        for (int i = 0; i < MapPropAnimeListFile.ArchiveIDsCount; i++)
        {
            listFile.AnimeArchiveIDs[i] = Bin.I32(data, 4 + i * 4);
        }
        return listFile;
    }

    // OFFSET 0x021D37AC
    private static bool BicycleSlopeAnimation_Add(BicycleSlopeAnimation[] bicycleSlopeAnimations, G3dRenderObj renderObj, MapPropAnimation animation, byte animeArchiveID)
    {
        if (animation.LoopCount == LoopCountInfinite)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < MapPropAnimationManager.MaxBicycleSlopeAnimations; i++)
        {
            BicycleSlopeAnimation slope = bicycleSlopeAnimations[i];
            if (slope.Loaded && slope.AnimeArchiveID == animeArchiveID && slope.RenderObj == renderObj)
            {
                return false;
            }
        }
        for (int i = 0; i < MapPropAnimationManager.MaxBicycleSlopeAnimations; i++)
        {
            BicycleSlopeAnimation slope = bicycleSlopeAnimations[i];
            if (!slope.Loaded)
            {
                slope.Loaded = true;
                slope.AnimeArchiveID = animeArchiveID;
                slope.Animation = animation;
                slope.RenderObj = renderObj;
                break;
            }
        }
        return true;
    }

    // OFFSET 0x021D380C
    private static byte[] MapPropAnimation_LoadFile(NARC animeNARC, int animeArchiveID, G3dModel? mapPropModel, out G3dAnimObj? animationObj, G3dTexture? mapPropTexture, G3dAllocator allocator)
    {
        if (animeArchiveID == AnimeArchiveIDNone)
        {
            ErrorHandling.GF_AssertFail();
        }
        byte[]? animationFile = NARC.NARC_AllocAndReadWholeMember(animeNARC, (uint)animeArchiveID, HeapIDField);
        if (animationFile == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        G3dAnim? animation = G3d.GetAnimByIndex(animationFile!, 0);
        if (animation == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        animationObj = G3d.AllocAnimObj(allocator, animation!, mapPropModel!);
        G3d.InitAnimObj(animationObj, animation!, mapPropModel!, mapPropTexture);
        return animationFile!;
    }

    // OFFSET 0x021D3868
    private static bool RenderObj_RemoveAnimationObj(G3dRenderObj? renderObj, G3dAnimObj? animationObj)
    {
        if (animationObj == null)
        {
            return false;
        }
        if (renderObj == null)
        {
            return false;
        }
        G3d.RemoveAnimObj(renderObj, animationObj);
        return true;
    }

    // OFFSET 0x021D3884
    private static void BicycleSlopeAnimation_RemoveFinished(BicycleSlopeAnimation[] bicycleSlopeAnimations)
    {
        for (int i = 0; i < MapPropAnimationManager.MaxBicycleSlopeAnimations; i++)
        {
            BicycleSlopeAnimation slope = bicycleSlopeAnimations[i];
            if (slope.Loaded && MapPropAnimation_IsLoopFinished(slope.Animation!))
            {
                RenderObj_RemoveAnimationObj(slope.RenderObj, slope.Animation!.AnimationObj);
                slope.Loaded = false;
                slope.AnimeArchiveID = 0;
                slope.Animation = null;
                slope.RenderObj = null;
            }
        }
    }

    // OFFSET 0x021D38B8
    public static MapPropAnimationManager MapPropAnimationManager_New()
    {
        MapPropAnimationManager manager = new();
        for (int i = 0; i < MapPropAnimationManager.MaxAnimations; i++)
        {
            MapPropAnimation animation = manager.Animations[i];
            animation.Loaded = false;
            animation.Paused = false;
            animation.AnimationObj = null;
            animation.CurrentLoop = 0;
            animation.AnimeArchiveID = 0;
            animation.AnimationFile = null;
            animation.LoopCount = LoopCountInfinite;
            animation.Looping = false;
            animation.Reversed = false;
            animation.PastoriaGymButtonGroup = 0;
        }
        for (int i = 0; i < MapPropAnimationManager.MaxBicycleSlopeAnimations; i++)
        {
            BicycleSlopeAnimation slope = manager.BicycleSlopeAnimations[i];
            slope.Loaded = false;
            slope.RenderObj = null;
            slope.Animation = null;
            slope.AnimeArchiveID = 0;
        }
        Heap.HeapExp_FndInitAllocator(manager.Allocator, HeapIDField, 4);
        manager.AnimeNARC = NARC.NARC_ctor(MapPropAnimeArchive, HeapIDField);
        manager.AnimeListNARC = NARC.NARC_ctor(MapPropAnimeListArchive, HeapIDField);
        return manager;
    }

    // OFFSET 0x021D3938
    private static G3dAnimObj? MapPropAnimation_GetAnimationObj(MapPropAnimation? animation)
    {
        if (animation == null)
        {
            return null;
        }
        return animation.AnimationObj;
    }

    // OFFSET 0x021D3944
    private static bool MapPropAnimeListFile_IsDeferredLoading(byte flags)
    {
        if ((flags & ListFlagDeferredLoading) == 1)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x021D3954
    private static bool MapPropAnimeListFile_IsDeferredAddToRenderObj(byte flags)
    {
        if (((flags >> ListFlagDeferredAddToRenderObjShift) & 1) == 1)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x021D3964
    private static bool MapPropAnimation_IsLooping(MapPropAnimation animation)
    {
        if (!animation.Loaded)
        {
            ErrorHandling.GF_AssertFail();
        }
        return animation.Looping;
    }

    // OFFSET 0x021D3978
    public static MapPropAnimation? MapPropAnimationManager_LoadPropAnimationForOneShot(int mapPropModelID, int mapPropAnimIndex, int animationLoopCount, int pastoriaGymButtonGroup, bool animationReversed, bool animationPaused, bool isDeferredLoading, G3dModel? mapPropModel, G3dTexture? mapPropTexture, MapPropAnimationManager manager)
    {
        MapPropAnimeListFile listFile = ReadAnimeListFile(manager, mapPropModelID);
        if (mapPropAnimIndex >= MapPropAnimeListFile.ArchiveIDsCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        int animeArchiveID = listFile.AnimeArchiveIDs[mapPropAnimIndex];
        if (animeArchiveID == AnimeArchiveIDNone)
        {
            return null;
        }
        if (isDeferredLoading != MapPropAnimeListFile_IsDeferredLoading(listFile.Flags))
        {
            return null;
        }
        for (int i = 0; i < MapPropAnimationManager.MaxAnimations; i++)
        {
            if (pastoriaGymButtonGroup != 0 && manager.Animations[i].PastoriaGymButtonGroup == pastoriaGymButtonGroup)
            {
                ErrorHandling.GF_AssertFail();
            }
        }
        for (int i = 0; i < MapPropAnimationManager.MaxAnimations; i++)
        {
            MapPropAnimation animation = manager.Animations[i];
            if (animation.Loaded)
            {
                continue;
            }
            animation.Loaded = true;
            animation.Paused = animationPaused;
            animation.AnimationObj = null;
            animation.CurrentLoop = 0;
            animation.AnimeArchiveID = animeArchiveID;
            animation.Reversed = animationReversed;
            animation.PastoriaGymButtonGroup = pastoriaGymButtonGroup;
            animation.AnimationFile = MapPropAnimation_LoadFile(manager.AnimeNARC!, animeArchiveID, mapPropModel, out animation.AnimationObj, mapPropTexture, manager.AnimationAllocator);
            animation.LoopCount = animationLoopCount;
            animation.Looping = true;
            MapPropAnimation_GoToFirstFrame(animation);
            return animation;
        }
        ErrorHandling.GF_AssertFail();
        return null;
    }

    // OFFSET 0x021D3A50
    public static void MapPropAnimationManager_LoadPropAnimations(int mapPropModelID, G3dModel? mapPropModel, G3dTexture? mapPropTexture, MapPropAnimationManager manager)
    {
        MapPropAnimeListFile listFile = ReadAnimeListFile(manager, mapPropModelID);
        if (listFile.HasAnimations == 0)
        {
            return;
        }
        for (int j = 0; j < MapPropAnimeListFile.ArchiveIDsCount; j++)
        {
            int animeArchiveID = listFile.AnimeArchiveIDs[j];
            if (animeArchiveID == AnimeArchiveIDNone)
            {
                return;
            }
            if (MapPropAnimeListFile_IsDeferredLoading(listFile.Flags))
            {
                return;
            }
            int i;
            for (i = 0; i < MapPropAnimationManager.MaxAnimations; i++)
            {
                MapPropAnimation animation = manager.Animations[i];
                if (animation.Loaded)
                {
                    continue;
                }
                animation.Loaded = true;
                animation.Paused = false;
                animation.AnimationObj = null;
                animation.CurrentLoop = 0;
                animation.AnimeArchiveID = animeArchiveID;
                animation.Reversed = false;
                animation.PastoriaGymButtonGroup = 0;
                animation.AnimationFile = MapPropAnimation_LoadFile(manager.AnimeNARC!, animeArchiveID, mapPropModel, out animation.AnimationObj, mapPropTexture, manager.AnimationAllocator);
                animation.LoopCount = LoopCountInfinite;
                animation.Looping = true;
                MapPropAnimation_GoToFirstFrame(animation);
                if (listFile.IsBicycleSlope != 0)
                {
                    animation.Paused = true;
                    animation.LoopCount = 1;
                }
                break;
            }
            if (i == MapPropAnimationManager.MaxAnimations)
            {
                ErrorHandling.GF_AssertFail();
            }
        }
    }

    // OFFSET 0x021D3B24
    public static bool MapPropAnimationManager_AddAnimationToRenderObj(int mapPropModelID, int mapPropAnimIndex, bool isDeferredAddToRenderObj, G3dRenderObj mapPropRenderObj, MapPropAnimationManager? manager)
    {
        if (manager == null)
        {
            ErrorHandling.GF_AssertFail();
            return false;
        }
        if (mapPropModelID >= MapPropAnimationManager_GetAnimeListNARCFileCount(manager))
        {
            return false;
        }
        MapPropAnimeListFile listFile = ReadAnimeListFile(manager, mapPropModelID);
        if (mapPropAnimIndex >= MapPropAnimeListFile.ArchiveIDsCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        int animeArchiveID = listFile.AnimeArchiveIDs[mapPropAnimIndex];
        if (animeArchiveID == AnimeArchiveIDNone)
        {
            return false;
        }
        if (isDeferredAddToRenderObj != MapPropAnimeListFile_IsDeferredAddToRenderObj(listFile.Flags))
        {
            return false;
        }
        for (int i = 0; i < MapPropAnimationManager.MaxAnimations; i++)
        {
            MapPropAnimation animation = manager.Animations[i];
            if (animation.AnimeArchiveID != animeArchiveID)
            {
                continue;
            }
            bool addToRenderObj;
            if (listFile.IsBicycleSlope != 0)
            {
                addToRenderObj = BicycleSlopeAnimation_Add(manager.BicycleSlopeAnimations, mapPropRenderObj, animation, (byte)animeArchiveID);
            }
            else
            {
                addToRenderObj = true;
            }
            if (addToRenderObj)
            {
                G3d.AddAnimObj(mapPropRenderObj, animation.AnimationObj!);
            }
            return true;
        }
        return false;
    }

    // OFFSET 0x021D3BE4
    public static bool MapPropAnimationManager_AddAllAnimationsToRenderObj(int mapPropModelID, G3dRenderObj mapPropRenderObj, MapPropAnimationManager? manager)
    {
        bool added = false;
        if (manager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (mapPropModelID >= MapPropAnimationManager_GetAnimeListNARCFileCount(manager!))
        {
            return false;
        }
        MapPropAnimeListFile listFile = ReadAnimeListFile(manager!, mapPropModelID);
        if (MapPropAnimeListFile_IsDeferredAddToRenderObj(listFile.Flags))
        {
            return false;
        }
        for (int j = 0; j < MapPropAnimeListFile.ArchiveIDsCount; j++)
        {
            int animeArchiveID = listFile.AnimeArchiveIDs[j];
            if (animeArchiveID == AnimeArchiveIDNone)
            {
                return added;
            }
            for (int i = 0; i < MapPropAnimationManager.MaxAnimations; i++)
            {
                MapPropAnimation animation = manager!.Animations[i];
                if (animation.AnimeArchiveID != animeArchiveID)
                {
                    continue;
                }
                bool addToRenderObj;
                if (listFile.IsBicycleSlope != 0)
                {
                    addToRenderObj = BicycleSlopeAnimation_Add(manager.BicycleSlopeAnimations, mapPropRenderObj, animation, (byte)animeArchiveID);
                }
                else
                {
                    addToRenderObj = true;
                }
                if (addToRenderObj)
                {
                    G3d.AddAnimObj(mapPropRenderObj, animation.AnimationObj!);
                    added = true;
                }
                break;
            }
        }
        return added;
    }

    // OFFSET 0x021D3CAC
    public static void MapPropAnimationManager_UnloadAllAnimations(MapPropAnimationManager? manager)
    {
        if (manager == null)
        {
            return;
        }
        for (int i = 0; i < MapPropAnimationManager.MaxAnimations; i++)
        {
            MapPropAnimation animation = manager.Animations[i];
            if (animation.Loaded)
            {
                G3d.FreeAnimObj(manager.AnimationAllocator, animation.AnimationObj!);
                animation.AnimationObj = null;
                Heap.Heap_Free(animation.AnimationFile!);
                animation.Loaded = false;
                animation.PastoriaGymButtonGroup = 0;
            }
        }
    }

    // OFFSET 0x021D3CE4
    public static void MapPropAnimationManager_UnloadAnimation(MapPropAnimation? animation, MapPropAnimationManager? manager)
    {
        if (manager == null)
        {
            return;
        }
        if (animation == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (animation!.Loaded)
        {
            G3d.FreeAnimObj(manager.AnimationAllocator, animation.AnimationObj!);
            animation.AnimationObj = null;
            Heap.Heap_Free(animation.AnimationFile!);
            animation.Loaded = false;
            animation.PastoriaGymButtonGroup = 0;
        }
    }

    // OFFSET 0x021D3D18
    public static void MapPropAnimationManager_RemoveAnimationFromRenderObj(MapPropAnimationManager manager, G3dRenderObj? mapPropRenderObj, int mapPropModelID, int mapPropAnimIndex)
    {
        MapPropAnimeListFile listFile = ReadAnimeListFile(manager, mapPropModelID);
        if (mapPropAnimIndex >= MapPropAnimeListFile.ArchiveIDsCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        int animeArchiveID = listFile.AnimeArchiveIDs[mapPropAnimIndex];
        if (animeArchiveID == AnimeArchiveIDNone)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (byte i = 0; i < MapPropAnimationManager.MaxAnimations; i++)
        {
            MapPropAnimation animation = manager.Animations[i];
            if (animation.Loaded && animation.AnimeArchiveID == animeArchiveID)
            {
                RenderObj_RemoveAnimationObj(mapPropRenderObj, animation.AnimationObj);
                return;
            }
        }
    }

    // OFFSET 0x021D3D7C
    public static void MapPropAnimationManager_Free(MapPropAnimationManager? manager)
    {
        if (manager == null)
        {
            return;
        }
        NARC.NARC_dtor(manager.AnimeNARC!);
        NARC.NARC_dtor(manager.AnimeListNARC!);
    }

    // OFFSET 0x021D3DA0
    public static MapPropAnimation MapPropAnimationManager_GetAnimationByPastoriaGymButtonGroup(int pastoriaGymButtonGroup, MapPropAnimationManager manager)
    {
        if (pastoriaGymButtonGroup == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapPropAnimation? animation = null;
        for (int i = 0; i < MapPropAnimationManager.MaxAnimations; i++)
        {
            if (manager.Animations[i].PastoriaGymButtonGroup == pastoriaGymButtonGroup)
            {
                animation = manager.Animations[i];
                if (!animation.Loaded)
                {
                    ErrorHandling.GF_AssertFail();
                }
                break;
            }
        }
        if (animation == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return animation!;
    }

    // OFFSET 0x021D3DE4
    public static MapPropAnimation MapPropAnimationManager_GetAnimation(int mapPropModelID, int mapPropAnimIndex, MapPropAnimationManager manager)
    {
        MapPropAnimeListFile listFile = ReadAnimeListFile(manager, mapPropModelID);
        if (mapPropAnimIndex >= MapPropAnimeListFile.ArchiveIDsCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        int animeArchiveID = listFile.AnimeArchiveIDs[mapPropAnimIndex];
        MapPropAnimation? animation = null;
        for (int i = 0; i < MapPropAnimationManager.MaxAnimations; i++)
        {
            if (manager.Animations[i].AnimeArchiveID == animeArchiveID)
            {
                animation = manager.Animations[i];
                if (!animation.Loaded)
                {
                    ErrorHandling.GF_AssertFail();
                }
                break;
            }
        }
        if (animation == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return animation!;
    }

    // OFFSET 0x021D3E40
    public static void MapPropAnimation_SetAnimationPaused(MapPropAnimation animation, bool paused)
    {
        animation.Paused = paused;
    }

    // OFFSET 0x021D3E44
    public static void MapPropAnimation_StartLoop(MapPropAnimation animation)
    {
        animation.Looping = true;
    }

    // OFFSET 0x021D3E4C
    public static bool MapPropAnimation_IsOnLastFrame(MapPropAnimation animation)
    {
        G3dAnimObj animationObj = animation.AnimationObj!;
        if (!animation.Reversed)
        {
            if (animationObj.Frame >= (animationObj.ResAnm!.NumFrames << Fx32.Shift) - Fx32.One)
            {
                return true;
            }
            return false;
        }
        if (animationObj.Frame == 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x021D3E7C
    public static void MapPropAnimation_GoToFirstFrame(MapPropAnimation animation)
    {
        G3dAnimObj animationObj = animation.AnimationObj!;
        if (!animation.Reversed)
        {
            animationObj.Frame = 0;
        }
        else
        {
            animationObj.Frame = (animationObj.ResAnm!.NumFrames << Fx32.Shift) - Fx32.One;
        }
    }

    // OFFSET 0x021D3E9C
    public static void MapPropAnimation_GoToLastFrame(MapPropAnimation animation)
    {
        G3dAnimObj animationObj = animation.AnimationObj!;
        if (!animation.Reversed)
        {
            animationObj.Frame = (animationObj.ResAnm!.NumFrames << Fx32.Shift) - Fx32.One;
        }
        else
        {
            animationObj.Frame = 0;
        }
    }

    // OFFSET 0x021D3EBC
    public static void MapPropAnimation_AdvanceFrame(MapPropAnimation animation)
    {
        G3dAnimObj animationObj = animation.AnimationObj!;
        if (!animation.Reversed)
        {
            animationObj.Frame += Fx32.One;
            if (animationObj.Frame == animationObj.ResAnm!.NumFrames << Fx32.Shift)
            {
                animationObj.Frame = 0;
            }
        }
        else if (animationObj.Frame <= 0)
        {
            animationObj.Frame = (animationObj.ResAnm!.NumFrames << Fx32.Shift) - Fx32.One;
        }
        else
        {
            animationObj.Frame -= Fx32.One;
        }
    }

    // OFFSET 0x021D3F04
    public static void MapPropAnimation_SetReversed(MapPropAnimation animation, bool reversed)
    {
        animation.Reversed = reversed;
    }

    // OFFSET 0x021D3F08
    public static void MapPropAnimation_SetLoopCount(MapPropAnimation animation, int loopCount)
    {
        animation.LoopCount = loopCount;
    }

    // OFFSET 0x021D3F0C
    public static void MapPropAnimation_SetPastoriaGymButtonGroup(MapPropAnimation animation, int pastoriaGymButtonGroup)
    {
        animation.PastoriaGymButtonGroup = pastoriaGymButtonGroup;
    }

    // OFFSET 0x021D3F10
    public static void MapPropAnimationManager_AdvanceAnimations(MapPropAnimationManager? manager)
    {
        if (manager == null)
        {
            return;
        }
        for (int i = 0; i < MapPropAnimationManager.MaxAnimations; i++)
        {
            MapPropAnimation animation = manager.Animations[i];
            if (!animation.Loaded || animation.Paused || !animation.Looping)
            {
                continue;
            }
            MapPropAnimation_AdvanceFrame(animation);
            if (animation.LoopCount == LoopCountInfinite)
            {
                continue;
            }
            if (MapPropAnimation_IsOnLastFrame(animation))
            {
                int nextLoop = animation.CurrentLoop + 1;
                if (nextLoop >= animation.LoopCount)
                {
                    animation.Looping = false;
                }
                else
                {
                    animation.CurrentLoop = nextLoop;
                }
            }
        }
        BicycleSlopeAnimation_RemoveFinished(manager.BicycleSlopeAnimations);
    }

    // OFFSET 0x021D3F70
    public static bool MapPropAnimation_IsLoopFinished(MapPropAnimation animation)
    {
        if (!MapPropAnimation_IsLooping(animation))
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x021D3F84
    public static ushort MapPropAnimationManager_GetAnimeListNARCFileCount(MapPropAnimationManager manager)
    {
        return NARC.NARC_GetFileCount(manager.AnimeListNARC!);
    }

    // OFFSET 0x021D3F94
    public static byte MapPropAnimationManager_GetPropAnimationCount(MapPropAnimationManager manager, int mapPropModelID)
    {
        MapPropAnimeListFile listFile = ReadAnimeListFile(manager, mapPropModelID);
        if (listFile.HasAnimations == 0)
        {
            return 0;
        }
        byte count;
        for (count = 0; count < MapPropAnimeListFile.ArchiveIDsCount; count++)
        {
            if (listFile.AnimeArchiveIDs[count] == AnimeArchiveIDNone)
            {
                break;
            }
        }
        return count;
    }

    // OFFSET 0x021D3FD0
    private static MapPropOneShotAnimation? MapPropOneShotAnimationManager_AllocItem(MapPropOneShotAnimationManager oneShotAnimationManager, byte tag)
    {
        if (tag == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        int freeIndex = MapPropOneShotAnimationManager.MaxItems;
        for (byte i = 0; i < MapPropOneShotAnimationManager.MaxItems; i++)
        {
            if (freeIndex == MapPropOneShotAnimationManager.MaxItems && oneShotAnimationManager.Items[i].Tag == 0)
            {
                freeIndex = i;
            }
        }
        if (freeIndex != MapPropOneShotAnimationManager.MaxItems)
        {
            oneShotAnimationManager.Items[freeIndex].Tag = tag;
            return oneShotAnimationManager.Items[freeIndex];
        }
        ErrorHandling.GF_AssertFail();
        return null;
    }

    // OFFSET 0x021D4020
    private static void MapPropOneShotAnimation_Reset(MapPropOneShotAnimation oneShotAnimation)
    {
        oneShotAnimation.Tag = 0;
        oneShotAnimation.MapPropModelID = 0;
        oneShotAnimation.CurrentAnimation = null;
        for (byte i = 0; i < MapPropOneShotAnimation.MaxRenderObjs; i++)
        {
            oneShotAnimation.MapPropRenderObjs[i] = null;
        }
    }

    // OFFSET 0x021D4040
    private static MapPropOneShotAnimation? MapPropOneShotAnimationManager_FindItem(MapPropOneShotAnimationManager oneShotAnimationManager, int tag)
    {
        for (int i = 0; i < MapPropOneShotAnimationManager.MaxItems; i++)
        {
            if (tag == oneShotAnimationManager.Items[i].Tag)
            {
                return oneShotAnimationManager.Items[i];
            }
        }
        return null;
    }

    // OFFSET 0x021D4068
    private static void MapPropOneShotAnimation_Init(G3dRenderObj? mapPropRenderObj, int animationCount, MapPropOneShotAnimation oneShotAnimation)
    {
        if (animationCount > MapPropOneShotAnimation.MaxAnimations)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (mapPropRenderObj != null)
        {
            oneShotAnimation.MapPropRenderObjs[0] = mapPropRenderObj;
        }
        oneShotAnimation.AnimationCount = animationCount;
    }

    // OFFSET 0x021D4084
    private static void MapPropOneShotAnimation_SetAnimation(MapPropOneShotAnimation oneShotAnimation, int index, MapPropAnimation? animation)
    {
        if (index >= oneShotAnimation.AnimationCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        oneShotAnimation.Animations[index] = animation;
    }

    // OFFSET 0x021D409C
    private static MapPropAnimation? MapPropOneShotAnimation_SwitchAnimation(MapPropOneShotAnimation oneShotAnimation, int index)
    {
        if (index >= oneShotAnimation.AnimationCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapPropAnimation? animation = oneShotAnimation.Animations[index];
        G3dAnimObj? animationObj = MapPropAnimation_GetAnimationObj(animation);
        G3dAnimObj? currentAnimationObj = MapPropAnimation_GetAnimationObj(oneShotAnimation.CurrentAnimation);
        for (byte i = 0; i < MapPropOneShotAnimation.MaxRenderObjs; i++)
        {
            G3dRenderObj? renderObj = oneShotAnimation.MapPropRenderObjs[i];
            if (renderObj != null)
            {
                RenderObj_RemoveAnimationObj(renderObj, currentAnimationObj);
                G3d.AddAnimObj(renderObj, animationObj!);
            }
        }
        oneShotAnimation.CurrentAnimation = animation;
        return animation;
    }

    // OFFSET 0x021D40F0
    private static void MapPropOneShotAnimation_LoadAnimations(int mapPropModelID, G3dModel? mapPropModel, G3dTexture? mapPropTexture, int animationCount, int animationLoopCount, bool animationReversed, MapPropAnimationManager animationManager, MapPropOneShotAnimation oneShotAnimation)
    {
        for (int i = 0; i < animationCount; i++)
        {
            MapPropAnimation? animation = MapPropAnimationManager_LoadPropAnimationForOneShot(mapPropModelID, i, animationLoopCount, 0, animationReversed, true, true, mapPropModel, mapPropTexture, animationManager);
            if (animation == null)
            {
                ErrorHandling.GF_AssertFail();
            }
            MapPropOneShotAnimation_SetAnimation(oneShotAnimation, i, animation);
        }
    }

    // OFFSET 0x021D414C
    private static void MapPropOneShotAnimation_Unload(MapPropAnimationManager? animationManager, MapPropOneShotAnimation oneShotAnimation)
    {
        G3dAnimObj? currentAnimationObj = MapPropAnimation_GetAnimationObj(oneShotAnimation.CurrentAnimation);
        for (int i = 0; i < MapPropOneShotAnimation.MaxRenderObjs; i++)
        {
            if (oneShotAnimation.MapPropRenderObjs[i] != null)
            {
                RenderObj_RemoveAnimationObj(oneShotAnimation.MapPropRenderObjs[i], currentAnimationObj);
            }
        }
        for (int i = 0; i < oneShotAnimation.AnimationCount; i++)
        {
            MapPropAnimationManager_UnloadAnimation(oneShotAnimation.Animations[i], animationManager);
            oneShotAnimation.Animations[i] = null;
        }
    }

    // OFFSET 0x021D4194
    public static MapPropOneShotAnimationManager MapPropOneShotAnimationManager_New()
    {
        return new MapPropOneShotAnimationManager();
    }

    // OFFSET 0x021D41B4
    public static void MapPropOneShotAnimationManager_Free(ref MapPropOneShotAnimationManager? oneShotAnimationManager)
    {
        if (oneShotAnimationManager != null)
        {
            oneShotAnimationManager = null;
        }
    }

    // OFFSET 0x021D41C8
    public static void MapPropOneShotAnimationManager_LoadPropAnimations(MapPropAnimationManager animationManager, MapPropOneShotAnimationManager oneShotAnimationManager, int tag, int mapPropModelID, G3dRenderObj? mapPropRenderObj, G3dModel? mapPropModel, G3dTexture? mapPropTexture, int animationCount, byte animationLoopCount, bool animationReversed)
    {
        MapPropOneShotAnimation? oneShotAnimation = MapPropOneShotAnimationManager_AllocItem(oneShotAnimationManager, (byte)tag);
        if (oneShotAnimation == null)
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        MapPropOneShotAnimation_Init(mapPropRenderObj, animationCount, oneShotAnimation);
        int loopCount = animationLoopCount;
        if (loopCount == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (loopCount == 0)
        {
            loopCount = 1;
        }
        MapPropOneShotAnimation_LoadAnimations(mapPropModelID, mapPropModel, mapPropTexture, animationCount, loopCount, animationReversed, animationManager, oneShotAnimation);
        oneShotAnimation.MapPropModelID = mapPropModelID;
    }

    // OFFSET 0x021D4220
    public static void MapPropOneShotAnimationManager_SetAnimationRenderObj(MapPropOneShotAnimationManager oneShotAnimationManager, int tag, byte renderObjIndex, G3dRenderObj? renderObj)
    {
        if (renderObjIndex >= MapPropOneShotAnimation.MaxRenderObjs)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapPropOneShotAnimation oneShotAnimation = MapPropOneShotAnimationManager_FindItem(oneShotAnimationManager, tag)!;
        if (oneShotAnimation.MapPropRenderObjs[renderObjIndex] != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        oneShotAnimation.MapPropRenderObjs[renderObjIndex] = renderObj;
    }

    // OFFSET 0x021D4250
    public static void MapPropOneShotAnimationManager_PlayAnimation(MapPropOneShotAnimationManager oneShotAnimationManager, int tag, int mapPropAnimIndex)
    {
        if (tag == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapPropOneShotAnimation oneShotAnimation = MapPropOneShotAnimationManager_FindItem(oneShotAnimationManager, tag)!;
        MapPropAnimation animation = MapPropOneShotAnimation_SwitchAnimation(oneShotAnimation, mapPropAnimIndex)!;
        MapPropAnimation_SetAnimationPaused(animation, false);
    }

    // OFFSET 0x021D4278
    public static void MapPropOneShotAnimationManager_PlayAnimationWithSoundEffect(MapPropOneShotAnimationManager oneShotAnimationManager, int tag, int mapPropAnimIndex, int soundEffectID)
    {
        if (tag == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapPropOneShotAnimation oneShotAnimation = MapPropOneShotAnimationManager_FindItem(oneShotAnimationManager, tag)!;
        MapPropAnimation animation = MapPropOneShotAnimation_SwitchAnimation(oneShotAnimation, mapPropAnimIndex)!;
        if (soundEffectID != 0)
        {
            SoundPlayback.Sound_PlayEffect((ushort)soundEffectID);
        }
        MapPropAnimation_SetAnimationPaused(animation, false);
    }

    // OFFSET 0x021D42B0
    public static void MapPropOneShotAnimationManager_UnloadAnimation(MapPropAnimationManager? animationManager, MapPropOneShotAnimationManager oneShotAnimationManager, int tag)
    {
        if (tag == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapPropOneShotAnimation oneShotAnimation = MapPropOneShotAnimationManager_FindItem(oneShotAnimationManager, tag)!;
        MapPropOneShotAnimation_Unload(animationManager, oneShotAnimation);
        MapPropOneShotAnimation_Reset(oneShotAnimation);
    }

    // OFFSET 0x021D42D8
    public static MapPropAnimation? MapPropOneShotAnimationManager_GetCurrentAnimation(MapPropOneShotAnimationManager oneShotAnimationManager, int tag)
    {
        if (tag == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        return MapPropOneShotAnimationManager_FindItem(oneShotAnimationManager, tag)!.CurrentAnimation;
    }

    // OFFSET 0x021D42F0
    public static bool MapPropOneShotAnimationManager_IsAnimationLoopFinished(MapPropOneShotAnimationManager oneShotAnimationManager, int tag)
    {
        MapPropAnimation? animation = MapPropOneShotAnimationManager_GetCurrentAnimation(oneShotAnimationManager, tag);
        if (animation == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (MapPropAnimation_IsLoopFinished(animation!))
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x021D4310
    public static int MapPropOneShotAnimationManager_GetAnimationMapPropModelID(MapPropOneShotAnimationManager oneShotAnimationManager, int tag)
    {
        return MapPropOneShotAnimationManager_FindItem(oneShotAnimationManager, tag)!.MapPropModelID;
    }
}
