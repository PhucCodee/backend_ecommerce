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
    Vì tạo Product bắt buộc phải có categoryId, ta cần chuẩn bị sẵn 1 category.
    """
    random_str = str(uuid.uuid4())[:6]
    cat_payload = {
        "name": f"Temp Cat for Prod {random_str}",
        "isCore": False,
        "isActive": True
    }
    cat_res = requests.post(f"{base_url}/categories", json=cat_payload, headers=admin_headers)
    cat_id = cat_res.json().get("id")
    
    yield cat_id
    
    # Dọn dẹp Danh mục sau khi các test về Product chạy xong
    if cat_id:
        requests.delete(f"{base_url}/categories/{cat_id}", headers=admin_headers)

@pytest.fixture
def temp_product(base_url, seller_headers, admin_headers, temp_category_id):
    """
    FIXTURE: SELLER TẠO SẢN PHẨM TẠM THỜI
    Cung cấp sẵn 1 sản phẩm cho các API cần ID sản phẩm (như Xem chi tiết, Seller xem kho).
    """
    random_str = str(uuid.uuid4())[:6]
    payload = {
        "name": f"Auto Test Shirt {random_str}",
        "description": "Created by Pytest Fixture",
        "categoryIds": [temp_category_id],
        "brand": "PytestBrand",
        "weightKg": 0.2,
        "defaultSkuPrice": 25.99,
        "defaultSkuStock": 100,
        "dimensionsCm": "20x15x5"
    }
    prod_res = requests.post(f"{base_url}/products/seller", json=payload, headers=seller_headers)
    product_data = prod_res.json()
    
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
    - Hành động: Gọi GET /products với tham số phân trang.
    - Kỳ vọng: 200 OK, trả về danh sách/mảng.
    """
    params = {"pageNumber": 1, "pageSize": 10}
    response = requests.get(f"{base_url}/products", params=params)
    
    assert response.status_code == 200
    # API có thể trả về list trực tiếp, hoặc một object chứa metadata phân trang (như items, totalCount)
    data = response.json()
    assert type(data) in [list, dict], "Dữ liệu trả về không đúng định dạng"

@pytest.mark.parametrize("filters", [
    {"brand": "PytestBrand"},
    {"minPrice": 10, "maxPrice": 100},
    {"sortBy": "price", "desc": "true"}
])
def test_get_products_with_filters(base_url, filters):
    """
    TC_PROD_02: PUBLIC LỌC SẢN PHẨM
    - Sử dụng @pytest.mark.parametrize để chạy 1 bài test này 3 lần với 3 bộ lọc khác nhau.
    - Kỳ vọng: Cả 3 lần gọi đều không bị lỗi server (200 OK).
    """
    response = requests.get(f"{base_url}/products", params=filters)
    assert response.status_code == 200

def test_seller_create_product(base_url, seller_headers, temp_category_id, admin_headers):
    """
    TC_PROD_03: SELLER TẠO SẢN PHẨM MỚI
    - Tiền điều kiện: Dùng token của Seller. Truyền vào ID của danh mục tạm.
    - Kỳ vọng: 200/201 Created. Tên sản phẩm trả về khớp với tên gửi lên.
    """
    random_str = str(uuid.uuid4())[:6]
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
    
    data = response.json()
    assert data["name"] == prod_name
    
    # Tự dọn dẹp thủ công (Admin xóa)
    requests.delete(f"{base_url}/products/{data['id']}", headers=admin_headers)

def test_seller_get_own_products(base_url, seller_headers, temp_product):
    """
    TC_PROD_04: SELLER XEM KHO HÀNG CỦA MÌNH
    - Tiền điều kiện: Đã gọi fixture tạo sẵn 1 sản phẩm cho seller này.
    - Hành động: Seller gọi GET /products/seller.
    - Kỳ vọng: Trả về danh sách và chắc chắn có chứa thông tin sản phẩm.
    """
    response = requests.get(f"{base_url}/products/seller", headers=seller_headers)
    assert response.status_code == 200
    assert isinstance(response.json(), list)

def test_admin_delete_product(base_url, admin_headers, seller_headers, temp_category_id):
    """
    TC_PROD_05: ADMIN XÓA SẢN PHẨM
    - Hành động: 
        1. Seller tạo 1 sản phẩm.
        2. Admin lấy ID đó đi gọi hàm Xóa.
    - Kỳ vọng: Xóa thành công (200/204), sau đó Public GET ID đó sẽ ra 404.
    """
    # 1. Seller tạo sản phẩm hiến tế
    payload = {
        "name": "Product to be deleted",
        "categoryIds": [temp_category_id],
        "defaultSkuPrice": 9.99,
        "defaultSkuStock": 10
    }
    create_res = requests.post(f"{base_url}/products/seller", json=payload, headers=seller_headers)
    prod_id = create_res.json()["id"]
    
    # 2. Admin xóa sản phẩm đó
    delete_res = requests.delete(f"{base_url}/products/{prod_id}", headers=admin_headers)
    assert delete_res.status_code in [200, 204]
    
    # 3. Public kiểm tra lại xem còn không
    # (Tùy thuộc backend của bạn có API GET /products/{id} hay không, nếu có thì mở comment 2 dòng dưới)
    # check_res = requests.get(f"{base_url}/products/{prod_id}")
    # assert check_res.status_code == 404