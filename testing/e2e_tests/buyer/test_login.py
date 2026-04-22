import re
from playwright.sync_api import Page, expect
from shared.config import Config


def test_buyer_login_and_logout(page: Page) -> None:
    # 1. Truy cập trang chủ
    page.goto(Config.FRONTEND_URL)
    
    # 2. Đăng nhập
    page.get_by_role("button", name="Login").click()
    page.get_by_role("textbox", name="Email or Username").fill(Config.BUYER_EMAIL)
    page.get_by_role("textbox", name="Password").fill(Config.BUYER_PASS)
    page.get_by_role("button", name="Sign in").click()
    
    # Assert (BẮT BUỘC): Kiểm tra xem đã đăng nhập thành công chưa
    # (Ví dụ: Chờ icon avatar xuất hiện, hoặc URL thay đổi)
    expect(page).to_have_url(Config.FRONTEND_URL + "/")
    
    # 3. Đăng xuất
    page.get_by_role("button").filter(has_text=re.compile(r"^$")).nth(3).click()
    page.get_by_role("menuitem", name="Sign out").click()
    
    # Assert: Kiểm tra đã về lại trạng thái chưa đăng nhập
    expect(page.get_by_role("button", name="Login")).to_be_visible()

import re
from playwright.sync_api import Page, expect


def test_example(page: Page) -> None:
    page.goto("http://localhost:3000/")
    page.get_by_role("button", name="Login").click()
    page.get_by_role("textbox", name="Email or Username").click()
    page.get_by_role("textbox", name="Email or Username").fill("goat")
    page.get_by_role("textbox", name="Password").dblclick()
    page.get_by_role("textbox", name="Password").fill("Phuc123")
    page.get_by_role("button", name="Sign in").click()
    page.get_by_role("button", name="Shop").click()
    page.get_by_text("Classic Crew T-Shirt$").click()
    page.get_by_role("button", name="Add to Cart").click()
    page.get_by_role("button", name="1", exact=True).click()
    page.get_by_role("button", name="Proceed to Checkout").click()
    page.get_by_role("button", name="Continue to Payment").click()
    page.get_by_role("textbox", name="Street Address").click()
    page.get_by_role("textbox", name="Street Address").fill("123 Main St")
    page.get_by_role("textbox", name="City").dblclick()
    page.get_by_role("textbox", name="City").fill("San Francisco")
    page.get_by_role("textbox", name="State").dblclick()
    page.get_by_role("textbox", name="State").fill("CA")
    page.get_by_role("textbox", name="ZIP Code").dblclick()
    page.get_by_role("textbox", name="ZIP Code").fill("94102")
    page.get_by_role("button", name="Continue to Payment").click()
    page.get_by_role("button", name="Continue to Review").click()
    page.get_by_role("textbox", name="Last 4 Digits").dblclick()
    page.get_by_role("textbox", name="Last 4 Digits").fill("1234")
    page.get_by_role("textbox", name="Expiry Date").click()
    page.get_by_role("button", name="Continue to Review").click()
    page.get_by_role("button", name="Place Order").click()
    page.get_by_role("button").filter(has_text=re.compile(r"^$")).nth(3).click()
    page.get_by_role("menuitem", name="Orders").click()
