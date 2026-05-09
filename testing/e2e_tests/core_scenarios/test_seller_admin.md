# 🎭 Seller & Admin Test Case Descriptions

## 👨‍💼 Test Case 1: Admin Dashboard Navigation (test_admin_flow)

**File:** `core_scenarios/test_seller_admin_flow.py`

**Mục đích:** Test admin login và navigate qua tất cả sections của admin dashboard

**Các bước quan trọng:**

1. **Admin Login**
   - Navigate to admin login (http://localhost:3001/login)
   - Fill credentials (west/Phuc123)
   - Screenshot: `admin_flow_login_success.png` - Admin login successful

2. **Navigate Users Section**
   - Click "Users" menu
   - Screenshot: `admin_flow_users_page.png` - Users management page

3. **Navigate Products Section**
   - Click "Products" menu
   - Screenshot: `admin_flow_products_page.png` - Products management page

4. **Navigate Categories Section**
   - Click "Categories" menu
   - Screenshot: `admin_flow_categories_page.png` - Categories management page

5. **Navigate Orders Section**
   - Click "Orders" menu
   - Screenshot: `admin_flow_orders_page.png` - Orders management page

6. **Navigate Coupons Section**
   - Click "Coupons" menu
   - Screenshot: `admin_flow_coupons_page.png` - Coupons management page

7. **Navigate Reports Section**
   - Click "Reports" menu
   - Screenshot: `admin_flow_reports_page.png` - Reports page

8. **Navigate Monitoring Section**
   - Click "Monitoring" menu
   - Screenshot: `admin_flow_monitoring_page.png` - Monitoring page

9. **Return to Dashboard**
   - Click "Admin Dashboard" header
   - Screenshot: `admin_flow_dashboard_overview.png` - Dashboard overview

**Expected Results:**
- ✅ Admin login thành công
- ✅ Tất cả 7 sections (Users, Products, Categories, Orders, Coupons, Reports, Monitoring) accessible
- ✅ Navigation mượt mà giữa các pages
- ✅ Dashboard header clickable

---

## 🏪 Test Case 2: Seller Dashboard Exploration (test_example)

**File:** `core_scenarios/test_seller_admin_flow.py`

**Mục đích:** Test seller login và explore toàn bộ seller dashboard với metrics và navigation

**Các bước quan trọng:**

1. **Seller Login**
   - Navigate to seller login (http://localhost:3002/login)
   - Fill credentials (stephen@gmail.com/Phuc123)
   - Screenshot: `seller_flow_login_success.png` - Seller login successful

2. **View Dashboard Metrics**
   - Click "Total Revenue" metric
   - Screenshot: `seller_flow_revenue_metric.png` - Revenue metric highlighted
   - Click "Total Orders" metric
   - Screenshot: `seller_flow_orders_metric.png` - Orders metric highlighted
   - Click "Products" (2453 low stock items)
   - Screenshot: `seller_flow_products_overview.png` - Products overview with low stock
   - Click "Avg. Order Value" metric
   - Screenshot: `seller_flow_avg_order_value.png` - Average order value metric

3. **Explore Dashboard Sections**
   - Click "Orders Overview" heading
   - Screenshot: `seller_flow_orders_overview.png` - Orders overview section
   - Click "Revenue Overview" heading
   - Screenshot: `seller_flow_revenue_overview.png` - Revenue overview section
   - Click "Low Stock Alerts" heading
   - Screenshot: `seller_flow_low_stock_alerts.png` - Low stock alerts section
   - Click "Recent Orders" heading
   - Screenshot: `seller_flow_recent_orders.png` - Recent orders section

4. **Navigate Menu Items**
   - Click "Products" in menu
   - Screenshot: `seller_flow_product_management.png` - Product management page
   - Click "Orders" in menu
   - Screenshot: `seller_flow_orders_page.png` - Orders page
   - Click "Settings" in menu
   - Screenshot: `seller_flow_settings_page.png` - Settings page
   - Click "Seller Portal" in menu
   - Screenshot: `seller_flow_portal_overview.png` - Seller portal overview

**Expected Results:**
- ✅ Seller login thành công
- ✅ Dashboard metrics hiển thị đúng (Total Revenue, Total Orders, Products with 2453 low stock, Avg Order Value)
- ✅ Tất cả dashboard sections accessible (Orders Overview, Revenue Overview, Low Stock Alerts, Recent Orders)
- ✅ Menu navigation hoạt động (Products, Orders, Settings, Seller Portal)
- ✅ Low stock alerts hiển thị số lượng chính xác (2453 items)

---

## 📸 Screenshot Locations

Sau khi chạy test, screenshot sẽ được lưu tại:
