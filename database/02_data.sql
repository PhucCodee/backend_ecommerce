-- ====================================
-- SAMPLE DATA FOR E-COMMERCE SYSTEM
-- ====================================
-- ====================================
-- 1. USERS (Buyers and Sellers)
-- ====================================
INSERT INTO
    users (email, username, status)
VALUES
    ('ronaldo@example.com', 'goat', 0),
    ('messi@example.com', 'messi', 0),
    ('lebron@example.com', 'king', 0),
    ('stephen@example.com', 'curry', 0),
    ('ye@example.com', 'west', 0);

-- ====================================
-- 2. USER CREDENTIALS
-- ====================================
INSERT INTO
    user_credentials (user_id, password_hash, password_salt)
VALUES
    (1, 'hash', 'salt'),
    (2, 'hash', 'salt'),
    (3, 'hash', 'salt'),
    (4, 'hash', 'salt'),
    (5, 'hash', 'salt');

-- ====================================
-- 3. USER PROFILES
-- ====================================
INSERT INTO
    user_profiles (
        user_id,
        first_name,
        last_name,
        phone,
        date_of_birth,
        gender,
        avatar_url,
        bio,
        language,
        timezone,
        currency
    )
VALUES
    (
        1,
        'Cristiano',
        'Ronaldo',
        '+351123456789',
        '1985-02-05',
        0,
        'https://example.com/avatars/ronaldo.jpg',
        'Football legend and entrepreneur. The GOAT.',
        0,
        'Europe/Lisbon',
        0
    ),
    (
        2,
        'Lionel',
        'Messi',
        '+5491123456789',
        '1987-06-24',
        0,
        'https://example.com/avatars/messi.jpg',
        '2nd GOAT. Loves mate and family.',
        0,
        'America/Argentina/Buenos_Aires',
        0
    ),
    (
        3,
        'LeBron',
        'James',
        '+12135551234',
        '1984-12-30',
        0,
        'https://example.com/avatars/lebron.jpg',
        'King of the court. Lakers.',
        0,
        'America/Los_Angeles',
        0
    ),
    (
        4,
        'Stephen',
        'Curry',
        '+19255551234',
        '1988-03-14',
        0,
        'https://example.com/avatars/stephen.jpg',
        'Chef Curry with the shot.',
        0,
        'America/Los_Angeles',
        0
    ),
    (
        5,
        'Kanye',
        'West',
        '+13105551234',
        '1977-06-08',
        0,
        'https://example.com/avatars/ye.jpg',
        'Producer, rapper, designer. Like children.',
        0,
        'America/Chicago',
        0
    );

-- ====================================
-- 4. USER ROLES
-- ====================================
INSERT INTO
    user_roles (user_id, role)
VALUES
    (1, 0),
    (2, 0),
    (3, 1),
    (3, 0), -- Seller can also buy
    (4, 1),
    (4, 0), -- Seller can also buy
    (5, 2);

-- ====================================
-- 5. CATEGORIES
-- ====================================
INSERT INTO
    categories (category_name, slug, description)
VALUES
    (
        'Electronics',
        'electronics',
        'Electronic devices'
    ),
    ('Clothing', 'clothing', 'Apparel and fashion'),
    ('Books', 'books', 'Books and reading'),
    ('Home', 'home', 'Home goods'),
    ('Sports', 'sports', 'Sports equipment');

-- ====================================
-- 6. PRODUCTS
-- ====================================
INSERT INTO
    products (
        seller_id,
        category_id,
        product_name,
        slug,
        base_sku,
        description,
        status,
        moderation_status
    )
VALUES
    -- Electronics
    (
        3,
        1,
        'Wireless Mouse',
        'wireless-mouse',
        'ELEC-001',
        'Comfortable wireless mouse',
        1,
        1
    ),
    (
        3,
        1,
        'USB Keyboard',
        'usb-keyboard',
        'ELEC-002',
        'Mechanical keyboard',
        1,
        1
    ),
    (
        3,
        1,
        'Laptop Stand',
        'laptop-stand',
        'ELEC-003',
        'Adjustable laptop stand',
        1,
        1
    ),
    (
        3,
        1,
        'Phone Charger',
        'phone-charger',
        'ELEC-004',
        'Fast charging cable',
        1,
        1
    ),
    -- Clothing
    (
        4,
        2,
        'T-Shirt',
        't-shirt',
        'CLOTH-001',
        'Cotton t-shirt',
        1,
        1
    ),
    (
        4,
        2,
        'Jeans',
        'jeans',
        'CLOTH-002',
        'Blue denim jeans',
        1,
        1
    ),
    (
        4,
        2,
        'Sneakers',
        'sneakers',
        'CLOTH-003',
        'Running shoes',
        1,
        1
    ),
    (
        4,
        2,
        'Hat',
        'hat',
        'CLOTH-004',
        'Baseball cap',
        1,
        1
    ),
    -- Books
    (
        3,
        3,
        'Novel',
        'novel',
        'BOOK-001',
        'Fiction book',
        1,
        1
    ),
    (
        3,
        3,
        'Cookbook',
        'cookbook',
        'BOOK-002',
        'Recipe collection',
        1,
        1
    ),
    (
        3,
        3,
        'Programming Guide',
        'programming-guide',
        'BOOK-003',
        'Learn to code',
        1,
        1
    ),
    (
        3,
        3,
        'Magazine',
        'magazine',
        'BOOK-004',
        'Monthly magazine',
        1,
        1
    ),
    -- Home
    (
        4,
        4,
        'Coffee Mug',
        'coffee-mug',
        'HOME-001',
        'Ceramic mug',
        1,
        1
    ),
    (
        4,
        4,
        'Pillow',
        'pillow',
        'HOME-002',
        'Soft pillow',
        1,
        1
    ),
    (
        4,
        4,
        'Lamp',
        'lamp',
        'HOME-003',
        'Desk lamp',
        1,
        1
    ),
    (
        4,
        4,
        'Plant Pot',
        'plant-pot',
        'HOME-004',
        'Decorative pot',
        1,
        1
    ),
    -- Sports
    (
        3,
        5,
        'Yoga Mat',
        'yoga-mat',
        'SPORT-001',
        'Exercise mat',
        1,
        1
    ),
    (
        3,
        5,
        'Water Bottle',
        'water-bottle',
        'SPORT-002',
        'Sports bottle',
        1,
        1
    ),
    (
        3,
        5,
        'Dumbbells',
        'dumbbells',
        'SPORT-003',
        'Weight training',
        1,
        1
    ),
    (
        3,
        5,
        'Jump Rope',
        'jump-rope',
        'SPORT-004',
        'Cardio equipment',
        1,
        1
    );

-- ====================================
-- 7. PRODUCT SKUs
-- ====================================
INSERT INTO
    product_skus (product_id, sku, price, is_default)
VALUES
    (1, 'ELEC-001-DEFAULT', 25.00, true),
    (2, 'ELEC-002-DEFAULT', 75.00, true),
    (3, 'ELEC-003-DEFAULT', 40.00, true),
    (4, 'ELEC-004-DEFAULT', 15.00, true),
    (5, 'CLOTH-001-DEFAULT', 20.00, true),
    (6, 'CLOTH-002-DEFAULT', 50.00, true),
    (7, 'CLOTH-003-DEFAULT', 80.00, true),
    (8, 'CLOTH-004-DEFAULT', 15.00, true),
    (9, 'BOOK-001-DEFAULT', 12.00, true),
    (10, 'BOOK-002-DEFAULT', 18.00, true),
    (11, 'BOOK-003-DEFAULT', 35.00, true),
    (12, 'BOOK-004-DEFAULT', 8.00, true),
    (13, 'HOME-001-DEFAULT', 10.00, true),
    (14, 'HOME-002-DEFAULT', 25.00, true),
    (15, 'HOME-003-DEFAULT', 30.00, true),
    (16, 'HOME-004-DEFAULT', 12.00, true),
    (17, 'SPORT-001-DEFAULT', 20.00, true),
    (18, 'SPORT-002-DEFAULT', 15.00, true),
    (19, 'SPORT-003-DEFAULT', 45.00, true),
    (20, 'SPORT-004-DEFAULT', 10.00, true);

-- ====================================
-- 8. INVENTORY (Stock for each SKU)
-- ====================================
INSERT INTO
    inventory (sku_id, quantity_available)
VALUES
    (1, 100),
    (2, 50),
    (3, 75),
    (4, 200),
    (5, 150),
    (6, 80),
    (7, 60),
    (8, 120),
    (9, 40),
    (10, 30),
    (11, 25),
    (12, 100),
    (13, 200),
    (14, 90),
    (15, 45),
    (16, 110),
    (17, 70),
    (18, 150),
    (19, 35),
    (20, 180);

-- ====================================
-- 9. SAMPLE ORDER
-- ====================================
INSERT INTO
    orders (
        order_number,
        user_id,
        status,
        subtotal,
        total_amount,
        currency
    )
VALUES
    ('ORD-2025-001', 1, 4, 100.00, 100.00, 0);

-- ====================================
-- 10. ORDER SHIPPING
-- ====================================
INSERT INTO
    order_shipping (
        order_id,
        recipient_name,
        phone,
        address_line1,
        city,
        country,
        shipping_method
    )
VALUES
    (
        1,
        'Cristiano Ronaldo',
        '0123456789',
        '1 Nguyen Hue',
        'Ho Chi Minh',
        'Vietnam',
        0
    );

-- ====================================
-- 11. ORDER PAYMENT
-- ====================================
INSERT INTO
    order_payments (
        order_id,
        payment_method,
        payment_status,
        amount,
        paid_at
    )
VALUES
    (1, 3, 2, 100.00, CURRENT_TIMESTAMP);

-- ====================================
-- 12. ORDER ITEMS
-- ====================================
INSERT INTO
    order_items (
        order_id,
        sku_id,
        product_name,
        sku,
        seller_id,
        quantity,
        unit_price,
        subtotal
    )
VALUES
    (
        1,
        1,
        'Wireless Mouse',
        'ELEC-001-DEFAULT',
        3,
        2,
        25.00,
        50.00
    ),
    (
        1,
        5,
        'T-Shirt',
        'CLOTH-001-DEFAULT',
        4,
        1,
        20.00,
        20.00
    ),
    (
        1,
        13,
        'Coffee Mug',
        'HOME-001-DEFAULT',
        4,
        3,
        10.00,
        30.00
    );