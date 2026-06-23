using System.Drawing;
using System.Windows.Forms;

namespace TapeAdhesionApp.UI.Views;

partial class SettingsForm
{
    private System.ComponentModel.IContainer components = null;
    private Label lblIpAddress;
    private TextBox txtIpAddress;
    private Button btnSave;
    private Button btnCancel;

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
        this.lblIpAddress = new Label();
        this.txtIpAddress = new TextBox();
        this.btnSave = new Button();
        this.btnCancel = new Button();
        this.SuspendLayout();

        // Form Settings
        this.BackColor = Color.FromArgb(45, 45, 48);
        this.ForeColor = Color.White;
        Font largeFont = new Font("Segoe UI", 14F, FontStyle.Bold);

        // lblIpAddress
        this.lblIpAddress.Text = "PLC IP ADDRESS:";
        this.lblIpAddress.Location = new Point(20, 30);
        this.lblIpAddress.AutoSize = true;
        this.lblIpAddress.Font = largeFont;

        // txtIpAddress
        this.txtIpAddress.Location = new Point(200, 25);
        this.txtIpAddress.Size = new Size(200, 35);
        this.txtIpAddress.Font = largeFont;

        // btnSave
        this.btnSave.Text = "SAVE";
        this.btnSave.Location = new Point(140, 90);
        this.btnSave.Size = new Size(120, 50);
        this.btnSave.Font = largeFont;
        this.btnSave.BackColor = Color.SeaGreen;
        this.btnSave.FlatStyle = FlatStyle.Flat;
        this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);

        // btnCancel
        this.btnCancel.Text = "CANCEL";
        this.btnCancel.Location = new Point(280, 90);
        this.btnCancel.Size = new Size(120, 50);
        this.btnCancel.Font = largeFont;
        this.btnCancel.BackColor = Color.DimGray;
        this.btnCancel.FlatStyle = FlatStyle.Flat;
        this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);

        // SettingsForm
        this.ClientSize = new Size(430, 170);
        this.Controls.Add(this.lblIpAddress);
        this.Controls.Add(this.txtIpAddress);
        this.Controls.Add(this.btnSave);
        this.Controls.Add(this.btnCancel);
        this.Name = "SettingsForm";
        this.Text = "Cài đặt hệ thống";
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Load += new System.EventHandler(this.SettingsForm_Load);
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
