# 24 — Build vs Buy Decisions

*For each major subsystem, evaluate whether to build custom, use open-source, or buy SaaS — with rationale, cost, and reversibility analysis.*

---

## 1. Decision Framework

Each component is evaluated on four axes:

| Axis | Build Custom | Open-Source / Self-Hosted | Buy SaaS |
|------|:-------------|:--------------------------|:----------|
| **Control** | Full | High | Low |
| **Cost (Year 1)** | Developer time | Developer time + hosting | Subscription |
| **Maintenance** | You own all bugs | You own infra; community owns code | Vendor owns it |
| **Time to Market** | Slowest | Medium | Fastest |
| **Reversibility** | N/A | Easy (code is yours) | Hard (vendor lock-in) |

**Decision Priority**: Open-Source Self-Hosted → Build Custom (core IP) → Buy SaaS (commodity).

---

## 2. Decision Register

### D-BB01: Authentication & Identity

| Option | Tool | Monthly Cost | Verdict |
|--------|------|:------------|:-------:|
| Build | ASP.NET Core Identity + JWT | $0 | **Selected** |
| Open-Source | Keycloak | $0 (hosting) | Considered |
| Buy | Auth0 / Clerk | $25-200/mo | Rejected |

**Rationale**: Authentication is core to an LMS. ASP.NET Core Identity is mature, well-documented, and gives full control over user model, password policies, and role-based auth. Keycloak adds complexity without benefit for a single-application deployment. SaaS auth adds vendor lock-in and recurring cost for a feature that's straightforward to implement.

**Reversibility**: High — standard JWT tokens are interoperable.

---

### D-BB02: Payment Processing

| Option | Tool | Transaction Cost | Verdict |
|--------|------|:----------------|:-------:|
| Build | Direct bank API integration | 0% (bank charges) | Rejected (complexity) |
| Open-Source | — | — | N/A |
| Buy | JazzCash + Easypaisa + Stripe SDKs | 2-3% per transaction | **Selected** |

**Rationale**: Payment processing is a commodity that requires PCI compliance, fraud detection, and regulatory adherence. Building custom payment infrastructure is neither practical nor safe. The gateway abstraction pattern (`IPaymentGateway`) allows swapping providers without code changes.

**Reversibility**: Medium — gateway abstraction layer isolates vendor specifics.

---

### D-BB03: Search Engine

| Option | Tool | Monthly Cost | Verdict |
|--------|------|:------------|:-------:|
| Build | PostgreSQL full-text search | $0 | Fallback |
| Open-Source | Meilisearch (self-hosted) | $0 (hosting) | **Selected** |
| Open-Source | Elasticsearch | $0 (hosting, high RAM) | Rejected |
| Buy | Algolia | $35-250/mo | Rejected |

**Rationale**: Meilisearch provides instant search with typo tolerance, is lightweight (< 100 MB RAM), and has an excellent developer experience. Elasticsearch is overkill for our dataset size (20K MCQs, ~200 courses). Algolia adds cost for a feature that self-hosted Meilisearch handles perfectly. PostgreSQL FTS is the fallback if Meilisearch fails.

**Reversibility**: High — search index is derived data; can regenerate from DB.

---

### D-BB04: Email Delivery

| Option | Tool | Monthly Cost | Verdict |
|--------|------|:------------|:-------:|
| Build | Custom SMTP server | $0 + reputation risk | Rejected |
| Open-Source | — | — | N/A |
| Buy | Resend (transactional) | Free tier → $20/mo | **Selected** |
| Buy | Mailchimp / SendGrid | $15-50/mo | Considered |

**Rationale**: Email deliverability is a solved problem that shouldn't be self-hosted. Resend offers modern API, React Email templates, and a generous free tier (3,000 emails/month). SendGrid is an alternative. The `IEmailSender` abstraction allows swapping providers.

**Reversibility**: High — SMTP is a standard protocol; any provider works.

---

### D-BB05: Object Storage (Files, Images, Videos)

| Option | Tool | Monthly Cost | Verdict |
|--------|------|:------------|:-------:|
| Build | Local file system | $0 | Dev only |
| Open-Source | MinIO (self-hosted) | $0 (hosting) | Dev/Staging |
| Buy | Cloudflare R2 | $0 (10 GB free) → $0.015/GB | **Selected** |
| Buy | AWS S3 | $0.023/GB + egress | Considered |

**Rationale**: Cloudflare R2 is S3-compatible with zero egress fees, which is critical for video-heavy LMS content. MinIO serves as the local/staging equivalent. The `IStorageService` abstraction targets S3 API, making all three interchangeable.

**Reversibility**: High — S3 API is a de facto standard.

---

### D-BB06: Database

| Option | Tool | Monthly Cost | Verdict |
|--------|------|:------------|:-------:|
| Build | — | — | N/A |
| Open-Source | PostgreSQL 17 (self-hosted) | $0 (hosting) | **Selected** |
| Open-Source | SQL Server Express | $0 (limited) | Rejected |
| Buy | Neon / Supabase Postgres | $0-25/mo | Future option |

**Rationale**: Decision D-001. PostgreSQL is free, feature-rich (JSONB, full-text search, row-level security), and the most portable relational database. Self-hosting avoids vendor lock-in and recurring costs. Managed PostgreSQL (Neon) is a future scaling option.

**Reversibility**: Medium — EF Core supports provider switching, but migrations are provider-specific.

---

### D-BB07: Caching

| Option | Tool | Monthly Cost | Verdict |
|--------|------|:------------|:-------:|
| Build | In-memory (IMemoryCache) | $0 | Dev fallback |
| Open-Source | Redis 7 (self-hosted) | $0 (hosting) | **Selected** |
| Buy | Redis Cloud / Upstash | $0-10/mo | Future option |

**Rationale**: Decision D-005. Redis is the industry standard for caching, session storage, and pub/sub (SignalR backplane). Self-hosted Redis has minimal overhead and zero cost. `IDistributedCache` abstraction allows falling back to in-memory in development.

**Reversibility**: High — standard Redis protocol.

---

### D-BB08: Background Job Processing

| Option | Tool | Monthly Cost | Verdict |
|--------|------|:------------|:-------:|
| Build | Hosted services + channels | $0 | Rejected (no dashboard) |
| Open-Source | Hangfire (self-hosted) | $0 | **Selected** |
| Open-Source | Quartz.NET | $0 | Considered |
| Buy | Azure Functions | Pay-per-use | Rejected |

**Rationale**: Hangfire provides a dashboard for monitoring, supports recurring jobs (CRON), delayed jobs, and fire-and-forget. Uses PostgreSQL for storage (no additional infrastructure). Quartz.NET lacks the dashboard. Azure Functions add cloud dependency.

**Reversibility**: Medium — job definitions are application code; changing scheduler requires refactoring job registration.

---

### D-BB09: Logging & Monitoring

| Option | Tool | Monthly Cost | Verdict |
|--------|------|:------------|:-------:|
| Build | File logging | $0 | Rejected |
| Open-Source | Serilog + Seq + Prometheus + Grafana | $0 (all self-hosted) | **Selected** |
| Buy | Datadog | $15/host/mo | Rejected |
| Buy | Application Insights | Pay-per-use | Rejected |

**Rationale**: The full observability stack (Serilog → Seq for logs, Prometheus → Grafana for metrics) is free and self-hosted. This gives complete control over retention, querying, and alerting without recurring costs. Seq's free solo license is sufficient. Commercial APM tools are justified only at larger scale.

**Reversibility**: High — OpenTelemetry is a standard; exporters can be swapped.

---

### D-BB10: Reverse Proxy & TLS

| Option | Tool | Monthly Cost | Verdict |
|--------|------|:------------|:-------:|
| Build | Kestrel direct | $0 | Rejected |
| Open-Source | Caddy | $0 | **Selected** |
| Open-Source | Nginx | $0 | Considered |
| Buy | Cloudflare proxy | $0 (free tier) | Layered on top |

**Rationale**: Decision D-003. Caddy provides automatic HTTPS via Let's Encrypt, HTTP/2, and simple configuration. Nginx requires manual Let's Encrypt setup. Caddy runs as a Docker container alongside the application. Cloudflare proxy can be layered in front for DDoS protection.

**Reversibility**: High — reverse proxy is infrastructure, not application code.

---

### D-BB11: Real-Time Communication

| Option | Tool | Monthly Cost | Verdict |
|--------|------|:------------|:-------:|
| Build | Raw WebSockets | $0 | Rejected (complexity) |
| Open-Source | SignalR (ASP.NET Core) | $0 | **Selected** |
| Open-Source | Socket.IO | $0 | Rejected (not .NET native) |
| Buy | Pusher / Ably | $25-50/mo | Rejected |

**Rationale**: SignalR is built into ASP.NET Core, supports WebSocket with automatic fallback, and integrates natively with the authentication system. Redis backplane enables multi-instance scaling. No reason to use a third-party service.

**Reversibility**: Low — SignalR is tightly coupled to ASP.NET Core (but that's the backend framework).

---

### D-BB12: PDF Generation (Certificates, Reports)

| Option | Tool | Monthly Cost | Verdict |
|--------|------|:------------|:-------:|
| Build | HTML-to-PDF pipeline | $0 | **Selected** |
| Open-Source | QuestPDF | $0 (free for revenue < $1M) | **Selected** |
| Open-Source | Puppeteer/Playwright | $0 (heavy runtime) | Considered |
| Buy | Aspose.PDF | $1,000/year license | Rejected (cost) |

**Rationale**: QuestPDF provides a fluent C# API for creating PDFs programmatically — ideal for certificates and reports. V1 uses Aspose.Words which requires an expensive license. QuestPDF is free for businesses under $1M revenue and produces high-quality output.

**Reversibility**: Medium — PDF generation is isolated behind a service interface.

---

### D-BB13: Video Hosting & Streaming

| Option | Tool | Monthly Cost | Verdict |
|--------|------|:------------|:-------:|
| Build | Self-hosted HLS transcoding | $0 + CPU cost | Phase 2 |
| Open-Source | — | — | N/A |
| Buy | YouTube Private/Unlisted | $0 | **Selected (MVP)** |
| Buy | Bunny Stream / Mux | $5-50/mo | Phase 2 option |

**Rationale**: V1 currently uses YouTube embeds. For MVP, continue with YouTube (zero cost, proven delivery). In Phase 2, evaluate Bunny Stream for DRM protection and analytics. Self-hosted HLS transcoding is only justified at scale.

**Reversibility**: High — video links are stored as references; provider can change.

---

### D-BB14: Frontend Framework

| Option | Tool | Monthly Cost | Verdict |
|--------|------|:------------|:-------:|
| Build | Vanilla JS/HTML | $0 | Rejected |
| Open-Source | Next.js 15 (App Router) | $0 | **Selected** |
| Open-Source | Remix / SvelteKit | $0 | Considered |
| Buy | — | — | N/A |

**Rationale**: Decision D-002. Next.js provides SSR, SSG, ISR, middleware, image optimization, and the largest React ecosystem. The App Router with React Server Components reduces client-side JavaScript. Self-hosted (not Vercel) to avoid vendor lock-in.

**Reversibility**: Low — frontend framework is a foundational choice.

---

## 3. Cost Summary (Year 1)

| Category | Tool | Monthly Cost | Annual Cost |
|----------|------|:------------|:------------|
| Auth | ASP.NET Core Identity | $0 | $0 |
| Payments | JazzCash + Easypaisa + Stripe | 2-3% per transaction | Variable |
| Search | Meilisearch | $0 | $0 |
| Email | Resend | $0 → $20 | $0-240 |
| Storage | Cloudflare R2 | ~$5 | ~$60 |
| Database | PostgreSQL | $0 | $0 |
| Cache | Redis | $0 | $0 |
| Jobs | Hangfire | $0 | $0 |
| Monitoring | Serilog + Seq + Prometheus + Grafana | $0 | $0 |
| Proxy | Caddy | $0 | $0 |
| Real-Time | SignalR | $0 | $0 |
| PDF | QuestPDF | $0 | $0 |
| Video | YouTube (MVP) | $0 | $0 |
| Frontend | Next.js | $0 | $0 |
| **VPS Hosting** | **Contabo / Hetzner** | **$15-30** | **$180-360** |
| **Domain + DNS** | **Cloudflare** | **~$1** | **~$12** |
| **Total (excl. transactions)** | | **$20-55/mo** | **$250-670/yr** |

---

## 4. Decisions Deferred to Phase 2+

| Component | Current | Phase 2+ Candidate | Trigger |
|-----------|---------|---------------------|---------|
| Video hosting | YouTube embeds | Bunny Stream / Mux | Need DRM or analytics |
| Email marketing | None | Loops.so / custom | Need drip campaigns |
| SMS notifications | None | Twilio / local provider | User demand |
| CDN | Cloudflare (free) | Cloudflare Pro | Traffic > 100K/month |
| Database hosting | Self-hosted | Neon / Supabase | Scale beyond single VPS |
| A/B testing | None | PostHog / custom | Product optimization phase |

---

*The build-vs-buy philosophy for V2 is: **own your core, delegate the commodity**. Authentication, business logic, and the learning experience are core. Email delivery, payment processing, and video hosting are commodity. Self-hosted open-source sits in between — giving control without recurring cost.*
