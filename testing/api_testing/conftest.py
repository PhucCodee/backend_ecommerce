# conftest.py
import pytest
import requests

BASE_URL = "http://localhost:8080/api"

@pytest.fixture
def base_url():
    return BASE_URL

@pytest.fixture
def admin_headers(base_url):
    payload = {"identifier": "west", "password": "Phuc123"}
    response = requests.post(f"{base_url}/auth/login", json=payload)
    token = response.text.strip() if response.status_code == 200 else ""
    return {"Authorization": f"Bearer {token}", "Content-Type": "application/json"}

@pytest.fixture
def seller_headers(base_url):
    payload = {"identifier": "stephen@gmail.com", "password": "Phuc123"}
    response = requests.post(f"{base_url}/auth/login", json=payload)
    token = response.text.strip() if response.status_code == 200 else ""
    return {"Authorization": f"Bearer {token}", "Content-Type": "application/json"}

@pytest.fixture
def user_headers(base_url):
    payload = {"identifier": "phuc1@gmail.com", "password": "Test123@"}
    response = requests.post(f"{base_url}/auth/login", json=payload)
    token = response.text.strip() if response.status_code == 200 else ""
    return {"Authorization": f"Bearer {token}", "Content-Type": "application/json"}