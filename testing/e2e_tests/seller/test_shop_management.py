"""Seller e2e tests - Product and Shop Management tests"""
import re
from playwright.sync_api import Page, expect
from shared.config import Config


def test_seller_login_and_logout(page: Page) -> None:
    """Test seller login and logout functionality"""
    page.goto(Config.FRONTEND_URL)
    
    # Click login button
    page.get_by_role("button", name="Login").click()
    page.wait_for_load_state("networkidle")
    
    # Fill seller credentials
    page.get_by_role("textbox", name="Email or Username").fill(Config.SELLER_EMAIL)
    page.get_by_role("textbox", name="Password").fill(Config.SELLER_PASS)
    
    # Click sign in
    page.get_by_role("button", name="Sign in").click()
    page.wait_for_load_state("networkidle")
    
    # Verify seller is logged in
    expect(page.get_by_role("button", name="My Shop")).to_be_visible()
    
    # Logout
    page.get_by_role("button").filter(has_text=re.compile(r"^$")).nth(3).click()
    page.get_by_role("menuitem", name="Sign out").click()
    page.wait_for_load_state("networkidle")
    
    # Verify logged out
    expect(page.get_by_role("button", name="Login")).to_be_visible()


def test_seller_access_shop_dashboard(authenticated_page_seller: Page) -> None:
    """Test seller can access their shop dashboard"""
    page = authenticated_page_seller
    
    # Navigate to shop
    page.get_by_role("button", name="My Shop").click()
    page.wait_for_load_state("networkidle")
    
    # Verify shop dashboard
    expect(page.get_by_role("heading", name="Shop Dashboard")).to_be_visible()
    expect(page.get_by_text("Total Sales")).to_be_visible()
    expect(page.get_by_text("Total Products")).to_be_visible()


def test_seller_view_products(authenticated_page_seller: Page) -> None:
    """Test seller can view their products"""
    page = authenticated_page_seller
    
    # Navigate to products
    page.get_by_role("button", name="Products").click()
    page.wait_for_load_state("networkidle")
    
    # Verify products list
    expect(page.get_by_role("heading", name="My Products")).to_be_visible()
    expect(page.get_by_role("button", name="Add Product")).to_be_visible()


def test_seller_create_product(authenticated_page_seller: Page) -> None:
    """Test seller can create a new product"""
    page = authenticated_page_seller
    
    # Navigate to products
    page.get_by_role("button", name="Products").click()
    page.wait_for_load_state("networkidle")
    
    # Click add product
    page.get_by_role("button", name="Add Product").click()
    page.wait_for_load_state("networkidle")
    
    # Fill product form
    page.get_by_role("textbox", name="Product Name").fill("Test Product ABC")
    page.get_by_role("textbox", name="Category").click()
    page.get_by_role("option", name="Electronics").click()
    page.get_by_role("textbox", name="Description").fill("Test product description")
    page.get_by_role("spinbutton", name="Price").fill("99.99")
    
    # Submit
    page.get_by_role("button", name="Create Product").click()
    page.wait_for_load_state("networkidle")
    
    # Verify success
    expect(page.get_by_text("Product created successfully")).to_be_visible()


def test_seller_edit_product(authenticated_page_seller: Page) -> None:
    """Test seller can edit product details"""
    page = authenticated_page_seller
    
    # Navigate to products
    page.get_by_role("button", name="Products").click()
    page.wait_for_load_state("networkidle")
    
    # Click on first product to edit
    page.get_by_role("row").nth(1).click()
    page.wait_for_load_state("networkidle")
    
    # Click edit button
    page.get_by_role("button", name="Edit").click()
    page.wait_for_load_state("networkidle")
    
    # Update product name
    page.get_by_role("textbox", name="Product Name").fill("Updated Product Name")
    
    # Save changes
    page.get_by_role("button", name="Save").click()
    page.wait_for_load_state("networkidle")
    
    # Verify success
    expect(page.get_by_text("Product updated successfully")).to_be_visible()


def test_seller_delete_product(authenticated_page_seller: Page) -> None:
    """Test seller can delete a product"""
    page = authenticated_page_seller
    
    # Navigate to products
    page.get_by_role("button", name="Products").click()
    page.wait_for_load_state("networkidle")
    
    # Click delete on first product
    delete_button = page.get_by_role("button", name="Delete").first
    if delete_button.is_visible():
        delete_button.click()
        page.wait_for_load_state("networkidle")
    
    # Confirm deletion if modal appears
    confirm_button = page.get_by_role("button", name="Delete")
    if confirm_button.is_visible():
        confirm_button.click()
        page.wait_for_load_state("networkidle")
    
    # Verify success
    expect(page.get_by_text("Product deleted successfully")).to_be_visible()