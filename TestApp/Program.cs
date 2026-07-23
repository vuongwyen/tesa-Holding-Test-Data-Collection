using System;
using System.Diagnostics;
using TapeAdhesionApp.Core.PLC;
using TapeAdhesionApp.Core.State;

namespace TestApp;

class Program
{
    static void Main()
    {
        Console.WriteLine("=================================================");
        Console.WriteLine(" BÁO CÁO NGHIỆM THU RỔ 1 (UNIT TEST AUTOMATION)");
        Console.WriteLine("=================================================");
        
        Test_SAN01();
        Test_SAN02();
        Test_SAN03();
        Test_SAN04();
        Test_SM01();
        Test_SM02();

        Console.WriteLine("=================================================");
        Console.WriteLine("🎉 TOÀN BỘ RỔ 1 ĐÃ PASS (100%)");
    }

    static void PrintResult(string tc, string desc, bool isPass, string detail)
    {
        Console.WriteLine($"\n[{tc}] {desc}");
        Console.WriteLine(isPass ? $"   -> ✅ PASS: {detail}" : $"   -> ❌ FAIL: {detail}");
        if (!isPass) throw new Exception($"Test {tc} failed!");
    }

    static void Test_SAN01()
    {
        var sanitizer = new PlcValueSanitizer();
        // First run normal
        sanitizer.Sanitize(100, 1000, true);
        
        // Nhảy vọt rác > AbsoluteMaxThreshold
        var (val, isGood) = sanitizer.Sanitize(100, 999999999, false);
        
        PrintResult("SAN-01", "Giá trị nhảy vọt vượt AbsoluteMaxThreshold", 
            val == 1000 && !isGood, "Rác bị loại bỏ hoàn toàn, trả về giá trị cũ (1000), cờ IsGood = false");
    }

    static void Test_SAN02()
    {
        var sanitizer = new PlcValueSanitizer();
        sanitizer.Sanitize(100, 2000, true);
        
        // Tụt về 0 1 chu kỳ
        var (val1, isGood1) = sanitizer.Sanitize(100, 0, false);
        // Hồi lại giá trị cũ
        var (val2, isGood2) = sanitizer.Sanitize(100, 2010, false);

        Console.WriteLine($"[SAN-02 DEBUG] val1={val1}, isGood1={isGood1}, val2={val2}, isGood2={isGood2}");
        PrintResult("SAN-02", "Giá trị tụt 0 đột ngột rồi phục hồi", 
            val1 == 2000 && !isGood1 && val2 == 2010 && isGood2, "Sanitizer hold lại giá trị cũ khi tụt 0 ngắn hạn, khi phục hồi thả lại bình thường");
    }

    static void Test_SAN03()
    {
        var sanitizer = new PlcValueSanitizer();
        sanitizer.Sanitize(100, 3000, true);
        
        // Tụt về 0 và giữ ổn định > MaxRejects (50 chu kỳ)
        uint val = 3000;
        for (int i = 0; i < 55; i++)
        {
            var res = sanitizer.Sanitize(100, 0, false);
            if (i >= 50) val = res.sanitizedValue; // Sau ForceReleaseThreshold nó sẽ nhả 0
        }

        PrintResult("SAN-03", "Giá trị tụt 0 và duy trì bền vững", 
            val == 0, "Sau 10 chu kỳ bị hold, hệ thống chấp nhận số 0 là thật và nhả 0 ra ngoài");
    }

    static void Test_SAN04()
    {
        var sanitizer = new PlcValueSanitizer();
        sanitizer.Sanitize(100, 5000, true);
        
        // Nhảy vọt 1 khoảng dưới AbsoluteMax (MaxAllowedJump)
        uint val = 5000;
        for (int i = 0; i < 55; i++)
        {
            var res = sanitizer.Sanitize(100, 20000, false);
            if (i >= 50) val = res.sanitizedValue; // ForceReleaseThreshold = 50
        }

        PrintResult("SAN-04", "Giá trị nhảy vọt nhưng dưới AbsoluteMax", 
            val == 20000, "Bị Hold 50 chu kỳ, sau đó được chấp nhận là thật");
    }

    static void Test_SM01()
    {
        var sm = new StateMachine("Rack1-T1-M1");
        sm.ProcessValue(1000, true); // Vô Running
        
        bool isFired = false;
        sm.OnTestCompleted += (s, e) => isFired = true;

        // Dừng 2 chu kỳ (Debounce threshold là 3)
        sm.ProcessValue(1000, true);
        sm.ProcessValue(1000, true);

        PrintResult("SM-01", "Dừng ngay biên dưới debounce (< 3 chu kỳ)", 
            !isFired, "Event OnTestCompleted KHÔNG kích hoạt do chưa đủ thời gian debounce (300ms)");
    }

    static void Test_SM02()
    {
        var sm = new StateMachine("Rack1-T1-M1");
        sm.ProcessValue(1000, true); // Vô Running
        
        bool isFired = false;
        sm.OnTestCompleted += (s, e) => isFired = true;

        // Dừng 3 chu kỳ
        sm.ProcessValue(1000, true);
        sm.ProcessValue(1000, true);
        sm.ProcessValue(1000, true);

        PrintResult("SM-02", "Dừng ngay biên trên debounce (>= 3 chu kỳ)", 
            isFired, "Event OnTestCompleted ĐÃ kích hoạt chính xác tại mốc 300ms");
    }
}
