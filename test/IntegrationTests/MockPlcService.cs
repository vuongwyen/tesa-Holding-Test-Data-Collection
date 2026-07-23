using System;
using System.Threading.Tasks;
using TapeAdhesionApp.Core.Models;
using TapeAdhesionApp.Core.PLC;

namespace IntegrationTests;

public class MockPlcService : IPlcService
{
    public bool IsConnected { get; private set; } = true;
    public bool SimulateNetworkDrop { get; set; } = false;
    
    // Giả lập 64 giá trị VD
    public uint[] HookValues = new uint[64];
    public int DelayMs { get; set; } = 0;
    public string RackId { get; }

    public MockPlcService(string rackId)
    {
        RackId = rackId;
    }

    public Task<bool> ConnectAsync(string ipAddress)
    {
        IsConnected = true;
        return Task.FromResult(true);
    }

    public void Disconnect()
    {
        IsConnected = false;
    }

    public async Task<PlcData?> ReadPlcDataAsync()
    {
        if (SimulateNetworkDrop)
        {
            IsConnected = false;
            throw new Exception("Mạng đứt ngang xương (Mô phỏng đứt cáp PLC)");
        }

        if (!IsConnected)
        {
            throw new Exception("Chưa kết nối tới PLC");
        }

        if (DelayMs > 0)
        {
            await Task.Delay(DelayMs);
        }

        var plcData = new PlcData(RackId) { IsConnected = true };
        for (int i = 0; i < 64; i++)
        {
            plcData.Hooks[i].RawValue = HookValues[i];
            // Sanitizer trong PlcManager thật ra cần RawValue?
            // Wait, trong luồng thật, PlcCommunicationService đọc raw bytes, parse ra RawValue và gọi Sanitizer.
            // Vì MockPlcService mock IPlcService, nên nó thay thế PlcCommunicationService.
            // Vậy MockPlcService phải tự làm việc sanitize?
            // Hoặc PlcData sẽ được trả về trực tiếp cho PlcManager.
            // PlcManager chỉ việc nhận PlcData và truyền cho StateMachine.
            // Nhưng trong app hiện tại, PlcCommunicationService mới là người gọi Sanitizer!
            // Ta cứ truyền thẳng vào CurrentValue coi như đã được xử lý (Mock pass-through).
            plcData.Hooks[i].CurrentValue = HookValues[i]; 
            plcData.Hooks[i].IsGood = true;
        }

        return plcData;
    }

    public Task<bool> WriteBitAsync(int byteAddress, int bitAddress, bool value)
    {
        return Task.FromResult(true);
    }
}
