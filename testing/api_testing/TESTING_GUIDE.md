# 🚀 Hướng dẫn Kiểm thử API Ecommerce

Tài liệu này hướng dẫn cách thiết lập, vận hành và quản lý bộ mã nguồn kiểm thử (Automation Testing) cho hệ thống Backend ASP.NET sử dụng ngôn ngữ **Python**.

---

## 📂 1. Cấu trúc Thư mục Dự án
Dự án được tổ chức theo mô hình **Domain-based**, giúp dễ dàng mở rộng và bảo trì.

```text
api_testing/
├── venv/                 # Môi trường ảo (Không lưu lên Git)
├── .gitignore            # Quy định các file không đẩy lên Git
├── conftest.py           # Cấu hình dùng chung (URL, Login Admin/Seller/User)
├── TESTING_GUIDE.md      # File hướng dẫn này
└── tests/                # Thư mục chứa các kịch bản kiểm thử
    ├── test_auth/        # Kiểm tra Đăng ký & Đăng nhập
    ├── test_categories/  # Kiểm tra CRUD Danh mục sản phẩm
    ├── test_products/    # Kiểm tra Sản phẩm (Seller & Admin)
    ├── test_skus/        # Kiểm tra Biến thể sản phẩm (SKU)
    └── test_users/       # Kiểm tra Profile & Quản lý người dùng

---

## 🛠️ 2. Thiết lập Ban đầu (Setup)

Nếu bạn vừa tải dự án về hoặc làm việc trên máy mới, hãy thực hiện các bước sau:

1. **Kích hoạt môi trường ảo (venv):**
   - Windows: `venv\Scripts\activate`
   - Linux/Mac: `source venv/bin/activate`
   - *Dấu hiệu thành công:* Có chữ `(venv)` ở đầu dòng lệnh trong terminal.

2. **Cài đặt thư viện cần thiết:**
   ```bash
   pip install pytest requests


## 🛠️ 3. Lệnh chạy
Mục tiêu	                            Câu lệnh
Chạy toàn bộ bài test	                pytest -v
Chạy một thư mục cụ thể	                pytest tests/test_products/ -v
Chạy một file cụ thể	                pytest tests/test_auth/test_auth.py -v
Chạy một hàm test cụ thể	            pytest -k "test_admin_login" -v
Dừng ngay khi gặp lỗi đầu tiên	        pytest -x -v
Hiện nội dung hàm print() ra màn hình	pytest -s -v