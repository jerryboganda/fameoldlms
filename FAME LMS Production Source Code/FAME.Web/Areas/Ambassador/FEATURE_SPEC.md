# Ambassador Portal - Feature Specification

**Version:** 1.0  
**Date:** February 2, 2026  
**Project:** FAME LMS Ambassador Portal

---

## 1. USER STORIES

### 1.1 Ambassador User Stories

| ID | Story | Acceptance Criteria |
|----|-------|---------------------|
| AMB-01 | As a student, I want to apply to become an ambassador so I can earn commissions | - Application form with university/contact details<br>- Auto-approve or manual review based on rules<br>- Email notification on approval |
| AMB-02 | As an ambassador, I want to access a dedicated portal | - Ambassador role assigned on approval<br>- Sidebar menu visible<br>- Dashboard loads with KPIs |
| AMB-03 | As an ambassador, I want unique referral links/codes | - Generate personal referral code (e.g., `REF-{UserId}-{Random}`)<br>- Copy-to-clipboard functionality<br>- Optional: Custom alias support |
| AMB-04 | As an ambassador, I want to share links and track invites | - Shareable URL: `firstaidmadeeasy.com.pk/ref/{code}`<br>- Track clicks (optional)<br>- See list of signups attributed to me |
| AMB-05 | As an ambassador, I want to see conversion funnel | - View: Clicks → Registrations → Verified → Subscribed<br>- Filter by date range<br>- Export capability |
| AMB-06 | As an ambassador, I want to track my earnings | - Pending earnings (awaiting approval)<br>- Available balance (approved, not paid)<br>- Total paid out<br>- Breakdown by package/plan |
| AMB-07 | As an ambassador, I want to request payouts | - Submit payout request<br>- Minimum threshold validation (e.g., Rs. 1000)<br>- Track request status |
| AMB-08 | As an ambassador, I want to manage payout methods | - Add bank account / JazzCash / EasyPaisa<br>- Mask sensitive details<br>- Edit/delete methods |
| AMB-09 | As an ambassador, I want to see my profile/stats | - University affiliation<br>- Join date, tier/level<br>- Lifetime stats |
| AMB-10 | As an ambassador, I want notifications | - Email on new conversion<br>- Email on payout processed<br>- In-app notification badge (if available) |

### 1.2 Admin User Stories

| ID | Story | Acceptance Criteria |
|----|-------|---------------------|
| ADM-01 | As an admin, I want to view all ambassador applications | - List with status filter<br>- Approve/Reject with notes<br>- Bulk actions |
| ADM-02 | As an admin, I want to manage ambassadors | - View all ambassadors<br>- Suspend/Reactivate<br>- Edit tier/commission rate override |
| ADM-03 | As an admin, I want to configure commission rules | - CRUD rules per package<br>- Set fixed amount or percentage<br>- First payment vs recurring<br>- Effective date ranges |
| ADM-04 | As an admin, I want to review conversions | - List all referral conversions<br>- Filter by ambassador, date, status<br>- Manual approve/reject/flag |
| ADM-05 | As an admin, I want to detect fraud | - Flag self-referrals<br>- Highlight suspicious patterns<br>- Manual review queue |
| ADM-06 | As an admin, I want to process payouts | - View pending payout requests<br>- Approve/Reject with notes<br>- Mark as paid<br>- Batch processing |
| ADM-07 | As an admin, I want to export reports | - Ambassador list export<br>- Conversion report export<br>- Payout history export |

---

## 2. COMMISSION & INCENTIVE RULES ENGINE

### 2.1 Commission Calculation Variables

| Variable | Description | Example Values |
|----------|-------------|----------------|
| Package | Which subscription package | DHA/HAAD Package, PLAB Package |
| Duration | Subscription duration | 1 month, 3 months, 12 months |
| Billing Cycle | First payment vs recurring | FirstPayment, Recurring |
| Commission Type | How to calculate | Fixed (Rs.), Percentage (%) |
| Commission Value | The amount/rate | Rs. 500, 10% |
| Tier Bonus | Ambassador level multiplier | Bronze (1x), Silver (1.2x), Gold (1.5x) |
| Currency | Payment currency | PKR, USD |

### 2.2 Commission Rule Schema

```
CommissionRule:
  - PackageId: int (nullable = all packages)
  - DurationMonths: int (nullable = all durations)
  - CommissionType: enum { Fixed, Percentage }
  - CommissionValue: decimal
  - AppliesTo: enum { FirstPaymentOnly, AllPayments }
  - MinTier: enum { Bronze, Silver, Gold } (nullable)
  - EffectiveFrom: datetime
  - EffectiveTo: datetime (nullable)
  - IsActive: bool
```

### 2.3 Commission Calculation Logic

```
FUNCTION CalculateCommission(enrollment, ambassador):
    rules = GetActiveRules(enrollment.PackageId, enrollment.Duration, ambassador.Tier)
    
    IF rules.IsEmpty:
        RETURN 0 // No applicable rule
    
    rule = rules.OrderByPriority().First() // Most specific wins
    
    baseAmount = enrollment.Enrollment_Price
    
    IF rule.CommissionType == Fixed:
        commission = rule.CommissionValue
    ELSE:
        commission = baseAmount * (rule.CommissionValue / 100)
    
    // Apply tier bonus
    commission = commission * ambassador.TierMultiplier
    
    RETURN commission
```

### 2.4 Edge Cases

| Scenario | Handling |
|----------|----------|
| Refund within 7 days | Revoke commission, update ledger with negative entry |
| Chargeback | Immediately flag, revoke commission, mark for review |
| Student switches plan | No additional commission on upgrade; refund delta on downgrade |
| Multiple ambassadors claim | First valid referral wins (by registration timestamp) |
| Self-referral | Block: same email domain, same UserId, same IP within 24h |
| Expired referral code | Reject, show "invalid code" message |
| Ambassador suspended | Pending commissions frozen, no new referrals |

### 2.5 Tier System

| Tier | Conversions Required | Bonus Multiplier | Perks |
|------|---------------------|------------------|-------|
| Bronze | 0-9 | 1.0x | Base commission |
| Silver | 10-49 | 1.2x | +20% commission |
| Gold | 50+ | 1.5x | +50% commission, priority support |

Tier calculated monthly based on trailing 90-day conversions.

---

## 3. TRACKING MODEL

### 3.1 Referral Attribution Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                     REFERRAL ATTRIBUTION FLOW                    │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. CLICK                                                       │
│     └─► User visits: /ref/{code}                                │
│         └─► Validate code                                       │
│             └─► Set cookie: ref_code={code}, 30 days            │
│                 └─► Redirect to registration page               │
│                                                                 │
│  2. REGISTRATION                                                │
│     └─► User submits registration form                          │
│         └─► Check cookie for ref_code                           │
│             └─► Create tbl_Referral record (Status: Registered) │
│                 └─► Clear cookie                                │
│                                                                 │
│  3. VERIFICATION                                                │
│     └─► User verifies email/phone                               │
│         └─► Update Referral (Status: Verified)                  │
│                                                                 │
│  4. SUBSCRIPTION (CONVERSION)                                   │
│     └─► Admin approves enrollment OR user pays                  │
│         └─► Create tbl_ReferralConversion                       │
│             └─► Calculate commission                            │
│                 └─► Create tbl_AmbassadorEarning (Pending)      │
│                                                                 │
│  5. COMMISSION APPROVAL                                         │
│     └─► Grace period passes (7 days)                            │
│         └─► No refund/chargeback                                │
│             └─► Update Earning (Status: Available)              │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### 3.2 Tracking Data Points

| Data Point | Storage | Privacy Note |
|------------|---------|--------------|
| Referral Code | Cookie (30 days) | First-party, user consent via terms |
| Click Timestamp | tbl_ReferralClick | Optional feature |
| IP Address | Hash only (SHA256) | Never store raw IP |
| User Agent | Hash only | For fraud detection |
| Device Fingerprint | Not collected | Privacy-friendly approach |

### 3.3 Cookie Specification

```
Name: fame_ref
Value: {ambassador_code}
Domain: .firstaidmadeeasy.com.pk
Path: /
Expires: 30 days
HttpOnly: true
Secure: true (production)
SameSite: Lax
```

---

## 4. ROLES & AUTHORIZATION

### 4.1 Role Additions

| Role | ID | Description |
|------|----|-----------  |
| Ambassador | 6 | Verified ambassador with portal access |

### 4.2 Permission Matrix

| Action | Student | Ambassador | Admin |
|--------|---------|------------|-------|
| View own dashboard | ✓ | ✓ (Ambassador) | ✓ |
| Generate referral link | ✗ | ✓ | ✗ |
| View own referrals | ✗ | ✓ | ✗ |
| View all referrals | ✗ | ✗ | ✓ |
| Request payout | ✗ | ✓ | ✗ |
| Process payouts | ✗ | ✗ | ✓ |
| Manage commission rules | ✗ | ✗ | ✓ |
| Suspend ambassador | ✗ | ✗ | ✓ |

### 4.3 Data Isolation

```csharp
// In AmbassadorRepository
public List<ReferralVM> GetReferrals(string ambassadorUserId)
{
    // ALWAYS filter by current user's ambassador ID
    return db.tbl_Referral
        .Where(r => r.Ambassador.UserId == ambassadorUserId)
        .Select(...)
        .ToList();
}
```

### 4.4 Controller Authorization

```csharp
[Authorize(Roles = "Ambassador")]
public class AmbassadorController : Controller { }

[Authorize(Roles = "Admin")]
public class AmbassadorAdminController : Controller { }
```

---

## 5. ACCEPTANCE CRITERIA SUMMARY

### 5.1 MVP (Must Have)

- [ ] Ambassador can register/apply
- [ ] Ambassador receives unique referral code
- [ ] Referral link works and sets cookie
- [ ] Registration captures referral attribution
- [ ] Enrollment creates conversion record
- [ ] Commission calculated per rules
- [ ] Ambassador sees dashboard with earnings
- [ ] Ambassador can request payout
- [ ] Admin can approve/reject ambassadors
- [ ] Admin can configure commission rules
- [ ] Admin can process payouts
- [ ] Data isolation enforced (ambassador sees only their data)

### 5.2 Nice to Have (Phase 2)

- [ ] Click tracking with analytics
- [ ] Tiered commission bonuses
- [ ] Campaign tags (track by university/event)
- [ ] QR code generation
- [ ] Leaderboard
- [ ] PDF statement export
- [ ] In-app notifications
- [ ] Advanced fraud detection

### 5.3 Quality Gates

- [ ] All CRUD operations work without errors
- [ ] Commission calculation matches expected values
- [ ] Authorization prevents data leakage
- [ ] UI is responsive on mobile
- [ ] Tables paginate at 100+ records
- [ ] Exports work for 1000+ records

---

## 6. GLOSSARY

| Term | Definition |
|------|------------|
| Ambassador | A verified user who refers students for commission |
| Referral | A student registration attributed to an ambassador |
| Conversion | A referral that resulted in a paid subscription |
| Commission | The earnings an ambassador receives for a conversion |
| Payout | The transfer of available balance to ambassador |
| Tier | Performance level affecting commission rate |
| Attribution | Linking a student signup to the referring ambassador |
| Grace Period | Days before commission becomes available (refund protection) |
