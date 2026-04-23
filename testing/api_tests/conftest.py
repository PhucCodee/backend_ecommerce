# conftest.py
from shared.config import Config
import pytest
import requests



@pytest.fixture(scope="session")
def base_url():
    return Config.API_URL

@pytest.fixture
def admin_headers(base_url):
    payload = {"identifier": "west", "password": "Phuc123"}
    response = requests.post(f"{base_url}/auth/login", json=payload)
    token = response.json()['data']['accessToken'] if response.status_code == 200 else ""
    return {"Authorization": f"Bearer {token}", "Content-Type": "application/json"}

@pytest.fixture
def seller_headers(base_url):
    payload = {"identifier": "stephen@gmail.com", "password": "Phuc123"}
    response = requests.post(f"{base_url}/auth/login", json=payload)
    token = response.json()['data']['accessToken']  if response.status_code == 200 else ""
    return {"Authorization": f"Bearer {token}", "Content-Type": "application/json"}

@pytest.fixture
def user_headers(base_url):
    payload = {"identifier": "phuc1@gmail.com", "password": "Test123@"}
    response = requests.post(f"{base_url}/auth/login", json=payload)
    token = response.json()['data']['accessToken']  if response.status_code == 200 else ""
    return {"Authorization": f"Bearer {token}", "Content-Type": "application/json"}