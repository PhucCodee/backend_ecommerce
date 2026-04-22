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