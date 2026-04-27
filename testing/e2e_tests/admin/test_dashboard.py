"""Admin e2e tests - Dashboard and Analytics tests"""
import re
from playwright.sync_api import Page, expect
from shared.config import Config


def test_admin_login_and_logout(page: Page) -> None:
    """Test admin login and logout functionality"""
    page.goto(Config.FRONTEND_URL)
    
    # Click login button
    page.get_by_role("button", name="Login").click()
    page.wait_for_load_state("networkidle")
    
    # Fill admin credentials
    page.get_by_role("textbox", name="Email or Username").fill(Config.ADMIN_EMAIL)
    page.get_by_role("textbox", name="Password").fill(Config.ADMIN_PASS)
    
    # Click sign in
    page.get_by_role("button", name="Sign in").click()
    page.wait_for_load_state("networkidle")
    
    # Verify admin is logged in
    expect(page.get_by_role("button", name="Dashboard")).to_be_visible()
    
    # Logout
    page.get_by_role("button").filter(has_text=re.compile(r"^$")).nth(3).click()
    page.get_by_role("menuitem", name="Sign out").click()
    page.wait_for_load_state("networkidle")
    
    # Verify logged out
    expect(page.get_by_role("button", name="Login")).to_be_visible()


def test_admin_access_dashboard(authenticated_page_admin: Page) -> None:
    """Test admin can access and view dashboard"""
    page = authenticated_page_admin
    
    # Navigate to dashboard
    page.get_by_role("button", name="Dashboard").click()
    page.wait_for_load_state("networkidle")
    
    # Verify dashboard elements are visible
    expect(page.get_by_role("heading", name="Dashboard")).to_be_visible()
    expect(page.get_by_text("Total Revenue")).to_be_visible()
    expect(page.get_by_text("Total Orders")).to_be_visible()
    expect(page.get_by_text("Total Customers")).to_be_visible()


def test_admin_manage_categories(authenticated_page_admin: Page) -> None:
    """Test admin can view product categories"""
    page = authenticated_page_admin
    
    # Navigate to categories
    page.get_by_role("button", name="Categories").click()
    page.wait_for_load_state("networkidle")
    
    # Verify categories list is visible
    expect(page.get_by_role("heading", name="Categories")).to_be_visible()
    expect(page.get_by_role("button", name="Create Category")).to_be_visible()


def test_admin_create_category(authenticated_page_admin: Page) -> None:
    """Test admin can create a new product category"""
    page = authenticated_page_admin
    
    # Navigate to categories
    page.get_by_role("button", name="Categories").click()
    page.wait_for_load_state("networkidle")
    
    # Click create button
    page.get_by_role("button", name="Create Category").click()
    page.wait_for_load_state("networkidle")
    
    # Fill form
    page.get_by_role("textbox", name="Category Name").fill("Test Electronics")
    page.get_by_role("textbox", name="Slug").fill("test-electronics")
    page.get_by_role("textbox", name="Description").fill("Test electronics category")
    
    # Submit
    page.get_by_role("button", name="Create").click()
    page.wait_for_load_state("networkidle")
    
    # Verify success
    expect(page.get_by_text("Category created successfully")).to_be_visible()


def test_admin_manage_users(authenticated_page_admin: Page) -> None:
    """Test admin can view and manage user accounts"""
    page = authenticated_page_admin
    
    # Navigate to users
    page.get_by_role("button", name="Users").click()
    page.wait_for_load_state("networkidle")
    
    # Verify users list is visible
    expect(page.get_by_role("heading", name="Users")).to_be_visible()
    expect(page.get_by_text("Email")).to_be_visible()