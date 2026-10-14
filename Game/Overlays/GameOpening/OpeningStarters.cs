using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate bool OpeningStarterAnimFunc(OpeningStarters starters, int index);

public sealed class OpeningStarters
{
    public const int Size = 0x2C;
    public const int StarterCount = 3;

    private const int HeapID = 0x4C;
    private const int Overlay = 77;
    private const uint SpeciesTable = 0x021D7934;
    private const uint PositionTable = 0x021D7958;
    private const uint EmitterTable = 0x021D7970;
    private const int EmittersPerStarter = 3;
    private const int ParticleHeapSize = 0x4800;
    private const int ParticleArchive = 0x3D;
    private const int ParticleMember = 4;
    private const int SpriteZ = 0x3FF;
    private const int FaceFront = 2;
    private const int AttributeX = 0;
    private const int AttributeY = 1;
    private const int AttributeHide = 6;
    private const int AttributeScaleX = 0xC;
    private const int AttributeScaleY = 0xD;
    private const int ScaleNormal = 0x100;
    private const int ScaleEnlarged = 0x400;
    private const int ScaleStep = -0x80;
    private const int AnimFrames = 6;
    private const int HoldFrames = 20;
    private const int CameraNear = 0x1000;
    private const int CameraFar = 0xE1 << 14;
    private const ushort WindowMask = 0x3F;
    private const ushort WindowEffect = 0x20;
    private const ushort WindowShowAll = 0x11;
    private const ushort WindowShowObj = 0x10;
    private const int ScreenCenterX = 0x80;
    private const int ScreenCenterY = 0x60;
    private const int WorldUnitsPerPixel = 0xAC;
    private const int EmitterDepth = 0x40;

    public PokemonSpriteManager? Manager;
    public readonly PokemonSprite?[] Sprites = new PokemonSprite?[StarterCount];
    public int Active;
    public byte[]? ParticleHeap;
    public ParticleSystem? Particles;
    public readonly int[] StarterIndices = new int[StarterCount];
    public byte State;
    public byte Timer;

    private static readonly OpeningStarterAnimFunc[] enterFuncs = { OpeningStarters_EnterFromLeft, OpeningStarters_EnterFromRightFast, OpeningStarters_EnterFromLeftSlow };
    private static readonly OpeningStarterAnimFunc[] exitFuncs = { OpeningStarters_ExitLeft, OpeningStarters_ExitLeftSlow, OpeningStarters_ExitRight };

    // OFFSET 0x021D6C70
    private static uint OpeningStarters_AllocTexture(uint size, int is4x4Comp)
    {
        uint key = Gfd.AllocTexVram(size, is4x4Comp, 0);
        ParticleSystem.ParticleSystem_RegisterTextureKey(key);
        if (key == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        return (key & 0xFFFF) << 3;
    }

    // OFFSET 0x021D6C94
    private static uint OpeningStarters_AllocPalette(uint size, int is4Pal)
    {
        uint key = Gfd.AllocPlttVram(size, is4Pal, 1);
        if (key == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        ParticleSystem.ParticleSystem_RegisterPaletteKey(key);
        return (key & 0xFFFF) << 3;
    }

    // OFFSET 0x021D6CB8
    public static int OpeningStarters_Size()
    {
        return Size;
    }

    // OFFSET 0x021D6CBC
    public static ParticleSystem OpeningStarters_GetParticleSystem(OpeningStarters? starters)
    {
        if (starters == null || starters.Particles == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return starters!.Particles!;
    }

    // OFFSET 0x021D6CD0
    public static void OpeningStarters_CreateEmitter(OpeningStarters? starters, int resourceID)
    {
        if (starters == null || starters.Particles == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        ParticleSystem.ParticleSystem_SetCameraProjection(starters!.Particles!, 1);
        ParticleSystem.ParticleSystem_CreateEmitterWithCallback(starters.Particles!, resourceID, null, null);
    }

    // OFFSET 0x021D6CFC
    public static void OpeningStarters_Init(OpeningStarters starters)
    {
        uint[] speciesTable = new uint[StarterCount];
        byte[] rawSpecies = RomData.Overlay(Overlay, SpeciesTable, StarterCount * 4);
        byte[] positions = RomData.Overlay(Overlay, PositionTable, StarterCount * 8);
        for (int i = 0; i < StarterCount; i++)
        {
            speciesTable[i] = Bin.U32(rawSpecies, i * 4);
        }
        Rtc.RTC_GetCurrentTime(out RtcTime time);
        int forward = (time.Second + time.Hour * time.Minute) & 1;
        int index = (int)((uint)time.Second % StarterCount);
        uint[] species = new uint[StarterCount];
        for (byte i = 0; i < StarterCount; i++)
        {
            species[i] = speciesTable[index];
            starters.StarterIndices[i] = index;
            if (forward != 0)
            {
                index++;
            }
            else
            {
                index--;
            }
            if (index >= StarterCount)
            {
                index = 0;
            }
            else if (index < 0)
            {
                index = StarterCount - 1;
            }
        }
        starters.Manager = PokemonSpriteManager.PokemonSpriteManager_New(HeapID);
        for (byte i = 0; i < StarterCount; i++)
        {
            PokemonSpriteTemplate template = new();
            PokemonSpriteTemplates.BuildPokemonSpriteTemplate(template, (ushort)species[i], 0, FaceFront, 0, 0, 0);
            int position = starters.StarterIndices[i] * 8;
            starters.Sprites[i] = PokemonSpriteManager.PokemonSpriteManager_CreateSprite(starters.Manager, template, (int)Bin.U32(positions, position), (int)Bin.U32(positions, position + 4), SpriteZ, i, null, null);
            PokemonSprite.PokemonSprite_SetAttribute(starters.Sprites[i]!, AttributeHide, 1);
        }
        starters.Active = 1;
        ParticleSystem.ParticleSystem_ZeroAll();
        starters.ParticleHeap = new byte[ParticleHeapSize];
        starters.Particles = ParticleSystem.ParticleSystem_New(OpeningStarters_AllocTexture, OpeningStarters_AllocPalette, starters.ParticleHeap, ParticleHeapSize, 1, HeapID);
        Camera.Camera_SetClipping(CameraNear, CameraFar, ParticleSystem.ParticleSystem_GetCamera(starters.Particles!)!);
        byte[] resource = ParticleSystem.ParticleSystem_LoadResourceFromNARC(ParticleArchive, ParticleMember, HeapID);
        ParticleSystem.ParticleSystem_SetResource(starters.Particles!, resource, 0xA, 1);
    }

    // OFFSET 0x021D6E28
    public static void OpeningStarters_Free(OpeningStarters starters)
    {
        ParticleSystem.ParticleSystem_Free(starters.Particles!);
        starters.ParticleHeap = null;
        PokemonSpriteManager.PokemonSpriteManager_Free(starters.Manager!);
    }

    // OFFSET 0x021D6E40
    public static void OpeningStarters_VBlank(OpeningStarters? starters)
    {
        if (starters != null)
        {
            PokemonSpriteManager.PokemonSpriteManager_UpdateCharAndPltt(starters.Manager!);
        }
    }

    // OFFSET 0x021D6E50
    public static void OpeningStarters_Draw(OpeningStarters? starters)
    {
        if (starters == null)
        {
            return;
        }
        Render3D.FlushBuffer();
        G3SwapRequest.G3_ResetG3X();
        Render3D.SetupSoftwareSpriteCamera();
        PokemonSpriteManager.PokemonSpriteManager_DrawSprites(starters.Manager!);
        OpeningStarters_DrawParticles();
        G3SwapRequest.G3_RequestSwapBuffers(1, 0);
    }

    // OFFSET 0x021D6E78
    public static bool OpeningStarters_Update(OpeningStarters starters, int index)
    {
        switch (starters.State)
        {
            case 0:
            case 1:
                if (starters.State == 0)
                {
                    Video.SetWindowOutside(0, (byte)((Video.GetWindowOutside(0) & ~WindowMask) | WindowShowAll | WindowEffect));
                    PokemonSprite.PokemonSprite_SetAttribute(starters.Sprites[index]!, AttributeScaleX, ScaleEnlarged);
                    PokemonSprite.PokemonSprite_SetAttribute(starters.Sprites[index]!, AttributeScaleY, ScaleEnlarged);
                    PokemonSprite.PokemonSprite_SetAttribute(starters.Sprites[index]!, AttributeHide, 0);
                    starters.State++;
                }
                if (enterFuncs[starters.StarterIndices[index]](starters, index))
                {
                    starters.Timer = 0;
                    starters.State++;
                }
                break;
            case 2:
            {
                ParticleSystem.ParticleSystem_SetCameraProjection(starters.Particles!, 1);
                byte[] emitters = RomData.Overlay(Overlay, EmitterTable, StarterCount * EmittersPerStarter * 4);
                for (int i = 0; i < EmittersPerStarter; i++)
                {
                    int resourceID = (int)Bin.U32(emitters, (starters.StarterIndices[index] * EmittersPerStarter + i) * 4);
                    ParticleSystem.ParticleSystem_CreateEmitterWithCallback(starters.Particles!, resourceID, OpeningStarters_PlaceEmitter, starters.Sprites[index]);
                }
                starters.State++;
                break;
            }
            case 3:
                if (ParticleSystem.ParticleSystem_GetActiveEmitterCount(starters.Particles!) == 0)
                {
                    Video.SetWindowOutside(0, (byte)((Video.GetWindowOutside(0) & ~WindowMask) | WindowShowObj | WindowEffect));
                    starters.State++;
                }
                break;
            case 4:
                if (exitFuncs[starters.StarterIndices[index]](starters, index))
                {
                    PokemonSprite.PokemonSprite_SetAttribute(starters.Sprites[index]!, AttributeHide, 1);
                    starters.Timer = 0;
                    starters.State++;
                }
                break;
            case 5:
                starters.Timer++;
                if (starters.Timer >= HoldFrames)
                {
                    starters.Timer = 0;
                    starters.State = 0;
                    return true;
                }
                break;
        }
        return false;
    }

    private static bool Animate(OpeningStarters starters, int index, int deltaX, int deltaY, bool shrink)
    {
        PokemonSprite sprite = starters.Sprites[index]!;
        PokemonSprite.PokemonSprite_AddAttribute(sprite, AttributeX, deltaX);
        PokemonSprite.PokemonSprite_AddAttribute(sprite, AttributeY, deltaY);
        if (shrink)
        {
            PokemonSprite.PokemonSprite_AddAttribute(sprite, AttributeScaleX, ScaleStep);
            PokemonSprite.PokemonSprite_AddAttribute(sprite, AttributeScaleY, ScaleStep);
        }
        starters.Timer++;
        if (starters.Timer >= AnimFrames)
        {
            PokemonSprite.PokemonSprite_SetAttribute(sprite, AttributeScaleX, ScaleNormal);
            PokemonSprite.PokemonSprite_SetAttribute(sprite, AttributeScaleY, ScaleNormal);
            return true;
        }
        return false;
    }

    // OFFSET 0x021D7004
    private static bool OpeningStarters_EnterFromLeft(OpeningStarters starters, int index)
    {
        return Animate(starters, index, 6, -0x10, true);
    }

    // OFFSET 0x021D7074
    private static bool OpeningStarters_ExitLeft(OpeningStarters starters, int index)
    {
        return Animate(starters, index, -3, -0x12, false);
    }

    // OFFSET 0x021D70CC
    private static bool OpeningStarters_EnterFromRightFast(OpeningStarters starters, int index)
    {
        return Animate(starters, index, -0x10, -0x10, true);
    }

    // OFFSET 0x021D713C
    private static bool OpeningStarters_ExitLeftSlow(OpeningStarters starters, int index)
    {
        return Animate(starters, index, -6, -0x10, false);
    }

    // OFFSET 0x021D7194
    private static bool OpeningStarters_EnterFromLeftSlow(OpeningStarters starters, int index)
    {
        return Animate(starters, index, 8, -0x10, true);
    }

    // OFFSET 0x021D7204
    private static bool OpeningStarters_ExitRight(OpeningStarters starters, int index)
    {
        return Animate(starters, index, 6, -0x14, false);
    }

    // OFFSET 0x021D725C
    private static void OpeningStarters_DrawParticles()
    {
        ParticleSystem.ParticleSystem_DrawAll();
        ParticleSystem.ParticleSystem_UpdateAll();
    }

    // OFFSET 0x021D7268
    private static void OpeningStarters_PlaceEmitter(SplEmitter emitter)
    {
        PokemonSprite sprite = (PokemonSprite)ParticleSystem.ParticleSystem_GetEmitterCallbackParam()!;
        int x = (PokemonSprite.PokemonSprite_GetAttribute(sprite, AttributeX) - ScreenCenterX) * WorldUnitsPerPixel;
        int y = (PokemonSprite.PokemonSprite_GetAttribute(sprite, AttributeY) - ScreenCenterY) * WorldUnitsPerPixel;
        VecFx32 basePosition = emitter.Resource!.Header.EmitterBasePos;
        emitter.Position.X = x + basePosition.X;
        emitter.Position.Y = y + basePosition.Y;
        emitter.Position.Z = EmitterDepth + basePosition.Z;
    }
}
