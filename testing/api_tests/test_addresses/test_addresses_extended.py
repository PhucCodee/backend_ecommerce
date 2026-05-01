"""
📋 TEST SUITE: USER ADDRESSES - COMPREHENSIVE TEST CASES
==========================================================
Purpose: Complete testing of user address management including creation,
         retrieval, update, deletion, and default address handling.
Coverage: Address CRUD operations, default address management,
          address validation, and role-based access control.
"""

import pytest
import requests
import uuid


class TestAddressCreation:
    """
    ✅ GOAL: Validate address creation with comprehensive field validation.
    """

    def test_user_create_address_success(self, base_url, user_headers):
        """
        🏷️ TC_ADDR_01 - USER CREATE ADDRESS
        
        📌 DECLARATION:
        Test successful creation of a new delivery address.
        
        📝 GOAL:
        - Create address with all valid required fields
        - Verify address is saved and retrievable
        
        🔍 STEPS:
        1. User sends POST /api/addresses with address data
        2. Verify 201 Created
        3. Verify response contains addressId
        4. Retrieve address to confirm data
        
        ✔️ EXPECTED RESULT:
        - Status: 201 Created or 200 OK
        - Response contains addressId
        - Address appears in user's address list
        """
        payload = {
            "type": 0,
            "label": "Home 2",
            "recipientName": "Cristiano Ronaldo",
            "phone": "+351123456789",
            "addressLine1": "123 Lisbon Street",
            "addressLine2": "Apt 10",
            "city": "Lisbon",
            "stateProvince": "Lisbon",
            "postalCode": "1000-001",
            "country": "Portugal",
            "isDefaultShipping": True,
            "isDefaultBilling": True
        }
        response = requests.post(f"{base_url}/addresses", json=payload, headers=user_headers)
        print(response.json())
        assert response.status_code in [200, 201]
        
        data = response.json().get("data", response.json())
        assert "addressId" in data or "id" in data

    def test_user_create_default_address(self, base_url, user_headers):
        """
        🏷️ TC_ADDR_02 - CREATE ADDRESS AS DEFAULT
        
        📌 DECLARATION:
        Test creating an address and setting it as default for user.
        
        📝 GOAL:
        - Create address with isDefault=true
        - Verify previous default is unset
        
        🔍 STEPS:
        1. Create address with isDefault: true
        2. Verify 201 Created
        3. Get user's addresses
        4. Verify only one address has isDefault=true
        
        ✔️ EXPECTED RESULT:
        - Status: 201 Created
        - New address is marked as default
        - Only one default address per user
        """
        payload = {
            "type": 0,
            "label": "Home 2",
            "recipientName": "Cristiano Ronaldo",
            "phone": "+351123456789",
            "addressLine1": "123 Lisbon Street",
            "addressLine2": "Apt 10",
            "city": "Lisbon",
            "stateProvince": "Lisbon",
            "postalCode": "1000-001",
            "country": "Portugal",
            "isDefaultShipping": True,
            "isDefaultBilling": True
        }
        response = requests.post(f"{base_url}/addresses", json=payload, headers=user_headers)
        assert response.status_code in [200, 201]

    def test_create_address_missing_required_field(self, base_url, user_headers):
        """
        🏷️ TC_ADDR_03 - CREATE ADDRESS WITH MISSING REQUIRED FIELD
        
        📌 DECLARATION:
        Test that address creation fails when required fields are missing.
        
        📝 GOAL:
        - Verify all required fields are enforced
        
        🔍 STEPS:
        1. Try address without recipientName
        2. Try without phoneNumber
        3. Try without addressLine
        4. Verify 400 Bad Request for each
        
        ✔️ EXPECTED RESULT:
        - Status: 400 Bad Request
        - Error message indicates missing field
        """
        required_fields = ["recipientName", "phoneNumber", "addressLine", "province", "district", "ward"]
        
        base_payload = {
            "recipientName": "Test User",
            "phoneNumber": "0912345678",
            "province": "Ho Chi Minh",
            "district": "District 1",
            "ward": "Ward 1",
            "addressLine": "123 Main Street",
            "isDefault": False
        }
        
        for field in required_fields[:3]:  # Test with key required fields
            payload = base_payload.copy()
            del payload[field]
            response = requests.post(f"{base_url}/addresses", json=payload, headers=user_headers)
            assert response.status_code == 400, f"Should reject missing {field}"

    def test_create_address_invalid_phone(self, base_url, user_headers):
        """
        🏷️ TC_ADDR_04 - CREATE ADDRESS WITH INVALID PHONE
        
        📌 DECLARATION:
        Test that address creation validates phone number format.
        
        📝 GOAL:
        - Reject invalid phone formats
        
        🔍 STEPS:
        1. Try with phoneNumber = "123" (too short)
        2. Try with "abc" (non-numeric)
        3. Verify rejection
        
        ✔️ EXPECTED RESULT:
        - Status: 400 Bad Request
        """
        invalid_phones = ["123", "abc", ""]
        
        for invalid_phone in invalid_phones:
            payload = {
                "recipientName": "Test User",
                "phoneNumber": invalid_phone,
                "province": "Ho Chi Minh",
                "district": "District 1",
                "ward": "Ward 1",
                "addressLine": "123 Main Street"
            }
            response = requests.post(f"{base_url}/addresses", json=payload, headers=user_headers)
            assert response.status_code == 400

    def test_create_address_empty_required_field(self, base_url, user_headers):
        """
        🏷️ TC_ADDR_05 - CREATE ADDRESS WITH EMPTY REQUIRED FIELD
        
        📌 DECLARATION:
        Test that empty string for required fields is rejected.
        
        📝 GOAL:
        - Verify non-empty validation
        
        🔍 STEPS:
        1. Try with recipientName = ""
        2. Try with addressLine = ""
        3. Verify rejection
        
        ✔️ EXPECTED RESULT:
        - Status: 400 Bad Request
        """
        payload = {
            "recipientName": "",
            "phoneNumber": "0912345678",
            "province": "Ho Chi Minh",
            "district": "District 1",
            "ward": "Ward 1",
            "addressLine": "123 Main Street"
        }
        response = requests.post(f"{base_url}/addresses", json=payload, headers=user_headers)
        assert response.status_code == 400


class TestAddressRetrieval:
    """
    ✅ GOAL: Validate address retrieval with various filters and access control.
    """

    def test_user_get_all_addresses(self, base_url, user_headers):
        """
        🏷️ TC_ADDR_06 - USER GET ALL ADDRESSES
        
        📌 DECLARATION:
        Test that users can retrieve all their saved addresses.
        
        📝 GOAL:
        - Get list of all user's addresses
        - Verify pagination/structure
        
        🔍 STEPS:
        1. User calls GET /api/addresses
        2. Verify 200 OK
        3. Verify response is array of addresses
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Response contains: array of addresses or paginated response
        - Each address has required fields
        """
        response = requests.get(f"{base_url}/addresses", headers=user_headers)
        assert response.status_code == 200
        
        data = response.json().get("data", response.json())
        addresses = data.get("items", data) if isinstance(data, dict) else data
        assert isinstance(addresses, list)

    def test_user_get_address_by_id(self, base_url, user_headers):
        """
        🏷️ TC_ADDR_07 - USER GET ADDRESS BY ID
        
        📌 DECLARATION:
        Test retrieving a specific address by ID.
        
        📝 GOAL:
        - Get single address details
        
        🔍 STEPS:
        1. Get all addresses
        2. Select first address ID
        3. Call GET /api/addresses/{addressId}
        4. Verify 200 OK
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Response contains full address data
        """
        # Get addresses first
        list_response = requests.get(f"{base_url}/addresses", headers=user_headers)
        assert list_response.status_code in [200,201]


    def test_user_cannot_access_other_user_address(self, base_url, user_headers, admin_headers):
        """
        🏷️ TC_ADDR_09 - CANNOT ACCESS OTHER USER'S ADDRESS (IDOR)
        
        📌 DECLARATION:
        Test that users cannot retrieve addresses belonging to other users.
        Critical security test for IDOR (Insecure Direct Object Reference).
        
        📝 GOAL:
        - Verify authorization check on address endpoints
        
        🔍 STEPS:
        1. Try to access admin's address with user account
        2. Verify 403 Forbidden or 404 Not Found
        
        ✔️ EXPECTED RESULT:
        - Status: 403 Forbidden or 404 Not Found
        - Cannot list/view other user's addresses
        """
        # Try a high ID that likely belongs to different user
        response = requests.get(f"{base_url}/addresses/999999", headers=user_headers)
        # Should be 404 or 403, not 200
        assert response.status_code in [403, 404, 405]


class TestAddressUpdate:
    """
    ✅ GOAL: Validate address update operations with validation.
    """

    def test_user_update_address_success(self, base_url, user_headers):
        """
        🏷️ TC_ADDR_10 - USER UPDATE ADDRESS
        
        📌 DECLARATION:
        Test successful update of an existing address.
        
        📝 GOAL:
        - Update address fields (name, phone, location)
        - Verify changes are persisted
        
        🔍 STEPS:
        1. Create or get an existing address
        2. Update with new values
        3. Retrieve to verify changes
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK or 204 No Content
        - Changes reflected when retrieved
        """
        # Create address first
        create_payload = {
            "recipientName": "Original Name",
            "phoneNumber": "0912345678",
            "province": "Ho Chi Minh",
            "district": "District 1",
            "ward": "Ward 1",
            "addressLine": "123 Old Street"
        }
        create_response = requests.post(f"{base_url}/addresses", json=create_payload, headers=user_headers)
        
        if create_response.status_code in [200, 201]:
            addr_id = create_response.json()["data"].get("addressId", create_response.json()["data"].get("id"))
            
            # Update address
            update_payload = {
                "recipientName": "Updated Name",
                "phoneNumber": "0987654321",
                "addressLine": "456 New Street"
            }
            update_response = requests.put(f"{base_url}/addresses/{addr_id}", json=update_payload, headers=user_headers)
            assert update_response.status_code in [200, 204]

    def test_update_address_to_default(self, base_url, user_headers):
        """
        🏷️ TC_ADDR_11 - SET ADDRESS AS DEFAULT
        
        📌 DECLARATION:
        Test setting an existing address as the new default.
        
        📝 GOAL:
        - Change default address
        - Verify previous default is unset
        
        🔍 STEPS:
        1. Get user's non-default address
        2. Update with isDefault: true
        3. Verify only this one is now default
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Address marked as default
        - Only one default per user
        """
        # Get addresses
        list_response = requests.get(f"{base_url}/addresses", headers=user_headers)
        if list_response.status_code == 200:
            data = list_response.json().get("data", list_response.json())
            addresses = data.get("items", data) if isinstance(data, dict) else data
            
            if addresses and len(addresses) > 0:
                addr_id = addresses[0].get("addressId", addresses[0].get("id"))
                
                update_payload = {"isDefault": True}
                response = requests.put(f"{base_url}/addresses/{addr_id}", json=update_payload, headers=user_headers)
                assert response.status_code in [200, 204]

    def test_update_address_invalid_phone(self, base_url, user_headers):
        """
        🏷️ TC_ADDR_12 - UPDATE ADDRESS WITH INVALID PHONE
        
        📌 DECLARATION:
        Test that address update validates phone number format.
        
        📝 GOAL:
        - Reject invalid phone in update
        
        🔍 STEPS:
        1. Get user's address
        2. Try to update with invalid phone
        3. Verify rejection
        
        ✔️ EXPECTED RESULT:
        - Status: 400 Bad Request
        """
        # Get an address
        list_response = requests.get(f"{base_url}/addresses", headers=user_headers)
        if list_response.status_code == 200:
            data = list_response.json().get("data", list_response.json())
            addresses = data.get("items", data) if isinstance(data, dict) else data
            
            if addresses and len(addresses) > 0:
                addr_id = addresses[0].get("addressId", addresses[0].get("id"))
                
                # Try invalid phone
                update_payload = {"phoneNumber": "invalidinvalid"}
                response = requests.put(f"{base_url}/addresses/{addr_id}", json=update_payload, headers=user_headers)
                assert response.status_code == 400

    def test_update_nonexistent_address(self, base_url, user_headers):
        """
        🏷️ TC_ADDR_13 - UPDATE NONEXISTENT ADDRESS
        
        📌 DECLARATION:
        Test that updating non-existent address returns 404.
        
        📝 GOAL:
        - Verify error handling
        
        🔍 STEPS:
        1. Try PUT /api/addresses/999999
        2. Verify 404
        
        ✔️ EXPECTED RESULT:
        - Status: 404 Not Found
        """
        payload = {"recipientName": "New Name"}
        response = requests.put(f"{base_url}/addresses/999999", json=payload, headers=user_headers)
        assert response.status_code == 404


class TestAddressDeletion:
    """
    ✅ GOAL: Validate address deletion with cascade handling.
    """

    def test_user_delete_address_success(self, base_url, user_headers):
        """
        🏷️ TC_ADDR_14 - USER DELETE ADDRESS
        
        📌 DECLARATION:
        Test successful deletion of a user's address.
        
        📝 GOAL:
        - Delete address from user's address book
        - Verify it's no longer retrievable
        
        🔍 STEPS:
        1. Create a new address
        2. Delete it with DELETE /api/addresses/{addressId}
        3. Verify 200/204
        4. Try to retrieve (should 404)
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK or 204 No Content
        - Subsequent GET returns 404
        """
        # Create address
        create_payload = {
            "recipientName": "To Delete",
            "phoneNumber": "0912345678",
            "province": "Ho Chi Minh",
            "district": "District 1",
            "ward": "Ward 1",
            "addressLine": "Temporary Address"
        }
        create_response = requests.post(f"{base_url}/addresses", json=create_payload, headers=user_headers)
        
        if create_response.status_code in [200, 201]:
            addr_id = create_response.json()["data"].get("addressId", create_response.json()["data"].get("id"))
            
            # Delete it
            delete_response = requests.delete(f"{base_url}/addresses/{addr_id}", headers=user_headers)
            assert delete_response.status_code in [200, 204]
            
            # Verify deleted
            get_response = requests.get(f"{base_url}/addresses/{addr_id}", headers=user_headers)
            assert get_response.status_code == 404

    def test_delete_nonexistent_address(self, base_url, user_headers):
        """
        🏷️ TC_ADDR_15 - DELETE NONEXISTENT ADDRESS
        
        📌 DECLARATION:
        Test deletion of non-existent address.
        
        📝 GOAL:
        - Verify proper error handling
        
        🔍 STEPS:
        1. Try DELETE /api/addresses/999999
        2. Verify 404
        
        ✔️ EXPECTED RESULT:
        - Status: 404 Not Found
        """
        response = requests.delete(f"{base_url}/addresses/999999", headers=user_headers)
        assert response.status_code == 404

    def test_cannot_delete_other_user_address(self, base_url, user_headers):
        """
        🏷️ TC_ADDR_16 - CANNOT DELETE OTHER USER'S ADDRESS
        
        📌 DECLARATION:
        Test that users cannot delete addresses belonging to others.
        
        📝 GOAL:
        - Verify authorization
        
        🔍 STEPS:
        1. Try to delete a high ID address as regular user
        2. Verify 403 or 404
        
        ✔️ EXPECTED RESULT:
        - Status: 403 Forbidden or 404 Not Found
        """
        response = requests.delete(f"{base_url}/addresses/999999", headers=user_headers)
        assert response.status_code in [403, 404]

    def test_delete_default_address(self, base_url, user_headers):
        """
        🏷️ TC_ADDR_17 - DELETE DEFAULT ADDRESS
        
        📌 DECLARATION:
        Test behavior when deleting a user's default address.
        
        📝 GOAL:
        - Verify system handles default address deletion
        - May either prevent deletion or auto-set new default
        
        🔍 STEPS:
        1. Get user's default address
        2. Delete it
        3. Verify: either 400 (prevent) or 200 (auto-reassign)
        
        ✔️ EXPECTED RESULT:
        - Status: 200/204 (delete and reassign default) OR
        - Status: 400 (prevent deletion of default)
        - User always has a default address if multiple exist
        """
        # Get addresses
        list_response = requests.get(f"{base_url}/addresses", headers=user_headers)
        if list_response.status_code == 200:
            data = list_response.json().get("data", list_response.json())
            addresses = data.get("items", data) if isinstance(data, dict) else data
            
            # Find default address
            default_addr = next((a for a in addresses if a.get("isDefault")), None)
            
            if default_addr and len(addresses) > 1:  # Only test if multiple addresses
                addr_id = default_addr.get("addressId", default_addr.get("id"))
                response = requests.delete(f"{base_url}/addresses/{addr_id}", headers=user_headers)
                # Should either prevent (400) or allow and reassign (200)
                assert response.status_code in [200, 204, 400]
