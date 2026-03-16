# 08 — Technical and Functional Audit

---

## 1. Critical Bugs and Security Issues

| ID | Category | Issue | Evidence | Severity |
|----|----------|-------|----------|----------|
| T-001 | Security | **CSRF token commented out on registration form** | `Register.cshtml` contains `@*@Html.AntiForgeryToken()*@` — token is wrapped in Razor comment syntax | **Critical** |
| T-002 | Security | **No CSRF token on login form** | `Login.cshtml` — no `@Html.AntiForgeryToken()` found in the login form | **Critical** |
| T-003 | Security | **Error page exposes full stack traces** | `Error.cshtml` renders `@Model.Exception`, `@Model.ControllerName`, `@Model.ActionName` — full exception details visible to end users | **Critical** |
| T-004 | Security | **`debug="true"` in production Web.config** | `<compilation debug="true" targetFramework="4.7.2">` — debug mode enabled in production environment | **Critical** |
| T-005 | Security | **JazzCash merchant credentials in client-side JavaScript** | `JazzCash.cshtml` contains hardcoded `pp_MerchantID: "Merc0003"` and `pp_Password: "0123456789"` in browser-accessible code | **Critical** |
| T-006 | Security | **All secrets in plaintext in Web.config** | Database SA password, JWT signing key, SMTP password, Zoom SDK keys, Gemini API key, SMS credentials — all in cleartext | **Critical** |
| T-007 | Security | **SA database user** | `Web.config` connection string uses `sa` (SQL Server superuser) — maximum database privilege from web application | **Critical** |
| T-008 | Security | **No security headers configured** | Missing: `X-Frame-Options`, `X-Content-Type-Options`, `Content-Security-Policy`, `Strict-Transport-Security`, `X-XSS-Protection` | **High** |

---

## 2. Functional Issues

| ID | Category | Issue | Evidence | Severity |
|----|----------|-------|----------|----------|
| T-009 | Content | **Pricing page is template placeholder** | `Home/Pricing.cshtml` shows USD pricing ($9/$19/$29), links to `learnly-signup.html` — broken links from Learnly HTML template | **High** |
| T-010 | Support | **Placeholder WhatsApp number in Support Overview** | `Home/SupportOverView.cshtml` links to `923000000000` — not a real phone number | **High** |
| T-011 | Content | **Multiple spelling errors on public pages** | "Obstetrucs", "Opthamology", "Anotomy", "Desigantion", "verfiy", "Occord", "INSTAGARM", "Plateform", "clearify", "according toe" | **High** |
| T-012 | Content | **Encoding artifacts (mojibake)** | Landing page contains "Â" artifacts from mis-encoded non-breaking spaces: "andÂ Afghanistan", "By TenÂ Teachers", "Â Lectures" | **Medium** |
| T-013 | Content | **Duplicate paragraphs** | Privacy Policy has duplicated "Website Improvement" section; About Us has duplicated Dr. Atif quote | **Medium** |
| T-014 | Branding | **Template branding in login page** | `Login.cshtml` meta tag: `og:site_name = "Keenthemes | First Aid Made Easy"` — third-party template name exposed | **Medium** |
| T-015 | Content | **Copyright year outdated** | Website footer displays "Copyright 2022" instead of 2026 | **Medium** |
| T-016 | Content | **Social counters show zero** | Landing page displays "0K+" for Facebook, Instagram, YouTube, Students, "0%+" for Satisfaction, "0+" for Years — either animation failure or no data | **Medium** |
| T-017 | Navigation | **Hidden social login buttons** | `Login.cshtml` uses `display:none !important` on `ExternalLoginPartial` — Google/Facebook OAuth configured but intentionally hidden | **Low** |

---

## 3. Broken Flows

| ID | Flow | Issue | Impact | Severity |
|----|------|-------|--------|----------|
| T-018 | Pricing → Purchase | Pricing page links to `learnly-signup.html` — non-existent page | Users clicking pricing CTAs reach 404 or similar error | **High** |
| T-019 | Landing → Support | Support Overview WhatsApp links to placeholder number | Support seekers cannot reach live help via primary quick channel | **High** |
| T-020 | Registration | CNIC upload at Step 5 before any service — may fail on mobile with large images | Registration dropout at file upload step | **Medium** |
| T-021 | Coupon Redemption | Coupon views exist (`Coupon/Index.cshtml`, `Coupon/List.cshtml`) but sidebar link is commented out | Feature exists but is inaccessible via navigation — dead feature | **Low** |
| T-022 | Self-Service Renewal | No visible renewal flow for expired subscriptions | Expired users must contact admin manually — retention risk | **High** |

---

## 4. Validation Issues

| ID | Area | Issue | Evidence | Severity |
|----|------|-------|----------|----------|
| T-023 | Registration | **Custom div-based validation** instead of HTML5/MVC DataAnnotations client-side | Uses `#fieldNameVal` pattern with manual show/hide — less accessible than native validation | **Medium** |
| T-024 | Registration | **CSRF protection disabled** | Token commented out — form accepts submissions without CSRF validation | **Critical** |
| T-025 | Contact Form | **No visible CSRF protection** | `ContactUs.cshtml` form has `id="kt_contact_form"` (Metronic template) — no `@Html.AntiForgeryToken()` visible | **Medium** |
| T-026 | File Upload | **1 GB max upload** | `maxRequestLength="1048576"` (1 GB) — excessive for profile photos and CNIC images, potential memory abuse | **Medium** |
| T-027 | JSON | **Max JSON members set to Int32.Max** | `maxJsonDeserializerMembers="2147483647"` and `maxJsonLength="2147483647"` — potential DoS vector via crafted JSON payloads | **Medium** |

---

## 5. Reliability Concerns

| ID | Area | Issue | Evidence | Severity |
|----|------|-------|----------|----------|
| T-028 | Configuration | **`debug="true"` causes runtime overhead** | Debug mode disables compilation optimizations, increases memory usage, and enables detailed error pages | **High** |
| T-029 | Architecture | **CDN dependency in fullscreen exam** | `TakeExam.cshtml` loads Bootstrap from CDN — exam page breaks if CDN is unavailable during timed test | **Medium** |
| T-030 | Architecture | **Empty BLL/Interfaces folder** | No abstractions for business logic — all services are concrete classes, making unit testing and DI substitution difficult | **Medium** |
| T-031 | Architecture | **Dead code: BLL/JzTimer folder** | Empty folder — appears to be an abandoned feature | **Low** |
| T-032 | Ops | **15+ stale DLL backups in production /bin/** | 15 `.dll.bak-*` files from Feb–Mar 2026 sitting in production `bin/` — unnecessary disk usage, potential confusion | **Low** |
| T-033 | SEO | **sitemap.xml in wrong directory** | Located at `Content/sitemap.xml` instead of root — search engines look at `/sitemap.xml` by default | **Medium** |
| T-034 | SEO | **Missing robots.txt** | No `robots.txt` found in production root — search engines have no crawl guidance, may index admin/API paths | **Medium** |
| T-035 | Architecture | **Multiple TakeCourse views** | Both `TakeCourse.cshtml` and `TakeCourseNew.cshtml` exist — unclear which is active, potential for serving wrong version | **Low** |
| T-036 | Configuration | **CORS allow-all policy** | `Startup.cs` configures CORS with no origin restriction — any domain can make authenticated API requests | **High** |
| T-037 | Configuration | **Session timeout: 6000 minutes** | ~4.2 days — excessively long for a web application; session hijacking risk | **High** |
| T-038 | Architecture | **Dual layout coexistence** | `_LayoutNew.cshtml` and `_LayoutStudent2026.cshtml` both active — routing determines which layout a student sees | **Medium** |
| T-039 | Error Handling | **No `customErrors` in production Web.config** | Missing `<customErrors mode="RemoteOnly">` means ASP.NET default error pages (detailed errors) shown to remote users | **High** |
| T-040 | Logging | **Serilog verbose logging in production** | 4 log levels active (Debug, Error, Information, Verbose) with daily rotation and overflow files — excessive IO for production | **Medium** |

---

## 6. Architecture Observations

### 6.1 Positive Patterns

| Pattern | Evidence | Assessment |
|---------|----------|-----------|
| **Structured BLL/DAL/Models separation** | Distinct folders for business logic, data access, and models | ✅ Clean architecture |
| **Autofac DI** | `Autofac 6.0.0` referenced — dependency injection in place | ✅ Good practice |
| **Feature flags** | Ambassador module uses `tbl_AmbassadorFeatureFlag` for phased rollout | ✅ Mature deployment pattern |
| **SignalR for real-time** | Chat and notifications use SignalR for push communication | ✅ Appropriate technology |
| **Backup discipline** | DLL backups created before every deployment | ✅ Good operational hygiene |
| **Multiple EF contexts** | 5 contexts for different modules — appropriate isolation | ✅ Module separation |
| **T4 templates** | `Model1.tt`, `Model1.Context.tt` for code generation from EDMX | ✅ Reduces manual coding |
| **Stored procedures** | 51 SPs — complex queries in DB rather than EF LINQ | ✅ Performance optimization |
| **Background services** | Email sender, notification sender, campaign scheduler — run on app start | ✅ Async processing |

### 6.2 Architectural Concerns

| Concern | Impact | Priority |
|---------|--------|----------|
| **No interface abstractions** | `BLL/Interfaces/` is empty — services lack contracts for testing/mocking | Medium |
| **EDMX + Code-First hybrid** | 2 paradigms in same application — migration management complexity | Low |
| **Controllers compiled in DLL** | Cannot inspect `[Authorize]` attributes without decompilation — authorization rules opaque | Medium |
| **Single deployment artifact** | All 45 controllers compiled into one DLL — any change requires full rebuild | Medium |
| **No CI/CD pipeline** observed | Deployment appears manual (PowerShell scripts + `iisreset`) | Medium |
| **Mixed frontend technologies** | jQuery + Razor (main) + Next.js (MCQ portal) + Metronic + custom CSS | Medium |

---

## 7. Technical Debt Inventory

| # | Item | Type | Estimated Effort |
|---|------|------|-----------------|
| 1 | Remove `debug="true"` from Web.config | Configuration | 5 minutes |
| 2 | Uncomment CSRF token on registration form | Security fix | 5 minutes |
| 3 | Add CSRF token to login form | Security fix | 10 minutes |
| 4 | Add `customErrors` to Web.config | Configuration | 5 minutes |
| 5 | Fix Error.cshtml to not show stack traces | Template fix | 30 minutes |
| 6 | Add security headers to Web.config | Configuration | 30 minutes |
| 7 | Move secrets to environment variables | Architecture | 4-8 hours |
| 8 | Create dedicated DB user (replace SA) | Database | 2-4 hours |
| 9 | Fix all spelling errors | Content | 2-4 hours |
| 10 | Fix encoding artifacts | Content | 1-2 hours |
| 11 | Remove/update broken Pricing page | Content | 1-2 hours |
| 12 | Fix placeholder WhatsApp number | Content | 5 minutes |
| 13 | Move sitemap.xml to root | SEO | 10 minutes |
| 14 | Create robots.txt | SEO | 30 minutes |
| 15 | Clean stale DLL backups | Operations | 30 minutes |
| 16 | Reduce session timeout | Security | 5 minutes |
| 17 | Restrict CORS policy | Security | 30 minutes |
| 18 | Add BLL interfaces | Architecture | 2-4 days |
| 19 | Unify layout systems | UX/Architecture | 1-2 weeks |
| 20 | Reduce JSON deserialization limits | Security | 5 minutes |

**Estimated Total Quick Wins (items 1-6, 12-16, 20)**: < 2 hours  
**Estimated Total Medium Effort (items 7-11, 17)**: 1-2 days  
**Estimated Total Strategic (items 18-19)**: 1-3 weeks

---

## 8. Technical Quality Ratings

| Dimension | Score | Key Factor |
|-----------|:-----:|-----------|
| Security Posture | 1.5/5 | Multiple critical vulnerabilities; all secrets exposed |
| Code Architecture | 3/5 | Clean BLL/DAL separation but no interfaces; DI in place |
| Configuration Quality | 2/5 | Debug mode on, excessive limits, no error handling config |
| Error Handling | 1.5/5 | Stack traces exposed, no custom errors, misspelled error page |
| Frontend Architecture | 3/5 | Modern CSS system but mixed with legacy; jQuery-dependent |
| Deployment Process | 2.5/5 | Backup discipline is good; manual process without CI/CD |
| SEO Readiness | 1.5/5 | No robots.txt, misplaced sitemap, no meta tags strategy |
| Logging & Monitoring | 3/5 | Serilog active but verbose production logging is excessive |

**Overall Technical Score: 2.5 / 5**

The platform has sound architectural foundations (clean layer separation, DI, background services, feature flags) but is undermined by critical security misconfigurations and accumulated technical debt. The majority of critical and high-severity issues are **low-effort fixes** (configuration changes, content corrections) that can be resolved rapidly.

---

*Technical findings are based on static code analysis and configuration review. Runtime behavior, actual database query performance, and server-level security require authenticated access and are documented in `18_Open_Questions_and_Not_Verified_Items.md`.*
