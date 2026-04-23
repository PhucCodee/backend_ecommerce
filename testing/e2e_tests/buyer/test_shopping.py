import re
from playwright.sync_api import Page, expect
from shared.config import Config

def test_simple_buy_pay(page: Page) -> None:
    page.goto(Config.FRONTEND_URL)
    
    # 1. Đăng nhập
    page.get_by_role("button", name="Login").click()
    page.get_by_role("textbox", name="Email or Username").fill(Config.BUYER_EMAIL)
    page.get_by_role("textbox", name="Password").fill(Config.BUYER_PASS)
    page.get_by_role("button", name="Sign in").click()
    
    # 2. Mua hàng
    page.get_by_role("button", name="Shop").click()
    page.get_by_text("Classic Crew T-Shirt$").click()
    page.get_by_role("button", name="Add to Cart").click()
    
    # 3. Giỏ hàng & Thanh toán
    page.get_by_role("button", name="1", exact=True).click() # Click icon giỏ hàng
    page.get_by_role("button", name="Proceed to Checkout").click()
    
    # Điền địa chỉ
    page.get_by_role("button", name="Continue to Payment").click()
    page.get_by_role("textbox", name="Street Address").fill("123 Main St")
    page.get_by_role("textbox", name="City").fill("San Francisco")
    page.get_by_role("textbox", name="State").fill("CA")
    page.get_by_role("textbox", name="ZIP Code").fill("94102")
    
    # Thanh toán & Review
    page.get_by_role("button", name="Continue to Payment").click()
    page.get_by_role("button", name="Continue to Review").click()
    page.get_by_role("textbox", name="Last 4 Digits").fill("1234")
    page.get_by_role("textbox", name="Expiry Date").fill("12/25") # Thêm dữ liệu còn thiếu
    
    page.get_by_role("button", name="Continue to Review").click()
    page.get_by_role("button", name="Place Order").click()
    
    # ASSERT BẮT BUỘC: Đảm bảo luồng mua hàng thành công
    # Cần chờ thông báo thành công hoặc URL đổi sang trang /orders
    expect(page.get_by_text("Order placed successfully")).to_be_visible(timeout=5000)


def test_checkout_empty_cart(page: Page) -> None:
    page.goto(Config.FRONTEND_URL)
    
    # Đăng nhập chuẩn
    page.get_by_role("button", name="Login").click()
    page.get_by_role("textbox", name="Email or Username").fill(Config.BUYER_EMAIL)
    page.get_by_role("textbox", name="Password").fill(Config.BUYER_PASS)
    page.get_by_role("button", name="Sign in").click()
    
    page.goto(f"{Config.FRONTEND_URL}/checkout")
    
    # Assert
    expect(page.get_by_role("heading")).to_contain_text("Your cart is empty")