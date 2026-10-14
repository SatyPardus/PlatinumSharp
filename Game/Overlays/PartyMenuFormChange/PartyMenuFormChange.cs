namespace PlatinumSharp.Game;

public static class PartyMenuFormChanges
{
    private const int Overlay = 118;
    private const int HeapID = 0xC;
    private const int ParticleHeapSize = 0x4800;
    private const int ParticleArchive = 0xB9;
    private const uint EmitterOffsets = 0x021D1170;
    private const int SpeciesRotom = 0x1DF;
    private const int SpeciesGiratina = 0x1E7;
    private const int SpeciesShaymin = 0x1EC;
    private const int MonDataSpecies = 5;
    private const int SeqGiratinaFormChange = 0x553;
    private const int SeqShayminFormChange = 0x554;
    private const int DisplayMain = 0;

    // OFFSET 0x021D0D80
    public static void PartyMenuFormChange_Init(PartyMenuApplication app)
    {
        if (app.FormChanger != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        app.FormChanger = new PartyMenuFormChange();
        app.FormChanger.PartySlot = app.CurrPartySlot;
    }

    // OFFSET 0x021D0DBC
    public static bool PartyMenuFormChange_ChangeForm(PartyMenuApplication app)
    {
        PartyMenuFormChange formChange = app.FormChanger!;
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(app.PartyMenu!.Party!, app.CurrPartySlot);
        switch (formChange.State)
        {
            case 0:
                formChange.Species = (int)Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null);
                if (formChange.Species == SpeciesGiratina)
                {
                    Pokemon.Pokemon_SetGiratinaFormByHeldItem(mon);
                    formChange.FramesBeforeFormChange = 0x41;
                    formChange.NarcIndex = 0;
                }
                else if (formChange.Species == SpeciesShaymin)
                {
                    Pokemon.Pokemon_SetShayminForm(mon, 1);
                    formChange.FramesBeforeFormChange = 0x23;
                    formChange.NarcIndex = 1;
                }
                else
                {
                    ErrorHandling.GF_AssertFail();
                }
                Pokedex.Pokedex_Capture(Pokedex.SaveData_GetPokedex(FieldSystemCore.FieldSystem_GetSaveData(app.PartyMenu.FieldSystem!)!), mon);
                formChange.State++;
                break;
            case 1:
            case 2:
                formChange.State++;
                break;
            case 3:
                PartyMenuFormChange_InitGraphics(app);
                formChange.State++;
                break;
            case 4:
                GXLayers.GXLayers_EngineAToggleLayers(1, 1);
                PartyMenuFormChange_StartAnimation(formChange);
                formChange.State++;
                break;
            case 5:
                formChange.ElapsedFrames++;
                if (formChange.ElapsedFrames == formChange.FramesBeforeFormChange)
                {
                    PartyMenus.PartyMenu_LoadMemberSpeciesIcon(app, app.CurrPartySlot);
                }
                PartyMenuFormChange_DrawParticles();
                if (formChange.ElapsedFrames > formChange.FramesBeforeFormChange && ParticleSystem.ParticleSystem_GetActiveEmitterCount((ParticleSystem)formChange.ParticleSystem!) == 0)
                {
                    formChange.State++;
                }
                break;
            case 6:
                PartyMenuFormChange_FreeGraphics(app);
                formChange.State++;
                break;
            case 7:
                Pokemon.Pokemon_PlayCry(mon);
                formChange.State++;
                break;
            case 8:
                if (SoundPlayback.Sound_IsPokemonCryPlaying() == 0)
                {
                    formChange.State++;
                }
                break;
            case 9:
            {
                GameString format = MessageLoader.MessageLoader_GetNewString(app.MessageLoader!, 0xCA)!;
                StringTemplate.StringTemplate_SetNickname(app.Template!, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
                StringTemplate.StringTemplate_Format(app.Template!, app.TmpString!, format);
                GameString.String_Free(format);
                PartyMenus.PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 1);
                formChange.State++;
                break;
            }
            case 10:
                if (Text.Text_IsPrinterActive(app.TextPrinterID) == 0)
                {
                    PartyMenuFormChange_Free(app);
                    app.PartyMenu.MenuSelectionResult = 0;
                    return true;
                }
                break;
        }
        return false;
    }

    // OFFSET 0x021D0F70
    private static void PartyMenuFormChange_Free(PartyMenuApplication app)
    {
        app.FormChanger = null;
    }

    // OFFSET 0x021D0F88
    private static void PartyMenuFormChange_InitGraphics(PartyMenuApplication app)
    {
        PartyMenus.PartyMenu_UpdateFormChangeGraphicsMode(app, false);
        PartyMenuFormChange_InitParticleSystem(app.FormChanger!);
        Video.SetBlendAlpha(DisplayMain, 0, 0x3F, 0x1F, 0);
    }

    // OFFSET 0x021D0FB8
    private static void PartyMenuFormChange_FreeGraphics(PartyMenuApplication app)
    {
        PartyMenuFormChange_FreeParticleSystem(app.FormChanger!);
        PartyMenus.PartyMenu_UpdateFormChangeGraphicsMode(app, true);
        Video.SetBlendControl(DisplayMain, 0);
    }

    // OFFSET 0x021D0FDC
    private static void PartyMenuFormChange_InitParticleSystem(PartyMenuFormChange formChange)
    {
        ParticleSystem.ParticleSystem_ZeroAll();
        byte[] heap = Heap.Heap_Alloc(HeapID, ParticleHeapSize)!;
        ParticleSystem particleSystem = ParticleSystem.ParticleSystem_New(PartyMenuFormChange_AllocTexture, PartyMenuFormChange_AllocPalette, heap, ParticleHeapSize, 1, HeapID)!;
        formChange.ParticleSystem = particleSystem;
        Camera.Camera_SetClipping(Fx32.One, 0x384000, ParticleSystem.ParticleSystem_GetCamera(particleSystem)!);
    }

    // OFFSET 0x021D1028
    private static void PartyMenuFormChange_StartAnimation(PartyMenuFormChange formChange)
    {
        ParticleSystem particleSystem = (ParticleSystem)formChange.ParticleSystem!;
        byte[] resource = ParticleSystem.ParticleSystem_LoadResourceFromNARC(ParticleArchive, (int)formChange.NarcIndex, HeapID);
        ParticleSystem.ParticleSystem_SetResource(particleSystem, resource, 0xA, 1);
        if (formChange.Species == SpeciesGiratina)
        {
            ParticleSystem.ParticleSystem_CreateEmitterWithCallback(particleSystem, 0, PartyMenuFormChange_EmitterCallback, formChange);
            ParticleSystem.ParticleSystem_CreateEmitterWithCallback(particleSystem, 1, PartyMenuFormChange_EmitterCallback, formChange);
            ParticleSystem.ParticleSystem_CreateEmitterWithCallback(particleSystem, 2, PartyMenuFormChange_EmitterCallback, formChange);
            SoundPlayback.Sound_PlayEffect(SeqGiratinaFormChange);
        }
        else if (formChange.Species == SpeciesShaymin)
        {
            ParticleSystem.ParticleSystem_CreateEmitterWithCallback(particleSystem, 0, PartyMenuFormChange_EmitterCallback, formChange);
            ParticleSystem.ParticleSystem_CreateEmitterWithCallback(particleSystem, 1, PartyMenuFormChange_EmitterCallback, formChange);
            SoundPlayback.Sound_PlayEffect(SeqShayminFormChange);
        }
    }

    // OFFSET 0x021D10B0
    private static void PartyMenuFormChange_EmitterCallback(SplEmitter emitter)
    {
        PartyMenuFormChange formChange = (PartyMenuFormChange)ParticleSystem.ParticleSystem_GetEmitterCallbackParam()!;
        uint[] offsets = RomData.OverlayU32(Overlay, (uint)(EmitterOffsets + formChange.PartySlot * 8), 2);
        emitter.Position.X = (int)offsets[0] + emitter.Resource!.Header.EmitterBasePos.X;
        emitter.Position.Y = (int)offsets[1] + emitter.Resource.Header.EmitterBasePos.Y;
    }

    // OFFSET 0x021D10E8
    private static int PartyMenuFormChange_DrawParticles()
    {
        G3SwapRequest.G3_ResetG3X();
        int drawn = ParticleSystem.ParticleSystem_DrawAll();
        if (drawn > 0)
        {
            G3SwapRequest.G3_ResetG3X();
        }
        ParticleSystem.ParticleSystem_UpdateAll();
        G3SwapRequest.G3_RequestSwapBuffers(1, 0);
        return drawn;
    }

    // OFFSET 0x021D110C
    private static void PartyMenuFormChange_FreeParticleSystem(PartyMenuFormChange formChange)
    {
        ParticleSystem particleSystem = (ParticleSystem)formChange.ParticleSystem!;
        ParticleSystem.ParticleSystem_GetHeapStart(particleSystem);
        ParticleSystem.ParticleSystem_Free(particleSystem);
    }

    // OFFSET 0x021D1128
    private static uint PartyMenuFormChange_AllocTexture(uint size, int is4x4Comp)
    {
        uint key = Gfd.AllocTexVram(size, is4x4Comp, 0);
        if (key == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        ParticleSystem.ParticleSystem_RegisterTextureKey(key);
        return (key & 0xFFFF) << 3;
    }

    // OFFSET 0x021D114C
    private static uint PartyMenuFormChange_AllocPalette(uint size, int is4Pal)
    {
        uint key = Gfd.AllocPlttVram(size, is4Pal, 1);
        if (key == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        ParticleSystem.ParticleSystem_RegisterPaletteKey(key);
        return (key & 0xFFFF) << 3;
    }
}
