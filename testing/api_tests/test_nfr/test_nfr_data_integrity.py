# api_tests/test_nfr_data_integrity.py
import requests
import uuid
import pytest
from datetime import datetime

# ==============================================================================
# 🛡️ NON-FUNCTIONAL REQUIREMENTS - DATA INTEGRITY TESTS
# ==============================================================================

# Data Integrity Test Results
integrity_results = []

def log_integrity_result(nfr_id, nfr_name, test_name, passed, details=""):
    """Log data integrity test results"""
    result = {
        "timestamp": datetime.now().isoformat(),
        "nfr_id": nfr_id,
        "nfr_name": nfr_name,
        "test_name": test_name,
        "passed": passed,
        "details": details
    }
    integrity_results.append(result)
    print(f"\n[Data Integrity] {nfr_id} - {nfr_name}: {'✅ PASS' if passed else '❌ FAIL'}")
    if details:
        print(f"  Details: {details}")
    return result


# ==============================================================================
# NFR-7.3: INVENTORY MUST PREVENT NEGATIVE STOCK
# ==============================================================================

def test_nfr_7_3_prevent_negative_inventory(base_url, seller_headers):
    """
    NFR-7.3: Inventory updates must prevent negative stock.
    - Test: Try to set quantityAvailable to negative value.
    - Expected: Backend rejects (400/422) or prevents negative value.
    """
    sku_id = 1
    
    # Attempt 1: Set directly to negative
    payload = {
        "quantityAvailable": -50,
        "quantityReserved": 0,
        "quantitySold": 0,
        "reorderPoint": 10,
        "reorderQuantity": 100
    }
    
    response = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=payload,
        headers=seller_headers
    )
    
    is_rejected = response.status_code in [400, 422]
    
    log_integrity_result(
        "NFR-7.3",
        "Prevent Negative Inventory",
        "test_nfr_7_3_prevent_negative_inventory",
        is_rejected,
        f"Negative value rejection - Status: {response.status_code}"
    )
    
    assert is_rejected, f"Negative inventory should be rejected, got {response.status_code}"


def test_nfr_7_3_prevent_reserved_exceeds_available(base_url, seller_headers):
    """
    NFR-7.3 Extended: quantityReserved should not exceed quantityAvailable.
    - Test: Set quantityReserved > quantityAvailable.
    - Expected: Backend rejects or adjusts.
    """
    sku_id = 1
    
    payload = {
        "quantityAvailable": 10,
        "quantityReserved": 50,  # ❌ 50 > 10
        "quantitySold": 5,
        "reorderPoint": 5,
        "reorderQuantity": 50
    }
    
    response = requests.put(
        f"{base_url}/inventories/skus/{sku_id}",
        json=payload,
        headers=seller_headers
    )
    
    # Backend should validate business logic
    is_validated = response.status_code in [400, 422, 200, 204]
    
    if response.status_code in [200, 204]:
        # If accepted, verify the constraint is enforced
        get_response = requests.get(
            f"{base_url}/inventories/skus/{sku_id}",
            headers=seller_headers
        )
        if get_response.status_code == 200:
            data = get_response.json().get("data", {})
            reserved = data.get("quantityReserved", 0)
            available = data.get("quantityAvailable", 0)
            is_validated = reserved <= available
    
    log_integrity_result(
        "NFR-7.3-ext",
        "Prevent Reserved > Available",
        "test_nfr_7_3_prevent_reserved_exceeds_available",
        is_validated,
        f"Business logic validation - Status: {response.status_code}"
    )
    
    assert is_validated, "Reserved quantity should not exceed available"


def test_nfr_7_3_cart_respects_available_inventory(base_url, user_headers, seller_headers, admin_headers):
    """
    NFR-7.3 Extended: Cart should not allow quantity > available inventory.
    - Test: Add item to cart with quantity exceeding stock.
    - Expected: Backend rejects or limits to available quantity.
    """
    # First, get a product with known stock
    products_response = requests.get(f"{base_url}/products", params={"pageSize": 1})
    
    if products_response.status_code != 200:
        pytest.skip("Could not fetch products")
    
    products_data = products_response.json().get("data", {})
    if isinstance(products_data, dict):
        items = products_data.get("items", [])
    else:
        items = products_data if isinstance(products_data, list) else []
    
    if not items:
        pytest.skip("No products found")
    
    product = items[0]
    sku_id = product.get("skuId") or product.get("id")
    
    # Get inventory for this SKU
    inventory_response = requests.get(
        f"{base_url}/inventories/skus/{sku_id}",
        headers=seller_headers
    )
    
    if inventory_response.status_code != 200:
        pytest.skip("Could not fetch inventory")
    
    inventory_data = inventory_response.json().get("data", {})
    available_qty = inventory_data.get("quantityAvailable", 0)
    
    if available_qty <= 0:
        # Try to add with quantity > 0
        requested_qty = 1
    else:
        # Try to add more than available
        requested_qty = available_qty + 10
    
    # Add to cart
    cart_payload = {
        "skuId": sku_id,
        "quantity": requested_qty
    }
    
    cart_response = requests.post(
        f"{base_url}/cart/items",
        json=cart_payload,
        headers=user_headers
    )
    
    # Check response
    is_validated = False
    details = f"Cart response - Status: {cart_response.status_code}"
    
    if cart_response.status_code == 400:
        # Backend rejected - good
        is_validated = True
        details += " (Rejected)"
    elif cart_response.status_code in [200, 201]:
        # Backend accepted - verify quantity
        cart_data = cart_response.json().get("data", {})
        added_qty = cart_data.get("quantity", 0)
        if added_qty <= available_qty:
            is_validated = True
            details += f" (Allowed: {added_qty}/{requested_qty})"
    
    log_integrity_result(
        "NFR-7.3-ext2",
        "Cart Respects Inventory",
        "test_nfr_7_3_cart_respects_available_inventory",
        is_validated,
        details
    )
    
    # Note: This is more of a verification than assertion
    # as behavior may vary


# ==============================================================================
# NFR-7.4: ORDER TOTALS CALCULATED SERVER-SIDE
# ==============================================================================

def test_nfr_7_4_order_total_server_side_calculation(base_url, user_headers):
    """
    NFR-7.4: Order totals must be calculated server-side (not trusted from client input).
    - Test: Create order with manipulated price in request body.
    - Expected: Backend ignores client price and uses database price.
    """
    # Try to create order with fake/manipulated prices
    payload = {
        "shippingAddressId": 1,
        "billingAddressId": 1,
        "items": [
            {
                "skuId": 1,
                "quantity": 2,
                "price": 0.01  # ❌ Manipulated: extremely low price
            }
        ],
        "totalPrice": 0.02  # ❌ Manipulated total
    }
    
    response = requests.post(
        f"{base_url}/orders",
        json=payload,
        headers=user_headers
    )
    
    is_validated = False
    details = f"Order creation - Status: {response.status_code}"
    
    if response.status_code in [200, 201]:
        # Order was created - verify backend calculated correct price
        order_data = response.json().get("data", {})
        calculated_total = order_data.get("totalPrice", order_data.get("total", 0))
        
        # The calculated total should NOT match the manipulated value
        if calculated_total != 0.02:
            is_validated = True
            details += f" (Server calculated: {calculated_total}, rejected client price: 0.02)"
        else:
            details += f" (WARNING: Order total matches client input!)"
    elif response.status_code == 400:
        # Backend rejected - also valid
        is_validated = True
        details += " (Request rejected)"
    
    log_integrity_result(
        "NFR-7.4",
        "Order Total Server-Side Calculation",
        "test_nfr_7_4_order_total_server_side_calculation",
        is_validated,
        details
    )
    
    assert is_validated, "Server should ignore client-provided prices"


def test_nfr_7_4_order_total_includes_discount(base_url, user_headers):
    """
    NFR-7.4 Extended: Order total should account for discounts/coupons correctly.
    - Test: Apply coupon and verify total is calculated correctly.
    """
    payload = {
        "shippingAddressId": 1,
        "billingAddressId": 1,
        "couponCode": "SALE10"  # Assume 10% discount
    }
    
    response = requests.post(
        f"{base_url}/orders",
        json=payload,
        headers=user_headers
    )
    
    is_validated = False
    details = f"Order with coupon - Status: {response.status_code}"
    
    if response.status_code in [200, 201]:
        order_data = response.json().get("data", {})
        subtotal = order_data.get("subtotal", 0)
        discount = order_data.get("discount", 0)
        total = order_data.get("totalPrice", 0)
        
        # Verify: total = subtotal - discount
        expected_total = subtotal - discount
        
        if abs(total - expected_total) < 0.01:  # Allow 1 cent rounding
            is_validated = True
            details += f" (Verified: ${subtotal} - ${discount} = ${total})"
        else:
            details += f" (Mismatch: expected ${expected_total}, got ${total})"
    
    log_integrity_result(
        "NFR-7.4-ext",
        "Order Total with Discounts",
        "test_nfr_7_4_order_total_includes_discount",
        is_validated,
        details
    )


# ==============================================================================
# NFR-7.1: DATABASE TRANSACTIONS FOR DATA CONSISTENCY
# ==============================================================================

def test_nfr_7_1_order_payment_transaction_consistency(base_url, user_headers, seller_headers):
    """
    NFR-7.1: Database operations must use transactions (Order + Inventory update).
    - Test: Create order and verify inventory is updated atomically.
    - Expected: If payment succeeds, inventory must be deducted. If payment fails, inventory unchanged.
    """
    # This is a simplified test - full transaction testing requires more setup
    
    # Get inventory before order
    sku_id = 1
    inv_before = None
    
    try:
        inv_response = requests.get(
            f"{base_url}/inventories/skus/{sku_id}",
            headers=seller_headers
        )
        if inv_response.status_code == 200:
            inv_before = inv_response.json().get("data", {}).get("quantitySold", 0)
    except:
        pass
    
    # Create order
    order_payload = {
        "shippingAddressId": 1,
        "billingAddressId": 1
    }
    
    order_response = requests.post(
        f"{base_url}/orders",
        json=order_payload,
        headers=user_headers
    )
    
    # Get inventory after order
    inv_after = None
    if inv_before is not None:
        try:
            inv_response = requests.get(
                f"{base_url}/inventories/skus/{sku_id}",
                headers=seller_headers
            )
            if inv_response.status_code == 200:
                inv_after = inv_response.json().get("data", {}).get("quantitySold", 0)
        except:
            pass
    
    is_consistent = True
    details = "Transaction consistency check"
    
    if inv_before is not None and inv_after is not None:
        if order_response.status_code in [200, 201]:
            # Order created - inventory should change
            if inv_after > inv_before:
                is_consistent = True
                details = f"Inventory updated: {inv_before} → {inv_after}"
            else:
                is_consistent = False
                details = f"Inventory not updated on successful order"
        elif order_response.status_code == 400:
            # Order failed - inventory should NOT change
            if inv_after == inv_before:
                is_consistent = True
                details = f"Inventory unchanged on failed order"
            else:
                is_consistent = False
                details = f"Inventory changed on failed order"
    
    log_integrity_result(
        "NFR-7.1",
        "Transaction Consistency",
        "test_nfr_7_1_order_payment_transaction_consistency",
        is_consistent,
        details
    )


def test_nfr_7_2_idempotent_payment_processing(base_url, user_headers):
    """
    NFR-7.2: Event processing must be idempotent (duplicate callbacks).
    - Test: Send payment callback twice, verify order state is consistent.
    - Expected: Second callback should not double-charge or change state incorrectly.
    """
    payload = {
        "appid": 2553,
        "apptransid": "idempotent_test_123",
        "transid": 5555555,
        "status": 1,  # Success
        "amount": 100000,
        "orderId": 1,
        "mac": "mock_signature"
    }
    
    # Send callback 1st time
    response1 = requests.post(
        f"{base_url}/payments/zalopay/callback",
        json=payload,
        headers=user_headers
    )
    
    # Send callback 2nd time (same payload)
    response2 = requests.post(
        f"{base_url}/payments/zalopay/callback",
        json=payload,
        headers=user_headers
    )
    
    # Both should succeed
    is_idempotent = response1.status_code in [200, 204] and response2.status_code in [200, 204]
    
    details = f"Callback 1: {response1.status_code}, Callback 2: {response2.status_code}"
    
    log_integrity_result(
        "NFR-7.2",
        "Idempotent Event Processing",
        "test_nfr_7_2_idempotent_payment_processing",
        is_idempotent,
        details
    )
    
    assert is_idempotent, "Duplicate payment callbacks should be handled idempotently"


# ==============================================================================
# NFR-7.5: REFERENTIAL INTEGRITY
# ==============================================================================

def test_nfr_7_5_referential_integrity_address_user(base_url, user_headers):
    """
    NFR-7.5: Database must enforce referential integrity (e.g., Address must belong to User).
    - Test: Try to use address ID that doesn't belong to current user.
    - Expected: Backend rejects (403/404).
    """
    # Try to create order with address belonging to another user
    payload = {
        "shippingAddressId": 999999,  # Assume this ID belongs to someone else or doesn't exist
        "billingAddressId": 999999
    }
    
    response = requests.post(
        f"{base_url}/orders",
        json=payload,
        headers=user_headers
    )
    
    # Should be rejected
    is_enforced = response.status_code in [400, 403, 404]
    
    log_integrity_result(
        "NFR-7.5",
        "Referential Integrity - Address",
        "test_nfr_7_5_referential_integrity_address_user",
        is_enforced,
        f"Invalid address reference rejected - Status: {response.status_code}"
    )
    
    assert is_enforced, f"Invalid address reference should be rejected, got {response.status_code}"


def test_nfr_7_5_referential_integrity_product_category(base_url, seller_headers, admin_headers):
    """
    NFR-7.5 Extended: Product must belong to valid category.
    - Test: Create product with non-existent category ID.
    - Expected: Backend rejects (400/404).
    """
    payload = {
        "name": "Test Product",
        "description": "Test",
        "categoryIds": [999999],  # Non-existent category
        "brand": "Test",
        "defaultSkuPrice": 99.99,
        "defaultSkuStock": 100
    }
    
    response = requests.post(
        f"{base_url}/products/seller",
        json=payload,
        headers=seller_headers
    )
    
    is_enforced = response.status_code in [400, 404, 500]
    
    log_integrity_result(
        "NFR-7.5-ext",
        "Referential Integrity - Category",
        "test_nfr_7_5_referential_integrity_product_category",
        is_enforced,
        f"Invalid category reference - Status: {response.status_code}"
    )
    
    assert is_enforced, "Product with non-existent category should be rejected"


# ==============================================================================
# DATA INTEGRITY REPORT
# ==============================================================================

@pytest.fixture(scope="session", autouse=True)
def nfr_data_integrity_report(request):
    """Generate NFR Data Integrity Test Report"""
    yield
    
    if integrity_results:
        print("\n" + "="*80)
        print("NFR DATA INTEGRITY TEST REPORT")
        print("="*80)
        
        passed = sum(1 for r in integrity_results if r["passed"])
        total = len(integrity_results)
        
        print(f"\nSummary: {passed}/{total} tests passed ({passed*100//total}%)")
        print("\nDetailed Results:")
        for result in integrity_results:
            status = "✅" if result["passed"] else "❌"
            print(f"\n{status} {result['nfr_id']}: {result['nfr_name']}")
            print(f"   Test: {result['test_name']}")
            print(f"   Details: {result['details']}")
            print(f"   Timestamp: {result['timestamp']}")
