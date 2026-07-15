# Kế Hoạch Triển Khai Môi Trường Test (Test Environment Deployment)

**Trạng thái:** BUNG XÕA (Caveman + Ponytail Mode)
**Mục tiêu:** Dựng ngay một môi trường Test hoàn chỉnh (Server + Đa ứng dụng Client) để "khè" sếp, chạy dữ liệu real-time giả lập để chứng minh tính khả thi của mô hình JSONB Multi-App.

---

## 1. Yêu Cầu Tổng Quan (Context Check)
Sếp muốn dựng một môi trường Test ngay lập tức để mô phỏng một nhà máy với hàng loạt thiết bị (Scale, QC, Barcode) ném dữ liệu lên Server mới. Không rườm rà, tập trung vào hiệu ứng thị giác (Dashboard) và luồng chạy thực tế.

---

## 2. Các Bước Hành Động (Task Breakdown)

### Phase 1: Thổi Lửa Web Server (Backend & Dashboard) 🚀
*Đập bỏ mọi rào cản, dựng Server lên mây bằng Docker.*
- [ ] Dọn dẹp VM Ubuntu cũ (`docker compose down -v`).
- [ ] Dựng Server với cấu hình `test-env`:
  - PostgreSQL 16 (Dùng chung 1 DB `tesa_test`).
  - .NET 8 API (Chạy cổng 5000).
- [ ] Gắn ngầm sẵn các biến môi trường đa ứng dụng:
  - `API_KEY_SCALE=SCALE_TEST_123`
  - `API_KEY_QC=QC_TEST_123`
  - `API_KEY_TAPE=TAPE_TEST_123`

### Phase 2: Chế tạo "Máy bơm dữ liệu" (Data Generator) 🦍
*Không thể chờ User nhập bằng tay, phải có Tool tự động bơm dữ liệu để Dashboard nhảy số liên tục.*
- [ ] Dùng Python hoặc C# Console App nhỏ gọn (Data Pumper).
- [ ] Mô phỏng 3 App chạy song song, mỗi 5 giây bắn 1 request:
  1. **App Cân (Scale):** Bắn Weight, Unit.
  2. **App QC:** Bắn mã lỗi, tên thanh tra.
  3. **App Tape (Băng keo):** Bắn lực kéo đứt (Adhesion force).

### Phase 3: Nâng cấp "Bộ Mặt" Dashboard (UI/UX) 🐴
*Áp dụng bản thiết kế `multi_app_architecture.md` vào code thật.*
- [ ] Bật logic **Dynamic Column**: Nếu `AppType = Scale`, hiện cột Cân. Nếu `AppType = QC`, hiện cột Lỗi.
- [ ] Thêm hiệu ứng `FadeInUp` và `Pulse` màu xanh lá (Online) / đỏ (Offline) cho danh sách thiết bị.

### Phase 4: Triển khai Client Thật (Edge PC) 💻
*Kết nối App WinForms hiện tại vào môi trường Test.*
- [ ] Trỏ `appsettings.json` của App Client `ScaleApp` về IP của VM.
- [ ] Trỏ App `tesa Holding Test Data Collection` về IP của VM (Dùng `API_KEY_TAPE`).
- [ ] Cầm máy quét Barcode quét thử, vứt lên cân, bấm nút Sync -> Dữ liệu bay lên Server trong 0.1s.

---

## 3. Phân Công (Agent Assignments)
- **Ponytail:** Đảm nhận Phase 1 & Phase 3 (Dựng Docker, tinh chỉnh Backend/Frontend Dashboard cho chuẩn Enterprise, không lỗi vặt).
- **Caveman:** Đảm nhận Phase 2 & Phase 4 (Code nhanh script tự động bắn dữ liệu, đập bàn phím trỏ IP cho các App WinForms).

---

## 4. Cổng Kiểm Duyệt (Socratic Gate - Review & Approve)

> [!WARNING]
> Kế hoạch này là **"Bung Xõa"** (Agile/Hacker mode). Nó sinh ra để ra sản phẩm chạy được trong 1-2 giờ tới nhằm mục đích trình bày với Sếp Lớn.

**Sếp có cần điều chỉnh gì trước khi chúng ta gõ lệnh không?**
1. Có cần cài đặt Server lên Cloudflare Tunnel để sếp lớn xem qua điện thoại không?
2. Script giả lập dữ liệu (Phase 2) sếp muốn làm bằng Python hay tôi code thẳng bằng 1 file C# nhỏ?
3. Sếp đồng ý với kế hoạch này thì cứ gõ `/create` để tôi khởi động lò phản ứng!
