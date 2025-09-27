# SecureChat UI

A modern, secure chat application built with F#, Fable.React, and Tailwind CSS. Deploys to Cloudflare Pages.

## Features

- **React + Fable** - Type-safe React components in F#
- **Tailwind CSS** - Modern utility-first styling
- **Dark Mode** - System preference detection with manual toggle
- **localStorage** - Persistent theme preference
- **Vite** - Fast development and optimized production builds
- **Cloudflare Pages** - Edge deployment with global CDN

## Development

```bash
# Install dependencies
npm install

# Run development server (starts Fable in watch mode + Vite)
npm run fable

# The app will be available at http://localhost:3000
```

## Building

```bash
# Build for production
npm run fable:build
npm run build

# Preview production build
npm run preview
```

## Deployment to Cloudflare Pages

### Method 1: Git Integration
1. Push code to GitHub/GitLab
2. Connect repository in Cloudflare Pages dashboard
3. Set build command: `npm run fable:build && npm run build`
4. Set output directory: `dist`

### Method 2: Direct Upload
```bash
# Build and deploy
npm run deploy
```

## Project Structure

```
SecureChat.UI/
├── src/
│   ├── Components/       # React components
│   │   ├── Login.fs      # Login form
│   │   ├── ChatRoom.fs   # Main chat interface
│   │   ├── MessageList.fs # Message display
│   │   ├── MessageInput.fs # Message input
│   │   └── ThemeToggle.fs  # Dark mode switcher
│   ├── Api.fs            # API client
│   ├── ThemeContext.fs   # Theme management
│   └── App.fs            # Main application
├── index.html            # HTML entry point
├── tailwind.config.js    # Tailwind configuration
├── vite.config.js        # Vite configuration
└── package.json          # Dependencies

```

## API Configuration

During development, the Vite proxy forwards `/api` requests to `http://localhost:8787` (local Worker).

For production, update the API URL in `Api.fs` or use environment variables.

## Theme System

- Default: Dark mode
- Preference stored in localStorage
- CSS class-based switching (`dark` class on `<html>`)
- Tailwind's built-in dark mode support

## Technologies

- **Fable 4** - F# to JavaScript compiler
- **Feliz 2** - F# React DSL
- **React 18** - UI framework
- **Tailwind CSS 3** - Utility-first CSS
- **Vite 5** - Build tool and dev server
- **Cloudflare Pages** - Edge hosting platform