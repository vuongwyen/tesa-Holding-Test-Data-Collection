using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TapeAdhesionApp.Core.Models;
using TapeAdhesionApp.Data.Database;

namespace TapeAdhesionApp.UI.Views;

public partial class SettingsForm : Form
{
    private readonly string _rackId;
    private BindingList<AddressRow> _rows;

    public SettingsForm(string rackId)
    {
        _rackId = rackId;
        _rows = new BindingList<AddressRow>();
        InitializeComponent();
        TapeAdhesionApp.UI.Utils.ControlExtensions.EnableDoubleBuffered(dgvAddresses);
        this.Text = $"Cấu hình PLC - {rackId}";
    }

    private async void SettingsForm_Load(object sender, EventArgs e)
    {
        dgvAddresses.AutoGenerateColumns = false;
        dgvAddresses.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            DataPropertyName = "Location", 
            HeaderText = "Vị trí", 
            ReadOnly = true,
            FillWeight = 50
        });
        dgvAddresses.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            DataPropertyName = "Address", 
            HeaderText = "Địa chỉ VD", 
            FillWeight = 50 
        });

        try
        {
            using var conn = new Microsoft.Data.Sqlite.SqliteConnection(DatabaseInitializer.ConnectionString);
            var json = await Dapper.SqlMapper.QueryFirstOrDefaultAsync<string>(conn, "SELECT Value FROM Settings WHERE Key = @Key", new { Key = $"PlcAddresses_{_rackId}" });
            
            int[] addresses = new int[64];
            for (int i = 0; i < 64; i++) addresses[i] = -1;

            if (!string.IsNullOrEmpty(json))
            {
                try { addresses = System.Text.Json.JsonSerializer.Deserialize<int[]>(json) ?? addresses; } catch {}
            }

            for (int i = 0; i < 64; i++)
            {
                int floor = (i / 16) + 1;
                int hook = (i % 16) + 1;
                _rows.Add(new AddressRow
                {
                    Index = i,
                    Location = $"Tầng {floor} - Móc {hook}",
                    Address = addresses[i] == -1 ? "" : addresses[i].ToString()
                });
            }
            dgvAddresses.DataSource = _rows;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi tải danh sách địa chỉ: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnBulkImport_Click(object sender, EventArgs e)
    {
        var inputForm = new Form
        {
            Text = "Nhập Sỉ Cấu Hình (Bulk Import)",
            Size = new Size(400, 500),
            StartPosition = FormStartPosition.CenterParent
        };
        
        var txtInput = new TextBox
        {
            Multiline = true,
            Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical,
            Text = "Dán dữ liệu từ Excel vào đây (Ví dụ: L1P1  110)\nHoặc cấu trúc: S4L1P1 110\n"
        };
        
        var btnApply = new Button { Text = "Áp dụng", Dock = DockStyle.Bottom, Height = 40, BackColor = Color.FromArgb(0, 165, 217), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnApply.Click += (s, args) =>
        {
            try
            {
                int matchCount = 0;
                var lines = txtInput.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    // Match pattern like L1P1 110 or S4L1P1 110
                    var match = System.Text.RegularExpressions.Regex.Match(line, @"L(\d+)P(\d+)\s+(\d+)");
                    if (match.Success)
                    {
                        int floor = int.Parse(match.Groups[1].Value);
                        int hook = int.Parse(match.Groups[2].Value);
                        string vdAddress = match.Groups[3].Value;
                        
                        if (floor >= 1 && floor <= 4 && hook >= 1 && hook <= 16)
                        {
                            if (int.TryParse(vdAddress, out int addressNum) && addressNum % 4 != 0)
                            {
                                MessageBox.Show($"Địa chỉ {vdAddress} tại Tầng {floor} Móc {hook} không hợp lệ (Phải chia hết cho 4). Đã bỏ qua.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                continue;
                            }
                            
                            int index = (floor - 1) * 16 + (hook - 1);
                            _rows[index].Address = vdAddress;
                            matchCount++;
                        }
                    }
                }
                MessageBox.Show($"Đã gán {matchCount} địa chỉ thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dgvAddresses.Refresh();
                inputForm.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi parse dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        inputForm.Controls.Add(txtInput);
        inputForm.Controls.Add(btnApply);
        inputForm.ShowDialog();
    }


    private async void BtnSave_Click(object sender, EventArgs e)
    {
        int[] addresses = new int[64];
        for (int i = 0; i < 64; i++)
        {
            string val = _rows[i].Address;
            if (string.IsNullOrWhiteSpace(val))
            {
                addresses[i] = -1;
            }
            else
            {
                string numericPart = new string(System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Where(val, char.IsDigit)));
                if (int.TryParse(numericPart, out int num))
                {
                    if (num % 4 != 0)
                    {
                        int floor = (i / 16) + 1;
                        int hook = (i % 16) + 1;
                        MessageBox.Show($"Lỗi tại Tầng {floor} - Móc {hook}: Địa chỉ VD{num} không hợp lệ. Các địa chỉ VD phải chia hết cho 4 (VD: 100, 104, 108...).", "Lỗi Cấu Hình", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    addresses[i] = num;
                }
                else
                {
                    addresses[i] = -1;
                }
            }
        }

        try
        {
            string json = System.Text.Json.JsonSerializer.Serialize(addresses);
            using var conn = new Microsoft.Data.Sqlite.SqliteConnection(DatabaseInitializer.ConnectionString);
            await Dapper.SqlMapper.ExecuteAsync(conn, @"
                INSERT INTO Settings (Key, Value) VALUES (@Key, @Value)
                ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value", 
                new { Key = $"PlcAddresses_{_rackId}", Value = json });

            PlcTags.LoadAddresses(_rackId, addresses);
            MessageBox.Show("Lưu cấu hình thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi lưu cài đặt: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
        this.Close();
    }

    public class AddressRow
    {
        public int Index { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
