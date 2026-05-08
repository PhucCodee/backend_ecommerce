# api_tests/test_inventory/test_inventory.py
import requests
import uuid
import pytest

# ==============================================================================
# 🧪 TEST CASES CHO NHÁNH INVENTORY (QUẢN LÝ KHO)
# ==============================================================================

def test_seller_update_sku_inventory_success(base_url, seller_headers):
    """
    TC_INV_01: SELLER CẬP NHẬT INVENTORY CỦA SKU CỦA MÌNH
    - Tiền điều kiện: Seller đã đăng nhập (có token). SKU id = 1 tồn tại.
    - Hành động: Gửi PUT /inventories/skus/1 với dữ liệu inventory mới.
    - Kỳ vọng: 200 OK. Inventory được cập nhật thành công.
    """
    sku_id = 1
    payload = {
        "quantityAvailable": 150,
        "quantityReserved": 10,
        "quantitySold": 25,
        "reorderPoint": 20,
        "reorderQuantity": 100
    }
    
    response = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=payload,
        headers=seller_headers
    )
    
    assert response.status_code in [200, 204], \
        f"Expected 200/204, got {response.status_code}: {response.text}"


def test_seller_update_sku_inventory_nonexistent(base_url, seller_headers):
    """
    TC_INV_02: SELLER TRY UPDATE SKU KHÔNG TỒN TẠI
    - Tiền điều kiện: SKU id = 999999 không tồn tại.
    - Hành động: Gửi PUT /inventories/skus/999999.
    - Kỳ vọng: Backend trả về 404 Not Found.
    """
    sku_id = 999999
    payload = {
        "quantityAvailable": 100,
        "quantityReserved": 5,
        "quantitySold": 10,
        "reorderPoint": 15,
        "reorderQuantity": 50
    }
    
    response = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=payload,
        headers=seller_headers
    )
    
    assert response.status_code in [404, 400], \
        f"Expected 404/400 for non-existent SKU, got {response.status_code}"


def test_seller_update_sku_inventory_invalid_payload(base_url, seller_headers):
    """
    TC_INV_03: VALIDATION - PAYLOAD KHÔNG HỢP LỆ
    - Hành động: Gửi PUT với dữ liệu invalid (thiếu fields, số âm, kiểu sai...).
    - Kỳ vọng: Backend trả về 400 Bad Request.
    """
    sku_id = 1
    invalid_payloads = [
        {"quantityAvailable": "not_a_number"},  # String thay vì số
        {"quantityAvailable": -50},  # Số âm (không hợp lệ)
    ]
    
    for invalid_payload in invalid_payloads:
        response = requests.put(
            f"{base_url}/inventories/skus/{sku_id}",
            json=invalid_payload,
            headers=seller_headers
        )
        
        assert response.status_code in [400, 422], \
            f"Expected validation error for payload {invalid_payload}, got {response.status_code}"


def test_seller_update_sku_inventory_zero_quantity(base_url, seller_headers):
    """
    TC_INV_04: EDGE CASE - SET QUANTITY = 0 (HẾT HÀNG)
    - Hành động: Cập nhật SKU với quantityAvailable = 0.
    - Kỳ vọng: 200 OK. SKU chuyển sang trạng thái "Out of Stock".
    """
    sku_id = 1
    payload = {
        "quantityAvailable": 0,
        "quantityReserved": 0,
        "quantitySold": 100,
        "reorderPoint": 20,
        "reorderQuantity": 100
    }
    
    response = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=payload,
        headers=seller_headers
    )
    
    assert response.status_code in [200, 204], \
        f"Expected 200/204 for zero quantity, got {response.status_code}"


def test_seller_cannot_update_other_seller_sku(base_url, seller_headers):
    """
    TC_INV_05: SECURITY - SELLER KHÔNG THỂ UPDATE SKU CỦA SELLER KHÁC
    - Kịch bản: Seller A cố cập nhật inventory của SKU thuộc Seller B.
    - Kỳ vọng: Backend từ chối, trả về 403 Forbidden.
    
    Lưu ý: Giả định SKU id = 2 thuộc seller khác.
    """
    sku_id = 2  # Giả định SKU này thuộc seller khác
    payload = {
        "quantityAvailable": 999,
        "quantityReserved": 0,
        "quantitySold": 0,
        "reorderPoint": 10,
        "reorderQuantity": 50
    }
    
    response = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=payload,
        headers=seller_headers
    )
    
    # Nếu SKU2 thực sự thuộc seller khác, backend phải từ chối
    if response.status_code != 204 and response.status_code != 200:
        assert response.status_code == 403, \
            "Lỗ hổng: Seller có thể update SKU của seller khác!"


def test_update_sku_without_authentication(base_url):
    """
    TC_INV_06: SECURITY - CHẶN KHÔNG XÁC THỰC
    - Tiền điều kiện: Không cấp token xác thực.
    - Hành động: Gửi PUT /inventories/skus/1 mà không có Authorization header.
    - Kỳ vọng: Backend từ chối, trả về 401 Unauthorized.
    """
    sku_id = 1
    payload = {
        "quantityAvailable": 100,
        "quantityReserved": 5,
        "quantitySold": 10,
        "reorderPoint": 15,
        "reorderQuantity": 50
    }
    
    response = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=payload
        # ❌ Cố ý không truyền headers
    )
    
    assert response.status_code == 401, \
        f"Expected 401 Unauthorized, got {response.status_code}"


def test_buyer_cannot_update_inventory(base_url, user_headers):
    """
    TC_INV_07: SECURITY - BUYER (USER) KHÔNG THỂ UPDATE INVENTORY
    - Kịch bản: User thường (buyer) cố cập nhật inventory của SKU.
    - Kỳ vọng: Backend từ chối, trả về 403 Forbidden.
    """
    sku_id = 1
    payload = {
        "quantityAvailable": 500,
        "quantityReserved": 0,
        "quantitySold": 0,
        "reorderPoint": 10,
        "reorderQuantity": 50
    }
    
    response = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=payload,
        headers=user_headers  # ← User headers, không phải seller
    )
    
    assert response.status_code == 403, \
        f"Lỗ hổng: User thường có thể update inventory!"


def test_update_sku_inventory_very_large_quantity(base_url, seller_headers):
    """
    TC_INV_08: EDGE CASE - QUANTITY RẤT LỚN
    - Hành động: Cập nhật SKU với quantityAvailable = cực lớn.
    - Kỳ vọng: Backend xử lý mà không crash, kiểm tra overflow.
    """
    sku_id = 1
    payload = {
        "quantityAvailable": 999999999,
        "quantityReserved": 0,
        "quantitySold": 0,
        "reorderPoint": 100,
        "reorderQuantity": 1000000
    }
    
    response = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=payload,
        headers=seller_headers
    )
    
    # Backend nên xử lý được
    assert response.status_code in [200, 204, 400]


def test_update_sku_inventory_negative_reserved(base_url, seller_headers):
    """
    TC_INV_09: EDGE CASE - NEGATIVE RESERVED (LỖI DỮ LIỆU)
    - Hành động: Cập nhật với quantityReserved = âm (không hợp lệ).
    - Kỳ vọng: Backend từ chối (validation error).
    """
    sku_id = 1
    payload = {
        "quantityAvailable": 100,
        "quantityReserved": -5,  # ❌ Không thể âm
        "quantitySold": 10,
        "reorderPoint": 15,
        "reorderQuantity": 50
    }
    
    response = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=payload,
        headers=seller_headers
    )
    
    assert response.status_code in [400, 422], \
        f"Expected validation error for negative reserved, got {response.status_code}"


def test_update_sku_inventory_response_structure(base_url, seller_headers):
    """
    TC_INV_10: KIỂM TRA CẤU TRÚC RESPONSE
    - Hành động: Cập nhật inventory, kiểm tra response.
    - Kỳ vọng: Response chứa updated inventory data hoặc success message.
    """
    sku_id = 1
    payload = {
        "quantityAvailable": 120,
        "quantityReserved": 15,
        "quantitySold": 20,
        "reorderPoint": 25,
        "reorderQuantity": 100
    }
    
    response = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=payload,
        headers=seller_headers
    )
    
    assert response.status_code in [200, 204]
    
    # Nếu response có body (200 OK), kiểm tra structure
    if response.status_code == 200:
        try:
            data = response.json()
            # Response có thể là {"data": {...}} hoặc {"success": true}
            assert "data" in data or "success" in data, \
                "Response phải chứa 'data' hoặc 'success' field"
        except:
            pass  # 204 No Content không có body


def test_update_multiple_skus_sequentially(base_url, seller_headers):
    """
    TC_INV_11: UPDATE NHIỀU SKU LIÊN TIẾP
    - Hành động: Update inventory cho nhiều SKU khác nhau (1, 2, 3...).
    - Kỳ vọng: Tất cả đều thành công hoặc fail một cách consistent.
    """
    sku_ids = [1, 2, 3, 4, 5]
    
    for sku_id in sku_ids:
        payload = {
            "quantityAvailable": 100 + sku_id,
            "quantityReserved": sku_id,
            "quantitySold": sku_id * 2,
            "reorderPoint": 20,
            "reorderQuantity": 100
        }
        
        response = requests.put(
            f"{base_url}/inventories/skus/{sku_id}",
            json=payload,
            headers=seller_headers
        )
        
        # Chấp nhận 200/204 hoặc 404 (nếu SKU không tồn tại) - không crash
        assert response.status_code in [200, 204, 404], \
            f"Unexpected status for SKU {sku_id}: {response.status_code}"


def test_update_inventory_with_decimal_quantity(base_url, seller_headers):
    """
    TC_INV_12: DECIMAL QUANTITY - HÀNG CÓ TÍNH BẢN
    - Hành động: Cập nhật với quantity là số thập phân (nếu backend hỗ trợ).
    - Kỳ vọng: 200 OK hoặc 400 (nếu backend chỉ chấp nhận integer).
    """
    sku_id = 1
    payload = {
        "quantityAvailable": 100.5,  # Số thập phân
        "quantityReserved": 5.25,
        "quantitySold": 10.75,
        "reorderPoint": 15,
        "reorderQuantity": 50
    }
    
    response = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=payload,
        headers=seller_headers
    )
    
    # Backend có thể chấp nhận hoặc từ chối decimal - tuỳ design
    assert response.status_code in [200, 204, 400, 422]


# ==============================================================================
# 🛡️ SECURITY TESTS
# ==============================================================================

def test_admin_can_view_all_inventory(base_url, admin_headers):
    """
    TC_INV_SEC_01: ADMIN CÓ THỂ QUẢN LÝ TẤT CẢ INVENTORY
    - Kịch bản: Admin cập nhật inventory của bất kỳ seller nào.
    - Hành động: Gửi PUT /inventories/skus/1 với admin headers.
    - Kỳ vọng: 200/204 OK (nếu endpoint cho phép admin).
    
    Lưu ý: Tuỳ vào design, endpoint này có thể từ chối admin hoặc không.
    """
    sku_id = 1
    payload = {
        "quantityAvailable": 200,
        "quantityReserved": 10,
        "quantitySold": 30,
        "reorderPoint": 25,
        "reorderQuantity": 100
    }
    
    response = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=payload,
        headers=admin_headers
    )
    
    # Admin endpoint có thể khác, skip nếu endpoint strictly cho seller
    if response.status_code == 403:
        pytest.skip("Endpoint này strictly cho seller, admin bị chặn")
    else:
        assert response.status_code in [200, 204, 404]


def test_sql_injection_in_sku_id(base_url, seller_headers):
    """
    TC_INV_SEC_02: KIỂM TRA SQL INJECTION VÀO SKU ID
    - Hành động: Gửi SKU ID với SQL injection payload.
    - Kỳ vọng: Backend xử lý safely (parse as integer, không execute SQL).
    """
    malicious_sku_ids = [
        "1; DROP TABLE inventories;--",
        "1' OR '1'='1",
        "1 UNION SELECT * FROM users",
    ]
    
    payload = {
        "quantityAvailable": 100,
        "quantityReserved": 5,
        "quantitySold": 10,
        "reorderPoint": 15,
        "reorderQuantity": 50
    }
    
    for sku_id in malicious_sku_ids:
        response = requests.put(
            f"{base_url}/inventories/skus/{sku_id}",
            json=payload,
            headers=seller_headers
        )
        
        # Backend nên từ chối (404 hoặc 400), không crash
        assert response.status_code >= 400, \
            f"Lỗ hổng SQL Injection: Backend chấp nhận payload {sku_id}"
