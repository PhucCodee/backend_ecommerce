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
    ('stephen@gmail.com', 'curry', 0),
    ('ye@gmail.com', 'west', 0),
    ('taylor@gmail.com', 'swift', 0),
    ('elon@gmail.com', 'musk', 0),
    ('bill@gmail.com', 'gates', 0),
    ('jeffrey@gmail.com', 'epstein', 0),
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
        'https://pub-0ba1e2c508fe4438a2885e8c43b1efec.r2.dev/goat.jpg',
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
        'https://pub-0ba1e2c508fe4438a2885e8c43b1efec.r2.dev/messi.jpg',
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
        'https://pub-0ba1e2c508fe4438a2885e8c43b1efec.r2.dev/lebron.png',
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
        'https://pub-0ba1e2c508fe4438a2885e8c43b1efec.r2.dev/curry.jpeg',
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
        'https://pub-0ba1e2c508fe4438a2885e8c43b1efec.r2.dev/kanye.jpg',
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
        'https://pub-0ba1e2c508fe4438a2885e8c43b1efec.r2.dev/swift.webp',
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
        'https://pub-0ba1e2c508fe4438a2885e8c43b1efec.r2.dev/musk.avif',
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
        'https://pub-0ba1e2c508fe4438a2885e8c43b1efec.r2.dev/gates.webp',
        'Philanthropist. Co-founder of Microsoft.',
        0,
        'America/Los_Angeles',
        0
    ),
    (
        9,
        'Jeffrey',
        'Epstein',
        '+17185550123',
        '1953-01-20',
        0,
        'https://pub-0ba1e2c508fe4438a2885e8c43b1efec.r2.dev/epstein.webp',
        'Financier. Known for controversial connections. Doesn''t like adults?',
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
        'https://pub-0ba1e2c508fe4438a2885e8c43b1efec.r2.dev/drake.jpg',
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
    (3, 0),
    -- Seller can also buy
    (3, 1),
    (4, 0),
    -- Seller can also buy
    (4, 1),
    (5, 2),
    (6, 2),
    (7, 0),
    (8, 0),
    (9, 0),
    (10, 0);