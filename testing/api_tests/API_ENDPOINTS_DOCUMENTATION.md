# E-Commerce API Endpoints Documentation

This document contains all extracted API endpoints from the Bruno API documentation for the e-commerce system.

---

## 1. AUTH ENDPOINTS

### 1.1 User Login
- **HTTP Method:** `POST`
- **Path:** `/api/auth/login`
- **Auth Required:** No
- **Required Parameters (Body):**
  - `identifier` (string) - Username or email
  - `password` (string) - User password
- **Key Response Fields:** 
  - `accessToken` (JWT)
  - `refreshToken`
  - `userId`
  - `username`
  - `email`
  - `roles` (array)

### 1.2 Admin Login
- **HTTP Method:** `POST`
- **Path:** `/api/auth/login`
- **Auth Required:** No
- **Required Parameters (Body):**
  - `identifier` (string) - Admin username/email
  - `password` (string) - Admin password
- **Key Response Fields:**
  - `accessToken` (JWT)
  - `refreshToken`
  - `userId`
  - `username`
  - `email`
  - `roles` (array - admin role)

### 1.3 Seller Login
- **HTTP Method:** `POST`
- **Path:** `/api/auth/login`
- **Auth Required:** No
- **Required Parameters (Body):**
  - `identifier` (string) - Seller email/username
  - `password` (string) - Seller password
- **Key Response Fields:**
  - `accessToken` (JWT)
  - `refreshToken`
  - `userId`
  - `email`
  - `roles` (array - seller role)

### 1.4 User Registration
- **HTTP Method:** `POST`
- **Path:** `/api/auth/register`
- **Auth Required:** No
- **Required Parameters (Body):**
  - `email` (string)
  - `username` (string)
  - `password` (string)
  - `confirmPassword` (string)
  - `firstName` (string)
  - `lastName` (string)
  - `phone` (string)
  - `acceptTerms` (boolean)
- **Key Response Fields:**
  - `userId`
  - `email`
  - `username`
  - `firstName`
  - `lastName`
  - `message` (success)

---

## 2. CATEGORY ENDPOINTS

### 2.1 Get All Categories
- **HTTP Method:** `GET`
- **Path:** `/api/categories`
- **Auth Required:** No
- **Required Parameters:** None
- **Optional Query Parameters:** None documented
- **Key Response Fields:**
  - `id`
  - `name`
  - `slug`
  - `description`
  - `imageUrl`
  - `displayOrder`
  - `isCore`
  - `isActive`
  - `parentCategoryId`
  - `children` (array)

### 2.2 Get Core Categories
- **HTTP Method:** `GET`
- **Path:** `/api/categories/core`
- **Auth Required:** No
- **Required Parameters:** None
- **Key Response Fields:**
  - `id`
  - `name`
  - `slug`
  - `imageUrl`
  - `displayOrder`
  - `isCore` (true)

### 2.3 Get Category by ID
- **HTTP Method:** `GET`
- **Path:** `/api/categories/{id}`
- **Auth Required:** No
- **Required Parameters (Path):**
  - `id` (integer) - Category ID
- **Key Response Fields:**
  - `id`
  - `name`
  - `slug`
  - `description`
  - `imageUrl`
  - `displayOrder`
  - `isCore`
  - `isActive`
  - `parentCategoryId`
  - `children` (array)

### 2.4 Get Category by Slug
- **HTTP Method:** `GET`
- **Path:** `/api/categories/slug/{slug}`
- **Auth Required:** No
- **Required Parameters (Path):**
  - `slug` (string) - Category slug
- **Key Response Fields:**
  - `id`
  - `name`
  - `slug`
  - `description`
  - `imageUrl`
  - `displayOrder`
  - `isCore`
  - `isActive`
  - `parentCategoryId`

### 2.5 Get Child Categories
- **HTTP Method:** `GET`
- **Path:** `/api/categories/{id}/children`
- **Auth Required:** No
- **Required Parameters (Path):**
  - `id` (integer) - Parent category ID
- **Key Response Fields:**
  - `id`
  - `name`
  - `slug`
  - `description`
  - `displayOrder`
  - `isActive`

### 2.6 Create Category
- **HTTP Method:** `POST`
- **Path:** `/api/categories`
- **Auth Required:** Yes (Bearer Token - Admin)
- **Required Parameters (Body):**
  - `name` (string)
  - `description` (string)
  - `imageUrl` (string)
  - `displayOrder` (integer)
  - `isCore` (boolean)
  - `isActive` (boolean)
- **Optional Parameters:**
  - `parentCategoryId` (integer)
- **Key Response Fields:**
  - `id`
  - `name`
  - `slug`
  - `description`
  - `imageUrl`
  - `displayOrder`
  - `isCore`
  - `isActive`
  - `parentCategoryId`

### 2.7 Update Category
- **HTTP Method:** `PUT`
- **Path:** `/api/categories/{id}`
- **Auth Required:** Yes (Bearer Token - Admin)
- **Required Parameters (Path):**
  - `id` (integer) - Category ID
- **Body Parameters:**
  - `name` (string)
  - `parentCategoryId` (integer)
  - `description` (string)
  - `imageUrl` (string)
  - `displayOrder` (integer)
  - `isCore` (boolean)
  - `isActive` (boolean)
- **Key Response Fields:**
  - `id`
  - `name`
  - `slug`
  - Updated fields

### 2.8 Delete Category
- **HTTP Method:** `DELETE`
- **Path:** `/api/categories/{id}`
- **Auth Required:** Yes (Bearer Token - Admin)
- **Required Parameters (Path):**
  - `id` (integer) - Category ID
- **Key Response Fields:**
  - `message` (success/error)
  - `success` (boolean)

---

## 3. PRODUCT ENDPOINTS

### 3.1 Get All Products
- **HTTP Method:** `GET`
- **Path:** `/api/products`
- **Auth Required:** No
- **Required Parameters:** None
- **Key Response Fields:**
  - `id`
  - `name`
  - `slug`
  - `description`
  - `brand`
  - `categoryIds` (array)
  - `weightKg`
  - `dimensionsCm`
  - `skus` (array)
  - `status`

### 3.2 Get Products with Pagination
- **HTTP Method:** `GET`
- **Path:** `/api/products`
- **Auth Required:** No
- **Query Parameters:**
  - `pageNumber` (integer) - Page number (1-indexed)
  - `pageSize` (integer) - Items per page
  - Additional filter parameters may be supported
- **Key Response Fields:**
  - `items` (array of products)
  - `pageNumber`
  - `pageSize`
  - `totalCount`
  - `totalPages`

### 3.3 Get Product by ID
- **HTTP Method:** `GET`
- **Path:** `/api/products/{id}`
- **Auth Required:** No
- **Required Parameters (Path):**
  - `id` (integer) - Product ID
- **Key Response Fields:**
  - `id`
  - `name`
  - `slug`
  - `description`
  - `brand`
  - `categoryIds` (array)
  - `weightKg`
  - `dimensionsCm`
  - `status`
  - `skus` (array with variant details)
  - `seller` (seller info)
  - `createdDate`
  - `lastModifiedDate`

### 3.4 Get Current Seller Products (Seller)
- **HTTP Method:** `GET`
- **Path:** `/api/products/seller`
- **Auth Required:** Yes (Bearer Token - Seller)
- **Required Parameters:** None
- **Key Response Fields:**
  - `id`
  - `name`
  - `slug`
  - `description`
  - `status`
  - `brand`
  - `skus` (seller's SKUs only)
  - `createdDate`
  - `lastModifiedDate`

### 3.5 Create Product (Seller)
- **HTTP Method:** `POST`
- **Path:** `/api/products/seller`
- **Auth Required:** Yes (Bearer Token - Seller)
- **Required Parameters (Body):**
  - `name` (string)
  - `description` (string)
  - `categoryIds` (array of integers)
  - `brand` (string)
  - `weightKg` (decimal)
  - `defaultSkuPrice` (decimal)
  - `defaultSkuStock` (integer)
  - `dimensionsCm` (string)
- **Key Response Fields:**
  - `id`
  - `name`
  - `slug`
  - `sellerId`
  - `status`
  - `message` (success)

### 3.6 Update Product (Admin)
- **HTTP Method:** `PUT`
- **Path:** `/api/products/{id}`
- **Auth Required:** Yes (Bearer Token - Admin)
- **Required Parameters (Path):**
  - `id` (integer) - Product ID
- **Optional Body Parameters:**
  - `brand` (string)
  - `weightKg` (decimal)
  - `dimensionsCm` (string)
  - Other updatable fields
- **Key Response Fields:**
  - `id`
  - `name`
  - Updated fields

### 3.7 Update Product (Seller)
- **HTTP Method:** `PUT`
- **Path:** `/api/products/seller/{id}`
- **Auth Required:** Yes (Bearer Token - Seller)
- **Required Parameters (Path):**
  - `id` (integer) - Product ID (owned by seller)
- **Body Parameters:**
  - `name` (string)
  - `slug` (string)
  - `description` (string)
  - `categoryIds` (array)
  - `brand` (string)
  - `weightKg` (decimal)
  - `dimensionsCm` (string)
  - `status` (active/inactive)
- **Key Response Fields:**
  - `id`
  - `name`
  - `status`
  - Updated fields

### 3.8 Delete Product (Admin)
- **HTTP Method:** `DELETE`
- **Path:** `/api/products/{id}`
- **Auth Required:** Yes (Bearer Token - Admin)
- **Required Parameters (Path):**
  - `id` (integer) - Product ID
- **Key Response Fields:**
  - `message` (success/error)
  - `success` (boolean)

### 3.9 Delete Product (Seller)
- **HTTP Method:** `DELETE`
- **Path:** `/api/products/seller/{id}`
- **Auth Required:** Yes (Bearer Token - Seller)
- **Required Parameters (Path):**
  - `id` (integer) - Product ID (owned by seller)
- **Key Response Fields:**
  - `message` (success/error)
  - `success` (boolean)

---

## 4. PRODUCT SKU ENDPOINTS

### 4.1 Get SKU by ID
- **HTTP Method:** `GET`
- **Path:** `/api/skus/{id}`
- **Auth Required:** No
- **Required Parameters (Path):**
  - `id` (integer) - SKU ID
- **Key Response Fields:**
  - `id`
  - `productId`
  - `variantAttributes` (JSON string)
  - `price` (decimal)
  - `costPrice` (decimal)
  - `compareAtPrice` (decimal)
  - `stock` (integer)
  - `weightKg` (decimal)
  - `dimensionsCm` (string)
  - `isActive` (boolean)
  - `isDefault` (boolean)
  - `images` (array)
  - `createdDate`
  - `lastModifiedDate`

### 4.2 Get SKUs by Product
- **HTTP Method:** `GET`
- **Path:** `/api/products/{productId}/skus`
- **Auth Required:** No
- **Required Parameters (Path):**
  - `productId` (integer) - Product ID
- **Key Response Fields:**
  - `id`
  - `productId`
  - `variantAttributes`
  - `price`
  - `stock`
  - `isActive`
  - `isDefault`
  - `images` (array)

### 4.3 Get Current Seller SKUs (Seller)
- **HTTP Method:** `GET`
- **Path:** `/api/skus/seller`
- **Auth Required:** Yes (Bearer Token - Seller)
- **Required Parameters:** None
- **Key Response Fields:**
  - `id`
  - `productId`
  - `productName`
  - `variantAttributes`
  - `price`
  - `costPrice`
  - `stock`
  - `isActive`
  - `isDefault`
  - `images` (array)

### 4.4 Create SKU (Seller)
- **HTTP Method:** `POST`
- **Path:** `/api/skus/seller`
- **Auth Required:** Yes (Bearer Token - Seller)
- **Required Parameters (Body):**
  - `productId` (integer)
  - `variantAttributes` (string - JSON)
  - `price` (decimal)
  - `costPrice` (decimal)
  - `stock` (integer)
  - `weightKg` (decimal)
  - `dimensionsCm` (string)
- **Optional Parameters:**
  - `compareAtPrice` (decimal)
  - `images` (array of image objects)
- **Image Object Properties:**
  - `imageUrl` (string - required)
  - `thumbnailUrl` (string)
  - `altText` (string)
  - `displayOrder` (integer)
- **Key Response Fields:**
  - `id`
  - `productId`
  - `variantAttributes`
  - `price`
  - `stock`
  - `message` (success)

### 4.5 Update SKU (Admin)
- **HTTP Method:** `PUT`
- **Path:** `/api/skus/{id}`
- **Auth Required:** Yes (Bearer Token - Admin)
- **Required Parameters (Path):**
  - `id` (integer) - SKU ID
- **Body Parameters:**
  - `images` (array) - Complete image list
- **Image Object Properties:**
  - `id` (integer - for existing images)
  - `imageUrl` (string)
  - `thumbnailUrl` (string)
  - `altText` (string)
  - `isPrimary` (boolean)
  - `displayOrder` (integer)
- **Key Response Fields:**
  - `id`
  - `images` (updated array)

### 4.6 Update SKU (Seller)
- **HTTP Method:** `PUT`
- **Path:** `/api/skus/seller/{id}`
- **Auth Required:** Yes (Bearer Token - Seller)
- **Required Parameters (Path):**
  - `id` (integer) - SKU ID (owned by seller)
- **Body Parameters:**
  - `variantAttributes` (string - JSON)
  - `price` (decimal)
  - `costPrice` (decimal)
  - `compareAtPrice` (decimal)
  - `stock` (integer)
  - `isActive` (boolean)
  - `isDefault` (boolean)
  - `weightKg` (decimal)
  - `dimensionsCm` (string)
  - `images` (array)
- **Image Management:**
  - `id: null` - Add new image
  - `id: <number>` with `isDeleted: true` - Remove image
  - `id: <number>` with data - Update existing image
- **Key Response Fields:**
  - `id`
  - `price`
  - `stock`
  - `images` (updated)

### 4.7 Delete SKU (Admin)
- **HTTP Method:** `DELETE`
- **Path:** `/api/skus/{id}`
- **Auth Required:** Yes (Bearer Token - Admin)
- **Required Parameters (Path):**
  - `id` (integer) - SKU ID
- **Key Response Fields:**
  - `message` (success/error)
  - `success` (boolean)

### 4.8 Delete SKU (Seller)
- **HTTP Method:** `DELETE`
- **Path:** `/api/skus/seller/{id}`
- **Auth Required:** Yes (Bearer Token - Seller)
- **Required Parameters (Path):**
  - `id` (integer) - SKU ID (owned by seller)
- **Key Response Fields:**
  - `message` (success/error)
  - `success` (boolean)

---

## 5. CART ENDPOINTS

### 5.1 Get Cart
- **HTTP Method:** `GET`
- **Path:** `/api/cart`
- **Auth Required:** Yes (Bearer Token - can be empty for guest with session ID)
- **Custom Headers:**
  - `X-Session-Id` (string) - For guest users
- **Required Parameters:** None
- **Key Response Fields:**
  - `id` (cart ID)
  - `userId` (null for guest)
  - `sessionId` (for guest carts)
  - `items` (array of cart items)
  - `totalPrice` (decimal)
  - `totalQuantity` (integer)
  - `lastModifiedDate`

### 5.2 Add Item to Cart
- **HTTP Method:** `POST`
- **Path:** `/api/cart/items`
- **Auth Required:** Yes (Bearer Token - can be empty for guest)
- **Custom Headers:**
  - `X-Session-Id` (string) - For guest users
- **Required Parameters (Body):**
  - `skuId` (integer) - SKU ID
  - `quantity` (integer) - Default 1 if not specified
- **Key Response Fields:**
  - `id` (cart ID)
  - `items` (updated array)
  - `totalPrice`
  - `message` (success)

### 5.3 Update Cart Item Quantity
- **HTTP Method:** `PUT`
- **Path:** `/api/cart/items/{cartItemId}`
- **Auth Required:** Yes (Bearer Token)
- **Required Parameters (Path):**
  - `cartItemId` (integer) - Cart item ID
- **Required Parameters (Body):**
  - `quantity` (integer) - New quantity
- **Key Response Fields:**
  - `id` (cart item ID)
  - `quantity` (updated)
  - `subtotal` (decimal)
  - `totalPrice` (cart total)

### 5.4 Delete Cart Item
- **HTTP Method:** `DELETE`
- **Path:** `/api/cart/items/{cartItemId}`
- **Auth Required:** Yes (Bearer Token)
- **Required Parameters (Path):**
  - `cartItemId` (integer) - Cart item ID
- **Key Response Fields:**
  - `id` (cart ID)
  - `items` (updated array)
  - `totalPrice` (updated)
  - `message` (success)

### 5.5 Clear Cart
- **HTTP Method:** `DELETE`
- **Path:** `/api/cart`
- **Auth Required:** Yes (Bearer Token)
- **Required Parameters:** None
- **Key Response Fields:**
  - `message` (cart cleared successfully)
  - `id` (cart ID)

### 5.6 Merge Cart (Guest to User)
- **HTTP Method:** `POST`
- **Path:** `/api/cart/merge`
- **Auth Required:** Yes (Bearer Token - authenticated user)
- **Custom Headers:**
  - `X-Session-Id` (string) - Guest session ID to merge from
- **Required Parameters:** None
- **Key Response Fields:**
  - `id` (merged cart ID)
  - `items` (combined items)
  - `totalPrice` (merged total)
  - `message` (merge successful)

---

## 6. ORDER ENDPOINTS

### 6.1 Create Order
- **HTTP Method:** `POST`
- **Path:** `/api/orders`
- **Auth Required:** Yes (Bearer Token - can be empty for guest)
- **Custom Headers:**
  - `X-Session-Id` (string) - For guest orders
- **Required Parameters (Body) - Option 1 (Using Existing Address):**
  - `shippingAddressId` (integer)
  - `billingAddressId` (integer)
- **Required Parameters (Body) - Option 2 (Using New Address):**
  - `newShippingAddress` (object)
    - `type` (integer) - 0: house, etc.
    - `label` (string)
    - `recipientName` (string)
    - `phone` (string)
    - `addressLine1` (string)
    - `city` (string)
    - `stateProvince` (string)
    - `postalCode` (string)
    - `country` (string)
  - `saveNewShippingAddress` (boolean)
  - `billingAddressId` (integer)
- **Optional Parameters:**
  - `couponCode` (string)
  - `customerNotes` (string)
- **Key Response Fields:**
  - `id` (order ID)
  - `orderNumber` (string)
  - `userId` (if authenticated)
  - `shippingAddressId`
  - `billingAddressId`
  - `items` (array of order items)
  - `subtotal` (decimal)
  - `shippingCost` (decimal)
  - `tax` (decimal)
  - `discount` (decimal from coupon)
  - `totalAmount` (decimal)
  - `status` (pending, confirmed, etc.)
  - `createdDate`

---

## 7. ADDRESS ENDPOINTS

### 7.1 Get Current User Addresses
- **HTTP Method:** `GET`
- **Path:** `/api/addresses`
- **Auth Required:** Yes (Bearer Token)
- **Required Parameters:** None
- **Key Response Fields:**
  - `id`
  - `userId`
  - `type` (0: house, etc.)
  - `label` (string)
  - `recipientName`
  - `phone`
  - `addressLine1`
  - `addressLine2`
  - `city`
  - `stateProvince`
  - `postalCode`
  - `country`
  - `isDefaultShipping` (boolean)
  - `isDefaultBilling` (boolean)
  - `createdDate`
  - `lastModifiedDate`

### 7.2 Create Address
- **HTTP Method:** `POST`
- **Path:** `/api/addresses`
- **Auth Required:** Yes (Bearer Token)
- **Required Parameters (Body):**
  - `type` (integer) - Address type (0: house, etc.)
  - `label` (string) - e.g., "Home", "Work"
  - `recipientName` (string)
  - `phone` (string)
  - `addressLine1` (string)
  - `city` (string)
  - `stateProvince` (string)
  - `postalCode` (string)
  - `country` (string)
- **Optional Parameters:**
  - `addressLine2` (string)
  - `isDefaultShipping` (boolean)
  - `isDefaultBilling` (boolean)
- **Key Response Fields:**
  - `id`
  - `userId`
  - All input fields
  - `createdDate`

### 7.3 Update Address
- **HTTP Method:** `PUT`
- **Path:** `/api/addresses/{id}`
- **Auth Required:** Yes (Bearer Token)
- **Required Parameters (Path):**
  - `id` (integer) - Address ID
- **Body Parameters:**
  - `type` (integer)
  - `label` (string)
  - `recipientName` (string)
  - `phone` (string)
  - `addressLine1` (string)
  - `addressLine2` (string)
  - `city` (string)
  - `stateProvince` (string)
  - `postalCode` (string)
  - `country` (string)
  - `isDefaultShipping` (boolean)
  - `isDefaultBilling` (boolean)
- **Key Response Fields:**
  - `id`
  - All updated fields
  - `lastModifiedDate`

### 7.4 Delete Address
- **HTTP Method:** `DELETE`
- **Path:** `/api/addresses/{id}`
- **Auth Required:** Yes (Bearer Token)
- **Required Parameters (Path):**
  - `id` (integer) - Address ID
- **Key Response Fields:**
  - `message` (success/error)
  - `success` (boolean)

---

## 8. USER ENDPOINTS

### 8.1 Get Current User Profile
- **HTTP Method:** `GET`
- **Path:** `/api/users/profile`
- **Auth Required:** Yes (Bearer Token)
- **Required Parameters:** None
- **Key Response Fields:**
  - `id`
  - `email`
  - `username`
  - `firstName`
  - `lastName`
  - `phone`
  - `dateOfBirth`
  - `gender`
  - `avatarUrl`
  - `bio`
  - `preferredLanguage`
  - `preferredCurrency`
  - `roles` (array)
  - `createdDate`
  - `lastModifiedDate`

### 8.2 Update Current User Profile
- **HTTP Method:** `PUT`
- **Path:** `/api/users/profile`
- **Auth Required:** Yes (Bearer Token)
- **Body Parameters (Optional - Cannot update email/username):**
  - `dateOfBirth` (string - YYYY-MM-DD)
  - `gender` (string)
  - `avatarUrl` (string)
  - `bio` (string)
  - `preferredLanguage` (string - e.g., "en")
  - `preferredCurrency` (string - e.g., "eur")
  - `timezone` (string - e.g., "Vietnam/Ha_Noi")
- **Key Response Fields:**
  - `id`
  - All updated fields
  - `lastModifiedDate`

### 8.3 Get All Users (Admin)
- **HTTP Method:** `GET`
- **Path:** `/api/users`
- **Auth Required:** Yes (Bearer Token - Admin)
- **Optional Query Parameters:**
  - `pageNumber` (integer) - Page number
  - `pageSize` (integer) - Items per page
- **Key Response Fields:**
  - `items` (array of users)
  - `pageNumber`
  - `pageSize`
  - `totalCount`
  - `totalPages`

### 8.4 Get User by ID (Admin)
- **HTTP Method:** `GET`
- **Path:** `/api/users/{id}`
- **Auth Required:** Yes (Bearer Token - Admin)
- **Required Parameters (Path):**
  - `id` (integer) - User ID
- **Key Response Fields:**
  - `id`
  - `email`
  - `username`
  - `firstName`
  - `lastName`
  - `phone`
  - `dateOfBirth`
  - `gender`
  - `avatarUrl`
  - `bio`
  - `preferredLanguage`
  - `preferredCurrency`
  - `roles` (array)
  - `createdDate`
  - `lastModifiedDate`

### 8.5 Create User (Admin)
- **HTTP Method:** `POST`
- **Path:** `/api/users`
- **Auth Required:** Yes (Bearer Token - Admin)
- **Required Parameters (Body):**
  - `email` (string)
  - `username` (string)
  - `password` (string)
  - `firstName` (string)
  - `lastName` (string)
  - `phone` (string)
  - `roles` (array of integers) - Role IDs (e.g., [0, 1] for buyer and seller)
- **Key Response Fields:**
  - `id`
  - `email`
  - `username`
  - `firstName`
  - `lastName`
  - `roles`
  - `createdDate`

### 8.6 Update User (Admin)
- **HTTP Method:** `PUT`
- **Path:** `/api/users/{id}`
- **Auth Required:** Yes (Bearer Token - Admin)
- **Required Parameters (Path):**
  - `id` (integer) - User ID
- **Body Parameters:**
  - `email` (string)
  - `username` (string)
  - `password` (string)
  - `firstName` (string)
  - `lastName` (string)
  - `phone` (string)
  - `dateOfBirth` (string - YYYY-MM-DD)
  - `gender` (string)
  - `avatarUrl` (string)
  - `bio` (string)
  - `preferredLanguage` (string)
  - `preferredCurrency` (string)
  - `timezone` (string)
- **Key Response Fields:**
  - `id`
  - All updated fields
  - `lastModifiedDate`

### 8.7 Delete User (Admin)
- **HTTP Method:** `DELETE`
- **Path:** `/api/users/{id}`
- **Auth Required:** Yes (Bearer Token - Admin)
- **Required Parameters (Path):**
  - `id` (integer) - User ID
- **Key Response Fields:**
  - `message` (success/error)
  - `success` (boolean)

---

## COMMON PATTERNS

### Authentication
- **Bearer Token:** Most endpoints use JWT Bearer token authentication
- **Format:** `Authorization: Bearer <token>`
- **Guest Access:** Some endpoints (cart, products, categories) allow guest access without token
- **Session ID:** Guest users can be tracked with `X-Session-Id` header for cart/order operations

### Pagination
- **Query Parameters:**
  - `pageNumber` (1-indexed)
  - `pageSize` (items per page)
- **Response Structure:**
  - `items` (array of results)
  - `pageNumber` (current page)
  - `pageSize` (items per page)
  - `totalCount` (total items)
  - `totalPages` (total pages)

### Role-Based Endpoints
- **Admin:** Requires admin role (admin management, product admin operations, user management)
- **Seller:** Requires seller role (product/SKU creation and management by seller)
- **Buyer:** Standard authenticated user (cart, orders, addresses, profile)
- **Guest:** No authentication required for browsing, but needs session ID for cart operations

---

## TEST COVERAGE GAPS TO IDENTIFY

Use this checklist to identify gaps in your test coverage:

- [ ] **Auth:** Login for all three roles (user, admin, seller), registration, token validation
- [ ] **Categories:** CRUD operations, hierarchy (parent-child), core categories filter
- [ ] **Products:** Get with pagination (all 7 parameter combinations), CRUD by admin, seller-specific operations
- [ ] **SKUs:** Variant attributes, image management (add/update/delete), seller vs admin operations
- [ ] **Cart:** Guest vs authenticated carts, session ID handling, cart merge functionality
- [ ] **Orders:** Creation with new and existing addresses, coupon application, guest orders
- [ ] **Addresses:** CRUD, default shipping/billing designation, address in order context
- [ ] **Users:** Profile management, admin user management with pagination, role assignments
- [ ] **Error Cases:** Unauthorized access, validation errors, non-existent resources, business logic violations
- [ ] **Edge Cases:** Empty carts, bulk operations, concurrent requests, guest-to-user transitions

