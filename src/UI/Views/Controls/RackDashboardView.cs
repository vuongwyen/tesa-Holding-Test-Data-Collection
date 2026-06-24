using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TapeAdhesionApp.Core.Models;

namespace TapeAdhesionApp.UI.Views.Controls;

public class RackDashboardView : UserControl
{
    private DataGridView dgvMeasurements;
    private BindingList<MeasurementRow> _rows;

    public string RackId { get; }

    public RackDashboardView(string rackId)
    {
        RackId = rackId;
        _rows = new BindingList<MeasurementRow>();
        
        InitializeComponent();
        InitializeGrid();
        PopulateRows();
    }

    private void InitializeComponent()
    {
        this.dgvMeasurements = new DataGridView();
        ((ISupportInitialize)(this.dgvMeasurements)).BeginInit();
        this.SuspendLayout();
        
        // dgvMeasurements
        this.dgvMeasurements.AllowUserToAddRows = false;
        this.dgvMeasurements.AllowUserToDeleteRows = false;
        this.dgvMeasurements.AllowUserToResizeRows = false;
        this.dgvMeasurements.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvMeasurements.EnableHeadersVisualStyles = false;
        this.dgvMeasurements.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        this.dgvMeasurements.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 165, 217); // Tesa Blue
        this.dgvMeasurements.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        this.dgvMeasurements.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        this.dgvMeasurements.ColumnHeadersHeight = 40;
        this.dgvMeasurements.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        this.dgvMeasurements.GridColor = Color.FromArgb(230, 230, 230);
        this.dgvMeasurements.BackgroundColor = Color.White;
        this.dgvMeasurements.BorderStyle = BorderStyle.None;
        this.dgvMeasurements.Dock = DockStyle.Fill;
        this.dgvMeasurements.Location = new Point(0, 0);
        this.dgvMeasurements.MultiSelect = false;
        this.dgvMeasurements.Name = "dgvMeasurements";
        this.dgvMeasurements.RowHeadersVisible = false;
        this.dgvMeasurements.RowTemplate.Height = 35;
        this.dgvMeasurements.Size = new Size(800, 600);
        this.dgvMeasurements.TabIndex = 0;
        this.dgvMeasurements.CellFormatting += DgvMeasurements_CellFormatting;

        this.Controls.Add(this.dgvMeasurements);
        this.Name = "RackDashboardView";
        this.Size = new Size(800, 600);
        
        ((ISupportInitialize)(this.dgvMeasurements)).EndInit();
        this.ResumeLayout(false);
    }

    private void InitializeGrid()
    {
        dgvMeasurements.AutoGenerateColumns = false;

        dgvMeasurements.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Floor", HeaderText = "Tầng", ReadOnly = true, FillWeight = 50 });
        dgvMeasurements.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "HookIndex", HeaderText = "Điểm đo", ReadOnly = true, FillWeight = 60 });
        dgvMeasurements.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Nart", HeaderText = "NART", FillWeight = 100 });
        dgvMeasurements.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Batch", HeaderText = "Batch", FillWeight = 100 });
        dgvMeasurements.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Tester", HeaderText = "Tester", FillWeight = 100 });
        dgvMeasurements.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Location", HeaderText = "Vị trí", ReadOnly = true, FillWeight = 150 });
        dgvMeasurements.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SamplePosition", HeaderText = "Vị trí mẫu", ReadOnly = false, FillWeight = 100 });
        dgvMeasurements.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PlcValue", HeaderText = "Thời gian (Phút)", ReadOnly = true, FillWeight = 80 });

        dgvMeasurements.DataSource = _rows;
    }

    private void PopulateRows()
    {
        for (int i = 0; i < 64; i++)
        {
            int floor = (i / 16) + 1;
            int hook = (i % 16) + 1;
            _rows.Add(new MeasurementRow
            {
                RackId = RackId,
                Floor = floor,
                HookIndex = hook,
                Location = $"Rack {RackId} - Tầng {floor} - Móc {hook}"
            });
        }
    }

    private void DgvMeasurements_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex >= 0 && e.RowIndex < _rows.Count)
        {
            var row = _rows[e.RowIndex];
            
            // Format PLC Value (ms) to Minutes
            if (dgvMeasurements.Columns[e.ColumnIndex].DataPropertyName == "PlcValue" && e.Value is uint msValue)
            {
                e.Value = (msValue / 60000.0).ToString("F2");
                e.FormattingApplied = true;
            }

            // Apply Row Color based on State
            if (row.State == "RUNNING")
            {
                e.CellStyle.BackColor = Color.Gold;
                e.CellStyle.ForeColor = Color.Black;
            }
            else if (row.State == "COMPLETED")
            {
                e.CellStyle.BackColor = Color.LightGreen;
                e.CellStyle.ForeColor = Color.Black;
            }
            else
            {
                e.CellStyle.BackColor = Color.White;
                e.CellStyle.ForeColor = Color.Black;
            }
        }
    }

    public MeasurementRow? GetRow(int floor, int hookIndex)
    {
        int index = (floor - 1) * 16 + (hookIndex - 1);
        if (index >= 0 && index < _rows.Count)
            return _rows[index];
        return null;
    }

    public void UpdateRowState(int floor, int hookIndex, uint value, string state)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => UpdateRowState(floor, hookIndex, value, state)));
            return;
        }

        var row = GetRow(floor, hookIndex);
        if (row != null)
        {
            row.PlcValue = value;
            row.State = state;
            // BindingList will auto notify DataGridView
        }
    }

    public void ClearRowInputs(int floor, int hookIndex)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => ClearRowInputs(floor, hookIndex)));
            return;
        }

        var row = GetRow(floor, hookIndex);
        if (row != null)
        {
            row.Nart = "";
            row.Batch = "";
            row.State = "IDLE";
            row.PlcValue = 0;
        }
    }
}
