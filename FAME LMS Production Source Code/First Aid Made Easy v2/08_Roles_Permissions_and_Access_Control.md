# 08 — Roles, Permissions, and Access Control

*8 roles, policy-based authorization, workflow redesign eliminating manual bottlenecks.*

---

## 1. Role Hierarchy

```
SuperAdmin                           ← Full system access, 1-2 people
  ├── Admin                          ← Content + user + payment management
  │   ├── AcademicManager            ← Course + exam content management
  │   ├── ContentEditor              ← Blog, FAQ, CMS pages
  │   ├── SupportAgent               ← Ticket resolution, student assistance
  │   └── FinanceStaff               ← Payment reports, refunds, coupons
  ├── Instructor                     ← Content creation for assigned courses
  └── Student                        ← Learning, exams, subscriptions
       └── Ambassador (add-on role)  ← Referral management (combined with Student)
```

### V1 → V2 Role Mapping

| V1 Role | V2 Role | Changes |
|---------|---------|---------|
| Admin | SuperAdmin + Admin | Split into two tiers for separation of privilege |
| Teacher | Instructor | Renamed, scoped to assigned courses only |
| Assistant | ContentEditor | Renamed, focused on content management |
| UniTeacher | *Removed* | Merged into Instructor with course assignment |
| SuppAgent | SupportAgent | Same function, enhanced tooling |
| Student | Student | Enhanced with self-service capabilities |
| *N/A* | AcademicManager | New: manages exam tracks, question bank, exam configs |
| *N/A* | FinanceStaff | New: payment analytics, refunds, coupon management |

---

## 2. Permission Matrix

### 2.1 Permissions per Module

| Permission | SuperAdmin | Admin | Academic Mgr | Content Ed | Support | Finance | Instructor | Student |
|-----------|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| **Identity** | | | | | | | | |
| View all users | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ |
| Create/edit users | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Assign roles | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Impersonate user | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Edit own profile | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Catalog** | | | | | | | | |
| View all courses | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ✅* | ✅* |
| Create/edit courses | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ✅* | ❌ |
| Publish/unpublish | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Delete courses | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Assessment** | | | | | | | | |
| Manage question bank | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ✅* | ❌ |
| Configure exams | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Import questions | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Take exams | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |
| View own results | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |
| View all results | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ |
| **Enrollment** | | | | | | | | |
| View all enrollments | ✅ | ✅ | ❌ | ❌ | ✅ | ✅ | ❌ | ❌ |
| Manual enrollment | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Override expiry | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| View own enrollment | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Payment** | | | | | | | | |
| View all transactions | ✅ | ✅ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| Process refunds | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| Manage packages | ✅ | ✅ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| Manage coupons | ✅ | ✅ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| Purchase package | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |
| View own invoices | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Communication** | | | | | | | | |
| Create announcements | ✅ | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ |
| View support tickets | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ |
| Respond to tickets | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ |
| Create ticket | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |
| Send email campaigns | ✅ | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Content (CMS)** | | | | | | | | |
| Edit FAQ/About/Terms | ✅ | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ |
| Manage blog posts | ✅ | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Certificate** | | | | | | | | |
| Manage templates | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| View own certificates | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Ambassador** | | | | | | | | |
| Approve ambassadors | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| View all referrals | ✅ | ✅ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| Manage payouts | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| View own referrals | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | 🟡 |
| **Analytics** | | | | | | | | |
| View admin dashboard | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ |
| View audit log | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Export reports | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ |
| **System** | | | | | | | | |
| Feature flags | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| System config | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Hangfire dashboard | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |

`✅*` = Scoped to assigned courses/content only. `🟡` = Only for users with Ambassador add-on role.

---

## 3. Policy-Based Authorization Implementation

### 3.1 Policy Definitions

```csharp
// SharedKernel/Constants/Policies.cs
public static class Policies
{
    // Module-level policies
    public const string CanManageUsers = "CanManageUsers";
    public const string CanViewUsers = "CanViewUsers";
    public const string CanManageCourses = "CanManageCourses";
    public const string CanPublishCourses = "CanPublishCourses";
    public const string CanManageQuestions = "CanManageQuestions";
    public const string CanConfigureExams = "CanConfigureExams";
    public const string CanManageEnrollments = "CanManageEnrollments";
    public const string CanViewTransactions = "CanViewTransactions";
    public const string CanProcessRefunds = "CanProcessRefunds";
    public const string CanManagePackages = "CanManagePackages";
    public const string CanManageCoupons = "CanManageCoupons";
    public const string CanManageContent = "CanManageContent";
    public const string CanManageTickets = "CanManageTickets";
    public const string CanManageAmbassadors = "CanManageAmbassadors";
    public const string CanViewAnalytics = "CanViewAnalytics";
    public const string CanViewAuditLog = "CanViewAuditLog";
    public const string CanManageSystem = "CanManageSystem";
    
    // Student-specific policies
    public const string StudentAccess = "StudentAccess";
    public const string CanTakeExams = "CanTakeExams";
    public const string CanPurchase = "CanPurchase";
    
    // Resource-based (checked at handler level)
    public const string CanAccessCourse = "CanAccessCourse";   // Requires active enrollment
    public const string CanAccessExam = "CanAccessExam";       // Requires active enrollment
}
```

### 3.2 Policy Registration

```csharp
// Program.cs
builder.Services.AddAuthorization(options =>
{
    // SuperAdmin can do everything
    options.AddPolicy(Policies.CanManageSystem, policy =>
        policy.RequireRole(Roles.SuperAdmin));

    // User management: SuperAdmin + Admin
    options.AddPolicy(Policies.CanManageUsers, policy =>
        policy.RequireRole(Roles.SuperAdmin, Roles.Admin));

    // Course management: SuperAdmin + Admin + AcademicManager + Instructor (scoped)
    options.AddPolicy(Policies.CanManageCourses, policy =>
        policy.RequireRole(Roles.SuperAdmin, Roles.Admin, Roles.AcademicManager, Roles.Instructor));

    // Publish courses: SuperAdmin + Admin + AcademicManager
    options.AddPolicy(Policies.CanPublishCourses, policy =>
        policy.RequireRole(Roles.SuperAdmin, Roles.Admin, Roles.AcademicManager));

    // Question bank: SuperAdmin + Admin + AcademicManager + Instructor (scoped)
    options.AddPolicy(Policies.CanManageQuestions, policy =>
        policy.RequireRole(Roles.SuperAdmin, Roles.Admin, Roles.AcademicManager, Roles.Instructor));

    // Student access
    options.AddPolicy(Policies.StudentAccess, policy =>
        policy.RequireRole(Roles.Student));

    options.AddPolicy(Policies.CanTakeExams, policy =>
        policy.RequireRole(Roles.Student));

    options.AddPolicy(Policies.CanPurchase, policy =>
        policy.RequireRole(Roles.Student));

    // Finance
    options.AddPolicy(Policies.CanViewTransactions, policy =>
        policy.RequireRole(Roles.SuperAdmin, Roles.Admin, Roles.FinanceStaff));

    options.AddPolicy(Policies.CanProcessRefunds, policy =>
        policy.RequireRole(Roles.SuperAdmin, Roles.FinanceStaff));

    // Support
    options.AddPolicy(Policies.CanManageTickets, policy =>
        policy.RequireRole(Roles.SuperAdmin, Roles.Admin, Roles.SupportAgent));

    // Content
    options.AddPolicy(Policies.CanManageContent, policy =>
        policy.RequireRole(Roles.SuperAdmin, Roles.Admin, Roles.ContentEditor));

    // Analytics
    options.AddPolicy(Policies.CanViewAnalytics, policy =>
        policy.RequireRole(Roles.SuperAdmin, Roles.Admin, Roles.AcademicManager, Roles.FinanceStaff));
});
```

### 3.3 Resource-Based Authorization

```csharp
// For course access: student must have active enrollment
public class CourseAccessHandler : AuthorizationHandler<CourseAccessRequirement, Guid>
{
    private readonly FameDbContext _db;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CourseAccessRequirement requirement,
        Guid courseId)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return;

        // Admin/Staff bypass
        if (context.User.IsInRole(Roles.SuperAdmin) || 
            context.User.IsInRole(Roles.Admin))
        {
            context.Succeed(requirement);
            return;
        }

        // Check active enrollment with access to this course
        var hasAccess = await _db.AccessGrants
            .AnyAsync(ag => 
                ag.Enrollment.UserId == userId &&
                ag.CourseId == courseId &&
                ag.Enrollment.Status == EnrollmentStatus.Active &&
                ag.Enrollment.ExpiresAt > DateTimeOffset.UtcNow);

        // Also allow if course has free preview
        if (!hasAccess)
        {
            var isFreePreview = await _db.Courses
                .AnyAsync(c => c.Id == courseId && c.HasFreePreview);
            hasAccess = isFreePreview;
        }

        if (hasAccess)
            context.Succeed(requirement);
    }
}
```

---

## 4. Workflow Redesign: Eliminating Manual Bottlenecks

### 4.1 V1 Manual Workflows → V2 Automated Workflows

| Workflow | V1 (Manual) | V2 (Automated) | Admin Role |
|----------|------------|----------------|------------|
| **Student Registration** | Admin reviews & approves each registration | Self-service: email verification → immediate access | None required |
| **Package Purchase** | Student contacts admin → bank transfer → admin verifies → admin activates | Student browses → selects → pays online → webhook → auto-enrollment | None required |
| **Subscription Renewal** | Student contacts admin → repeat payment flow | Auto-renewal or one-click renewal → auto-extension | None required |
| **Course Enrollment** | Admin manually creates enrollment record | Triggered automatically by payment webhook | None required |
| **Support Ticket** | WhatsApp message → informal tracking | In-app ticket system → assigned to SupportAgent → tracked → resolved | SupportAgent responds |
| **Certificate Issuance** | Admin manually generates | Auto-generated on course completion or exam pass | Template management only |
| **Ambassador Approval** | Hidden, manual process | Public application → Admin reviews → approve/reject via dashboard | Admin reviews |
| **Content Publishing** | Direct file upload to server | Draft → Review → Publish workflow. AcademicManager approves. | AcademicManager approves |

### 4.2 Admin Workload Comparison

| Task | V1 Daily Admin Time | V2 Daily Admin Time |
|------|:------------------:|:------------------:|
| Processing registrations | 30-60 min | 0 min |
| Processing payments/enrollments | 60-120 min | 0 min |
| Handling subscription renewals | 30-60 min | 0 min |
| Support (WhatsApp) | 60+ min | 30 min (ticket system) |
| Content management | 30 min | 30 min (better tools) |
| Reporting | 30 min (manual) | 5 min (dashboards) |
| **Total** | **4-6 hours** | **~1 hour** |

---

## 5. Role Onboarding

### 5.1 Default Permissions on Account Creation

| Scenario | Default Role | Auto-Provisions |
|----------|------------|-----------------|
| Public registration | Student | Email verified, no subscription |
| Admin creates staff member | Specified role(s) | Admin selects role, sends invite email |
| Ambassador application approved | Student + Ambassador | Ambassador dashboard access, referral tools |
| Instructor invited | Instructor | Assigned to specific courses |

### 5.2 Role Assignment Rules

- Only **SuperAdmin** can assign the SuperAdmin role
- **Admin** can assign all roles except SuperAdmin
- A user can have **multiple roles** (e.g., Student + Ambassador)
- Role changes are logged in the audit log with before/after values
- Removing a role immediately revokes associated permissions (no session caching)

---

## 6. Authentication Flow Details

### 6.1 Registration Flow

```
1. POST /api/auth/register
   Body: { email, password, displayName }
   
2. Server validates:
   - Email format and uniqueness
   - Password complexity (min 10, upper+lower+digit+special)
   - Display name not empty
   
3. Create ApplicationUser with:
   - EmailConfirmed = false
   - Role = Student
   
4. Generate 6-digit verification code → store in Redis (15 min TTL)
   
5. Send verification email via Hangfire background job
   
6. Response: 201 Created { message: "Verification email sent" }
   
7. POST /api/auth/verify-email
   Body: { email, code }
   
8. Verify code match → Set EmailConfirmed = true → Issue JWT tokens
   
9. Response: 200 OK + Set httpOnly cookies (access_token, refresh_token)
```

### 6.2 Login Flow

```
1. POST /api/auth/login
   Body: { email, password }
   
2. Rate limiting check: 5 attempts per IP per 15 minutes
   
3. Find user → Verify password → Check:
   - EmailConfirmed == true
   - IsActive == true
   - LockoutEnd == null or < now
   
4. Log LoginAudit (success or failure)
   
5. On success: Generate JWT pair → Set cookies → Response 200
   On failure: Increment lockout counter → Response 401
```

### 6.3 Token Refresh Flow

```
1. GET /api/auth/refresh
   Cookie: refresh_token=<token>
   
2. Validate refresh token (exists, not expired, not revoked)
   
3. Revoke old refresh token (one-time use)
   
4. Generate new JWT pair → Set new cookies → Response 200
```

---

## 7. Security Constraints

| Constraint | Implementation |
|-----------|---------------|
| **Principle of Least Privilege** | Every endpoint requires explicit authorization policy |
| **Role-scoped data access** | Instructors see only assigned courses; Students see only own data |
| **Impersonation audit** | SuperAdmin impersonation logged with original and impersonated user IDs |
| **Session revocation** | Token revocation list in Redis, checked on every authenticated request |
| **Concurrent session limit** | Max 3 active refresh tokens per user (oldest revoked on 4th login) |
| **Password change invalidates sessions** | All refresh tokens revoked on password change |
| **Role change takes immediate effect** | JWT claims refreshed on next token rotation |

---

*This document defines the complete authorization model for FAME V2. All endpoints must specify an authorization policy. Resource-based checks are required for any data that belongs to a specific user or is scoped to a specific entity.*
