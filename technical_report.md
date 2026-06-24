# BÁO CÁO KỸ THUẬT: HỆ THỐNG THU THẬP DỮ LIỆU TESA HOLDING (TAPE ADHESION TEST)

## 1. Khảo sát và Phân tích nghiệp vụ

### 1.1. Bối cảnh và Bài toán
Nhà máy cần thực hiện các bài test độ bám dính của băng keo (Tape Adhesion Test). Các mẫu băng keo được treo với một quả tạ. Khi băng keo mất độ bám dính, tạ sẽ rơi xuống (Drop). Quá trình này được giám sát bởi hệ thống PLC (Siemens S7-200 Smart).
Trước đây, quá trình ghi nhận thời gian rơi và đối soát mã mẫu (NART/Batch) gặp nhiều khó khăn, yêu cầu tính tự động hóa cao để giảm thiểu thao tác thủ công của Lab Operator.

### 1.2. Quy mô hệ thống
- **Số lượng giàn (Rack):** 4 Racks.
- **Cấu trúc mỗi Rack:** 4 tầng (Floor), mỗi tầng 16 vị trí đo (Hook).
- **Tổng số điểm đo đồng thời:** 256 vị trí.
- **Phần cứng:** 4 bộ PLC S7-200 Smart kết nối qua mạng TCP/IP.

### 1.3. Yêu cầu nghiệp vụ cốt lõi
1. **Auto-Start & Auto-Record:** Hệ thống tự động bắt đầu tính thời gian khi treo tạ và tự động lưu lịch sử khi tạ rơi mà không cần bấm nút "Start/Stop" trên phần mềm.
2. **Quản lý dữ liệu mẫu:** Ghi nhận đầy đủ `NART Code`, `Batch Code`, `Vị trí mẫu`, `Tester`, `Thời gian Drop (Phút)`.
3. **Hiệu năng:** Giao diện không được đơ/lag khi giám sát đồng thời 256 điểm đo theo thời gian thực (Polling rate 100ms).
4. **Báo cáo:** Xuất file Excel tự động, phân chia các Rack thành các Sheet (Tab) riêng biệt trong cùng một file.

---

## 2. Thiết kế Hệ thống (System Architecture)

### 2.1. Kiến trúc Tổng thể (High-level Architecture)
Hệ thống được xây dựng trên nền tảng **.NET (C# WinForms)**, áp dụng mô hình phân lớp (N-Tier) để tách biệt UI, Logic và Data.
*   **Presentation Layer (UI):** Giao diện dạng DataGrid View đa luồng.
*   **Business Logic Layer (Core):** `MainPresenter` và `StateMachine` quản lý trạng thái máy học (FSM).
*   **Hardware Interface (PLC):** Giao tiếp qua giao thức S7 (S7.Net) quản lý nhiều kết nối TCP song song.
*   **Data Access Layer:** Local SQLite Database sử dụng Micro-ORM Dapper.

### 2.2. Thiết kế Giao diện (UI/UX Design)
Thay vì sử dụng 256 thành phần Control (như TextBox/Button) riêng lẻ gây tràn bộ nhớ và treo luồng đồ họa (UI Thread), hệ thống áp dụng thiết kế **Tabbed DataGrid**:
- **TabControl:** Chia màn hình thành 5 Tab (4 Tab cho 4 Rack và 1 Tab Quản lý kết nối).
- **DataGridView (Virtual Mode):** Render 64 hàng mỗi Rack cực kỳ nhẹ nhàng. Sử dụng màu sắc (Color Coding) nguyên dòng để thể hiện trạng thái:
  - `Trắng (IDLE)`: Đang chờ.
  - `Vàng (RUNNING)`: Tạ đang treo, PLC đang đếm.
  - `Xanh lá (COMPLETED)`: Tạ đã rơi, đã lưu dữ liệu.

### 2.3. Thiết kế Cơ sở dữ liệu (Database Schema)
Sử dụng **SQLite** lưu trữ cục bộ, cấu trúc bảng `TestRecords` bao gồm:
- `RackId`, `Floor`, `HookIndex`, `Location` (Định danh vị trí phần cứng)
- `NartCode`, `BatchCode`, `SamplePosition`, `Tester` (Dữ liệu do Operator nhập)
- `DropTime`, `PlcValue`, `CompletedAt` (Dữ liệu máy tự thu thập)

---

## 3. Giải pháp Kỹ thuật và Thuật toán (Technical Solutions)

### 3.1. Thuật toán Auto-Start và Debounce (Chống nhiễu)
Để giải quyết bài toán tự động nhận diện "Treo tạ" và "Rớt tạ" mà không cần nút bấm vật lý, hệ thống triển khai một **Finite State Machine (FSM)** cho mỗi điểm đo.

*   **Logic Auto-Start:** 
    Khi Operator điền `NART` và `Batch` vào giao diện. Vòng lặp Polling đọc giá trị PLC liên tục. Nếu `Giá trị PLC > 0` $\Rightarrow$ Kích hoạt trạng thái **RUNNING**.
*   **Logic Rớt tạ (Drop Detection & Debounce):**
    Giá trị PLC liên tục tăng. Nếu giá trị ở chu kỳ hiện tại **bằng** giá trị ở chu kỳ trước (`currentValue == lastValue`), bộ đếm `_unchangedCycles` sẽ tăng lên. 
    Chỉ khi giá trị không đổi trong **3 chu kỳ liên tiếp** (khoảng 300ms), hệ thống mới xác nhận tạ rơi hoàn toàn (chống nhiễu mạng/lag) $\Rightarrow$ Kích hoạt trạng thái **COMPLETED** $\Rightarrow$ Bắn Event lưu Database.

### 3.2. Xử lý Đa luồng (Multi-threading & Concurrency)
- **Tách biệt UI và PLC:** Quá trình giao tiếp với 4 PLC được đưa vào 4 `Task` chạy ngầm hoàn toàn độc lập (Background Worker).
- **Polling Loop:** Vòng lặp `while (!cancellationToken.IsCancellationRequested)` với tần số 100ms quét toàn bộ 64 vùng nhớ (VD) của mỗi PLC.
- **Cross-thread UI Update:** Khi Background Task phát hiện thay đổi trạng thái, việc cập nhật UI được đưa về luồng chính thông qua `InvokeOnUI` (Wrapper của Control.Invoke) để tránh Exception `Cross-thread operation not valid`.

### 3.3. Giải pháp Đọc Dữ liệu Khối (Block Reading)
Đọc từng vùng nhớ đơn lẻ 256 lần sẽ làm sập mạng (Network Congestion). Giải pháp là dùng `ReadClass` hoặc gộp byte (ReadBytes) để đọc **1 Block Data duy nhất** chứa toàn bộ 64 giá trị DWord (256 bytes) từ mỗi PLC trong 1 lần request, giúp thời gian phản hồi chỉ tính bằng mili-giây.

### 3.4. Xuất Báo Cáo Thông Minh (Dynamic Excel Generation)
Sử dụng thư viện `ClosedXML` để thao tác với bộ nhớ đệm (Memory Stream) thay vì gọi COM+ Excel (Tránh lỗi không cài Office).
- Thuật toán `GroupBy(RackId)` tự động phân mảnh dữ liệu.
- Vòng lặp tạo Sheet tương ứng cho từng Rack.
- Quy đổi dữ liệu thời gian thô (milli-giây) sang Phút (`Math.Round(ms / 60000.0, 2)`) ngay tại thời điểm xuất báo cáo để Operator dễ dàng đọc hiểu.

---

## 4. Đánh giá và Mở rộng
- **Mức độ ổn định:** Hệ thống có khả năng chịu lỗi (Fault-tolerance). Nếu mất kết nối PLC, UI không bị treo, khi có mạng lại, Auto-Reconnect sẽ kết nối lại.
- **Bảo trì:** Mã nguồn được tổ chức theo Domain-Driven Design (Core, Data, UI), dễ dàng thêm Rack thứ 5 hoặc chuyển đổi DB từ SQLite sang SQL Server nếu nhà máy mở rộng quy mô.
