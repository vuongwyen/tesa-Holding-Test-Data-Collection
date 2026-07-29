using Dapper;
using Microsoft.Data.Sqlite;
using System.IO;

namespace TapeAdhesionApp.Data.Database;

public static class DatabaseInitializer
{
    // Pinned to EXE directory so app.db is always the same file,
    // regardless of which working directory the process is launched from.
    private static readonly string AppDataFolder = 
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "TesaTapeAdhesionApp");
        
    private static readonly string DbFile = Path.Combine(AppDataFolder, "app.db");

    public static string ConnectionString => $"Data Source={DbFile}";

    public static void Initialize()
    {
        // Đảm bảo thư mục tồn tại trước khi tạo file db
        if (!Directory.Exists(AppDataFolder))
        {
            Directory.CreateDirectory(AppDataFolder);
        }

        // Microsoft.Data.Sqlite creates the file automatically on first open if Mode is not set to ReadOnly
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        
        using var pragmaCmd = connection.CreateCommand();
        pragmaCmd.CommandText = "PRAGMA journal_mode=WAL;";
        pragmaCmd.ExecuteNonQuery();

        using var command = connection.CreateCommand();

        string createTestRecordsTable = @"
            CREATE TABLE IF NOT EXISTS TestRecords (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                HookId TEXT NOT NULL DEFAULT '',
                BatchCode TEXT NOT NULL,
                NartCode TEXT NOT NULL,
                DropTime INTEGER NOT NULL,
                CompletedAt TEXT NOT NULL
            );";

        string createCheckpointsTable = @"
            CREATE TABLE IF NOT EXISTS Checkpoints (
                SessionId TEXT PRIMARY KEY,
                HookId TEXT NOT NULL DEFAULT '',
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
        
        // Simple schema migration for existing databases
        try { connection.Execute("ALTER TABLE TestRecords ADD COLUMN HookId TEXT NOT NULL DEFAULT ''"); } catch { }
        try { connection.Execute("ALTER TABLE Checkpoints ADD COLUMN HookId TEXT NOT NULL DEFAULT ''"); } catch { }
        
        // Multi-rack migrations
        string[] newColumns = new[]
        {
            "ALTER TABLE TestRecords ADD COLUMN RackId TEXT DEFAULT ''",
            "ALTER TABLE TestRecords ADD COLUMN Floor INTEGER DEFAULT 0",
            "ALTER TABLE TestRecords ADD COLUMN HookIndex INTEGER DEFAULT 0",
            "ALTER TABLE TestRecords ADD COLUMN Tester TEXT DEFAULT ''",
            "ALTER TABLE TestRecords ADD COLUMN Location TEXT DEFAULT ''",
            "ALTER TABLE TestRecords ADD COLUMN SamplePosition TEXT DEFAULT ''",
            "ALTER TABLE TestRecords ADD COLUMN PlcValue INTEGER DEFAULT 0",
            "ALTER TABLE TestRecords ADD COLUMN TestCondition TEXT DEFAULT ''",
            "ALTER TABLE TestRecords ADD COLUMN SampleNote TEXT DEFAULT ''",
            "ALTER TABLE TestRecords ADD COLUMN StartedAt TEXT DEFAULT NULL"
        };
        foreach (var cmd in newColumns)
        {
            try { connection.Execute(cmd); } catch { }
        }

        // Seed default IP - update to actual PLC IP if needed via Settings form
        connection.Execute("INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('PlcIpAddress', '192.168.2.1')");
    }
    
    public static void BackupDatabase()
    {
        try
        {
            if (File.Exists(DbFile))
            {
                string backupFolder = Path.Combine(AppDataFolder, "Backups");
                if (!Directory.Exists(backupFolder)) Directory.CreateDirectory(backupFolder);
                string backupFile = Path.Combine(backupFolder, $"app_backup_{DateTime.Now:yyyyMMdd}.db");
                if (!File.Exists(backupFile)) // Only backup once a day
                {
                    File.Copy(DbFile, backupFile, true);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DB Backup Error] {ex.Message}");
        }
    }
}
