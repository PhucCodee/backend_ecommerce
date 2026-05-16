from playwright.sync_api import Page, expect
from utils.screenshot_manager import ScreenshotManager

def test_admin_flow(page: Page) -> None:
    page.goto("http://localhost:3001/login")
    page.get_by_placeholder("Enter your email or username").click()
    page.get_by_placeholder("Enter your email or username").fill("west")
    page.get_by_placeholder("Enter your password").click()
    page.get_by_placeholder("Enter your password").fill("Phuc123@")
    ScreenshotManager.capture(page, "login", "admin", "Admin login page")

    page.get_by_role("button", name="Sign in").click()
    page.get_by_role("heading", name="Admin Dashboard").click()
    ScreenshotManager.capture(page, "1-admin_dashboard", "admin", "Admin dashboard page")
    
    page.get_by_text("Users", exact=True).click()
    ScreenshotManager.capture(page, "2-users", "admin", "Users page")

    page.get_by_role("heading", name="User Management").click()
    page.get_by_text("Products").click()
    page.get_by_role("heading", name="Product Moderation").click()
    ScreenshotManager.capture(page, "3-product_moderation", "admin", "Product moderation page")

    page.get_by_text("Categories", exact=True).click()
    page.get_by_role("heading", name="Category Management").click()
    ScreenshotManager.capture(page, "4-category_management", "admin", "Category management page")

    page.get_by_text("Orders").click()
    ScreenshotManager.capture(page, "5-orders", "admin", "Orders page")

    page.get_by_role("heading", name="Order Monitoring").click()
    ScreenshotManager.capture(page, "6-order_monitoring", "admin", "Order monitoring page")

    page.get_by_text("Coupons").click()
    ScreenshotManager.capture(page, "7-coupons", "admin", "Coupons page")

    page.get_by_text("Reports").click()
    ScreenshotManager.capture(page, "8-reports", "admin", "Reports page")
    page.get_by_role("heading", name="Reports & Analytics").click()

    page.get_by_text("Monitoring").click()
    ScreenshotManager.capture(page, "9-monitoring", "admin", "Monitoring page")
    page.get_by_role("heading", name="System Monitoring").click()


def test_example(page: Page) -> None:
    page.goto("http://localhost:3002/")
    page.goto("http://localhost:3002/login")
    page.get_by_placeholder("Enter your email or username").click()
    page.get_by_placeholder("Enter your email or username").fill("stephen@gmail.com")
    page.get_by_placeholder("Enter your email or username").press("Tab")
    page.get_by_placeholder("Enter your password").fill("Phuc123@")
    ScreenshotManager.capture(page, "1-login", "seller", "Seller login page")

    page.get_by_placeholder("Enter your password").press("Enter")
    page.get_by_text("Statistics").click()
    ScreenshotManager.capture(page, "2-statistics", "seller", "Seller statistics page")

    page.get_by_text("Dashboard").click()
    ScreenshotManager.capture(page, "3-dashboard", "seller", "Seller dashboard page")
    page.get_by_role("heading", name="Dashboard").click()

    page.get_by_role("menu").get_by_text("Products").click()
    page.get_by_role("heading", name="Products & SKUs").click()

    ScreenshotManager.capture(page, "4-products", "seller", "Seller products page")
    page.get_by_text("Inventory").click()
    page.get_by_role("heading", name="Inventory Management").click()

    ScreenshotManager.capture(page, "5-inventory", "seller", "Seller inventory page")
    page.get_by_text("Orders").click()
    page.get_by_role("heading", name="Order Management").click()

    ScreenshotManager.capture(page, "6-orders", "seller", "Seller orders page")
    page.get_by_text("Statistics").click()
    ScreenshotManager.capture(page, "7-statistics", "seller", "Seller statistics page")
    page.get_by_role("heading", name="Statistics").click()
