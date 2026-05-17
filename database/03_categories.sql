-- Root fashion category
INSERT INTO categories (
        category_name,
        slug,
        parent_category_id,
        description,
        image_url,
        display_order,
        is_core,
        is_active
    )
VALUES (
        'Fashion',
        'fashion',
        NULL,
        'Fashion and apparel products',
        'https://images.unsplash.com/photo-1624222244232-5f1ae13bbd53?q=80&w=1170&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D',
        1,
        TRUE,
        TRUE
    );
-- Fashion children
INSERT INTO categories (
        category_name,
        slug,
        parent_category_id,
        description,
        image_url,
        display_order,
        is_core,
        is_active
    )
VALUES (
        'Men',
        'men',
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'fashion'
        ),
        'Men''s fashion products',
        'https://images.unsplash.com/photo-1559582798-678dfc71ccd8?q=80&w=764&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D',
        2,
        TRUE,
        TRUE
    ),
    (
        'Women',
        'women',
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'fashion'
        ),
        'Women''s fashion products',
        'https://images.unsplash.com/photo-1596451984287-7a274406cbca?q=80&w=1170&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D',
        2,
        TRUE,
        TRUE
    ),
    (
        'Tops',
        'tops',
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'fashion'
        ),
        'T-shirts, polos, shirts, and tops',
        'https://plus.unsplash.com/premium_photo-1678218594563-9fe0d16c6838?q=80&w=687&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D',
        3,
        TRUE,
        TRUE
    ),
    (
        'Bottoms',
        'bottoms',
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'fashion'
        ),
        'Pants, jeans, shorts, and trousers',
        'https://images.unsplash.com/photo-1686485238414-2444d32d8c76?q=80&w=688&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D',
        3,
        TRUE,
        TRUE
    ),
    (
        'Outerwear',
        'outerwear',
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'fashion'
        ),
        'Jackets, hoodies, and coats',
        'https://plus.unsplash.com/premium_photo-1670088466079-6682af2baaee?q=80&w=686&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D',
        4,
        TRUE,
        TRUE
    ),
    (
        'Footwear',
        'footwear',
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'fashion'
        ),
        'Shoes, sneakers, and sandals',
        'https://images.unsplash.com/photo-1587563871167-1ee9c731aefb?q=80&w=1131&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D',
        4,
        TRUE,
        TRUE
    ),
    (
        'Accessories',
        'accessories',
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'fashion'
        ),
        'Caps, bags, belts, jewelry, and accessories',
        'https://images.unsplash.com/photo-1770017863938-237bd953a6e6?q=80&w=1172&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D',
        4,
        TRUE,
        TRUE
    );
-- Root collection category
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
        'Collections',
        'collections',
        NULL,
        'Limited drops and curated collections',
        5,
        FALSE,
        TRUE
    );
-- Collection children
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
        'summer-drop-2026',
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'collections'
        ),
        'Seasonal summer collection for 2026',
        6,
        FALSE,
        TRUE
    ),
    (
        'Curry Collab',
        'curry-collab',
        (
            SELECT category_id
            FROM categories
            WHERE slug = 'collections'
        ),
        'Special collaboration collection',
        6,
        FALSE,
        TRUE
    );