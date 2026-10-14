using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed class EncounterEffect
{
    public int State;
    public readonly int[] FlashDone = new int[1];
    public object? Param;
    public FieldSystem FieldSystem = null!;
    public int[]? Done;
    public readonly int[] EffectDone = new int[1];
    public NARC? Narc;
}

public sealed class LinearInterpolationTask
{
    public int X;
    public int StartX;
    public int DeltaX;
    public int CurrentStep;
    public int NumSteps;
    public int Screen;
}

public sealed class QuadraticInterpolationTask
{
    public int X;
    public int StartX;
    public int Velocity;
    public int Acceleration;
    public int CurrentStep;
    public int NumSteps;
}

public sealed class EncounterEffectFlash
{
    public int State;
    public int Count;
    public int Counter;
    public int Screen;
    public int FlashBrightness;
    public int EndBrightness;
    public readonly LinearInterpolationTask FlashTask = new();
    public readonly LinearInterpolationTask EndTask = new();
    public int[]? Done;
}

public sealed class EncounterEffectScreenSlice
{
    public readonly QuadraticInterpolationTask Offset = new();
    public int PixelsPerSlice;
    public int State;
    public HBlankSystem? HBlankSystem;
    public HBlankTask? HBlankTask;
    public SysTask? VBlankTask;
    public int[]? Done;
}

public sealed class EncounterEffectScreenCross
{
    public readonly QuadraticInterpolationTask X = new();
    public readonly QuadraticInterpolationTask Y = new();
    public int SplitLine;
    public int State;
    public HBlankSystem? HBlankSystem;
    public HBlankTask? HBlankTask;
    public SysTask? VBlankTask;
    public int[]? Done;
}

public sealed class EncounterEffectSpriteManager
{
    public const int ResourceTypes = 4;

    public SpriteList? List;
    public readonly G2dRenderer Renderer = new();
    public readonly SpriteResourceCollection?[] Collections = new SpriteResourceCollection?[ResourceTypes];
}

public sealed class EncounterEffectSpriteResources
{
    public readonly SpriteResource?[] Resources = new SpriteResource?[EncounterEffectSpriteManager.ResourceTypes];
    public readonly SpriteResourcesHeader Header = new();
}

public static class EncounterEffects
{
    public const int EffectCount = 36;

    private const uint EffectTable = 0x021F9A74;
    private const int HeapIDField = 4;
    private const int NarcEncounterEffects = 0x70;
    private const int TaskPriority = 5;
    private const int BrightnessTaskPriority = 10;
    private const int VBlankTaskPriority = 0x400;
    private const int ScreenMain = 1;
    private const int ScreenSub = 2;
    private const int DisplayMain = 0;
    private const int DisplaySub = 1;
    private const int WindowAllLayers = 0x3F;
    private const int WindowsNone = 0;
    private const int Window0Only = 1;
    private const int Window1Only = 2;
    private const int ScreenRight = 0xFF;
    private const int ScreenBottom = 0xC0;
    private const int ScreenMiddleLine = 0x60;
    private const int BgLayers = 4;
    private const int BgOffsetMask = 0x1FF;
    private const int FlashSteps = 3;
    private const int FlashEndSteps = 8;
    private const int VRamMain = 1;
    private const int ResourceIDNone = -1;

    private static uint[]? effectTable;

    private static SysTaskFunc? PortedEffect(uint address)
    {
        switch (address)
        {
            case 0x021E2338:
                return EncounterEffect_Grass_HigherLevel;
            case 0x021E24A8:
                return EncounterEffect_Grass_LowerLevel;
            case 0x021E2B54:
                return EncounterEffect_TrainerGrassLowerLevel;
            case 0x021E2EB0:
                return EncounterEffect_TrainerGrassHigherLevel;
        }
        return null;
    }

    // OFFSET 0x021DDBE8
    public static void EncounterEffect_Start(int effectID, FieldSystem fieldSystem, int[]? done)
    {
        effectTable ??= RomData.Overlay(5, EffectTable, EffectCount * 4).Chunk(4).Select(b => Bin.U32(b, 0) & ~1u).ToArray();
        SysTaskFunc function = PortedEffect(effectTable[effectID]) ?? throw Unported.Missing(effectTable[effectID]);
        SysTask task = SysTaskExtensions.SysTask_StartAndAllocateParam<EncounterEffect>(function, 0x24, TaskPriority, HeapIDField)!;
        EncounterEffect effect = (EncounterEffect)SysTaskManager.SysTask_GetParam(task)!;
        effect.FieldSystem = fieldSystem;
        effect.Done = done;
        effect.Narc = NARC.NARC_ctor(NarcEncounterEffects, HeapIDField);
        if (effect.Done != null)
        {
            effect.Done[0] = 0;
        }
        effect.EffectDone[0] = 0;
    }

    // OFFSET 0x021DDC28
    private static void EncounterEffect_Finish(EncounterEffect effect, SysTask task)
    {
        NARC.NARC_dtor(effect.Narc!);
        effect.Param = null;
        SysTaskExtensions.SysTask_FinishAndFreeParam(task);
    }

    // OFFSET 0x021DDC44
    private static void EncounterEffect_Flash(int screen, int flashBrightness, int endBrightness, int[]? done, int count)
    {
        EncounterEffectFlash flash = new();
        SysTask.SysTask_Start(EncounterEffect_FlashTask, flash, TaskPriority);
        flash.Done = done;
        if (done != null)
        {
            done[0] = 0;
        }
        flash.Screen = screen;
        flash.FlashBrightness = flashBrightness;
        flash.EndBrightness = endBrightness;
        flash.Count = count;
    }

    // OFFSET 0x021DDC88
    private static void EncounterEffect_FlashTask(SysTask task, object? param)
    {
        EncounterEffectFlash flash = (EncounterEffectFlash)param!;
        switch (flash.State)
        {
            case 0:
                if (flash.Screen == ScreenMain)
                {
                    BrightnessInterpolation_Init(flash.EndTask, 0, flash.EndBrightness, ScreenSub, FlashEndSteps);
                }
                else if (flash.Screen == ScreenSub)
                {
                    BrightnessInterpolation_Init(flash.EndTask, 0, flash.EndBrightness, ScreenMain, FlashEndSteps);
                }
                flash.State++;
                break;
            case 1:
                BrightnessInterpolation_Init(flash.FlashTask, 0, flash.FlashBrightness, flash.Screen, FlashSteps);
                flash.State++;
                break;
            case 2:
                if (BrightnessInterpolation_Update(flash.FlashTask) != 0)
                {
                    flash.State++;
                }
                break;
            case 3:
                BrightnessInterpolation_Init(flash.FlashTask, flash.FlashBrightness, 0, flash.Screen, FlashSteps);
                flash.State++;
                break;
            case 4:
                if (BrightnessInterpolation_Update(flash.FlashTask) != 0)
                {
                    flash.Counter++;
                    if (flash.Counter == flash.Count)
                    {
                        flash.State = 5;
                    }
                    else
                    {
                        flash.State = 1;
                    }
                }
                break;
            case 5:
                flash.State = 0;
                flash.Counter = 0;
                if (flash.Done != null)
                {
                    flash.Done[0] = 1;
                }
                SysTask.SysTask_Done(task);
                return;
        }
        BrightnessInterpolation_Update(flash.EndTask);
    }

    // OFFSET 0x021DDD7C
    private static int EncounterEffect_GetEffectDone(EncounterEffect effect)
    {
        return effect.EffectDone[0];
    }

    // OFFSET 0x021DDD80
    private static void LinearInterpolationTaskS32_Init(LinearInterpolationTask task, int start, int end, int numSteps)
    {
        task.X = start;
        task.StartX = start;
        task.DeltaX = end - start;
        task.NumSteps = numSteps;
        task.CurrentStep = 0;
    }

    // OFFSET 0x021DDD90
    private static int LinearInterpolationTaskS32_Update(LinearInterpolationTask task)
    {
        task.X = task.DeltaX * task.CurrentStep / task.NumSteps + task.StartX;
        if (task.CurrentStep + 1 <= task.NumSteps)
        {
            task.CurrentStep++;
            return 0;
        }
        task.CurrentStep = task.NumSteps;
        return 1;
    }

    // OFFSET 0x021DDDBC
    private static void LinearInterpolationTaskFX32_Init(LinearInterpolationTask task, int start, int end, int numSteps)
    {
        task.X = start;
        task.StartX = start;
        task.DeltaX = end - start;
        task.NumSteps = numSteps;
        task.CurrentStep = 0;
    }

    // OFFSET 0x021DDDCC
    private static int LinearInterpolationTaskFX32_Update(LinearInterpolationTask task)
    {
        task.X = Fx32.Div(Fx32.Mul(task.DeltaX, task.CurrentStep << 12), task.NumSteps << 12) + task.StartX;
        if (task.CurrentStep + 1 <= task.NumSteps)
        {
            task.CurrentStep++;
            return 0;
        }
        task.CurrentStep = task.NumSteps;
        return 1;
    }

    // OFFSET 0x021DDE14
    private static void QuadraticInterpolationTaskFX32_Init(QuadraticInterpolationTask task, int start, int end, int velocity, int numSteps)
    {
        int distance = Fx32.Mul(velocity, numSteps << 12);
        int remaining = (end - start) - distance;
        int acceleration = Fx32.Div(Fx32.Mul(remaining, 2 << 12), (numSteps * numSteps) << 12);
        task.X = start;
        task.StartX = start;
        task.Velocity = velocity;
        task.Acceleration = acceleration;
        task.CurrentStep = 0;
        task.NumSteps = numSteps;
    }

    // OFFSET 0x021DDE74
    private static int QuadraticInterpolationTaskFX32_Update(QuadraticInterpolationTask task)
    {
        int step = task.CurrentStep;
        int linear = Fx32.Mul(task.Velocity, step << 12);
        int quadratic = Fx32.Div(Fx32.Mul(task.Acceleration, (step * step) << 12), 2 << 12);
        task.X = task.StartX + (linear + quadratic);
        if (task.CurrentStep + 1 <= task.NumSteps)
        {
            task.CurrentStep++;
            return 0;
        }
        task.CurrentStep = task.NumSteps;
        return 1;
    }

    // OFFSET 0x021DDEDC
    private static void Screen_SetMasterBrightness(int screen, int brightness)
    {
        if (screen == ScreenMain)
        {
            Video.SetMasterBrightness(DisplayMain, brightness);
        }
        else
        {
            Video.SetMasterBrightness(DisplaySub, brightness);
        }
    }

    // OFFSET 0x021DDEFC
    private static void BrightnessInterpolation_Init(LinearInterpolationTask task, int start, int end, int screen, int numSteps)
    {
        task.Screen = screen;
        LinearInterpolationTaskS32_Init(task, start, end, numSteps);
    }

    // OFFSET 0x021DDF08
    private static int BrightnessInterpolation_Update(LinearInterpolationTask task)
    {
        int done = LinearInterpolationTaskS32_Update(task);
        SysTask.SysTask_ExecuteAfterVBlank(BrightnessInterpolation_VBlankTask, task, BrightnessTaskPriority);
        return done;
    }

    // OFFSET 0x021DDF24
    private static void BrightnessInterpolation_VBlankTask(SysTask task, object? param)
    {
        LinearInterpolationTask interpolation = (LinearInterpolationTask)param!;
        Screen_SetMasterBrightness(interpolation.Screen, interpolation.X);
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x021DDF38
    private static EncounterEffectScreenSlice ScreenSlice_New()
    {
        EncounterEffectScreenSlice slice = new();
        Video.SetWindowInside(DisplayMain, 0, WindowAllLayers);
        Video.SetWindowInside(DisplayMain, 1, 0);
        return slice;
    }

    // OFFSET 0x021DDF74
    private static void ScreenSlice_Delete(EncounterEffectScreenSlice slice)
    {
        if (slice.HBlankTask != null)
        {
            ScreenSlice_Stop(slice);
        }
        Video.SetVisibleWindows(DisplayMain, WindowsNone);
    }

    private static void ScreenSlice_SetWindows(int offset)
    {
        if (offset >= 0)
        {
            Video.SetWindowHorizontal(DisplayMain, 0, 0, (byte)(ScreenRight - offset));
            Video.SetWindowVertical(DisplayMain, 0, 0, ScreenBottom);
            Video.SetWindowHorizontal(DisplayMain, 1, 0, (byte)(offset + 1));
            Video.SetWindowVertical(DisplayMain, 1, 0, ScreenBottom);
        }
        else
        {
            Video.SetWindowHorizontal(DisplayMain, 0, 0, (byte)(offset + ScreenRight));
            Video.SetWindowVertical(DisplayMain, 0, 0, ScreenBottom);
            Video.SetWindowHorizontal(DisplayMain, 1, 0, (byte)(1 - offset));
            Video.SetWindowVertical(DisplayMain, 1, 0, ScreenBottom);
        }
    }

    // OFFSET 0x021DDF9C
    private static void ScreenSlice_Start(EncounterEffect effect, EncounterEffectScreenSlice slice, int pixelsPerSlice, int numSteps, int start, int end, int speed)
    {
        if (slice.HBlankTask != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        effect.EffectDone[0] = 0;
        slice.HBlankSystem = effect.FieldSystem.FieldMapContext!.HBlankSystem;
        slice.PixelsPerSlice = pixelsPerSlice;
        slice.State = 0;
        slice.Done = effect.EffectDone;
        QuadraticInterpolationTaskFX32_Init(slice.Offset, start, end, speed, numSteps);
        ScreenSlice_SetWindows(start);
        SysTask.SysTask_ExecuteAfterVBlank(ScreenSlice_StartTask, slice, VBlankTaskPriority);
    }

    // OFFSET 0x021DE028
    private static void ScreenSlice_StartTask(SysTask task, object? param)
    {
        EncounterEffectScreenSlice slice = (EncounterEffectScreenSlice)param!;
        slice.VBlankTask = SysTask.SysTask_ExecuteAfterVBlank(ScreenSlice_VBlankTask, slice, VBlankTaskPriority);
        slice.HBlankTask = HBlankSystem.HBlankSystem_StartTask(slice.HBlankSystem!, ScreenSlice_HBlankCallback, slice);
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x021DE058
    private static void ScreenSlice_Restart(EncounterEffect effect, EncounterEffectScreenSlice slice, int pixelsPerSlice, int numSteps, int start, int end, int speed)
    {
        if (EncounterEffect_GetEffectDone(effect) != 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        slice.PixelsPerSlice = pixelsPerSlice;
        slice.State = 0;
        QuadraticInterpolationTaskFX32_Init(slice.Offset, start, end, speed, numSteps);
    }

    // OFFSET 0x021DE088
    private static void ScreenSlice_VBlankTask(SysTask task, object? param)
    {
        EncounterEffectScreenSlice slice = (EncounterEffectScreenSlice)param!;
        switch (slice.State)
        {
            case 0:
                if (QuadraticInterpolationTaskFX32_Update(slice.Offset) != 0)
                {
                    slice.State++;
                }
                ScreenSlice_SetWindows(slice.Offset.X >> 12);
                break;
            case 1:
                ScreenSlice_Stop(slice);
                break;
        }
    }

    // OFFSET 0x021DE0F0
    private static void ScreenSlice_Stop(EncounterEffectScreenSlice slice)
    {
        Video.SetWindowInside(DisplayMain, 0, WindowAllLayers);
        Video.SetWindowOutside(DisplayMain, 0);
        Video.SetWindowHorizontal(DisplayMain, 0, 0, 0);
        Video.SetWindowVertical(DisplayMain, 0, 0, 0);
        Video.SetVisibleWindows(DisplayMain, Window0Only);
        slice.Done![0] = 1;
        HBlankSystem.HBlankTask_Delete(slice.HBlankTask!);
        slice.HBlankTask = null;
        SysTask.SysTask_Done(slice.VBlankTask!);
        slice.VBlankTask = null;
    }

    // OFFSET 0x021DE14C
    private static void ScreenSlice_HBlankCallback(HBlankTask task, object? param)
    {
        EncounterEffectScreenSlice slice = (EncounterEffectScreenSlice)param!;
        int offset = slice.Offset.X;
        if (((Video.VCount / slice.PixelsPerSlice) & 1) != 0)
        {
            offset = -offset;
        }
        offset >>= 12;
        for (int layer = 0; layer < BgLayers; layer++)
        {
            Video.SetBgOffset(layer, (uint)(offset & BgOffsetMask));
        }
        if (offset >= 0)
        {
            Video.SetVisibleWindows(DisplayMain, Window0Only);
            Video.SetWindowOutside(DisplayMain, 0);
        }
        else
        {
            Video.SetVisibleWindows(DisplayMain, Window1Only);
            Video.SetWindowOutside(DisplayMain, WindowAllLayers);
        }
    }

    // OFFSET 0x021DE1CC
    private static EncounterEffectScreenCross ScreenCross_New()
    {
        EncounterEffectScreenCross cross = new();
        Video.SetWindowInside(DisplayMain, 0, WindowAllLayers);
        Video.SetWindowInside(DisplayMain, 1, WindowAllLayers);
        Video.SetWindowOutside(DisplayMain, 0);
        return cross;
    }

    // OFFSET 0x021DE218
    private static void ScreenCross_Delete(EncounterEffectScreenCross cross)
    {
        if (cross.HBlankTask != null)
        {
            ScreenCross_Stop(cross);
        }
        Video.SetVisibleWindows(DisplayMain, WindowsNone);
    }

    // OFFSET 0x021DE240
    private static void ScreenCross_Start(EncounterEffect effect, EncounterEffectScreenCross cross, int numSteps, int speedX, int speedY)
    {
        if (cross.HBlankTask != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        effect.EffectDone[0] = 0;
        cross.HBlankSystem = effect.FieldSystem.FieldMapContext!.HBlankSystem;
        cross.SplitLine = ScreenMiddleLine;
        cross.State = 0;
        cross.Done = effect.EffectDone;
        QuadraticInterpolationTaskFX32_Init(cross.X, 0, ScreenRight << 12, speedX, numSteps);
        QuadraticInterpolationTaskFX32_Init(cross.Y, 0, ScreenMiddleLine << 12, speedY, numSteps);
        Video.SetWindowHorizontal(DisplayMain, 0, 0, ScreenRight);
        Video.SetWindowVertical(DisplayMain, 0, 0, ScreenBottom);
        Video.SetWindowHorizontal(DisplayMain, 1, 0, ScreenRight);
        Video.SetWindowVertical(DisplayMain, 1, 0, ScreenBottom);
        SysTask.SysTask_ExecuteAfterVBlank(ScreenCross_StartTask, cross, VBlankTaskPriority);
    }

    // OFFSET 0x021DE2AC
    private static void ScreenCross_StartTask(SysTask task, object? param)
    {
        EncounterEffectScreenCross cross = (EncounterEffectScreenCross)param!;
        cross.VBlankTask = SysTask.SysTask_ExecuteAfterVBlank(ScreenCross_VBlankTask, cross, VBlankTaskPriority);
        cross.HBlankTask = HBlankSystem.HBlankSystem_StartTask(cross.HBlankSystem!, ScreenCross_HBlankCallback, cross);
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x021DE2DC
    private static void ScreenCross_VBlankTask(SysTask task, object? param)
    {
        EncounterEffectScreenCross cross = (EncounterEffectScreenCross)param!;
        switch (cross.State)
        {
            case 0:
            {
                QuadraticInterpolationTaskFX32_Update(cross.Y);
                if (QuadraticInterpolationTaskFX32_Update(cross.X) != 0)
                {
                    cross.State++;
                }
                int x = cross.X.X >> 12;
                int y = cross.Y.X >> 12;
                Video.SetWindowHorizontal(DisplayMain, 0, 0, (byte)(ScreenRight - x));
                Video.SetWindowVertical(DisplayMain, 0, 0, (byte)(ScreenMiddleLine - y));
                Video.SetWindowHorizontal(DisplayMain, 1, (byte)x, ScreenRight);
                Video.SetWindowVertical(DisplayMain, 1, (byte)(y + ScreenMiddleLine), ScreenBottom);
                break;
            }
            case 1:
                ScreenCross_Stop(cross);
                break;
        }
    }

    // OFFSET 0x021DE344
    private static void ScreenCross_HBlankCallback(HBlankTask task, object? param)
    {
        EncounterEffectScreenCross cross = (EncounterEffectScreenCross)param!;
        if (Video.VCount <= cross.SplitLine)
        {
            Video.SetVisibleWindows(DisplayMain, Window0Only);
        }
        else
        {
            Video.SetVisibleWindows(DisplayMain, Window1Only);
        }
    }

    // OFFSET 0x021DE374
    private static void ScreenCross_Stop(EncounterEffectScreenCross cross)
    {
        Video.SetWindowInside(DisplayMain, 0, WindowAllLayers);
        Video.SetWindowOutside(DisplayMain, 0);
        Video.SetWindowHorizontal(DisplayMain, 0, 0, 0);
        Video.SetWindowVertical(DisplayMain, 0, 0, 0);
        Video.SetVisibleWindows(DisplayMain, Window0Only);
        cross.Done![0] = 1;
        HBlankSystem.HBlankTask_Delete(cross.HBlankTask!);
        cross.HBlankTask = null;
        SysTask.SysTask_Done(cross.VBlankTask!);
        cross.VBlankTask = null;
    }

    // OFFSET 0x021DE47C
    private static void SpriteManager_Init(EncounterEffectSpriteManager manager, int maxSprites, int capacity)
    {
        manager.List = SpriteUtil.SpriteList_InitRendering(maxSprites, manager.Renderer, HeapIDField);
        for (int i = 0; i < EncounterEffectSpriteManager.ResourceTypes; i++)
        {
            manager.Collections[i] = SpriteResourceCollection.SpriteResourceCollection_New(capacity, i, HeapIDField);
        }
    }

    // OFFSET 0x021DE4AC
    private static void SpriteManager_Delete(EncounterEffectSpriteManager manager)
    {
        Sprite.SpriteList_Delete(manager.List);
        for (int i = 0; i < EncounterEffectSpriteManager.ResourceTypes; i++)
        {
            SpriteResourceCollection.SpriteResourceCollection_Delete(manager.Collections[i]);
        }
    }

    // OFFSET 0x021DE4CC
    private static void SpriteResources_Load(NARC narc, EncounterEffectSpriteManager manager, EncounterEffectSpriteResources resources, int paletteMember, int paletteCount, int tilesMember, int cellMember, int animMember, int resourceID)
    {
        resources.Resources[0] = SpriteResourceCollection.SpriteResourceCollection_AddTilesFromEx(manager.Collections[0], narc, tilesMember, 0, resourceID, VRamMain, HeapIDField, 1);
        resources.Resources[1] = SpriteResourceCollection.SpriteResourceCollection_AddPaletteFrom(manager.Collections[1], narc, paletteMember, 0, resourceID, VRamMain, paletteCount, HeapIDField);
        resources.Resources[2] = SpriteResourceCollection.SpriteResourceCollection_AddFrom(manager.Collections[2], narc, cellMember, 0, resourceID, 2, HeapIDField);
        resources.Resources[3] = SpriteResourceCollection.SpriteResourceCollection_AddFrom(manager.Collections[3], narc, animMember, 0, resourceID, 3, HeapIDField);
        SpriteTransfer.SpriteTransfer_RequestCharAtEnd(resources.Resources[0]);
        SpriteResource.SpriteResource_ReleaseData(resources.Resources[0]);
        SpriteTransfer.SpriteTransfer_RequestPlttFreeSpace(resources.Resources[1]);
        SpriteUtil.SpriteResourcesHeader_Init(resources.Header, resourceID, resourceID, resourceID, resourceID, ResourceIDNone, ResourceIDNone, 0, 0, manager.Collections[0], manager.Collections[1], manager.Collections[2], manager.Collections[3], null, null);
    }

    // OFFSET 0x021DE5A4
    private static void SpriteResources_Unload(EncounterEffectSpriteManager manager, EncounterEffectSpriteResources resources)
    {
        SpriteTransfer.SpriteTransfer_ResetCharTransfer(resources.Resources[0]);
        SpriteTransfer.SpriteTransfer_ResetPlttTransfer(resources.Resources[1]);
        for (int i = 0; i < EncounterEffectSpriteManager.ResourceTypes; i++)
        {
            SpriteResourceCollection.SpriteResourceCollection_Remove(manager.Collections[i], resources.Resources[i]!);
        }
    }

    // OFFSET 0x021DE62C
    private static Sprite SpriteManager_AddSprite(EncounterEffectSpriteManager manager, EncounterEffectSpriteResources resources, int x, int y, int z, int priority)
    {
        SpriteListTemplate template = new();
        template.List = manager.List;
        template.ResourceData = resources.Header;
        template.Position.X = x;
        template.Position.Y = y;
        template.Position.Z = z;
        template.Priority = priority;
        template.VramType = VRamMain;
        template.HeapID = HeapIDField;
        Sprite? sprite = Sprite.SpriteList_Add(template);
        if (sprite == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return sprite!;
    }

    // OFFSET 0x021DE660
    private static VecFx32 VecFx32_Make(int x, int y, int z)
    {
        VecFx32 vector = default;
        vector.X = x;
        vector.Y = y;
        vector.Z = z;
        return vector;
    }

    private sealed class TrainerGrassLowerLevelData
    {
        public readonly QuadraticInterpolationTask Scale = new();
        public readonly LinearInterpolationTask Rotation = new();
        public readonly QuadraticInterpolationTask Slide = new();
        public EncounterEffectScreenSlice? Slice;
        public readonly EncounterEffectSpriteManager SpriteManager = new();
        public readonly EncounterEffectSpriteResources SpriteResources = new();
        public readonly Sprite?[] Sprites = new Sprite?[2];
        public Camera? Camera;
        public readonly QuadraticInterpolationTask CameraDistance = new();
    }

    private sealed class GrassEffectData
    {
        public Camera? Camera;
        public readonly QuadraticInterpolationTask CameraDistance = new();
        public EncounterEffectScreenSlice? Slice;
    }

    private static void EncounterEffect_Grass(SysTask task, EncounterEffect effect, int flashBrightness, int pixelsPerSlice, int firstEnd, int zoomOutDistance, int zoomOutSpeed)
    {
        GrassEffectData data = (GrassEffectData)effect.Param!;
        switch (effect.State)
        {
            case 0:
                data = new GrassEffectData();
                effect.Param = data;
                data.Slice = ScreenSlice_New();
                effect.State++;
                break;
            case 1:
                EncounterEffect_Flash(ScreenMain, flashBrightness, -16, effect.FlashDone, 2);
                effect.State++;
                break;
            case 2:
            {
                if (effect.FlashDone[0] == 0)
                {
                    break;
                }
                effect.FlashDone[0] = 0;
                effect.State++;
                ScreenSlice_Start(effect, data.Slice!, pixelsPerSlice, 7, 0, firstEnd, -0xC000);
                data.Camera = effect.FieldSystem.Camera;
                int distance = Camera.Camera_GetDistance(data.Camera!);
                QuadraticInterpolationTaskFX32_Init(data.CameraDistance, distance, distance + 0x32000, 0x1E000, 6);
                break;
            }
            case 3:
            {
                int done = QuadraticInterpolationTaskFX32_Update(data.CameraDistance);
                Camera.Camera_SetDistance(data.CameraDistance.X, data.Camera!);
                if (done != 1)
                {
                    break;
                }
                effect.State++;
                ScreenSlice_Restart(effect, data.Slice!, pixelsPerSlice, 6, firstEnd, 0xFF000, 0x1E000);
                data.Camera = effect.FieldSystem.Camera;
                int distance = Camera.Camera_GetDistance(data.Camera!);
                QuadraticInterpolationTaskFX32_Init(data.CameraDistance, distance, distance - zoomOutDistance, zoomOutSpeed, 6);
                break;
            }
            case 4:
            {
                int done = QuadraticInterpolationTaskFX32_Update(data.CameraDistance);
                Camera.Camera_SetDistance(data.CameraDistance.X, data.Camera!);
                if (done == 1 && EncounterEffect_GetEffectDone(effect) == 1)
                {
                    effect.State++;
                }
                break;
            }
            case 5:
                ScreenFadeControl.SetColorBrightness(0);
                for (int layer = 0; layer < BgLayers; layer++)
                {
                    Video.SetBgOffset(layer, 0);
                }
                if (effect.Done != null)
                {
                    effect.Done[0] = 1;
                }
                ScreenSlice_Delete(data.Slice!);
                EncounterEffect_Finish(effect, task);
                break;
        }
    }

    // OFFSET 0x021E2338
    private static void EncounterEffect_Grass_HigherLevel(SysTask task, object? param)
    {
        EncounterEffect_Grass(task, (EncounterEffect)param!, 16, 2, -0x3000, 0x32000, -0xFF000);
    }

    // OFFSET 0x021E24A8
    private static void EncounterEffect_Grass_LowerLevel(SysTask task, object? param)
    {
        EncounterEffect_Grass(task, (EncounterEffect)param!, -16, 5, -0x2000, 0x1E000, -0x64000);
    }

    // OFFSET 0x021E2B54
    private static void EncounterEffect_TrainerGrassLowerLevel(SysTask task, object? param)
    {
        EncounterEffect effect = (EncounterEffect)param!;
        TrainerGrassLowerLevelData data = (TrainerGrassLowerLevelData)effect.Param!;
        switch (effect.State)
        {
            case 0:
                data = new TrainerGrassLowerLevelData();
                effect.Param = data;
                data.Camera = effect.FieldSystem.Camera;
                data.Slice = ScreenSlice_New();
                SpriteManager_Init(data.SpriteManager, 2, 1);
                SpriteResources_Load(effect.Narc!, data.SpriteManager, data.SpriteResources, 0, 1, 5, 7, 6, 600000);
                for (int i = 0; i < 2; i++)
                {
                    data.Sprites[i] = SpriteManager_AddSprite(data.SpriteManager, data.SpriteResources, 128 << 12, 96 << 12, 0, 0);
                    Sprite.Sprite_SetDrawFlag(data.Sprites[i]!, 0);
                    Sprite.Sprite_SetPriority(data.Sprites[i]!, (uint)(i * 2));
                }
                GXLayers.GXLayers_EngineAToggleLayers(0x10, 1);
                effect.State++;
                break;
            case 1:
                EncounterEffect_Flash(ScreenMain, -16, -16, effect.FlashDone, 2);
                effect.State++;
                break;
            case 2:
                if (effect.FlashDone[0] != 0)
                {
                    effect.State++;
                }
                break;
            case 3:
            {
                QuadraticInterpolationTaskFX32_Init(data.Scale, 0x29, 0x1000, 2, 10);
                VecFx32 scale = VecFx32_Make(data.Scale.X, data.Scale.X, data.Scale.X);
                for (int i = 0; i < 2; i++)
                {
                    Sprite.Sprite_SetDrawFlag(data.Sprites[i]!, 1);
                    Sprite.Sprite_SetAffineOverwriteMode(data.Sprites[i]!, 2);
                    Sprite.Sprite_SetAffineScale(data.Sprites[i]!, scale);
                }
                LinearInterpolationTaskS32_Init(data.Rotation, 0, 0xFFFF, 10);
                effect.State++;
                break;
            }
            case 4:
            {
                int done = QuadraticInterpolationTaskFX32_Update(data.Scale);
                VecFx32 scale = VecFx32_Make(data.Scale.X, data.Scale.X, data.Scale.X);
                LinearInterpolationTaskS32_Update(data.Rotation);
                for (int i = 0; i < 2; i++)
                {
                    Sprite.Sprite_SetAffineScale(data.Sprites[i]!, scale);
                }
                Sprite.Sprite_SetAffineZRotation(data.Sprites[0]!, (ushort)data.Rotation.X);
                Sprite.Sprite_SetAffineZRotation(data.Sprites[1]!, (ushort)(data.Rotation.X - 0x100));
                if (done == 1)
                {
                    effect.State++;
                }
                break;
            }
            case 5:
            {
                ScreenSlice_Start(effect, data.Slice!, 96, 6, 0, 255 << 12, 10 << 12);
                Sprite.Sprite_SetAnim(data.Sprites[0]!, 1);
                Sprite.Sprite_SetAnim(data.Sprites[1]!, 2);
                QuadraticInterpolationTaskFX32_Init(data.Slide, 0, 255 << 12, 10 << 12, 6);
                int distance = Camera.Camera_GetDistance(data.Camera!);
                QuadraticInterpolationTaskFX32_Init(data.CameraDistance, distance, distance - (500 << 12), -(10 << 12), 6);
                Sprite.Sprite_SetAffineZRotation(data.Sprites[0]!, 0);
                Sprite.Sprite_SetAffineZRotation(data.Sprites[1]!, 0);
                effect.State++;
                break;
            }
            case 6:
                QuadraticInterpolationTaskFX32_Update(data.Slide);
                Sprite.Sprite_SetPosition(data.Sprites[0]!, VecFx32_Make((128 << 12) - data.Slide.X, 96 << 12, 0));
                Sprite.Sprite_SetPosition(data.Sprites[1]!, VecFx32_Make(data.Slide.X + (128 << 12), 96 << 12, 0));
                QuadraticInterpolationTaskFX32_Update(data.CameraDistance);
                Camera.Camera_SetDistance(data.CameraDistance.X, data.Camera!);
                if (EncounterEffect_GetEffectDone(effect) != 0)
                {
                    effect.State++;
                }
                break;
            case 7:
                ScreenFadeControl.SetColorBrightness(0);
                if (effect.Done != null)
                {
                    effect.Done[0] = 1;
                }
                for (int i = 0; i < 2; i++)
                {
                    Sprite.Sprite_Delete(data.Sprites[i]!);
                }
                SpriteResources_Unload(data.SpriteManager, data.SpriteResources);
                SpriteManager_Delete(data.SpriteManager);
                ScreenSlice_Delete(data.Slice!);
                EncounterEffect_Finish(effect, task);
                return;
        }
        Sprite.SpriteList_Update(data.SpriteManager.List);
    }

    private sealed class TrainerGrassHigherLevelData
    {
        public readonly LinearInterpolationTask Slide = new();
        public readonly LinearInterpolationTask Rotation = new();
        public EncounterEffectScreenCross? Cross;
        public readonly EncounterEffectSpriteManager SpriteManager = new();
        public readonly EncounterEffectSpriteResources SpriteResources = new();
        public readonly Sprite?[] Sprites = new Sprite?[2];
        public Camera? Camera;
        public readonly QuadraticInterpolationTask CameraDistance = new();
    }

    private static void TrainerGrassHigherLevel_SetPositions(TrainerGrassHigherLevelData data)
    {
        Sprite.Sprite_SetPosition(data.Sprites[0]!, VecFx32_Make((128 << 12) - data.Slide.X, 64 << 12, 0));
        Sprite.Sprite_SetPosition(data.Sprites[1]!, VecFx32_Make(data.Slide.X + (128 << 12), 128 << 12, 0));
    }

    // OFFSET 0x021E2EB0
    private static void EncounterEffect_TrainerGrassHigherLevel(SysTask task, object? param)
    {
        EncounterEffect effect = (EncounterEffect)param!;
        TrainerGrassHigherLevelData data = (TrainerGrassHigherLevelData)effect.Param!;
        switch (effect.State)
        {
            case 0:
                data = new TrainerGrassHigherLevelData();
                effect.Param = data;
                data.Camera = effect.FieldSystem.Camera;
                data.Cross = ScreenCross_New();
                SpriteManager_Init(data.SpriteManager, 2, 1);
                SpriteResources_Load(effect.Narc!, data.SpriteManager, data.SpriteResources, 0, 1, 2, 4, 3, 600000);
                for (int i = 0; i < 2; i++)
                {
                    data.Sprites[i] = SpriteManager_AddSprite(data.SpriteManager, data.SpriteResources, 128 << 12, 0, 0, 0);
                    Sprite.Sprite_SetDrawFlag(data.Sprites[i]!, 0);
                    Sprite.Sprite_SetAffineOverwriteMode(data.Sprites[i]!, 2);
                }
                GXLayers.GXLayers_EngineAToggleLayers(0x10, 1);
                effect.State++;
                break;
            case 1:
                EncounterEffect_Flash(ScreenMain, 16, -16, effect.FlashDone, 2);
                effect.State++;
                break;
            case 2:
                if (effect.FlashDone[0] != 0)
                {
                    effect.State++;
                }
                break;
            case 3:
                LinearInterpolationTaskFX32_Init(data.Slide, -(192 << 12), 192 << 12, 8);
                Sprite.Sprite_SetDrawFlag(data.Sprites[0]!, 1);
                Sprite.Sprite_SetDrawFlag(data.Sprites[1]!, 1);
                TrainerGrassHigherLevel_SetPositions(data);
                LinearInterpolationTaskS32_Init(data.Rotation, 0, 0x1FFFE, 8);
                effect.State++;
                break;
            case 4:
            {
                int done = LinearInterpolationTaskFX32_Update(data.Slide);
                TrainerGrassHigherLevel_SetPositions(data);
                LinearInterpolationTaskS32_Update(data.Rotation);
                Sprite.Sprite_SetAffineZRotation(data.Sprites[0]!, (ushort)data.Rotation.X);
                Sprite.Sprite_SetAffineZRotation(data.Sprites[1]!, (ushort)(-data.Rotation.X));
                if (done == 1)
                {
                    effect.State++;
                }
                break;
            }
            case 5:
            {
                ScreenCross_Start(effect, data.Cross!, 8, 1 << 12, 1 << 12);
                int distance = Camera.Camera_GetDistance(data.Camera!);
                QuadraticInterpolationTaskFX32_Init(data.CameraDistance, distance, distance - (500 << 12), -(10 << 12), 8);
                effect.State++;
                break;
            }
            case 6:
                QuadraticInterpolationTaskFX32_Update(data.CameraDistance);
                Camera.Camera_SetDistance(data.CameraDistance.X, data.Camera!);
                if (EncounterEffect_GetEffectDone(effect) == 1)
                {
                    effect.State++;
                }
                break;
            case 7:
                ScreenFadeControl.SetColorBrightness(0);
                if (effect.Done != null)
                {
                    effect.Done[0] = 1;
                }
                for (int i = 0; i < 2; i++)
                {
                    Sprite.Sprite_Delete(data.Sprites[i]!);
                }
                SpriteResources_Unload(data.SpriteManager, data.SpriteResources);
                SpriteManager_Delete(data.SpriteManager);
                ScreenCross_Delete(data.Cross!);
                EncounterEffect_Finish(effect, task);
                return;
        }
        Sprite.SpriteList_Update(data.SpriteManager.List);
    }
}
