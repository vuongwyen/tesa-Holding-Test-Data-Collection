using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TapeAdhesionApp.Core.PLC;

namespace TapeAdhesionApp.UI.Views;

public partial class ScannerForm : Form
{
    private readonly string _rackId;
    private readonly PlcCommunicationService _plcManager;
    private CancellationTokenSource? _cts;
    private bool _isScanning = false;
    private Dictionary<int, uint> _lastValues = new();
    
    public string? LastDiscoveredAddress { get; private set; }

    public ScannerForm(string rackId, IPlcService plcManager)
    {
        _rackId = rackId;
        _plcManager = (PlcCommunicationService)plcManager;
        
        InitializeComponent();
        this.Text = $"PLC Scanner - {rackId} (Broker Mode)";
    }

    private async void BtnStartScan_Click(object sender, EventArgs e)
    {
        if (_isScanning)
        {
            StopScan();
            return;
        }

        var addressesToScan = ParseAddresses(txtAddresses.Text);
        if (addressesToScan.Count == 0)
        {
            MessageBox.Show("Vui lòng nhập ít nhất 1 địa chỉ VD hợp lệ!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }


        if (!_plcManager.IsConnected)
        {
            lblStatus.Text = "Trạng thái: Đang kết nối PLC...";
            lblStatus.ForeColor = Color.Orange;
            
            // Wait up to 3 seconds for connection
            int timeout = 30;
            while (!_plcManager.IsConnected && timeout > 0)
            {
                await Task.Delay(100);
                timeout--;
            }

            if (!_plcManager.IsConnected)
            {
                MessageBox.Show("Không thể kết nối đến PLC. Vui lòng kiểm tra lại IP và mạng.", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Trạng thái: Đã dừng (Lỗi kết nối)";
                lblStatus.ForeColor = Color.Red;
                return;
            }
        }

        StartScan(addressesToScan);
    }

    private void StartScan(List<int> addresses)
    {
        _isScanning = true;
        btnStartScan.Text = "DỪNG QUÉT";
        btnStartScan.BackColor = Color.Gray;
        lblStatus.Text = $"Trạng thái: Đang quét {addresses.Count} địa chỉ...";
        lblStatus.ForeColor = Color.Green;
        txtAddresses.Enabled = false;

        _lastValues.Clear();
        foreach (var addr in addresses)
        {
            _lastValues[addr] = 0; // Initialize with 0
        }

        _cts = new CancellationTokenSource();
        _ = ScanLoopAsync(addresses, _cts.Token);
    }

    private void StopScan()
    {
        _isScanning = false;
        _cts?.Cancel();
        btnStartScan.Text = "BẮT ĐẦU QUÉT";
        btnStartScan.BackColor = Color.FromArgb(227, 0, 15);
        lblStatus.Text = "Trạng thái: Đã dừng";
        lblStatus.ForeColor = Color.Gray;
        txtAddresses.Enabled = true;
    }

        private readonly PlcValueSanitizer _sanitizer = new();

        private async Task ScanLoopAsync(List<int> addresses, CancellationToken token)
        {
            bool isFirstRun = true;
            
            // Tìm khoảng nhớ chứa tất cả địa chỉ
            int minAddress = addresses.Min();
            int maxAddress = addresses.Max();
            int lengthToRead = maxAddress - minAddress + 4; // Cần đọc đến hết 4 byte của địa chỉ lớn nhất

            while (!token.IsCancellationRequested && _plcManager.IsConnected)
            {
                if (token.IsCancellationRequested) break;

                // Hỏi PLC 1 cục duy nhất!
                var buffer = await _plcManager.ReadRawBytesAsync(minAddress, lengthToRead);

                if (buffer != null && buffer.Length == lengthToRead)
                {
                    // Lọc từng giá trị ra từ RAM
                    foreach (var addr in addresses)
                    {
                        if (token.IsCancellationRequested) break;
                        
                        int localOffset = addr - minAddress;
                        
                        uint rawVal = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(
                            new ReadOnlySpan<byte>(buffer, localOffset, 4)
                        );
                        
                        var (sanitizedVal, isGood) = _sanitizer.Sanitize(addr, rawVal, isFirstRun);

                        bool showRaw = chkShowRawData.Checked;
                        uint displayVal = showRaw ? rawVal : sanitizedVal;

                        if (isFirstRun)
                        {
                            _lastValues[addr] = displayVal;
                            
                            // Báo cáo mọi giá trị kể cả bằng 0 trong lần quét đầu tiên
                            if (showRaw || isGood)
                            {
                                LogStationary(addr, displayVal);
                            }
                        }
                        else
                        {
                            if (_lastValues[addr] != displayVal)
                            {
                                uint oldVal = _lastValues[addr];
                                _lastValues[addr] = displayVal;

                                if (showRaw || isGood)
                                {
                                    LogChange(addr, oldVal, displayVal);
                                }
                            }
                        }
                    }
                }
            
                isFirstRun = false;
                await Task.Delay(500, token).ContinueWith(t => { }); // Nghỉ nửa giây
            }
        }

    private void LogChange(int address, uint oldValue, uint newValue)
    {
        if (this.InvokeRequired)
        {
            this.BeginInvoke(new Action(() => LogChange(address, oldValue, newValue)));
            return;
        }

        string time = DateTime.Now.ToString("HH:mm:ss");
        string oldMin = (oldValue / 600.0).ToString("F1");
        string newMin = (newValue / 600.0).ToString("F1");
        string logMsg = $"[{time}] VD{address} thay đổi: {oldValue} -> {newValue} ({oldMin}p -> {newMin}p)";
        
        lstLogs.Items.Insert(0, logMsg);
        
        // Keep only last 1000 logs
        if (lstLogs.Items.Count > 1000)
        {
            lstLogs.Items.RemoveAt(1000);
        }

        LastDiscoveredAddress = address.ToString();
    }

    private void LogStationary(int address, uint value)
    {
        if (this.InvokeRequired)
        {
            this.BeginInvoke(new Action(() => LogStationary(address, value)));
            return;
        }

        string time = DateTime.Now.ToString("HH:mm:ss");
        string min = (value / 600.0).ToString("F1");
        string logMsg = $"[{time}] VD{address} đứng yên: {value} ({min}p)";
        
        lstLogs.Items.Insert(0, logMsg);
        
        if (lstLogs.Items.Count > 1000)
        {
            lstLogs.Items.RemoveAt(1000);
        }
        
        LastDiscoveredAddress = address.ToString();
    }

    private List<int> ParseAddresses(string text)
    {
        var result = new List<int>();
        var tokens = text.Split(new[] { ',', ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var token in tokens)
        {
            string numericPart = new string(token.Where(char.IsDigit).ToArray());
            if (int.TryParse(numericPart, out int num))
            {
                if (!result.Contains(num))
                {
                    result.Add(num);
                }
            }
        }
        
        return result;
    }

    private void BtnApply_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(LastDiscoveredAddress))
        {
            MessageBox.Show("Chưa quét được địa chỉ nào để copy!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        
        Clipboard.SetText(LastDiscoveredAddress);
        MessageBox.Show($"Đã copy VD{LastDiscoveredAddress} vào Clipboard.\nBố có thể Paste (Ctrl+V) vào ô tương ứng ở màn hình cài đặt.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ScannerForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        StopScan();
        _plcManager.Disconnect();
        _plcManager.Dispose();
    }
    private async void BtnSearchValue_Click(object sender, EventArgs e)
    {
        string? inputStr = Prompt.ShowDialog("Nhập giá trị thời gian (PHÚT) cần tìm:", "Truy vết Giá trị");
        if (string.IsNullOrWhiteSpace(inputStr) || !int.TryParse(inputStr, out int minutes)) return;

        uint targetRaw = (uint)(minutes * 600);
        uint tolerance = 600; // +- 1 minute tolerance

        lblStatus.Text = $"Trạng thái: Đang truy vết giá trị ~ {minutes} phút (Raw: {targetRaw})...";
        lblStatus.ForeColor = Color.Orange;
        lstLogs.Items.Clear();
        lstLogs.Items.Add($"Bắt đầu quét tìm giá trị Raw ~ {targetRaw} (+- {tolerance})");

        try
        {
            // Quét VD0 đến VD8000
            int startAddress = 0;
            int totalBytes = 8000;
            int maxPdu = 200;
            
            var results = new List<string>();

            for (int offset = 0; offset < totalBytes; offset += maxPdu)
            {
                int readLen = Math.Min(maxPdu, totalBytes - offset);
                var buffer = await _plcManager.ReadRawBytesAsync(startAddress + offset, readLen);
                
                if (buffer != null)
                {
                    for (int i = 0; i <= buffer.Length - 4; i++)
                    {
                        uint val = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(
                            new ReadOnlySpan<byte>(buffer, i, 4)
                        );

                        if (Math.Abs((long)val - targetRaw) <= tolerance)
                        {
                            int foundAddr = startAddress + offset + i;
                            double foundMin = val / 600.0;
                            results.Add($"[VD{foundAddr}] => Raw: {val} (~ {foundMin:F1} phút)");
                        }
                    }
                }
            }

            if (results.Count > 0)
            {
                lstLogs.Items.Add($"TÌM THẤY {results.Count} KẾT QUẢ:");
                foreach (var r in results) lstLogs.Items.Add(r);
            }
            else
            {
                lstLogs.Items.Add("Không tìm thấy địa chỉ nào khớp.");
            }
            
            lblStatus.Text = "Trạng thái: Đã truy vết xong.";
            lblStatus.ForeColor = Color.Green;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi truy vết: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblStatus.Text = "Trạng thái: Lỗi";
            lblStatus.ForeColor = Color.Red;
        }
    }
}

public static class Prompt
{
    public static string? ShowDialog(string text, string caption)
    {
        Form prompt = new Form()
        {
            Width = 400, Height = 170, FormBorderStyle = FormBorderStyle.FixedDialog,
            Text = caption, StartPosition = FormStartPosition.CenterParent
        };
        Label textLabel = new Label() { Left = 20, Top = 20, Text = text, Width = 350 };
        TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 340 };
        Button confirmation = new Button() { Text = "OK", Left = 260, Width = 100, Top = 80, DialogResult = DialogResult.OK };
        prompt.Controls.Add(textBox);
        prompt.Controls.Add(confirmation);
        prompt.Controls.Add(textLabel);
        prompt.AcceptButton = confirmation;
        return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : null;
    }
}
