# Laptop Deal Agent - React Dashboard

A beautiful dark-themed React SPA built with F#, Fable, and DaisyUI v4 that displays all ASUS ROG Flow Z13 Black Friday deals in real-time.

## Features

✨ **Beautiful UI**
- Dark theme by default (DaisyUI v4)
- Responsive grid layout
- Smooth animations and transitions
- Custom Cloudflare orange accents (#f38020)

📊 **Real-time Stats**
- Total deals tracked
- Black Friday deal count
- In-stock availability
- Best price found

🔍 **Advanced Filtering**
- Filter by: All, Black Friday Only, In Stock Only
- Sort by: Most Recent, Lowest Price
- Real-time search and filtering

🎯 **Deal Cards**
- Model information with RAM size
- Current price with discount badges
- Retailer and detection time
- Direct links to purchase
- Stock status indicators

⚡ **Auto-refresh**
- Polls API every 5 minutes
- Manual refresh button
- Loading states and error handling

## Tech Stack

- **F#** - Type-safe functional programming
- **Fable** - F# to JavaScript compiler
- **Feliz** - F# DSL for React
- **React 18** - UI library
- **DaisyUI v4** - Component library with theming
- **Tailwind CSS** - Utility-first CSS
- **Vite** - Fast build tool and dev server

## Project Structure

```
LaptopDealAgent.UI/
├── src/
│   ├── Components/
│   │   ├── DealCard.fs      # Individual deal card
│   │   ├── DealList.fs      # Grid of deals with filtering
│   │   ├── Header.fs        # Top navigation with refresh
│   │   └── Stats.fs         # Statistics dashboard
│   ├── Types.fs             # Data models
│   ├── Api.fs               # API client
│   ├── App.fs               # Main application
│   ├── index.css            # Tailwind + custom styles
│   └── LaptopDealAgent.UI.fsproj
├── index.html               # Entry point
├── tailwind.config.js       # DaisyUI configuration
├── vite.config.js           # Vite configuration
├── package.json
└── README.md
```

## Getting Started

### Prerequisites

- .NET 8.0+
- Node.js 18+
- Running LaptopDealAgent worker (see ../LaptopDealAgent)

### Installation

```bash
cd samples/LaptopDealAgent.UI

# Install NPM dependencies
npm install

# Restore .NET packages (if needed)
dotnet restore src
```

### Development

```bash
# Start Fable watch + Vite dev server
npm run fable

# This will:
# 1. Compile F# to JavaScript (watch mode)
# 2. Start Vite dev server on http://localhost:3000
# 3. Auto-reload on file changes
```

The dev server will proxy API requests to `http://localhost:8787` (your local worker).

Make sure the LaptopDealAgent worker is running:

```bash
cd ../LaptopDealAgent
npm run dev
```

### Building for Production

```bash
# Compile F# to JavaScript
npm run fable:build

# Build optimized production bundle
npm run build

# Output will be in dist/
```

### Deployment

#### Option 1: Cloudflare Pages

```bash
# Deploy to Cloudflare Pages
npm run deploy

# Or manually:
npx wrangler pages deploy dist --project-name laptop-deal-dashboard
```

#### Option 2: Static Hosting

The `dist/` folder contains a static SPA that can be hosted anywhere:

- GitHub Pages
- Netlify
- Vercel
- AWS S3 + CloudFront
- Any web server

**Important**: Configure your worker URL in production. Update `Api.fs`:

```fsharp
let private baseUrl = "https://your-worker.workers.dev/api"
```

Or use environment variables with Vite.

## API Integration

The UI consumes these API endpoints from the worker:

### `GET /api/deals`

Returns current deals:

```json
{
  "deals": [
    {
      "model": "GZ302EA-R9641TB",
      "price": 1299.99,
      "currency": "USD",
      "retailer": "Best Buy",
      "url": "https://...",
      "inStock": true,
      "isBlackFridayDeal": true,
      "discountPercentage": 18.5,
      "detectedAt": "2025-11-14T22:30:00Z"
    }
  ],
  "lastUpdated": "2025-11-14T22:30:00Z",
  "totalDeals": 5
}
```

### `GET /api/analysis`

Returns price analysis:

```json
{
  "analyses": [
    {
      "model": "GZ302EA-R9641TB",
      "currentBestPrice": 1299.99,
      "bestRetailer": "Best Buy",
      "averagePrice": 1450.00,
      "lowestHistoricalPrice": 1299.99,
      "priceTrend": "decreasing",
      "recommendation": "Great deal! Current price is at historical low."
    }
  ],
  "generatedAt": "2025-11-14T22:30:00Z"
}
```

### `POST /api/trigger`

Manually triggers a new search on the worker.

## Customization

### Theme Colors

Edit `tailwind.config.js`:

```javascript
dark: {
  primary: "#f38020",     // Cloudflare orange
  secondary: "#2196F3",   // Blue
  accent: "#4CAF50",      // Green
  "base-100": "#000000",  // Background
  // ...
}
```

### Auto-refresh Interval

Edit `App.fs`:

```fsharp
// Change 300000 (5 minutes) to your desired interval in milliseconds
let interval = JS.setInterval (fun () -> loadData() |> Promise.start) 300000.0
```

### API Base URL

Edit `Api.fs`:

```fsharp
let private baseUrl = "/api"  // Relative URL (uses proxy in dev)
// Or absolute: "https://your-worker.workers.dev/api"
```

## DaisyUI Components Used

- **Card** - Deal cards and containers
- **Stats** - Statistics dashboard
- **Badge** - Status indicators (Black Friday, In Stock)
- **Button** - Actions and filters
- **Button Group** - Filter toggles
- **Select** - Sort dropdown
- **Alert** - Error messages
- **Loading** - Loading spinner
- **Navbar** - Header

## Development Tips

### Hot Reload

The Fable compiler watches for F# file changes and recompiles automatically. Vite hot-reloads the browser.

### Debugging

1. Open browser DevTools
2. Source maps are enabled - you can see F# code in Sources tab
3. Set breakpoints in compiled JS or use `console.log` in F#:

```fsharp
open Browser

console.log("Debug message", someValue)
```

### Adding Components

1. Create new `.fs` file in `src/Components/`
2. Add to `.fsproj` in the correct compilation order
3. Import in `App.fs` or parent component:

```fsharp
open LaptopDealAgent.UI.Components.MyComponent

// Use in render:
MyComponent.MyComponent props
```

### Styling

Use DaisyUI classes whenever possible:

```fsharp
Html.div [
    prop.className "card bg-base-200 shadow-xl"
    prop.children [ ... ]
]
```

For custom styles, add to `src/index.css`.

## Troubleshooting

### API calls fail

- Ensure worker is running on port 8787
- Check Vite proxy configuration in `vite.config.js`
- Open browser DevTools → Network tab to see requests

### Fable compilation errors

```bash
# Clean and rebuild
rm -rf src/.fable
dotnet clean src
npm run fable:build
```

### Styling not applied

```bash
# Rebuild Tailwind
rm -rf dist
npm run build
```

### TypeScript errors in console

These are usually harmless - Fable generates JS, not TS. You can ignore them or configure Vite to suppress warnings.

## Contributing

This UI is part of the CloudflareFS LaptopDealAgent sample. Contributions welcome!

## License

Licensed under MIT OR Apache-2.0 (same as parent project).

---

**Built with** ❤️ **using F#, Fable, React, and DaisyUI**
