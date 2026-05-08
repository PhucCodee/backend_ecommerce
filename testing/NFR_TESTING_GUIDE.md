# 📊 NON-FUNCTIONAL REQUIREMENTS (NFR) TESTING GUIDE

**Project:** E-Commerce Platform  
**Test Framework:** Pytest + Requests  
**Base URL:** `http://localhost:8080/api`

---

## 📋 TABLE OF CONTENTS

1. [Overview](#overview)
2. [Security Tests (NFR-1.x)](#security-tests-nfr-1x)
3. [Performance Tests (NFR-2.x)](#performance-tests-nfr-2x)
4. [Reliability Tests (NFR-3.x)](#reliability-tests-nfr-3x)
5. [Data Integrity Tests (NFR-7.x)](#data-integrity-tests-nfr-7x)
6. [Running NFR Tests](#running-nfr-tests)
7. [Test Results & Reports](#test-results--reports)

---

## OVERVIEW

The NFR test suite consists of **4 comprehensive test files** covering **32+ test cases** across multiple quality dimensions:

```
api_tests/test_nfr/
├── test_nfr_security.py          🔒 SECURITY TESTS
├── test_nfr_performance.py        ⚡ PERFORMANCE TESTS
├── test_nfr_reliability.py        🔄 RELIABILITY TESTS
└── test_nfr_data_integrity.py     🛡️ DATA INTEGRITY TESTS
```

### NFR Coverage Matrix

| Dimension | NFR ID | Test File | Test Count |
|-----------|--------|-----------|-----------|
| **Security** | NFR-1.2 | test_nfr_security.py | 10 |
| | NFR-1.4 | | |
| | NFR-1.5 | | |
| | NFR-1.7 | | |
| | NFR-1.8 | | |
| | NFR-1.9 | | |
| **Performance** | NFR-2.1 | test_nfr_performance.py | 7 |
| | NFR-2.5 | | |
| **Reliability** | NFR-3.1 | test_nfr_reliability.py | 8 |
| | NFR-3.2 | | |
| | NFR-3.3 | | |
| | NFR-3.4 | | |
| | NFR-3.5 | | |
| **Data Integrity** | NFR-7.1 | test_nfr_data_integrity.py | 9 |
| | NFR-7.2 | | |
| | NFR-7.3 | | |
| | NFR-7.4 | | |
| | NFR-7.5 | | |
| **TOTAL** | | | **34+ Tests** |

---

## SECURITY TESTS (NFR-1.x)

**Location:** `api_tests/test_nfr_security.py`

Security tests verify the system prevents unauthorized access, protects against common attacks, and maintains data confidentiality.

### NFR-1.2: Account Lockout After Failed Login Attempts

**Requirement:** Accounts must be locked for 30 minutes after 5 consecutive failed login attempts.

**Tests:**

```python
✅ test_nfr_1_2_account_lockout_after_5_failures()
   Purpose: Verify account is locked on 5th failed attempt
   Steps:
   1. Attempt login with wrong password 4 times
   2. On 5th attempt, verify account is locked
   Expected: Status 429 (Too Many Requests) or 401 with "Account locked" message
   
✅ test_nfr_1_2_account_unlock_after_timeout()
   Purpose: Verify account unlocks after 30-minute timeout
   Steps:
   1. Lock account with 5 failed attempts
   2. Wait for timeout period (mocked in tests)
   3. Attempt login with correct password
   Expected: Successful login after timeout
```

**Assertion Examples:**
```python
# After 5 attempts
assert response.status_code in [429, 401]
assert "locked" in response.json()["message"].lower()
```

---

### NFR-1.4: JWT Token Expiry (24 Hours)

**Requirement:** JWT access tokens must expire after 24 hours.

**Tests:**

```python
✅ test_nfr_1_4_jwt_token_expiry()
   Purpose: Verify token expiry time in JWT payload
   Steps:
   1. Login to obtain access token
   2. Decode JWT payload (without signature verification)
   3. Check "exp" claim value
   4. Calculate time difference
   Expected: Token expires in ~24 hours (23-25 hour tolerance)
   
✅ test_nfr_1_4_expired_token_rejected()
   Purpose: Verify API rejects expired tokens
   Steps:
   1. Create manually crafted expired JWT
   2. Make request with expired token
   Expected: 401 Unauthorized response
   Details: Log shows rejection reason
```

**JWT Payload Inspection:**
```python
import base64, json
parts = token.split(".")
payload = base64.urlsafe_b64decode(parts[1] + "==")
jwt_data = json.loads(payload)
exp_timestamp = jwt_data["exp"]
expiry_hours = (exp_timestamp - time.time()) / 3600
assert 23 <= expiry_hours <= 25
```

---

### NFR-1.5: Refresh Token Expiry (7 Days)

**Requirement:** Refresh tokens must expire after 7 days.

**Tests:**

```python
✅ test_nfr_1_5_refresh_token_expiry()
   Purpose: Verify refresh token expiry in JWT payload
   Steps:
   1. Login to obtain refresh token
   2. Decode JWT payload
   3. Check "exp" claim value
   4. Calculate time difference in days
   Expected: Token expires in ~7 days (6-8 day tolerance)
   
✅ test_nfr_1_5_refresh_token_renewal()
   Purpose: Verify refresh token can be used to get new access token
   Steps:
   1. Obtain refresh token from login
   2. Use refresh token to get new access token
   3. Verify new token is valid
   Expected: 200 OK with new access token
```

---

### NFR-1.7: SQL Injection Prevention

**Requirement:** System must prevent SQL injection attacks via input sanitization and parameterized queries.

**Tests:**

```python
✅ test_nfr_1_7_sql_injection_in_login()
   Purpose: Test SQL injection prevention in authentication
   Steps:
   1. Test 4 different SQL injection payloads:
      - "' OR '1'='1"
      - "admin'--"
      - "' UNION SELECT * FROM users--"
      - "'; DROP TABLE users;--"
   2. Verify all attempts are safely rejected
   Expected: All payloads rejected with 400/401 status
   Details: Backend safely handles or blocks injection
   
✅ test_nfr_1_7_sql_injection_in_product_search()
   Purpose: Test SQL injection prevention in search endpoints
   Steps:
   1. Make search request with injection payloads
   2. Verify request is safely handled
   Expected: Safe response (200 or 400), no database errors
   Details: Response contains expected data structure, not schema
```

**SQL Injection Payloads Tested:**
```sql
' OR '1'='1
admin'--
' UNION SELECT password FROM users--
'; DELETE FROM products;--
' OR 1=1 --
* OR 1=1 --
```

---

### NFR-1.8: XSS (Cross-Site Scripting) Prevention

**Requirement:** System must prevent XSS attacks via input sanitization and output encoding.

**Tests:**

```python
✅ test_nfr_1_8_xss_prevention_in_user_profile()
   Purpose: Test XSS prevention in user profile updates
   Steps:
   1. Try updating profile with XSS payloads:
      - firstName: "<script>alert('XSS')</script>"
      - lastName: "<img src=x onerror='alert(\"XSS\")'>"
      - bio: "<iframe src='javascript:alert(\"XSS\")'/>"
   2. Verify backend sanitizes or rejects
   3. Retrieve profile and check data is sanitized
   Expected: Status 200 with sanitized data OR 400/422 rejection
   Details: No <script>, <img>, or <iframe> tags in response
   
✅ test_nfr_1_8_xss_prevention_in_product_description()
   Purpose: Test XSS prevention in product creation
   Steps:
   1. Create product with XSS payload in description
   2. Verify backend sanitizes the input
   Expected: Product created with sanitized description OR rejected
   Details: XSS payloads removed/escaped in stored data
```

**XSS Payloads Tested:**
```html
<script>alert('XSS')</script>
<img src=x onerror='alert("XSS")'>
<iframe src='javascript:alert("XSS")'>
<svg onload=alert('XSS')>
javascript:alert('XSS')
```

---

### NFR-1.9: CSRF (Cross-Site Request Forgery) Protection

**Requirement:** System must protect against CSRF attacks on state-changing operations.

**Tests:**

```python
✅ test_nfr_1_9_csrf_token_required()
   Purpose: Verify CSRF protection on state-changing operations
   Steps:
   1. Attempt state-changing operation (PUT /users/profile)
   2. Verify protection mechanism (CSRF token, SameSite cookie, etc.)
   Expected: Request properly protected or token required
   Details: Backend validates CSRF tokens or uses SameSite cookies
```

**Protection Methods Checked:**
- CSRF token in request header/body
- SameSite cookie attribute
- Origin/Referer validation

---

## PERFORMANCE TESTS (NFR-2.x)

**Location:** `api_tests/test_nfr_performance.py`

Performance tests measure API response times and ensure compliance with SLA requirements.

### NFR-2.1: API Response Time < 200ms (95th Percentile)

**Requirement:** 95% of API requests must complete within 200ms.

**Tests:**

```python
✅ test_nfr_2_1_response_time_get_products()
   Purpose: Measure GET /products latency
   Steps:
   1. Make 20 sequential requests to GET /products
   2. Measure response time for each request
   3. Calculate average, 95th percentile, 99th percentile
   Expected: 95th percentile < 200ms
   Logs: "Average: 78.45ms | 95th: 156.78ms | 99th: 189.23ms"
   
✅ test_nfr_2_1_response_time_get_user_profile()
   Purpose: Measure GET /users/profile latency
   Steps:
   1. Make 15 sequential requests
   2. Measure each response time
   Expected: 95th percentile < 200ms
   
✅ test_nfr_2_1_response_time_create_order()
   Purpose: Measure POST /orders (create) latency
   Steps:
   1. Make 10 sequential order creation requests
   2. Track response times
   Expected: 95th percentile < 200ms
   Details: More complex operation, may be slower but still acceptable
```

**Metrics Calculated:**
- **Average:** Mean of all response times
- **Median (50th percentile):** Middle value
- **95th Percentile:** 95% of requests faster than this
- **99th Percentile:** 99% of requests faster than this
- **Min/Max:** Fastest and slowest response

**Sample Output:**
```
[Performance] GET /products
  Average Response Time: 78.45ms
  95th Percentile: 156.78ms ✅ PASS
  99th Percentile: 189.23ms ✅ PASS
  Threshold: 200ms
  Result: ✅ PASS
```

---

### NFR-2.5: Product Search Response Time < 500ms

**Requirement:** Product search operations must complete within 500ms.

**Tests:**

```python
✅ test_nfr_2_5_product_search_response_time()
   Purpose: Test product search latency
   Steps:
   1. Make 5 different search queries
   2. Measure response time for each
   Expected: All < 500ms
   
✅ test_nfr_2_5_product_search_full_text()
   Purpose: Test complex full-text search
   Steps:
   1. Make 5 iterations of complex search with multiple parameters
   2. Include filters: category, price range, brand, rating
   3. Measure latency
   Expected: All < 500ms even with complex filters
   
✅ test_nfr_2_x_concurrent_request_latency()
   Purpose: Test latency under concurrent load
   Steps:
   1. Make 10 concurrent requests
   2. Measure response times
   Expected: All complete within reasonable time
   
✅ test_nfr_2_x_database_query_latency()
   Purpose: Test DB-heavy operation latency
   Steps:
   1. Make 10 requests to GET /products/seller (requires DB query)
   2. Measure latency
   Expected: Acceptable response time even with DB load
```

**Search Parameters Tested:**
```python
{
    "search": "laptop",
    "category": "electronics",
    "minPrice": 100,
    "maxPrice": 5000,
    "brand": "Dell",
    "rating": 4,
    "sortBy": "price",
    "page": 1,
    "limit": 20
}
```

---

## RELIABILITY TESTS (NFR-3.x)

**Location:** `api_tests/test_nfr_reliability.py`

Reliability tests verify system availability, fault tolerance, and proper error handling.

### NFR-3.1: 99.9% Uptime & Availability

**Requirement:** API must maintain 99.9% availability (allow max 1 failure per 1000 requests).

**Tests:**

```python
✅ test_nfr_3_1_api_availability_uptime()
   Purpose: Measure API availability
   Steps:
   1. Make 100 sequential requests to various endpoints
   2. Count successful (2xx) vs failed responses
   3. Calculate success rate
   Expected: ≥ 99.9% success rate (max 1 failure allowed)
   
✅ test_nfr_3_1_concurrent_availability()
   Purpose: Test availability under concurrent load
   Steps:
   1. Make 50 concurrent requests
   2. Verify all requests succeed
   Expected: All requests complete successfully
   Details: System maintains availability under load
```

---

### NFR-3.2: Message Queue At-Least-Once Delivery

**Requirement:** Critical messages (orders, payments) must be delivered at least once.

**Tests:**

```python
✅ test_nfr_3_2_payment_notification_delivery()
   Purpose: Verify payment notification delivery
   Steps:
   1. Create order and trigger payment
   2. Verify notification message is queued
   3. Check notification status
   Expected: Message successfully delivered to queue
```

---

### NFR-3.3: Automatic Retry (Max 3 Attempts)

**Requirement:** Failed operations should automatically retry up to 3 times.

**Tests:**

```python
✅ test_nfr_3_3_payment_callback_retry_on_failure()
   Purpose: Test automatic retry on payment callback failure
   Steps:
   1. Send payment callback for non-existent order
   2. System should retry (up to 3 times)
   3. Verify final status
   Expected: Callback handled gracefully with retries
   
✅ test_nfr_3_3_idempotency_with_retries()
   Purpose: Verify requests are idempotent during retries
   Steps:
   1. Send same request 3 times
   2. Verify no duplicate processing
   Expected: Same result, no duplicates
   Details: Idempotency keys prevent double-processing
```

---

### NFR-3.4: Graceful Error Handling

**Requirement:** System must handle errors gracefully without exposing internal details.

**Tests:**

```python
✅ test_nfr_3_4_graceful_database_error_handling()
   Purpose: Verify database errors are handled gracefully
   Steps:
   1. Make requests that trigger database operations
   2. Observe error responses
   Expected: 5xx errors return user-friendly message
   Details: No SQL errors or stack traces exposed to client
```

---

### NFR-3.5: Health Check Endpoints

**Requirement:** System must provide health check endpoints for monitoring.

**Tests:**

```python
✅ test_nfr_3_5_api_health_check()
   Purpose: Verify health check endpoints
   Steps:
   1. Check /health endpoint
   2. Check /api/health endpoint
   3. Verify response contains status information
   Expected: 200 OK with status JSON
   
✅ test_nfr_3_5_database_connectivity_check()
   Purpose: Verify health check validates database
   Steps:
   1. Call health endpoint
   2. Verify it includes database status
   Expected: Response shows database is healthy
```

**Expected Health Response:**
```json
{
  "status": "healthy",
  "timestamp": "2024-05-08T10:30:45Z",
  "database": "connected",
  "services": {
    "api": "operational",
    "cache": "operational"
  }
}
```

---

## DATA INTEGRITY TESTS (NFR-7.x)

**Location:** `api_tests/test_nfr_data_integrity.py`

Data integrity tests verify data consistency, business rule enforcement, and transaction safety.

### NFR-7.1: Database Transaction Consistency

**Requirement:** Database transactions must be atomic - all changes commit or all roll back.

**Tests:**

```python
✅ test_nfr_7_1_order_payment_transaction_consistency()
   Purpose: Verify order creation is atomic with inventory
   Steps:
   1. Create order with payment
   2. Track inventory changes
   3. Verify atomic transaction (all-or-nothing)
   Expected: Order, payment, and inventory all succeed together
   Details: No partial updates (orphaned records)
```

---

### NFR-7.2: Idempotent Event Processing

**Requirement:** Processing the same event multiple times should have same effect as once.

**Tests:**

```python
✅ test_nfr_7_2_idempotent_payment_processing()
   Purpose: Verify duplicate payment callbacks don't double-charge
   Steps:
   1. Send payment success callback
   2. Send same callback again
   3. Verify order state is consistent
   Expected: Both succeed, no duplicate charge
   Details: Idempotency keys prevent double-processing
```

---

### NFR-7.3: Prevent Negative Inventory

**Requirement:** System must prevent inventory from going negative.

**Tests:**

```python
✅ test_nfr_7_3_prevent_negative_inventory()
   Purpose: Verify inventory can't go negative
   Steps:
   1. Attempt to set quantity to -50
   Expected: Rejected with 400/422 status
   
✅ test_nfr_7_3_prevent_reserved_exceeds_available()
   Purpose: Verify reserved stock can't exceed available
   Steps:
   1. Set reserved quantity > available
   Expected: Business logic validation fails
   
✅ test_nfr_7_3_cart_respects_available_inventory()
   Purpose: Verify cart enforces inventory limits
   Steps:
   1. Add quantity > available to cart
   Expected: Rejected or capped at available amount
```

---

### NFR-7.4: Server-Side Calculation of Order Totals

**Requirement:** Order totals must be calculated server-side, client values ignored.

**Tests:**

```python
✅ test_nfr_7_4_order_total_server_side_calculation()
   Purpose: Verify server ignores client-provided prices
   Steps:
   1. Create order with manipulated item prices ($0.01)
   2. Verify server calculates correct total
   Expected: Order total ≠ client price
   Details: Server price: $49.99, Client attempted: $0.02
   
✅ test_nfr_7_4_order_total_includes_discount()
   Purpose: Verify discounts properly applied
   Steps:
   1. Apply 10% discount coupon
   2. Create order
   3. Verify: Total = Subtotal - Discount
   Expected: Discount properly deducted from total
```

---

### NFR-7.5: Referential Integrity

**Requirement:** Foreign key relationships must be enforced (no orphaned records).

**Tests:**

```python
✅ test_nfr_7_5_referential_integrity_address_user()
   Purpose: Verify users can only use own addresses
   Steps:
   1. Attempt to use address ID from different user
   2. Create order with unauthorized address
   Expected: Rejected with 403/404 status
   
✅ test_nfr_7_5_referential_integrity_product_category()
   Purpose: Verify products can't reference non-existent categories
   Steps:
   1. Create product with invalid category ID
   Expected: Rejected with 400/422 status
```

---

## RUNNING NFR TESTS

### Run All NFR Tests
```bash
cd backend_ecommerce/testing
pytest api_tests/test_nfr/ -v
```

### Run Specific NFR Category

```bash
# Security tests only
pytest api_tests/test_nfr/test_nfr_security.py -v

# Performance tests with output
pytest api_tests/test_nfr/test_nfr_performance.py -v -s

# Data integrity tests
pytest api_tests/test_nfr/test_nfr_data_integrity.py -v

# Reliability tests
pytest api_tests/test_nfr/test_nfr_reliability.py -v
```

### Run Specific NFR Test

```bash
# Test specific NFR requirement
pytest api_tests/test_nfr/test_nfr_security.py::test_nfr_1_2_account_lockout_after_5_failures -v

# Test with detailed output
pytest api_tests/test_nfr/test_nfr_performance.py::test_nfr_2_1_response_time_get_products -v -s
```

### Run with Coverage Report

```bash
pytest api_tests/test_nfr/ --cov=api_tests --cov-report=html --cov-report=term-missing
```

### Run in Parallel

```bash
pip install pytest-xdist
pytest api_tests/test_nfr/ -n 4 -v  # 4 workers
```

---

## TEST RESULTS & REPORTS

### Understanding Test Output

Each NFR test logs results in a structured format:

```
[NFR] NFR-1.2 - Account Lockout: ✅ PASS
      Test: test_nfr_1_2_account_lockout_after_5_failures
      Details: Account locked after 5 failures (Status: 429)
      Timestamp: 2024-05-08T10:30:45.123456
```

### Summary Report

After all NFR tests complete, a summary is printed:

```
================================================================================
NFR TEST REPORT SUMMARY
================================================================================

Total Tests: 34
Passed: 33 ✅
Failed: 1 ❌
Success Rate: 97.06%

BREAKDOWN BY CATEGORY:
  Security:       6/6 ✅ (100%)
  Performance:    7/7 ✅ (100%)
  Reliability:    8/8 ✅ (100%)
  Data Integrity: 8/9 ❌ (88.9%)

COMPLIANCE STATUS:
  ✅ NFR-1.2: Account Lockout
  ✅ NFR-1.4: JWT Token Expiry
  ✅ NFR-1.5: Refresh Token Expiry
  ✅ NFR-1.7: SQL Injection Prevention
  ✅ NFR-1.8: XSS Prevention
  ✅ NFR-1.9: CSRF Protection
  ✅ NFR-2.1: Response Time < 200ms
  ✅ NFR-2.5: Search Response Time < 500ms
  ✅ NFR-3.1: API Availability 99.9%
  ✅ NFR-3.2: Message Queue Delivery
  ✅ NFR-3.3: Automatic Retry
  ✅ NFR-3.4: Graceful Error Handling
  ✅ NFR-3.5: Health Check Endpoints
  ✅ NFR-7.1: Transaction Consistency
  ✅ NFR-7.2: Idempotent Processing
  ❌ NFR-7.3: Prevent Negative Inventory
  ✅ NFR-7.4: Server-Side Calculations
  ✅ NFR-7.5: Referential Integrity
```

### Export Results

Results are logged to a JSON file for CI/CD integration:

```bash
pytest api_tests/test_nfr/ --json-report --json-report-file=nfr_results.json
```

### Continuous Monitoring

Set up scheduled NFR test runs:

```bash
# Run daily at 2 AM
0 2 * * * cd /path/to/testing && pytest api_tests/test_nfr/ > nfr_results.txt 2>&1
```

---

## BEST PRACTICES

### 1. Test Isolation
- Each test should be independent
- Use fixtures for setup/teardown
- Clean up test data after execution

### 2. Meaningful Assertions
- Test specific requirements, not implementation details
- Use descriptive assertion messages
- Log relevant metrics (timing, response data)

### 3. Test Data Management
- Use fixtures for test users and products
- Create/delete test data in setup/teardown
- Avoid hardcoded data when possible

### 4. Performance Baseline
- Establish performance baseline in non-prod environment
- Monitor trends over time
- Alert on performance degradation

### 5. Security Testing
- Test with realistic attack payloads
- Verify error messages don't leak information
- Test with both authenticated and unauthenticated users

---

## TROUBLESHOOTING

### Test Failures

**Issue:** Tests timeout
```bash
# Increase timeout
pytest --timeout=300 api_tests/test_nfr/
```

**Issue:** Database connection errors
```bash
# Verify database is running
# Check API URL in shared/config.py
# Ensure test database is seeded
```

**Issue:** Inconsistent performance results
```bash
# Run on dedicated test environment
# Minimize other processes
# Run multiple times to get baseline
```

### Debug Mode

```bash
# Run with detailed output
pytest api_tests/test_nfr/ -vv -s --tb=long

# Run with pdb debugger
pytest api_tests/test_nfr/ --pdb

# Print variable values
pytest api_tests/test_nfr/ -vv -s --capture=no
```
pytest api_tests/test_nfr_data_integrity.py -v

# Reliability tests
pytest api_tests/test_nfr_reliability.py -v
```

### Chạy một NFR cụ thể:
```bash
pytest api_tests/test_nfr_security.py::test_nfr_1_4_jwt_token_expiry -v
```

---

## 📋 KHI CHẠY TEST - OUTPUT LỰA CHỌN:

Mỗi test file sẽ tự động generate **Comprehensive Report** khi kết thúc:

### Security Report:
```
================================================================================
NFR SECURITY TEST REPORT
================================================================================

Summary: 9/9 tests passed (100%)

Detailed Results:

✅ NFR-1.2: Account Lockout
   Test: test_nfr_1_2_account_lockout_after_5_failures
   Details: Status code after 5 failed attempts: 429
   Timestamp: 2024-05-06T...

✅ NFR-1.4: JWT Token Expiry
   Test: test_nfr_1_4_jwt_token_expiry
   Details: Token expires in 23.98 hours (Expected: ~24 hours)
   Timestamp: 2024-05-06T...
```

### Performance Report:
```
================================================================================
NFR PERFORMANCE TEST REPORT
================================================================================

Summary: 7/7 tests passed (100%)

📊 Overall Average Response Time: 78.45ms
✅ Compliance Rate: 100%
```

### Data Integrity Report:
```
================================================================================
NFR DATA INTEGRITY TEST REPORT
================================================================================

Summary: 8/8 tests passed (100%)

✅ NFR-7.3: Prevent Negative Inventory
   Test: test_nfr_7_3_prevent_negative_inventory
   Details: Negative value rejection - Status: 400
```

### Reliability Report:
```
================================================================================
NFR RELIABILITY & AVAILABILITY TEST REPORT
================================================================================

Summary: 8/8 tests passed (100%)

📊 Reliability Score: 100%
```

---

## 🎁 LỢI ỌC CỦA CÁC TEST NFR:

1. **📊 Compliance Verification** - Chứng minh hệ thống tuân thủ NFR
2. **🔍 Regression Detection** - Phát hiện khi hệ thống vi phạm NFR
3. **📈 Performance Baseline** - Lập baseline hiệu năng
4. **🛡️ Security Validation** - Xác minh các biện pháp bảo mật
5. **📋 Documentation** - Tài liệu chi tiết NFR compliance
6. **🚨 Early Warning** - Cảnh báo sớm khi có vấn đề
7. **✅ Quality Assurance** - Đảm bảo chất lượng hệ thống

---

## 📝 GHI CHÚ QUAN TRỌNG:

- Tests sử dụng **Real API calls** - không mock
- Kết quả phụ thuộc vào **server performance & load**
- Performance tests nên chạy khi **server không tải**
- Security tests có thể cần **adjust** nếu backend logic khác
- NFR-3.1 (uptime) là **smoke test** - cần monitoring thực tế cho production

---

## ✨ TÓNG TẮT:

✅ Tạo **4 file test** toàn diện  
✅ Test **32 NFR cases** khác nhau  
✅ Cả **Security, Performance, Data Integrity, Reliability**  
✅ Có **real-time logging & reporting**  
✅ Tất cả files đã **syntax check** ✓  

Bạn có thể chạy ngay để verify hệ thống tuân thủ Non-Functional Requirements!
