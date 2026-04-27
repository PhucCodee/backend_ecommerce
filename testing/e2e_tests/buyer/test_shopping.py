"""Buyer e2e tests - Shopping Cart and Checkout tests"""
import re
from playwright.sync_api import Page, expect
from shared.config import Config


def test_browse_products(page: Page) -> None:
    """Test buyer can browse products"""
    page.goto(Config.FRONTEND_URL)
    
    # Navigate to shop
    page.get_by_role("button", name="Shop").click()
    page.wait_for_load_state("networkidle")
    
    # Verify products are displayed
    expect(page.get_by_text("Products")).to_be_visible()
    expect(page.get_by_role("button", name="Add to Cart")).first.to_be_visible()


def test_search_products(page: Page) -> None:
    """Test buyer can search for products"""
    page.goto(Config.FRONTEND_URL)
    
    # Navigate to shop
    page.get_by_role("button", name="Shop").click()
    page.wait_for_load_state("networkidle")
    
    # Search for product
    search_box = page.get_by_placeholder("Search products")
    if search_box.is_visible():
        search_box.fill("Shirt")
        page.wait_for_load_state("networkidle")
        
        # Verify search results
        expect(page.get_by_text("Shirt")).to_be_visible()


def test_view_product_details(page: Page) -> None:
    """Test buyer can view product details"""
    page.goto(Config.FRONTEND_URL)
    
    # Navigate to shop
    page.get_by_role("button", name="Shop").click()
    page.wait_for_load_state("networkidle")
    
    # Click on first product
    page.get_by_role("button", name="Add to Cart").first.click()
    page.wait_for_load_state("networkidle")
    
    # Verify product details page
    expect(page.get_by_text("Product Details")).to_be_visible()
    expect(page.get_by_text("Price")).to_be_visible()


def test_add_product_to_cart(authenticated_page_buyer: Page) -> None:
    """Test buyer can add product to cart"""
    page = authenticated_page_buyer
    
    # Navigate to shop
    page.get_by_role("button", name="Shop").click()
    page.wait_for_load_state("networkidle")
    
    # Add first product to cart
    page.get_by_role("button", name="Add to Cart").first.click()
    page.wait_for_load_state("networkidle")
    
    # Verify success message
    expect(page.get_by_text("Added to cart")).to_be_visible()


def test_add_multiple_items_to_cart(authenticated_page_buyer: Page) -> None:
    """Test buyer can add multiple products to cart"""
    page = authenticated_page_buyer
    
    # Navigate to shop
    page.get_by_role("button", name="Shop").click()
    page.wait_for_load_state("networkidle")
    
    # Add multiple products
    page.get_by_role("button", name="Add to Cart").nth(0).click()
    page.wait_for_load_state("networkidle")
    
    page.get_by_role("button", name="Add to Cart").nth(1).click()
    page.wait_for_load_state("networkidle")
    
    # Verify items added
    expect(page.get_by_text("Added to cart")).to_be_visible()


def test_view_shopping_cart(authenticated_page_buyer: Page) -> None:
    """Test buyer can view shopping cart"""
    page = authenticated_page_buyer
    
    # Add item to cart first
    page.get_by_role("button", name="Shop").click()
    page.wait_for_load_state("networkidle")
    page.get_by_role("button", name="Add to Cart").first.click()
    page.wait_for_load_state("networkidle")
    
    # Click cart icon
    cart_button = page.locator("[data-testid='cart-button']")
    if not cart_button.is_visible():
        # Try alternative cart button
        page.get_by_role("button").filter(has_text=re.compile(r"Cart|cart")).first.click()
    else:
        cart_button.click()
    
    page.wait_for_load_state("networkidle")
    
    # Verify cart page
    expect(page.get_by_role("heading", name="Shopping Cart")).to_be_visible()


def test_update_cart_quantity(authenticated_page_buyer: Page) -> None:
    """Test buyer can update product quantity in cart"""
    page = authenticated_page_buyer
    
    # Add item to cart
    page.get_by_role("button", name="Shop").click()
    page.wait_for_load_state("networkidle")
    page.get_by_role("button", name="Add to Cart").first.click()
    page.wait_for_load_state("networkidle")
    
    # Open cart
    cart_button = page.locator("[data-testid='cart-button']")
    if not cart_button.is_visible():
        page.get_by_role("button").filter(has_text=re.compile(r"Cart|cart")).first.click()
    else:
        cart_button.click()
    
    page.wait_for_load_state("networkidle")
    
    # Update quantity
    quantity_input = page.get_by_role("spinbutton").first
    if quantity_input.is_visible():
        quantity_input.fill("3")
        page.wait_for_load_state("networkidle")
    
    # Verify update
    expect(quantity_input).to_have_value("3")


def test_remove_item_from_cart(authenticated_page_buyer: Page) -> None:
    """Test buyer can remove item from cart"""
    page = authenticated_page_buyer
    
    # Add item to cart
    page.get_by_role("button", name="Shop").click()
    page.wait_for_load_state("networkidle")
    page.get_by_role("button", name="Add to Cart").first.click()
    page.wait_for_load_state("networkidle")
    
    # Open cart
    cart_button = page.locator("[data-testid='cart-button']")
    if not cart_button.is_visible():
        page.get_by_role("button").filter(has_text=re.compile(r"Cart|cart")).first.click()
    else:
        cart_button.click()
    
    page.wait_for_load_state("networkidle")
    
    # Remove item
    remove_button = page.get_by_role("button", name="Delete").first
    if remove_button.is_visible():
        remove_button.click()
        page.wait_for_load_state("networkidle")
    
    # Verify removal
    expect(page.get_by_text("Item removed")).to_be_visible()