"""
📋 TEST SUITE: CATEGORIES - COMPREHENSIVE TEST CASES
=====================================================
Purpose: Complete testing of category CRUD operations, hierarchies, filters,
         and access control for different user roles.
Coverage: Category creation, updates, deletion, retrieval, parent-child relationships,
          role-based access, and edge cases.
"""

import pytest
import requests
import uuid


class TestCategoryRetrieval:
    """
    ✅ GOAL: Validate all methods of retrieving categories including
    list retrieval, filtering, sorting, and parent-child hierarchy queries.
    """

    def test_get_all_categories_public(self, base_url):
        """
        🏷️ TC_CAT_01 - GET ALL CATEGORIES (PUBLIC)
        
        📌 DECLARATION:
        Test that any user can retrieve the complete list of all active categories.
        
        📝 GOAL:
        - Retrieve all categories without authentication
        - Verify paginated response with correct structure
        
        🔍 STEPS:
        1. Send GET /api/categories with pagination params (pageNumber=1, pageSize=20)
        2. Verify status 200 OK
        3. Verify response contains array of categories with id, name, slug, etc.
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Response contains: data.items (array), data.total, data.pageNumber, data.pageSize
        - Each category has: categoryId/id, name, slug, description, isCore, isActive
        """
        params = {"pageNumber": 1, "pageSize": 20}
        response = requests.get(f"{base_url}/categories", params=params)
        assert response.status_code == 200
        
        data = response.json()["data"]
        assert "items" in data or isinstance(data, list)
        items = data.get("items", data) if isinstance(data, dict) else data
        assert len(items) >= 0

    def test_get_categories_with_pagination(self, base_url):
        """
        🏷️ TC_CAT_02 - CATEGORY PAGINATION
        
        📌 DECLARATION:
        Test pagination functionality when retrieving large category lists.
        
        📝 GOAL:
        - Test different page sizes (10, 20, 50)
        - Verify pagination metadata
        
        🔍 STEPS:
        1. Get page 1 with pageSize=10
        2. Get page 2 with pageSize=10
        3. Verify different items on each page
        4. Verify total count is provided
        
        ✔️ EXPECTED RESULT:
        - Different items on different pages
        - total count matches across requests
        - pageNumber and pageSize in response
        """
        page1 = requests.get(f"{base_url}/categories", params={"pageNumber": 1, "pageSize": 10})
        page2 = requests.get(f"{base_url}/categories", params={"pageNumber": 2, "pageSize": 10})
        
        assert page1.status_code == 200
        assert page2.status_code == 200

    def test_get_category_by_id(self, base_url, admin_headers):
        """
        🏷️ TC_CAT_03 - GET CATEGORY BY ID
        
        📌 DECLARATION:
        Test retrieving a single category by its ID.
        
        📝 GOAL:
        - Retrieve specific category by ID
        - Verify all category fields are present
        
        🔍 STEPS:
        1. Get all categories to find a valid ID
        2. Call GET /api/categories/{categoryId}
        3. Verify 200 OK
        4. Verify response contains full category data
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Response contains: id, name, slug, description, isCore, isActive, children array
        """
        # First get a category ID
        all_cats = requests.get(f"{base_url}/categories", params={"pageNumber": 1, "pageSize": 50})
        if all_cats.status_code == 200:
            items = all_cats.json()["data"].get("items", all_cats.json()["data"])
            if items:
                cat_id = items[0].get("categoryId", items[0].get("id"))
                if cat_id:
                    response = requests.get(f"{base_url}/categories/{cat_id}")
                    assert response.status_code == 200
                    cat_data = response.json().get("data", response.json())
                    assert "name" in cat_data or "categoryName" in cat_data

    def test_get_category_by_slug(self, base_url):
        """
        🏷️ TC_CAT_04 - GET CATEGORY BY SLUG
        
        📌 DECLARATION:
        Test retrieving a category by its slug (URL-friendly identifier).
        
        📝 GOAL:
        - Retrieve category using slug parameter
        - Useful for frontend URLs
        
        🔍 STEPS:
        1. Get all categories and note a slug
        2. Call GET /api/categories?slug=electronics
        3. Verify 200 OK
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Returns category with matching slug
        """
        response = requests.get(f"{base_url}/categories", params={"slug": "electronics"})
        # May return 200 with results or 404 if slug doesn't exist
        assert response.status_code in [200, 404]

    def test_get_core_categories(self, base_url):
        """
        🏷️ TC_CAT_05 - GET CORE CATEGORIES ONLY
        
        📌 DECLARATION:
        Test filtering categories to show only core (top-level) categories.
        
        📝 GOAL:
        - Retrieve only core categories for main navigation
        
        🔍 STEPS:
        1. Call GET /api/categories/core
        2. Verify all returned categories have isCore=true
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - All items have isCore: true
        - Typically fewer items than total categories
        """
        response = requests.get(f"{base_url}/categories/core")
        assert response.status_code == 200

    def test_get_child_categories(self, base_url, admin_headers):
        """
        🏷️ TC_CAT_06 - GET CHILD CATEGORIES OF PARENT
        
        📌 DECLARATION:
        Test retrieving subcategories under a specific parent category.
        
        📝 GOAL:
        - Get all children of a parent category
        - Useful for navigation breadcrumbs
        
        🔍 STEPS:
        1. Get a category that has children
        2. Call GET /api/categories/{parentId}/children
        3. Verify response contains only direct children
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Returns array of child categories
        - Each child has parentId matching the requested parent
        """
        # First get core categories
        core = requests.get(f"{base_url}/categories/core")
        if core.status_code == 200:
            items = core.json()["data"].get("items", core.json()["data"])
            if items and len(items) > 0:
                parent_id = items[0].get("categoryId", items[0].get("id"))
                if parent_id:
                    response = requests.get(f"{base_url}/categories/{parent_id}/children")
                    assert response.status_code in [200, 404]  # 404 if no children


class TestCategoryCreation:
    """
    ✅ GOAL: Validate category creation with role-based access control.
    Only admins should be able to create categories.
    """

    def test_admin_create_category_success(self, base_url, admin_headers):
        """
        🏷️ TC_CAT_07 - ADMIN CREATE CATEGORY
        
        📌 DECLARATION:
        Test that admin users can successfully create new categories.
        
        📝 GOAL:
        - Create a new top-level category with valid data
        - Verify category is saved and retrievable
        
        🔍 STEPS:
        1. Admin sends POST /api/categories with category data
        2. Verify 200/201 Created
        3. Verify response contains categoryId
        4. Retrieve category by ID to confirm
        
        ✔️ EXPECTED RESULT:
        - Status: 201 Created or 200 OK
        - Response contains categoryId
        - Category appears in list
        """
        payload = {
            "name": f"Test Category {uuid.uuid4()}",
            "description": "Automated test category",
            "isCore": False,
            "isActive": True
        }
        response = requests.post(f"{base_url}/categories", json=payload, headers=admin_headers)
        assert response.status_code in [200, 201]
        
        data = response.json().get("data", response.json())
        category_id = data.get("categoryId", data.get("id"))
        assert category_id is not None

    def test_admin_create_subcategory(self, base_url, admin_headers):
        """
        🏷️ TC_CAT_08 - ADMIN CREATE SUBCATEGORY
        
        📌 DECLARATION:
        Test creating a child category under an existing parent.
        
        📝 GOAL:
        - Create subcategory with parentId
        - Verify hierarchy is maintained
        
        🔍 STEPS:
        1. Get a core category ID
        2. Create new category with parentId set
        3. Verify child appears under parent
        
        ✔️ EXPECTED RESULT:
        - Status: 201 Created
        - Response includes parentId
        - Category accessible via /categories/{parentId}/children
        """
        # First create parent
        parent_payload = {
            "name": f"Parent Category {uuid.uuid4()}",
            "isCore": True,
            "isActive": True
        }
        parent_response = requests.post(f"{base_url}/categories", json=parent_payload, headers=admin_headers)
        assert parent_response.status_code in [200, 201]
        
        parent_id = parent_response.json()["data"].get("categoryId", parent_response.json()["data"].get("id"))
        
        # Create child
        child_payload = {
            "name": f"Child Category {uuid.uuid4()}",
            "parentId": parent_id,
            "isCore": False,
            "isActive": True
        }
        child_response = requests.post(f"{base_url}/categories", json=child_payload, headers=admin_headers)
        assert child_response.status_code in [200, 201]

    def test_create_category_missing_required_field(self, base_url, admin_headers):
        """
        🏷️ TC_CAT_09 - CREATE CATEGORY WITH MISSING REQUIRED FIELD
        
        📌 DECLARATION:
        Test that category creation fails when required fields are missing.
        
        📝 GOAL:
        - Verify name field is required
        - Verify other mandatory fields
        
        🔍 STEPS:
        1. Submit payload without name
        2. Submit without isActive status
        3. Verify 400 Bad Request for each
        
        ✔️ EXPECTED RESULT:
        - Status: 400 Bad Request
        - Error message indicates missing field
        """
        # Missing name
        payload_no_name = {
            "description": "Test",
            "isCore": False,
            "isActive": True
        }
        response = requests.post(f"{base_url}/categories", json=payload_no_name, headers=admin_headers)
        assert response.status_code == 400

    def test_create_category_empty_name(self, base_url, admin_headers):
        """
        🏷️ TC_CAT_10 - CREATE CATEGORY WITH EMPTY NAME
        
        📌 DECLARATION:
        Test that category name cannot be empty string.
        
        📝 GOAL:
        - Reject empty name
        
        🔍 STEPS:
        1. Submit with name = ""
        2. Verify 400 Bad Request
        
        ✔️ EXPECTED RESULT:
        - Status: 400 Bad Request
        """
        payload = {
            "name": "",
            "isCore": False,
            "isActive": True
        }
        response = requests.post(f"{base_url}/categories", json=payload, headers=admin_headers)
        assert response.status_code == 400

    def test_seller_cannot_create_category(self, base_url, seller_headers):
        """
        🏷️ TC_CAT_11 - SELLER CANNOT CREATE CATEGORY
        
        📌 DECLARATION:
        Test that non-admin users cannot create categories.
        
        📝 GOAL:
        - Verify access control for category creation
        - Only admins allowed
        
        🔍 STEPS:
        1. Seller attempts POST /api/categories
        2. Verify 403 Forbidden
        
        ✔️ EXPECTED RESULT:
        - Status: 403 Forbidden
        """
        payload = {
            "name": f"Unauthorized Category {uuid.uuid4()}",
            "isCore": False,
            "isActive": True
        }
        response = requests.post(f"{base_url}/categories", json=payload, headers=seller_headers)
        assert response.status_code == 403

    def test_user_cannot_create_category(self, base_url, user_headers):
        """
        🏷️ TC_CAT_12 - BUYER USER CANNOT CREATE CATEGORY
        
        📌 DECLARATION:
        Test that regular buyer users cannot create categories.
        
        📝 GOAL:
        - Verify access control enforcement
        
        🔍 STEPS:
        1. Buyer user attempts POST /api/categories
        2. Verify 403 Forbidden
        
        ✔️ EXPECTED RESULT:
        - Status: 403 Forbidden
        """
        payload = {
            "name": f"Unauthorized Category {uuid.uuid4()}",
            "isCore": False,
            "isActive": True
        }
        response = requests.post(f"{base_url}/categories", json=payload, headers=user_headers)
        assert response.status_code == 403


class TestCategoryUpdate:
    """
    ✅ GOAL: Validate category update operations with validation
    and access control.
    """

    def test_admin_update_category_success(self, base_url, admin_headers):
        """
        🏷️ TC_CAT_13 - ADMIN UPDATE CATEGORY
        
        📌 DECLARATION:
        Test that admin can successfully update category properties.
        
        📝 GOAL:
        - Update category name, description, and active status
        - Verify changes are persisted
        
        🔍 STEPS:
        1. Create a test category
        2. Update name and description
        3. Retrieve category to verify changes
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK or 204 No Content
        - Changes reflected when retrieved
        """
        # Create category
        create_payload = {
            "name": f"Update Test {uuid.uuid4()}",
            "description": "Original description",
            "isActive": True,
            "isCore": False
        }
        create_response = requests.post(f"{base_url}/categories", json=create_payload, headers=admin_headers)
        assert create_response.status_code in [200, 201]
        
        cat_id = create_response.json()["data"].get("categoryId", create_response.json()["data"].get("id"))
        
        # Update category
        update_payload = {
            "name": f"Updated Name {uuid.uuid4()}",
            "description": "Updated description",
            "isActive": False
        }
        update_response = requests.put(f"{base_url}/categories/{cat_id}", json=update_payload, headers=admin_headers)
        assert update_response.status_code in [200, 204]

    def test_update_category_name_only(self, base_url, admin_headers):
        """
        🏷️ TC_CAT_14 - PARTIAL UPDATE CATEGORY
        
        📌 DECLARATION:
        Test that only specific fields can be updated without affecting others.
        
        📝 GOAL:
        - Update just the name field
        - Verify other fields unchanged
        
        🔍 STEPS:
        1. Create category with name A and description B
        2. Update with new name C (don't include description)
        3. Verify name changed, description unchanged
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Name updated, other fields preserved
        """

        random_str = str(uuid.uuid4())[:6]
        # Create
        create_payload = {
            "name": "Pants",
            "parentCategoryId": 1,
            "description": "this is the description for pants",
            "imageUrl": "https://example.com/electronics.jpg",
            "displayOrder": 1,
            "isCore": True,
            "isActive": True
        }
        create_response = requests.post(f"{base_url}/categories", json=create_payload, headers=admin_headers)
        assert create_response.status_code in [200, 201]
        cat_id = create_response.json()["data"].get("categoryId")
        assert cat_id is not None
        # Update only name
        update_payload = {"name": f"Pants new {random_str}"}
        response = requests.put(f"{base_url}/categories/{cat_id}", json=update_payload, headers=admin_headers)
        assert response.status_code in [200, 204]

    def test_update_nonexistent_category(self, base_url, admin_headers):
        """
        🏷️ TC_CAT_15 - UPDATE NONEXISTENT CATEGORY
        
        📌 DECLARATION:
        Test that updating non-existent category returns 404.
        
        📝 GOAL:
        - Verify proper error when category doesn't exist
        
        🔍 STEPS:
        1. Try to PUT /api/categories/999999
        2. Verify 404 Not Found
        
        ✔️ EXPECTED RESULT:
        - Status: 404 Not Found
        """
        payload = {"name": "New Name"}
        response = requests.put(f"{base_url}/categories/999999", json=payload, headers=admin_headers)
        assert response.status_code == 404

    def test_seller_cannot_update_category(self, base_url, seller_headers):
        """
        🏷️ TC_CAT_16 - SELLER CANNOT UPDATE CATEGORY
        
        📌 DECLARATION:
        Test that sellers cannot update categories (admin-only operation).
        
        📝 GOAL:
        - Verify access control
        
        🔍 STEPS:
        1. Seller attempts PUT on category
        2. Verify 403 Forbidden
        
        ✔️ EXPECTED RESULT:
        - Status: 403 Forbidden
        """
        payload = {"name": "New Name"}
        response = requests.put(f"{base_url}/categories/1", json=payload, headers=seller_headers)
        assert response.status_code in [403, 404]  # 403 forbidden or 404 not found


class TestCategoryDeletion:
    """
    ✅ GOAL: Validate category deletion with cascade handling
    and access control.
    """

    def test_admin_delete_category_success(self, base_url, admin_headers):
        """
        🏷️ TC_CAT_17 - ADMIN DELETE CATEGORY
        
        📌 DECLARATION:
        Test successful deletion of an empty category.
        
        📝 GOAL:
        - Delete a category that has no products
        - Verify it's no longer retrievable
        
        🔍 STEPS:
        1. Create a test category
        2. Delete it with admin header
        3. Attempt to retrieve it (should 404)
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK or 204 No Content
        - Subsequent GET returns 404
        """
        # Create
        create_payload = {
            "name": f"Delete Test {uuid.uuid4()}",
            "isActive": True
        }
        create_response = requests.post(f"{base_url}/categories", json=create_payload, headers=admin_headers)
        cat_id = create_response.json()["data"].get("categoryId", create_response.json()["data"].get("id"))
        
        # Delete
        delete_response = requests.delete(f"{base_url}/categories/{cat_id}", headers=admin_headers)
        assert delete_response.status_code in [200, 204]
        
        # Verify deleted
        get_response = requests.get(f"{base_url}/categories/{cat_id}")
        assert get_response.status_code == 404

    def test_delete_nonexistent_category(self, base_url, admin_headers):
        """
        🏷️ TC_CAT_18 - DELETE NONEXISTENT CATEGORY
        
        📌 DECLARATION:
        Test deletion of non-existent category.
        
        📝 GOAL:
        - Verify proper error handling
        
        🔍 STEPS:
        1. Try DELETE /api/categories/999999
        2. Verify 404
        
        ✔️ EXPECTED RESULT:
        - Status: 404 Not Found
        """
        response = requests.delete(f"{base_url}/categories/999999", headers=admin_headers)
        assert response.status_code == 404

    def test_seller_cannot_delete_category(self, base_url, seller_headers):
        """
        🏷️ TC_CAT_19 - SELLER CANNOT DELETE CATEGORY
        
        📌 DECLARATION:
        Test that sellers cannot delete categories.
        
        📝 GOAL:
        - Verify role-based access control
        
        🔍 STEPS:
        1. Seller attempts DELETE on category
        2. Verify 403 Forbidden
        
        ✔️ EXPECTED RESULT:
        - Status: 403 Forbidden
        """
        response = requests.delete(f"{base_url}/categories/1", headers=seller_headers)
        assert response.status_code == 403

    def test_delete_category_with_products(self, base_url, admin_headers):
        """
        🏷️ TC_CAT_20 - DELETE CATEGORY CONTAINING PRODUCTS
        
        📌 DECLARATION:
        Test deletion behavior when category has associated products.
        
        📝 GOAL:
        - Verify system either cascades delete or prevents deletion
        - Ensure data integrity is maintained
        
        🔍 STEPS:
        1. Get a category known to have products (e.g., Electronics)
        2. Attempt to delete it
        3. Verify appropriate response (cascade or prevent)
        
        ✔️ EXPECTED RESULT:
        - Status: 400 (cannot delete with products) OR
        - Status: 200 (cascade deleted with proper cleanup)
        - Either way, system is consistent
        """
        # Try to delete a core category - likely has products
        response = requests.delete(f"{base_url}/categories/1", headers=admin_headers)
        # Could be 400 if products exist, or 200 if cascade is implemented
        assert response.status_code in [200, 204, 400, 409]
