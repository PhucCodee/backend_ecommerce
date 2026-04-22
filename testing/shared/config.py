import os
from dotenv import load_dotenv

# Lệnh này sẽ tự động tìm file .env ở thư mục gốc và load các biến vào bộ nhớ
load_dotenv()

class Config:
    # Lấy giá trị từ file .env, nếu không có thì lấy giá trị mặc định là localhost
    FRONTEND_URL = os.getenv("FRONTEND_URL", "http://localhost:3000")
    API_URL = os.getenv("API_URL", "http://localhost:8080/api")
    
    # Tài khoản
    BUYER_EMAIL = os.getenv("TEST_BUYER_EMAIL", "default_user")
    BUYER_PASS = os.getenv("TEST_BUYER_PASS", "default_pass")