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
        ApplicationConfiguration.Initialize();

        // Khởi tạo SQLite Database
        TapeAdhesionApp.Data.Database.DatabaseInitializer.Initialize();

        // Dependencies
        var plcService = new PlcCommunicationService();
        var stateMachine = new StateMachine();
        var mainForm = new MainForm();
        
        var testRepo = new TapeAdhesionApp.Data.Database.TestRepository();
        var excelService = new TapeAdhesionApp.Data.Export.ExcelReportService();
        
        // Presenter
        var presenter = new MainPresenter(mainForm, plcService, stateMachine, testRepo, excelService);

        // Run application
        Application.Run(mainForm);
        
        // Cleanup
        presenter.Dispose();
        plcService.Dispose();
    }
}