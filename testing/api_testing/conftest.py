# conftest.py
import pytest
import requests

# URL gốc của backend ASP.NET đang chạy ở máy bạn
BASE_URL = "http://localhost:8080/api"

@pytest.fixture
def base_url():
    """Trả về URL gốc để các test case sử dụng."""
    return BASE_URL

@pytest.fixture
def admin_headers(base_url):
    """Tự động đăng nhập Admin và trả về Header chứa Token."""
    payload = {
        "identifier": "west",
        "password": "Phuc123"
    }
    response = requests.post(f"{base_url}/auth/login", json=payload)
    
    # Lấy token từ API trả về (giả sử API của bạn trả về chuỗi token trực tiếp hoặc trong dict)
    # Tùy thuộc vào backend của bạn, bạn có thể cần sửa dòng này thành response.json()["token"]
    token = response.text.strip() 
    
    return {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }

@pytest.fixture
def seller_headers(base_url):
    """Tự động đăng nhập Seller và trả về Header chứa Token."""
    payload = {
        "identifier": "stephen@gmail.com",
        "password": "Phuc123"
    }
    response = requests.post(f"{base_url}/auth/login", json=payload)
    token = response.text.strip()
    return {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }

@pytest.fixture
def user_headers(base_url):
    """Tự động đăng nhập User thường và trả về Header."""
    payload = {"identifier": "phuc1@gmail.com", "password": "Test123@"}
    response = requests.post(f"{base_url}/auth/login", json=payload)
    token = response.text.strip()
    return {"Authorization": f"Bearer {token}", "Content-Type": "application/json"}