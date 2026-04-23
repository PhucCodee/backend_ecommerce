# tests/test_products/test_products.py
import pytest
import requests
import uuid

# ==============================================================================
# 🛠️ FIXTURES DÀNH RIÊNG CHO PRODUCT
# ==============================================================================

@pytest.fixture
def temp_category_id(base_url, admin_headers):
    """
    FIXTURE: TẠO DANH MỤC TẠM ĐỂ SẢN PHẨM CÓ THỂ NẰM TRONG ĐÓ
    """
    cat_payload = {
        "name": f"Temp Cat for Prod {str(uuid.uuid4())[:4]}",
        "isCore": False,
        "isActive": True
    }
    cat_res = requests.post(f"{base_url}/categories", json=cat_payload, headers=admin_headers)
    
    # Lấy ID từ object "data"
    cat_id = cat_res.json()["data"]["categoryId"]
    
    yield cat_id
    
    # Dọn dẹp Danh mục sau khi test chạy xong
    if cat_id:
        requests.delete(f"{base_url}/categories/{cat_id}", headers=admin_headers)

@pytest.fixture
def temp_product(base_url, seller_headers, admin_headers, temp_category_id):
    """
    FIXTURE: SELLER TẠO SẢN PHẨM TẠM THỜI
    """
  
    payload = {
        "name": f"Auto Test Shirt ",
        "description": "Created by Pytest Fixture",
        "categoryIds": [temp_category_id],
        "brand": "PytestBrand",
        "weightKg": 0.2,
        "defaultSkuPrice": 25.99,
        "defaultSkuStock": 100,
        "dimensionsCm": "20x15x5"
    }
    prod_res = requests.post(f"{base_url}/products/seller", json=payload, headers=seller_headers)
    
    # Lấy object data chứa thông tin sản phẩm
    product_data = prod_res.json()["data"]
    
    yield product_data
    
    # Admin dọn dẹp Sản phẩm sau khi test xong
    prod_id = product_data.get("id")
    if prod_id:
        requests.delete(f"{base_url}/products/{prod_id}", headers=admin_headers)

# ==============================================================================
# 🧪 TEST CASES
# ==============================================================================

def test_get_products_public(base_url):
    """
    TC_PROD_01: PUBLIC XEM DANH SÁCH SẢN PHẨM
    """
    params = {"pageNumber": 1, "pageSize": 10}
    response = requests.get(f"{base_url}/products", params=params)
    
    assert response.status_code == 200
    
    # Backend thường trả về {"data": [...]} hoặc {"data": {"items": [...]}}
    response_body = response.json()
    assert "data" in response_body, "API không trả về object 'data'"

@pytest.mark.parametrize("filters", [
    {"brand": "PytestBrand"},
    {"minPrice": 10, "maxPrice": 100},
    {"sortBy": "price", "desc": "true"}
])
def test_get_products_with_filters(base_url, filters):
    """
    TC_PROD_02: PUBLIC LỌC SẢN PHẨM
    """
    response = requests.get(f"{base_url}/products", params=filters)
    assert response.status_code == 200

def test_seller_create_product(base_url, seller_headers, temp_category_id, admin_headers):
    """
    TC_PROD_03: SELLER TẠO SẢN PHẨM MỚI
    """
    random_str = str(uuid.uuid4())[:4]
    prod_name = f"New Seller Hat {random_str}"
    payload = {
        "name": prod_name,
        "description": "Hat for automation testing",
        "categoryIds": [temp_category_id],
        "brand": "AutoWear",
        "weightKg": 0.1,
        "defaultSkuPrice": 15.00,
        "defaultSkuStock": 50,
        "dimensionsCm": "10x10x5"
    }
    
    response = requests.post(f"{base_url}/products/seller", json=payload, headers=seller_headers)
    assert response.status_code in [200, 201]
    
    # Trích xuất data và id ra biến riêng cho dễ đọc, tránh lỗi ngoặc kép lồng nhau
    product_data = response.json()["data"]
    prod_id = product_data["id"]
    
    assert product_data["name"] == prod_name
    
    # Tự dọn dẹp thủ công
    requests.delete(f"{base_url}/products/{prod_id}", headers=admin_headers)

def test_seller_get_own_products(base_url, seller_headers, temp_product):
    """
    TC_PROD_04: SELLER XEM KHO HÀNG CỦA MÌNH
    """
    response = requests.get(f"{base_url}/products/seller", headers=seller_headers)
    assert response.status_code == 200
    
    # Đảm bảo có key "data"
    response_body = response.json()
    assert "data" in response_body

def test_admin_delete_product(base_url, admin_headers, seller_headers, temp_category_id):
    """
    TC_PROD_05: ADMIN XÓA SẢN PHẨM
    """
    # 1. Seller tạo sản phẩm hiến tế
    payload = {
        "name": "Product to be deleted",
        "categoryIds": [temp_category_id],
        "defaultSkuPrice": 9.99,
        "defaultSkuStock": 10
    }
    create_res = requests.post(f"{base_url}/products/seller", json=payload, headers=seller_headers)
    
    # Lấy ID từ bên trong object "data"
    prod_id = create_res.json()["data"]["id"]
    
    # 2. Admin xóa sản phẩm đó
    delete_res = requests.delete(f"{base_url}/products/{prod_id}", headers=admin_headers)
    assert delete_res.status_code in [200, 204]


# ==============================================================================
# 🛡️ SECURITY TESTS (KIỂM THỬ BẢO MẬT)
# ==============================================================================

# Danh sách các mã độc SQLi phổ biến (Từ điển Payload)
SQLI_PAYLOADS = [
    "Shirt' OR '1'='1",
    "Shirts'+OR+1=1--",
    "'; DROP TABLE Products; --",
    "' UNION SELECT null, username, password FROM Users --",
    "1 OR 1=1",
    "\" OR \"\"=\""
]

@pytest.mark.parametrize("payload", SQLI_PAYLOADS)
def test_api_prevent_sql_injection_on_search(base_url, payload):
    """
    TC_SEC_01: Kiểm tra lỗi SQL Injection trên tham số tìm kiếm (search).
    Kỳ vọng: Trả về 200 OK (data rỗng) hoặc 400 Bad Request. 
    Tuyệt đối không sập server (500) và không lộ toàn bộ dữ liệu.
    """
    params = {"search": payload}
    response = requests.get(f"{base_url}/products", params=params)

    # 1. Server không được sập (Trả về 500 thường nghĩa là DB đã cố gắng chạy mã độc và báo lỗi cú pháp)
    assert response.status_code != 500, f"Cảnh báo: API bị sập (Lỗi 500) với payload: {payload}"

    # 2. Nếu trả về 200, đảm bảo danh sách sản phẩm phải trống
    # (Vì hệ thống an toàn sẽ coi payload là 1 chuỗi ký tự bình thường, ko có SP nào tên như vậy)
    if response.status_code == 200:
        response_body = response.json()
        
        # Xử lý linh hoạt 2 trường hợp JSON trả về từ Backend
        data = response_body.get("data", [])
        if isinstance(data, dict) and "items" in data:
            items = data["items"]
        else:
            items = data
            
        assert len(items) == 0, f"Lỗ hổng SQLi! API đã trả về dữ liệu khi truyền mã độc: {payload}"


@pytest.mark.parametrize("payload", [
    "price; DROP TABLE Products; --",
    "(SELECT CASE WHEN (1=1) THEN price ELSE name END)",
    "price ASC, (SELECT Sleep(5))"
])
def test_api_prevent_sql_injection_on_sort(base_url, payload):
    """
    TC_SEC_02: Kiểm tra SQL Injection trên tham số sắp xếp (sortBy).
    Lưu ý: Mệnh đề ORDER BY rất khó dùng Parameterized Query, nên Dev hay nối chuỗi ẩu ở đây.
    """
    params = {"sortBy": payload}
    response = requests.get(f"{base_url}/products", params=params)
    
    # Ở tham số Sort, Backend chuẩn nên bắt lỗi ngay từ khâu Validation (Validation Error)
    # và trả về 400 Bad Request (VD: "Trường sắp xếp không hợp lệ").
    # Tuyệt đối không được trả về 500 (Sập DB do nối chuỗi).
    assert response.status_code != 500, f"Lỗ hổng SQLi ở sortBy! Server báo lỗi 500 với payload: {payload}"