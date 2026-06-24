using System;

namespace TapeAdhesionApp.Core.Models;

public class HookData
{
    public string RackId { get; set; } = string.Empty;
    public int Floor { get; set; }
    public int HookIndex { get; set; }
    public string HookId { get; set; } = string.Empty;
    public uint CurrentValue { get; set; }
}

public class PlcData
{
    public string RackId { get; set; } = string.Empty;
    public bool IsConnected { get; set; }
    
    // Array of 64 hooks (4 floors * 16 hooks) for one rack
    public HookData[] Hooks { get; set; } = new HookData[64];
    
    // Timestamp when this data was fetched
    public DateTime Timestamp { get; set; } = DateTime.Now;

    public PlcData(string rackId)
    {
        RackId = rackId;
        for (int i = 0; i < 64; i++)
        {
            int floor = (i / 16) + 1;
            int hook = (i % 16) + 1;
            Hooks[i] = new HookData 
            { 
                RackId = rackId,
                Floor = floor,
                HookIndex = hook,
                HookId = $"{rackId}-T{floor}-M{hook}", 
                CurrentValue = 0 
            };
        }
    }
}
