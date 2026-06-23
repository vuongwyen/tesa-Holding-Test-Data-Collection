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

        // Dependencies
        var plcService = new PlcCommunicationService();
        var stateMachine = new StateMachine();
        var mainForm = new MainForm();
        
        // Presenter
        var presenter = new MainPresenter(mainForm, plcService, stateMachine);

        // Run application
        Application.Run(mainForm);
        
        // Cleanup
        presenter.Dispose();
        plcService.Dispose();
    }
}