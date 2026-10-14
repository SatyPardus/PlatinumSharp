namespace PlatinumSharp.Game;

public sealed partial class FieldSystem
{
    public const int MapHeaderDataOffset = 0x14;
    public const int MapObjectManagerOffset = 0x38;

    public MapHeaderData? MapHeaderData;
    public MapObjectManager? MapObjectManager;
}
