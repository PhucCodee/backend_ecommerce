# tests/test_skus/test_skus.py
import requests

def test_get_skus_by_product_id(base_url):
    """Test lấy danh sách SKU của một sản phẩm cụ thể"""
    product_id = 21 # Dựa theo doc của bạn
    response = requests.get(f"{base_url}/products/{product_id}/skus")
    assert response.status_code in [200, 404] # 404 nếu sản phẩm 21 không còn tồn tại
    if response.status_code == 200:
        assert isinstance(response.json(), list)

def test_seller_get_current_skus(base_url, seller_headers):
    """Test Seller lấy danh sách SKU của chính họ"""
    response = requests.get(f"{base_url}/skus/seller", headers=seller_headers)
    assert response.status_code == 200