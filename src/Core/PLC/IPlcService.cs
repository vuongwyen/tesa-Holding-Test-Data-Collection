using System.Threading.Tasks;
using TapeAdhesionApp.Core.Models;

namespace TapeAdhesionApp.Core.PLC;

public interface IPlcService
{
    bool IsConnected { get; }
    
    Task<bool> ConnectAsync(string ipAddress);
    void Disconnect();
    
    // Reads a block of data representing the current state of V-Memory
    Task<PlcData?> ReadPlcDataAsync();
    
    // Write a boolean to a specific bit address (e.g., V1.0)
    Task<bool> WriteBitAsync(int byteAddress, int bitAddress, bool value);
}
