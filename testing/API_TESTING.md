# 🧪 API TESTING DOCUMENTATION

**Project:** E-Commerce Platform  
**Test Framework:** Pytest + Requests  
**Base URL:** `http://localhost:8080/api`

---

## 📋 TABLE OF CONTENTS

1. [Authentication Tests](#1-authentication-tests)
2. [User & Profile Tests](#2-user--profile-tests)
3. [Address Tests](#3-address-tests)
4. [Category Tests](#4-category-tests)
5. [Product Tests](#5-product-tests)
6. [SKU Tests](#6-sku-tests)
7. [Cart Tests](#7-cart-tests)
8. [Order Tests](#8-order-tests)
9. [Coupon Tests](#9-coupon-tests)
10. [Payment Tests](#10-payment-tests)

---

## 1. AUTHENTICATION TESTS

### TC_AUTH_01: User Registration Success

| Field | Value |
|-------|-------|
| **Test ID** | TC_AUTH_01 |
| **Test Name** | User Registration with Valid Data |
| **Endpoint** | `POST /auth/register` |
| **Method** | POST |
| **Authentication** | None (Public) |

**Test Intent:**  
Verify that a new user can successfully register with valid credentials.

**Input (Request Body):**
```json
{
  "email": "valid_user_<random>@gmail.com",
  "username": "validuser_<random>",
  "password": "Test123@Password!",
  "confirmPassword": "Test123@Password!",
  "firstName": "Valid",
  "lastName": "User",
  "phone": "0901234567",
  "acceptTerms": true
}
```

**Expected Output:**
- **Status Code:** 200 OK or 201 Created
- **Response Body:**
```json
{
  "success": true,
  "data": {
    "userId": "...",
    "email": "valid_user_<random>@gmail.com",
    "username": "validuser_<random>",
    "message": "User registered successfully"
  }
}
```

**Assertions:**
- Status code is 200 or 201
- User can login with provided credentials

---

### TC_AUTH_02: Duplicate Email Prevention

| Field | Value |
|-------|-------|
| **Test ID** | TC_AUTH_02 |
| **Test Name** | Registration Duplicate Email Rejection |
| **Endpoint** | `POST /auth/register` |
| **Method** | POST |
| **Authentication** | None |

**Test Intent:**  
Ensure system prevents registration with duplicate email addresses.

**Input (Request Body):**
```json
{
  "email": "duplicate_test@gmail.com",
  "username": "user_<random>",
  "password": "Test123@",
  "confirmPassword": "Test123@",
  "firstName": "First",
  "lastName": "User",
  "phone": "0901234567",
  "acceptTerms": true
}
```
*(Same email used twice)*

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
