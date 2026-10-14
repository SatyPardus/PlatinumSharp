using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed partial class FieldSystem
{
    public const int CameraOffset = 0x24;

    public Camera? Camera;
}

public static class FieldCamera
{
    public const int CameraTypeCount = 0x11;

    private const int Overlay = 5;
    private const int HeapIDField = 4;
    private const uint CameraConfigTable = 0x021F8AE4;
    private const int CameraConfigSize = 0x18;
    private const int ConfigDistance = 0x00;
    private const int ConfigAngle = 0x04;
    private const int ConfigProjection = 0x0C;
    private const int ConfigFovY = 0x0E;
    private const int ConfigNearClip = 0x10;
    private const int ConfigFarClip = 0x14;
    private const int HistorySize = 7;
    private const int HistoryDelay = 6;

    // OFFSET 0x021D5B40
    public static void FieldCamera_Create(Func<VecFx32> target, FieldSystem fieldSystem, int cameraType, int withHistory)
    {
        if ((uint)cameraType >= CameraTypeCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        byte[] config = RomData.Overlay(Overlay, CameraConfigTable + (uint)(cameraType * CameraConfigSize), CameraConfigSize);
        fieldSystem.Camera = Camera.Camera_Alloc(HeapIDField);
        CameraAngle angle = new()
        {
            X = (ushort)Bin.U16(config, ConfigAngle),
            Y = (ushort)Bin.U16(config, ConfigAngle + 2),
            Z = (ushort)Bin.U16(config, ConfigAngle + 4),
            Padding = (ushort)Bin.U16(config, ConfigAngle + 6),
        };
        Camera.Camera_InitWithTarget(target, Bin.I32(config, ConfigDistance), angle, (ushort)Bin.U16(config, ConfigFovY), config[ConfigProjection], true, fieldSystem.Camera);
        Camera.Camera_SetAsActive(fieldSystem.Camera);
        Camera.Camera_SetClipping(Bin.I32(config, ConfigNearClip), Bin.I32(config, ConfigFarClip), fieldSystem.Camera);
        if (withHistory != 0)
        {
            Camera.Camera_InitHistory(HistorySize, HistoryDelay, Camera.DelayMaskY, HeapIDField, fieldSystem.Camera);
        }
    }

    // OFFSET 0x021D5BA8
    public static void FieldCamera_Delete(FieldSystem fieldSystem)
    {
        Camera.Camera_ClearActive();
        Camera.Camera_DeleteHistory(fieldSystem.Camera!);
        Camera.Camera_Delete(fieldSystem.Camera!);
    }
}
