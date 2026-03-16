# 27 — MVP Scope Definition

*The precise boundary of what ships in V2 MVP (Phase 1). Everything not listed here is explicitly deferred.*

---

## 1. MVP Objective

> **Launch a stable, modern LMS that replicates the core revenue-generating functionality of V1** — student registration, subscription purchase, course access, and video learning — **on a maintainable, secure, and extensible codebase.**

The MVP is **not** a feature-complete replacement of V1. It is the minimum product that:
1. Lets new students register and pay
2. Lets existing students login and access their courses
3. Lets admins manage users, courses, and payments
4. Runs on modern infrastructure with CI/CD and monitoring

---

## 2. MVP Scope — Included (P0)

### 2.1 Student-Facing

| # | Feature | Stories | Notes |
|:-:|---------|:-------:|-------|
| 1 | Student registration (email + password) | S-007 | Email verification required |
| 2 | Student login / logout | S-008, S-009 | JWT with refresh token rotation |
| 3 | Password reset | S-010 | Email-based reset link |
| 4 | Google social login | S-013 | OAuth 2.0 |
| 5 | User profile (view/edit) | S-011 | Name, email, avatar, password change |
| 6 | Browse exam tracks & courses | S-015, S-021 | Public pages, SEO-optimized |
| 7 | Course detail page | S-016 | Description, modules, pricing CTA |
| 8 | Search courses & content | S-017 | Meilisearch instant search |
| 9 | Pricing page | S-022 | All packages, PKR + USD |
| 10 | Checkout (JazzCash) | S-023, S-026 | Primary Pakistan gateway |
| 11 | Checkout (Easypaisa) | S-024, S-026 | Secondary Pakistan gateway |
| 12 | Checkout (Stripe) | S-025, S-026 | International payments |
| 13 | Subscription management | S-027 | Auto-expire, grace period, renewal reminders |
| 14 | Coupon codes | S-028 | Discount codes at checkout |
| 15 | Payment history | S-029 | Transaction list, receipt download |
| 16 | My Courses dashboard | S-032 | Enrolled courses with progress |
| 17 | Course enrollment | S-031 | Auto-enroll on payment |
| 18 | Chapter reading view | S-033 | Rich content display |
| 19 | Video lectures (YouTube) | S-039, S-041 | Embedded player with controls |
| 20 | Progress tracking | S-034, S-035, S-040 | Per-chapter and per-video |
| 21 | Bookmarks | S-037 | Bookmark chapters |

### 2.2 Admin-Facing

| # | Feature | Stories | Notes |
|:-:|---------|:-------:|-------|
| 22 | Admin dashboard | — | Overview stats, quick actions |
| 23 | User management (CRUD) | S-014, S-044, S-045 | Search, filter, detail view |
| 24 | Role assignment | S-047 | Assign/remove roles |
| 25 | Manual subscription management | S-046 | Grant, extend, cancel |
| 26 | Course management (CRUD) | S-018, S-019, S-020 | Create, edit, archive courses |
| 27 | Video management | S-042 | Add/edit YouTube video IDs |
| 28 | Revenue dashboard | S-030 | Total revenue, MRR, gateway breakdown |
| 29 | Audit log | S-049 | Who did what, when |

### 2.3 Infrastructure

| # | Feature | Stories | Notes |
|:-:|---------|:-------:|-------|
| 30 | Docker Compose deployment | S-002, S-055 | All services containerized |
| 31 | CI/CD pipeline | S-003 | GitHub Actions: lint → test → build → deploy |
| 32 | Observability stack | S-006 | Serilog + Seq + Prometheus + Grafana |
| 33 | Automated backups | — | Daily DB backup + WAL archiving |
| 34 | V1 data migration | S-050 to S-054 | Users, courses, MCQs, subscriptions, files |
| 35 | URL redirects (V1 → V2) | S-054 | 301 redirects for SEO preservation |

---

## 3. MVP Scope — Excluded (Deferred)

### Deferred to Phase 2

| Feature | Reason for Deferral | Phase |
|---------|---------------------|:-----:|
| Exam / assessment engine | Complex; not needed for initial course access | 2 |
| Question bank management | Depends on exam engine | 2 |
| In-app notifications (SignalR) | Email notifications sufficient for MVP | 2 |
| Announcement system | Can use email for MVP announcements | 2 |
| Chat system | Not revenue-critical | 2 |
| Spaced repetition (SM-2) | Depends on exam engine data | 2 |
| Student analytics (admin) | Basic stats in admin dashboard suffice | 2 |
| Study streak tracking | Nice-to-have, not essential | 2 |
| Learning path recommendations | Requires usage data first | 2 |

### Deferred to Phase 3+

| Feature | Reason for Deferral | Phase |
|---------|---------------------|:-----:|
| Certificate generation | Requires course completion tracking to mature | 3 |
| Ambassador / referral program | V1 ambassador system continues temporarily | 3 |
| Discussion forums | Community feature; not learning-critical | 4 |
| Blog / content marketing | WordPress or separate setup can bridge | 4 |
| Mobile app | Responsive web serves mobile for now | 4 |
| Gamification | Engagement feature; needs user base first | 4 |
| Multi-language (Urdu) | English-only for MVP | 4 |

---

## 4. MVP Success Criteria

The MVP is considered successful when **all** of the following are true:

| # | Criterion | Measurement |
|:-:|-----------|-------------|
| 1 | All V1 users can log in | > 99% of migrated users authenticate successfully |
| 2 | New students can register and pay | End-to-end flow works for all 3 gateways |
| 3 | Students can access course content | All migrated courses load with correct content |
| 4 | Video lectures play correctly | All YouTube embeds load and track progress |
| 5 | Admin can manage users and courses | CRUD operations work without errors |
| 6 | Payment processing is reliable | Webhook processing succeeds > 99%; no double-charges |
| 7 | Error rate < 0.5% | Measured over first 48 hours post-launch |
| 8 | Page load time < 2s | P95 for key pages (home, course, dashboard) |
| 9 | No data loss from migration | Validated by V1 vs V2 record count comparison |
| 10 | Rollback capability confirmed | Can revert to V1 within 30 minutes if needed |

---

## 5. MVP Feature Count

| Category | Included | Deferred | Total (V2 Full) |
|----------|:--------:|:--------:|:---------------:|
| Student features | 21 | 16 | 37 |
| Admin features | 8 | 12 | 20 |
| Infrastructure | 6 | 0 | 6 |
| **Total** | **35** | **28** | **63** |

**MVP represents ~56% of total V2 scope, covering 100% of revenue-critical paths.**

---

## 6. V1 Feature Gap (MVP vs V1)

Features in V1 that are **not** in V2 MVP:

| V1 Feature | V2 Status | Impact | Bridge Strategy |
|-----------|:---------:|--------|-----------------|
| Exam taking | Phase 2 | Medium | Keep V1 exam system accessible via redirect for 30 days |
| Certificate download | Phase 3 | Low | V1 certificates remain valid; manual issuance if needed |
| Ambassador portal | Phase 3 | Low | V1 ambassador system runs in parallel |
| Blog pages | Phase 4 | Low | Keep V1 blog accessible or set up WordPress |
| Chat system | Phase 2 | Low | Support via email/WhatsApp |
| Daily reports | Phase 3 | Low | Admin uses Grafana dashboards instead |
| File manager | Phase 2 | Low | Direct S3 access via admin tools |

**Critical gap mitigation**: The exam engine is the largest gap. The bridge strategy is to keep V1 exam routes accessible for 30 days post-launch while Phase 2 development begins immediately.

---

## 7. MVP Timeline

| Sprint | Duration | Deliverables |
|:------:|:--------:|-------------|
| Sprint 1 | Weeks 5-6 | Auth, course catalog, course detail pages |
| Sprint 2 | Weeks 7-8 | Payment checkout (3 gateways), subscription management |
| Sprint 3 | Weeks 9-10 | Enrollment, progress tracking, video player |
| Sprint 4 | Weeks 11-12 | Admin panel (users, courses, revenue) |
| Sprint 5 | Weeks 13-14 | Data migration, deployment, launch |
| **Total** | **10 weeks** | **35 features** |

Phase 0 (Foundation) adds 4 weeks before Sprint 1, making the total **14 weeks to MVP**.

---

## 8. Go / No-Go Checklist (Pre-Launch)

Before flipping the switch from V1 to V2:

- [ ] All 10 MVP success criteria met
- [ ] V1 full backup taken and verified (< 1 hour old)
- [ ] V2 staging environment tested by stakeholder
- [ ] Payment gateway webhooks pointing to V2 endpoints
- [ ] DNS TTL lowered to 60 seconds (for quick rollback)
- [ ] V1 → V2 URL redirect map deployed
- [ ] Monitoring dashboards active and alerting configured
- [ ] Rollback procedure documented and rehearsed
- [ ] Communication sent to students about maintenance window
- [ ] Emergency contact list ready (developer, hosting provider)

---

*The MVP is deliberately conservative. It focuses on the revenue path (register → pay → learn) and defers everything that doesn't directly support that path. The goal is a stable, modern foundation — not feature parity with V1 on day one.*
