import os
from dotenv import load_dotenv

# Lệnh này sẽ tự động tìm file .env ở thư mục gốc và load các biến vào bộ nhớ
load_dotenv()

class Config:
    # Lấy giá trị từ file .env, nếu không có thì lấy giá trị mặc định là localhost
    FRONTEND_URL = os.getenv("FRONTEND_URL", "http://localhost:3000")
    API_URL = os.getenv("API_URL", "http://localhost:8080/api")
    
    # Tài khoản Buyer
    BUYER_EMAIL = os.getenv("TEST_BUYER_EMAIL", "goat")
    BUYER_PASS = os.getenv("TEST_BUYER_PASS", "Phuc123")
    BUYER_USERNAME = os.getenv("TEST_BUYER_USERNAME", "goat")
    
    # Tài khoản Seller
    SELLER_EMAIL = os.getenv("TEST_SELLER_EMAIL", "stephen@gmail.com")
    SELLER_PASS = os.getenv("TEST_SELLER_PASS", "Phuc123")
    SELLER_USERNAME = os.getenv("TEST_SELLER_USERNAME", "stephen@gmail.com")
    
    # Tài khoản Admin
    ADMIN_EMAIL = os.getenv("TEST_ADMIN_EMAIL", "west")
    ADMIN_PASS = os.getenv("TEST_ADMIN_PASS", "Phuc123")
    ADMIN_USERNAME = os.getenv("TEST_ADMIN_USERNAME", "west")
    
    # Playwright config
    HEADLESS = os.getenv("HEADLESS", "false").lower() == "true"
    SLOW_MO = 5000
    TIMEOUT = int(os.getenv("TIMEOUT", "30000"))
    
    # Test config
    BASE_URL = os.getenv("BASE_URL", "http://localhost:3000")