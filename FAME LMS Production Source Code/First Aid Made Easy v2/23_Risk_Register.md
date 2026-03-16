# 23 — Risk Register

*Categorized risks for the V2 project with likelihood, impact, mitigation strategies, and owners.*

---

## 1. Risk Assessment Matrix

| | **Low Impact** | **Medium Impact** | **High Impact** | **Critical Impact** |
|---|:---:|:---:|:---:|:---:|
| **Very Likely** | Medium | High | Critical | Critical |
| **Likely** | Low | Medium | High | Critical |
| **Possible** | Low | Medium | Medium | High |
| **Unlikely** | Low | Low | Medium | Medium |
| **Rare** | Low | Low | Low | Medium |

---

## 2. Technical Risks

| ID | Risk | Likelihood | Impact | Rating | Mitigation | Contingency |
|:--:|------|:----------:|:------:|:------:|-----------|-------------|
| R-T01 | V1 password hash format incompatible with dual-hash migration | Possible | High | **High** | Test with 100+ V1 user samples before migration; document ASP.NET Identity v2 hash format exactly | Force password reset for affected users |
| R-T02 | V1 database has undocumented schema changes (no source code) | Likely | Medium | **Medium** | Full schema dump and comparison before migration; map all tables regardless of use | Skip unmapped tables; migrate manually later |
| R-T03 | Payment gateway test/sandbox environments differ from production | Possible | High | **High** | Test with small real transactions on staging before go-live; maintain sandbox + production test accounts | Fall back to bank transfers temporarily |
| R-T04 | PostgreSQL migration from SQL Server introduces data integrity issues | Possible | High | **High** | Comprehensive validation queries post-migration; record counts, FK integrity, data spot-checks | Rollback to V1; fix migration script |
| R-T05 | Video streaming bandwidth exceeds server capacity | Possible | Medium | **Medium** | Use S3-compatible CDN for video delivery; adaptive bitrate; bandwidth monitoring | Throttle video quality; add CDN layer |
| R-T06 | SignalR connection limits under load | Unlikely | Medium | **Medium** | Load test WebSocket connections; use Redis backplane for multi-instance; graceful degradation | Fall back to polling for notifications |
| R-T07 | Meilisearch index corruption | Unlikely | Low | **Low** | Automated re-index job; separate search from core functionality | Disable search; fall back to DB queries |
| R-T08 | Docker container resource exhaustion | Possible | Medium | **Medium** | Resource limits in docker-compose; memory monitoring; OOM kill alerts | Restart affected containers; scale up VPS |

---

## 3. Project Risks

| ID | Risk | Likelihood | Impact | Rating | Mitigation | Contingency |
|:--:|------|:----------:|:------:|:------:|-----------|-------------|
| R-P01 | Scope creep delays MVP launch | Very Likely | High | **Critical** | Strict MVP scope document (file 27); change request process; say no to non-P0 features | Cut P0 scope to absolute minimum; launch with fewer tracks |
| R-P02 | Single developer bottleneck (bus factor = 1) | Likely | Critical | **Critical** | Document everything (this blueprint); coding standards; automated testing; onboarding guide | Hire contract developer; leverage documentation |
| R-P03 | Underestimated complexity in payment integrations | Likely | Medium | **Medium** | Start payment integration in Sprint 2; allocate 50% buffer; test early with sandbox | Launch with single gateway; add others post-MVP |
| R-P04 | V1 still needed for features not in V2 MVP | Likely | Medium | **Medium** | Feature gap analysis before cutover; maintain V1 in read-only mode for 30 days | Run V1 and V2 in parallel for specific features |
| R-P05 | Stakeholder expectations exceed MVP scope | Likely | Medium | **Medium** | Clear MVP scope communication; demo regularly; document what's deferred and when it ships | Show phase roadmap; confirm priorities |
| R-P06 | Testing takes longer than planned | Possible | Medium | **Medium** | Automated test suite from Phase 0; test alongside development (not after); CI enforcement | Reduce E2E test scope; prioritize happy-path |

---

## 4. Business Risks

| ID | Risk | Likelihood | Impact | Rating | Mitigation | Contingency |
|:--:|------|:----------:|:------:|:------:|-----------|-------------|
| R-B01 | Students cannot login after migration | Possible | Critical | **High** | Dual-hash password strategy; test with 100+ V1 users; have password reset ready | Emergency password reset page; extend V1 access |
| R-B02 | Revenue disruption during migration cutover | Possible | Critical | **High** | Weekend night migration; 2-hour maintenance window; pre-announce; instant fallback to V1 | Revert to V1 immediately; reconcile any V2-window transactions |
| R-B03 | Payment gateway credentials don't transfer | Unlikely | High | **Medium** | Verify credentials with each gateway early; document account ownership | New gateway accounts; manual enrollment during gap |
| R-B04 | Student data lost during migration | Rare | Critical | **Medium** | Full V1 backup before migration; validation queries; V1 kept intact for 30 days | Restore from V1 backup |
| R-B05 | SEO ranking drop after URL changes | Likely | Medium | **Medium** | 301 redirects for all V1 URLs; XML sitemap; resubmit to search consoles | Monitor rankings weekly; adjust redirects |
| R-B06 | Ambassador referral tracking breaks during transition | Possible | Medium | **Medium** | Migrate referral data; verify commission calculations match V1 behavior | Manual commission reconciliation |

---

## 5. Security Risks

| ID | Risk | Likelihood | Impact | Rating | Mitigation | Contingency |
|:--:|------|:----------:|:------:|:------:|-----------|-------------|
| R-S01 | Secrets accidentally committed to repository | Possible | Critical | **High** | Pre-commit hooks; .env files in .gitignore; secret scanning in CI; environment variables only | Rotate ALL credentials immediately; audit git history |
| R-S02 | Webhook endpoint abused (payment fraud) | Possible | Critical | **High** | HMAC signature verification; IP allow-listing; idempotency keys; rate limiting | Disable webhook; manual payment processing; investigate |
| R-S03 | JWT signing key compromised | Rare | Critical | **Medium** | Key stored in environment variable; not in config files; key rotation capability built | Rotate key immediately; force all users to re-login |
| R-S04 | DDoS attack during or after migration | Unlikely | High | **Medium** | Cloudflare proxy (free tier); rate limiting; auto-scaling preparation | Enable Cloudflare under-attack mode; wait it out |
| R-S05 | Unauthorized access to admin endpoints | Unlikely | Critical | **Medium** | Defense in depth (route → resource → data); audit logging; anomaly detection | Disable admin access; investigate via audit log |

---

## 6. Operational Risks

| ID | Risk | Likelihood | Impact | Rating | Mitigation | Contingency |
|:--:|------|:----------:|:------:|:------:|-----------|-------------|
| R-O01 | VPS provider outage | Unlikely | Critical | **Medium** | Automated backups to off-site S3; documented recovery procedure; DNS TTL set low | Provision new VPS from backup; redirect DNS |
| R-O02 | Database corruption | Rare | Critical | **Medium** | Daily backups; WAL archiving; pg_checksums enabled; RAID storage | Restore from latest backup (RPO < 24h) |
| R-O03 | SSL certificate expiry | Unlikely | Medium | **Medium** | Caddy auto-renews via ACME; monitoring alert 30 days before expiry | Manual certificate renewal via Let's Encrypt |
| R-O04 | Docker image registry (GHCR) unavailable | Unlikely | Medium | **Medium** | Pin image versions; keep local copies of latest images | Deploy from local builds |
| R-O05 | Dependency with critical vulnerability | Possible | High | **High** | Dependabot enabled; weekly dependency audit; lockfile pinning | Upgrade immediately; emergency patch deploy |

---

## 7. Risk Summary

| Rating | Count | Action Required |
|:------:|:-----:|-----------------|
| **Critical** | 2 | Active management: R-P01 (scope creep), R-P02 (bus factor) |
| **High** | 7 | Mitigation planned and tracked |
| **Medium** | 14 | Monitored; mitigation in place |
| **Low** | 2 | Accepted; monitored |
| **Total** | **25** | |

### Top 5 Risks Requiring Active Management

1. **R-P01**: Scope creep — Strict MVP scope, change request process
2. **R-P02**: Single developer — Comprehensive documentation, automated tests
3. **R-T01**: Password migration — Early testing with V1 data samples
4. **R-B01**: Student login failure — Dual-hash + emergency password reset
5. **R-B02**: Revenue disruption — Weekend migration with instant rollback

---

*Risks are not problems — they are anticipated problems. This register ensures we know what can go wrong and have planned responses. The top 5 risks should be reviewed weekly during development.*
