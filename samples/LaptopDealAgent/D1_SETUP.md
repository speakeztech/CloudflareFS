# D1 Database Setup Guide

This guide explains how to set up and use Cloudflare D1 for the Laptop Deal Agent.

## Why D1 Instead of KV?

**D1 Advantages:**
- ✅ **Relational queries**: JOIN across deals, models, retailers
- ✅ **Time-series analysis**: Query price history by date ranges
- ✅ **Aggregations**: MIN/MAX/AVG price calculations
- ✅ **Indexing**: Fast lookups on model, retailer, price, dates
- ✅ **Data integrity**: Foreign keys, constraints, transactions
- ✅ **Efficient filtering**: Dashboard can query exactly what it needs
- ✅ **Proper types**: DECIMAL for prices, TIMESTAMP for dates

**KV Limitations for this use case:**
- ❌ No querying capability (only get/put by key)
- ❌ Difficult to filter and aggregate
- ❌ No relationships between data
- ❌ Client-side filtering required for dashboard
- ❌ Awkward key naming for historical data

## Setup Instructions

### 1. Create D1 Database

```bash
cd samples/LaptopDealAgent

# Create the D1 database
wrangler d1 create laptop-deals
```

This will output something like:
```
✅ Successfully created DB 'laptop-deals'!

[[d1_databases]]
binding = "DB"
database_name = "laptop-deals"
database_id = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
```

### 2. Update wrangler.toml

Copy the `database_id` from the output above and update `wrangler.toml`:

```toml
[[d1_databases]]
binding = "DB"
database_name = "laptop-deals"
database_id = "your-actual-database-id-here"  # Replace with the ID from step 1
```

### 3. Run Initial Migration

Apply the database schema:

```bash
# Local development
wrangler d1 execute laptop-deals --local --file=./migrations/0001_initial_schema.sql

# Production
wrangler d1 execute laptop-deals --file=./migrations/0001_initial_schema.sql
```

### 4. Verify Schema

Check that tables were created:

```bash
# Local
wrangler d1 execute laptop-deals --local --command="SELECT name FROM sqlite_master WHERE type='table'"

# Production
wrangler d1 execute laptop-deals --command="SELECT name FROM sqlite_master WHERE type='table'"
```

You should see:
- `models`
- `retailers`
- `deals`
- `price_history`

### 5. Verify Seed Data

Check that models and retailers were inserted:

```bash
# Check models
wrangler d1 execute laptop-deals --local --command="SELECT * FROM models"

# Check retailers
wrangler d1 execute laptop-deals --local --command="SELECT * FROM retailers"
```

## Database Schema

### Tables

**models** - Laptop models we're tracking
- `id` - Primary key
- `model_number` - Unique model identifier (e.g., "GZ302EA-R9641TB")
- `ram_size` - RAM in GB (64 or 128)
- `full_name` - Full product name
- `max_price` - Price threshold ($2000 or $2300)

**retailers** - Reputable retailers
- `id` - Primary key
- `name` - Retailer name (e.g., "Best Buy")
- `domain` - Domain (e.g., "bestbuy.com")
- `is_reputable` - Boolean flag

**deals** - Current and historical deals
- `id` - Primary key
- `model_id` - Foreign key to models
- `retailer_id` - Foreign key to retailers
- `url` - Product URL (unique per model)
- `price`, `quantity`, `stock_text`, `condition`, `title` - Product details
- `in_stock`, `is_black_friday_deal` - Flags
- `discount_percentage`, `original_price` - Discount info
- `first_seen`, `last_seen`, `last_updated` - Timestamps

**price_history** - Historical price/quantity changes
- `id` - Primary key
- `deal_id` - Foreign key to deals
- `price` - Price at this point in time
- `quantity` - Stock quantity (if available)
- `timestamp` - When this change occurred

## Development Workflow

### Local Testing

```bash
# Start local dev server with D1
wrangler dev --local

# Test endpoints
curl http://localhost:8787/api/deals
curl http://localhost:8787/api/analysis
curl http://localhost:8787/api/history
```

### Querying Data

```bash
# Get all deals
wrangler d1 execute laptop-deals --local --command="
  SELECT d.url, d.price, d.quantity, m.model_number, r.name
  FROM deals d
  JOIN models m ON d.model_id = m.id
  JOIN retailers r ON d.retailer_id = r.id
  WHERE d.in_stock = 1
  ORDER BY d.last_seen DESC
"

# Get price history for a specific deal
wrangler d1 execute laptop-deals --local --command="
  SELECT ph.price, ph.quantity, ph.timestamp
  FROM price_history ph
  JOIN deals d ON ph.deal_id = d.id
  WHERE d.url = 'https://example.com/product'
  ORDER BY ph.timestamp DESC
  LIMIT 10
"

# Get best price per model
wrangler d1 execute laptop-deals --local --command="
  SELECT m.model_number, MIN(d.price) as best_price
  FROM deals d
  JOIN models m ON d.model_id = m.id
  WHERE d.in_stock = 1 AND d.price IS NOT NULL
  GROUP BY m.model_number
"
```

### Resetting Data

```bash
# Drop all tables (careful!)
wrangler d1 execute laptop-deals --local --command="
  DROP TABLE IF EXISTS price_history;
  DROP TABLE IF EXISTS deals;
  DROP TABLE IF EXISTS retailers;
  DROP TABLE IF EXISTS models;
"

# Re-run migration
wrangler d1 execute laptop-deals --local --file=./migrations/0001_initial_schema.sql
```

## API Endpoints

All endpoints now query D1 directly:

### GET /api/deals
Returns all current in-stock deals:
```json
{
  "deals": [
    {
      "Model": "GZ302EA-R9641TB",
      "Price": 1899.99,
      "Quantity": 3,
      "Retailer": "Best Buy",
      "Url": "https://...",
      "InStock": true,
      "IsBlackFridayDeal": true,
      ...
    }
  ],
  "lastUpdated": "2024-11-14T12:00:00Z",
  "totalDeals": 5
}
```

### GET /api/analysis
Returns price analysis from D1:
```json
{
  "analyses": [
    {
      "Model": { "ModelNumber": "GZ302EA-R9641TB", ... },
      "CurrentBestPrice": 1899.99,
      "BestRetailer": "Best Buy",
      "AveragePrice": 1950.00,
      "LowestHistoricalPrice": 1899.99,
      "Recommendation": "Excellent deal! Consider purchasing."
    }
  ],
  "generatedAt": "2024-11-14T12:00:00Z"
}
```

### GET /api/history
Returns deals by model:
```json
{
  "GZ302EA_R9641TB": [ /* deals array */ ],
  "GZ302EA_XS99": [ /* deals array */ ]
}
```

## Data Flow

1. **Scheduled Task Runs** (hourly cron)
   - Orchestrator spawns SearchActors
   - Actors find deals via AI analysis
   - Valid deals returned to orchestrator

2. **Upsert to D1**
   - For each deal, call `D1Storage.upsertDeal`
   - Looks up model_id and retailer_id
   - Inserts new deal OR updates existing (by URL)
   - If price/quantity changed, adds entry to price_history

3. **Dashboard Queries**
   - React SPA calls `/api/deals`
   - Worker queries D1 with JOINs
   - Returns structured JSON

## Storage Architecture

**Durable Object Storage** (for actor state):
- SearchActor's `TrackedUrls` map (idempotency checking)
- NotificationManager's notification history
- Transient, per-actor state

**D1 Database** (for persistent historical data):
- All deals (current and past)
- Complete price/quantity history
- Model and retailer metadata
- Queryable, relational, indexed

## Performance Considerations

- D1 has a limit of 100,000 reads/day on free tier
- Each dashboard page load = ~3 queries (deals, analysis)
- Hourly cron = ~24 runs/day with ~10 writes each
- Well within free tier limits for this use case

## Troubleshooting

### "Database not found"
- Make sure you created the database: `wrangler d1 create laptop-deals`
- Verify `database_id` in wrangler.toml matches

### "Table doesn't exist"
- Run the migration: `wrangler d1 execute laptop-deals --local --file=./migrations/0001_initial_schema.sql`

### "No deals returned"
- Check if data exists: `wrangler d1 execute laptop-deals --local --command="SELECT COUNT(*) FROM deals"`
- Trigger a search: `curl -X POST http://localhost:8787/api/trigger`

### Local vs Production
- Always test with `--local` flag first
- Deploy schema to production separately: `wrangler d1 execute laptop-deals --file=...` (no --local flag)
- Production and local are separate databases

## Benefits Realized

✅ **Dashboard is now fast**: Direct SQL queries instead of fetching all data and filtering client-side

✅ **Price history is queryable**: Can easily show charts of price changes over time

✅ **Data integrity**: Foreign keys ensure valid model/retailer references

✅ **Scalable**: Indexes make queries fast even with thousands of deals

✅ **Analytics ready**: Can add complex aggregations and trend analysis

✅ **Cost efficient**: D1 free tier is generous for this use case
