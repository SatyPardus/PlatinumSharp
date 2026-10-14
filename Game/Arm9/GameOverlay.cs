using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class LoadedOverlay
{
    public int Id;
    public int IsActive;
}

public static class GameOverlay
{
    public const int LoadNormal = 0;
    public const int LoadNoInit = 1;
    public const int LoadNoInitAsync = 2;

    private const int RegionMain = 0;
    private const int RegionItcm = 1;
    private const int RegionDtcm = 2;
    private const int SlotsPerRegion = 8;

    private static readonly LoadedOverlay[] sLoadedMain = CreateSlots();
    private static readonly LoadedOverlay[] sLoadedItcm = CreateSlots();
    private static readonly LoadedOverlay[] sLoadedDtcm = CreateSlots();

    private static LoadedOverlay[] CreateSlots()
    {
        LoadedOverlay[] slots = new LoadedOverlay[SlotsPerRegion];
        for (int i = 0; i < SlotsPerRegion; i++)
        {
            slots[i] = new LoadedOverlay();
        }
        return slots;
    }

    // OFFSET 0x020064F0
    private static void FreeOverlayAllocation(LoadedOverlay loadedOverlay)
    {
        if (loadedOverlay.IsActive != 1)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (!OverlaySystem.Unload(loadedOverlay.Id))
        {
            ErrorHandling.GF_AssertFail();
        }
        loadedOverlay.IsActive = 0;
    }

    // OFFSET 0x02006514
    public static void Overlay_UnloadByID(int overlayID)
    {
        LoadedOverlay[] table = GetLoadedOverlaysInRegion(Overlay_GetLoadDestination(overlayID));
        for (int i = 0; i < SlotsPerRegion; i++)
        {
            if (table[i].IsActive == 1 && table[i].Id == overlayID)
            {
                FreeOverlayAllocation(table[i]);
                return;
            }
        }
    }

    // OFFSET 0x02006544
    public static int Overlay_GetLoadDestination(int overlayID)
    {
        if (!OverlaySystem.LoadInfo(out OverlayInfo info, overlayID))
        {
            ErrorHandling.GF_AssertFail();
        }
        uint address = info.RamAddress;
        if (address <= 0x02000000 && address >= 0x01000000)
        {
            return RegionItcm;
        }
        if (address <= 0x027E4000 && address >= 0x027E0000)
        {
            return RegionDtcm;
        }
        return RegionMain;
    }

    // OFFSET 0x02006590
    public static bool Overlay_LoadByID(int overlayID, int loadType)
    {
        uint savedDma = 0xFFFFFFFF;
        if (!CanOverlayBeLoaded(overlayID))
        {
            return false;
        }
        int region = Overlay_GetLoadDestination(overlayID);
        LoadedOverlay[] table = GetLoadedOverlaysInRegion(region);
        int slot;
        for (slot = 0; slot < SlotsPerRegion; slot++)
        {
            if (table[slot].IsActive == 0)
            {
                table[slot].IsActive = 1;
                table[slot].Id = overlayID;
                break;
            }
        }
        if (slot >= SlotsPerRegion)
        {
            ErrorHandling.GF_AssertFail();
            return false;
        }
        if (region == RegionItcm || region == RegionDtcm)
        {
            savedDma = OverlaySystem.SetDefaultDma(0xFFFFFFFF);
        }
        bool result;
        switch (loadType)
        {
            case LoadNormal:
                result = LoadOverlayNormal(0, overlayID);
                break;
            case LoadNoInit:
                result = LoadOverlayNoInit(0, overlayID);
                break;
            case LoadNoInitAsync:
                result = LoadOverlayNoInitAsync(0, overlayID);
                break;
            default:
                ErrorHandling.GF_AssertFail();
                return false;
        }
        if (region == RegionItcm || region == RegionDtcm)
        {
            OverlaySystem.SetDefaultDma(savedDma);
        }
        if (!result)
        {
            ErrorHandling.GF_AssertFail();
            return false;
        }
        return true;
    }

    // OFFSET 0x02006644
    private static bool CanOverlayBeLoaded(int overlayID)
    {
        if (!GetOverlayRamBounds(overlayID, out uint start, out uint end))
        {
            return false;
        }
        LoadedOverlay[] table = GetLoadedOverlaysInRegion(Overlay_GetLoadDestination(overlayID));
        for (int i = 0; i < SlotsPerRegion; i++)
        {
            if (table[i].IsActive != 1)
            {
                continue;
            }
            if (!GetOverlayRamBounds(table[i].Id, out uint otherStart, out uint otherEnd))
            {
                continue;
            }
            if ((start >= otherStart && start < otherEnd)
                || (end > otherStart && end <= otherEnd)
                || (start <= otherStart && end >= otherEnd))
            {
                ErrorHandling.GF_AssertFail();
                return false;
            }
        }
        return true;
    }

    // OFFSET 0x020066C0
    private static LoadedOverlay[] GetLoadedOverlaysInRegion(int region)
    {
        switch (region)
        {
            case RegionItcm:
                return sLoadedItcm;
            case RegionDtcm:
                return sLoadedDtcm;
            default:
                return sLoadedMain;
        }
    }

    // OFFSET 0x020066E4
    private static bool GetOverlayRamBounds(int overlayID, out uint start, out uint end)
    {
        if (!OverlaySystem.LoadInfo(out OverlayInfo info, overlayID))
        {
            ErrorHandling.GF_AssertFail();
            start = 0;
            end = 0;
            return false;
        }
        start = info.RamAddress;
        end = start + (info.RamSize + info.BssSize);
        return true;
    }

    // OFFSET 0x0200671C
    private static bool LoadOverlayNormal(int processor, int overlayID)
    {
        return OverlaySystem.Load(overlayID);
    }

    // OFFSET 0x02006724
    private static bool LoadOverlayNoInit(int processor, int overlayID)
    {
        if (!OverlaySystem.LoadInfo(out OverlayInfo info, overlayID))
        {
            return false;
        }
        if (!OverlaySystem.LoadImage(info))
        {
            return false;
        }
        OverlaySystem.Start(info);
        return true;
    }

    // OFFSET 0x0200675C
    private static bool LoadOverlayNoInitAsync(int processor, int overlayID)
    {
        if (!OverlaySystem.LoadInfo(out OverlayInfo info, overlayID))
        {
            return false;
        }
        OverlaySystem.LoadImage(info);
        OverlaySystem.Start(info);
        return true;
    }
}
