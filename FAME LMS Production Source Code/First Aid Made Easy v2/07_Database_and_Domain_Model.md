# 07 — Database and Domain Model (PostgreSQL 17 + EF Core 9)

*~40 entities across 9 bounded contexts. Entity definitions, relationships, migration strategy, and data conventions.*

---

## 1. Bounded Contexts Overview

```
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│   Identity   │  │   Catalog    │  │  Assessment  │
│              │  │              │  │              │
│ User         │  │ ExamTrack    │  │ Question     │
│ Role         │  │ Course       │  │ QuestionOpt  │
│ UserRole     │  │ Module       │  │ Exam         │
│ RefreshToken │  │ Chapter      │  │ ExamQuestion │
│ UserProfile  │  │ Lecture      │  │ ExamAttempt  │
│ LoginAudit   │  │ Resource     │  │ AttemptAnswer│
│              │  │ ChapterMarker│  │ ExamResult   │
└──────┬───────┘  └──────┬───────┘  └──────┬───────┘
       │                 │                 │
       │ userId          │ courseId         │ userId
       ▼                 ▼                 ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│  Enrollment  │  │   Payment    │  │Communication │
│              │  │              │  │              │
│ Subscription │  │ Package      │  │ Notification │
│ PackageIncl  │  │ PackageCourse│  │ EmailLog     │
│ Enrollment   │  │ Transaction  │  │ Announcement │
│ AccessGrant  │  │ Coupon       │  │ SupportTicket│
│              │  │ CouponUsage  │  │ TicketMessage│
│              │  │ Invoice      │  │              │
└──────────────┘  └──────────────┘  └──────────────┘

┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│  Certificate │  │  Ambassador  │  │  Analytics   │
│              │  │              │  │              │
│ CertTemplate │  │ Ambassador   │  │ DailyMetric  │
│ Certificate  │  │ ReferralLink │  │ UserActivity │
│              │  │ Referral     │  │ ContentStat  │
│              │  │ Commission   │  │ AuditLog     │
│              │  │ Payout       │  │ FeatureFlag  │
└──────────────┘  └──────────────┘  └──────────────┘
```

---

## 2. Base Entity Types

```csharp
// SharedKernel/Domain/Entity.cs
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.CreateVersion7(); // v7 = time-ordered
}

// SharedKernel/Domain/AuditableEntity.cs
public abstract class AuditableEntity : Entity
{
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }              // UserId
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

// SharedKernel/Domain/SoftDeletableEntity.cs
public abstract class SoftDeletableEntity : AuditableEntity
{
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
```

---

## 3. Entity Definitions by Context

### 3.1 Identity Context

```csharp
// Extends ASP.NET Core Identity
public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? TimeZone { get; set; } = "Asia/Karachi";
    public string? PreferredExamTrack { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public UserProfile? Profile { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public ICollection<ExamAttempt> ExamAttempts { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}

public class UserProfile : Entity
{
    public string UserId { get; set; } = string.Empty;          // FK to ApplicationUser
    public string? Institution { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Cnic { get; set; }                           // Optional, encrypted at rest
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }
    public DateOnly? DateOfBirth { get; set; }
}

public class RefreshToken : Entity
{
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;           // Hashed
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedByIp { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
}

public class LoginAudit : Entity
{
    public string UserId { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
}
```

### 3.2 Catalog Context

```csharp
public class ExamTrack : SoftDeletableEntity
{
    public string Name { get; set; } = string.Empty;            // "FCPS-1", "USMLE Step 1"
    public string Slug { get; set; } = string.Empty;            // "fcps-1", "usmle-step-1"
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Course> Courses { get; set; } = [];
}

public class Course : SoftDeletableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public Guid ExamTrackId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; }
    public bool HasFreePreview { get; set; }
    public int TotalDurationMinutes { get; set; }               // Computed on save
    public int LectureCount { get; set; }                       // Computed on save

    // Navigation
    public ExamTrack ExamTrack { get; set; } = null!;
    public ICollection<Module> Modules { get; set; } = [];
}

public class Module : SoftDeletableEntity                       // "Section" in v1
{
    public string Title { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public int DisplayOrder { get; set; }

    public Course Course { get; set; } = null!;
    public ICollection<Chapter> Chapters { get; set; } = [];
}

public class Chapter : SoftDeletableEntity                      // "Sub-section" in v1
{
    public string Title { get; set; } = string.Empty;
    public Guid ModuleId { get; set; }
    public int DisplayOrder { get; set; }

    public Module Module { get; set; } = null!;
    public ICollection<Lecture> Lectures { get; set; } = [];
}

public class Lecture : SoftDeletableEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ChapterId { get; set; }
    public int DisplayOrder { get; set; }
    public string? YoutubeVideoId { get; set; }                // YouTube video ID
    public int DurationSeconds { get; set; }
    public bool IsFreePreview { get; set; }
    public string? NotesMarkdown { get; set; }                 // Lecture notes

    public Chapter Chapter { get; set; } = null!;
    public ICollection<ChapterMarker> Markers { get; set; } = [];
    public ICollection<Resource> Resources { get; set; } = [];
}

public class ChapterMarker : Entity
{
    public Guid LectureId { get; set; }
    public int TimestampSeconds { get; set; }
    public string Label { get; set; } = string.Empty;
}

public class Resource : SoftDeletableEntity
{
    public string Title { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;        // S3 URL
    public string FileType { get; set; } = string.Empty;       // "pdf", "doc", etc.
    public long FileSizeBytes { get; set; }
    public Guid? LectureId { get; set; }                       // Optional: attached to specific lecture
    public Guid? CourseId { get; set; }                         // Optional: course-level resource
}
```

### 3.3 Assessment Context

```csharp
public class Question : SoftDeletableEntity
{
    public string Text { get; set; } = string.Empty;           // HTML/Markdown
    public string? Explanation { get; set; }                    // Shown after answering
    public Guid? ExamTrackId { get; set; }                     // Optional: track-specific
    public string? TopicTag { get; set; }                      // "Cardiology", "Pathology"
    public string? SubTopicTag { get; set; }
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
    public string? SourceReference { get; set; }               // Textbook, page number

    public ICollection<QuestionOption> Options { get; set; } = [];
}

public class QuestionOption : Entity
{
    public Guid QuestionId { get; set; }
    public string Key { get; set; } = string.Empty;            // "A", "B", "C", "D"
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }

    public Question Question { get; set; } = null!;
}

public enum DifficultyLevel { Easy = 1, Medium = 2, Hard = 3 }

public class Exam : SoftDeletableEntity
{
    public string Title { get; set; } = string.Empty;
    public Guid? ExamTrackId { get; set; }
    public int TimeLimitMinutes { get; set; }                  // 0 = untimed
    public int PassingPercentage { get; set; } = 60;
    public bool IsPublished { get; set; }
    public bool ShuffleQuestions { get; set; } = true;
    public bool ShuffleOptions { get; set; } = false;

    public ICollection<ExamQuestion> ExamQuestions { get; set; } = [];
}

public class ExamQuestion : Entity
{
    public Guid ExamId { get; set; }
    public Guid QuestionId { get; set; }
    public int DisplayOrder { get; set; }

    public Exam Exam { get; set; } = null!;
    public Question Question { get; set; } = null!;
}

public class ExamAttempt : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public Guid ExamId { get; set; }
    public ExamMode Mode { get; set; }                         // Timed, Practice, Custom
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public bool IsSubmitted { get; set; }

    public Exam Exam { get; set; } = null!;
    public ExamResult? Result { get; set; }
    public ICollection<AttemptAnswer> Answers { get; set; } = [];
}

public enum ExamMode { Timed = 1, Practice = 2, Custom = 3 }

public class AttemptAnswer : Entity
{
    public Guid AttemptId { get; set; }
    public Guid QuestionId { get; set; }
    public string? SelectedOptionKey { get; set; }             // null = unanswered
    public bool IsCorrect { get; set; }
    public int TimeSpentSeconds { get; set; }                  // Time on this question
    public bool WasFlagged { get; set; }
}

public class ExamResult : Entity
{
    public Guid AttemptId { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public int IncorrectAnswers { get; set; }
    public int UnansweredCount { get; set; }
    public decimal ScorePercentage { get; set; }
    public bool Passed { get; set; }
    public int TotalTimeSeconds { get; set; }
    public string TopicBreakdownJson { get; set; } = "{}";     // jsonb: per-topic scores

    public ExamAttempt Attempt { get; set; } = null!;
}
```

### 3.4 Enrollment Context

```csharp
public class Enrollment : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public Guid PackageId { get; set; }
    public Guid? TransactionId { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
    public bool AutoRenew { get; set; }

    public Package Package { get; set; } = null!;
    public ICollection<AccessGrant> AccessGrants { get; set; } = [];
}

public enum EnrollmentStatus { Active = 1, Expired = 2, Cancelled = 3, Suspended = 4 }

public class AccessGrant : Entity
{
    public Guid EnrollmentId { get; set; }
    public Guid CourseId { get; set; }
    public DateTimeOffset GrantedAt { get; set; }
}
```

### 3.5 Payment Context

```csharp
public class Package : SoftDeletableEntity
{
    public string Name { get; set; } = string.Empty;           // "PLAB-1 Complete"
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal PricePkr { get; set; }
    public decimal? PriceUsd { get; set; }
    public int DurationDays { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    public string? Badge { get; set; }                         // "Most Popular", "Best Value"
    public string? FeaturesJson { get; set; }                  // jsonb: feature list for pricing page

    public ICollection<PackageCourse> PackageCourses { get; set; } = [];
}

public class PackageCourse : Entity
{
    public Guid PackageId { get; set; }
    public Guid CourseId { get; set; }
}

public class Transaction : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public Guid PackageId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "PKR";
    public PaymentGateway Gateway { get; set; }
    public string? GatewayTransactionId { get; set; }
    public string? GatewayResponse { get; set; }               // jsonb: full gateway response
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    public DateTimeOffset? PaidAt { get; set; }
    public string? FailureReason { get; set; }
    public Guid? CouponId { get; set; }
    public decimal DiscountAmount { get; set; }
}

public enum PaymentGateway { JazzCash = 1, Easypaisa = 2, Stripe = 3, Manual = 4 }
public enum TransactionStatus { Pending = 1, Completed = 2, Failed = 3, Refunded = 4, Cancelled = 5 }

public class Coupon : SoftDeletableEntity
{
    public string Code { get; set; } = string.Empty;           // Unique, uppercase
    public CouponType Type { get; set; }
    public decimal Value { get; set; }                          // Percentage or fixed amount
    public string Currency { get; set; } = "PKR";
    public int? MaxUsages { get; set; }                        // null = unlimited
    public int UsageCount { get; set; }
    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
    public Guid? RestrictToPackageId { get; set; }             // null = all packages
    public Guid? RestrictToTrackId { get; set; }               // null = all tracks
    public bool IsActive { get; set; } = true;
    public Guid? AmbassadorId { get; set; }                    // null = not ambassador-linked
}

public enum CouponType { Percentage = 1, FixedAmount = 2, FreeTrial = 3 }

public class Invoice : AuditableEntity
{
    public Guid TransactionId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;  // Auto-generated: FAME-2026-00001
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = "PKR";
    public string? PdfUrl { get; set; }                        // S3 URL to generated PDF
}
```

### 3.6 Communication Context

```csharp
public class Notification : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}

public enum NotificationType { Info = 1, Warning = 2, Success = 3, ExpiryReminder = 4, NewContent = 5 }

public class SupportTicket : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Normal;
    public string? AssignedToUserId { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }

    public ICollection<TicketMessage> Messages { get; set; } = [];
}

public enum TicketStatus { Open = 1, InProgress = 2, WaitingOnCustomer = 3, Resolved = 4, Closed = 5 }
public enum TicketPriority { Low = 1, Normal = 2, High = 3, Urgent = 4 }

public class TicketMessage : AuditableEntity
{
    public Guid TicketId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsStaffReply { get; set; }
    public string? AttachmentUrl { get; set; }
}

public class EmailLog : AuditableEntity
{
    public string ToEmail { get; set; } = string.Empty;
    public string? ToUserId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public EmailStatus Status { get; set; }
    public string? Error { get; set; }
    public DateTimeOffset? SentAt { get; set; }
}

public enum EmailStatus { Queued = 1, Sent = 2, Failed = 3, Bounced = 4 }

public class Announcement : SoftDeletableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public AnnouncementType Type { get; set; }                 // Banner, Modal, Toast
    public string? TargetRoles { get; set; }                   // Comma-separated, null = all
    public DateTimeOffset? StartsAt { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
    public bool IsDismissible { get; set; } = true;
}

public enum AnnouncementType { Banner = 1, Modal = 2, Toast = 3 }
```

### 3.7 Certificate Context

```csharp
public class CertificateTemplate : SoftDeletableEntity
{
    public string Name { get; set; } = string.Empty;
    public string HtmlTemplate { get; set; } = string.Empty;   // Handlebars/Liquid template
    public CertificateType Type { get; set; }
    public bool IsDefault { get; set; }
}

public enum CertificateType { CourseCompletion = 1, ExamPass = 2, Achievement = 3 }

public class Certificate : AuditableEntity
{
    public string CertificateNumber { get; set; } = string.Empty; // Unique: FAME-CERT-2026-XXXXX
    public string UserId { get; set; } = string.Empty;
    public Guid TemplateId { get; set; }
    public Guid? CourseId { get; set; }
    public Guid? ExamAttemptId { get; set; }
    public string? PdfUrl { get; set; }
    public string? PngUrl { get; set; }
    public string MetadataJson { get; set; } = "{}";           // Student name, score, date, etc.
}
```

### 3.8 Ambassador Context

```csharp
public class Ambassador : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public AmbassadorStatus Status { get; set; } = AmbassadorStatus.Pending;
    public string? SocialMediaUrl { get; set; }
    public string? PromotionStrategy { get; set; }
    public AmbassadorTier Tier { get; set; } = AmbassadorTier.Bronze;
    public decimal CommissionRate { get; set; } = 10.0m;        // Percentage
    public decimal TotalEarnings { get; set; }
    public decimal PendingPayout { get; set; }

    public ICollection<ReferralLink> ReferralLinks { get; set; } = [];
    public ICollection<Referral> Referrals { get; set; } = [];
}

public enum AmbassadorStatus { Pending = 1, Approved = 2, Suspended = 3, Rejected = 4 }
public enum AmbassadorTier { Bronze = 1, Silver = 2, Gold = 3, Platinum = 4 }

public class ReferralLink : AuditableEntity
{
    public Guid AmbassadorId { get; set; }
    public string Code { get; set; } = string.Empty;           // Unique referral code
    public string? TargetUrl { get; set; }                     // Optional: specific course/package
    public int ClickCount { get; set; }
}

public class Referral : AuditableEntity
{
    public Guid AmbassadorId { get; set; }
    public Guid ReferralLinkId { get; set; }
    public string ReferredUserId { get; set; } = string.Empty;
    public Guid? TransactionId { get; set; }
    public ReferralStatus Status { get; set; } = ReferralStatus.Registered;
}

public enum ReferralStatus { Clicked = 1, Registered = 2, Converted = 3 }

public class Commission : AuditableEntity
{
    public Guid AmbassadorId { get; set; }
    public Guid ReferralId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "PKR";
    public CommissionStatus Status { get; set; } = CommissionStatus.Pending;
}

public enum CommissionStatus { Pending = 1, Approved = 2, Paid = 3, Cancelled = 4 }
```

### 3.9 Analytics Context

```csharp
public class AuditLog : Entity
{
    public string? UserId { get; set; }
    public string Action { get; set; } = string.Empty;        // "CreateCourse", "UpdateUser"
    public string EntityType { get; set; } = string.Empty;     // "Course", "User"
    public string? EntityId { get; set; }
    public string? OldValuesJson { get; set; }                 // jsonb: before values
    public string? NewValuesJson { get; set; }                 // jsonb: after values
    public string? IpAddress { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

public class UserActivity : Entity
{
    public string UserId { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;   // "LectureWatched", "ExamCompleted"
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public int DurationSeconds { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? MetadataJson { get; set; }                  // jsonb: activity-specific data
}

public class FeatureFlag : AuditableEntity
{
    public string Key { get; set; } = string.Empty;            // "self_service_checkout"
    public bool IsEnabled { get; set; }
    public string? Description { get; set; }
    public int? RolloutPercentage { get; set; }                // null = all or none
    public string? TargetRoles { get; set; }                   // Comma-separated
}
```

---

## 4. Entity Relationship Diagram (Key Relationships)

```
ApplicationUser ──1:N── Enrollment ──N:1── Package ──N:N── Course
       │                     │
       │                     └── N:1── Transaction ──N:1── Coupon
       │
       ├──1:N── ExamAttempt ──N:1── Exam ──N:N──┐
       │              │                           │
       │              ├── 1:N── AttemptAnswer     Question ──1:N── QuestionOption
       │              └── 1:1── ExamResult
       │
       ├──1:1── UserProfile
       ├──1:N── RefreshToken
       ├──1:N── Notification
       ├──1:N── SupportTicket
       ├──1:N── Certificate
       └──0:1── Ambassador ──1:N── ReferralLink ──1:N── Referral

Course ──N:1── ExamTrack
Course ──1:N── Module ──1:N── Chapter ──1:N── Lecture ──1:N── ChapterMarker
                                                        ──1:N── Resource
```

---

## 5. Database Conventions

| Convention | Rule | Example |
|-----------|------|---------|
| **Table names** | Plural, PascalCase | `Courses`, `ExamAttempts` |
| **Column names** | PascalCase (EF Core default) | `CreatedAt`, `UserId` |
| **Primary keys** | `Id` (Guid v7, time-ordered) | Sortable by creation time |
| **Foreign keys** | `{Entity}Id` | `CourseId`, `UserId` |
| **Indexes** | On all foreign keys + frequently queried columns | `IX_Enrollment_UserId` |
| **Soft delete** | Global query filter: `IsDeleted == false` | `entity.HasQueryFilter(e => !e.IsDeleted)` |
| **JSON columns** | `jsonb` for flexible metadata | `TopicBreakdownJson`, `MetadataJson` |
| **Enums** | Stored as `int`, name-mapped in API responses | `Status = 1` → `"Active"` |
| **Timestamps** | `DateTimeOffset` (UTC), `timestamptz` in PG | Never `DateTime` |
| **Money** | `decimal(18,2)` | Never `float` or `double` |
| **Strings** | `varchar` with explicit `MaxLength` | `entity.Property(e => e.Name).HasMaxLength(200)` |

---

## 6. Migration Strategy from V1

### 6.1 Data Migration Approach

| V1 Source | V2 Target | Strategy |
|-----------|-----------|----------|
| `AspNetUsers` (SQL Server) | `ApplicationUser` (PostgreSQL) | ETL script: map columns, rehash passwords |
| `tblCourse` | `Course` + `Module` + `Chapter` + `Lecture` | ETL: flatten→normalize hierarchy |
| `tblQuestion` + `tblQuestionOptions` | `Question` + `QuestionOption` | Direct mapping, clean text encoding |
| `tblExamTrack` | `ExamTrack` | Direct mapping |
| `tblSubscription` | `Package` | Map 6 existing packages |
| `tblEnrollment` | `Enrollment` + `AccessGrant` | Map active enrollments, recalculate expiry |
| `tblPayment` | `Transaction` | Historical data import |
| `tbl_Ambassador` + `tbl_Referral` | `Ambassador` + `Referral` | Direct mapping |
| `tblCertificate` | `Certificate` | Map existing certificates |

### 6.2 Password Migration

ASP.NET Identity v2 (V1) uses a different hashing format than ASP.NET Core Identity (V2). Options:
1. **Force password reset** for all users on first V2 login — simplest, cleanest
2. **Dual-hash check** — try v2 hash first, fallback to v1 hash, then rehash on success
3. **Pre-migration rehash** — impossible without plaintext passwords

**Decision**: Option 2 (dual-hash) for seamless transition. On successful v1 hash login, immediately rehash to v2 format.

### 6.3 ID Migration

V1 uses `int` primary keys. V2 uses `Guid v7`. Migration strategy:
1. Create mapping table: `V1_V2_IdMap(EntityType, V1Id, V2Id)`
2. ETL scripts generate new Guids, store mapping
3. All V1 foreign key relationships resolved via mapping table
4. Mapping table retained for 6 months post-migration for troubleshooting

---

## 7. EF Core Configuration Example

```csharp
// Infrastructure/Persistence/Configurations/CourseConfiguration.cs
public sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title).HasMaxLength(300).IsRequired();
        builder.Property(e => e.Slug).HasMaxLength(300).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(5000);
        builder.Property(e => e.ThumbnailUrl).HasMaxLength(500);

        builder.HasIndex(e => e.Slug).IsUnique();
        builder.HasIndex(e => e.ExamTrackId);
        builder.HasIndex(e => e.IsPublished);

        builder.HasOne(e => e.ExamTrack)
            .WithMany(t => t.Courses)
            .HasForeignKey(e => e.ExamTrackId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Modules)
            .WithOne(m => m.Course)
            .HasForeignKey(m => m.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Soft delete global filter
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
```

---

## 8. Total Entity Count

| Context | Entities | Enums |
|---------|:--------:|:-----:|
| Identity | 4 | 0 |
| Catalog | 6 | 0 |
| Assessment | 6 | 3 |
| Enrollment | 2 | 1 |
| Payment | 4 | 4 |
| Communication | 5 | 5 |
| Certificate | 2 | 1 |
| Ambassador | 4 | 4 |
| Analytics | 3 | 0 |
| **Total** | **36** | **18** |

---

*This document defines the complete data model for FAME V2. All entities follow the base class pattern, use Guid v7 keys, and are configured via IEntityTypeConfiguration. The migration strategy preserves all V1 data while moving to the new schema.*
