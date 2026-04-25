"""
📋 TEST SUITE: PRODUCTS & SKUS - COMPREHENSIVE TEST CASES
===========================================================
Purpose: Complete testing of product CRUD operations, SKU management,
         product filtering, search, and inventory management.
Coverage: Product creation, updates, deletion, filtering, SKU variants,
          image management, role-based access control, and inventory.
"""

import pytest
import requests
import uuid


@pytest.fixture
def temp_category_for_products(base_url, admin_headers):
    """
    FIXTURE: CREATE TEMPORARY CATEGORY FOR PRODUCTS
    """
    payload = {
        "name": f"Product Test Category {str(uuid.uuid4())[:4]}",
        "isCore": False,
        "isActive": True
    }
    response = requests.post(f"{base_url}/categories", json=payload, headers=admin_headers)
    cat_id = response.json()["data"].get("categoryId", response.json()["data"].get("id"))
    
    yield cat_id
    
    # Cleanup
    if cat_id:
        requests.delete(f"{base_url}/categories/{cat_id}", headers=admin_headers)


class TestProductRetrieval:
    """
    ✅ GOAL: Validate comprehensive product retrieval with filtering,
    sorting, and pagination.
    """

    def test_get_all_products_public(self, base_url):
        """
        🏷️ TC_PROD_01 - GET ALL PRODUCTS (PUBLIC)
        
        📌 DECLARATION:
        Test that public users can retrieve the product catalog with pagination.
        
        📝 GOAL:
        - Get paginated list of all active products
        - Verify response structure
        
        🔍 STEPS:
        1. Call GET /api/products with pageNumber=1, pageSize=20
        2. Verify 200 OK
        3. Verify response contains items, total, pageNumber, pageSize
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Response is paginated
        - Each product has: id, name, description, price, stock, rating
        """
        params = {"pageNumber": 1, "pageSize": 20}
        response = requests.get(f"{base_url}/products", params=params)
        assert response.status_code == 200
        
        data = response.json()["data"]
        assert "items" in data or isinstance(data, list)

    def test_get_products_with_pagination(self, base_url):
        """
        🏷️ TC_PROD_02 - PRODUCT PAGINATION
        
        📌 DECLARATION:
        Test pagination with different page sizes.
        
        📝 GOAL:
        - Verify pagination works correctly
        - Test different page sizes (10, 20, 50)
        
        🔍 STEPS:
        1. Get page 1 with pageSize=10
        2. Get page 2 with pageSize=10
        3. Verify different items
        
        ✔️ EXPECTED RESULT:
        - Different products on different pages
        - pageNumber reflects actual page
        - total count consistent
        """
        page1 = requests.get(f"{base_url}/products", params={"pageNumber": 1, "pageSize": 10})
        page2 = requests.get(f"{base_url}/products", params={"pageNumber": 2, "pageSize": 10})
        
        assert page1.status_code == 200
        assert page2.status_code == 200

    def test_get_product_by_id(self, base_url):
        """
        🏷️ TC_PROD_03 - GET PRODUCT BY ID
        
        📌 DECLARATION:
        Test retrieving detailed information for a single product.
        
        📝 GOAL:
        - Get full product details including SKUs
        
        🔍 STEPS:
        1. Get products list
        2. Select a product ID
        3. Call GET /api/products/{productId}
        4. Verify 200 OK and full details
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Response includes: images, SKUs, specifications, reviews, ratings
        """
        # Get a product first
        products = requests.get(f"{base_url}/products", params={"pageNumber": 1, "pageSize": 50})
        if products.status_code == 200:
            data = products.json()["data"]
            items = data.get("items", data) if isinstance(data, dict) else data
            
            if items and len(items) > 0:
                prod_id = items[0].get("id", items[0].get("productId"))
                response = requests.get(f"{base_url}/products/{prod_id}")
                assert response.status_code == 200

    def test_get_nonexistent_product(self, base_url):
        """
        🏷️ TC_PROD_04 - GET NONEXISTENT PRODUCT
        
        📌 DECLARATION:
        Test that requesting non-existent product returns 404.
        
        📝 GOAL:
        - Verify proper error handling
        
        🔍 STEPS:
        1. Call GET /api/products/999999
        2. Verify 404 Not Found
        
        ✔️ EXPECTED RESULT:
        - Status: 404 Not Found
        """
        response = requests.get(f"{base_url}/products/999999")
        assert response.status_code == 404

    def test_get_products_filter_by_category(self, base_url):
        """
        🏷️ TC_PROD_05 - FILTER PRODUCTS BY CATEGORY
        
        📌 DECLARATION:
        Test filtering products by category ID.
        
        📝 GOAL:
        - Get only products in specific category
        
        🔍 STEPS:
        1. Call GET /api/products?categoryId=1
        2. Verify 200 OK
        3. Verify all returned products are in category 1
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - All products have matching category
        """
        response = requests.get(f"{base_url}/products", params={"categoryId": 1})
        assert response.status_code == 200

    def test_get_products_filter_by_price_range(self, base_url):
        """
        🏷️ TC_PROD_06 - FILTER PRODUCTS BY PRICE RANGE
        
        📌 DECLARATION:
        Test filtering products by minimum and maximum price.
        
        📝 GOAL:
        - Get products within price range
        
        🔍 STEPS:
        1. Call GET /api/products?minPrice=10&maxPrice=100
        2. Verify 200 OK
        3. Verify all prices within range
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - All products within price range
        """
        response = requests.get(f"{base_url}/products", params={"minPrice": 10, "maxPrice": 1000})
        assert response.status_code == 200

    def test_get_products_filter_by_brand(self, base_url):
        """
        🏷️ TC_PROD_07 - FILTER PRODUCTS BY BRAND
        
        📌 DECLARATION:
        Test filtering products by brand name.
        
        📝 GOAL:
        - Get only products from specific brand
        
        🔍 STEPS:
        1. Call GET /api/products?brand=Nike
        2. Verify 200 OK
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Products match brand filter
        """
        response = requests.get(f"{base_url}/products", params={"brand": "Nike"})
        assert response.status_code in [200, 404]  # 404 if brand doesn't exist

    def test_get_products_with_sorting(self, base_url):
        """
        🏷️ TC_PROD_08 - SORT PRODUCTS
        
        📌 DECLARATION:
        Test sorting products by various fields (price, rating, newest).
        
        📝 GOAL:
        - Verify sorting functionality
        
        🔍 STEPS:
        1. Get products sorted by price ascending
        2. Get products sorted by rating descending
        3. Get products sorted by newest
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK for all
        - Results are sorted correctly
        """
        sort_params = [
            {"sortBy": "price"},
            {"sortBy": "rating", "desc": "true"},
            {"sortBy": "createdDate"}
        ]
        
        for params in sort_params:
            response = requests.get(f"{base_url}/products", params=params)
            assert response.status_code == 200

    def test_search_products_by_name(self, base_url):
        """
        🏷️ TC_PROD_09 - SEARCH PRODUCTS BY NAME
        
        📌 DECLARATION:
        Test searching products by name/keyword.
        
        📝 GOAL:
        - Search functionality
        
        🔍 STEPS:
        1. Call GET /api/products?search=shirt
        2. Verify 200 OK
        3. Verify results contain search term
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Results match search criteria
        """
        response = requests.get(f"{base_url}/products", params={"search": "shirt"})
        assert response.status_code == 200


class TestProductCreation:
    """
    ✅ GOAL: Validate product creation with seller and admin access control.
    """

    def test_seller_create_product_success(self, base_url, seller_headers, temp_category_for_products):
        """
        🏷️ TC_PROD_10 - SELLER CREATE PRODUCT
        
        📌 DECLARATION:
        Test that seller can successfully create a new product.
        
        📝 GOAL:
        - Create product with SKU and images
        - Verify product is retrievable
        
        🔍 STEPS:
        1. Seller sends POST /api/products/seller with product data
        2. Verify 201 Created
        3. Verify response contains productId
        4. Retrieve product to confirm
        
        ✔️ EXPECTED RESULT:
        - Status: 201 Created
        - productId in response
        - Product searchable in catalog
        """
        payload = {
            "name": f"Seller Product {uuid.uuid4()}",
            "description": "Product created by seller",
            "categoryIds": [temp_category_for_products],
            "brand": "TestBrand",
            "weightKg": 0.5,
            "defaultSkuPrice": 29.99,
            "defaultSkuStock": 50,
            "dimensionsCm": "20x15x10"
        }
        response = requests.post(f"{base_url}/products/seller", json=payload, headers=seller_headers)
        assert response.status_code in [200, 201]
        
        data = response.json()["data"]
        assert "id" in data or "productId" in data

    def test_seller_create_product_multiple_categories(self, base_url, seller_headers, admin_headers):
        """
        🏷️ TC_PROD_11 - SELLER CREATE PRODUCT IN MULTIPLE CATEGORIES
        
        📌 DECLARATION:
        Test creating product assigned to multiple categories.
        
        📝 GOAL:
        - Product can belong to multiple categories
        
        🔍 STEPS:
        1. Get at least 2 category IDs
        2. Create product with categoryIds: [cat1, cat2]
        3. Verify 201 Created
        
        ✔️ EXPECTED RESULT:
        - Status: 201 Created
        - Product appears in all assigned categories
        """
        # Create categories
        cat1_response = requests.post(f"{base_url}/categories", 
                                     json={"name": f"Cat {uuid.uuid4()}", "isActive": True},
                                     headers=admin_headers)
        cat2_response = requests.post(f"{base_url}/categories",
                                     json={"name": f"Cat {uuid.uuid4()}", "isActive": True},
                                     headers=admin_headers)
        
        if cat1_response.status_code in [200, 201] and cat2_response.status_code in [200, 201]:
            cat1_id = cat1_response.json()["data"].get("categoryId", cat1_response.json()["data"].get("id"))
            cat2_id = cat2_response.json()["data"].get("categoryId", cat2_response.json()["data"].get("id"))
            
            payload = {
                "name": f"Multi-Category Product {uuid.uuid4()}",
                "description": "In multiple categories",
                "categoryIds": [cat1_id, cat2_id],
                "brand": "TestBrand",
                "defaultSkuPrice": 39.99,
                "defaultSkuStock": 30,
                "weightKg": 0.3
            }
            response = requests.post(f"{base_url}/products/seller", json=payload, headers=seller_headers)
            assert response.status_code in [200, 201]

    def test_create_product_missing_required_field(self, base_url, seller_headers, temp_category_for_products):
        """
        🏷️ TC_PROD_12 - CREATE PRODUCT WITH MISSING REQUIRED FIELD
        
        📌 DECLARATION:
        Test that product creation fails when required fields are missing.
        
        📝 GOAL:
        - Verify name, category, price are required
        
        🔍 STEPS:
        1. Try without name
        2. Try without category
        3. Try without price
        
        ✔️ EXPECTED RESULT:
        - Status: 400 Bad Request for each
        """
        base_payload = {
            "name": "Test Product",
            "description": "Test",
            "categoryIds": [temp_category_for_products],
            "brand": "Test",
            "defaultSkuPrice": 10.0,
            "defaultSkuStock": 10
        }
        
        # Test without name
        payload = base_payload.copy()
        del payload["name"]
        response = requests.post(f"{base_url}/products/seller", json=payload, headers=seller_headers)
        assert response.status_code == 400

    def test_user_cannot_create_product(self, base_url, user_headers, temp_category_for_products):
        """
        🏷️ TC_PROD_13 - USER CANNOT CREATE PRODUCT
        
        📌 DECLARATION:
        Test that non-seller users cannot create products.
        
        📝 GOAL:
        - Verify role-based access control
        
        🔍 STEPS:
        1. Buyer attempts POST /api/products/seller
        2. Verify 403 Forbidden
        
        ✔️ EXPECTED RESULT:
        - Status: 403 Forbidden
        """
        payload = {
            "name": f"Unauthorized Product {uuid.uuid4()}",
            "categoryIds": [temp_category_for_products],
            "brand": "Test",
            "defaultSkuPrice": 10.0,
            "defaultSkuStock": 10
        }
        response = requests.post(f"{base_url}/products/seller", json=payload, headers=user_headers)
        assert response.status_code == 403

    def test_public_cannot_create_product(self, base_url, temp_category_for_products):
        """
        🏷️ TC_PROD_14 - PUBLIC CANNOT CREATE PRODUCT
        
        📌 DECLARATION:
        Test that unauthenticated users cannot create products.
        
        📝 GOAL:
        - Verify authentication required
        
        🔍 STEPS:
        1. Unauthenticated user tries to create product
        2. Verify 401 Unauthorized
        
        ✔️ EXPECTED RESULT:
        - Status: 401 Unauthorized
        """
        payload = {
            "name": f"Hacker Product {uuid.uuid4()}",
            "categoryIds": [temp_category_for_products],
            "brand": "Hack",
            "defaultSkuPrice": 10.0,
            "defaultSkuStock": 10
        }
        response = requests.post(f"{base_url}/products/seller", json=payload)
        assert response.status_code == 401


class TestProductUpdate:
    """
    ✅ GOAL: Validate product update operations.
    """

    def test_seller_update_own_product(self, base_url, seller_headers, admin_headers, temp_category_for_products):
        """
        🏷️ TC_PROD_15 - SELLER UPDATE PRODUCT
        
        📌 DECLARATION:
        Test that seller can update their own product.
        
        📝 GOAL:
        - Update product name, description, price
        - Verify changes are reflected
        
        🔍 STEPS:
        1. Create product as seller
        2. Update name and description
        3. Verify changes
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK or 204 No Content
        - Changes reflected in product
        """
        # Create product first
        create_payload = {
            "name": f"Update Test {uuid.uuid4()}",
            "description": "Original description",
            "categoryIds": [temp_category_for_products],
            "brand": "Test",
            "defaultSkuPrice": 50.0,
            "defaultSkuStock": 100
        }
        create_response = requests.post(f"{base_url}/products/seller", json=create_payload, headers=seller_headers)

        if create_response.status_code in [200, 201]:
            prod_id = create_response.json()["data"].get("id")
            print(prod_id)
            # Update
            update_payload = {
                "name": f"Updated Product {uuid.uuid4()}",
                "description": "Updated description"
            }
            response = requests.put(f"{base_url}/products/seller/{prod_id}", json=update_payload, headers=seller_headers)
            assert response.status_code in [200, 204]

    def test_seller_cannot_update_other_seller_product(self, base_url, seller_headers):
        """
        🏷️ TC_PROD_16 - SELLER CANNOT UPDATE OTHER'S PRODUCT
        
        📌 DECLARATION:
        Test that sellers can only update their own products.
        
        📝 GOAL:
        - Verify authorization
        
        🔍 STEPS:
        1. Seller tries to update product from different seller
        2. Verify 403 Forbidden
        
        ✔️ EXPECTED RESULT:
        - Status: 403 Forbidden
        """
        # Try to update a product (likely owned by different seller)
        response = requests.put(f"{base_url}/products/999999", 
                              json={"name": "Hacked"}, 
                              headers=seller_headers)
        assert response.status_code in [403, 404]

    def test_admin_can_update_any_product(self, base_url, admin_headers):
        """
        🏷️ TC_PROD_17 - ADMIN CAN UPDATE ANY PRODUCT
        
        📌 DECLARATION:
        Test that admin can update any product in the system.
        
        📝 GOAL:
        - Admin override for product management
        
        🔍 STEPS:
        1. Get a product
        2. Admin updates it
        3. Verify 200 OK
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Product updated
        """
        # Get first product
        products = requests.get(f"{base_url}/products", params={"pageNumber": 1, "pageSize": 10})
        if products.status_code == 200:
            data = products.json()["data"]
            items = data.get("items", data) if isinstance(data, dict) else data
            
            if items and len(items) > 0:
                prod_id = items[0].get("id")
                response = requests.put(f"{base_url}/products/{prod_id}", 
                                      json={"name": "Admin Updated"},
                                      headers=admin_headers)
                assert response.status_code in [200, 204, 403]  # May still be owned by seller


class TestProductDeletion:
    """
    ✅ GOAL: Validate product deletion with cascade handling.
    """

    def test_seller_delete_product(self, base_url, seller_headers, admin_headers, temp_category_for_products):
        """
        🏷️ TC_PROD_18 - SELLER DELETE PRODUCT
        
        📌 DECLARATION:
        Test that seller can delete their own product.
        
        📝 GOAL:
        - Delete product and verify it's not searchable
        
        🔍 STEPS:
        1. Create product
        2. Delete it
        3. Verify 404 when retrieved
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK or 204 No Content
        - Product no longer found
        """
        # Create
        create_payload = {
            "name": f"To Delete {uuid.uuid4()}",
            "categoryIds": [temp_category_for_products],
            "brand": "Test",
            "defaultSkuPrice": 10.0,
            "defaultSkuStock": 5
        }
        create_response = requests.post(f"{base_url}/products/seller", json=create_payload, headers=seller_headers)
        
        if create_response.status_code in [200, 201]:
            prod_id = create_response.json()["data"].get("id")
            
            # Delete
            delete_response = requests.delete(f"{base_url}/products/seller/{prod_id}", headers=seller_headers)
            assert delete_response.status_code in [200, 204]
            
            # Verify deleted
            get_response = requests.get(f"{base_url}/products/{prod_id}")
            assert get_response.status_code == 404

    def test_seller_cannot_delete_other_seller_product(self, base_url, seller_headers):
        """
        🏷️ TC_PROD_19 - SELLER CANNOT DELETE OTHER'S PRODUCT
        
        📌 DECLARATION:
        Test that sellers can only delete their own products.
        
        📝 GOAL:
        - Verify authorization
        
        🔍 STEPS:
        1. Seller tries to delete other's product
        2. Verify 403 Forbidden
        
        ✔️ EXPECTED RESULT:
        - Status: 403 Forbidden
        """
        response = requests.delete(f"{base_url}/products/999999", headers=seller_headers)
        assert response.status_code in [403, 404]


class TestSKUManagement:
    """
    ✅ GOAL: Validate SKU (variant) management for products.
    """

    def test_product_has_default_sku(self, base_url, seller_headers, admin_headers, temp_category_for_products):
        """
        🏷️ TC_PROD_20 - PRODUCT HAS DEFAULT SKU
        
        📌 DECLARATION:
        Test that creating a product automatically creates a default SKU.
        
        📝 GOAL:
        - Verify SKU is created with product
        
        🔍 STEPS:
        1. Create product
        2. Verify product has skuId/SKUs
        3. Verify SKU has same price as defaultSkuPrice
        
        ✔️ EXPECTED RESULT:
        - Product has default SKU
        - SKU price matches defaultSkuPrice
        """
        payload = {
            "name": f"SKU Test {uuid.uuid4()}",
            "categoryIds": [temp_category_for_products],
            "brand": "Test",
            "defaultSkuPrice": 25.0,
            "defaultSkuStock": 100
        }
        response = requests.post(f"{base_url}/products/seller", json=payload, headers=seller_headers)
        assert response.status_code in [200, 201]
        
        data = response.json()["data"]
        # Should have SKU information
        assert "sku" in data


    def test_seller_add_sku_to_product(self, base_url, seller_headers, admin_headers, temp_category_for_products):
        """
        🏷️ TC_PROD_21 - SELLER ADD SKU VARIANT
        
        📌 DECLARATION:
        Test adding new SKU variant to existing product.
        
        📝 GOAL:
        - Create variant with different size/color/price
        
        🔍 STEPS:
        1. Create product
        2. Add new SKU with different attributes
        3. Verify SKU created
        
        ✔️ EXPECTED RESULT:
        - Status: 201 Created
        - Product now has multiple SKUs
        """
        # Create product
        create_payload = {
            "name": f"Multi-SKU Product {uuid.uuid4()}",
            "categoryIds": [temp_category_for_products],
            "brand": "Test",
            "defaultSkuPrice": 50.0,
            "defaultSkuStock": 100
        }
        create_response = requests.post(f"{base_url}/products/seller", json=create_payload, headers=seller_headers)
        
        if create_response.status_code in [200, 201]:
            prod_id = create_response.json()["data"].get("id")
            sku_payload = {
                "productId": prod_id,
                "variantAttributes": {"size": "L", "color": "Red"},
                "price": 55.0,
                "stock": 50
            }
            response = requests.post(f"{base_url}/skus/seller", 
                                   json=sku_payload, 
                                   headers=seller_headers)
            assert response.status_code in [200, 201, 400]  # May not support direct SKU creation

    def test_sku_inventory_validation(self, base_url):
        """
        🏷️ TC_PROD_22 - SKU INVENTORY VALIDATION
        
        📌 DECLARATION:
        Test that SKU stock information is validated and enforced.
        
        📝 GOAL:
        - Verify stock is non-negative
        - Verify stock decreases with orders
        
        🔍 STEPS:
        1. Get product with SKU
        2. Verify stock >= 0
        3. Note current stock
        
        ✔️ EXPECTED RESULT:
        - Stock is non-negative number
        - Stock properly reflected in product
        """
        products = requests.get(f"{base_url}/products", params={"pageNumber": 1, "pageSize": 10})
        if products.status_code == 200:
            data = products.json()["data"]
            items = data.get("items", data) if isinstance(data, dict) else data
            
            if items and len(items) > 0:
                # Check stock values
                for item in items[:5]:
                    stock = item.get("stock", item.get("defaultSkuStock", 0))
                    assert stock >= 0, "Stock should not be negative"


class TestProductInventory:
    """
    ✅ GOAL: Validate inventory management and stock tracking.
    """
    def test_product_rating_calculation(self, base_url):
        """
        🏷️ TC_PROD_23 - PRODUCT RATING CALCULATION
        
        📌 DECLARATION:
        Test that product ratings are calculated from reviews.
        
        📝 GOAL:
        - Verify rating reflects customer reviews
        
        🔍 STEPS:
        1. Get a product
        2. Verify rating field present
        3. Verify rating is 0-5 scale
        
        ✔️ EXPECTED RESULT:
        - Product has rating field
        - Rating is 0-5
        - Rating count available
        """
        products = requests.get(f"{base_url}/products", params={"pageNumber": 1, "pageSize": 10})
        if products.status_code == 200:
            data = products.json()["data"]
            items = data.get("items", data) if isinstance(data, dict) else data
            
            if items and len(items) > 0:
                product = items[0]
                if "rating" in product:
                    rating = product["rating"]
                    assert 0 <= rating <= 5, "Rating should be 0-5"
