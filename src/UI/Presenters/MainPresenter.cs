using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TapeAdhesionApp.Core.Models;
using TapeAdhesionApp.Core.PLC;
using TapeAdhesionApp.Core.State;
using TapeAdhesionApp.Data.Database;
using TapeAdhesionApp.Data.Export;
using TapeAdhesionApp.UI.Views;

namespace TapeAdhesionApp.UI.Presenters;

public class MainPresenter : IDisposable
{
    private readonly IMainView _view;
    private readonly PlcManager _plcManager;
    private readonly TestRepository _testRepo;
    private readonly ExcelReportService _excelService;
    
    private ConcurrentDictionary<string, StateMachine> _stateMachines = new();
    private Dictionary<string, CancellationTokenSource> _pollingTokens = new();
    
    private string[] _rackIds = { "Rack1", "Rack2", "Rack3", "Rack4" };

    public MainPresenter(
        IMainView view, 
        PlcManager plcManager, 
        TestRepository testRepo,
        ExcelReportService excelService)
    {
        _view = view;
        _plcManager = plcManager;
        _testRepo = testRepo;
        _excelService = excelService;
        
        // Initialize 256 state machines (4 racks * 64 hooks)
        foreach (var rackId in _rackIds)
        {
            for (int i = 0; i < 64; i++)
            {
                int floor = (i / 16) + 1;
                int hook = (i % 16) + 1;
                string hookId = $"{rackId}-T{floor}-M{hook}";
                
                var sm = new StateMachine(hookId);
                sm.OnTestCompleted += StateMachine_OnTestCompleted;
                _stateMachines[hookId] = sm;
            }
        }

        // Event bindings
        _view.ConnectRackClicked += OnConnectRackClicked;
        _view.DisconnectRackClicked += OnDisconnectRackClicked;
        _view.SettingsClicked += OnSettingsClicked;
        _view.ScannerClicked += OnScannerClicked;
        _view.DeleteSelectedRecordsClicked += OnDeleteSelectedRecordsClicked;
        _view.ExportHistoryClicked += OnExportHistoryClicked;
        
        if (_view is Form form)
        {
            form.Load += async (s, e) => await InitializeAsync();
            form.FormClosing += (s, e) => StopAllPolling();
        }
    }

    private async Task InitializeAsync()
    {
        var history = await _testRepo.GetAllTestRecordsAsync();
        _view.LoadHistoryData(history);
        
        using var conn = new Microsoft.Data.Sqlite.SqliteConnection(DatabaseInitializer.ConnectionString);
        foreach (var rackId in _rackIds)
        {
            var json = await Dapper.SqlMapper.QueryFirstOrDefaultAsync<string>(conn, "SELECT Value FROM Settings WHERE Key = @Key", new { Key = $"PlcAddresses_{rackId}" });
            int[] addresses = new int[64];
            for (int i = 0; i < 64; i++) addresses[i] = -1;
            if (!string.IsNullOrEmpty(json)) { try { addresses = System.Text.Json.JsonSerializer.Deserialize<int[]>(json) ?? addresses; } catch {} }
            PlcTags.LoadAddresses(rackId, addresses);
        }
    }

    private async void OnConnectRackClicked(string rackId, string ipAddress)
    {
        using var conn = new Microsoft.Data.Sqlite.SqliteConnection(DatabaseInitializer.ConnectionString);
        await Dapper.SqlMapper.ExecuteAsync(conn, @"
            INSERT INTO Settings (Key, Value) VALUES (@Key, @Value)
            ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value", 
            new { Key = $"PlcIpAddress_{rackId}", Value = ipAddress });

        bool connected = await _plcManager.ConnectRackAsync(rackId, ipAddress);
        
        _view.UpdateRackConnectionStatus(rackId, connected);

        if (connected)
        {
            StartPollingLoop(rackId);
        }
        else
        {
            MessageBox.Show($"Không thể kết nối đến PLC của {rackId} ({ipAddress}).", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnDisconnectRackClicked(string rackId)
    {
        StopPollingLoop(rackId);
        _plcManager.DisconnectRack(rackId);
        _view.UpdateRackConnectionStatus(rackId, false);
    }

    private void StartPollingLoop(string rackId)
    {
        if (_pollingTokens.ContainsKey(rackId)) return;

        var cts = new CancellationTokenSource();
        _pollingTokens[rackId] = cts;
        
        Task.Run(async () =>
        {
            var service = _plcManager.GetService(rackId);
            while (!cts.Token.IsCancellationRequested)
            {
                if (service.IsConnected)
                {
                    var data = await service.ReadPlcDataAsync();
                    if (data != null && data.IsConnected)
                    {
                        ProcessPlcData(data);
                    }
                    else
                    {
                        _view.UpdateRackConnectionStatus(rackId, false);
                    }
                }
                await Task.Delay(100, cts.Token);
            }
        }, cts.Token);
    }

    private void StopPollingLoop(string rackId)
    {
        if (_pollingTokens.TryGetValue(rackId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
            _pollingTokens.Remove(rackId);
        }
    }

    private void StopAllPolling()
    {
        foreach (var rackId in _rackIds)
        {
            StopPollingLoop(rackId);
        }
    }

    private void ProcessPlcData(PlcData data)
    {
        string rackId = data.RackId;

        for (int i = 0; i < 64; i++)
        {
            var hookData = data.Hooks[i];
            int floor = hookData.Floor;
            int hookIndex = hookData.HookIndex;
            string hookId = hookData.HookId;

            if (_stateMachines.TryGetValue(hookId, out var sm))
            {
                // Get current row info from UI to check NART/Batch
                var rowData = _view.GetRowData(rackId, floor, hookIndex);
                
                if (rowData != null)
                {
                    // Luôn luôn xử lý giá trị để cập nhật trạng thái UI (Xanh/Đỏ/Trắng)
                    sm.ProcessValue(hookData.CurrentValue, hookData.IsGood);

                    // Map state enum to string
                    string stateStr = "IDLE";
                    if (sm.CurrentState == HookState.Running) stateStr = "RUNNING";
                    if (sm.CurrentState == HookState.Completed) stateStr = "COMPLETED";

                    // Update UI (value and state)
                    _view.UpdateMeasurementRow(rackId, floor, hookIndex, hookData.CurrentValue, stateStr);
                }
            }
        }
    }

    private async void StateMachine_OnTestCompleted(object? sender, TestCompletedEventArgs e)
    {
        string hookId = e.HookId;
        
        // Parse rackId, floor, hook from hookId "Rack1-T1-M1"
        var parts = hookId.Split('-');
        if (parts.Length != 3) return;
        
        string rackId = parts[0];
        int floor = int.Parse(parts[1].Substring(1));
        int hookIndex = int.Parse(parts[2].Substring(1));

        var rowData = _view.GetRowData(rackId, floor, hookIndex);
        if (rowData == null) return;

        // Chỉ lưu DB nếu người dùng đã điền Nart hoặc Batch
        bool hasInputs = !string.IsNullOrWhiteSpace(rowData.Nart) || !string.IsNullOrWhiteSpace(rowData.Batch);
        if (!hasInputs) return;

        var record = new TestRecord
        {
            RackId = rackId,
            Floor = floor,
            HookIndex = hookIndex,
            HookId = hookId,
            BatchCode = rowData.Batch,
            NartCode = rowData.Nart,
            Tester = rowData.Tester,
            Location = rowData.Location,
            DropTime = e.DropTime,
            PlcValue = e.LastValue, // Need to add LastValue to TestCompletedEventArgs
            CompletedAt = DateTime.Now
        };

        await _testRepo.SaveTestRecordAsync(record);
        _view.AddTestRecord(record);
        _view.FlashRackTab(rackId);
    }

    private async void OnDeleteSelectedRecordsClicked(List<int> ids)
    {
        await _testRepo.DeleteTestRecordsAsync(ids);
    }

    private async void OnExportHistoryClicked(string? rackId)
    {
        var history = await _testRepo.GetAllTestRecordsAsync();
        
        if (rackId != null)
        {
            history = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(history, r => r.RackId == rackId));
        }

        bool exportSuccess = _excelService.ExportReport(history, out string savedFilePath);
        if (exportSuccess)
        {
            MessageBox.Show($"Báo cáo được lưu thành công tại:\n{savedFilePath}", "Hoàn thành", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void OnSettingsClicked(string rackId)
    {
        using var settingsForm = new SettingsForm(rackId);
        settingsForm.ShowDialog();
    }

    private void OnScannerClicked(string rackId)
    {
        var plcService = _plcManager.GetService(rackId);
        using var scannerForm = new ScannerForm(rackId, plcService);
        scannerForm.ShowDialog();
    }

    public void Dispose()
    {
        StopAllPolling();
        _plcManager.Dispose();
        _testRepo.Dispose();
    }
}
