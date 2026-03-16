# 03 — User Journeys, Personas, and Information Architecture

*Route maps for 3 portals, 7 personas, and 6 redesigned user journeys.*

---

## 1. User Personas

### Primary Personas

#### P1: Medical Student (Pakistan-based)
- **Name**: Ayesha — Final-year MBBS student preparing for FCPS-1
- **Age**: 23, Karachi
- **Device**: Android phone (70% of usage), laptop (30%)
- **Connection**: 4G mobile data, 15-30 Mbps
- **Goals**: Pass FCPS-1 on first attempt, comprehensive MCQ practice, video lectures for weak topics
- **Pain Points (V1)**: Long registration wait, manual payment process, inconsistent mobile experience
- **Willingness to Pay**: PKR 3,000–10,000 for comprehensive access

#### P2: International Medical Graduate
- **Name**: Dr. Ahmed — Pakistani doctor preparing for AMC-1 (Australia)
- **Age**: 28, Sydney
- **Device**: MacBook Pro (60%), iPhone (40%)
- **Connection**: Broadband, 50+ Mbps
- **Goals**: Structured AMC-1 prep, practice exams under timed conditions, track readiness
- **Pain Points (V1)**: No international payment option, slow page loads, no progress analytics
- **Willingness to Pay**: USD 30–100

#### P3: Returning Student
- **Name**: Fatima — Previously used FAME for PLAB, now preparing for UKMLA
- **Age**: 26, London
- **Device**: Windows laptop (80%), Android (20%)
- **Connection**: Broadband, 100+ Mbps
- **Goals**: Quick re-enrollment in new track, retain history from previous subscription
- **Pain Points (V1)**: Must re-contact admin to renew, no subscription management, no historical data
- **Willingness to Pay**: USD 50–120

### Secondary Personas

#### P4: Admin / Content Manager
- **Name**: Bilal — FAME operations manager
- **Primary Tasks**: Manage enrollments, upload content, handle support tickets, generate reports
- **Pain Points (V1)**: Every enrollment is manual, no bulk operations, limited reporting

#### P5: Dr. Atif (Founder / Instructor)
- **Primary Tasks**: Create courses, manage exam content, review analytics, strategic decisions
- **Pain Points (V1)**: No content analytics, no self-service purchases means constant admin work

#### P6: Ambassador
- **Name**: Sana — Medical student who refers peers for commission
- **Primary Tasks**: Share referral links, track conversions, request payouts
- **Pain Points (V1)**: Ambassador area is hidden, limited tracking, no payout system

#### P7: Support Agent
- **Name**: Hassan — Part-time support staff
- **Primary Tasks**: Respond to student queries, resolve payment issues, escalate technical problems
- **Pain Points (V1)**: No ticketing system, WhatsApp-only support, no knowledge base

---

## 2. Information Architecture

### 2.1 Public Site (Unauthenticated)

```
/ (Home)
├── /courses                          ← Browse all courses
│   ├── /courses/[track]              ← Filter by exam track (e.g., /courses/fcps-1)
│   └── /courses/[track]/[slug]       ← Course detail + preview
├── /pricing                          ← Subscription packages
├── /about                            ← About FAME, Dr. Atif, team
├── /blog                             ← Blog listing
│   └── /blog/[slug]                  ← Blog post
├── /faq                              ← Frequently asked questions
├── /contact                          ← Contact form + details
├── /terms                            ← Terms of service
├── /privacy                          ← Privacy policy
├── /ambassador                       ← Ambassador program info + signup
├── /verify/[certificateId]           ← Certificate verification
├── /login                            ← Sign in
├── /register                         ← Sign up
├── /forgot-password                  ← Password reset request
└── /reset-password/[token]           ← Password reset form
```

### 2.2 Student Portal (Authenticated — Student Role)

```
/dashboard                            ← Student home
├── /dashboard/courses                ← My enrolled courses
│   └── /dashboard/courses/[id]       ← Course player (video + notes)
│       └── /dashboard/courses/[id]/[sectionId]  ← Specific section
├── /dashboard/exams                  ← My exams
│   ├── /dashboard/exams/start/[id]   ← Start exam (mode selection)
│   ├── /dashboard/exams/take/[id]    ← Exam in progress
│   └── /dashboard/exams/result/[id]  ← Exam results + analytics
├── /dashboard/progress               ← Overall progress + analytics
├── /dashboard/revision               ← Spaced repetition queue (P1)
├── /dashboard/bookmarks              ← Bookmarked questions
├── /dashboard/certificates           ← My certificates
├── /dashboard/subscription           ← Subscription management
│   ├── /dashboard/subscription/upgrade ← Upgrade plan
│   └── /dashboard/subscription/invoices ← Payment history
├── /dashboard/support                ← Support tickets
│   └── /dashboard/support/[ticketId] ← Ticket detail
├── /dashboard/notifications          ← Notification center
└── /dashboard/settings               ← Profile, password, preferences
```

### 2.3 Admin Panel (Authenticated — Admin/Staff Roles)

```
/admin                                ← Admin dashboard
├── /admin/users                      ← User management
│   ├── /admin/users/[id]             ← User detail + activity
│   └── /admin/users/create           ← Create user
├── /admin/courses                    ← Course management
│   ├── /admin/courses/create         ← New course wizard
│   └── /admin/courses/[id]/edit      ← Edit course + sections
├── /admin/questions                  ← MCQ bank management
│   ├── /admin/questions/import       ← Bulk import
│   └── /admin/questions/[id]/edit    ← Edit question
├── /admin/exams                      ← Exam configuration
│   └── /admin/exams/create           ← Create exam from question bank
├── /admin/enrollments                ← Enrollment management
├── /admin/subscriptions              ← Subscription packages
│   └── /admin/subscriptions/[id]/edit ← Edit package
├── /admin/payments                   ← Payment transactions
│   └── /admin/payments/[id]          ← Transaction detail
├── /admin/coupons                    ← Coupon management
├── /admin/ambassadors                ← Ambassador management
│   └── /admin/ambassadors/[id]       ← Ambassador detail
├── /admin/blog                       ← Blog management
│   ├── /admin/blog/create            ← New post
│   └── /admin/blog/[id]/edit         ← Edit post
├── /admin/certificates               ← Certificate templates
├── /admin/content                    ← CMS pages (FAQ, About, etc.)
├── /admin/support                    ← Support ticket queue
│   └── /admin/support/[ticketId]     ← Ticket detail + respond
├── /admin/reports                    ← Analytics & reports
│   ├── /admin/reports/revenue        ← Revenue dashboard
│   ├── /admin/reports/students       ← Student analytics
│   └── /admin/reports/content        ← Content analytics
├── /admin/emails                     ← Email templates + campaigns
├── /admin/audit-log                  ← System audit log
├── /admin/jobs                       ← Background job monitoring
└── /admin/settings                   ← System configuration + feature flags
```

### 2.4 Ambassador Portal (Authenticated — Ambassador Role)

```
/ambassador                           ← Ambassador dashboard
├── /ambassador/referrals             ← Referral tracking
├── /ambassador/earnings              ← Commission & payouts
├── /ambassador/links                 ← Generate referral links
└── /ambassador/settings              ← Profile & payout preferences
```

---

## 3. Redesigned User Journeys

### Journey 1: New Student Registration → First Course Access

#### V1 Flow (7+ Steps, Hours to Days)
```
1. Visit website → 2. Find registration → 3. Fill 7-step wizard (name, email, 
   phone, CNIC, password, city, institution) → 4. Wait for admin approval 
   (hours/days) → 5. Receive approval email → 6. Contact admin for package info 
   → 7. Bank transfer → 8. Send payment proof to admin → 9. Wait for admin 
   to verify → 10. Admin activates enrollment → 11. Login → 12. Start learning
```
**Time: Hours to days** | **Admin touchpoints: 3** | **Dropout risk: Very High**

#### V2 Flow (3 Steps, < 10 Minutes)
```
Step 1: Register (30 seconds)
  ├── Enter: name, email, password
  ├── OR: Click "Continue with Google"
  └── Submit → Email verification sent

Step 2: Verify Email (60 seconds)
  ├── Click link in email
  ├── OR: Enter 6-digit code
  └── Account activated, redirected to dashboard

Step 3: Browse → Purchase → Start (5-8 minutes)
  ├── Browse course catalog / pricing page
  ├── Select package → Checkout page
  ├── Choose payment method (JazzCash / Easypaisa / Stripe)
  ├── Complete payment → Webhook confirms
  ├── Enrollment auto-created → Courses unlocked
  └── Redirected to first course → Start learning
```
**Time: < 10 minutes** | **Admin touchpoints: 0** | **Dropout risk: Low**

---

### Journey 2: Existing Student → Take an Exam

#### V1 Flow
```
1. Login → 2. Navigate to exam section (confusing, varies by view) → 
3. Select exam → 4. Start exam → 5. Answer questions (keyboard works!) 
→ 6. Submit → 7. See basic score → End
```

#### V2 Flow
```
1. Login / Resume session
2. Dashboard → "Take Exam" card or sidebar link
3. Select exam track → Choose exam type:
   ├── Timed Exam (real exam conditions)
   ├── Practice Mode (immediate feedback)
   └── Custom Exam (select topics, count, difficulty) [P1]
4. Pre-exam panel: confirm settings, view lab values option
5. Exam in progress:
   ├── Question + 4 options (A/B/C/D keyboard shortcuts)
   ├── Navigation panel (answered/unanswered/flagged)
   ├── Strike-through elimination
   ├── Flag for review
   ├── Lab values panel (slide-out)
   └── Timer (timed mode only)
6. Review flagged questions (if any)
7. Submit → Confirmation dialog
8. Results page:
   ├── Score + pass/fail
   ├── Per-topic breakdown (strengths & weaknesses)
   ├── Time analysis per question
   ├── Percentile rank vs cohort
   ├── Recommended study areas with links to lectures
   └── Option to add missed questions to revision queue
9. Certificate generated (if passing score)
```

---

### Journey 3: Student → Renew Subscription

#### V1 Flow
```
1. Subscription expires → 2. No notification → 3. Notice access revoked on login 
→ 4. Contact admin (WhatsApp/email) → 5. Admin confirms amount → 6. Bank transfer 
→ 7. Send proof → 8. Admin verifies → 9. Admin manually extends → 10. Access restored
```
**Time: 1-3 days** | **Admin touchpoints: 3+**

#### V2 Flow
```
1. Day -7: Email + in-app notification: "Your subscription expires in 7 days"
2. Day -3: Reminder notification with one-click renewal link
3. Day -1: Final reminder
4. Student clicks "Renew Now" → pre-filled checkout (same plan)
5. Complete payment → Subscription extended → Confirmation email
   
   OR (auto-renewal enabled):
   
1. Day -3: Notification: "Your subscription will auto-renew in 3 days"
2. Day 0: Payment processed automatically → Confirmation email
3. Student continues learning uninterrupted
```
**Time: < 2 minutes (manual) / 0 minutes (auto)** | **Admin touchpoints: 0**

---

### Journey 4: Admin → Add New Course Content

#### V1 Flow
```
1. Login to admin → 2. Navigate to course management → 3. Create course 
   (limited fields) → 4. Manually copy files to server → 5. Add YouTube URLs 
   one by one → 6. No preview → 7. Published immediately (no draft state)
```

#### V2 Flow
```
1. Login to admin → Admin dashboard
2. Courses → Create New Course
3. Course wizard:
   ├── Step 1: Basic info (title, track, description, thumbnail upload)
   ├── Step 2: Curriculum (add sections → add lectures via drag-and-drop)
   ├── Step 3: Settings (pricing, prerequisites, enrollment rules)
   └── Step 4: Preview → Review → Save as Draft
4. For each lecture:
   ├── Enter YouTube video URL → Auto-fetch thumbnail + duration
   ├── Add chapter markers (timestamps)
   ├── Attach supplementary materials (PDF, notes)
   └── Mark as free preview (optional)
5. Review complete course → Publish
6. Course appears in catalog (SSG page regenerated via ISR)
```

---

### Journey 5: New Ambassador → First Referral → Payout

#### V1 Flow
```
1. Find ambassador area (hidden) → 2. Register → 3. Wait for approval → 
4. Get referral link → 5. Share → 6. Hope someone signs up → 7. Manual tracking 
→ 8. Contact admin for payout → 9. Admin calculates commission → 10. Bank transfer
```

#### V2 Flow
```
1. Visit /ambassador → Learn about program → Click "Become an Ambassador"
2. Fill application: name, email, social media, how you'll promote
3. Admin reviews → Approves (notification sent)
4. Ambassador dashboard:
   ├── Generate referral links (per course or general)
   ├── Share to social media (pre-filled copy)
   └── Track in real-time: clicks, registrations, conversions
5. Referred student uses link → Registers → Purchases → Ambassador credited
6. Earnings accumulate → View in dashboard
7. Request payout when minimum reached → Admin approves → Payment sent
8. Monthly report: performance summary, leaderboard rank
```

---

### Journey 6: Student → Get Help

#### V1 Flow
```
1. Look for help → Find WhatsApp link (sometimes broken) → 2. Send message 
→ 3. Wait for response (no SLA) → 4. Issue may or may not be tracked
```

#### V2 Flow
```
1. Click "Help" in sidebar → Help center
2. Browse FAQs by category:
   ├── Getting Started
   ├── Courses & Content
   ├── Exams & Assessment
   ├── Payments & Subscriptions
   └── Account & Technical
3. Search knowledge base → Instant results
4. If not resolved → "Create Support Ticket"
   ├── Select category → Describe issue → Attach screenshot
   ├── Ticket created → ID provided → Email confirmation
   └── Track status in /dashboard/support
5. Support agent responds → Student notified
6. Resolved → Satisfaction rating
7. WhatsApp available as secondary channel (link in help center)
```

---

## 4. Navigation Architecture

### 4.1 Public Site Navigation

```
┌─────────────────────────────────────────────────────────┐
│  [FAME Logo]   Courses   Pricing   Blog   About   FAQ  │
│                                          [Login] [Sign Up] │
└─────────────────────────────────────────────────────────┘
```

- **Mobile**: Hamburger menu → slide-out drawer
- **Courses** dropdown: exam tracks as sub-items
- **CTA buttons**: always visible, high contrast

### 4.2 Student Portal Navigation

```
┌──────────────────┬───────────────────────────────────────┐
│  [FAME Logo]     │  Search [_______]     [Bell] [Avatar] │
├──────────────────┼───────────────────────────────────────┤
│  ≡ Dashboard     │                                       │
│  📚 My Courses   │                                       │
│  📝 Exams        │          [Page Content]                │
│  📊 Progress     │                                       │
│  🔖 Bookmarks    │                                       │
│  📄 Certificates │                                       │
│  ─────────────── │                                       │
│  💳 Subscription │                                       │
│  🎫 Support      │                                       │
│  ⚙️ Settings     │                                       │
└──────────────────┴───────────────────────────────────────┘
```

- **Mobile**: Bottom tab bar (Dashboard, Courses, Exams, More)
- **Sidebar**: collapsible, persisted state
- **Notification bell**: unread count badge

### 4.3 Admin Panel Navigation

```
┌──────────────────┬───────────────────────────────────────┐
│  [FAME Admin]    │  Search [_______]     [Bell] [Avatar] │
├──────────────────┼───────────────────────────────────────┤
│  ≡ Dashboard     │                                       │
│  ─── CONTENT ─── │                                       │
│  📚 Courses      │          [Page Content]                │
│  📝 Questions    │                                       │
│  📰 Blog         │                                       │
│  📄 Pages        │                                       │
│  ─── USERS ───── │                                       │
│  👥 Users        │                                       │
│  📋 Enrollments  │                                       │
│  🤝 Ambassadors  │                                       │
│  ─── COMMERCE ── │                                       │
│  💰 Payments     │                                       │
│  📦 Packages     │                                       │
│  🏷️ Coupons      │                                       │
│  ─── SYSTEM ──── │                                       │
│  📊 Reports      │                                       │
│  📧 Emails       │                                       │
│  🎫 Support      │                                       │
│  📋 Audit Log    │                                       │
│  ⚙️ Settings     │                                       │
└──────────────────┴───────────────────────────────────────┘
```

---

## 5. Key Screen Wireframe Descriptions

### Landing Page (/)
- **Hero**: Full-width gradient background, headline "Master Medical Exams with Confidence", sub-headline about 13+ exam tracks, dual CTA ("Browse Courses" + "Start Free Trial")
- **Trust Bar**: "5,000+ students", "13 exam tracks", "77,000+ MCQs", Dr. Atif credentials
- **Exam Tracks**: Horizontal scroll of track cards (FCPS-1, USMLE, AMC-1, PLAB, etc.)
- **How It Works**: 3-step: Register → Choose Your Track → Start Learning
- **Testimonials**: Real student quotes with photo, name, exam, result
- **Pricing Preview**: 3 most popular packages with "View All Plans" link
- **Footer**: Links, social, copyright, legal

### Exam Engine (/dashboard/exams/take/[id])
- **Full-screen mode** (sidebar hidden for focus)
- **Top bar**: Timer (timed mode) | Question # / Total | Flag button | Lab Values button
- **Main area**: Question text + 4 option cards (A/B/C/D)
- **Bottom bar**: Previous | Navigation grid | Next | Submit
- **Keyboard overlay hint** (first exam): "Press A/B/C/D to answer, N for next"
- **Strike-through**: Right-click or dedicated button on each option
- **Lab Values panel**: Slide-in from right, searchable, categorized

### Checkout (/pricing → Checkout)
- **Left**: Order summary (package name, duration, features, price)
- **Right**: Payment method selection (JazzCash / Easypaisa / Stripe card)
- **Coupon field**: Enter code → Apply → Updated total
- **Trust indicators**: Secure payment badge, money-back guarantee, support contact
- **Submit**: "Complete Purchase" → Loading → Success → Redirect to dashboard

---

*This document defines who uses FAME V2, how they navigate it, and what their ideal experience looks like. All design and development work should reference these journeys as acceptance criteria.*
