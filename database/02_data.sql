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
    ( 9, 'Beyoncé', 'Knowles', '+17185550123', '1981-09-04', 1, 'https://example.com/avatars/beyonce.jpg', 'Queen Bey. Singer, actress, icon.', 0, 'America/Chicago', 0
    ),
    ( 10, 'Aubrey', 'Graham', '+14165550123', '1986-10-24', 0, 'https://example.com/avatars/drake.jpg', 'Rapper, singer, and global superstar.', 0, 'America/Toronto', 0
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
-- - is_core = true  => core browsing categories
-- - is_core = false => limited collections
-- - parent_category_id NULL => root
-- ====================================

-- Root categories
INSERT INTO categories (category_name, slug, parent_category_id, description, display_order, is_core, is_active)
VALUES
  ('Clothing', 'clothing', NULL, 'All apparel & fashion', 1, TRUE, TRUE),
  ('Limited Collection', 'limited-collection', NULL, 'Limited drops / curated collections', 2, FALSE, TRUE);

-- Core children under Clothing
INSERT INTO categories (category_name, slug, parent_category_id, description, display_order, is_core, is_active)
VALUES
  ('Men',        'clothing-men',        (SELECT category_id FROM categories WHERE slug='clothing'), 'Menswear', 10, TRUE, TRUE),
  ('Women',      'clothing-women',      (SELECT category_id FROM categories WHERE slug='clothing'), 'Womenswear', 20, TRUE, TRUE),
  ('Kids',       'clothing-kids',       (SELECT category_id FROM categories WHERE slug='clothing'), 'Kidswear', 30, TRUE, TRUE),
  ('Accessories','clothing-accessories',(SELECT category_id FROM categories WHERE slug='clothing'), 'Caps, belts, beanies, etc.', 40, TRUE, TRUE),
  ('Footwear',   'clothing-footwear',   (SELECT category_id FROM categories WHERE slug='clothing'), 'Shoes & sneakers', 50, TRUE, TRUE),
  ('Outerwear',  'clothing-outerwear',  (SELECT category_id FROM categories WHERE slug='clothing'), 'Jackets & coats', 60, TRUE, TRUE),
  ('Activewear', 'clothing-activewear', (SELECT category_id FROM categories WHERE slug='clothing'), 'Training & sportswear', 70, TRUE, TRUE),
  ('Basics',     'clothing-basics',     (SELECT category_id FROM categories WHERE slug='clothing'), 'Everyday essentials', 80, TRUE, TRUE);

-- Limited collection children
INSERT INTO categories (category_name, slug, parent_category_id, description, display_order, is_core, is_active)
VALUES
  ('Summer Drop 2026', 'limited-summer-drop-2026', (SELECT category_id FROM categories WHERE slug='limited-collection'), 'Seasonal limited drop', 10, FALSE, TRUE),
  ('Curry Collab',     'limited-curry-collab',     (SELECT category_id FROM categories WHERE slug='limited-collection'), 'Special collaboration pieces', 20, FALSE, TRUE);

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
VALUES
(4, 'Classic Crew T-Shirt',          'classic-crew-tshirt',          'CLOTH-101', 'Everyday cotton crew tee', 1, 1),
(4, 'Oversized Graphic Tee',         'oversized-graphic-tee',         'CLOTH-102', 'Relaxed fit graphic tee', 1, 1),
(4, 'Slim Fit Jeans',                'slim-fit-jeans',                'CLOTH-103', 'Stretch denim slim fit', 1, 1),
(4, 'High-Rise Straight Jeans',      'high-rise-straight-jeans',      'CLOTH-104', 'High-rise straight leg denim', 1, 1),
(4, 'Pleated Midi Skirt',            'pleated-midi-skirt',            'CLOTH-105', 'Light pleated midi skirt', 1, 1),
(4, 'Fleece Pullover Hoodie',        'fleece-pullover-hoodie',        'CLOTH-106', 'Warm fleece hoodie', 1, 1),
(4, 'Bomber Jacket',                 'bomber-jacket',                 'CLOTH-107', 'Classic bomber outerwear', 1, 1),
(4, 'Trench Coat',                   'trench-coat',                   'CLOTH-108', 'Long trench coat with belt', 1, 1),
(4, 'Performance Running Sneakers',  'performance-running-sneakers',  'CLOTH-109', 'Cushioned running sneakers', 1, 1),
(4, 'Canvas Low-Top Sneakers',       'canvas-lowtop-sneakers',        'CLOTH-110', 'Everyday canvas sneakers', 1, 1),
(4, 'Leather Belt',                  'leather-belt',                  'CLOTH-111', 'Genuine leather belt', 1, 1),
(4, 'Baseball Cap',                  'baseball-cap',                  'CLOTH-112', 'Adjustable cotton cap', 1, 1),
(4, 'Knit Beanie',                   'knit-beanie',                   'CLOTH-113', 'Warm knit beanie', 1, 1),
(4, 'Yoga Leggings',                 'yoga-leggings',                 'CLOTH-114', 'High-stretch leggings', 1, 1),
(4, 'Sports Bra',                    'sports-bra',                    'CLOTH-115', 'Medium support sports bra', 1, 1),
(4, 'Kids Hoodie',                   'kids-hoodie',                   'CLOTH-116', 'Soft kids hoodie', 1, 1),
(4, 'Kids Sneakers',                 'kids-sneakers',                 'CLOTH-117', 'Durable kids sneakers', 1, 1),
(4, 'Lightweight Windbreaker',       'lightweight-windbreaker',       'CLOTH-118', 'Packable windbreaker jacket', 1, 1),
(4, 'Puffer Vest',                   'puffer-vest',                   'CLOTH-119', 'Warm insulated vest', 1, 1),
(4, 'Linen Short-Sleeve Shirt',      'linen-shortsleeve-shirt',       'CLOTH-120', 'Breathable linen shirt', 1, 1);

-- ====================================
-- Product ↔ Category links
-- Each product has 1 primary category (is_primary=TRUE)
-- Some products also tagged into limited collections (is_primary=FALSE)
-- ====================================

INSERT INTO product_categories (product_id, category_id, is_primary)
VALUES
-- Basics
((SELECT product_id FROM products WHERE slug='classic-crew-tshirt'),     (SELECT category_id FROM categories WHERE slug='clothing-basics'), TRUE),

-- Men
((SELECT product_id FROM products WHERE slug='slim-fit-jeans'),          (SELECT category_id FROM categories WHERE slug='clothing-men'), TRUE),
((SELECT product_id FROM products WHERE slug='linen-shortsleeve-shirt'), (SELECT category_id FROM categories WHERE slug='clothing-men'), TRUE),
((SELECT product_id FROM products WHERE slug='bomber-jacket'),           (SELECT category_id FROM categories WHERE slug='clothing-men'), TRUE),

-- Women
((SELECT product_id FROM products WHERE slug='high-rise-straight-jeans'),(SELECT category_id FROM categories WHERE slug='clothing-women'), TRUE),
((SELECT product_id FROM products WHERE slug='pleated-midi-skirt'),      (SELECT category_id FROM categories WHERE slug='clothing-women'), TRUE),
((SELECT product_id FROM products WHERE slug='trench-coat'),             (SELECT category_id FROM categories WHERE slug='clothing-women'), TRUE),
((SELECT product_id FROM products WHERE slug='yoga-leggings'),           (SELECT category_id FROM categories WHERE slug='clothing-women'), TRUE),
((SELECT product_id FROM products WHERE slug='sports-bra'),              (SELECT category_id FROM categories WHERE slug='clothing-women'), TRUE),

-- Kids
((SELECT product_id FROM products WHERE slug='kids-hoodie'),             (SELECT category_id FROM categories WHERE slug='clothing-kids'), TRUE),
((SELECT product_id FROM products WHERE slug='kids-sneakers'),           (SELECT category_id FROM categories WHERE slug='clothing-kids'), TRUE),

-- Accessories
((SELECT product_id FROM products WHERE slug='leather-belt'),            (SELECT category_id FROM categories WHERE slug='clothing-accessories'), TRUE),
((SELECT product_id FROM products WHERE slug='baseball-cap'),            (SELECT category_id FROM categories WHERE slug='clothing-accessories'), TRUE),
((SELECT product_id FROM products WHERE slug='knit-beanie'),             (SELECT category_id FROM categories WHERE slug='clothing-accessories'), TRUE),

-- Footwear
((SELECT product_id FROM products WHERE slug='performance-running-sneakers'), (SELECT category_id FROM categories WHERE slug='clothing-footwear'), TRUE),
((SELECT product_id FROM products WHERE slug='canvas-lowtop-sneakers'),       (SELECT category_id FROM categories WHERE slug='clothing-footwear'), TRUE),

-- Outerwear
((SELECT product_id FROM products WHERE slug='fleece-pullover-hoodie'),  (SELECT category_id FROM categories WHERE slug='clothing-outerwear'), TRUE),
((SELECT product_id FROM products WHERE slug='lightweight-windbreaker'), (SELECT category_id FROM categories WHERE slug='clothing-outerwear'), TRUE),
((SELECT product_id FROM products WHERE slug='puffer-vest'),             (SELECT category_id FROM categories WHERE slug='clothing-outerwear'), TRUE),

-- Activewear
((SELECT product_id FROM products WHERE slug='performance-running-sneakers'), (SELECT category_id FROM categories WHERE slug='clothing-activewear'), FALSE),
((SELECT product_id FROM products WHERE slug='yoga-leggings'),                (SELECT category_id FROM categories WHERE slug='clothing-activewear'), FALSE),
((SELECT product_id FROM products WHERE slug='sports-bra'),                   (SELECT category_id FROM categories WHERE slug='clothing-activewear'), FALSE),
((SELECT product_id FROM products WHERE slug='lightweight-windbreaker'),      (SELECT category_id FROM categories WHERE slug='clothing-activewear'), FALSE),

-- Limited tags
((SELECT product_id FROM products WHERE slug='oversized-graphic-tee'),    (SELECT category_id FROM categories WHERE slug='clothing-men'), TRUE),
((SELECT product_id FROM products WHERE slug='oversized-graphic-tee'),    (SELECT category_id FROM categories WHERE slug='limited-summer-drop-2026'), FALSE),

((SELECT product_id FROM products WHERE slug='linen-shortsleeve-shirt'),  (SELECT category_id FROM categories WHERE slug='limited-summer-drop-2026'), FALSE),

((SELECT product_id FROM products WHERE slug='knit-beanie'),              (SELECT category_id FROM categories WHERE slug='limited-curry-collab'), FALSE),
((SELECT product_id FROM products WHERE slug='lightweight-windbreaker'),  (SELECT category_id FROM categories WHERE slug='limited-curry-collab'), FALSE)
ON CONFLICT DO NOTHING;

-- ====================================
-- 7. PRODUCT SKUs
-- ====================================
INSERT INTO product_skus (product_id, sku, price, is_default)
VALUES
((SELECT product_id FROM products WHERE slug='classic-crew-tshirt'),         'CLOTH-101-DEFAULT', 19.99, TRUE),
((SELECT product_id FROM products WHERE slug='oversized-graphic-tee'),        'CLOTH-102-DEFAULT', 29.99, TRUE),
((SELECT product_id FROM products WHERE slug='slim-fit-jeans'),               'CLOTH-103-DEFAULT', 59.99, TRUE),
((SELECT product_id FROM products WHERE slug='high-rise-straight-jeans'),     'CLOTH-104-DEFAULT', 64.99, TRUE),
((SELECT product_id FROM products WHERE slug='pleated-midi-skirt'),           'CLOTH-105-DEFAULT', 44.99, TRUE),
((SELECT product_id FROM products WHERE slug='fleece-pullover-hoodie'),       'CLOTH-106-DEFAULT', 49.99, TRUE),
((SELECT product_id FROM products WHERE slug='bomber-jacket'),                'CLOTH-107-DEFAULT', 79.99, TRUE),
((SELECT product_id FROM products WHERE slug='trench-coat'),                  'CLOTH-108-DEFAULT', 99.99, TRUE),
((SELECT product_id FROM products WHERE slug='performance-running-sneakers'), 'CLOTH-109-DEFAULT', 89.99, TRUE),
((SELECT product_id FROM products WHERE slug='canvas-lowtop-sneakers'),       'CLOTH-110-DEFAULT', 54.99, TRUE),
((SELECT product_id FROM products WHERE slug='leather-belt'),                 'CLOTH-111-DEFAULT', 24.99, TRUE),
((SELECT product_id FROM products WHERE slug='baseball-cap'),                 'CLOTH-112-DEFAULT', 18.99, TRUE),
((SELECT product_id FROM products WHERE slug='knit-beanie'),                  'CLOTH-113-DEFAULT', 16.99, TRUE),
((SELECT product_id FROM products WHERE slug='yoga-leggings'),                'CLOTH-114-DEFAULT', 39.99, TRUE),
((SELECT product_id FROM products WHERE slug='sports-bra'),                   'CLOTH-115-DEFAULT', 29.99, TRUE),
((SELECT product_id FROM products WHERE slug='kids-hoodie'),                  'CLOTH-116-DEFAULT', 34.99, TRUE),
((SELECT product_id FROM products WHERE slug='kids-sneakers'),                'CLOTH-117-DEFAULT', 39.99, TRUE),
((SELECT product_id FROM products WHERE slug='lightweight-windbreaker'),      'CLOTH-118-DEFAULT', 59.99, TRUE),
((SELECT product_id FROM products WHERE slug='puffer-vest'),                  'CLOTH-119-DEFAULT', 69.99, TRUE),
((SELECT product_id FROM products WHERE slug='linen-shortsleeve-shirt'),      'CLOTH-120-DEFAULT', 42.99, TRUE);

-- ====================================
-- 8. INVENTORY
-- ====================================
INSERT INTO inventory (sku_id, quantity_available)
VALUES
((SELECT sku_id FROM product_skus WHERE sku='CLOTH-101-DEFAULT'), 200),
((SELECT sku_id FROM product_skus WHERE sku='CLOTH-102-DEFAULT'), 120),
((SELECT sku_id FROM product_skus WHERE sku='CLOTH-103-DEFAULT'), 80),
((SELECT sku_id FROM product_skus WHERE sku='CLOTH-104-DEFAULT'), 75),
((SELECT sku_id FROM product_skus WHERE sku='CLOTH-105-DEFAULT'), 90),
((SELECT sku_id FROM product_skus WHERE sku='CLOTH-106-DEFAULT'), 110),
((SELECT sku_id FROM product_skus WHERE sku='CLOTH-107-DEFAULT'), 60),
((SELECT sku_id FROM product_skus WHERE sku='CLOTH-108-DEFAULT'), 50),
((SELECT sku_id FROM product_skus WHERE sku='CLOTH-109-DEFAULT'), 70),
((SELECT sku_id FROM product_skus WHERE sku='CLOTH-110-DEFAULT'), 95),
((SELECT sku_id FROM product_skus WHERE sku='CLOTH-111-DEFAULT'), 140),
((SELECT sku_id FROM product_skus WHERE sku='CLOTH-112-DEFAULT'), 160),
((SELECT sku_id FROM product_skus WHERE sku='CLOTH-113-DEFAULT'), 130),
((SELECT sku_id FROM product_skus WHERE sku='CLOTH-114-DEFAULT'), 100),
((SELECT sku_id FROM product_skus WHERE sku='CLOTH-115-DEFAULT'), 100),
((SELECT sku_id FROM product_skus WHERE sku='CLOTH-116-DEFAULT'), 85),
((SELECT sku_id FROM product_skus WHERE sku='CLOTH-117-DEFAULT'), 80),
((SELECT sku_id FROM product_skus WHERE sku='CLOTH-118-DEFAULT'), 65),
((SELECT sku_id FROM product_skus WHERE sku='CLOTH-119-DEFAULT'), 55),
((SELECT sku_id FROM product_skus WHERE sku='CLOTH-120-DEFAULT'), 90);


---------------------------------DATABASE USER-------------------------------------------
-- 1. Tạo user mới dành riêng cho chatbot (đổi mật khẩu cho bảo mật nhé)
CREATE USER chatbot_user WITH PASSWORD 'chatbot';

-- 2. Cấp quyền kết nối vào database (thay 'database' bằng tên DB thực tế của bạn)
GRANT CONNECT ON DATABASE "database" TO chatbot_user;

-- 3. Cấp quyền sử dụng schema public
GRANT USAGE ON SCHEMA public TO chatbot_user;

-- 4. CHỈ cấp quyền SELECT (Đọc) trên các bảng cần thiết cho Product Search & Order Tracking
GRANT SELECT ON 
    categories, 
    products, 
    product_categories, 
    product_skus, 
    inventory,
    -- Giả sử hệ thống của bạn có các bảng order như code python hôm trước:
    orders, 
    order_items, 
    order_shipping, 
    order_payments, 
    order_fulfillment 
TO chatbot_user;

-- 5. Cấp quyền Đọc một phần thông tin User (nếu cần để chào tên khách, hiển thị profile)
GRANT SELECT ON users, user_profiles TO chatbot_user;

-- 6. CHẶN ĐỨT ĐƯỜNG truy cập vào bảng mật khẩu (Phòng hờ rủi ro)
REVOKE ALL ON user_credentials FROM chatbot_user;

-- 7. Thiết lập Default Privileges: Nếu sau này bạn tạo thêm bảng mới, chatbot cũng sẽ KHÔNG tự động có quyền đọc (phải cấp tay)
ALTER DEFAULT PRIVILEGES IN SCHEMA public REVOKE ALL ON TABLES FROM chatbot_user;





----------------------------------------------------------------------------------------------------------------------

INSERT INTO products (seller_id,product_name,slug,base_sku,description,status,moderation_status)
VALUES
-- 1
(4, 'Essential Black Cotton T-Shirt', 'essential-black-cotton-tee', 'CLOTH-301-DEFAULT',
'Premium black cotton t-shirt made from 100% combed cotton. Soft, breathable, and lightweight for everyday wear. Designed with a modern regular fit that works well for casual outfits, streetwear layering, or minimal style. Perfect for hot weather, gym sessions, or daily basics. Durable stitching, fade-resistant fabric, and comfortable neckline.', 1, 1),

-- 2
(4, 'Oversized Streetwear Hoodie Washed Grey', 'oversized-hoodie-grey', 'CLOTH-302-DEFAULT',
'Oversized hoodie in washed grey tone with vintage streetwear aesthetic. Made from heavyweight cotton fleece (350gsm) providing warmth and structure. Drop shoulder design, large front pocket, and relaxed silhouette. Ideal for urban fashion, skate style, and layering in colder seasons.', 1, 1),

-- 3
(4, 'Slim Fit Stretch Denim Jeans Blue', 'slim-fit-stretch-jeans-blue', 'CLOTH-303-DEFAULT',
'Slim fit blue denim jeans crafted with stretch fabric for maximum comfort and mobility. Mid-rise waist, tapered leg, and classic five-pocket design. Suitable for casual wear, office casual outfits, or night outings. Durable and flexible for daily usage.', 1, 1),

-- 4
(4, 'Athletic Quick Dry Training Shirt', 'athletic-quickdry-shirt', 'CLOTH-304-DEFAULT',
'High-performance quick-dry shirt designed for sports and fitness. Lightweight polyester fabric with moisture-wicking technology keeps you cool and dry during intense workouts. Ideal for running, gym training, football, and outdoor activities.', 1, 1),

-- 5
(4, 'Minimalist White Tee Clean Fit', 'minimalist-white-tee', 'CLOTH-305-DEFAULT',
'Clean and simple white t-shirt with minimalist design. Ultra-soft cotton blend with smooth texture. Perfect for layering under jackets or wearing standalone. Timeless style for any wardrobe.', 1, 1),

-- 6
(4, 'Heavyweight Hoodie Premium Black 400gsm', 'heavyweight-hoodie-black', 'CLOTH-306-DEFAULT',
'Luxury heavyweight hoodie made from 400gsm cotton fleece. Thick, warm, and structured fit. Designed for winter and premium streetwear outfits. High durability and comfort with a slightly oversized silhouette.', 1, 1),

-- 7
(4, 'Distressed Ripped Jeans Street Style', 'distressed-ripped-jeans', 'CLOTH-307-DEFAULT',
'Street-style ripped jeans with distressed detailing. Slim fit with modern cut. Designed for edgy outfits and casual urban fashion. Soft denim with worn-in look.', 1, 1),

-- 8
(4, 'Performance Running Shorts Ultra Light', 'performance-running-shorts', 'CLOTH-308-DEFAULT',
'Ultra-lightweight running shorts with breathable mesh panels. Designed for high-performance activities such as running, gym workouts, and training. Elastic waistband with secure fit.', 1, 1),

-- 9
(4, 'Classic Polo Shirt Cotton Blend', 'classic-polo-shirt', 'CLOTH-309-DEFAULT',
'Classic polo shirt made from cotton blend fabric. Breathable, slightly stretchy, and comfortable. Suitable for semi-formal occasions, office wear, or casual weekends.', 1, 1),

-- 10
(4, 'Cargo Pants Multi Pocket Tactical', 'cargo-pants-tactical', 'CLOTH-310-DEFAULT',
'Tactical cargo pants with multiple utility pockets. Durable fabric designed for outdoor use, travel, or streetwear fashion. Relaxed fit with functional design.', 1, 1),

-- 11
(4, 'Vintage Graphic Tee Retro Print', 'vintage-graphic-tee', 'CLOTH-311-DEFAULT',
'Retro graphic t-shirt featuring vintage-style prints. Soft cotton material with faded wash for authentic vintage feel. Great for casual and streetwear outfits.', 1, 1),

-- 12
(4, 'Compression Gym Shirt Muscle Fit', 'compression-gym-shirt', 'CLOTH-312-DEFAULT',
'Tight-fitting compression shirt designed to enhance muscle performance and blood circulation. Ideal for gym, bodybuilding, and sports training.', 1, 1),

-- 13
(4, 'Lightweight Windbreaker Jacket Packable', 'lightweight-windbreaker-packable', 'CLOTH-313-DEFAULT',
'Packable windbreaker jacket made from water-resistant material. Lightweight and easy to carry. Perfect for travel, hiking, or unpredictable weather.', 1, 1),

-- 14
(4, 'Soft Knit Sweater Winter Essential', 'soft-knit-sweater', 'CLOTH-314-DEFAULT',
'Warm knit sweater made from soft blended yarn. Comfortable for cold weather with stylish modern fit. Suitable for layering in winter outfits.', 1, 1),

-- 15
(4, 'Denim Jacket Classic Blue', 'denim-jacket-classic-blue', 'CLOTH-315-DEFAULT',
'Classic blue denim jacket with timeless design. Durable and versatile for all seasons. Can be styled with casual or semi-street outfits.', 1, 1),

-- 16
(4, 'Sports Jogger Pants Slim Fit', 'sports-jogger-pants', 'CLOTH-316-DEFAULT',
'Slim fit jogger pants designed for comfort and mobility. Elastic waistband, breathable fabric, and modern athletic look. Ideal for gym or daily wear.', 1, 1),

-- 17
(4, 'Basic Tank Top Cotton Ribbed', 'basic-tanktop', 'CLOTH-317-DEFAULT',
'Ribbed cotton tank top with stretch fit. Lightweight and breathable. Suitable for summer, gym, or layering.', 1, 1),

-- 18
(4, 'Thermal Long Sleeve Shirt Winter', 'thermal-long-sleeve', 'CLOTH-318-DEFAULT',
'Thermal long sleeve shirt designed to retain body heat in cold weather. Soft inner lining and comfortable fit. Ideal for winter layering.', 1, 1),

-- 19
(4, 'Relaxed Fit Linen Pants Summer', 'linen-pants-relaxed', 'CLOTH-319-DEFAULT',
'Breathable linen pants with relaxed fit. Perfect for hot weather and beach style outfits. Lightweight and comfortable.', 1, 1),

-- 20
(4, 'Urban Streetwear Zip Hoodie Black', 'zip-hoodie-black', 'CLOTH-320-DEFAULT',
'Zip-up hoodie designed for modern streetwear style. Comfortable cotton blend with smooth zipper and adjustable hood. Perfect for layering and everyday wear.', 1, 1);


-- ====================================
-- SKU FOR NEW PRODUCTS (CLOTH-301 → 320)
-- ====================================

INSERT INTO product_skus (product_id, sku, variant_attributes, price, is_default)
VALUES

-- 301 - Essential Black Tee
((SELECT product_id FROM products WHERE slug='essential-black-cotton-tee'), 'CLOTH-301-BLACK-M', '{"color":"black","size":"M"}', 19.99, TRUE),
((SELECT product_id FROM products WHERE slug='essential-black-cotton-tee'), 'CLOTH-301-BLACK-L', '{"color":"black","size":"L"}', 19.99, FALSE),
((SELECT product_id FROM products WHERE slug='essential-black-cotton-tee'), 'CLOTH-301-BLACK-XL', '{"color":"black","size":"XL"}', 20.99, FALSE),

-- 302 - Oversized Hoodie
((SELECT product_id FROM products WHERE slug='oversized-hoodie-grey'), 'CLOTH-302-GREY-M', '{"color":"grey","size":"M"}', 65.99, TRUE),
((SELECT product_id FROM products WHERE slug='oversized-hoodie-grey'), 'CLOTH-302-GREY-L', '{"color":"grey","size":"L"}', 65.99, FALSE),
((SELECT product_id FROM products WHERE slug='oversized-hoodie-grey'), 'CLOTH-302-GREY-XL', '{"color":"grey","size":"XL"}', 67.99, FALSE),

-- 303 - Jeans
((SELECT product_id FROM products WHERE slug='slim-fit-stretch-jeans-blue'), 'CLOTH-303-30', '{"size":30}', 59.99, TRUE),
((SELECT product_id FROM products WHERE slug='slim-fit-stretch-jeans-blue'), 'CLOTH-303-32', '{"size":32}', 59.99, FALSE),
((SELECT product_id FROM products WHERE slug='slim-fit-stretch-jeans-blue'), 'CLOTH-303-34', '{"size":34}', 61.99, FALSE),

-- 304 - Training Shirt
((SELECT product_id FROM products WHERE slug='athletic-quickdry-shirt'), 'CLOTH-304-BLACK-M', '{"color":"black","size":"M"}', 22.99, TRUE),
((SELECT product_id FROM products WHERE slug='athletic-quickdry-shirt'), 'CLOTH-304-BLACK-L', '{"color":"black","size":"L"}', 22.99, FALSE),
((SELECT product_id FROM products WHERE slug='athletic-quickdry-shirt'), 'CLOTH-304-BLUE-M', '{"color":"blue","size":"M"}', 23.99, FALSE),

-- 305 - White Tee
((SELECT product_id FROM products WHERE slug='minimalist-white-tee'), 'CLOTH-305-WHITE-M', '{"color":"white","size":"M"}', 15.99, TRUE),
((SELECT product_id FROM products WHERE slug='minimalist-white-tee'), 'CLOTH-305-WHITE-L', '{"color":"white","size":"L"}', 15.99, FALSE),

-- 306 - Heavy Hoodie
((SELECT product_id FROM products WHERE slug='heavyweight-hoodie-black'), 'CLOTH-306-BLACK-L', '{"color":"black","size":"L"}', 79.99, TRUE),
((SELECT product_id FROM products WHERE slug='heavyweight-hoodie-black'), 'CLOTH-306-BLACK-XL', '{"color":"black","size":"XL"}', 82.99, FALSE),

-- 307 - Ripped Jeans
((SELECT product_id FROM products WHERE slug='distressed-ripped-jeans'), 'CLOTH-307-30', '{"size":30}', 70.00, TRUE),
((SELECT product_id FROM products WHERE slug='distressed-ripped-jeans'), 'CLOTH-307-32', '{"size":32}', 70.00, FALSE),

-- 308 - Running Shorts
((SELECT product_id FROM products WHERE slug='performance-running-shorts'), 'CLOTH-308-BLACK-M', '{"color":"black","size":"M"}', 27.99, TRUE),
((SELECT product_id FROM products WHERE slug='performance-running-shorts'), 'CLOTH-308-BLACK-L', '{"color":"black","size":"L"}', 27.99, FALSE),

-- 309 - Polo
((SELECT product_id FROM products WHERE slug='classic-polo-shirt'), 'CLOTH-309-NAVY-M', '{"color":"navy","size":"M"}', 35.99, TRUE),
((SELECT product_id FROM products WHERE slug='classic-polo-shirt'), 'CLOTH-309-NAVY-L', '{"color":"navy","size":"L"}', 35.99, FALSE),

-- 310 - Cargo Pants
((SELECT product_id FROM products WHERE slug='cargo-pants-tactical'), 'CLOTH-310-32', '{"size":32}', 60.00, TRUE),
((SELECT product_id FROM products WHERE slug='cargo-pants-tactical'), 'CLOTH-310-34', '{"size":34}', 60.00, FALSE),

-- 311 - Graphic Tee
((SELECT product_id FROM products WHERE slug='vintage-graphic-tee'), 'CLOTH-311-BLACK-M', '{"color":"black","size":"M"}', 25.99, TRUE),

-- 312 - Compression Shirt
((SELECT product_id FROM products WHERE slug='compression-gym-shirt'), 'CLOTH-312-BLACK-M', '{"color":"black","size":"M"}', 30.00, TRUE),

-- 313 - Windbreaker
((SELECT product_id FROM products WHERE slug='lightweight-windbreaker-packable'), 'CLOTH-313-GREEN-M', '{"color":"green","size":"M"}', 59.99, TRUE),

-- 314 - Sweater
((SELECT product_id FROM products WHERE slug='soft-knit-sweater'), 'CLOTH-314-BEIGE-M', '{"color":"beige","size":"M"}', 49.99, TRUE),

-- 315 - Denim Jacket
((SELECT product_id FROM products WHERE slug='denim-jacket-classic-blue'), 'CLOTH-315-BLUE-L', '{"color":"blue","size":"L"}', 89.99, TRUE),

-- 316 - Jogger
((SELECT product_id FROM products WHERE slug='sports-jogger-pants'), 'CLOTH-316-M', '{"size":"M"}', 45.99, TRUE),

-- 317 - Tanktop
((SELECT product_id FROM products WHERE slug='basic-tanktop'), 'CLOTH-317-WHITE-M', '{"color":"white","size":"M"}', 12.99, TRUE),

-- 318 - Thermal
((SELECT product_id FROM products WHERE slug='thermal-long-sleeve'), 'CLOTH-318-BLACK-M', '{"color":"black","size":"M"}', 34.99, TRUE),

-- 319 - Linen Pants
((SELECT product_id FROM products WHERE slug='linen-pants-relaxed'), 'CLOTH-319-M', '{"size":"M"}', 42.99, TRUE),

-- 320 - Zip Hoodie
((SELECT product_id FROM products WHERE slug='zip-hoodie-black'), 'CLOTH-320-BLACK-M', '{"color":"black","size":"M"}', 55.99, TRUE);



-- ====================================
-- INVENTORY FOR ALL NEW SKUs (CLOTH-301 → 320)
-- ====================================

INSERT INTO inventory (
    sku_id,
    quantity_available,
    quantity_reserved,
    quantity_sold,
    reorder_point,
    reorder_quantity
)
SELECT 
    sku_id,

    -- Available stock: realistic distribution
    CASE 
        WHEN RANDOM() < 0.1 THEN 0                         -- 10% out of stock
        WHEN RANDOM() < 0.3 THEN (RANDOM()*10)::INT + 1    -- low stock (1-10)
        WHEN RANDOM() < 0.7 THEN (RANDOM()*50)::INT + 20   -- medium (20-70)
        ELSE (RANDOM()*150)::INT + 50                      -- high stock (50-200)
    END,

    -- Reserved (cart holding)
    (RANDOM()*5)::INT,

    -- Sold count (simulate popularity)
    (RANDOM()*300)::INT,

    -- Reorder logic
    10,
    50

FROM product_skus
WHERE sku LIKE 'CLOTH-3%';