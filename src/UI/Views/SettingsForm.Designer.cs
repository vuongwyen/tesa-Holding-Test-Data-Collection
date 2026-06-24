using System.Drawing;
using System.Windows.Forms;

namespace TapeAdhesionApp.UI.Views;

partial class SettingsForm
{
    private System.ComponentModel.IContainer components = null;
    private DataGridView dgvAddresses;
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
        this.dgvAddresses = new DataGridView();
        this.btnSave = new Button();
        this.btnCancel = new Button();
        ((System.ComponentModel.ISupportInitialize)(this.dgvAddresses)).BeginInit();
        this.SuspendLayout();

        // Form Settings
        this.BackColor = Color.White;
        this.ForeColor = Color.Black;
        Font largeFont = new Font("Segoe UI", 12F, FontStyle.Bold);

        // dgvAddresses
        this.dgvAddresses.Dock = DockStyle.Top;
        this.dgvAddresses.Height = 480;
        this.dgvAddresses.AllowUserToAddRows = false;
        this.dgvAddresses.AllowUserToDeleteRows = false;
        this.dgvAddresses.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvAddresses.RowHeadersVisible = false;
        this.dgvAddresses.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        this.dgvAddresses.EnableHeadersVisualStyles = false;
        this.dgvAddresses.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        this.dgvAddresses.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 165, 217); // Tesa Blue
        this.dgvAddresses.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        this.dgvAddresses.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        this.dgvAddresses.ColumnHeadersHeight = 35;
        this.dgvAddresses.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        this.dgvAddresses.GridColor = Color.FromArgb(230, 230, 230);
        this.dgvAddresses.BackgroundColor = Color.White;
        this.dgvAddresses.BorderStyle = BorderStyle.None;
        this.dgvAddresses.Font = new Font("Segoe UI", 10F);

        // btnSave
        this.btnSave.Text = "LƯU";
        this.btnSave.Location = new Point(140, 500);
        this.btnSave.Size = new Size(120, 40);
        this.btnSave.Font = largeFont;
        this.btnSave.BackColor = Color.FromArgb(0, 165, 217); // Tesa Blue
        this.btnSave.ForeColor = Color.White;
        this.btnSave.FlatStyle = FlatStyle.Flat;
        this.btnSave.FlatAppearance.BorderSize = 0;
        this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);

        // btnCancel
        this.btnCancel.Text = "HỦY";
        this.btnCancel.Location = new Point(280, 500);
        this.btnCancel.Size = new Size(120, 40);
        this.btnCancel.Font = largeFont;
        this.btnCancel.BackColor = Color.White;
        this.btnCancel.ForeColor = Color.FromArgb(227, 0, 15); // Tesa Red
        this.btnCancel.FlatStyle = FlatStyle.Flat;
        this.btnCancel.FlatAppearance.BorderColor = Color.FromArgb(227, 0, 15);
        this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);

        // SettingsForm
        this.ClientSize = new Size(500, 560);
        this.Controls.Add(this.dgvAddresses);
        this.Controls.Add(this.btnSave);
        this.Controls.Add(this.btnCancel);
        this.Name = "SettingsForm";
        this.Text = "Cấu hình địa chỉ PLC (VD Address)";
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Load += new System.EventHandler(this.SettingsForm_Load);
        
        ((System.ComponentModel.ISupportInitialize)(this.dgvAddresses)).EndInit();
        this.ResumeLayout(false);
    }
}
