"""
Pytest configuration and fixtures for e2e tests using Playwright
"""
import pytest
from playwright.sync_api import sync_playwright, Page, Browser, BrowserContext
from shared.config import Config
import os

import pytest
from shared.config import Config  # Import đường dẫn tới file config của bạn



@pytest.fixture(scope="session")
def browser():
    """
    Create and manage a browser instance for the entire test session
    """
    with sync_playwright() as p:
        browser = p.chromium.launch(
            headless=Config.HEADLESS,
            slow_mo=Config.SLOW_MO
        )
        yield browser
        browser.close()


@pytest.fixture
def context(browser: Browser):
    """
    Create a new browser context for each test
    """
    context = browser.new_context(
        viewport={"width": 1280, "height": 720},
    )
    yield context
    context.close()


@pytest.fixture
def page(context: BrowserContext) -> Page:
    """
    Create a new page for each test
    """
    page = context.new_page()
    page.set_default_timeout(Config.TIMEOUT)
    page.set_default_navigation_timeout(Config.TIMEOUT)
    
    yield page
    
    page.close()


@pytest.fixture
def authenticated_page_buyer(page: Page) -> Page:
    """
    Create an authenticated page for buyer tests
    """
    page.goto(Config.FRONTEND_URL)
    page.get_by_role("button", name="Login").click()
    page.get_by_role("textbox", name="Email or Username").fill(Config.BUYER_EMAIL)
    page.get_by_role("textbox", name="Password").fill(Config.BUYER_PASS)
    page.get_by_role("button", name="Sign in").click()
    page.wait_for_load_state("networkidle")
    
    yield page


@pytest.fixture
def authenticated_page_seller(page: Page) -> Page:
    """
    Create an authenticated page for seller tests
    """
    page.goto(Config.FRONTEND_URL)
    page.get_by_role("button", name="Login").click()
    page.get_by_role("textbox", name="Email or Username").fill(Config.SELLER_EMAIL)
    page.get_by_role("textbox", name="Password").fill(Config.SELLER_PASS)
    page.get_by_role("button", name="Sign in").click()
    page.wait_for_load_state("networkidle")
    
    yield page


@pytest.fixture
def authenticated_page_admin(page: Page) -> Page:
    """
    Create an authenticated page for admin tests
    """
    page.goto(Config.FRONTEND_URL)
    page.get_by_role("button", name="Login").click()
    page.get_by_role("textbox", name="Email or Username").fill(Config.ADMIN_EMAIL)
    page.get_by_role("textbox", name="Password").fill(Config.ADMIN_PASS)
    page.get_by_role("button", name="Sign in").click()
    page.wait_for_load_state("networkidle")
    
    yield page


# Pytest hooks for additional logging
def pytest_configure(config):
    """Configure pytest plugins"""
    config.addinivalue_line(
        "markers", "e2e: mark test as e2e test"
    )


@pytest.hookimpl(tryfirst=True, hookwrapper=True)
def pytest_runtest_makereport(item, call):
    """Take screenshot on test failure"""
    outcome = yield
    rep = outcome.get_result()
    
    if rep.failed and "page" in item.fixturenames:
        page = item.funcargs.get("page")
        if page:
            screenshot_dir = "testing/report/screenshots"
            os.makedirs(screenshot_dir, exist_ok=True)
            screenshot_path = f"{screenshot_dir}/{item.name}_{call.when}.png"
            try:
                page.screenshot(path=screenshot_path)
                rep.sections.append(("screenshot", f"Screenshot saved: {screenshot_path}"))
            except Exception as e:
                rep.sections.append(("screenshot error", f"Failed to capture screenshot: {e}"))
