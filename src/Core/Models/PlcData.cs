using System;

namespace TapeAdhesionApp.Core.Models;

public class PlcData
{
    public bool IsConnected { get; set; }
    
    // Status
    public bool MachineState { get; set; }
    public bool DropSignal { get; set; }
    
    // Real-time Variables
    public uint DropTime { get; set; }
    public uint Variable110 { get; set; }
    public uint Variable368 { get; set; }
    
    // Timestamp when this data was fetched
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
