namespace TapeAdhesionApp.Core.Models;

/// <summary>
/// Mappings for S7-200 V-Memory to S7NetPlus DB1 addresses.
/// In S7-200, V-Memory is equivalent to DB1 in S7-300/400.
/// </summary>
public static class PlcTags
{
    // The Data Block number corresponding to V-Memory for S7-200
    public const int VMemoryDataBlock = 1;

    // Dictionary of VD addresses for the 64 hooks per Rack (Key = RackId).
    public static readonly Dictionary<string, int[]> HookAddressesByRack = new();

    public static int[] GetAddresses(string rackId)
    {
        if (!HookAddressesByRack.ContainsKey(rackId))
        {
            var defaults = new int[64];
            for (int i = 0; i < 64; i++) defaults[i] = -1;
            HookAddressesByRack[rackId] = defaults;
        }
        return HookAddressesByRack[rackId];
    }

    public static void LoadAddresses(string rackId, int[] newAddresses)
    {
        if (newAddresses != null && newAddresses.Length == 64)
        {
            if (!HookAddressesByRack.ContainsKey(rackId))
            {
                HookAddressesByRack[rackId] = new int[64];
            }
            
            for (int i = 0; i < 64; i++)
            {
                HookAddressesByRack[rackId][i] = newAddresses[i];
            }
        }
    }
}
