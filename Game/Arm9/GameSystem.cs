using System.Security.Cryptography;
using static PlatinumSharp.Game.Globals;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class SystemCacheEntry
{
    public object? Data;
    public uint Hash;
}

public sealed class HeapCanary
{
    public uint Value;
}

public static class GameSystem
{
    public const int ButtonModeNormal = 0;
    public const int ButtonModeStartIsX = 1;
    public const int ButtonModeSwapXY = 2;
    public const int ButtonModeLIsA = 3;

    private const int MainTaskCount = 0xA0;
    private const int VBlankTaskCount = 0x20;
    private const int PostVBlankTaskCount = 0x20;
    private const int PrintTaskCount = 4;
    private const int HeapTemplateCount = 4;
    private const int TotalHeapCount = 0x7B;
    private const uint HeapTemplates = 0x020E5674;
    private const int DigestBytes = 16;
    private const int SystemCacheSize = 0x80;
    private const int DefaultAutorepeatRate = 4;
    private const int DefaultAutorepeatDelay = 8;
    private const uint HeapCanaryValue = 0x2F93A1BC;
    private const int DisplayMain = 0;
    private const int DisplaySub = 1;
    private const uint DisplayModeMask = 0xFFFCFFFF;
    private const uint SubDisplayOnMask = 0xFFFEFFFF;
    private const ushort PowerAllMask = 0xFDF1;
    private const ushort PowerAll = 0x020E;
    private const ushort PowerMainOnTop = 0x8000;
    private const int SwappedButtonsMask = 0xF3FF;
    private const int ShoulderButtonsMask = 0xFCFF;

    private static readonly SystemCacheEntry[] sSystemCache = CreateSystemCache();

    private static SystemCacheEntry[] CreateSystemCache()
    {
        SystemCacheEntry[] cache = new SystemCacheEntry[SystemCacheSize];
        for (int i = 0; i < SystemCacheSize; i++)
        {
            cache[i] = new SystemCacheEntry();
        }
        return cache;
    }

    // OFFSET 0x02017728
    private static void VBlankIntr()
    {
        SysTaskManager.SysTaskManager_ExecuteTasks(gSystem.VBlankTaskManager!);
        gSystem.VBlankIntrCounter++;
    }

    // OFFSET 0x0201775C
    private static void DummyVBlankIntr()
    {
    }

    // OFFSET 0x0201777C
    public static void SetDummyVBlankIntr()
    {
        Interrupts.Disable(Interrupts.VBlank);
        Interrupts.SetHandler(Interrupts.VBlank, DummyVBlankIntr);
        Interrupts.Enable(Interrupts.VBlank);
    }

    // OFFSET 0x02017798
    public static void SetVBlankCallback(Action<object?>? callback, object? data)
    {
        gSystem.VBlankCallback = callback;
        gSystem.VBlankCallbackData = data;
    }

    // OFFSET 0x020177A4
    public static void DisableHBlank()
    {
        SetHBlankEnabled(false);
        gSystem.HBlankCallback = null;
        gSystem.HBlankCallbackData = null;
    }

    // OFFSET 0x020177BC
    public static bool SetHBlankCallback(HBlankCallback? callback, object? data)
    {
        if (callback == null)
        {
            SetHBlankEnabled(false);
            gSystem.HBlankCallback = null;
            gSystem.HBlankCallbackData = null;
            return true;
        }
        if (gSystem.HBlankCallback == null)
        {
            gSystem.HBlankCallbackData = data;
            gSystem.HBlankCallback = callback;
            SetHBlankEnabled(true);
            return true;
        }
        return false;
    }

    // OFFSET 0x020177F4
    private static void HBlankIntr()
    {
        if (gSystem.HBlankCallback != null)
        {
            gSystem.HBlankCallback(gSystem.HBlankCallbackData);
        }
    }

    // OFFSET 0x02017808
    private static void SetHBlankEnabled(bool enabled)
    {
        Interrupts.MasterEnable = false;
        if (!enabled)
        {
            Interrupts.Disable(Interrupts.HBlank);
            Video.HBlankInterruptEnabled = false;
        }
        else
        {
            Interrupts.SetHandler(Interrupts.HBlank, HBlankIntr);
            Interrupts.Enable(Interrupts.HBlank);
            Video.HBlankInterruptEnabled = true;
        }
        Interrupts.MasterEnable = true;
    }

    // OFFSET 0x02017850
    private static void InitHeapSystem()
    {
        byte[] digest = MD5.HashData(Platform.GetLowEntropyData());
        uint preSize = 0;
        for (uint i = 0; i < DigestBytes; i++)
        {
            preSize += digest[i];
        }
        preSize = (byte)preSize;
        while ((preSize & 3) != 0)
        {
            preSize++;
        }
        Heap.Heap_InitSystem(HeapTemplates, HeapTemplateCount, TotalHeapCount, preSize);
    }

    // OFFSET 0x0201789C
    public static void InitSystem()
    {
        Platform.InitHardware();
        Video.PowerControl = (ushort)((Video.PowerControl & PowerAllMask) | PowerAll);
        InitHeapSystem();
        gSystem.MainTaskManager = SysTaskManager.SysTaskManager_Init(MainTaskCount, new SysTaskManager());
        gSystem.VBlankTaskManager = SysTaskManager.SysTaskManager_Init(VBlankTaskCount, new SysTaskManager());
        gSystem.PostVBlankTaskManager = SysTaskManager.SysTaskManager_Init(PostVBlankTaskCount, new SysTaskManager());
        gSystem.PrintTaskManager = SysTaskManager.SysTaskManager_Init(PrintTaskCount, new SysTaskManager());
        Video.SetDisplayControl(DisplayMain, Video.GetDisplayControl(DisplayMain) & DisplayModeMask);
        Video.SetDisplayControl(DisplaySub, Video.GetDisplayControl(DisplaySub) & SubDisplayOnMask);
        Video.PowerControl |= PowerMainOnTop;
        Interrupts.SetHandler(Interrupts.VBlank, VBlankIntr);
        Interrupts.Enable(Interrupts.VBlank);
        Interrupts.Enable(Interrupts.FifoReceive);
        Interrupts.MasterEnable = true;
        Video.VBlankInterruptEnabled = true;
        Boot.CheckForMemoryTampering();
        gSystem.VBlankCallback = null;
        gSystem.HBlankCallback = null;
        gSystem.Unused10 = null;
        gSystem.Unused14 = null;
        gSystem.HeapCanary = null;
        gSystem.FrameCounter = 0;
        gSystem.WhichScreenIs3D = 0;
        MathUtil.InitCRC16Table(0);
    }

    // OFFSET 0x020179E4
    public static void InitVRAM()
    {
        Video.ClearVideoMemory();
        Video.ResetObjectAttributes(DisplayMain);
        Video.ResetObjectAttributes(DisplaySub);
        Video.ClearStandardPalettes(DisplayMain);
        Video.ClearStandardPalettes(DisplaySub);
    }

    // OFFSET 0x02017A40
    public static byte[]? ReadFileToHeap(int heapID, string filename)
    {
        FsFile file = new();
        byte[]? buffer;
        FileSystem.InitFile(file);
        if (FileSystem.OpenFile(file, filename))
        {
            int size = FileSystem.GetLength(file);
            buffer = new byte[size];
            if (FileSystem.ReadFile(file, buffer, 0, size) != size)
            {
                buffer = null;
            }
            FileSystem.CloseFile(file);
        }
        else
        {
            buffer = null;
        }
        return buffer;
    }

    // OFFSET 0x02017A94
    public static void ReadFileToBuffer(string filename, byte[]? buffer)
    {
        FsFile file = new();
        FileSystem.InitFile(file);
        if (FileSystem.OpenFile(file, filename))
        {
            int size = FileSystem.GetLength(file);
            if (buffer != null)
            {
                FileSystem.ReadFile(file, buffer, 0, size);
            }
            FileSystem.CloseFile(file);
        }
    }

    // OFFSET 0x02017ACC
    public static void ClearUnusedSystemCache()
    {
        for (int i = SystemCacheSize - 1; i > -1; i--)
        {
            if (sSystemCache[i].Data != null)
            {
                sSystemCache[i].Data = null;
                sSystemCache[i].Hash = 0;
            }
        }
    }

    // OFFSET 0x02017AF4
    public static void InitKeypadAndTouchpad()
    {
        gSystem.ButtonMode = ButtonModeNormal;
        gSystem.HeldKeysRaw = 0;
        gSystem.PressedKeysRaw = 0;
        gSystem.PressedKeysRepeatableRaw = 0;
        gSystem.HeldKeys = 0;
        gSystem.PressedKeys = 0;
        gSystem.PressedKeysRepeatable = 0;
        gSystem.AutorepeatTimer = 0;
        gSystem.AutorepeatRate = DefaultAutorepeatRate;
        gSystem.AutorepeatDelay = DefaultAutorepeatDelay;
        gSystem.TouchX = 0;
        gSystem.TouchY = 0;
        gSystem.TouchPressed = 0;
        gSystem.TouchHeld = 0;
        gSystem.TouchAutoSampling = 0;
        Input.InitTouch();
    }

    // OFFSET 0x02017B70
    public static void SetGBACartridgeVersion(int version)
    {
        gSystem.GbaCartridgeVersion = (byte)version;
    }

    // OFFSET 0x02017B7C
    public static void SleepLock(byte mask)
    {
        gSystem.InhibitSleep |= mask;
    }

    // OFFSET 0x02017B8C
    public static void SleepUnlock(byte mask)
    {
        gSystem.InhibitSleep &= (byte)~mask;
    }

    // OFFSET 0x02017B9C
    public static void ReadKeypadAndTouchpad()
    {
        if (Platform.IsFolded)
        {
            gSystem.PressedKeys = 0;
            gSystem.HeldKeys = 0;
            gSystem.PressedKeysRepeatable = 0;
            gSystem.TouchPressed = 0;
            gSystem.TouchHeld = 0;
            return;
        }
        int raw = (ushort)(Input.Keys & Input.KeyMask);
        gSystem.PressedKeysRaw = raw & (raw ^ gSystem.HeldKeysRaw);
        gSystem.PressedKeysRepeatableRaw = raw & (raw ^ gSystem.HeldKeysRaw);
        if (raw != 0 && gSystem.HeldKeysRaw == raw)
        {
            if (--gSystem.AutorepeatTimer == 0)
            {
                gSystem.PressedKeysRepeatableRaw = raw;
                gSystem.AutorepeatTimer = gSystem.AutorepeatRate;
            }
        }
        else
        {
            gSystem.AutorepeatTimer = gSystem.AutorepeatDelay;
        }
        gSystem.HeldKeysRaw = raw;
        gSystem.PressedKeys = gSystem.PressedKeysRaw;
        gSystem.HeldKeys = raw;
        gSystem.PressedKeysRepeatable = gSystem.PressedKeysRepeatableRaw;
        ApplyButtonMode();
        Input.GetCalibratedPoint(out ushort x, out ushort y, out ushort touch, out ushort validity);
        if (validity == Input.TouchValid)
        {
            gSystem.TouchX = x;
            gSystem.TouchY = y;
        }
        else if (gSystem.TouchHeld != 0)
        {
            switch (validity)
            {
                case Input.TouchInvalidX:
                    gSystem.TouchY = y;
                    break;
                case Input.TouchInvalidY:
                    gSystem.TouchX = x;
                    break;
                case Input.TouchInvalidXY:
                    break;
            }
        }
        else
        {
            touch = 0;
        }
        gSystem.TouchPressed = (ushort)(touch & (touch ^ gSystem.TouchHeld));
        gSystem.TouchHeld = touch;
    }

    private static int MapStartToX(int keys)
    {
        if ((keys & Input.KeyStart) != 0)
        {
            keys |= Input.KeyX;
        }
        return keys;
    }

    private static int SwapXY(int keys)
    {
        int swapped = 0;
        if ((keys & Input.KeyX) != 0)
        {
            swapped |= Input.KeyY;
        }
        if ((keys & Input.KeyY) != 0)
        {
            swapped |= Input.KeyX;
        }
        return (keys & SwappedButtonsMask) | swapped;
    }

    private static int MapLToA(int keys)
    {
        if ((keys & Input.KeyL) != 0)
        {
            keys |= Input.KeyA;
        }
        return keys;
    }

    // OFFSET 0x02017CA0
    private static void ApplyButtonMode()
    {
        switch (gSystem.ButtonMode)
        {
            case ButtonModeNormal:
                break;
            case ButtonModeStartIsX:
                gSystem.PressedKeys = MapStartToX(gSystem.PressedKeys);
                gSystem.HeldKeys = MapStartToX(gSystem.HeldKeys);
                gSystem.PressedKeysRepeatable = MapStartToX(gSystem.PressedKeysRepeatable);
                break;
            case ButtonModeSwapXY:
                gSystem.PressedKeys = SwapXY(gSystem.PressedKeys);
                gSystem.HeldKeys = SwapXY(gSystem.HeldKeys);
                gSystem.PressedKeysRepeatable = SwapXY(gSystem.PressedKeysRepeatable);
                break;
            case ButtonModeLIsA:
                gSystem.PressedKeys = MapLToA(gSystem.PressedKeys);
                gSystem.HeldKeys = MapLToA(gSystem.HeldKeys);
                gSystem.PressedKeysRepeatable = MapLToA(gSystem.PressedKeysRepeatable);
                gSystem.PressedKeys &= ShoulderButtonsMask;
                gSystem.HeldKeys &= ShoulderButtonsMask;
                gSystem.PressedKeysRepeatable &= ShoulderButtonsMask;
                break;
        }
    }

    // OFFSET 0x02017DD4
    public static void SetAutorepeat(int rate, int delay)
    {
        gSystem.AutorepeatRate = rate;
        gSystem.AutorepeatDelay = delay;
    }

    // OFFSET 0x02017DE0
    public static void ResetLock(byte mask)
    {
        gSystem.InhibitReset |= mask;
    }

    // OFFSET 0x02017DF0
    public static void ResetUnlock(byte mask)
    {
        gSystem.InhibitReset &= (byte)~mask;
    }

    // OFFSET 0x02017E00
    public static void InitHeapCanary(int heapID)
    {
        if (gSystem.HeapCanary != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        gSystem.HeapCanary = new HeapCanary();
        gSystem.HeapCanary.Value = HeapCanaryValue;
    }

    // OFFSET 0x02017E2C
    public static void FreeHeapCanary()
    {
        if (gSystem.HeapCanary == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        gSystem.HeapCanary!.Value = 0;
        gSystem.HeapCanary = null;
    }

    // OFFSET 0x02017E54
    public static bool HeapCanaryOK()
    {
        if (gSystem.HeapCanary != null && gSystem.HeapCanary.Value == HeapCanaryValue)
        {
            return true;
        }
        return false;
    }
}
