# 02 — Complete V2 Feature Inventory and Prioritization

*80+ features across 15 modules, each sized and prioritized for phased delivery.*

---

## 1. Feature Inventory Overview

| Module | Total Features | P0 (MVP) | P1 (Post-MVP) | P2 (Future) |
|--------|:-------------:|:--------:|:--------------:|:------------:|
| Identity & Authentication | 8 | 6 | 2 | 0 |
| Course Catalog & Content | 10 | 7 | 2 | 1 |
| Video & Learning Player | 8 | 5 | 2 | 1 |
| Assessment / MCQ Engine | 12 | 7 | 3 | 2 |
| Student Dashboard | 6 | 4 | 2 | 0 |
| Payment & Subscriptions | 9 | 6 | 2 | 1 |
| Admin Panel | 10 | 7 | 2 | 1 |
| Communication & Notifications | 6 | 3 | 2 | 1 |
| Certificate System | 4 | 2 | 1 | 1 |
| Ambassador / Referral | 5 | 2 | 2 | 1 |
| Blog & Content Marketing | 4 | 2 | 1 | 1 |
| Support & Help Center | 5 | 3 | 1 | 1 |
| SEO & Public Site | 6 | 5 | 1 | 0 |
| Analytics & Reporting | 6 | 3 | 2 | 1 |
| System Administration | 4 | 3 | 1 | 0 |
| **Total** | **103** | **65** | **26** | **12** |

---

## 2. Sizing Legend

| Size | Story Points | Approximate Effort | Description |
|------|:----------:|:------------------:|-------------|
| **S** | 1-2 | 1-2 days | Straightforward, well-understood, minimal dependencies |
| **M** | 3-5 | 3-5 days | Moderate complexity, some design decisions, limited dependencies |
| **L** | 8-13 | 1-2 weeks | Significant complexity, multiple components, cross-cutting concerns |
| **XL** | 20+ | 2-4 weeks | Major subsystem, high complexity, many dependencies |

---

## 3. Complete Feature List by Module

### Module 1: Identity & Authentication

| ID | Feature | Priority | Size | V1 Status | V2 Notes |
|----|---------|:--------:|:----:|-----------|----------|
| F-001 | Email + password registration | P0 | M | Exists (7-step wizard) | Reduce to 3 steps: email → verify → profile. Remove CNIC requirement from registration. |
| F-002 | Email verification | P0 | S | Exists | Modernize: magic link or 6-digit code. Resend with cooldown. |
| F-003 | Social login (Google) | P1 | M | Not available | Google OAuth2 for frictionless registration. Map to existing account by email. |
| F-004 | Forgot password / reset | P0 | S | Exists | Time-limited reset tokens (1 hour). Rate-limited. |
| F-005 | Profile management | P0 | M | Exists (basic) | Photo upload, display name, exam track preference, timezone. |
| F-006 | Role-based access control | P0 | L | Exists (6 roles) | 8 roles with policy-based authorization. See file 08. |
| F-007 | Session management | P0 | M | Exists (6000-min timeout) | JWT + refresh tokens + Redis. Configurable timeout. Device management. |
| F-008 | Two-factor authentication | P1 | M | Not available | TOTP-based 2FA for admin accounts. Optional for students. |

### Module 2: Course Catalog & Content

| ID | Feature | Priority | Size | V1 Status | V2 Notes |
|----|---------|:--------:|:----:|-----------|----------|
| F-009 | Course listing page | P0 | M | Exists | Grid/list view, filter by exam track, sort by date/popularity. SSG for performance. |
| F-010 | Course detail page | P0 | M | Exists | Structured content: overview, curriculum, instructor, reviews. SEO-optimized. |
| F-011 | Exam track browsing | P0 | S | Exists (13+ tracks) | Dedicated landing page per track. Track-specific stats and recommendations. |
| F-012 | Course search | P0 | M | Not available | Meilisearch-powered: instant results, typo tolerance, faceted filtering. |
| F-013 | Content management (admin) | P0 | L | Exists (basic) | Draft → Review → Published workflow. Bulk operations. Content scheduling. |
| F-014 | Course preview (free) | P0 | M | Not available | First 2-3 lectures free per course. Guest-accessible. Conversion-focused CTA. |
| F-015 | Textbook/resource library | P0 | M | Exists (~20 textbooks) | Organized by exam track. PDF viewer or link-out. Download tracking. |
| F-016 | Content versioning | P1 | L | Not available | Track content changes. Ability to revert. Change history visible to admins. |
| F-017 | Lecture notes / supplementary materials | P1 | M | Not available | Markdown-based notes attached to lectures. Downloadable PDF export. |
| F-018 | AI-generated summaries | P2 | L | Not available | GPT-powered lecture summaries and key points. Future feature. |

### Module 3: Video & Learning Player

| ID | Feature | Priority | Size | V1 Status | V2 Notes |
|----|---------|:--------:|:----:|-----------|----------|
| F-019 | YouTube embedded player | P0 | M | Exists | Enhanced wrapper: React component with custom controls overlay. |
| F-020 | Playback speed control | P0 | S | Exists (YouTube native) | Custom UI: 0.5x, 0.75x, 1x, 1.25x, 1.5x, 2x. Persisted preference. |
| F-021 | Resume position tracking | P0 | M | Exists (basic) | Server-synced progress. Resume exactly where left off, across devices. |
| F-022 | Chapter/section markers | P0 | M | Not available | Jump to specific topics within long lectures. Admin-defined timestamps. |
| F-023 | Keyboard shortcuts | P0 | S | Not available | Space (play/pause), arrow keys (seek), M (mute), F (fullscreen). |
| F-024 | Picture-in-picture mode | P1 | S | Not available | Native browser PiP support. Study notes alongside video. |
| F-025 | Video completion tracking | P1 | M | Exists (basic) | 90% watched = completed. Progress bar per section. Certificate of completion. |
| F-026 | Offline viewing | P2 | XL | Not available | Future: PWA with service worker caching for downloaded lectures. |

### Module 4: Assessment / MCQ Engine

| ID | Feature | Priority | Size | V1 Status | V2 Notes |
|----|---------|:--------:|:----:|-----------|----------|
| F-027 | Timed exam mode | P0 | L | Exists (strong: 4.0/5) | Preserve all v1 strengths. React rebuild with improved state management. |
| F-028 | Practice mode (untimed) | P0 | M | Exists | No timer, immediate answer feedback, explanation shown after each answer. |
| F-029 | Question navigation panel | P0 | M | Exists | Visual grid: answered/unanswered/flagged. Click-to-jump. |
| F-030 | Keyboard shortcut answering | P0 | S | Exists (A/B/C/D keys) | Preserve: A/B/C/D to answer, N/P for next/prev. Add customization. |
| F-031 | Strike-through elimination | P0 | S | Exists | Preserve: click to cross out wrong options. Visual feedback. |
| F-032 | Question flagging/bookmarking | P0 | M | Exists (basic) | Enhanced: flag for review during exam + persist to personal question bank. |
| F-033 | Exam result with detailed analytics | P0 | L | Exists (basic score) | Per-topic breakdown, time analysis, comparison with cohort, weak areas highlighted. |
| F-034 | Lab values reference panel | P1 | M | Exists | Slide-out panel available during exam. Searchable. Common lab values for each track. |
| F-035 | Spaced repetition engine | P1 | L | Not available | SM-2 algorithm: auto-schedule revision of incorrectly answered questions. Daily queue. |
| F-036 | Custom exam builder | P1 | L | Not available | Students create custom exams: select topics, question count, difficulty. |
| F-037 | Exam readiness score | P2 | L | Not available | Algorithm: predicted pass probability based on practice exam performance trends. |
| F-038 | AI-powered question explanations | P2 | M | Not available | Future: LLM-generated explanations for why each option is correct/incorrect. |

### Module 5: Student Dashboard

| ID | Feature | Priority | Size | V1 Status | V2 Notes |
|----|---------|:--------:|:----:|-----------|----------|
| F-039 | Dashboard home | P0 | M | Exists | Welcome card, active subscription, recent courses, upcoming exams, daily stats. |
| F-040 | My courses | P0 | M | Exists | Grid view with progress, last accessed, resume button. |
| F-041 | Progress tracker | P0 | L | Exists (basic) | Visual progress: courses completed, lectures watched, exams taken, accuracy %. |
| F-042 | Study streak | P0 | S | Not available | GitHub-style activity heatmap. Streak counter. Daily study goal. |
| F-043 | Notification center | P1 | M | Not available | In-app notifications for: new content, subscription expiry, exam results, announcements. |
| F-044 | Study planner | P1 | L | Not available | Calendar-based study planning. Set goals per week. Track adherence. |

### Module 6: Payment & Subscriptions

| ID | Feature | Priority | Size | V1 Status | V2 Notes |
|----|---------|:--------:|:----:|-----------|----------|
| F-045 | Package listing / pricing page | P0 | M | Exists (broken) | Dynamic from DB. Comparison table. Track-specific packages. PKR + USD toggle. |
| F-046 | Self-service checkout flow | P0 | XL | Not available | Browse → Select → Pay → Instant Access. Zero admin involvement. |
| F-047 | JazzCash payment gateway | P0 | L | Exists (sandbox creds) | Server-side integration. Production credentials in secrets manager. Webhook verification. |
| F-048 | Easypaisa payment gateway | P0 | L | Exists (sandbox creds) | Same pattern as JazzCash. Server-side only. |
| F-049 | Stripe international payments | P0 | L | Not available | USD payments for AMC/USMLE/PLAB students outside Pakistan. |
| F-050 | Subscription management | P0 | M | Not available | View active plan, renewal date, upgrade/downgrade, cancel, invoices. |
| F-051 | Coupon/discount system | P1 | M | Exists (disabled) | Re-enable and enhance: percentage/fixed, usage limits, expiry, per-track restrictions. |
| F-052 | Automated renewal | P1 | M | Not available | Opt-in auto-renewal. Reminders at 7/3/1 days before expiry. 3-day grace period. |
| F-053 | Invoice generation | P2 | M | Not available | PDF invoices for each payment. Tax details for institutional buyers. |

### Module 7: Admin Panel

| ID | Feature | Priority | Size | V1 Status | V2 Notes |
|----|---------|:--------:|:----:|-----------|----------|
| F-054 | Admin dashboard | P0 | L | Exists | KPIs: active students, revenue, new registrations, conversion rate, subscription health. |
| F-055 | User management | P0 | L | Exists | List, search, filter, create, edit, deactivate. Role assignment. Impersonation (audit-logged). |
| F-056 | Course management | P0 | L | Exists | CRUD courses, sections, lectures. Bulk operations. Drag-and-drop reordering. |
| F-057 | MCQ bank management | P0 | XL | Exists | CRUD questions. Bulk import (CSV/Excel). Tag by topic/difficulty. Usage statistics. |
| F-058 | Enrollment management | P0 | M | Exists (manual) | View all enrollments. Override access dates. Bulk operations. Activity logs. |
| F-059 | Revenue / payment reports | P0 | L | Exists (basic) | Revenue by period, gateway, plan, track. Refund tracking. Export to CSV. |
| F-060 | Subscription package configuration | P0 | M | Exists | CRUD packages: name, price (PKR/USD), duration, courses included, features, display order. |
| F-061 | Site content management | P1 | L | Exists (limited) | CMS-like editing for FAQ, About, Terms, Privacy, Home page sections. WYSIWYG editor. |
| F-062 | Email template management | P1 | M | Not available | CRUD email templates. Variable interpolation. Preview. Test send. |
| F-063 | System configuration | P2 | M | Not available | Feature flags, maintenance mode, global announcements, rate limits via admin UI. |

### Module 8: Communication & Notifications

| ID | Feature | Priority | Size | V1 Status | V2 Notes |
|----|---------|:--------:|:----:|-----------|----------|
| F-064 | Transactional emails | P0 | L | Exists (basic) | Templates: welcome, verification, password reset, enrollment confirmation, expiry warning, receipt. |
| F-065 | In-app notifications | P0 | M | Not available | Real-time via SSE/WebSocket. Mark read/unread. Settings per notification type. |
| F-066 | Push notifications (web) | P1 | M | Not available | Service worker-based push. Opt-in. Study reminders, new content alerts. |
| F-067 | Announcement system | P0 | M | Exists (basic) | Admin creates announcements. Banner or modal. Target by role, plan, or track. Dismissible. |
| F-068 | Email marketing campaigns | P1 | L | Started in v1 | Bulk email with segmentation. Unsubscribe management. Open/click tracking. |
| F-069 | WhatsApp integration | P2 | M | Exists (link only) | WhatsApp Business API: order confirmations, expiry reminders. Opt-in only. |

### Module 9: Certificate System

| ID | Feature | Priority | Size | V1 Status | V2 Notes |
|----|---------|:--------:|:----:|-----------|----------|
| F-070 | Course completion certificate | P0 | L | Exists (basic) | Auto-generated PDF/PNG via server-side rendering. Unique certificate ID. Verifiable URL. |
| F-071 | Exam performance certificate | P0 | M | Exists (basic) | Issued on passing score. Shows percentage, rank, date. Template-based. |
| F-072 | Certificate verification page | P1 | S | Not available | Public URL: enter certificate ID → view details, verify authenticity. |
| F-073 | Custom certificate templates | P2 | L | Exists (admin) | Admin WYSIWYG template editor. Multiple designs per certificate type. |

### Module 10: Ambassador / Referral Program

| ID | Feature | Priority | Size | V1 Status | V2 Notes |
|----|---------|:--------:|:----:|-----------|----------|
| F-074 | Ambassador registration | P0 | M | Exists (hidden) | Public-facing registration. Approval workflow. Dashboard access on approval. |
| F-075 | Referral link generation | P0 | S | Exists | Unique referral codes per ambassador. Trackable links. UTM parameters. |
| F-076 | Referral tracking & analytics | P1 | L | Exists (basic) | Track: clicks, registrations, conversions, revenue attributed. Ambassador dashboard. |
| F-077 | Commission management | P1 | M | Exists (basic) | Define commission rates per plan/track. Calculate earnings. Payout request system. |
| F-078 | Tiered rewards | P2 | M | Not available | Bronze/Silver/Gold/Platinum tiers based on performance. Higher commissions at higher tiers. |

### Module 11: Blog & Content Marketing

| ID | Feature | Priority | Size | V1 Status | V2 Notes |
|----|---------|:--------:|:----:|-----------|----------|
| F-079 | Blog post listing | P0 | M | Exists | Grid/list view. Categorized by exam track. SSG for SEO. |
| F-080 | Blog post detail page | P0 | M | Exists | Rich text rendering. Author info. Related posts. Social sharing. Comments (optional). |
| F-081 | Blog admin (CRUD) | P1 | L | Exists | Rich text editor (TipTap or similar). Image upload. SEO fields. Draft/publish workflow. |
| F-082 | Blog RSS feed | P2 | S | Not available | Auto-generated RSS/Atom feed for blog posts. |

### Module 12: Support & Help Center

| ID | Feature | Priority | Size | V1 Status | V2 Notes |
|----|---------|:--------:|:----:|-----------|----------|
| F-083 | FAQ page | P0 | M | Exists (empty) | Categorized Q&A. Search. Expandable sections. Admin-managed. |
| F-084 | Help center / knowledge base | P0 | L | Not available | Categorized articles: Getting Started, Courses, Exams, Payments, Account. Search. |
| F-085 | Support ticket system | P0 | L | Exists (basic) | Create ticket → assign → respond → resolve. Email notifications. Priority levels. |
| F-086 | Live chat widget | P1 | M | Not available | Third-party integration (Tawk.to or Crisp). Fallback to ticket when offline. |
| F-087 | Chatbot / automated responses | P2 | L | Not available | Rule-based chatbot for common questions. Escalation to human agent. |

### Module 13: SEO & Public Site

| ID | Feature | Priority | Size | V1 Status | V2 Notes |
|----|---------|:--------:|:----:|-----------|----------|
| F-088 | Home / landing page | P0 | L | Exists | Hero section, value propositions, exam tracks, testimonials, CTA. SSG. |
| F-089 | About page | P0 | M | Exists (basic) | Dr. Atif bio, team, mission, credentials, achievements. Professional photography. |
| F-090 | Pricing page | P0 | M | Exists (broken) | Dynamic from DB. Comparison table. "Most popular" badge. Free trial CTA. |
| F-091 | SEO infrastructure | P0 | M | Not available | robots.txt, sitemap.xml, meta tags, og:image, JSON-LD, canonical URLs. |
| F-092 | Terms & Privacy pages | P0 | S | Exists (placeholder) | Actual legal content. GDPR-style consent. Cookie banner. |
| F-093 | Contact page | P1 | S | Exists (basic) | Contact form, map, email, phone, social links. CAPTCHA. |

### Module 14: Analytics & Reporting

| ID | Feature | Priority | Size | V1 Status | V2 Notes |
|----|---------|:--------:|:----:|-----------|----------|
| F-094 | Student progress analytics | P0 | L | Exists (basic) | Per-student: course progress, exam history, accuracy trends, time spent. |
| F-095 | Revenue analytics | P0 | L | Exists (basic) | Daily/weekly/monthly revenue. By gateway, plan, track. Conversion funnel. |
| F-096 | Content analytics | P0 | M | Not available | Most/least popular courses. Completion rates. Drop-off points. |
| F-097 | Cohort analytics | P1 | L | Not available | Compare performance across registration cohorts, exam tracks, subscription plans. |
| F-098 | Export & scheduled reports | P1 | M | Not available | CSV/Excel export. Scheduled email reports (daily/weekly/monthly). |
| F-099 | Real-time dashboard | P2 | L | Not available | WebSocket-powered: active users, live exam count, today's revenue. |

### Module 15: System Administration

| ID | Feature | Priority | Size | V1 Status | V2 Notes |
|----|---------|:--------:|:----:|-----------|----------|
| F-100 | Feature flag management | P0 | M | Not available | Toggle features on/off without deployment. Percentage rollout. Per-role targeting. |
| F-101 | Audit log viewer | P0 | M | Not available | Searchable log: who did what, when. Filterable by user, action, entity. |
| F-102 | Background job monitoring | P0 | M | Not available | Hangfire dashboard: job status, failures, retries, schedules. Admin-only access. |
| F-103 | System health dashboard | P1 | L | Not available | Server metrics, DB connection pool, cache hit rate, error rate, response times. |

---

## 4. MVP Scope Summary (P0 Features)

**65 features** form the MVP, targeting the critical path: secure authentication, course access, MCQ exams, self-service payment, and basic admin tools.

The MVP delivers:
- Complete student journey: register → browse → pay → enroll → learn → take exams → get certificate
- All payment gateways operational (JazzCash, Easypaisa, Stripe)
- Full MCQ exam engine with all v1 strengths preserved
- Admin tools for content macnagement, user management, and reporting
- SEO-ready public site
- Security-by-design (all 16 SEC findings resolved)

Detailed MVP scoping rules in file 26.

---

## 5. Feature Dependency Map

```
F-001 (Registration) ─→ F-006 (RBAC) ─→ F-007 (Sessions)
                                          ↓
F-009 (Course Listing) ─→ F-010 (Course Detail) ─→ F-019 (Video Player)
                                                     ↓
F-045 (Pricing) ─→ F-046 (Checkout) ─→ F-047/048/049 (Payment Gateways)
                                        ↓
                         F-058 (Enrollment) ─→ F-040 (My Courses)
                                               ↓
                         F-027 (Timed Exam) ─→ F-033 (Exam Results)
                                               ↓
                         F-070 (Certificate) ─→ F-072 (Verification)
```

**Critical path**: Registration → RBAC → Course Access → Payment → Enrollment → Learning → Assessment → Certificate

---

*This inventory is the authoritative feature list for V2. Feature IDs (F-001 through F-103) should be referenced in all subsequent planning, backlog, and implementation documents.*
