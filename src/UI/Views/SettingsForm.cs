using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TapeAdhesionApp.Core.Models;
using TapeAdhesionApp.Data.Database;

namespace TapeAdhesionApp.UI.Views;

public partial class SettingsForm : Form
{
    private readonly SettingsRepository _settingsRepo;
    private BindingList<AddressRow> _rows;

    public SettingsForm(SettingsRepository settingsRepo)
    {
        _settingsRepo = settingsRepo;
        _rows = new BindingList<AddressRow>();
        InitializeComponent();
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
            var addresses = await _settingsRepo.GetPlcAddressesAsync();
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
            MessageBox.Show($"Lỗi tải cài đặt: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
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
                // Lọc lấy phần số nếu user gõ chữ (ví dụ "VD368" -> "368")
                string numericPart = new string(System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Where(val, char.IsDigit)));
                if (int.TryParse(numericPart, out int num))
                {
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
            await _settingsRepo.SavePlcAddressesAsync(addresses);
            PlcTags.LoadAddresses(addresses);
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
