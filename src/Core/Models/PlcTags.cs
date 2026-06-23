namespace TapeAdhesionApp.Core.Models;

/// <summary>
/// Mappings for S7-200 V-Memory to S7NetPlus DB1 addresses.
/// In S7-200, V-Memory is equivalent to DB1 in S7-300/400.
/// </summary>
public static class PlcTags
{
    // The Data Block number corresponding to V-Memory for S7-200
    public const int VMemoryDataBlock = 1;

    // Example mappings (to be adjusted based on actual PLC program):
    // Read tags
    public const int V0_0_MachineState = 0; // V0.0 -> DB1.DBX0.0
    public const int V0_1_DropSignal = 0;   // V0.1 -> DB1.DBX0.1 (Bit 1)
    
    public const int VD10_DropTime = 10;    // VD10 -> DB1.DBD10
    public const int VD110_Variable = 110;  // VD110 -> DB1.DBD110
    public const int VD368_Variable = 368;  // VD368 -> DB1.DBD368

    // Write tags
    public const int V1_0_ResetCommand = 1; // V1.0 -> DB1.DBX1.0
}
