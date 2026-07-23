using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TapeAdhesionApp.Core.Models;
using TapeAdhesionApp.Core.State;

namespace IntegrationTests;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=================================================");
        Console.WriteLine(" BÁO CÁO NGHIỆM THU RỔ 2 (INTEGRATION & PERF)");
        Console.WriteLine("=================================================");

        await Test_PLC01_Disconnect();
        Test_PERF03_BulkLoad();

        Console.WriteLine("=================================================");
        Console.WriteLine("🎉 TOÀN BỘ RỔ 2 ĐÃ PASS (100%)");
    }

    static void PrintResult(string tc, string desc, bool isPass, string detail)
    {
        Console.WriteLine($"\n[{tc}] {desc}");
        Console.WriteLine(isPass ? $"   -> ✅ PASS: {detail}" : $"   -> ❌ FAIL: {detail}");
        if (!isPass) throw new Exception($"Test {tc} failed!");
    }

    static async Task Test_PLC01_Disconnect()
    {
        var mockPlc = new MockPlcService("Rack1");
        await mockPlc.ConnectAsync("127.0.0.1");

        bool exceptionCaught = false;
        try
        {
            // Simulate sudden drop
            mockPlc.SimulateNetworkDrop = true;
            var data = await mockPlc.ReadPlcDataAsync();
        }
        catch (Exception ex)
        {
            exceptionCaught = true;
            Console.WriteLine($"      (Bắt được lỗi mô phỏng: {ex.Message})");
        }

        PrintResult("PLC-01", "Ngắt kết nối mạng PLC đột ngột", 
            exceptionCaught && !mockPlc.IsConnected, 
            "Hệ thống phát hiện mất mạng ngay lập tức và ném exception an toàn để Presenter bắt (không crash App).");
    }

    static void Test_PERF03_BulkLoad()
    {
        var stateMachines = new StateMachine[64];
        for (int i = 0; i < 64; i++)
        {
            stateMachines[i] = new StateMachine($"Rack1-T{(i / 16) + 1}-M{(i % 16) + 1}");
        }

        // Bơm data 64 hook liên tục thay đổi (giả lập PLC Bulk Read)
        var sw = new Stopwatch();
        long totalProcessingTicks = 0;
        int cycles = 1000;

        for (int cycle = 0; cycle < cycles; cycle++)
        {
            sw.Restart();
            
            // Xử lý 64 giá trị cùng lúc
            for (int i = 0; i < 64; i++)
            {
                // currentValue = 1000 + cycle (luôn tăng)
                stateMachines[i].ProcessValue((uint)(1000 + cycle), true);
            }
            
            sw.Stop();
            totalProcessingTicks += sw.ElapsedTicks;
        }

        double avgMs = (double)totalProcessingTicks / TimeSpan.TicksPerMillisecond / cycles;
        
        PrintResult("PERF-03", "Chịu tải xử lý 64 Hook đồng loạt", 
            avgMs < 100.0, 
            $"Thời gian xử lý 1 vòng quét cho 64 ô nhớ trung bình tốn {avgMs:F4} ms (Yêu cầu < 100 ms). CPU siêu nhẹ.");
    }
}
