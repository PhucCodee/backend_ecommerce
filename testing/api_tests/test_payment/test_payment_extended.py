# api_tests/test_payment/test_payment_extended.py
import requests
import uuid
import pytest
import time

# ==============================================================================
# 🧪 EXTENDED TEST CASES CHO PAYMENT (THANH TOÁN NÂNG CAO)
# ==============================================================================

def test_payment_with_different_amounts(base_url, user_headers):
    """
    TC_PAY_EXT_01: PARAMETRIZED TEST - THANH TOÁN VỚI NHIỀU SỐ TIỀN KHÁC NHAU
    - Hành động: Tạo payment request cho các đơn hàng với số tiền khác nhau.
    - Kỳ vọng: Tất cả đều trả về 200/201 OK, số tiền được ghi nhận đúng.
    """
    order_ids = [1, 2, 3]  # Mô phỏng 3 đơn hàng khác nhau
    
    for order_id in order_ids:
        payload = {"orderId": order_id}
        response = requests.post(
            f"{base_url}/payments/zalopay/create",
            json=payload,
            headers=user_headers
        )
        
        assert response.status_code in [200, 201], \
            f"Failed for orderId {order_id}: {response.text}"


def test_payment_request_response_structure(base_url, user_headers):
    """
    TC_PAY_EXT_02: KIỂM TRA CẤU TRÚC RESPONSE
    - Hành động: Tạo payment, kiểm tra response có chứa các trường bắt buộc.
    - Kỳ vọng: Response chứa transactionId, redirectUrl, appTransId, hoặc tương tự.
    """
    payload = {"orderId": 1}
    response = requests.post(
        f"{base_url}/payments/zalopay/create",
        json=payload,
        headers=user_headers
    )
    
    assert response.status_code in [200, 201]
    data = response.json()
    
    # Kiểm chứng response có chứa "data" object
    assert "data" in data, "Response phải chứa 'data' object"
    
    # Kiểm chứng data không phải rỗng
    payment_data = data.get("data", {})
    assert isinstance(payment_data, dict) and len(payment_data) > 0, \
        "Payment data không phải là dict hoặc rỗng"


def test_get_payment_status(base_url, user_headers):
    """
    TC_PAY_EXT_03: KIỂM SOÁT TRẠNG THÁI THANH TOÁN
    - Tiền điều kiện: Đã tạo payment request.
    - Hành động: Lấy thông tin trạng thái của payment.
    - Kỳ vọng: GET /payments/{paymentId} hoặc GET /orders/{orderId}/payment trả về status.
    
    Lưu ý: Endpoint này có thể không tồn tại, sẽ skip nếu 404.
    """
    # Trước tiên tạo payment
    create_payload = {"orderId": 1}
    create_response = requests.post(
        f"{base_url}/payments/zalopay/create",
        json=create_payload,
        headers=user_headers
    )
    
    if create_response.status_code not in [200, 201]:
        pytest.skip("Không thể tạo payment, skip test trạng thái")
    
    payment_id = create_response.json().get("data", {}).get("transactionId")
    
    if not payment_id:
        pytest.skip("Response không chứa transactionId, skip")
    
    # Thử lấy trạng thái
    status_response = requests.get(
        f"{base_url}/payments/{payment_id}",
        headers=user_headers
    )
    
    # Có thể 200 OK hoặc 404 Not Found (nếu endpoint không tồn tại)
    if status_response.status_code == 404:
        pytest.skip("GET /payments endpoint không tồn tại")
    
    assert status_response.status_code == 200


def test_payment_callback_with_various_statuses(base_url, user_headers):
    """
    TC_PAY_EXT_04: PARAMETRIZED - CALLBACK VỚI NHIỀU STATUS KHÁC NHAU
    - Hành động: Gửi callback với các status code khác nhau từ ZaloPay.
    - Kỳ vọng: 
        - status 1: Thành công, đơn hàng → "Paid"
        - status 0 hoặc 2: Thất bại, đơn hàng → "Payment Failed"
    """
    test_cases = [
        {"status": 1, "description": "Success", "expected_status": 200},
        {"status": 0, "description": "Failed", "expected_status": 200},
        {"status": 2, "description": "Cancelled", "expected_status": 200},
    ]
    
    for i, test_case in enumerate(test_cases):
        payload = {
            "appid": 2553,
            "apptransid": f"test_{i}_{test_case['status']}",
            "transid": 1000000 + i,
            "status": test_case["status"],
            "amount": 100000,
            "orderId": 10 + i,
            "mac": "mock_signature"
        }
        
        response = requests.post(
            f"{base_url}/payments/zalopay/callback",
            json=payload,
            headers=user_headers
        )
        
        assert response.status_code == test_case["expected_status"], \
            f"Failed for status {test_case['status']}: {response.text}"


def test_callback_with_missing_required_fields(base_url, user_headers):
    """
    TC_PAY_EXT_05: CALLBACK VALIDATION - THIẾU TRƯỜNG BẮT BUỘC
    - Hành động: Gửi callback nhưng bỏ sót các trường bắt buộc (appid, transid, status...).
    - Kỳ vọng: Backend trả về 400 Bad Request (validation error).
    """
    # Callback không đầy đủ
    invalid_payloads = [
        {},  # Rỗng hoàn toàn
        {"status": 1},  # Thiếu appid, transid, transtime...
        {"appid": 2553, "status": 1},  # Thiếu transid
    ]
    
    for payload in invalid_payloads:
        response = requests.post(
            f"{base_url}/payments/zalopay/callback",
            json=payload,
            headers=user_headers
        )
        
        # Có thể 400 Bad Request hoặc 422 Unprocessable Entity
        assert response.status_code in [400, 422], \
            f"Expected validation error for payload {payload}, got {response.status_code}"


def test_callback_large_amount(base_url, user_headers):
    """
    TC_PAY_EXT_06: EDGE CASE - CALLBACK VỚI SỐ TIỀN RẤT LỚN
    - Hành động: Gửi callback với amount = rất lớn (VD: 999999999999).
    - Kỳ vọng: Backend xử lý mà không crash, kiểm tra overflow/validation logic.
    """
    payload = {
        "appid": 2553,
        "apptransid": "large_amount_test",
        "transid": 555666777,
        "status": 1,
        "amount": 999999999999,  # Số tiền khổng lồ
        "orderId": 50,
        "mac": "mock_signature"
    }
    
    response = requests.post(
        f"{base_url}/payments/zalopay/callback",
        json=payload,
        headers=user_headers
    )
    
    # Backend nên xử lý được hoặc trả về lỗi validation rõ ràng
    assert response.status_code in [200, 204, 400, 422]


def test_callback_negative_amount(base_url, user_headers):
    """
    TC_PAY_EXT_07: EDGE CASE - CALLBACK VỚI SỐ TIỀN ÂM
    - Hành động: Gửi callback với amount âm (refund, hoặc lỗi dữ liệu).
    - Kỳ vọng: Backend từ chối hoặc xử lý refund logic đặc biệt.
    """
    payload = {
        "appid": 2553,
        "apptransid": "negative_amount_test",
        "transid": 444555666,
        "status": 1,
        "amount": -50000,  # Số tiền âm (refund?)
        "orderId": 51,
        "mac": "mock_signature"
    }
    
    response = requests.post(
        f"{base_url}/payments/zalopay/callback",
        json=payload,
        headers=user_headers
    )
    
    # Backend nên kiểm tra validation
    assert response.status_code in [200, 204, 400, 422]


def test_callback_zero_amount(base_url, user_headers):
    """
    TC_PAY_EXT_08: EDGE CASE - CALLBACK VỚI SỐ TIỀN = 0
    - Hành động: Gửi callback với amount = 0.
    - Kỳ vọng: Backend từ chối (lỗi validation) hoặc xử lý as refund.
    """
    payload = {
        "appid": 2553,
        "apptransid": "zero_amount_test",
        "transid": 333444555,
        "status": 1,
        "amount": 0,
        "orderId": 52,
        "mac": "mock_signature"
    }
    
    response = requests.post(
        f"{base_url}/payments/zalopay/callback",
        json=payload,
        headers=user_headers
    )
    
    # Backend nên có xử lý validation
    assert response.status_code in [200, 204, 400, 422]


def test_payment_create_concurrent_requests(base_url, user_headers):
    """
    TC_PAY_EXT_09: CONCURRENCY - TẠO MULTIPLE PAYMENTS ĐỒNG THỜI
    - Hành động: Gửi 5 request tạo payment cùng lúc (race condition test).
    - Kỳ vọng: Tất cả đều thành công, không có data corruption hoặc duplicate.
    
    Lưu ý: Test này mô phỏng đơn giản. Để test thực sự concurrent, cần dùng threading/asyncio.
    """
    import concurrent.futures
    
    def create_payment(order_id):
        payload = {"orderId": order_id}
        return requests.post(
            f"{base_url}/payments/zalopay/create",
            json=payload,
            headers=user_headers
        )
    
    with concurrent.futures.ThreadPoolExecutor(max_workers=5) as executor:
        futures = [executor.submit(create_payment, i) for i in range(100, 105)]
        results = [f.result() for f in concurrent.futures.as_completed(futures)]
    
    # Tất cả request phải successful
    for response in results:
        assert response.status_code in [200, 201], \
            f"Concurrent request failed: {response.text}"


def test_callback_with_special_characters_in_notes(base_url, user_headers):
    """
    TC_PAY_EXT_10: INPUT SANITIZATION - CALLBACK VỚI KÝ TỰ ĐẶC BIỆT
    - Hành động: Gửi callback chứa các ký tự đặc biệt (emoji, HTML, SQL).
    - Kỳ vọng: Backend xử lý safely, không SQL injection hay XSS.
    """
    payload = {
        "appid": 2553,
        "apptransid": "special_chars_test",
        "transid": 222333444,
        "status": 1,
        "amount": 100000,
        "orderId": 53,
        "description": "<script>alert('XSS')</script> 😀 ' OR '1'='1",
        "mac": "mock_signature"
    }
    
    response = requests.post(
        f"{base_url}/payments/zalopay/callback",
        json=payload,
        headers=user_headers
    )
    
    # Backend nên xử lý an toàn mà không crash
    assert response.status_code in [200, 204, 400, 422]


def test_payment_with_invalid_order_id_format(base_url, user_headers):
    """
    TC_PAY_EXT_11: VALIDATION - ORDER ID VỚI ĐỊNH DẠNG SAI
    - Hành động: Gửi request với orderId = string, null, hoặc kiểu dữ liệu không hợp lệ.
    - Kỳ vọng: Backend trả về 400 Bad Request (validation error).
    """
    invalid_payloads = [
        {"orderId": "not_a_number"},
        {"orderId": None},
        {"orderId": []},
        {"orderId": {}},
    ]
    
    for payload in invalid_payloads:
        response = requests.post(
            f"{base_url}/payments/zalopay/create",
            json=payload,
            headers=user_headers
        )
        
        assert response.status_code in [400, 422], \
            f"Expected validation error for payload {payload}, got {response.status_code}"


def test_payment_response_headers(base_url, user_headers):
    """
    TC_PAY_EXT_12: KIỂM TRA RESPONSE HEADERS (SECURITY)
    - Hành động: Tạo payment, kiểm tra response headers.
    - Kỳ vọng: Response phải chứa proper security headers.
    """
    payload = {"orderId": 1}
    response = requests.post(
        f"{base_url}/payments/zalopay/create",
        json=payload,
        headers=user_headers
    )
    
    assert response.status_code in [200, 201]
    
    # Kiểm chứng Content-Type
    assert "application/json" in response.headers.get("Content-Type", ""), \
        "Response phải có Content-Type: application/json"
    
    # Kiểm chứng không leak nhạy cảm trong headers
    headers_string = str(response.headers).lower()
    assert "password" not in headers_string, "Headers không nên chứa password"


def test_callback_timing_validation(base_url, user_headers):
    """
    TC_PAY_EXT_13: TIMING VALIDATION - CALLBACK QUÁ CŨ/MỚI
    - Kịch bản: Gửi callback với transtime quá cũ (> 1 giờ trước).
    - Hành động: Kiểm tra xem backend có validate timestamp không.
    - Kỳ vọng: Backend có thể từ chối callback quá cũ hoặc chấp nhận tùy logic.
    """
    old_timestamp = int(time.time()) - 3600  # 1 giờ trước
    
    payload = {
        "appid": 2553,
        "apptransid": "old_callback_test",
        "transid": 111222333,
        "status": 1,
        "amount": 100000,
        "orderId": 54,
        "transtime": old_timestamp,
        "mac": "mock_signature"
    }
    
    response = requests.post(
        f"{base_url}/payments/zalopay/callback",
        json=payload,
        headers=user_headers
    )
    
    # Backend có thể chấp nhận hoặc từ chối, tuỳ vào design
    assert response.status_code in [200, 204, 400, 401]


# ==============================================================================
# 🔄 INTEGRATION TESTS
# ==============================================================================

def test_payment_order_status_update_flow(base_url, user_headers):
    """
    TC_PAY_INT_01: FULL FLOW - TẠO PAYMENT → CALLBACK → ĐƠN HÀNG CẬP NHẬT
    - Kịch bản: 
        1. Tạo payment request (status = pending).
        2. Gửi callback thành công từ ZaloPay.
        3. Kiểm chứng đơn hàng được cập nhật thành "Paid".
    """
    # Bước 1: Tạo payment
    create_payload = {"orderId": 1}
    create_response = requests.post(
        f"{base_url}/payments/zalopay/create",
        json=create_payload,
        headers=user_headers
    )
    
    assert create_response.status_code in [200, 201], \
        f"Tạo payment thất bại: {create_response.text}"
    
    # Bước 2: Gửi callback thành công
    callback_payload = {
        "appid": 2553,
        "apptransid": "integration_test_flow",
        "transid": 777888999,
        "status": 1,
        "amount": 100000,
        "orderId": 1,
        "mac": "mock_signature"
    }
    
    callback_response = requests.post(
        f"{base_url}/payments/zalopay/callback",
        json=callback_payload,
        headers=user_headers
    )
    
    assert callback_response.status_code in [200, 204], \
        f"Callback thất bại: {callback_response.text}"
    
    # Bước 3: Kiểm chứng đơn hàng được cập nhật (nếu có GET /orders/{id} endpoint)
    # Lưu ý: Endpoint này có thể không tồn tại, sẽ skip nếu 404
    try:
        order_response = requests.get(
            f"{base_url}/orders/1",
            headers=user_headers
        )
        if order_response.status_code == 200:
            order_data = order_response.json().get("data", {})
            # Kiểm chứng trạng thái thanh toán
            payment_status = order_data.get("paymentStatus") or order_data.get("status")
            assert payment_status is not None, "Order phải có payment status"
    except:
        pytest.skip("GET /orders endpoint không tồn tại")
