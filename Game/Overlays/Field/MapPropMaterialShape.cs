using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed class MapPropMaterialShape
{
    public const int LocatorSize = 4;
    public const int IDsSize = 4;
    public const int LocatorIDsCount = 0;
    public const int LocatorIDsIndex = 2;
    public const int IDsMaterialID = 0;
    public const int IDsShapeID = 2;
    public const int Size = 0x08;

    public byte[]? IDsLocators;
    public byte[]? IDs;

    // OFFSET 0x021D56BC
    public static MapPropMaterialShape MapPropMaterialShape_Alloc()
    {
        return new MapPropMaterialShape();
    }

    // OFFSET 0x021D56D4
    public static void MapPropMaterialShape_Load(string path, MapPropMaterialShape materialShape)
    {
        FsFile file = new();
        FileSystem.InitFile(file);
        if (!FileSystem.OpenFile(file, path))
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        byte[] header = new byte[4];
        if (FileSystem.ReadFile(file, header, 2, 2) < 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (FileSystem.ReadFile(file, header, 0, 2) < 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        int locatorsSize = Bin.U16(header, 2) * LocatorSize;
        int idsSize = Bin.U16(header, 0) * IDsSize;
        materialShape.IDsLocators = new byte[locatorsSize];
        materialShape.IDs = new byte[idsSize];
        if (FileSystem.ReadFile(file, materialShape.IDsLocators, 0, locatorsSize) < 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (FileSystem.ReadFile(file, materialShape.IDs, 0, idsSize) < 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        FileSystem.CloseFile(file);
    }

    // OFFSET 0x021D5778
    public static void MapPropMaterialShape_Free(MapPropMaterialShape materialShape)
    {
        materialShape.IDs = null;
        materialShape.IDsLocators = null;
    }

    // OFFSET 0x021D5790
    public static void MapProp_GetMaterialShapeIDsCount(int mapPropModelID, MapPropMaterialShape materialShape, out ushort idsCount)
    {
        idsCount = (ushort)Bin.U16(materialShape.IDsLocators!, mapPropModelID * LocatorSize + LocatorIDsCount);
    }

    // OFFSET 0x021D579C
    public static void MapProp_GetMaterialShapeIDsLocator(int mapPropModelID, MapPropMaterialShape materialShape, out ushort idsCount, out ushort idsIndex)
    {
        idsCount = (ushort)Bin.U16(materialShape.IDsLocators!, mapPropModelID * LocatorSize + LocatorIDsCount);
        idsIndex = (ushort)Bin.U16(materialShape.IDsLocators!, mapPropModelID * LocatorSize + LocatorIDsIndex);
    }

    // OFFSET 0x021D57B4
    public static BufferView MapPropMaterialShape_GetMaterialShapeIDsAt(ushort index, MapPropMaterialShape materialShape)
    {
        return new BufferView(materialShape.IDs!, index * IDsSize);
    }
}
