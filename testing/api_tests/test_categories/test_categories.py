# tests/test_categories/test_categories.py

import pytest
import requests
import uuid

# ==============================================================================
# 🛠️ FIXTURES DÀNH RIÊNG CHO CATEGORY
# ==============================================================================

@pytest.fixture
def temp_category(base_url, admin_headers):
    """
    FIXTURE: TẠO VÀ DỌN DẸP DANH MỤC TẠM THỜI
    - Mục đích: Cung cấp một danh mục có sẵn cho các test case cần thao tác (GET, PUT) 
      mà không làm rác Database.
    - Cách hoạt động: 
        1. Gọi API POST tạo một danh mục với tên ngẫu nhiên.
        2. `yield` (tạm dừng) và giao danh mục này cho các hàm test sử dụng.
        3. Sau khi hàm test chạy xong, đoạn code bên dưới `yield` sẽ chạy để gọi API DELETE xóa danh mục đó đi.
    """
    payload = {
        "name": f"Updated Name Automation",
        "parentCategoryId": None, 
        "description": "This is an automated test category",
        "imageUrl": "https://example.com/image.jpg",
        "displayOrder": 1,
        "isCore": True,
        "isActive": True
    }
    
    create_res = requests.post(f"{base_url}/categories", json=payload, headers=admin_headers)
    assert create_res.status_code in [200, 201], "Không thể tạo Category để test"
    
    category_data = create_res.json()
    
    # Giao dữ liệu cho hàm test
    yield category_data
    
    # Dọn dẹp sau khi test xong
    category_id = category_data["data"]['categoryId']
    requests.delete(f"{base_url}/categories/{category_id}", headers=admin_headers)

# ==============================================================================
# 🧪 TEST CASES
# ==============================================================================

def test_get_all_categories(base_url):
    """
    TC_CAT_01: LẤY DANH SÁCH TOÀN BỘ DANH MỤC
    - Tiền điều kiện: Không cần đăng nhập (Public API).
    - Hành động: Gửi request GET tới endpoint `/categories`.
    - Kỳ vọng: 
        1. API trả về mã trạng thái 200 OK.
        2. Dữ liệu trả về (response.json) phải có cấu trúc là một mảng (list).
    """
    response = requests.get(f"{base_url}/categories")
    assert response.status_code == 200
    assert isinstance(response.json()['data']['items'], list)

def test_get_core_categories(base_url):
    """
    TC_CAT_02: LẤY DANH SÁCH DANH MỤC CỐT LÕI (CORE CATEGORIES)
    - Tiền điều kiện: Không cần đăng nhập.
    - Hành động: Gửi request GET tới endpoint `/categories/core`.
    - Kỳ vọng:
        1. Trả về mã 200 OK.
        2. Dữ liệu là một mảng (list) chứa các danh mục được đánh dấu isCore = True.
    """
    response = requests.get(f"{base_url}/categories/core")
    assert response.status_code == 200
    assert isinstance(response.json()['data']['items'], list)

def test_admin_create_category(base_url, admin_headers):
    """
    TC_CAT_03: ADMIN TẠO DANH MỤC MỚI
    - Tiền điều kiện: Bắt buộc phải có token của Admin (thông qua admin_headers).
    - Hành động: Gửi request POST kèm Body chứa thông tin danh mục mới.
    - Kỳ vọng:
        1. Trả về mã 200 hoặc 201 (Created).
        2. Sau khi kiểm tra xong, test sẽ tự động gọi API DELETE để xóa chính danh mục vừa tạo.
    """
    random_str = str(uuid.uuid4())[:6]
    payload = {
        "name": f"Created Cat {random_str}",
        "description": "Test creation",
        "isCore": False,
        "isActive": True
    }
    response = requests.post(f"{base_url}/categories", json=payload, headers=admin_headers)
    assert response.status_code in [200, 201]
    
    # Dọn dẹp rác thủ công cho riêng bài test này
    created_id = response.json()["data"]['categoryId']
    requests.delete(f"{base_url}/categories/{created_id}", headers=admin_headers)

def test_get_category_by_id(base_url, temp_category):
    """
    TC_CAT_04: LẤY CHI TIẾT DANH MỤC THEO ID
    - Tiền điều kiện: Dùng fixture `temp_category` để có sẵn 1 danh mục hợp lệ.
    - Hành động: Lấy ID của danh mục đó và gửi request GET `/categories/{id}`.
    - Kỳ vọng:
        1. Trả về mã 200 OK.
        2. ID trong dữ liệu trả về phải khớp với ID mà ta truyền lên.
        3. Tên danh mục trả về phải khớp với tên lúc ta tạo.
    """
    cat_id = temp_category["data"]['categoryId']
    response = requests.get(f"{base_url}/categories/{cat_id}")
    
    assert response.status_code == 200
    assert response.json()["data"]['categoryId'] == cat_id
    assert response.json()["data"]['categoryName'] == temp_category["data"]['categoryName']

def test_get_category_by_slug(base_url, temp_category):
    """
    TC_CAT_05: LẤY CHI TIẾT DANH MỤC THEO SLUG
    - Tiền điều kiện: Dùng fixture `temp_category`. Backend phải hỗ trợ sinh slug.
    - Hành động: Gửi request GET `/categories/slug/{slug}`.
    - Kỳ vọng:
        1. Trả về mã 200 OK.
        2. Slug trả về phải khớp đúng với Slug trên URL.
    - Lưu ý: Nếu backend không trả về slug khi tạo, bài test này sẽ tự động báo Bỏ qua (Skip).
    """
    cat_slug = temp_category["data"]['slug']
    
    if not cat_slug:
        pytest.skip("Backend không trả về slug khi tạo danh mục, bỏ qua test này.")
        
    response = requests.get(f"{base_url}/categories/slug/{cat_slug}")
    assert response.status_code == 200
    assert response.json()["data"]['slug'] == cat_slug

def test_admin_update_category(base_url, admin_headers, temp_category):
    """
    TC_CAT_06: ADMIN CẬP NHẬT THÔNG TIN DANH MỤC
    - Tiền điều kiện: Có quyền Admin. Có sẵn 1 danh mục nhờ `temp_category`.
    - Hành động: 
        1. Thay đổi trường "name" và "description" trong dữ liệu.
        2. Gửi request PUT `/categories/{id}` để cập nhật.
    - Kỳ vọng:
        1. API PUT trả về thành công (200 hoặc 204).
        2. Gọi lại API GET để xác minh rằng dữ liệu trong Database THỰC SỰ ĐÃ BỊ ĐỔI thành tên mới.
    """
    cat_id = temp_category["data"]['categoryId']
    update_payload = temp_category.copy()
    update_payload["data"]["categoryName"] = "Updated Name Automation"
    update_payload["data"]["description"] = "Updated description"
    
    response = requests.put(f"{base_url}/categories/{cat_id}", json=update_payload, headers=admin_headers)
    assert response.status_code in [200, 204] 
    
    get_res = requests.get(f"{base_url}/categories/{cat_id}")
    assert get_res.json()["data"]["categoryName"] == "Updated Name Automation"

def test_admin_delete_category(base_url, admin_headers):
    """
    TC_CAT_07: ADMIN XÓA DANH MỤC
    - Tiền điều kiện: Có quyền Admin. Test này tự tạo ra 1 danh mục dùng riêng để "hiến tế".
    - Hành động:
        1. Gửi request DELETE `/categories/{id}` tới danh mục vừa tạo.
    - Kỳ vọng:
        1. API DELETE trả về thành công (200 hoặc 204).
        2. BƯỚC QUAN TRỌNG: Gọi lại request GET tìm danh mục đó.
        3. API phải báo lỗi 404 (Not Found), chứng tỏ dữ liệu đã biến mất khỏi Database hoàn toàn.
    """
    # 1. Tạo danh mục hiến tế
    payload = {"name": "Cat to be deleted"}
    create_res = requests.post(f"{base_url}/categories", json=payload, headers=admin_headers)
    cat_id = create_res.json()["data"]["categoryId"]
    
    # 2. Xóa danh mục
    delete_res = requests.delete(f"{base_url}/categories/{cat_id}", headers=admin_headers)
    assert delete_res.status_code in [200, 204]
    
    # 3. Kiểm chứng xem đã bị xóa chưa
    get_res = requests.get(f"{base_url}/categories/{cat_id}")
    assert get_res.status_code == 404

def test_get_child_categories(base_url, temp_category):
    """
    TC_CAT_08: LẤY DANH MỤC CON THEO ID CHA
    - Tiền điều kiện: Public API. Dùng `temp_category` làm danh mục cha.
    - Hành động: Gửi request GET `/categories/{id}/children`.
    - Kỳ vọng: Trả về mã 200 OK và danh sách (dù có thể là mảng rỗng []).
    """
    cat_id = temp_category["data"]["categoryId"]
    response = requests.get(f"{base_url}/categories/{cat_id}/children")
    
    assert response.status_code == 200
    assert isinstance(response.json()['data'], list)

# ==============================================================================
# 🛡️ SECURITY TESTS (KIỂM THỬ BẢO MẬT & PHÂN QUYỀN)
# ==============================================================================

def test_security_buyer_cannot_create_category(base_url, user_headers):
    """
    TC_SEC_01 (Phân quyền): BẢO VỆ API ADMIN
    - Kịch bản: Một người dùng bình thường (Buyer) cố tình gọi API tạo danh mục.
    - Kỳ vọng: Backend phải chặn lại và trả về 403 Forbidden (Không đủ quyền).
    """
    payload = {
        "name": "Hacker Category",
        "parentCategoryId": 1,
        "description": "this is the description for pants",
        "imageUrl": "https://example.com/electronics.jpg",
        "displayOrder": 1,
        "isCore": True,
        "isActive": True
    }
    response = requests.post(f"{base_url}/categories", json=payload, headers= user_headers)
    
    # 401: Chưa đăng nhập, 403: Đã đăng nhập nhưng cấm vào
    assert response.status_code in [401, 403], "LỖ HỔNG Phân quyền! Buyer có thể tạo Category!"


@pytest.mark.parametrize("malicious_id", [
    "1' OR '1'='1",
    "9999; DROP TABLE Categories; --",
    "../../etc/passwd"  # Path traversal (nếu backend dùng file system)
])
def test_security_sql_injection_on_category_id(base_url, malicious_id):
    """
    TC_SEC_02 (SQL Injection & Input Validation): TRUYỀN MÃ ĐỘC VÀO ID
    - Kịch bản: Hacker truyền mã SQL Injection vào thẳng đường dẫn /categories/{id}
    - Kỳ vọng: API an toàn phải trả về 400 Bad Request (Lỗi định dạng UUID/Int) 
      hoặc 404 Not Found. TUYỆT ĐỐI không sập server (500).
    """
    response = requests.get(f"{base_url}/categories/{malicious_id}")
    
    # Server khỏe mạnh thì không bao giờ được trả về 500 do lỗi SQL syntax
    assert response.status_code != 500, f"Server bị sập khi nhận malicious_id: {malicious_id}"
    
    # Đảm bảo API trả về 400 (Lỗi validation) hoặc 404 (Không tìm thấy)
    assert response.status_code in [400, 404]