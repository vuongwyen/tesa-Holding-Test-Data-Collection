using System.Drawing;
using System.Windows.Forms;

namespace TapeAdhesionApp.UI.Views;

partial class ScannerForm
{
    private System.ComponentModel.IContainer components = null;
    private TextBox txtAddresses;
    private Button btnStartScan;
    private ListBox lstLogs;
    private Button btnApply;
    private Label lblStatus;
    private Label lblInstruction;
    private CheckBox chkShowRawData;
    private Button btnSearchValue;

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
        this.txtAddresses = new TextBox();
        this.btnStartScan = new Button();
        this.lstLogs = new ListBox();
        this.btnApply = new Button();
        this.lblStatus = new Label();
        this.lblInstruction = new Label();
        this.chkShowRawData = new CheckBox();
        this.btnSearchValue = new Button();
        this.SuspendLayout();

        // Form Settings
        this.BackColor = Color.White;
        this.ForeColor = Color.Black;
        Font regularFont = new Font("Segoe UI", 10F);
        Font boldFont = new Font("Segoe UI", 10F, FontStyle.Bold);
        
        // lblInstruction
        this.lblInstruction.Text = "Nhập danh sách địa chỉ VD cần dò (cách nhau bởi dấu phẩy hoặc xuống dòng):";
        this.lblInstruction.Location = new Point(10, 10);
        this.lblInstruction.Size = new Size(580, 25);
        this.lblInstruction.Font = regularFont;

        // txtAddresses
        this.txtAddresses.Multiline = true;
        this.txtAddresses.ScrollBars = ScrollBars.Vertical;
        this.txtAddresses.Location = new Point(10, 40);
        this.txtAddresses.Size = new Size(560, 100);
        this.txtAddresses.Font = regularFont;

        // btnStartScan
        this.btnStartScan.Text = "BẮT ĐẦU QUÉT";
        this.btnStartScan.Location = new Point(10, 150);
        this.btnStartScan.Size = new Size(150, 40);
        this.btnStartScan.Font = boldFont;
        this.btnStartScan.BackColor = Color.FromArgb(227, 0, 15); // Tesa Red
        this.btnStartScan.ForeColor = Color.White;
        this.btnStartScan.FlatStyle = FlatStyle.Flat;
        this.btnStartScan.Click += new System.EventHandler(this.BtnStartScan_Click);

        // lblStatus
        this.lblStatus.Text = "Trạng thái: Đang chờ...";
        this.lblStatus.Location = new Point(170, 160);
        this.lblStatus.Size = new Size(400, 25);
        this.lblStatus.Font = boldFont;
        this.lblStatus.ForeColor = Color.Gray;

        // chkShowRawData
        this.chkShowRawData.Text = "Hiển thị Dữ liệu Thô (Chưa qua màng lọc)";
        this.chkShowRawData.Location = new Point(10, 200);
        this.chkShowRawData.Size = new Size(500, 25);
        this.chkShowRawData.Font = regularFont;
        this.chkShowRawData.Checked = false;

        // lstLogs
        this.lstLogs.Location = new Point(10, 230);
        this.lstLogs.Size = new Size(560, 220);
        this.lstLogs.Font = new Font("Consolas", 11F);
        this.lstLogs.IntegralHeight = false;

        // btnSearchValue
        this.btnSearchValue.Text = "TRUY VẾT";
        this.btnSearchValue.Location = new Point(10, 460);
        this.btnSearchValue.Size = new Size(150, 40);
        this.btnSearchValue.Font = boldFont;
        this.btnSearchValue.BackColor = Color.FromArgb(40, 167, 69); // Green
        this.btnSearchValue.ForeColor = Color.White;
        this.btnSearchValue.FlatStyle = FlatStyle.Flat;
        this.btnSearchValue.Click += new System.EventHandler(this.BtnSearchValue_Click);

        // btnApply
        this.btnApply.Text = "COPY ĐỊA CHỈ CUỐI";
        this.btnApply.Location = new Point(420, 460);
        this.btnApply.Size = new Size(150, 40);
        this.btnApply.Font = boldFont;
        this.btnApply.BackColor = Color.FromArgb(0, 165, 217); // Tesa Blue
        this.btnApply.ForeColor = Color.White;
        this.btnApply.FlatStyle = FlatStyle.Flat;
        this.btnApply.Click += new System.EventHandler(this.BtnApply_Click);

        // ScannerForm
        this.ClientSize = new Size(580, 510);
        this.Controls.Add(this.lblInstruction);
        this.Controls.Add(this.txtAddresses);
        this.Controls.Add(this.btnStartScan);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.chkShowRawData);
        this.Controls.Add(this.lstLogs);
        this.Controls.Add(this.btnApply);
        this.Controls.Add(this.btnSearchValue);
        this.Name = "ScannerForm";
        this.Text = "PLC Address Scanner (Dò mìn)";
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.FormClosing += new FormClosingEventHandler(this.ScannerForm_FormClosing);

        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
