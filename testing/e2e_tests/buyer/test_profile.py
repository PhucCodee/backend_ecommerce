import re
from playwright.sync_api import Page, expect
from shared.config import Config

def test_update_profile(page: Page) -> None:
    page.goto(Config.FRONTEND_URL)
    
    page.get_by_role("button", name="Login").click()
    page.get_by_role("textbox", name="Email or Username").fill(Config.BUYER_EMAIL)
    page.get_by_role("textbox", name="Password").fill(Config.BUYER_PASS)
    page.get_by_role("button", name="Sign in").click()
    
    # Lời khuyên: Nhờ Dev thêm data-testid="user-avatar" vào nút này
    page.get_by_role("button").filter(has_text=re.compile(r"^$")).nth(2).click()
    page.get_by_role("menuitem", name="Profile").click()
    
    # Sửa thông tin
    page.locator("div").filter(has_text=re.compile(r"^First Name$")).click()
    page.get_by_role("button", name="Edit").click()
    page.get_by_role("textbox", name="First Name").fill("Lionel")
    page.get_by_role("textbox", name="Last Name").fill("Messi")
    page.get_by_role("textbox", name="Phone Number").fill("+123456789")
    page.get_by_role("button", name="Save").click()
    
    # ASSERT: Kiểm tra thông báo lưu thành công
    expect(page.get_by_text("Profile updated successfully")).to_be_visible()


def test_add_new_address(page: Page) -> None:
    page.goto(Config.FRONTEND_URL) # THÊM DÒNG NÀY ĐỂ TEST CHẠY ĐỘC LẬP
    
    page.get_by_role("button", name="Login").click()
    page.get_by_role("textbox", name="Email or Username").fill(Config.BUYER_EMAIL)
    page.get_by_role("textbox", name="Password").fill(Config.BUYER_PASS)
    page.get_by_role("button", name="Sign in").click()
    
    expect(page).to_have_url(f"{Config.FRONTEND_URL}/")
    
    # Vào quản lý địa chỉ
    page.get_by_role("button").filter(has_text=re.compile(r"^$")).nth(2).click()
    page.get_by_role("menuitem", name="Profile").click()
    page.get_by_role("tab", name="Addresses").click()
    page.get_by_role("button", name="Add New").click()
    
    # (Bạn bị thiếu khúc điền thông tin địa chỉ ở đây, tôi giả lập thêm vào)
    page.get_by_role("textbox", name="Street").fill("268 Ly Thuong Kiet")
    page.get_by_role("button", name="Add Address").click()
    
    # ASSERT
    expect(page.get_by_text("Address added")).to_be_visible()