# 🧪 API TESTING DOCUMENTATION

**Project:** E-Commerce Platform  
**Test Framework:** Pytest + Requests  
**Base URL:** `http://localhost:8080/api`  
**Configuration:** Located in `conftest.py` with fixtures for authentication

---

## 📋 TABLE OF CONTENTS

1. [Overview & Setup](#overview--setup)
2. [Authentication Tests](#authentication-tests)
3. [User & Profile Tests](#user--profile-tests)
4. [Address Tests](#address-tests)
5. [Category Tests](#category-tests)
6. [Product Tests](#product-tests)
7. [SKU Tests](#sku-tests)
8. [Cart Tests](#cart-tests)
9. [Order Tests](#order-tests)
10. [Coupon Tests](#coupon-tests)
11. [Payment Tests](#payment-tests)
12. [Inventory Tests](#inventory-tests)
13. [Running Tests](#running-tests)

---

## OVERVIEW & SETUP

### Test Structure
```
api_tests/
├── conftest.py                 # Shared fixtures & configuration
├── test_auth/                  # Authentication tests
│   ├── test_auth.py           # Basic auth tests
│   └── test_auth_extended.py  # Extended auth scenarios
├── test_users/                 # User profile & management tests
├── test_addresses/             # User address management tests
├── test_categories/            # Category CRUD tests
├── test_products/              # Product management tests
├── test_skus/                  # SKU management tests
├── test_cart/                  # Shopping cart tests
├── test_orders/                # Order management tests
├── test_coupons/               # Coupon & discount tests
├── test_payment/               # Payment processing tests
├── test_inventory/             # Inventory management tests
└── test_nfr/                   # Non-functional requirement tests
    ├── test_nfr_security.py
    ├── test_nfr_performance.py
    ├── test_nfr_reliability.py
    └── test_nfr_data_integrity.py
```

### Pytest Fixtures (conftest.py)

**Global Fixtures:**
- `base_url`: API base URL (http://localhost:8080/api)
- `admin_headers`: JWT token headers for admin user (username: "west")
- `seller_headers`: JWT token headers for seller user (email: "stephen@gmail.com")
- `user_headers`: JWT token headers for regular user (username: "goat")

**Usage Example:**
```python
def test_example(base_url, admin_headers):
    response = requests.get(
        f"{base_url}/categories",
        headers=admin_headers
    )
    assert response.status_code == 200
```

---

## AUTHENTICATION TESTS

**Location:** `api_tests/test_auth/`

### Basic Tests (test_auth.py)

| Test Name | Endpoint | Method | Purpose |
|-----------|----------|--------|---------|
| `test_user_registration_success` | POST /auth/register | POST | Verify successful user registration |
| `test_login_success` | POST /auth/login | POST | Verify successful login returns JWT token |
| `test_login_fail_wrong_password` | POST /auth/login | POST | Verify login rejection with wrong password |

### Extended Tests (test_auth_extended.py)

**Registration Validation Tests:**
- `test_user_registration_success_with_valid_data` - Valid credentials and data
- `test_user_registration_duplicate_email_rejection` - Duplicate email prevention
- `test_user_registration_duplicate_username_rejection` - Duplicate username prevention
- `test_registration_password_mismatch_rejection` - Password confirmation validation
- `test_registration_weak_password_rejection` - Password strength validation
- `test_registration_missing_required_fields` - Required field validation
- `test_registration_invalid_email_format` - Email format validation
- `test_registration_invalid_phone_format` - Phone format validation
- `test_registration_terms_not_accepted` - Terms acceptance enforcement

**Login Tests:**
- `test_user_login_success_with_username` - Login using username
- `test_user_login_success_with_email` - Login using email
- `test_login_fail_wrong_password` - Wrong password rejection
- `test_login_fail_nonexistent_user` - Non-existent user handling
- `test_login_fail_empty_credentials` - Empty credentials validation
- `test_seller_login_success` - Seller account login
- `test_admin_login_success` - Admin account login
- `test_login_returns_user_data` - Verify user data in response
- `test_token_used_in_protected_endpoint` - JWT token validation in protected routes

---

## USER & PROFILE TESTS

**Location:** `api_tests/test_users/`

### User Profile Tests

**Available Tests:**
- `test_get_user_profile` - Retrieve authenticated user profile
- `test_update_user_profile` - Update user information (firstName, lastName, bio, etc.)
- `test_get_user_by_id` - Get user details by ID (admin/authorized users)
- `test_list_all_users` - List users (admin only)
- `test_user_profile_validation` - Profile field validation
- `test_unauthorized_profile_access` - Verify access control

### User Role Tests
- `test_user_role_assignment` - Verify role-based access control
- `test_seller_role_privileges` - Seller-specific permissions
- `test_admin_role_privileges` - Admin-specific permissions

---

## ADDRESS TESTS

**Location:** `api_tests/test_addresses/`

### Address CRUD Operations

| Operation | Endpoint | Purpose |
|-----------|----------|---------|
| Create | POST /users/addresses | Add new user address |
| Read | GET /users/addresses | List user addresses |
| Read | GET /users/addresses/{id} | Get specific address |
| Update | PUT /users/addresses/{id} | Update address details |
| Delete | DELETE /users/addresses/{id} | Remove address |

### Address Tests
- `test_create_user_address` - Create new address with valid data
- `test_get_user_addresses` - List all user addresses
- `test_get_user_address_by_id` - Retrieve specific address
- `test_update_user_address` - Update address information
- `test_delete_user_address` - Delete address
- `test_address_validation` - Validate address fields (postal code, phone, etc.)
- `test_default_address_management` - Set/change default address
- `test_address_access_control` - Verify users can only access own addresses

---

## CATEGORY TESTS

**Location:** `api_tests/test_categories/`

### Category CRUD Operations

| Operation | Endpoint | Authentication | Purpose |
|-----------|----------|-----------------|---------|
| Create | POST /categories | Admin | Create new product category |
| Read | GET /categories | Public | List all categories |
| Read | GET /categories/{id} | Public | Get category details |
| Update | PUT /categories/{id} | Admin | Update category information |
| Delete | DELETE /categories/{id} | Admin | Remove category |

### Category Tests
- `test_get_all_categories` - List all product categories
- `test_create_category` - Create new category (admin only)
- `test_create_category_unauthorized` - Verify non-admin rejection
- `test_update_category` - Update category details
- `test_delete_category` - Delete category
- `test_category_hierarchy` - Test parent/child relationships
- `test_get_child_categories` - Retrieve subcategories
- `test_get_category_by_slug` - Category slug lookup

---

## PRODUCT TESTS

**Location:** `api_tests/test_products/`

### Product Operations

**Public Endpoints:**
- `test_get_all_products_public` - List products (public)
- `test_get_products_with_pagination` - Pagination support
- `test_get_product_by_id` - Get product details
- `test_get_products_filter_by_category` - Filter by category
- `test_get_products_filter_by_price_range` - Price range filtering
- `test_get_products_filter_by_brand` - Brand filtering
- `test_get_products_with_sorting` - Sort by price, rating, name, etc.
- `test_search_products_by_name` - Full-text search

**Seller Operations:**
- `test_seller_create_product` - Create product as seller
- `test_seller_get_own_products` - List own products
- `test_seller_update_product` - Update product details
- `test_seller_delete_product` - Delete own product

**Admin Operations:**
- `test_admin_delete_product` - Admin product removal
- `test_admin_approve_product` - Product approval workflow
- `test_admin_manage_visibility` - Control product visibility

**Security Tests:**
- `test_api_prevent_sql_injection_on_search` - SQL injection prevention
- `test_api_prevent_sql_injection_on_sort` - SQL injection in sorting

---

## SKU TESTS

**Location:** `api_tests/test_skus/`

### SKU Management

| Operation | Endpoint | Purpose |
|-----------|----------|---------|
| Create | POST /products/{id}/skus | Add product variant |
| Read | GET /products/{id}/skus | List product variants |
| Update | PUT /products/{id}/skus/{skuId} | Update SKU details |
| Delete | DELETE /products/{id}/skus/{skuId} | Remove variant |

### SKU Tests
- `test_create_product_sku` - Create new SKU/variant
- `test_get_product_skus` - List all SKUs for product
- `test_update_sku` - Update SKU price, stock, attributes
- `test_delete_sku` - Delete SKU variant
- `test_sku_price_tracking` - Price history tracking
- `test_sku_stock_management` - Stock level management

---

## CART TESTS

**Location:** `api_tests/test_cart/`

### Cart Operations

**Guest Cart (Session-based):**
- `test_guest_get_empty_cart` - Retrieve empty guest cart
- `test_guest_add_multiple_items` - Add multiple items to guest cart
- `test_guest_update_item_quantity` - Update item quantity
- `test_guest_delete_item` - Remove item from cart
- `test_guest_cart_session_persistence` - Cart persistence across requests

**User Cart (Persistent):**
- `test_user_get_cart` - Get user's persistent cart
- `test_user_add_to_cart` - Add item to user cart
- `test_user_cart_persistent` - Verify persistence after logout
- `test_cart_merge_on_login` - Merge guest & user cart on login

**Cart Operations:**
- `test_add_item_to_cart` - Add product with quantity
- `test_update_cart_item` - Update item quantity
- `test_delete_cart_item` - Remove item
- `test_clear_cart` - Clear all items
- `test_merge_cart_on_login` - Guest to user cart merge
- `test_cart_merge_resolves_duplicates` - Handle duplicate items
- `test_cart_subtotal_calculation` - Verify cart calculations

**Cart Validation:**
- `test_cart_stock_availability` - Check stock before add
- `test_cart_item_limits` - Maximum items validation
- `test_cart_price_recalculation` - Recalculate on price changes

---

## ORDER TESTS

**Location:** `api_tests/test_orders/`

### Order Operations

| Operation | Endpoint | Purpose |
|-----------|----------|---------|
| Create | POST /orders | Create order from cart |
| Read | GET /orders | List user orders |
| Read | GET /orders/{id} | Get order details |
| Update | PUT /orders/{id}/status | Update order status |

### Order Tests
- `test_create_order_from_cart` - Convert cart to order
- `test_get_user_orders` - List user's orders
- `test_get_order_by_id` - Retrieve specific order
- `test_order_status_tracking` - Order status progression
- `test_seller_view_orders` - Seller order visibility
- `test_admin_manage_orders` - Admin order management
- `test_order_total_calculation` - Server-side total verification
- `test_order_payment_linking` - Link payment to order

**Order Workflow:**
- `test_order_status_pending` - Initial pending status
- `test_order_status_confirmed` - Confirm order
- `test_order_status_shipped` - Mark as shipped
- `test_order_status_delivered` - Mark as delivered
- `test_order_cancellation` - Cancel order flow

---

## COUPON TESTS

**Location:** `api_tests/test_coupons/`

### Coupon Operations

| Operation | Endpoint | Authentication | Purpose |
|-----------|----------|-----------------|---------|
| Create | POST /coupons | Admin | Create discount coupon |
| Read | GET /coupons | Public | List available coupons |
| Validate | POST /coupons/validate | Public | Check coupon validity |
| Apply | POST /cart/apply-coupon | User | Apply coupon to cart |

### Coupon Tests
- `test_create_coupon` - Create new coupon (admin)
- `test_apply_valid_coupon_to_cart` - Apply active coupon
- `test_coupon_discount_calculation` - Verify discount amount
- `test_coupon_usage_limit` - Enforce usage limits
- `test_coupon_expiration` - Expired coupon rejection
- `test_coupon_minimum_amount` - Minimum order requirement
- `test_coupon_category_restriction` - Category-specific coupons
- `test_coupon_user_limit` - Per-user usage limits
- `test_remove_coupon_from_cart` - Remove applied coupon

---

## PAYMENT TESTS

**Location:** `api_tests/test_payment/`

### Payment Processing

| Operation | Endpoint | Purpose |
|-----------|----------|---------|
| Create | POST /payments | Initiate payment |
| Verify | POST /payments/verify | Verify payment status |
| Callback | POST /payments/callback | Payment gateway webhook |

### Payment Tests
- `test_initiate_payment` - Start payment process
- `test_payment_status_processing` - Processing status
- `test_payment_status_success` - Successful payment
- `test_payment_status_failed` - Failed payment handling
- `test_payment_verification` - Verify payment completion
- `test_payment_gateway_callback` - Handle webhook callbacks
- `test_refund_process` - Process refunds
- `test_payment_idempotency` - Prevent duplicate charges

---

## INVENTORY TESTS

**Location:** `api_tests/test_inventory/`

### Inventory Management

| Operation | Endpoint | Purpose |
|-----------|----------|---------|
| Read | GET /inventory/{skuId} | Check stock levels |
| Update | PUT /inventory/{skuId} | Update stock |
| Track | GET /inventory/history | View stock changes |

### Inventory Tests
- `test_get_inventory_status` - Check current stock
- `test_update_inventory` - Update stock levels
- `test_prevent_negative_inventory` - Prevent negative stock
- `test_inventory_reservation` - Reserve stock on order
- `test_inventory_release` - Release reserved stock
- `test_low_stock_alerts` - Monitor low stock
- `test_inventory_history_tracking` - Track all changes

---

## RUNNING TESTS

### Run All Tests
```bash
cd backend_ecommerce/testing
pytest api_tests/ -v
```

### Run Specific Test Module
```bash
# All auth tests
pytest api_tests/test_auth/ -v

# Basic auth tests only
pytest api_tests/test_auth/test_auth.py -v

# Extended tests only
pytest api_tests/test_auth/test_auth_extended.py -v
```

### Run Specific Test
```bash
pytest api_tests/test_auth/test_auth_extended.py::TestAuthentication::test_user_registration_success_with_valid_data -v
```

### Run with Output
```bash
# Show print statements
pytest api_tests/ -v -s

# Show coverage
pytest api_tests/ --cov=. --cov-report=html
```

### Run by Markers
```bash
# Run only smoke tests
pytest api_tests/ -m smoke -v

# Run excluding slow tests
pytest api_tests/ -m "not slow" -v
```

### Parallel Execution
```bash
# Install pytest-xdist first
pip install pytest-xdist

# Run tests in parallel (4 workers)
pytest api_tests/ -n 4 -v
```

---

## Test Configuration

**Configuration File:** `pytest.ini`
```ini
[pytest]
testpaths = 
    api_tests
    e2e_tests
python_files = test_*.py
python_classes = Test*
python_functions = test_*
pythonpath = .
```

**Requirements:** `requirements.txt`
```
pytest>=7.0
requests>=2.28.0
python-dotenv
```

---

## Environment Setup

**1. Install Dependencies:**
```bash
pip install -r requirements.txt
```

**2. Set Base URL (if needed):**
Edit `shared/config.py`:
```python
class Config:
    API_URL = "http://localhost:8080/api"
```

**3. Prepare Test Data:**
- Ensure test user accounts exist (west, stephen@gmail.com, goat)
- API server running and accessible
- Database seeded with test categories and products

**Expected Output:**
- **First Request:** 200 or 201 Created ✓
- **Second Request:** 
  - **Status Code:** 400 Bad Request or 409 Conflict
  - **Response Body:**
```json
{
  "success": false,
  "error": "Email already exists",
  "code": "DUPLICATE_EMAIL"
}
```

**Assertions:**
- First registration succeeds
- Second registration fails with 400/409
- Error message clearly states email duplication

---

### TC_AUTH_03: Login Success

| Field | Value |
|-------|-------|
| **Test ID** | TC_AUTH_03 |
| **Test Name** | Successful Login |
| **Endpoint** | `POST /auth/login` |
| **Method** | POST |
| **Authentication** | None |

**Test Intent:**  
Verify user can successfully login with valid credentials.

**Input (Request Body):**
```json
{
  "identifier": "west",
  "password": "Phuc123"
}
```

**Expected Output:**
- **Status Code:** 200 OK
- **Response Body:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGc...",
    "user": {
      "userId": "...",
      "username": "west",
      "email": "user@gmail.com",
      "role": "User"
    },
    "expiresIn": 3600
  }
}
```

**Assertions:**
- Status code is 200
- Response contains JWT token starting with "eyJ"
- Token can be used for authenticated requests

---

### TC_AUTH_04: Login Failure - Wrong Password

| Field | Value |
|-------|-------|
| **Test ID** | TC_AUTH_04 |
| **Test Name** | Login Fails with Wrong Password |
| **Endpoint** | `POST /auth/login` |
| **Method** | POST |
| **Authentication** | None |

**Test Intent:**  
Ensure system rejects login attempts with incorrect password.

**Input (Request Body):**
```json
{
  "identifier": "west",
  "password": "WrongPassword123!"
}
```

**Expected Output:**
- **Status Code:** 400 Bad Request or 401 Unauthorized
- **Response Body:**
```json
{
  "success": false,
  "error": "Invalid credentials",
  "code": "INVALID_PASSWORD"
}
```

**Assertions:**
- Status code is 400 or 401
- No token is returned
- Error message is user-friendly

---

## 2. USER & PROFILE TESTS

### TC_USER_01: Get Own Profile

| Field | Value |
|-------|-------|
| **Test ID** | TC_USER_01 |
| **Test Name** | User Get Own Profile |
| **Endpoint** | `GET /users/profile` |
| **Method** | GET |
| **Authentication** | Required (User Token) |

**Test Intent:**  
Verify authenticated user can retrieve their own profile information.

**Input:**
- **Headers:**
```json
{
  "Authorization": "Bearer <valid_token>",
  "Content-Type": "application/json"
}
```

**Expected Output:**
- **Status Code:** 200 OK
- **Response Body:**
```json
{
  "success": true,
  "data": {
    "userId": "...",
    "username": "testuser",
    "email": "test@gmail.com",
    "firstName": "Test",
    "lastName": "User",
    "phone": "0901234567",
    "role": "User"
  }
}
```

**Assertions:**
- Status code is 200
- Email matches the authenticated user
- All expected profile fields are present

---

### TC_USER_02: Update Own Profile

| Field | Value |
|-------|-------|
| **Test ID** | TC_USER_02 |
| **Test Name** | User Update Profile Information |
| **Endpoint** | `PUT /users/profile` |
| **Method** | PUT |
| **Authentication** | Required (User Token) |

**Test Intent:**  
Verify user can update their own profile information.

**Input (Request Body):**
```json
{
  "firstName": "UpdatedFirst",
  "lastName": "UpdatedLast",
  "phone": "0909999888"
}
```

**Expected Output:**
- **Status Code:** 200 OK or 204 No Content
- **Response Body (if 200):**
```json
{
  "success": true,
  "data": {
    "userId": "...",
    "firstName": "UpdatedFirst",
    "lastName": "UpdatedLast",
    "phone": "0909999888",
    "message": "Profile updated successfully"
  }
}
```

**Assertions:**
- Status code is 200 or 204
- Profile can be retrieved to verify changes
- Updated fields match the input

---

### TC_USER_03: Admin Get All Users

| Field | Value |
|-------|-------|
| **Test ID** | TC_USER_03 |
| **Test Name** | Admin Get All Users with Pagination |
| **Endpoint** | `GET /users` |
| **Method** | GET |
| **Authentication** | Required (Admin Token) |

**Test Intent:**  
Verify admin can retrieve paginated list of all users.

**Input:**
- **Query Parameters:**
```
?pageNumber=1&pageSize=10
```
- **Headers:**
```json
{
  "Authorization": "Bearer <admin_token>"
}
```

**Expected Output:**
- **Status Code:** 200 OK
- **Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "userId": "...",
        "username": "user1",
        "email": "user1@gmail.com",
        "role": "User"
      }
    ],
    "total": 150,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 15
  }
}
```

**Assertions:**
- Status code is 200
- Items array is not empty
- Pagination metadata is correct

---

### TC_SEC_01: User Cannot Access Admin Endpoint (Broken Access Control)

| Field | Value |
|-------|-------|
| **Test ID** | TC_SEC_01 |
| **Test Name** | Security - User Cannot Access Admin Endpoint |
| **Endpoint** | `GET /users` |
| **Method** | GET |
| **Authentication** | Required (Regular User Token) |

**Test Intent:**  
Verify role-based access control prevents regular users from accessing admin endpoints.

**Input:**
- **Headers:**
```json
{
  "Authorization": "Bearer <user_token>"
}
```

**Expected Output:**
- **Status Code:** 403 Forbidden
- **Response Body:**
```json
{
  "success": false,
  "error": "Access denied. Admin role required.",
  "code": "FORBIDDEN"
}
```

**Assertions:**
- Status code is 403 (not 401)
- User is authenticated but denied access
- Error message indicates permission issue

---

## 3. ADDRESS TESTS

### TC_ADDR_01: User Create Address

| Field | Value |
|-------|-------|
| **Test ID** | TC_ADDR_01 |
| **Test Name** | User Create Address |
| **Endpoint** | `POST /addresses` |
| **Method** | POST |
| **Authentication** | Required (User Token) |

**Test Intent:**  
Verify user can create a new delivery address.

**Input (Request Body):**
```json
{
  "type": 0,
  "label": "Home",
  "recipientName": "Cristiano Ronaldo",
  "phone": "+351123456789",
  "addressLine1": "123 Lisbon Street",
  "addressLine2": "Apt 10",
  "city": "Lisbon",
  "stateProvince": "Lisbon",
  "postalCode": "1000-001",
  "country": "Portugal",
  "isDefaultShipping": true,
  "isDefaultBilling": true
}
```

**Expected Output:**
- **Status Code:** 200 OK or 201 Created
- **Response Body:**
```json
{
  "success": true,
  "data": {
    "addressId": "...",
    "label": "Home",
    "recipientName": "Cristiano Ronaldo",
    "isDefaultShipping": true,
    "isDefaultBilling": true
  }
}
```

**Assertions:**
- Status code is 200 or 201
- addressId is returned
- Address appears in user's address list

---

### TC_ADDR_02: Get User Addresses

| Field | Value |
|-------|-------|
| **Test ID** | TC_ADDR_02 |
| **Test Name** | User Get All Addresses |
| **Endpoint** | `GET /addresses` |
| **Method** | GET |
| **Authentication** | Required (User Token) |

**Test Intent:**  
Verify user can retrieve their list of saved addresses.

**Input:**
- **Headers:**
```json
{
  "Authorization": "Bearer <user_token>"
}
```

**Expected Output:**
- **Status Code:** 200 OK
- **Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "addressId": "...",
      "label": "Home",
      "recipientName": "Cristiano Ronaldo",
      "phone": "+351123456789",
      "isDefaultShipping": true
    }
  ]
}
```

**Assertions:**
- Status code is 200
- Response is an array of addresses
- Default address is marked appropriately

---

### TC_ADDR_03: Update Address

| Field | Value |
|-------|-------|
| **Test ID** | TC_ADDR_03 |
| **Test Name** | User Update Address |
| **Endpoint** | `PUT /addresses/{addressId}` |
| **Method** | PUT |
| **Authentication** | Required (User Token) |

**Test Intent:**  
Verify user can update an existing address.

**Input:**
- **URL Parameter:** `addressId = 12`
- **Request Body:**
```json
{
  "recipientName": "Cristiano Updated",
  "phone": "+351987654321",
  "city": "Porto"
}
```

**Expected Output:**
- **Status Code:** 200 OK or 204 No Content
- **Response Body (if 200):**
```json
{
  "success": true,
  "data": {
    "addressId": "12",
    "recipientName": "Cristiano Updated",
    "phone": "+351987654321",
    "city": "Porto"
  }
}
```

**Assertions:**
- Status code is 200 or 204
- Changes can be verified via GET
- Non-provided fields remain unchanged

---

## 4. CATEGORY TESTS

### TC_CAT_01: Get All Categories (Public)

| Field | Value |
|-------|-------|
| **Test ID** | TC_CAT_01 |
| **Test Name** | Get All Categories |
| **Endpoint** | `GET /categories` |
| **Method** | GET |
| **Authentication** | None (Public) |

**Test Intent:**  
Verify public users can retrieve complete list of product categories.

**Input:**
- **Query Parameters:** (Optional)
```
?pageNumber=1&pageSize=20
```

**Expected Output:**
- **Status Code:** 200 OK
- **Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "categoryId": "1",
        "name": "Electronics",
        "slug": "electronics",
        "description": "Electronic devices",
        "isCore": true,
        "isActive": true,
        "displayOrder": 1
      }
    ],
    "total": 25,
    "pageNumber": 1,
    "pageSize": 20
  }
}
```

**Assertions:**
- Status code is 200
- Items array contains categories
- Each category has required fields

---

### TC_CAT_02: Get Core Categories

| Field | Value |
|-------|-------|
| **Test ID** | TC_CAT_02 |
| **Test Name** | Get Core Categories |
| **Endpoint** | `GET /categories/core` |
| **Method** | GET |
| **Authentication** | None (Public) |

**Test Intent:**  
Verify core categories are retrieved (main categories at root level).

**Input:**
- **Headers:** None

**Expected Output:**
- **Status Code:** 200 OK
- **Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "categoryId": "1",
        "name": "Electronics",
        "isCore": true,
        "parentCategoryId": null
      }
    ]
  }
}
```

**Assertions:**
- Status code is 200
- All items have `isCore: true`
- No parent category (root level)

---

### TC_CAT_03: Admin Create Category

| Field | Value |
|-------|-------|
| **Test ID** | TC_CAT_03 |
| **Test Name** | Admin Create Category |
| **Endpoint** | `POST /categories` |
| **Method** | POST |
| **Authentication** | Required (Admin Token) |

**Test Intent:**  
Verify admin can create new product categories.

**Input (Request Body):**
```json
{
  "name": "Fashion",
  "description": "Clothing and accessories",
  "isCore": true,
  "isActive": true,
  "displayOrder": 5
}
```

**Expected Output:**
- **Status Code:** 200 OK or 201 Created
- **Response Body:**
```json
{
  "success": true,
  "data": {
    "categoryId": "26",
    "name": "Fashion",
    "slug": "fashion",
    "description": "Clothing and accessories",
    "isCore": true,
    "isActive": true
  }
}
```

**Assertions:**
- Status code is 200 or 201
- categoryId is returned
- Category is retrievable via GET

---

## 5. PRODUCT TESTS

### TC_PROD_01: Get Public Products

| Field | Value |
|-------|-------|
| **Test ID** | TC_PROD_01 |
| **Test Name** | Get Products (Public Browse) |
| **Endpoint** | `GET /products` |
| **Method** | GET |
| **Authentication** | None (Public) |

**Test Intent:**  
Verify public users can browse and filter available products.

**Input (Query Parameters):**
```
?pageNumber=1&pageSize=10&sortBy=name&desc=false
```

**Expected Output:**
- **Status Code:** 200 OK
- **Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "productId": "1",
        "name": "Nike Shoes",
        "description": "High quality shoes",
        "brand": "Nike",
        "categoryNames": ["Footwear", "Sports"],
        "rating": 4.5,
        "reviewCount": 120,
        "weightKg": 0.3
      }
    ],
    "total": 500,
    "pageNumber": 1,
    "pageSize": 10
  }
}
```

**Assertions:**
- Status code is 200
- Items array is not empty
- Pagination data is correct

---

### TC_PROD_02: Seller Create Product

| Field | Value |
|-------|-------|
| **Test ID** | TC_PROD_02 |
| **Test Name** | Seller Create Product |
| **Endpoint** | `POST /products/seller` |
| **Method** | POST |
| **Authentication** | Required (Seller Token) |

**Test Intent:**  
Verify seller can create a new product listing.

**Input (Request Body):**
```json
{
  "name": "Premium Laptop",
  "description": "High performance laptop for professionals",
  "categoryIds": [1, 2],
  "brand": "Dell",
  "weightKg": 2.5,
  "defaultSkuPrice": 999.99,
  "defaultSkuStock": 50,
  "dimensionsCm": "35x25x2"
}
```

**Expected Output:**
- **Status Code:** 200 OK or 201 Created
- **Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "101",
    "name": "Premium Laptop",
    "slug": "premium-laptop",
    "sellerId": "...",
    "defaultSkuId": "501",
    "createdAt": "2024-05-04T10:30:00Z",
    "status": "Draft"
  }
}
```

**Assertions:**
- Status code is 200 or 201
- Product ID is returned
- Status is "Draft" (awaiting approval)

---

### TC_PROD_03: Admin Approve Product

| Field | Value |
|-------|-------|
| **Test ID** | TC_PROD_03 |
| **Test Name** | Admin Approve Seller Product |
| **Endpoint** | `PUT /products/{productId}/approve` |
| **Method** | PUT |
| **Authentication** | Required (Admin Token) |

**Test Intent:**  
Verify admin can approve seller-created products for public listing.

**Input:**
- **URL Parameter:** `productId = 101`
- **Request Body:**
```json
{
  "status": "Active"
}
```

**Expected Output:**
- **Status Code:** 200 OK
- **Response Body:**
```json
{
  "success": true,
  "data": {
    "productId": "101",
    "status": "Active",
    "approvedAt": "2024-05-04T11:00:00Z",
    "approvedBy": "admin"
  }
}
```

**Assertions:**
- Status code is 200
- Product status changes to "Active"
- Product is now publicly visible

---

## 6. SKU TESTS

### TC_SKU_01: Get Product SKUs

| Field | Value |
|-------|-------|
| **Test ID** | TC_SKU_01 |
| **Test Name** | Get SKUs for Product |
| **Endpoint** | `GET /skus` |
| **Method** | GET |
| **Authentication** | None (Public) |

**Test Intent:**  
Verify users can view all SKUs (size/color variants) for a product.

**Input (Query Parameters):**
```
?productId=101
```

**Expected Output:**
- **Status Code:** 200 OK
- **Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "skuId": "501",
      "productId": "101",
      "sku": "LAPTOP-DELL-001",
      "price": 999.99,
      "stock": 50,
      "attributes": {
        "color": "Silver",
        "storage": "512GB SSD"
      }
    }
  ]
}
```

**Assertions:**
- Status code is 200
- SKU list contains variants
- Each SKU has price and stock info

---

### TC_SKU_02: Seller Update SKU Stock

| Field | Value |
|-------|-------|
| **Test ID** | TC_SKU_02 |
| **Test Name** | Seller Update SKU Stock |
| **Endpoint** | `PUT /skus/{skuId}/stock` |
| **Method** | PUT |
| **Authentication** | Required (Seller Token) |

**Test Intent:**  
Verify seller can update inventory levels for product variants.

**Input:**
- **URL Parameter:** `skuId = 501`
- **Request Body:**
```json
{
  "quantity": 75,
  "operation": "set"
}
```

**Expected Output:**
- **Status Code:** 200 OK or 204 No Content
- **Response Body (if 200):**
```json
{
  "success": true,
  "data": {
    "skuId": "501",
    "stock": 75,
    "updatedAt": "2024-05-04T11:15:00Z"
  }
}
```

**Assertions:**
- Status code is 200 or 204
- Stock quantity is updated
- Change is reflected in inventory

---

## 7. CART TESTS

### TC_CART_01: Get Cart (Guest Session)

| Field | Value |
|-------|-------|
| **Test ID** | TC_CART_01 |
| **Test Name** | Guest Get Cart |
| **Endpoint** | `GET /cart` |
| **Method** | GET |
| **Authentication** | Session Header Required |

**Test Intent:**  
Verify guest users can retrieve their shopping cart using session ID.

**Input (Headers):**
```json
{
  "X-Session-Id": "uuid-1234-5678-9012",
  "Content-Type": "application/json"
}
```

**Expected Output:**
- **Status Code:** 200 OK
- **Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [],
    "subtotal": 0,
    "tax": 0,
    "total": 0
  }
}
```

**Assertions:**
- Status code is 200
- Items array exists (may be empty)
- Total and subtotal are calculable

---

### TC_CART_02: Add Item to Cart

| Field | Value |
|-------|-------|
| **Test ID** | TC_CART_02 |
| **Test Name** | Add Item to Cart |
| **Endpoint** | `POST /cart/items` |
| **Method** | POST |
| **Authentication** | Session/User Token Required |

**Test Intent:**  
Verify products can be added to cart with quantity control.

**Input (Request Body):**
```json
{
  "skuId": 501,
  "quantity": 2
}
```
**Headers:**
```json
{
  "X-Session-Id": "uuid-1234-5678-9012"
}
```

**Expected Output:**
- **Status Code:** 200 OK or 201 Created
- **Response Body:**
```json
{
  "success": true,
  "data": {
    "cartItemId": "cart-item-1",
    "skuId": 501,
    "productName": "Premium Laptop",
    "quantity": 2,
    "unitPrice": 999.99,
    "totalPrice": 1999.98
  }
}
```

**Assertions:**
- Status code is 200 or 201
- Item is added to cart
- Quantity matches input

---

### TC_CART_03: Update Cart Item Quantity

| Field | Value |
|-------|-------|
| **Test ID** | TC_CART_03 |
| **Test Name** | Update Cart Item |
| **Endpoint** | `PUT /cart/items/{cartItemId}` |
| **Method** | PUT |
| **Authentication** | Session/User Token Required |

**Test Intent:**  
Verify cart items can have quantities adjusted.

**Input:**
- **URL Parameter:** `cartItemId = cart-item-1`
- **Request Body:**
```json
{
  "quantity": 5
}
```

**Expected Output:**
- **Status Code:** 200 OK or 204 No Content
- **Response Body (if 200):**
```json
{
  "success": true,
  "data": {
    "cartItemId": "cart-item-1",
    "quantity": 5,
    "totalPrice": 4999.95
  }
}
```

**Assertions:**
- Status code is 200 or 204
- Quantity is updated
- Total price recalculated

---

### TC_CART_04: Delete Cart Item

| Field | Value |
|-------|-------|
| **Test ID** | TC_CART_04 |
| **Test Name** | Delete Item from Cart |
| **Endpoint** | `DELETE /cart/items/{cartItemId}` |
| **Method** | DELETE |
| **Authentication** | Session/User Token Required |

**Test Intent:**  
Verify items can be removed from cart.

**Input:**
- **URL Parameter:** `cartItemId = cart-item-1`

**Expected Output:**
- **Status Code:** 200 OK or 204 No Content
- **Response Body (if 200):**
```json
{
  "success": true,
  "message": "Item removed from cart"
}
```

**Assertions:**
- Status code is 200 or 204
- Item is removed from cart
- Item count decreases

---

### TC_CART_05: Clear Cart

| Field | Value |
|-------|-------|
| **Test ID** | TC_CART_05 |
| **Test Name** | Clear All Cart Items |
| **Endpoint** | `DELETE /cart` |
| **Method** | DELETE |
| **Authentication** | Session/User Token Required |

**Test Intent:**  
Verify users can empty their entire shopping cart.

**Input:**
- **Headers:**
```json
{
  "X-Session-Id": "uuid-1234-5678-9012"
}
```

**Expected Output:**
- **Status Code:** 200 OK or 204 No Content
- **Response Body (if 200):**
```json
{
  "success": true,
  "message": "Cart cleared successfully",
  "data": {
    "items": [],
    "total": 0
  }
}
```

**Assertions:**
- Status code is 200 or 204
- Cart becomes empty
- Total is zero

---

### TC_CART_06: Merge Cart on Login

| Field | Value |
|-------|-------|
| **Test ID** | TC_CART_06 |
| **Test Name** | Merge Guest Cart with User Cart |
| **Endpoint** | `POST /cart/merge` |
| **Method** | POST |
| **Authentication** | User Token + Session ID |

**Test Intent:**  
Verify guest cart items are merged into user account cart upon login.

**Input:**
- **Headers:**
```json
{
  "Authorization": "Bearer <user_token>",
  "X-Session-Id": "uuid-guest-session"
}
```

**Expected Output:**
- **Status Code:** 200 OK
- **Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "skuId": 501,
        "quantity": 2
      }
    ],
    "mergedItemCount": 1,
    "duplicatesHandled": "Quantities combined"
  }
}
```

**Assertions:**
- Status code is 200
- Guest items are preserved
- User's existing cart items are retained

---

## 8. ORDER TESTS

### TC_ORDER_01: Create Order with New Address

| Field | Value |
|-------|-------|
| **Test ID** | TC_ORDER_01 |
| **Test Name** | Create Order with New Shipping Address |
| **Endpoint** | `POST /orders` |
| **Method** | POST |
| **Authentication** | Required (User Token + Session ID) |

**Test Intent:**  
Verify users can create orders with inline address creation.

**Input (Request Body):**
```json
{
  "newShippingAddress": {
    "type": 0,
    "label": "Delivery Address",
    "recipientName": "Customer Name",
    "phone": "0901234567",
    "addressLine1": "123 Main Street",
    "city": "Ho Chi Minh City",
    "stateProvince": "District 1",
    "postalCode": "700000",
    "country": "VN"
  },
  "saveNewShippingAddress": true,
  "billingAddressId": 15,
  "couponCode": "SALE10",
  "customerNotes": "Deliver after 5 PM"
}
```
**Headers:**
```json
{
  "Authorization": "Bearer <user_token>",
  "X-Session-Id": "uuid-1234"
}
```

**Expected Output:**
- **Status Code:** 200 OK or 201 Created
- **Response Body:**
```json
{
  "success": true,
  "data": {
    "orderId": "ORD-001234",
    "orderNumber": "ORD-001234",
    "status": "Pending",
    "total": 899.99,
    "shippingAddress": {
      "addressId": "16",
      "label": "Delivery Address"
    },
    "createdAt": "2024-05-04T12:00:00Z"
  }
}
```

**Assertions:**
- Status code is 200 or 201
- Order ID is generated
- Status is "Pending"

---

### TC_ORDER_02: Create Order with Existing Address

| Field | Value |
|-------|-------|
| **Test ID** | TC_ORDER_02 |
| **Test Name** | Create Order with Saved Address |
| **Endpoint** | `POST /orders` |
| **Method** | POST |
| **Authentication** | Required (User Token + Session ID) |

**Test Intent:**  
Verify users can create orders using previously saved addresses.

**Input (Request Body):**
```json
{
  "shippingAddressId": 12,
  "billingAddressId": 15,
  "couponCode": "SALE10",
  "customerNotes": "Call before delivery"
}
```

**Expected Output:**
- **Status Code:** 200 OK or 201 Created
- **Response Body:**
```json
{
  "success": true,
  "data": {
    "orderId": "ORD-001235",
    "status": "Pending",
    "shippingAddressId": 12,
    "total": 899.99
  }
}
```

**Assertions:**
- Status code is 200 or 201
- Correct address ID is used
- Order is created successfully

---

### TC_ORDER_03: Get Order Details

| Field | Value |
|-------|-------|
| **Test ID** | TC_ORDER_03 |
| **Test Name** | Get Order Details |
| **Endpoint** | `GET /orders/{orderId}` |
| **Method** | GET |
| **Authentication** | Required (User/Admin Token) |

**Test Intent:**  
Verify users can retrieve their order details and tracking information.

**Input:**
- **URL Parameter:** `orderId = ORD-001234`

**Expected Output:**
- **Status Code:** 200 OK
- **Response Body:**
```json
{
  "success": true,
  "data": {
    "orderId": "ORD-001234",
    "orderNumber": "ORD-001234",
    "status": "Shipping",
    "subtotal": 899.99,
    "tax": 50.00,
    "shippingCost": 0,
    "discount": 90.00,
    "total": 859.99,
    "items": [
      {
        "productName": "Premium Laptop",
        "quantity": 1,
        "unitPrice": 999.99
      }
    ],
    "shippingAddress": {...},
    "tracking": {
      "status": "In Transit",
      "carrier": "GHN",
      "trackingNumber": "GHN123456"
    }
  }
}
```

**Assertions:**
- Status code is 200
- Order details are complete
- Tracking information is available

---

### TC_ORDER_04: Cancel Order

| Field | Value |
|-------|-------|
| **Test ID** | TC_ORDER_04 |
| **Test Name** | Cancel Order |
| **Endpoint** | `PUT /orders/{orderId}/cancel` |
| **Method** | PUT |
| **Authentication** | Required (User Token) |

**Test Intent:**  
Verify users can cancel pending orders.

**Input:**
- **URL Parameter:** `orderId = ORD-001234`
- **Request Body:**
```json
{
  "reason": "Changed my mind",
  "notes": "Cancel and refund immediately"
}
```

**Expected Output:**
- **Status Code:** 200 OK
- **Response Body:**
```json
{
  "success": true,
  "data": {
    "orderId": "ORD-001234",
    "status": "Cancelled",
    "cancelledAt": "2024-05-04T13:00:00Z",
    "refundStatus": "Initiated",
    "refundAmount": 859.99
  }
}
```

**Assertions:**
- Status code is 200
- Order status changes to "Cancelled"
- Refund is initiated

---

## 9. COUPON TESTS

### TC_COUPON_01: Validate Coupon Code

| Field | Value |
|-------|-------|
| **Test ID** | TC_COUPON_01 |
| **Test Name** | Validate Coupon Code |
| **Endpoint** | `POST /coupons/validate` |
| **Method** | POST |
| **Authentication** | None (Public) |

**Test Intent:**  
Verify users can check if coupon codes are valid and get discount information.

**Input (Request Body):**
```json
{
  "code": "SALE10",
  "cartTotal": 899.99
}
```

**Expected Output:**
- **Status Code:** 200 OK
- **Response Body:**
```json
{
  "success": true,
  "data": {
    "couponCode": "SALE10",
    "discountType": "Percentage",
    "discountValue": 10,
    "discountAmount": 90.00,
    "finalTotal": 809.99,
    "expiresAt": "2024-12-31",
    "minOrderValue": 100
  }
}
```

**Assertions:**
- Status code is 200
- Discount is calculated correctly
- Expiry date is valid

---

### TC_COUPON_02: Apply Coupon to Order

| Field | Value |
|-------|-------|
| **Test ID** | TC_COUPON_02 |
| **Test Name** | Apply Coupon to Order |
| **Endpoint** | `POST /coupons/apply` |
| **Method** | POST |
| **Authentication** | Required (User Token) |

**Test Intent:**  
Verify users can apply validated coupons to their orders.

**Input (Request Body):**
```json
{
  "couponCode": "SALE10",
  "orderId": "ORD-001234"
}
```

**Expected Output:**
- **Status Code:** 200 OK
- **Response Body:**
```json
{
  "success": true,
  "data": {
    "orderId": "ORD-001234",
    "couponApplied": "SALE10",
    "originalTotal": 899.99,
    "discountAmount": 90.00,
    "newTotal": 809.99
  }
}
```

**Assertions:**
- Status code is 200
- Discount is applied to order
- New total is calculated

---

### TC_COUPON_03: Invalid Coupon Rejection

| Field | Value |
|-------|-------|
| **Test ID** | TC_COUPON_03 |
| **Test Name** | Reject Invalid Coupon |
| **Endpoint** | `POST /coupons/validate` |
| **Method** | POST |
| **Authentication** | None (Public) |

**Test Intent:**  
Ensure system rejects invalid, expired, or insufficient-value coupons.

**Input (Request Body):**
```json
{
  "code": "INVALID999",
  "cartTotal": 50.00
}
```

**Expected Output:**
- **Status Code:** 400 Bad Request or 404 Not Found
- **Response Body:**
```json
{
  "success": false,
  "error": "Coupon not found or expired",
  "code": "INVALID_COUPON"
}
```

**Assertions:**
- Status code is 400 or 404
- Error message is clear
- No discount is applied

---

## 10. PAYMENT TESTS

### TC_PAY_01: Process ZaloPay Payment

| Field | Value |
|-------|-------|
| **Test ID** | TC_PAY_01 |
| **Test Name** | Create ZaloPay Payment Request |
| **Endpoint** | `POST /payments/zalopay/create-order` |
| **Method** | POST |
| **Authentication** | Required (User Token) |

**Test Intent:**  
Verify payment gateway integration for ZaloPay returns valid payment URL.

**Input (Request Body):**
```json
{
  "orderId": "ORD-001234",
  "amount": 809.99,
  "description": "Payment for order ORD-001234"
}
```

**Expected Output:**
- **Status Code:** 200 OK
- **Response Body:**
```json
{
  "success": true,
  "data": {
    "paymentUrl": "https://zalopay.vn/...",
    "appTransId": "240504_...",
    "zaloPayTransId": "...",
    "amount": 809.99,
    "expiresAt": "2024-05-04T12:30:00Z"
  }
}
```

**Assertions:**
- Status code is 200
- Payment URL is valid and non-empty
- Transaction ID is generated

---

### TC_PAY_02: Payment Webhook Callback

| Field | Value |
|-------|-------|
| **Test ID** | TC_PAY_02 |
| **Test Name** | Handle Payment Success Webhook |
| **Endpoint** | `POST /payments/zalopay/callback` |
| **Method** | POST |
| **Authentication** | None (Webhook) |

**Test Intent:**  
Verify system correctly processes successful payment notifications from gateway.

**Input (Request Body - Webhook Signature):**
```json
{
  "appid": "2553",
  "apptransid": "240504_...",
  "transid": "...",
  "status": 1,
  "amount": 809.99,
  "discountamount": 0,
  "failreason": "",
  "timestamp": "1714829400000",
  "mac": "signature_hash"
}
```

**Expected Output:**
- **Status Code:** 200 OK
- **Response Body:**
```json
{
  "success": true,
  "returncode": 1,
  "returnmessage": "success"
}
```

**Assertions:**
- Status code is 200
- Return code indicates success
- Order status updates to "Paid"

---

### TC_PAY_03: Payment Failure Handling

| Field | Value |
|-------|-------|
| **Test ID** | TC_PAY_03 |
| **Test Name** | Handle Payment Failure |
| **Endpoint** | `POST /payments/zalopay/callback` |
| **Method** | POST |
| **Authentication** | None (Webhook) |

**Test Intent:**  
Verify system handles failed payments gracefully.

**Input (Request Body - Failed Payment):**
```json
{
  "appid": "2553",
  "apptransid": "240504_...",
  "status": 2,
  "failreason": "user_cancel",
  "timestamp": "1714829400000"
}
```

**Expected Output:**
- **Status Code:** 200 OK
- **Response Body:**
```json
{
  "success": true,
  "returncode": 1,
  "returnmessage": "failure_recorded"
}
```

**Assertions:**
- Status code is 200
- Failure is logged
- Order status remains "Pending Payment"
- User can retry

---

## 🔄 TEST EXECUTION FLOW

### Prerequisites Checklist:
- [ ] Backend API running on `http://localhost:8080/api`
- [ ] PostgreSQL database initialized with test data
- [ ] RabbitMQ running for message queues
- [ ] `.env` file configured with test credentials
- [ ] Python 3.8+ with pytest installed

### Running Tests:

**Install dependencies:**
```bash
cd testing
pip install -r requirements.txt
```

**Run all tests:**
```bash
pytest -v
```

**Run specific test category:**
```bash
pytest -v test_auth/
pytest -v test_products/
pytest -v test_orders/
```

**Run with coverage report:**
```bash
pytest --cov=. --cov-report=html
```

**Run in parallel:**
```bash
pytest -n auto
```

---

## 📊 EXPECTED RESULTS SUMMARY

| Category | Test Count | Pass Criteria |
|----------|-----------|---------------|
| Authentication | 4 | All 200/201/400/401 codes correct |
| Users & Profile | 4 | Role-based access enforced |
| Addresses | 3 | Full CRUD operations working |
| Categories | 3 | Public listing & admin control |
| Products | 3 | Seller creation & admin approval |
| SKUs | 2 | Variant management working |
| Cart | 6 | Session & merge logic correct |
| Orders | 4 | Order creation & tracking functional |
| Coupons | 3 | Validation & application working |
| Payments | 3 | Gateway integration & webhooks verified |
| **TOTAL** | **35** | **100% Pass Rate Target** |

---

## 🐛 COMMON ISSUES & TROUBLESHOOTING

| Issue | Cause | Solution |
|-------|-------|----------|
| 404 Not Found | Endpoint path incorrect | Check API route definitions |
| 401 Unauthorized | Missing/Invalid token | Regenerate token via login |
| 403 Forbidden | Insufficient permissions | Use correct role (Admin/Seller/User) |
| Cart empty errors | Missing session or products | Create products first, use valid session ID |
| Payment failures | ZaloPay config | Verify API keys and callback URL in .env |
| Duplicate email | User exists | Use unique email or cleanup test data |

---

**Document Version:** 1.0  
**Last Updated:** May 4, 2026  
**Maintained By:** QA Team
