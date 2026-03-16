# 13 — Performance, Scalability, and Caching Strategy

*Response time targets, caching architecture, database optimization, CDN strategy, load testing plan, and scaling playbook.*

---

## 1. Performance Requirements

### 1.1 Response Time Targets

| Endpoint Category | P50 | P95 | P99 | Max |
|------------------|:---:|:---:|:---:|:---:|
| Static pages (SSG/ISR) | <50ms | <100ms | <200ms | 500ms |
| API reads (cached) | <30ms | <80ms | <150ms | 300ms |
| API reads (DB) | <100ms | <250ms | <500ms | 1s |
| API writes | <200ms | <500ms | <1s | 2s |
| Exam submission + scoring | <500ms | <1s | <2s | 3s |
| Search (Meilisearch) | <20ms | <50ms | <100ms | 200ms |
| File upload | <1s | <3s | <5s | 10s |
| Payment initiation | <1s | <2s | <3s | 5s |

### 1.2 Throughput Targets

| Metric | Target | V1 Estimated |
|--------|:------:|:------------:|
| Concurrent users | 500 | ~50-100 |
| Requests/second | 1,000 | ~50 |
| Active websocket connections | 500 | ~20 |
| Exam submissions/minute | 100 | ~10 |
| Database connections (pool) | 100 | ~20 |

### 1.3 Core Web Vitals Targets

| Metric | Target | Measurement |
|--------|:------:|-------------|
| LCP (Largest Contentful Paint) | <2.5s | All pages |
| FID (First Input Delay) | <100ms | Interactive pages |
| CLS (Cumulative Layout Shift) | <0.1 | All pages |
| TTFB (Time to First Byte) | <200ms | Server-rendered pages |
| INP (Interaction to Next Paint) | <200ms | All pages |

---

## 2. Caching Architecture

### 2.1 Caching Layers

```
Client                CDN/ISR           Redis             Database
  │                     │                 │                  │
  │ Request             │                 │                  │
  │────────────────────►│                 │                  │
  │                     │                 │                  │
  │  Cache hit? ────────┤                 │                  │
  │  (ISR/static)       │                 │                  │
  │◄────────────────────│ YES: serve      │                  │
  │                     │                 │                  │
  │                     │ NO: forward ───►│                  │
  │                     │                 │ Cache hit? ──────┤
  │                     │                 │                  │
  │                     │                 │ YES: return ─────┤
  │◄────────────────────│◄────────────────│                  │
  │                     │                 │                  │
  │                     │                 │ NO: query ──────►│
  │                     │                 │                  │
  │                     │                 │◄──── Result ─────│
  │                     │                 │ Cache it ────────┤
  │◄────────────────────│◄────────────────│                  │
```

### 2.2 Cache Strategy by Data Type

| Data Type | Cache Location | TTL | Invalidation |
|-----------|:-------------:|:---:|:------------:|
| **Static assets** (JS, CSS, images) | CDN + browser | 1 year | Content hash in filename |
| **Public pages** (pricing, about) | ISR | 1 hour | On-demand revalidation |
| **Course catalog** | ISR + Redis | 15 min | On course update |
| **Package list** | Redis | 1 hour | On package update |
| **User session** | Redis | 15 min align with JWT | On logout/token refresh |
| **Course progress** | Redis | 5 min | On progress update |
| **MCQ questions** (exam) | Redis | 24 hours | On question update |
| **Search index** | Meilisearch | Real-time sync | On content change |
| **User profile** | None (always fresh) | — | — |
| **Payment data** | None (never cached) | — | — |

### 2.3 Redis Cache Implementation

```csharp
public sealed class CacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<CacheService> _logger;
    
    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiry = null,
        CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        
        // Try cache first
        var cached = await db.StringGetAsync(key);
        if (cached.HasValue)
        {
            _logger.LogDebug("Cache HIT: {Key}", key);
            return JsonSerializer.Deserialize<T>(cached!);
        }
        
        // Cache miss — execute factory
        _logger.LogDebug("Cache MISS: {Key}", key);
        var value = await factory();
        
        if (value is not null)
        {
            var json = JsonSerializer.Serialize(value);
            await db.StringSetAsync(key, json, expiry ?? TimeSpan.FromMinutes(15));
        }
        
        return value;
    }
    
    public async Task InvalidateAsync(string pattern)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: pattern).ToArray();
        if (keys.Length > 0)
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(keys);
            _logger.LogInformation("Invalidated {Count} cache keys matching {Pattern}",
                keys.Length, pattern);
        }
    }
}
```

### 2.4 Cache Key Convention

```
fame:{module}:{entity}:{id}         — Single entity
fame:{module}:{entity}:list:{hash}  — List with filter hash
fame:{module}:{entity}:count        — Count queries
fame:session:{userId}               — User session data

Examples:
fame:catalog:course:a1b2c3d4        — Single course
fame:catalog:course:list:abc123     — Course list (hash of filters)
fame:assessment:questions:exam:x1y2 — Questions for an exam
fame:identity:session:user123       — User session
```

---

## 3. Database Performance

### 3.1 Indexing Strategy

```sql
-- High-traffic queries: course catalog
CREATE INDEX IX_Courses_ExamTrackId_IsPublished 
    ON Courses (ExamTrackId, IsPublished) 
    INCLUDE (Title, Slug, ThumbnailUrl, DisplayOrder)
    WHERE IsDeleted = false;

-- Enrollment lookup
CREATE INDEX IX_Enrollments_UserId_Status 
    ON Enrollments (UserId, Status) 
    INCLUDE (PackageId, ExpiresAt)
    WHERE IsDeleted = false;

-- Progress tracking
CREATE INDEX IX_CourseProgress_UserId_CourseId 
    ON CourseProgress (UserId, CourseId) 
    INCLUDE (CompletedLectures, TotalLectures, PercentComplete);

-- Transaction history
CREATE INDEX IX_Transactions_UserId_Status_PaidAt 
    ON Transactions (UserId, Status, PaidAt DESC);

-- MCQ questions for exam generation
CREATE INDEX IX_Questions_CourseId_Difficulty_IsActive 
    ON Questions (CourseId, Difficulty, IsActive) 
    WHERE IsDeleted = false;

-- Notification fetching
CREATE INDEX IX_Notifications_UserId_ReadAt_CreatedAt 
    ON Notifications (UserId, ReadAt, CreatedAt DESC);
```

### 3.2 Query Optimization Rules

1. **No N+1 queries** — Use `.Include()` or explicit joins; lint with EF Core analyzers
2. **Projection** — Never `SELECT *`; use `.Select()` to project only needed columns
3. **Pagination** — Keyset pagination for large datasets (not OFFSET/LIMIT)
4. **Async all the way** — `ToListAsync()`, `FirstOrDefaultAsync()`, never blocking `.Result`
5. **Read replicas** — Configure read queries against replica when scaling
6. **Connection pooling** — Npgsql pool: min 10, max 100 connections

### 3.3 Keyset Pagination

```csharp
// Efficient pagination for large tables (no OFFSET performance degradation)
public async Task<PagedResult<CourseDto>> GetCourses(
    Guid? afterId, int pageSize = 20, CancellationToken ct = default)
{
    var query = _db.Courses
        .Where(c => c.IsPublished && !c.IsDeleted)
        .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id);
    
    if (afterId.HasValue)
    {
        var cursor = await _db.Courses.FindAsync(afterId.Value);
        query = query.Where(c => 
            c.DisplayOrder > cursor!.DisplayOrder || 
            (c.DisplayOrder == cursor!.DisplayOrder && c.Id > afterId));
    }
    
    var items = await query
        .Take(pageSize + 1)  // Fetch one extra to detect hasMore
        .Select(c => new CourseDto { ... })
        .ToListAsync(ct);
    
    return new PagedResult<CourseDto>
    {
        Items = items.Take(pageSize).ToList(),
        HasMore = items.Count > pageSize,
        NextCursor = items.Count > pageSize ? items[pageSize - 1].Id : null,
    };
}
```

---

## 4. Frontend Performance

### 4.1 Next.js Rendering Strategy

| Route | Strategy | Rationale |
|-------|----------|-----------|
| `/` (Homepage) | SSG + ISR (1h) | Rarely changes, must be fast |
| `/courses` | ISR (15 min) | Catalog changes infrequently |
| `/courses/[slug]` | ISR (15 min) | Individual course pages |
| `/pricing` | ISR (1h) | Pricing changes are admin-controlled |
| `/blog/[slug]` | SSG at build | Content is static |
| `/dashboard` | CSR | Personalized, needs auth |
| `/exam/[id]` | CSR | Real-time, interactive |
| `/admin/*` | CSR | Internal, SEO irrelevant |

### 4.2 Bundle Optimization

```typescript
// next.config.ts
const nextConfig: NextConfig = {
  experimental: {
    optimizePackageImports: [
      '@radix-ui/react-icons',
      'date-fns',
      'lodash-es',
    ],
  },
  images: {
    remotePatterns: [
      { hostname: 'r2.fame.pk' },  // S3-compatible storage
    ],
    formats: ['image/avif', 'image/webp'],
  },
  // Split large vendor chunks
  webpack: (config) => {
    config.optimization.splitChunks = {
      ...config.optimization.splitChunks,
      cacheGroups: {
        ...config.optimization.splitChunks?.cacheGroups,
        radix: {
          test: /[\\/]node_modules[\\/]@radix-ui/,
          name: 'radix',
          priority: 10,
        },
      },
    };
    return config;
  },
};
```

### 4.3 Image Optimization

- All images served through Next.js `<Image>` component
- Automatic format conversion (WebP/AVIF)
- Responsive `srcSet` generation
- Lazy loading below the fold
- CDN caching with immutable cache headers
- Course thumbnails: 400×300 WebP, ~30KB average

### 4.4 Code Splitting & Lazy Loading

```typescript
// Exam engine — large, only loaded when needed
const ExamEngine = dynamic(() => import('@/features/exam/ExamEngine'), {
  loading: () => <ExamSkeleton />,
  ssr: false, // Client-only
});

// Rich text editor — only for content editors
const RichEditor = dynamic(() => import('@/components/RichEditor'), {
  loading: () => <Skeleton className="h-[300px]" />,
  ssr: false,
});
```

---

## 5. CDN & Static Asset Strategy

### 5.1 Asset Pipeline

```
Source → Build → Hash → Deploy to R2 → Serve via CDN
                  │
                  ├── JS:  /assets/app-[hash].js
                  ├── CSS: /assets/app-[hash].css
                  └── IMG: /images/[name]-[hash].webp
```

### 5.2 Cache Headers

| Asset Type | Cache-Control | CDN TTL |
|-----------|---------------|---------|
| Hashed assets (JS, CSS) | `public, max-age=31536000, immutable` | 1 year |
| Images (optimized) | `public, max-age=86400, s-maxage=604800` | 7 days |
| HTML pages | `public, max-age=0, s-maxage=3600, stale-while-revalidate=86400` | 1 hour |
| API responses | `private, no-cache` | Not cached |

---

## 6. Video Streaming Optimization

Video lectures are the heaviest asset type. Strategy:

1. **Storage**: Course videos stored on S3-compatible storage (Cloudflare R2 or Backblaze B2)
2. **Delivery**: Streamed via signed URLs (expiry: 4 hours)
3. **Encoding**: HLS (HTTP Live Streaming) with multiple quality levels:
   - 360p (500 Kbps) — mobile/slow connections
   - 720p (2.5 Mbps) — standard
   - 1080p (5 Mbps) — high quality
4. **Progressive loading**: Adaptive bitrate based on connection speed
5. **DRM**: Not in MVP; token-based access control sufficient for initial launch

---

## 7. Scaling Playbook

### 7.1 Scale-Up Path (Single Server)

Phase 1 (Launch):
```
Single VPS (8 vCPU, 16GB RAM):
├── Caddy (reverse proxy)
├── Next.js (port 3000)
├── ASP.NET Core API (port 5000)
├── PostgreSQL (port 5432)
├── Redis (port 6379)
├── Meilisearch (port 7700)
└── Hangfire (in-process)
```

### 7.2 Scale-Out Path (When Needed)

Phase 2 (500+ concurrent users):
```
VPS 1 (App):                 VPS 2 (Data):
├── Caddy                    ├── PostgreSQL (primary)
├── Next.js ×2               ├── Redis
└── ASP.NET Core API ×2      └── Meilisearch
```

Phase 3 (1000+ concurrent users):
```
Load Balancer
├── App Node 1                Data:
│   ├── Next.js              ├── PostgreSQL (primary + replica)
│   └── ASP.NET Core API     ├── Redis (Sentinel)
├── App Node 2                └── Meilisearch
│   ├── Next.js
│   └── ASP.NET Core API
└── Worker Node
    └── Hangfire
```

### 7.3 Scaling Triggers

| Metric | Threshold | Action |
|--------|:---------:|--------|
| CPU utilization | >70% sustained 5 min | Scale up or add node |
| Memory utilization | >80% | Scale up RAM |
| DB connections | >80 of 100 | Add read replica |
| Response time P95 | >500ms | Profile + optimize or scale |
| Disk usage | >70% | Expand storage or archive |
| Redis memory | >75% | Increase instance or evict |

---

## 8. Load Testing Plan

### 8.1 Test Scenarios (k6)

```javascript
// scenarios/peak-load.js
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  scenarios: {
    // Simulate typical usage pattern
    browsing: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '2m', target: 100 },   // Ramp up
        { duration: '5m', target: 200 },   // Sustained load
        { duration: '2m', target: 500 },   // Peak
        { duration: '2m', target: 0 },     // Ramp down
      ],
    },
    // Exam submission burst
    exam_burst: {
      executor: 'constant-arrival-rate',
      rate: 50,                             // 50 submissions/min
      timeUnit: '1m',
      duration: '3m',
      preAllocatedVUs: 100,
    },
  },
  thresholds: {
    http_req_duration: ['p(95)<500', 'p(99)<1000'],
    http_req_failed: ['rate<0.01'],         // <1% error rate
  },
};
```

### 8.2 Test Targets

| Scenario | Users | Duration | Success Criteria |
|----------|:-----:|:--------:|-----------------|
| Steady state | 100 | 10 min | P95 < 300ms, 0% errors |
| Peak load | 500 | 5 min | P95 < 500ms, <0.1% errors |
| Exam burst | 100 simultaneous submissions | 1 min | All scored correctly, P95 < 2s |
| Soak test | 200 | 1 hour | No memory leaks, stable P95 |

---

## 9. Monitoring & Performance Dashboards

| Dashboard | Key Metrics | Tool |
|-----------|-------------|------|
| **API Performance** | Request rate, latency percentiles, error rate | Grafana + Prometheus |
| **Database** | Query time, connection pool, slow query log | pg_stat_statements + Grafana |
| **Cache** | Hit/miss ratio, evictions, memory usage | Redis INFO + Grafana |
| **Frontend** | Core Web Vitals, JS errors, page load times | Vercel Analytics or self-hosted |
| **Infrastructure** | CPU, memory, disk, network | node_exporter + Grafana |

---

*Performance is a feature. The caching strategy eliminates redundant DB queries for the most common paths (course catalog, pricing, static pages). The scaling playbook ensures growth is handled methodically. Load testing validates production readiness before launch.*
