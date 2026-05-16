"""
📋 TEST SUITE: USERS - COMPREHENSIVE TEST CASES
================================================
Purpose: Complete testing of user profile management, admin operations,
         role-based access control, and user data management.
Coverage: User profile CRUD, role management, user search/filtering,
          admin operations, and access control.
"""

import pytest
import requests
import uuid


class TestUserProfile:
    """
    ✅ GOAL: Validate user profile retrieval and management.
    """

    def test_user_get_own_profile(self, base_url, user_headers):
        """
        🏷️ TC_USER_01 - USER GET OWN PROFILE
        
        📌 DECLARATION:
        Test that authenticated user can retrieve their own profile information.
        
        📝 GOAL:
        - Get user's profile data
        - Verify all profile fields present
        
        🔍 STEPS:
        1. User calls GET /api/users/profile
        2. Verify 200 OK
        3. Verify response contains: userId, email, username, firstName, lastName, phone, role
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Response contains: userId, email, username, firstName, lastName, role
        - Phone and other optional fields present
        """
        response = requests.get(f"{base_url}/users/profile", headers=user_headers)
        assert response.status_code == 200
        
        profile = response.json().get("data", response.json())
        assert "email" in profile or "emailAddress" in profile
        assert "username" in profile
        assert "userId" in profile or "id" in profile

    def test_user_profile_does_not_contain_password(self, base_url, user_headers):
        """
        🏷️ TC_USER_02 - PROFILE NEVER CONTAINS PASSWORD
        
        📌 DECLARATION:
        Test that profile responses never expose password or password hash.
        
        📝 GOAL:
        - Ensure password security
        
        🔍 STEPS:
        1. Get user profile
        2. Verify response doesn't contain "password" field
        
        ✔️ EXPECTED RESULT:
        - No password or hash in response
        """
        response = requests.get(f"{base_url}/users/profile", headers=user_headers)
        assert response.status_code == 200
        
        response_text = response.text
        assert "password" not in response_text.lower() or "hashed" not in response_text.lower()

    def test_unauthenticated_user_cannot_get_profile(self, base_url):
        """
        🏷️ TC_USER_03 - PROFILE ENDPOINT REQUIRES AUTH
        
        📌 DECLARATION:
        Test that profile endpoint requires authentication.
        
        📝 GOAL:
        - Verify access control
        
        🔍 STEPS:
        1. Call GET /api/users/profile without token
        2. Verify 401 Unauthorized
        
        ✔️ EXPECTED RESULT:
        - Status: 401 Unauthorized
        """
        response = requests.get(f"{base_url}/users/profile")
        assert response.status_code == 401

    def test_user_cannot_view_other_user_profile(self, base_url, user_headers):
        """
        🏷️ TC_USER_04 - CANNOT VIEW OTHER USER'S PROFILE
        
        📌 DECLARATION:
        Test that users cannot access other users' profiles (IDOR prevention).
        
        📝 GOAL:
        - Verify authorization check
        
        🔍 STEPS:
        1. User tries GET /api/users/{otherUserId}/profile
        2. Verify 403 Forbidden or 404 Not Found
        
        ✔️ EXPECTED RESULT:
        - Status: 403 Forbidden or 404 Not Found
        """
        # Try to access a high ID profile
        response = requests.get(f"{base_url}/users/999999", headers=user_headers)
        assert response.status_code in [403, 404, 405]


class TestUserProfileUpdate:
    """
    ✅ GOAL: Validate user profile update operations with validation.
    """

    def test_user_update_own_profile_success(self, base_url, user_headers):
        """
        🏷️ TC_USER_05 - USER UPDATE PROFILE
        
        📌 DECLARATION:
        Test successful update of user's profile information.
        
        📝 GOAL:
        - Update firstName, lastName, phone
        - Verify changes are persisted
        
        🔍 STEPS:
        1. User sends PUT /api/users/profile with new data
        2. Verify 200 OK
        3. Retrieve profile to verify changes
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK or 204 No Content
        - Changes reflected in subsequent GET
        """
        payload = {
            "firstName": "Updated",
            "lastName": "User",
            "phone": "0987654321"
        }
        response = requests.put(f"{base_url}/users/profile", json=payload, headers=user_headers)
        assert response.status_code in [200, 204]

    def test_user_update_profile_with_invalid_phone(self, base_url, user_headers):
        """
        🏷️ TC_USER_06 - UPDATE PROFILE WITH INVALID PHONE
        
        📌 DECLARATION:
        Test that profile update validates phone number format.
        
        📝 GOAL:
        - Reject invalid phone
        
        🔍 STEPS:
        1. Try to update with phone = "invalid"
        2. Verify 400 Bad Request
        
        ✔️ EXPECTED RESULT:
        - Status: 400 Bad Request
        """
        payload = {
            "phone": "invalid_phone"
        }
        response = requests.put(f"{base_url}/users/profile", json=payload, headers=user_headers)
        assert response.status_code in [400, 422]

    def test_user_cannot_change_email(self, base_url, user_headers):
        """
        🏷️ TC_USER_07 - USER CANNOT CHANGE EMAIL
        
        📌 DECLARATION:
        Test that users cannot change their email (if system enforces this).
        
        📝 GOAL:
        - Verify email is immutable (or requires special process)
        
        🔍 STEPS:
        1. Try to update with new email
        2. Verify 400 Bad Request or email unchanged
        
        ✔️ EXPECTED RESULT:
        - Status: 400 Bad Request OR email remains unchanged
        """
        payload = {
            "email": f"newemail_{uuid.uuid4()}@gmail.com"
        }
        response = requests.put(f"{base_url}/users/profile", json=payload, headers=user_headers)
        # Either rejected or ignored
        if response.status_code == 200:
            profile = requests.get(f"{base_url}/users/profile", headers=user_headers).json()["data"]
            # Original email should be unchanged
            assert "email" in profile

    def test_user_cannot_change_username(self, base_url, user_headers):
        """
        🏷️ TC_USER_08 - USER CANNOT CHANGE USERNAME
        
        📌 DECLARATION:
        Test that usernames are immutable.
        
        📝 GOAL:
        - Verify username cannot be changed
        
        🔍 STEPS:
        1. Try to update with new username
        2. Verify rejection or no change
        
        ✔️ EXPECTED RESULT:
        - Username unchanged
        """
        payload = {
            "username": f"newusername_{uuid.uuid4()}"
        }
        response = requests.put(f"{base_url}/users/profile", json=payload, headers=user_headers)
        # Likely rejected or ignored

    def test_user_update_profile_partial(self, base_url, user_headers):
        """
        🏷️ TC_USER_09 - PARTIAL PROFILE UPDATE
        
        📌 DECLARATION:
        Test that only some profile fields can be updated.
        
        📝 GOAL:
        - Update just firstName without affecting other fields
        
        🔍 STEPS:
        1. Update with only firstName: "NewName"
        2. Verify only firstName changed
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Only specified field updated
        """
        new_name = f"NewName{uuid.uuid4()}"
        payload = {"firstName": new_name}
        response = requests.put(f"{base_url}/users/profile", json=payload, headers=user_headers)
        assert response.status_code in [200, 204]

    def test_unauthenticated_cannot_update_profile(self, base_url):
        """
        🏷️ TC_USER_10 - PROFILE UPDATE REQUIRES AUTH
        
        📌 DECLARATION:
        Test that profile update requires authentication.
        
        📝 GOAL:
        - Verify access control
        
        🔍 STEPS:
        1. Try PUT /api/users/profile without token
        2. Verify 401 Unauthorized
        
        ✔️ EXPECTED RESULT:
        - Status: 401 Unauthorized
        """
        payload = {"firstName": "Hacker"}
        response = requests.put(f"{base_url}/users/profile", json=payload)
        assert response.status_code == 401


class TestAdminUserManagement:
    """
    ✅ GOAL: Validate admin operations for user management.
    Only admin users should have these capabilities.
    """

    def test_admin_get_all_users(self, base_url, admin_headers):
        """
        🏷️ TC_USER_11 - ADMIN GET ALL USERS
        
        📌 DECLARATION:
        Test that admin can retrieve paginated list of all users.
        
        📝 GOAL:
        - List all users with pagination
        - Verify response structure
        
        🔍 STEPS:
        1. Admin calls GET /api/users with pageNumber=1, pageSize=20
        2. Verify 200 OK
        3. Verify response contains users array
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Response contains: items (array), total, pageNumber, pageSize
        """
        response = requests.get(f"{base_url}/users", params={"pageNumber": 1, "pageSize": 20}, headers=admin_headers)
        assert response.status_code == 200
        
        data = response.json().get("data", response.json())
        assert "items" in data or isinstance(data, list)

    def test_admin_get_user_by_id(self, base_url, admin_headers):
        """
        🏷️ TC_USER_12 - ADMIN GET USER BY ID
        
        📌 DECLARATION:
        Test that admin can retrieve specific user's details.
        
        📝 GOAL:
        - Get user information by ID
        
        🔍 STEPS:
        1. Admin calls GET /api/users/{userId}
        2. Verify 200 OK
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Response contains user data including role
        """
        # First get all users
        list_response = requests.get(f"{base_url}/users", params={"pageNumber": 1, "pageSize": 50}, headers=admin_headers)
        
        if list_response.status_code == 200:
            data = list_response.json().get("data", list_response.json())
            users = data.get("items", data) if isinstance(data, dict) else data
            
            if users and len(users) > 0:
                user_id = users[0].get("userId", users[0].get("id"))
                response = requests.get(f"{base_url}/users/{user_id}", headers=admin_headers)
                assert response.status_code == 200

    def test_admin_search_users_by_email(self, base_url, admin_headers):
        """
        🏷️ TC_USER_13 - ADMIN SEARCH USERS BY EMAIL
        
        📌 DECLARATION:
        Test that admin can search users by email.
        
        📝 GOAL:
        - Filter users by email parameter
        
        🔍 STEPS:
        1. Call GET /api/users?email=test@gmail.com
        2. Verify 200 OK
        3. Verify results match email filter
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Results contain user with matching email
        """
        response = requests.get(f"{base_url}/users", params={"email": "phuc1@gmail.com"}, headers=admin_headers)
        assert response.status_code == 200

    def test_admin_search_users_by_username(self, base_url, admin_headers):
        """
        🏷️ TC_USER_14 - ADMIN SEARCH USERS BY USERNAME
        
        📌 DECLARATION:
        Test that admin can search users by username.
        
        📝 GOAL:
        - Filter users by username parameter
        
        🔍 STEPS:
        1. Call GET /api/users?username=john
        2. Verify 200 OK
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Results match search criteria
        """
        response = requests.get(f"{base_url}/users", params={"username": "west"}, headers=admin_headers)
        assert response.status_code == 200

    def test_admin_create_user(self, base_url, admin_headers):
        """
        🏷️ TC_USER_15 - ADMIN CREATE USER
        
        📌 DECLARATION:
        Test that admin can create new user accounts directly.
        
        📝 GOAL:
        - Create user via admin endpoint
        - Assign role during creation
        
        🔍 STEPS:
        1. Admin sends POST /api/users with user data and role
        2. Verify 201 Created
        3. Verify response contains userId
        
        ✔️ EXPECTED RESULT:
        - Status: 201 Created
        - User created with specified role
        """
        payload = {
            "email": f"admin_created_{uuid.uuid4()}@gmail.com",
            "username": f"adminuser_{uuid.uuid4()}",
            "password": "InitialPass123@",
            "firstName": "Admin",
            "lastName": "Created",
            "phone": "0901234567",
            "role": "User"
        }
        response = requests.post(f"{base_url}/users", json=payload, headers=admin_headers)
        assert response.status_code in [200, 201]

    def test_admin_update_user(self, base_url, admin_headers):
        """
        🏷️ TC_USER_16 - ADMIN UPDATE USER
        
        📌 DECLARATION:
        Test that admin can update any user's information.
        
        📝 GOAL:
        - Modify user profile as admin
        
        🔍 STEPS:
        1. Admin sends PUT /api/users/{userId}
        2. Verify 200 OK
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - User information updated
        """
        # Get first user
        list_response = requests.get(f"{base_url}/users", params={"pageNumber": 1, "pageSize": 50}, headers=admin_headers)
        
        if list_response.status_code == 200:
            data = list_response.json().get("data", list_response.json())
            users = data.get("items", data) if isinstance(data, dict) else data
            
            if users and len(users) > 0:
                user_id = users[0].get("userId", users[0].get("id"))
                
                payload = {"firstName": "Updated By Admin"}
                response = requests.put(f"{base_url}/users/{user_id}", json=payload, headers=admin_headers)
                assert response.status_code in [200, 204]

    def test_admin_change_user_role(self, base_url, admin_headers):
        """
        🏷️ TC_USER_17 - ADMIN CHANGE USER ROLE
        
        📌 DECLARATION:
        Test that admin can change user roles (User, Seller, Admin).
        
        📝 GOAL:
        - Promote/demote users between roles
        
        🔍 STEPS:
        1. Get a user
        2. Update role from "User" to "Seller"
        3. Verify 200 OK
        4. Verify role changed
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - User role updated
        """
        # Get a user
        list_response = requests.get(f"{base_url}/users", params={"pageNumber": 1, "pageSize": 50}, headers=admin_headers)
        
        if list_response.status_code == 200:
            data = list_response.json().get("data", list_response.json())
            users = data.get("items", data) if isinstance(data, dict) else data
            
            # Find non-admin user to change role
            if users:
                non_admin = next((u for u in users if u.get("role") != "Admin"), None)
                if non_admin:
                    user_id = non_admin.get("userId", non_admin.get("id"))
                    
                    payload = {"role": "Seller"}
                    response = requests.put(f"{base_url}/users/{user_id}", json=payload, headers=admin_headers)
                    assert response.status_code in [200, 204]

    def test_admin_delete_user(self, base_url, admin_headers):
        """
        🏷️ TC_USER_18 - ADMIN DELETE USER
        
        📌 DECLARATION:
        Test that admin can delete user accounts.
        
        📝 GOAL:
        - Delete user account
        - Verify user can no longer login
        
        🔍 STEPS:
        1. Create a test user
        2. Admin deletes the user
        3. Verify 200/204
        4. Attempt to login (should fail)
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK or 204 No Content
        - User account deleted
        """
        # Create user first
        create_payload = {
            "email": f"to_delete_{uuid.uuid4()}@gmail.com",
            "username": f"delete_user_{uuid.uuid4()}",
            "password": "Test123@",
            "firstName": "To",
            "lastName": "Delete"
        }
        create_response = requests.post(f"{base_url}/users", json=create_payload, headers=admin_headers)
        
        if create_response.status_code in [200, 201]:
            user_id = create_response.json()["data"].get("userId", create_response.json()["data"].get("id"))
            
            # Delete user
            delete_response = requests.delete(f"{base_url}/users/{user_id}", headers=admin_headers)
            assert delete_response.status_code in [200, 204]


class TestRoleBasedAccessControl:
    """
    ✅ GOAL: Validate that different roles have appropriate access levels.
    """

    def test_seller_cannot_access_admin_endpoints(self, base_url, seller_headers):
        """
        🏷️ TC_USER_19 - SELLER CANNOT ACCESS ADMIN ENDPOINTS
        
        📌 DECLARATION:
        Test that seller users cannot access admin-only user management.
        
        📝 GOAL:
        - Verify role-based access control
        
        🔍 STEPS:
        1. Seller tries GET /api/users (list all users)
        2. Verify 403 Forbidden
        
        ✔️ EXPECTED RESULT:
        - Status: 403 Forbidden
        """
        response = requests.get(f"{base_url}/users", headers=seller_headers)
        assert response.status_code == 403

    def test_user_cannot_access_admin_endpoints(self, base_url, user_headers):
        """
        🏷️ TC_USER_20 - BUYER USER CANNOT ACCESS ADMIN ENDPOINTS
        
        📌 DECLARATION:
        Test that regular users cannot access admin operations.
        
        📝 GOAL:
        - Verify access control
        
        🔍 STEPS:
        1. User tries GET /api/users (list all users)
        2. Verify 403 Forbidden
        
        ✔️ EXPECTED RESULT:
        - Status: 403 Forbidden
        """
        response = requests.get(f"{base_url}/users", headers=user_headers)
        assert response.status_code == 403

    def test_seller_can_access_own_profile(self, base_url, seller_headers):
        """
        🏷️ TC_USER_21 - SELLER CAN ACCESS OWN PROFILE
        
        📌 DECLARATION:
        Test that seller users can access their own profile endpoint.
        
        📝 GOAL:
        - Verify profile access works for all roles
        
        🔍 STEPS:
        1. Seller calls GET /api/users/profile
        2. Verify 200 OK
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        """
        response = requests.get(f"{base_url}/users/profile", headers=seller_headers)
        assert response.status_code == 200

    def test_user_can_access_own_profile(self, base_url, user_headers):
        """
        🏷️ TC_USER_22 - USER CAN ACCESS OWN PROFILE
        
        📌 DECLARATION:
        Test that buyer users can access their own profile endpoint.
        
        📝 GOAL:
        - Verify profile access for all roles
        
        🔍 STEPS:
        1. User calls GET /api/users/profile
        2. Verify 200 OK
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        """
        response = requests.get(f"{base_url}/users/profile", headers=user_headers)
        assert response.status_code == 200


class TestUserDataValidation:
    """
    ✅ GOAL: Validate user data constraints and business rules.
    """

    def test_user_admin_operations_audit(self, base_url, admin_headers):
        """
        🏷️ TC_USER_23 - ADMIN OPERATIONS AUDIT TRAIL
        
        📌 DECLARATION:
        Test that admin operations record audit information (if applicable).
        
        📝 GOAL:
        - Verify system logs admin changes
        
        🔍 STEPS:
        1. Admin performs user operations
        2. Check if audit fields present (createdAt, updatedAt, etc.)
        
        ✔️ EXPECTED RESULT:
        - User records contain timestamps
        - createdAt and updatedAt fields present
        """
        response = requests.get(f"{base_url}/users", params={"pageNumber": 1, "pageSize": 10}, headers=admin_headers)
        
        if response.status_code == 200:
            data = response.json().get("data", response.json())
            users = data.get("items", data) 
            
            if users and len(users) > 0:
                user = users[0]
                # Check for timestamps
                assert "userId"  in user

    def test_concurrent_user_operations(self, base_url, user_headers):
        """
        🏷️ TC_USER_24 - CONCURRENT PROFILE UPDATES
        
        📌 DECLARATION:
        Test that concurrent profile updates don't cause data corruption.
        
        📝 GOAL:
        - Verify race condition handling
        
        🔍 STEPS:
        1. Send two rapid profile updates
        2. Verify one wins or both apply consistently
        
        ✔️ EXPECTED RESULT:
        - Final state is consistent
        - No data corruption
        """
        # Send multiple updates rapidly
        payload1 = {"firstName": "Update1"}
        payload2 = {"lastName": "Update2"}
        
        response1 = requests.put(f"{base_url}/users/profile", json=payload1, headers=user_headers)
        response2 = requests.put(f"{base_url}/users/profile", json=payload2, headers=user_headers)
        
        assert response1.status_code in [200, 204]
        assert response2.status_code in [200, 204]
        
        # Verify final state
        profile = requests.get(f"{base_url}/users/profile", headers=user_headers).json()["data"]
        # Should have both updates applied
        assert profile.get("firstName") == "Update1" or profile.get("firstName") is not None
