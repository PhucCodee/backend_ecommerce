# tests/test_auth/test_auth.py
import requests
import uuid

def test_user_registration_success(base_url):
    # Tạo chuỗi ngẫu nhiên để email không bao giờ bị trùng khi chạy test nhiều lần
    random_str = str(uuid.uuid4())[:8]
    payload = {
        "email": f"auto_{random_str}@gmail.com",
        "username": f"user_{random_str}",
        "password": "Test123@Password!",
        "confirmPassword": "Test123@Password!",
        "firstName": "Auto",
        "lastName": "Bot",
        "phone": "0901234567",
        "acceptTerms": True
    }
    response = requests.post(f"{base_url}/auth/register", json=payload)
    assert response.status_code in [200, 201]

def test_login_success(base_url):
    payload = {"identifier": "west", "password": "Phuc123"}
    response = requests.post(f"{base_url}/auth/login", json=payload)
    assert response.status_code == 200
    assert "eyJ" in response.text  # Kiểm tra token JWT hợp lệ

def test_login_fail_wrong_password(base_url):
    payload = {"identifier": "west", "password": "WrongPassword123!"}
    response = requests.post(f"{base_url}/auth/login", json=payload)
    assert response.status_code in [400, 401] # Thường là 401 Unauthorized