CREATE SCHEMA IF NOT EXISTS catalog;
CREATE SCHEMA IF NOT EXISTS identity;
CREATE SCHEMA IF NOT EXISTS basket;
CREATE SCHEMA IF NOT EXISTS orders;

CREATE TABLE IF NOT EXISTS catalog.catalog_brands (
    id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS catalog.catalog_types (
    id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS catalog.catalog_items (
    id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT NOT NULL,
    price NUMERIC(18, 2) NOT NULL CHECK (price >= 0),
    catalog_brand_id UUID NOT NULL REFERENCES catalog.catalog_brands(id),
    catalog_type_id UUID NOT NULL REFERENCES catalog.catalog_types(id),
    picture_uri TEXT NOT NULL,
    available_stock INTEGER NOT NULL DEFAULT 0 CHECK (available_stock >= 0),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_catalog_items_brand ON catalog.catalog_items (catalog_brand_id);
CREATE INDEX IF NOT EXISTS idx_catalog_items_type ON catalog.catalog_items (catalog_type_id);

CREATE TABLE IF NOT EXISTS identity.user_profiles (
    id UUID PRIMARY KEY,
    username VARCHAR(100) NOT NULL UNIQUE,
    email VARCHAR(256) NOT NULL UNIQUE,
    phone_number VARCHAR(50) NOT NULL DEFAULT '',
    role VARCHAR(32) NOT NULL,
    refresh_token_hash TEXT NULL,
    password_hash TEXT NOT NULL DEFAULT '',
    email_confirmed BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS basket.baskets (
    id UUID PRIMARY KEY,
    buyer_id UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS basket.basket_items (
    id UUID PRIMARY KEY,
    basket_id UUID NOT NULL REFERENCES basket.baskets(id) ON DELETE CASCADE,
    catalog_item_id UUID NOT NULL,
    product_name VARCHAR(100) NOT NULL,
    unit_price NUMERIC(18, 2) NOT NULL CHECK (unit_price >= 0),
    quantity INTEGER NOT NULL CHECK (quantity > 0),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS orders.orders (
    id UUID PRIMARY KEY,
    buyer_id UUID NOT NULL,
    order_number VARCHAR(50) NOT NULL UNIQUE,
    status VARCHAR(32) NOT NULL,
    total NUMERIC(18, 2) NOT NULL CHECK (total >= 0),
    shipping_address_line1 VARCHAR(150) NOT NULL,
    shipping_address_city VARCHAR(100) NOT NULL,
    shipping_address_state VARCHAR(100) NOT NULL,
    shipping_address_postal_code VARCHAR(25) NOT NULL,
    shipping_address_country VARCHAR(100) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS orders.order_items (
    id UUID PRIMARY KEY,
    order_id UUID NOT NULL REFERENCES orders.orders(id) ON DELETE CASCADE,
    catalog_item_id UUID NOT NULL,
    product_name VARCHAR(100) NOT NULL,
    unit_price NUMERIC(18, 2) NOT NULL CHECK (unit_price >= 0),
    units INTEGER NOT NULL CHECK (units > 0),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ============================================================================
-- SEED DATA
-- ============================================================================

-- Seed Catalog Types
INSERT INTO catalog.catalog_types (id, name, created_at, updated_at) VALUES 
    ('10000000-0000-0000-0000-000000000001'::uuid, 'Apparel', NOW(), NOW()),
    ('10000000-0000-0000-0000-000000000002'::uuid, 'Bags', NOW(), NOW()),
    ('10000000-0000-0000-0000-000000000003'::uuid, 'Accessories', NOW(), NOW())
ON CONFLICT DO NOTHING;

-- Seed Catalog Brands
INSERT INTO catalog.catalog_brands (id, name, created_at, updated_at) VALUES 
    ('20000000-0000-0000-0000-000000000001'::uuid, 'Contoso', NOW(), NOW()),
    ('20000000-0000-0000-0000-000000000002'::uuid, 'Fabrikam', NOW(), NOW()),
    ('20000000-0000-0000-0000-000000000003'::uuid, 'Adventure Works', NOW(), NOW())
ON CONFLICT DO NOTHING;

-- Seed Catalog Items
INSERT INTO catalog.catalog_items (id, name, description, price, catalog_brand_id, catalog_type_id, picture_uri, available_stock, created_at, updated_at) VALUES 
    ('30000000-0000-0000-0000-000000000001'::uuid, 'Trail Jacket', 'Water-resistant technical jacket.', 129.00, '20000000-0000-0000-0000-000000000001'::uuid, '10000000-0000-0000-0000-000000000001'::uuid, '/images/trail-jacket.png', 12, NOW(), NOW()),
    ('30000000-0000-0000-0000-000000000002'::uuid, 'Commuter Tote', 'Structured daily bag with laptop sleeve.', 89.00, '20000000-0000-0000-0000-000000000002'::uuid, '10000000-0000-0000-0000-000000000002'::uuid, '/images/commuter-tote.png', 8, NOW(), NOW()),
    ('30000000-0000-0000-0000-000000000003'::uuid, 'Summit Bottle', 'Insulated bottle for travel and hiking.', 34.50, '20000000-0000-0000-0000-000000000003'::uuid, '10000000-0000-0000-0000-000000000003'::uuid, '/images/summit-bottle.png', 21, NOW(), NOW()),
    ('30000000-0000-0000-0000-000000000004'::uuid, 'Weekend Duffel', 'Carry-on duffel for short trips.', 109.00, '20000000-0000-0000-0000-000000000002'::uuid, '10000000-0000-0000-0000-000000000002'::uuid, '/images/weekend-duffel.png', 5, NOW(), NOW()),
    ('30000000-0000-0000-0000-000000000005'::uuid, 'Base Layer Set', 'Thermal base layer for cold weather.', 75.00, '20000000-0000-0000-0000-000000000001'::uuid, '10000000-0000-0000-0000-000000000001'::uuid, '/images/base-layer.png', 17, NOW(), NOW())
ON CONFLICT DO NOTHING;

-- Seed Users (passwords are hashed using PBKDF2-SHA256 with 10000 iterations)
-- admin password: Admin123!
-- buyer password: Buyer123!
INSERT INTO identity.user_profiles (id, username, email, phone_number, role, password_hash, email_confirmed, created_at, updated_at) VALUES 
    ('40000000-0000-0000-0000-000000000001'::uuid, 'admin', 'admin@eshop.local', '555-0100', 'Admin', 'PBKDF2$10000$VNAw8pxPEDXoKxOCCJ8m9Q==$JoWLyE3dFiMn4VGHvWRNqNzHqHZmT5LGg/eW2bXvNRM=', TRUE, NOW(), NOW()),
    ('40000000-0000-0000-0000-000000000002'::uuid, 'buyer', 'buyer@eshop.local', '555-0110', 'User', 'PBKDF2$10000$bqCuVvJ3vUQYh+e9yL5U8g==$b7kJ5sK8pFd4zD3yL9qZ2V8q4mN5xH3r2wD7kL5xZ4M=', TRUE, NOW(), NOW())
ON CONFLICT DO NOTHING;
