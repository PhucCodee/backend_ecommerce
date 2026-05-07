# tests/test_products/test_products.py
import pytest
import requests

@pytest.mark.parametrize("query_params", [
    {"pageNumber": 1, "pageSize": 5},
    {"sortBy": "price", "desc": "true"},
    {"brand": "apple"}
])
def test_get_products_with_parameters(base_url, query_params):
    """Test API lấy danh sách sản phẩm với nhiều bộ lọc khác nhau"""
    response = requests.get(f"{base_url}/products", params=query_params)
    assert response.status_code == 200
    assert type(response.json()) in [list, dict]

def test_seller_create_product(base_url, seller_headers):
    """Test Seller có thể tạo sản phẩm mới"""
    payload = {
        "name": "Auto Test T-Shirt",
        "description": "Created by Pytest",
        "categoryIds": [1],
        "brand": "CurryWear",
        "weightKg": 0.25,
        "defaultSkuPrice": 19.99,
        "defaultSkuStock": 200,
        "dimensionsCm": "30x25x2"
    }
    response = requests.post(f"{base_url}/products/seller", json=payload, headers=seller_headers)
    assert response.status_code in [200, 201]
    
def test_admin_delete_product(base_url, admin_headers):
    """Test Admin có quyền xóa sản phẩm (dùng ID giả định 9999 để test an toàn)"""
    product_id = 9999 
    response = requests.delete(f"{base_url}/products/{product_id}", headers=admin_headers)
    # Trả về 200 (xóa thành công), 204 (No Content), hoặc 404 (Không tìm thấy) đều hợp lý
    assert response.status_code in [200, 204, 404]