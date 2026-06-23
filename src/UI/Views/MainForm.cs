using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TapeAdhesionApp.Core.Models;

namespace TapeAdhesionApp.UI.Views;

public partial class MainForm : Form, IMainView
{
    private BindingList<TestRecord> _testHistory;
    
    // Các Control trên UI
    private TextBox txtBatch = new();
    private TextBox txtNart = new();
    private Button btnStart = new();
    private Button btnStop = new();
    private Button btnReset = new();
    private Label lblConnectionStatus = new();
    private Label lblMachineState = new();
    private Label lblRunningTime = new();
    private Label lblPosition = new();
    private DataGridView dgvHistory = new();

    public MainForm()
    {
        InitializeComponent();
        
        // Khởi tạo danh sách test history (Thread-Safe DataSource)
        _testHistory = new BindingList<TestRecord>();
        dgvHistory.DataSource = _testHistory;
    }

    // Properties cung cấp cho Presenter
    public string BatchCode => txtBatch.Text;
    public string NartCode => txtNart.Text;

    public void SetInputs(string batchCode, string nartCode)
    {
        InvokeOnUI(() => 
        {
            txtBatch.Text = batchCode;
            txtNart.Text = nartCode;
        });
    }

    // Các Event 
    public event EventHandler? StartTestClicked;
    public event EventHandler? StopTestClicked;
    public event EventHandler? ResetClicked;
    public event EventHandler? InputsChanged;

    // Đảm bảo cập nhật giao diện (Thread-Safety) khi add record từ background thread
    public void AddTestRecord(TestRecord record)
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action(() => _testHistory.Add(record)));
        }
        else
        {
            _testHistory.Add(record);
        }
    }

    // Hàm tiện ích để Presenter ép mọi tác vụ cập nhật UI về luồng chính
    public void InvokeOnUI(Action action)
    {
        if (this.IsDisposed || !this.IsHandleCreated) return;
        
        if (this.InvokeRequired)
        {
            this.BeginInvoke(action); // Bất đồng bộ để không chặn Background Thread
        }
        else
        {
            action();
        }
    }

    public void EnableStartButton(bool enable)
    {
        InvokeOnUI(() => btnStart.Enabled = enable);
    }

    public void UpdateMachineState(string stateName)
    {
        InvokeOnUI(() => lblMachineState.Text = $"STATE: {stateName}");
    }

    public void UpdatePlcConnectionStatus(bool isConnected)
    {
        InvokeOnUI(() => 
        {
            lblConnectionStatus.Text = isConnected ? "PLC: CONNECTED" : "PLC: OFFLINE";
            lblConnectionStatus.ForeColor = isConnected ? Color.LimeGreen : Color.Red;
        });
    }

    public void UpdateRunningTime(string formattedTime)
    {
        InvokeOnUI(() => lblRunningTime.Text = $"TIME: {formattedTime}");
    }

    public void UpdateSamplePosition(string position)
    {
        InvokeOnUI(() => lblPosition.Text = position);
    }

    private void TxtInput_TextChanged(object? sender, EventArgs e)
    {
        InputsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void TxtInput_Enter(object? sender, EventArgs e)
    {
        // Tự động bôi đen toàn bộ text khi Operator click chuột vào để tiện dùng máy quét mã vạch
        if (sender is TextBox txt)
        {
            txt.SelectAll();
        }
    }

    private void BtnStart_Click(object? sender, EventArgs e)
    {
        StartTestClicked?.Invoke(this, EventArgs.Empty);
    }

    private void BtnStop_Click(object? sender, EventArgs e)
    {
        StopTestClicked?.Invoke(this, EventArgs.Empty);
    }

    private void BtnReset_Click(object? sender, EventArgs e)
    {
        ResetClicked?.Invoke(this, EventArgs.Empty);
    }
}
