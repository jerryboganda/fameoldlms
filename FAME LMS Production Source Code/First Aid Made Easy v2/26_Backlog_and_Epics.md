# 26 — Backlog and Epics

*All work organized into epics with user stories, acceptance criteria, and priority. Maps to features F-001 through F-103 and the phase roadmap (file 17).*

---

## 1. Epic Index

| Epic | Name | Phase | Priority | Story Count |
|:----:|------|:-----:|:--------:|:-----------:|
| E-01 | Project Foundation | 0 | P0 | 6 |
| E-02 | Identity & Authentication | 1 (Sprint 1) | P0 | 8 |
| E-03 | Course Catalog & Content | 1 (Sprint 1) | P0 | 7 |
| E-04 | Payment & Subscriptions | 1 (Sprint 2) | P0 | 9 |
| E-05 | Student Enrollment & Learning | 1 (Sprint 3) | P0 | 8 |
| E-06 | Video & Lecture Delivery | 1 (Sprint 3) | P0 | 5 |
| E-07 | User Management (Admin) | 1 (Sprint 4) | P0 | 6 |
| E-08 | Migration & Launch | 1 (Sprint 5) | P0 | 7 |
| E-09 | Exam & Assessment Engine | 2 (Sprint 6) | P1 | 10 |
| E-10 | Communication & Notifications | 2 (Sprint 7) | P1 | 7 |
| E-11 | Spaced Repetition & Analytics | 2 (Sprint 8) | P1 | 6 |
| E-12 | Certificate Generation | 3 | P1 | 5 |
| E-13 | Ambassador & Referrals | 3 | P2 | 6 |
| E-14 | Advanced Features | 4 | P2 | 5 |
| | | | **Total** | **95** |

---

## 2. Epic Details

### E-01: Project Foundation (Phase 0)

**Goal**: Repository setup, infrastructure, CI/CD, design system foundation.

| Story | Title | Acceptance Criteria | Features |
|:-----:|-------|--------------------:|:--------:|
| S-001 | Initialize monorepo with Turborepo | `turbo build` succeeds; Next.js + ASP.NET Core projects scaffold | — |
| S-002 | Configure Docker Compose for local dev | `docker compose up` starts all services (DB, Redis, API, Web) | — |
| S-003 | Set up CI/CD pipeline | Push to main triggers lint → test → build; deploy to staging on tag | — |
| S-004 | Create database schema & run first migration | EF Core migration applies; all core tables exist in PostgreSQL | — |
| S-005 | Implement design system foundation | Button, Input, Card, Dialog components built with Tailwind + Radix | — |
| S-006 | Configure observability stack | Serilog → Seq logging works; Prometheus scrapes metrics; Grafana dashboard loads | — |

---

### E-02: Identity & Authentication (Phase 1 Sprint 1)

**Goal**: Users can register, login, and manage their accounts.

| Story | Title | Acceptance Criteria | Features |
|:-----:|-------|--------------------:|:--------:|
| S-007 | Student registration with email | New user registers → email verification sent → account activated | F-001 |
| S-008 | Email/password login with JWT | User logs in → receives access + refresh tokens → can access protected routes | F-002 |
| S-009 | Refresh token rotation | Expired access token → silent refresh → new tokens issued | F-002 |
| S-010 | Password reset via email | User requests reset → email with link → sets new password | F-003 |
| S-011 | User profile page | User views and edits name, email, avatar, password | F-005 |
| S-012 | Role-based route protection | Admin routes reject students; student routes reject anonymous | F-006 |
| S-013 | Social login (Google) | User clicks "Sign in with Google" → account created/linked | F-004 |
| S-014 | Admin user management CRUD | Admin lists, searches, creates, edits, deactivates users | F-056 |

---

### E-03: Course Catalog & Content (Phase 1 Sprint 1)

**Goal**: Courses, modules, and chapters are browsable and searchable.

| Story | Title | Acceptance Criteria | Features |
|:-----:|-------|--------------------:|:--------:|
| S-015 | Public course listing page | Visitors see all exam tracks with course counts, prices | F-007 |
| S-016 | Course detail page | Visitor sees course description, module/chapter structure, pricing, CTA | F-008 |
| S-017 | Instant search (Meilisearch) | User types in search → results appear within 50ms → matches courses, topics | F-009 |
| S-018 | Admin course CRUD | Admin creates/edits/archives courses with metadata and thumbnail | F-061 |
| S-019 | Module & chapter management | Admin creates/reorders modules and chapters within a course | F-062 |
| S-020 | Content editor for chapters | Admin edits chapter content (rich text) and attaches resources | F-063 |
| S-021 | Exam track landing pages | Each exam track (PLAB, USMLE, etc.) has a dedicated landing page | F-007 |

---

### E-04: Payment & Subscriptions (Phase 1 Sprint 2)

**Goal**: Students can purchase subscriptions and access paid content.

| Story | Title | Acceptance Criteria | Features |
|:-----:|-------|--------------------:|:--------:|
| S-022 | Pricing page | Visitor sees all packages with PKR/USD prices, feature comparison | F-013 |
| S-023 | Checkout flow (JazzCash) | Student selects package → pays via JazzCash → subscription activated | F-015, F-016 |
| S-024 | Checkout flow (Easypaisa) | Student selects package → pays via Easypaisa → subscription activated | F-015, F-017 |
| S-025 | Checkout flow (Stripe) | International student pays via card/Stripe → subscription activated | F-015, F-018 |
| S-026 | Webhook processing | Gateway sends payment confirmation → system activates subscription automatically | F-016, F-017, F-018 |
| S-027 | Subscription lifecycle management | Expired subscriptions auto-deactivate; grace period applied; renewal reminders sent | F-019, F-020 |
| S-028 | Coupon code system | Admin creates coupons; student applies at checkout → discount applied | F-021 |
| S-029 | Payment history & receipts | Student views all past transactions; downloads receipt PDF | F-022 |
| S-030 | Revenue dashboard (admin) | Admin sees total revenue, MRR, subscription counts, gateway breakdown | F-070 |

---

### E-05: Student Enrollment & Learning (Phase 1 Sprint 3)

**Goal**: Subscribed students can access courses and track progress.

| Story | Title | Acceptance Criteria | Features |
|:-----:|-------|--------------------:|:--------:|
| S-031 | Course enrollment on subscription | After payment, student is auto-enrolled in track courses | F-023 |
| S-032 | My Courses dashboard | Student sees enrolled courses with progress percentage | F-024 |
| S-033 | Chapter reading view | Student reads chapter content; progress auto-saved on scroll completion | F-025 |
| S-034 | Progress tracking (per chapter) | Chapter marked complete when student finishes reading / watching | F-026 |
| S-035 | Course progress bar | Course card shows percentage complete based on completed chapters | F-026 |
| S-036 | Study streak tracking | System tracks daily study activity; shows current streak on dashboard | F-027 |
| S-037 | Bookmark chapters | Student bookmarks chapters for quick access later | F-028 |
| S-038 | Learning path recommendations | System suggests next chapter/course based on progress | F-029 |

---

### E-06: Video & Lecture Delivery (Phase 1 Sprint 3)

**Goal**: Students can watch video lectures within courses.

| Story | Title | Acceptance Criteria | Features |
|:-----:|-------|--------------------:|:--------:|
| S-039 | Video player (YouTube embed) | Student watches embedded video within chapter view | F-030 |
| S-040 | Video progress tracking | System records watch time; marks lecture complete at 90% | F-031 |
| S-041 | Video playback controls | Speed control (0.5x-2x), quality selector, fullscreen | F-030 |
| S-042 | Admin video management | Admin adds/edits YouTube video IDs for lectures | F-064 |
| S-043 | Video-only lecture view | Clean playback page without distracting sidebar elements | F-030 |

---

### E-07: User Management — Admin (Phase 1 Sprint 4)

**Goal**: Admins can manage all users, roles, and subscriptions.

| Story | Title | Acceptance Criteria | Features |
|:-----:|-------|--------------------:|:--------:|
| S-044 | User search & filtering | Admin searches by name, email, role; filters by status, subscription | F-056 |
| S-045 | User detail view (admin) | Admin sees full user profile, subscription history, activity log | F-057 |
| S-046 | Manual subscription assignment | Admin grants/extends/cancels subscriptions manually | F-058 |
| S-047 | Role assignment | Admin assigns/removes roles for users | F-059 |
| S-048 | User bulk actions | Admin exports user list; bulk email; bulk role change | F-060 |
| S-049 | Audit log viewer | Admin views who did what and when (entity changes, logins) | F-071 |

---

### E-08: Migration & Launch (Phase 1 Sprint 5)

**Goal**: Migrate V1 data and launch V2 for production traffic.

| Story | Title | Acceptance Criteria | Features |
|:-----:|-------|--------------------:|:--------:|
| S-050 | User data migration script | All V1 users migrated with dual-hash passwords; validated | — |
| S-051 | Course & content migration | All V1 courses, modules, chapters migrated; images transferred to S3 | — |
| S-052 | MCQ data migration | All 20,000+ MCQs migrated with options, correct answers, explanations | — |
| S-053 | Subscription & payment history migration | Active subscriptions transferred; payment history preserved | — |
| S-054 | URL redirect mapping | All V1 URLs 301-redirect to V2 equivalents | — |
| S-055 | Production deployment | V2 deployed on production VPS; DNS switched; SSL active | — |
| S-056 | Post-launch monitoring (48 hours) | Dashboards monitored; error rate < 0.1%; no data loss; user reports addressed | — |

---

### E-09: Exam & Assessment Engine (Phase 2 Sprint 6)

**Goal**: Students can take practice exams and review results.

| Story | Title | Acceptance Criteria | Features |
|:-----:|-------|--------------------:|:--------:|
| S-057 | Exam listing by track | Student sees available exams for enrolled track | F-032 |
| S-058 | Start exam session | Student starts timed exam; questions loaded; timer starts | F-033 |
| S-059 | MCQ question display | Question with 4 options displayed; student selects one | F-034 |
| S-060 | Exam navigation | Student can navigate between questions; flag for review | F-035 |
| S-061 | Exam submission & scoring | Student submits → system calculates score → result displayed | F-036 |
| S-062 | Exam review mode | Student reviews answers with correct/incorrect highlighting and explanations | F-037 |
| S-063 | Exam history & analytics | Student sees past exam scores, trends, weak areas | F-038 |
| S-064 | Question bank management (admin) | Admin creates/edits/imports MCQs with metadata | F-065 |
| S-065 | Exam builder (admin) | Admin creates exams from question bank with rules (count, difficulty, topics) | F-066 |
| S-066 | Tutor mode (immediate feedback) | In tutor mode, correct answer shown after each question | F-039 |

---

### E-10: Communication & Notifications (Phase 2 Sprint 7)

**Goal**: Users receive notifications; announcements reach students; basic chat.

| Story | Title | Acceptance Criteria | Features |
|:-----:|-------|--------------------:|:--------:|
| S-067 | In-app notification center | Bell icon shows unread count; dropdown lists recent notifications | F-040 |
| S-068 | Real-time notifications (SignalR) | Notifications appear instantly without page refresh | F-041 |
| S-069 | Email notifications | Key events (registration, payment, exam result) send emails | F-042 |
| S-070 | Admin announcement system | Admin creates announcements; students see banner/modal | F-043 |
| S-071 | Communication preferences | Student toggles which notification types they receive | F-044 |
| S-072 | Basic chat (student → support) | Student sends message; support agent sees in admin panel | F-045 |
| S-073 | Chat notification integration | New chat message triggers real-time + email notification | F-045 |

---

### E-11: Spaced Repetition & Analytics (Phase 2 Sprint 8)

**Goal**: SM-2 spaced repetition for MCQ review; admin analytics dashboard.

| Story | Title | Acceptance Criteria | Features |
|:-----:|-------|--------------------:|:--------:|
| S-074 | Daily review queue | Student sees today's review cards based on SM-2 algorithm | F-046 |
| S-075 | Flashcard review UI | Card flips to show answer; student rates difficulty (1-5) | F-047 |
| S-076 | Review statistics | Student sees review streak, cards due, mastery progress | F-048 |
| S-077 | Admin analytics dashboard | Admin sees registrations, revenue, active users, exam stats | F-068, F-069, F-070 |
| S-078 | Student progress reports (admin) | Admin views individual student progress across courses/exams | F-067 |
| S-079 | Export analytics to CSV | Admin downloads analytics data for external analysis | F-070 |

---

### E-12: Certificate Generation (Phase 3)

**Goal**: Students earn certificates upon course/exam completion.

| Story | Title | Acceptance Criteria | Features |
|:-----:|-------|--------------------:|:--------:|
| S-080 | Auto-generate certificate on completion | Student completes course/exam → certificate PDF generated | F-049 |
| S-081 | Certificate template management | Admin uploads/edits certificate templates | F-073 |
| S-082 | Certificate download | Student downloads certificate as PDF | F-050 |
| S-083 | Certificate verification page | Public URL verifies certificate authenticity via unique code | F-051 |
| S-084 | Certificate gallery | Student views all earned certificates in profile | F-052 |

---

### E-13: Ambassador & Referral Program (Phase 3)

**Goal**: Ambassadors earn commission for referring students.

| Story | Title | Acceptance Criteria | Features |
|:-----:|-------|--------------------:|:--------:|
| S-085 | Ambassador registration | User applies to become ambassador; admin approves | F-053 |
| S-086 | Referral link generation | Ambassador gets unique referral link; tracks clicks | F-054 |
| S-087 | Referral tracking on enrollment | Student enrolls via referral link → ambassador credited | F-054 |
| S-088 | Commission calculation | System calculates commission based on referral rules | F-055 |
| S-089 | Ambassador dashboard | Ambassador sees referrals, earnings, payouts | F-053 |
| S-090 | Admin ambassador management | Admin views ambassadors, approves payouts, adjusts rates | F-074 |

---

### E-14: Advanced Features (Phase 4)

**Goal**: Future enhancements for growth and engagement.

| Story | Title | Acceptance Criteria | Features |
|:-----:|-------|--------------------:|:--------:|
| S-091 | Discussion forums (per course) | Students post questions; teachers/peers answer | F-075 |
| S-092 | Blog / content marketing | Admin publishes blog posts; public SEO-optimized pages | F-076 |
| S-093 | Mobile app (React Native) | Core learning experience on iOS/Android | F-077 |
| S-094 | Gamification (badges, leaderboard) | Students earn badges; leaderboard shows top performers | F-078 |
| S-095 | Multi-language support | Interface supports English + Urdu | F-079 |

---

## 3. Priority Legend

| Priority | Meaning | Phase |
|:--------:|---------|:-----:|
| **P0** | Must-have for MVP launch | 0-1 |
| **P1** | Important; builds on MVP | 2-3 |
| **P2** | Nice-to-have; growth features | 3-4 |
| **P3** | Future / exploratory | 4+ |

---

## 4. Feature → Story Traceability

Every feature from the Feature Inventory (file 02) maps to at least one story:

| Feature Range | Epic(s) | Coverage |
|:------------:|:-------:|:--------:|
| F-001 to F-006 | E-02 | ✅ Full |
| F-007 to F-012 | E-03 | ✅ Full |
| F-013 to F-022 | E-04 | ✅ Full |
| F-023 to F-029 | E-05 | ✅ Full |
| F-030 to F-031 | E-06 | ✅ Full |
| F-032 to F-039 | E-09 | ✅ Full |
| F-040 to F-045 | E-10 | ✅ Full |
| F-046 to F-048 | E-11 | ✅ Full |
| F-049 to F-052 | E-12 | ✅ Full |
| F-053 to F-055 | E-13 | ✅ Full |
| F-056 to F-074 | E-07, E-04, E-09, E-11, E-12 | ✅ Full |
| F-075 to F-079+ | E-14 | ✅ Mapped |

---

*This backlog represents the complete scope of V2 across all phases. During sprint planning, stories are pulled from the current phase's epics in priority order. Stories marked P0 are non-negotiable for MVP.*
