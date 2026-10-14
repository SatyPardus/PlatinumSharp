namespace PlatinumSharp.Game;

public static partial class PokemonSummaryScreenApp
{
    private const uint CameraPositionAddress = 0x020F4FF0;
    private const uint ConditionCenterRect0 = 0x020F5038;
    private const uint ConditionCenterRect1 = 0x020F506E;
    private const uint ConditionCenterRect2 = 0x020F50A4;
    private const uint ConditionCenterRect3 = 0x020F50DA;
    private const ushort Disp3DShadingMask = 0xCFFD;
    private const ushort Disp3DMask = 0xCFFF;
    private const ushort Disp3DAlphaTestMask = 0xCFFB;
    private const ushort Disp3DAlphaBlend = 8;
    private const ushort Disp3DAntiAlias = 0x10;
    private const ushort Disp3DEdgeMarkingMask = 0xCFDF;
    private const int GxMtxMode = 0x10;
    private const int GxMtxIdentity = 0x15;
    private const int GxColor = 0x20;
    private const int GxVtx16 = 0x23;
    private const int GxPolygonAttr = 0x29;
    private const int GxBeginVtxs = 0x40;
    private const int GxEndVtxs = 0x41;
    private const uint GraphPolygonAttr = 0x121400C0;
    private const uint GraphColor = 0x3FE8;
    private const int GraphStateDone = 4;
    private const int GraphStateFinish = 3;
    private const int MonSpriteX = 0x34;
    private const int MonSpriteY = 0x68;
    private const int MonSpriteAttrFlip = 0x23;
    private const uint StatCenter = 0;
    private const byte StatTableBase = 0xFF;

    // OFFSET 0x020916B4
    private static void PokemonSummaryScreen_Setup3DGfx()
    {
        Render3D.Reset();
        Render3D.Display3DControl &= Disp3DShadingMask;
        Render3D.Display3DControl &= Disp3DAlphaTestMask;
        Render3D.Display3DControl = (ushort)((Render3D.Display3DControl & Disp3DMask) | Disp3DAlphaBlend);
        Render3D.Display3DControl = (ushort)((Render3D.Display3DControl & Disp3DMask) | Disp3DAntiAlias);
        Render3D.Display3DControl &= Disp3DEdgeMarkingMask;
        Render3D.SetFog(false, 0, 0, 0);
        Render3D.SetClearColor(0, 0, 0x7FFF, 0x3F, false);
        Render3D.SwapBuffers(0, 0);
        Render3D.SetViewport(0, 0, 255, 191);
        GXLayers.GXLayers_EngineAToggleLayers(1, 1);
        Video.SetBgControl(0, (ushort)((Video.GetBgControl(0) & ~3) | 2));
    }

    // OFFSET 0x02091750
    private static void PokemonSummaryScreen_Update3DGfx(PokemonSummaryScreen summaryScreen)
    {
        if (summaryScreen.Page == PageCondition)
        {
            G3SwapRequest.G3_ResetG3X();
            Camera.Camera_ComputeViewMatrix();
            Render3D.WriteCommand(GxMtxMode, 0);
            Render3D.WriteCommand(GxMtxIdentity);
            Render3D.WriteCommand(GxMtxMode, 2);
            Render3D.WriteCommand(GxMtxIdentity);
            Render3D.FlushGlobalState();
            DrawSpiderGraph(summaryScreen.Graph);
            Render3D.FlushGlobalState();
            UpdateSpiderGraph(summaryScreen);
        }
        Render3D.SetupSoftwareSpriteCamera();
        PokemonSpriteManager.PokemonSpriteManager_DrawSprites(summaryScreen.MonSprite.SpriteManager!);
        Render3D.SwapBuffers(1, 0);
    }

    // OFFSET 0x020917B0
    private static void PokemonSummaryScreen_FreeCameraAndMonSprite(PokemonSummaryScreen summaryScreen)
    {
        Camera.Camera_Delete(summaryScreen.MonSprite.Camera!);
        PokemonAnimManager.PokemonAnimManager_DeleteAnim(summaryScreen.MonSprite.MonAnimMan!, 0);
        PokemonAnimManager.PokemonAnimManager_Free(summaryScreen.MonSprite.MonAnimMan!);
        PokemonSpriteManager.PokemonSpriteManager_Free(summaryScreen.MonSprite.SpriteManager!);
    }

    // OFFSET 0x020917E0
    private static void PokemonSummaryScreen_SetupCamera(PokemonSummaryScreen summaryScreen)
    {
        uint[] raw = RomData.Arm9U32(CameraPositionAddress, 3);
        VecFx32 position = new() { X = (int)raw[0], Y = (int)raw[1], Z = (int)raw[2] };
        CameraAngle angle = new() { X = 0, Y = 0, Z = 0 };
        summaryScreen.MonSprite.Camera = Camera.Camera_Alloc(HeapIDSummary);
        Camera.Camera_InitWithPosition(position, 0x10000, angle, 0x5C1, 1, summaryScreen.MonSprite.Camera);
        Camera.Camera_SetClipping(0, 0x64000, summaryScreen.MonSprite.Camera);
        Camera.Camera_ReleaseTarget(summaryScreen.MonSprite.Camera);
        Camera.Camera_SetAsActive(summaryScreen.MonSprite.Camera);
    }

    private static void SendVertex(in VecFx16 vertex)
    {
        Render3D.WriteCommand(GxVtx16, (uint)(ushort)vertex.X | ((uint)(ushort)vertex.Y << 16), (ushort)vertex.Z);
    }

    // OFFSET 0x02091850
    private static void DrawSpiderGraph(SpiderGraph graph)
    {
        Render3D.WriteCommand(GxPolygonAttr, GraphPolygonAttr);
        Render3D.WriteCommand(GxBeginVtxs, 1);
        for (int i = 0; i < SpiderGraph.MaxRects; i++)
        {
            SpiderGraphRectangle rect = graph.CurrRects[i];
            Render3D.WriteCommand(GxColor, GraphColor);
            SendVertex(rect.TopLeft);
            Render3D.WriteCommand(GxColor, GraphColor);
            SendVertex(rect.TopRight);
            Render3D.WriteCommand(GxColor, GraphColor);
            SendVertex(rect.BottomRight);
            Render3D.WriteCommand(GxColor, GraphColor);
            SendVertex(rect.BottomLeft);
        }
        Render3D.WriteCommand(GxEndVtxs);
    }

    // OFFSET 0x020918CC
    private static void VecFx16_Add(ref VecFx16 vec, in VecFx16 delta)
    {
        vec.X = (short)(vec.X + delta.X);
        vec.Y = (short)(vec.Y + delta.Y);
        vec.Z = (short)(vec.Z + delta.Z);
    }

    // OFFSET 0x020918EC
    private static void UpdateSpiderGraph(PokemonSummaryScreen summaryScreen)
    {
        SpiderGraph graph = summaryScreen.Graph;
        if (graph.State == GraphStateDone)
        {
            PokemonSummaryScreen_UpdateConditionFlashAnims(summaryScreen);
            return;
        }
        if (graph.State == GraphStateFinish)
        {
            for (int i = 0; i < SpiderGraph.MaxRects; i++)
            {
                SpiderGraphRectangle max = graph.MaxRectsData[i];
                SpiderGraphRectangle current = graph.CurrRects[i];
                current.TopLeft = max.TopLeft;
                current.TopRight = max.TopRight;
                current.BottomLeft = max.BottomLeft;
                current.BottomRight = max.BottomRight;
            }
        }
        else
        {
            for (int i = 0; i < SpiderGraph.MaxRects; i++)
            {
                SpiderGraphRectangle delta = graph.DeltaRects[i];
                SpiderGraphRectangle current = graph.CurrRects[i];
                VecFx16_Add(ref current.TopLeft, delta.TopLeft);
                VecFx16_Add(ref current.TopRight, delta.TopRight);
                VecFx16_Add(ref current.BottomLeft, delta.BottomLeft);
                VecFx16_Add(ref current.BottomRight, delta.BottomRight);
            }
        }
        graph.State++;
        if (graph.State == GraphStateDone)
        {
            PokemonSummaryScreen_UpdateConditionFlashSprites(summaryScreen, 1);
            return;
        }
        PokemonSummaryScreen_UpdateConditionFlashSprites(summaryScreen, 0);
    }

    private static VecFx16 ReadVecFx16(uint address)
    {
        ushort[] raw = RomData.Arm9U16(address, 3);
        return new VecFx16 { X = (short)raw[0], Y = (short)raw[1], Z = (short)raw[2] };
    }

    private static void SetRectToPoint(SpiderGraphRectangle rect, in VecFx16 point)
    {
        rect.TopLeft = point;
        rect.TopRight = point;
        rect.BottomLeft = point;
        rect.BottomRight = point;
    }

    // OFFSET 0x020919E8
    public static void PokemonSummaryScreen_InitConditionRects(PokemonSummaryScreen summaryScreen)
    {
        if (summaryScreen.Page != PageCondition)
        {
            return;
        }
        SpiderGraph graph = summaryScreen.Graph;
        SetRectToPoint(graph.CurrRects[0], ReadVecFx16(ConditionCenterRect0));
        SetRectToPoint(graph.CurrRects[1], ReadVecFx16(ConditionCenterRect1));
        SetRectToPoint(graph.CurrRects[2], ReadVecFx16(ConditionCenterRect2));
        SetRectToPoint(graph.CurrRects[3], ReadVecFx16(ConditionCenterRect3));
        PokemonSummaryScreen_InitMaxAndDeltaConditionRects(summaryScreen);
    }

    // OFFSET 0x02091B78
    private static void SetConditionVecFromStat(uint table, out VecFx16 vec, byte stat)
    {
        ushort[] raw = RomData.Arm9U16(table, 9);
        if (stat == StatTableBase)
        {
            vec = new VecFx16 { X = (short)raw[0], Y = (short)raw[1], Z = (short)raw[2] };
            return;
        }
        if (stat == StatCenter)
        {
            vec = new VecFx16 { X = (short)raw[3], Y = (short)raw[4], Z = (short)raw[5] };
            return;
        }
        vec = new VecFx16
        {
            X = (short)((short)raw[3] + (short)raw[6] * stat),
            Y = (short)((short)raw[4] + (short)raw[7] * stat),
            Z = (short)((short)raw[5] + (short)raw[8] * stat),
        };
    }

    private static short ConditionStep(int from, int to)
    {
        int distance = to - from;
        float step = (float)distance / 4096f / 4f;
        if (step > 0)
        {
            return (short)(int)(4096f * step + 0.5f);
        }
        return (short)(int)(4096f * step - 0.5f);
    }

    // OFFSET 0x02091BD4
    private static void SetConditionVecDifference(in VecFx16 current, in VecFx16 max, out VecFx16 delta)
    {
        delta = new VecFx16
        {
            X = ConditionStep(current.X, max.X),
            Y = ConditionStep(current.Y, max.Y),
            Z = ConditionStep(current.Z, max.Z),
        };
    }

    // OFFSET 0x02091D50
    public static void PokemonSummaryScreen_InitMaxAndDeltaConditionRects(PokemonSummaryScreen summaryScreen)
    {
        PokemonSummaryMonData monData = summaryScreen.MonData;
        SpiderGraph graph = summaryScreen.Graph;
        SpiderGraphRectangle[] max = graph.MaxRectsData;
        SetConditionVecFromStat(0x020F4FFC, out max[0].TopLeft, monData.Cool);
        SetConditionVecFromStat(0x020F500E, out max[0].TopRight, monData.Beauty);
        SetConditionVecFromStat(0x020F5020, out max[0].BottomRight, monData.Cute);
        SetConditionVecFromStat(0x020F5032, out max[0].BottomLeft, 0);
        SetConditionVecFromStat(0x020F5044, out max[1].TopLeft, monData.Tough);
        SetConditionVecFromStat(0x020F5056, out max[1].TopRight, monData.Cool);
        SetConditionVecFromStat(0x020F5068, out max[1].BottomRight, 0);
        SetConditionVecFromStat(0x020F507A, out max[1].BottomLeft, monData.Smart);
        SetConditionVecFromStat(0x020F508C, out max[2].TopLeft, monData.Tough);
        SetConditionVecFromStat(0x020F509E, out max[2].TopRight, 0);
        SetConditionVecFromStat(0x020F50B0, out max[2].BottomRight, monData.Cute);
        SetConditionVecFromStat(0x020F50C2, out max[2].BottomLeft, monData.Smart);
        SetConditionVecFromStat(0x020F50D4, out max[3].TopLeft, 0);
        SetConditionVecFromStat(0x020F50E6, out max[3].TopRight, monData.Beauty);
        SetConditionVecFromStat(0x020F50F8, out max[3].BottomRight, monData.Cute);
        SetConditionVecFromStat(0x020F510A, out max[3].BottomLeft, monData.Smart);
        for (int i = 0; i < SpiderGraph.MaxRects; i++)
        {
            SpiderGraphRectangle current = graph.CurrRects[i];
            SpiderGraphRectangle delta = graph.DeltaRects[i];
            SetConditionVecDifference(current.TopLeft, max[i].TopLeft, out delta.TopLeft);
            SetConditionVecDifference(current.TopRight, max[i].TopRight, out delta.TopRight);
            SetConditionVecDifference(current.BottomLeft, max[i].BottomLeft, out delta.BottomLeft);
            SetConditionVecDifference(current.BottomRight, max[i].BottomRight, out delta.BottomRight);
        }
        graph.State = 0;
    }

    // OFFSET 0x02091F8C
    private static void PokemonSummaryScreen_LoadMonSprite(PokemonSummaryScreen summaryScreen)
    {
        PokemonSummaryMonSpriteData monSprite = summaryScreen.MonSprite;
        monSprite.SpriteManager = PokemonSpriteManager.PokemonSpriteManager_New(HeapIDSummary);
        object mon = PokemonSummaryScreen_MonData(summaryScreen);
        PokemonSpriteTemplate template = new();
        if (summaryScreen.Data!.DataType == DataTypeBoxMon)
        {
            PokemonSpriteTemplates.BoxPokemon_BuildSpriteTemplate(template, (BoxPokemon)mon, 2, 0);
        }
        else
        {
            PokemonSpriteTemplates.Pokemon_BuildSpriteTemplate(template, (Pokemon)mon, 2);
        }
        PokemonSpriteMetadata.PokemonSprite_LoadAnimFrames(summaryScreen.NarcPlPokeData!, monSprite.Frames, summaryScreen.MonData.Species, 1);
        monSprite.Flip = (SpeciesData.SpeciesData_GetFormValue(summaryScreen.MonData.Species, summaryScreen.MonData.Form, FormParamFlip) ^ 1) != 0;
        monSprite.Sprite = PokemonSpriteManager.PokemonSpriteManager_CreateSprite(monSprite.SpriteManager, template, MonSpriteX, MonSpriteY, 0, 0, monSprite.Frames, null);
        PokemonSprite.PokemonSprite_SetAttribute(monSprite.Sprite, MonSpriteAttrFlip, monSprite.Flip ? 1 : 0);
    }

    // OFFSET 0x02092028
    private static void PokemonSummaryScreen_LoadMonAnimation(PokemonSummaryScreen summaryScreen)
    {
        PokemonSummaryMonSpriteData monSprite = summaryScreen.MonSprite;
        if (summaryScreen.MonData.IsEgg != 0)
        {
            PokemonSpriteMetadata.PokemonSprite_LoadAnim(summaryScreen.NarcPlPokeData!, monSprite.MonAnimMan!, monSprite.Sprite!, 0, 2, monSprite.Flip ? 1 : 0, 0);
            return;
        }
        PokemonSprite.PokemonSprite_InitAnim(monSprite.Sprite!, 1);
        PokemonSpriteMetadata.PokemonSprite_LoadAnim(summaryScreen.NarcPlPokeData!, monSprite.MonAnimMan!, monSprite.Sprite!, summaryScreen.MonData.Species, 2, monSprite.Flip ? 1 : 0, 0);
    }

    // OFFSET 0x02092098
    public static void PokemonSummaryScreen_ChangeMonSprite(PokemonSummaryScreen summaryScreen)
    {
        PokemonAnimManager.PokemonAnimManager_DeleteAnim(summaryScreen.MonSprite.MonAnimMan!, 0);
        PokemonSpriteManager.PokemonSpriteManager_Free(summaryScreen.MonSprite.SpriteManager!);
        PokemonSummaryScreen_LoadMonSprite(summaryScreen);
        PokemonSummaryScreen_LoadMonAnimation(summaryScreen);
    }
}
