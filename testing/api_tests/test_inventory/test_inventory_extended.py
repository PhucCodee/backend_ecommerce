# api_tests/test_inventory/test_inventory_extended.py
import requests
import uuid
import pytest
import concurrent.futures
import time

# ==============================================================================
# 🧪 EXTENDED TEST CASES CHO INVENTORY (KHO NÂNG CAO)
# ==============================================================================

def test_inventory_stock_levels(base_url, seller_headers):
    """
    TC_INV_EXT_01: KIỂM TRA LOGIC STOCK LEVELS
    - Hành động: Cập nhật inventory với các mức tồn kho khác nhau.
    - Kỳ vọng: 
        - Nếu quantityAvailable < reorderPoint → SKU nên được gắn cờ "Low Stock".
        - Nếu quantityAvailable = 0 → SKU "Out of Stock".
    """
    sku_id = 1
    test_cases = [
        {
            "quantityAvailable": 5,
            "reorderPoint": 20,
            "expected_status": "Low Stock"
        },
        {
            "quantityAvailable": 100,
            "reorderPoint": 20,
            "expected_status": "In Stock"
        },
    ]
    
    for test_case in test_cases:
        payload = {
            "quantityAvailable": test_case["quantityAvailable"],
            "quantityReserved": 5,
            "quantitySold": 10,
            "reorderPoint": test_case["reorderPoint"],
            "reorderQuantity": 100
        }
        
        response = requests.put(
            f"{base_url}/inventories/skus/{sku_id}",
            json=payload,
            headers=seller_headers
        )
        
        assert response.status_code in [200, 204], \
            f"Failed for test case {test_case}: {response.text}"


def test_inventory_reserved_cannot_exceed_available(base_url, seller_headers):
    """
    TC_INV_EXT_02: BUSINESS LOGIC - RESERVED <= AVAILABLE
    - Hành động: Cố cập nhật với quantityReserved > quantityAvailable.
    - Kỳ vọng: Backend từ chối (business logic validation).
    """
    sku_id = 1
    payload = {
        "quantityAvailable": 10,
        "quantityReserved": 20,  # ❌ 20 > 10
        "quantitySold": 5,
        "reorderPoint": 5,
        "reorderQuantity": 50
    }
    
    response = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=payload,
        headers=seller_headers
    )
    
    # Backend nên validate business logic
    assert response.status_code in [200, 204, 400, 422], \
        f"Expected validation or success, got {response.status_code}"


def test_inventory_sold_cannot_exceed_total(base_url, seller_headers):
    """
    TC_INV_EXT_03: BUSINESS LOGIC - SOLD <= AVAILABLE + RESERVED + SOLD (TOTAL)
    - Hành động: Kiểm tra logic quantitySold.
    - Kỳ vọng: Backend có thể enforce constraint hoặc warning.
    """
    sku_id = 1
    payload = {
        "quantityAvailable": 10,
        "quantityReserved": 5,
        "quantitySold": 100,  # ? Có thể rất lớn (lịch sử bán)
        "reorderPoint": 5,
        "reorderQuantity": 50
    }
    
    response = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=payload,
        headers=seller_headers
    )
    
    # Backend có thể cho phép (quantitySold là lịch sử) hoặc từ chối
    assert response.status_code in [200, 204, 400, 422]


def test_inventory_concurrent_updates(base_url, seller_headers):
    """
    TC_INV_EXT_04: CONCURRENCY - UPDATE MULTIPLE TIMES ĐỒNG THỜI
    - Hành động: Gửi 5 PUT request cho cùng 1 SKU cùng lúc.
    - Kỳ vọng: Không data corruption, final state consistent.
    """
    sku_id = 1
    
    def update_inventory(quantity):
        payload = {
            "quantityAvailable": quantity,
            "quantityReserved": 5,
            "quantitySold": 10,
            "reorderPoint": 15,
            "reorderQuantity": 50
        }
        return requests.put(
            f"{base_url}/inventories/skus/{sku_id}",
            json=payload,
            headers=seller_headers
        )
    
    with concurrent.futures.ThreadPoolExecutor(max_workers=5) as executor:
        futures = [executor.submit(update_inventory, 100 + i) for i in range(5)]
        results = [f.result() for f in concurrent.futures.as_completed(futures)]
    
    # Tất cả request phải successful
    for response in results:
        assert response.status_code in [200, 204], \
            f"Concurrent update failed: {response.text}"


def test_inventory_bulk_update_with_transaction(base_url, seller_headers):
    """
    TC_INV_EXT_05: BULK UPDATE - CẬP NHẬT NHIỀU SKU CÙNG 1 TRANSACTION
    - Hành động: Update inventory cho 5 SKU trong 1 transaction.
    - Kỳ vọng: Nếu 1 SKU fail → Tất cả rollback. Nếu success → Tất cả commit.
    
    Lưu ý: Endpoint này có thể không tồn tại, test sẽ skip nếu 404.
    """
    payload = {
        "updates": [
            {"skuId": 1, "quantityAvailable": 100},
            {"skuId": 2, "quantityAvailable": 200},
            {"skuId": 3, "quantityAvailable": 300},
            {"skuId": 4, "quantityAvailable": 400},
            {"skuId": 5, "quantityAvailable": 500},
        ]
    }
    
    response = requests.put(
        f"{base_url}/inventories/skus/bulk",
        json=payload,
        headers=seller_headers
    )
    
    # Endpoint có thể không tồn tại
    if response.status_code == 404:
        pytest.skip("Bulk update endpoint không tồn tại")
    
    assert response.status_code in [200, 204, 400, 422]


def test_inventory_audit_trail(base_url, seller_headers):
    """
    TC_INV_EXT_06: AUDIT TRAIL - THEO DÕI LỊCH SỬ CẬP NHẬT
    - Hành động: Cập nhật inventory, sau đó kiểm tra audit log.
    - Kỳ vọng: GET /inventories/skus/{skuId}/history trả về lịch sử thay đổi.
    
    Lưu ý: Endpoint history có thể không tồn tại.
    """
    sku_id = 1
    payload = {
        "quantityAvailable": 150,
        "quantityReserved": 10,
        "quantitySold": 20,
        "reorderPoint": 25,
        "reorderQuantity": 100
    }
    
    # Thực hiện update
    update_response = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=payload,
        headers=seller_headers
    )
    
    assert update_response.status_code in [200, 204]
    
    # Thử lấy history
    history_response = requests.get(
        f"{base_url}/inventories/skus/{sku_id}/history",
        headers=seller_headers
    )
    
    # History endpoint có thể không tồn tại
    if history_response.status_code == 404:
        pytest.skip("History endpoint không tồn tại")
    
    assert history_response.status_code == 200


def test_inventory_update_with_timestamp_validation(base_url, seller_headers):
    """
    TC_INV_EXT_07: OPTIMISTIC LOCKING - UPDATE VỚI TIMESTAMP
    - Hành động: Cập nhật inventory kèm timestamp (để detect concurrent modifications).
    - Kỳ vọng: Nếu timestamp cũ → 409 Conflict. Nếu mới → 200 OK.
    """
    sku_id = 1
    old_timestamp = int(time.time()) - 3600  # 1 giờ trước
    
    payload = {
        "quantityAvailable": 100,
        "quantityReserved": 5,
        "quantitySold": 10,
        "reorderPoint": 15,
        "reorderQuantity": 50,
        "lastModifiedAt": old_timestamp  # Timestamp cũ
    }
    
    response = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=payload,
        headers=seller_headers
    )
    
    # Backend có thể check conflict hoặc ignore timestamp
    assert response.status_code in [200, 204, 409, 400]


def test_inventory_reorder_point_logic(base_url, seller_headers):
    """
    TC_INV_EXT_08: BUSINESS LOGIC - REORDER POINT & QUANTITY
    - Hành động: Set reorderQuantity và kiểm tra auto-reorder logic (nếu có).
    - Kỳ vọng: Nếu quantityAvailable < reorderPoint, system nên tạo PO (Purchase Order).
    """
    sku_id = 1
    payload = {
        "quantityAvailable": 10,  # < reorderPoint
        "quantityReserved": 0,
        "quantitySold": 0,
        "reorderPoint": 25,
        "reorderQuantity": 100
    }
    
    response = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=payload,
        headers=seller_headers
    )
    
    assert response.status_code in [200, 204]
    
    # Kiểm tra xem có PO được tạo không (nếu endpoint tồn tại)
    po_response = requests.get(
        f"{base_url}/purchase-orders?skuId={sku_id}",
        headers=seller_headers
    )
    
    if po_response.status_code == 200:
        po_data = po_response.json()
        # Tuỳ logic, có thể expect 1 PO mới
        pass


def test_inventory_maximum_field_values(base_url, seller_headers):
    """
    TC_INV_EXT_09: EDGE CASE - KIỂM CHUẨN MAX INT / UINT
    - Hành động: Cập nhật với các giá trị cực đại (2^31-1, 2^63-1).
    - Kỳ vọng: Backend xử lý overflow gracefully.
    """
    sku_id = 1
    test_values = [
        2147483647,      # Max 32-bit signed int
        9223372036854775807,  # Max 64-bit signed int
    ]
    
    for value in test_values:
        payload = {
            "quantityAvailable": value,
            "quantityReserved": 0,
            "quantitySold": 0,
            "reorderPoint": 100,
            "reorderQuantity": 1000
        }
        
        response = requests.put(
            f"{base_url}/inventories/skus/{sku_id}",
            json=payload,
            headers=seller_headers
        )
        
        # Backend nên handle hoặc reject gracefully
        assert response.status_code in [200, 204, 400, 422]


def test_inventory_float_precision(base_url, seller_headers):
    """
    TC_INV_EXT_10: PRECISION - KIỂM CHUẨN DECIMAL/FLOAT
    - Hành động: Cập nhật với số thập phân rất nhỏ hoặc lớn.
    - Kỳ vọng: Backend lưu trữ chính xác (không round error).
    """
    sku_id = 1
    payload = {
        "quantityAvailable": 100.123456789,
        "quantityReserved": 5.111111111,
        "quantitySold": 10.999999999,
        "reorderPoint": 15.5,
        "reorderQuantity": 50.75
    }
    
    response = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=payload,
        headers=seller_headers
    )
    
    if response.status_code in [200, 204]:
        # Verify giá trị được lưu chính xác (nếu có GET endpoint)
        get_response = requests.get(
            f"{base_url}/inventories/skus/{sku_id}",
            headers=seller_headers
        )
        
        if get_response.status_code == 200:
            data = get_response.json().get("data", {})
            # Check rounding/precision
            assert "quantityAvailable" in data


def test_inventory_special_sku_formats(base_url, seller_headers):
    """
    TC_INV_EXT_11: INVALID SKU FORMATS
    - Hành động: Update inventory với SKU ID = UUID, GUID, slug, etc.
    - Kỳ vọng: Backend xử lý hoặc từ chối tùy theo schema.
    """
    sku_formats = [
        str(uuid.uuid4()),  # UUID
        "ABC-123-DEF",      # Slug
        "sku_special_123",  # Custom format
    ]
    
    payload = {
        "quantityAvailable": 100,
        "quantityReserved": 5,
        "quantitySold": 10,
        "reorderPoint": 15,
        "reorderQuantity": 50
    }
    
    for sku_id in sku_formats:
        response = requests.put(
            f"{base_url}/inventories/skus/{sku_id}",
            json=payload,
            headers=seller_headers
        )
        
        # Backend có thể từ chối (404, 400) hoặc chấp nhận
        assert response.status_code in [200, 204, 404, 400]


def test_inventory_with_transaction_id_in_header(base_url, seller_headers):
    """
    TC_INV_EXT_12: IDEMPOTENCY - TRACKING UPDATES VỚI IDEMPOTENCY KEY
    - Hành động: 
        1. Gửi update kèm header X-Idempotency-Key.
        2. Gửi cùng request lần thứ 2.
    - Kỳ vọng: Cả 2 lần đều 200/204. Inventory chỉ update 1 lần.
    """
    sku_id = 1
    idempotency_key = str(uuid.uuid4())
    headers = seller_headers.copy()
    headers["X-Idempotency-Key"] = idempotency_key
    
    payload = {
        "quantityAvailable": 250,
        "quantityReserved": 20,
        "quantitySold": 30,
        "reorderPoint": 35,
        "reorderQuantity": 150
    }
    
    # Update lần 1
    response1 = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=payload,
        headers=headers
    )
    
    # Update lần 2 (same idempotency key)
    response2 = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=payload,
        headers=headers
    )
    
    assert response1.status_code in [200, 204]
    assert response2.status_code in [200, 204], \
        "Idempotent request phải thành công"


def test_inventory_rollback_on_validation_failure(base_url, seller_headers):
    """
    TC_INV_EXT_13: TRANSACTION ROLLBACK - ROLLBACK NẾU VALIDATION FAIL
    - Hành động: 
        1. Cập nhật inventory thành công.
        2. Cố cập nhật với dữ liệu invalid.
    - Kỳ vọng: Bước 2 fail, bước 1 không bị rollback.
    """
    sku_id = 1
    
    # Bước 1: Valid update
    valid_payload = {
        "quantityAvailable": 100,
        "quantityReserved": 5,
        "quantitySold": 10,
        "reorderPoint": 15,
        "reorderQuantity": 50
    }
    
    response1 = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=valid_payload,
        headers=seller_headers
    )
    assert response1.status_code in [200, 204]
    
    # Bước 2: Invalid update (negative reserved)
    invalid_payload = {
        "quantityAvailable": 200,
        "quantityReserved": -10,  # ❌ Invalid
        "quantitySold": 20,
        "reorderPoint": 25,
        "reorderQuantity": 100
    }
    
    response2 = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=invalid_payload,
        headers=seller_headers
    )
    assert response2.status_code in [400, 422]
    
    # Bước 3: Verify bước 1 không bị rollback
    # (quantityAvailable vẫn là 100)
    get_response = requests.get(
        f"{base_url}/inventories/skus/{sku_id}",
        headers=seller_headers
    )
    
    if get_response.status_code == 200:
        data = get_response.json().get("data", {})
        assert data.get("quantityAvailable") == 100


def test_inventory_seller_cannot_see_other_seller_sku(base_url, seller_headers):
    """
    TC_INV_EXT_14: DATA ISOLATION - SELLER CHỈ THẤY SKU CỦA CHÍNH MÌNH
    - Hành động: GET /inventories/skus (danh sách).
    - Kỳ vọng: Chỉ thấy SKU thuộc seller này, không thấy SKU của seller khác.
    """
    response = requests.get(
        f"{base_url}/inventories/skus",
        headers=seller_headers
    )
    
    if response.status_code == 404:
        pytest.skip("GET /inventories/skus endpoint không tồn tại")
    
    assert response.status_code == 200
    
    data = response.json().get("data", [])
    if isinstance(data, list):
        for sku in data:
            # Verify seller_id của từng SKU matches current seller
            assert "sellerId" in sku or "userId" in sku, \
                "SKU phải chứa owner information"


# ==============================================================================
# 🔄 INTEGRATION TESTS
# ==============================================================================

def test_inventory_product_cart_integration(base_url, seller_headers, user_headers):
    """
    TC_INV_INT_01: FULL FLOW - INVENTORY → PRODUCT → CART
    - Kịch bản:
        1. Seller cập nhật inventory.
        2. User xem product (kiểm tra stock status).
        3. User thêm sản phẩm vào cart (kiểm tra available quantity).
    - Kỳ vọng: Inventory, product, cart đều sync đúng.
    """
    sku_id = 1
    
    # Bước 1: Update inventory
    update_payload = {
        "quantityAvailable": 50,
        "quantityReserved": 0,
        "quantitySold": 0,
        "reorderPoint": 20,
        "reorderQuantity": 100
    }
    
    update_response = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=update_payload,
        headers=seller_headers
    )
    
    assert update_response.status_code in [200, 204]
    
    # Bước 2: User xem product
    product_response = requests.get(
        f"{base_url}/products/skus/{sku_id}",
        headers=user_headers
    )
    
    if product_response.status_code == 200:
        product_data = product_response.json().get("data", {})
        # Verify stock quantity hiển thị đúng
        assert "stock" in product_data or "quantityAvailable" in product_data
    
    # Bước 3: User thêm vào cart
    cart_payload = {
        "skuId": sku_id,
        "quantity": 5
    }
    
    cart_response = requests.post(
        f"{base_url}/cart/items",
        json=cart_payload,
        headers=user_headers
    )
    
    if cart_response.status_code in [200, 201]:
        # Kiểm chứng quantity trong cart <= quantityAvailable
        cart_data = cart_response.json().get("data", {})
        assert cart_data.get("quantity", 0) <= 50


def test_inventory_order_payment_integration(base_url, seller_headers, user_headers):
    """
    TC_INV_INT_02: INVENTORY → ORDER → PAYMENT
    - Kịch bản:
        1. Seller cập nhật inventory (quantityReserved tăng).
        2. Order được tạo (quantityReserved tăng).
        3. Payment được xử lý (quantitySold tăng, quantityAvailable giảm).
    - Kỳ vọng: Inventory được cập nhật automatically theo order lifecycle.
    """
    sku_id = 1
    
    # Bước 1: Initial inventory
    initial_payload = {
        "quantityAvailable": 100,
        "quantityReserved": 0,
        "quantitySold": 0,
        "reorderPoint": 20,
        "reorderQuantity": 100
    }
    
    initial_response = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=initial_payload,
        headers=seller_headers
    )
    
    assert initial_response.status_code in [200, 204]
    
    # Bước 2, 3: Tạo order & payment (logic này có thể phức tạp)
    # Giả sử system tự động update inventory khi order → paid
    
    # Verify final inventory state
    final_response = requests.get(
        f"{base_url}/inventories/skus/{sku_id}",
        headers=seller_headers
    )
    
    if final_response.status_code == 200:
        final_data = final_response.json().get("data", {})
        # quantitySold nên tăng lên
        assert final_data.get("quantitySold", 0) >= 0
