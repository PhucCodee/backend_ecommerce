# api_tests/test_payment/test_payment.py
import requests
import uuid
import pytest

# ==============================================================================
# 🧪 TEST CASES CHO NHÁNH PAYMENT (THANH TOÁN)
# ==============================================================================

def test_create_payment_request_success(base_url, user_headers):
    """
    TC_PAY_01: USER TẠO YÊU CẦU THANH TOÁN (ZaloPay)
    - Tiền điều kiện: User đã đăng nhập (có token).
    - Hành động: Gửi POST /payments/zalopay/create với orderId hợp lệ.
    - Kỳ vọng: 200/201 Created. Response chứa payment URL hoặc transaction ID.
    """
    payload = {
        "orderId": 1
    }
    
    response = requests.post(
        f"{base_url}/payments/zalopay/create",
        json=payload,
        headers=user_headers
    )
    
    # Chấp nhận 200 OK hoặc 201 Created
    assert response.status_code in [200, 201], \
        f"Expected 200/201, got {response.status_code}: {response.text}"
    
    # Kiểm chứng response có chứa thông tin thanh toán
    data = response.json()
    assert "data" in data or "redirectUrl" in data or "paymentUrl" in data, \
        "Response không chứa thông tin thanh toán cần thiết"


def test_create_payment_request_invalid_order(base_url, user_headers):
    """
    TC_PAY_02: TẠO THANH TOÁN VỚI ORDER ID KHÔNG TỒN TẠI
    - Tiền điều kiện: Order ID được sử dụng không tồn tại hoặc không thuộc user.
    - Hành động: Gửi POST /payments/zalopay/create với orderId không hợp lệ.
    - Kỳ vọng: Backend trả về lỗi 400 Bad Request hoặc 404 Not Found.
    """
    payload = {
        "orderId": 999999  # Order ID giả định không tồn tại
    }
    
    response = requests.post(
        f"{base_url}/payments/zalopay/create",
        json=payload,
        headers=user_headers
    )
    
    # Chấp nhận lỗi client-side (4xx)
    assert response.status_code >= 400 and response.status_code < 500, \
        f"Expected 4xx error, got {response.status_code}"


def test_create_payment_missing_order_id(base_url, user_headers):
    """
    TC_PAY_03: VALIDATION - THIẾU ORDER ID
    - Hành động: Gửi request mà không kèm orderId.
    - Kỳ vọng: Backend trả về 400 Bad Request (validation error).
    """
    payload = {}  # Rỗng, không có orderId
    
    response = requests.post(
        f"{base_url}/payments/zalopay/create",
        json=payload,
        headers=user_headers
    )
    
    assert response.status_code in [400, 422], \
        f"Expected validation error (400/422), got {response.status_code}"


def test_create_payment_without_authentication(base_url):
    """
    TC_PAY_04: KIỂM TRA BẢO MẬT - CHẶN KHÔNG XÁC THỰC
    - Tiền điều kiện: Không cấp token xác thực.
    - Hành động: Gửi POST /payments/zalopay/create mà không kèm header Authorization.
    - Kỳ vọng: Backend từ chối, trả về 401 Unauthorized.
    """
    payload = {"orderId": 1}
    
    response = requests.post(
        f"{base_url}/payments/zalopay/create",
        json=payload
        # ❌ Cố ý không truyền headers
    )
    
    assert response.status_code == 401, \
        f"Expected 401 Unauthorized, got {response.status_code}"


def test_payment_callback_success(base_url, user_headers):
    """
    TC_PAY_05: XỬ LÝ CALLBACK TỪ ZALOPAY (THANH TOÁN THÀNH CÔNG)
    - Tiền điều kiện: ZaloPay đã xác nhận thanh toán từ phía user.
    - Hành động: Hệ thống nhận callback từ ZaloPay, xử lý cập nhật trạng thái đơn hàng.
    - Kỳ vọng: 200 OK. Đơn hàng được chuyển sang trạng thái "Paid" hoặc tương tự.
    
    Lưu ý: Callback thực tế từ ZaloPay sẽ chứa signature & data được mã hóa.
    Test này là mô phỏng callback cơ bản.
    """
    payload = {
        "appid": 2553,
        "apptransid": "230101_123456",
        "transid": 123456789,
        "status": 1,  # 1 = Thành công
        "amount": 100000,
        "discountamount": 0,
        "orderId": 1,
        "transtime": 1700000000,
        "mac": "mock_signature"  # Mô phỏng chữ ký từ ZaloPay
    }
    
    response = requests.post(
        f"{base_url}/payments/zalopay/callback",
        json=payload,
        headers=user_headers
    )
    
    # Callback endpoint thường trả về 200 OK nếu xử lý thành công
    assert response.status_code in [200, 204], \
        f"Expected 200/204 for callback, got {response.status_code}: {response.text}"


def test_payment_callback_invalid_signature(base_url, user_headers):
    """
    TC_PAY_06: KIỂM TRA BẢO MẬT - CALLBACK VỚI CHỮ KÝ KHÔNG HỢP LỆ
    - Tiền điều kiện: Backend có validate chữ ký (MAC) từ ZaloPay.
    - Hành động: Gửi callback với chữ ký giả mạo.
    - Kỳ vọng: Backend từ chối, trả về 400 Bad Request hoặc 401 Unauthorized.
    """
    payload = {
        "appid": 2553,
        "apptransid": "230101_123456",
        "transid": 123456789,
        "status": 1,
        "amount": 100000,
        "orderId": 1,
        "mac": "invalid_fake_signature_12345"  # Chữ ký không hợp lệ
    }
    
    response = requests.post(
        f"{base_url}/payments/zalopay/callback",
        json=payload,
        headers=user_headers
    )
    
    # Backend nên từ chối callback không hợp lệ
    assert response.status_code in [400, 401, 403], \
        f"Expected error status, got {response.status_code}"


def test_payment_callback_failed_payment(base_url, user_headers):
    """
    TC_PAY_07: CALLBACK - THANH TOÁN THẤT BẠI
    - Kịch bản: User hủy thanh toán hoặc thanh toán thất bại từ phía ZaloPay.
    - Hành động: Gửi callback với status = 0 hoặc 2 (failed).
    - Kỳ vọng: 200 OK. Đơn hàng vẫn được cập nhật trạng thái thành "Payment Failed".
    """
    payload = {
        "appid": 2553,
        "apptransid": "230101_456789",
        "transid": 987654321,
        "status": 0,  # 0 = Thất bại
        "amount": 100000,
        "orderId": 2,
        "mac": "mock_signature"
    }
    
    response = requests.post(
        f"{base_url}/payments/zalopay/callback",
        json=payload,
        headers=user_headers
    )
    
    # Hệ thống vẫn phải xử lý callback thất bại một cách bình thường
    assert response.status_code in [200, 204], \
        f"Expected 200/204, got {response.status_code}"


def test_payment_callback_duplicate(base_url, user_headers):
    """
    TC_PAY_08: XỬ LÝ CALLBACK TRÙNG LẶP (IDEMPOTENCY)
    - Kịch bản: ZaloPay gửi callback 2 lần cho cùng một transaction (retry mechanism).
    - Hành động: 
        1. Gửi callback lần đầu.
        2. Gửi cùng callback lần thứ hai.
    - Kỳ vọng: Cả 2 lần đều 200 OK. Đơn hàng chỉ được cập nhật một lần (idempotent).
    """
    payload = {
        "appid": 2553,
        "apptransid": "230101_duplicate",
        "transid": 111222333,
        "status": 1,
        "amount": 100000,
        "orderId": 3,
        "mac": "mock_signature"
    }
    
    # Callback lần đầu
    response1 = requests.post(
        f"{base_url}/payments/zalopay/callback",
        json=payload,
        headers=user_headers
    )
    assert response1.status_code in [200, 204]
    
    # Callback lần thứ hai (cùng dữ liệu)
    response2 = requests.post(
        f"{base_url}/payments/zalopay/callback",
        json=payload,
        headers=user_headers
    )
    assert response2.status_code in [200, 204], \
        "Hệ thống phải idempotent với duplicate callbacks"


# ==============================================================================
# 🛡️ SECURITY TESTS
# ==============================================================================

def test_user_cannot_create_payment_for_others_order(base_url, user_headers, seller_headers):
    """
    TC_SEC_PAY_01: BẢO VỀ - User không được thanh toán cho Order của người khác
    - Kịch bản: User A cố tạo payment cho Order của User B.
    - Hành động: Sử dụng token của User A, gửi request với orderId thuộc User B.
    - Kỳ vọng: Backend từ chối, trả về 403 Forbidden hoặc 404 Not Found.
    """
    # Giả định orderId = 999 thuộc seller, không phải user
    payload = {"orderId": 999}
    
    response = requests.post(
        f"{base_url}/payments/zalopay/create",
        json=payload,
        headers=user_headers  # ← Sử dụng token user thường
    )
    
    # Backend nên từ chối
    assert response.status_code in [403, 404], \
        "Lỗ hổng: User có thể thanh toán cho order không phải của mình!"
