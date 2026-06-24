using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TapeAdhesionApp.Core.PLC;

public class PlcManager : IDisposable
{
    private readonly ConcurrentDictionary<string, PlcCommunicationService> _services = new();

    public PlcManager()
    {
    }

    public PlcCommunicationService GetService(string rackId)
    {
        return _services.GetOrAdd(rackId, id => new PlcCommunicationService(id));
    }

    public async Task<bool> ConnectRackAsync(string rackId, string ipAddress)
    {
        var service = GetService(rackId);
        return await service.ConnectAsync(ipAddress);
    }

    public void DisconnectRack(string rackId)
    {
        if (_services.TryGetValue(rackId, out var service))
        {
            service.Disconnect();
        }
    }

    public bool IsConnected(string rackId)
    {
        if (_services.TryGetValue(rackId, out var service))
        {
            return service.IsConnected;
        }
        return false;
    }

    public void Dispose()
    {
        foreach (var service in _services.Values)
        {
            service.Dispose();
        }
        _services.Clear();
    }
}
