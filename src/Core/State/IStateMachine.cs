using System;
using TapeAdhesionApp.Core.Models;

namespace TapeAdhesionApp.Core.State;

public interface IStateMachine
{
    MachineState CurrentState { get; }
    
    // Triggered when the state machine completes a test cycle
    event EventHandler<TestCompletedEventArgs> OnTestCompleted;
    
    // Called periodically by the polling loop with fresh PLC data
    void Tick(PlcData data);
    
    // Forces the state machine back to IDLE
    void Reset();
}

public class TestCompletedEventArgs : EventArgs
{
    public uint FinalDropTime { get; set; }
    public DateTime CompletedAt { get; set; }
}
