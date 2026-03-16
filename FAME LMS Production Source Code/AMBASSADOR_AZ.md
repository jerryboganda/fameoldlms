# Ambassador Portal: The Complete A-Z Guide

> **Authoritative Documentation for FAME LMS Ambassador System**
> Last Updated: Feb 5, 2026

## 1. Overview
The Ambassador Portal is a performance-based marketing system integrated into FAME LMS. It allows verified users (Ambassadors) to refer new students and earn commissions on successful subscriptions (Conversions).

## 2. Terminology
- **Ambassador**: A verified user (usually a medical student or professional) who promotes the platform.
- **Referral Code**: A unique 8-character alphanumeric code (e.g., `87F1R2A9`) assigned to each ambassador.
- **Referral**: A new student who registers using an ambassador's link or code.
- **Conversion**: A referral that results in a paid subscription/enrollment.
- **Earning**: Commission awarded to an ambassador (starts as "Pending").
- **Payout**: A transfer of "Available" funds from FAME to the ambassador's bank or e-wallet.

---

## 3. The Ambassador Lifecycle

### Phase 1: Application & Approval
1.  **Application**: A student applies via the `/Ambassador/Profile/Apply` page, providing university, country, and contact details.
2.  **Pending State**: The account is created with `Status = "Pending"`.
3.  **Admin Review**: An Admin reviews the application in the Admin Dashboard.
4.  **Activation**: On approval, the user is assigned the **"Ambassador"** role and granted access to the `/Ambassador` area.

### Phase 2: Promotion & Tracking
1.  **Links**: Ambassadors get a unique link: `firstaidmadeeasy.com.pk/ref/{code}`.
2.  **Cookies**: Visiting the link sets a **30-day cookie** (`fame_ref`).
3.  **Attribution**: If the visitor registers within 30 days, they are permanently linked to the ambassador in `tbl_Referral`.
4.  **Privacy**: Clicks track IPHash and UserAgentHash (SHA256) instead of raw data to ensure privacy.

### Phase 3: Conversions & Commissions
1.  **Enrollment**: When a referred student subscribes to a package, a `tbl_ReferralConversion` is created.
2.  **Calculation**: The `CommissionService` calculates the reward based on:
    -   **Package Type** (e.g., DHA, PLAB).
    -   **Ambassador Tier** (Bronze, Silver, Gold).
    -   **Rule Type** (Percentage or Flat Amount).
3.  **Hold Period**: Earnings are generated as **"Pending"** with a **7-day grace period** (to protect against refunds/chargebacks).

### Phase 4: Payouts
1.  **Balance**: Once the grace period passes, earnings move to **"Available Balance"**.
2.  **Request**: Once the minimum threshold (Rs. 1000) is met, the ambassador can request a payment.
3.  **Methods**: Supports Bank Transfer, JazzCash, and EasyPaisa. Sensitive bank details are **AES-256 encrypted**.
4.  **Processing**: Admin reviews the request, executes the transfer, and marks it as **"Paid"**.

---

## 4. Ambassador Tiers & Rules

### Tier System
| Tier | Conversions Required | Commission Multiplier |
|------|---------------------|----------------------|
| **Bronze** | 0 - 9 | 1.0x (Standard) |
| **Silver** | 10 - 49 | 1.2x (+20% Bonus) |
| **Gold** | 50+ | 1.5x (+50% Bonus) |

### Commission Rules
Admins can define rules in the `tbl_CommissionRule` table:
- **RuleName**: descriptive name.
- **CommissionType**: `Percentage` or `Flat`.
- **CommissionValue**: The rate/amount.
- **AppliesTo**: `FirstPaymentOnly` or `AllPayments`.
- **MinTier**: Minimum tier required for this rule to apply.

---

## 5. Technical Implementation Details

### Database Schema (Core Tables)
- `tbl_Ambassador`: Profile, Tier, ReferralCode, Status.
- `tbl_Referral`: Links referred students to ambassadors.
- `tbl_ReferralClick`: Raw traffic tracking.
- `tbl_ReferralConversion`: Successful sales records.
- `tbl_AmbassadorEarning`: Ledger of all commissions and adjustments.
- `tbl_PayoutMethod`: Encrypted payment details.
- `tbl_PayoutRequest`: Payout lifecycle tracking.
- `tbl_CommissionRule`: Configurable logic for the commission engine.

### Security & Anti-Fraud
- **Self-Referral**: Blocked by comparing `ReferredUserId` and `Ambassador.UserId`.
- **IP Hashing**: Uses `SHA256` hashing for click/registration tracking.
- **Data Encryption**: Bank account details are encrypted in the database using a secure key.
- **Role Isolation**: Only users with the `Ambassador` role can access the portal.

---

## 6. How to Use (Quick Start)

### For Ambassadors:
1.  Go to **Ambassador Portal** -> **Profile** -> **Apply**.
2.  Once approved, copy your **Referral Link** from the Dashboard.
3.  Share on social media or with classmates.
4.  Monitor your **Earnings** tab.
5.  Add a **Payout Method** and hit **Request Payout** when you hit the limit.

### For Admins:
1.  Access **Ambassador Admin** (via `/Ambassador/Admin`).
2.  Review **Applications** to approve new members.
3.  Configure **Commission Rules** per package/tier.
4.  Review **Pending Conversions** and **Payout Requests**.
5.  Mark payouts as **Paid** after processing transactions.

---
*End of Ambassador A-Z Documentation.*
