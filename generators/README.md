# CloudflareFS Generators

This directory contains TypeScript-to-F# binding generators and utilities for CloudflareFS.

## Structure

- `glutinum/` - Glutinum-based TypeScript to F# conversion
- `preprocessors/` - TypeScript preprocessing for Glutinum compatibility
- `postprocessors/` - F# binding post-processing and enhancement

## Usage

### Generate Bindings

```bash
cd generators/glutinum
.\generate-bindings.ps1
```

### Preprocess TypeScript

For complex TypeScript definitions that cause issues with Glutinum:

```bash
node preprocessors/simplify-types.js input.d.ts output.d.ts
```

## Known Issues

Glutinum 0.12.0 has stack overflow issues with:
- Recursive interface definitions
- Complex intersection types
- Certain generic patterns

Use the preprocessors to simplify these before generation.