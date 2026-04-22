# tests/test_skus/test_skus.py
import pytest
import requests
import uuid

# ==============================================================================
# 🛠️ FIXTURES DÀNH RIÊNG CHO SKU
# ==============================================================================

@pytest.fixture
def base_product(base_url, admin_headers, seller_headers):
    """
    FIXTURE: TẠO MÔI TRƯỜNG HOÀN CHỈNH CHO SKU (CATEGORY -> PRODUCT)
    Vì SKU bắt buộc phải thuộc về một Product, ta phải tạo Product trước.
    Và Product thì bắt buộc phải có Category.
    """

    
    # 1. Admin tạo Category tạm
    cat_payload = {"name": f"Cat for SKU", "isActive": True}
    cat_res = requests.post(f"{base_url}/categories", json=cat_payload, headers=admin_headers)
    cat_id = cat_res.json()["data"]["categoryId"]
    
    # 2. Seller tạo Product tạm (nhét vào Category vừa tạo)
    prod_payload = {
        "name": f"Product for SKU",
        "categoryIds": [cat_id],
        "defaultSkuPrice": 10.0,
        "defaultSkuStock": 10
    }
    prod_res = requests.post(f"{base_url}/products/seller", json=prod_payload, headers=seller_headers)
    prod_id = prod_res.json()["data"]["id"]
    
    # Giao ID sản phẩm này cho các test case bên dưới sử dụng
    yield prod_id
    
    # 3. Dọn dẹp rác sau khi test chạy xong
    requests.delete(f"{base_url}/products/{prod_id}", headers=admin_headers)
    requests.delete(f"{base_url}/categories/{cat_id}", headers=admin_headers)

# ==============================================================================
# 🧪 TEST CASES
# ==============================================================================

def test_get_skus_by_product_id(base_url, base_product):
    """
    TC_SKU_01: LẤY DANH SÁCH SKU CỦA MỘT SẢN PHẨM (PUBLIC)
    - Tiền điều kiện: Dùng fixture `base_product` để có 1 ID sản phẩm hợp lệ.
    - Hành động: Gửi request GET `/products/{id}/skus`.
    - Kỳ vọng: 
        1. Trả về mã 200 OK.
        2. Chắc chắn phải là danh sách.
    """
    response = requests.get(f"{base_url}/products/{base_product}/skus")
    
    assert response.status_code == 200
    assert isinstance(response.json()["data"]["items"], list)

def test_seller_get_all_skus(base_url, seller_headers):
    """
    TC_SKU_02: SELLER LẤY DANH SÁCH TẤT CẢ SKU TRONG KHO CỦA MÌNH
    - Hành động: Seller gọi GET `/skus/seller` (Hoặc endpoint tương đương theo doc của bạn).
    - Kỳ vọng: Trả về mã 200 OK.
    """
    # Lưu ý: Nếu đường dẫn API của bạn khác, hãy sửa lại phần "/skus/seller" cho đúng
    response = requests.get(f"{base_url}/skus/seller", headers=seller_headers)
    
    assert response.status_code == 200
    assert isinstance(response.json()["data"]["items"], list)

def test_seller_create_sku(base_url, seller_headers, base_product):
    """
    TC_SKU_03: SELLER TẠO THÊM SKU MỚI CHO SẢN PHẨM
    - Tiền điều kiện: Đã có sẵn ID sản phẩm (`base_product`).
    - Hành động: Gọi POST tạo SKU mới (ví dụ: Size XL, Màu Xanh) với giá và số lượng khác.
    - Kỳ vọng: Trả về 200/201 Created. Giá và số lượng phải khớp với lúc gửi lên.
    """
    # Ghi chú: Payload này phụ thuộc vào cấu trúc backend của bạn, 
    # Nếu backend dùng endpoint POST /skus/seller thì dùng:
    payload = {
        "productId": base_product,
        "skuCode": f"AUTO-SKU-{str(uuid.uuid4())[:4]}",
        "price": 99.99,
        "stock": 500,
        "attributes": {"color": "Blue", "size": "XL"}
    }
    
    response = requests.post(f"{base_url}/skus/seller", json=payload, headers=seller_headers)
    
    # Có thể một số backend quy định API tạo SKU nằm ở đường dẫn khác:
    # response = requests.post(f"{base_url}/products/{base_product}/skus", ...)
    
    assert response.status_code in [200, 201]
    data = response.json()["data"]
    assert float(data.get("price", 0)) == 99.99
    assert data.get("stock") == 500

def test_create_sku_validation_error(base_url, seller_headers):
    """
    TC_SKU_04: KIỂM TRA BẮT LỖI (VALIDATION) KHI GỬI THIẾU DỮ LIỆU
    - Tiền điều kiện: Cố tình là một Hacker/User gửi sai dữ liệu.
    - Hành động: Tạo SKU nhưng cố tình bỏ quên trường bắt buộc là `productId`.
    - Kỳ vọng: Backend phải chặn lại và báo lỗi 400 Bad Request.
    """
    payload = {
        # Cố tình thiếu productId
        "skuCode": "INVALID-SKU",
        "price": 24.99,
        "stock": 50
    }
    
    response = requests.post(f"{base_url}/skus/seller", json=payload, headers=seller_headers)
    
    # Nếu API của bạn quá xịn, nó có thể báo 422 Unprocessable Entity, 400 là phổ biến nhất.
    assert response.status_code in [400, 422, 404]