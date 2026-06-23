using System;
using TapeAdhesionApp.Core.Models;

namespace TapeAdhesionApp.Core.State;

public class StateMachine : IStateMachine
{
    public MachineState CurrentState { get; private set; } = MachineState.IDLE;
    public event EventHandler<TestCompletedEventArgs>? OnTestCompleted;

    private uint _lastVariable110;
    private uint _lastVariable368;
    
    // Bộ đếm thời gian cho chức năng lọc nhiễu (Debounce)
    private DateTime? _debounceStartTime;
    private DateTime? _debounceStopTime;

    // Ngưỡng thời gian xác nhận thay đổi là thật, không phải nhiễu (1 giây)
    private readonly TimeSpan DebounceStartThreshold = TimeSpan.FromSeconds(1);
    private readonly TimeSpan DebounceStopThreshold = TimeSpan.FromSeconds(1);

    public void Tick(PlcData data)
    {
        if (data == null || !data.IsConnected)
        {
            // Bỏ qua nếu mất kết nối. Cơ chế Crash Recovery (Checkpoint) sẽ xử lý khôi phục sau.
            return;
        }

        // Kiểm tra xem vị trí mẫu / thông số đang test có thay đổi liên tục không
        bool isPositionChanging = data.Variable110 != _lastVariable110 || data.Variable368 != _lastVariable368;
        
        // Tín hiệu tạ rơi từ biến Bool hoặc một điều kiện tương tự
        bool isDropDetected = data.DropSignal;

        switch (CurrentState)
        {
            case MachineState.IDLE:
                if (isPositionChanging)
                {
                    // Bắt đầu thấy có sự thay đổi, chuyển sang chờ xác nhận (tránh nhiễu tay chạm)
                    CurrentState = MachineState.DEBOUNCE_START;
                    _debounceStartTime = data.Timestamp;
                }
                break;

            case MachineState.DEBOUNCE_START:
                if (!isPositionChanging)
                {
                    // Không còn thay đổi nữa -> Là nhiễu (False Start), quay về IDLE
                    CurrentState = MachineState.IDLE;
                    _debounceStartTime = null;
                }
                else
                {
                    // Nếu vẫn tiếp tục thay đổi và vượt qua ngưỡng 1 giây
                    if (_debounceStartTime.HasValue && (data.Timestamp - _debounceStartTime.Value) >= DebounceStartThreshold)
                    {
                        CurrentState = MachineState.RUNNING;
                        _debounceStartTime = null;
                        // (Ở Giai đoạn 4 sẽ gọi logic lưu Checkpoint Database tại đây định kỳ mỗi 5s)
                    }
                }
                break;

            case MachineState.RUNNING:
                if (isDropDetected)
                {
                    // Bắt đầu phát hiện tạ rơi, chờ xác nhận (tránh tạ nảy lên nảy xuống)
                    CurrentState = MachineState.DEBOUNCE_STOP;
                    _debounceStopTime = data.Timestamp;
                }
                break;

            case MachineState.DEBOUNCE_STOP:
                if (!isDropDetected)
                {
                    // Tín hiệu tạ rơi bị mất -> Là do nảy/nhiễu, quay lại trạng thái RUNNING
                    CurrentState = MachineState.RUNNING;
                    _debounceStopTime = null;
                }
                else
                {
                    // Nếu tín hiệu tạ rơi ổn định vượt quá ngưỡng 1 giây
                    if (_debounceStopTime.HasValue && (data.Timestamp - _debounceStopTime.Value) >= DebounceStopThreshold)
                    {
                        CurrentState = MachineState.COMPLETED;
                        _debounceStopTime = null;
                        
                        // Kích hoạt sự kiện hoàn thành bài test để UI hoặc Database bắt lấy
                        OnTestCompleted?.Invoke(this, new TestCompletedEventArgs
                        {
                            FinalDropTime = data.DropTime,
                            CompletedAt = data.Timestamp
                        });
                    }
                }
                break;

            case MachineState.COMPLETED:
                // Đợi lệnh Reset từ giao diện hoặc PLC để quay về IDLE
                break;
        }

        // Cập nhật giá trị cũ để so sánh cho chu kỳ Tick tiếp theo
        _lastVariable110 = data.Variable110;
        _lastVariable368 = data.Variable368;
    }

    public void Reset()
    {
        CurrentState = MachineState.IDLE;
        _debounceStartTime = null;
        _debounceStopTime = null;
        _lastVariable110 = 0;
        _lastVariable368 = 0;
    }
}
