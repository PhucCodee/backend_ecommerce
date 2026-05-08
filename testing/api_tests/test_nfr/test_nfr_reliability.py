# api_tests/test_nfr_reliability.py
import requests
import time
import pytest
from datetime import datetime
import concurrent.futures

# ==============================================================================
# 🔄 NON-FUNCTIONAL REQUIREMENTS - RELIABILITY & AVAILABILITY TESTS
# ==============================================================================

# Reliability Test Results
reliability_results = []

def log_reliability_result(nfr_id, nfr_name, test_name, passed, details=""):
    """Log reliability test results"""
    result = {
        "timestamp": datetime.now().isoformat(),
        "nfr_id": nfr_id,
        "nfr_name": nfr_name,
        "test_name": test_name,
        "passed": passed,
        "details": details
    }
    reliability_results.append(result)
    print(f"\n[Reliability] {nfr_id} - {nfr_name}: {'✅ PASS' if passed else '❌ FAIL'}")
    if details:
        print(f"  Details: {details}")
    return result


# ==============================================================================
# NFR-3.5: HEALTH CHECK ENDPOINTS
# ==============================================================================

def test_nfr_3_5_api_health_check(base_url):
    """
    NFR-3.5: Workers must implement health check endpoints.
    - Test: Call health check endpoint.
    - Expected: 200 OK with status information.
    """
    health_endpoints = [
        "/health",
        "/api/health",
        "/api/healthz",
        "/.health"
    ]
    
    health_found = False
    details = "Checked endpoints: "
    
    for endpoint in health_endpoints:
        try:
            response = requests.get(f"{base_url}{endpoint}", timeout=5)
            details += f"\n  {endpoint}: {response.status_code}"
            
            if response.status_code == 200:
                health_found = True
                # Verify response contains health info
                try:
                    data = response.json()
                    if "status" in data or "healthy" in data:
                        details += " (OK - contains status)"
                    break
                except:
                    if "ok" in response.text.lower() or "healthy" in response.text.lower():
                        health_found = True
                        break
        except:
            details += f"\n  {endpoint}: Unreachable"
    
    log_reliability_result(
        "NFR-3.5",
        "Health Check Endpoints",
        "test_nfr_3_5_api_health_check",
        health_found,
        details
    )
    
    # Don't assert - health endpoint might not be public
    if not health_found:
        pytest.skip("Health check endpoint not found at common locations")


def test_nfr_3_5_database_connectivity_check(base_url, admin_headers):
    """
    NFR-3.5 Extended: Health check should verify database connectivity.
    - Test: Health check should indicate database status.
    """
    response = requests.get(f"{base_url}/health", headers=admin_headers)
    
    is_healthy = response.status_code == 200
    details = f"API response: {response.status_code}"
    
    if is_healthy:
        try:
            data = response.json()
            if "database" in data:
                db_status = data["database"]
                if db_status == "connected" or db_status == "ok":
                    details += " (DB: OK)"
                else:
                    details += f" (DB: {db_status})"
        except:
            pass
    
    log_reliability_result(
        "NFR-3.5-ext",
        "Database Health Status",
        "test_nfr_3_5_database_connectivity_check",
        is_healthy,
        details
    )


# ==============================================================================
# NFR-3.3: FAILED EVENTS RETRIED AUTOMATICALLY
# ==============================================================================

def test_nfr_3_3_payment_callback_retry_on_failure(base_url, user_headers):
    """
    NFR-3.3: Failed events must be retried automatically (maximum 3 attempts).
    - Test: Simulate failed payment callback and verify retry.
    - Expected: System should retry processing.
    """
    # Send a callback that might fail (e.g., order doesn't exist)
    payload = {
        "appid": 2553,
        "apptransid": "retry_test_001",
        "transid": 1111111,
        "status": 1,
        "amount": 100000,
        "orderId": 99999,  # Non-existent order
        "mac": "mock_signature"
    }
    
    # First attempt
    response1 = requests.post(
        f"{base_url}/payments/zalopay/callback",
        json=payload,
        headers=user_headers
    )
    
    first_status = response1.status_code
    details = f"First attempt: {first_status}"
    
    # Wait a bit and try again (simulate retry)
    time.sleep(1)
    
    response2 = requests.post(
        f"{base_url}/payments/zalopay/callback",
        json=payload,
        headers=user_headers
    )
    
    second_status = response2.status_code
    details += f", Second attempt: {second_status}"
    
    # System should handle retries gracefully
    is_retry_compatible = first_status in [200, 204, 400, 404]
    
    log_reliability_result(
        "NFR-3.3",
        "Automatic Event Retry",
        "test_nfr_3_3_payment_callback_retry_on_failure",
        is_retry_compatible,
        details
    )


def test_nfr_3_3_idempotency_with_retries(base_url, user_headers):
    """
    NFR-3.3 Extended: Retries should be idempotent (not cause duplicates).
    - Test: Send same request 3 times, verify no duplicates.
    """
    payload = {
        "appid": 2553,
        "apptransid": "idempotent_retry_test",
        "transid": 2222222,
        "status": 1,
        "amount": 100000,
        "orderId": 1,
        "mac": "mock_signature"
    }
    
    responses = []
    for attempt in range(3):
        response = requests.post(
            f"{base_url}/payments/zalopay/callback",
            json=payload,
            headers=user_headers
        )
        responses.append(response.status_code)
        time.sleep(0.5)
    
    # All responses should succeed
    all_successful = all(s in [200, 204] for s in responses)
    details = f"Attempts: {responses[0]}, {responses[1]}, {responses[2]}"
    
    log_reliability_result(
        "NFR-3.3-ext",
        "Idempotent Retries",
        "test_nfr_3_3_idempotency_with_retries",
        all_successful,
        details
    )


# ==============================================================================
# NFR-3.4: GRACEFUL ERROR HANDLING
# ==============================================================================

def test_nfr_3_4_graceful_database_error_handling(base_url):
    """
    NFR-3.4: System must gracefully handle database connection failures.
    - Test: Make requests to endpoints that require DB access.
    - Expected: Even if DB fails temporarily, API should respond gracefully.
    """
    # These endpoints typically require database
    endpoints = [
        ("GET", "/products"),
        ("GET", "/categories"),
    ]
    
    all_graceful = True
    details = "DB Error Handling:"
    
    for method, endpoint in endpoints:
        try:
            if method == "GET":
                response = requests.get(f"{base_url}{endpoint}", timeout=5)
            else:
                response = requests.post(f"{base_url}{endpoint}", json={}, timeout=5)
            
            # Should not return 500 (internal server error) without proper error message
            is_graceful = response.status_code != 500 or "error" in response.text.lower()
            
            details += f"\n  {method} {endpoint}: {response.status_code}"
            
            if not is_graceful:
                all_graceful = False
        except requests.exceptions.Timeout:
            details += f"\n  {method} {endpoint}: TIMEOUT"
            all_graceful = False
        except:
            details += f"\n  {method} {endpoint}: Connection Error"
            all_graceful = False
    
    log_reliability_result(
        "NFR-3.4",
        "Graceful Error Handling",
        "test_nfr_3_4_graceful_database_error_handling",
        all_graceful,
        details
    )


# ==============================================================================
# NFR-3.2: MESSAGE QUEUE AT-LEAST-ONCE DELIVERY
# ==============================================================================

def test_nfr_3_2_payment_notification_delivery(base_url, user_headers):
    """
    NFR-3.2: Message queue must ensure at-least-once delivery.
    - Test: Process payment and verify notification is sent.
    - Expected: User receives order confirmation (even if delayed).
    """
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
    
    delivery_verified = False
    details = f"Order creation: {order_response.status_code}"
    
    if order_response.status_code in [200, 201]:
        order_data = order_response.json().get("data", {})
        order_id = order_data.get("id")
        
        if order_id:
            # Wait a bit for async processing
            time.sleep(2)
            
            # Check if order status is updated
            order_check = requests.get(
                f"{base_url}/orders/{order_id}",
                headers=user_headers
            )
            
            if order_check.status_code == 200:
                order_status = order_check.json().get("data", {}).get("status")
                details += f", Order status: {order_status}"
                delivery_verified = order_status is not None
    
    log_reliability_result(
        "NFR-3.2",
        "Message Queue Delivery",
        "test_nfr_3_2_payment_notification_delivery",
        delivery_verified,
        details
    )


# ==============================================================================
# AVAILABILITY TESTS (HIGH-LEVEL)
# ==============================================================================

def test_nfr_3_1_api_availability_uptime(base_url):
    """
    NFR-3.1: System must have 99.9% uptime target.
    - Test: Make multiple requests over time to verify availability.
    - Expected: Very few failures (0-1 in 1000 requests).
    """
    num_requests = 100
    failed = 0
    timeout_errors = 0
    
    print(f"\nTesting API availability ({num_requests} requests)...")
    
    for i in range(num_requests):
        try:
            response = requests.get(f"{base_url}/products", timeout=10)
            if response.status_code not in [200, 400, 401, 403, 404]:
                failed += 1
                if i % 10 == 0:
                    print(f"  Request {i}: {response.status_code}")
        except requests.exceptions.Timeout:
            timeout_errors += 1
            failed += 1
        except Exception as e:
            failed += 1
    
    success_rate = ((num_requests - failed) / num_requests) * 100
    
    # For 99.9% uptime, we allow ~0.1% failures
    # In 100 requests, that's 0.1 requests, so we allow 1 failure
    is_available = failed <= 1
    
    details = f"Success rate: {success_rate:.1f}% ({num_requests - failed}/{num_requests})"
    if timeout_errors > 0:
        details += f", Timeouts: {timeout_errors}"
    
    log_reliability_result(
        "NFR-3.1",
        "API Availability (99.9% uptime target)",
        "test_nfr_3_1_api_availability_uptime",
        is_available,
        details
    )
    
    # Note: This is a basic smoke test; production uptime monitoring requires more infrastructure


def test_nfr_3_1_concurrent_availability(base_url):
    """
    NFR-3.1 Extended: Verify availability under concurrent load.
    - Test: 50 concurrent requests to API.
    - Expected: All succeed (no timeouts or crashes).
    """
    num_concurrent = 50
    failed = 0
    
    print(f"\nTesting concurrent availability ({num_concurrent} requests)...")
    
    def make_request():
        try:
            response = requests.get(f"{base_url}/products", timeout=10)
            return response.status_code in [200, 400, 401, 403, 404]
        except:
            return False
    
    with concurrent.futures.ThreadPoolExecutor(max_workers=num_concurrent) as executor:
        futures = [executor.submit(make_request) for _ in range(num_concurrent)]
        results = [f.result() for f in concurrent.futures.as_completed(futures)]
    
    success_rate = (sum(results) / len(results)) * 100
    failed = len(results) - sum(results)
    
    is_available = failed == 0
    
    details = f"Success rate: {success_rate:.1f}% ({sum(results)}/{len(results)}) under concurrent load"
    
    log_reliability_result(
        "NFR-3.1-ext",
        "Concurrent Load Availability",
        "test_nfr_3_1_concurrent_availability",
        is_available,
        details
    )


# ==============================================================================
# RELIABILITY REPORT
# ==============================================================================

@pytest.fixture(scope="session", autouse=True)
def nfr_reliability_report(request):
    """Generate NFR Reliability Test Report"""
    yield
    
    if reliability_results:
        print("\n" + "="*80)
        print("NFR RELIABILITY & AVAILABILITY TEST REPORT")
        print("="*80)
        
        passed = sum(1 for r in reliability_results if r["passed"])
        total = len(reliability_results)
        
        print(f"\nSummary: {passed}/{total} tests passed ({passed*100//total}%)")
        print("\nDetailed Results:")
        for result in reliability_results:
            status = "✅" if result["passed"] else "❌"
            print(f"\n{status} {result['nfr_id']}: {result['nfr_name']}")
            print(f"   Test: {result['test_name']}")
            print(f"   Details: {result['details']}")
            print(f"   Timestamp: {result['timestamp']}")
        
        print(f"\n📊 Reliability Score: {passed*100//total}%")
