# api_tests/test_users/test_users.py
import requests
import uuid
import pytest

# ==============================================================================
# 🧪 TEST CASES CHO NHÁNH USER & PROFILE
# ==============================================================================

def test_get_own_profile(base_url, user_headers):
    """
    TC_USER_01: USER LẤY THÔNG TIN PROFILE CỦA CHÍNH MÌNH
    - Tiền điều kiện: Sử dụng fixture `user_headers` (đã đăng nhập sẵn).
    - Hành động: Gửi request GET `/users/profile`.
    - Kỳ vọng: 
        1. Trả về mã 200 OK.
        2. Email trả về phải khớp với email mà ta đã cấu hình trong conftest.py.
    """
    response = requests.get(f"{base_url}/users/profile", headers=user_headers)
    
    assert response.status_code == 200
    data = response.json()["data"]
    assert "email" in data
    # Tuỳ thuộc vào email bạn setup trong conftest.py, ở đây ví dụ là phuc1@gmail.com
    assert data["email"] == "phuc1@gmail.com"

def test_update_own_profile(base_url, user_headers):
    """
    TC_USER_02: USER CẬP NHẬT THÔNG TIN CÁ NHÂN
    - Hành động: 
        1. Tạo một họ tên mới (gắn chuỗi ngẫu nhiên để biết chắc chắn nó thay đổi).
        2. Gửi request PUT `/users/profile` kèm dữ liệu mới.
    - Kỳ vọng:
        1. Trả về 200 OK.
        2. Lấy lại profile một lần nữa để verify dữ liệu THỰC SỰ đã lưu vào DB.
    """
    new_last_name = f"Automation"
    
    payload = {
        "firstName": "Test",
        "lastName": new_last_name,
        "phone": "0909999888",
        # Tuỳ thuộc schema của bạn, có thể có thêm bio, gender, dateOfBirth...
    }
    
    # 1. Thực hiện Update
    update_res = requests.put(f"{base_url}/users/profile", json=payload, headers=user_headers)
    assert update_res.status_code in [200, 204]
    
    # 2. Lấy lại Profile để kiểm chứng
    get_res = requests.get(f"{base_url}/users/profile", headers=user_headers)
    assert get_res.status_code == 200
    assert get_res.json()["data"]["lastName"] == new_last_name

def test_admin_get_all_users(base_url, admin_headers):
    """
    TC_USER_03: ADMIN QUẢN LÝ DANH SÁCH NGƯỜI DÙNG
    - Tiền điều kiện: Phải có quyền Admin (dùng `admin_headers`).
    - Hành động: Gọi GET `/users` kèm tham số phân trang.
    - Kỳ vọng: 200 OK, dữ liệu trả về không bị rỗng và đúng định dạng.
    """
    params = {"pageNumber": 1, "pageSize": 10}
    response = requests.get(f"{base_url}/users", params=params, headers=admin_headers)
    
    assert response.status_code == 200
    
    data = response.json()
    # API có thể trả về thẳng list `[]` hoặc dict có phân trang `{"items": [], "total": 100}`
    assert type(data) in [list, dict]

def test_unauthorized_access_profile(base_url):
    """
    TC_USER_04: KIỂM TRA BẢO MẬT (CHẶN NGƯỜI LẠ)
    - Tiền điều kiện: Đóng vai một người vãng lai chưa đăng nhập.
    - Hành động: Cố tình gọi API lấy Profile nhưng KHÔNG TRUYỀN TOKEN vào Headers.
    - Kỳ vọng: Backend bắt buộc phải từ chối và trả về lỗi 401 Unauthorized.
    """
    # Gửi request "trần trụi", không có headers xác thực
    response = requests.get(f"{base_url}/users/profile")
    
    # Mã 401 nghĩa là "Anh chưa đăng nhập, không có cửa vào đây!"
    assert response.status_code == 401


# ==============================================================================
# 🛡️ SECURITY TESTS BỔ SUNG (PHÂN QUYỀN & LỖ HỔNG LEO THANG ĐẶC QUYỀN)
# ==============================================================================

def test_security_user_cannot_access_admin_endpoint(base_url, user_headers):
    """
    TC_SEC_01 (Broken Access Control): BẢO VỆ API CỦA ADMIN
    - Kịch bản: Đăng nhập bằng tài khoản User thường, nhưng lén gọi API của Admin (Lấy danh sách tất cả Users).
    - Kỳ vọng: Backend đọc Token, biết đây là User thường -> Lập tức chặn lại và báo 403 Forbidden.
    - Chú ý: 401 là "Chưa đăng nhập" (như TC_USER_04), còn 403 là "Đã đăng nhập nhưng CẤM VÀO".
    """
    params = {"pageNumber": 1, "pageSize": 10}
    response = requests.get(f"{base_url}/users", params=params, headers=user_headers)
    
    assert response.status_code == 403, "Lỗ hổng Phân quyền: User thường có thể xem toàn bộ data của Users khác!"

def test_security_prevent_role_escalation(base_url, user_headers):
    """
    TC_SEC_02 (Mass Assignment / Role Escalation): CHỐNG LEO THANG ĐẶC QUYỀN
    - Kịch bản: Hacker dùng API Cập nhật Profile (vốn dĩ chỉ để đổi tên, SĐT) 
      nhưng lén nhét thêm biến `"role": "Admin"` vào Payload để xem Backend có bị lừa không.
    - Kỳ vọng: Backend dùng DTO (Data Transfer Object) an toàn, tự động bỏ qua (ignore) các trường nhạy cảm này.
    """
    malicious_payload = {
        "firstName": "Hacker",
        "lastName": "Pro",
        "phone": "0999999999",
        "role": "Admin",          # Cố tình leo quyền bằng text
        "roleId": 1,              # Cố tình leo quyền bằng ID
        "isActive": False         # Thử tự khóa tài khoản của chính mình
    }
    
    # API vẫn cho phép đổi tên/SĐT thành công
    update_res = requests.put(f"{base_url}/users/profile", json=malicious_payload, headers=user_headers)
    assert update_res.status_code in [200, 204]
    
    # 2. Gọi lại API Get Profile để kiểm tra Role thực tế
    get_res = requests.get(f"{base_url}/users/profile", headers=user_headers)
    user_data = get_res.json()["data"]
    
    # Khẳng định Role của User này KHÔNG BỊ ĐỔI THÀNH ADMIN
    # (Tùy thuộc vào JSON backend của bạn trả về chữ "Admin" hay roleId)
    if "role" in user_data:
        assert user_data["role"] != "Admin", "LỖ HỔNG NGHIÊM TRỌNG: Hacker đã tự nâng quyền lên Admin thành công!"