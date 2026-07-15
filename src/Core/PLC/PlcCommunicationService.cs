using S7.Net;
using System;
using System.Buffers.Binary;
using System.Threading;
using System.Threading.Tasks;
using TapeAdhesionApp.Core.Models;

namespace TapeAdhesionApp.Core.PLC;

public class PlcCommunicationService : IPlcService, IDisposable
{
    private Plc? _plc;
    private string _ipAddress = string.Empty;
    private bool _isConnecting = false;
    private readonly SemaphoreSlim _plcLock = new SemaphoreSlim(1, 1);
    private readonly PlcValueSanitizer _sanitizer = new();
    private bool _isFirstRead = true;

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
            await Task.Run(async () =>
            {
                await _plcLock.WaitAsync();
                try
                {
                    _plc = new Plc(CpuType.S7200Smart, ipAddress, 0, 1); // Sử dụng chuẩn S7-200 Smart
                    _plc.Open();
                }
                finally
                {
                    _plcLock.Release();
                }
            });

            return IsConnected;
        }
        catch (PlcException plcEx)
        {
            System.Diagnostics.Debug.WriteLine($"[PLC Connect Error] Lỗi giao thức S7: {plcEx.Message}");
            return false;
        }
        catch (System.Net.Sockets.SocketException sockEx)
        {
            System.Diagnostics.Debug.WriteLine($"[PLC Connect Error] Lỗi kết nối mạng: {sockEx.Message}");
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PLC Connect Error] Lỗi không xác định: {ex.Message}");
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
            _plcLock.Wait();
            try
            {
                _plc.Close();
            }
            finally
            {
                _plcLock.Release();
            }
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

                byte[] buffer = new byte[bytesToRead];
                _plcLock.Wait();
                try
                {
                    int offset = 0;
                    int chunkSize = 200; // PDU size limit for S7-200 Smart is around 240 bytes
                    while (offset < bytesToRead)
                    {
                        int size = Math.Min(chunkSize, bytesToRead - offset);
                        byte[] chunk = _plc!.ReadBytes(DataType.DataBlock, PlcTags.VMemoryDataBlock, offset, size);
                        if (chunk != null && chunk.Length == size)
                        {
                            Array.Copy(chunk, 0, buffer, offset, size);
                        }
                        offset += size;
                    }
                }
                finally
                {
                    _plcLock.Release();
                }

                for (int i = 0; i < 64; i++)
                {
                    int address = addresses[i];
                    uint currentValue = 0;

                    if (address >= 0 && address + 3 < buffer.Length)
                    {
                        currentValue = BinaryPrimitives.ReadUInt32BigEndian(
                            new ReadOnlySpan<byte>(buffer, address, 4)
                        );
                    }

                    var (sanitizedVal, isGood) = _sanitizer.Sanitize(address, currentValue, _isFirstRead);

                    data.Hooks[i].RawValue = currentValue;
                    data.Hooks[i].CurrentValue = sanitizedVal;
                    data.Hooks[i].IsGood = isGood;
                }

                _isFirstRead = false;
                return data;
            });
        }
        catch (PlcException plcEx)
        {
            System.Diagnostics.Debug.WriteLine($"[PLC Read Error] Lỗi giao thức S7 (Có thể PLC từ chối do quá tải kết nối): {plcEx.Message}");
            return new PlcData(RackId) { IsConnected = false };
        }
        catch (System.Net.Sockets.SocketException sockEx)
        {
            System.Diagnostics.Debug.WriteLine($"[PLC Read Error] Mất kết nối mạng: {sockEx.Message}");
            return new PlcData(RackId) { IsConnected = false };
        }
        catch (IndexOutOfRangeException idxEx)
        {
            System.Diagnostics.Debug.WriteLine($"[PLC Read Error] Lỗi ranh giới vùng nhớ (vượt PDU hoặc địa chỉ sai): {idxEx.Message}");
            return new PlcData(RackId) { IsConnected = true }; // Vẫn keep connection, nhưng mảng bị lỗi
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PLC Read Error] Lỗi chung: {ex.Message}");
            // Mất kết nối đột ngột, trả về đối tượng có cờ IsConnected = false để StateMachine bỏ qua hoặc UI báo lỗi
            return new PlcData(RackId) { IsConnected = false };
        }
    }

    private async Task<PlcData?> TryAutoReconnectAsync()
    {
        if (_isConnecting || string.IsNullOrEmpty(_ipAddress)) 
            return new PlcData(RackId) { IsConnected = false };

        System.Diagnostics.Debug.WriteLine($"[PLC {RackId}] Mất kết nối! Đang thử kết nối lại (Auto-Reconnect)...");
        
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
            await Task.Run(async () => 
            {
                await _plcLock.WaitAsync();
                try
                {
                    _plc!.WriteBit(DataType.DataBlock, PlcTags.VMemoryDataBlock, byteAddress, bitAddress, value);
                }
                finally
                {
                    _plcLock.Release();
                }
            });
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PLC Write Error] {ex.Message}");
            return false;
        }
    }

    public async Task<byte[]?> ReadRawBytesAsync(int startAddress, int length)
    {
        if (!IsConnected) return null;
        try
        {
            return await Task.Run(async () => 
            {
                byte[] buffer = new byte[length];
                await _plcLock.WaitAsync();
                try
                {
                    int offset = 0;
                    int chunkSize = 200;
                    while (offset < length)
                    {
                        int size = Math.Min(chunkSize, length - offset);
                        byte[] chunk = _plc!.ReadBytes(DataType.DataBlock, PlcTags.VMemoryDataBlock, startAddress + offset, size);
                        if (chunk != null && chunk.Length == size)
                        {
                            Array.Copy(chunk, 0, buffer, offset, size);
                        }
                        else
                        {
                            return null; // Lỗi đọc 1 chunk -> hỏng toàn bộ
                        }
                        offset += size;
                    }
                    return buffer;
                }
                finally
                {
                    _plcLock.Release();
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PLC ReadRaw Error] {ex.Message}");
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
