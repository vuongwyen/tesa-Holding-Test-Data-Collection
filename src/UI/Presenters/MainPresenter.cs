using System;
using System.ComponentModel;
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
    private readonly IPlcService _plcService;
    private readonly IStateMachine _stateMachine;
    private readonly TestRepository _testRepo;
    private readonly ExcelReportService _excelService;
    
    private CancellationTokenSource? _pollingCts;
    private bool _isTestRunning = false;
    private string _currentSessionId = string.Empty;
    private DateTime _lastCheckpointTime = DateTime.MinValue;

    public MainPresenter(
        IMainView view, 
        IPlcService plcService, 
        IStateMachine stateMachine,
        TestRepository testRepo,
        ExcelReportService excelService)
    {
        _view = view;
        _plcService = plcService;
        _stateMachine = stateMachine;
        _testRepo = testRepo;
        _excelService = excelService;
        
        // Đăng ký sự kiện từ View
        _view.StartTestClicked += OnStartTestClicked;
        _view.StopTestClicked += OnStopTestClicked;
        _view.ResetClicked += OnResetClicked;
        _view.InputsChanged += OnInputsChanged;
        
        if (_view is Form form)
        {
            form.Load += async (s, e) => await InitializeAsync();
        }
        
        // Đăng ký sự kiện từ StateMachine
        _stateMachine.OnTestCompleted += StateMachine_OnTestCompleted;
        
        // Khởi tạo giao diện ban đầu
        _view.EnableStartButton(false);
    }

    private async Task InitializeAsync()
    {
        // Load lịch sử cũ
        var history = await _testRepo.GetAllTestRecordsAsync();
        foreach (var record in history)
        {
            _view.AddTestRecord(record);
        }

        // Kiểm tra crash recovery
        var checkpoint = await _testRepo.GetIncompleteCheckpointAsync();
        if (checkpoint != null)
        {
            _view.InvokeOnUI(() => 
            {
                MessageBox.Show("Phát hiện phiên kiểm tra bị gián đoạn do mất điện/crash. Đang phục hồi dữ liệu...", 
                    "Crash Recovery", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            });

            _view.SetInputs((string)checkpoint.BatchCode, (string)checkpoint.NartCode);
            // Optionally auto-start polling to resume monitoring
            // OnStartTestClicked(this, EventArgs.Empty);
        }
    }

    private void OnInputsChanged(object? sender, EventArgs e)
    {
        bool hasInputs = !string.IsNullOrWhiteSpace(_view.BatchCode) && !string.IsNullOrWhiteSpace(_view.NartCode);
        _view.EnableStartButton(hasInputs && !_isTestRunning);
    }

    private async void OnStartTestClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_view.BatchCode) || string.IsNullOrWhiteSpace(_view.NartCode)) return;

        _isTestRunning = true;
        _currentSessionId = Guid.NewGuid().ToString();
        _view.EnableStartButton(false);
        _stateMachine.Reset();
        
        // Clear old checkpoints
        await _testRepo.ClearAllCheckpointsAsync();
        
        StartPollingLoop();
    }

    private async void OnStopTestClicked(object? sender, EventArgs e)
    {
        StopPollingLoop();
        _isTestRunning = false;
        _stateMachine.Reset();
        
        await _testRepo.ClearAllCheckpointsAsync();
        
        OnInputsChanged(this, EventArgs.Empty);
    }

    private void OnResetClicked(object? sender, EventArgs e)
    {
        _stateMachine.Reset();
    }

    private void StartPollingLoop()
    {
        if (_pollingCts != null) return;
        
        _pollingCts = new CancellationTokenSource();
        var token = _pollingCts.Token;

        Task.Run(async () =>
        {
            if (!_plcService.IsConnected)
            {
                await _plcService.ConnectAsync("192.168.0.1"); 
            }

            while (!token.IsCancellationRequested)
            {
                var plcData = await _plcService.ReadPlcDataAsync();
                
                if (plcData != null)
                {
                    _stateMachine.Tick(plcData);
                    
                    // Logic Checkpoint 5s
                    if (_stateMachine.CurrentState == MachineState.RUNNING)
                    {
                        if ((DateTime.Now - _lastCheckpointTime).TotalSeconds >= 5)
                        {
                            await _testRepo.SaveCheckpointAsync(_currentSessionId, _view.BatchCode, _view.NartCode, plcData.DropTime);
                            _lastCheckpointTime = DateTime.Now;
                        }
                    }
                }

                UpdateViewSafe(plcData);
                await Task.Delay(500, token);
            }
        }, token);
    }

    private void StopPollingLoop()
    {
        if (_pollingCts != null)
        {
            _pollingCts.Cancel();
            _pollingCts.Dispose();
            _pollingCts = null;
        }
        
        _plcService.Disconnect();
        
        _view.InvokeOnUI(() => 
        {
            _view.UpdatePlcConnectionStatus(false);
            _view.UpdateMachineState("OFFLINE");
        });
    }

    private void UpdateViewSafe(PlcData? data)
    {
        if (data == null) return;

        _view.InvokeOnUI(() => 
        {
            _view.UpdatePlcConnectionStatus(data.IsConnected);
            _view.UpdateMachineState(_stateMachine.CurrentState.ToString());
            
            if (data.IsConnected)
            {
                _view.UpdateRunningTime($"{data.DropTime} ms");
                _view.UpdateSamplePosition($"VD110: {data.Variable110} | VD368: {data.Variable368}");
            }
        });
    }

    private async void StateMachine_OnTestCompleted(object? sender, TestCompletedEventArgs e)
    {
        // 1. Lưu Record hoàn chỉnh
        var newRecord = new TestRecord
        {
            BatchCode = _view.BatchCode,
            NartCode = _view.NartCode,
            DropTime = e.FinalDropTime,
            CompletedAt = e.CompletedAt
        };

        await _testRepo.SaveTestRecordAsync(newRecord);
        
        // 2. Xóa Checkpoint của phiên này
        await _testRepo.ClearCheckpointAsync(_currentSessionId);

        // 3. Cập nhật UI
        _view.AddTestRecord(newRecord);

        // 4. Xuất Excel tự động
        var history = await _testRepo.GetAllTestRecordsAsync();
        bool exportSuccess = _excelService.ExportReport(_view.BatchCode, _view.NartCode, history, out string savedFilePath);
        
        if (exportSuccess)
        {
            _view.InvokeOnUI(() => 
            {
                MessageBox.Show($"Test hoàn thành! Đã lưu báo cáo tại:\n{savedFilePath}", "Hoàn thành", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }
        
        _isTestRunning = false;
        _view.InvokeOnUI(() => OnInputsChanged(this, EventArgs.Empty));
    }

    public void Dispose()
    {
        StopPollingLoop();
    }
}
