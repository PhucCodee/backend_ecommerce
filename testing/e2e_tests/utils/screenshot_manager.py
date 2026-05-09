"""Screenshot Manager for E2E tests"""
from datetime import datetime
from pathlib import Path
from playwright.sync_api import Page


class ScreenshotManager:
    """Manage screenshot capture and storage"""
    
    SCREENSHOTS_DIR = Path(__file__).parent.parent / "reports" / "screenshots"
    
    @classmethod
    def capture(cls, page: Page, filename: str, scenario: str = "", description: str = ""):
        """
        Capture screenshot with timestamp
        
        Args:
            page: Playwright page object
            filename: Screenshot filename (without extension)
            scenario: Scenario name (will be prepended to filename)
            description: Description of what the screenshot shows
        """
        # Ensure directory exists
        cls.SCREENSHOTS_DIR.mkdir(parents=True, exist_ok=True)
        
        # Generate timestamp
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        
        # Create filename with scenario prefix
        if scenario:
            full_filename = f"{scenario}_{filename}_{timestamp}.png"
        else:
            full_filename = f"{filename}_{timestamp}.png"
        
        # Full path
        screenshot_path = cls.SCREENSHOTS_DIR / full_filename
        
        # Take screenshot
        try:
            page.screenshot(path=str(screenshot_path), full_page=True)
            print(f"📸 Screenshot saved: {full_filename}")
            if description:
                print(f"   Description: {description}")
            return str(screenshot_path)
        except Exception as e:
            print(f"⚠️ Failed to capture screenshot: {e}")
            return None
    
    @classmethod
    def capture_on_failure(cls, page: Page, test_name: str):
        """Capture screenshot on test failure"""
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        filename = f"{test_name}_FAIL_{timestamp}.png"
        screenshot_path = cls.SCREENSHOTS_DIR / filename
        cls.SCREENSHOTS_DIR.mkdir(parents=True, exist_ok=True)
        
        try:
            page.screenshot(path=str(screenshot_path), full_page=True)
            print(f"📸 Failure screenshot: {filename}")
            return str(screenshot_path)
        except Exception as e:
            print(f"⚠️ Failed to capture failure screenshot: {e}")
            return None
    
    @classmethod
    def get_screenshots_dir(cls) -> Path:
        """Get screenshots directory path"""
        return cls.SCREENSHOTS_DIR
    
    @classmethod
    def clear_old_screenshots(cls, days: int = 7):
        """Clear screenshots older than specified days"""
        import time
        current_time = time.time()
        
        if not cls.SCREENSHOTS_DIR.exists():
            return
        
        for screenshot in cls.SCREENSHOTS_DIR.glob("*.png"):
            file_age = current_time - screenshot.stat().st_mtime
            if file_age > (days * 24 * 60 * 60):
                screenshot.unlink()
                print(f"🗑️  Deleted old screenshot: {screenshot.name}")