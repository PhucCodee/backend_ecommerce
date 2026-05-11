-- ====================================
-- SAMPLE DATA FOR E-COMMERCE SYSTEM
-- ====================================
-- ====================================
-- 1. USERS (Buyers and Sellers)
-- ====================================
INSERT INTO users (email, username, status)
VALUES (
        'justintran2901@gmail.com',
        'goat',
        0
    ),
    ('messi@gmail.com', 'messi', 0),
    ('lebron@gmail.com', 'king', 0),
    (
        'stephen@gmail.com',
        'curry',
        0
    ),
    ('ye@gmail.com', 'west', 0),
    (
        'taylor@gmail.com',
        'taylorswift',
        0
    ),
    (
        'elon@gmail.com',
        'elonmusk',
        0
    ),
    (
        'bill@gmail.com',
        'billgates',
        0
    ),
    (
        'beyonce@gmail.com',
        'beyonce',
        0
    ),
    ('drake@gmail.com', 'drake', 0);
-- ====================================
-- 2. USER CREDENTIALS
-- ====================================
INSERT INTO user_credentials (user_id, password_hash)
VALUES (
        1,
        -- Default password: Phuc123@
        '$2a$11$wHn6BvKLmdA5D9/vuIoVUeaJD64/z8Ek0VkaKO2Oz.Gk418LYwvYK'
    ),
    (
        2,
        '$2a$11$wHn6BvKLmdA5D9/vuIoVUeaJD64/z8Ek0VkaKO2Oz.Gk418LYwvYK'
    ),
    (
        3,
        '$2a$11$wHn6BvKLmdA5D9/vuIoVUeaJD64/z8Ek0VkaKO2Oz.Gk418LYwvYK'
    ),
    (
        4,
        '$2a$11$wHn6BvKLmdA5D9/vuIoVUeaJD64/z8Ek0VkaKO2Oz.Gk418LYwvYK'
    ),
    (
        5,
        '$2a$11$wHn6BvKLmdA5D9/vuIoVUeaJD64/z8Ek0VkaKO2Oz.Gk418LYwvYK'
    ),
    (
        6,
        '$2a$11$wHn6BvKLmdA5D9/vuIoVUeaJD64/z8Ek0VkaKO2Oz.Gk418LYwvYK'
    ),
    (
        7,
        '$2a$11$wHn6BvKLmdA5D9/vuIoVUeaJD64/z8Ek0VkaKO2Oz.Gk418LYwvYK'
    ),
    (
        8,
        '$2a$11$wHn6BvKLmdA5D9/vuIoVUeaJD64/z8Ek0VkaKO2Oz.Gk418LYwvYK'
    ),
    (
        9,
        '$2a$11$wHn6BvKLmdA5D9/vuIoVUeaJD64/z8Ek0VkaKO2Oz.Gk418LYwvYK'
    ),
    (
        10,
        '$2a$11$wHn6BvKLmdA5D9/vuIoVUeaJD64/z8Ek0VkaKO2Oz.Gk418LYwvYK'
    );
-- ====================================
-- 3. USER PROFILES
-- ====================================
INSERT INTO user_profiles (
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
VALUES (
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
    ),
    (
        6,
        'Taylor',
        'Swift',
        '+16175550123',
        '1989-12-13',
        1,
        'https://example.com/avatars/taylor.jpg',
        'Singer-songwriter. Lover of cats and catchy hooks.',
        0,
        'America/New_York',
        0
    ),
    (
        7,
        'Elon',
        'Musk',
        '+16505550123',
        '1971-06-28',
        0,
        'https://example.com/avatars/elon.jpg',
        'Entrepreneur. Rockets, cars, and memes.',
        0,
        'America/Los_Angeles',
        0
    ),
    (
        8,
        'Bill',
        'Gates',
        '+12065550123',
        '1955-10-28',
        0,
        'https://example.com/avatars/bill.jpg',
        'Philanthropist. Co-founder of Microsoft.',
        0,
        'America/Los_Angeles',
        0
    ),
    (
        9,
        'Beyoncé',
        'Knowles',
        '+17185550123',
        '1981-09-04',
        1,
        'https://example.com/avatars/beyonce.jpg',
        'Queen Bey. Singer, actress, icon.',
        0,
        'America/Chicago',
        0
    ),
    (
        10,
        'Aubrey',
        'Graham',
        '+14165550123',
        '1986-10-24',
        0,
        'https://example.com/avatars/drake.jpg',
        'Rapper, singer, and global superstar.',
        0,
        'America/Toronto',
        0
    );
-- ====================================
-- 3.1 USER ADDRESSES (multiple addresses per user)
-- address_type_enum values: house, apartment, office, other
-- ====================================
INSERT INTO user_addresses (
        user_id,
        address_type,
        is_default_shipping,
        is_default_billing,
        label,
        recipient_name,
        phone,
        address_line1,
        address_line2,
        city,
        state_province,
        postal_code,
        country,
        latitude,
        longitude
    )
VALUES -- User 1: 3 addresses
    (
        1,
        0,
        TRUE,
        TRUE,
        'Home',
        'Cristiano Ronaldo',
        '+351123456789',
        '123 Lisbon Street',
        'Apt 10',
        'Lisbon',
        'Lisbon',
        '1000-001',
        'Portugal',
        38.722300,
        -9.139300
    ),
    (
        1,
        2,
        FALSE,
        FALSE,
        'Office',
        'Cristiano Ronaldo',
        '+351123456789',
        '45 Business Park',
        NULL,
        'Lisbon',
        'Lisbon',
        '1000-002',
        'Portugal',
        38.716900,
        -9.142700
    ),
    (
        1,
        1,
        FALSE,
        FALSE,
        'Parents House',
        'Maria Ronaldo',
        '+351987654321',
        '7 Family Avenue',
        NULL,
        'Funchal',
        'Madeira',
        '9000-001',
        'Portugal',
        32.666900,
        -16.924100
    ),
    -- User 2: 2 addresses
    (
        2,
        1,
        TRUE,
        TRUE,
        'Main Apartment',
        'Lionel Messi',
        '+5491123456789',
        '88 Palermo Blvd',
        'Floor 12',
        'Buenos Aires',
        'CABA',
        'C1425',
        'Argentina',
        -34.603700,
        -58.381600
    ),
    (
        2,
        0,
        FALSE,
        FALSE,
        'Family Home',
        'Lionel Messi',
        '+5491123456789',
        '12 Rosario Road',
        NULL,
        'Rosario',
        'Santa Fe',
        'S2000',
        'Argentina',
        -32.944200,
        -60.650500
    ),
    -- User 3: 2 addresses (different default shipping/billing)
    (
        3,
        0,
        TRUE,
        FALSE,
        'LA Home',
        'LeBron James',
        '+12135551234',
        '23 Beverly Hills',
        NULL,
        'Los Angeles',
        'California',
        '90210',
        'USA',
        34.073600,
        -118.400400
    ),
    (
        3,
        2,
        FALSE,
        TRUE,
        'Business HQ',
        'LeBron James',
        '+12135551234',
        '500 Downtown Plaza',
        'Suite 800',
        'Los Angeles',
        'California',
        '90015',
        'USA',
        34.040700,
        -118.246800
    ),
    -- User 4: 2 addresses
    (
        4,
        0,
        TRUE,
        TRUE,
        'Home',
        'Stephen Curry',
        '+19255551234',
        '30 Bay Area Lane',
        NULL,
        'San Francisco',
        'California',
        '94105',
        'USA',
        37.774900,
        -122.419400
    ),
    (
        4,
        3,
        FALSE,
        FALSE,
        'Warehouse',
        'Stephen Curry',
        '+19255551234',
        '990 Storage Street',
        NULL,
        'Oakland',
        'California',
        '94607',
        'USA',
        37.804400,
        -122.271200
    ),
    -- User 5: 1 address
    (
        5,
        0,
        TRUE,
        TRUE,
        'Home',
        'Kanye West',
        '+13105551234',
        '1 Chicago Ave',
        NULL,
        'Chicago',
        'Illinois',
        '60601',
        'USA',
        41.878100,
        -87.629800
    );
-- ====================================
-- 4. USER ROLES
-- ====================================
INSERT INTO user_roles (user_id, role)
VALUES (1, 0),
    (2, 0),
    (3, 1),
    (3, 0),
    -- Seller can also buy
    (4, 1),
    (4, 0),
    -- Seller can also buy
    (5, 2),
    (6, 2),
    (7, 2),
    (8, 2),
    (9, 2),
    (10, 2);
-- ====================================
-- 5. CATEGORIES
-- - is_core = true  => core browsing categories
-- - is_core = false => limited collections
-- - parent_category_id NULL => root
-- ====================================
-- Root categories
INSERT INTO categories (
        category_name,
        slug,
        parent_category_id,
        description,
        display_order,
        is_core,
        is_active
    )
VALUES (
        'Clothing',
        'clothing',
        NULL,
        'All apparel & fashion',
        1,
        TRUE,
        TRUE
    ),
    (
        'Limited Collection',
        'limited-collection',
        NULL,
        'Limited drops / curated collections',
        2,
        FALSE,
        TRUE
    );
-- Core children under Clothing
INSERT INTO categories (
        category_name,
        slug,
        parent_category_id,
        description,
        display_order,
        is_core,
        is_active
    )
VALUES (
        'Men',
        'clothing-men',
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing'
        ),
        'Menswear',
        10,
        TRUE,
        TRUE
    ),
    (
        'Women',
        'clothing-women',
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing'
        ),
        'Womenswear',
        20,
        TRUE,
        TRUE
    ),
    (
        'Kids',
        'clothing-kids',
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing'
        ),
        'Kidswear',
        30,
        TRUE,
        TRUE
    ),
    (
        'Accessories',
        'clothing-accessories',
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing'
        ),
        'Caps, belts, beanies, etc.',
        40,
        TRUE,
        TRUE
    ),
    (
        'Footwear',
        'clothing-footwear',
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing'
        ),
        'Shoes & sneakers',
        50,
        TRUE,
        TRUE
    ),
    (
        'Outerwear',
        'clothing-outerwear',
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing'
        ),
        'Jackets & coats',
        60,
        TRUE,
        TRUE
    ),
    (
        'Activewear',
        'clothing-activewear',
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing'
        ),
        'Training & sportswear',
        70,
        TRUE,
        TRUE
    ),
    (
        'Basics',
        'clothing-basics',
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing'
        ),
        'Everyday essentials',
        80,
        TRUE,
        TRUE
    );
-- Limited collection children
INSERT INTO categories (
        category_name,
        slug,
        parent_category_id,
        description,
        display_order,
        is_core,
        is_active
    )
VALUES (
        'Summer Drop 2026',
        'limited-summer-drop-2026',
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'limited-collection'
        ),
        'Seasonal limited drop',
        10,
        FALSE,
        TRUE
    ),
    (
        'Curry Collab',
        'limited-curry-collab',
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'limited-collection'
        ),
        'Special collaboration pieces',
        20,
        FALSE,
        TRUE
    );
-- ====================================
-- 6. PRODUCTS
-- ====================================
INSERT INTO products (
        seller_id,
        product_name,
        slug,
        base_sku,
        description,
        status,
        moderation_status
    )
VALUES (
        4,
        'Classic Crew T-Shirt',
        'classic-crew-tshirt',
        'CLOTH-101',
        'Everyday cotton crew tee',
        1,
        1
    ),
    (
        4,
        'Oversized Graphic Tee',
        'oversized-graphic-tee',
        'CLOTH-102',
        'Relaxed fit graphic tee',
        1,
        1
    ),
    (
        4,
        'Slim Fit Jeans',
        'slim-fit-jeans',
        'CLOTH-103',
        'Stretch denim slim fit',
        1,
        1
    ),
    (
        4,
        'High-Rise Straight Jeans',
        'high-rise-straight-jeans',
        'CLOTH-104',
        'High-rise straight leg denim',
        1,
        1
    ),
    (
        4,
        'Pleated Midi Skirt',
        'pleated-midi-skirt',
        'CLOTH-105',
        'Light pleated midi skirt',
        1,
        1
    ),
    (
        4,
        'Fleece Pullover Hoodie',
        'fleece-pullover-hoodie',
        'CLOTH-106',
        'Warm fleece hoodie',
        1,
        1
    ),
    (
        4,
        'Bomber Jacket',
        'bomber-jacket',
        'CLOTH-107',
        'Classic bomber outerwear',
        1,
        1
    ),
    (
        4,
        'Trench Coat',
        'trench-coat',
        'CLOTH-108',
        'Long trench coat with belt',
        1,
        1
    ),
    (
        4,
        'Performance Running Sneakers',
        'performance-running-sneakers',
        'CLOTH-109',
        'Cushioned running sneakers',
        1,
        1
    ),
    (
        4,
        'Canvas Low-Top Sneakers',
        'canvas-lowtop-sneakers',
        'CLOTH-110',
        'Everyday canvas sneakers',
        1,
        1
    ),
    (
        4,
        'Leather Belt',
        'leather-belt',
        'CLOTH-111',
        'Genuine leather belt',
        1,
        1
    ),
    (
        4,
        'Baseball Cap',
        'baseball-cap',
        'CLOTH-112',
        'Adjustable cotton cap',
        1,
        1
    ),
    (
        4,
        'Knit Beanie',
        'knit-beanie',
        'CLOTH-113',
        'Warm knit beanie',
        1,
        1
    ),
    (
        4,
        'Yoga Leggings',
        'yoga-leggings',
        'CLOTH-114',
        'High-stretch leggings',
        1,
        1
    ),
    (
        4,
        'Sports Bra',
        'sports-bra',
        'CLOTH-115',
        'Medium support sports bra',
        1,
        1
    ),
    (
        4,
        'Kids Hoodie',
        'kids-hoodie',
        'CLOTH-116',
        'Soft kids hoodie',
        1,
        1
    ),
    (
        4,
        'Kids Sneakers',
        'kids-sneakers',
        'CLOTH-117',
        'Durable kids sneakers',
        1,
        1
    ),
    (
        4,
        'Lightweight Windbreaker',
        'lightweight-windbreaker',
        'CLOTH-118',
        'Packable windbreaker jacket',
        1,
        1
    ),
    (
        4,
        'Puffer Vest',
        'puffer-vest',
        'CLOTH-119',
        'Warm insulated vest',
        1,
        1
    ),
    (
        4,
        'Linen Short-Sleeve Shirt',
        'linen-shortsleeve-shirt',
        'CLOTH-120',
        'Breathable linen shirt',
        1,
        1
    );
-- ====================================
-- Product ↔ Category links
-- Each product has 1 primary category (is_primary=TRUE)
-- Some products also tagged into limited collections (is_primary=FALSE)
-- ====================================
INSERT INTO product_categories (
        product_id,
        category_id,
        is_primary
    )
VALUES -- Basics
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'classic-crew-tshirt'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing-basics'
        ),
        TRUE
    ),
    -- Men
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'slim-fit-jeans'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing-men'
        ),
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'linen-shortsleeve-shirt'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing-men'
        ),
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'bomber-jacket'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing-men'
        ),
        TRUE
    ),
    -- Women
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'high-rise-straight-jeans'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing-women'
        ),
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'pleated-midi-skirt'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing-women'
        ),
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'trench-coat'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing-women'
        ),
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'yoga-leggings'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing-women'
        ),
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'sports-bra'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing-women'
        ),
        TRUE
    ),
    -- Kids
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'kids-hoodie'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing-kids'
        ),
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'kids-sneakers'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing-kids'
        ),
        TRUE
    ),
    -- Accessories
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'leather-belt'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing-accessories'
        ),
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'baseball-cap'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing-accessories'
        ),
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'knit-beanie'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing-accessories'
        ),
        TRUE
    ),
    -- Footwear
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'performance-running-sneakers'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing-footwear'
        ),
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'canvas-lowtop-sneakers'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing-footwear'
        ),
        TRUE
    ),
    -- Outerwear
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'fleece-pullover-hoodie'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing-outerwear'
        ),
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'lightweight-windbreaker'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing-outerwear'
        ),
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'puffer-vest'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing-outerwear'
        ),
        TRUE
    ),
    -- Activewear
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'performance-running-sneakers'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing-activewear'
        ),
        FALSE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'yoga-leggings'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing-activewear'
        ),
        FALSE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'sports-bra'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing-activewear'
        ),
        FALSE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'lightweight-windbreaker'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing-activewear'
        ),
        FALSE
    ),
    -- Limited tags
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'oversized-graphic-tee'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'clothing-men'
        ),
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'oversized-graphic-tee'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'limited-summer-drop-2026'
        ),
        FALSE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'linen-shortsleeve-shirt'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'limited-summer-drop-2026'
        ),
        FALSE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'knit-beanie'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'limited-curry-collab'
        ),
        FALSE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'lightweight-windbreaker'
        ),
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'limited-curry-collab'
        ),
        FALSE
    ) ON CONFLICT DO NOTHING;
-- ====================================
-- 7. PRODUCT SKUs
-- ====================================
INSERT INTO product_skus (
        product_id,
        sku,
        price,
        is_default
    )
VALUES (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'classic-crew-tshirt'
        ),
        'CLOTH-101-DEFAULT',
        450000,
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'oversized-graphic-tee'
        ),
        'CLOTH-102-DEFAULT',
        490000,
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'slim-fit-jeans'
        ),
        'CLOTH-103-DEFAULT',
        690000,
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'high-rise-straight-jeans'
        ),
        'CLOTH-104-DEFAULT',
        600000,
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'pleated-midi-skirt'
        ),
        'CLOTH-105-DEFAULT',
        490000,
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'fleece-pullover-hoodie'
        ),
        'CLOTH-106-DEFAULT',
        1390000,
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'bomber-jacket'
        ),
        'CLOTH-107-DEFAULT',
        1900000,
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'trench-coat'
        ),
        'CLOTH-108-DEFAULT',
        1800000,
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'performance-running-sneakers'
        ),
        'CLOTH-109-DEFAULT',
        6900000,
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'canvas-lowtop-sneakers'
        ),
        'CLOTH-110-DEFAULT',
        2500000,
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'leather-belt'
        ),
        'CLOTH-111-DEFAULT',
        500000,
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'baseball-cap'
        ),
        'CLOTH-112-DEFAULT',
        800000,
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'knit-beanie'
        ),
        'CLOTH-113-DEFAULT',
        800000,
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'yoga-leggings'
        ),
        'CLOTH-114-DEFAULT',
        780000,
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'sports-bra'
        ),
        'CLOTH-115-DEFAULT',
        500000,
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'kids-hoodie'
        ),
        'CLOTH-116-DEFAULT',
        600000,
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'kids-sneakers'
        ),
        'CLOTH-117-DEFAULT',
        450000,
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'lightweight-windbreaker'
        ),
        'CLOTH-118-DEFAULT',
        1200000,
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'puffer-vest'
        ),
        'CLOTH-119-DEFAULT',
        690000,
        TRUE
    ),
    (
        (
            SELECT product_id
            FROM products
            WHERE slug = 'linen-shortsleeve-shirt'
        ),
        'CLOTH-120-DEFAULT',
        420000,
        TRUE
    );
-- ====================================
-- 8. INVENTORY
-- ====================================
INSERT INTO inventory (sku_id, quantity_available)
VALUES (
        (
            SELECT sku_id
            FROM product_skus
            WHERE sku = 'CLOTH-101-DEFAULT'
        ),
        200
    ),
    (
        (
            SELECT sku_id
            FROM product_skus
            WHERE sku = 'CLOTH-102-DEFAULT'
        ),
        120
    ),
    (
        (
            SELECT sku_id
            FROM product_skus
            WHERE sku = 'CLOTH-103-DEFAULT'
        ),
        80
    ),
    (
        (
            SELECT sku_id
            FROM product_skus
            WHERE sku = 'CLOTH-104-DEFAULT'
        ),
        75
    ),
    (
        (
            SELECT sku_id
            FROM product_skus
            WHERE sku = 'CLOTH-105-DEFAULT'
        ),
        90
    ),
    (
        (
            SELECT sku_id
            FROM product_skus
            WHERE sku = 'CLOTH-106-DEFAULT'
        ),
        110
    ),
    (
        (
            SELECT sku_id
            FROM product_skus
            WHERE sku = 'CLOTH-107-DEFAULT'
        ),
        60
    ),
    (
        (
            SELECT sku_id
            FROM product_skus
            WHERE sku = 'CLOTH-108-DEFAULT'
        ),
        50
    ),
    (
        (
            SELECT sku_id
            FROM product_skus
            WHERE sku = 'CLOTH-109-DEFAULT'
        ),
        70
    ),
    (
        (
            SELECT sku_id
            FROM product_skus
            WHERE sku = 'CLOTH-110-DEFAULT'
        ),
        95
    ),
    (
        (
            SELECT sku_id
            FROM product_skus
            WHERE sku = 'CLOTH-111-DEFAULT'
        ),
        140
    ),
    (
        (
            SELECT sku_id
            FROM product_skus
            WHERE sku = 'CLOTH-112-DEFAULT'
        ),
        160
    ),
    (
        (
            SELECT sku_id
            FROM product_skus
            WHERE sku = 'CLOTH-113-DEFAULT'
        ),
        130
    ),
    (
        (
            SELECT sku_id
            FROM product_skus
            WHERE sku = 'CLOTH-114-DEFAULT'
        ),
        100
    ),
    (
        (
            SELECT sku_id
            FROM product_skus
            WHERE sku = 'CLOTH-115-DEFAULT'
        ),
        100
    ),
    (
        (
            SELECT sku_id
            FROM product_skus
            WHERE sku = 'CLOTH-116-DEFAULT'
        ),
        85
    ),
    (
        (
            SELECT sku_id
            FROM product_skus
            WHERE sku = 'CLOTH-117-DEFAULT'
        ),
        80
    ),
    (
        (
            SELECT sku_id
            FROM product_skus
            WHERE sku = 'CLOTH-118-DEFAULT'
        ),
        65
    ),
    (
        (
            SELECT sku_id
            FROM product_skus
            WHERE sku = 'CLOTH-119-DEFAULT'
        ),
        55
    ),
    (
        (
            SELECT sku_id
            FROM product_skus
            WHERE sku = 'CLOTH-120-DEFAULT'
        ),
        90
    );