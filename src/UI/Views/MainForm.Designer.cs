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
        this.tabControlMain = new TabControl();
        this.tabDashboard = new TabPage();
        this.tabControlRacks = new TabControl();
        this.tabHistory = new TabPage();
        this.pnlHistoryToolbar = new Panel();
        this.lblSearch = new Label();
        this.txtSearchHistory = new TextBox();
        this.lblFilter = new Label();
        this.cmbFilterRackId = new ComboBox();
        this.btnDeleteSelected = new Button();
        this.btnExportHistory = new Button();
        this.btnBackupDb = new Button();
        this.btnRestoreDb = new Button();
        this.dgvHistory = new DataGridView();
        this.tabPlcConnections = new TabPage();
        this.pnlPlcConnections = new FlowLayoutPanel();
        this.btnSettings = new Button();

        this.tabControlMain.SuspendLayout();
        this.tabDashboard.SuspendLayout();
        this.tabHistory.SuspendLayout();
        this.pnlHistoryToolbar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvHistory)).BeginInit();
        this.tabPlcConnections.SuspendLayout();
        this.SuspendLayout();

        // tabControlMain
        this.tabControlMain.Controls.Add(this.tabDashboard);
        this.tabControlMain.Controls.Add(this.tabHistory);
        this.tabControlMain.Controls.Add(this.tabPlcConnections);
        this.tabControlMain.Dock = DockStyle.Fill;
        this.tabControlMain.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        this.tabControlMain.Location = new Point(0, 0);
        this.tabControlMain.Name = "tabControlMain";
        this.tabControlMain.SelectedIndex = 0;
        this.tabControlMain.Size = new Size(1200, 800);
        this.tabControlMain.TabIndex = 0;

        // tabDashboard
        this.tabDashboard.Controls.Add(this.tabControlRacks);
        this.tabDashboard.Location = new Point(4, 26);
        this.tabDashboard.Name = "tabDashboard";
        this.tabDashboard.Padding = new Padding(3);
        this.tabDashboard.Size = new Size(1192, 770);
        this.tabDashboard.TabIndex = 0;
        this.tabDashboard.Text = "Dashboard";
        this.tabDashboard.UseVisualStyleBackColor = true;

        // tabControlRacks
        this.tabControlRacks.Dock = DockStyle.Fill;
        this.tabControlRacks.Location = new Point(3, 3);
        this.tabControlRacks.Name = "tabControlRacks";
        this.tabControlRacks.SelectedIndex = 0;
        this.tabControlRacks.Size = new Size(1186, 764);
        this.tabControlRacks.TabIndex = 0;
        this.tabControlRacks.DrawMode = TabDrawMode.OwnerDrawFixed;
        this.tabControlRacks.DrawItem += TabControlRacks_DrawItem;

        // tabHistory
        this.tabHistory.Controls.Add(this.dgvHistory);
        this.tabHistory.Controls.Add(this.pnlHistoryToolbar);
        this.tabHistory.Location = new Point(4, 26);
        this.tabHistory.Name = "tabHistory";
        this.tabHistory.Padding = new Padding(3);
        this.tabHistory.Size = new Size(1192, 770);
        this.tabHistory.TabIndex = 1;
        this.tabHistory.Text = "Lịch sử Test / Xuất Báo Cáo";
        this.tabHistory.UseVisualStyleBackColor = true;

        // pnlHistoryToolbar
        this.pnlHistoryToolbar.Controls.Add(this.lblSearch);
        this.pnlHistoryToolbar.Controls.Add(this.txtSearchHistory);
        this.pnlHistoryToolbar.Controls.Add(this.lblFilter);
        this.pnlHistoryToolbar.Controls.Add(this.cmbFilterRackId);
        this.pnlHistoryToolbar.Controls.Add(this.btnDeleteSelected);
        this.pnlHistoryToolbar.Controls.Add(this.btnExportHistory);
        this.pnlHistoryToolbar.Controls.Add(this.btnBackupDb);
        this.pnlHistoryToolbar.Controls.Add(this.btnRestoreDb);
        this.pnlHistoryToolbar.Dock = DockStyle.Top;
        this.pnlHistoryToolbar.Location = new Point(3, 3);
        this.pnlHistoryToolbar.Name = "pnlHistoryToolbar";
        this.pnlHistoryToolbar.Size = new Size(1186, 50);
        this.pnlHistoryToolbar.TabIndex = 0;

        // lblSearch
        this.lblSearch.AutoSize = true;
        this.lblSearch.Location = new Point(10, 15);
        this.lblSearch.Name = "lblSearch";
        this.lblSearch.Size = new Size(67, 19);
        this.lblSearch.Text = "Tìm kiếm:";

        // txtSearchHistory
        this.txtSearchHistory.Location = new Point(80, 12);
        this.txtSearchHistory.Name = "txtSearchHistory";
        this.txtSearchHistory.Size = new Size(200, 25);
        
        // lblFilter
        this.lblFilter.AutoSize = true;
        this.lblFilter.Location = new Point(300, 15);
        this.lblFilter.Name = "lblFilter";
        this.lblFilter.Size = new Size(41, 19);
        this.lblFilter.Text = "Rack:";
        
        // cmbFilterRackId
        this.cmbFilterRackId.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cmbFilterRackId.Location = new Point(350, 12);
        this.cmbFilterRackId.Name = "cmbFilterRackId";
        this.cmbFilterRackId.Size = new Size(120, 25);

        // btnExportHistory
        this.btnExportHistory.BackColor = Color.FromArgb(0, 165, 217); // Tesa Blue
        this.btnExportHistory.ForeColor = Color.White;
        this.btnExportHistory.FlatStyle = FlatStyle.Flat;
        this.btnExportHistory.FlatAppearance.BorderSize = 0;
        this.btnExportHistory.Location = new Point(490, 10);
        this.btnExportHistory.Name = "btnExportHistory";
        this.btnExportHistory.Size = new Size(160, 30);
        this.btnExportHistory.Text = "Xuất báo cáo (.xlsx)";
        this.btnExportHistory.UseVisualStyleBackColor = false;

        // btnBackupDb
        this.btnBackupDb.BackColor = Color.FromArgb(40, 167, 69);
        this.btnBackupDb.ForeColor = Color.White;
        this.btnBackupDb.FlatStyle = FlatStyle.Flat;
        this.btnBackupDb.FlatAppearance.BorderSize = 0;
        this.btnBackupDb.Location = new Point(810, 10);
        this.btnBackupDb.Name = "btnBackupDb";
        this.btnBackupDb.Size = new Size(100, 30);
        this.btnBackupDb.Text = "Sao lưu DB";
        this.btnBackupDb.UseVisualStyleBackColor = false;

        // btnRestoreDb
        this.btnRestoreDb.BackColor = Color.FromArgb(255, 193, 7);
        this.btnRestoreDb.ForeColor = Color.Black;
        this.btnRestoreDb.FlatStyle = FlatStyle.Flat;
        this.btnRestoreDb.FlatAppearance.BorderSize = 0;
        this.btnRestoreDb.Location = new Point(920, 10);
        this.btnRestoreDb.Name = "btnRestoreDb";
        this.btnRestoreDb.Size = new Size(120, 30);
        this.btnRestoreDb.Text = "Khôi phục DB";
        this.btnRestoreDb.UseVisualStyleBackColor = false;

        // btnDeleteSelected
        this.btnDeleteSelected.BackColor = Color.White;
        this.btnDeleteSelected.ForeColor = Color.FromArgb(227, 0, 15); // Tesa Red
        this.btnDeleteSelected.FlatStyle = FlatStyle.Flat;
        this.btnDeleteSelected.FlatAppearance.BorderColor = Color.FromArgb(227, 0, 15);
        this.btnDeleteSelected.Location = new Point(660, 10);
        this.btnDeleteSelected.Name = "btnDeleteSelected";
        this.btnDeleteSelected.Size = new Size(120, 30);
        this.btnDeleteSelected.Text = "Xóa mục đã chọn";
        this.btnDeleteSelected.UseVisualStyleBackColor = false;

        // dgvHistory
        this.dgvHistory.AllowUserToAddRows = false;
        this.dgvHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvHistory.EnableHeadersVisualStyles = false;
        this.dgvHistory.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        this.dgvHistory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 165, 217); // Tesa Blue
        this.dgvHistory.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        this.dgvHistory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        this.dgvHistory.ColumnHeadersHeight = 40;
        this.dgvHistory.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        this.dgvHistory.GridColor = Color.FromArgb(230, 230, 230);
        this.dgvHistory.BackgroundColor = Color.White;
        this.dgvHistory.BorderStyle = BorderStyle.None;
        this.dgvHistory.Dock = DockStyle.Fill;
        this.dgvHistory.Location = new Point(3, 53);
        this.dgvHistory.Name = "dgvHistory";
        this.dgvHistory.ReadOnly = true;
        this.dgvHistory.RowTemplate.Height = 30;
        this.dgvHistory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        this.dgvHistory.Size = new Size(1186, 714);
        this.dgvHistory.TabIndex = 1;

        // tabPlcConnections
        this.tabPlcConnections.Controls.Add(this.pnlPlcConnections);
        this.tabPlcConnections.Location = new Point(4, 26);
        this.tabPlcConnections.Name = "tabPlcConnections";
        this.tabPlcConnections.Padding = new Padding(3);
        this.tabPlcConnections.Size = new Size(1192, 770);
        this.tabPlcConnections.TabIndex = 2;
        this.tabPlcConnections.Text = "Quản lý PLC";
        this.tabPlcConnections.UseVisualStyleBackColor = true;

        // pnlPlcConnections
        this.pnlPlcConnections.Dock = DockStyle.Fill;
        this.pnlPlcConnections.Location = new Point(3, 43);
        this.pnlPlcConnections.Name = "pnlPlcConnections";
        this.pnlPlcConnections.Size = new Size(1186, 724);
        this.pnlPlcConnections.TabIndex = 0;

        // btnSettings
        this.btnSettings.Dock = DockStyle.Top;
        this.btnSettings.Location = new Point(3, 3);
        this.btnSettings.Name = "btnSettings";
        this.btnSettings.Size = new Size(1186, 40);
        this.btnSettings.TabIndex = 1;
        this.btnSettings.Text = "Global Settings (DB Path, etc.)";
        this.btnSettings.UseVisualStyleBackColor = true;

        // statusStrip
        this.statusStrip = new StatusStrip();
        this.statusStrip.BackColor = Color.FromArgb(0, 165, 217); // Tesa Blue
        this.lblStatus = new ToolStripStatusLabel();
        this.lblStatus.ForeColor = Color.White;
        this.lblStatus.Text = "Server: Chưa kết nối";
        this.statusStrip.Items.Add(this.lblStatus);

        // MainForm
        this.BackColor = Color.White;
        this.ClientSize = new Size(1200, 800);
        this.Controls.Add(this.tabControlMain);
        this.Controls.Add(this.statusStrip);
        this.Name = "MainForm";
        this.Text = "Tape Adhesion Test App - Scale Edition";
        this.WindowState = FormWindowState.Maximized;

        this.tabControlMain.ResumeLayout(false);
        this.tabDashboard.ResumeLayout(false);
        this.tabHistory.ResumeLayout(false);
        this.pnlHistoryToolbar.ResumeLayout(false);
        this.pnlHistoryToolbar.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvHistory)).EndInit();
        this.tabPlcConnections.ResumeLayout(false);
        this.statusStrip.ResumeLayout(false);
        this.statusStrip.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private TabControl tabControlMain;
    private TabPage tabDashboard;
    private TabControl tabControlRacks;
    private TabPage tabHistory;
    private Panel pnlHistoryToolbar;
    private Label lblSearch;
    private TextBox txtSearchHistory;
    private Label lblFilter;
    private ComboBox cmbFilterRackId;
    private Button btnDeleteSelected;
    private Button btnExportHistory;
    private Button btnBackupDb;
    private Button btnRestoreDb;
    private DataGridView dgvHistory;
    private TabPage tabPlcConnections;
    private FlowLayoutPanel pnlPlcConnections;
    private Button btnSettings;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel lblStatus;
}
