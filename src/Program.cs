using System;
using System.Windows.Forms;
using TapeAdhesionApp.Core.PLC;
using TapeAdhesionApp.Core.State;
using TapeAdhesionApp.UI.Presenters;
using TapeAdhesionApp.UI.Views;

namespace TapeAdhesionApp;

static class Program
{
    [STAThread]
    static void Main()
    {
        // Global Exception Handling cho WinForms và Background Threads
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

        ApplicationConfiguration.Initialize();

        // Khởi tạo SQLite Database
        TapeAdhesionApp.Data.Database.DatabaseInitializer.Initialize();

        // Dependencies
        var plcManager = new PlcManager();
        var mainForm = new MainForm();
        
        var testRepo = new TapeAdhesionApp.Data.Database.TestRepository();
        var excelService = new TapeAdhesionApp.Data.Export.ExcelReportService();
        var settingsRepo = new TapeAdhesionApp.Data.Database.SettingsRepository();
        
        // Presenter
        var presenter = new MainPresenter(mainForm, plcManager, testRepo, excelService, settingsRepo);

        // Run application
        Application.Run(mainForm);
        
        // Cleanup
        presenter.Dispose();
        plcManager.Dispose();
        testRepo.Dispose();
        settingsRepo.Dispose();
    }

    static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    {
        LogException(e.Exception);
    }

    static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            LogException(ex);
        }
    }

    static void LogException(Exception ex)
    {
        try
        {
            string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_log.txt");
            string message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] FATAL ERROR: {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}{new string('-', 50)}{Environment.NewLine}";
            System.IO.File.AppendAllText(logPath, message);
        }
        catch 
        { 
            // Fallback nếu không ghi được file
        }
        
        MessageBox.Show($"Đã xảy ra lỗi hệ thống: {ex.Message}\n{ex.StackTrace}", "Lỗi Nghiêm Trọng", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}