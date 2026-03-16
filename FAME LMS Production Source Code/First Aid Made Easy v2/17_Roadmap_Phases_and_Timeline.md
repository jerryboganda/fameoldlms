# 17 — Roadmap, Phases, and Timeline

*Four release phases from MVP to full feature parity, with specific milestones, dependencies, and go/no-go criteria.*

---

## 1. Release Strategy

```
Phase 0        Phase 1          Phase 2          Phase 3          Phase 4
Foundation     MVP              Growth           Maturity         Scale
──────────────────────────────────────────────────────────────────────────►
Weeks 1-4      Weeks 5-14       Weeks 15-22      Weeks 23-30      Weeks 31+

Setup &        Core LMS &       Assessments &    Ambassador &     Analytics &
Architecture   Enrollment       Communication    Advanced         Optimization
```

---

## 2. Phase 0 — Foundation (Weeks 1-4)

**Goal**: Repository, CI/CD, infrastructure, design system scaffold.

| Week | Deliverable | Estimated Effort |
|:----:|-------------|:----------------:|
| 1 | Monorepo setup (Turborepo, pnpm workspaces) | 16h |
| 1 | Docker Compose (dev + staging) | 8h |
| 1 | PostgreSQL schema + EF Core setup | 16h |
| 1 | CI pipeline (lint, test, build) | 8h |
| 2 | ASP.NET Core API scaffold (modules, middleware, health checks) | 24h |
| 2 | Next.js app scaffold (App Router, layouts, route groups) | 16h |
| 2 | Design tokens + Tailwind config + base components | 16h |
| 3 | Identity module: registration, login, JWT, refresh tokens | 24h |
| 3 | Role-based authorization (policies + handlers) | 16h |
| 3 | Email service + templates (welcome, verify, reset) | 8h |
| 4 | Admin shell (sidebar, header, basic pages) | 16h |
| 4 | Student shell (sidebar, header, dashboard placeholder) | 16h |
| 4 | Staging deployment + DNS setup | 8h |

**Phase 0 Exit Criteria:**
- [ ] User can register, verify email, login, logout
- [ ] JWT + refresh token flow works end-to-end
- [ ] Admin can login and see admin dashboard shell
- [ ] CI pipeline passes on every PR
- [ ] Staging environment accessible via staging URL
- [ ] Design system renders base components correctly

---

## 3. Phase 1 — MVP (Weeks 5-14)

**Goal**: Core LMS — students can browse, purchase, enroll, and access course content.

### Sprint 1 (Weeks 5-6): Course Catalog

| Task | Module | Effort |
|------|--------|:------:|
| Course entity + CRUD API | Catalog | 16h |
| ExamTrack + Module + Chapter entities | Catalog | 12h |
| Lecture entity + video URL management | Catalog | 8h |
| Admin: Course management pages | Catalog | 20h |
| Public: Pricing page | Catalog + Payment | 12h |
| Public: Course catalog page (ISR) | Catalog | 12h |
| Public: Individual course page | Catalog | 8h |
| Meilisearch integration (course index) | Catalog | 8h |

### Sprint 2 (Weeks 7-8): Payment & Enrollment

| Task | Module | Effort |
|------|--------|:------:|
| Package entity + management | Payment | 8h |
| V1 package data migration | Payment | 4h |
| Checkout page UI | Payment | 16h |
| JazzCash server-side integration | Payment | 16h |
| Easypaisa server-side integration | Payment | 12h |
| Stripe integration | Payment | 8h |
| Webhook processing + enrollment creation | Payment + Enrollment | 16h |
| Coupon system (validate, apply) | Payment | 12h |
| Invoice generation (PDF) | Payment | 8h |

### Sprint 3 (Weeks 9-10): Video Learning

| Task | Module | Effort |
|------|--------|:------:|
| Video player component (HLS) | Frontend | 16h |
| Course progress tracking | Enrollment | 12h |
| Lecture completion marking | Enrollment | 8h |
| Course page (student view) | Frontend | 16h |
| Lab values reference panel | Frontend | 8h |
| S3 signed URL generation | Infrastructure | 8h |
| Student dashboard (courses, progress) | Frontend | 12h |

### Sprint 4 (Weeks 11-12): User Management

| Task | Module | Effort |
|------|--------|:------:|
| Admin: User list + search | Identity / Admin | 12h |
| Admin: User detail + edit | Identity / Admin | 8h |
| Admin: Role management | Identity / Admin | 8h |
| Student: Profile settings | Identity | 8h |
| Student: Subscription management | Payment | 12h |
| Admin: Transaction list + detail | Payment / Admin | 12h |
| Admin: Dashboard (stats) | Admin | 12h |

### Sprint 5 (Weeks 13-14): V1 Data Migration + Polish

| Task | Module | Effort |
|------|--------|:------:|
| ETL pipeline: Users + passwords | Migration | 16h |
| ETL pipeline: Courses + content | Migration | 12h |
| ETL pipeline: Payments + enrollments | Migration | 12h |
| ETL pipeline: Files → S3 | Migration | 8h |
| Migration validation suite | Migration | 8h |
| Bug fixes + performance tuning | All | 24h |
| End-to-end testing | QA | 16h |
| Production deployment prep | DevOps | 8h |

**Phase 1 Exit Criteria (MVP Launch):**
- [ ] Student can register, purchase package, and access video courses
- [ ] All 3 payment gateways functional (JazzCash, Easypaisa, Stripe)
- [ ] Admin can manage courses, users, and view transactions
- [ ] V1 data migrated and validated
- [ ] All V1 students can login with existing passwords
- [ ] Core Web Vitals pass on all pages
- [ ] Zero critical/high security findings
- [ ] Load test: 200 concurrent users, P95 <500ms

---

## 4. Phase 2 — Growth (Weeks 15-22)

**Goal**: Assessment engine, communication, and learning engagement features.

### Sprint 6 (Weeks 15-16): MCQ Exam Engine

| Task | Module | Effort |
|------|--------|:------:|
| Question bank management (admin) | Assessment | 16h |
| CSV question import | Assessment | 8h |
| Exam configuration (admin) | Assessment | 12h |
| Exam engine UI (student) | Assessment / Frontend | 24h |
| Exam scoring + result calculation | Assessment | 12h |
| Exam result review mode | Assessment / Frontend | 12h |
| V1 question data migration | Migration | 8h |
| V1 exam result migration | Migration | 8h |

### Sprint 7 (Weeks 17-18): Communication

| Task | Module | Effort |
|------|--------|:------:|
| Notification system (DB + SignalR) | Communication | 16h |
| Notification center UI | Communication / Frontend | 12h |
| Announcement system (admin) | Communication | 12h |
| Expiry warning emails (Hangfire jobs) | Communication | 8h |
| Support ticket system | Communication | 16h |
| Communication preferences | Communication | 8h |

### Sprint 8 (Weeks 19-20): Spaced Repetition & Analytics

| Task | Module | Effort |
|------|--------|:------:|
| SM-2 algorithm implementation | Assessment | 12h |
| Daily revision queue UI | Assessment / Frontend | 12h |
| Student analytics dashboard | Analytics / Frontend | 16h |
| Admin analytics dashboard | Analytics / Frontend | 16h |
| Certificate auto-generation | Certificate | 16h |
| Certificate template management | Certificate / Admin | 8h |

### Sprint 9 (Weeks 21-22): Polish & Optimization

| Task | Module | Effort |
|------|--------|:------:|
| Mobile responsive audit + fixes | Frontend | 16h |
| Performance optimization pass | All | 16h |
| SEO audit + fixes | Frontend | 8h |
| Accessibility audit + fixes | Frontend | 12h |
| Integration testing suite | QA | 16h |
| Bug fixes | All | 16h |

**Phase 2 Exit Criteria:**
- [ ] Students can take MCQ exams with full scoring
- [ ] Spaced repetition system functional
- [ ] Real-time notifications working
- [ ] Certificates auto-generated on course completion
- [ ] Admin has analytics dashboards
- [ ] Mobile experience polished

---

## 5. Phase 3 — Maturity (Weeks 23-30)

**Goal**: Ambassador program, advanced features, operational maturity.

### Sprint 10-11 (Weeks 23-26): Ambassador Module

| Task | Module | Effort |
|------|--------|:------:|
| Ambassador registration + approval flow | Ambassador | 16h |
| Referral link generation | Ambassador | 8h |
| Referral tracking (cookie + code) | Ambassador | 12h |
| Commission calculation on purchase | Ambassador + Payment | 12h |
| Ambassador dashboard | Ambassador / Frontend | 16h |
| Payout management (admin) | Ambassador / Admin | 12h |
| Ambassador analytics | Ambassador / Analytics | 8h |

### Sprint 12-13 (Weeks 27-30): Advanced Features

| Task | Module | Effort |
|------|--------|:------:|
| Blog/content management system | Content | 16h |
| Email marketing: campaign builder | Communication | 16h |
| Email marketing: list management | Communication | 12h |
| Advanced search (Meilisearch filters) | Catalog | 8h |
| Course reviews/ratings | Catalog | 12h |
| Auto-renewal subscriptions | Payment | 12h |
| Subscription upgrade/downgrade | Payment | 12h |
| Admin: bulk operations | Admin | 8h |
| Performance profiling + optimization | All | 16h |

**Phase 3 Exit Criteria:**
- [ ] Ambassador program fully operational
- [ ] Blog/CMS live
- [ ] Email marketing campaigns functional
- [ ] Auto-renewal working for all gateways
- [ ] Operational runbooks documented

---

## 6. Phase 4 — Scale (Weeks 31+)

**Goal**: Continuous improvement, scaling, and optional features.

| Feature | Priority | Effort |
|---------|:--------:|:------:|
| Multi-factor authentication | P1 | M |
| Mobile app (React Native/Expo) | P2 | XL |
| Live class scheduling (Zoom/Meet integration) | P2 | L |
| AI-powered question generation | P2 | L |
| Dark mode | P2 | S |
| Multi-language support (Urdu) | P2 | L |
| Horizontal scaling (multi-node) | P2 | M |
| Advanced reporting (custom report builder) | P2 | L |
| Instructor-created courses | P2 | L |
| Gamification (badges, leaderboards) | P2 | M |

---

## 7. Feature-Phase Mapping Summary

| Phase | Features (F-IDs) | Count |
|:-----:|-----------------|:-----:|
| 0 | F-001 to F-005 (Auth, RBAC) | 5 |
| 1 | F-006 to F-030 (Catalog, Payment, Enrollment, Video) | 25 |
| 2 | F-031 to F-060 (Assessment, Communication, Certificates, Analytics) | 30 |
| 3 | F-061 to F-085 (Ambassador, Blog, Marketing, Advanced Payment) | 25 |
| 4 | F-086 to F-103 (Scale, Mobile, AI, i18n) | 18 |

---

## 8. Risk-Adjusted Timeline

| Phase | Optimistic | Realistic | Pessimistic |
|:-----:|:----------:|:---------:|:-----------:|
| 0 | 3 weeks | 4 weeks | 5 weeks |
| 1 | 8 weeks | 10 weeks | 14 weeks |
| 2 | 6 weeks | 8 weeks | 10 weeks |
| 3 | 6 weeks | 8 weeks | 10 weeks |
| **Total to full feature** | **23 weeks** | **30 weeks** | **39 weeks** |
| **MVP Launch** | **11 weeks** | **14 weeks** | **19 weeks** |

Key assumptions:
- 1 senior full-stack developer (primary)
- 1 junior developer assisting from Phase 1 Sprint 3
- Part-time UI/UX input
- No major scope changes

---

## 9. Go/No-Go Criteria per Phase

| Criterion | Phase 0→1 | Phase 1→Launch | Phase 2 | Phase 3 |
|-----------|:---------:|:--------------:|:-------:|:-------:|
| All exit criteria met | ✅ Required | ✅ Required | ✅ Required | ✅ Required |
| No critical bugs | ✅ Required | ✅ Required | ✅ Required | ✅ Required |
| Security scan clean | ✅ Required | ✅ Required | ✅ Required | ✅ Required |
| Load test passing | — | ✅ Required | ✅ Required | ✅ Required |
| Migration validated | — | ✅ Required | — | — |
| Stakeholder sign-off | ✅ Required | ✅ Required | ✅ Required | ✅ Required |
| Rollback plan tested | — | ✅ Required | ✅ Required | ✅ Required |

---

*This roadmap is designed for a small team moving fast. The MVP at Week 14 delivers the highest-impact improvement: self-service enrollment replacing the manual bottleneck. Each subsequent phase adds value incrementally without requiring a big-bang rewrite.*
