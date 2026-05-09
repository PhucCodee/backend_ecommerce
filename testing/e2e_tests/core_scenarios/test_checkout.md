# 🎭 E2E Test Case Descriptions

## 🛒 Test Case 1: Guest Buying Flow (test_guest_buying_flow)

**File:** `core_scenarios/test_checkout_payment_flow.py`

**Mục đích:** Test luồng mua hàng của guest (không đăng nhập) → thêm sản phẩm → đăng nhập → checkout → thanh toán COD

**Các bước quan trọng:**

1. **Navigate to Shop Page**
   - Click "Shop" button từ home page
   - Screenshot: `guest_flow_shop_page.png` - Shop page loaded

2. **Search Product**
   - Search "shirt" trong shop
   - Click vào sản phẩm "Linen Short-Sleeve Shirt"
   - Screenshot: `guest_flow_search_shirt.png` - Searching for shirt
   - Screenshot: `guest_flow_product_detail.png` - Product detail page

3. **Add to Cart**
   - Click "Add to Cart"
   - Screenshot: `guest_flow_added_to_cart.png` - Product added to cart

4. **Login**
   - Click "Login" button
   - Fill credentials (goat/Phuc123)
   - Screenshot: `guest_flow_login_success.png` - Login successful

5. **Adjust Quantity**
   - Click quantity buttons (tăng lên 3)
   - Screenshot: `guest_flow_quantity_adjusted.png` - Quantity adjusted to 3

6. **Proceed to Checkout**
   - Click "Proceed to Checkout"
   - Screenshot: `guest_flow_checkout_page.png` - Checkout page

7. **Fill Delivery Address**
   - Fill address form (123 Nguyen Hue, Ho Chi Minh, HCM, 70000)
   - Screenshot: `guest_flow_address_filled.png` - Delivery address filled

8. **Select Payment Method**
   - Choose "Cash on Delivery"
   - Screenshot: `guest_flow_payment_selected.png` - COD payment selected

9. **Place Order**
   - Click "Place Order"
   - Screenshot: `guest_flow_order_placed.png` - Order placed successfully

**Expected Results:**
- ✅ Guest có thể browse và add to cart
- ✅ Login thành công sau khi add to cart
- ✅ Checkout flow hoàn tất với COD payment
- ✅ Order được tạo thành công

---

## 💳 Test Case 2: User Checkout Flow with ZaloPay (test_user_checkout_flow_zalo)

**File:** `core_scenarios/test_checkout_payment_flow.py`

**Mục đích:** Test luồng mua hàng của user đã login → search sản phẩm → checkout → thanh toán ZaloPay

**Các bước quan trọng:**

1. **Login**
   - Navigate to login page
   - Fill credentials (goat/Phuc123)
   - Screenshot: `zalopay_flow_login_success.png` - Login successful

2. **Navigate to Shop**
   - Click "Shop" button
   - Screenshot: `zalopay_flow_shop_page.png` - Shop page loaded

3. **Search Product**
   - Search "jeans"
   - Click vào sản phẩm "WomenHigh-Rise Straight"
   - Screenshot: `zalopay_flow_search_jeans.png` - Searching for jeans
   - Screenshot: `zalopay_flow_product_detail.png` - Product detail page

4. **Add to Cart**
   - Click "Add to Cart"
   - Adjust quantity (click + buttons)
   - Screenshot: `zalopay_flow_added_to_cart.png` - Product in cart

5. **Proceed to Checkout**
   - Click "Proceed to Checkout"
   - Screenshot: `zalopay_flow_checkout_page.png` - Checkout page

6. **Fill Delivery Address**
   - Fill address form (123 Nguyen Hue, Ho Chi Minh, HCM, 700000)
   - Screenshot: `zalopay_flow_address_filled.png` - Address filled

7. **Select ZaloPay Payment**
   - Choose "ZaloPay" payment method
   - Screenshot: `zalopay_flow_zalopay_selected.png` - ZaloPay selected

8. **Place Order & Pay**
   - Click "Place Order & Pay"
   - Expect popup (ZaloPay redirect)
   - Screenshot: `zalopay_flow_zalopay_popup.png` - ZaloPay popup opened

**Expected Results:**
- ✅ User login thành công
- ✅ Search và add product thành công
- ✅ Checkout với ZaloPay redirect
- ✅ Order được submit và redirect đến ZaloPay

---

## 📸 Screenshot Locations


