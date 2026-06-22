-- Create loyalty schema and tables
CREATE SCHEMA IF NOT EXISTS loyalty;

CREATE TABLE IF NOT EXISTS loyalty.membership_tiers (
    id uuid PRIMARY KEY,
    name varchar(32) NOT NULL,
    min_points bigint NOT NULL DEFAULT 0,
    max_points bigint NULL,
    created_at timestamptz NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS loyalty.loyalty_accounts (
    id uuid PRIMARY KEY,
    user_id uuid NOT NULL,
    points_balance bigint NOT NULL DEFAULT 0,
    tier_id uuid NULL,
    created_at timestamptz NOT NULL DEFAULT NOW(),
    updated_at timestamptz NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_user FOREIGN KEY(user_id) REFERENCES identity.user_profiles(id)
);
CREATE UNIQUE INDEX IF NOT EXISTS idx_loyalty_accounts_user_id ON loyalty.loyalty_accounts(user_id);

CREATE TABLE IF NOT EXISTS loyalty.loyalty_transactions (
    id uuid PRIMARY KEY,
    account_id uuid NOT NULL,
    type varchar(20) NOT NULL,
    points bigint NOT NULL,
    order_id uuid NULL,
    source_event_id varchar(128) NULL,
    created_at timestamptz NOT NULL DEFAULT NOW(),
    expires_at timestamptz NULL,
    CONSTRAINT fk_account FOREIGN KEY(account_id) REFERENCES loyalty.loyalty_accounts(id)
);
CREATE INDEX IF NOT EXISTS idx_loyalty_transactions_account_id ON loyalty.loyalty_transactions(account_id);
CREATE UNIQUE INDEX IF NOT EXISTS idx_loyalty_transactions_source_event ON loyalty.loyalty_transactions(source_event_id) WHERE source_event_id IS NOT NULL;

CREATE TABLE IF NOT EXISTS loyalty.reward_rules (
    id uuid PRIMARY KEY,
    name varchar(128) NOT NULL,
    earn_multiplier numeric(5,2) NOT NULL DEFAULT 1.0,
    min_order_total numeric(12,2) NOT NULL DEFAULT 0.00,
    valid_from timestamptz NULL,
    valid_to timestamptz NULL,
    is_active boolean NOT NULL DEFAULT true,
    created_at timestamptz NOT NULL DEFAULT NOW()
);

-- Seed membership tiers
INSERT INTO loyalty.membership_tiers (id, name, min_points, max_points, created_at)
VALUES
('a0000000-0000-0000-0000-000000000001'::uuid, 'Bronze', 0, 999, NOW()),
('a0000000-0000-0000-0000-000000000002'::uuid, 'Silver', 1000, 4999, NOW()),
('a0000000-0000-0000-0000-000000000003'::uuid, 'Gold', 5000, NULL, NOW())
ON CONFLICT DO NOTHING;

-- Default reward rule
INSERT INTO loyalty.reward_rules (id, name, earn_multiplier, min_order_total, is_active, created_at)
VALUES ('b0000000-0000-0000-0000-000000000001'::uuid, 'Default Earn Rule', 1.0, 0.00, true, NOW())
ON CONFLICT DO NOTHING;
