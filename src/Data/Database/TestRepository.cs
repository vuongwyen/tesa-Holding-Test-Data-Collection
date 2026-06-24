using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TapeAdhesionApp.Core.Models;

namespace TapeAdhesionApp.Data.Database;

public class TestRepository : IDisposable
{
    private readonly string _connectionString;
    private bool _disposedValue;

    public TestRepository()
    {
        _connectionString = DatabaseInitializer.ConnectionString;
    }

    public async Task SaveTestRecordAsync(TestRecord record)
    {
        using var connection = new SqliteConnection(_connectionString);
        string sql = @"
            INSERT INTO TestRecords (RackId, Floor, HookIndex, HookId, BatchCode, NartCode, Tester, Location, SamplePosition, DropTime, PlcValue, CompletedAt) 
            VALUES (@RackId, @Floor, @HookIndex, @HookId, @BatchCode, @NartCode, @Tester, @Location, @SamplePosition, @DropTime, @PlcValue, @CompletedAt)";
        
        await connection.ExecuteAsync(sql, new
        {
            record.RackId,
            record.Floor,
            record.HookIndex,
            record.HookId,
            record.BatchCode,
            record.NartCode,
            record.Tester,
            record.Location,
            record.SamplePosition,
            record.DropTime,
            record.PlcValue,
            CompletedAt = record.CompletedAt.ToString("O") // ISO 8601 format
        });
    }

    public async Task<IEnumerable<TestRecord>> GetAllTestRecordsAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        string sql = "SELECT * FROM TestRecords ORDER BY CompletedAt DESC";
        
        var rawData = await connection.QueryAsync(sql);
        
        return rawData.Select(d => new TestRecord
        {
            Id = Convert.ToInt32(d.Id),
            RackId = d.RackId == null ? "" : (string)d.RackId,
            Floor = d.Floor == null ? 0 : Convert.ToInt32(d.Floor),
            HookIndex = d.HookIndex == null ? 0 : Convert.ToInt32(d.HookIndex),
            HookId = (string)d.HookId,
            BatchCode = (string)d.BatchCode,
            NartCode = (string)d.NartCode,
            Tester = d.Tester == null ? "" : (string)d.Tester,
            Location = d.Location == null ? "" : (string)d.Location,
            SamplePosition = d.SamplePosition == null ? "" : (string)d.SamplePosition,
            DropTime = Convert.ToUInt32(d.DropTime),
            PlcValue = d.PlcValue == null ? 0 : Convert.ToUInt32(d.PlcValue),
            CompletedAt = DateTime.Parse((string)d.CompletedAt)
        });
    }
    
    public async Task DeleteTestRecordsAsync(IEnumerable<int> ids)
    {
        if (ids == null || !ids.Any()) return;

        using var connection = new SqliteConnection(_connectionString);
        string sql = "DELETE FROM TestRecords WHERE Id IN @Ids";
        await connection.ExecuteAsync(sql, new { Ids = ids });
    }
    
    // ---- Checkpoint Logic ----
    
    public async Task SaveCheckpointAsync(string sessionId, string hookId, string batchCode, string nartCode, uint currentDropTime)
    {
        using var connection = new SqliteConnection(_connectionString);
        string sql = @"
            INSERT INTO Checkpoints (SessionId, HookId, BatchCode, NartCode, CurrentDropTime, LastUpdate)
            VALUES (@SessionId, @HookId, @BatchCode, @NartCode, @CurrentDropTime, @LastUpdate)
            ON CONFLICT(SessionId) DO UPDATE SET 
                CurrentDropTime = excluded.CurrentDropTime,
                LastUpdate = excluded.LastUpdate";
                
        await connection.ExecuteAsync(sql, new
        {
            SessionId = sessionId,
            HookId = hookId,
            BatchCode = batchCode,
            NartCode = nartCode,
            CurrentDropTime = currentDropTime,
            LastUpdate = DateTime.Now.ToString("O")
        });
    }

    public async Task ClearCheckpointAsync(string sessionId)
    {
        using var connection = new SqliteConnection(_connectionString);
        string sql = "DELETE FROM Checkpoints WHERE SessionId = @SessionId";
        await connection.ExecuteAsync(sql, new { SessionId = sessionId });
    }

    public async Task<dynamic?> GetIncompleteCheckpointAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        string sql = "SELECT * FROM Checkpoints LIMIT 1";
        return await connection.QueryFirstOrDefaultAsync(sql);
    }
    
    public async Task ClearAllCheckpointsAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        string sql = "DELETE FROM Checkpoints";
        await connection.ExecuteAsync(sql);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            // Tạm thời không có unmanaged resources để giải phóng, nhưng tuân thủ chuẩn IDisposable.
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
