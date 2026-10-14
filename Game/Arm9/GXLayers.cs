using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public sealed class GXBanks
{
    public uint Bg;
    public uint BgExtPltt;
    public uint SubBg;
    public uint SubBgExtPltt;
    public uint Obj;
    public uint ObjExtPltt;
    public uint SubObj;
    public uint SubObjExtPltt;
    public uint Tex;
    public uint TexPltt;
}

public static class GXLayers
{
    private const int DisplayMain = 0;
    private const int DisplaySub = 1;
    private const uint LayerMask = 0xFFFFE0FF;
    private const uint DisplayOn = 0x10000;
    private const ushort PowerMainOnTop = 0x8000;

    private static int sSubLayers;
    private static int sMainLayers;

    private static void ApplyLayers(int display, int layers)
    {
        Video.SetDisplayControl(display, (Video.GetDisplayControl(display) & LayerMask) | ((uint)layers << 8));
    }

    // OFFSET 0x0201FE94
    public static void GXLayers_SetBanks(GXBanks banks)
    {
        Video.ResetBanks();
        Video.SetBanks(banks.Bg, banks.BgExtPltt, banks.SubBg, banks.SubBgExtPltt, banks.Obj, banks.ObjExtPltt, banks.SubObj, banks.SubObjExtPltt, banks.Tex, banks.TexPltt);
    }

    // OFFSET 0x0201FF00
    public static void GXLayers_DisableEngineALayers()
    {
        sMainLayers = 0;
    }

    // OFFSET 0x0201FF0C
    public static void GXLayers_EngineAToggleLayers(int layerMask, int enable)
    {
        if (enable == 1)
        {
            if ((sMainLayers & layerMask) != 0)
            {
                return;
            }
        }
        else
        {
            if ((sMainLayers & layerMask) == 0)
            {
                return;
            }
        }
        sMainLayers ^= layerMask;
        ApplyLayers(DisplayMain, sMainLayers);
    }

    // OFFSET 0x0201FF48
    public static void GXLayers_EngineASetLayers(int layers)
    {
        sMainLayers = layers;
        ApplyLayers(DisplayMain, sMainLayers);
    }

    // OFFSET 0x0201FF68
    public static void GXLayers_DisableEngineBLayers()
    {
        sSubLayers = 0;
    }

    // OFFSET 0x0201FF74
    public static void GXLayers_EngineBToggleLayers(int layerMask, int enable)
    {
        if (enable == 1)
        {
            if ((sSubLayers & layerMask) != 0)
            {
                return;
            }
        }
        else
        {
            if ((sSubLayers & layerMask) == 0)
            {
                return;
            }
        }
        sSubLayers ^= layerMask;
        ApplyLayers(DisplaySub, sSubLayers);
    }

    // OFFSET 0x0201FFB0
    public static void GXLayers_EngineBSetLayers(int layers)
    {
        sSubLayers = layers;
        ApplyLayers(DisplaySub, sSubLayers);
    }

    // OFFSET 0x0201FFD0
    public static void GXLayers_TurnBothDispOn()
    {
        Video.SetDisplayControl(DisplayMain, Video.GetDisplayControl(DisplayMain) | DisplayOn);
        Video.SetDisplayControl(DisplaySub, Video.GetDisplayControl(DisplaySub) | DisplayOn);
    }

    // OFFSET 0x0201FFE8
    public static void GXLayers_SwapDisplay()
    {
        if (gSystem.WhichScreenIs3D == 0)
        {
            Video.PowerControl |= PowerMainOnTop;
        }
        else
        {
            Video.PowerControl = (ushort)(Video.PowerControl & ~PowerMainOnTop);
        }
    }

    // OFFSET 0x02020014
    public static int GXLayers_EngineAGetLayers()
    {
        return sMainLayers;
    }
}
