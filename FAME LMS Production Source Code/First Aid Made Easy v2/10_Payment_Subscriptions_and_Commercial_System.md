# 10 — Payment, Subscriptions, and Commercial System

*Self-service checkout, three payment gateways, subscription lifecycle, coupons, and automated enrollment.*

---

## 1. V1 → V2 Commercial Transformation

| Aspect | V1 | V2 |
|--------|----|----|
| **Purchase flow** | Contact admin → bank transfer → wait | Browse → Checkout → Pay → Instant access |
| **Payment gateways** | JazzCash + Easypaisa (sandbox credentials) | JazzCash + Easypaisa (production) + Stripe |
| **Credential storage** | Client-side JavaScript | Server-side, secrets manager |
| **Enrollment** | Manual admin activation | Webhook-triggered, instant |
| **Subscription management** | None (contact admin) | Self-service dashboard |
| **Renewal** | Contact admin, repeat process | One-click or auto-renewal |
| **Coupons** | Exist in DB, disabled in UI | Full coupon system restored |
| **Pricing page** | Broken/incomplete | Dynamic, always up-to-date |
| **Currency** | PKR only (effectively) | PKR + USD |
| **Admin involvement** | Every transaction | Exception handling only |

---

## 2. V1 Subscription Packages (To Migrate)

| # | Package Name | Price (PKR) | Price (USD) | Duration | Included Tracks |
|---|-------------|:-----------:|:-----------:|:--------:|----------------|
| 1 | AMC-1 Package | 10,000 | 35 | 6 months | AMC-1 courses |
| 2 | Basic Package | 3,000 | 15 | 3 months | Basic clinical courses |
| 3 | Clinical Package | 8,000 | 30 | 6 months | All clinical courses |
| 4 | NRE-1 Package | 3,000 | 12 | 3 months | NRE-1 courses |
| 5 | NRE-2 Package | 3,000 | 12 | 3 months | NRE-2 courses |
| 6 | PLAB-1 Package | 10,000 | 40 | 6 months | PLAB-1 courses |

These establish V2 baseline pricing. Additional packages may be created via admin panel.

---

## 3. Self-Service Checkout Flow

### 3.1 Complete Flow Diagram

```
Student                  Frontend              Backend               Gateway
  │                        │                     │                     │
  │ 1. Browse /pricing     │                     │                     │
  │───────────────────────►│                     │                     │
  │                        │  GET /api/packages  │                     │
  │                        │────────────────────►│                     │
  │                        │  Package list       │                     │
  │◄───────────────────────│◄────────────────────│                     │
  │                        │                     │                     │
  │ 2. Select package      │                     │                     │
  │───────────────────────►│                     │                     │
  │                        │                     │                     │
  │ 3. Apply coupon        │                     │                     │
  │───────────────────────►│ POST /api/coupons   │                     │
  │                        │   /validate         │                     │
  │                        │────────────────────►│                     │
  │                        │  Discount amount    │                     │
  │◄───────────────────────│◄────────────────────│                     │
  │                        │                     │                     │
  │ 4. Choose gateway      │                     │                     │
  │   & confirm            │                     │                     │
  │───────────────────────►│ POST /api/checkout  │                     │
  │                        │────────────────────►│                     │
  │                        │                     │ Create Transaction  │
  │                        │                     │ (status: Pending)   │
  │                        │                     │                     │
  │                        │                     │ POST gateway/init   │
  │                        │                     │────────────────────►│
  │                        │                     │ Redirect URL        │
  │                        │                     │◄────────────────────│
  │                        │  Redirect to gateway│                     │
  │◄───────────────────────│◄────────────────────│                     │
  │                        │                     │                     │
  │ 5. Complete payment    │                     │                     │
  │   on gateway page      │                     │                     │
  │────────────────────────────────────────────────────────────────────►│
  │                        │                     │                     │
  │                        │                     │ 6. Webhook: paid    │
  │                        │                     │◄────────────────────│
  │                        │                     │ Verify signature    │
  │                        │                     │ Update Transaction  │
  │                        │                     │ Create Enrollment   │
  │                        │                     │ Create AccessGrants │
  │                        │                     │ Queue welcome email │
  │                        │                     │                     │
  │ 7. Redirect back       │                     │                     │
  │◄────────────────────────────────────────────────────────────────────│
  │                        │                     │                     │
  │ 8. Show success page   │                     │                     │
  │───────────────────────►│ GET /api/enrollment │                     │
  │                        │   /status           │                     │
  │                        │────────────────────►│                     │
  │  ✅ Enrolled!          │  Active enrollment  │                     │
  │◄───────────────────────│◄────────────────────│                     │
```

### 3.2 Checkout Page UI

```
┌──────────────────────────────────────────────────────────────┐
│  Checkout                                                    │
│                                                              │
│  ┌─────────────────────────┐  ┌────────────────────────────┐│
│  │  Order Summary           │  │  Payment Method            ││
│  │                         │  │                            ││
│  │  PLAB-1 Complete Pack   │  │  ○ JazzCash               ││
│  │  Duration: 6 months     │  │    Pay with JazzCash wallet││
│  │  Includes:              │  │                            ││
│  │  ✓ All PLAB-1 courses   │  │  ○ Easypaisa              ││
│  │  ✓ MCQ practice exams   │  │    Pay with Easypaisa      ││
│  │  ✓ Mock tests           │  │                            ││
│  │  ✓ Lab values reference │  │  ○ Card (Visa/Mastercard)  ││
│  │                         │  │    International payments   ││
│  │  ─────────────────────  │  │                            ││
│  │  Subtotal: PKR 10,000   │  │                            ││
│  │  Discount: -PKR 1,000   │  │                            ││
│  │  ─────────────────────  │  │                            ││
│  │  Total: PKR 9,000       │  │                            ││
│  │                         │  │                            ││
│  │  Coupon: [FAME10____]   │  │                            ││
│  │          [Apply]        │  │                            ││
│  └─────────────────────────┘  └────────────────────────────┘│
│                                                              │
│  🔒 Secure payment  •  Money-back guarantee  •  Need help?  │
│                                                              │
│                        [Complete Purchase — PKR 9,000]       │
└──────────────────────────────────────────────────────────────┘
```

---

## 4. Payment Gateway Integrations

### 4.1 JazzCash Integration

```csharp
// Server-side only — no credentials in frontend
public class JazzCashPaymentService : IPaymentGateway
{
    private readonly JazzCashSettings _settings; // From secrets manager
    
    public async Task<PaymentInitResult> InitiatePayment(PaymentRequest request)
    {
        var payload = new Dictionary<string, string>
        {
            ["pp_MerchantID"] = _settings.MerchantId,
            ["pp_Password"] = _settings.Password,
            ["pp_Amount"] = (request.Amount * 100).ToString("0"), // Paisa
            ["pp_TxnRefNo"] = request.TransactionId.ToString(),
            ["pp_Description"] = $"FAME - {request.PackageName}",
            ["pp_TxnCurrency"] = "PKR",
            ["pp_TxnDateTime"] = DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
            ["pp_TxnExpiryDateTime"] = DateTime.UtcNow.AddHours(1).ToString("yyyyMMddHHmmss"),
            ["pp_ReturnURL"] = $"{_settings.ReturnBaseUrl}/checkout/callback/jazzcash",
            ["pp_TxnType"] = "MWALLET",
        };
        
        // Generate HMAC signature
        payload["pp_SecureHash"] = GenerateHmacHash(payload, _settings.IntegritySalt);
        
        return new PaymentInitResult
        {
            RedirectUrl = _settings.PaymentUrl,
            FormData = payload,
        };
    }

    public async Task<PaymentVerifyResult> VerifyWebhook(HttpRequest request)
    {
        var responseCode = request.Form["pp_ResponseCode"];
        var txnRefNo = request.Form["pp_TxnRefNo"];
        var secureHash = request.Form["pp_SecureHash"];
        
        // Verify HMAC signature
        if (!VerifyHmacHash(request.Form, _settings.IntegritySalt, secureHash))
            return PaymentVerifyResult.InvalidSignature();
        
        return responseCode == "000"
            ? PaymentVerifyResult.Success(txnRefNo, request.Form["pp_Amount"])
            : PaymentVerifyResult.Failed(responseCode, request.Form["pp_ResponseMessage"]);
    }
}
```

### 4.2 Easypaisa Integration

```csharp
public class EasypaisaPaymentService : IPaymentGateway
{
    private readonly EasypaisaSettings _settings;
    
    public async Task<PaymentInitResult> InitiatePayment(PaymentRequest request)
    {
        var payload = new
        {
            storeId = _settings.StoreId,
            amount = request.Amount,
            postBackURL = $"{_settings.ReturnBaseUrl}/checkout/callback/easypaisa",
            orderRefNum = request.TransactionId.ToString(),
            expiryDate = DateTime.UtcNow.AddHours(1).ToString("yyyyMMdd HHmmss"),
            autoRedirect = 1,
            paymentMethod = "InitialRequest",
            emailAddr = request.Email,
        };
        
        // Generate token from Easypaisa API
        var token = await RequestToken(payload);
        
        return new PaymentInitResult
        {
            RedirectUrl = $"{_settings.PaymentUrl}?token={token}",
        };
    }
}
```

### 4.3 Stripe Integration (International)

```csharp
public class StripePaymentService : IPaymentGateway
{
    private readonly StripeSettings _settings;
    
    public async Task<PaymentInitResult> InitiatePayment(PaymentRequest request)
    {
        StripeConfiguration.ApiKey = _settings.SecretKey;
        
        var options = new Stripe.Checkout.SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
            {
                new()
                {
                    PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        UnitAmount = (long)(request.AmountUsd * 100),
                        ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                        {
                            Name = request.PackageName,
                            Description = $"FAME - {request.PackageName} ({request.DurationDays} days)",
                        },
                    },
                    Quantity = 1,
                },
            },
            Mode = "payment",
            SuccessUrl = $"{_settings.ReturnBaseUrl}/checkout/success?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = $"{_settings.ReturnBaseUrl}/checkout/cancelled",
            ClientReferenceId = request.TransactionId.ToString(),
            CustomerEmail = request.Email,
            Metadata = new Dictionary<string, string>
            {
                ["transactionId"] = request.TransactionId.ToString(),
                ["packageId"] = request.PackageId.ToString(),
            },
        };
        
        var session = await new Stripe.Checkout.SessionService().CreateAsync(options);
        
        return new PaymentInitResult { RedirectUrl = session.Url };
    }
    
    public async Task<PaymentVerifyResult> VerifyWebhook(HttpRequest request)
    {
        var json = await new StreamReader(request.Body).ReadToEndAsync();
        var stripeEvent = EventUtility.ConstructEvent(
            json,
            request.Headers["Stripe-Signature"],
            _settings.WebhookSecret);
        
        if (stripeEvent.Type == Events.CheckoutSessionCompleted)
        {
            var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
            return PaymentVerifyResult.Success(
                session.ClientReferenceId,
                session.AmountTotal?.ToString());
        }
        
        return PaymentVerifyResult.Ignored();
    }
}
```

### 4.4 Gateway Abstraction

```csharp
public interface IPaymentGateway
{
    PaymentGateway GatewayType { get; }
    Task<PaymentInitResult> InitiatePayment(PaymentRequest request);
    Task<PaymentVerifyResult> VerifyWebhook(HttpRequest request);
}

public class PaymentGatewayFactory
{
    private readonly IEnumerable<IPaymentGateway> _gateways;
    
    public IPaymentGateway GetGateway(PaymentGateway gatewayType) =>
        _gateways.FirstOrDefault(g => g.GatewayType == gatewayType)
        ?? throw new InvalidOperationException($"Gateway {gatewayType} not configured");
}
```

---

## 5. Webhook Processing (Post-Payment)

```csharp
// Features/ProcessPaymentWebhook/ProcessPaymentWebhookHandler.cs
public sealed class ProcessPaymentWebhookHandler
{
    public async Task<Result> Handle(PaymentVerifyResult verification, CancellationToken ct)
    {
        // 1. Find the pending transaction
        var transaction = await _db.Transactions
            .Include(t => t.Package)
            .FirstOrDefaultAsync(t => t.Id == verification.TransactionId, ct);
        
        if (transaction is null)
            return Result.NotFound("Transaction not found");
        
        if (transaction.Status != TransactionStatus.Pending)
            return Result.Conflict("Transaction already processed"); // Idempotency
        
        // 2. Update transaction status
        transaction.Status = TransactionStatus.Completed;
        transaction.PaidAt = DateTimeOffset.UtcNow;
        transaction.GatewayTransactionId = verification.GatewayTransactionId;
        
        // 3. Create enrollment
        var enrollment = new Enrollment
        {
            UserId = transaction.UserId,
            PackageId = transaction.PackageId,
            TransactionId = transaction.Id,
            StartsAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(transaction.Package.DurationDays),
            Status = EnrollmentStatus.Active,
        };
        
        // 4. Create access grants for each course in the package
        var packageCourseIds = await _db.PackageCourses
            .Where(pc => pc.PackageId == transaction.PackageId)
            .Select(pc => pc.CourseId)
            .ToListAsync(ct);
        
        foreach (var courseId in packageCourseIds)
        {
            enrollment.AccessGrants.Add(new AccessGrant
            {
                CourseId = courseId,
                GrantedAt = DateTimeOffset.UtcNow,
            });
        }
        
        _db.Enrollments.Add(enrollment);
        
        // 5. Generate invoice
        var invoice = new Invoice
        {
            TransactionId = transaction.Id,
            UserId = transaction.UserId,
            InvoiceNumber = await GenerateInvoiceNumber(ct),
            Subtotal = transaction.Amount + transaction.DiscountAmount,
            Discount = transaction.DiscountAmount,
            Total = transaction.Amount,
            Currency = transaction.Currency,
        };
        _db.Invoices.Add(invoice);
        
        // 6. Handle coupon usage
        if (transaction.CouponId.HasValue)
        {
            var coupon = await _db.Coupons.FindAsync(transaction.CouponId.Value);
            if (coupon is not null)
                coupon.UsageCount++;
        }
        
        // 7. Handle ambassador referral commission
        if (transaction.CouponId.HasValue)
        {
            await ProcessAmbassadorCommission(transaction, ct);
        }
        
        await _db.SaveChangesAsync(ct);
        
        // 8. Publish event for emails, notifications, analytics
        await _publisher.Publish(new PaymentCompletedEvent(
            transaction.Id, enrollment.Id, transaction.UserId), ct);
        
        return Result.Success();
    }
}
```

---

## 6. Subscription Lifecycle

### 6.1 State Machine

```
                 ┌─────────┐
    Purchase ───►│  Active  │◄─── Renew
                 └────┬─────┘
                      │
            Expires   │   Cancel
        ┌─────────────┼────────────┐
        ▼             │            ▼
   ┌─────────┐       │      ┌───────────┐
   │ Expired  │       │      │ Cancelled │
   └────┬─────┘       │      └───────────┘
        │             │
  Grace │   Suspend   │
  period│   (admin)   │
        ▼             ▼
   ┌─────────┐  ┌───────────┐
   │ Expired  │  │ Suspended │
   │(no grace)│  └───────────┘
   └──────────┘
```

### 6.2 Automated Lifecycle Events

| Event | Timing | Action |
|-------|--------|--------|
| **Expiry Warning 1** | 7 days before expiry | Email + in-app notification |
| **Expiry Warning 2** | 3 days before expiry | Email + in-app notification + banner |
| **Expiry Warning 3** | 1 day before expiry | Email + push notification |
| **Auto-Renewal** | Day of expiry (if opted in) | Process payment → extend subscription |
| **Expiry** | Day of expiry (if no renewal) | Status → Expired, access revoked |
| **Grace Period** | 3 days after expiry | Read-only access, prominent renewal CTA |
| **Grace End** | 3 days after expiry | Full access revocation |

### 6.3 Subscription Management Dashboard

```
┌──────────────────────────────────────────────────────────────┐
│  My Subscription                                             │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  PLAB-1 Complete Package                   ✅ Active   │  │
│  │                                                        │  │
│  │  Started: March 15, 2026                               │  │
│  │  Expires: September 15, 2026 (183 days remaining)      │  │
│  │  Auto-Renew: On [Toggle]                               │  │
│  │                                                        │  │
│  │  Included Courses:                                     │  │
│  │  ✓ PLAB-1 Clinical Pathology                          │  │
│  │  ✓ PLAB-1 Pharmacology                               │  │
│  │  ✓ PLAB-1 MCQ Practice Tests                         │  │
│  │  ✓ PLAB-1 Mock Exams                                 │  │
│  │                                                        │  │
│  │  [Upgrade Plan]  [Cancel Subscription]                 │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                              │
│  Payment History                                             │
│  ┌────────────┬────────────┬──────────┬──────────┬────────┐  │
│  │ Date       │ Amount     │ Method   │ Status   │ Invoice│  │
│  ├────────────┼────────────┼──────────┼──────────┼────────┤  │
│  │ Mar 15     │ PKR 9,000  │ JazzCash │ Paid     │ [PDF]  │  │
│  │ Sep 15 '25 │ PKR 10,000 │ Easypaisa│ Paid     │ [PDF]  │  │
│  └────────────┴────────────┴──────────┴──────────┴────────┘  │
└──────────────────────────────────────────────────────────────┘
```

---

## 7. Coupon System

### 7.1 Coupon Types

| Type | Example | Behavior |
|------|---------|----------|
| **Percentage** | FAME10 = 10% off | Applies percentage discount to package price |
| **Fixed Amount** | SAVE1000 = PKR 1,000 off | Subtracts fixed amount (floor: 0) |
| **Free Trial** | TRYFAME = 7-day free access | Creates enrollment with 0 payment, 7-day expiry |

### 7.2 Coupon Validation Rules

```csharp
public async Task<Result<CouponValidation>> ValidateCoupon(
    string code, Guid packageId, string userId)
{
    var coupon = await _db.Coupons
        .FirstOrDefaultAsync(c => c.Code == code.ToUpper() && c.IsActive && !c.IsDeleted);
    
    if (coupon is null)
        return Result<CouponValidation>.BadRequest("Invalid coupon code");
    
    // Check expiry
    if (coupon.ValidUntil.HasValue && coupon.ValidUntil < DateTimeOffset.UtcNow)
        return Result<CouponValidation>.BadRequest("Coupon has expired");
    
    if (coupon.ValidFrom.HasValue && coupon.ValidFrom > DateTimeOffset.UtcNow)
        return Result<CouponValidation>.BadRequest("Coupon is not yet active");
    
    // Check usage limit
    if (coupon.MaxUsages.HasValue && coupon.UsageCount >= coupon.MaxUsages)
        return Result<CouponValidation>.BadRequest("Coupon usage limit reached");
    
    // Check package restriction
    if (coupon.RestrictToPackageId.HasValue && coupon.RestrictToPackageId != packageId)
        return Result<CouponValidation>.BadRequest("Coupon not valid for this package");
    
    // Check one-per-user
    var alreadyUsed = await _db.Transactions
        .AnyAsync(t => t.CouponId == coupon.Id && t.UserId == userId 
            && t.Status == TransactionStatus.Completed);
    if (alreadyUsed)
        return Result<CouponValidation>.BadRequest("You have already used this coupon");
    
    // Calculate discount
    var package = await _db.Packages.FindAsync(packageId);
    var discount = coupon.Type switch
    {
        CouponType.Percentage => package.PricePkr * (coupon.Value / 100),
        CouponType.FixedAmount => Math.Min(coupon.Value, package.PricePkr),
        CouponType.FreeTrial => package.PricePkr, // Full discount
        _ => 0m
    };
    
    return Result<CouponValidation>.Success(new CouponValidation
    {
        CouponId = coupon.Id,
        Code = coupon.Code,
        DiscountAmount = discount,
        FinalAmount = package.PricePkr - discount,
    });
}
```

### 7.3 Ambassador Referral Coupons

Ambassador referral codes function as coupon codes:
- Auto-generated when ambassador creates a referral link
- Typically 5-10% discount for the referred student
- Commission triggered on successful purchase with the code
- Tracked in both Coupon and Referral tables

---

## 8. Pricing Page Specification

### 8.1 API Response

```json
GET /api/packages

{
  "packages": [
    {
      "id": "uuid",
      "name": "PLAB-1 Complete",
      "slug": "plab-1-complete",
      "description": "Complete PLAB-1 preparation package",
      "pricePkr": 10000,
      "priceUsd": 40,
      "durationDays": 180,
      "badge": "Most Popular",
      "features": [
        "All PLAB-1 video lectures",
        "MCQ practice exams (2000+ questions)",
        "5 full-length mock tests",
        "Lab values reference",
        "Certificate on completion"
      ],
      "courseCount": 4,
      "isActive": true,
      "displayOrder": 1
    }
  ]
}
```

### 8.2 Currency Toggle

The pricing page includes a PKR/USD toggle:
- Default currency detected by IP geolocation (Pakistan → PKR, others → USD)
- User can manually toggle
- Stripe used for USD, JazzCash/Easypaisa for PKR
- USD pricing pre-set per package (not live conversion)

---

## 9. Revenue Analytics (Admin)

| Metric | Visualization | Query |
|--------|:-------------|-------|
| Total revenue (period) | Big number + comparison | SUM(Transactions.Amount) WHERE Status=Completed |
| Revenue by gateway | Pie chart | GROUP BY Gateway |
| Revenue by package | Bar chart | GROUP BY PackageId |
| Revenue by exam track | Bar chart | JOIN Package → Course → ExamTrack |
| Daily revenue trend | Line chart (30 days) | GROUP BY DATE(PaidAt) |
| Conversion rate | Funnel | Registrations → Checkout starts → Payments → Completions |
| Coupon usage | Table | Coupons with usage counts, revenue impact |
| Refund rate | Percentage | Refunds / Total transactions |

---

*This document defines the entire commercial system for FAME V2. The self-service checkout flow is the single highest-impact change — it removes the manual enrollment bottleneck that caps growth. Every payment interaction is server-side, and enrollment is fully automated.*
