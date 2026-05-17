INSERT INTO product_categories (
        product_id,
        category_id,
        is_primary
    )
SELECT p.product_id,
    c.category_id,
    mapping.is_primary
FROM (
        VALUES ('striped-knit-polo-shirt', 'fashion', FALSE),
            ('studded-cotton-crew-tee', 'fashion', FALSE),
            (
                'baggy-tokyo-racing-graphic-jersey',
                'fashion',
                FALSE
            ),
            (
                'boxy-heavyweight-graphic-cutoff-tank',
                'fashion',
                FALSE
            ),
            (
                'evolution-straight-leg-sweatpants',
                'fashion',
                FALSE
            ),
            ('slim-taper-chino-pants', 'fashion', FALSE),
            ('baggy-utility-pants', 'fashion', FALSE),
            ('baggy-jeans-bag-secured', 'fashion', FALSE),
            ('cotton-workwear-jacket', 'fashion', FALSE),
            (
                'loose-fit-scuba-baseball-jacket',
                'fashion',
                FALSE
            ),
            ('vaporfly-4', 'fashion', FALSE),
            ('giannis-freak-7-ep', 'fashion', FALSE),
            ('dri-fit-club-cap', 'fashion', FALSE),
            ('jersey-knit-puff-sleeve-top', 'fashion', FALSE),
            ('embroidered-bow-cropped-tee', 'fashion', FALSE),
            ('open-knit-crochet-sweater', 'fashion', FALSE),
            ('tube-mesh-midi-dress', 'fashion', FALSE),
            (
                'eyelet-bustier-flounce-mini-dress',
                'fashion',
                FALSE
            ),
            ('adizero-adios-pro-4', 'fashion', FALSE),
            ('superstar-ii', 'fashion', FALSE),
            (
                'adicolor-classic-trefoil-baseball-cap',
                'fashion',
                FALSE
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
            ('striped-knit-polo-shirt', 'tops', TRUE),
            ('studded-cotton-crew-tee', 'tops', TRUE),
            (
                'baggy-tokyo-racing-graphic-jersey',
                'tops',
                TRUE
            ),
            (
                'boxy-heavyweight-graphic-cutoff-tank',
                'tops',
                TRUE
            ),
            ('jersey-knit-puff-sleeve-top', 'tops', TRUE),
            ('embroidered-bow-cropped-tee', 'tops', TRUE),
            ('open-knit-crochet-sweater', 'tops', TRUE),
            ('tube-mesh-midi-dress', 'tops', TRUE),
            (
                'eyelet-bustier-flounce-mini-dress',
                'tops',
                TRUE
            ),
            (
                'evolution-straight-leg-sweatpants',
                'bottoms',
                TRUE
            ),
            ('slim-taper-chino-pants', 'bottoms', TRUE),
            ('baggy-utility-pants', 'bottoms', TRUE),
            ('baggy-jeans-bag-secured', 'bottoms', TRUE),
            ('cotton-workwear-jacket', 'outerwear', TRUE),
            (
                'loose-fit-scuba-baseball-jacket',
                'outerwear',
                TRUE
            ),
            ('vaporfly-4', 'footwear', TRUE),
            ('giannis-freak-7-ep', 'footwear', TRUE),
            ('adizero-adios-pro-4', 'footwear', TRUE),
            ('superstar-ii', 'footwear', TRUE),
            ('dri-fit-club-cap', 'accessories', TRUE),
            (
                'adicolor-classic-trefoil-baseball-cap',
                'accessories',
                TRUE
            ),
            (
                'noveau-signature-patch-varsity-jacket',
                'fashion',
                FALSE
            ),
            (
                'noveau-signature-patch-varsity-jacket',
                'women',
                FALSE
            ),
            (
                'noveau-signature-patch-varsity-jacket',
                'outerwear',
                TRUE
            ),
            ('pokemon-ut-graphic-t-shirt', 'fashion', FALSE),
            ('pokemon-ut-graphic-t-shirt', 'men', FALSE),
            ('pokemon-ut-graphic-t-shirt', 'women', FALSE),
            ('pokemon-ut-graphic-t-shirt', 'tops', TRUE),
            (
                'pokemon-ut-graphic-t-shirt',
                'pokemon-collab',
                FALSE
            ),
            ('pokemon-ut-graphic-t-shirt-1', 'fashion', FALSE),
            ('pokemon-ut-graphic-t-shirt-1', 'men', FALSE),
            ('pokemon-ut-graphic-t-shirt-1', 'women', FALSE),
            ('pokemon-ut-graphic-t-shirt-1', 'tops', TRUE),
            (
                'pokemon-ut-graphic-t-shirt-1',
                'pokemon-collab',
                FALSE
            ),
            ('pokemon-ut-graphic-t-shirt-2', 'fashion', FALSE),
            ('pokemon-ut-graphic-t-shirt-2', 'men', FALSE),
            ('pokemon-ut-graphic-t-shirt-2', 'women', FALSE),
            ('pokemon-ut-graphic-t-shirt-2', 'tops', TRUE),
            (
                'pokemon-ut-graphic-t-shirt-2',
                'pokemon-collab',
                FALSE
            )
    ) AS mapping(product_slug, category_slug, is_primary)
    JOIN products p ON p.slug = mapping.product_slug
    JOIN categories c ON c.slug = mapping.category_slug;