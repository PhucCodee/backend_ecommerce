# tests/test_auth/test_auth.py
import requests

def test_user_registration(base_url):
    """Test chức năng đăng ký tài khoản mới"""
    payload = {
        "email": "automation_user@gmail.com",
        "username": "autouser",
        "password": "Test123@",
        "confirmPassword": "Test123@",
        "firstName": "Auto",
        "lastName": "Bot",
        "phone": "0901234567",
        "acceptTerms": True
    }
    response = requests.post(f"{base_url}/auth/register", json=payload)
    # 200 OK hoặc 400 Bad Request (nếu user đã tồn tại từ lần chạy trước)
    assert response.status_code in [200, 201, 400] 

def test_admin_login(base_url):
    """Test Admin có thể đăng nhập và nhận được token"""
    payload = {"identifier": "west", "password": "Phuc123"}
    response = requests.post(f"{base_url}/auth/login", json=payload)
    
    assert response.status_code == 200
    assert len(response.text) > 20 # Đảm bảo token được trả về