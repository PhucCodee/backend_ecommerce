# conftest.py
from shared.config import Config
import pytest
import requests
from py.xml import html



@pytest.fixture(scope="session")
def base_url():
    return Config.API_URL

@pytest.fixture
def admin_headers(base_url):
    payload = {"identifier": "west", "password": "Phuc123@"}
    response = requests.post(f"{base_url}/auth/login", json=payload)
    token = response.json()['data']['accessToken'] if response.status_code == 200 else ""
    return {"Authorization": f"Bearer {token}", "Content-Type": "application/json"}

@pytest.hookimpl(hookwrapper=True)
def pytest_runtest_makereport(item, call):
    """Attach test docstrings and custom metadata for pytest-html."""
    outcome = yield
    report = outcome.get_result()

    if report.when == "call":
        doc = item.function.__doc__ or ""
        report.description = doc.strip()

        if hasattr(report, "extra"):
            extra = report.extra
        else:
            extra = []

        # Add request/response or other metadata here if needed in future.
        report.extra = extra


def pytest_html_results_table_header(cells):
    cells.insert(1, html.th('Module'))
    cells.insert(2, html.th('Description'))


def pytest_html_results_table_row(report, cells):
    # Extract module name from nodeid (e.g., test_addresses/test_addresses_extended.py::TestAddressCreation)
    parts = report.nodeid.split("::")
    module_path = parts[0].replace("\\", "/").split("/")[-1].replace(".py", "")
    
    cells.insert(1, html.td(module_path))
    
    desc = getattr(report, 'description', '')
    cells.insert(2, html.td(desc or ''))



def pytest_html_results_table_html(report, data):
    if report.failed and report.longreprtext:
        data.append('Failure details:')
        data.append(str(report.longreprtext))
@pytest.fixture
def seller_headers(base_url):
    payload = {"identifier": "stephen@gmail.com", "password": "Phuc123@"}
    response = requests.post(f"{base_url}/auth/login", json=payload)
    token = response.json()['data']['accessToken']  if response.status_code == 200 else ""
    return {"Authorization": f"Bearer {token}", "Content-Type": "application/json"}

@pytest.fixture
def user_headers(base_url):
    payload = {"identifier": "goat", "password": "Phuc123@"}
    response = requests.post(f"{base_url}/auth/login", json=payload)
    token = response.json()['data']['accessToken']  if response.status_code == 200 else ""
    return {"Authorization": f"Bearer {token}", "Content-Type": "application/json"}