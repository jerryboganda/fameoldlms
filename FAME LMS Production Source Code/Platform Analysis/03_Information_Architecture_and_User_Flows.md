# 03 — Information Architecture and User Flows

---

## 1. Overall Site Structure

The FAME LMS is organized into a multi-tier architecture with separate portals for different user roles, public-facing marketing pages, and modular feature areas.

### Top-Level Structure

```
firstaidmadeeasy.com.pk/
├── Landing/Home/         → Public marketing site (16 pages)
├── Account/              → Authentication (21 views)
├── Home/                 → Public content pages (16 views)
├── Student/              → Student LMS portal (22 views)
├── Admin/                → Admin management (14 views)
├── Teacher/              → Teacher panel (5 views)
├── SuppAgent/            → Support agent dashboard (1 view)
├── Areas/
│   ├── Ambassador/       → Referral program portal
│   ├── Blogs/            → Blog system
│   ├── Certificate/      → Certificate management
│   ├── DailyReport/      → Staff daily reports
│   ├── FileManager/      → File management
│   └── Landing/          → Marketing landing pages
├── Course/               → Course management (admin) (9 views)
├── Exam/                 → Exam management (admin) (3 views)
├── Questions/            → MCQ bank management (9 views)
├── QuestionPaper/        → Exam paper creation (15 views)
├── Enrollment/           → Subscription management (6 views)
├── Chat/                 → Messaging system (11 views)
├── Ticket/               → Support tickets (4 views)
├── Communication/        → Email marketing (10 views)
├── Notification/         → Notifications (4 views)
├── Report/               → Reports (7 views)
└── [14 more view folders]
```

**Total**: ~301 view files across 40 view folders + 6 Area modules

---

## 2. Navigation Hierarchy

### Public Site Navigation (Landing Area)

```
Homepage (Landing/Home/Index)
├── Login → /Account/Login
├── Register → /Account/Register
├── Free Demo Lectures → /Landing/Home/DemoVideos
├── Free Trial MCQ → /QuestionPaper/SolveFreeTrialTest
├── Guidelines → /Landing/Home/Guidelines
├── Our Packages → /Landing/Home/OurPackages
├── About Us → /Landing/Home/AboutUs
├── Contact Us → /Landing/Home/ContactUs
├── Success Stories → /Landing/Home/SuccessStories
├── Our Differences → /Landing/Home/OurDifferences
├── Collaboration → /Landing/Home/Collaboration
├── Team/Ambassadors → /Landing/Home/Team
├── FAQs → /Landing/Home/Faqs
├── Privacy Policy → /Landing/Home/PrivacyPolicy
├── Terms & Conditions → /Landing/Home/TermsAndConditions
├── Blogs → /blogs
└── Book Mistakes → /BookMistakes/Student
```

**Footer Links**: Online Platform section, Links section, Contacts section with newsletter subscription

### Student Portal Navigation (Sidebar: `_SidebarPro.cshtml`)

```
Student Portal
├── Dashboard → /Student/Index
├── Learning
│   ├── My Courses → /Student/MyCourses
│   ├── Live Classes → /Student/MyMeetings
│   ├── Create Exam → /QuestionPaper/CreateTest
│   ├── Mock Tests → /Student/MockTests
│   ├── Progress Tracker → /Student/MyProgress
│   ├── My Certificates → /Certificate/Student
│   ├── Guideline Videos → /Student/GuidelineVideos
│   └── My Subscription → /Student/MySubscriptions
├── Support
│   └── Support Center → /Home/SupportOverView
└── Social Media
    ├── Facebook
    ├── Instagram
    ├── YouTube
    └── TikTok
```

### Admin Panel Navigation (Sidebar: `_TeacherAside.cshtml`, 680 lines)

```
Admin Panel (Role-conditional)
├── Settings (Admin only)
│   ├── System Settings
│   ├── SMTP Settings
│   └── Email Report Settings
├── Courses
│   ├── Manage Courses
│   ├── Packages, Website Packages
│   ├── Guideline Videos
│   ├── File Manager
│   ├── Copy Sections, Book Code, Book Mistakes
│   └── Certificates (Manager, Templates, Rules, Issues, Upload, Google Sheets)
├── Enrollments
│   ├── Student Requests (Register, Extension, Device Delete)
│   ├── Add Subscriptions
│   └── Installments
├── Students
│   ├── Student List, Search
│   ├── Progress/Question/Trial/Subscription Reports
│   └── Reset Password, Verify Email, Reset Devices
├── Question Bank
│   ├── MCQs (Add, List, Import, AI Upload 2026)
│   ├── Question Papers (Add, List)
│   └── Question Systems, Partitions
├── Manage (Admin only)
│   ├── Support Agents, FAQs, Tutorials
│   ├── Trial Videos, Popup Slides
│   └── Task Reminder, Approve Comments
├── Communication
│   ├── Notifications
│   ├── Email Marketing (Campaigns, Templates, Audiences, Senders, Drips)
│   ├── Live Classes (Create/List)
│   └── Chat (Groups, Friends, Conversations)
├── Extras
│   ├── Student Groups
│   └── Blogs
└── Ambassador Program (Admin only)
    ├── Dashboard, All Ambassadors
    ├── Applications, Payout Requests
    └── Commission Rules
```

---

## 3. Major Journey Maps

### Journey 1: New Student Registration

```
Landing Page → "Register New User" CTA
↓
Step 1: User Info (Full Name, Father Name, Sponsor Name, Email]
↓
Step 2: Email Verification (6-digit code, countdown timer, resend)
↓
Step 3: Job Info (Country, Mobile, Occupation, City, Institute, Year of MBBS)
↓
Step 4: Enrollment Info (Package, Exam Attempt, Duration, Payment Method)
↓
Step 5: CNIC Upload (Front + Back images)
↓
Step 6: Profile Photo Upload
↓
Step 7: Password Creation (with strength meter)
↓
Registration Request Submitted → Admin Manual Approval → Access Granted
```

**Observations**:
- ⚠️ **7-step wizard is lengthy** — 1,139 lines of code in Register.cshtml
- ⚠️ **CNIC upload at registration** is unusual for an online education platform — collects significant PII before any service delivery
- ⚠️ **Manual admin approval** creates bottleneck and delays access
- ✅ **Email verification** with code is a good practice
- ✅ **Password strength meter** provides good visual feedback
- ❌ **CSRF token is commented out** on the registration form — security vulnerability

### Journey 2: Student Login and Dashboard

```
Login Page (/Account/Login)
↓
Email + Password → Verify Email Confirmed → Login
↓
Student Dashboard (/Student/Index)
├── Welcome Card (name, days remaining)
├── Stats Grid (Active Courses, Live Classes, Days Left)
├── Quick Actions (Courses, Mock Tests, Live Classes, Support)
└── Recent Courses (up to 4, with thumbnails and progress)
```

**Observations**:
- ✅ **Personalized welcome** with first name extraction
- ✅ **Subscription awareness** — conditional rendering for active vs expired
- ✅ **Days remaining** shown with danger color when < 10 days
- ⚠️ **No "Continue Learning"** button for last-accessed lecture
- ⚠️ **External login options hidden** (`display:none !important` on ExternalLoginPartial)
- ❌ **No CSRF token** on login form

### Journey 3: Course Consumption

```
Dashboard → My Courses → Course Card Click
↓
TakeCourse.cshtml — Video Player + Sidebar
├── Video Player (full-width on mobile, YouTube embed)
├── Section Accordion (hierarchical navigation)
│   ├── Section → Sub-section → Video List
│   ├── Lecture count and duration per section
│   └── Focus Mode toggle
└── Navigation: Previous/Next lesson
```

**Observations**:
- ✅ **Hierarchical content structure** (Course → Section → Sub-section → Video)
- ✅ **Focus Mode** for distraction-free learning
- ✅ **Duration display** in human-readable format (Xh Ym)
- ✅ **Mobile-first** extensive responsive CSS
- ⚠️ **Video relies on YouTube** — platform doesn't control hosting
- ⚠️ **No bookmarking or resume position** visible

### Journey 4: Exam Taking

```
Create Exam (4-step wizard) or Mock Test Selection
↓
Start Exam (/Student/StartExam)
↓
Take Exam (/Student/TakeExam) — Fullscreen standalone page
├── Fixed Header (timer, exam title)
├── Question Display (stem + options A–E)
├── Navigation Sidebar (question dots grid)
├── Actions: Flag, Strike-through, Lab Values
├── Keyboard Shortcuts (A–E, arrows)
└── Modes: Normal, Tutor (immediate feedback), Review
↓
Submit → Exam Result (/Student/ExamResult)
├── Circular Score Chart (ApexCharts)
├── Q-dot Grid (correct/wrong/skipped)
├── Metric Cards
└── Actions: Print/PDF, Review All Questions
```

**Observations**:
- ✅ **USMLE-style interface** — matches industry standard
- ✅ **Keyboard shortcuts** for efficient navigation
- ✅ **Strike-through** for elimination strategy
- ✅ **Lab Values modal** — essential for medical exams
- ✅ **Tutor mode** with immediate feedback
- ✅ **Review mode** for post-exam analysis
- ⚠️ **Results loaded client-side** — potential for empty states if API fails

### Journey 5: Support Request

```
Student Portal → Support Center (/Home/SupportOverView)
├── WhatsApp Quick Help — links to 923000000000 (PLACEHOLDER!)
├── Email Support
├── My Tickets → /Student/TicketList
├── Help Center/FAQs
└── Create Ticket → /Student/Ticket
    ↓
    Ticket Created → Admin/Agent handles → /Ticket/Reply
    ↓
    Student views reply → /Student/TicketReply
```

**Observations**:
- ❌ **WhatsApp number is a placeholder** (`923000000000`) — students cannot reach support
- ✅ **Ticket system** is functional with student/admin views
- ⚠️ **FAQ mostly empty** — 6 of 7 questions have no visible answers on homepage
- ✅ **Tutorial videos** available as self-help

---

## 4. Key Friction Points

| # | Friction Point | Journey | Impact | Severity |
|---|---------------|---------|--------|----------|
| 1 | **7-step registration wizard** with CNIC upload | Registration | High drop-off risk — barrier to entry for prospective students | High |
| 2 | **Manual admin approval** after registration | Registration → Access | Undefined wait time delays learning; no status visibility for student | High |
| 3 | **Placeholder WhatsApp number** in Support Center | Support | Students in need cannot reach live support via the primary quick channel | Critical |
| 4 | **No self-service package purchase** visible | Payment → Access | Students cannot independently complete enrollment without admin intervention | High |
| 5 | **Mixed layout systems** across sections | Cross-platform | Inconsistent navigation and visual language when moving between student portal and public pages | Medium |
| 6 | **Pricing page shows USD template content** | Package selection | Confusing—Pakistani students see dollar pricing with broken action links | High |
| 7 | **Login page has hidden social login buttons** | Login | Google/Facebook OAuth configured but hidden with `display:none` — missed convenience opportunity | Medium |
| 8 | **Empty FAQ answers** on homepage | Self-service help | Only 1 of 7 FAQ questions has a visible answer; defeats the purpose of the FAQ section | Medium |
| 9 | **No "Continue Learning" shortcut** on dashboard | Course resumption | Students must navigate to My Courses → find course → locate where they left off | Medium |
| 10 | **Expired subscription with no renewal button** | Retention | Dashboard shows expired status but the self-service renewal flow is not apparent | High |

---

## 5. Missing Steps and Confusing Loops

### Missing Elements
1. **Email confirmation success page** — After email verification, the transition to the next step could be clearer
2. **Payment confirmation page** — No clear "payment received, access pending" state visible
3. **Onboarding flow** — No guided first-use experience after initial login
4. **Subscription renewal self-service** — Expired users have no clear path to renew without contacting support/admin
5. **Course completion celebration** — No visible completion state or congratulatory message

### Potential Confusion Points
1. **Two pricing pages exist**: `/Home/Pricing` (template, USD) and `/Landing/Home/OurPackages` (real, PKR) — which one do students find?
2. **Two About Us pages**: `/Home/AboutUs` (old theme) and `/Landing/Home/AboutUs` — content may differ
3. **JazzCash page uses old layout** while student portal uses 2026 layout — visual discontinuity during payment
4. **Coupon system exists** (views present) but is **disabled** (sidebar link commented out) — dead feature confusion if reachable via direct URL
5. **TakeCourseNew.cshtml** and **TakeCourse.cshtml** both exist — unclear which is active and whether students can reach both

---

## 6. Content Discoverability Assessment

| Content Type | Discovery Method | Quality |
|-------------|-----------------|---------|
| Courses | Dashboard recent courses + My Courses page | ✅ Good — search + filter by package |
| MCQ Exams | Create Exam wizard + Mock Tests page | ✅ Good — step-by-step creation |
| Free Trial MCQ | Landing page CTA | ✅ Good — prominent call-to-action |
| Demo Videos | Landing page CTA | ✅ Good — direct link from homepage |
| Certificates | Student sidebar link | ✅ Good — dedicated section |
| Support | Student sidebar + Support Overview | ⚠️ Partial — placeholder contact info |
| Guidelines | Landing page link | ✅ Good |
| Live Classes | Student sidebar | ✅ Good |
| Pricing/Packages | Multiple competing pages | ❌ Poor — confusion between template and real pages |
| Study Schedule | Student views exist | ⚠️ Partial — not prominently featured in sidebar |
| Guideline Videos | Student sidebar | ✅ Good |
| FAQs | Footer link + Support section | ⚠️ Partial — mostly empty content |

---

*This analysis is based on view file inspection, navigation structure analysis, and public website observation. Actual user behavior data (analytics, heatmaps, funnel metrics) would provide additional journey insights and is recommended as a follow-up.*
