# 12 — Security Architecture and Hardening

*Authentication hardening, authorization enforcement, data protection, input validation, OWASP compliance, and security monitoring.*

---

## 1. Security Posture: V1 → V2

| V1 Finding | Severity | V2 Mitigation | Status |
|------------|:--------:|---------------|--------|
| SEC-001: Credentials in client JS | Critical | Server-side only, secrets manager | ✅ Designed |
| SEC-002: No CSP headers | High | Strict CSP + helmet middleware | ✅ Designed |
| SEC-003: SQL injection vectors | High | Parameterized queries only (EF Core) | ✅ Designed |
| SEC-004: Missing rate limiting | High | ASP.NET Core rate limiter middleware | ✅ Designed |
| SEC-005: Weak session management | High | JWT + httpOnly cookies + refresh rotation | ✅ Designed |
| SEC-006: No HTTPS enforcement | High | HSTS + Caddy auto-TLS | ✅ Designed |
| SEC-007: Path traversal risk | Medium | Input validation + allow-list | ✅ Designed |
| SEC-008: No audit log | Medium | Full audit trail in DB + Serilog | ✅ Designed |
| SEC-009: Verbose error messages | Medium | Global exception handler, generic responses | ✅ Designed |
| SEC-010: Insecure file uploads | Medium | Type validation, size limits, quarantine scan | ✅ Designed |
| SEC-011: CORS misconfiguration | Medium | Strict origin allow-list | ✅ Designed |
| SEC-012: Missing anti-CSRF | Medium | Double-submit cookie + SameSite | ✅ Designed |
| SEC-013: Information disclosure via headers | Low | Remove server headers | ✅ Designed |
| SEC-014: No dependency scanning | Low | GitHub Dependabot + audit CI step | ✅ Designed |
| SEC-015: No brute-force protection | Medium | Account lockout + progressive delay | ✅ Designed |
| SEC-016: Open redirect potential | Low | Redirect URL validation | ✅ Designed |

All 16 V1 security findings addressed.

---

## 2. Authentication Security

### 2.1 Password Policy

```csharp
public static class PasswordPolicy
{
    public const int MinLength = 10;          // Up from 6 in V1
    public const int MaxLength = 128;
    public const bool RequireUppercase = true;
    public const bool RequireLowercase = true;
    public const bool RequireDigit = true;
    public const bool RequireNonAlphanumeric = false; // Reduces support burden
    public const int MaxFailedAttempts = 5;
    public const int LockoutMinutes = 15;
    public const int PasswordHistoryCount = 5; // Cannot reuse last 5 passwords
}
```

### 2.2 JWT Token Configuration

```csharp
public sealed class JwtSettings
{
    public string Issuer { get; set; } = "fame-api";
    public string Audience { get; set; } = "fame-web";
    public int AccessTokenExpiryMinutes { get; set; } = 15;    // Short-lived
    public int RefreshTokenExpiryDays { get; set; } = 30;
    public int MaxConcurrentSessions { get; set; } = 3;
    // Key loaded from environment/secrets manager — never in appsettings
}
```

### 2.3 Token Delivery (httpOnly Cookies)

```csharp
// Tokens delivered in httpOnly cookies, NOT in response body
public void SetAuthCookies(HttpResponse response, string accessToken, string refreshToken)
{
    response.Cookies.Append("fame.access", accessToken, new CookieOptions
    {
        HttpOnly = true,
        Secure = true,             // HTTPS only
        SameSite = SameSiteMode.Strict,
        MaxAge = TimeSpan.FromMinutes(15),
        Path = "/api",
        Domain = ".firstaidmadeeasy.com.pk",
    });
    
    response.Cookies.Append("fame.refresh", refreshToken, new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        MaxAge = TimeSpan.FromDays(30),
        Path = "/api/auth/refresh",    // Minimal path
        Domain = ".firstaidmadeeasy.com.pk",
    });
}
```

### 2.4 Refresh Token Rotation

```
Client                 API                    Database
  │                     │                       │
  │ Request with        │                       │
  │ expired access      │                       │
  │────────────────────►│                       │
  │   401 Unauthorized  │                       │
  │◄────────────────────│                       │
  │                     │                       │
  │ POST /auth/refresh  │                       │
  │ (refresh cookie)    │                       │
  │────────────────────►│                       │
  │                     │ Validate refresh      │
  │                     │ token                 │
  │                     │──────────────────────►│
  │                     │                       │
  │                     │ Revoke old refresh    │
  │                     │ Issue new pair        │
  │                     │──────────────────────►│
  │                     │                       │
  │  New access +       │                       │
  │  refresh cookies    │                       │
  │◄────────────────────│                       │
  │                     │                       │
  │ Retry original      │                       │
  │ request             │                       │
  │────────────────────►│                       │
```

If a revoked refresh token is presented (replay attack), ALL sessions for that user are invalidated and an alert is triggered.

### 2.5 Brute Force Protection

```csharp
// Progressive delay after failed attempts
public static TimeSpan GetLockoutDuration(int failedAttempts) => failedAttempts switch
{
    <= 3 => TimeSpan.Zero,              // No delay
    4 => TimeSpan.FromSeconds(30),
    5 => TimeSpan.FromMinutes(1),
    6 => TimeSpan.FromMinutes(5),
    7 => TimeSpan.FromMinutes(15),
    _ => TimeSpan.FromMinutes(30),      // Max lockout
};
```

---

## 3. Authorization Security

### 3.1 Defense in Depth

```
Layer 1: Route-level   → [Authorize(Policy = "...")] on every endpoint
Layer 2: Resource-level → IAuthorizationHandler checks ownership
Layer 3: Data-level     → Global query filters (e.g., soft delete, tenant)
Layer 4: Field-level    → DTO projection excludes unauthorized fields
```

### 3.2 Policy Enforcement

```csharp
// Every endpoint must have an authorization attribute
// The analyzer enforces this at build time

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]             // Explicit — public catalog
    public async Task<IActionResult> List() { ... }
    
    [HttpGet("{id}")]
    [Authorize(Policy = Policies.CourseAccess)]  // Must be enrolled
    public async Task<IActionResult> Get(Guid id) { ... }
    
    [HttpPost]
    [Authorize(Policy = Policies.ContentEditor)] // Requires role
    public async Task<IActionResult> Create() { ... }
}
```

### 3.3 Global Query Filters

```csharp
// Applied automatically to every query — defense against data leaks
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Soft delete filter — deleted records never appear
    modelBuilder.Entity<Course>().HasQueryFilter(c => !c.IsDeleted);
    modelBuilder.Entity<Enrollment>().HasQueryFilter(e => !e.IsDeleted);
    
    // Active enrollment filter for course access
    // (Overridable with .IgnoreQueryFilters() for admin queries)
}
```

---

## 4. Input Validation

### 4.1 Validation Pipeline

```
Request → [Model Binding] → [FluentValidation] → [Handler] → [EF Core params]
              ↓                    ↓                              ↓
        Type-safe DTOs      Business rules              Parameterized SQL
        Trim strings        Length limits                No raw SQL
        Null checks         Regex patterns               No interpolation
```

### 4.2 FluentValidation Rules

```csharp
public sealed class CreateCourseValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must be under 200 characters")
            .Must(NotContainHtml).WithMessage("HTML is not allowed");
        
        RuleFor(x => x.Description)
            .MaximumLength(5000);
        
        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(1, 1440); // 1 min to 24 hours
        
        RuleFor(x => x.Slug)
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must be lowercase, hyphen-separated");
    }
    
    private static bool NotContainHtml(string value) =>
        !Regex.IsMatch(value ?? "", @"<[^>]+>");
}
```

### 4.3 File Upload Security

```csharp
public sealed class FileUploadValidator
{
    private static readonly HashSet<string> AllowedImageTypes = new()
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };
    
    private static readonly HashSet<string> AllowedDocTypes = new()
    {
        "application/pdf"
    };
    
    private static readonly byte[][] MagicBytes = new[]
    {
        new byte[] { 0xFF, 0xD8, 0xFF },           // JPEG
        new byte[] { 0x89, 0x50, 0x4E, 0x47 },     // PNG
        new byte[] { 0x52, 0x49, 0x46, 0x46 },     // WebP (RIFF)
        new byte[] { 0x47, 0x49, 0x46, 0x38 },     // GIF
        new byte[] { 0x25, 0x50, 0x44, 0x46 },     // PDF
    };
    
    public Result Validate(IFormFile file, FileUploadContext context)
    {
        // 1. Size limit
        if (file.Length > context.MaxSizeBytes)
            return Result.BadRequest($"File exceeds {context.MaxSizeMb}MB limit");
        
        // 2. Content-type allow-list
        var allowed = context.IsImage ? AllowedImageTypes : AllowedDocTypes;
        if (!allowed.Contains(file.ContentType.ToLower()))
            return Result.BadRequest("File type not allowed");
        
        // 3. Extension validation
        var ext = Path.GetExtension(file.FileName).ToLower();
        if (!context.AllowedExtensions.Contains(ext))
            return Result.BadRequest("File extension not allowed");
        
        // 4. Magic byte verification (prevent disguised files)
        using var stream = file.OpenReadStream();
        var header = new byte[4];
        stream.Read(header, 0, 4);
        if (!MagicBytes.Any(magic => header.Take(magic.Length).SequenceEqual(magic)))
            return Result.BadRequest("File content does not match declared type");
        
        // 5. Filename sanitization
        // Use a generated UUID filename, never the original
        
        return Result.Success();
    }
}
```

---

## 5. HTTP Security Headers

### 5.1 Caddy Configuration

```caddyfile
firstaidmadeeasy.com.pk {
    header {
        # HSTS — force HTTPS for 2 years, include subdomains
        Strict-Transport-Security "max-age=63072000; includeSubDomains; preload"
        
        # Content Security Policy
        Content-Security-Policy "default-src 'self'; script-src 'self' 'nonce-{nonce}'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https://r2.fame.pk; font-src 'self'; connect-src 'self' wss://api.firstaidmadeeasy.com.pk; frame-ancestors 'none'; base-uri 'self'; form-action 'self'"
        
        # Prevent MIME sniffing
        X-Content-Type-Options "nosniff"
        
        # Prevent framing (clickjacking)
        X-Frame-Options "DENY"
        
        # Referrer policy
        Referrer-Policy "strict-origin-when-cross-origin"
        
        # Permissions policy
        Permissions-Policy "camera=(), microphone=(), geolocation=(), payment=(self)"
        
        # Remove server identification
        -Server
        -X-Powered-By
    }
}
```

### 5.2 CORS Configuration

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins(
                "https://firstaidmadeeasy.com.pk",
                "https://www.firstaidmadeeasy.com.pk")
            .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
            .WithHeaders("Content-Type", "Authorization", "X-XSRF-TOKEN")
            .AllowCredentials()     // Required for cookies
            .SetPreflightMaxAge(TimeSpan.FromHours(1));
    });
});
```

---

## 6. Rate Limiting

```csharp
builder.Services.AddRateLimiter(options =>
{
    // Global: 100 requests per minute per IP
    options.AddFixedWindowLimiter("Global", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
        opt.QueueLimit = 0;
    });
    
    // Auth endpoints: 10 per minute per IP (brute force protection)
    options.AddFixedWindowLimiter("Auth", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 10;
        opt.QueueLimit = 0;
    });
    
    // Payment endpoints: 5 per minute per user
    options.AddFixedWindowLimiter("Payment", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
        opt.QueueLimit = 0;
    });
    
    // API general: 600 per minute per user (authenticated)
    options.AddTokenBucketLimiter("Api", opt =>
    {
        opt.TokenLimit = 60;
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
        opt.TokensPerPeriod = 10;
    });
    
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
```

---

## 7. Secrets Management

### 7.1 Secret Categories

| Secret | Storage | Access Pattern |
|--------|---------|---------------|
| DB connection string | Environment variable | Startup injection |
| JWT signing key | Environment variable | Loaded once, cached |
| JazzCash credentials | Environment variable | Per-request |
| Easypaisa credentials | Environment variable | Per-request |
| Stripe secret key | Environment variable | Per-request |
| SendGrid API key | Environment variable | Per-request |
| Redis connection | Environment variable | Startup injection |
| S3/R2 credentials | Environment variable | Per-request |

### 7.2 Configuration Loading

```csharp
// appsettings.json — structure only, NO values
{
  "Jwt": {
    "Issuer": "",
    "Audience": "",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 30
  },
  "Payment": {
    "JazzCash": { "MerchantId": "", "Password": "", "IntegritySalt": "" },
    "Easypaisa": { "StoreId": "", "HashKey": "" },
    "Stripe": { "SecretKey": "", "WebhookSecret": "" }
  }
}

// Environment variables override (production)
// JWT__SigningKey=...
// Payment__JazzCash__MerchantId=...
// Payment__Stripe__SecretKey=sk_live_...

// Docker Compose: loaded from .env file (not committed)
```

### 7.3 Pre-commit Hook

```bash
#!/bin/bash
# .husky/pre-commit — prevents committing secrets
PATTERNS="sk_live|sk_test|password|secret|api_key|apikey|token"
if git diff --cached --diff-filter=ACM | grep -iE "$PATTERNS"; then
    echo "ERROR: Potential secret detected in staged files!"
    echo "Use environment variables or secrets manager instead."
    exit 1
fi
```

---

## 8. Audit Logging

### 8.1 Events to Audit

| Category | Events |
|----------|--------|
| **Authentication** | Login, logout, failed login, password change, password reset, lockout |
| **Authorization** | Access denied, role change, permission change |
| **Data** | Create, update, delete for sensitive entities (Users, Enrollments, Payments) |
| **Payment** | Transaction created, completed, failed, refunded |
| **Admin** | User impersonation, role assignment, config change |
| **Security** | Rate limit hit, suspicious activity, token replay detected |

### 8.2 Audit Log Entity

```csharp
public sealed class AuditLog
{
    public long Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string Action { get; set; } = string.Empty;       // e.g., "User.Login"
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }    // JSON
    public string? NewValues { get; set; }    // JSON
    public AuditSeverity Severity { get; set; }
    public string? AdditionalData { get; set; }  // JSON
}
```

### 8.3 Automatic Change Tracking

```csharp
// EF Core SaveChanges interceptor
public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    var auditEntries = new List<AuditLog>();
    
    foreach (var entry in ChangeTracker.Entries<IAuditable>())
    {
        if (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
        {
            auditEntries.Add(new AuditLog
            {
                Action = $"{entry.Entity.GetType().Name}.{entry.State}",
                EntityType = entry.Entity.GetType().Name,
                EntityId = entry.Property("Id").CurrentValue?.ToString(),
                OldValues = entry.State != EntityState.Added
                    ? JsonSerializer.Serialize(entry.OriginalValues.ToObject()) : null,
                NewValues = entry.State != EntityState.Deleted
                    ? JsonSerializer.Serialize(entry.CurrentValues.ToObject()) : null,
                UserId = _currentUser.Id,
                IpAddress = _httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                Timestamp = DateTimeOffset.UtcNow,
            });
        }
    }
    
    AuditLogs.AddRange(auditEntries);
    return await base.SaveChangesAsync(ct);
}
```

---

## 9. OWASP Top 10 Compliance Matrix

| # | OWASP 2021 | V2 Controls |
|---|------------|-------------|
| A01 | Broken Access Control | Policy-based auth on every endpoint, resource-level handlers, global query filters |
| A02 | Cryptographic Failures | TLS 1.3, Argon2id password hashing, AES-256 for PII at rest |
| A03 | Injection | EF Core parameterized queries, FluentValidation, no raw SQL |
| A04 | Insecure Design | Threat modeling per module, abuse case analysis, defense in depth |
| A05 | Security Misconfiguration | Security headers, minimal API surface, no default credentials |
| A06 | Vulnerable Components | Dependabot, `dotnet audit`, npm audit in CI |
| A07 | Auth Failures | Rate limiting, account lockout, refresh token rotation, MFA-ready |
| A08 | Data Integrity Failures | Webhook signature verification, CSP, SRI for CDN assets |
| A09 | Logging & Monitoring | Audit log, structured logging, alerting on anomalies |
| A10 | SSRF | No user-controlled URLs in server requests, allow-listed domains |

---

## 10. Security Monitoring & Alerting

| Alert | Trigger | Channel |
|-------|---------|---------|
| Brute force detected | 10+ failed logins from same IP in 5 min | Email to admin + auto-block |
| Token replay | Revoked refresh token presented | Email to admin + user session invalidation |
| Rate limit storm | 100+ 429 responses to IP in 1 min | Log + temporary IP block |
| Privilege escalation attempt | Non-admin accessing admin endpoint | Log + alert |
| Unusual payment pattern | 5+ failed payment attempts from user in 1 hour | Flag account for review |
| SQL injection attempt | Detected malicious patterns in input | Log + block request |

---

*Security is not a feature — it is a property of the system. Every layer enforces its own controls. The V2 design eliminates all 16 V1 findings and adds proactive detection capabilities. The architecture assumes breach and defends accordingly.*
