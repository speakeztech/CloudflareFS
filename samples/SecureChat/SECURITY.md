# SecureChat Security Architecture

## Overview
SecureChat implements security using Cloudflare Workers' native features.

## Core Security Principles

### 1. Password Storage
Password hashes are stored in Cloudflare's Secret Store using the pattern:
```
USER_<USERNAME>_PASSWORD = <SHA256_HASH>
```

This provides:
- Hardware-level security
- Separation of authentication from application data
- No password data in D1 or KV

### 2. Timing-Safe Comparisons
All password comparisons use `crypto.subtle.timingSafeEqual` to prevent timing attacks:
```fsharp
let timingSafeEqual (a: string) (b: string) : bool =
    emitJsExpr (a, b) """
        const encoder = new TextEncoder();
        const aBytes = encoder.encode($0);
        const bBytes = encoder.encode($1);
        return aBytes.byteLength === bBytes.byteLength &&
               crypto.subtle.timingSafeEqual(aBytes, bBytes)
    """
```

### 3. User Management via Administrative Scripts
Users are created ONLY through PowerShell administrative scripts, not through API endpoints:
- No self-registration endpoint
- Controlled user creation by administrators
- Audit trail through script execution
- Optional R2 bucket provisioning per user

### 4. Session Management
- Sessions stored in both D1 (for queries) and KV (for fast lookups)
- 24-hour expiration by default
- Cryptographically secure token generation via `crypto.randomUUID()`
- No session data includes password information

## User Management

### Creating Users
```powershell
# Basic user creation
.\scripts\add-user.ps1 -Username alice -Password "SecurePassword123!"

# With R2 storage for file uploads
.\scripts\add-user.ps1 -Username bob -Password "AnotherSecure456!" -WithStorage
```

### Security Requirements
1. **Username**: Alphanumeric + underscore only
2. **Password**: Minimum 8 characters (enforce stronger policies as needed)
3. **Storage**: R2 buckets are optional and isolated per user

### Removing Users
```powershell
.\scripts\remove-user.ps1 -Username alice
```
This completely removes:
- Password secret from Cloudflare
- R2 bucket (if exists)
- All user data

### Listing Users
```powershell
.\scripts\list-users.ps1
```
Shows all configured users and identifies orphaned configurations.

## Authentication Flow

1. **Login Request** → Client sends username/password
2. **Secret Lookup** → Check `USER_<USERNAME>_PASSWORD` in Cloudflare Secrets
3. **Hash Comparison** → SHA-256 hash with timing-safe comparison
4. **Session Creation** → Generate UUID token, store in D1 + KV
5. **Token Return** → Client receives bearer token for subsequent requests

## Data Security

### Database Design
The D1 database stores ONLY:
- Session tokens (no passwords)
- Messages (with optional encryption)
- Room metadata
- Participant lists (usernames only)

### Message Encryption
Optional end-to-end encryption for messages:
- AES-GCM encryption via Web Crypto API
- Encryption keys managed separately
- Encrypted messages marked in database

### R2 Storage Isolation
Each user gets their own R2 bucket:
- Pattern: `<username>-chat-storage`
- Complete isolation between users
- No cross-user data access

## Deployment Security

### Required Secrets
Set these via `wrangler secret put`:
```bash
# User passwords (created by add-user.ps1)
wrangler secret put USER_ALICE_PASSWORD
wrangler secret put USER_BOB_PASSWORD

# Optional: Encryption key for messages
wrangler secret put ENCRYPTION_KEY

# Optional: JWT secret for enhanced tokens
wrangler secret put JWT_SECRET
```

### Environment Configuration
```toml
# wrangler.toml - NO PASSWORDS HERE
[vars]
ENVIRONMENT = "production"
API_VERSION = "1.0.0"
# All sensitive data in secrets, not vars
```

## Security Checklist

✅ **Password Storage**
- [ ] Passwords stored ONLY in Cloudflare Secrets
- [ ] No password data in D1, KV, or R2
- [ ] SHA-256 hashing with salt

✅ **Authentication**
- [ ] Timing-safe password comparison
- [ ] No self-registration endpoint
- [ ] Admin-only user creation

✅ **Session Management**
- [ ] Cryptographically secure tokens
- [ ] Automatic expiration
- [ ] Dual storage (D1 + KV)

✅ **Data Protection**
- [ ] User isolation via separate R2 buckets
- [ ] Optional message encryption
- [ ] No credential logging

✅ **Deployment**
- [ ] Secrets via wrangler, not config files
- [ ] No hardcoded credentials
- [ ] Audit trail via scripts

## Threat Model

This implementation protects against:
- **Password database breaches** - Passwords not in database
- **Timing attacks** - Timing-safe comparisons
- **Session hijacking** - Secure token generation
- **Cross-user data access** - Isolated storage
- **Credential exposure** - Hardware-secured secrets

## Compliance Notes

This architecture supports compliance with:
- GDPR - Data isolation and right to deletion
- HIPAA - Encryption and access controls (with additional configuration)
- SOC 2 - Secure credential management
- PCI DSS - Separation of authentication data

## Not Implemented (Intentionally)

These features are omitted to maintain focus on core security:
- OAuth/SAML (use Cloudflare Access for this)
- 2FA (use Cloudflare Access for this)
- Password reset (admin-managed users)
- Self-service registration (security by design)

## Enterprise Considerations

1. Add Cloudflare Access for additional authentication
2. Implement rate limiting via Cloudflare Rules
3. Add audit logging to Cloudflare Logpush
4. Consider Cloudflare Zero Trust for network security