namespace TapeAdhesionApp.Core.State;

public enum MachineState
{
    IDLE,
    DEBOUNCE_START,
    RUNNING,
    DEBOUNCE_STOP,
    COMPLETED
}
