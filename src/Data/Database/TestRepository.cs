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
            INSERT INTO TestRecords (BatchCode, NartCode, DropTime, CompletedAt) 
            VALUES (@BatchCode, @NartCode, @DropTime, @CompletedAt)";
        
        await connection.ExecuteAsync(sql, new
        {
            record.BatchCode,
            record.NartCode,
            record.DropTime,
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
            BatchCode = (string)d.BatchCode,
            NartCode = (string)d.NartCode,
            DropTime = Convert.ToUInt32(d.DropTime),
            CompletedAt = DateTime.Parse((string)d.CompletedAt)
        });
    }
    
    // ---- Checkpoint Logic ----
    
    public async Task SaveCheckpointAsync(string sessionId, string batchCode, string nartCode, uint currentDropTime)
    {
        using var connection = new SqliteConnection(_connectionString);
        string sql = @"
            INSERT INTO Checkpoints (SessionId, BatchCode, NartCode, CurrentDropTime, LastUpdate)
            VALUES (@SessionId, @BatchCode, @NartCode, @CurrentDropTime, @LastUpdate)
            ON CONFLICT(SessionId) DO UPDATE SET 
                CurrentDropTime = excluded.CurrentDropTime,
                LastUpdate = excluded.LastUpdate";
                
        await connection.ExecuteAsync(sql, new
        {
            SessionId = sessionId,
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
