# 36 — Assumptions and Open Decisions

*What this blueprint assumes to be true, and decisions that need validation before or during implementation.*

---

## 1. Assumptions

### 1.1 Business Assumptions

| # | Assumption | Risk if Wrong | Validation |
|:-:|-----------|---------------|------------|
| A-01 | The founder will dedicate ~40 hours/week to V2 development | Timeline doubles if part-time | Confirm availability before Phase 0 |
| A-02 | Current revenue from V1 can sustain operations during V2 build | V2 project abandoned mid-build | Verify V1 revenue covers hosting + living expenses for 7+ months |
| A-03 | Existing students will migrate to V2 when provided a seamless login experience | User churn if migration is friction-heavy | Dual-hash password strategy; extensive testing |
| A-04 | The 6 existing subscription packages and pricing remain valid for V2 | Pricing page mismatches reality | Confirm with stakeholder before building pricing page |
| A-05 | JazzCash and Easypaisa will continue as primary payment channels for Pakistan | Must integrate alternative gateways | Verify gateway contracts are active; check for new options |
| A-06 | YouTube embeds are acceptable for video delivery in MVP | Copyright takedowns or playback issues | Verify YouTube TOS compliance; test with existing content |
| A-07 | The 13 exam tracks map cleanly to V2 course catalog structure | Data model needs revision | Validate V1 track-course relationships before migration |
| A-08 | Ambassador commission rates and rules are documented or discoverable | Ambassador program misimplemented | Query V1 database for commission rates; confirm with founder |

### 1.2 Technical Assumptions

| # | Assumption | Risk if Wrong | Validation |
|:-:|-----------|---------------|------------|
| A-09 | The V1 SQL Server database is accessible for migration | Cannot migrate data | Verify connection string, credentials, and backup access |
| A-10 | V1 uses ASP.NET Identity v2 with PBKDF2 password hashing | Dual-hash strategy fails | Inspect PasswordHash format in AspNetUsers table |
| A-11 | A single VPS (8 GB RAM, 4 vCPU) can run all V2 services | Performance issues from day one | Load test with expected traffic before launch |
| A-12 | Docker Compose is sufficient for production (no Kubernetes needed) | Scaling limitations | Acceptable for < 500 concurrent users; reassess at scale |
| A-13 | PostgreSQL 17 handles all V1 data types without lossy conversion | Data integrity issues post-migration | Test migration with full V1 data dump on staging |
| A-14 | EF Core 9 Code-First generates efficient queries for all use cases | Performance bottlenecks in ORM | Use raw SQL for complex aggregations; monitor query performance |
| A-15 | Cloudflare R2 egress-free storage is cost-effective for video files | Storage costs escalate unexpectedly | Calculate estimated storage needs (course images + documents) |
| A-16 | GitHub Actions free tier (2,000 minutes/month) is sufficient for CI/CD | Need paid plan or self-hosted runner | Optimize build times; use caching; monitor usage |

### 1.3 Content Assumptions

| # | Assumption | Risk if Wrong | Validation |
|:-:|-----------|---------------|------------|
| A-17 | All 20,000+ MCQs have a single correct answer out of 4 options | Exam engine needs multi-answer support | Query V1 MCQ data structure; confirm format |
| A-18 | Course content is primarily text + YouTube videos (no interactive simulations) | Need additional content types | Audit V1 course content types |
| A-19 | Existing course images are < 10 MB each and can be served from S3 | Large files need special handling | Check V1 image sizes and formats |
| A-20 | Medical content doesn't require special accessibility beyond WCAG AA | Regulatory compliance issues | Research medical education accessibility standards |

---

## 2. Open Decisions

### 2.1 Must Decide Before Phase 0

| # | Decision | Options | Factors | Deadline |
|:-:|----------|---------|---------|:--------:|
| OD-01 | VPS provider | Contabo ($5/8GB) / Hetzner ($7/8GB) / DigitalOcean ($12/8GB) | Cost, latency to Pakistan, reliability | Week 1 Day 1 |
| OD-02 | Domain strategy for V2 | Same domain (firstaidmadeeasy.com.pk) / Subdomain (v2.firstaidmadeeasy.com.pk) / New domain | SEO, DNS cutover complexity, student confusion | Week 1 Day 1 |
| OD-03 | Git hosting | GitHub (private) / GitLab / Bitbucket | CI/CD integration, cost, familiarity | Week 1 Day 1 |
| OD-04 | Staging environment | Same VPS (Docker) / Separate VPS / Local only | Cost, testing fidelity | Week 1 |

### 2.2 Must Decide Before Phase 1

| # | Decision | Options | Factors | Deadline |
|:-:|----------|---------|---------|:--------:|
| OD-05 | Email delivery provider | Resend / SendGrid / SMTP2GO / Local SMTP | Cost, deliverability, API quality | Sprint 1 |
| OD-06 | V1 → V2 migration approach | Big-bang weekend cutover / Gradual (V1 + V2 parallel) | Complexity, risk, user experience | Sprint 4 |
| OD-07 | V1 feature bridge strategy | Redirect to V1 for exams / Disable exams until Phase 2 / Accelerate exam engine | User impact, development time | Sprint 5 |
| OD-08 | SSL certificate approach | Caddy auto-ACME / Cloudflare origin certificate / Let's Encrypt certbot | Automation, Cloudflare dependency | Sprint 5 |

### 2.3 Must Decide Before Phase 2

| # | Decision | Options | Factors | Deadline |
|:-:|----------|---------|---------|:--------:|
| OD-09 | Exam engine: timed vs untimed default | Timed (like V1) / Both modes / Untimed-only for practice | Student preference, exam fidelity | Sprint 6 |
| OD-10 | Spaced repetition: automatic vs opt-in | Auto-enroll all students / Opt-in feature / Both | Engagement, notification fatigue | Sprint 8 |
| OD-11 | Chat implementation | Real-time (SignalR) / Async (like email) / Both | Complexity, student expectations, support staffing | Sprint 7 |

### 2.4 Can Decide Later

| # | Decision | Options | Factors | Deadline |
|:-:|----------|---------|---------|:--------:|
| OD-12 | Video hosting upgrade path | Bunny Stream / Mux / Cloudflare Stream / Self-hosted HLS | Cost, DRM needs, analytics | Phase 3+ |
| OD-13 | Mobile app technology | React Native (Expo) / Flutter / PWA only | Team skills, feature parity needs | Phase 4 |
| OD-14 | Multi-language strategy | i18next / next-intl / Custom | Scope of Urdu content, RTL support | Phase 4 |
| OD-15 | Self-hosted vs managed database | Keep self-hosted / Migrate to Neon / Supabase | Scale needs, ops overhead, cost | When DB > 50 GB |
| OD-16 | Monitoring: self-hosted vs SaaS | Keep Seq + Grafana / Move to Datadog or New Relic | Team size, cost justification | When team > 3 |

---

## 3. Dependencies

### 3.1 External Dependencies

| Dependency | Required For | Risk | Mitigation |
|-----------|-------------|------|------------|
| JazzCash merchant account | Payment processing | Account approval delay | Apply early; have manual enrollment fallback |
| Easypaisa merchant account | Payment processing | Account approval delay | Apply early; have manual enrollment fallback |
| Stripe account (Pakistan) | International payments | Stripe may not support Pakistan directly | Use Stripe Atlas or alternative international gateway |
| Google OAuth credentials | Social login | Low risk | Configure in Google Cloud Console |
| Domain DNS access | SSL and deployment | Low risk | Ensure DNS registrar access documented |
| V1 SQL Server access | Data migration | Medium risk | Verify credentials and connectivity early |
| YouTube API (optional) | Video metadata | Low risk | Only needed for enhanced features; embeds work without API |

### 3.2 Internal Dependencies

| Dependency | Required For | Owner |
|-----------|-------------|-------|
| V1 database schema documentation | Migration script | Developer (analyze V1 DB) |
| Complete list of V1 URL patterns | 301 redirect map | Developer (crawl V1 site) |
| Ambassador commission rates | Ambassador module | Founder (business decision) |
| Course content inventory | Content migration | Founder (identify what content exists) |
| Payment gateway credentials | Checkout integration | Founder (business accounts) |

---

## 4. Constraints

| Constraint | Impact | Accept / Mitigate |
|-----------|--------|-------------------|
| No source code for V1 | Cannot inspect business logic; infer from DB + views | Accept — blueprint designed around this |
| Solo developer for Phase 0-1 | Limited velocity; no peer review | Accept — AI-assisted; comprehensive docs |
| Budget-sensitive | Must minimize SaaS subscriptions | Accept — open-source stack chosen |
| Pakistan primary market | Payment gateways limited; latency to international CDNs | Mitigate — VPS in nearest region; CDN for static |
| Medical domain | Content accuracy required; potential regulatory concerns | Accept — teachers validate content; legal TOS review |

---

## 5. Decision Log Template

When open decisions are resolved, record them here:

```
Decision: OD-XX
Date: YYYY-MM-DD
Chosen Option: [selected option]
Rationale: [why this option was chosen]
Decided By: [founder / developer / team]
Impact: [any changes to blueprint files needed]
```

---

*This file is meant to be a living document. As assumptions are validated (or invalidated) and decisions are made, update this file. Any invalidated assumption should trigger a review of dependent blueprint files.*
