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

    public string RackId { get; }
    public bool IsConnected => _plc != null && _plc.IsConnected;

    public PlcCommunicationService(string rackId)
    {
        RackId = rackId;
    }

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

            // Sử dụng chuẩn S7-200 Smart (Phù hợp với thiết bị thực tế tại xưởng)
            _plc = new Plc(CpuType.S7200Smart, ipAddress, 0, 1);
            
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
                var data = new PlcData(RackId) { IsConnected = true };
                
                // Đọc 400 bytes từ DB1 (V-Memory trên S7-200) để bao trùm cả VD110 và VD368
                byte[] buffer = _plc!.ReadBytes(DataType.DataBlock, PlcTags.VMemoryDataBlock, 0, 400);

                for (int i = 0; i < 64; i++)
                {
                    int address = PlcTags.HookAddresses[i];
                    if (address >= 0 && address + 3 < buffer.Length)
                    {
                        data.Hooks[i].CurrentValue = S7.Net.Types.DWord.FromByteArray(
                            new byte[] { buffer[address], buffer[address + 1], buffer[address + 2], buffer[address + 3] }
                        );
                    }
                    else
                    {
                        data.Hooks[i].CurrentValue = 0; // Dummy
                    }
                }

                return data;
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PLC Read Error] {ex.Message}");
            // Mất kết nối đột ngột, trả về đối tượng có cờ IsConnected = false để StateMachine bỏ qua hoặc UI báo lỗi
            return new PlcData(RackId) { IsConnected = false };
        }
    }

    private async Task<PlcData?> TryAutoReconnectAsync()
    {
        if (_isConnecting || string.IsNullOrEmpty(_ipAddress)) 
            return new PlcData(RackId) { IsConnected = false };

        Console.WriteLine($"[PLC {RackId}] Mất kết nối! Đang thử kết nối lại (Auto-Reconnect)...");
        
        // Trễ 2 giây để tránh làm quá tải module Wi-Fi của PLC, đây là cấu hình an toàn cho mạng nhà máy
        await Task.Delay(2000);
        
        await ConnectAsync(_ipAddress);
        return new PlcData(RackId) { IsConnected = IsConnected };
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
