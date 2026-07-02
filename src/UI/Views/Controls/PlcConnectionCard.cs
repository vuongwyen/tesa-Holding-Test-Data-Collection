using System;
using System.Drawing;
using System.Windows.Forms;

namespace TapeAdhesionApp.UI.Views.Controls;

public class PlcConnectionCard : UserControl
{
    private Label lblRackName;
    private TextBox txtIpAddress;
    private Label lblStatus;
    private Button btnConnect;
    private Button btnSettings;
    private Button btnScanner;

    public string RackId { get; }

    public event Action<string, string>? ConnectRequested;
    public event Action<string>? DisconnectRequested;
    public event Action? SettingsRequested;
    public event Action? ScannerRequested;

    private bool _isConnected;

    public PlcConnectionCard(string rackId, string defaultIp)
    {
        RackId = rackId;
        InitializeComponent();
        lblRackName.Text = rackId;
        txtIpAddress.Text = defaultIp;
    }

    private void InitializeComponent()
    {
        this.lblRackName = new Label();
        this.txtIpAddress = new TextBox();
        this.lblStatus = new Label();
        this.btnConnect = new Button();
        this.btnSettings = new Button();
        this.SuspendLayout();
        
        // lblRackName
        this.lblRackName.AutoSize = true;
        this.lblRackName.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
        this.lblRackName.Location = new Point(10, 10);
        this.lblRackName.Name = "lblRackName";
        this.lblRackName.Size = new Size(100, 21);
        
        // btnSettings
        this.btnSettings.Location = new Point(190, 6);
        this.btnSettings.Name = "btnSettings";
        this.btnSettings.Size = new Size(30, 30);
        this.btnSettings.Text = "⚙";
        this.btnSettings.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point);
        this.btnSettings.FlatStyle = FlatStyle.Flat;
        this.btnSettings.FlatAppearance.BorderSize = 0;
        this.btnSettings.BackColor = Color.White;
        this.btnSettings.ForeColor = Color.FromArgb(100, 100, 100);
        this.btnSettings.UseVisualStyleBackColor = false;
        this.btnSettings.Click += (s, e) => SettingsRequested?.Invoke();
        this.btnSettings.Cursor = Cursors.Hand;
        
        // btnScanner
        this.btnScanner = new Button();
        this.btnScanner.Location = new Point(155, 6);
        this.btnScanner.Name = "btnScanner";
        this.btnScanner.Size = new Size(30, 30);
        this.btnScanner.Text = "🔍";
        this.btnScanner.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
        this.btnScanner.FlatStyle = FlatStyle.Flat;
        this.btnScanner.FlatAppearance.BorderSize = 0;
        this.btnScanner.BackColor = Color.White;
        this.btnScanner.ForeColor = Color.FromArgb(0, 165, 217); // Tesa Blue
        this.btnScanner.UseVisualStyleBackColor = false;
        this.btnScanner.Click += (s, e) => ScannerRequested?.Invoke();
        this.btnScanner.Cursor = Cursors.Hand;
        
        // txtIpAddress
        this.txtIpAddress.Location = new Point(10, 40);
        this.txtIpAddress.Name = "txtIpAddress";
        this.txtIpAddress.Size = new Size(120, 23);
        this.txtIpAddress.BorderStyle = BorderStyle.FixedSingle;
        
        // btnConnect
        this.btnConnect.Location = new Point(140, 39);
        this.btnConnect.Name = "btnConnect";
        this.btnConnect.Size = new Size(80, 25);
        this.btnConnect.Text = "Connect";
        this.btnConnect.FlatStyle = FlatStyle.Flat;
        this.btnConnect.BackColor = Color.FromArgb(0, 165, 217); // Tesa Blue
        this.btnConnect.ForeColor = Color.White;
        this.btnConnect.FlatAppearance.BorderSize = 0;
        this.btnConnect.UseVisualStyleBackColor = false;
        this.btnConnect.Click += new EventHandler(this.BtnConnect_Click);
        
        // lblStatus
        this.lblStatus.AutoSize = true;
        this.lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        this.lblStatus.Location = new Point(10, 70);
        this.lblStatus.Name = "lblStatus";
        this.lblStatus.Size = new Size(86, 15);
        this.lblStatus.Text = "Disconnected";
        this.lblStatus.ForeColor = Color.Red;

        this.Controls.Add(this.btnScanner);
        this.Controls.Add(this.btnSettings);
        this.Controls.Add(this.btnConnect);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.txtIpAddress);
        this.Controls.Add(this.lblRackName);
        
        this.Name = "PlcConnectionCard";
        this.Size = new Size(230, 100);
        this.BorderStyle = BorderStyle.FixedSingle;
        this.BackColor = Color.White;
        
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private void BtnConnect_Click(object? sender, EventArgs e)
    {
        if (_isConnected)
        {
            DisconnectRequested?.Invoke(RackId);
        }
        else
        {
            ConnectRequested?.Invoke(RackId, txtIpAddress.Text.Trim());
        }
    }

    public void UpdateStatus(bool isConnected)
    {
        _isConnected = isConnected;
        if (isConnected)
        {
            lblStatus.Text = "Connected";
            lblStatus.ForeColor = Color.Green;
            btnConnect.Text = "Disconnect";
            txtIpAddress.Enabled = false;
        }
        else
        {
            lblStatus.Text = "Disconnected";
            lblStatus.ForeColor = Color.Red;
            btnConnect.Text = "Connect";
            txtIpAddress.Enabled = true;
        }
    }
}
