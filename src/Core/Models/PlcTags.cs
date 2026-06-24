namespace TapeAdhesionApp.Core.Models;

/// <summary>
/// Mappings for S7-200 V-Memory to S7NetPlus DB1 addresses.
/// In S7-200, V-Memory is equivalent to DB1 in S7-300/400.
/// </summary>
public static class PlcTags
{
    // The Data Block number corresponding to V-Memory for S7-200
    public const int VMemoryDataBlock = 1;

    // Array of VD addresses for the 64 hooks per Rack (4 floors * 16 hooks).
    // Using -1 as a placeholder for unknown addresses.
    public static readonly int[] HookAddresses = new int[64];

    static PlcTags()
    {
        // Khởi tạo toàn bộ mảng với giá trị -1
        for (int i = 0; i < 64; i++)
        {
            HookAddresses[i] = -1;
        }
    }

    public static void LoadAddresses(int[] newAddresses)
    {
        if (newAddresses != null && newAddresses.Length == 64)
        {
            for (int i = 0; i < 64; i++)
            {
                HookAddresses[i] = newAddresses[i];
            }
        }
    }
}
