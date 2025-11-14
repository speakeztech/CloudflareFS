# Laptop Deal Agent - ASUS ROG Flow Z13 Black Friday Tracker

An intelligent Cloudflare Worker built with F# and CloudflareFS that automatically searches for Black Friday deals on specific ASUS ROG Flow Z13 laptop models every hour.

## Overview

This project demonstrates an **AI-powered parallel actor system** built with F# and Fable that:

- 🤖 **AI-Powered Analysis** - Uses Cloudflare AI (Llama-3-8B) to intelligently extract product details from web pages
- ⚡ **Parallel Actor System** - Durable Objects act as independent search agents, one per laptop model
- 🔄 **Idempotent Processing** - Only processes new URLs, price drops, or stock drops (ignoring duplicates)
- 💰 **Smart Price Filtering** - Enforces thresholds ($2,000 for 64GB, $2,300 for 128GB)
- ✅ **Strict Validation** - Verifies model numbers, condition (new/refurbished), and retailer reputation
- 📦 **Quantity Tracking** - Extracts stock availability ("Only 3 left") from listings
- 📊 **Deep Page Scraping** - Fetches and analyzes individual product pages, not just search results
- 🏪 **Reputable Retailers Only** - Validates against whitelist (Best Buy, Amazon, Newegg, etc.)
- 📱 **React Dashboard** - Beautiful DaisyUI v4 interface to view all deals
- 🔔 **Pushover Notifications** - Instant phone alerts linking to dashboard

## Target Laptops

The agent specifically tracks two variants of the ASUS ROG Flow Z13 (2025):

1. **GZ302EA-R9641TB** - 64GB RAM variant
   - Full specs: 13.4" 2.5K 180Hz Touch-Screen
   - Processor: AMD Ryzen AI Max+ 395
   - RAM: 64GB

2. **GZ302EA-XS99** - 128GB RAM variant
   - Full specs: 13.4" 2.5K 180Hz Touch-Screen
   - Processor: AMD Ryzen AI Max+ 395
   - RAM: 128GB

The agent uses sophisticated validation to ensure it only tracks these exact models, not similar ASUS ROG laptops.

## Architecture

### 🎯 AI-Powered Actor System

**See [AI_ACTOR_SYSTEM.md](AI_ACTOR_SYSTEM.md) for comprehensive architecture documentation.**

Quick overview:

```
Orchestrator (Main Worker)
    │
    ├─→ SearchActor (64GB) ──→ AI Analyzer ──→ Valid Deals
    │   • Idempotent state                      │
    │   • Price tracking                        │
    │   • URL deduplication                     │
    │                                           ↓
    └─→ SearchActor (128GB) ──→ AI Analyzer ──→ KV Storage
        • Independent execution                  │
        • Parallel processing                    │
        • Isolated failures                      ↓
                                            React Dashboard
                                                 │
                                                 ↓
                                          Pushover Notifications
```

### Core Components

1. **AIAnalyzer.fs** - Cloudflare AI-powered page analysis
   - Deep page scraping (fetches actual HTML)
   - Intelligent product extraction using Llama-3-8B
   - Validates model, condition, price, retailer, quantity
   - Returns structured PriceInfo or rejects invalid listings

2. **SearchActor.fs** - Durable Object actor (one per model)
   - Maintains idempotent state (tracked URLs + prices + quantities)
   - Only processes new URLs, price drops, or stock drops
   - Coordinates parallel searches
   - Persists state across worker invocations with price and quantity history

3. **SearchOrchestrator.fs** - Parallel coordination
   - Spawns actors for each model
   - Executes searches in parallel
   - Aggregates results from all actors

4. **Types.fs** - Enhanced data structures
   - Added: `Condition`, `Quantity`, `StockText`, `Title`
   - New: `TrackedUrl` with price and quantity history for idempotency
   - New: `SearchActorState` for maintaining actor state

5. **PriceAnalyzer.fs** - Price analysis and reporting
   - Price trend analysis
   - Historical comparison
   - Purchase recommendations
   - HTML report generation

6. **NotificationManager.fs** - Durable Object for smart notifications
   - Deduplication
   - Rate limiting
   - Multi-channel support (Pushover, Telegram, Discord, SMS)

### Price Thresholds

| Model | RAM | Max Price | Enforced By |
|-------|-----|-----------|-------------|
| GZ302EA-R9641TB | 64GB | **$2,000** | AI Analyzer |
| GZ302EA-XS99 | 128GB | **$2,300** | AI Analyzer |

### Validation Rules

✅ **Must Pass (or rejected):**
- Exact model number match
- New OR Certified Refurbished condition
- Price under threshold
- Reputable retailer (Best Buy, Amazon, Newegg, B&H, ASUS, etc.)
- In stock or available

📦 **Extracted (if available):**
- Quantity (e.g., "Only 3 left in stock")
- Stock text
- Discount percentage
- Original price

## Prerequisites

- .NET 8.0 or later
- Node.js 18+
- Wrangler CLI: `npm install -g wrangler`
- Cloudflare account with:
  - Workers enabled
  - D1 database created
  - AI binding configured
  - Durable Objects enabled

## Setup

### 1. Create D1 Database

```bash
cd samples/LaptopDealAgent

# Create D1 database
wrangler d1 create laptop-deals

# Note the database ID returned and update wrangler.toml
```

See [D1_SETUP.md](D1_SETUP.md) for complete database setup instructions.

### 2. Run Database Migration

```bash
# Apply schema locally for development
wrangler d1 execute laptop-deals --local --file=./migrations/0001_initial_schema.sql

# Apply schema to production
wrangler d1 execute laptop-deals --file=./migrations/0001_initial_schema.sql
```

### 3. Update Configuration

Edit `wrangler.toml` and update the D1 database ID:

```toml
[[d1_databases]]
binding = "DB"
database_name = "laptop-deals"
database_id = "your-d1-database-id"  # Replace with ID from step 1
```

### 4. Install Dependencies

```bash
cd samples/LaptopDealAgent
npm install
```

### 4. Build the Project

```bash
# Build F# to JavaScript using Fable
npm run build
```

This compiles the F# code to JavaScript in the `dist/` directory.

## Development

### Local Testing

```bash
# Run locally with Wrangler
npm run dev
```

This starts a local development server. You can access:

- `http://localhost:8787/` - Run the agent and view the report
- `http://localhost:8787/status` - Check agent status
- `http://localhost:8787/history` - View price history
- `http://localhost:8787/trigger` - Manually trigger a search (POST)

### Manual Trigger

```bash
# Trigger a manual search
curl -X POST http://localhost:8787/trigger
```

## Deployment

### Deploy to Cloudflare

```bash
npm run deploy
```

### Verify Scheduled Execution

After deployment, the worker will automatically run every hour based on the cron schedule in `wrangler.toml`:

```toml
[triggers]
crons = ["0 * * * *"]  # Every hour at minute 0
```

You can view execution logs in the Cloudflare dashboard or via Wrangler:

```bash
wrangler tail laptop-deal-agent
```

## Usage

### Viewing Reports

Navigate to your deployed worker URL (e.g., `https://laptop-deal-agent.your-subdomain.workers.dev/`) to see:

- Current best prices for both laptop models
- Price trends (increasing/decreasing/stable)
- Historical pricing data
- Purchase recommendations

### API Endpoints

#### `GET /`
Returns an HTML report with current prices and analysis.

#### `GET /status`
Returns JSON status information:
```json
{
  "status": "running",
  "timestamp": "2025-11-14T12:00:00Z",
  "models": [
    "ASUS ROG Flow Z13 (2025) GZ302 GZ302EA-R9641TB 64GB RAM",
    "ASUS ROG Flow Z13 (2025) GZ302 GZ302EA-XS99 128GB RAM"
  ]
}
```

#### `GET /history`
Returns JSON with recent price history for both models.

#### `POST /trigger`
Manually triggers a search and analysis.

## Features

### 📱 Phone Notifications (NEW!)

The agent includes a sophisticated notification system powered by **Cloudflare Durable Objects**:

- **Smart Deduplication** - Never get spammed with the same deal twice
- **Rate Limiting** - Control notification frequency (daily limits, minimum hours between)
- **Intelligent Filtering** - Only notify for significant deals (price thresholds, historical lows)
- **Multiple Channels** - Pushover, Telegram, Discord, SMS, and more
- **Stateful Singleton** - Durable Object manages notification state persistently

**Recommended**: Use [Pushover](https://pushover.net/) for instant, reliable phone notifications ($5 one-time purchase).

See [NOTIFICATIONS.md](NOTIFICATIONS.md) for complete setup guide and [EXAMPLE_INTEGRATION.md](EXAMPLE_INTEGRATION.md) for code examples.

### Intelligent Model Validation

The agent uses multiple validation techniques to ensure accuracy:

- Model number matching (GZ302EA-R9641TB, GZ302EA-XS99)
- RAM size verification (64GB, 128GB)
- Product family validation (ROG Flow Z13)
- Year verification (2025)
- Processor validation (AMD Ryzen AI Max+ 395)

### Price Extraction

Supports multiple price formats:
- `$1,299.99`
- `USD 1299`
- `€1.299,99`
- `£1,299.99`

### Black Friday Detection

Identifies deals through keywords:
- "Black Friday"
- "Cyber Monday"
- "Holiday Deal"
- "Special Offer"
- Discount percentages (e.g., "25% off")

### Stock Availability

Filters out items that are:
- Out of stock
- Sold out
- Unavailable
- Pre-order only

### Supported Retailers

Auto-detects major retailers:
- Best Buy
- Amazon
- Newegg
- Micro Center
- ASUS Official Store
- B&H Photo
- And others

## Customization

### Change Search Frequency

Edit the cron schedule in `wrangler.toml`:

```toml
[triggers]
crons = ["0 */2 * * *"]  # Every 2 hours
# or
crons = ["0 0 * * *"]    # Once daily at midnight
```

### Add More Search Keywords

Edit `LaptopDealAgent.fs`:

```fsharp
let config = {
    SearchKeywords = [
        "ASUS ROG Flow Z13 2025 GZ302EA-R9641TB 64GB Black Friday"
        "Your additional keyword here"
        // ...
    ]
    MaxSearchResults = 20
    MinPriceConfidence = 0.7
    EnableNotifications = true
}
```

### Extend to Other Products

1. Add new models to `Types.fs` `LaptopModel` type
2. Update validation logic in `SearchAgent.fs`
3. Add search keywords in `LaptopDealAgent.fs`

## Project Structure

```
LaptopDealAgent/
├── LaptopDealAgent.fsproj    # F# project file
├── package.json              # NPM configuration
├── wrangler.toml             # Cloudflare Worker config
├── Types.fs                  # Data structures
├── SearchAgent.fs            # Web search and scraping
├── PriceAnalyzer.fs          # Price analysis
├── LaptopDealAgent.fs        # Main worker
├── README.md                 # This file
└── dist/                     # Compiled JavaScript (generated)
    └── LaptopDealAgent.js
```

## Monitoring

### View Logs

```bash
# Stream live logs
wrangler tail laptop-deal-agent

# View specific execution
wrangler tail laptop-deal-agent --format=json
```

### Check Price History

Visit `/history` endpoint to see historical data stored in KV.

## Troubleshooting

### Build Errors

If you encounter build errors:

```bash
# Clean and rebuild
rm -rf dist/
dotnet clean
dotnet build
npm run build
```

### KV Namespace Issues

Ensure your KV namespace is correctly configured:

```bash
# List KV namespaces
wrangler kv:namespace list

# Test KV access
wrangler kv:key put --namespace-id=your-id test "value"
wrangler kv:key get --namespace-id=your-id test
```

### No Search Results

The current implementation includes placeholder search results. For production use, you should integrate with actual search APIs:

- Google Custom Search API
- Bing Web Search API
- SerpApi
- Or other web scraping services

## Future Enhancements

- [ ] Integration with real web search APIs
- [x] **Phone notifications via Pushover/Telegram/Discord** (IMPLEMENTED!)
- [x] **Durable Object for smart notification management** (IMPLEMENTED!)
- [ ] Database storage (D1) for more comprehensive history
- [ ] Price prediction using ML models
- [ ] Multi-currency support
- [ ] Comparison charts and visualizations
- [ ] Web dashboard for notification settings

## Contributing

This project is part of the CloudflareFS samples. Contributions are welcome!

## License

Licensed under either of:

- Apache License, Version 2.0 ([LICENSE-APACHE](../../LICENSE-APACHE))
- MIT license ([LICENSE-MIT](../../LICENSE-MIT))

at your option.

## Acknowledgments

Built with:
- [CloudflareFS](https://github.com/speakeztech/CloudflareFS) - F# bindings for Cloudflare
- [Fable](https://fable.io/) - F# to JavaScript compiler
- [Cloudflare Workers](https://workers.cloudflare.com/) - Edge computing platform

---

**Note**: This agent is designed for personal use and price monitoring. Always verify prices and product details on the retailer's website before making a purchase. The agent provides recommendations but cannot guarantee accuracy of web-scraped data.
