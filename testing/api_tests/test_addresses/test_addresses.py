import pytest
import requests
import uuid

# ==============================================================================
# 🛠️ FIXTURES DÀNH RIÊNG CHO ADDRESS
# ==============================================================================

@pytest.fixture
def temp_address(base_url, buyer_headers):
    """
    FIXTURE: TẠO ĐỊA CHỈ TẠM THỜI ĐỂ TEST UPDATE VÀ DELETE
    """
    payload = {
        "receiverName": f"Phuc Vo {str(uuid.uuid4())[:4]}",
        "phone": "0899123456",
        "street": "268 Ly Thuong Kiet, Quan 10",
        "city": "Ho Chi Minh",
        "isDefault": False
    }
    
    response = requests.post(f"{base_url}/addresses", json=payload, headers=buyer_headers)
    
    # Lấy ID của địa chỉ vừa tạo
    address_data = response.json().get("data", {})
    address_id = address_data.get("id")
    
    yield address_data
    
    # Teardown: Dọn dẹp địa chỉ sau khi test xong (nếu chưa bị xóa trong test case)
    if address_id:
        # Gọi thử lệnh Xóa, nếu API báo 404 (do test case Delete đã xóa rồi) thì bỏ qua
        requests.delete(f"{base_url}/addresses/{address_id}", headers=buyer_headers)


# ==============================================================================
# 🧪 TEST CASES (CHUẨN CRUD)
# ==============================================================================

def test_create_address(base_url, buyer_headers):
    """
    TC_ADDR_01: BUYER THÊM ĐỊA CHỈ MỚI THÀNH CÔNG
    """
    payload = {
        "receiverName": "Người Nhận Test",
        "phone": "0900000000",
        "street": "123 Đường Test",
        "city": "Ha Noi",
        "isDefault": True
    }
    
    response = requests.post(f"{base_url}/addresses", json=payload, headers=buyer_headers)
    assert response.status_code in [200, 201]
    
    data = response.json().get("data", {})
    assert data["city"] == "Ha Noi"
    assert data["isDefault"] is True

    # Dọn dẹp thủ công
    requests.delete(f"{base_url}/addresses/{data['id']}", headers=buyer_headers)


def test_get_my_addresses(base_url, buyer_headers, temp_address):
    """
    TC_ADDR_02: BUYER XEM DANH SÁCH ĐỊA CHỈ CỦA CHÍNH MÌNH
    """
    response = requests.get(f"{base_url}/addresses", headers=buyer_headers)
    assert response.status_code == 200
    
    data = response.json().get("data", [])
    # Đảm bảo danh sách trả về là một mảng và có ít nhất 1 địa chỉ (do fixture tạo ra)
    assert isinstance(data, list)
    assert len(data) >= 1


def test_update_address(base_url, buyer_headers, temp_address):
    """
    TC_ADDR_03: BUYER CẬP NHẬT ĐỊA CHỈ CÓ SẴN
    """
    address_id = temp_address["id"]
    update_payload = {
        "receiverName": "Tên Đã Sửa",
        "phone": "0111222333",
        "street": temp_address["street"], # Giữ nguyên đường
        "city": temp_address["city"],     # Giữ nguyên TP
        "isDefault": True
    }
    
    response = requests.put(f"{base_url}/addresses/{address_id}", json=update_payload, headers=buyer_headers)
    assert response.status_code == 200
    
    data = response.json().get("data", {})
    assert data["receiverName"] == "Tên Đã Sửa"
    assert data["phone"] == "0111222333"


def test_delete_address(base_url, buyer_headers, temp_address):
    """
    TC_ADDR_04: BUYER XÓA ĐỊA CHỈ
    """
    address_id = temp_address["id"]
    
    response = requests.delete(f"{base_url}/addresses/{address_id}", headers=buyer_headers)
    assert response.status_code in [200, 204]
    
    # Kiểm tra lại xem thực sự đã bị xóa chưa
    get_response = requests.get(f"{base_url}/addresses", headers=buyer_headers)
    data = get_response.json().get("data", [])
    
    # Đảm bảo ID vừa xóa không còn nằm trong danh sách
    id_list = [addr["id"] for addr in data]
    assert address_id not in id_list


# ==============================================================================
# 🛡️ SECURITY TESTS (KIỂM THỬ BẢO MẬT IDOR)
# ==============================================================================

def test_idor_update_other_user_address(base_url, buyer_headers, temp_address):
    """
    TC_SEC_01 (IDOR): BUYER NÀY CỐ TÌNH SỬA ĐỊA CHỈ CỦA BUYER KHÁC
    Lưu ý: Yêu cầu Backend phải check quyền sở hữu (Owner Check).
    """
    # Lấy ID địa chỉ của Buyer A (từ fixture)
    target_address_id = temp_address["id"]
    
    # Tạo một token ảo hoặc lấy token của User B để đóng giả làm Hacker
    # Tạm thời ở đây ta dùng 1 Header rác, hoặc nếu bạn có fixture `buyer_b_headers` thì càng tốt
    hacker_headers = {
        "Authorization": "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI... (Token của người khác)"
    }
    
    malicious_payload = {
        "street": "Hacked Street",
    }
    
    # Lấy Header của người khác, đi gọi API sửa địa chỉ của mình
    response = requests.put(f"{base_url}/addresses/{target_address_id}", json=malicious_payload, headers=hacker_headers)
    
    # KỲ VỌNG BẮT BUỘC:
    # 401 Unauthorized (Nếu token fake) hoặc
    # 403 Forbidden / 404 Not Found (Nếu Backend an toàn không cho phép truy cập)
    # TUYỆT ĐỐI KHÔNG ĐƯỢC LÀ 200 OK
    assert response.status_code in [401, 403, 404], "LỖ HỔNG IDOR! Hệ thống cho phép sửa địa chỉ người khác!"