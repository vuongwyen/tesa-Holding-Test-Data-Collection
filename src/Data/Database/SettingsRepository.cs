using Dapper;
using Microsoft.Data.Sqlite;

namespace TapeAdhesionApp.Data.Database;

public class SettingsRepository : IDisposable
{
    private readonly string _connectionString = DatabaseInitializer.ConnectionString;
    private SqliteConnection? _connection;

    public SettingsRepository()
    {
        _connection = new SqliteConnection(_connectionString);
        _connection.Open();
    }

    public async Task<string?> GetSettingAsync(string key)
    {
        string sql = "SELECT Value FROM Settings WHERE Key = @Key";
        return await _connection.QuerySingleOrDefaultAsync<string>(sql, new { Key = key });
    }

    public async Task SaveSettingAsync(string key, string value)
    {
        string sql = @"
            INSERT INTO Settings (Key, Value) 
            VALUES (@Key, @Value) 
            ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value";
        
        await _connection.ExecuteAsync(sql, new { Key = key, Value = value });
    }

    public async Task<string> GetRackIpAsync(string rackId, string defaultIp = "192.168.2.1")
    {
        using var connection = new SqliteConnection(_connectionString);
        string sql = "SELECT Value FROM Settings WHERE Key = @Key";
        var ip = await connection.QueryFirstOrDefaultAsync<string>(sql, new { Key = $"PlcIpAddress_{rackId}" });
        return string.IsNullOrEmpty(ip) ? defaultIp : ip;
    }

    public async Task SaveRackIpAsync(string rackId, string ipAddress)
    {
        using var connection = new SqliteConnection(_connectionString);
        string sql = @"
            INSERT INTO Settings (Key, Value) VALUES (@Key, @Value)
            ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value";
        
        await connection.ExecuteAsync(sql, new { Key = $"PlcIpAddress_{rackId}", Value = ipAddress });
    }

    public async Task<int[]> GetPlcAddressesAsync(string rackId)
    {
        using var connection = new SqliteConnection(_connectionString);
        string sql = "SELECT Value FROM Settings WHERE Key = @Key";
        var json = await connection.QueryFirstOrDefaultAsync<string>(sql, new { Key = $"PlcAddresses_{rackId}" });
        
        if (string.IsNullOrEmpty(json))
        {
            // Mặc định trả về mảng -1 nếu chưa có
            int[] defaultAddresses = new int[64];
            for (int i = 0; i < 64; i++) defaultAddresses[i] = -1;
            return defaultAddresses;
        }

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<int[]>(json) ?? new int[64];
        }
        catch
        {
            return new int[64];
        }
    }

    public async Task SavePlcAddressesAsync(string rackId, int[] addresses)
    {
        string json = System.Text.Json.JsonSerializer.Serialize(addresses);
        using var connection = new SqliteConnection(_connectionString);
        string sql = @"
            INSERT INTO Settings (Key, Value) VALUES (@Key, @Value)
            ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value";
        
        await connection.ExecuteAsync(sql, new { Key = $"PlcAddresses_{rackId}", Value = json });
    }

    public void Dispose()
    {
        if (_connection != null)
        {
            _connection.Dispose();
            _connection = null;
        }
    }
}
