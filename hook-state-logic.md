# Kế hoạch triển khai: Xử lý trạng thái móc (Hook State Logic)

## Mục tiêu
Dựa theo Option A đã chốt, chúng ta sẽ xây dựng trí nhớ cho phần mềm ngay từ tầng giao tiếp PLC (`PlcCommunicationService`), giúp phân biệt được 3 trạng thái của một ô nhớ:
1. **IDLE (Trắng/Mặc định)**: Ô nhớ đứng yên và có giá trị = 0 (Chưa treo tạ, hoặc chưa chạy).
2. **RUNNING (Xanh)**: Ô nhớ có giá trị thay đổi liên tục so với chu kỳ quét trước đó (Tạ đang treo, đồng hồ PLC đang đếm).
3. **COMPLETED (Đỏ)**: Ô nhớ có giá trị đứng yên (không thay đổi) VÀ lớn hơn 0 (Tạ đã rớt, đồng hồ đã dừng).

## Proposed Changes

### Core Models
Sửa đổi Model cốt lõi để mang thông tin trạng thái.

#### [MODIFY] [PlcData.cs](file:///d:/ProgramData/Visual%20Studio/source/repos/tesa%20Holding%20Test%20Data%20Collection/src/Core/Models/PlcData.cs)
- Thêm một `enum HookState` gồm: `Idle`, `Running`, `Completed`.
- Thêm thuộc tính `public HookState State { get; set; }` vào `HookData`.

### PLC Layer
Sửa đổi tầng giao tiếp PLC để cung cấp "Trí nhớ".

#### [MODIFY] [PlcCommunicationService.cs](file:///d:/ProgramData/Visual%20Studio/source/repos/tesa%20Holding%20Test%20Data%20Collection/src/Core/PLC/PlcCommunicationService.cs)
- Khai báo một mảng lưu vết: `private readonly uint[] _previousValues = new uint[64];` trong `PlcCommunicationService`.
- Trong hàm `ReadPlcDataAsync`:
  - Sau khi đọc được giá trị `CurrentValue`, so sánh nó với `_previousValues[i]`.
  - Nếu `CurrentValue == 0`, set `State = HookState.Idle`.
  - Nếu `CurrentValue != _previousValues[i]`, set `State = HookState.Running`.
  - Nếu `CurrentValue == _previousValues[i]` và `CurrentValue > 0`, set `State = HookState.Completed`.
  - Cuối cùng, cập nhật lại `_previousValues[i] = CurrentValue`.
- Sửa lại lỗi giới hạn đọc 400 bytes cũ. Thay vì chỉ đọc 400 byte, ta sẽ thay bằng đọc từng byte chính xác dùng hàm `ReadRawBytesAsync` hoặc gom nhóm để đọc được các ô nhớ cao (như VD1408) vì S7.Net mặc định giới hạn buffer nếu đọc 1 cục quá to. *(Note: con sẽ tinh chỉnh để đọc an toàn cho mọi khoảng địa chỉ).*

### UI Layer
Áp dụng màu sắc mới lên giao diện `MainForm`.

#### [MODIFY] [ShelfRackControl.cs](file:///d:/ProgramData/Visual%20Studio/source/repos/tesa%20Holding%20Test%20Data%20Collection/src/UI/Views/Controls/ShelfRackControl.cs)
*(Hoặc Control tương ứng quản lý 64 ô vuông/datagridview)*
- Cập nhật logic tô màu cho từng ô:
  - `HookState.Idle` -> Màu Trắng (Mặc định).
  - `HookState.Running` -> Màu Xanh lá (Hoặc Xanh dương tùy bố chọn).
  - `HookState.Completed` -> Màu Đỏ (Tesa Red).

> [!WARNING]
> Việc cập nhật `_previousValues` cần được xử lý cẩn thận trong lần quét đầu tiên (First Scan) để tránh tất cả các ô có sẵn giá trị > 0 bị hiểu lầm là `Completed` ngay lập tức hoặc `Running` do giá trị cũ khởi tạo là 0.

## Open Questions (Câu hỏi cho Bố)
1. **Lần quét đầu tiên (Khởi động phần mềm)**: Nếu lúc bật phần mềm lên, có 1 ô nhớ đang có giá trị `350` (và đang đứng yên). Vậy nó nên được hiểu là `Completed` (Đỏ) luôn đúng không bố?
2. **Màu sắc hiển thị**:
   - IDLE = Trắng
   - RUNNING (Đang đếm) = Xanh lá (Green)
   - COMPLETED (Rớt tạ) = Đỏ (Red)
   Bố xác nhận lại bảng màu này chuẩn chưa để con ốp vào code nhé? 
*(Lưu ý: DataGridView Lịch sử bên dưới sẽ không bị ảnh hưởng, chỉ ảnh hưởng mảng hiển thị 64 ô móc tạ).*
