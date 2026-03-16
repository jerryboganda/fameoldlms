# Ambassador Portal - Zero-Downtime Deployment Guide

## 🎯 Overview

This guide ensures **ZERO DOWNTIME** deployment of the Ambassador Portal to the production FAME LMS at **www.firstaidmadeeasy.com.pk**.

### Key Safety Features:
1. **Feature Flags**: All new functionality is **DISABLED by default** until you explicitly enable it
2. **Isolated Code**: New Ambassador area won't affect existing routes/functionality
3. **Safe Hooks**: Registration hook is wrapped in try-catch - will never break registration
4. **Rollback Ready**: Simply disable feature flags to revert (no code rollback needed)

---

## 📋 Pre-Deployment Checklist

- [ ] Database backup completed
- [ ] Application backup completed  
- [ ] Off-peak hours selected (recommended: late night or early morning)
- [ ] Admin access to SQL Server Management Studio
- [ ] Access to IIS or hosting panel

---

## 🚀 Deployment Steps

### Phase 1: Database - Feature Flags (2 minutes)
**Risk: ZERO** - Only adds new settings rows

```sql
-- Run in SQL Server Management Studio
-- Connect to FAME_DB production database

-- Execute the feature flags script
-- This adds settings with ALL FEATURES DISABLED
```

1. Open SQL Server Management Studio
2. Connect to production database `FAME_DB`
3. Open file: `Database/deploy_ambassador_phase1_feature_flags.sql`
4. Execute the script
5. Verify output shows all features as `🔴 DISABLED`

**Expected Result:**
```
Feature:AmbassadorProgram = false (DISABLED)
Feature:AmbassadorReferralTracking = false (DISABLED)
Feature:AmbassadorCommissionProcessing = false (DISABLED)
```

---

### Phase 2: Database - Ambassador Tables (5 minutes)
**Risk: ZERO** - Only creates NEW tables, doesn't modify existing ones

1. Open file: `Database/create_ambassador_tables.sql`
2. Execute the script
3. Verify all 9 tables created successfully
4. Verify views and stored procedures created

**Tables Created:**
- tbl_Ambassador
- tbl_ReferralClick
- tbl_Referral
- tbl_ReferralConversion
- tbl_CommissionRule
- tbl_AmbassadorEarning
- tbl_PayoutMethod
- tbl_PayoutRequest
- tbl_AmbassadorAuditLog

---

### Phase 3: Deploy Application Code (5-10 minutes)
**Risk: LOW** - New code is isolated; feature flags prevent execution

#### Option A: Visual Studio Publish (Recommended)
1. Build solution in Release mode
2. Right-click FAME.Web → Publish
3. Publish to production folder/FTP

#### Option B: Manual File Copy
Copy these new/modified files to production:

**New Files (copy entire folders):**
```
Areas/Ambassador/               (entire folder)
BLL/AmbassadorRepository.cs
BLL/AmbassadorAuditService.cs
BLL/AmbassadorEnums.cs
BLL/CommissionService.cs
BLL/FeatureFlags.cs
BLL/PayoutService.cs
BLL/ReferralRepository.cs
BLL/ReferralTrackingHelper.cs
BLL/Interfaces/IAmbassadorRepository.cs
BLL/Interfaces/IAmbassadorAuditService.cs
BLL/Interfaces/ICommissionService.cs
BLL/Interfaces/IPayoutService.cs
BLL/Interfaces/IReferralRepository.cs
Controllers/ReferralController.cs
DAL/AmbassadorEntities.cs
Models/AmbassadorViewModels.cs
```

**Modified Files:**
```
App_Start/AutofacConfig.cs      (added DI registrations)
Controllers/AccountController.cs (added safe referral hook)
Views/Shared/_TeacherAside.cshtml (added admin menu)
```

#### Option C: Git Deploy
```bash
git pull origin main
# Restart application pool in IIS
```

---

### Phase 4: Verify Deployment (5 minutes)

1. **Check site is running**: Visit https://www.firstaidmadeeasy.com.pk
2. **Check student registration**: Try the registration page (should work normally)
3. **Check admin login**: Login as admin
4. **Check new menu**: Look for "Ambassador Program" in admin sidebar

**At this point, the menu will be visible but features are DISABLED.**

---

### Phase 5: Enable Features (Gradual)

#### Step 5A: Enable Admin Panel
Allows admins to access Ambassador dashboard and review applications.

```sql
UPDATE tbl_Settings SET Value = 'true' WHERE Name = 'Feature:AmbassadorProgram';
```

**Test:**
- Go to `/Ambassador/Admin`
- Should see empty dashboard
- Try the Applications page

#### Step 5B: Enable Referral Tracking
Starts tracking referral links and attributing new registrations.

```sql
UPDATE tbl_Settings SET Value = 'true' WHERE Name = 'Feature:AmbassadorReferralTracking';
```

**Test:**
1. Create a test ambassador (or have one approved)
2. Visit their referral link: `/ref/TESTCODE`
3. Complete a test registration
4. Check if referral appears in ambassador dashboard

#### Step 5C: Enable Commission Processing
Starts calculating and awarding commissions automatically.

```sql
UPDATE tbl_Settings SET Value = 'true' WHERE Name = 'Feature:AmbassadorCommissionProcessing';
```

**Test:**
1. Create another test referral
2. Check earnings appear for the ambassador

---

## ⚠️ Emergency Rollback

If ANY issues occur, immediately disable all features:

```sql
-- EMERGENCY: Disable all ambassador features instantly
UPDATE tbl_Settings SET Value = 'false' WHERE Name LIKE 'Feature:Ambassador%';

-- Verify disabled
SELECT Name, Value FROM tbl_Settings WHERE Name LIKE 'Feature:Ambassador%';
```

This will:
- ✅ Stop all referral tracking
- ✅ Stop all commission processing
- ✅ Keep admin panel visible but non-functional
- ✅ NOT affect any existing registrations or students
- ✅ NOT require code rollback

---

## 📊 Monitoring

After deployment, monitor these:

1. **Error Logs**: Check `App_Data/Logs/log.txt` for any ambassador-related errors
2. **Registration Flow**: Verify students can still register normally
3. **CPU/Memory**: Monitor server resources (should be minimal impact)

---

## 🔧 Configuration

### Adjust Settings via Database

```sql
-- Minimum payout amount (ETB)
UPDATE tbl_Settings SET Value = '1000' WHERE Name = 'Ambassador:MinPayoutAmount';

-- Commission hold period (days before earnings become available)
UPDATE tbl_Settings SET Value = '14' WHERE Name = 'Ambassador:CommissionHoldDays';

-- Check current settings
SELECT * FROM tbl_Settings WHERE Name LIKE 'Ambassador:%';
```

---

## 📝 Post-Deployment Tasks

1. [ ] Add "Become an Ambassador" link to student dashboard (optional)
2. [ ] Create initial commission rules in Admin > Rules
3. [ ] Configure payout methods (Bank Transfer, Mobile Money)
4. [ ] Train admin staff on approving applications
5. [ ] Prepare marketing materials with referral link format

---

## 📞 Support

If you encounter any issues during deployment:
1. First: Disable features via SQL (see Emergency Rollback)
2. Check error logs
3. The existing LMS will continue working normally regardless

---

## ✅ Deployment Complete Checklist

- [ ] Phase 1: Feature flags added (all disabled)
- [ ] Phase 2: Ambassador tables created
- [ ] Phase 3: Application code deployed
- [ ] Phase 4: Site verified working
- [ ] Phase 5A: Admin panel enabled and tested
- [ ] Phase 5B: Referral tracking enabled and tested
- [ ] Phase 5C: Commission processing enabled and tested
- [ ] Monitoring in place

**Estimated Total Time: 20-30 minutes**
**Downtime: ZERO**
