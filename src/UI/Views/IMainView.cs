namespace TapeAdhesionApp.UI.Views;

public interface IMainView
{
    // Inputs
    string BatchCode { get; }
    string NartCode { get; }
    
    // Outputs
    void UpdatePlcConnectionStatus(bool isConnected);
    void UpdateMachineState(string stateName);
    void UpdateRunningTime(string formattedTime);
    void UpdateSamplePosition(string position);
    void EnableStartButton(bool enable);
    
    // Commands
    event EventHandler StartTestClicked;
    event EventHandler StopTestClicked;
    event EventHandler ResetClicked;
}
