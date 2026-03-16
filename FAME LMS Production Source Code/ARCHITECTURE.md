# ARCHITECTURE.md - FAME LMS System Map

## 🏰 Overview
FAME LMS is a hybrid medical learning platform. It combines a legacy ASP.NET MVC monolithic backend for management and deep student features with a modern Next.js frontend for high-speed MCQ practice.

## 🗺️ Logical Layers

### 1. Presentation Layer (MVC)
- **Path**: `FAME.Web/Views`
- **Engine**: Razor (.cshtml)
- **Theme**: Metronic 8 + Custom FAME 2026 UI.
- **Key Files**: `_LayoutStudent2026.cshtml` (Master layout), `TakeExam.cshtml` (Legacy exam logic).

### 2. Service/API Layer
- **MVC Controllers**: `FAME.Web/Controllers/` (Handles primary page loads and form submissions).
- **Web API**: `FAME.Web/ControllersApi/` (JSON endpoints for JS-heavy components).
- **Next.js Portal**: `fame-mcq-portal/` (Consumes JSON data, likely for standalone usage or high-performance exams).

### 3. Business Logic Layer (BLL)
- **Path**: `FAME.Web/BLL/`
- **Pattern**: Repository Pattern using Interfaces.
- **Dependencies**: Autofac for DI.
- **Key Logic**: `Result_BLL.cs` (Exam grading), `CourseRepository.cs` (Data fetching).

### 4. Data Access Layer (DAL)
- **Path**: `FAME.Web/DAL/`
- **ORM**: Entity Framework 6.4.4
- **Database**: SQL Server (`FAME_DB`)
- **Dual-Context Architecture**:
  - `FAMEEntities` (Database-First / EDMX) — Core LMS tables (courses, students, exams, subscriptions)
  - `AmbassadorDbContext` (Code-First) — Ambassador module tables (11 tables), isolated to avoid EDMX mapping conflicts. Uses lightweight `AmbassadorUser`/`AmbassadorRole` POCOs with `.Ignore()` for cross-context navigation properties.
- **Connection Strings**: `DefaultConnection` (plain SqlClient), `FAMEEntities` (EntityClient/EDMX), `FAME_CodeFirst` (SqlClient + MARS)
- **Logic**: Stored procedures heavily used for complex queries (e.g., `sp_MyCourses`).

### 5. Ambassador Module (Feb 6, 2026)
- **Area**: `FAME.Web/Areas/Ambassador/`
- **Context**: `AmbassadorDbContext` (Code-First, isolated from EDMX)
- **Controllers**: `AdminController` (admin panel), `AmbassadorController` (ambassador portal)
- **BLL Services**: `AmbassadorRepository`, `ReferralRepository`, `CommissionService`, `PayoutService`, `AmbassadorAuditService`
- **Database Tables**: `tbl_Ambassador`, `tbl_AmbassadorApplication`, `tbl_Referral`, `tbl_ReferralConversion`, `tbl_CommissionRule`, `tbl_Commission`, `tbl_PayoutRequest`, `tbl_PayoutBatch`, `tbl_AmbassadorTier`, `tbl_AmbassadorAuditLog`, `tbl_AmbassadorFeatureFlag`
- **Docs**: See [AMBASSADOR_AZ.md](AMBASSADOR_AZ.md)

### 6. MCQ Exam Engine
- **Key Files**: `Views/Student/TakeExam.cshtml`, `Views/Student/ExamResult.cshtml`
- **API Endpoints**: `CreateTest` (start exam), `GetQuestionToSolve` (load questions/review)
- **Event Handling**: jQuery event delegation with `data-oidx` attributes (no inline onclick — safe for all special characters in option text)
- **Modes**: Normal, Tutor Mode (immediate feedback), Review Mode (`?review=true`, read-only)
- **Known Data**: 77,708 options in DB; 51% contain newlines, 0.5% apostrophes, 0.04% double quotes

---

## 🛠️ Infrastructure & Deployment
- **Platform**: Windows / IIS.
- **Binary**: `First Aid Made Easy.dll` (Core logic).
- **Sync**: Hot-swap views/CSS/JS from `Source` to `Production` root.

## 🤖 AI Agent & Skill Logic
- **Path**: `.agent/`
- **Skills**: `clean-code`, `systematic-debugging`, `app-builder`, `ui-ux-pro-max`.
- **Scripts**: `checklist.py`, `security_scan.py`, `.agent/skills/ui-ux-pro-max/scripts/search.py`.

---
*Updated Feb 6, 2026.*
