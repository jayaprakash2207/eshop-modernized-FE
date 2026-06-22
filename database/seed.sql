INSERT INTO catalog.catalog_brands (id, name)
VALUES
    ('11111111-1111-1111-1111-111111111111', 'Contoso'),
    ('22222222-2222-2222-2222-222222222222', 'Fabrikam'),
    ('33333333-3333-3333-3333-333333333333', 'Adventure Works')
ON CONFLICT (id) DO NOTHING;

INSERT INTO catalog.catalog_types (id, name)
VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Apparel'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Bags'),
    ('cccccccc-cccc-cccc-cccc-cccccccccccc', 'Accessories')
ON CONFLICT (id) DO NOTHING;

INSERT INTO catalog.catalog_items (
    id,
    name,
    description,
    price,
    catalog_brand_id,
    catalog_type_id,
    picture_uri,
    available_stock
)
VALUES
    (
        '44444444-4444-4444-4444-444444444444',
        'Trail Jacket',
        'Water-resistant technical jacket.',
        129.00,
        '11111111-1111-1111-1111-111111111111',
        'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
        '/images/trail-jacket.png',
        12
    ),
    (
        '55555555-5555-5555-5555-555555555555',
        'Commuter Tote',
        'Structured daily bag with laptop sleeve.',
        89.00,
        '22222222-2222-2222-2222-222222222222',
        'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
        '/images/commuter-tote.png',
        8
    )
ON CONFLICT (id) DO NOTHING;

INSERT INTO identity.user_profiles (
    id,
    username,
    email,
    phone_number,
    role,
    refresh_token_hash,
    password_hash,
    email_confirmed
)
VALUES
    (
        '77777777-7777-7777-7777-777777777777',
        'admin',
        'admin@eshop.local',
        '555-0100',
        'Admin',
        NULL,
        'Admin123!',
        TRUE
    ),
    (
        '88888888-8888-8888-8888-888888888888',
        'buyer',
        'buyer@eshop.local',
        '555-0110',
        'User',
        NULL,
        'Buyer123!',
        TRUE
    )
ON CONFLICT (id) DO NOTHING;
