# Laptop Deal Agent - ASUS ROG Flow Z13 Black Friday Tracker

An intelligent Cloudflare Worker built with F# and CloudflareFS that automatically searches for Black Friday deals on specific ASUS ROG Flow Z13 laptop models every hour.

## Overview

This project demonstrates an agentic system built with F# and Fable that:

- **Searches the web** for specific laptop models using Cloudflare AI
- **Validates product matches** to ensure accuracy (model numbers, RAM configurations)
- **Extracts pricing information** from search results
- **Tracks price history** using Cloudflare KV storage
- **Analyzes price trends** and provides purchase recommendations
- **Runs automatically** every hour via Cloudflare cron triggers
- **Generates HTML reports** with current best prices and historical data

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

### Components

1. **Types.fs** - Core data structures
   - `LaptopModel` - Discriminated union for the two laptop variants
   - `SearchResult` - Web search results
   - `PriceInfo` - Extracted price information
   - `PriceHistoryEntry` - Historical price data
   - `DealAnalysis` - Price analysis and recommendations

2. **SearchAgent.fs** - Web search and scraping
   - Model number extraction and validation
   - Price extraction from text (supports $, €, £)
   - Black Friday deal detection
   - Discount percentage extraction
   - Stock availability checking
   - Retailer identification

3. **PriceAnalyzer.fs** - Price analysis and reporting
   - Price trend analysis (increasing/decreasing/stable)
   - Historical price comparison
   - Purchase recommendations
   - KV storage operations
   - HTML report generation

4. **LaptopDealAgent.fs** - Main worker
   - Scheduled task execution
   - HTTP request handling
   - Manual trigger support
   - Status endpoints

### Data Flow

```
Cron Trigger (hourly)
    ↓
Search Agent
    ↓ (Web searches with AI)
Multiple Search Results
    ↓ (Validation & Parsing)
Filtered Price Information
    ↓
KV Storage (Price History)
    ↓
Price Analyzer
    ↓ (Analysis & Recommendations)
HTML Report
```

## Prerequisites

- .NET 8.0 or later
- Node.js 18+
- Wrangler CLI: `npm install -g wrangler`
- Cloudflare account with:
  - Workers enabled
  - KV namespace created
  - AI binding configured (optional, for enhanced search)

## Setup

### 1. Create KV Namespace

```bash
# Create a KV namespace for price history
wrangler kv:namespace create "PRICE_HISTORY"

# Note the namespace ID returned
```

### 2. Update Configuration

Edit `wrangler.toml` and update the KV namespace ID:

```toml
[[kv_namespaces]]
binding = "PRICE_HISTORY"
id = "your-kv-namespace-id-here"  # Replace with actual ID
```

### 3. Install Dependencies

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
