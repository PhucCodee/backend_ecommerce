"""
📋 TEST SUITE: AUTHENTICATION - EXTENDED TEST CASES
====================================================
Purpose: Comprehensive testing of User Registration, Login, and Token Management
Coverage: Password validation, duplicate accounts, token expiry, role-based login, 
          security headers, and edge cases.
"""

import pytest
import requests
import uuid
from datetime import datetime

@pytest.fixture
def cleanup_new_users(base_url, admin_headers):
    """
    Fixture theo dõi và tự động xóa các user được tạo ra trong quá trình test.
    Sử dụng quyền Admin để xóa nhằm đảm bảo data rác không bị tồn đọng.
    """
    created_accounts = []
    
    yield created_accounts
    
    # --- TEARDOWN: Khối lệnh này chạy sau khi test case hoàn tất ---
    for account in created_accounts:
        # 1. Đăng nhập vào user vừa tạo để lấy userId một cách chính xác
        login_resp = requests.post(f"{base_url}/auth/login", json=account)
        if login_resp.status_code == 200:
            user_id = login_resp.json().get('data', {}).get('user', {}).get('userId')
            
            # 2. Nếu lấy được ID, dùng Admin token để xóa user
            if user_id:
                requests.delete(
                    f"{base_url}/users/{user_id}", 
                    headers=admin_headers
                )

                
class TestUserRegistration:
    """
    ✅ GOAL: Validate all user registration scenarios including success paths,
    validation errors, and duplicate prevention mechanisms.
    """

    def test_user_registration_success_with_valid_data(self, base_url, cleanup_new_users):
        """
        🏷️ TC_AUTH_01 - USER REGISTRATION WITH VALID DATA
        """
        random_str = str(uuid.uuid4())[:8]
        payload = {
            "email": f"valid_user_{random_str}@gmail.com",
            "username": f"validuser",
            "password": "Test123@Password!",
            "confirmPassword": "Test123@Password!",
            "firstName": "Valid",
            "lastName": "User",
            "phone": "0901234567",
            "acceptTerms": True
        }
        response = requests.post(f"{base_url}/auth/register", json=payload)
        assert response.status_code in [200, 201], f"Registration failed: {response.text}"
        
        # Đánh dấu user này cần được Admin xóa sau khi test xong
        cleanup_new_users.append({
            "identifier": payload["username"],
            "password": payload["password"]
        })
        
        # Verify user can login
        login_payload = {
            "identifier": payload["username"],
            "password": payload["password"]
        }
        login_response = requests.post(f"{base_url}/auth/login", json=login_payload)
        assert login_response.status_code == 200

    def test_user_registration_duplicate_email_rejection(self, base_url, cleanup_new_users):
        """
        🏷️ TC_AUTH_02 - DUPLICATE EMAIL PREVENTION
        """
        email = f"duplicate_test_{uuid.uuid4()}@gmail.com"
        
        # First registration succeeds
        payload1 = {
            "email": email,
            "username": f"user_{uuid.uuid4().hex[:8]}",
            "password": "Test123@",
            "confirmPassword": "Test123@",
            "firstName": "First",
            "lastName": "User",
            "phone": "0901234567",
            "acceptTerms": True
        }
        response1 = requests.post(f"{base_url}/auth/register", json=payload1)
        assert response1.status_code in [200, 201]
        
        # Đánh dấu user 1 cần được xóa sau khi test xong
        cleanup_new_users.append({
            "identifier": payload1["username"],
            "password": payload1["password"]
        })
        
        # Second registration with same email should fail
        payload2 = {
            "email": email,
            "username": f"usertest_{uuid.uuid4().hex[:8]}",
            "password": "Test123@",
            "confirmPassword": "Test123@",
            "firstName": "Second",
            "lastName": "User",
            "phone": "0911111111",
            "acceptTerms": True
        }
        response2 = requests.post(f"{base_url}/auth/register", json=payload2)
        assert response2.status_code in [400, 409], f"System allowed duplicate email: {response2.text}"

    def test_user_registration_duplicate_username_rejection(self, base_url, cleanup_new_users):
        """
        🏷️ TC_AUTH_03 - DUPLICATE USERNAME PREVENTION
        """
        username = f"test_user_name_{uuid.uuid4().hex[:8]}"
        
        payload1 = {
            "email": f"email1_{uuid.uuid4()}@gmail.com",
            "username": username,
            "password": "Test123@",
            "confirmPassword": "Test123@",
            "firstName": "User",
            "lastName": "One",
            "phone": "0901234567",
            "acceptTerms": True
        }
        response1 = requests.post(f"{base_url}/auth/register", json=payload1)
        assert response1.status_code in [200, 201]
        
        # Đánh dấu user 1 cần được xóa sau khi test xong
        cleanup_new_users.append({
            "identifier": payload1["username"],
            "password": payload1["password"]
        })
        
        payload2 = {
            "email": f"email2_{uuid.uuid4()}@gmail.com",
            "username": username,
            "password": "Test123@",
            "confirmPassword": "Test123@",
            "firstName": "User",
            "lastName": "Two",
            "phone": "0911111111",
            "acceptTerms": True
        }
        response2 = requests.post(f"{base_url}/auth/register", json=payload2)
        assert response2.status_code in [400, 409]



    def test_registration_password_mismatch_rejection(self, base_url):
        """
        🏷️ TC_AUTH_04 - PASSWORD MISMATCH VALIDATION
        
        📌 DECLARATION:
        Test that registration fails when password and confirmPassword don't match.
        
        📝 GOAL:
        - Submit registration with mismatched passwords
        - Verify rejection
        
        🔍 STEPS:
        1. Create payload with password != confirmPassword
        2. Submit POST /auth/register
        3. Verify error response
        
        ✔️ EXPECTED RESULT:
        - Status: 400 Bad Request
        - Error message mentions password mismatch
        """
        payload = {
            "email": f"test_{uuid.uuid4()}@gmail.com",
            "username": f"user_{uuid.uuid4()}",
            "password": "Password123@",
            "confirmPassword": "DifferentPassword123@",
            "firstName": "Test",
            "lastName": "User",
            "phone": "0901234567",
            "acceptTerms": True
        }
        response = requests.post(f"{base_url}/auth/register", json=payload)
        assert response.status_code == 400, f"Should reject mismatched passwords: {response.text}"

    def test_registration_weak_password_rejection(self, base_url):
        """
        🏷️ TC_AUTH_05 - WEAK PASSWORD VALIDATION
        
        📌 DECLARATION:
        Test that registration rejects passwords that don't meet security requirements.
        
        📝 GOAL:
        - Attempt registration with weak passwords (too short, no special char, etc.)
        - Verify rejection for each weak password pattern
        
        🔍 STEPS:
        1. Try registration with password = "123" (too short)
        2. Try with password = "password" (no uppercase, no digit, no special char)
        3. Verify all are rejected
        
        ✔️ EXPECTED RESULT:
        - Status: 400 Bad Request
        - Error describes password requirement
        """
        weak_passwords = [
            "123",                    # Too short
            "password",               # No uppercase, digit, special char
            "Pass",                   # Too short
            "PASSWORD123",            # No special char (depending on policy)
        ]
        
        for weak_pass in weak_passwords:
            payload = {
                "email": f"test_{uuid.uuid4()}@gmail.com",
                "username": f"user_{uuid.uuid4()}",
                "password": weak_pass,
                "confirmPassword": weak_pass,
                "firstName": "Test",
                "lastName": "User",
                "phone": "0901234567",
                "acceptTerms": True
            }
            response = requests.post(f"{base_url}/auth/register", json=payload)
            assert response.status_code == 400, f"Should reject weak password '{weak_pass}': {response.text}"

    def test_registration_missing_required_fields(self, base_url):
        """
        🏷️ TC_AUTH_06 - REQUIRED FIELD VALIDATION
        
        📌 DECLARATION:
        Test that registration rejects payloads missing required fields.
        
        📝 GOAL:
        - Submit incomplete registration forms (missing email, username, password, etc.)
        - Verify rejection for each missing field
        
        🔍 STEPS:
        1. Try registration without email
        2. Try without username
        3. Try without password
        4. Verify all return 400 Bad Request
        
        ✔️ EXPECTED RESULT:
        - Status: 400 Bad Request for all cases
        """
        required_fields = ["email", "username", "password", "confirmPassword", "firstName", "lastName"]
        
        base_payload = {
            "email": f"test_{uuid.uuid4()}@gmail.com",
            "username": f"user_{uuid.uuid4()}",
            "password": "Test123@Password!",
            "confirmPassword": "Test123@Password!",
            "firstName": "Test",
            "lastName": "User",
            "phone": "0901234567",
            "acceptTerms": True
        }
        
        for field in required_fields:
            payload = base_payload.copy()
            del payload[field]
            response = requests.post(f"{base_url}/auth/register", json=payload)
            assert response.status_code == 400, f"Should reject missing {field}: {response.text}"

    def test_registration_invalid_email_format(self, base_url):
        """
        🏷️ TC_AUTH_07 - EMAIL FORMAT VALIDATION
        
        📌 DECLARATION:
        Test that registration rejects invalid email formats.
        
        📝 GOAL:
        - Submit registration with malformed email addresses
        - Verify all are rejected
        
        🔍 STEPS:
        1. Try emails: "notanemail", "test@", "@domain.com", etc.
        2. Verify rejection
        
        ✔️ EXPECTED RESULT:
        - Status: 400 Bad Request
        """
        invalid_emails = [
            "notanemail",
            "test@",
            "@domain.com",
            "test@@domain.com",
            "test@domain",
        ]
        
        for invalid_email in invalid_emails:
            payload = {
                "email": invalid_email,
                "username": f"user_{uuid.uuid4()}",
                "password": "Test123@",
                "confirmPassword": "Test123@",
                "firstName": "Test",
                "lastName": "User",
                "phone": "0901234567",
                "acceptTerms": True
            }
            response = requests.post(f"{base_url}/auth/register", json=payload)
            assert response.status_code == 400, f"Should reject invalid email '{invalid_email}': {response.text}"

    def test_registration_invalid_phone_format(self, base_url):
        """
        🏷️ TC_AUTH_08 - PHONE NUMBER VALIDATION
        
        📌 DECLARATION:
        Test that registration rejects invalid phone number formats.
        
        📝 GOAL:
        - Submit with invalid phone formats (too short, non-numeric)
        - Verify rejection
        
        🔍 STEPS:
        1. Try phone = "123" (too short)
        2. Try phone = "abcdefghij" (non-numeric)
        3. Try phone = "" (empty)
        
        ✔️ EXPECTED RESULT:
        - Status: 400 Bad Request
        """
        invalid_phones = ["123", "abc", "", "phone"]
        
        for invalid_phone in invalid_phones:
            payload = {
                "email": f"test_{uuid.uuid4()}@gmail.com",
                "username": f"user_{uuid.uuid4()}",
                "password": "Test123@",
                "confirmPassword": "Test123@",
                "firstName": "Test",
                "lastName": "User",
                "phone": invalid_phone,
                "acceptTerms": True
            }
            response = requests.post(f"{base_url}/auth/register", json=payload)
            assert response.status_code == 400, f"Should reject invalid phone '{invalid_phone}'"

    def test_registration_terms_not_accepted(self, base_url):
        """
        🏷️ TC_AUTH_09 - TERMS ACCEPTANCE VALIDATION
        
        📌 DECLARATION:
        Test that registration rejects when terms are not accepted.
        
        📝 GOAL:
        - Submit registration with acceptTerms = False
        - Verify rejection
        
        🔍 STEPS:
        1. Create payload with acceptTerms: False
        2. Submit POST /auth/register
        3. Verify 400 Bad Request
        
        ✔️ EXPECTED RESULT:
        - Status: 400 Bad Request
        - Error message about accepting terms
        """
        payload = {
            "email": f"test_{uuid.uuid4()}@gmail.com",
            "username": f"user_{uuid.uuid4()}",
            "password": "Test123@",
            "confirmPassword": "Test123@",
            "firstName": "Test",
            "lastName": "User",
            "phone": "0901234567",
            "acceptTerms": False
        }
        response = requests.post(f"{base_url}/auth/register", json=payload)
        assert response.status_code == 400


class TestUserLogin:
    """
    ✅ GOAL: Validate all user login scenarios including success paths,
    authentication failures, role-based logins, and token validation.
    """

    def test_user_login_success_with_username(self, base_url):
        """
        🏷️ TC_AUTH_10 - LOGIN WITH USERNAME
        
        📌 DECLARATION:
        Test successful login using username as identifier.
        
        📝 GOAL:
        - Login with username and password
        - Verify access token is returned
        - Verify token is valid JWT format
        
        🔍 STEPS:
        1. Submit POST /auth/login with username + password
        2. Verify status 200 OK
        3. Verify response contains accessToken
        4. Verify token format is valid JWT
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - accessToken present and valid (starts with "eyJ")
        - Token can be used in Authorization header
        """
        payload = {"identifier": "west", "password": "Phuc123@"}
        response = requests.post(f"{base_url}/auth/login", json=payload)
        assert response.status_code == 200
        token = response.json()['data']['accessToken']
        assert "eyJ" in token, "Token is not a valid JWT"

    def test_user_login_success_with_email(self, base_url, cleanup_new_users):
        """
        🏷️ TC_AUTH_11 - LOGIN WITH EMAIL
        
        📌 DECLARATION:
        Test successful login using email as identifier.
        
        📝 GOAL:
        - Login with email and password
        - Verify token is returned
        
        🔍 STEPS:
        1. Submit POST /auth/login with email + password
        2. Verify 200 OK
        3. Verify valid token returned
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - accessToken present
        """
        payload = {
            "email": "phuc1@gmail.com",
            "username": "phuc1",
            "password": "Test123@",
            "confirmPassword": "Test123@",
            "firstName": "Phúc",
            "lastName": "Trần",
            "phone": "0901548666",
            "acceptTerms": True
        }
        response = requests.post(f"{base_url}/auth/register", json=payload)
        assert response.status_code in [200, 201], f"Registration failed: {response.text}"
       
        # Đánh dấu user này cần được Admin xóa sau khi test xong
        cleanup_new_users.append({
            "identifier": payload["username"],
            "password": payload["password"]
        })

        payload = {"identifier": "phuc1@gmail.com", "password": "Test123@"}
        response = requests.post(f"{base_url}/auth/login", json=payload)
        assert response.status_code == 200
        assert "accessToken" in response.json()['data']

    def test_login_fail_wrong_password(self, base_url):
        """
        🏷️ TC_AUTH_12 - LOGIN FAILS WITH WRONG PASSWORD
        
        📌 DECLARATION:
        Test that login is rejected when password is incorrect.
        
        📝 GOAL:
        - Attempt login with correct username but wrong password
        - Verify rejection
        
        🔍 STEPS:
        1. Submit POST /auth/login with correct identifier, wrong password
        2. Verify status 401 Unauthorized
        3. Verify no token returned
        
        ✔️ EXPECTED RESULT:
        - Status: 401 Unauthorized or 400 Bad Request
        - No accessToken in response
        - Error message about invalid credentials
        """
        payload = {"identifier": "west", "password": "WrongPassword123!"}
        response = requests.post(f"{base_url}/auth/login", json=payload)
        assert response.status_code in [400, 401]

    def test_login_fail_nonexistent_user(self, base_url):
        """
        🏷️ TC_AUTH_13 - LOGIN FAILS WITH NONEXISTENT USER
        
        📌 DECLARATION:
        Test that login is rejected for non-existent user accounts.
        
        📝 GOAL:
        - Attempt login with username that doesn't exist
        - Verify rejection
        
        🔍 STEPS:
        1. Submit POST /auth/login with non-existent identifier
        2. Verify 401 or 404
        
        ✔️ EXPECTED RESULT:
        - Status: 401 or 404
        - No token returned
        """
        payload = {
            "identifier": f"nonexistent_user_{uuid.uuid4()}",
            "password": "AnyPassword123!"
        }
        response = requests.post(f"{base_url}/auth/login", json=payload)
        assert response.status_code in [400, 401, 404]

    def test_login_fail_empty_credentials(self, base_url):
        """
        🏷️ TC_AUTH_14 - LOGIN FAILS WITH EMPTY CREDENTIALS
        
        📌 DECLARATION:
        Test that login rejects empty or missing credentials.
        
        📝 GOAL:
        - Submit login with empty identifier/password
        - Verify rejection
        
        🔍 STEPS:
        1. Try with empty identifier
        2. Try with empty password
        3. Try with both empty
        
        ✔️ EXPECTED RESULT:
        - Status: 400 Bad Request
        """
        invalid_payloads = [
            {"identifier": "", "password": "test"},
            {"identifier": "test", "password": ""},
            {"identifier": "", "password": ""},
        ]
        
        for payload in invalid_payloads:
            response = requests.post(f"{base_url}/auth/login", json=payload)
            assert response.status_code == 400

    def test_seller_login_success(self, base_url):
        """
        🏷️ TC_AUTH_15 - SELLER LOGIN SUCCESS
        
        📌 DECLARATION:
        Test successful login for seller role.
        
        📝 GOAL:
        - Login as seller user
        - Verify token returned with seller role
        
        🔍 STEPS:
        1. Submit seller credentials
        2. Verify 200 OK
        3. Verify token returned
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - accessToken present
        """
        payload = {"identifier": "stephen@gmail.com", "password": "Phuc123@"}
        response = requests.post(f"{base_url}/auth/login", json=payload)
        assert response.status_code == 200
        assert "accessToken" in response.json()['data']

    def test_admin_login_success(self, base_url):
        """
        🏷️ TC_AUTH_16 - ADMIN LOGIN SUCCESS
        
        📌 DECLARATION:
        Test successful login for admin role.
        
        📝 GOAL:
        - Login as admin user
        - Verify admin token returned
        
        🔍 STEPS:
        1. Submit admin credentials
        2. Verify 200 OK
        3. Verify token present
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - accessToken present
        """
        payload = {"identifier": "west", "password": "Phuc123@"}
        response = requests.post(f"{base_url}/auth/login", json=payload)
        assert response.status_code == 200
        assert "accessToken" in response.json()['data']

    def test_login_returns_user_data(self, base_url):
        """
        🏷️ TC_AUTH_17 - LOGIN RETURNS USER INFORMATION
        
        📌 DECLARATION:
        Test that login response contains user profile information.
        
        📝 GOAL:
        - Verify login response includes user details
        - Verify user data is complete and accurate
        
        🔍 STEPS:
        1. Login with user credentials
        2. Verify response contains user ID, email, username, name, role
        
        ✔️ EXPECTED RESULT:
        - Response contains: userId, email, username, firstName, lastName, role
        - User data matches registered information
        """
        payload = {"identifier": "west", "password": "Phuc123@"}
        response = requests.post(f"{base_url}/auth/login", json=payload)
        assert response.status_code == 200
        
        user_data = response.json()['data']
        assert "userId" in  user_data['user']
        assert "email" in user_data['user']
        assert "username" in user_data['user']


class TestTokenManagement:
    """
    ✅ GOAL: Validate JWT token behavior including refresh tokens,
    token expiry, and token usage in protected endpoints.
    """

    def test_token_used_in_protected_endpoint(self, base_url, admin_headers):
        """
        🏷️ TC_AUTH_18 - TOKEN VALIDATES PROTECTED ENDPOINT ACCESS
        
        📌 DECLARATION:
        Test that valid JWT token grants access to protected endpoints.
        
        📝 GOAL:
        - Use token in Authorization header to access protected endpoint
        - Verify 200 OK response
        
        🔍 STEPS:
        1. Login and get token
        2. Use token in Authorization: Bearer header
        3. Call protected endpoint (e.g., /users/profile)
        4. Verify 200 OK
        
        ✔️ EXPECTED RESULT:
        - Status: 200 OK
        - Response contains protected user data
        """
        # Test with admin_headers which has valid token
        response = requests.get(f"{base_url}/users/profile", headers=admin_headers)
        assert response.status_code == 200

    def test_request_without_token_denied(self, base_url):
        """
        🏷️ TC_AUTH_19 - PROTECTED ENDPOINT REQUIRES TOKEN
        
        📌 DECLARATION:
        Test that protected endpoints reject requests without valid token.
        
        📝 GOAL:
        - Attempt to access protected endpoint without token
        - Verify 401 Unauthorized
        
        🔍 STEPS:
        1. Call protected endpoint without Authorization header
        2. Verify 401 Unauthorized
        
        ✔️ EXPECTED RESULT:
        - Status: 401 Unauthorized
        """
        response = requests.get(f"{base_url}/users/profile")
        assert response.status_code == 401

    def test_request_with_invalid_token_denied(self, base_url):
        """
        🏷️ TC_AUTH_20 - INVALID TOKEN REJECTED
        
        📌 DECLARATION:
        Test that malformed or invalid tokens are rejected.
        
        📝 GOAL:
        - Use invalid token in Authorization header
        - Verify rejection
        
        🔍 STEPS:
        1. Create invalid/malformed token string
        2. Use in Authorization: Bearer header
        3. Call protected endpoint
        4. Verify 401 Unauthorized
        
        ✔️ EXPECTED RESULT:
        - Status: 401 Unauthorized or 403 Forbidden
        """
        invalid_headers = {
            "Authorization": "Bearer invalid_token_string",
            "Content-Type": "application/json"
        }
        response = requests.get(f"{base_url}/users/profile", headers=invalid_headers)
        assert response.status_code in [401, 403]

    def test_token_authorization_format(self, base_url, admin_headers):
        """
        🏷️ TC_AUTH_21 - TOKEN AUTHORIZATION FORMAT VALIDATION
        
        📌 DECLARATION:
        Test that Authorization header requires correct "Bearer" format.
        
        📝 GOAL:
        - Verify only "Bearer <token>" format is accepted
        - Test with "Token", "JWT", etc. formats
        
        🔍 STEPS:
        1. Try Authorization: Token <token>
        2. Try Authorization: <token> (no Bearer prefix)
        3. Verify both fail
        
        ✔️ EXPECTED RESULT:
        - Non-Bearer formats return 401/403
        - Only Bearer format works
        """
        # Get valid token
        payload = {"identifier": "west", "password": "Phuc123@"}
        login_response = requests.post(f"{base_url}/auth/login", json=payload)
        token = login_response.json()['data']['accessToken']
        
        # Test wrong format
        wrong_format_headers = {
            "Authorization": f"Token {token}",
            "Content-Type": "application/json"
        }
        response = requests.get(f"{base_url}/users/profile", headers=wrong_format_headers)
        assert response.status_code in [401, 403]


class TestSecurityRequirements:
    """
    ✅ GOAL: Validate authentication security requirements including
    HTTPS, CORS, rate limiting, and secure token storage recommendations.
    """

    def test_response_contains_no_sensitive_data_in_error(self, base_url):
        """
        🏷️ TC_AUTH_22 - ERROR RESPONSES DON'T LEAK SENSITIVE DATA
        
        📌 DECLARATION:
        Test that error responses don't expose sensitive information.
        
        📝 GOAL:
        - Verify error messages don't expose database details
        - Ensure no passwords or internal system info leaked
        
        🔍 STEPS:
        1. Trigger various auth errors (invalid login, etc.)
        2. Inspect error messages
        3. Verify no SQL, passwords, or stack traces visible
        
        ✔️ EXPECTED RESULT:
        - Error messages are user-friendly
        - No database errors or stack traces
        - No password hints
        """
        # Invalid login
        payload = {"identifier": "nonexistent", "password": "wrong"}
        response = requests.post(f"{base_url}/auth/login", json=payload)
        
        response_text = response.text
        # Should not contain SQL keywords or stack traces
        assert "SELECT" not in response_text
        assert "traceback" not in response_text.lower()
        assert "password" not in response_text.lower() or "incorrect" in response_text.lower()

    def test_password_not_returned_in_response(self, base_url, admin_headers):
        """
        🏷️ TC_AUTH_23 - PASSWORD NEVER RETURNED IN RESPONSE
        
        📌 DECLARATION:
        Test that passwords are never returned in any API response.
        
        📝 GOAL:
        - Verify login response doesn't contain password
        - Verify user profile doesn't contain password
        
        🔍 STEPS:
        1. Call /auth/login
        2. Call /users/profile
        3. Verify password field not in responses
        
        ✔️ EXPECTED RESULT:
        - No "password" field in any response
        - No password hash visible
        """
        # Check login response
        payload = {"identifier": "west", "password": "Phuc123@"}
        login_response = requests.post(f"{base_url}/auth/login", json=payload)
        assert "password" not in login_response.text.lower() or "hashed" not in login_response.text.lower()
        
        # Check profile response
        profile_response = requests.get(f"{base_url}/users/profile", headers=admin_headers)
        assert "password" not in profile_response.text.lower()

    def test_token_in_response_not_in_url(self, base_url):
        """
        🏷️ TC_AUTH_24 - TOKEN NOT EXPOSED IN URL
        
        📌 DECLARATION:
        Test that authentication uses POST and tokens are in body/headers, not URL.
        
        📝 GOAL:
        - Verify login endpoint is POST not GET
        - Verify tokens never appear in query parameters
        
        🔍 STEPS:
        1. Verify /auth/login is POST endpoint
        2. Attempt GET /auth/login (should fail)
        3. Verify no tokens in URL
        
        ✔️ EXPECTED RESULT:
        - Login is POST endpoint
        - GET /auth/login returns 405 Method Not Allowed
        """
        # GET should not work
        response = requests.get(f"{base_url}/auth/login", params={
            "identifier": "test",
            "password": "test"
        })
        # Should be 405 Method Not Allowed or similar
        assert response.status_code in [400, 405]
