# Tài liệu Đặc tả Yêu cầu Sản phẩm (PRD)

**Tên dự án:** Smart Audio Guide – Ứng dụng Thuyết minh Địa điểm Tự động
**Người phụ trách (PO/PM):** Võ Minh Sang
**Phiên bản tài liệu:** 1.0 – Draft
**Ngày cập nhật cuối:** 19/03/2026

---

## 1. TỔNG QUAN

### 1.1. Vấn đề cần giải quyết

Khách du lịch khi đến các điểm tham quan, gian hàng hoặc điểm dừng thường thiếu thông tin hướng dẫn súc tích, dễ hiểu và đúng thời điểm. Họ phải tự đọc bảng thông tin khô khan, tra cứu trên mạng hoặc phụ thuộc hoàn toàn vào hướng dẫn viên (thường không phải lúc nào cũng có). Ở chiều ngược lại, chủ gian hàng/đơn vị vận hành khó kiểm soát được thông điệp truyền thông tại điểm bán và không có công cụ chủ động để “kể câu chuyện” về sản phẩm/dịch vụ cho khách một cách nhất quán.

### 1.2. Mục tiêu dự án

- Cung cấp trải nghiệm nghe thuyết minh tự động, theo ngữ cảnh vị trí địa lý (GPS/Geofence hoặc QR), giúp khách hiểu nhanh về địa điểm, sản phẩm và câu chuyện phía sau.
- Tăng mức độ tương tác và khả năng chuyển đổi (mua hàng/sử dụng dịch vụ) tại các gian hàng, điểm tham quan nhờ nội dung thuyết minh hấp dẫn, nhất quán.
- Xây dựng một nền tảng để chủ POI có thể tự tạo, quản lý và cập nhật nội dung thuyết minh (text, TTS, audio thu sẵn) mà không phụ thuộc hoàn toàn vào đội kỹ thuật.

## 2. ĐẶC TẢ TRẢI NGHIỆM KHỞI ĐỘNG

**Mục tiêu:** Đảm bảo thời gian từ lúc khách mở app đến lúc có thể tương tác trên bản đồ là dưới 2 giây. Giao diện phải mượt mà, không chớp giật và hệ thống phải âm thầm chuẩn bị sẵn dữ liệu (âm thanh, bản đồ) cho các bước di chuyển tiếp theo của khách.

### 2.1. Trải nghiệm màn hình Splash & Xử lý ngầm

Tại màn hình Splash, ứng dụng rẽ nhánh theo người dùng:

- **Lần đầu mở ứng dụng:** Hiển thị các bước thiết lập ngôn ngữ và cấp quyền vị trí.
- **Đã từng mở ứng dụng:** Hiển thị quảng cáo, các sự thay đổi hoặc cập nhật mới của ứng dụng.

Đồng thời, hệ thống chạy ngầm các tác vụ bên dưới:
- **Chuẩn bị giao diện:** Tải sẵn danh sách địa danh cục bộ (không tốn thời gian). Nếu mở lần đầu, hiển thị khung tải xám (skeleton).
- **Cập nhật dữ liệu & Vị trí (chỉ chạy khi đã có quyền định vị):** Thực hiện ngay việc xác định tọa độ hiện tại, kết nối API để cập nhật thông tin gian hàng mới nhất và gửi yêu cầu máy chủ dịch, tải trước âm thanh của **10 quán ăn gần nhất (bán kính 1.5 km)**. Khi khách đi bộ tới nơi, âm thanh đã nằm sẵn để phát.

### 2.2. Cơ chế Đồng bộ & Chống giật giao diện

Khi mở lại, giao diện ban đầu dùng dữ liệu ngoại tuyến cũ. Khi dữ liệu trực tuyến mới tải xong:
- **Gộp dữ liệu:** Lưu ngầm (upsert) vào IndexedDB.
- **Làm mới giao diện:** KHÔNG tự động ghi đè để tránh giật hình. Hiện thông báo ngắn (toast/snackbar): _"Đã có bản cập nhật mới. Chạm để làm mới"_, chỉ vẽ lại khi khách bấm vào.

### 2.3. Xử lý ngoại lệ và rủi ro

- **Từ chối quyền GPS:** Tắt phát âm thanh tự động (geofence), chuyển sang hướng dẫn chọn thủ công trên bản đồ hoặc quét mã QR.
- **Mất sóng / sóng yếu:** Hủy API sau 5 giây, chuyển hẳn sang chế độ ưu tiên ngoại tuyến (IndexedDB/Workbox Cache) để không gián đoạn.
- **Lỗi định vị trong nhà (sai số >50m):** Tắt âm thanh tự động, hiện thông báo: _"Tín hiệu định vị yếu. Vui lòng quét mã QR để nghe"_.
- **Đầy bộ nhớ thiết bị:** Áp dụng thuật toán LRU tự động xóa bộ nhớ đệm âm thanh của các quán đã đi qua >30 phút trước, chỉ giữ ngôn ngữ đang dùng.
