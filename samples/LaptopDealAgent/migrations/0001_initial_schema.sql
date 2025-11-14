-- Laptop Deal Agent Database Schema
-- Migration: 0001_initial_schema.sql

-- Models table: Store the laptop models we're tracking
CREATE TABLE IF NOT EXISTS models (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    model_number TEXT UNIQUE NOT NULL,
    ram_size INTEGER NOT NULL,
    full_name TEXT NOT NULL,
    max_price DECIMAL(10,2) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Retailers table: Store reputable retailers
CREATE TABLE IF NOT EXISTS retailers (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT UNIQUE NOT NULL,
    domain TEXT NOT NULL,
    is_reputable BOOLEAN DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Deals table: Main table for tracking deals
CREATE TABLE IF NOT EXISTS deals (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    model_id INTEGER NOT NULL,
    retailer_id INTEGER NOT NULL,
    url TEXT NOT NULL,
    price DECIMAL(10,2),
    quantity INTEGER,
    stock_text TEXT,
    condition TEXT,
    in_stock BOOLEAN DEFAULT 1,
    is_black_friday_deal BOOLEAN DEFAULT 0,
    discount_percentage REAL,
    original_price DECIMAL(10,2),
    title TEXT,
    first_seen TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_seen TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (model_id) REFERENCES models(id),
    FOREIGN KEY (retailer_id) REFERENCES retailers(id),
    UNIQUE(url, model_id)  -- Prevent duplicate URLs per model
);

-- Price history table: Track all price changes over time
CREATE TABLE IF NOT EXISTS price_history (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    deal_id INTEGER NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    quantity INTEGER,
    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (deal_id) REFERENCES deals(id)
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS idx_deals_model ON deals(model_id);
CREATE INDEX IF NOT EXISTS idx_deals_last_seen ON deals(last_seen DESC);
CREATE INDEX IF NOT EXISTS idx_deals_price ON deals(price);
CREATE INDEX IF NOT EXISTS idx_deals_in_stock ON deals(in_stock);
CREATE INDEX IF NOT EXISTS idx_price_history_timestamp ON price_history(timestamp DESC);
CREATE INDEX IF NOT EXISTS idx_price_history_deal ON price_history(deal_id);

-- Insert our target models
INSERT OR IGNORE INTO models (model_number, ram_size, full_name, max_price) VALUES
    ('GZ302EA-R9641TB', 64, 'ASUS ROG Flow Z13 (2025) GZ302 GZ302EA-R9641TB 64GB RAM', 2000.00),
    ('GZ302EA-XS99', 128, 'ASUS ROG Flow Z13 (2025) GZ302 GZ302EA-XS99 128GB RAM', 2300.00);

-- Insert reputable retailers
INSERT OR IGNORE INTO retailers (name, domain, is_reputable) VALUES
    ('Best Buy', 'bestbuy.com', 1),
    ('Amazon', 'amazon.com', 1),
    ('Newegg', 'newegg.com', 1),
    ('Micro Center', 'microcenter.com', 1),
    ('B&H Photo', 'bhphotovideo.com', 1),
    ('ASUS Store', 'store.asus.com', 1),
    ('Microsoft Store', 'microsoft.com', 1),
    ('Walmart', 'walmart.com', 1),
    ('Target', 'target.com', 1),
    ('Costco', 'costco.com', 1);
