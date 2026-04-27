#test_login.py

import re
from playwright.sync_api import Page, expect
from shared.config import Config


def test_buyer_login_and_logout(page: Page) -> None:
    """Test buyer login and logout workflow"""
    # 1. Go to frontend
    page.goto(Config.FRONTEND_URL)
    
    # 2. Click login button
    page.get_by_role("button", name="Login").click()
    page.wait_for_load_state("networkidle")
    
    # Fill login form
    page.get_by_role("textbox", name="Email or Username").fill(Config.BUYER_EMAIL)
    page.get_by_role("textbox", name="Password").fill(Config.BUYER_PASS)
    page.get_by_role("button", name="Sign in").click()
    page.wait_for_load_state("networkidle")
    
    # Assert: Verify logged in
    expect(page.get_by_role("button", name="Shop")).to_be_visible()
    
    # 3. Logout
    page.get_by_role("button").filter(has_text=re.compile(r"^$")).nth(3).click()
    page.get_by_role("menuitem", name="Sign out").click()
    page.wait_for_load_state("networkidle")
    
    # Assert: Verify logged out
    expect(page.get_by_role("button", name="Login")).to_be_visible()


def test_register(page: Page) -> None:
    """Test new buyer account registration"""
    page.goto(Config.FRONTEND_URL)
    page.get_by_role("button", name="Sign Up").click()
    page.wait_for_load_state("networkidle")
    
    page.get_by_role("textbox", name="First Name").fill("Phúc")
    page.get_by_role("textbox", name="Last Name").fill("Võ")
    page.get_by_role("textbox", name="Username").fill("phucvovo")
    page.get_by_role("textbox", name="Email").fill("phuc@hcmut.edu.vn")
    page.get_by_role("textbox", name="Phone (Optional)").fill("0999000123")
    page.get_by_role("textbox", name="Password", exact=True).fill("Haovovo2@")
    page.get_by_role("textbox", name="Confirm Password").fill("Haovovo2@")
    page.get_by_role("checkbox", name="I agree to the Terms of").click()
    page.get_by_role("button", name="Create Account").click()
    page.wait_for_load_state("networkidle")
    
    # Assert: Verify successful registration
    expect(page.get_by_text("Account created successfully")).to_be_visible(timeout=10000)

