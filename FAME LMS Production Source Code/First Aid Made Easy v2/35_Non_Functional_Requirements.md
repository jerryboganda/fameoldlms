# 35 — Non-Functional Requirements

*Measurable performance, reliability, security, and usability targets that V2 must meet.*

---

## 1. Performance

### 1.1 Response Time

| Category | P50 Target | P95 Target | P99 Target | Measurement |
|----------|:----------:|:----------:|:----------:|-------------|
| Static pages (SSG) | < 50 ms | < 100 ms | < 200 ms | Caddy / CDN served |
| Server-rendered pages (SSR) | < 200 ms | < 500 ms | < 1000 ms | Next.js render time |
| API reads (single entity) | < 50 ms | < 100 ms | < 200 ms | Endpoint response time |
| API reads (paginated list) | < 100 ms | < 200 ms | < 500 ms | Includes DB query |
| API writes (create/update) | < 100 ms | < 300 ms | < 500 ms | Includes validation + persist |
| Search queries | < 30 ms | < 50 ms | < 100 ms | Meilisearch response |
| Exam submission + scoring | < 500 ms | < 1000 ms | < 2000 ms | Calculate + persist result |
| Payment webhook processing | < 1000 ms | < 2000 ms | < 3000 ms | Verify + activate subscription |

### 1.2 Throughput

| Metric | Target | Notes |
|--------|:------:|-------|
| Concurrent users | 500 | Simultaneous active sessions |
| Requests per second | 1,000 | Sustained under normal load |
| Peak requests per second | 2,500 | During enrollment spikes |
| WebSocket connections | 1,000 | SignalR persistent connections |
| Background jobs per minute | 100 | Hangfire processing rate |

### 1.3 Core Web Vitals

| Metric | Target | Measurement |
|--------|:------:|-------------|
| Largest Contentful Paint (LCP) | < 2.5s | 75th percentile |
| First Input Delay (FID) | < 100ms | 75th percentile |
| Cumulative Layout Shift (CLS) | < 0.1 | 75th percentile |
| Time to First Byte (TTFB) | < 200ms | Server response |
| First Contentful Paint (FCP) | < 1.8s | 75th percentile |
| Total Blocking Time (TBT) | < 200ms | Lab metric |

### 1.4 Bundle Size

| Asset | Target | Enforcement |
|-------|:------:|-------------|
| Initial JS bundle | < 100 KB (gzipped) | Webpack analyzer in CI |
| Per-route JS | < 50 KB (gzipped) | Dynamic imports, tree shaking |
| CSS | < 30 KB (gzipped) | Tailwind purge |
| Total page weight (home) | < 500 KB | Lighthouse audit |

---

## 2. Reliability & Availability

| Metric | Target | Notes |
|--------|:------:|-------|
| Uptime | 99.5% | ~44 hours downtime/year max |
| Planned maintenance window | < 2 hours/month | Weekend nights (PKT) |
| Unplanned downtime per incident | < 30 minutes | Target MTTR |
| Recovery Time Objective (RTO) | < 1 hour | Full system restore |
| Recovery Point Objective (RPO) | < 1 hour | Max data loss window |
| Error rate (5xx) | < 0.5% | Of total requests |
| Error rate (4xx client) | < 5% | Expected from validation |
| Database backup frequency | Daily full + continuous WAL | Point-in-time recovery |
| Backup retention | 30 days | Rolling window |

---

## 3. Scalability

| Dimension | Current Capacity | Scaling Trigger | Scaling Action |
|-----------|:---------------:|:----------------|:---------------|
| Users | 10,000 | 8,000 active | Add Redis cluster |
| Courses | 500 | 400 courses | Optimize catalog queries |
| MCQ questions | 100,000 | 80,000 questions | Partition question table |
| Concurrent exams | 200 | 150 simultaneous | Add API instance |
| File storage | 100 GB | 80 GB used | Expand R2 bucket |
| Database size | 50 GB | 40 GB | Optimize, archive old data |

---

## 4. Security

### 4.1 Authentication

| Requirement | Specification |
|-------------|--------------|
| Password minimum length | 10 characters |
| Password complexity | At least 1 uppercase, 1 lowercase, 1 digit, 1 special |
| Password hashing | Argon2id (memory: 64 MB, iterations: 3, parallelism: 1) |
| Account lockout | 5 failed attempts → 15-minute lockout |
| JWT access token lifetime | 15 minutes |
| JWT refresh token lifetime | 30 days |
| Token delivery | httpOnly, Secure, SameSite=Strict cookies |
| Session invalidation | Refresh token rotation; single-use tokens |

### 4.2 Data Protection

| Requirement | Specification |
|-------------|--------------|
| Encryption at rest | PostgreSQL volume encryption (LUKS/dm-crypt) |
| Encryption in transit | TLS 1.2+ (enforced by Caddy) |
| PII access logging | All admin access to student data is audit-logged |
| Data masking | Phone numbers, emails masked in admin lists |
| Backup encryption | AES-256 encrypted backups |
| Secret storage | Environment variables; never in source code |

### 4.3 Input Validation

| Requirement | Specification |
|-------------|--------------|
| All API inputs | Validated via FluentValidation before processing |
| File uploads | Magic byte verification + extension whitelist |
| Max upload size | 10 MB (images), 50 MB (documents) |
| Rate limiting | 100 req/min anonymous, 300 req/min authenticated |
| Auth rate limiting | 5 login attempts per 15 minutes per IP |
| SQL injection | Parameterized queries via EF Core (no raw concatenation) |
| XSS prevention | React auto-escaping + CSP headers |

### 4.4 Compliance

| Standard | Status | Notes |
|----------|:------:|-------|
| OWASP Top 10 (2021) | Full compliance | All 10 categories addressed (file 12) |
| PCI DSS (payment data) | Not applicable | No card data stored; gateways handle PCI |
| HTTPS everywhere | Required | HTTP → HTTPS redirect enforced |
| Cookie security | Required | httpOnly, Secure, SameSite flags |
| CORS policy | Restricted | Only configured origins allowed |

---

## 5. Usability

| Requirement | Target | Measurement |
|-------------|:------:|-------------|
| Accessibility (WCAG) | AA compliance | Axe audit in CI |
| Keyboard navigation | Full support | All interactive elements reachable |
| Screen reader support | Labels on all controls | aria attributes on all components |
| Color contrast ratio | ≥ 4.5:1 (normal text) | Automated contrast checker |
| Mobile responsiveness | All pages | 320px to 2560px viewport support |
| Maximum time to complete registration | < 2 minutes | From landing to email verification |
| Maximum time to complete checkout | < 3 minutes | From pricing page to payment confirmation |
| Form error messages | Inline, immediate | Validation on blur + submit |
| Page load indicator | Always visible | Skeleton loaders, progress bars |

---

## 6. Maintainability

| Requirement | Target | Enforcement |
|-------------|:------:|-------------|
| Code coverage (backend) | ≥ 80% | CI gate |
| Code coverage (frontend) | ≥ 70% | CI gate |
| Lint errors | 0 | CI gate (ESLint + dotnet format) |
| Type safety | 100% strict | TypeScript strict mode; C# nullable enabled |
| Documentation | All public APIs | XML doc comments + OpenAPI/Swagger |
| Dependency updates | Monthly review | Dependabot PRs |
| Build time | < 5 minutes | CI build + test |
| Deploy time | < 10 minutes | Including health check |
| Rollback time | < 5 minutes | Docker image swap |

---

## 7. Observability

| Requirement | Specification |
|-------------|--------------|
| Structured logging | All logs in JSON via Serilog |
| Log retention | 30 days hot (Seq), 90 days cold (file) |
| Metrics collection | Prometheus scrape every 15 seconds |
| Dashboard refresh | 30-second auto-refresh in Grafana |
| Alert response time | < 5 minutes for Critical alerts |
| Correlation IDs | Every request tagged with unique ID |
| Error tracking | All unhandled exceptions logged with stack trace + context |
| Audit trail | All admin actions logged with before/after values |

---

## 8. Compatibility

| Dimension | Requirement |
|-----------|-------------|
| Browsers | Chrome 90+, Firefox 90+, Safari 15+, Edge 90+ |
| Mobile browsers | Chrome Mobile, Safari Mobile (iOS 15+, Android 10+) |
| Screen sizes | 320px (mobile) to 2560px (ultra-wide) |
| JavaScript | Required (graceful degradation for search only) |
| Operating systems | Windows, macOS, Linux, iOS, Android |
| Network | Functional on 3G (< 1 Mbps) for non-video content |

---

*These NFRs are not aspirational — they are testable, measurable requirements enforced by CI/CD gates, monitoring alerts, and load tests. Any deviation from these targets constitutes a bug with the same priority as a functional defect.*
