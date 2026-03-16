# 04 — System Architecture and Technology Decisions

*Modular Monolith backend + Next.js 15 frontend. Every technology choice justified with tradeoffs.*

---

## 1. Architecture Overview

### 1.1 High-Level Architecture

```
┌──────────────────────────────────────────────────────────────────────┐
│                           INTERNET                                   │
└────────────────────────────────┬─────────────────────────────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │     Cloudflare CDN       │
                    │   (DNS + DDoS + Cache)   │
                    └────────────┬─────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │    Caddy Reverse Proxy   │
                    │   (Auto-TLS + Headers)   │
                    └───────┬─────────┬────────┘
                            │         │
               ┌────────────▼──┐ ┌────▼────────────┐
               │  Next.js 15   │ │  ASP.NET Core 9  │
               │  (Frontend)   │ │  (API Backend)   │
               │  Port 3000    │ │  Port 5000       │
               └──────┬────┬──┘ └──┬──────┬────────┘
                      │    │       │      │
                      │    │       │      │
         ┌────────────┘    │       │      └──────────────┐
         │                 │       │                      │
    ┌────▼────┐   ┌───────▼───┐  ┌▼──────────┐   ┌──────▼──────┐
    │ Meili-  │   │ Redis 7   │  │PostgreSQL  │   │ S3 Storage  │
    │ search  │   │ (Cache +  │  │   17       │   │(Cloudflare  │
    │         │   │  Session) │  │            │   │   R2)       │
    └─────────┘   └───────────┘  └────────────┘   └─────────────┘
```

### 1.2 Architecture Style: Modular Monolith

**Decision**: Modular Monolith with vertical slice architecture.

**Why not Microservices?**
- Team size (3-8 people) doesn't justify the operational overhead
- Single database reduces distributed transaction complexity
- Shared deployment simplifies DevOps for a resource-constrained team
- Module boundaries can be extracted to services later if needed (audit: RC-5)

**Why not Traditional Layered Monolith?**
- V1's layered architecture (Controllers → BLL → DAL) created tight coupling
- Feature changes required touching 3+ layers across multiple projects
- Vertical slices keep all related code together per feature

**Module Boundaries**:
```
FAME.Api/
├── Modules/
│   ├── Identity/        ← Auth, users, roles, profiles
│   ├── Catalog/         ← Courses, sections, lectures, resources
│   ├── Assessment/      ← Questions, exams, results, analytics
│   ├── Enrollment/      ← Subscriptions, packages, access rules
│   ├── Payment/         ← Gateways, transactions, invoices, coupons
│   ├── Communication/   ← Emails, notifications, announcements
│   ├── Certificate/     ← Generation, templates, verification
│   ├── Ambassador/      ← Referrals, commissions, payouts
│   ├── Content/         ← Blog, CMS pages, FAQ, help articles
│   ├── Analytics/       ← Reports, dashboards, exports
│   └── Admin/           ← System config, feature flags, audit log
└── SharedKernel/        ← Common types, interfaces, base classes
```

Each module is a folder with its own:
- `Features/` — Vertical slices (one folder per use case)
- `Domain/` — Entities and value objects
- `Infrastructure/` — EF configurations, external service clients
- Module registration via `IServiceCollection` extension method

Modules communicate via:
- Shared interfaces in `SharedKernel`
- Domain events (MediatR `INotification`)
- Never direct entity references across modules (use IDs)

---

## 2. Technology Stack — Detailed Decisions

### 2.1 Frontend: Next.js 15

| Aspect | Decision | Rationale |
|--------|----------|-----------|
| **Framework** | Next.js 15 (App Router) | SSR/SSG for SEO, React Server Components for performance, industry-leading DX |
| **Language** | TypeScript 5 (strict mode) | Type safety, refactoring confidence, better DX |
| **Styling** | Tailwind CSS 4 | Atomic utility classes, small bundle, design token support |
| **UI Primitives** | Radix UI | Accessible, unstyled, composable. No opinionated design decisions. |
| **State Management** | Zustand (client) + TanStack Query (server) | Zustand: minimal, no boilerplate. TanStack Query: cache, deduplication, optimistic updates. |
| **Forms** | React Hook Form + Zod | Performant (uncontrolled), schema-based validation, shared with backend. |
| **Charts** | Recharts | React-native charting, composable, lightweight. |
| **Rich Text** | TipTap | ProseMirror-based, extensible, headless. For blog/CMS editing. |
| **Monorepo** | Turborepo | Task caching, parallel execution, incremental builds. |

**Tradeoff Considered**: Remix vs Next.js
- Remix has stronger progressive enhancement story
- Next.js has larger ecosystem, more deployment options, better SSG story
- Team familiarity with React + Next.js ecosystem is higher in hiring market
- **Decision**: Next.js 15

### 2.2 Backend: ASP.NET Core 9

| Aspect | Decision | Rationale |
|--------|----------|-----------|
| **Framework** | ASP.NET Core 9 / .NET 9 | Performance leader, C# team expertise, mature ecosystem |
| **Language** | C# 13 | Records for DTOs, pattern matching, null safety |
| **ORM** | EF Core 9 (Code-First) | LINQ, migrations, interceptors, compiled queries |
| **Auth** | ASP.NET Core Identity + JWT | Battle-tested identity system, JWT for API auth |
| **Validation** | FluentValidation | Expressive rules, testable, separation of concerns |
| **Mediator** | MediatR | CQRS-light: commands/queries as objects, pipeline behaviors |
| **Background Jobs** | Hangfire | Persistent jobs, dashboard, retry policies, cron scheduling |
| **Logging** | Serilog → Seq | Structured logging, rich querying, .NET native |
| **Mapping** | Mapster | Code-gen mapping, faster than AutoMapper, less configuration |

**Tradeoff Considered**: Node.js/Express vs ASP.NET Core
- Node.js would unify frontend/backend language (TypeScript)
- ASP.NET Core is significantly faster for CPU-bound work (exam scoring, report generation)
- C# has superior tooling for domain modeling (records, pattern matching, strong typing)
- Existing team has C# expertise from v1
- **Decision**: ASP.NET Core 9

### 2.3 Database: PostgreSQL 17

| Aspect | Decision | Rationale |
|--------|----------|-----------|
| **Engine** | PostgreSQL 17 | Open-source, no licensing cost, feature-rich |
| **Driver** | Npgsql (via EF Core) | Native .NET PostgreSQL driver, high performance |
| **Migrations** | EF Core Migrations | Version-controlled schema, up/down scripts |
| **JSON Support** | `jsonb` columns | Flexible metadata storage (exam config, audit data) |
| **Full-Text** | PostgreSQL FTS + Meilisearch | PG for basic search, Meilisearch for fast UI search |

**Tradeoff: PostgreSQL vs SQL Server**
| Factor | PostgreSQL | SQL Server |
|--------|-----------|------------|
| License cost | Free | $3,000+/core or Azure SQL |
| JSON support | Native `jsonb` with indexing | Limited JSON functions |
| Linux Docker | Native, lightweight | Larger image, more memory |
| Community | Largest OSS database community | Microsoft ecosystem |
| EF Core support | Excellent (Npgsql) | Excellent (native) |
| V1 migration | Requires data migration | Direct compatibility |

**Decision**: PostgreSQL. The licensing cost savings alone justify the migration effort. The `jsonb` support enables flexible metadata storage without schema changes. Better Docker support reduces operational complexity.

### 2.4 Cache & Session: Redis 7

| Aspect | Decision | Rationale |
|--------|----------|-----------|
| **Use Cases** | Session store, API response cache, rate limiting, pub/sub for real-time | Unified cache layer, persistent sessions |
| **Session** | Redis-backed distributed sessions | Survives app restarts, enables horizontal scaling |
| **Cache Strategy** | Cache-aside with configurable TTLs | Simple, predictable, easy to invalidate |
| **Rate Limiting** | Redis sliding window | Accurate, distributed, ASP.NET Core middleware integration |

**Tradeoff: Redis vs In-Memory Cache**
- In-memory is simpler but lost on restart and not shareable across instances
- Redis adds one more service to manage but enables session persistence and future scaling
- **Decision**: Redis from day one to avoid painful migration later

### 2.5 Search: Meilisearch

| Aspect | Decision | Rationale |
|--------|----------|-----------|
| **Use Cases** | Course search, question bank search, help article search | Instant results for student-facing search |
| **Sync Strategy** | EF Core SaveChanges interceptor → queue → Meilisearch indexer | Near-real-time, decoupled from write path |
| **Indexes** | `courses`, `questions`, `articles` | Three primary search indexes |

**Tradeoff: Meilisearch vs Elasticsearch vs Algolia**
| Factor | Meilisearch | Elasticsearch | Algolia |
|--------|-----------|--------------|---------|
| Memory usage | ~50MB | ~1GB+ | N/A (SaaS) |
| Setup complexity | Minimal (single binary) | Moderate (JVM, cluster) | Zero (SaaS) |
| Cost | Free (self-hosted) | Free (self-hosted) | $$$$ at scale |
| Typo tolerance | Excellent (built-in) | Good (configurable) | Excellent |
| Faceted search | Yes | Yes | Yes |

**Decision**: Meilisearch. Lightweight enough for single-VPS deployment, excellent typo tolerance for medical terminology, zero ongoing cost.

### 2.6 Object Storage: S3-Compatible

| Aspect | Decision | Rationale |
|--------|----------|-----------|
| **Provider** | Cloudflare R2 (primary) or MinIO (self-hosted fallback) | R2: no egress fees, S3-compatible. MinIO: zero-cost self-hosted alternative. |
| **Use Cases** | Course thumbnails, PDFs, certificates, user avatars, blog images | All file uploads go to object storage |
| **Access** | Signed URLs for private content, public bucket for thumbnails | Security by default, public only when needed |
| **CDN** | Cloudflare CDN in front of R2 | Automatic with R2, separate setup for MinIO |

### 2.7 Reverse Proxy: Caddy

| Aspect | Decision | Rationale |
|--------|----------|-----------|
| **TLS** | Automatic Let's Encrypt via Caddy | Zero-config HTTPS, auto-renewal |
| **Headers** | Security headers injected at proxy level | CSP, HSTS, X-Frame-Options — one config for all services |
| **Routing** | `/api/*` → ASP.NET Core, `/*` → Next.js | Clean separation, single entry point |
| **Compression** | Gzip + Brotli at proxy level | Offloads compression from application servers |

**Tradeoff: Caddy vs Nginx vs Traefik**
- Nginx requires manual TLS config and renewal scripts
- Traefik is Docker-native but more complex for simple setups
- Caddy: automatic TLS, simple config, Go-based (compiled, fast)
- **Decision**: Caddy for simplicity and auto-TLS

---

## 3. System Architecture Diagrams

### 3.1 C4 — Context Diagram

```
┌──────────────────────────────────────────────────────┐
│                    FAME V2 System                     │
│                                                      │
│  ┌──────────────┐   ┌──────────────┐                │
│  │  Next.js 15  │   │ ASP.NET Core │                │
│  │  Frontend    │◄──►  9 API       │                │
│  └──────────────┘   └──────────────┘                │
└──────────┬──────────────────┬────────────────────────┘
           │                  │
    ┌──────▼──────┐   ┌──────▼──────┐   ┌────────────┐
    │  Students   │   │   Admins    │   │  Payment   │
    │  (Browser)  │   │  (Browser)  │   │  Gateways  │
    └─────────────┘   └─────────────┘   │ (JazzCash, │
                                        │ Easypaisa, │
                                        │  Stripe)   │
                                        └────────────┘
    ┌─────────────┐   ┌─────────────┐   ┌────────────┐
    │  Email      │   │  YouTube    │   │  Cloudflare│
    │  (SMTP)     │   │  (Videos)   │   │  (CDN/R2)  │
    └─────────────┘   └─────────────┘   └────────────┘
```

### 3.2 C4 — Container Diagram

```
┌───────────────────────────────────────────────────────────────────┐
│                        VPS (Ubuntu 22.04)                          │
│                                                                   │
│  ┌────────────────────────────────────────────────────────────┐   │
│  │                Docker Compose                               │   │
│  │                                                            │   │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  │   │
│  │  │  Caddy   │  │ Next.js  │  │ ASP.NET  │  │ Hangfire │  │   │
│  │  │  :443    │─►│  :3000   │  │  :5000   │  │  Worker  │  │   │
│  │  │          │─►│          │  │          │  │          │  │   │
│  │  └──────────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘  │   │
│  │                     │             │              │        │   │
│  │  ┌──────────┐  ┌────▼─────┐  ┌───▼──────┐  ┌───▼──────┐ │   │
│  │  │  Meili-  │  │ Redis 7  │  │PostgreSQL│  │  Seq     │ │   │
│  │  │  search  │  │  :6379   │  │   :5432  │  │  :5341   │ │   │
│  │  │  :7700   │  │          │  │          │  │          │ │   │
│  │  └──────────┘  └──────────┘  └──────────┘  └──────────┘ │   │
│  └────────────────────────────────────────────────────────────┘   │
│                                                                   │
│  Volumes: pgdata, redis-data, meilisearch-data, seq-data          │
└───────────────────────────────────────────────────────────────────┘
```

### 3.3 Request Flow

```
Student Browser
     │
     ▼
Cloudflare CDN ──── Static assets served from edge cache
     │
     ▼ (cache miss)
Caddy (:443)
     │
     ├── /api/* ──► ASP.NET Core (:5000)
     │                    │
     │              ┌─────┴─────┐
     │              │           │
     │         PostgreSQL    Redis
     │              │
     │         Hangfire Worker (background)
     │
     └── /* ──► Next.js (:3000)
                    │
              ┌─────┴─────────┐
              │               │
         SSR/SSG         API calls to
         pages           ASP.NET Core
```

---

## 4. Data Flow Architecture

### 4.1 Payment Flow (Self-Service)

```
Student                Next.js              API                  Gateway         Hangfire
  │                      │                   │                      │               │
  │ Select Package       │                   │                      │               │
  │─────────────────────►│                   │                      │               │
  │                      │ POST /api/checkout│                      │               │
  │                      │──────────────────►│                      │               │
  │                      │                   │ Create PaymentIntent │               │
  │                      │                   │─────────────────────►│               │
  │                      │                   │ Redirect URL         │               │
  │                      │                   │◄─────────────────────│               │
  │                      │ Redirect Student  │                      │               │
  │                      │◄──────────────────│                      │               │
  │ Complete Payment     │                   │                      │               │
  │─────────────────────────────────────────────────────────────────►               │
  │                      │                   │ Webhook: paid        │               │
  │                      │                   │◄─────────────────────│               │
  │                      │                   │ Create Enrollment    │               │
  │                      │                   │──────────────────────────────────────►│
  │                      │                   │                      │  Send Welcome │
  │                      │                   │                      │  Email        │
  │ Redirect to Dashboard│                   │                      │               │
  │◄─────────────────────│                   │                      │               │
```

### 4.2 Exam Submission Flow

```
Student                Next.js              API                  PostgreSQL       Redis
  │                      │                   │                      │               │
  │ Submit Answers       │                   │                      │               │
  │─────────────────────►│                   │                      │               │
  │                      │ POST /api/exams   │                      │               │
  │                      │   /submit         │                      │               │
  │                      │──────────────────►│                      │               │
  │                      │                   │ Calculate Results    │               │
  │                      │                   │─────────────────────►│               │
  │                      │                   │ Store ExamAttempt    │               │
  │                      │                   │─────────────────────►│               │
  │                      │                   │ Invalidate Cache     │               │
  │                      │                   │──────────────────────────────────────►│
  │                      │                   │ Return ResultSummary │               │
  │                      │◄──────────────────│                      │               │
  │ Show Results         │                   │                      │               │
  │◄─────────────────────│                   │                      │               │
```

---

## 5. Cross-Cutting Concerns

### 5.1 Authentication & Authorization Flow

```
┌─────────────┐     ┌──────────────┐     ┌─────────────┐
│   Next.js   │────►│    Caddy     │────►│  ASP.NET    │
│  (httpOnly  │     │  (forwards   │     │   Core      │
│   cookie    │     │   cookie)    │     │ (validates  │
│   set on    │     │              │     │  JWT from   │
│   login)    │     │              │     │  cookie,    │
│             │     │              │     │  checks     │
│             │     │              │     │  policies)  │
└─────────────┘     └──────────────┘     └─────────────┘
                                                │
                                          ┌─────▼─────┐
                                          │   Redis   │
                                          │ (session  │
                                          │  store,   │
                                          │  revoked  │
                                          │  tokens)  │
                                          └───────────┘
```

- JWT access token (15 min) in httpOnly cookie
- Refresh token (7 days) in separate httpOnly cookie
- Token refresh handled automatically by Next.js middleware
- Token revocation list in Redis for immediate logout

### 5.2 Error Handling Strategy

| Layer | Strategy |
|-------|----------|
| **Caddy** | Custom error pages for 502/503/504 |
| **Next.js** | `error.tsx` boundaries per route segment |
| **API** | Global exception handler → `ProblemDetails` (RFC 7807) |
| **Business Logic** | Result pattern (never throw for expected failures) |
| **Database** | Retry with exponential backoff for transient failures |

### 5.3 Caching Strategy

| Content | Cache Location | TTL | Invalidation |
|---------|:-------------:|:---:|:------------:|
| Static assets (JS/CSS/images) | CDN + Browser | 1 year | Content-hash filenames |
| SSG pages (home, pricing, blog) | CDN | ISR 60s | On-demand revalidation |
| API responses (course list) | Redis | 5 min | Cache-aside, invalidate on write |
| User session | Redis | 15 min (sliding) | Explicit on logout |
| Search index | Meilisearch | Real-time | Sync on entity save |

---

## 6. Infrastructure Requirements

### 6.1 Minimum VPS Specification

| Resource | Minimum | Recommended | Notes |
|----------|:-------:|:-----------:|-------|
| **CPU** | 4 vCPU | 8 vCPU | ASP.NET Core + PostgreSQL + Meilisearch |
| **RAM** | 8 GB | 16 GB | PostgreSQL needs ~2GB, Redis ~512MB, Meilisearch ~512MB |
| **Storage** | 100 GB SSD | 200 GB SSD | Database + logs + backups |
| **Network** | 1 Gbps | 1 Gbps | Standard for most VPS providers |
| **OS** | Ubuntu 22.04 LTS | Ubuntu 22.04 LTS | Docker Compose deployment |

### 6.2 Estimated Monthly Cost

| Service | Provider | Cost |
|---------|----------|:----:|
| VPS (8 vCPU, 16GB) | Contabo / Hetzner | $15-30/mo |
| Domain + DNS | Cloudflare (free tier) | $0/mo |
| CDN | Cloudflare (free tier) | $0/mo |
| Object Storage | Cloudflare R2 (10GB free) | $0-5/mo |
| Email Sending | Resend (100/day free) or Mailgun | $0-15/mo |
| SSL Certificate | Let's Encrypt (via Caddy) | $0/mo |
| **Total** | | **$15-50/mo** |

Compared to V1's likely hosting costs (Windows Server with SQL Server licensing), V2 represents a significant cost reduction.

---

## 7. Decision Log

| # | Decision | Date | Alternatives Considered | Final Choice | Rationale |
|---|----------|------|------------------------|--------------|-----------|
| D-001 | Architecture style | 2026-03 | Microservices, Layered Monolith | Modular Monolith | Team size, deployment simplicity (RC-5) |
| D-002 | Frontend framework | 2026-03 | Remix, SvelteKit, Angular | Next.js 15 | Ecosystem, SSR/SSG, hiring pool |
| D-003 | Backend framework | 2026-03 | Node.js/Express, Go/Gin | ASP.NET Core 9 | Performance, C# expertise, domain modeling |
| D-004 | Database | 2026-03 | SQL Server, MySQL, MongoDB | PostgreSQL 17 | Cost, JSON support, Linux/Docker native |
| D-005 | Cache | 2026-03 | In-memory only, Memcached | Redis 7 | Session persistence, pub/sub, rate limiting |
| D-006 | Search | 2026-03 | Elasticsearch, Algolia, PG FTS only | Meilisearch | Lightweight, zero cost, typo tolerance |
| D-007 | Object storage | 2026-03 | Local filesystem, AWS S3 | Cloudflare R2 | No egress fees, S3-compatible, CDN integrated |
| D-008 | Reverse proxy | 2026-03 | Nginx, Traefik, HAProxy | Caddy | Auto-TLS, simple config, Go-based |
| D-009 | Monorepo tool | 2026-03 | Nx, Lerna, pnpm workspaces only | Turborepo | Simple, fast, Vercel-maintained, good caching |
| D-010 | Background jobs | 2026-03 | Quartz.NET, custom worker, RabbitMQ | Hangfire | Dashboard, persistence, retry, cron, .NET native |

---

*This architecture document serves as the technical foundation for all subsequent design documents. Implementation details for frontend, backend, database, security, and DevOps are expanded in their respective files (05-13).*
