# tests/test_coupons/test_coupons.py
"""
📋 TEST SUITE: COUPONS - BASIC TEST CASES
==========================================
Purpose: Basic testing of coupon management (creation, retrieval, deletion)
Coverage: Admin coupon creation, validation, and application
"""

import pytest
import requests
import uuid
from datetime import datetime, timedelta


def test_admin_create_percentage_coupon(base_url, admin_headers):
    """
    TC_COUP_01: ADMIN TẠO MÃ GIẢM GIÁ THEO PHẦN TRĂM
    - Tiền điều kiện: Phải có quyền Admin.
    - Hành động: POST /coupons với discountType=0 (percentage).
    - Kỳ vọng: 200/201 Created.
    """
    random_code = f"SAVE{str(uuid.uuid4())[:8].upper()}"
    payload = {
        "code": random_code,
        "description": "20% off for new users",
        "discountType": 0,  # 0 = percentage
        "discountValue": 20,
        "minOrderAmount": 100000,
        "validFrom": (datetime.utcnow()).isoformat() + "Z",
        "validUntil": (datetime.utcnow() + timedelta(days=30)).isoformat() + "Z",
        "isActive": True
    }
    response = requests.post(f"{base_url}/coupons", json=payload, headers=admin_headers)
    assert response.status_code in [200, 201]
    assert response.json()["data"]["code"] == random_code


def test_admin_create_fixed_amount_coupon(base_url, admin_headers):
    """
    TC_COUP_02: ADMIN TẠO MÃ GIẢM GIÁ THEO SỐ TIỀN CỐ ĐỊNH
    - Hành động: POST /coupons với discountType=1 (fixed amount).
    - Kỳ vọng: 200/201 Created.
    """
    random_code = f"SAVE{str(uuid.uuid4())[:8].upper()}"
    payload = {
        "code": random_code,
        "description": "Save 50,000 VND on orders over 200,000",
        "discountType": 1,  # 1 = fixed amount
        "discountValue": 50000,
        "minOrderAmount": 200000,
        "isActive": True
    }
    response = requests.post(f"{base_url}/coupons", json=payload, headers=admin_headers)
    assert response.status_code in [200, 201]


def test_admin_create_free_shipping_coupon(base_url, admin_headers):
    """
    TC_COUP_03: ADMIN TẠO MÃ MIỄN PHÍ SHIPPING
    - Hành động: POST /coupons với discountType=2 (free shipping).
    - Kỳ vọng: 200/201 Created.
    """
    random_code = f"SHIP{str(uuid.uuid4())[:8].upper()}"
    payload = {
        "code": random_code,
        "description": "Free shipping for all orders",
        "discountType": 2,  # 2 = free shipping
        "discountValue": 0,
        "isActive": True
    }
    response = requests.post(f"{base_url}/coupons", json=payload, headers=admin_headers)
    assert response.status_code in [200, 201]


def test_admin_delete_coupon(base_url, admin_headers):
    """
    TC_COUP_04: ADMIN XÓA MÃ GIẢM GIÁ
    - Hành động: Tạo coupon -> DELETE /coupons/{couponId}.
    - Kỳ vọng: 200/204 No Content.
    """
    # 1. Create coupon first
    random_code = f"TEMP{str(uuid.uuid4())[:8].upper()}"
    create_payload = {
        "code": random_code,
        "description": "Temp coupon to delete",
        "discountType": 0,
        "discountValue": 10,
        "isActive": True
    }
    create_res = requests.post(f"{base_url}/coupons", json=create_payload, headers=admin_headers)
    assert create_res.status_code in [200, 201]
    
    coupon_id = create_res.json()["data"].get("id") or create_res.json()["data"].get("couponId")
    
    # 2. Delete coupon
    delete_res = requests.delete(f"{base_url}/coupons/{coupon_id}", headers=admin_headers)
    assert delete_res.status_code in [200, 204]


def test_non_admin_cannot_create_coupon(base_url, user_headers):
    """
    TC_COUP_05: NGƯỜI DÙNG BÌNH THƯỜNG KHÔNG THỂ TẠO MÃ GIẢM GIÁ
    - Tiền điều kiện: Dùng user_headers (không phải admin).
    - Hành động: Cố tình POST /coupons.
    - Kỳ vọng: 403 Forbidden hoặc 401 Unauthorized.
    """
    payload = {
        "code": "HACKER",
        "discountType": 0,
        "discountValue": 99,
        "isActive": True
    }
    response = requests.post(f"{base_url}/coupons", json=payload, headers=user_headers)
    assert response.status_code in [400, 401, 403]


def test_seller_cannot_create_coupon(base_url, seller_headers):
    """
    TC_COUP_06: NGƯỜI BÁN KHÔNG THỂ TẠO MÃ GIẢM GIÁ TOÀN HỆ THỐNG
    - Tiền điều kiện: Dùng seller_headers.
    - Hành động: Cố tình POST /coupons.
    - Kỳ vọng: 403 Forbidden.
    """
    payload = {
        "code": "SELLER_HACK",
        "discountType": 0,
        "discountValue": 99,
        "isActive": True
    }
    response = requests.post(f"{base_url}/coupons", json=payload, headers=seller_headers)
    assert response.status_code in [400, 401, 403]


def test_coupon_code_must_be_unique(base_url, admin_headers):
    """
    TC_COUP_07: MÃ GIẢM GIÁ PHẢI DUY NHẤT
    - Hành động: Tạo 2 coupon cùng code.
    - Kỳ vọng: Cái thứ 2 bị từ chối (400/409).
    """
    duplicate_code = f"UNIQUE{str(uuid.uuid4())[:4].upper()}"
    
    payload = {
        "code": duplicate_code,
        "description": "First coupon",
        "discountType": 0,
        "discountValue": 10,
        "isActive": True
    }
    
    # Create first
    res1 = requests.post(f"{base_url}/coupons", json=payload, headers=admin_headers)
    assert res1.status_code in [200, 201]
    
    # Try to create duplicate
    res2 = requests.post(f"{base_url}/coupons", json=payload, headers=admin_headers)
    assert res2.status_code in [400, 409]


def test_invalid_discount_value_rejected(base_url, admin_headers):
    """
    TC_COUP_08: TỪ CHỐI GIÁ TRỊ GIẢM GIÁ KHÔNG HỢP LỆ
    - Hành động: Tạo coupon với discountValue âm hoặc quá lớn.
    - Kỳ vọng: 400 Bad Request.
    """
    payload = {
        "code": f"INVALID{str(uuid.uuid4())[:4].upper()}",
        "description": "Invalid discount",
        "discountType": 0,
        "discountValue": -10,  # Negative value not allowed
        "isActive": True
    }
    response = requests.post(f"{base_url}/coupons", json=payload, headers=admin_headers)
    assert response.status_code == 400
