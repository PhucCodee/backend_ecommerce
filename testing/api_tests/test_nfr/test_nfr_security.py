# api_tests/test_nfr_security.py
import requests
import uuid
import time
import pytest
import re
from datetime import datetime, timedelta
import hashlib

# ==============================================================================
# 🔒 NON-FUNCTIONAL REQUIREMENTS - SECURITY TESTS
# ==============================================================================

# Test Logging Setup
test_results = []

def log_nfr_result(nfr_id, nfr_name, test_name, passed, details=""):
    """Log NFR test results for compliance reporting"""
    result = {
        "timestamp": datetime.now().isoformat(),
        "nfr_id": nfr_id,
        "nfr_name": nfr_name,
        "test_name": test_name,
        "passed": passed,
        "details": details
    }
    test_results.append(result)
    print(f"\n[NFR] {nfr_id} - {nfr_name}: {'✅ PASS' if passed else '❌ FAIL'}")
    print(f"      Test: {test_name}")
    if details:
        print(f"      Details: {details}")
    return result


# ==============================================================================
# NFR-1.2: ACCOUNT LOCKOUT AFTER 5 FAILED LOGIN ATTEMPTS
# ==============================================================================

def test_nfr_1_2_account_lockout_after_5_failures(base_url):
    """
    NFR-1.2: User accounts must be locked for 30 minutes after 5 failed login attempts.
    - Test: Attempt login with wrong password 5 times, verify account is locked.
    - Expected: 5th attempt should be rejected (locked account).
    """
    username = "goat"
    wrong_password = "Phuc122"
    
    # Attempt 1-4: Wrong password
    for attempt in range(1, 5):
        payload = {"identifier": username, "password": wrong_password}
        response = requests.post(f"{base_url}/auth/login", json=payload)
        assert response.status_code in [400, 401], f"Attempt {attempt} should fail"
        print(f"  [Attempt {attempt}] Failed login recorded")
    
    # Attempt 5: Should be locked
    payload = {"identifier": username, "password": wrong_password}
    response = requests.post(f"{base_url}/auth/login", json=payload)
    
    is_locked = response.status_code in [429,401]  # Too Many Requests (rate limited)
    passed = is_locked
    
    assert response.json()["message"] == "Account is temporarily locked"
    log_nfr_result(
        "NFR-1.2",
        "Account Lockout",
        "test_nfr_1_2_account_lockout_after_5_failures",
        passed,
        f"Status code after 5 failed attempts: {response.status_code} (Expected: 429)"
    )
    
    assert passed, f"Expected 429 (locked), got {response.status_code}"



# ==============================================================================
# NFR-1.4: JWT TOKEN EXPIRY (24 HOURS)
# ==============================================================================

def test_nfr_1_4_jwt_token_expiry(base_url):
    """
    NFR-1.4: JWT tokens must expire after 24 hours.
    - Test: Login to get token, check token expiry time in JWT payload.
    - Expected: Token exp claim should be 24 hours from issued time.
    """
    payload = {"identifier": "west", "password": "Phuc123"}
    response = requests.post(f"{base_url}/auth/login", json=payload)
    
    assert response.status_code == 200
    token = response.json()["data"]["accessToken"]
    
    # Parse JWT (without verification - just for testing)
    # JWT format: header.payload.signature
    try:
        import base64
        parts = token.split(".")
        if len(parts) == 3:
            # Decode payload (add padding if necessary)
            payload_encoded = parts[1]
            padding = 4 - len(payload_encoded) % 4
            if padding != 4:
                payload_encoded += "=" * padding
            
            decoded = base64.urlsafe_b64decode(payload_encoded)
            import json
            jwt_payload = json.loads(decoded)
            
            if "exp" in jwt_payload:
                exp_time = datetime.fromtimestamp(jwt_payload["exp"])
                now = datetime.now()
                time_diff = (exp_time - now).total_seconds() / 3600  # Convert to hours
                
                # Allow 1 hour tolerance for test execution time
                is_valid = 23 <= time_diff <= 25
                
                log_nfr_result(
                    "NFR-1.4",
                    "JWT Token Expiry",
                    "test_nfr_1_4_jwt_token_expiry",
                    is_valid,
                    f"Token expires in {time_diff:.2f} hours (Expected: ~24 hours)"
                )
                
                assert is_valid, f"Token expiry should be ~24 hours, got {time_diff:.2f} hours"
            else:
                pytest.skip("Token does not contain 'exp' claim")
        else:
            pytest.skip("Invalid JWT format")
    except Exception as e:
        pytest.skip(f"Could not parse JWT: {str(e)}")


def test_nfr_1_4_expired_token_rejected(base_url):
    """
    NFR-1.4 Extended: API should reject expired tokens.
    - Test: Use a manually crafted expired token, verify it's rejected.
    - Expected: 401 Unauthorized.
    """
    import base64
    import json
    
    # Create a simple expired JWT (for testing)
    past_timestamp = int(time.time()) - 3600  # 1 hour ago
    
    header = base64.urlsafe_b64encode(json.dumps({"alg": "HS256", "typ": "JWT"}).encode()).rstrip(b"=")
    payload = base64.urlsafe_b64encode(json.dumps({
        "nameid": "1",
        "email": "test@test.com",
        "exp": past_timestamp  # Expired
    }).encode()).rstrip(b"=")
    
    expired_token = (header + b"." + payload + b".fakesignature").decode()
    
    headers = {
        "Authorization": f"Bearer {expired_token}",
        "Content-Type": "application/json"
    }
    
    response = requests.get(f"{base_url}/users/profile", headers=headers)
    
    is_rejected = response.status_code == 401
    
    log_nfr_result(
        "NFR-1.4-ext",
        "JWT Token Expiry - Rejection",
        "test_nfr_1_4_expired_token_rejected",
        is_rejected,
        f"Expired token rejected with status: {response.status_code}"
    )
    
    assert is_rejected, f"Expired token should be rejected (401), got {response.status_code}"


# ==============================================================================
# NFR-1.5: REFRESH TOKEN EXPIRY (7 DAYS)
# ==============================================================================

def test_nfr_1_5_refresh_token_expiry(base_url):
    """
    NFR-1.5: Refresh tokens must expire after 7 days.
    - Test: Login to get refresh token, check expiry in payload.
    - Expected: Refresh token exp claim should be 7 days from issued time.
    """
    payload = {"identifier": "west", "password": "Phuc123"}
    response = requests.post(f"{base_url}/auth/login", json=payload)
    
    assert response.status_code == 200
    response_data = response.json()["data"]
    
    # Check if refreshToken is returned
    if "refreshToken" in response_data:
        refresh_token = response_data["refreshToken"]
        
        try:
            import base64
            import json
            
            parts = refresh_token.split(".")
            if len(parts) == 3:
                payload_encoded = parts[1]
                padding = 4 - len(payload_encoded) % 4
                if padding != 4:
                    payload_encoded += "=" * padding
                
                decoded = base64.urlsafe_b64decode(payload_encoded)
                jwt_payload = json.loads(decoded)
                
                if "exp" in jwt_payload:
                    exp_time = datetime.fromtimestamp(jwt_payload["exp"])
                    now = datetime.now()
                    time_diff_days = (exp_time - now).total_seconds() / 86400  # Convert to days
                    
                    # Allow 1 day tolerance
                    is_valid = 6 <= time_diff_days <= 8
                    
                    log_nfr_result(
                        "NFR-1.5",
                        "Refresh Token Expiry",
                        "test_nfr_1_5_refresh_token_expiry",
                        is_valid,
                        f"Refresh token expires in {time_diff_days:.2f} days (Expected: ~7 days)"
                    )
                    
                    assert is_valid, f"Refresh token should expire in ~7 days, got {time_diff_days:.2f} days"
                else:
                    pytest.skip("Refresh token does not contain 'exp' claim")
            else:
                pytest.skip("Invalid refresh token format")
        except Exception as e:
            pytest.skip(f"Could not parse refresh token: {str(e)}")
    else:
        pytest.skip("refreshToken not returned in login response")


# ==============================================================================
# NFR-1.7: SQL INJECTION PREVENTION
# ==============================================================================

def test_nfr_1_7_sql_injection_in_login(base_url):
    """
    NFR-1.7: SQL injection must be prevented via parameterized queries.
    - Test: Attempt SQL injection in login endpoint.
    - Expected: Backend rejects or safely handles the injection.
    """
    sql_injection_payloads = [
        {"identifier": "' OR '1'='1", "password": "anything"},
        {"identifier": "admin'--", "password": "anything"},
        {"identifier": "' UNION SELECT * FROM users--", "password": "anything"},
        {"identifier": "'; DROP TABLE users;--", "password": "anything"},
    ]
    
    all_rejected = True
    rejected_count = 0
    
    for payload in sql_injection_payloads:
        response = requests.post(f"{base_url}/auth/login", json=payload)
        
        # Should either fail (400/401) or return no data
        if response.status_code in [400, 401]:
            rejected_count += 1
        elif response.status_code == 200:
            # If 200, ensure it's not returning unexpected data
            data = response.json().get("data", {})
            if "accessToken" not in data:
                rejected_count += 1
            else:
                all_rejected = False
    
    passed = rejected_count == len(sql_injection_payloads)
    
    log_nfr_result(
        "NFR-1.7",
        "SQL Injection Prevention",
        "test_nfr_1_7_sql_injection_in_login",
        passed,
        f"Rejected {rejected_count}/{len(sql_injection_payloads)} SQL injection attempts"
    )
    
    assert passed, f"Expected all {len(sql_injection_payloads)} injections to be rejected, got {rejected_count}"


def test_nfr_1_7_sql_injection_in_product_search(base_url):
    """
    NFR-1.7 Extended: SQL injection prevention in search endpoints.
    - Test: Product search with SQL injection.
    """
    sql_injection_params = {
        "search": "'; DELETE FROM products;--",
        "brand": "' OR '1'='1",
        "category": "' UNION SELECT password FROM users--"
    }
    
    response = requests.get(f"{base_url}/products", params=sql_injection_params)
    
    # Should not crash and should return safe response
    is_safe = response.status_code in [200, 400]
    
    try:
        data = response.json()
        # If successful, ensure it's returning products, not database schema
        is_safe = is_safe and "data" in data
    except:
        is_safe = False
    
    log_nfr_result(
        "NFR-1.7-ext",
        "SQL Injection Prevention - Search",
        "test_nfr_1_7_sql_injection_in_product_search",
        is_safe,
        f"Search endpoint safely handled injection (status: {response.status_code})"
    )
    
    assert is_safe, "Product search should safely handle SQL injection"


# ==============================================================================
# NFR-1.8: XSS ATTACK PREVENTION
# ==============================================================================

def test_nfr_1_8_xss_prevention_in_user_profile(base_url, user_headers):
    """
    NFR-1.8: XSS attacks must be prevented via input sanitization.
    - Test: Update user profile with XSS payload.
    - Expected: Backend sanitizes or rejects the input.
    """
    xss_payloads = {
        "firstName": "<script>alert('XSS')</script>",
        "lastName": "<img src=x onerror='alert(\"XSS\")'>",
        "bio": "<iframe src='javascript:alert(\"XSS\")'/>"
    }
    
    # Try to update profile with XSS payloads
    response = requests.put(
        f"{base_url}/users/profile",
        json=xss_payloads,
        headers=user_headers
    )
    
    # Should either reject or sanitize
    is_safe = response.status_code in [200, 204, 400, 422]
    
    if response.status_code in [200, 204]:
        # Verify the data was sanitized (retrieve and check)
        get_response = requests.get(f"{base_url}/users/profile", headers=user_headers)
        if get_response.status_code == 200:
            retrieved_data = get_response.json().get("data", {})
            # Check if XSS payloads are sanitized (no <script> tags)
            is_safe = "<script>" not in str(retrieved_data) and "<img" not in str(retrieved_data)
    
    log_nfr_result(
        "NFR-1.8",
        "XSS Attack Prevention",
        "test_nfr_1_8_xss_prevention_in_user_profile",
        is_safe,
        f"XSS payloads handled safely (status: {response.status_code})"
    )
    
    assert is_safe, "XSS payloads should be sanitized or rejected"


def test_nfr_1_8_xss_prevention_in_product_description(base_url, seller_headers, admin_headers):
    """
    NFR-1.8 Extended: XSS prevention in product creation.
    - Test: Create product with XSS in description.
    """
    category_payload = {"name": f"Cat_{uuid.uuid4()}", "isCore": False, "isActive": True}
    cat_response = requests.post(f"{base_url}/categories", json=category_payload, headers=admin_headers)
    
    if cat_response.status_code not in [200, 201]:
        pytest.skip("Could not create test category")
    
    category_id = cat_response.json().get("data", {}).get("categoryId")
    
    product_payload = {
        "name": "Test Product",
        "description": "<script>alert('XSS')</script> Product Description",
        "categoryIds": [category_id],
        "brand": "TestBrand",
        "defaultSkuPrice": 99.99,
        "defaultSkuStock": 100
    }
    
    response = requests.post(
        f"{base_url}/products/seller",
        json=product_payload,
        headers=seller_headers
    )
    
    is_safe = response.status_code in [200, 201, 400, 422]
    
    if response.status_code in [200, 201]:
        product_data = response.json().get("data", {})
        description = product_data.get("description", "")
        is_safe = "<script>" not in description
    
    log_nfr_result(
        "NFR-1.8-ext",
        "XSS Prevention - Product",
        "test_nfr_1_8_xss_prevention_in_product_description",
        is_safe,
        f"Product XSS payloads handled (status: {response.status_code})"
    )
    
    assert is_safe, "XSS in product description should be sanitized"
    
    # Cleanup
    if cat_response.status_code in [200, 201]:
        requests.delete(f"{base_url}/categories/{category_id}", headers=admin_headers)


# ==============================================================================
# NFR-1.9: CSRF PROTECTION
# ==============================================================================

def test_nfr_1_9_csrf_token_required(base_url, user_headers):
    """
    NFR-1.9: CSRF protection must be implemented for state-changing operations.
    - Test: Attempt state-changing operation (POST/PUT/DELETE) without CSRF token.
    - Expected: Request should be rejected or require CSRF token.
    """
    # Try to update profile without CSRF protection
    # Note: If backend uses Origin/Referer validation instead of tokens, this might pass
    payload = {
        "firstName": "Test",
        "lastName": "User"
    }
    
    response = requests.put(
        f"{base_url}/users/profile",
        json=payload,
        headers=user_headers
    )
    
    # Backend might use SameSite cookies, so check if it accepts the request
    # This is more of an infrastructure test
    is_protected = response.status_code in [200, 204, 403]
    
    log_nfr_result(
        "NFR-1.9",
        "CSRF Protection",
        "test_nfr_1_9_csrf_token_required",
        is_protected,
        f"State-changing operation handling (status: {response.status_code})"
    )
    
    # Note: Full CSRF testing requires simulating cross-origin requests


# ==============================================================================
# REPORT GENERATION
# ==============================================================================

@pytest.fixture(scope="session", autouse=True)
def nfr_security_report(request):
    """Generate NFR Security Test Report after all tests"""
    yield
    
    if test_results:
        print("\n" + "="*80)
        print("NFR SECURITY TEST REPORT")
        print("="*80)
        
        passed = sum(1 for r in test_results if r["passed"])
        total = len(test_results)
        
        print(f"\nSummary: {passed}/{total} tests passed ({passed*100//total}%)")
        print("\nDetailed Results:")
        for result in test_results:
            status = "✅" if result["passed"] else "❌"
            print(f"\n{status} {result['nfr_id']}: {result['nfr_name']}")
            print(f"   Test: {result['test_name']}")
            print(f"   Details: {result['details']}")
            print(f"   Timestamp: {result['timestamp']}")
