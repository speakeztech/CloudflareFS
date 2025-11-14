# AI-Powered Actor System for Laptop Deal Hunting

This document explains the idempotent, parallel actor-based architecture for intelligent laptop deal discovery.

## Architecture Overview

The system uses a **distributed actor model** powered by Cloudflare Durable Objects, with **Cloudflare AI** for intelligent page analysis.

```
┌─────────────────────────────────────────────────────────────┐
│               Main Orchestrator (Worker)                     │
│                                                              │
│  Spawns parallel SearchActor instances                      │
└────────────────┬────────────────────────────────────────────┘
                 │
                 │ Parallel execution
                 ▼
┌────────────────────────────────────────────────────────────────┐
│                     SearchActor Pool                            │
│                (Durable Object per Model)                       │
│                                                                 │
│  ┌──────────────────┐          ┌──────────────────┐          │
│  │  Actor: 64GB     │          │  Actor: 128GB    │          │
│  │  GZ302EA-R9641TB │          │  GZ302EA-XS99    │          │
│  │                  │          │                  │          │
│  │  • Idempotency   │          │  • Idempotency   │          │
│  │  • Price $2000   │          │  • Price Tracking │          │
│  │  • URL Tracking  │          │  • URL Tracking  │          │
│  └────────┬─────────┘          └────────┬─────────┘          │
└───────────│────────────────────────────│────────────────────┘
            │                             │
            │ Deep analyze URLs           │
            ▼                             ▼
┌──────────────────────────────────────────────────────────────┐
│                   Cloudflare AI                               │
│              (@cf/meta/llama-3-8b-instruct)                  │
│                                                              │
│  For each URL:                                              │
│  1. Fetch page content                                      │
│  2. Extract product details                                 │
│  3. Validate model number                                   │
│  4. Validate condition (New/Certified Refurbished)          │
│  5. Validate retailer reputation                            │
│  6. Extract quantity                                        │
│  7. Check price threshold                                   │
└──────────────────────────────────────────────────────────────┘
            │
            │ Valid deals
            ▼
┌──────────────────────────────────────────────────────────────┐
│                  Aggregated Results                           │
│                                                              │
│  • Stored in KV (price history)                             │
│  • Available via API (/api/deals)                           │
│  • Displayed in React dashboard                             │
│  • Sent via Pushover notifications                          │
└──────────────────────────────────────────────────────────────┘
```

## Key Components

### 1. SearchActor (Durable Object)

**File:** `SearchActor.fs`

Each SearchActor is a **stateful singleton** for a specific laptop model:

**Responsibilities:**
- Maintain idempotency state (tracked URLs + prices)
- Execute web searches for assigned model
- Deep analyze each URL with AI
- Track price changes over time
- Filter duplicates (only process new URLs or price drops)

**State:**
```fsharp
type SearchActorState = {
    Model: LaptopModel
    TrackedUrls: Map<string, TrackedUrl>
    TotalDealsFound: int
    LastRun: DateTime option
}

type TrackedUrl = {
    Url: string
    LastPrice: decimal
    LastSeen: DateTime
    FirstSeen: DateTime
    PriceHistory: (DateTime * decimal) list
}
```

**Key Methods:**
- `ExecuteSearch` - Orchestrates search for the model
- `ShouldProcessUrl` - Idempotency check (new URL or price drop?)
- `UpdateTrackedUrl` - Updates price tracking
- `PerformWebSearch` - Gets candidate URLs (placeholder for real search API)

**Idempotency Logic:**
```fsharp
member this.ShouldProcessUrl(url: string, newPrice: decimal) : bool * string =
    match st.TrackedUrls.TryFind url with
    | None ->
        (true, "New URL")  // ✓ Process new URLs
    | Some tracked ->
        if newPrice < tracked.LastPrice then
            (true, $"Price drop!")  // ✓ Process price drops
        else
            (false, $"Already tracked")  // ✗ Skip unchanged
```

### 2. AIAnalyzer

**File:** `AIAnalyzer.fs`

Uses **Cloudflare Workers AI** to intelligently analyze product pages.

**Key Functions:**

#### `analyzePageWithAI`
Sends page HTML to Llama-3-8B with detailed prompts:

```fsharp
let prompt = $"""
Analyze this product listing page.

TARGET DEVICE:
- Model: {modelNumber}
- RAM: {ramSize}GB
- Max price: ${maxPrice}

VALIDATION:
1. Exact model match
2. New or Certified Refurbished only
3. In stock
4. Price under threshold
5. Reputable retailer

EXTRACT:
- Price
- Condition
- Quantity
- Stock status
- Retailer
"""
```

**AI Response Parsing:**
- Expects JSON from AI
- Validates all criteria
- Returns `PriceInfo option` (Some if valid, None otherwise)

#### `isReputableRetailer`
Validates retailer against whitelist:
- Best Buy
- Amazon
- Newegg
- Micro Center
- B&H Photo
- ASUS Official
- Microsoft Store
- Walmart, Target, Costco

#### `fetchPageContent`
Fetches actual HTML from product URLs for analysis.

### 3. SearchOrchestrator

**File:** `SearchOrchestrator.fs`

Coordinates parallel actor execution.

**Key Function:** `orchestrateParallelSearches`

```fsharp
let searchTasks = [
    (GZ302EA_R9641TB, "search query...", 10)
    (GZ302EA_XS99, "search query...", 10)
]

// Execute in parallel using actors
let! results =
    searchTasks
    |> List.map (fun (model, query, max) ->
        executeSearch env model query max
    )
    |> Promise.all

// Aggregate results
let allDeals = results |> Array.collect List.toArray |> Array.toList
```

**Benefits:**
- Each model searched independently
- Parallel execution for speed
- Isolated failure domains
- Per-model idempotency tracking

## Price Thresholds

Enforced at the AI analysis level:

| Model | RAM | Max Price |
|-------|-----|-----------|
| GZ302EA-R9641TB | 64GB | **$2,000** |
| GZ302EA-XS99 | 128GB | **$2,300** |

Any listing above these thresholds is **automatically rejected** by the AI analyzer.

## Validation Rules

### Must Pass (Or Rejected):

✅ **Model Number** - Exact match (GZ302EA-R9641TB or GZ302EA-XS99)
✅ **RAM Size** - Exact match (64GB or 128GB)
✅ **Condition** - "New" OR "Certified Refurbished" only
✅ **Price** - Under threshold ($2000 or $2300)
✅ **Stock** - Available for purchase
✅ **Retailer** - On reputable list

### Extracted (If Available):

- **Quantity** - e.g., "Only 3 left in stock"
- **Discount %** - Calculated from original price
- **Stock Text** - Full stock message
- **Title** - Product title from page

## Idempotency in Action

### Scenario 1: New Deal Found

```
1. Actor searches for URLs
2. Finds: https://bestbuy.com/asus-rog-z13
3. Checks: Not in TrackedUrls
4. Decision: ✓ Process (new URL)
5. AI analyzes page → $1,899
6. Validates: ✓ All checks pass
7. Stores: URL → $1,899 in TrackedUrls
8. Returns: PriceInfo to orchestrator
```

### Scenario 2: Same URL, Same Price

```
1. Actor searches for URLs
2. Finds: https://bestbuy.com/asus-rog-z13 (again)
3. Checks: TrackedUrls[$1,899]
4. AI analyzes page → $1,899 (same)
5. Decision: ✗ Skip (no change)
6. Returns: None
```

### Scenario 3: Price Drop!

```
1. Actor searches for URLs
2. Finds: https://bestbuy.com/asus-rog-z13
3. Checks: TrackedUrls[$1,899]
4. AI analyzes page → $1,699
5. Decision: ✓ Process (price drop $200!)
6. Updates: URL → $1,699 in TrackedUrls
7. Adds to history: (now, $1,699)
8. Returns: PriceInfo with discount %
9. → Triggers notification (if configured)
```

## Integration with Notifications

When a price drop is detected, the system can trigger Pushover notifications:

```fsharp
if newPrice < tracked.LastPrice then
    // This is a price drop - notify user!
    let event = {
        Model = model.ModelNumber
        Price = newPrice
        PreviousLowestPrice = Some tracked.LastPrice
        Priority = High  // Price drop = high priority
        // ...
    }

    notifyDeal env userId event rules channels dashboardUrl
```

## Data Storage

### Durable Object Storage (per Actor)

- **Key:** `"state"`
- **Value:** `SearchActorState`
- **Persistence:** Permanent
- **Scope:** Per model (one actor per model)

### KV Storage (Global)

- **current_deals** - Latest deals from all actors
- **current_analyses** - Price analyses
- **price_history_{model}_{retailer}_{timestamp}** - Individual price entries

## API Endpoints

### Existing

- `GET /api/deals` - All current deals
- `GET /api/analysis` - Price analysis
- `GET /api/history` - Price history

### New (Actor Management)

These could be added for debugging:

- `POST /api/actors/reset` - Reset all actor states
- `GET /api/actors/status` - Get all actor statuses
- `POST /api/actors/{model}/search` - Manually trigger search for model

## Configuration

### wrangler.toml

```toml
# Durable Object for parallel search actors
[[durable_objects.bindings]]
name = "SEARCH_ACTOR"
class_name = "SearchActorDO"
script_name = "laptop-deal-agent"

[[migrations]]
tag = "v2"
new_classes = ["SearchActorDO"]

# AI binding for page analysis
[ai]
binding = "AI"
```

### Environment

```fsharp
type LaptopAgentEnv =
    inherit Env
    abstract PRICE_HISTORY: obj     // KV namespace
    abstract AI: obj                // AI binding
    abstract SEARCH_ACTOR: obj      // Durable Object namespace
```

## Benefits of This Architecture

### 1. **Idempotency**
- No duplicate processing
- Price-drop-only updates
- Efficient resource usage

### 2. **Parallelism**
- Each model searched independently
- Faster total execution time
- Better resource utilization

### 3. **Isolation**
- Per-model state
- Isolated failures
- Independent scaling

### 4. **Intelligence**
- AI validates products automatically
- Extracts structured data from HTML
- Handles variations in page layouts

### 5. **Accuracy**
- Strict validation rules
- Model number verification
- Condition checking
- Retailer reputation

### 6. **Cost Efficiency**
- Only process new/changed listings
- Skip redundant AI calls
- Minimize KV writes

## Example Flow (Complete)

**Hour 1:**
```
1. Orchestrator spawns 2 actors (64GB, 128GB)
2. Each actor searches web
3. 64GB finds 5 URLs, 128GB finds 3 URLs
4. Actors analyze all URLs with AI in parallel
5. Results: 2 valid deals (64GB: 1, 128GB: 1)
6. Both stored in KV
7. Notification sent for 64GB deal ($1,899 < $2,000)
```

**Hour 2 (1 hour later):**
```
1. Same actors (persistent state)
2. Find same 8 URLs + 1 new URL
3. Idempotency: Skip 8 existing (same price)
4. Process: 1 new URL
5. AI validates new URL → Valid!
6. Result: 1 new deal added
7. Notification sent
```

**Hour 3 (Price drop!):**
```
1. Same actors
2. Find same 9 URLs
3. Idempotency: 8 unchanged, 1 price drop ($1,899 → $1,699)
4. Process: 1 price drop
5. Update tracking, add to history
6. Result: 1 updated deal
7. Notification: "🔥 PRICE DROP $200!"
```

## Development Tips

### Testing Idempotency

```bash
# Reset actor state
curl -X POST https://your-worker.workers.dev/api/actors/reset

# First run - should find deals
curl https://your-worker.workers.dev/api/trigger

# Second run - should skip (idempotent)
curl https://your-worker.workers.dev/api/trigger
```

### Debugging AI Analysis

Check logs for AI prompts and responses:

```bash
wrangler tail laptop-deal-agent --format=pretty
```

Look for:
- `AI Response: {...}` - What AI returned
- `✓ Valid deal found:` - Successful validation
- `Invalid condition:` - Validation failure reasons

### Adding Real Search API

Replace `PerformWebSearch` in SearchActor.fs:

```fsharp
member this.PerformWebSearch(query: string, maxResults: int) =
    promise {
        // Integrate with Google Custom Search
        let apiKey = env.GOOGLE_SEARCH_API_KEY
        let searchEngineId = env.GOOGLE_SEARCH_ENGINE_ID

        let! response =
            Fetch.fetch
                $"https://www.googleapis.com/customsearch/v1?key={apiKey}&cx={searchEngineId}&q={query}"
                []

        let! json = response.json()
        let items = json?items |> unbox<array>

        return items |> Array.map (fun item -> item?link |> unbox<string>) |> Array.toList
    }
```

## Cost Considerations

### Cloudflare AI

- ~$0.01 per 1,000 neurons
- Llama-3-8B: 8 billion parameters
- Cost per page analysis: ~$0.00008

### Durable Objects

- $0.15 per million requests
- $0.20 per GB-month storage
- Minimal cost for 2 actors with small state

### Idempotency Savings

Without idempotency (process all URLs every hour):
- 10 URLs × 24 hours = 240 AI calls/day
- Cost: 240 × $0.00008 = $0.0192/day

With idempotency (process only new/changed):
- 1-2 new URLs per hour average
- 30 AI calls/day
- Cost: 30 × $0.00008 = $0.0024/day
- **Savings: 87.5%**

## Future Enhancements

- [ ] Machine learning for price prediction
- [ ] Automatic retailer discovery
- [ ] Multi-region search (US, EU, Asia)
- [ ] Historical price charts in dashboard
- [ ] Email digest of weekly deals
- [ ] Browser extension integration
- [ ] Webhook support for custom integrations

---

**This AI-powered actor system demonstrates best practices for building intelligent, scalable, and cost-effective data collection systems on Cloudflare's edge platform using F# and Fable.**
