"""Buyer e2e tests - Profile and Address Management tests"""
import re
from playwright.sync_api import Page, expect
from shared.config import Config


def test_update_profile(authenticated_page_buyer: Page) -> None:
    """Test buyer can update profile information"""
    page = authenticated_page_buyer
    
    # Click user menu
    page.get_by_role("button").filter(has_text=re.compile(r"^$")).nth(3).click()
    page.wait_for_load_state("networkidle")
    
    # Click "My Account" or "Profile"
    page.get_by_role("menuitem", name="My Account").click()
    page.wait_for_load_state("networkidle")
    
    # Click edit button
    page.get_by_role("button", name="Edit").click()
    page.wait_for_load_state("networkidle")
    
    # Update profile info
    page.get_by_role("textbox", name="First Name").fill("Lionel")
    page.get_by_role("textbox", name="Last Name").fill("Messi")
    page.get_by_role("textbox", name="Phone Number").fill("+1234567890")
    
    # Save changes
    page.get_by_role("button", name="Save").click()
    page.wait_for_load_state("networkidle")
    
    # Assert: Verify success message
    expect(page.get_by_text("Profile updated successfully")).to_be_visible()


def test_add_new_address(authenticated_page_buyer: Page) -> None:
    """Test buyer can add new address"""
    page = authenticated_page_buyer
    
    # Navigate to profile
    page.get_by_role("button").filter(has_text=re.compile(r"^$")).nth(3).click()
    page.get_by_role("menuitem", name="My Account").click()
    page.wait_for_load_state("networkidle")
    
    # Go to addresses tab
    page.get_by_role("tab", name="Addresses").click()
    page.wait_for_load_state("networkidle")
    
    # Click add new address
    page.get_by_role("button", name="Add New").click()
    page.wait_for_load_state("networkidle")
    
    # Fill address form
    page.get_by_role("textbox", name="Street").fill("268 Ly Thuong Kiet")
    page.get_by_role("textbox", name="Ward").fill("Ben Thanh")
    page.get_by_role("textbox", name="District").fill("District 1")
    page.get_by_role("textbox", name="City").fill("Ho Chi Minh City")
    page.get_by_role("textbox", name="Postal Code").fill("70000")
    
    # Submit
    page.get_by_role("button", name="Add Address").click()
    page.wait_for_load_state("networkidle")
    
    # Assert: Verify success
    expect(page.get_by_text("Address added successfully")).to_be_visible()


def test_update_address(authenticated_page_buyer: Page) -> None:
    """Test buyer can update existing address"""
    page = authenticated_page_buyer
    
    # Navigate to profile
    page.get_by_role("button").filter(has_text=re.compile(r"^$")).nth(3).click()
    page.get_by_role("menuitem", name="My Account").click()
    page.wait_for_load_state("networkidle")
    
    # Go to addresses tab
    page.get_by_role("tab", name="Addresses").click()
    page.wait_for_load_state("networkidle")
    
    # Click edit on first address
    page.get_by_role("button", name="Edit").first.click()
    page.wait_for_load_state("networkidle")
    
    # Update address
    page.get_by_role("textbox", name="Street").fill("Updated Street Address")
    
    # Save
    page.get_by_role("button", name="Save").click()
    page.wait_for_load_state("networkidle")
    
    # Assert
    expect(page.get_by_text("Address updated successfully")).to_be_visible()


def test_delete_address(authenticated_page_buyer: Page) -> None:
    """Test buyer can delete address"""
    page = authenticated_page_buyer
    
    # Navigate to profile
    page.get_by_role("button").filter(has_text=re.compile(r"^$")).nth(3).click()
    page.get_by_role("menuitem", name="My Account").click()
    page.wait_for_load_state("networkidle")
    
    # Go to addresses tab
    page.get_by_role("tab", name="Addresses").click()
    page.wait_for_load_state("networkidle")
    
    # Click delete on first address
    page.get_by_role("button", name="Delete").first.click()
    page.wait_for_load_state("networkidle")
    
    # Confirm deletion if modal appears
    confirm_button = page.get_by_role("button", name="Delete")
    if confirm_button.is_visible():
        confirm_button.click()
        page.wait_for_load_state("networkidle")
    
    # Assert
    expect(page.get_by_text("Address deleted successfully")).to_be_visible()