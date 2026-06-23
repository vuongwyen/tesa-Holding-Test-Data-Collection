using S7.Net;
using System;
using System.Threading.Tasks;
using TapeAdhesionApp.Core.Models;

namespace TapeAdhesionApp.Core.PLC;

public class PlcCommunicationService : IPlcService, IDisposable
{
    private Plc? _plc;
    private string _ipAddress = string.Empty;
    private bool _isConnecting = false;

    public bool IsConnected => _plc != null && _plc.IsConnected;

    public async Task<bool> ConnectAsync(string ipAddress)
    {
        if (_isConnecting) return false;
        
        _ipAddress = ipAddress;
        _isConnecting = true;

        try
        {
            if (_plc != null)
            {
                _plc.Close();
                _plc = null;
            }

            // Mặc định sử dụng Rack 0, Slot 1 cho S7-200.
            _plc = new Plc(CpuType.S7200, ipAddress, 0, 1);
            
            // Chạy hàm Open trên background thread/task pool để không block UI (tránh UI freeze)
            await Task.Run(() => _plc.Open());
            
            return _plc.IsConnected;
        }
        catch (Exception ex)
        {
            // TODO: Ghi log ra file local (Offline Log)
            Console.WriteLine($"[PLC Connect Error] {ex.Message}");
            return false;
        }
        finally
        {
            _isConnecting = false;
        }
    }

    public void Disconnect()
    {
        if (_plc != null && _plc.IsConnected)
        {
            _plc.Close();
        }
    }

    public async Task<PlcData?> ReadPlcDataAsync()
    {
        if (!IsConnected)
        {
            return await TryAutoReconnectAsync();
        }

        try
        {
            return await Task.Run(() =>
            {
                var data = new PlcData { IsConnected = true };
                
                // Đọc một mảng byte (ví dụ 400 bytes) từ DB1 (tương đương V-Memory trên S7-200)
                byte[] buffer = _plc!.ReadBytes(DataType.DataBlock, PlcTags.VMemoryDataBlock, 0, 400);

                // Parse giá trị theo các hằng số mapping đã định nghĩa
                data.MachineState = S7.Net.Types.Boolean.GetValue(buffer[PlcTags.V0_0_MachineState], 0);
                data.DropSignal = S7.Net.Types.Boolean.GetValue(buffer[PlcTags.V0_1_DropSignal], 1);
                
                // Đọc DWord (4 bytes)
                data.DropTime = S7.Net.Types.DWord.FromByteArray(
                    new byte[] { buffer[PlcTags.VD10_DropTime], buffer[PlcTags.VD10_DropTime + 1], buffer[PlcTags.VD10_DropTime + 2], buffer[PlcTags.VD10_DropTime + 3] }
                );
                
                data.Variable110 = S7.Net.Types.DWord.FromByteArray(
                    new byte[] { buffer[PlcTags.VD110_Variable], buffer[PlcTags.VD110_Variable + 1], buffer[PlcTags.VD110_Variable + 2], buffer[PlcTags.VD110_Variable + 3] }
                );

                data.Variable368 = S7.Net.Types.DWord.FromByteArray(
                    new byte[] { buffer[PlcTags.VD368_Variable], buffer[PlcTags.VD368_Variable + 1], buffer[PlcTags.VD368_Variable + 2], buffer[PlcTags.VD368_Variable + 3] }
                );

                return data;
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PLC Read Error] {ex.Message}");
            // Mất kết nối đột ngột, trả về đối tượng có cờ IsConnected = false để StateMachine bỏ qua hoặc UI báo lỗi
            return new PlcData { IsConnected = false };
        }
    }

    private async Task<PlcData?> TryAutoReconnectAsync()
    {
        if (_isConnecting || string.IsNullOrEmpty(_ipAddress)) 
            return new PlcData { IsConnected = false };

        Console.WriteLine("[PLC] Mất kết nối! Đang thử kết nối lại (Auto-Reconnect)...");
        
        // Trễ 2 giây để tránh làm quá tải module Wi-Fi của PLC, đây là cấu hình an toàn cho mạng nhà máy
        await Task.Delay(2000);
        
        await ConnectAsync(_ipAddress);
        return new PlcData { IsConnected = IsConnected };
    }

    public async Task<bool> WriteBitAsync(int byteAddress, int bitAddress, bool value)
    {
        if (!IsConnected) return false;

        try
        {
            await Task.Run(() => 
            {
                _plc!.WriteBit(DataType.DataBlock, PlcTags.VMemoryDataBlock, byteAddress, bitAddress, value);
            });
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PLC Write Error] {ex.Message}");
            return false;
        }
    }

    private bool _disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Disconnect();
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
