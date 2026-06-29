# Kế hoạch triển khai: Chế độ Dò mìn (PLC Address Scanner)

## Mục tiêu
Tạo ra một công cụ "Dò mìn" ngay trong ứng dụng C# để tự động quét toàn bộ các địa chỉ VD (từ danh sách của user). Khi người dùng tác động vật lý lên máy (ví dụ: treo tạ vào Móc 1), công cụ sẽ phát hiện ngay địa chỉ VD nào bị thay đổi giá trị và thông báo trực quan trên giao diện.

## Phase 1: Chuẩn bị UI (ScannerForm)
- Tạo một Windows Form mới: `ScannerForm`.
- **Giao diện bao gồm:**
  - Một `TextBox` lớn (hoặc `DataGridView`) để nhập danh sách tất cả các địa chỉ VD nghi ngờ (dạng `110, 204, 320, 1404...`).
  - Một nút **BẮT ĐẦU QUÉT**.
  - Một vùng hiển thị kết quả (Log Panel hoặc ListBox) hiển thị to, rõ ràng: `VDxxx VỪA THAY ĐỔI -> GIÁ TRỊ MỚI: YYY`.
  - Nút **THÊM VÀO RACK** để tự động gán địa chỉ vừa phát hiện vào một vị trí (Ví dụ L1P1) của Rack hiện tại.

## Phase 2: Logic Quét (Scanner Logic)
- Khi ấn **BẮT ĐẦU QUÉT**, phần mềm sẽ khởi tạo kết nối PLC.
- Chạy một Task ngầm (Background Worker hoặc `Task.Run` với `CancellationToken`), đọc liên tục (ví dụ chu kỳ 500ms) toàn bộ các địa chỉ VD có trong danh sách.
- Ở mỗi chu kỳ, so sánh giá trị đọc được với giá trị ở chu kỳ trước.
- **Nếu có sự thay đổi khác 0 (hoặc lớn hơn sai số)**:
  - Bắn sự kiện (Event) lên UI.
  - UI sẽ In đậm và đổi màu (Tesa Red) dòng địa chỉ đó, kèm tiếng Beep (SystemSounds.Beep).
  - Tự động lưu vết lại thời gian phát hiện.

## Phase 3: Tích hợp (Integration)
- Thêm một nút **"DÒ MÌN"** (Scanner Mode) trên `SettingsForm` hiện tại (bên cạnh nút "NHẬP SỈ").
- Truyền `SettingsRepository` và thông tin `RackId` sang cho `ScannerForm` để khi quét xong, người dùng có thể gán ngay vào vị trí.

## Verify (Kiểm tra)
- Chạy `ScannerForm` mà không có PLC thật (mock data) để xem giao diện có update không.
- Thử nhập liệu 64 địa chỉ, test xem chu kỳ 500ms có làm đứng UI (UI Freeze) hay không. (Phải chắc chắn dùng Async/Await hoặc Invoke).

## Open Questions (Câu hỏi cho Bố)
1. Bố có muốn phần mềm tự động phát âm thanh (tiếng Beep) khi dò ra một địa chỉ mới không, để bố đỡ phải nhìn chằm chằm vào màn hình máy tính?
2. Bố muốn công cụ này quét với chu kỳ bao nhiêu mili-giây? (Mặc định con đề xuất 500ms để không làm quá tải PLC S7-200).
