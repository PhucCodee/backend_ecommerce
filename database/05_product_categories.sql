INSERT INTO product_categories (
        product_id,
        category_id,
        is_primary
    )
SELECT p.product_id,
    c.category_id,
    mapping.is_primary
FROM (
        VALUES ('striped-knit-polo-shirt', 'fashion', TRUE),
            ('studded-cotton-crew-tee', 'fashion', TRUE),
            (
                'baggy-tokyo-racing-graphic-jersey',
                'fashion',
                TRUE
            ),
            (
                'boxy-heavyweight-graphic-cutoff-tank',
                'fashion',
                TRUE
            ),
            (
                'evolution-straight-leg-sweatpants',
                'fashion',
                TRUE
            ),
            ('slim-taper-chino-pants', 'fashion', TRUE),
            ('baggy-utility-pants', 'fashion', TRUE),
            ('baggy-jeans-bag-secured', 'fashion', TRUE),
            ('cotton-workwear-jacket', 'fashion', TRUE),
            (
                'loose-fit-scuba-baseball-jacket',
                'fashion',
                TRUE
            ),
            ('vaporfly-4', 'fashion', TRUE),
            ('giannis-freak-7-ep', 'fashion', TRUE),
            ('dri-fit-club-cap', 'fashion', TRUE),
            ('jersey-knit-puff-sleeve-top', 'fashion', TRUE),
            ('embroidered-bow-cropped-tee', 'fashion', TRUE),
            ('open-knit-crochet-sweater', 'fashion', TRUE),
            ('tube-mesh-midi-dress', 'fashion', TRUE),
            (
                'eyelet-bustier-flounce-mini-dress',
                'fashion',
                TRUE
            ),
            ('adizero-adios-pro-4', 'fashion', TRUE),
            ('superstar-ii', 'fashion', TRUE),
            (
                'adicolor-classic-trefoil-baseball-cap',
                'fashion',
                TRUE
            ),
            ('striped-knit-polo-shirt', 'men', FALSE),
            ('studded-cotton-crew-tee', 'men', FALSE),
            (
                'baggy-tokyo-racing-graphic-jersey',
                'men',
                FALSE
            ),
            (
                'boxy-heavyweight-graphic-cutoff-tank',
                'men',
                FALSE
            ),
            (
                'evolution-straight-leg-sweatpants',
                'men',
                FALSE
            ),
            ('slim-taper-chino-pants', 'men', FALSE),
            ('baggy-utility-pants', 'men', FALSE),
            ('baggy-jeans-bag-secured', 'men', FALSE),
            ('cotton-workwear-jacket', 'men', FALSE),
            ('loose-fit-scuba-baseball-jacket', 'men', FALSE),
            ('vaporfly-4', 'men', FALSE),
            ('giannis-freak-7-ep', 'men', FALSE),
            ('dri-fit-club-cap', 'men', FALSE),
            ('jersey-knit-puff-sleeve-top', 'women', FALSE),
            ('embroidered-bow-cropped-tee', 'women', FALSE),
            ('open-knit-crochet-sweater', 'women', FALSE),
            ('tube-mesh-midi-dress', 'women', FALSE),
            (
                'eyelet-bustier-flounce-mini-dress',
                'women',
                FALSE
            ),
            ('adizero-adios-pro-4', 'women', FALSE),
            ('superstar-ii', 'women', FALSE),
            (
                'adicolor-classic-trefoil-baseball-cap',
                'women',
                FALSE
            ),
            ('striped-knit-polo-shirt', 'tops', FALSE),
            ('studded-cotton-crew-tee', 'tops', FALSE),
            (
                'baggy-tokyo-racing-graphic-jersey',
                'tops',
                FALSE
            ),
            (
                'boxy-heavyweight-graphic-cutoff-tank',
                'tops',
                FALSE
            ),
            ('jersey-knit-puff-sleeve-top', 'tops', FALSE),
            ('embroidered-bow-cropped-tee', 'tops', FALSE),
            ('open-knit-crochet-sweater', 'tops', FALSE),
            ('tube-mesh-midi-dress', 'tops', FALSE),
            (
                'eyelet-bustier-flounce-mini-dress',
                'tops',
                FALSE
            ),
            (
                'evolution-straight-leg-sweatpants',
                'bottoms',
                FALSE
            ),
            ('slim-taper-chino-pants', 'bottoms', FALSE),
            ('baggy-utility-pants', 'bottoms', FALSE),
            ('baggy-jeans-bag-secured', 'bottoms', FALSE),
            ('cotton-workwear-jacket', 'outerwear', FALSE),
            (
                'loose-fit-scuba-baseball-jacket',
                'outerwear',
                FALSE
            ),
            ('vaporfly-4', 'footwear', FALSE),
            ('giannis-freak-7-ep', 'footwear', FALSE),
            ('adizero-adios-pro-4', 'footwear', FALSE),
            ('superstar-ii', 'footwear', FALSE),
            ('dri-fit-club-cap', 'accessories', FALSE),
            (
                'adicolor-classic-trefoil-baseball-cap',
                'accessories',
                FALSE
            )
    ) AS mapping(product_slug, category_slug, is_primary)
    JOIN products p ON p.slug = mapping.product_slug
    JOIN categories c ON c.slug = mapping.category_slug;