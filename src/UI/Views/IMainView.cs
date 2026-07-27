using System;
using System.Collections.Generic;
using TapeAdhesionApp.Core.Models;

namespace TapeAdhesionApp.UI.Views;

public interface IMainView
{
    // Events
    event Action<string, string> ConnectRackClicked;
    event Action<string> DisconnectRackClicked;
    event Action<string> SettingsClicked;
    event Action<string> ScannerClicked;
    event Action LoadHistory;
    event Action<List<int>> DeleteSelectedRecordsClicked;
    event Action<string?> ExportHistoryClicked; // string? rackId (null = all)
    event Action<MeasurementRow> RowInfoChanged;

    // Methods
    void UpdateRackConnectionStatus(string rackId, bool isConnected);
    void UpdateMeasurementRow(string rackId, int floor, int hookIndex, uint value, string state);
    void FlashRackTab(string rackId);
    MeasurementRow? GetRowData(string rackId, int floor, int hookIndex);
    
    // History
    void LoadHistoryData(IEnumerable<TestRecord> records);
    void AddTestRecord(TestRecord record);
    void UpdateRecordInfoInHistory(string hookId, string batch, string nart, string tester, string samplePosition);
}
