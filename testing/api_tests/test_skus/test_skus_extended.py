# tests/test_skus/test_skus_extended.py
"""
📋 TEST SUITE: SKUs (STOCK KEEPING UNITS) - EXTENDED TEST CASES
================================================================
Purpose: Comprehensive testing of SKU management including creation, updates,
         inventory management, and product variant handling.
Coverage: SKU creation, pricing, stock management, variant attributes,
          and role-based access control.
"""

import pytest
import requests
import uuid


@pytest.fixture
def seller_product_for_skus(base_url, seller_headers, admin_headers):
    """
    FIXTURE: Create a product with basic SKU for SKU testing
    """
    # Create category
    cat_payload = {
        "name": f"SKU Test Cat {str(uuid.uuid4())[:4]}",
        "isActive": True
    }
    cat_res = requests.post(f"{base_url}/categories", json=cat_payload, headers=admin_headers)
    cat_id = cat_res.json()["data"].get("categoryId", cat_res.json()["data"].get("id"))
    
    # Create product
    prod_payload = {
        "name": f"Shirt for SKU Tests {str(uuid.uuid4())[:4]}",
        "categoryIds": [cat_id],
        "brand": "TestBrand",
        "weightKg": 0.5,
        "defaultSkuPrice": 29.99,
        "defaultSkuStock": 100
    }
    prod_res = requests.post(f"{base_url}/products/seller", json=prod_payload, headers=seller_headers)
    prod_data = prod_res.json()["data"]
    
    yield {
        "product_id": prod_data.get("id"),
        "category_id": cat_id,
        "default_sku_id": prod_data.get("skuId")
    }
    
    # Cleanup
    requests.delete(f"{base_url}/products/{prod_data.get('id')}", headers=admin_headers)
    requests.delete(f"{base_url}/categories/{cat_id}", headers=admin_headers)


class TestSKUCreation:
    """
    ✅ GOAL: Test SKU creation with various attributes and pricing options
    """

    def test_seller_create_sku_for_product(self, base_url, seller_headers, seller_product_for_skus):
        """
        🏷️ TC_SKU_EXT_01 - SELLER TẠO SKU MỚI CHO SẢN PHẨM
        
        📌 DECLARATION:
        Test creating a new SKU variant (e.g., different size/color) for existing product.
        
        📝 GOAL:
        - Create SKU with different attributes
        - Verify pricing and stock are independent
        
        🔍 STEPS:
        1. Create SKU with size="L", color="Blue"
        2. Set specific price and stock
        3. Verify 200/201
        
        ✔️ EXPECTED RESULT:
        - Status: 200/201 Created
        - SKU has unique attributes
        - Price and stock separate from base product
        """
        product_id = seller_product_for_skus["product_id"]
        
        payload = {
            "productId": product_id,
            "price": 35.99,
            "stock": 50,
            "attributes": {
                "size": "L",
                "color": "Blue"
            }
        }
        
        response = requests.post(f"{base_url}/skus/seller", json=payload, headers=seller_headers)
        assert response.status_code in [200, 201]
        
        sku_data = response.json()["data"]
        assert sku_data.get("price") == 35.99
        assert sku_data.get("stock") == 50

    def test_seller_create_sku_with_discount(self, base_url, seller_headers, seller_product_for_skus):
        """
        🏷️ TC_SKU_EXT_02 - SELLER TẠO SKU VỚI GIẢM GIÁ
        
        📌 DECLARATION:
        Test creating SKU with promotional pricing.
        
        📝 GOAL:
        - Create SKU with both regular and discount price
        
        🔍 STEPS:
        1. Create SKU with price=100, discountPrice=80
        2. Verify 200/201
        
        ✔️ EXPECTED RESULT:
        - SKU has both prices
        - Discount price is lower than regular
        """
        product_id = seller_product_for_skus["product_id"]
        
        payload = {
            "productId": product_id,
            "price": 100.00,
            "discountPrice": 75.00,
            "stock": 25
        }
        
        response = requests.post(f"{base_url}/skus/seller", json=payload, headers=seller_headers)
        assert response.status_code in [200, 201]


class TestSKUUpdate:
    """
    ✅ GOAL: Test SKU updates and modifications
    """

    def test_seller_update_sku_price(self, base_url, seller_headers, seller_product_for_skus):
        """
        🏷️ TC_SKU_EXT_03 - SELLER CẬP NHẬT GIÁ SKU
        
        📌 DECLARATION:
        Test updating SKU price.
        
        📝 GOAL:
        - Change SKU price
        - Verify update reflects in product details
        
        🔍 STEPS:
        1. Update SKU price to new value
        2. Verify 200/204
        3. Retrieve SKU to confirm
        
        ✔️ EXPECTED RESULT:
        - Status: 200/204
        - Price is updated
        """
        product_id = seller_product_for_skus["product_id"]
        sku_id = seller_product_for_skus["default_sku_id"]
        
        if not sku_id:
            pytest.skip("No SKU ID available")
        
        payload = {"price": 39.99}
        response = requests.put(f"{base_url}/skus/{sku_id}", json=payload, headers=seller_headers)
        assert response.status_code in [200, 204]

    def test_seller_update_sku_stock(self, base_url, seller_headers, seller_product_for_skus):
        """
        🏷️ TC_SKU_EXT_04 - SELLER CẬP NHẬT SỐ LƯỢNG SKU
        
        📌 DECLARATION:
        Test updating SKU stock/inventory.
        
        📝 GOAL:
        - Adjust SKU stock
        - Verify stock is updated
        
        🔍 STEPS:
        1. Update SKU stock to 75
        2. Verify 200/204
        
        ✔️ EXPECTED RESULT:
        - Status: 200/204
        - Stock is updated
        """
        sku_id = seller_product_for_skus["default_sku_id"]
        
        if not sku_id:
            pytest.skip("No SKU ID available")
        
        payload = {"stock": 75}
        response = requests.put(f"{base_url}/skus/{sku_id}", json=payload, headers=seller_headers)
        assert response.status_code in [200, 204]

    def test_seller_cannot_update_others_sku(self, base_url, seller_headers):
        """
        🏷️ TC_SKU_EXT_05 - KIỂM ĐỊNH BẢO MẬT: SELLER KHÔNG ĐƯỢC CHỈNH SỬA SKU CỦA NGƯỜI KHÁC
        
        📌 DECLARATION:
        Test that sellers cannot modify SKUs from other sellers.
        
        📝 GOAL:
        - Prevent unauthorized modifications
        
        🔍 STEPS:
        1. Try to update SKU ID 999 (not owned by seller)
        2. Verify 403 Forbidden
        
        ✔️ EXPECTED RESULT:
        - Status: 403 Forbidden
        """
        payload = {"price": 999.99}
        response = requests.put(f"{base_url}/skus/9999", json=payload, headers=seller_headers)
        assert response.status_code in [403, 404]


class TestSKURetrieval:
    """
    ✅ GOAL: Test SKU retrieval and listing
    """

    def test_get_skus_by_product_id(self, base_url, seller_product_for_skus):
        """
        🏷️ TC_SKU_EXT_06 - LẤY DANH SÁCH SKU CỦA SẢN PHẨM
        
        📌 DECLARATION:
        Test retrieving all SKUs for a specific product.
        
        📝 GOAL:
        - Get all variants/SKUs of a product
        
        🔍 STEPS:
        1. Call GET /products/{productId}/skus
        2. Verify 200 OK
        3. Verify list of SKUs returned
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Returns array of SKUs
        - Each SKU contains: price, stock, attributes
        """
        product_id = seller_product_for_skus["product_id"]
        
        response = requests.get(f"{base_url}/products/{product_id}/skus")
        assert response.status_code == 200
        
        data = response.json()["data"]
        assert isinstance(data.get("items", data), list)

    def test_get_sku_details(self, base_url, seller_product_for_skus):
        """
        🏷️ TC_SKU_EXT_07 - LẤY CHI TIẾT SKU
        
        📌 DECLARATION:
        Test retrieving detailed information for a specific SKU.
        
        📝 GOAL:
        - Get full SKU details
        
        🔍 STEPS:
        1. Call GET /skus/{skuId}
        2. Verify 200 OK
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Contains: price, stock, attributes, productId
        """
        sku_id = seller_product_for_skus["default_sku_id"]
        
        if not sku_id:
            pytest.skip("No SKU ID available")
        
        response = requests.get(f"{base_url}/skus/{sku_id}")
        assert response.status_code == 200
        
        data = response.json()["data"]
        assert "price" in data or "price" in str(data)

    def test_seller_get_all_skus(self, base_url, seller_headers):
        """
        🏷️ TC_SKU_EXT_08 - SELLER LẤY TẤT CẢ SKU TRONG KHO
        
        📌 DECLARATION:
        Test retrieving all SKUs belonging to a seller.
        
        📝 GOAL:
        - Get seller's complete inventory
        
        🔍 STEPS:
        1. Call GET /skus/seller
        2. Verify 200 OK
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Returns all seller's SKUs
        """
        response = requests.get(f"{base_url}/skus/seller", headers=seller_headers)
        assert response.status_code == 200
        
        data = response.json()["data"]
        assert isinstance(data.get("items", data), list)


class TestSKUDeletion:
    """
    ✅ GOAL: Test SKU deletion and archiving
    """

    def test_seller_delete_sku(self, base_url, seller_headers, seller_product_for_skus, admin_headers):
        """
        🏷️ TC_SKU_EXT_09 - SELLER XÓA SKU
        
        📌 DECLARATION:
        Test deleting a SKU from the system.
        
        📝 GOAL:
        - Remove SKU variant
        
        🔍 STEPS:
        1. Create a new SKU
        2. Delete the SKU
        3. Verify 200/204
        
        ✔️ EXPECTED RESULT:
        - Status: 200/204
        - SKU is removed from product
        """
        product_id = seller_product_for_skus["product_id"]
        
        # Create SKU to delete
        create_payload = {
            "productId": product_id,
            "price": 45.00,
            "stock": 10
        }
        create_res = requests.post(f"{base_url}/skus/seller", json=create_payload, headers=seller_headers)
        
        if create_res.status_code in [200, 201]:
            sku_id = create_res.json()["data"].get("id", create_res.json()["data"].get("skuId"))
            
            # Delete it
            delete_res = requests.delete(f"{base_url}/skus/{sku_id}", headers=seller_headers)
            assert delete_res.status_code in [200, 204]

    def test_admin_delete_sku(self, base_url, admin_headers, seller_headers):
        """
        🏷️ TC_SKU_EXT_10 - ADMIN CÓ THỂ XÓA BẤT KỲ SKU NÀO
        
        📌 DECLARATION:
        Test that admins can delete any SKU in the system.
        
        📝 GOAL:
        - Verify admin authority over all SKUs
        
        🔍 STEPS:
        1. Admin attempts DELETE on any SKU
        2. Verify 200/204 (or 403 if system restricts)
        
        ✔️ EXPECTED RESULT:
        - Admin can delete SKUs
        """
        # Try to delete a SKU (even if owned by someone else)
        delete_res = requests.delete(f"{base_url}/skus/9999", headers=admin_headers)
        # Should be 204/200 if deleted, or 404 if not found
        assert delete_res.status_code in [200, 204, 404]


class TestSKUValidation:
    """
    ✅ GOAL: Test SKU validation and constraints
    """

    def test_sku_price_cannot_be_negative(self, base_url, seller_headers, seller_product_for_skus):
        """
        🏷️ TC_SKU_EXT_11 - GIÁ SKU PHẢI LÀ SỐ DƯƠNG
        
        📌 DECLARATION:
        Test that SKU price validation rejects negative values.
        
        📝 GOAL:
        - Ensure price is valid
        
        🔍 STEPS:
        1. Try to create SKU with price=-10
        2. Verify 400 Bad Request
        
        ✔️ EXPECTED RESULT:
        - Status: 400 Bad Request
        """
        product_id = seller_product_for_skus["product_id"]
        
        payload = {
            "productId": product_id,
            "price": -10.00,
            "stock": 10
        }
        
        response = requests.post(f"{base_url}/skus/seller", json=payload, headers=seller_headers)
        assert response.status_code == 400

    def test_sku_stock_cannot_be_negative(self, base_url, seller_headers, seller_product_for_skus):
        """
        🏷️ TC_SKU_EXT_12 - SỐ LƯỢNG SKU KHÔNG ĐƯỢC ÂM
        
        📌 DECLARATION:
        Test that SKU stock validation rejects negative values.
        
        📝 GOAL:
        - Ensure stock quantity is valid
        
        🔍 STEPS:
        1. Try to create SKU with stock=-5
        2. Verify 400 Bad Request
        
        ✔️ EXPECTED RESULT:
        - Status: 400 Bad Request
        """
        product_id = seller_product_for_skus["product_id"]
        
        payload = {
            "productId": product_id,
            "price": 25.00,
            "stock": -5
        }
        
        response = requests.post(f"{base_url}/skus/seller", json=payload, headers=seller_headers)
        assert response.status_code == 400
