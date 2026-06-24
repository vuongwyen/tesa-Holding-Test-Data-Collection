# Kế hoạch Nâng cấp Hệ thống Đa Kết nối (Multi-Rack Scale)

Nâng cấp hệ thống từ việc quản lý 1 PLC (16 móc) lên quản lý 4-6 PLC (256 - 384 móc), sử dụng kiến trúc Tab phân cấp (Option A) và thiết kế Tab Quản lý kết nối PLC động.

## Task Breakdown

### 1. UI Layer
- [ ] Bổ sung Tab "Quản lý PLC".
- [ ] Thiết kế `PlcConnectionCard` UserControl hiển thị ô nhập IP, trạng thái, nút Connect.
- [ ] Xóa lưới 16 móc cố định trong `MainForm`, thay bằng logic khởi tạo động: `TabControl` (Racks) -> `TabControl` (Tầng) -> `TableLayoutPanel` (Móc).
- [ ] Chuyển `DrawMode` của TabControl sang `OwnerDrawFixed` để vẽ màu nền cho tiêu đề Tab (cảnh báo).

### 2. Core & Data Layer
- [ ] Tách `PlcService` khỏi Singleton (nếu có), hỗ trợ đa kết nối.
- [ ] Tạo `PlcManager` chứa danh sách các PLC theo từng Rack (`Dictionary<string, IPlcService>`).
- [ ] Cập nhật `SettingsRepository` để lưu trữ nhiều IP (PlcIp_Rack1, PlcIp_Rack2...).

### 3. Presenter Layer
- [ ] Cập nhật logic Polling: Thay vì 1 vòng lặp `while`, tạo nhiều vòng lặp `Task` chạy ngầm cho từng Rack.
- [ ] Đổi chuẩn Hook ID thành `R{rack}-T{floor}-M{hook}`.
- [ ] Gọi hàm cảnh báo UI khi `OnTestCompleted` phát ra.

## Agent Assignments
- **Orchestrator**: Theo dõi toàn bộ quá trình, đảm bảo không bỏ sót việc liên kết các layer.
- **Frontend Specialist**: Tạo `PlcConnectionCard` và làm phần Custom Draw cho các tiêu đề Tab nhấp nháy.
- **Backend Specialist**: Đập đi xây lại `PlcManager` và điều phối Multi-threading Polling Loop.

## Verification Checklist (Phase 4)
- [ ] Xác nhận giao diện khởi tạo được 4 Racks * 4 Tầng * 16 Móc mượt mà.
- [ ] Thử nhập 2 IP PLC khác nhau và kiểm tra xem có chạy song song được không.
- [ ] Móc ở Rack 2 hoàn thành, Tab Rack 2 và Tab Tầng của nó phải đổi màu xanh lá/đỏ nhấp nháy.
