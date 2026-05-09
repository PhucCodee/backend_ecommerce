
from playwright.sync_api import Page, expect
import re
from utils.screenshot_manager import ScreenshotManager


def test_admin_flow(page: Page) -> None:
    # Step 1: Admin login
    page.goto("http://localhost:3001/login")
    expect(page.get_by_placeholder("Enter your email or username")).to_be_visible()
    page.get_by_placeholder("Enter your email or username").dblclick()
    page.get_by_placeholder("Enter your email or username").fill("west")
    page.get_by_placeholder("Enter your email or username").press("Tab")
    page.get_by_placeholder("Enter your password").fill("Phuc123")
    page.get_by_placeholder("Enter your password").press("Enter")
    ScreenshotManager.capture(page, "login_success", "admin_flow", "Admin login successful")
    
    # Step 2: Users section
    page.get_by_text("Users").click()
    ScreenshotManager.capture(page, "users_page", "admin_flow", "Users management page")
    
    # Step 3: Products section
    page.get_by_text("Products").click()
    ScreenshotManager.capture(page, "products_page", "admin_flow", "Products management page")
    
    # Step 4: Categories section
    page.get_by_text("Categories", exact=True).click()
    ScreenshotManager.capture(page, "categories_page", "admin_flow", "Categories management page")
    
    # Step 5: Orders section
    page.get_by_text("Orders").click()
    ScreenshotManager.capture(page, "orders_page", "admin_flow", "Orders management page")
    
    # Step 6: Coupons section
    page.get_by_text("Coupons").click()
    ScreenshotManager.capture(page, "coupons_page", "admin_flow", "Coupons management page")
    
    # Step 7: Reports section
    page.get_by_text("Reports").click()
    ScreenshotManager.capture(page, "reports_page", "admin_flow", "Reports page")
    
    # Step 8: Monitoring section
    page.get_by_text("Monitoring").click()
    ScreenshotManager.capture(page, "monitoring_page", "admin_flow", "Monitoring page")
    
    # Step 9: Return to dashboard
    page.locator("div").filter(has_text=re.compile(r"^Admin Dashboard$")).click()
    ScreenshotManager.capture(page, "dashboard_overview", "admin_flow", "Dashboard overview")


def test_example(page: Page) -> None:
    # Step 1: Seller login
    page.goto("http://localhost:3002/")
    page.goto("http://localhost:3002/login")
    page.get_by_placeholder("Enter your email or username").click()
    page.get_by_placeholder("Enter your email or username").fill("stephen@gmail.com")
    page.get_by_placeholder("Enter your password").click()
    page.get_by_placeholder("Enter your password").fill("Phuc123")
    page.get_by_placeholder("Enter your password").press("Enter")
    ScreenshotManager.capture(page, "login_success", "seller_flow", "Seller login successful")
    
    # Step 2: Dashboard metrics
    page.get_by_text("Total Revenue").click()
    ScreenshotManager.capture(page, "revenue_metric", "seller_flow", "Revenue metric highlighted")
    
    page.locator("div").filter(has_text=re.compile(r"^Total Orders$")).click()
    ScreenshotManager.capture(page, "orders_metric", "seller_flow", "Orders metric highlighted")
    
    page.get_by_text("Products2453 low stock items").click()
    ScreenshotManager.capture(page, "products_overview", "seller_flow", "Products overview with low stock")
    
    page.locator("div").filter(has_text=re.compile(r"^Avg\. Order Value$")).click()
    ScreenshotManager.capture(page, "avg_order_value", "seller_flow", "Average order value metric")
    
    # Step 3: Dashboard sections
    page.get_by_role("heading", name="Orders Overview").click()
    ScreenshotManager.capture(page, "orders_overview", "seller_flow", "Orders overview section")
    
    page.get_by_role("heading", name="Revenue Overview").click()
    ScreenshotManager.capture(page, "revenue_overview", "seller_flow", "Revenue overview section")
    
    page.get_by_role("heading", name="Low Stock Alerts").click()
    ScreenshotManager.capture(page, "low_stock_alerts", "seller_flow", "Low stock alerts section")
    
    page.get_by_role("heading", name="Recent Orders").click()
    ScreenshotManager.capture(page, "recent_orders", "seller_flow", "Recent orders section")
    
    # Step 4: Menu navigation
    page.get_by_role("menu").get_by_text("Products").click()
    ScreenshotManager.capture(page, "product_management", "seller_flow", "Product management page")
    
    page.get_by_role("heading", name="Product Management").click()
    ScreenshotManager.capture(page, "product_management_heading", "seller_flow", "Product management heading")
    
    page.get_by_text("Orders").click()
    ScreenshotManager.capture(page, "orders_page", "seller_flow", "Orders page")
    
    page.get_by_text("Settings").click()
    ScreenshotManager.capture(page, "settings_page", "seller_flow", "Settings page")
    
    page.get_by_text("Seller Portal").click()
    ScreenshotManager.capture(page, "portal_overview", "seller_flow", "Seller portal overview")