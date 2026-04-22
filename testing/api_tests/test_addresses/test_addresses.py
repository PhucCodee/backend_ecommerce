# tests/test_addresses/test_addresses.py
import requests
import uuid
import pytest

# ==============================================================================
# 🛠️ FIXTURE: TẠO ĐỊA CHỈ TẠM THỜI
# ==============================================================================
@pytest.fixture
def temp_address(base_url, user_headers):
    """
    Tự động tạo 1 địa chỉ để test các chức năng Update/Delete.
    Sau khi test xong sẽ tự động dọn dẹp.
    """
    payload = {
        "type": 0,
        "label": f"Home {str(uuid.uuid4())[:4]}",
        "recipientName": "Auto Testing User",
        "phone": "+84123456789",
        "addressLine1": "123 Auto Street",
        "city": "HCM",
        "stateProvince": "District 1",
        "postalCode": "70000",
        "country": "VN",
        "isDefaultShipping": True,
        "isDefaultBilling": True
    }
    
    res = requests.post(f"{base_url}/addresses", json=payload, headers=user_headers)
    
    # Lấy ID địa chỉ (Tùy backend của bạn bọc data hay trả thẳng)
    data = res.json()
    address_id = data.get("data", data).get("addressId")
    
    yield address_id
    
    # Dọn dẹp
    if address_id:
        requests.delete(f"{base_url}/addresses/{address_id}", headers=user_headers)

# ==============================================================================
# 🧪 TEST CASES CHO NHÁNH ADDRESS
# ==============================================================================

def test_create_address(base_url, user_headers):
    """
    TC_ADDR_01: TẠO ĐỊA CHỈ MỚI
    - Kỳ vọng: 200/201 Created. Dữ liệu trả về đúng như đã gửi.
    """
    label_name = f"Office {str(uuid.uuid4())[:4]}"
    payload = {
        "type": 1,
        "label": label_name,
        "recipientName": "Cristiano Ronaldo",
        "phone": "+351123456789",
        "addressLine1": "123 Lisbon Street",
        "city": "Lisbon",
        "stateProvince": "Lisbon",
        "postalCode": "1000",
        "country": "Portugal"
    }
    
    res = requests.post(f"{base_url}/addresses", json=payload, headers=user_headers)
    assert res.status_code in [200, 201]
    
    data = res.json().get("data", res.json())
    assert data["label"] == label_name
    
    # Tự dọn dẹp
    requests.delete(f"{base_url}/addresses/{data['addressId']}", headers=user_headers)

def test_get_user_addresses(base_url, user_headers, temp_address):
    """
    TC_ADDR_02: LẤY DANH SÁCH ĐỊA CHỈ CỦA USER
    - Kỳ vọng: Trả về danh sách, và chứa địa chỉ vừa tạo.
    """
    res = requests.get(f"{base_url}/addresses", headers=user_headers)
    assert res.status_code == 200
    
    data = res.json().get("data", res.json())
    assert isinstance(data, list)
    # Kiểm tra xem danh sách có trống không
    assert len(data) > 0

def test_update_address(base_url, user_headers, temp_address):
    """
    TC_ADDR_03: CẬP NHẬT ĐỊA CHỈ
    """
    payload = {
        "type": 0,
        "label": "Updated Home Label",
        "recipientName": "Updated Name",
        "phone": "0987654321",
        "addressLine1": "456 Updated St",
        "city": "Hanoi",
        "stateProvince": "Ba Dinh",
        "postalCode": "10000",
        "country": "VN"
    }
    
    res = requests.put(f"{base_url}/addresses/{temp_address}", json=payload, headers=user_headers)
    assert res.status_code in [200, 204]

def test_delete_address(base_url, user_headers):
    """
    TC_ADDR_04: XÓA ĐỊA CHỈ
    """
    # 1. Tạo 1 địa chỉ để hiến tế
    payload = {
        "type": 0, "label": "To Be Deleted", "recipientName": "Delete Me",
        "phone": "123", "addressLine1": "123", "city": "1", "stateProvince": "1",
        "postalCode": "1", "country": "1"
    }
    create_res = requests.post(f"{base_url}/addresses", json=payload, headers=user_headers)
    addr_id = create_res.json().get("data", create_res.json())["addressId"]
    
    # 2. Xóa
    del_res = requests.delete(f"{base_url}/addresses/{addr_id}", headers=user_headers)
    assert del_res.status_code in [200, 204]