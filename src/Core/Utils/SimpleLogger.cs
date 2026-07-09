using System;
using System.IO;

namespace TapeAdhesionApp.Core.Utils;

public static class SimpleLogger
{
    private static readonly string LogFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "TesaTapeAdhesionApp", "app_error.log");
    
    public static void LogError(string msg)
    {
        try
        {
            File.AppendAllText(LogFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}{Environment.NewLine}");
        }
        catch { }
    }
}
