#test_login.py

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



def test_register(page:Page) -> None:
    page.goto("http://localhost:3000/")
    page.get_by_role("button", name="Sign Up").click()
    page.get_by_role("textbox", name="First Name").click()
    page.get_by_role("textbox", name="First Name").fill("Phúc")
    page.get_by_role("textbox", name="Last Name").click()
    page.get_by_role("textbox", name="Last Name").fill("Võ")
    page.get_by_role("textbox", name="Username").click()
    page.get_by_role("textbox", name="Username").fill("phucvovo")
    page.get_by_role("textbox", name="Email").click()
    page.get_by_role("textbox", name="Email").fill("phuc@hcmut.edu.vn")
    page.get_by_role("textbox", name="Phone (Optional)").click()
    page.get_by_role("textbox", name="Phone (Optional)").fill("0899000123")
    page.get_by_role("textbox", name="Password", exact=True).click()
    page.get_by_role("textbox", name="Password", exact=True).fill("Phucvovo2@")
    page.get_by_role("textbox", name="Confirm Password").click()
    page.get_by_role("textbox", name="Confirm Password").fill("Phucvovo2@")
    page.get_by_role("checkbox", name="I agree to the Terms of").click()
    page.get_by_role("button", name="Create Account").click()
    not expect(page.locator("#root")).to_contain_text("Request failed with status code 400")
