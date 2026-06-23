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

    public void Dispose()
    {
        if (_connection != null)
        {
            _connection.Dispose();
            _connection = null;
        }
    }
}
