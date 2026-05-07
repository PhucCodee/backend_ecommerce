# tests/test_categories/test_categories.py
import requests

def test_get_all_categories(base_url):
    """Kiểm tra API lấy danh sách toàn bộ danh mục (Không cần đăng nhập)"""
    response = requests.get(f"{base_url}/categories")
    
    # 1. Kiểm tra mã trạng thái trả về phải là 200 OK
    assert response.status_code == 200
    
    # 2. Kiểm tra dữ liệu trả về phải là một danh sách (list)
    assert isinstance(response.json(), list)

def test_admin_create_category(base_url, admin_headers):
    """Kiểm tra quyền Admin có thể tạo được danh mục mới"""
    payload = {
        "name": "Pants Test Automation",
        "parentCategoryId": 1,
        "description": "Created by Python automation",
        "imageUrl": "https://example.com/pants.jpg",
        "displayOrder": 1,
        "isCore": True,
        "isActive": True
    }
    
    # Gửi request kèm theo admin_headers (đã tự động có token)
    response = requests.post(
        f"{base_url}/categories", 
        json=payload, 
        headers=admin_headers
    )
    
    # 1. Kiểm tra tạo thành công (200 hoặc 201 Created)
    assert response.status_code in [200, 201]
    
    # 2. Kiểm tra tên danh mục trả về đúng như tên mình vừa gửi
    data = response.json()
    assert data["name"] == "Pants Test Automation"