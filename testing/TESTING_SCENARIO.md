# 🗺️ KẾ HOẠCH KIỂM THỬ TOÀN DIỆN (TEST PLAN) - LUỒNG BUYER

**Dự án:** E-commerce System (ReactJS + ASP.NET Core)
**Đối tượng:** Buyer (Khách mua hàng)
**Công cụ:** Pytest, Playwright, Requests (Python), k6/JMeter

---

## 🏗️ GIAI ĐOẠN 1: CẤU TRÚC PAGE OBJECT MODEL (POM)
*Gom nhóm các thao tác UI để dễ dàng bảo trì khi giao diện thay đổi.*

1. **`home_page.py`**: Xử lý thanh tìm kiếm, chọn danh mục, click banner.
2. **`product_detail_page.py`**: Xử lý chọn Size/Màu, kiểm tra giá, nút `[Thêm vào giỏ hàng]`.
3. **`cart_page.py`**: Xử lý tăng/giảm số lượng, xóa item, xác minh Tổng tiền (Total Price), nút `[Thanh toán]`.
4. **`checkout_page.py`**: Xử lý form điền địa chỉ, chọn phương thức thanh toán, nút `[Đặt hàng]`.
5. **`profile_page.py`**: Xử lý cập nhật thông tin cá nhân (Tên, SDT, Ngày sinh), quản lý Sổ địa chỉ.

---

## 🛒 GIAI ĐOẠN 2: KỊCH BẢN KIỂM THỬ CHỨC NĂNG (E2E FLOWS)

### 🥇 Nhóm P0: Luồng Tiền Về (The Golden Paths - Must Pass)
*Đảm bảo khách hàng có thể mua và thanh toán thành công trong mọi trường hợp.*

| ID | Tên Test Case | Mô tả Hành động (Steps) | Kết quả kỳ vọng (Expected) |
|:---|:---|:---|:---|
| **TC_BUYER_01** | Đặt hàng thành công (User) | Login -> Tìm kiếm SP -> Thêm Giỏ hàng -> Checkout -> Xác nhận | Hiện trang "Đặt hàng thành công", có mã Order ID, giỏ hàng reset về 0. |
| **TC_BUYER_02** | Khách vãng lai & Gộp giỏ | Guest thêm SP vào giỏ -> Bấm Thanh toán -> Bị chặn bắt Login -> Điền form Login | Login xong, SP cũ vẫn còn trong giỏ (Tính năng Merge Cart hoạt động). |

### 🥈 Nhóm P1: Trải nghiệm cốt lõi (Core UX)
*Đảm bảo các thao tác quản lý dữ liệu cá nhân hoạt động mượt mà.*

| ID | Tên Test Case | Mô tả Hành động (Steps) | Kết quả kỳ vọng (Expected) |
|:---|:---|:---|:---|
| **TC_BUYER_03** | Tính toán lại Giỏ hàng | Thêm SP A & B -> Tăng số lượng A -> Xóa B | Tổng tiền cập nhật Real-time chính xác, không cần tải lại trang. |
| **TC_BUYER_04** | Thêm địa chỉ giao hàng | Login -> Vào Profile -> Thêm địa chỉ mới -> Save | Địa chỉ lưu thành công, hiển thị đúng ở màn hình Checkout. |

### 🥉 Nhóm P2: Luồng ngoại lệ & Bắt lỗi (Edge Cases)
*Đảm bảo hệ thống xử lý tốt khi người dùng thao tác sai hoặc thiếu dữ liệu.*

| ID | Tên Test Case | Mô tả Hành động (Steps) | Kết quả kỳ vọng (Expected) |
|:---|:---|:---|:---|
| **TC_BUYER_05** | Checkout giỏ hàng trống | Xóa sạch giỏ -> Gõ URL `/checkout` | Bị đá về trang chủ hoặc báo lỗi "Giỏ hàng trống", không cho đặt. |
| **TC_BUYER_06** | Sản phẩm hết hàng | Tìm một SKU có Tồn kho = 0 -> Bấm Thêm vào giỏ | Nút bị mờ (Disabled), hiển thị chữ "Tạm hết hàng". |

---

## 🛡️ GIAI ĐOẠN 3: KIỂM THỬ BẢO MẬT (SECURITY TESTING)
*Chống lại các lỗ hổng phổ biến của hệ thống RESTful & JWT.*

| ID | Tên Test Case | Mô tả Hành động (Payload) | Kết quả kỳ vọng (Expected) |
|:---|:---|:---|:---|
| **TC_SEC_01** | Lỗ hổng IDOR (Phân quyền) | Buyer A lấy Token của mình gọi API xem/sửa Giỏ hàng hoặc Đơn hàng của Buyer B. | API ASP.NET chặn lại, trả về mã `403 Forbidden` hoặc `404 Not Found`. |
| **TC_SEC_02** | Stored XSS | Khi đặt hàng, điền Ghi chú: `<script>alert(1)</script>` | Backend lưu an toàn, khi hiển thị lại trên UI chỉ hiện Text thuần. |
| **TC_SEC_03** | SQL Injection | Gõ vào thanh tìm kiếm: `Shirt' OR '1'='1` | Không dump lỗi DB. Trả về mảng rỗng hoặc kết quả an toàn. |
| **TC_SEC_04** | Rate Limiting | Cố tình gọi API Login sai mật khẩu 50 lần/phút. | Hệ thống trả về `429 Too Many Requests`, tạm khóa IP. |

---

## 🚀 GIAI ĐOẠN 4: KIỂM THỬ HIỆU NĂNG (PERFORMANCE TESTING)
*Đánh giá sức chịu tải của Backend và sự hiệu quả của Redis Cache.*

| ID | Tên Test Case | Tải trọng (Load) | Kết quả kỳ vọng (SLA) |
|:---|:---|:---|:---|
| **TC_PERF_01** | Tốc độ API Danh sách SP | Gọi `GET /api/products` liên tục. So sánh lần đầu (DB) và các lần sau (Redis). | Lần đầu < 500ms. Có Cache < 100ms. |
| **TC_PERF_02** | Tải đồng thời Giỏ hàng | 100 User cùng gửi `POST /api/cart/items` trong vòng 10 giây. | 100% request thành công, Response < 200ms. |
| **TC_PERF_03** | Race Condition (Giành hàng) | 10 User cùng Checkout 1 SKU đang chỉ còn 1 tồn kho. | 1 người thành công, 9 người nhận thông báo "Hết hàng", kho không bị âm. |

---

## 🛠️ CHIẾN LƯỢC THỰC THI (EXECUTION STRATEGY)

1. **Chuẩn bị dữ liệu (Hybrid Approach):** - Không tạo dữ liệu rác bằng UI. Sử dụng thư viện `requests` (API Tests) để tự động tạo Sản phẩm, gán tồn kho trước mỗi E2E Test, và dọn dẹp sau khi chạy xong.
2. **State Management:** - Áp dụng cơ chế lưu phiên đăng nhập (`storageState`) của Playwright để bỏ qua màn hình Login cho các bài test P1, P2.
3. **CI/CD Integration:**
   - Cấu hình GitHub Actions / GitLab CI: Tự động chạy toàn bộ nhóm P0 mỗi khi có Pull Request mới ghép vào nhánh `main`.