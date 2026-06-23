using Dapper;
using Microsoft.Data.Sqlite;
using System.IO;

namespace TapeAdhesionApp.Data.Database;

public static class DatabaseInitializer
{
    private const string DbFile = "app.db";
    public static string ConnectionString => $"Data Source={DbFile}";

    public static void Initialize()
    {
        // Microsoft.Data.Sqlite creates the file automatically on first open if Mode is not set to ReadOnly
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        string createTestRecordsTable = @"
            CREATE TABLE IF NOT EXISTS TestRecords (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                BatchCode TEXT NOT NULL,
                NartCode TEXT NOT NULL,
                DropTime INTEGER NOT NULL,
                CompletedAt TEXT NOT NULL
            );";

        string createCheckpointsTable = @"
            CREATE TABLE IF NOT EXISTS Checkpoints (
                SessionId TEXT PRIMARY KEY,
                BatchCode TEXT NOT NULL,
                NartCode TEXT NOT NULL,
                CurrentDropTime INTEGER NOT NULL,
                LastUpdate TEXT NOT NULL
            );";

        string createSettingsTable = @"
            CREATE TABLE IF NOT EXISTS Settings (
                Key TEXT PRIMARY KEY,
                Value TEXT NOT NULL
            );";

        connection.Execute(createTestRecordsTable);
        connection.Execute(createCheckpointsTable);
        connection.Execute(createSettingsTable);

        // Seed default settings
        connection.Execute("INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('PlcIpAddress', '192.168.0.1')");
    }
}
