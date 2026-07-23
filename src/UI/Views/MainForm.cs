using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using Microsoft.Data.Sqlite;
using TapeAdhesionApp.Core.Models;
using TapeAdhesionApp.Data.Database;
using TapeAdhesionApp.UI.Views.Controls;

namespace TapeAdhesionApp.UI.Views;

public partial class MainForm : Form, IMainView
{
    private List<TestRecord> _allRecords = new();
    private Dictionary<string, RackDashboardView> _rackDashboards = new();
    private Dictionary<string, PlcConnectionCard> _connectionCards = new();
    private Dictionary<string, bool> _rackFlashState = new();

    public MainForm()
    {
        InitializeComponent();
        TapeAdhesionApp.UI.Utils.ControlExtensions.EnableDoubleBuffered(dgvHistory);
        InitializeRacks();
        InitializeHistoryToolbar();

        btnSettings.Click += (s, e) => SettingsClicked?.Invoke("");
    }

    private void InitializeRacks()
    {
        string[] rackIds = { "Rack1", "Rack2", "Rack3", "Rack4" };
        
        foreach (var rackId in rackIds)
        {
            _rackFlashState[rackId] = false;

            // 1. Create Dashboard View
            var dashboard = new RackDashboardView(rackId) { Dock = DockStyle.Fill };
            _rackDashboards[rackId] = dashboard;
            
            var tabPage = new TabPage(rackId) { Name = rackId };
            tabPage.Controls.Add(dashboard);
            tabControlRacks.TabPages.Add(tabPage);

            // 2. Create Connection Card
            var card = new PlcConnectionCard(rackId, "192.168.2.1");
            card.ConnectRequested += (id, ip) => ConnectRackClicked?.Invoke(id, ip);
            card.DisconnectRequested += (id) => DisconnectRackClicked?.Invoke(id);
            card.SettingsRequested += () => SettingsClicked?.Invoke(rackId);
            card.ScannerRequested += () => ScannerClicked?.Invoke(rackId);
            _connectionCards[rackId] = card;
            pnlPlcConnections.Controls.Add(card);

            // 3. Add to Filter
            cmbFilterRackId.Items.Add(rackId);
        }
    }

    private void InitializeHistoryToolbar()
    {
        cmbFilterRackId.Items.Insert(0, "Tất cả");
        cmbFilterRackId.SelectedIndex = 0;

        txtSearchHistory.TextChanged += (s, e) => ApplyFilter();
        cmbFilterRackId.SelectedIndexChanged += (s, e) => ApplyFilter();

        btnDeleteSelected.Click += BtnDeleteSelected_Click;
        btnExportHistory.Click += BtnExportHistory_Click;
        btnBackupDb.Click += BtnBackupDb_Click;
        btnRestoreDb.Click += BtnRestoreDb_Click;

        dgvHistory.CellFormatting += DgvHistory_CellFormatting;
    }

    private void DgvHistory_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
        {
            var column = dgvHistory.Columns[e.ColumnIndex];
            if (column.DataPropertyName == "DropTime" && e.Value is uint msValue)
            {
                e.Value = (msValue / 60000.0).ToString("F2") + " phút";
                e.FormattingApplied = true;
            }
        }
    }

    private void TabControlRacks_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;

        var tabPage = tabControlRacks.TabPages[e.Index];
        var rackId = tabPage.Name;
        
        bool isFlashing = _rackFlashState.ContainsKey(rackId) && _rackFlashState[rackId];
        bool isSelected = e.State.HasFlag(DrawItemState.Selected);
        
        // Background
        Brush backBrush = new SolidBrush(Color.White);
        if (isFlashing) backBrush = new SolidBrush(Color.FromArgb(200, 255, 200)); // Light green
        
        e.Graphics.FillRectangle(backBrush, e.Bounds);

        // Tesa Red Color for active tab
        Color tesaRed = Color.FromArgb(227, 0, 15);
        Color tesaGray = Color.FromArgb(100, 100, 100);

        // Text
        Brush textBrush = new SolidBrush(isSelected ? tesaRed : tesaGray);
        Font textFont = new Font(e.Font ?? tabControlRacks.Font, isSelected ? FontStyle.Bold : FontStyle.Regular);
        
        StringFormat format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        e.Graphics.DrawString(tabPage.Text, textFont, textBrush, e.Bounds, format);

        // Red Underline for active tab
        if (isSelected)
        {
            using Pen redPen = new Pen(tesaRed, 3);
            e.Graphics.DrawLine(redPen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        }

        backBrush.Dispose();
        textBrush.Dispose();
        textFont.Dispose();
    }

    // --- IMainView Events ---
    public event Action<string, string>? ConnectRackClicked;
    public event Action<string>? DisconnectRackClicked;
    public event Action<string>? SettingsClicked;
    public event Action<string>? ScannerClicked;
    public event Action? LoadHistory;
    public event Action<List<int>>? DeleteSelectedRecordsClicked;
    public event Action<string?>? ExportHistoryClicked;

    // --- History Logic ---
    public void LoadHistoryData(IEnumerable<TestRecord> records)
    {
        InvokeOnUI(() =>
        {
            _allRecords = records.ToList();
            ApplyFilter();
        });
    }

    public void AddTestRecord(TestRecord record)
    {
        InvokeOnUI(() =>
        {
            _allRecords.Insert(0, record);
            ApplyFilter();
        });
    }

    private void ApplyFilter()
    {
        string searchText = txtSearchHistory.Text.Trim().ToLower();
        string selectedRack = cmbFilterRackId.SelectedItem?.ToString() ?? "Tất cả";

        var filtered = _allRecords.AsEnumerable();

        if (selectedRack != "Tất cả")
        {
            filtered = filtered.Where(r => r.RackId == selectedRack);
        }

        if (!string.IsNullOrEmpty(searchText))
        {
            filtered = filtered.Where(r => 
                (r.BatchCode != null && r.BatchCode.ToLower().Contains(searchText)) || 
                (r.NartCode != null && r.NartCode.ToLower().Contains(searchText)) ||
                (r.Tester != null && r.Tester.ToLower().Contains(searchText)));
        }

        dgvHistory.DataSource = new BindingList<TestRecord>(filtered.ToList());
    }

    private void BtnDeleteSelected_Click(object? sender, EventArgs e)
    {
        if (dgvHistory.SelectedRows.Count == 0) return;

        var confirm = MessageBox.Show($"Xóa {dgvHistory.SelectedRows.Count} bản ghi?", "Xác nhận", MessageBoxButtons.YesNo);
        if (confirm != DialogResult.Yes) return;

        var selectedIds = new List<int>();
        foreach (DataGridViewRow row in dgvHistory.SelectedRows)
        {
            if (row.DataBoundItem is TestRecord record)
            {
                selectedIds.Add(record.Id);
                _allRecords.RemoveAll(r => r.Id == record.Id);
            }
        }

        ApplyFilter();
        DeleteSelectedRecordsClicked?.Invoke(selectedIds);
    }

    private void BtnExportHistory_Click(object? sender, EventArgs e)
    {
        string? rackId = cmbFilterRackId.SelectedItem?.ToString();
        if (rackId == "Tất cả") rackId = null;
        
        ExportHistoryClicked?.Invoke(rackId);
    }

    // --- Dashboard Updates ---
    public void UpdateRackConnectionStatus(string rackId, bool isConnected)
    {
        InvokeOnUI(() =>
        {
            if (_connectionCards.TryGetValue(rackId, out var card))
            {
                card.UpdateStatus(isConnected);
            }
            if (_rackDashboards.TryGetValue(rackId, out var dashboard))
            {
                dashboard.SetConnectionState(isConnected);
            }
        });
    }

    public void UpdateMeasurementRow(string rackId, int floor, int hookIndex, uint value, string state)
    {
        InvokeOnUI(() =>
        {
            if (_rackDashboards.TryGetValue(rackId, out var dashboard))
            {
                dashboard.UpdateRowState(floor, hookIndex, value, state);
            }
        });
    }

    public void FlashRackTab(string rackId)
    {
        InvokeOnUI(() =>
        {
            _rackFlashState[rackId] = true;
            tabControlRacks.Invalidate(); // Trigger DrawItem
            
            // Auto reset flash after 5 seconds
            var timer = new System.Windows.Forms.Timer { Interval = 5000 };
            timer.Tick += (s, e) => 
            {
                _rackFlashState[rackId] = false;
                tabControlRacks.Invalidate();
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        });
    }

    public MeasurementRow? GetRowData(string rackId, int floor, int hookIndex)
    {
        if (_rackDashboards.TryGetValue(rackId, out var dashboard))
        {
            return dashboard.GetRow(floor, hookIndex);
        }
        return null;
    }

    public void InvokeOnUI(Action action)
    {
        if (this.IsDisposed || !this.IsHandleCreated) return;
        
        if (this.InvokeRequired)
        {
            this.BeginInvoke(action);
        }
        else
        {
            action();
        }
    }
    private void BtnBackupDb_Click(object? sender, EventArgs e)
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "SQLite DB|*.db",
            Title = "Lưu bản sao DB",
            FileName = $"TesaBackup_{DateTime.Now:yyyyMMdd_HHmmss}.db"
        };

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            try
            {
                using var source = new SqliteConnection(DatabaseInitializer.ConnectionString);
                source.Open();
                using var destination = new SqliteConnection($"Data Source={sfd.FileName}");
                source.BackupDatabase(destination);
                MessageBox.Show("Sao lưu thành công!", "Hoàn thành", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi sao lưu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void BtnRestoreDb_Click(object? sender, EventArgs e)
    {
        if (MessageBox.Show("Khôi phục sẽ ghi đè dữ liệu hiện tại và yêu cầu khởi động lại App. Bạn có chắc chắn?", "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "SQLite DB|*.db",
                Title = "Chọn file DB khôi phục"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    SqliteConnection.ClearAllPools();
                    using var source = new SqliteConnection($"Data Source={ofd.FileName}");
                    source.Open();
                    using var destination = new SqliteConnection(DatabaseInitializer.ConnectionString);
                    source.BackupDatabase(destination);
                    
                    MessageBox.Show("Khôi phục thành công! App sẽ tự động thoát. Vui lòng mở lại.", "Hoàn thành", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Exit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khôi phục: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
