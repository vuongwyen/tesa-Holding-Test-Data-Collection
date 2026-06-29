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
        
        _isConnecting = true;
        _ipAddress = ipAddress;
        
        try
        {
            await Task.Run(() =>
            {
                _plc = new Plc(CpuType.S7200Smart, ipAddress, 0, 1); // Sử dụng chuẩn S7-200 Smart
                _plc.Open();
            });

            return IsConnected;
        }
        catch (Exception ex)
        {
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
                
                int[] addresses = PlcTags.GetAddresses(RackId);
                int maxAddress = 0;
                for (int i = 0; i < 64; i++)
                {
                    if (addresses[i] > maxAddress)
                        maxAddress = addresses[i];
                }

                // Read exactly up to maxAddress + 4 to cover all configured tags
                int bytesToRead = maxAddress + 4;
                if (bytesToRead < 4) bytesToRead = 4; // minimum

                byte[] buffer = _plc!.ReadBytes(DataType.DataBlock, PlcTags.VMemoryDataBlock, 0, bytesToRead);

                for (int i = 0; i < 64; i++)
                {
                    int address = addresses[i];
                    uint currentValue = 0;

                    if (address >= 0 && address + 3 < buffer.Length)
                    {
                        currentValue = S7.Net.Types.DWord.FromByteArray(
                            new byte[] { buffer[address], buffer[address + 1], buffer[address + 2], buffer[address + 3] }
                        );
                    }

                    data.Hooks[i].CurrentValue = currentValue;
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

    public async Task<byte[]?> ReadRawBytesAsync(int startAddress, int length)
    {
        if (!IsConnected) return null;
        try
        {
            return await Task.Run(() => 
            {
                return _plc!.ReadBytes(DataType.DataBlock, PlcTags.VMemoryDataBlock, startAddress, length);
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PLC ReadRaw Error] {ex.Message}");
            return null;
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
