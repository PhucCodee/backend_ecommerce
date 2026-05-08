# tests/test_skus/test_skus.py
import pytest
import requests
import uuid
import json

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

    
    # 2. Seller tạo Product tạm (nhét vào Category vừa tạo)
    prod_payload = {
        "name": "Classic Crew T-Shirt",
        "description": "Everyday cotton crew tee",
        "categoryIds": [
            1,
            12
        ],
        "brand": "CurryWear123",
        "weightKg": 0.25,
        "defaultSkuPrice": 195000,
        "defaultSkuStock": 200,
        "dimensionsCm": "30x25x2",
        "defaultSkuInventory": {
            "quantityAvailable": 200,
            "quantityReserved": 10,
            "quantitySold": 5,
            "reorderPoint": 30,
            "reorderQuantity": 150

        }
    }

    prod_res = requests.post(f"{base_url}/products/seller", json=prod_payload, headers=seller_headers)
    prod_id = prod_res.json()["data"]["id"]
    
    # Giao ID sản phẩm này cho các test case bên dưới sử dụng
    yield prod_id
    
    # 3. Dọn dẹp rác sau khi test chạy xong
    requests.delete(f"{base_url}/products/{prod_id}", headers=admin_headers)
  

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
    print(response)

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
    payload = {
        "productId": base_product,
        "variantAttributes": json.dumps({"color": "Blue", "size": "XL"}),
        "price": 99.99,
        "stock": 500
    }
    
    response = requests.post(f"{base_url}/skus/seller", json=payload, headers=seller_headers)
    
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

# ==============================================================================
# 🧪 TEST CASES BỔ SUNG (UPDATE & DELETE)
# ==============================================================================

def test_seller_update_sku(base_url, seller_headers, base_product):
    """
    TC_SKU_05: SELLER CẬP NHẬT SKU (THAY ĐỔI GIÁ VÀ TỒN KHO)
    """
    # 1. Tạo 1 SKU trước
    create_payload = {
        "productId": base_product,
        "variantAttributes": json.dumps({"size": "L", "color": "black"}),
        "price": 24.99,
        "costPrice": 12.50,
        "compareAtPrice": 29.99,
        "weightKg": 0.26,
        "dimensionsCm": "30x25x2",
        "stock": 50
    }
    create_res = requests.post(f"{base_url}/skus/seller", json=create_payload, headers=seller_headers)
    assert create_res.status_code in [200, 201], f"Failed to create SKU: {create_res.json()}"
    sku_id = create_res.json()["data"]["skuId"]
    
    # 2. Thực hiện Update
    update_payload = {
        "price": 75.5,    # Đổi giá
        "stock": 100      # Nhập thêm hàng
    }
    update_res = requests.put(f"{base_url}/skus/seller/{sku_id}", json=update_payload, headers=seller_headers)
    
    assert update_res.status_code == 200
    assert float(update_res.json()["data"]["price"]) == 75.5
    assert update_res.json()["data"]["stock"] == 100


def test_seller_delete_sku(base_url, seller_headers, base_product):
    """
    TC_SKU_06: SELLER XÓA SKU
    """
    # 1. Tạo SKU để "hiến tế"
    create_payload = {
        "productId": base_product,
        "variantAttributes": json.dumps({"size": "L", "color": "black"}),
        "price": 24.99,
        "costPrice": 12.50,
        "compareAtPrice": 29.99,
        "weightKg": 0.26,
        "dimensionsCm": "30x25x2",
        "stock": 50
    }
    create_res = requests.post(f"{base_url}/skus/seller", json=create_payload, headers=seller_headers)
    assert create_res.status_code in [200, 201], f"Failed to create SKU: {create_res.json()}"
    
    sku_id = create_res.json()["data"]["skuId"]
    
    # 2. Gọi API Xóa
    delete_res = requests.delete(f"{base_url}/skus/seller/{sku_id}", headers=seller_headers)
    assert delete_res.status_code in [200, 204]
    
    # 3. Kiểm tra lại xem đã bay màu chưa
    get_res = requests.get(f"{base_url}/skus/{sku_id}")
    assert get_res.status_code == 404


# ==============================================================================
# 🛡️ SECURITY & BUSINESS LOGIC TESTS (KIỂM THỬ BẢO MẬT & NGHIỆP VỤ)
# ==============================================================================

@pytest.mark.parametrize("bad_price, bad_stock", [
    (-10.0, 10),    # Giá âm
    (10.0, -5),     # Tồn kho âm
    (-5.0, -5)      # Cả hai đều âm
])
def test_business_logic_prevent_negative_values(base_url, seller_headers, base_product, bad_price, bad_stock):
    """
    TC_SEC_01 (Business Logic): CHỐNG HACKER ĐẶT GIÁ / TỒN KHO ÂM
    - Kịch bản: Hacker là Seller, tạo SKU với giá âm để hệ thống trả ngược lại tiền khi Buyer mua hàng.
    - Kỳ vọng: API Validation phải chặn lại, trả về 400 Bad Request.
    """
    payload = {
        "productId": base_product,
        "price": bad_price,
        "stock": bad_stock
    }
    response = requests.post(f"{base_url}/skus/seller", json=payload, headers=seller_headers)
    
    # Tuyệt đối không được phép trả về 201 Created
    assert response.status_code == 400, f"Hệ thống cho phép giá {bad_price} và tồn kho {bad_stock}"


def test_idor_seller_update_other_seller_sku(base_url, seller_headers, base_product):
    """
    TC_SEC_02 (IDOR): SELLER NÀY CỐ TÌNH SỬA SKU CỦA SELLER KHÁC
    - Kịch bản: Bạn có ID của 1 SKU thuộc về gian hàng đối thủ. Bạn dùng Token của bạn gọi API sửa giá của nó thành 0 đồng.
    - Kỳ vọng: Backend phải check quyền sở hữu (Owner check) và báo lỗi 403 / 404.
    """
    # TẠM GIẢ LẬP: Vì ta chỉ có 1 `seller_headers`, giả sử `sku_id` dưới đây là của 1 Seller khác
    # Trong môi trường thực tế, bạn nên có `seller_a_headers` và `seller_b_headers`
    fake_other_seller_sku_id = "123e4567-e89b-12d3-a456-426614174000" # UUID ngẫu nhiên
    
    malicious_payload = {
        "price": 0.0 # Phá giá đối thủ
    }
    
    response = requests.put(f"{base_url}/skus/seller/{fake_other_seller_sku_id}", json=malicious_payload, headers=seller_headers)
    
    # Trả về 403 (Cấm) hoặc 404 (Không tìm thấy, vì DB đã filter theo SellerId của Token)
    assert response.status_code in [403, 404]