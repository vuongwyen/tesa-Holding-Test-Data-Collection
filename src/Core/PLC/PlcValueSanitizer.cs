using System;
using System.Collections.Generic;

namespace TapeAdhesionApp.Core.PLC;

public class PlcValueSanitizer
{
    private readonly Dictionary<int, uint> _oldValues = new();
    private readonly Dictionary<int, int> _rejectCounters = new();
    private readonly Dictionary<int, bool> _justReleasedJump = new();
    
    private const int MaxRejects = 10; // Đếm số lần reject liên tiếp trước khi buông (10 chu kỳ = 1.0 giây nếu 100ms)
    // Chờ đã: Nếu chu kỳ là 100ms thì 10 chu kỳ = 1 giây. User muốn 5 giây thì phải là 50 chu kỳ.
    // Lần quét của PlcCommunicationService là độc lập, nhưng thực tế nó được gọi từ MainPresenter (100ms).
    // Ta để 50 chu kỳ cho chắc ăn.
    private const int ForceReleaseThreshold = 50; 
    private const uint MaxAllowedJump = 6000;
    private const uint AbsoluteMaxThreshold = 60000000;

    public PlcValueSanitizer()
    {
    }

    public (uint sanitizedValue, bool isGood) Sanitize(int address, uint currentValue, bool isFirstRun)
    {
        if (isFirstRun)
        {
            if (currentValue > AbsoluteMaxThreshold)
            {
                // Tầng 1: Rác tuyệt đối ở lần đọc đầu
                System.Diagnostics.Debug.WriteLine($"[Sanitizer] Address {address} - Giá trị nền phi lý: {currentValue}. Ép về 0.");
                _oldValues[address] = 0;
                _rejectCounters[address] = 0;
                _justReleasedJump[address] = false;
                return (0, false);
            }
            
            _oldValues[address] = currentValue;
            _rejectCounters[address] = 0;
            _justReleasedJump[address] = false;
            return (currentValue, true);
        }

        if (!_oldValues.TryGetValue(address, out uint oldValue))
        {
            oldValue = 0;
        }



        // Tình huống 2: Đứng im
        if (currentValue == oldValue)
        {
            _rejectCounters[address] = 0;
            return (currentValue, true);
        }

        // Tình huống 3: Tăng
        if (currentValue > oldValue)
        {
            uint diff = currentValue - oldValue;
            if (diff <= MaxAllowedJump)
            {
                // Tăng bình thường
                _oldValues[address] = currentValue;
                _rejectCounters[address] = 0;
                _justReleasedJump[address] = false;
                return (currentValue, true);
            }
            else
            {
                // Tăng đột biến (Nhảy ảo)
                return HandleRejection(address, currentValue, oldValue, "Tăng đột biến");
            }
        }

        // Tình huống 4: Giảm (Rác chiều giảm, tụt lơ lửng)
        if (currentValue < oldValue)
        {
            // BẪY KÉP: Nếu vừa bị BUÔNG do nhảy ảo, mà giờ lại tụt giảm (về lại số thực)
            if (_justReleasedJump.TryGetValue(address, out bool justReleased) && justReleased)
            {
                // CHẤP NHẬN NGAY LẬP TỨC
                System.Diagnostics.Debug.WriteLine($"[Sanitizer] Address {address} - Thoát bẫy kép (về số thực): {oldValue} -> {currentValue}");
                _oldValues[address] = currentValue;
                _rejectCounters[address] = 0;
                _justReleasedJump[address] = false;
                return (currentValue, true);
            }

            return HandleRejection(address, currentValue, oldValue, "Giảm bất thường");
        }

        return (currentValue, true);
    }

    private (uint sanitizedValue, bool isGood) HandleRejection(int address, uint currentValue, uint oldValue, string reason)
    {
        if (!_rejectCounters.TryGetValue(address, out int rejects))
            rejects = 0;

        rejects++;
        _rejectCounters[address] = rejects;

        if (rejects >= ForceReleaseThreshold)
        {
            // Ép BUÔNG (Force Release)
            System.Diagnostics.Debug.WriteLine($"[Sanitizer] Address {address} - BUÔNG sau {rejects} lần chặn. Chấp nhận rác: {oldValue} -> {currentValue}");
            _oldValues[address] = currentValue;
            _rejectCounters[address] = 0;
            
            // Ghi nhớ trạng thái vừa buông rác nhảy ảo
            if (currentValue > oldValue)
                _justReleasedJump[address] = true;
                
            return (currentValue, true);
        }
        else
        {
            if (rejects == 1 || rejects % 10 == 0) // Log less frequently
                System.Diagnostics.Debug.WriteLine($"[Sanitizer] Address {address} - {reason}: {oldValue} -> {currentValue} (Từ chối lần {rejects}/{ForceReleaseThreshold})");
            
            return (oldValue, false);
        }
    }
}
