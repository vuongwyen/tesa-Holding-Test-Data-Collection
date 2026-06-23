using System;
using System.Drawing;
using System.Windows.Forms;
using TapeAdhesionApp.Data.Database;

namespace TapeAdhesionApp.UI.Views;

public partial class SettingsForm : Form
{
    private readonly SettingsRepository _settingsRepo;

    public SettingsForm(SettingsRepository settingsRepo)
    {
        _settingsRepo = settingsRepo;
        InitializeComponent();
    }

    private async void SettingsForm_Load(object sender, EventArgs e)
    {
        try
        {
            var ip = await _settingsRepo.GetSettingAsync("PlcIpAddress");
            if (!string.IsNullOrEmpty(ip))
            {
                txtIpAddress.Text = ip;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải cài đặt: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnSave_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtIpAddress.Text))
        {
            MessageBox.Show("Vui lòng nhập IP.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            await _settingsRepo.SaveSettingAsync("PlcIpAddress", txtIpAddress.Text.Trim());
            MessageBox.Show("Lưu cài đặt thành công. Vui lòng bấm CONNECT lại để áp dụng.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
}
