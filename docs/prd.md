# Tài liệu Đặc tả Yêu cầu Sản phẩm (PRD)

**Tên dự án:** Smart Audio Guide – Ứng dụng Thuyết minh Địa điểm Tự động  
**Người phụ trách (PO/PM):** Võ Minh Sang
**Phiên bản tài liệu:** 1.0 – Draft  
**Ngày cập nhật cuối:** 19/03/2026

---

## 1. TỔNG QUAN
## 1. TỔNG QUAN

### 1.1. Vấn đề cần giải quyết
### 1.1. Vấn đề cần giải quyết

Khách du lịch khi đến các điểm tham quan, gian hàng hoặc điểm dừng thường thiếu thông tin hướng dẫn súc tích, dễ hiểu và đúng thời điểm. Họ phải tự đọc bảng thông tin khô khan, tra cứu trên mạng hoặc phụ thuộc hoàn toàn vào hướng dẫn viên (thường không phải lúc nào cũng có). Ở chiều ngược lại, chủ gian hàng/đơn vị vận hành khó kiểm soát được thông điệp truyền thông tại điểm bán và không có công cụ chủ động để “kể câu chuyện” về sản phẩm/dịch vụ cho khách một cách nhất quán.
   
### 1.2. Mục tiêu dự án
   
### 1.2. Mục tiêu dự án

- Cung cấp trải nghiệm nghe thuyết minh tự động, theo ngữ cảnh vị trí địa lý (GPS/Geofence hoặc QR), giúp khách hiểu nhanh về địa điểm, sản phẩm và câu chuyện phía sau.
- Tăng mức độ tương tác và khả năng chuyển đổi (mua hàng/sử dụng dịch vụ) tại các gian hàng, điểm tham quan nhờ nội dung thuyết minh hấp dẫn, nhất quán.
- Xây dựng một nền tảng để chủ POI có thể tự tạo, quản lý và cập nhật nội dung thuyết minh (text, TTS, audio thu sẵn) mà không phụ thuộc hoàn toàn vào đội kỹ thuật.

## 2. ĐẶC TẢ TRẢI NGHIỆM KHỞI ĐỘNG
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

## 3. TRẢI NGHIỆM TIÊU THỤ NỘI DUNG (CORE POI & AUDIO EXPERIENCE)

**Mục tiêu:** Mang lại trải nghiệm "Zero-click" (không cần chạm) để khách tham quan có thể vừa đi dạo, vừa nghe kể chuyện mà không cần cắm mặt vào màn hình điện thoại. Đồng thời, hệ thống phải xử lý mượt mà sự cố rớt mạng và chống phát âm thanh rác (spam).

### 3.1. Luồng tiếp cận gian hàng
Khách tham quan có thể kích hoạt nội dung thuyết minh của một gian hàng (POI) thông qua 3 phương thức:
- **Tiếp cận thụ động (Geofence Auto-play):** Khách chỉ cần đút điện thoại trong túi, đi bộ vào bán kính 30m của gian hàng, âm thanh sẽ tự động phát.
- **Tiếp cận chủ động (Map/List Tap):** Khách bấm trực tiếp vào biểu tượng gian hàng trên bản đồ màn hình chính để xem trước nội dung.
- **Tiếp cận dự phòng (QR Code):** Dành cho khu vực trong nhà (sóng GPS yếu), khách quét mã QR tại quầy để mở thẳng giao diện bài thuyết minh.

### 3.2. Đặc tả Logic Bộ máy Định vị
Để tránh việc âm thanh phát loạn xạ khi khách đi ngang qua nhiều gian hàng, thuật toán Geofence bắt buộc tuân thủ 3 quy tắc:
- **Chống nhiễu ranh giới (Debounce 3 giây):** Khách đi vào vùng 30m sẽ được đưa vào trạng thái chờ. Khách phải **đứng lại hoặc di chuyển chậm trong vùng đó liên tục 3 giây**, hệ thống mới xác nhận và bắt đầu phát âm thanh. Tránh tình trạng khách đi xe lướt ngang qua mà app vẫn kêu.
- **Chống làm phiền (Cooldown 5 phút):** Khi khách đi ra khỏi gian hàng, gian hàng đó sẽ bị khóa (Cooldown) trong 5 phút. Nếu khách đi vòng lại ngay lập tức, ứng dụng sẽ KHÔNG tự động phát lại bài cũ.
- **Xử lý xung đột vị trí (Priority Logic):** Nếu khách đứng ở điểm giao thoa giữa 2 gian hàng, hệ thống sẽ tự động chọn gian hàng để phát dựa theo: (1) Mức độ ưu tiên của gian hàng (Audio Priority) => (2) Khoảng cách đến tâm gian hàng nào gần hơn.

### 3.3. Chiến lược Âm thanh 4 Cấp độ (4-Tier Hybrid Audio)
Đây là cốt lõi công nghệ để đảm bảo app luôn có tiếng dù mạng internet tại hội chợ tệ đến đâu. Khi có lệnh phát âm thanh
- **Tier 1 (Hoàn hảo - Trễ 0ms):** Lấy file âm thanh đã được tải sẵn trong bộ nhớ máy (do luồng Hotset ở phần 2.1 đã chuẩn bị). Phát ngay lập tức.
- **Tier 1.5 (Chờ dịch - Trễ 2-5s):** Khách dùng ngôn ngữ lạ (VD: tiếng Pháp) chưa có sẵn file. App gửi yêu cầu lên Backend dịch và dùng AI (Edge-TTS) tạo file MP3 tức thì. Trả file về, phát và lưu luôn vào máy cho người sau.
- **Tier 2 (Cloud Stream - Trễ 3-8s):** Nếu điện thoại khách hết dung lượng bộ nhớ, không cho lưu file, app sẽ phát âm thanh dạng Stream trực tiếp từ máy chủ xuống (như nghe nhạc Spotify).
- **Tier 3 (Mất mạng hoàn toàn - Trễ 0ms):** Rớt mạng 4G. App tự động gọi hàm `window.speechSynthesis` (bộ đọc văn bản AI mặc định có sẵn trong hệ điều hành iOS/Android) để đọc text thay thế.

### 3.4. Giao diện Tiêu thụ nội dung (POI Detail UI/UX)
Khi khách đang ở trong gian hàng, giao diện hiển thị:
- **Mini-player (Trình phát nhạc thu nhỏ):** Nằm đè lên bản đồ, chứa nút Pause/Play, thanh tiến trình (progress bar), và nút "Đóng".
- **Fall-back Ngôn ngữ hiển thị:** Trong trường hợp gian hàng chưa cập nhật ngôn ngữ của khách, hệ thống áp dụng luật sa thải: Cố gắng hiện **Tiếng Anh** (Ngôn ngữ quốc tế) => Nếu không có tiếng Anh, hiện **Tiếng Việt** nhưng không có thuyết minh Tiếng Việt (KHÔNG phát âm thanh tiếng Việt cho khách ngoại quốc để tránh gây khó chịu, chỉ hiện chữ và hình ảnh).

### 3.5. Xử lý rủi ro và các trường hợp biên (Edge Cases)
- **Xung đột luồng âm thanh:** Khách đang nghe dở gian hàng A lại đi nhanh sang vùng gian hàng B. Hệ thống **không phát đè**. Âm thanh gian hàng A giảm nhỏ lại (Audio Ducking), ứng dụng rung nhẹ và hiện thông báo: _"Bạn đã đến gian hàng B, chạm để nghe nội dung"_.
- **Đang nghe thì có điện thoại:** Ứng dụng tự động Tạm dừng (Pause). Khi tắt cuộc gọi, hiện Popup hỏi khách có muốn nghe tiếp từ đoạn bị ngắt hay không.
- **Chạy nền (Background Playback):** Khi khách tắt màn hình điện thoại đút vào túi quần, âm thanh vẫn phải tiếp tục phát và GPS vẫn phải tiếp tục quét (sử dụng Foreground Service / Wake Lock).