namespace PlatinumSharp.Game;

public sealed partial class FieldSystem
{
    public const int BottomScreenOffset = 0x18;
    public const int TerrainCollisionManagerOffset = 0x5C;
    public const int SkipMapAttributesOffset = 0x60;
    public const int MapLoadModeOffset = 0x74;
    public const int WildEncounterAttemptsOffset = 0x78;
    public const int WildMonsDefeatedOffset = 0x7A;
    public const int CommUnionRoomOffset = 0x7C;
    public const int Unknown80Offset = 0x80;
    public const int JournalEntryOffset = 0x9C;

    public int BottomScreen;
    public object? TerrainCollisionManager;
    public int SkipMapAttributes;
    public MapLoadMode? MapLoadMode;
    public ushort WildEncounterAttempts;
    public ushort WildMonsDefeated;
    public object? CommUnionRoom;
    public object? Unknown80;
    public Journal? JournalEntry;
}
