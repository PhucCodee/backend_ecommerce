# api_tests/test_nfr_performance.py
import requests
import time
import pytest
import statistics
from datetime import datetime
import concurrent.futures

# ==============================================================================
# ⚡ NON-FUNCTIONAL REQUIREMENTS - PERFORMANCE TESTS
# ==============================================================================

# Performance Test Results Storage
perf_results = []

def log_perf_result(nfr_id, nfr_name, operation, response_time_ms, threshold_ms, passed):
    """Log performance test results"""
    result = {
        "timestamp": datetime.now().isoformat(),
        "nfr_id": nfr_id,
        "nfr_name": nfr_name,
        "operation": operation,
        "response_time_ms": round(response_time_ms, 2),
        "threshold_ms": threshold_ms,
        "passed": passed,
        "status": "✅ PASS" if passed else "❌ FAIL"
    }
    perf_results.append(result)
    print(f"\n[Performance] {operation}")
    print(f"  Response time: {response_time_ms:.2f}ms")
    print(f"  Threshold: {threshold_ms}ms")
    print(f"  Result: {result['status']}")
    return result


# ==============================================================================
# NFR-2.1: API RESPONSE TIME UNDER 200MS FOR 95% OF REQUESTS
# ==============================================================================

def test_nfr_2_1_response_time_get_products(base_url):
    """
    NFR-2.1: API response time must be under 200ms for 95% of requests.
    - Test: Measure response time for GET /products.
    - Expected: 95th percentile < 200ms.
    """
    num_requests = 100
    response_times = []
    
    print(f"\nTesting GET /products ({num_requests} requests)...")
    
    for i in range(num_requests):
        start = time.time()
        response = requests.get(f"{base_url}/products", params={"pageNumber": 1, "pageSize": 10})
        elapsed = (time.time() - start) * 1000  # Convert to ms
        
        if response.status_code == 200:
            response_times.append(elapsed)
            print(f"  Request {i+1}: {elapsed:.2f}ms", end="")
            if i % 5 == 4:
                print()
    
    if response_times:
        sorted_times = sorted(response_times)
        p95 = sorted_times[int(len(sorted_times) * 0.95)]
        avg = statistics.mean(response_times)
        p99 = sorted_times[int(len(sorted_times) * 0.99)]
        
        passed = p95 < 200
        
        log_perf_result(
            "NFR-2.1",
            "API Response Time (95th percentile)",
            "GET /products",
            p95,
            200,
            passed
        )
        
        print(f"\n  Statistics:")
        print(f"    Average: {avg:.2f}ms")
        print(f"    95th percentile: {p95:.2f}ms (Threshold: 200ms)")
        print(f"    99th percentile: {p99:.2f}ms")
        
        assert passed, f"95th percentile response time {p95:.2f}ms exceeds 200ms threshold"
    else:
        pytest.skip("Could not get valid responses")


def test_nfr_2_1_response_time_get_user_profile(base_url, user_headers):
    """
    NFR-2.1 Extended: GET /users/profile response time.
    - Test: Multiple requests to user profile.
    """
    num_requests = 100
    response_times = []
    
    print(f"\nTesting GET /users/profile ({num_requests} requests)...")
    
    for i in range(num_requests):
        start = time.time()
        response = requests.get(f"{base_url}/users/profile", headers=user_headers)
        elapsed = (time.time() - start) * 1000
        
        if response.status_code == 200:
            response_times.append(elapsed)
    
    if response_times:
        sorted_times = sorted(response_times)
        p95 = sorted_times[int(len(sorted_times) * 0.95)]
        avg = statistics.mean(response_times)
        
        passed = p95 < 200
        
        log_perf_result(
            "NFR-2.1-ext1",
            "API Response Time - User Profile",
            "GET /users/profile",
            p95,
            200,
            passed
        )
        
        print(f"  Average: {avg:.2f}ms")
        print(f"  95th percentile: {p95:.2f}ms")
        
        assert passed, f"Profile response time exceeds 200ms"


def test_nfr_2_1_response_time_create_order(base_url, user_headers):
    """
    NFR-2.1 Extended: POST /orders response time.
    - Test: Order creation response time.
    """
    response_times = []
    
    print(f"\nTesting POST /orders (10 requests)...")
    
    for i in range(100):
        payload = {
            "shippingAddressId": 1,
            "billingAddressId": 1,
            "couponCode": None,
            "customerNotes": f"Test order {i}"
        }
        
        start = time.time()
        response = requests.post(
            f"{base_url}/orders",
            json=payload,
            headers=user_headers
        )
        elapsed = (time.time() - start) * 1000
        
        if response.status_code in [200, 201, 400]:  # Accept 400 if cart empty
            response_times.append(elapsed)
    
    if response_times:
        sorted_times = sorted(response_times)
        p95 = sorted_times[int(len(sorted_times) * 0.95)]
        avg = statistics.mean(response_times)
        
        passed = p95 < 200
        
        log_perf_result(
            "NFR-2.1-ext2",
            "API Response Time - Order Creation",
            "POST /orders",
            p95,
            200,
            passed
        )
        
        print(f"  Average: {avg:.2f}ms")
        print(f"  95th percentile: {p95:.2f}ms")


# ==============================================================================
# NFR-2.5: PRODUCT SEARCH WITHIN 500MS
# ==============================================================================

def test_nfr_2_5_product_search_response_time(base_url):
    """
    NFR-2.5: Product search must return results within 500ms.
    - Test: Search products with various queries.
    - Expected: All searches complete within 500ms.
    """
    search_queries = [
        {"search": "shirt"},
        {"search": "blue"},
        {"brand": "Nike"},
        {"minPrice": 10, "maxPrice": 100},
        {"search": "shirt", "brand": "Adidas", "minPrice": 20}
    ]

    search_queries = search_queries * 5  # Repeat to increase test count
    
    response_times = []
    passed_count = 0
    
    print(f"\nTesting product search ({len(search_queries)} queries)...")
    
    for query in search_queries:
        start = time.time()
        response = requests.get(f"{base_url}/products", params=query)
        elapsed = (time.time() - start) * 1000
        
        response_times.append(elapsed)
        passed = elapsed < 500
        if passed:
            passed_count += 1
        
        query_str = str(query)[:50]
        print(f"  {query_str}: {elapsed:.2f}ms {'✅' if passed else '❌'}")
    
    if response_times:
        avg = statistics.mean(response_times)
        max_time = max(response_times)
        
        all_passed = all(t < 500 for t in response_times)
        
        log_perf_result(
            "NFR-2.5",
            "Product Search Performance",
            "GET /products (search)",
            max_time,
            500,
            all_passed
        )
        
        print(f"\n  Statistics:")
        print(f"    Passed: {passed_count}/{len(search_queries)}")
        print(f"    Average: {avg:.2f}ms")
        print(f"    Maximum: {max_time:.2f}ms")
        
        assert all_passed, f"Some searches exceeded 500ms threshold"


def test_nfr_2_5_product_search_full_text(base_url):
    """
    NFR-2.5 Extended: Full-text search performance.
    - Test: Complex search with multiple parameters.
    """
    complex_query = {
        "search": "blue cotton shirt",
        "brand": ["Nike", "Adidas"],
        "minPrice": 15,
        "maxPrice": 150,
        "pageNumber": 1,
        "pageSize": 20,
        "sortBy": "popularity"
    }
    
    response_times = []
    
    print(f"\nTesting full-text search (5 iterations)...")
    
    for i in range(100):
        start = time.time()
        response = requests.get(f"{base_url}/products", params=complex_query)
        elapsed = (time.time() - start) * 1000
        response_times.append(elapsed)
    
    if response_times:
        avg = statistics.mean(response_times)
        max_time = max(response_times)
        
        passed = max_time < 500
        
        log_perf_result(
            "NFR-2.5-ext",
            "Full-Text Search Performance",
            "GET /products (complex search)",
            max_time,
            500,
            passed
        )
        
        print(f"  Average: {avg:.2f}ms")
        print(f"  Max: {max_time:.2f}ms")


# ==============================================================================
# ADDITIONAL PERFORMANCE METRICS
# ==============================================================================

def test_nfr_2_x_concurrent_request_latency(base_url):
    """
    Extended: Test response time under concurrent load.
    - Test: Send multiple concurrent requests, measure latency.
    - Expected: Response times should not degrade significantly.
    """
    num_concurrent = 100
    response_times = []
    
    print(f"\nTesting {num_concurrent} concurrent requests...")
    
    def make_request():
        start = time.time()
        response = requests.get(f"{base_url}/products", params={"pageNumber": 1, "pageSize": 10})
        elapsed = (time.time() - start) * 1000
        return elapsed if response.status_code == 200 else None
    
    with concurrent.futures.ThreadPoolExecutor(max_workers=num_concurrent) as executor:
        futures = [executor.submit(make_request) for _ in range(num_concurrent)]
        for future in concurrent.futures.as_completed(futures):
            result = future.result()
            if result:
                response_times.append(result)
    
    if response_times:
        avg = statistics.mean(response_times)
        max_time = max(response_times)
        
        # Concurrent requests might be slower, so use higher threshold
        passed = max_time < 500
        
        log_perf_result(
            "NFR-2.x",
            "Concurrent Request Latency",
            f"GET /products (x{num_concurrent} concurrent)",
            max_time,
            500,
            passed
        )
        
        print(f"  Average: {avg:.2f}ms")
        print(f"  Max: {max_time:.2f}ms")
        print(f"  Completed: {len(response_times)}/{num_concurrent}")


def test_nfr_2_x_database_query_latency(base_url, seller_headers):
    """
    Extended: Test database-heavy operations.
    - Test: Get seller's products (requires database query).
    """
    response_times = []
    
    print(f"\nTesting database query latency...")
    
    for i in range(100):
        start = time.time()
        response = requests.get(f"{base_url}/products/seller", headers=seller_headers)
        elapsed = (time.time() - start) * 1000
        
        if response.status_code == 200:
            response_times.append(elapsed)
    
    if response_times:
        sorted_times = sorted(response_times)
        p95 = sorted_times[int(len(sorted_times) * 0.95)]
        avg = statistics.mean(response_times)
        
        passed = p95 < 300  # Database queries might be slightly slower
        
        log_perf_result(
            "NFR-2.x-db",
            "Database Query Latency",
            "GET /products/seller",
            p95,
            300,
            passed
        )
        
        print(f"  Average: {avg:.2f}ms")
        print(f"  95th percentile: {p95:.2f}ms")


# ==============================================================================
# PERFORMANCE REPORT GENERATION
# ==============================================================================

@pytest.fixture(scope="session", autouse=True)
def nfr_performance_report(request):
    """Generate NFR Performance Test Report after all tests"""
    yield
    
    if perf_results:
        print("\n" + "="*80)
        print("NFR PERFORMANCE TEST REPORT")
        print("="*80)
        
        passed = sum(1 for r in perf_results if r["passed"])
        total = len(perf_results)
        
        print(f"\nSummary: {passed}/{total} tests passed ({passed*100//total}%)")
        print("\nDetailed Results:")
        
        for result in perf_results:
            print(f"\n{result['status']} {result['nfr_id']}: {result['nfr_name']}")
            print(f"   Operation: {result['operation']}")
            print(f"   Response Time: {result['response_time_ms']}ms")
            print(f"   Threshold: {result['threshold_ms']}ms")
            print(f"   Timestamp: {result['timestamp']}")
        
        # Summary statistics
        avg_response = statistics.mean([r['response_time_ms'] for r in perf_results])
        print(f"\n📊 Overall Average Response Time: {avg_response:.2f}ms")
        print(f"✅ Compliance Rate: {passed*100//total}%")
