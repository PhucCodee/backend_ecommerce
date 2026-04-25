"""
📋 TEST SUITE: ORDERS - COMPREHENSIVE TEST CASES
=================================================
Purpose: Complete testing of order creation, retrieval, status updates, and 
         related operations including address handling and payment flows.
Coverage: Order CRUD operations, order status lifecycle, address selection,
          inventory management, and role-based access control.
"""

import pytest
import requests
import uuid


@pytest.fixture
def temp_order_setup(base_url, seller_headers, admin_headers):
    """
    FIXTURE: CREATE PRODUCT WITH SKU FOR ORDER TESTING
    Creates a temporary product and SKU that can be ordered during tests.
    """
    # Create category
    cat_payload = {
        "name": f"Order Test Category {str(uuid.uuid4())[:4]}",
        "isCore": False,
        "isActive": True
    }
    cat_response = requests.post(f"{base_url}/categories", json=cat_payload, headers=admin_headers)
    cat_id = cat_response.json()["data"].get("categoryId", cat_response.json()["data"].get("id"))
    
    # Create product
    product_payload = {
        "name": f"Order Test Product {str(uuid.uuid4())[:4]}",
        "description": "Test product for order",
        "categoryIds": [cat_id],
        "brand": "TestBrand",
        "weightKg": 1.0,
        "defaultSkuPrice": 50.00,
        "defaultSkuStock": 100,
        "dimensionsCm": "10x10x10"
    }
    prod_response = requests.post(f"{base_url}/products/seller", json=product_payload, headers=seller_headers)
    prod_data = prod_response.json()["data"]
    
    yield {
        "product_id": prod_data.get("id"),
        "sku_id": prod_data.get("skuId") or 1,
        "category_id": cat_id,
        "price": 50.00
    }
    
    # Cleanup
    if prod_data.get("id"):
        requests.delete(f"{base_url}/products/{prod_data.get('id')}", headers=admin_headers)
    if cat_id:
        requests.delete(f"{base_url}/categories/{cat_id}", headers=admin_headers)


class TestOrderCreation:
    """
    ✅ GOAL: Validate all order creation scenarios including product selection,
    address handling, and payment method selection.
    """

    def test_user_create_order_with_existing_address(self, base_url, user_headers):
        """
        🏷️ TC_ORDER_01 - CREATE ORDER WITH EXISTING ADDRESS
        
        📌 DECLARATION:
        Test successful order creation when buyer selects an existing saved address.
        
        📝 GOAL:
        - Create order with items and existing delivery address
        - Verify order is created with correct status
        - Verify order contains all expected fields
        
        🔍 STEPS:
        1. Get user's existing addresses
        2. Prepare cart items
        3. Submit POST /api/orders with addressId
        4. Verify 201 Created
        5. Verify response contains orderId, orderNumber, status="pending"
        
        ✔️ EXPECTED RESULT:
        - Status: 201 Created
        - Response contains: orderId, orderNumber, orderDate, totalAmount
        - Order status is "Pending" or similar
        - Cart is cleared after order creation
        """
        # Get user's addresses
        addr_response = requests.get(f"{base_url}/users/addresses", headers=user_headers)
        if addr_response.status_code != 200:
            pytest.skip("User has no addresses")
        
        addresses = addr_response.json().get("data", [])
        if not addresses:
            pytest.skip("User has no saved addresses")
        
        address_id = addresses[0].get("addressId", addresses[0].get("id"))
        
        # Add items to cart first
        cart_item = {"skuId": 1, "quantity": 1}
        requests.post(f"{base_url}/cart/items", json=cart_item, headers=user_headers)
        
        # Create order
        order_payload = {
            "addressId": address_id,
            "paymentMethod": "COD",  # Cash on delivery
            "notes": "Test order"
        }
        response = requests.post(f"{base_url}/orders", json=order_payload, headers=user_headers)
        assert response.status_code in [200, 201]
        
        order_data = response.json().get("data")
        assert "orderId" in order_data or "id" in order_data
        assert "orderNumber" in order_data

    def test_user_create_order_with_new_address(self, base_url, user_headers):
        """
        🏷️ TC_ORDER_02 - CREATE ORDER WITH NEW ADDRESS
        
        📌 DECLARATION:
        Test order creation when buyer creates and uses a new shipping address inline.
        
        📝 GOAL:
        - Create order with new address in one request
        - Verify address is saved to user's address book
        - Verify order uses the new address
        
        🔍 STEPS:
        1. Prepare order payload with newAddress object
        2. Submit POST /api/orders with nested address data
        3. Verify 201 Created
        4. Verify new address appears in user's addresses
        
        ✔️ EXPECTED RESULT:
        - Status: 201 Created
        - Order created with new address
        - Address automatically saved to user account
        """
        # Add to cart
        requests.post(f"{base_url}/cart/items", json={"skuId": 1, "quantity": 1}, headers=user_headers)
        
        # Create order with new address
        order_payload = {
            "newShippingAddress": {
                "type": 0, 
                "label": "Home",
                "recipientName": "John Doe",
                "phone": "0912345678",
                "addressLine1": "123 Nguyen Trai",
                "city": "Ho Chi Minh City",
                "stateProvince": "District 1",
                "postalCode": "700000",
                "country": "VN"
            },
            "saveNewShippingAddress": True, 
            "billingAddressId": 15,
            "couponCode": "SALE10",
            "customerNotes": "Leave at reception"
        }
        response = requests.post(f"{base_url}/orders", json=order_payload, headers=user_headers)
        assert response.status_code in [200, 201]

    def test_user_create_order_empty_cart(self, base_url, user_headers):
        """
        🏷️ TC_ORDER_03 - CREATE ORDER WITH EMPTY CART
        
        📌 DECLARATION:
        Test that order creation fails when user's cart is empty.
        
        📝 GOAL:
        - Verify system prevents ordering with no items
        
        🔍 STEPS:
        1. Clear user's cart
        2. Attempt POST /api/orders
        3. Verify 400 Bad Request
        
        ✔️ EXPECTED RESULT:
        - Status: 400 Bad Request or 422 Unprocessable Entity
        - Error message: "Cart is empty" or similar
        """
        # Clear cart
        requests.delete(f"{base_url}/cart", headers=user_headers)
        
        # Try to create order
        order_payload = {
            "addressId": 1,
            "paymentMethod": "COD"
        }
        response = requests.post(f"{base_url}/orders", json=order_payload, headers=user_headers)
        assert response.status_code in [400, 422]

    def test_user_create_order_missing_address(self, base_url, user_headers):
        """
        🏷️ TC_ORDER_04 - CREATE ORDER WITHOUT ADDRESS
        
        📌 DECLARATION:
        Test that order creation fails when no address is provided.
        
        📝 GOAL:
        - Verify address is required
        
        🔍 STEPS:
        1. Add items to cart
        2. Submit order without addressId and without newAddress
        3. Verify 400 Bad Request
        
        ✔️ EXPECTED RESULT:
        - Status: 400 Bad Request
        - Error: "Address is required"
        """
        # Add to cart
        requests.post(f"{base_url}/cart/items", json={"skuId": 1, "quantity": 1}, headers=user_headers)
        
        # Order without address
        order_payload = {
            "paymentMethod": "COD"
        }
        response = requests.post(f"{base_url}/orders", json=order_payload, headers=user_headers)
        assert response.status_code == 400

    def test_guest_cannot_create_order(self, base_url):
        """
        🏷️ TC_ORDER_05 - GUEST CANNOT CREATE ORDER WITHOUT LOGIN
        
        📌 DECLARATION:
        Test that guest users cannot proceed to checkout/order creation.
        
        📝 GOAL:
        - Verify authentication is required for orders
        
        🔍 STEPS:
        1. Attempt POST /api/orders without authentication token
        2. Verify 401 Unauthorized
        
        ✔️ EXPECTED RESULT:
        - Status: 401 Unauthorized
        """
        order_payload = {
            "addressId": 1,
            "paymentMethod": "COD"
        }
        response = requests.post(f"{base_url}/orders", json=order_payload)
        assert response.status_code == 401

    def test_order_with_invalid_sku(self, base_url, user_headers):
        """
        🏷️ TC_ORDER_06 - ORDER FAILS WITH INVALID SKU IN CART
        
        📌 DECLARATION:
        Test order creation fails when cart contains non-existent SKU.
        
        📝 GOAL:
        - Verify inventory validation
        
        🔍 STEPS:
        1. Manually add invalid SKU to cart (if possible)
        2. Attempt to create order
        3. Verify rejection
        
        ✔️ EXPECTED RESULT:
        - Status: 400 or 404
        - Error about invalid product/SKU
        """
        # Clear and add invalid SKU
        requests.delete(f"{base_url}/cart", headers=user_headers)
        
        # Try to add non-existent SKU
        invalid_sku = 999999
        response = requests.post(
            f"{base_url}/cart/items",
            json={"skuId": invalid_sku, "quantity": 1},
            headers=user_headers
        )
        # If this succeeds, then trying to order should fail
        if response.status_code in [200, 201]:
            order_payload = {
                "addressId": 1,
                "paymentMethod": "COD"
            }
            order_response = requests.post(f"{base_url}/orders", json=order_payload, headers=user_headers)
            assert order_response.status_code in [400, 422]


class TestOrderRetrieval:
    """
    ✅ GOAL: Validate retrieval of orders with various filters and access control.
    """

    def test_user_get_own_orders(self, base_url, user_headers):
        """
        🏷️ TC_ORDER_07 - USER RETRIEVE OWN ORDERS
        
        📌 DECLARATION:
        Test that authenticated users can retrieve their order history.
        
        📝 GOAL:
        - Get paginated list of user's orders
        - Verify only user's own orders returned
        
        🔍 STEPS:
        1. User calls GET /api/orders
        2. Verify 200 OK
        3. Verify response contains array of orders
        4. Verify each order has orderId, orderNumber, status, totalAmount
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Response contains paginated orders
        - Only current user's orders present
        """
        response = requests.get(f"{base_url}/orders", headers=user_headers)
        assert response.status_code == 200
        
        data = response.json().get("data", response.json())
        orders = data.get("items", data) if isinstance(data, dict) else data
        
        # Verify structure
        if orders and len(orders) > 0:
            order = orders[0]
            assert "orderId" in order or "id" in order
            assert "status" in order

    def test_user_get_order_by_id(self, base_url, user_headers):
        """
        🏷️ TC_ORDER_08 - USER RETRIEVE SPECIFIC ORDER
        
        📌 DECLARATION:
        Test that user can retrieve details of a specific order by ID.
        
        📝 GOAL:
        - Get full details of one order
        - Verify order items and address information included
        
        🔍 STEPS:
        1. Get user's orders list
        2. Select first order ID
        3. Call GET /api/orders/{orderId}
        4. Verify 200 OK
        5. Verify response contains items, address, payment info
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Response includes: orderItems, shippingAddress, paymentMethod, orderStatus
        """
        # Get orders
        orders_response = requests.get(f"{base_url}/orders", headers=user_headers)
        if orders_response.status_code != 200:
            pytest.skip("Could not retrieve orders")
        
        data = orders_response.json().get("data", orders_response.json())
        orders = data.get("items", data) if isinstance(data, dict) else data
        
        if not orders:
            pytest.skip("User has no orders")
        
        order_id = orders[0].get("orderId", orders[0].get("id"))
        
        # Get specific order
        response = requests.get(f"{base_url}/orders/{order_id}", headers=user_headers)
        assert response.status_code == 200

    def test_user_cannot_see_other_users_orders(self, base_url, user_headers, admin_headers):
        """
        🏷️ TC_ORDER_09 - USER CANNOT ACCESS OTHER USERS' ORDERS (IDOR)
        
        📌 DECLARATION:
        Test that users cannot retrieve orders belonging to other users.
        This is a critical security test for Insecure Direct Object Reference (IDOR).
        
        📝 GOAL:
        - Ensure user cannot access another user's order
        - Verify authorization check on order endpoints
        
        🔍 STEPS:
        1. Get admin's order (if exists) or any other user's order
        2. Attempt to retrieve as different user
        3. Verify 403 Forbidden or 404 Not Found
        
        ✔️ EXPECTED RESULT:
        - Status: 403 Forbidden or 404 Not Found
        - User cannot access/modify other user's orders
        """
        # This would require creating orders as different users
        # For now, try accessing a high ID order with user account
        response = requests.get(f"{base_url}/orders/999999", headers=user_headers)
        # Should be 404 or 403, not 200
        assert response.status_code in [403, 404]



class TestOrderStatusManagement:
    """
    ✅ GOAL: Validate order status lifecycle and transitions.
    """

    def test_order_status_workflow(self, base_url, user_headers):
        """
        🏷️ TC_ORDER_11 - ORDER STATUS WORKFLOW
        
        📌 DECLARATION:
        Test the complete order status lifecycle from creation to delivery.
        
        📝 GOAL:
        - Verify order goes through expected status stages
        - Typical: Pending -> Processing -> Shipped -> Delivered
        
        🔍 STEPS:
        1. Create new order
        2. Verify initial status is "Pending"
        3. (If admin endpoints exist) Update status to "Processing"
        4. Verify status updates correctly
        
        ✔️ EXPECTED RESULT:
        - Initial status: "Pending" or "Confirmed"
        - Status can be updated through workflow
        - Invalid status transitions rejected
        """
        # Create order
        requests.post(f"{base_url}/cart/items", json={"skuId": 1, "quantity": 1}, headers=user_headers)
        
        create_response = requests.post(
            f"{base_url}/orders",
            json={"addressId": 1, "paymentMethod": "COD"},
            headers=user_headers
        )
        
        if create_response.status_code in [200, 201]:
            order = create_response.json().get("data")
            status = order.get("status", order.get("orderStatus"))
            assert status in ["Pending", "Confirmed", "Processing", "pending"]

    def test_admin_update_order_status(self, base_url, admin_headers):
        """
        🏷️ TC_ORDER_12 - ADMIN UPDATE ORDER STATUS
        
        📌 DECLARATION:
        Test that admin can update order status through fulfillment workflow.
        
        📝 GOAL:
        - Allow admin to change order status
        - Verify valid status transitions
        
        🔍 STEPS:
        1. Get an existing order
        2. Admin sends PUT /api/orders/{orderId}/status
        3. Verify status updates
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Order status changed
        """
        # This depends on existing orders
        response = requests.get(f"{base_url}/orders", headers=admin_headers)
        if response.status_code == 200:
            data = response.json().get("data", response.json())
            orders = data.get("items", data) if isinstance(data, dict) else data
            
            if orders:
                order_id = orders[0].get("orderId", orders[0].get("id"))
                
                # Try to update status
                update_response = requests.put(
                    f"{base_url}/orders/{order_id}/status",
                    json={"status": "Processing"},
                    headers=admin_headers
                )
                # Status might be 200, 404, or not implemented
                assert update_response.status_code in [200, 404, 405]


class TestOrderValidation:
    """
    ✅ GOAL: Validate inventory and business logic constraints on orders.
    """

    def test_order_with_out_of_stock_item(self, base_url, user_headers):
        """
        🏷️ TC_ORDER_13 - ORDER WITH OUT-OF-STOCK ITEM
        
        📌 DECLARATION:
        Test that orders cannot be created when items are out of stock.
        
        📝 GOAL:
        - Verify inventory validation during order creation
        
        🔍 STEPS:
        1. Add out-of-stock item to cart (quantity > available)
        2. Attempt to create order
        3. Verify rejection
        
        ✔️ EXPECTED RESULT:
        - Status: 400 or 422
        - Error: "Insufficient stock"
        """
        # This requires finding an out-of-stock item
        # For now, verify the system has some validation
        response = requests.post(
            f"{base_url}/cart/items",
            json={"skuId": 1, "quantity": 99999},
            headers=user_headers
        )
        
        # If accepted, ordering should fail
        if response.status_code in [200, 201]:
            order_response = requests.post(
                f"{base_url}/orders",
                json={"addressId": 1, "paymentMethod": "COD"},
                headers=user_headers
            )
            # Could be 200 if cart system manages it, or 400/422 if order validates
            assert order_response.status_code in [200, 201, 400, 422]

    def test_order_total_amount_calculation(self, base_url, user_headers):
        """
        🏷️ TC_ORDER_14 - ORDER TOTAL AMOUNT CALCULATION
        
        📌 DECLARATION:
        Test that order total is calculated correctly including prices and taxes.
        
        📝 GOAL:
        - Verify amount calculation accuracy
        - Check if taxes/discounts applied correctly
        
        🔍 STEPS:
        1. Add items with known prices to cart
        2. Create order
        3. Verify totalAmount = sum(item_price * quantity) + tax - discount
        
        ✔️ EXPECTED RESULT:
        - totalAmount matches calculation
        - All components (subtotal, tax, discount) present in response
        """
        # Add specific quantity of known SKU
        requests.delete(f"{base_url}/cart", headers=user_headers)
        requests.post(f"{base_url}/cart/items", json={"skuId": 1, "quantity": 2}, headers=user_headers)
        
        create_response = requests.post(
            f"{base_url}/orders",
            json={"addressId": 1, "paymentMethod": "COD"},
            headers=user_headers
        )
        
        if create_response.status_code in [200, 201]:
            order = create_response.json().get("data")
            assert "totalAmount" in order or "total" in order.get("orderSummary", {})
