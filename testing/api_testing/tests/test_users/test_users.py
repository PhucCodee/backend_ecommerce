# tests/test_users/test_users.py
import requests

def test_admin_get_all_users(base_url, admin_headers):
    """Test Admin lấy danh sách toàn bộ hệ thống user"""
    response = requests.get(f"{base_url}/users", headers=admin_headers)
    assert response.status_code == 200

def test_user_get_profile(base_url, user_headers):
    """Test User bình thường có thể xem profile của mình"""
    response = requests.get(f"{base_url}/users/profile", headers=user_headers)
    assert response.status_code == 200
    assert "email" in response.json()

def test_user_update_profile(base_url, user_headers):
    """Test User cập nhật thông tin cá nhân"""
    payload = {
        "dateOfBirth": "2004-01-01",
        "gender": "female",
        "bio": "Updated by automation",
        "preferredLanguage": "en"
    }
    response = requests.put(f"{base_url}/users/profile", json=payload, headers=user_headers)
    assert response.status_code == 200