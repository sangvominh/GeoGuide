# Tài liệu Đặc tả Yêu cầu Sản phẩm (PRD)

**Tên dự án:** Smart Audio Guide – Ứng dụng Thuyết minh Địa điểm Tự động  
**Người phụ trách (PO/PM):** Võ Minh Sang
**Phiên bản tài liệu:** 1.0 – Draft  
**Ngày cập nhật cuối:** 19/03/2026

---

## 1. TỔNG QUAN (OVERVIEW)

### 1.1. Vấn đề cần giải quyết (Problem Statement)

Khách du lịch khi đến các điểm tham quan, gian hàng hoặc điểm dừng thường thiếu thông tin hướng dẫn súc tích, dễ hiểu và đúng thời điểm. Họ phải tự đọc bảng thông tin khô khan, tra cứu trên mạng hoặc phụ thuộc hoàn toàn vào hướng dẫn viên (thường không phải lúc nào cũng có). Ở chiều ngược lại, chủ gian hàng/đơn vị vận hành khó kiểm soát được thông điệp truyền thông tại điểm bán và không có công cụ chủ động để “kể câu chuyện” về sản phẩm/dịch vụ cho khách một cách nhất quán.

### 1.2. Mục tiêu dự án (Objectives)

- Cung cấp trải nghiệm nghe thuyết minh tự động, theo ngữ cảnh vị trí địa lý (GPS/Geofence hoặc QR), giúp khách hiểu nhanh về địa điểm, sản phẩm và câu chuyện phía sau.
- Tăng mức độ tương tác và khả năng chuyển đổi (mua hàng/sử dụng dịch vụ) tại các gian hàng, điểm tham quan nhờ nội dung thuyết minh hấp dẫn, nhất quán.
- Xây dựng một nền tảng để chủ POI có thể tự tạo, quản lý và cập nhật nội dung thuyết minh (text, TTS, audio thu sẵn) mà không phụ thuộc hoàn toàn vào đội kỹ thuật.

## 2. ĐẶC TẢ TRẢI NGHIỆM KHỞI ĐỘNG (FRONTEND STARTUP FLOW)

**Mục tiêu:** Đảm bảo thời gian từ lúc khách mở app đến lúc có thể tương tác trên bản đồ là dưới 2 giây. Giao diện phải mượt mà, không chớp giật và hệ thống phải âm thầm chuẩn bị sẵn dữ liệu (âm thanh, bản đồ) cho các bước di chuyển tiếp theo của khách.

### 2.1. Luồng xử lý "Không thời gian chờ" (Zero-Wait Startup & Parallel Processing)

Để tối ưu tốc độ, hệ thống không đợi tải xong toàn bộ dữ liệu mới hiển thị, mà chia làm các nhánh xử lý song song ngay tại màn hình Splash:

1. **Khởi tạo & Xin quyền (0.5s đầu):** - Hiển thị màn hình chọn ngôn ngữ.
   - Kích hoạt yêu cầu cấp quyền Vị trí (Location Permission) từ Hệ điều hành. Tiến trình đếm ngược Timeout của GPS (5s) chỉ chính thức bắt đầu **sau khi** người dùng phản hồi Popup xin quyền.

2. **Rẽ nhánh hiển thị (Cold Start vs. Warm Start):**
   - **Kịch bản Warm Start (Khách đã từng mở app):** Truy xuất ngay dữ liệu danh sách quán ăn từ bộ nhớ máy (IndexedDB) để dựng giao diện bản đồ và danh sách (Độ trễ 0ms).
   - **Kịch bản Cold Start (Khách mới tải app lần đầu):** IndexedDB trống. Hệ thống bắt buộc hiển thị trạng thái Skeleton Loading (khung xám tải trang) trong lúc chờ API trả về dữ liệu.

3. **Tiến trình chạy ngầm song song (Background Tasks):**
   - **Luồng Định vị:** Gọi `waitForPosition()` để chốt tọa độ hiện tại.
   - **Luồng Online:** Kết nối API để cập nhật thông tin mới nhất của các gian hàng.
   - **Luồng Hotset (Đón đầu):** Đây là lõi tối ưu trải nghiệm. Ngay khi có tọa độ GPS, hệ thống tự động gửi API yêu cầu Server dịch và tải trước Audio của **10 quán ăn gần nhất (bán kính 1.5km)**. Khi khách đi bộ tới nơi, âm thanh đã nằm sẵn trong máy chờ phát.

### 2.2. Cơ chế Đồng bộ & Chống giật giao diện (Anti-Jitter Sync)

Trong kịch bản Warm Start, giao diện đã được vẽ bằng dữ liệu cũ (Offline), nhưng sau 2-3 giây, dữ liệu mới (Online) mới tải về xong. Để tránh việc màn hình bị chớp giật (UI Jitter) hoặc văng thao tác của khách:

- **Nguyên tắc Merge:** Dữ liệu mới tải về sẽ được lưu ngầm (Upsert) vào IndexedDB.
- **Cập nhật UI:** Giao diện KHÔNG tự động ghi đè (overwrite) ngay lập tức. Hệ thống sẽ hiển thị một Toast/Snackbar nhỏ ở góc màn hình: _"Đã có bản cập nhật thông tin mới. Chạm để làm mới"_. Chỉ khi khách bấm vào, giao diện mới re-render.

### 2.3. Xử lý ngoại lệ và Rủi ro (Edge Cases & Error Handling)

Hệ thống tự động xử lý mượt mà các rủi ro kỹ thuật đặc thù của môi trường ngoài trời/hội chợ:

- **Khách từ chối quyền GPS (Permission Denied):** App vô hiệu hóa tính năng Geofence (phát âm thanh tự động theo vị trí). Chuyển sang hướng dẫn khách tương tác thủ công bằng cách **chọn trực tiếp trên bản đồ** hoặc **quét mã QR** dán tại quán ăn.
- **Trường hợp mất sóng / Sóng yếu:** App tự động hủy bỏ luồng gọi API (Luồng Online) sau 5 giây Timeout và chuyển hẳn sang chế độ "Offline-First", sử dụng 100% dữ liệu đang có trong IndexedDB và Workbox Cache để không làm gián đoạn luồng đi.
- **Lỗi định vị trong nhà (Indoor GPS Jitter/Drift):** Nếu tín hiệu GPS quá yếu hoặc sai số quá lớn (>50m), app sẽ vô hiệu hóa kích hoạt âm thanh tự động và bật Popup: _"Tín hiệu định vị yếu. Vui lòng quét mã QR tại gian hàng để nghe thuyết minh"_.
- **Đầy bộ nhớ điện thoại (QuotaExceededError):** Việc tải gói Âm thanh/Bản đồ Offline (Hotset/Warmup) có thể làm đầy dung lượng thiết bị. Hệ thống áp dụng thuật toán LRU (Least Recently Used) để **tự động xóa cache** của các ngôn ngữ và file âm thanh của những quán ăn khách đã đi qua từ 30 phút trước, chỉ ưu tiên giữ (Pin) ngôn ngữ đang sử dụng.
