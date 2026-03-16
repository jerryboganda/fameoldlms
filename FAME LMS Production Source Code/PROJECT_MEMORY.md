# PROJECT_MEMORY.md

> **Authoritative Project Context**
> Last Updated: Feb 7-8, 2026
>
> This file acts as the single source of truth for the FAME LMS project, consolidating previous context files (`CODEBASE.md`, `SESSION_LOG.md`, `README.md`, etc.).

---

## 1. 🏁 Project Overview

**FAME LMS (First Aid Made Easy)** is a comprehensive Learning Management System for medical exam preparation.

- **Main Goal**: Prepare students for medical exams (FCPS, NRE, JCAT, etc.) via video lectures, mock tests, and live interaction.
- **Production URL**: [firstaidmadeeasy.com.pk](https://firstaidmadeeasy.com.pk)
- **Student Portal**: [firstaidmadeeasy.com.pk/Student/](https://firstaidmadeeasy.com.pk/Student/)
- **Source Path**: `c:\Users\Administrator\Desktop\FAME LMS Production Source Code`
- **Production Path**: `c:\firstaidmadeeasy.com.pk` (IIS Root)

### Core Components
1.  **FAME.Web (ASP.NET MVC)**: Main LMS platform (courses, subscriptions, admin).
2.  **FAME MCQ Portal (Next.js)**: High-performance standalone specific content.

---

## 2. 💻 Tech Stack & Environment

**OS**: Windows Server
**Terminal**: PowerShell
**IDE**: Visual Studio / VS Code

### Backend
-   **Framework**: ASP.NET MVC 5.2.9, Web API 2 (.NET Framework 4.7.2)
-   **Database**: SQL Server (`FAME_DB` on `198.23.185.173,1433`), Entity Framework 6.4.4
-   **EF Contexts**:
    -   `FAMEEntities` — Database-First (EDMX) for core LMS tables
    -   `AmbassadorDbContext` — Code-First for Ambassador module (isolated to avoid EDMX conflicts)
-   **DI**: Autofac 6.0.0 (`InstancePerRequest()`)
-   **Real-time**: SignalR
-   **Auth**: ASP.NET Identity (Roles: Admin, Teacher, Student, Assistant, UniTeacher, SuppAgent)
-   **Logging**: Serilog (→ `App_Data/Logs/`)
-   **Build**: MSBuild 16.11.6 (`C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools`)

### Frontend
-   **Framework**: jQuery, Razor Views (`.cshtml`)
-   **Design System (FAME 2026)**:
    -   **Base**: Bootstrap 5.3 + Metronic 8 Theme
    -   **Colors**: Clinical Teal (`#0F766E`) & Professional Slate (`#1E293B`). *Purple/Violet is banned.*
    -   **CSS**: `fame-student-2026.css` (in `~/bundles/student-2026-css`)
    -   **Icons**: Bootstrap Icons, Lucide
-   **MCQ Portal**: Next.js 15, React 19, Tailwind v4

---

## 3. 📂 Project Structure

### Source Code
```
FAME LMS Production Source Code/
├── FAME.Web/                    # Main ASP.NET MVC project
│   ├── Areas/
│   │   └── Ambassador/          # Ambassador Program Module (Feb 6)
│   │       ├── Controllers/     # AdminController, AmbassadorController
│   │       ├── Views/Admin/     # Dashboard, Ambassadors, Applications, Payouts, CommissionRules
│   │       └── Views/Ambassador/# Apply, Dashboard, Referrals, Earnings, Payouts
│   ├── BLL/                     # Business Logic Layer
│   │   ├── AmbassadorRepository.cs
│   │   ├── ReferralRepository.cs
│   │   ├── CommissionService.cs
│   │   ├── PayoutService.cs
│   │   └── AmbassadorAuditService.cs
│   ├── DAL/                     # Data Access Layer (EF6)
│   │   ├── FAMEEntities.edmx    # Database-First (core LMS)
│   │   ├── AmbassadorDbContext.cs # Code-First (Ambassador module)
│   │   └── GuidelineContext.cs  # Code-First (Guideline Videos)
│   ├── Controllers/             # MVC Controllers
│   │   ├── StudentController.cs # Student portal (MyCourses, TakeCourse, GuidelineVideos, etc.)
│   │   ├── ProfileController.cs # Profile/MyProfile (routes Students → ProfileStudent view)
│   │   ├── ManageController.cs  # ChangePassword (routes Students → StChangePassword view)
│   │   └── GuidelineVideoController.cs # Admin CRUD for guideline videos
│   ├── ControllersApi/          # API Controllers
│   ├── Models/
│   │   ├── AmbassadorViewModels.cs # All Ambassador VMs
│   │   ├── CourseVM.cs          # Course model (inherits IsEnrolled: Lessons, DurationInMin, Progress)
│   │   ├── SectionVM.cs         # Section model (inherits IsEnrolled: Lessons, DurationInMin)
│   │   ├── SectionSubVM.cs      # Sub-section model (own Lessons, DurationInMin)
│   │   ├── VideoVM.cs           # Video model (Video_Length decimal?, IsWatched)
│   │   ├── UserVM.cs            # User profile model (CNIC, Institute, City, OccupationID, etc.)
│   │   └── GuidelineVideoVM.cs  # Guideline video model
│   ├── Views/                   # Razor Views
│   │   ├── Student/             # Student Portal
│   │   │   ├── Index.cshtml     # Dashboard (2026 redesign)
│   │   │   ├── MyCourses.cshtml # Course list (2026 design)
│   │   │   ├── TakeCourse.cshtml# Video player + playlist (Focus Mode, lecture counts, durations)
│   │   │   ├── TakeExam.cshtml  # Exam engine (event delegation, review mode, custom test)
│   │   │   ├── ExamResult.cshtml# Exam results
│   │   │   ├── GuidelineVideos.cshtml # YouTube guideline videos (thumbnails, /live/ fix)
│   │   │   └── MockTests.cshtml
│   │   ├── Profile/
│   │   │   ├── ProfileStudent.cshtml  # Student profile (2026 redesign, all fields)
│   │   │   └── MyProfile.cshtml       # Non-student profile (2026 design)
│   │   ├── Manage/
│   │   │   ├── StChangePassword.cshtml # Student change password (2026 redesign)
│   │   │   └── ChangePassword.cshtml   # Non-student change password
│   │   ├── Notification/
│   │   │   └── MessageIndex.cshtml     # Admin Popup Slides Manager (Feb 7)
│   │   ├── QuestionPaper/
│   │   │   └── CreateTest.cshtml       # Create Exam wizard (4-step)
│   │   └── Shared/
│   │       ├── _LayoutStudent2026.cshtml # New 2026 layout
│   │       ├── _SidebarPro.cshtml       # Student sidebar (social buttons, Create Exam link)
│   │       ├── _HeaderPro.cshtml        # Student header (logo on mobile, dropdown fix)
│   │       └── _TeacherAside.cshtml     # Admin sidebar (Popup Slides link)
│   ├── Content/                 # CSS/Assets
│   │   └── fame-student-2026.css # 2026 Design System (2138 lines)
│   ├── Scripts/                 # JS
│   └── PopupSlidesHandler.ashx  # Landing page popup slides API (CRUD)
├── FAME.Tests/                  # Unit Tests
├── Database/                    # SQL Scripts
│   ├── create_ambassador_tables.sql
│   ├── create_exam_attempt_tables.sql
│   └── create_tbl_GuidelineVideo.sql
├── fame-mcq-portal/             # Next.js App
└── .agent/                      # Antigravity AI Kit
```

### Production Deployment (`C:\firstaidmadeeasy.com.pk`)
-   **Text-based files** (Views, CSS, JS) can be **hot-swapped** (copied directly).
-   **Compiled files** (C#) require building `FAME.Web` -> `bin/First Aid Made Easy.dll` -> Copy to Production `bin/`.
-   **IIS Reset** is required after bin updates or to clear cache.

---

## 4. 📝 Conventions & Standards

-   **Code Style**: PascalCase (C#), camelCase (JS).
-   **Images**: Stored in `~/Images/`. Use `Url.Content("~/Images/" + name)`.
-   **Razor**: Ensure `Microsoft.AspNet.Identity` namespace for `User.Identity`.
-   **Mobile-First**: Breakpoint at `992px` (LG). Min touch target 44px.

---

## 5. 🗺️ Roadmap & Active Plans

### A. Antigravity Kit (Active)
Goal: Enable AI specialist workflows.
-   [x] Setup `PROJECT_MEMORY.md` & `ARCHITECTURE.md`.
-   [x] Installed `ui-ux-pro-max` skill for deep design intelligence.
-   [x] Windows/C# compatibility for scripts.
-   [ ] Map workflows (create/refactor) to C#.
-   [ ] Audit `FAME.Web/BLL` for clean code.

### B. Course Repository Refactor (Planned)
Goal: Fix N+1 queries in `CourseRepository.cs`.
-   **Issues**: `GetOurCourses` and `GetList` trigger N+1.
-   **Plan**: Use EF projection (`.Select()`) and `Include()` to flatten queries.

### C. FAME 2026 UI Overhaul (Feb 2026)
-   **Status**: Active / In-Progress.
-   **New Layout**: `_LayoutStudent2026.cshtml`.
-   **Features**: USMLE-style Exam Engine, Lab Values Modal, "Tutor Mode".
-   [x] Student Dashboard redesign (`Views/Student/Index.cshtml`) — Feb 6
-   [x] My Courses page redesign (`Views/Student/MyCourses.cshtml`) — Feb 2
-   [x] Social media follow buttons in sidebar — Feb 6
-   [x] Create Exam wizard restoration (4-step flow) — Feb 6
-   [x] Student Profile page redesign (`Views/Profile/ProfileStudent.cshtml`) — Feb 7
-   [x] Change Password page redesign (`Views/Manage/StChangePassword.cshtml`) — Feb 7
-   [x] Mobile header: logo on mobile/tablet, text on desktop — Feb 7
-   [x] Profile dropdown: compact dropdown (removed blur overlay + bottom sheet) — Feb 7
-   [x] TakeCourse: lecture counts & duration per section/subsection — Feb 7
-   [x] TakeCourse: "Cinema Mode" renamed to "Focus Mode" — Feb 7

### D. Ambassador Program (Feb 6, 2026) ✅
Goal: Full referral/commission system for student ambassadors.
-   [x] Database tables created (11 tables: `tbl_Ambassador`, `tbl_Referral`, `tbl_CommissionRule`, etc.)
-   [x] Separate `AmbassadorDbContext` (Code-First, isolated from EDMX)
-   [x] BLL services: `AmbassadorRepository`, `ReferralRepository`, `CommissionService`, `PayoutService`, `AmbassadorAuditService`
-   [x] Admin panel: Dashboard, Manage Ambassadors, Applications, Commission Rules, Payouts
-   [x] Ambassador portal: Apply, Dashboard, Referrals, Earnings, Payout Requests
-   [x] All 5 admin pages verified HTTP 200
-   See [AMBASSADOR_AZ.md](AMBASSADOR_AZ.md) for complete feature documentation.

### E. MCQ Exam Engine Fixes (Feb 4-6, 2026) ✅
Goal: Fix critical exam bugs affecting student experience.
-   [x] Exam Result 0% score fix (Feb 4 — FormCollection parsing)
-   [x] Video player apostrophe fix (Feb 4 — data-attributes)
-   [x] MCQ option click bug — partial fix (Feb 5 — CSS z-index, JS escaping)
-   [x] MCQ option click bug — **comprehensive fix** (Feb 6 — jQuery event delegation, affects 51% of all options)
-   [x] "Review All Questions" button fix (Feb 6 — review mode with `?review=true`)

### F. Landing Page Popup Image Slider (Feb 7, 2026) ✅
Goal: Replace hardcoded SweetAlert welcome popup with admin-configurable responsive image slider.
-   [x] Database table `tbl_PopupSlides` created (Id, Title, ImagePath, LinkUrl, SortOrder, IsActive, CreatedDate, ModifiedDate)
-   [x] `PopupSlidesHandler.ashx` — ASHX handler with public + admin API endpoints (GET active, GET all, POST save, POST toggle, DELETE)
-   [x] Admin Popup Slides Manager (`Views/Notification/MessageIndex.cshtml`) — card grid, modal form, drag-to-upload, stats bar
-   [x] "Popup Slides" link in admin sidebar (`_TeacherAside.cshtml`) under Manage section
-   [x] Landing page `Areas/Landing/Views/Home/Index.cshtml` — responsive image slider with CSS transitions, nav arrows, dots, auto-advance, swipe, ESC close
-   [x] Old `Swal.fire()` popup completely removed
-   [x] `@@media` Razor escaping fixed (prevents 500 errors)
-   [x] Upload directory: `/Images/Slides/`
-   [x] All 9 automated tests passed, end-to-end insert/delete verified

### G. Guideline Videos Fixes (Feb 7, 2026) ✅
Goal: Fix broken video playback and missing thumbnails on Guideline Videos page.
-   [x] Added `youtube.com/live/` URL handler in `playVideo()` JS function — converts to `/embed/` format
-   [x] YouTube thumbnail extraction — dynamically shows `img.youtube.com/vi/{ID}/hqdefault.jpg` for all YouTube URL formats
-   [x] Fallback: non-YouTube videos keep generic placeholder
-   **Affected videos**: FCPS-1 Guidelines Webinar, NRE-1 Guidelines Webinar (both used `/live/` URLs)

### H. Student Profile & Security Pages Redesign (Feb 7, 2026) ✅
Goal: Migrate profile and password pages from old Metronic layout to 2026 design.
-   [x] `ProfileStudent.cshtml` — complete rewrite with `_LayoutStudent2026.cshtml`
    -   Teal hero banner with avatar + camera edit badge
    -   Settings/Password tab navigation
    -   4 card sections: Personal Info, Family & Sponsor, Professional Info, CNIC Documents
    -   All original fields preserved (CNIC, Job Designation, City, Institute, Year of Graduation, etc.)
    -   NRE-specific conditional sections (Roll Slip upload, Exam Type dropdown)
    -   CNIC upload zones show when images are missing (regardless of IsAccepted status)
    -   Avatar instant preview on file select
-   [x] `StChangePassword.cshtml` — complete rewrite with `_LayoutStudent2026.cshtml`
    -   Shield icon hero banner
    -   Password eye-toggle buttons
    -   Password requirements info box
    -   SweetAlert2 success notification

### I. Mobile Header Logo (Feb 7, 2026) ✅
Goal: Show FAME logo in header on mobile/tablet instead of page title text.
-   [x] `_HeaderPro.cshtml` — added `<img>` for logo + CSS responsive toggle
-   Mobile/Tablet (< 992px): Shows `FAME_Logo_Final.png`
-   Desktop (≥ 992px): Shows page title text (sidebar already has logo)

### J. Profile Dropdown Blur Fix (Feb 7, 2026) ✅
Goal: Fix mobile profile button creating full-screen blur overlay.
-   [x] Removed `backdrop-filter: blur(2px)` from overlay CSS
-   [x] Replaced full-screen bottom sheet with compact 260px dropdown
-   [x] Removed overlay activation and body scroll lock from JS
-   [x] Dropdown now appears directly under the profile button on all screen sizes

### K. TakeCourse Enhancements (Feb 7, 2026) ✅
Goal: Add lecture counts and duration info to course player sidebar.
-   [x] Course sidebar header: "Course Content" with total lectures + duration in `Xh Ym` format
-   [x] Section accordion headers: lecture count + duration per section
-   [x] Sub-section headers: lecture count + duration per sub-section
-   [x] "Cinema Mode" renamed to "Focus Mode"
-   Data sourced from existing model properties: `Lessons` (int), `DurationInMin` (decimal)

---

## 6. 📘 Technical Documentation

### MCQ Parsing Pipeline
**Source**: `DHA_MCQs.pdf` (Workspace Root).
**Method**: Python (`pymupdf`) -> JSON.
**Script**: `parse_pdf_to_json.py`.
**Output**: `mcq-data.json`.

**Logic**:
1.  **Extract**: Read PDF text stream.
2.  **Chunk**: Regex split by `\n+(?=[ \t]*(?:MCQ\s+\d+[:.]?|\d+\.))`.
3.  **Parse**: Extract Question, Options (A-E), Correct Answer, and Explanation.
    -   *Structured Explanations*: Parses "Definition", "Clinical Presentation", etc., into a JSON object for rich UI.

---

## 7. 📜 Session Log & History

### Feb 7-8, 2026 (Latest)

#### Landing Page Popup → Image Slider
-   **Problem**: Landing page had a hardcoded `Swal.fire()` popup — not manageable by admin, single static image.
-   **Solution**: Built full admin-managed image slider system:
    -   Created `tbl_PopupSlides` database table
    -   `PopupSlidesHandler.ashx` — ASHX handler (no controller DLL needed) with REST-style API
    -   Admin UI: card-based manager with drag-upload, toggle active/inactive, sortable
    -   Landing page: responsive modal slider with CSS transitions, arrow/dot navigation, auto-advance, swipe gestures, ESC close
    -   Fixed Razor `@@media` escaping issue (was causing 500 errors)
-   **Files Created**: `PopupSlidesHandler.ashx`, `tbl_PopupSlides` table
-   **Files Modified**: `Areas/Landing/Views/Home/Index.cshtml`, `Views/Notification/MessageIndex.cshtml`, `Views/Shared/_TeacherAside.cshtml`

#### Profile Dropdown Blur Fix (Mobile)
-   **Problem**: Tapping profile button on mobile blurred the entire screen with a bottom sheet overlay.
-   **Root Cause**: `backdrop-filter: blur(2px)` on overlay + full-screen bottom sheet pattern with body scroll lock.
-   **Fix**: Replaced bottom sheet with compact 260px dropdown positioned directly under button. Removed overlay activation and body scroll lock from JS.
-   **Files Modified**: `Views/Shared/_HeaderPro.cshtml` (both prod & source)

#### Guideline Videos — YouTube /live/ URL Fix
-   **Problem**: 2 guideline videos (NRE-1, FCPS-1) not playing — broken image icon in modal.
-   **Root Cause**: `playVideo()` JS had no handler for `youtube.com/live/` URL format. Raw URL set as iframe `src` → YouTube blocks with X-Frame-Options.
-   **Fix**: Added `else if (videoPath.includes('youtube.com/live/'))` branch to extract video ID and build `/embed/` URL.
-   **Files Modified**: `Views/Student/GuidelineVideos.cshtml`

#### Guideline Videos — YouTube Thumbnails
-   **Problem**: All videos showed generic dark placeholder thumbnails instead of actual video previews.
-   **Fix**: Razor code extracts YouTube video ID from URL and builds `img.youtube.com/vi/{ID}/hqdefault.jpg` thumbnail. Added teal play button overlay. Handles all 3 YouTube URL formats: `watch?v=`, `youtu.be/`, `youtube.com/live/`.
-   **Files Modified**: `Views/Student/GuidelineVideos.cshtml`

#### Student Profile Page — Complete Redesign
-   **Problem**: "My Profile" linked to `ProfileStudent.cshtml` which used old `_LayoutNew.cshtml` (Metronic dark theme).
-   **Solution**: Complete rewrite using `_LayoutStudent2026.cshtml`:
    -   Teal hero banner with circular avatar + camera edit badge
    -   Settings / Password tab navigation (links to ChangePassword page)
    -   4 card sections: Personal Info, Family & Sponsor, Professional Info, CNIC Documents
    -   CNIC upload fix: upload zone now shows whenever images are missing (previously locked when `IsAccepted = true`)
    -   Avatar instant preview on file select
    -   All original model fields preserved
-   **Files Modified**: `Views/Profile/ProfileStudent.cshtml` (both prod & source, backup at `.bak`)

#### Change Password Page — Complete Redesign
-   **Problem**: `/Manage/ChangePassword` used old `_LayoutNew.cshtml` for students.
-   **Solution**: Rewrite of `StChangePassword.cshtml` with `_LayoutStudent2026.cshtml`:
    -   Shield icon hero banner with teal gradient
    -   Tab navigation matching profile page (Settings / Password)
    -   Eye toggle buttons for show/hide password
    -   Password requirements info box
    -   SweetAlert2 success notification
-   **Files Modified**: `Views/Manage/StChangePassword.cshtml` (both prod & source, backup at `.bak`)

#### Cinema Mode → Focus Mode Rename
-   **Change**: Renamed "Cinema Mode" to "Focus Mode" in `TakeCourse.cshtml` (button label + CSS comment).
-   **Files Modified**: `Views/Student/TakeCourse.cshtml`

#### TakeCourse — Lecture Counts & Duration
-   **Problem**: No lecture count or duration displayed in course sidebar.
-   **Solution**: Added lecture counts and duration to three levels:
    -   Course sidebar header: "Course Content — ▶ X Lectures ⏱ Xh Ym"
    -   Each section accordion: lecture count + duration below section name
    -   Each sub-section header: lecture count + duration right-aligned
    -   Duration converted from minutes (`DurationInMin`) to `Xh Ym` format
-   **Data Source**: Existing model properties — `CourseVM.Lessons`, `CourseVM.DurationInMin`, `SectionVM.Lessons`, `SectionVM.DurationInMin`, `SectionSubVM.Lessons`, `SectionSubVM.DurationInMin`
-   **Files Modified**: `Views/Student/TakeCourse.cshtml`

#### Mobile Header Logo
-   **Problem**: Mobile/tablet header showed page title text; user wanted logo.
-   **Solution**: Added `<img>` with `fame-header-logo` class pointing to `~/Images/FAME_Logo_Final.png`. CSS responsive toggle:
    -   Mobile/Tablet (< 992px): Shows logo (32px height), hides text
    -   Desktop (≥ 992px): Shows text, hides logo (sidebar already has logo)
-   **Files Modified**: `Views/Shared/_HeaderPro.cshtml`

#### Summary of All Files Modified (Feb 7-8)
| File | Change |
|------|--------|
| `Views/Shared/_HeaderPro.cshtml` | Blur fix + compact dropdown + mobile logo |
| `Views/Student/GuidelineVideos.cshtml` | /live/ URL fix + YouTube thumbnails |
| `Views/Profile/ProfileStudent.cshtml` | Complete rewrite (2026 design) |
| `Views/Manage/StChangePassword.cshtml` | Complete rewrite (2026 design) |
| `Views/Student/TakeCourse.cshtml` | Focus Mode rename + lecture counts/duration |
| `PopupSlidesHandler.ashx` | NEW — popup slides API handler |
| `Views/Notification/MessageIndex.cshtml` | Rewritten — Popup Slides admin UI |
| `Areas/Landing/Views/Home/Index.cshtml` | Popup → image slider |
| `Views/Shared/_TeacherAside.cshtml` | Added Popup Slides admin link |

---

### Feb 6, 2026

#### Ambassador Program — 4-Layer Fix
All 5 admin pages were returning errors. Resolved through 4 successive debugging layers:
1.  **EF Entity Mapping**: `tbl_Ambassador is not part of model` → Created separate `AmbassadorDbContext` (Code-First) to avoid EDMX conflicts.
2.  **EDMX Entity Leakage**: Navigation properties in Ambassador entities referenced EDMX types (`AspNetUsers`) → Created lightweight `AmbassadorUser`/`AmbassadorRole` POCOs, used `.Ignore()` for cross-context nav props.
3.  **Missing View Models**: `AdminApplicationsVM does not exist` → Created 5 admin VM classes in `AmbassadorViewModels.cs`.
4.  **Razor Syntax**: `Unexpected 'if' keyword after '@'` → Removed `@` prefix from `if` statements inside C# code blocks in Ambassadors.cshtml and Payouts.cshtml.
-   **Result**: All 5 admin pages verified returning HTTP 200.
-   **Files Created**: `DAL/AmbassadorDbContext.cs`
-   **Files Modified**: `DAL/AmbassadorEntities.cs`, `DAL/FAMEEntities.Partial.cs`, `Models/AmbassadorViewModels.cs`, `BLL/AmbassadorRepository.cs` (25× context swap), `BLL/ReferralRepository.cs`, `BLL/CommissionService.cs`, `BLL/PayoutService.cs`, `BLL/AmbassadorAuditService.cs`, `Areas/Ambassador/Controllers/AdminController.cs`, `Areas/Ambassador/Views/Admin/*.cshtml`, `FAME.Web.csproj`

#### MCQ "Review All Questions" — Fix
-   **Problem**: Clicking "Review All Questions" on ExamResult page called `CreateTest` API (treats result ID as paper ID → error).
-   **Fix**: Added `?review=true` URL parameter. `TakeExam.cshtml` detects review mode and calls `GetQuestionToSolve` instead. Forces tutor mode, read-only UI, "REVIEW MODE" banner, "Back to Results" button.
-   **Files Modified**: `Views/Student/ExamResult.cshtml`, `Views/Student/TakeExam.cshtml`

#### MCQ Unclickable Options — Comprehensive Fix (MAJOR)
-   **Problem**: Many mock tests had randomly unclickable MCQ options.
-   **Root Cause Analysis** (database query):
    -   77,708 total options in database
    -   39,966 contain newlines (51.4%!) — breaks inline `onclick` HTML attribute
    -   387 contain apostrophes — breaks JS string interpolation
    -   29 contain double quotes — breaks HTML attributes
-   **Additional CSS Bug**: `.strike-action` had `pointer-events: auto !important` creating invisible 24px hitbox blocking clicks.
-   **Fix**: Replaced ALL inline `onclick`/`oncontextmenu` handlers with jQuery event delegation using `data-oidx` attributes. Option text set via `.text()` (safe for all special characters). Strike button `pointer-events: none` by default.
-   **Impact**: Fixed 51%+ of all exam questions that were potentially affected.
-   **Validation**: 8/8 checks passed (newlines, apostrophes, quotes, Unicode, long text, HTML entities, event delegation, strike buttons).
-   **Files Modified**: `Views/Student/TakeExam.cshtml` (both source and production)

### Feb 5, 2026
-   **MCQ Fixes (Partial)**: Initial fix for unclickable options in `TakeExam.cshtml`.
    -   Fixed CSS z-index/overlap of "Strike-through" (Red X).
    -   Fixed JS string escaping for apostrophes/quotes in questions.
    -   *(Note: Feb 6 replaced this with comprehensive event delegation fix.)*
-   **Consolidation**: Merged project documentation into this memory file.

### Feb 4, 2026
-   **Exam Result 0% Fix**: Fixed backend logic in `StudentController` & `Result_BLL` to correctly parse `FormCollection` data when calculating scores.
-   **Video Player**: Fixed crash on video titles with apostrophes (e.g., "Crohn's Disease") by using `data-attributes` instead of inline JS.
-   **Course Visibility**: Fixed `sp_MyCourses` SQL procedure to show courses with non-NULL types (restored JCAT/NRE courses).

### Feb 2-3, 2026 (UI Modernization)
-   **Design Shift**: Replaced "Legacy Colorful" with "Clinical Teal" (`#0F766E`).
-   **Mobile Patch**: Fixed "zoomed out" viewport bug.
-   **Backend**: Fixed DI crash in `MockTestsController` (Autofac).


---

## 📚 Technical Documentation
- [MCQ Parsing Method](MCQ_Parsing_Method.md) — PDF-to-JSON pipeline for DHA MCQs
- [Ambassador Portal (A-Z)](AMBASSADOR_AZ.md) — Complete feature docs: apply, referrals, commissions, payouts, admin management
- [Architecture Map](ARCHITECTURE.md) — System layers, EF contexts, deployment model

---
---
## ⚡ Antigravity Kit Mandate (Full-Stack)
- **Rule**: For ANY frontend or backend task, the **Antigravity Kit** workflow MUST be used automatically.
- **Workflow**: 
  1.  **Announce**: `🤖 Applying knowledge of @[agent]...`
  2.  **Analyze**: Follow `AGENT_FLOW.md` for classification and strategy.
  3.  **UI/UX**: Always use `ui-ux-pro-max` for design-heavy tasks.
  4.  **Verify**: Always run `checklist.py` before delivery.
- **Goal**: Ensure premium, high-contrast, non-generic designs and robust backend architecture without manual requests.

---
*End of Project Memory.*
