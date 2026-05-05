INSERT INTO product_images (
    sku_id,
    image_url,
    thumbnail_url,
    alt_text,
    display_order,
    is_primary
)
VALUES
    -- ===================
    -- 101: Classic Crew T-Shirt
    -- ===================
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-101-DEFAULT'),
        'https://www.rails.com/cdn/shop/files/CLASSIC-CREW-WHITE-1A_4e0ca3f7-f0be-416a-b8e8-732a257e9127.jpg?v=1752088426&width=1690',
        'https://www.rails.com/cdn/shop/files/CLASSIC-CREW-WHITE-1A_4e0ca3f7-f0be-416a-b8e8-732a257e9127.jpg?v=1752088426&width=1690',
        'Classic crew t-shirt front view',
        1,
        TRUE
    ),
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-101-DEFAULT'),
        'https://www.rails.com/cdn/shop/files/CLASSIC-CREW-WHITE-5_b0e23804-aa17-4df3-893b-5737112bfa41.jpg?v=1752088427&width=1690',
        'https://www.rails.com/cdn/shop/files/CLASSIC-CREW-WHITE-5_b0e23804-aa17-4df3-893b-5737112bfa41.jpg?v=1752088427&width=1690',
        'Classic crew t-shirt back view',
        2,
        FALSE
    ),

    -- ===================
    -- 102: Oversized Graphic Tee
    -- ===================
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-102-DEFAULT'),
        'https://dekryptic.com/cdn/shop/files/TheBoondocks-RileyKhaledAcidWashOversizeOliveT-Shirt-Front.jpg?v=1728418770&width=1600',
        'https://dekryptic.com/cdn/shop/files/TheBoondocks-RileyKhaledAcidWashOversizeOliveT-Shirt-Front.jpg?v=1728418770&width=1600',
        'Oversized Graphic Tee front view',
        1,
        TRUE
    ),
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-102-DEFAULT'),
        'https://dekryptic.com/cdn/shop/files/TheBoondocks-RileyKhaledAcidWashOversizeOliveT-Shirt-Back.jpg?v=1728418770&width=1600',
        'https://dekryptic.com/cdn/shop/files/TheBoondocks-RileyKhaledAcidWashOversizeOliveT-Shirt-Back.jpg?v=1728418770&width=1600',
        'Oversized Graphic Tee back view',
        2,
        FALSE
    ),

    -- ===================
    -- 103: Slim Fit Jeans
    -- ===================
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-103-DEFAULT'),
        'https://yame.vn/cdn/shop/files/qu-n-jean-the-original-02-xanh-nh-t-1174882602.jpg?v=1760783019&width=713',
        'https://yame.vn/cdn/shop/files/qu-n-jean-the-original-02-xanh-nh-t-1174882602.jpg?v=1760783019&width=713',
        'Slim Fit Jeans front view',
        1,
        TRUE
    ),
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-103-DEFAULT'),
        'https://yame.vn/cdn/shop/files/TheOriginal018c.jpg?v=1771833319&width=713',
        'https://yame.vn/cdn/shop/files/TheOriginal018c.jpg?v=1771833319&width=713',
        'Slim Fit Jeans info',
        2,
        FALSE
    ),

    -- ===================
    -- 104: High-Rise Straight Jeans
    -- ===================
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-104-DEFAULT'),
        'https://yame.vn/cdn/shop/files/0024722Thumb1.jpg?v=1760794163&width=713',
        'https://yame.vn/cdn/shop/files/0024722Thumb1.jpg?v=1760794163&width=713',
        'High-Rise Straight Jeans front view',
        1,
        TRUE
    ),
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-104-DEFAULT'),
        'https://yame.vn/cdn/shop/files/TheOriginal039.NonBranded051.TheOriginal038.jpg?v=1771835653&width=713',
        'https://yame.vn/cdn/shop/files/TheOriginal039.NonBranded051.TheOriginal038.jpg?v=1771835653&width=713',
        'High-Rise Straight Jeans model',
        2,
        FALSE
    ),

    -- ===================
    -- 105: Pleated Midi Skirt
    -- ===================
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-105-DEFAULT'),
        'https://www.mytheresa.com/image/1094/1238/100/ae/P01120769.jpg',
        'https://www.mytheresa.com/image/1094/1238/100/ae/P01120769.jpg',
        'Pleated Midi Skirt front view',
        1,
        TRUE
    ),
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-105-DEFAULT'),
        'https://www.mytheresa.com/image/1094/1238/100/ae/P01120769_b1.jpg',
        'https://www.mytheresa.com/image/1094/1238/100/ae/P01120769_b1.jpg',
        'Pleated Midi Skirt model',
        2,
        FALSE
    ),

    -- ===================
    -- 106: Fleece Pullover Hoodie
    -- ===================
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-106-DEFAULT'),
        'https://image.celine.com/8e5db438534ad53/original/RY0OL1233-07OW_1_SPR26_W_V1.jpg?im=Resize=(1200)',
        'https://image.celine.com/8e5db438534ad53/original/RY0OL1233-07OW_1_SPR26_W_V1.jpg?im=Resize=(1200)',
        'Fleece Pullover Hoodie front view',
        1,
        TRUE
    ),
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-106-DEFAULT'),
        'https://image.celine.com/2d574374293eb444/original/RY0OL1233-07OW_2_SPR26_W_V1.jpg?im=Resize=(1200)',
        'https://image.celine.com/2d574374293eb444/original/RY0OL1233-07OW_2_SPR26_W_V1.jpg?im=Resize=(1200)',
        'Fleece Pullover Hoodie back view',
        2,
        FALSE
    ),

    -- ===================
    -- 107: Bomber Jacket
    -- ===================
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-107-DEFAULT'),
        'https://image.celine.com/415a051918fc59eb/original/2V56D896C-38NO_1_WIN22.jpg?im=Resize=(1200)',
        'https://image.celine.com/415a051918fc59eb/original/2V56D896C-38NO_1_WIN22.jpg?im=Resize=(1200)',
        'Bomber Jacket front view',
        1,
        TRUE
    ),
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-107-DEFAULT'),
        'https://image.celine.com/464d2ae346813295/original/2V56D896C-38NO_2_WIN22.jpg?im=Resize=(1200)',
        'https://image.celine.com/464d2ae346813295/original/2V56D896C-38NO_2_WIN22.jpg?im=Resize=(1200)',
        'Bomber Jacket back view',
        2,
        FALSE
    ),

    -- ===================
    -- 108: Trench Coat
    -- ===================
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-108-DEFAULT'),
        'https://www.mytheresa.com/image/1094/1238/100/50/P01172334.jpg',
        'https://www.mytheresa.com/image/1094/1238/100/50/P01172334.jpg',
        'Trench Coat front view',
        1,
        TRUE
    ),
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-108-DEFAULT'),
        'https://www.mytheresa.com/image/1094/1238/100/50/P01172334_b2.jpg',
        'https://www.mytheresa.com/image/1094/1238/100/50/P01172334_b2.jpg',
        'Trench Coat back view',
        2,
        FALSE
    ),

    -- ===================
    -- 109: Performance Running Sneakers
    -- ===================
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-109-DEFAULT'),
        'https://down-vn.img.susercontent.com/file/vn-11134201-820l4-mhkoi6kcqeio77@resize_w450_nl.webp',
        'https://down-vn.img.susercontent.com/file/vn-11134201-820l4-mhkoi6kcqeio77@resize_w450_nl.webp',
        'Performance Running Sneakers front view',
        1,
        TRUE
    ),
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-109-DEFAULT'),
        'https://down-vn.img.susercontent.com/file/vn-11134201-820l4-mhkoi6ixb0g38b@resize_w450_nl.webp',
        'https://down-vn.img.susercontent.com/file/vn-11134201-820l4-mhkoi6ixb0g38b@resize_w450_nl.webp',
        'Performance Running Sneakers side view',
        2,
        FALSE
    ),

    -- ===================
    -- 110: Canvas Low-Top Sneakers
    -- ===================
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-110-DEFAULT'),
        'https://bizweb.dktcdn.net/thumb/1024x1024/100/449/458/products/alex-16-black-4190.jpg?v=1774582009967',
        'https://bizweb.dktcdn.net/thumb/1024x1024/100/449/458/products/alex-16-black-4190.jpg?v=1774582009967',
        'Canvas Low-Top Sneakers front view',
        1,
        TRUE
    ),
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-110-DEFAULT'),
        'https://bizweb.dktcdn.net/thumb/1024x1024/100/449/458/products/z7663317345139-300f0a552d175bae7a7fc8a14804dd61.jpg?v=1774595872440',
        'https://bizweb.dktcdn.net/thumb/1024x1024/100/449/458/products/z7663317345139-300f0a552d175bae7a7fc8a14804dd61.jpg?v=1774595872440',
        'Canvas Low-Top Sneakers side view',
        2,
        FALSE
    ),

    -- ===================
    -- 111: Leather Belt
    -- ===================
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-111-DEFAULT'),
        'https://babuhandmade.com/wp-content/uploads/2022/01/mau-day-lung-nam-dep-day-lung-hang-hieu-cao-cap-day-lung-da-cao-cap-mua-day-lung-nam-day-lung-da-cao-cap-day-lung-vi-da-day-lung-nam-da-bo-day-lung-hang-hieu-nam-black-belts-leather-belt-belts-1.jpg',
        'https://babuhandmade.com/wp-content/uploads/2022/01/mau-day-lung-nam-dep-day-lung-hang-hieu-cao-cap-day-lung-da-cao-cap-mua-day-lung-nam-day-lung-da-cao-cap-day-lung-vi-da-day-lung-nam-da-bo-day-lung-hang-hieu-nam-black-belts-leather-belt-belts-1.jpg',
        'Leather Belt main view',
        1,
        TRUE
    ),
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-111-DEFAULT'),
        'https://babuhandmade.com/wp-content/uploads/2022/01/mau-day-lung-nam-dep-day-lung-hang-hieu-cao-cap-day-lung-da-cao-cap-mua-day-lung-nam-day-lung-da-cao-cap-day-lung-vi-da-day-lung-nam-da-bo-day-lung-hang-hieu-nam-black-belts-leather-belt-belts-7.jpg',
        'https://babuhandmade.com/wp-content/uploads/2022/01/mau-day-lung-nam-dep-day-lung-hang-hieu-cao-cap-day-lung-da-cao-cap-mua-day-lung-nam-day-lung-da-cao-cap-day-lung-vi-da-day-lung-nam-da-bo-day-lung-hang-hieu-nam-black-belts-leather-belt-belts-7.jpg',
        'Leather Belt detail view',
        2,
        FALSE
    ),

    -- ===================
    -- 112: Baseball Cap
    -- ===================
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-112-DEFAULT'),
        'https://nhatminhsports.vn/wp-content/uploads/2026/03/Mu-golf-unisex-vai-twill-TM-x-UA-TaylorMade-TL720-Mau-Navy-1.jpg',
        'https://nhatminhsports.vn/wp-content/uploads/2026/03/Mu-golf-unisex-vai-twill-TM-x-UA-TaylorMade-TL720-Mau-Navy-1.jpg',
        'Baseball Cap front view',
        1,
        TRUE
    ),
    -- (
    --     (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-112-DEFAULT'),
    --     '',
    --     '',
    --     'Baseball Cap back/side view',
    --     2,
    --     FALSE
    -- ),

    -- ===================
    -- 113: Knit Beanie
    -- ===================
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-113-DEFAULT'),
        'https://nhatminhsports.vn/wp-content/uploads/2025/09/Mu-Len-Golf-Nam-UN921-TaylorMade-Mau-Xanh-Duong-Dam-1.jpg',
        'https://nhatminhsports.vn/wp-content/uploads/2025/09/Mu-Len-Golf-Nam-UN921-TaylorMade-Mau-Xanh-Duong-Dam-1.jpg',
        'Knit Beanie front view',
        1,
        TRUE
    ),
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-113-DEFAULT'),
        'https://nhatminhsports.vn/wp-content/uploads/2025/09/Mu-Len-Golf-Nam-UN921-TaylorMade-Mau-Xanh-Duong-Dam-2-768x768.jpg',
        'https://nhatminhsports.vn/wp-content/uploads/2025/09/Mu-Len-Golf-Nam-UN921-TaylorMade-Mau-Xanh-Duong-Dam-2-768x768.jpg',
        'Knit Beanie worn view',
        2,
        FALSE
    ),

    -- ===================
    -- 114: Yoga Leggings
    -- ===================
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-114-DEFAULT'),
        'https://nhatminhsports.vn/wp-content/uploads/2025/04/TL114-Black-01.avif',
        'https://nhatminhsports.vn/wp-content/uploads/2025/04/TL114-Black-01.avif',
        'Yoga Leggings front view',
        1,
        TRUE
    ),
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-114-DEFAULT'),
        'https://nhatminhsports.vn/wp-content/uploads/2025/04/TL114-Black-03.avif',
        'https://nhatminhsports.vn/wp-content/uploads/2025/04/TL114-Black-03.avif',
        'Yoga Leggings back view',
        2,
        FALSE
    ),

    -- ===================
    -- 115: Sports Bra
    -- ===================
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-115-DEFAULT'),
        'https://bizweb.dktcdn.net/100/369/010/products/123.jpg?v=1720495274940',
        'https://bizweb.dktcdn.net/100/369/010/products/123.jpg?v=1720495274940',
        'Sports Bra front view',
        1,
        TRUE
    ),
    -- (
    --     (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-115-DEFAULT'),
    --     '',
    --     '',
    --     'Sports Bra back view',
    --     2,
    --     FALSE
    -- ),

    -- ===================
    -- 116: Kids Hoodie
    -- ===================
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-116-DEFAULT'),
        'https://dytbw3ui6vsu6.cloudfront.net/media/catalog/product/resize/914x914/ADLV/24FW-TP-HD-KD-KTB-SBL/24FW-TP-HD-KD-KTB-SBL-002.webp?v=1772476515',
        'https://dytbw3ui6vsu6.cloudfront.net/media/catalog/product/resize/914x914/ADLV/24FW-TP-HD-KD-KTB-SBL/24FW-TP-HD-KD-KTB-SBL-002.webp?v=1772476515',
        'Kids Hoodie front view',
        1,
        TRUE
    ),
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-116-DEFAULT'),
        'https://dytbw3ui6vsu6.cloudfront.net/media/catalog/product/resize/914x914/ADLV/24FW-TP-HD-KD-KTB-SBL/24FW-TP-HD-KD-KTB-SBL-006.webp?v=1772476515',
        'https://dytbw3ui6vsu6.cloudfront.net/media/catalog/product/resize/914x914/ADLV/24FW-TP-HD-KD-KTB-SBL/24FW-TP-HD-KD-KTB-SBL-006.webp?v=1772476515',
        'Kids Hoodie back view',
        2,
        FALSE
    ),

    -- ===================
    -- 117: Kids Sneakers
    -- ===================
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-117-DEFAULT'),
        'https://product.hstatic.net/200000940675/product/4_a5528bbda3c742f6a5ad0b277a194741_1024x1024.jpg',
        'https://product.hstatic.net/200000940675/product/4_a5528bbda3c742f6a5ad0b277a194741_1024x1024.jpg',
        'Kids Sneakers front view',
        1,
        TRUE
    ),
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-117-DEFAULT'),
        'https://product.hstatic.net/200000940675/product/5_388dd2911482488d8da135e0c46425d5_1024x1024.jpg',
        'https://product.hstatic.net/200000940675/product/5_388dd2911482488d8da135e0c46425d5_1024x1024.jpg',
        'Kids Sneakers side view',
        2,
        FALSE
    ),

    -- ===================
    -- 118: Lightweight Windbreaker
    -- ===================
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-118-DEFAULT'),
        'https://nhatminhsports.vn/wp-content/uploads/2025/09/M20179_zoom_D-768x768.jpg',
        'https://nhatminhsports.vn/wp-content/uploads/2025/09/M20179_zoom_D-768x768.jpg',
        'Lightweight Windbreaker front view',
        1,
        TRUE
    ),
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-118-DEFAULT'),
        'https://nhatminhsports.vn/wp-content/uploads/2025/09/M20179_zoom_D3-768x768.jpg',
        'https://nhatminhsports.vn/wp-content/uploads/2025/09/M20179_zoom_D3-768x768.jpg',
        'Lightweight Windbreaker back view',
        2,
        FALSE
    ),

    -- ===================
    -- 119: Puffer Vest
    -- ===================
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-119-DEFAULT'),
        'https://www.mytheresa.com/image/1094/1238/100/b7/P01131632.jpg',
        'https://www.mytheresa.com/image/1094/1238/100/b7/P01131632.jpg',
        'Puffer Vest front view',
        1,
        TRUE
    ),
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-119-DEFAULT'),
        'https://www.mytheresa.com/image/1094/1238/100/b7/P01131632_b1.jpg',
        'https://www.mytheresa.com/image/1094/1238/100/b7/P01131632_b1.jpg',
        'Puffer Vest back view',
        2,
        FALSE
    ),

    -- ===================
    -- 120: Linen Short-Sleeve Shirt
    -- ===================
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-120-DEFAULT'),
        'https://www.mytheresa.com/image/1094/1238/100/07/P01001978.jpg',
        'https://www.mytheresa.com/image/1094/1238/100/07/P01001978.jpg',
        'Linen Short-Sleeve Shirt front view',
        1,
        TRUE
    ),
    (
        (SELECT sku_id FROM product_skus WHERE sku = 'CLOTH-120-DEFAULT'),
        'https://www.mytheresa.com/image/1094/1238/100/07/P01001978_d1.jpg',
        'https://www.mytheresa.com/image/1094/1238/100/07/P01001978_d1.jpg',
        'Linen Short-Sleeve Shirt back view',
        2,
        FALSE
    );