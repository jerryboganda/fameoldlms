# Ambassador Portal - Architecture & Data Model

**Version:** 1.0  
**Date:** February 2, 2026

---

## 1. DATABASE SCHEMA

### 1.1 Entity Relationship Diagram (Textual)

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                           AMBASSADOR PORTAL SCHEMA                                   │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                     │
│  AspNetUsers (existing)                                                              │
│       │                                                                             │
│       │ 1:1                                                                         │
│       ▼                                                                             │
│  ┌─────────────────┐                                                                │
│  │ tbl_Ambassador  │─────────────────┐                                              │
│  │─────────────────│                 │                                              │
│  │ Id (PK)         │                 │ 1:N                                          │
│  │ UserId (FK)     │                 ▼                                              │
│  │ ReferralCode    │        ┌─────────────────────┐                                 │
│  │ Status          │        │ tbl_ReferralClick   │ (optional)                      │
│  │ Tier            │        │─────────────────────│                                 │
│  │ University      │        │ Id, AmbassadorId    │                                 │
│  │ CreatedAt       │        │ IPHash, UAHash      │                                 │
│  └────────┬────────┘        │ CreatedAt           │                                 │
│           │                 └─────────────────────┘                                 │
│           │ 1:N                                                                     │
│           ▼                                                                         │
│  ┌─────────────────┐        ┌─────────────────────┐                                 │
│  │ tbl_Referral    │───────►│ AspNetUsers         │ (referred student)              │
│  │─────────────────│  N:1   │ (ReferredUserId)    │                                 │
│  │ Id (PK)         │        └─────────────────────┘                                 │
│  │ AmbassadorId    │                                                                │
│  │ ReferredUserId  │                                                                │
│  │ RegisteredAt    │                                                                │
│  │ Status          │                                                                │
│  └────────┬────────┘                                                                │
│           │ 1:1                                                                     │
│           ▼                                                                         │
│  ┌──────────────────────┐   ┌─────────────────────┐                                 │
│  │ tbl_ReferralConversion│──►│ tbl_EnrollmentMaster│ (existing)                     │
│  │──────────────────────│   └─────────────────────┘                                 │
│  │ Id (PK)              │                                                           │
│  │ ReferralId (FK)      │                                                           │
│  │ EnrollmentId (FK)    │                                                           │
│  │ Amount, Currency     │                                                           │
│  │ Status               │                                                           │
│  │ ConvertedAt          │                                                           │
│  └──────────┬───────────┘                                                           │
│             │ 1:1                                                                   │
│             ▼                                                                       │
│  ┌────────────────────────┐                                                         │
│  │ tbl_AmbassadorEarning  │ (Ledger)                                                │
│  │────────────────────────│                                                         │
│  │ Id (PK)                │                                                         │
│  │ AmbassadorId (FK)      │                                                         │
│  │ ConversionId (FK)      │                                                         │
│  │ Amount, Currency       │                                                         │
│  │ Status                 │                                                         │
│  │ CreatedAt              │                                                         │
│  └────────────────────────┘                                                         │
│                                                                                     │
│  ┌────────────────────────┐                                                         │
│  │ tbl_CommissionRule     │                                                         │
│  │────────────────────────│                                                         │
│  │ Id (PK)                │                                                         │
│  │ PackageId (FK, null)   │──► tbl_Package                                          │
│  │ DurationMonths (null)  │                                                         │
│  │ CommissionType         │                                                         │
│  │ CommissionValue        │                                                         │
│  │ AppliesTo              │                                                         │
│  │ MinTier (null)         │                                                         │
│  │ EffectiveFrom/To       │                                                         │
│  │ IsActive               │                                                         │
│  └────────────────────────┘                                                         │
│                                                                                     │
│  ┌────────────────────────┐                                                         │
│  │ tbl_PayoutMethod       │                                                         │
│  │────────────────────────│                                                         │
│  │ Id (PK)                │                                                         │
│  │ AmbassadorId (FK)      │                                                         │
│  │ MethodType             │                                                         │
│  │ AccountTitle           │                                                         │
│  │ MaskedDetails          │                                                         │
│  │ EncryptedPayload       │                                                         │
│  │ IsDefault, IsActive    │                                                         │
│  │ CreatedAt              │                                                         │
│  └────────────────────────┘                                                         │
│                                                                                     │
│  ┌────────────────────────┐                                                         │
│  │ tbl_PayoutRequest      │                                                         │
│  │────────────────────────│                                                         │
│  │ Id (PK)                │                                                         │
│  │ AmbassadorId (FK)      │                                                         │
│  │ PayoutMethodId (FK)    │                                                         │
│  │ Amount, Currency       │                                                         │
│  │ Status                 │                                                         │
│  │ RequestedAt            │                                                         │
│  │ ProcessedAt, ProcessedBy│                                                        │
│  │ AdminNotes             │                                                         │
│  └────────────────────────┘                                                         │
│                                                                                     │
│  ┌────────────────────────┐                                                         │
│  │ tbl_AmbassadorAuditLog │                                                         │
│  │────────────────────────│                                                         │
│  │ Id (PK)                │                                                         │
│  │ AmbassadorId (FK, null)│                                                         │
│  │ Action                 │                                                         │
│  │ EntityType, EntityId   │                                                         │
│  │ OldValue, NewValue     │                                                         │
│  │ PerformedBy            │                                                         │
│  │ PerformedAt            │                                                         │
│  │ IPHash                 │                                                         │
│  └────────────────────────┘                                                         │
│                                                                                     │
└─────────────────────────────────────────────────────────────────────────────────────┘
```

### 1.2 Table Definitions

#### tbl_Ambassador
```sql
CREATE TABLE tbl_Ambassador (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId NVARCHAR(128) NOT NULL UNIQUE,
    ReferralCode NVARCHAR(20) NOT NULL UNIQUE,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- Pending, Active, Suspended, Rejected
    Tier NVARCHAR(20) NOT NULL DEFAULT 'Bronze', -- Bronze, Silver, Gold
    University NVARCHAR(200) NULL,
    Country NVARCHAR(100) NULL,
    PhoneNumber NVARCHAR(20) NULL,
    ApplicationNotes NVARCHAR(MAX) NULL,
    ApprovedBy NVARCHAR(128) NULL,
    ApprovedAt DATETIME NULL,
    SuspendedReason NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT FK_Ambassador_User FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id),
    CONSTRAINT FK_Ambassador_ApprovedBy FOREIGN KEY (ApprovedBy) REFERENCES AspNetUsers(Id)
);

CREATE INDEX IX_Ambassador_ReferralCode ON tbl_Ambassador(ReferralCode);
CREATE INDEX IX_Ambassador_Status ON tbl_Ambassador(Status);
CREATE INDEX IX_Ambassador_UserId ON tbl_Ambassador(UserId);
```

#### tbl_ReferralClick (Optional)
```sql
CREATE TABLE tbl_ReferralClick (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    AmbassadorId INT NOT NULL,
    IPHash NVARCHAR(64) NULL,
    UserAgentHash NVARCHAR(64) NULL,
    Referer NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT FK_ReferralClick_Ambassador FOREIGN KEY (AmbassadorId) REFERENCES tbl_Ambassador(Id)
);

CREATE INDEX IX_ReferralClick_AmbassadorId ON tbl_ReferralClick(AmbassadorId);
CREATE INDEX IX_ReferralClick_CreatedAt ON tbl_ReferralClick(CreatedAt);
```

#### tbl_Referral
```sql
CREATE TABLE tbl_Referral (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AmbassadorId INT NOT NULL,
    ReferredUserId NVARCHAR(128) NOT NULL,
    RegisteredAt DATETIME NOT NULL DEFAULT GETDATE(),
    VerifiedAt DATETIME NULL,
    Source NVARCHAR(50) NOT NULL DEFAULT 'Link', -- Link, Code, Manual
    Status NVARCHAR(20) NOT NULL DEFAULT 'Registered', -- Registered, Verified, Converted, Invalid
    IPHash NVARCHAR(64) NULL,
    Notes NVARCHAR(500) NULL,
    
    CONSTRAINT FK_Referral_Ambassador FOREIGN KEY (AmbassadorId) REFERENCES tbl_Ambassador(Id),
    CONSTRAINT FK_Referral_ReferredUser FOREIGN KEY (ReferredUserId) REFERENCES AspNetUsers(Id),
    CONSTRAINT UQ_Referral_ReferredUser UNIQUE (ReferredUserId) -- One ambassador per student
);

CREATE INDEX IX_Referral_AmbassadorId ON tbl_Referral(AmbassadorId);
CREATE INDEX IX_Referral_Status ON tbl_Referral(Status);
CREATE INDEX IX_Referral_RegisteredAt ON tbl_Referral(RegisteredAt);
```

#### tbl_ReferralConversion
```sql
CREATE TABLE tbl_ReferralConversion (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ReferralId INT NOT NULL,
    EnrollmentId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Currency NVARCHAR(10) NOT NULL DEFAULT 'PKR',
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- Pending, Approved, Revoked, Disputed
    ConvertedAt DATETIME NOT NULL DEFAULT GETDATE(),
    ApprovedAt DATETIME NULL,
    RevokedAt DATETIME NULL,
    RevokeReason NVARCHAR(500) NULL,
    
    CONSTRAINT FK_Conversion_Referral FOREIGN KEY (ReferralId) REFERENCES tbl_Referral(Id),
    CONSTRAINT FK_Conversion_Enrollment FOREIGN KEY (EnrollmentId) REFERENCES tbl_EnrollmentMaster(Enrollment_Id)
);

CREATE INDEX IX_Conversion_ReferralId ON tbl_ReferralConversion(ReferralId);
CREATE INDEX IX_Conversion_Status ON tbl_ReferralConversion(Status);
CREATE INDEX IX_Conversion_ConvertedAt ON tbl_ReferralConversion(ConvertedAt);
```

#### tbl_CommissionRule
```sql
CREATE TABLE tbl_CommissionRule (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PackageId INT NULL, -- NULL = all packages
    DurationMonths INT NULL, -- NULL = all durations
    CommissionType NVARCHAR(20) NOT NULL, -- Fixed, Percentage
    CommissionValue DECIMAL(18,2) NOT NULL,
    AppliesTo NVARCHAR(20) NOT NULL DEFAULT 'FirstPaymentOnly', -- FirstPaymentOnly, AllPayments
    MinTier NVARCHAR(20) NULL, -- Bronze, Silver, Gold
    EffectiveFrom DATETIME NOT NULL,
    EffectiveTo DATETIME NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedBy NVARCHAR(128) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT FK_CommissionRule_Package FOREIGN KEY (PackageId) REFERENCES tbl_Package(PackageID),
    CONSTRAINT FK_CommissionRule_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES AspNetUsers(Id)
);

CREATE INDEX IX_CommissionRule_PackageId ON tbl_CommissionRule(PackageId);
CREATE INDEX IX_CommissionRule_IsActive ON tbl_CommissionRule(IsActive);
```

#### tbl_AmbassadorEarning
```sql
CREATE TABLE tbl_AmbassadorEarning (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AmbassadorId INT NOT NULL,
    ConversionId INT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Currency NVARCHAR(10) NOT NULL DEFAULT 'PKR',
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- Pending, Available, PaidOut, Revoked
    Type NVARCHAR(20) NOT NULL DEFAULT 'Commission', -- Commission, Bonus, Adjustment, Reversal
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    AvailableAt DATETIME NULL,
    PaidOutAt DATETIME NULL,
    PayoutRequestId INT NULL,
    
    CONSTRAINT FK_Earning_Ambassador FOREIGN KEY (AmbassadorId) REFERENCES tbl_Ambassador(Id),
    CONSTRAINT FK_Earning_Conversion FOREIGN KEY (ConversionId) REFERENCES tbl_ReferralConversion(Id)
);

CREATE INDEX IX_Earning_AmbassadorId ON tbl_AmbassadorEarning(AmbassadorId);
CREATE INDEX IX_Earning_Status ON tbl_AmbassadorEarning(Status);
CREATE INDEX IX_Earning_CreatedAt ON tbl_AmbassadorEarning(CreatedAt);
```

#### tbl_PayoutMethod
```sql
CREATE TABLE tbl_PayoutMethod (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AmbassadorId INT NOT NULL,
    MethodType NVARCHAR(50) NOT NULL, -- BankTransfer, JazzCash, EasyPaisa
    AccountTitle NVARCHAR(200) NOT NULL,
    MaskedAccountNumber NVARCHAR(50) NOT NULL, -- e.g., ****1234
    EncryptedPayload NVARCHAR(MAX) NOT NULL, -- Encrypted full details
    IsDefault BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT FK_PayoutMethod_Ambassador FOREIGN KEY (AmbassadorId) REFERENCES tbl_Ambassador(Id)
);

CREATE INDEX IX_PayoutMethod_AmbassadorId ON tbl_PayoutMethod(AmbassadorId);
```

#### tbl_PayoutRequest
```sql
CREATE TABLE tbl_PayoutRequest (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AmbassadorId INT NOT NULL,
    PayoutMethodId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Currency NVARCHAR(10) NOT NULL DEFAULT 'PKR',
    Status NVARCHAR(20) NOT NULL DEFAULT 'Requested', -- Requested, Approved, Paid, Rejected
    RequestedAt DATETIME NOT NULL DEFAULT GETDATE(),
    ProcessedAt DATETIME NULL,
    ProcessedBy NVARCHAR(128) NULL,
    AdminNotes NVARCHAR(500) NULL,
    TransactionRef NVARCHAR(100) NULL,
    
    CONSTRAINT FK_PayoutRequest_Ambassador FOREIGN KEY (AmbassadorId) REFERENCES tbl_Ambassador(Id),
    CONSTRAINT FK_PayoutRequest_Method FOREIGN KEY (PayoutMethodId) REFERENCES tbl_PayoutMethod(Id),
    CONSTRAINT FK_PayoutRequest_ProcessedBy FOREIGN KEY (ProcessedBy) REFERENCES AspNetUsers(Id)
);

CREATE INDEX IX_PayoutRequest_AmbassadorId ON tbl_PayoutRequest(AmbassadorId);
CREATE INDEX IX_PayoutRequest_Status ON tbl_PayoutRequest(Status);
CREATE INDEX IX_PayoutRequest_RequestedAt ON tbl_PayoutRequest(RequestedAt);
```

#### tbl_AmbassadorAuditLog
```sql
CREATE TABLE tbl_AmbassadorAuditLog (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    AmbassadorId INT NULL,
    Action NVARCHAR(100) NOT NULL,
    EntityType NVARCHAR(100) NOT NULL,
    EntityId INT NULL,
    OldValue NVARCHAR(MAX) NULL,
    NewValue NVARCHAR(MAX) NULL,
    PerformedBy NVARCHAR(128) NOT NULL,
    PerformedAt DATETIME NOT NULL DEFAULT GETDATE(),
    IPHash NVARCHAR(64) NULL,
    
    CONSTRAINT FK_AuditLog_Ambassador FOREIGN KEY (AmbassadorId) REFERENCES tbl_Ambassador(Id),
    CONSTRAINT FK_AuditLog_PerformedBy FOREIGN KEY (PerformedBy) REFERENCES AspNetUsers(Id)
);

CREATE INDEX IX_AuditLog_AmbassadorId ON tbl_AmbassadorAuditLog(AmbassadorId);
CREATE INDEX IX_AuditLog_PerformedAt ON tbl_AmbassadorAuditLog(PerformedAt);
CREATE INDEX IX_AuditLog_Action ON tbl_AmbassadorAuditLog(Action);
```

---

## 2. SERVICE LAYER DESIGN

### 2.1 Interfaces

```
BLL/Interfaces/
├── IAmbassadorRepository.cs      - CRUD for ambassadors
├── IReferralRepository.cs        - Referral tracking
├── ICommissionService.cs         - Commission calculation
├── IPayoutService.cs             - Payout processing
├── IAmbassadorAuditService.cs    - Audit logging
```

### 2.2 Interface Definitions

```csharp
// IAmbassadorRepository.cs
public interface IAmbassadorRepository
{
    // Ambassador CRUD
    AmbassadorVM GetById(int id);
    AmbassadorVM GetByUserId(string userId);
    AmbassadorVM GetByReferralCode(string code);
    List<AmbassadorVM> GetList(AmbassadorFilterVM filter);
    int Apply(AmbassadorApplicationVM model);
    bool Approve(int ambassadorId, string approvedBy);
    bool Reject(int ambassadorId, string reason, string rejectedBy);
    bool Suspend(int ambassadorId, string reason, string suspendedBy);
    bool Reactivate(int ambassadorId, string reactivatedBy);
    bool UpdateTier(int ambassadorId, string tier);
    
    // Dashboard
    AmbassadorDashboardVM GetDashboard(int ambassadorId);
    
    // Validation
    bool IsValidReferralCode(string code);
    bool CanApply(string userId);
}

// IReferralRepository.cs
public interface IReferralRepository
{
    // Referral management
    int CreateReferral(string ambassadorCode, string referredUserId, string source, string ipHash);
    bool UpdateReferralStatus(int referralId, string status);
    ReferralVM GetById(int id);
    ReferralVM GetByReferredUserId(string userId);
    List<ReferralVM> GetByAmbassador(int ambassadorId, ReferralFilterVM filter);
    
    // Click tracking
    void RecordClick(string referralCode, string ipHash, string uaHash, string referer);
    int GetClickCount(int ambassadorId, DateTime? from, DateTime? to);
    
    // Conversion
    int CreateConversion(int referralId, int enrollmentId, decimal amount, string currency);
    bool ApproveConversion(int conversionId);
    bool RevokeConversion(int conversionId, string reason);
    List<ConversionVM> GetConversions(int ambassadorId, ConversionFilterVM filter);
    List<ConversionVM> GetAllConversions(ConversionFilterVM filter); // Admin
}

// ICommissionService.cs
public interface ICommissionService
{
    // Rule management
    List<CommissionRuleVM> GetRules(bool activeOnly = true);
    CommissionRuleVM GetRule(int id);
    int CreateRule(CommissionRuleVM rule);
    bool UpdateRule(CommissionRuleVM rule);
    bool DeactivateRule(int ruleId);
    
    // Calculation
    decimal CalculateCommission(int ambassadorId, int enrollmentId);
    void ProcessConversionCommission(int conversionId);
    
    // Grace period
    void ProcessPendingCommissions(); // Called by scheduled job
}

// IPayoutService.cs
public interface IPayoutService
{
    // Payout methods
    List<PayoutMethodVM> GetMethods(int ambassadorId);
    int AddMethod(PayoutMethodVM method);
    bool UpdateMethod(PayoutMethodVM method);
    bool DeleteMethod(int methodId);
    bool SetDefaultMethod(int methodId, int ambassadorId);
    
    // Balance
    BalanceVM GetBalance(int ambassadorId);
    
    // Payout requests
    int RequestPayout(int ambassadorId, int methodId, decimal amount);
    bool ApprovePayout(int requestId, string approvedBy);
    bool RejectPayout(int requestId, string reason, string rejectedBy);
    bool MarkAsPaid(int requestId, string transactionRef, string paidBy);
    List<PayoutRequestVM> GetRequests(int ambassadorId);
    List<PayoutRequestVM> GetAllRequests(PayoutFilterVM filter); // Admin
}
```

### 2.3 Implementation Classes

```
BLL/
├── AmbassadorRepository.cs
├── ReferralRepository.cs
├── CommissionService.cs
├── PayoutService.cs
├── AmbassadorAuditService.cs
├── SecurityHelper.cs (existing - add encryption methods)
```

---

## 3. CONTROLLER & VIEW STRUCTURE

### 3.1 Area Structure

```
Areas/
└── Ambassador/
    ├── AmbassadorAreaRegistration.cs
    ├── Controllers/
    │   ├── DashboardController.cs      - Ambassador dashboard
    │   ├── ReferralsController.cs      - Referral management
    │   ├── EarningsController.cs       - Earnings & ledger
    │   ├── PayoutsController.cs        - Payout methods & requests
    │   ├── ProfileController.cs        - Ambassador profile
    │   └── AdminController.cs          - Admin management (separate auth)
    ├── Views/
    │   ├── Dashboard/
    │   │   └── Index.cshtml
    │   ├── Referrals/
    │   │   ├── Index.cshtml
    │   │   └── _ReferralList.cshtml
    │   ├── Earnings/
    │   │   ├── Index.cshtml
    │   │   └── _EarningsLedger.cshtml
    │   ├── Payouts/
    │   │   ├── Index.cshtml
    │   │   ├── Methods.cshtml
    │   │   ├── _AddMethod.cshtml
    │   │   └── _RequestPayout.cshtml
    │   ├── Profile/
    │   │   └── Index.cshtml
    │   ├── Admin/
    │   │   ├── Index.cshtml           - Ambassador list
    │   │   ├── Applications.cshtml
    │   │   ├── Rules.cshtml
    │   │   ├── Conversions.cshtml
    │   │   └── Payouts.cshtml
    │   └── Shared/
    │       ├── _Layout.cshtml         - Or use _LayoutTeacher
    │       └── _AmbassadorNav.cshtml
    └── Models/ (optional - can use main Models folder)
        └── AmbassadorViewModels.cs
```

### 3.2 Routes

| Route | Controller | Action | Access |
|-------|------------|--------|--------|
| `/Ambassador` | Dashboard | Index | Ambassador |
| `/Ambassador/Referrals` | Referrals | Index | Ambassador |
| `/Ambassador/Earnings` | Earnings | Index | Ambassador |
| `/Ambassador/Payouts` | Payouts | Index | Ambassador |
| `/Ambassador/Payouts/Methods` | Payouts | Methods | Ambassador |
| `/Ambassador/Profile` | Profile | Index | Ambassador |
| `/Ambassador/Admin` | Admin | Index | Admin |
| `/Ambassador/Admin/Applications` | Admin | Applications | Admin |
| `/Ambassador/Admin/Rules` | Admin | Rules | Admin |
| `/Ambassador/Admin/Conversions` | Admin | Conversions | Admin |
| `/Ambassador/Admin/Payouts` | Admin | Payouts | Admin |
| `/ref/{code}` | Home (main) | Referral | Public |

---

## 4. SECURITY NOTES

### 4.1 Anti-Fraud Measures

| Check | Implementation |
|-------|----------------|
| Self-referral (same user) | Block if `ReferredUserId == Ambassador.UserId` |
| Self-referral (same email domain) | Block if personal email domains match |
| Same IP registration | Flag if IP hash matches ambassador's recent login |
| Rapid bulk registrations | Flag if >5 registrations from same IP in 24h |
| Multiple device fingerprints | Not implemented (privacy-first) |

### 4.2 Data Protection

| Data | Protection |
|------|------------|
| IP Address | SHA256 hash only, never raw |
| Bank account details | AES-256 encryption with key in config |
| Payout method display | Mask all but last 4 digits |
| Referral code | Random alphanumeric, not guessable |

### 4.3 Input Validation

- All monetary inputs: Range validation, max 2 decimal places
- Referral code: Alphanumeric only, 6-20 chars
- Status values: Enum validation
- Foreign keys: Existence checks

---

## 5. MIGRATION PLAN

### 5.1 Execution Order

1. Run SQL script to create tables
2. Update EDMX or add partial classes
3. Add enum values to `ENUM.cs`
4. Add "Ambassador" role to AspNetRoles
5. Register services in Autofac
6. Deploy Area with controllers/views
7. Add menu items to sidebar
8. Test integration points

### 5.2 Rollback Steps

```sql
-- In case of rollback (order matters due to FKs)
DROP TABLE IF EXISTS tbl_AmbassadorAuditLog;
DROP TABLE IF EXISTS tbl_PayoutRequest;
DROP TABLE IF EXISTS tbl_PayoutMethod;
DROP TABLE IF EXISTS tbl_AmbassadorEarning;
DROP TABLE IF EXISTS tbl_ReferralConversion;
DROP TABLE IF EXISTS tbl_Referral;
DROP TABLE IF EXISTS tbl_ReferralClick;
DROP TABLE IF EXISTS tbl_CommissionRule;
DROP TABLE IF EXISTS tbl_Ambassador;

DELETE FROM AspNetRoles WHERE Name = 'Ambassador';
```

---

## 6. CONFIGURATION

### 6.1 Web.config AppSettings

```xml
<appSettings>
  <!-- Ambassador Portal Settings -->
  <add key="Ambassador:MinPayoutAmount" value="1000" />
  <add key="Ambassador:GracePeriodDays" value="7" />
  <add key="Ambassador:ReferralCookieDays" value="30" />
  <add key="Ambassador:DefaultCommissionPercent" value="10" />
  <add key="Ambassador:PayoutEncryptionKey" value="[GENERATE-SECURE-KEY]" />
</appSettings>
```

### 6.2 ENUM Additions

```csharp
// Add to BLL/ENUM.cs
public enum Roles { Student = 1, Teacher = 2, Admin = 3, Assistant = 4, SuppAgent = 5, Ambassador = 6 }

public enum AmbassadorStatus { Pending, Active, Suspended, Rejected }
public enum AmbassadorTier { Bronze, Silver, Gold }
public enum ReferralStatus { Registered, Verified, Converted, Invalid }
public enum ConversionStatus { Pending, Approved, Revoked, Disputed }
public enum EarningStatus { Pending, Available, PaidOut, Revoked }
public enum EarningType { Commission, Bonus, Adjustment, Reversal }
public enum PayoutMethodType { BankTransfer, JazzCash, EasyPaisa }
public enum PayoutStatus { Requested, Approved, Paid, Rejected }
public enum CommissionType { Fixed, Percentage }
public enum CommissionAppliesTo { FirstPaymentOnly, AllPayments }
```
