using System;
using System.Threading;
using System.Threading.Tasks;
using TapeAdhesionApp.Core.Models;
using TapeAdhesionApp.Core.PLC;
using TapeAdhesionApp.Core.State;
using TapeAdhesionApp.UI.Views;

namespace TapeAdhesionApp.UI.Presenters;

public class MainPresenter : IDisposable
{
    private readonly IMainView _view;
    private readonly IPlcService _plcService;
    private readonly IStateMachine _stateMachine;
    
    private CancellationTokenSource? _pollingCts;
    private bool _isTestRunning = false;

    public MainPresenter(IMainView view, IPlcService plcService, IStateMachine stateMachine)
    {
        _view = view;
        _plcService = plcService;
        _stateMachine = stateMachine;
        
        // Đăng ký sự kiện từ View
        _view.StartTestClicked += OnStartTestClicked;
        _view.StopTestClicked += OnStopTestClicked;
        _view.ResetClicked += OnResetClicked;
        _view.InputsChanged += OnInputsChanged;
        
        // Đăng ký sự kiện từ StateMachine
        _stateMachine.OnTestCompleted += StateMachine_OnTestCompleted;
        
        // Khởi tạo giao diện ban đầu
        _view.EnableStartButton(false);
    }

    private void OnInputsChanged(object? sender, EventArgs e)
    {
        // Ràng buộc nhập liệu: Bắt buộc phải có Batch và Nart mới cho Start
        bool hasInputs = !string.IsNullOrWhiteSpace(_view.BatchCode) && !string.IsNullOrWhiteSpace(_view.NartCode);
        _view.EnableStartButton(hasInputs && !_isTestRunning);
    }

    private void OnStartTestClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_view.BatchCode) || string.IsNullOrWhiteSpace(_view.NartCode)) return;

        _isTestRunning = true;
        _view.EnableStartButton(false);
        _stateMachine.Reset();
        
        StartPollingLoop();
    }

    private void OnStopTestClicked(object? sender, EventArgs e)
    {
        StopPollingLoop();
        _isTestRunning = false;
        _stateMachine.Reset();
        
        // Kích hoạt lại nút Start nếu input vẫn hợp lệ
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

        // Vòng lặp Polling chạy trên Background Thread
        Task.Run(async () =>
        {
            if (!_plcService.IsConnected)
            {
                // TODO: Load IP từ cấu hình ở Giai đoạn 4
                await _plcService.ConnectAsync("192.168.0.1"); 
            }

            while (!token.IsCancellationRequested)
            {
                // 1. Lấy dữ liệu từ PLC
                var plcData = await _plcService.ReadPlcDataAsync();
                
                // 2. Chạy tick State Machine để bắt lọc nhiễu
                if (plcData != null)
                {
                    _stateMachine.Tick(plcData);
                }

                // 3. Update UI an toàn (View sẽ lo phần Invoke Required)
                UpdateViewSafe(plcData);

                // 4. Nghỉ 500ms
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

        // Bắt buộc đẩy việc cập nhật UI sang luồng chính qua hàm InvokeOnUI do View cung cấp
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

    private void StateMachine_OnTestCompleted(object? sender, TestCompletedEventArgs e)
    {
        var newRecord = new TestRecord
        {
            BatchCode = _view.BatchCode,
            NartCode = _view.NartCode,
            DropTime = e.FinalDropTime,
            CompletedAt = e.CompletedAt
        };

        // Giao việc Add vào BindingList cho View để đảm bảo Thread-Safety
        _view.AddTestRecord(newRecord);
    }

    public void Dispose()
    {
        StopPollingLoop();
    }
}
