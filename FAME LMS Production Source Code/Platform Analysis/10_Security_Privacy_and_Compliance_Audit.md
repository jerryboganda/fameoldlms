# 10 — Security, Privacy, and Compliance Audit

---

> **DISCLAIMER**: This is a non-invasive, observational security review conducted through static code and configuration analysis. No penetration testing, exploitation, or unauthorized access was performed. Findings are based on what is visible in source code, production configuration files, and public website observation.

---

## 1. Security Observations

### 1.1 Critical Security Findings

#### SEC-001: All Credentials Stored in Plaintext in Web.config

**Severity**: Critical  
**Evidence**: `Web.config` (tracked in source repository)

| Credential Type | Status | Risk |
|----------------|--------|------|
| SQL Server SA password | Plaintext in connection string | Full database compromise if repo exposed |
| JWT signing key | Plaintext in app settings | Token forgery, impersonation of any user |
| Gmail SMTP app password | Plaintext in SMTP config | Email account compromise |
| Zoom SDK key and secret | Plaintext in app settings | Unauthorized Zoom integration use |
| Firebase FCM token | Plaintext in app settings | Unauthorized push notification sending |
| Google Gemini API key | Plaintext in app settings | API quota abuse |
| Multiple SMS credentials | Plaintext (some commented out) | Unauthorized SMS sending |
| RapidAPI keys (2) | Plaintext in app settings | API quota abuse |
| Old/legacy credentials | Commented out but still in file | Historical credential exposure |

**Additional Risk**: These credentials also appear in `PROJECT_SSOT.md` — a tracked documentation file that explicitly lists all credentials with values.

**Impact**: If the repository is exposed (e.g., accidental GitHub push, backup leak, developer workstation compromise), **all platform services are immediately compromizable**.

**Recommendation**: 
1. Immediately move all secrets to environment variables or a secure vault (e.g., Azure Key Vault)
2. Rotate ALL exposed credentials
3. Remove credential values from `PROJECT_SSOT.md`
4. Add `Web.config` transformation for secrets

---

#### SEC-002: SA Database User Access from Web Application

**Severity**: Critical  
**Evidence**: `Web.config` connection string uses `User ID=sa`

The web application connects to SQL Server as `sa` — the built-in superuser account with unrestricted database privileges. Any SQL injection vulnerability (even in a stored procedure) would grant an attacker:
- Full read/write to all databases on the server
- Ability to create/drop databases
- Ability to execute system-level procedures (e.g., `xp_cmdshell`)
- Ability to create new logins/users

**Recommendation**: Create a dedicated database user with minimum required permissions (read/write to `FAME_DB` tables only, execute on specific stored procedures).

---

#### SEC-003: CSRF Protection Disabled on Critical Forms

**Severity**: Critical  
**Evidence**:

| Form | CSRF Token Status | File |
|------|-------------------|------|
| Registration | **Commented out**: `@*@Html.AntiForgeryToken()*@` | `Account/Register.cshtml` |
| Login | **Missing** — no anti-forgery token | `Account/Login.cshtml` |
| Contact Us | **Missing** — `kt_contact_form` template | `Home/ContactUs.cshtml` |
| Logout | ✅ **Present** — only verified location | `Shared/_HeaderPro.cshtml` |

**Impact**: Without CSRF protection, an attacker can craft a malicious page that submits forms on behalf of authenticated users (e.g., forced registration, credential changes).

---

#### SEC-004: Production Debug Mode Enabled

**Severity**: Critical  
**Evidence**: `Web.config`: `<compilation debug="true" targetFramework="4.7.2">`

**Impact**:
- Detailed error messages including stack traces returned to clients
- Compilation caching disabled (performance overhead)
- ASP.NET debug information available
- Potential information leakage about server configuration

---

#### SEC-005: Error Page Exposes Full Stack Traces

**Severity**: Critical  
**Evidence**: `Views/Shared/Error.cshtml` renders:
- `@Model.ControllerName` — reveals internal routing
- `@Model.ActionName` — reveals internal action names
- `@Model.Exception` — reveals full exception stack trace

Combined with the missing `customErrors` configuration, this means **any unhandled exception in production reveals internal application details to the end user**.

**Additional note**: The error page text contains a misspelling: "An Error Occord While Processing The Request"

---

#### SEC-006: JazzCash Merchant Credentials in Client-Side Code

**Severity**: Critical  
**Evidence**: `Views/Enrollment/JazzCash.cshtml` contains:
- `pp_MerchantID: "Merc0003"` — merchant identifier
- `pp_Password: "0123456789"` — merchant password
- `pp_SecureHash` — payment hash visible in JavaScript

These are visible to anyone viewing the page source.

---

### 1.2 High Security Findings

#### SEC-007: No Security Headers Configured

**Severity**: High  
**Evidence**: No security headers found in `Web.config` `<system.webServer>` section.

| Missing Header | Risk |
|----------------|------|
| `X-Frame-Options` | Platform can be iframed — clickjacking vulnerability |
| `Content-Security-Policy` | No restriction on script/style sources — XSS amplification |
| `Strict-Transport-Security` | No HSTS — HTTPS not enforced browser-side |
| `X-Content-Type-Options` | MIME sniffing possible |
| `X-XSS-Protection` | Legacy XSS filter not enabled |
| `Referrer-Policy` | Referrer leakage to third parties |
| `Permissions-Policy` | No restriction on browser feature access (camera, mic, geolocation) |

---

#### SEC-008: No HTTPS Enforcement in Configuration

**Severity**: High  
**Evidence**:
- No `<rewrite>` rules for HTTP→HTTPS redirect in `Web.config`
- No `requireSSL` attribute on cookies or forms authentication
- No `[RequireHttps]` global filter detectable (compiled in DLL)
- No HSTS header configured

**Note**: SSL may be enforced at the reverse proxy/load balancer level, but this cannot be verified from application code. See `18_Open_Questions.md`.

---

#### SEC-009: CORS Allow-All Policy

**Severity**: High  
**Evidence**: `PROJECT_MEMORY.md` documents CORS configuration in `Startup.cs` with no origin restriction.

**Impact**: Any external website can make authenticated cross-origin API requests to the FAME backend if the user is logged in.

---

#### SEC-010: Excessive Session Timeout

**Severity**: High  
**Evidence**: `Web.config`: `<sessionState timeout="6000">` — 6000 minutes = ~4.2 days

**Impact**: 
- User sessions persist for over 4 days without activity
- Shared/public computer users remain logged in long after leaving
- Session hijacking window is dramatically extended
- More concurrent sessions held in server memory

**Recommendation**: Reduce to 30–60 minutes with "Remember Me" option using a persistent authentication cookie.

---

### 1.3 Medium Security Findings

| ID | Finding | Evidence | Severity |
|----|---------|----------|----------|
| SEC-011 | **Google Client ID exposed in Register page** | Meta tag in `Register.cshtml` | Medium — public by design but aids targeted OAuth attacks |
| SEC-012 | **`AllowInspect = true`** in settings | Web.config | Medium — browser dev tools access may be intentionally unrestricted |
| SEC-013 | **Max JSON deserializer members = Int32.Max** | Web.config `maxJsonDeserializerMembers="2147483647"` | Medium — potential DoS via crafted JSON with billions of members |
| SEC-014 | **1 GB upload limit** | `maxRequestLength="1048576"` | Medium — enables large file upload-based DoS |
| SEC-015 | **No rate limiting** visible | No rate limiting configuration observed | Medium — brute force login, registration spam |
| SEC-016 | **Device limit = 2** with admin approval | `AllowedDevices` setting | Low — device management exists but is admin-dependent |

---

## 2. Privacy Concerns

### 2.1 Data Collection Points

| Data Type | Collection Point | Sensitivity | Concern |
|-----------|-----------------|-------------|---------|
| Full Name | Registration Step 1 | Medium | ✅ Expected |
| Father Name | Registration Step 1 | Medium | ⚠️ Unusual — not standard for online education |
| Sponsor Name | Registration Step 1 | Medium | ⚠️ Unusual |
| Email | Registration Step 1 | Medium | ✅ Expected |
| Mobile Number | Registration Step 3 | Medium | ✅ Expected for verification |
| Country & City | Registration Step 3 | Low | ✅ Expected |
| Occupation/Designation | Registration Step 3 | Low | ✅ Relevant for medical platform |
| Year of MBBS | Registration Step 3 | Low | ✅ Relevant |
| Institute | Registration Step 3 | Low | ✅ Relevant |
| **CNIC (front + back)** | Registration Step 5 | **High** | ⚠️ **Significant PII** — national ID card with photo, address, DOB, biometric data. Collected BEFORE service delivery. |
| Profile Photo | Registration Step 6 | Medium | ⚠️ Some concern — collected at registration, not after |
| Payment Details | JazzCash form | High | ✅ Expected for payment |

### 2.2 PII Exposure in Admin Panel

**File**: `Views/Admin/StudentDetail.cshtml` (674 lines)

The admin student detail page displays:
- Email address
- Mobile number
- CNIC number
- Father name
- City, Institute

**Finding**: No data masking is visible in the admin edit form for sensitive fields (CNIC, mobile). Any admin/teacher/assistant role can potentially view full student PII.

---

## 3. Policy Review

### 3.1 Privacy Policy

**File**: `Areas/Landing/Views/Home/PrivacyPolicy.cshtml`

| Aspect | Assessment |
|--------|-----------|
| **Exists** | ✅ Yes — accessible at `/Landing/Home/PrivacyPolicy` |
| **Covers data collection** | ✅ Yes — lists types of information collected |
| **Covers purpose** | ✅ Yes — explains why data is collected |
| **Covers sharing** | ✅ Yes — describes sharing practices |
| **Covers security** | ❌ **Concerning** — contains the statement: "Information, as it stands, is completely susceptible to loss and alteration with no safeguards in place" |
| **Covers user rights** | ⚠️ Partial — mentions rights but lacks specific procedures |
| **Encoding issues** | ❌ — Mojibake characters (`site?s` instead of `site's`) |
| **Duplicate content** | ❌ — "Website Improvement" paragraph appears twice |
| **Last updated date** | ⚠️ Not observed |
| **GDPR/data regulation compliance** | ❌ — No mention of GDPR, PDPA, or jurisdiction-specific data protection |
| **Cookie policy** | ❌ — No separate cookie policy or consent mechanism |
| **Data retention** | ❌ — No data retention period specified |
| **CNIC justification** | ❌ — No explanation for why national ID is required for online education |

**Critical Issue**: The statement "completely susceptible to loss and alteration with no safeguards" is either a gross mischaracterization used in a legal boilerplate OR a genuine admission of poor data security practices. Either way, it **severely damages trust** and potentially creates legal liability.

### 3.2 Terms and Conditions

**File**: `Areas/Landing/Views/Home/TermsAndConditions.cshtml`

| Aspect | Assessment |
|--------|-----------|
| **Exists** | ✅ Yes — accessible at `/Landing/Home/TermsAndConditions` |
| **Eligibility** | ✅ 18+ requirement stated |
| **Account responsibilities** | ✅ Covered |
| **Prohibited activities** | ✅ Listed |
| **IP ownership** | ✅ Claimed |
| **User submissions** | ✅ Covered |
| **Disclaimers** | ✅ Present |
| **Governing law** | ⚠️ "Rawalpindi, Pakistan" — but footer says "Lahore, Punjab, Pakistan" — **jurisdiction inconsistency** |
| **Content quality** | ⚠️ Some grammatical awkwardness (likely AI-generated/translated) |
| **Refund policy** | ❌ No clear refund policy found in T&C or separately |
| **Subscription terms** | ⚠️ Not clear how subscription expiry, renewal, and access termination are handled |

### 3.3 Jurisdiction Inconsistency

| Location | Stated Address |
|----------|----------------|
| Terms & Conditions | **Rawalpindi**, Pakistan |
| Website Footer | **Mall Road, Lahore**, Punjab, Pakistan |
| WhatsApp Number | +92 318 0049742 (area code suggests Punjab) |

**Finding**: The T&C and footer disagree on platform jurisdiction/location. This is a compliance concern that could complicate legal proceedings.

---

## 4. Data Handling Concerns

| Concern | Detail | Severity |
|---------|--------|----------|
| **CNIC images stored in `/Images/`** | User-uploaded CNIC documents appear to be stored in the general `/Images/` directory alongside course thumbnails and profile photos | High |
| **No encryption at rest** evidence | No configuration for encrypted storage observed — CNIC images likely stored as plain files | High |
| **No data retention policy** | No visible automated cleanup of old CNIC images, expired user data, or inactive accounts | Medium |
| **SA database access** | If DB is compromised, all user data (including CNIC, personal details) is accessible | Critical |
| **Log files may contain PII** | Serilog at Verbose level may log user data in requests/responses | Medium |
| **Backup retention unclear** | Only 4 deployment backups visible — database backup retention not observable | Medium |

---

## 5. Compliance Gaps

| Requirement | Status | Gap |
|-------------|--------|-----|
| **HTTPS enforcement** | Not Verified | No enforcement in application config |
| **Data encryption at rest** | Not Verified | No evidence of encrypted storage |
| **Data encryption in transit** | Partial | HTTPS likely active but not enforced; some SMTP uses `ssl=false` (commented out) |
| **Cookie consent** | ❌ Missing | No cookie banner or consent mechanism |
| **Data export/deletion rights** | ❌ Missing | No self-service data export or account deletion |
| **User deletion procedure** | ✅ Exists | `sp_DeleteUser_v2.sql` handles cascade deletion — 40+ FK constraints handled |
| **Access control** | ✅ Exists | Role-based access (6 roles), ASP.NET Identity |
| **Audit logging** | ✅ Partial | Serilog logging active; Ambassador module has dedicated audit log |
| **Incident response** | ❌ Missing | No visible incident response plan or procedure |
| **Security testing** | ❌ Missing | No evidence of regular security scanning or testing |
| **GDPR compliance** | ❌ Not addressed | No GDPR provisions despite serving students from multiple countries |
| **Pakistan PDPA** | ❌ Not addressed | Pakistan's Personal Data Protection Act not referenced |
| **PCI DSS** | ⚠️ Risk | JazzCash integration with client-side credentials may violate PCI requirements |

---

## 6. Security Posture Summary

### Rating by Category

| Category | Score | Key Issue |
|----------|:-----:|-----------|
| Authentication | 3/5 | ASP.NET Identity is solid; CSRF and session management are weak |
| Authorization | 3/5 (Assumed) | Role-based system exists but controller-level rules are not inspectable |
| Data Protection | 1.5/5 | Credentials exposed, no encryption evidence, CNIC stored as plain files |
| Network Security | 2/5 | No security headers, no HTTPS enforcement, CORS allow-all |
| Privacy Compliance | 1.5/5 | Concerning privacy policy, no cookie consent, no GDPR, no data retention |
| Configuration Security | 1/5 | Debug mode, SA access, excessive limits, all secrets in plaintext |
| Application Security | 2/5 | CSRF disabled on key forms, stack traces exposed, DoS vectors open |

**Overall Security Score: 1.5 / 5**

### Security Maturity Level: **Early**

The platform uses a solid authentication framework (ASP.NET Identity with role-based access) but has **critical configuration and operational security failures** that negate the framework's protections. The exposure of all credentials in tracked repository files represents the single highest-risk finding in this entire audit.

---

## 7. Priority Security Actions

| Priority | Action | Effort |
|----------|--------|--------|
| 1 | Set `debug="false"` in production Web.config | 1 minute |
| 2 | Uncomment CSRF token on registration form | 1 minute |
| 3 | Add `<customErrors mode="RemoteOnly">` to Web.config | 5 minutes |
| 4 | Fix `Error.cshtml` to not display `@Model.Exception` | 10 minutes |
| 5 | Add security headers to `<system.webServer>` | 30 minutes |
| 6 | Rotate all exposed credentials immediately | 2–4 hours |
| 7 | Move all secrets from Web.config to environment variables | 4–8 hours |
| 8 | Create dedicated DB user to replace SA | 2–4 hours |
| 9 | Reduce session timeout to 60 minutes | 5 minutes |
| 10 | Restrict CORS to known origins | 30 minutes |
| 11 | Reduce upload size limits | 5 minutes |
| 12 | Implement HTTPS enforcement | 1–2 hours |
| 13 | Add cookie consent mechanism | 4–8 hours |
| 14 | Rewrite Privacy Policy | 1–2 days |

---

*This security review is non-invasive and based on observable evidence from source code and configuration files. A comprehensive security assessment including penetration testing, dependency vulnerability scanning, and server-level configuration review is strongly recommended.*
