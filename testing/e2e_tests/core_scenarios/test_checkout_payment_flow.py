
from playwright.sync_api import Page, expect
import re
from utils.screenshot_manager import ScreenshotManager


def test_guest_buying_flow(page: Page) -> None:
    page.goto("http://localhost:3000/")
    
    # Step 1: Shop page
    page.get_by_role("button", name="Shop").click()
    ScreenshotManager.capture(page, "shop_page", "guest_flow", "Shop page loaded")
    
    # Step 2: Search product
    page.locator("div").filter(has_text=re.compile(r"^All ProductsShowing 20 curated piecesFiltersFeatured$")).get_by_placeholder("Search products...").click()
    page.locator("div").filter(has_text=re.compile(r"^All ProductsShowing 20 curated piecesFiltersFeatured$")).get_by_placeholder("Search products...").fill("shirt")
    ScreenshotManager.capture(page, "search_shirt", "guest_flow", "Searching for shirt")
    
    # Step 3: Product detail
    page.get_by_role("img", name="Linen Short-Sleeve Shirt").click()
    page.get_by_text("Linen Short-Sleeve Shirt").click()
    ScreenshotManager.capture(page, "product_detail", "guest_flow", "Product detail page")
    
    # Step 4: Add to cart
    page.get_by_role("button", name="Add to Cart").click()
    ScreenshotManager.capture(page, "added_to_cart", "guest_flow", "Product added to cart")
    
    # Step 5: Login
    page.get_by_role("button", name="Login").click()
    page.get_by_placeholder("Enter your email or username").click()
    page.get_by_placeholder("Enter your email or username").fill("hoat")
    page.get_by_placeholder("Enter your email or username").press("Tab")
    page.get_by_placeholder("Enter your password").fill("Phuc123")
    page.get_by_placeholder("Enter your email or username").dblclick()
    page.get_by_placeholder("Enter your email or username").fill("goat")
    page.get_by_placeholder("Enter your email or username").press("Enter")
    ScreenshotManager.capture(page, "login_success", "guest_flow", "Login successful")
    
    # Step 6: Adjust quantity
    page.get_by_role("button", name="1").click()
    page.get_by_role("button", name="1").click()
    ScreenshotManager.capture(page, "quantity_adjusted", "guest_flow", "Quantity adjusted to 3")
    
    # Step 7: Checkout
    page.get_by_role("heading", name="Linen Short-Sleeve Shirt").click()
    page.get_by_role("button", name="Proceed to Checkout").click()
    ScreenshotManager.capture(page, "checkout_page", "guest_flow", "Checkout page")
    
    # Step 8: Fill address
    page.get_by_placeholder("Nguyen Hue").click()
    page.get_by_placeholder("Nguyen Hue").fill("123 Nguyen Hue")
    page.get_by_placeholder("Nguyen Hue").press("Tab")
    page.get_by_placeholder("Ho Chi Minh").fill("Ho Chi Minh")
    page.get_by_placeholder("Ho Chi Minh").press("Tab")
    page.get_by_placeholder("HCM").fill("HCM")
    page.get_by_placeholder("HCM").press("Tab")
    page.get_by_placeholder("700000").fill("70000")
    page.get_by_placeholder("700000").press("Tab")
    page.get_by_placeholder("Optional delivery instructions").fill("no")
    ScreenshotManager.capture(page, "address_filled", "guest_flow", "Delivery address filled")
    
    # Step 9: Payment method
    page.get_by_role("button", name="Continue to Payment").click()
    page.locator("div").filter(has_text=re.compile(r"^Cash on DeliveryPay when your order arrives$")).first.click()
    page.locator(".bg-card > div:nth-child(2)").first.click()
    ScreenshotManager.capture(page, "payment_selected", "guest_flow", "COD payment selected")
    
    # Step 10: Place order
    page.get_by_role("button", name="Continue to Review").click()
    page.get_by_role("button", name="Place Order").click()
    ScreenshotManager.capture(page, "order_placed", "guest_flow", "Order placed successfully")


def test_user_checkout_flow_zalo(page: Page) -> None:
    page.goto("http://localhost:3000/")
    
    # Step 1: Login
    page.get_by_role("button", name="Login").click()
    page.get_by_placeholder("Enter your email or username").click()
    page.get_by_placeholder("Enter your email or username").fill("goat")
    page.get_by_placeholder("Enter your email or username").press("Tab")
    page.get_by_placeholder("Enter your password").fill("Phuc123")
    page.get_by_role("button", name="Sign in").click()
    ScreenshotManager.capture(page, "login_success", "zalopay_flow", "Login successful")
    
    # Step 2: Shop page
    page.get_by_role("button", name="Shop").click()
    ScreenshotManager.capture(page, "shop_page", "zalopay_flow", "Shop page loaded")
    
    # Step 3: Search product
    page.locator("div").filter(has_text=re.compile(r"^All ProductsShowing 20 curated piecesFiltersFeatured$")).get_by_placeholder("Search products...").click()
    page.locator("div").filter(has_text=re.compile(r"^All ProductsShowing 20 curated piecesFiltersFeatured$")).get_by_placeholder("Search products...").fill("jeans")
    ScreenshotManager.capture(page, "search_jeans", "zalopay_flow", "Searching for jeans")
    
    # Step 4: Product detail
    page.get_by_text("WomenHigh-Rise Straight").click()
    ScreenshotManager.capture(page, "product_detail", "zalopay_flow", "Product detail page")
    
    # Step 5: Add to cart
    page.get_by_role("button", name="Add to Cart").click()
    page.get_by_role("button", name="1", exact=True).click()
    page.get_by_role("button", name="1").click()
    ScreenshotManager.capture(page, "added_to_cart", "zalopay_flow", "Product in cart")
    
    # Step 6: Checkout
    page.get_by_role("button", name="Proceed to Checkout").click()
    ScreenshotManager.capture(page, "checkout_page", "zalopay_flow", "Checkout page")
    
    # Step 7: Fill address
    page.get_by_role("button", name="Continue to Payment").click()
    page.get_by_placeholder("Nguyen Hue").click()
    page.get_by_placeholder("Nguyen Hue").fill("123 Nguyen Hue")
    page.get_by_placeholder("Ho Chi Minh").click()
    page.get_by_placeholder("Ho Chi Minh").fill("Ho Chi Minh")
    page.get_by_placeholder("HCM").click()
    page.get_by_placeholder("HCM").fill("HCM")
    page.get_by_placeholder("700000").click()
    page.get_by_placeholder("700000").fill("700000")
    page.get_by_placeholder("Optional delivery instructions").click()
    page.get_by_placeholder("Optional delivery instructions").fill("no")
    ScreenshotManager.capture(page, "address_filled", "zalopay_flow", "Address filled")
    
    # Step 8: ZaloPay payment
    page.get_by_role("button", name="Continue to Payment").click()
    page.locator("div").filter(has_text=re.compile(r"^ZaloPayPay securely via ZaloPay — redirected after placing order$")).first.click()
    ScreenshotManager.capture(page, "zalopay_selected", "zalopay_flow", "ZaloPay selected")
    
    # Step 9: Place order
    page.get_by_role("button", name="Continue to Review").click()
    with page.expect_popup() as page1_info:
        page.get_by_role("button", name="Place Order & Pay").click()
    page1 = page1_info.value
    ScreenshotManager.capture(page, "zalopay_popup", "zalopay_flow", "ZaloPay popup opened")