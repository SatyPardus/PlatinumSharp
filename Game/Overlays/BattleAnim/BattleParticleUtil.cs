namespace PlatinumSharp.Game;

public static partial class BattleParticleUtil
{
    private const uint PaletteTable = 0x0221FBD0;
    private const uint DefaultPalette = 0x0221F96C;
    private const int PaletteSize = 0x20;
    private const int ParticleHeapSize = 0x4800;
    private const int SmallParticleHeapSize = 0x4200;
    private const int CameraNear = 0x1000;
    private const int CameraFar = 0x384000;

    // OFFSET 0x0221F800
    public static ushort[] BattleParticleUtil_GetPalette(int id)
    {
        uint address = DefaultPalette;
        if (id != -1)
        {
            address = Nitro.Bin.U32(RomData.Overlay(11, (uint)(PaletteTable + id * 4), 4), 0);
        }
        byte[] raw = RomData.Overlay(11, address, PaletteSize);
        ushort[] colors = new ushort[PaletteSize / 2];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = (ushort)Nitro.Bin.U16(raw, i * 2);
        }
        return colors;
    }

    // OFFSET 0x0221F81C
    public static void BattleParticleUtil_LoadPalette(PaletteData paletteData, int id, int unused, int bufferID, int paletteSlot)
    {
        ushort[] colors = BattleParticleUtil_GetPalette(id);
        PaletteData.PaletteData_LoadBuffer(paletteData, colors, 0, bufferID, (ushort)(paletteSlot << 4), PaletteSize);
    }

    // OFFSET 0x0221F840
    public static ParticleSystem? BattleParticleUtil_CreateParticleSystem(int heapID)
    {
        byte[] heap = new byte[ParticleHeapSize];
        ParticleSystem? particleSystem = ParticleSystem.ParticleSystem_New(BattleParticleUtil_AllocTexture, BattleParticleUtil_AllocPalette, heap, ParticleHeapSize, 1, heapID);
        Camera? camera = ParticleSystem.ParticleSystem_GetCamera(particleSystem!);
        if (camera != null)
        {
            Camera.Camera_SetClipping(CameraNear, CameraFar, camera);
        }
        return particleSystem;
    }

    // OFFSET 0x0221F888
    public static ParticleSystem? BattleParticleUtil_CreateSmallParticleSystem(int heapID)
    {
        byte[] heap = new byte[SmallParticleHeapSize];
        ParticleSystem? particleSystem = ParticleSystem.ParticleSystem_New(BattleParticleUtil_AllocTexture, BattleParticleUtil_AllocPalette, heap, SmallParticleHeapSize, 1, heapID);
        Camera? camera = ParticleSystem.ParticleSystem_GetCamera(particleSystem!);
        if (camera != null)
        {
            Camera.Camera_SetClipping(CameraNear, CameraFar, camera);
        }
        return particleSystem;
    }

    // OFFSET 0x0221F8D8
    public static void BattleParticleUtil_FreeParticleSystem(ParticleSystem particleSystem)
    {
        ParticleSystem.ParticleSystem_GetHeapStart(particleSystem);
        ParticleSystem.ParticleSystem_Free(particleSystem);
    }

    // OFFSET 0x0221F8F0
    public static void BattleParticleUtil_UpdateAndDraw()
    {
        G3SwapRequest.G3_ResetG3X();
        if (ParticleSystem.ParticleSystem_GetActiveAmount() == 0)
        {
            return;
        }
        if (ParticleSystem.ParticleSystem_DrawAll() > 0)
        {
            G3SwapRequest.G3_ResetG3X();
            Render3D.SetupSoftwareSpriteCamera();
        }
        ParticleSystem.ParticleSystem_UpdateAll();
    }

    // OFFSET 0x0221F914
    private static uint BattleParticleUtil_AllocTexture(uint size, int is4x4Comp)
    {
        uint key = Gfd.AllocTexVram(size, is4x4Comp, 0);
        ParticleSystem.ParticleSystem_RegisterTextureKey(key);
        return (key & 0xFFFF) << 3;
    }

    // OFFSET 0x0221F930
    private static uint BattleParticleUtil_AllocPalette(uint size, int is4Pal)
    {
        uint key = Gfd.AllocPlttVram(size, is4Pal, 0);
        ParticleSystem.ParticleSystem_RegisterPaletteKey(key);
        return (key & 0xFFFF) << 3;
    }
}
