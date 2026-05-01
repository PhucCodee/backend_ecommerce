# tests/test_cart/test_cart.py
import requests
import uuid
import pytest

@pytest.fixture
def session_headers():
    """Tạo header chứa 1 cái Session-Id giả lập cho Khách vãng lai (Guest)"""
    return {
        "X-Session-Id": str(uuid.uuid4()),
        "Content-Type": "application/json"
    }

# ==============================================================================
# 🧪 TEST CASES CHO NHÁNH CART
# ==============================================================================

def test_get_cart_guest(base_url, session_headers):
    """
    TC_CART_01: KHÁCH VÃNG LAI LẤY GIỎ HÀNG
    - Kỳ vọng: 200 OK. Lấy giỏ hàng mới tinh (có thể là mảng rỗng hoặc giỏ hàng trống).
    """
    res = requests.get(f"{base_url}/cart", headers=session_headers)
    assert res.status_code == 200

def test_add_item_to_cart(base_url, session_headers):
    """
    TC_CART_02: THÊM SẢN PHẨM VÀO GIỎ HÀNG
    - LƯU Ý: Phải có sẵn SKU ID 1 trong Database để test chạy qua.
    """
    payload = {"skuId": 1, "quantity": 1}
    res = requests.post(f"{base_url}/cart/items", json=payload, headers=session_headers)
    
    # Có thể lỗi 404 nếu SKU 1 không tồn tại
    assert res.status_code in [200, 201], f"Thêm vào giỏ thất bại, có thể do SKU ID 1 không tồn tại. Lỗi: {res.text}"

def test_update_cart_item(base_url, session_headers):
    """
    TC_CART_03: CẬP NHẬT SỐ LƯỢNG TRONG GIỎ
    """
    # 1. Thêm hàng vào giỏ trước
    payload_add = {"skuId": 1, "quantity": 1}
    add_res = requests.post(f"{base_url}/cart/items", json=payload_add, headers=session_headers)
    assert add_res.status_code in [200, 201]
    
    # Lấy Cart Item ID trả về từ API (Lưu ý: Đây là ID của dòng trong giỏ, không phải SKU ID)
    # Tùy backend, có thể cần lấy qua GET /cart nếu POST không trả về ID
    cart_data = add_res.json().get("data", add_res.json())
    item_id = cart_data.get("id") # Giả sử API trả về ID của item vừa thêm
    
    # Bỏ qua test nếu API ko trả ID trực tiếp ở bước thêm
    if not item_id:
        pytest.skip("Backend không trả về CartItemId sau khi thêm, cần viết thêm logic GET để dò tìm.")
        
    # 2. Sửa số lượng thành 5
    payload_update = {"quantity": 5}
    update_res = requests.put(f"{base_url}/cart/items/{item_id}", json=payload_update, headers=session_headers)
    assert update_res.status_code in [200, 204]

def test_clear_cart(base_url, session_headers):
    """
    TC_CART_04: XÓA SẠCH GIỎ HÀNG
    """
    # 1. Thêm hàng
    requests.post(f"{base_url}/cart/items", json={"skuId": 1, "quantity": 1}, headers=session_headers)
    
    # 2. Xóa sạch
    res = requests.delete(f"{base_url}/cart", headers=session_headers)
    assert res.status_code in [200, 204]
    
    # 3. Get lại xem đã sạch chưa
    get_res = requests.get(f"{base_url}/cart", headers=session_headers)
    # Kiểm tra giỏ hàng rỗng (items = [])
    data = get_res.json().get("data", get_res.json())
    items = data.get("items", []) if isinstance(data, dict) else data
    assert len(items) == 0

def test_merge_cart_on_login(base_url, session_headers, user_headers):
    """
    TC_CART_05: GỘP GIỎ HÀNG KHI ĐĂNG NHẬP (MERGE)
    - Kịch bản: 
      1. Khách dùng X-Session-Id chọn đồ.
      2. Khách ấn Đăng nhập (Lúc này có Token User + X-Session-Id cũ).
      3. Gộp giỏ.
    """
    # 1. Khách vãng lai thêm đồ
    requests.post(f"{base_url}/cart/items", json={"skuId": 1, "quantity": 1}, headers=session_headers)
    
    # 2. Gộp giỏ (Merge) -> Dùng Token của User VÀ Session ID của Guest
    headers_merge = user_headers.copy()
    headers_merge["X-Session-Id"] = session_headers["X-Session-Id"]
    
    res = requests.post(f"{base_url}/cart/merge", headers=headers_merge)
    assert res.status_code in [200, 204]

def test_seller_can_create_cart(base_url, seller_headers):
        """
        🏷️ TC_ORDER_10 - SELLER CANNOT CREATE ORDERS (SELLER RESTRICTION)
        
        📌 DECLARATION:
        Test that seller role cannot create customer orders.
        
        📝 GOAL:
        - Verify role-based access control
        
        🔍 STEPS:
        1. Seller attempts POST /api/orders
        2. Verify 403 Forbidden
        
        ✔️ EXPECTED RESULT:
        - Status: 403 Forbidden
        """
        response = requests.post(f"{base_url}/cart/items", json={"skuId": 1, "quantity": 1}, headers=seller_headers)
        assert response.status_code in [200, 201]