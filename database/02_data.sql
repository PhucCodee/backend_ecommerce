-- ====================================
-- SAMPLE DATA FOR E-COMMERCE SYSTEM
-- ====================================
-- ====================================
-- 1. USERS (Buyers and Sellers)
-- ====================================
INSERT INTO
    users (email, username, status)
VALUES
    ('ronaldo@gmail.com', 'goat', 0),
    ('messi@gmail.com', 'messi', 0),
    ('lebron@gmail.com', 'king', 0),
    ('stephen@gmail.com', 'curry', 0),
    ('ye@gmail.com', 'west', 0),
    ('taylor@gmail.com', 'taylorswift', 0),
    ('elon@gmail.com', 'elonmusk', 0),
    ('bill@gmail.com', 'billgates', 0),
    ('beyonce@gmail.com', 'beyonce', 0),
    ('drake@gmail.com', 'drake', 0);

-- ====================================
-- 2. USER CREDENTIALS
-- ====================================
INSERT INTO
    user_credentials (user_id, password_hash, password_salt)
VALUES
    (
        1, -- Default password: Phuc123
        'ERxRCpHri3I03wLKg/e4k0+kMPnS5URxegssf3JvSIw=',
        'ugUJ3KRZmig0d7wJUQsq8ttaAGbtjqEcOE+ZgAekw2Y='
    ),
    (
        2,
        'ERxRCpHri3I03wLKg/e4k0+kMPnS5URxegssf3JvSIw=',
        'ugUJ3KRZmig0d7wJUQsq8ttaAGbtjqEcOE+ZgAekw2Y='
    ),
    (
        3,
        'ERxRCpHri3I03wLKg/e4k0+kMPnS5URxegssf3JvSIw=',
        'ugUJ3KRZmig0d7wJUQsq8ttaAGbtjqEcOE+ZgAekw2Y='
    ),
    (
        4,
        'ERxRCpHri3I03wLKg/e4k0+kMPnS5URxegssf3JvSIw=',
        'ugUJ3KRZmig0d7wJUQsq8ttaAGbtjqEcOE+ZgAekw2Y='
    ),
    (
        5,
        'ERxRCpHri3I03wLKg/e4k0+kMPnS5URxegssf3JvSIw=',
        'ugUJ3KRZmig0d7wJUQsq8ttaAGbtjqEcOE+ZgAekw2Y='
    ),
    (
        6,
        'ERxRCpHri3I03wLKg/e4k0+kMPnS5URxegssf3JvSIw=',
        'ugUJ3KRZmig0d7wJUQsq8ttaAGbtjqEcOE+ZgAekw2Y='
    ),
    (
        7,
        'ERxRCpHri3I03wLKg/e4k0+kMPnS5URxegssf3JvSIw=',
        'ugUJ3KRZmig0d7wJUQsq8ttaAGbtjqEcOE+ZgAekw2Y='
    ),
    (
        8,
        'ERxRCpHri3I03wLKg/e4k0+kMPnS5URxegssf3JvSIw=',
        'ugUJ3KRZmig0d7wJUQsq8ttaAGbtjqEcOE+ZgAekw2Y='
    ),
    (
        9,
        'ERxRCpHri3I03wLKg/e4k0+kMPnS5URxegssf3JvSIw=',
        'ugUJ3KRZmig0d7wJUQsq8ttaAGbtjqEcOE+ZgAekw2Y='
    ),
    (
        10,
        'ERxRCpHri3I03wLKg/e4k0+kMPnS5URxegssf3JvSIw=',
        'ugUJ3KRZmig0d7wJUQsq8ttaAGbtjqEcOE+ZgAekw2Y='
    );

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
    (5, 2),
    (6, 2),
    (7, 2),
    (8, 2),
    (9, 2),
    (10, 2);

-- ====================================
-- 5. CATEGORIES
-- ====================================
INSERT INTO categories (category_name, slug, description) VALUES
('Tops', 'tops', 'T-shirts, hoodies, and shirts'),
('Bottoms', 'bottoms', 'Jeans, joggers, and trousers'),
('Outerwear', 'outerwear', 'Jackets, coats, and blazers'),
('Footwear', 'footwear', 'Sneakers, boots, and formal shoes'),
('Accessories', 'accessories', 'Hats, bags, and jewelry');
    


-- ====================================
-- 6. PRODUCTS
-- ====================================

INSERT INTO products (seller_id, category_id, product_name, slug, base_sku, description) VALUES
-- ID 1: Tops
(3, 1, 'Vintage Wash Heavyweight Tee', 'vintage-wash-tee', 'CLOTH-TEE-001', 'Boxy fit, garment-dyed t-shirt with a vintage feel.'),
-- ID 2: Bottoms
(4, 2, 'Tech-Wear Cargo Pants', 'tech-cargo-pants', 'CLOTH-PAN-001', 'Water-repellent cargo pants with 6 functional pockets.' ),
-- ID 3: Outerwear
(3, 3, 'Sherpa Lined Denim Jacket', 'sherpa-denim-jacket', 'CLOTH-JKT-001', 'Classic blue denim jacket with warm faux-shearling lining.'),
-- ID 4: Footwear
(4, 4, 'Urban Nomad Knit Sneakers', 'urban-nomad-sneakers', 'CLOTH-SHN-001', 'Breathable knit upper with responsive foam cushioning.'),
-- ID 5: Accessories
(3, 5, 'Signature Embroidered Beanie', 'signature-beanie', 'CLOTH-ACC-001', 'Tight-knit acrylic beanie with fold-over cuff.'),
-- ID 6: High Performance (Category 3 - Outerwear)
(4, 3, 'Alpine Explorer Hiking Jacket', 'alpine-explorer-jacket', 'CLOTH-JKT-002', '3-layer Gore-Tex shell, 100% waterproof and breathable.'),
(3, 1, 'Oversized Street Tee', 'oversized-street-tee', 'CLOTH-TEE-002', 'Heavy cotton oversized streetwear t-shirt.'),
(4, 1, 'Minimal Logo Tee', 'minimal-logo-tee', 'CLOTH-TEE-003', 'Clean minimalist t-shirt with small embroidered logo.'),
(3, 1, 'Vintage Graphic Tee', 'vintage-graphic-tee', 'CLOTH-TEE-004', 'Retro graphic print cotton t-shirt.'),
(4, 2, 'Slim Fit Denim Jeans', 'slim-fit-denim-jeans', 'CLOTH-PAN-002', 'Stretch slim-fit denim jeans.'),
(3, 2, 'Relaxed Fit Sweatpants', 'relaxed-sweatpants', 'CLOTH-PAN-003', 'Comfortable fleece sweatpants for daily wear.'),
(4, 2, 'Urban Jogger Pants', 'urban-jogger-pants', 'CLOTH-PAN-004', 'Tapered joggers with elastic ankle cuffs.'),
(3, 3, 'Lightweight Windbreaker', 'lightweight-windbreaker', 'CLOTH-JKT-003', 'Ultra lightweight windbreaker jacket.'),
(4, 3, 'Puffer Winter Jacket', 'puffer-winter-jacket', 'CLOTH-JKT-004', 'Insulated puffer jacket for cold weather.'),
(3, 3, 'Utility Field Jacket', 'utility-field-jacket', 'CLOTH-JKT-005', 'Military inspired field jacket with pockets.'),
(4, 4, 'Runner Pro Sneakers', 'runner-pro-sneakers', 'CLOTH-SHN-002', 'Lightweight running sneakers with mesh upper.'),
(3, 4, 'Classic Canvas Sneakers', 'canvas-sneakers', 'CLOTH-SHN-003', 'Timeless canvas sneakers with rubber sole.'),
(4, 4, 'Street Skate Shoes', 'street-skate-shoes', 'CLOTH-SHN-004', 'Durable skateboarding shoes.'),
(3, 5, 'Classic Baseball Cap', 'classic-baseball-cap', 'CLOTH-ACC-002', 'Adjustable cotton baseball cap.'),
(4, 5, 'Minimalist Leather Belt', 'leather-belt', 'CLOTH-ACC-003', 'Premium leather belt with metal buckle.'),
(3, 5, 'Urban Sling Bag', 'urban-sling-bag', 'CLOTH-ACC-004', 'Compact sling bag for everyday carry.'),
(4, 1, 'Performance Training Tee', 'training-performance-tee', 'CLOTH-TEE-005', 'Moisture-wicking training shirt.'),
(3, 2, 'Cargo Utility Shorts', 'cargo-utility-shorts', 'CLOTH-PAN-005', 'Functional cargo shorts with multiple pockets.'),
(4, 3, 'Softshell Tactical Jacket', 'softshell-tactical-jacket', 'CLOTH-JKT-006', 'Durable tactical jacket with softshell fabric.'),
(3, 4, 'Trail Running Shoes', 'trail-running-shoes', 'CLOTH-SHN-005', 'Trail running shoes with rugged outsole.'),
(4, 5, 'Winter Knit Scarf', 'winter-knit-scarf', 'CLOTH-ACC-005', 'Warm knitted scarf for winter.');

-- ====================================
-- 7. PRODUCT SKUs
-- ====================================
INSERT INTO product_skus (product_id, sku, price, is_default, variant_attributes) VALUES

-- Product 1
(1, 'CLOTH-TEE-001-BLK-M', 35.00, true,  '{"color": "Charcoal Black", "size": "M"}'),
(1, 'CLOTH-TEE-001-BLK-L', 35.00, false, '{"color": "Charcoal Black", "size": "L"}'),
(1, 'CLOTH-TEE-001-BLK-XL', 38.00, false, '{"color": "Charcoal Black", "size": "XL"}'),

-- Product 2
(2, 'CLOTH-PAN-001-OLV-S', 85.00, true,  '{"color": "Olive Drab", "size": "S"}'),
(2, 'CLOTH-PAN-001-OLV-M', 85.00, false, '{"color": "Olive Drab", "size": "M"}'),
(2, 'CLOTH-PAN-001-OLV-L', 85.00, false, '{"color": "Olive Drab", "size": "L"}'),

-- Product 3
(3, 'CLOTH-JKT-001-BLU-M', 120.00, true,  '{"color": "Indio Blue", "size": "M"}'),
(3, 'CLOTH-JKT-001-BLU-L', 120.00, false, '{"color": "Indio Blue", "size": "L"}'),

-- Product 4
(4, 'CLOTH-SHN-001-WHT-9', 110.00, true,  '{"color": "Triple White", "size": "9"}'),
(4, 'CLOTH-SHN-001-WHT-10',110.00, false, '{"color": "Triple White", "size": "10"}'),
(4, 'CLOTH-SHN-001-WHT-11',110.00, false, '{"color": "Triple White", "size": "11"}'),

-- Product 5
(5,'CLOTH-ACC-001-BLK-STD',25,true,'{"color":"Black","size":"One Size"}'),

-- Product 6
(6,'CLOTH-JKT-002-RED-M',220,true,'{"color":"Red","size":"M"}'),
(6,'CLOTH-JKT-002-RED-L',220,false,'{"color":"Red","size":"L"}'),

-- Product 7
(7,'CLOTH-TEE-002-BLK-M',32,true,'{"color":"Black","size":"M"}'),
(7,'CLOTH-TEE-002-BLK-L',32,false,'{"color":"Black","size":"L"}'),

-- Product 8
(8,'CLOTH-TEE-003-WHT-M',30,true,'{"color":"White","size":"M"}'),
(8,'CLOTH-TEE-003-WHT-L',30,false,'{"color":"White","size":"L"}'),

-- Product 9
(9,'CLOTH-TEE-004-GRY-M',34,true,'{"color":"Grey","size":"M"}'),
(9,'CLOTH-TEE-004-GRY-L',34,false,'{"color":"Grey","size":"L"}'),

-- Product 10
(10,'CLOTH-PAN-002-BLU-30',70,true,'{"color":"Blue","size":"30"}'),
(10,'CLOTH-PAN-002-BLU-32',70,false,'{"color":"Blue","size":"32"}'),

-- Product 11
(11,'CLOTH-PAN-003-GRY-M',55,true,'{"color":"Grey","size":"M"}'),
(11,'CLOTH-PAN-003-GRY-L',55,false,'{"color":"Grey","size":"L"}'),

-- Product 12
(12,'CLOTH-PAN-004-BLK-M',60,true,'{"color":"Black","size":"M"}'),
(12,'CLOTH-PAN-004-BLK-L',60,false,'{"color":"Black","size":"L"}'),

-- Product 13
(13,'CLOTH-JKT-003-NVY-M',95,true,'{"color":"Navy","size":"M"}'),
(13,'CLOTH-JKT-003-NVY-L',95,false,'{"color":"Navy","size":"L"}'),

-- Product 14
(14,'CLOTH-JKT-004-BLK-M',150,true,'{"color":"Black","size":"M"}'),
(14,'CLOTH-JKT-004-BLK-L',150,false,'{"color":"Black","size":"L"}'),

-- Product 15
(15,'CLOTH-JKT-005-OLV-M',130,true,'{"color":"Olive","size":"M"}'),
(15,'CLOTH-JKT-005-OLV-L',130,false,'{"color":"Olive","size":"L"}'),

-- Product 16
(16,'CLOTH-SHN-002-WHT-9',120,true,'{"color":"White","size":"9"}'),
(16,'CLOTH-SHN-002-WHT-10',120,false,'{"color":"White","size":"10"}'),

-- Product 17
(17,'CLOTH-SHN-003-BLK-9',65,true,'{"color":"Black","size":"9"}'),
(17,'CLOTH-SHN-003-BLK-10',65,false,'{"color":"Black","size":"10"}'),

-- Product 18
(18,'CLOTH-SHN-004-BLK-9',90,true,'{"color":"Black","size":"9"}'),
(18,'CLOTH-SHN-004-BLK-10',90,false,'{"color":"Black","size":"10"}'),

-- Product 19
(19,'CLOTH-ACC-002-BLK-STD',25,true,'{"color":"Black","size":"One Size"}'),
(19,'CLOTH-ACC-002-NVY-STD',25,false,'{"color":"Navy","size":"One Size"}'),

-- Product 20
(20,'CLOTH-ACC-003-BRN-M',45,true,'{"color":"Brown","size":"M"}'),
(20,'CLOTH-ACC-003-BRN-L',45,false,'{"color":"Brown","size":"L"}'),

-- Product 21
(21,'CLOTH-ACC-004-BLK-STD',50,true,'{"color":"Black","size":"One Size"}'),
(21,'CLOTH-ACC-004-GRY-STD',50,false,'{"color":"Grey","size":"One Size"}'),

-- Product 22
(22,'CLOTH-TEE-005-BLK-M',38,true,'{"color":"Black","size":"M"}'),
(22,'CLOTH-TEE-005-BLK-L',38,false,'{"color":"Black","size":"L"}'),

-- Product 23
(23,'CLOTH-PAN-005-KHK-M',40,true,'{"color":"Khaki","size":"M"}'),
(23,'CLOTH-PAN-005-KHK-L',40,false,'{"color":"Khaki","size":"L"}'),

-- Product 24
(24,'CLOTH-JKT-006-BLK-M',140,true,'{"color":"Black","size":"M"}'),
(24,'CLOTH-JKT-006-BLK-L',140,false,'{"color":"Black","size":"L"}'),

-- Product 25
(25,'CLOTH-SHN-005-GRN-9',130,true,'{"color":"Green","size":"9"}'),
(25,'CLOTH-SHN-005-GRN-10',130,false,'{"color":"Green","size":"10"}'),

-- Product 26
(26,'CLOTH-ACC-005-GRY-STD',28,true,'{"color":"Grey","size":"One Size"}'),
(26,'CLOTH-ACC-005-BLK-STD',28,false,'{"color":"Black","size":"One Size"}');

-- ====================================
-- 8. INVENTORY (Stock for each SKU)
-- ====================================
INSERT INTO inventory (sku_id, quantity_available) VALUES
(1, 50),  -- Tee Black M
(2, 75),  -- Tee Black L
(3, 30),  -- Tee Black XL
(4, 20),  -- Cargo S
(5, 45),  -- Cargo M
(6, 40),  -- Cargo L
(7, 15),  -- Jacket M
(8, 12),  -- Jacket L
(9, 25),  -- Sneakers 9
(10, 30), -- Sneakers 10
(11, 20), -- Sneakers 11
(12,40),(13,35),
(14,50),(15,45),
(16,38),(17,36),
(18,30),(19,28),
(20,40),(21,35),
(22,32),(23,29),
(24,25),(25,22),
(26,18),(27,20),
(28,17),(29,15),
(30,45),(31,40),
(32,50),(33,48),
(34,35),(35,33),
(36,60),(37,55),
(38,30),(39,28),
(40,25),(41,22),
(42,20),(43,18),
(44,26),(45,24),
(46,21),(47,19),
(48,15),(49,14),
(50,40),(51,38);

-- ====================================
-- 9. SAMPLE ORDER
-- ====================================
INSERT INTO orders (order_number, user_id, status, subtotal, total_amount, currency)
VALUES 
    ('ORD-2025-001', 1, 4, 155.00, 155.00, 0);
-- ====================================

-- ORDER ITEMS (Updated for Clothing)
-- ====================================
INSERT INTO order_items (order_id, sku_id, product_name, sku, seller_id, quantity, unit_price, subtotal)
VALUES
    (1, 1, 'Vintage Wash Heavyweight Tee', 'VNTG-TEE-BLK-M', 3, 2, 35.00, 70.00),
    (1, 4, 'Tech-Wear Cargo Pants', 'TECH-CRG-OLV-S', 4, 1, 85.00, 85.00);

-- ====================================
-- ORDER SHIPPING
-- ====================================
INSERT INTO order_shipping (order_id,recipient_name,phone,address_line1,city,country,shipping_method)
VALUES
    (1,'Cristiano Ronaldo','0123456789','1 Nguyen Hue','Ho Chi Minh','Vietnam',0);

-- ====================================
-- ORDER PAYMENT
-- ====================================
INSERT INTO order_payments (order_id, payment_method, payment_status, amount, paid_at)
VALUES 
    (1, 3, 2, 155.00, CURRENT_TIMESTAMP);


