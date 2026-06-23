namespace TapeAdhesionApp.UI.Views;

public interface IMainView
{
    // Inputs
    string BatchCode { get; }
    string NartCode { get; }
    void SetInputs(string batchCode, string nartCode);
    
    // Outputs
    void UpdatePlcConnectionStatus(bool isConnected);
    void UpdateMachineState(string stateName);
    void UpdateRunningTime(string formattedTime);
    void UpdateSamplePosition(string position);
    void EnableStartButton(bool enable);
    void AddTestRecord(TapeAdhesionApp.Core.Models.TestRecord record);
    void InvokeOnUI(Action action);
    
    // Commands
    event EventHandler StartTestClicked;
    event EventHandler StopTestClicked;
    event EventHandler ResetClicked;
    event EventHandler InputsChanged;
}
