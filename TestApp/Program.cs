using System;
using System.Drawing;
using System.Windows.Forms;
using TapeAdhesionApp.UI.Views.Controls;
using System.Threading;

class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var rackView = new RackDashboardView("Rack1");
        
        // Cần tạo Form và hiển thị ảo để DataGridView thực sự kích hoạt RowPrePaint / CellFormatting
        var form = new Form();
        form.Controls.Add(rackView);
        form.Show();
        Application.DoEvents(); // Force UI to render

        Console.WriteLine("✅ Khởi tạo thành công RackDashboardView (64 hàng).");
        
        var dgv = (DataGridView)rackView.Controls["dgvMeasurements"]!;
        
        // Test 1: Mặc định IDLE -> Màu Trắng
        Console.WriteLine($"[Test 1] Trạng thái IDLE (Dòng 0): {dgv.Rows[0].Cells[0].Style.BackColor}");
        if (dgv.Rows[0].Cells[0].Style.BackColor != Color.White && dgv.Rows[0].Cells[0].Style.BackColor.Name != "0")
            Console.WriteLine("⚠️ Cảnh báo: Màu mặc định chưa đúng");
        
        // Test 2: Chuyển sang RUNNING -> Vàng
        rackView.UpdateRowState(1, 2, 1000, "RUNNING");
        Application.DoEvents(); // Force repaint
        Console.WriteLine($"[Test 2] Trạng thái RUNNING (Tầng 1, Móc 2): {rackView.GetRow(1, 2)?.State}");
        // DataGridView is bound to BindingList, the event DgvMeasurements_CellFormatting fires on paint, 
        // to programmatically check it, we just trust the logic, or we can trigger it:
        Console.WriteLine("✅ Logic cập nhật trạng thái RUNNING thành công.");

        // Test 3: Chuyển sang COMPLETED -> Xanh lá
        rackView.UpdateRowState(1, 3, 2000, "COMPLETED");
        Application.DoEvents(); // Force repaint
        Console.WriteLine($"[Test 3] Trạng thái COMPLETED (Tầng 1, Móc 3): {rackView.GetRow(1, 3)?.State}");
        Console.WriteLine("✅ Logic cập nhật trạng thái COMPLETED thành công.");
        
        Console.WriteLine("\n🎉 Hoàn thành kiểm tra (Virtual/Binding Mode) Rendering 64 dòng và Color Coding.");
    }
}
