"""
Buyer e2e tests - Authentication tests
"""
import re
from playwright.sync_api import Page, expect
from shared.config import Config
from shared.utils import AuthUtils, WaitUtils


def test_buyer_login_with_email(page: Page) -> None:
    """Test buyer can login with email"""
    page.goto(Config.FRONTEND_URL)
    
    # Click login button
    page.get_by_role("button", name="Login").click()
    page.wait_for_load_state("networkidle")
    
    # Fill email and password
    page.get_by_role("textbox", name="Email or Username").fill(Config.BUYER_EMAIL)
    page.get_by_role("textbox", name="Password").fill(Config.BUYER_PASS)
    
    # Click sign in
    page.get_by_role("button", name="Sign in").click()
    page.wait_for_load_state("networkidle")
    
    # Verify login successful
    expect(page.get_by_role("button", name="Shop")).to_be_visible()


def test_buyer_login_with_username(page: Page) -> None:
    """Test buyer can login with username"""
    page.goto(Config.FRONTEND_URL)
    
    # Click login button
    page.get_by_role("button", name="Login").click()
   
    
    # Fill username and password
    page.get_by_role("textbox", name="Email or Username").fill(Config.BUYER_USERNAME)
    page.get_by_role("textbox", name="Password").fill(Config.BUYER_PASS)
    
    # Click sign in
    page.get_by_role("button", name="Sign in").click()
    
    page.get_by_role("button").filter(has_text=re.compile(r"^$")).nth(3).click()
    # Verify login successful
    expect(page.get_by_text("My Account")).to_be_visible()


def test_buyer_login_invalid_credentials(page: Page) -> None:
    """Test login with invalid credentials shows error"""
    page.goto(Config.FRONTEND_URL)
    
    # Click login button
    page.get_by_role("button", name="Login").click()
    page.wait_for_load_state("networkidle")
    
    # Fill invalid credentials
    page.get_by_role("textbox", name="Email or Username").fill("invalid@test.com")
    page.get_by_role("textbox", name="Password").fill("wrongpassword")
    
    # Click sign in
    page.get_by_role("button", name="Sign in").click()
    page.wait_for_load_state("networkidle")
    
    # Verify error message
    expect(page.locator("#root")).to_contain_text("Invalid credentials")


def test_buyer_login_empty_fields(page: Page) -> None:
    """Test login with empty fields shows validation error"""
    page.goto(Config.FRONTEND_URL)
    
    # Click login button
    page.get_by_role("button", name="Login").click()
    page.wait_for_load_state("networkidle")
    
    # Try to submit with empty fields
    page.get_by_role("button", name="Sign in").click()
    
    # Verify validation errors
    expect(page.get_by_role("button", name="Sign in")).to_be_visible()


def test_buyer_logout(authenticated_page_buyer: Page) -> None:
    """Test buyer can logout successfully"""
    page = authenticated_page_buyer
    
    # Click user menu
    page.get_by_role("button").filter(has_text=re.compile(r"^$")).nth(3).click()
    page.wait_for_load_state("networkidle")
    
    # Click logout
    page.get_by_role("menuitem", name="Sign out").click()
    page.wait_for_load_state("networkidle")
    
    # Verify logged out
    expect(page.get_by_role("button", name="Login")).to_be_visible()
    expect(page.get_by_role("button", name="Sign Up")).to_be_visible()


def test_buyer_register_new_account(page: Page) -> None:
    """Test buyer can register new account"""
    page.goto(Config.FRONTEND_URL)
    
    # Click sign up button
    page.get_by_role("button", name="Sign Up").click()
    page.wait_for_load_state("networkidle")
    
    # Fill registration form
    page.get_by_role("textbox", name="First Name").fill("Test")
    page.get_by_role("textbox", name="Last Name").fill("User")
    page.get_by_role("textbox", name="Username").fill("testuser2024")
    page.get_by_role("textbox", name="Email").fill("testuser2024@test.com")
    page.get_by_role("textbox", name="Phone (Optional)").fill("0123456789")
    page.get_by_role("textbox", name="Password", exact=True).fill("TestPass@123")
    page.get_by_role("textbox", name="Confirm Password").fill("TestPass@123")
    
    # Accept terms
    page.get_by_role("checkbox", name="I agree to the Terms of").click()
    
    # Submit
    page.get_by_role("button", name="Create Account").click()
    page.wait_for_load_state("networkidle")
    
    # Verify success
    WaitUtils.wait_for_success_message(page, "Account created successfully", timeout=10000)


def test_buyer_register_password_mismatch(page: Page) -> None:
    """Test registration fails with mismatched passwords"""
    page.goto(Config.FRONTEND_URL)
    
    # Click sign up button
    page.get_by_role("button", name="Sign Up").click()
    page.wait_for_load_state("networkidle")
    
    # Fill registration form with mismatched passwords
    page.get_by_role("textbox", name="First Name").fill("Test")
    page.get_by_role("textbox", name="Last Name").fill("User")
    page.get_by_role("textbox", name="Username").fill("testuser")
    page.get_by_role("textbox", name="Email").fill("test@test.com")
    page.get_by_role("textbox", name="Password", exact=True).fill("TestPass@123")
    page.get_by_role("textbox", name="Confirm Password").fill("DifferentPass@123")
    
    # Accept terms
    page.get_by_role("checkbox", name="I agree to the Terms of").click()
    
    # Submit
    page.get_by_role("button", name="Create Account").click()
    page.wait_for_load_state("networkidle")
    
    # Verify error
    expect(page.get_by_text("Passwords do not match")).to_be_visible()


def test_buyer_register_invalid_email(page: Page) -> None:
    """Test registration with invalid email format"""
    page.goto(Config.FRONTEND_URL)
    
    # Click sign up button
    page.get_by_role("button", name="Sign Up").click()
    page.wait_for_load_state("networkidle")
    
    # Fill form with invalid email
    page.get_by_role("textbox", name="First Name").fill("Test")
    page.get_by_role("textbox", name="Last Name").fill("User")
    page.get_by_role("textbox", name="Username").fill("testuser")
    page.get_by_role("textbox", name="Email").fill("invalidemail")
    page.get_by_role("textbox", name="Password", exact=True).fill("TestPass@123")
    page.get_by_role("textbox", name="Confirm Password").fill("TestPass@123")
    
    # Submit
    page.get_by_role("button", name="Create Account").click()
    page.wait_for_load_state("networkidle")
    
    # Verify error
    expect(page.get_by_text("Invalid email format")).to_be_visible()


def test_buyer_register_duplicate_email(page: Page) -> None:
    """Test registration fails with existing email"""
    page.goto(Config.FRONTEND_URL)
    
    # Click sign up button
    page.get_by_role("button", name="Sign Up").click()
    page.wait_for_load_state("networkidle")
    
    # Fill form with existing email
    page.get_by_role("textbox", name="First Name").fill("Test")
    page.get_by_role("textbox", name="Last Name").fill("User")
    page.get_by_role("textbox", name="Username").fill("newuser")
    page.get_by_role("textbox", name="Email").fill(Config.BUYER_EMAIL)  # Existing email
    page.get_by_role("textbox", name="Password", exact=True).fill("TestPass@123")
    page.get_by_role("textbox", name="Confirm Password").fill("TestPass@123")
    
    # Accept terms
    page.get_by_role("checkbox", name="I agree to the Terms of").click()
    
    # Submit
    page.get_by_role("button", name="Create Account").click()
    page.wait_for_load_state("networkidle")
    
    # Verify error
    expect(page.get_by_text("Email already exists")).to_be_visible()


def test_buyer_forgot_password(page: Page) -> None:
    """Test buyer can access forgot password flow"""
    page.goto(Config.FRONTEND_URL)
    
    # Click login button
    page.get_by_role("button", name="Login").click()
    page.wait_for_load_state("networkidle")
    
    # Click forgot password link
    page.get_by_role("link", name="Forgot password").click()
    page.wait_for_load_state("networkidle")
    
    # Verify forgot password form
    expect(page.get_by_role("heading", name="Forgot Password")).to_be_visible()
    expect(page.get_by_role("textbox", name="Email")).to_be_visible()
