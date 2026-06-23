using System.Drawing;
using System.Windows.Forms;

namespace TapeAdhesionApp.UI.Views;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        Label lblBatch = new Label();
        Label lblNart = new Label();

        ((System.ComponentModel.ISupportInitialize)(this.dgvHistory)).BeginInit();
        this.SuspendLayout();

        // Thiết kế Giao diện Công nghiệp (Industrial UI)
        Font largeFont = new Font("Segoe UI", 16F, FontStyle.Bold);
        Font titleFont = new Font("Segoe UI", 24F, FontStyle.Bold);
        
        this.BackColor = Color.FromArgb(45, 45, 48); // Màu tối công nghiệp
        this.ForeColor = Color.White;
        
        // lblBatch
        lblBatch.Text = "BATCH CODE:";
        lblBatch.Location = new Point(20, 30);
        lblBatch.AutoSize = true;
        lblBatch.Font = largeFont;

        // txtBatch
        this.txtBatch.Location = new Point(200, 25);
        this.txtBatch.Size = new Size(300, 40);
        this.txtBatch.Font = largeFont;
        this.txtBatch.TextChanged += new System.EventHandler(this.TxtInput_TextChanged);
        this.txtBatch.Enter += new System.EventHandler(this.TxtInput_Enter);

        // lblNart
        lblNart.Text = "NART CODE:";
        lblNart.Location = new Point(20, 80);
        lblNart.AutoSize = true;
        lblNart.Font = largeFont;

        // txtNart
        this.txtNart.Location = new Point(200, 75);
        this.txtNart.Size = new Size(300, 40);
        this.txtNart.Font = largeFont;
        this.txtNart.TextChanged += new System.EventHandler(this.TxtInput_TextChanged);
        this.txtNart.Enter += new System.EventHandler(this.TxtInput_Enter);

        // btnStart
        this.btnStart.Text = "START / CONNECT";
        this.btnStart.Location = new Point(520, 25);
        this.btnStart.Size = new Size(250, 90);
        this.btnStart.Font = largeFont;
        this.btnStart.BackColor = Color.SeaGreen;
        this.btnStart.FlatStyle = FlatStyle.Flat;
        this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);

        // lblConnectionStatus
        this.lblConnectionStatus.Text = "PLC: OFFLINE";
        this.lblConnectionStatus.Location = new Point(20, 150);
        this.lblConnectionStatus.AutoSize = true;
        this.lblConnectionStatus.Font = titleFont;
        this.lblConnectionStatus.ForeColor = Color.Red;

        // lblMachineState
        this.lblMachineState.Text = "STATE: IDLE";
        this.lblMachineState.Location = new Point(350, 150);
        this.lblMachineState.AutoSize = true;
        this.lblMachineState.Font = titleFont;
        this.lblMachineState.ForeColor = Color.Gold;

        // lblRunningTime
        this.lblRunningTime.Text = "TIME: 0 ms";
        this.lblRunningTime.Location = new Point(20, 220);
        this.lblRunningTime.AutoSize = true;
        this.lblRunningTime.Font = titleFont;
        this.lblRunningTime.ForeColor = Color.DeepSkyBlue;

        // lblPosition
        this.lblPosition.Text = "VD110: 0 | VD368: 0";
        this.lblPosition.Location = new Point(350, 220);
        this.lblPosition.AutoSize = true;
        this.lblPosition.Font = titleFont;
        this.lblPosition.ForeColor = Color.LightGray;

        // btnStop
        this.btnStop.Text = "STOP TEST";
        this.btnStop.Location = new Point(20, 300);
        this.btnStop.Size = new Size(200, 80);
        this.btnStop.Font = largeFont;
        this.btnStop.BackColor = Color.Crimson;
        this.btnStop.FlatStyle = FlatStyle.Flat;
        this.btnStop.Click += new System.EventHandler(this.BtnStop_Click);

        // btnReset
        this.btnReset.Text = "RESET STATUS";
        this.btnReset.Location = new Point(240, 300);
        this.btnReset.Size = new Size(240, 80);
        this.btnReset.Font = largeFont;
        this.btnReset.BackColor = Color.DimGray;
        this.btnReset.FlatStyle = FlatStyle.Flat;
        this.btnReset.Click += new System.EventHandler(this.BtnReset_Click);

        // btnSettings
        this.btnSettings.Text = "⚙ CÀI ĐẶT";
        this.btnSettings.Location = new Point(500, 300);
        this.btnSettings.Size = new Size(240, 80);
        this.btnSettings.Font = largeFont;
        this.btnSettings.BackColor = Color.Teal;
        this.btnSettings.FlatStyle = FlatStyle.Flat;
        this.btnSettings.Click += new System.EventHandler(this.BtnSettings_Click);

        // dgvHistory
        this.dgvHistory.Location = new Point(20, 400);
        this.dgvHistory.Size = new Size(760, 300);
        this.dgvHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvHistory.AllowUserToAddRows = false;
        this.dgvHistory.ReadOnly = true;
        this.dgvHistory.RowTemplate.Height = 40;
        this.dgvHistory.DefaultCellStyle.Font = new Font("Segoe UI", 12F);
        this.dgvHistory.DefaultCellStyle.ForeColor = Color.Black;
        this.dgvHistory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);

        // MainForm
        this.ClientSize = new Size(800, 720);
        this.Controls.Add(lblBatch);
        this.Controls.Add(this.txtBatch);
        this.Controls.Add(lblNart);
        this.Controls.Add(this.txtNart);
        this.Controls.Add(this.btnStart);
        this.Controls.Add(this.lblConnectionStatus);
        this.Controls.Add(this.lblMachineState);
        this.Controls.Add(this.lblRunningTime);
        this.Controls.Add(this.lblPosition);
        this.Controls.Add(this.btnStop);
        this.Controls.Add(this.btnReset);
        this.Controls.Add(this.btnSettings);
        this.Controls.Add(this.dgvHistory);
        this.Name = "MainForm";
        this.Text = "Tape Adhesion Testing Machine";
        this.StartPosition = FormStartPosition.CenterScreen;

        ((System.ComponentModel.ISupportInitialize)(this.dgvHistory)).EndInit();
        this.ResumeLayout(false);
    }
}
