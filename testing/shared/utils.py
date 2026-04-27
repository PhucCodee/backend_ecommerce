"""
Utility functions for e2e testing with Playwright
"""
import re
from typing import Optional
from playwright.sync_api import Page, expect


class AuthUtils:
    """Utilities for authentication flows"""
    
    @staticmethod
    def login(page: Page, email: str, password: str, wait_for_redirect: bool = True) -> None:
        """
        Log in user with email and password
        
        Args:
            page: Playwright page object
            email: User email
            password: User password
            wait_for_redirect: Whether to wait for redirect after login
        """
        page.get_by_role("button", name="Login").click()
        page.get_by_role("textbox", name="Email or Username").fill(email)
        page.get_by_role("textbox", name="Password").fill(password)
        page.get_by_role("button", name="Sign in").click()
        
        if wait_for_redirect:
            page.wait_for_load_state("networkidle")
    
    @staticmethod
    def logout(page: Page) -> None:
        """
        Log out the current user
        
        Args:
            page: Playwright page object
        """
        # Click user avatar/menu button
        page.get_by_role("button").filter(has_text=re.compile(r"^$")).nth(3).click()
        page.get_by_role("menuitem", name="Sign out").click()
        page.wait_for_load_state("networkidle")


class CartUtils:
    """Utilities for shopping cart operations"""
    
    @staticmethod
    def add_to_cart(page: Page, product_name: str) -> None:
        """
        Search for and add a product to cart
        
        Args:
            page: Playwright page object
            product_name: Name of the product to add
        """
        # Navigate to shop if not already there
        if "shop" not in page.url:
            page.get_by_role("button", name="Shop").click()
            page.wait_for_load_state("networkidle")
        
        # Search and click product
        page.get_by_placeholder("Search products").fill(product_name)
        page.get_by_text(product_name).first.click()
        
        # Add to cart
        page.get_by_role("button", name="Add to Cart").click()
        page.wait_for_load_state("networkidle")
    
    @staticmethod
    def view_cart(page: Page) -> None:
        """
        Open the shopping cart
        
        Args:
            page: Playwright page object
        """
        cart_button = page.locator("[data-testid='cart-button']")
        if cart_button.is_visible():
            cart_button.click()
        else:
            page.get_by_role("button").filter(has_text=re.compile(r"Cart")).click()
        
        page.wait_for_load_state("networkidle")
    
    @staticmethod
    def clear_cart(page: Page) -> None:
        """
        Clear all items from cart
        
        Args:
            page: Playwright page object
        """
        CartUtils.view_cart(page)
        clear_button = page.get_by_role("button", name="Clear Cart")
        if clear_button.is_visible():
            clear_button.click()
            page.wait_for_load_state("networkidle")


class ProfileUtils:
    """Utilities for user profile operations"""
    
    @staticmethod
    def navigate_to_profile(page: Page) -> None:
        """
        Navigate to user profile page
        
        Args:
            page: Playwright page object
        """
        page.get_by_role("button").filter(has_text=re.compile(r"^$")).nth(2).click()
        page.get_by_role("menuitem", name="Profile").click()
        page.wait_for_load_state("networkidle")
    
    @staticmethod
    def update_profile(page: Page, first_name: str = None, last_name: str = None, 
                      phone: str = None) -> None:
        """
        Update user profile information
        
        Args:
            page: Playwright page object
            first_name: First name to update
            last_name: Last name to update
            phone: Phone number to update
        """
        ProfileUtils.navigate_to_profile(page)
        
        page.get_by_role("button", name="Edit").click()
        
        if first_name:
            page.get_by_role("textbox", name="First Name").fill(first_name)
        if last_name:
            page.get_by_role("textbox", name="Last Name").fill(last_name)
        if phone:
            page.get_by_role("textbox", name="Phone Number").fill(phone)
        
        page.get_by_role("button", name="Save").click()
        page.wait_for_load_state("networkidle")


class WaitUtils:
    """Utilities for waiting and assertions"""
    
    @staticmethod
    def wait_for_success_message(page: Page, message: str = "Success", timeout: int = 5000) -> None:
        """
        Wait for a success message to appear
        
        Args:
            page: Playwright page object
            message: Success message text to wait for
            timeout: Timeout in milliseconds
        """
        expect(page.get_by_text(message)).to_be_visible(timeout=timeout)
    
    @staticmethod
    def wait_for_element_visible(page: Page, selector: str, timeout: int = 5000) -> None:
        """
        Wait for an element to be visible
        
        Args:
            page: Playwright page object
            selector: Element selector
            timeout: Timeout in milliseconds
        """
        page.wait_for_selector(selector, timeout=timeout)
    
    @staticmethod
    def wait_for_navigation(page: Page, expected_url: str = None, timeout: int = 5000) -> None:
        """
        Wait for navigation to complete
        
        Args:
            page: Playwright page object
            expected_url: Expected URL after navigation
            timeout: Timeout in milliseconds
        """
        page.wait_for_load_state("networkidle", timeout=timeout)
        if expected_url:
            expect(page).to_have_url(expected_url, timeout=timeout)
