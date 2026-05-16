"""
Enhanced Pytest configuration for E2E tests with Playwright
Includes screenshot capture, browser management, and test hooks
"""
import pytest
import os
from datetime import datetime
from pathlib import Path
from playwright.sync_api import sync_playwright, Page, Browser, BrowserContext, expect
from shared.config import Config

# Directories
SCREENSHOTS_DIR = Path(__file__).parent / "reports" / "screenshots"
SCREENSHOTS_DIR.mkdir(parents=True, exist_ok=True)


# ===============================================================================
# BROWSER & PAGE FIXTURES
# ===============================================================================

@pytest.fixture(scope="session")
def browser():
    """
    Create and manage a browser instance for the entire test session.
    Uses Chromium browser with optional headless mode and slow motion.
    """
    with sync_playwright() as p:
        browser = p.chromium.launch(
            headless=False,
            slow_mo=getattr(Config, 'SLOW_MO', 500)
        )
        yield browser
        browser.close()


@pytest.fixture
def context(browser: Browser):
    """
    Create a new browser context for each test.
    Isolated cookies, storage, and network settings.
    """
    context = browser.new_context(
        viewport={"width": 1280, "height": 720},
        ignore_https_errors=True,
        accept_downloads=True,
    )
    yield context
    context.close()


@pytest.fixture
def page(context: BrowserContext):
    """
    Create a new page for each test.
    Sets default timeouts for navigation and interactions.
    """
    page = context.new_page()
    page.set_default_timeout(getattr(Config, 'TIMEOUT', 30000))
    page.set_default_navigation_timeout(getattr(Config, 'TIMEOUT', 30000))
    
    yield page
    
    page.close()


# ===============================================================================
# URL FIXTURES - APPLICATION URLS
# ===============================================================================

@pytest.fixture
def buyer_url():
    """Buyer application URL"""
    return getattr(Config, 'BUYER_URL', 'http://localhost:3000')


@pytest.fixture
def admin_url():
    """Admin application URL"""
    return getattr(Config, 'ADMIN_URL', 'http://localhost:3001')


@pytest.fixture
def seller_url():
    """Seller application URL"""
    return getattr(Config, 'SELLER_URL', 'http://localhost:3002')


@pytest.fixture
def api_base_url():
    """API base URL"""
    return getattr(Config, 'API_URL', 'http://localhost:8080/api')


# ===============================================================================
# AUTHENTICATION FIXTURES
# ===============================================================================

@pytest.fixture
def buyer_creds():
    """Buyer credentials"""
    return {
        "identifier": "goat",
        "password": "Phuc123@",
        "email": "goat@example.com"
    }


@pytest.fixture
def seller_creds():
    """Seller credentials"""
    return {
        "identifier": "stephen@gmail.com",
        "password": "Phuc123@",
        "email": "stephen@gmail.com"
    }


@pytest.fixture
def admin_creds():
    """Admin credentials"""
    return {
        "identifier": "west",
        "password": "Phuc123@",
        "email": "west@example.com"
    }


# ===============================================================================
# HELPER FIXTURES
# ===============================================================================

@pytest.fixture
def wait_and_screenshot(page: Page):
    """Helper to wait for element and take screenshot"""
    def _wait_and_screenshot(selector, name=""):
        try:
            page.wait_for_selector(selector, timeout=5000)
            if name:
                timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
                screenshot_path = SCREENSHOTS_DIR / f"{name}_{timestamp}.png"
                page.screenshot(path=str(screenshot_path))
                print(f"📸 Screenshot: {name}")
        except Exception as e:
            print(f"⚠️ Error: {e}")
    
    return _wait_and_screenshot


# ===============================================================================
# SESSION FIXTURES - RESET BETWEEN TESTS
# ===============================================================================

@pytest.fixture(autouse=True)
def reset_state():
    """Reset state between tests"""
    yield
    # Add cleanup code here if needed


# ===============================================================================
# TEST HOOKS - SCREENSHOT CAPTURE
# ===============================================================================

@pytest.hookimpl(tryfirst=True, hookwrapper=True)
def pytest_runtest_makereport(item, call):
    """
    Pytest hook to capture screenshots on test completion.
    Captures on both success and failure.
    """
    outcome = yield
    rep = outcome.get_result()
    
    # Only capture for test calls (not setup/teardown)
    if rep.when == "call" and "page" in item.fixturenames:
        page = item.funcargs.get("page")
        if page:
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            test_name = item.name.replace("::", "_")
            status = "PASS" if rep.passed else "FAIL"
            
            screenshot_name = f"{test_name}_{status}_{timestamp}.png"
            screenshot_path = SCREENSHOTS_DIR / screenshot_name
            
            try:
                page.screenshot(path=str(screenshot_path), full_page=True)
                print(f"\n📸 Screenshot saved: {screenshot_path}")
            except Exception as e:
                print(f"⚠️ Failed to capture screenshot: {e}")


# ===============================================================================
# PYTEST CONFIGURATION
# ===============================================================================

def pytest_configure(config):
    """Configure pytest with custom markers"""
    config.addinivalue_line(
        "markers", 
        "e2e: mark test as end-to-end test"
    )
    config.addinivalue_line(
        "markers", 
        "core_scenario: mark test as core business scenario"
    )
    config.addinivalue_line(
        "markers", 
        "slow: mark test as slow running"
    )
