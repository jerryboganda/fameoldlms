# 18 — Testing and Quality Assurance Strategy

*Testing pyramid, test categories, automation framework, coverage targets, and QA workflows.*

---

## 1. Testing Pyramid

```
                    ╱╲
                   ╱  ╲           E2E Tests (Playwright)
                  ╱    ╲          ~30 critical flows
                 ╱──────╲         Run: nightly + pre-release
                ╱        ╲
               ╱          ╲       Integration Tests
              ╱            ╲      ~150 tests (API + DB)
             ╱──────────────╲     Run: CI on every PR
            ╱                ╲
           ╱                  ╲   Unit Tests
          ╱                    ╲  ~500+ tests
         ╱──────────────────────╲ Run: CI on every push
        ╱                        ╲
       ╱     Component Tests      ╲ ~200 tests (React Testing Library)
      ╱────────────────────────────╲ Run: CI on every push
```

---

## 2. Coverage Targets

| Layer | Target Coverage | Enforcement |
|-------|:--------------:|-------------|
| **Backend unit tests** | ≥80% line coverage | CI fails below threshold |
| **Frontend component tests** | ≥70% line coverage | CI fails below threshold |
| **Integration tests** | All API endpoints covered | PR checklist |
| **E2E tests** | All critical user flows | Nightly pipeline |
| **Overall** | ≥75% combined | Codecov dashboard |

---

## 3. Backend Testing

### 3.1 Unit Tests (xUnit + NSubstitute)

```csharp
// Tests/Modules/Payment/ValidateCouponTests.cs
public class ValidateCouponTests
{
    private readonly ICouponService _sut;
    private readonly FakeDbContext _db;
    
    public ValidateCouponTests()
    {
        _db = FakeDbContext.CreateInMemory();
        _sut = new CouponService(_db);
    }
    
    [Fact]
    public async Task ValidCoupon_ReturnsDiscount()
    {
        // Arrange
        var packageId = Guid.CreateVersion7();
        _db.Packages.Add(new Package { Id = packageId, PricePkr = 10_000 });
        _db.Coupons.Add(new Coupon
        {
            Code = "FAME10",
            Type = CouponType.Percentage,
            Value = 10,
            IsActive = true,
        });
        await _db.SaveChangesAsync();
        
        // Act
        var result = await _sut.ValidateCoupon("FAME10", packageId, "user1");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1000m, result.Value.DiscountAmount);
        Assert.Equal(9000m, result.Value.FinalAmount);
    }
    
    [Fact]
    public async Task ExpiredCoupon_ReturnsError()
    {
        // Arrange
        _db.Coupons.Add(new Coupon
        {
            Code = "EXPIRED",
            IsActive = true,
            ValidUntil = DateTimeOffset.UtcNow.AddDays(-1),
        });
        await _db.SaveChangesAsync();
        
        // Act
        var result = await _sut.ValidateCoupon("EXPIRED", Guid.NewGuid(), "user1");
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("expired", result.Error, StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public async Task AlreadyUsedCoupon_ReturnsError()
    {
        // Arrange
        var coupon = new Coupon { Code = "ONCE", IsActive = true };
        _db.Coupons.Add(coupon);
        _db.Transactions.Add(new Transaction
        {
            CouponId = coupon.Id,
            UserId = "user1",
            Status = TransactionStatus.Completed,
        });
        await _db.SaveChangesAsync();
        
        // Act
        var result = await _sut.ValidateCoupon("ONCE", Guid.NewGuid(), "user1");
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("already used", result.Error, StringComparison.OrdinalIgnoreCase);
    }
}
```

### 3.2 Integration Tests (WebApplicationFactory)

```csharp
// Tests/Integration/PaymentApiTests.cs
public class PaymentApiTests : IClassFixture<FameWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly FameWebAppFactory _factory;
    
    public PaymentApiTests(FameWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task Checkout_WithValidPackage_ReturnsRedirectUrl()
    {
        // Arrange
        await _factory.SeedTestData();
        await _client.AuthenticateAsStudent();
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/checkout", new
        {
            PackageId = TestData.BasicPackageId,
            Gateway = "jazzcash",
        });
        
        // Assert
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<CheckoutResponse>();
        Assert.NotNull(body?.RedirectUrl);
    }
    
    [Fact]
    public async Task Checkout_Unauthenticated_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/checkout", new
        {
            PackageId = Guid.NewGuid(),
            Gateway = "jazzcash",
        });
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task JazzCashWebhook_ValidSignature_CreatesEnrollment()
    {
        // Arrange
        await _factory.SeedTestData();
        var transaction = await _factory.CreatePendingTransaction();
        
        var webhookPayload = JazzCashTestHelper.CreateValidWebhook(transaction.Id);
        
        // Act
        var response = await _client.PostAsync(
            "/api/webhooks/jazzcash",
            new FormUrlEncodedContent(webhookPayload));
        
        // Assert
        response.EnsureSuccessStatusCode();
        
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FameDbContext>();
        var enrollment = await db.Enrollments
            .FirstOrDefaultAsync(e => e.TransactionId == transaction.Id);
        Assert.NotNull(enrollment);
        Assert.Equal(EnrollmentStatus.Active, enrollment.Status);
    }
}
```

### 3.3 Test Utilities

```csharp
// Tests/Shared/FameWebAppFactory.cs
public class FameWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace PostgreSQL with in-memory for speed
            services.RemoveAll<DbContextOptions<FameDbContext>>();
            services.AddDbContext<FameDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
            
            // Replace external services with fakes
            services.AddSingleton<IEmailService, FakeEmailService>();
            services.AddSingleton<IPaymentGateway, FakePaymentGateway>();
            services.AddSingleton<IStorageService, FakeStorageService>();
        });
    }
    
    public async Task SeedTestData()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FameDbContext>();
        await TestData.SeedAll(db);
    }
}
```

---

## 4. Frontend Testing

### 4.1 Component Tests (Vitest + React Testing Library)

```typescript
// __tests__/components/CourseCard.test.tsx
import { render, screen } from '@testing-library/react';
import { CourseCard } from '@/components/CourseCard';

describe('CourseCard', () => {
  const course = {
    id: '1',
    title: 'PLAB-1 Clinical Pathology',
    examTrack: 'PLAB-1',
    thumbnailUrl: '/images/plab1.jpg',
    progress: 65,
    lectureCount: 24,
    completedLectures: 16,
  };

  it('renders course title', () => {
    render(<CourseCard course={course} />);
    expect(screen.getByText('PLAB-1 Clinical Pathology')).toBeInTheDocument();
  });

  it('shows progress bar at correct percentage', () => {
    render(<CourseCard course={course} />);
    const progressBar = screen.getByRole('progressbar');
    expect(progressBar).toHaveAttribute('aria-valuenow', '65');
  });

  it('displays lecture count', () => {
    render(<CourseCard course={course} />);
    expect(screen.getByText('16 / 24 lectures')).toBeInTheDocument();
  });

  it('shows exam track badge', () => {
    render(<CourseCard course={course} />);
    expect(screen.getByText('PLAB-1')).toBeInTheDocument();
  });
});
```

### 4.2 Hook Tests

```typescript
// __tests__/hooks/useExamTimer.test.ts
import { renderHook, act } from '@testing-library/react';
import { useExamTimer } from '@/hooks/useExamTimer';

describe('useExamTimer', () => {
  beforeEach(() => vi.useFakeTimers());
  afterEach(() => vi.useRealTimers());

  it('counts down from initial time', () => {
    const { result } = renderHook(() => useExamTimer(3600)); // 1 hour
    
    expect(result.current.timeRemaining).toBe(3600);
    expect(result.current.formattedTime).toBe('60:00');
    
    act(() => vi.advanceTimersByTime(1000));
    expect(result.current.timeRemaining).toBe(3599);
  });

  it('triggers warning at 5 minutes', () => {
    const onWarning = vi.fn();
    const { result } = renderHook(() => useExamTimer(360, { onWarning }));
    
    act(() => vi.advanceTimersByTime(60_000)); // 1 minute
    
    expect(result.current.isWarning).toBe(true);
    expect(onWarning).toHaveBeenCalledWith(300);
  });

  it('auto-submits at zero', () => {
    const onExpire = vi.fn();
    renderHook(() => useExamTimer(5, { onExpire }));
    
    act(() => vi.advanceTimersByTime(5000));
    
    expect(onExpire).toHaveBeenCalledOnce();
  });
});
```

---

## 5. E2E Tests (Playwright)

### 5.1 Critical Flow Tests

```typescript
// e2e/student-enrollment.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Student Enrollment Flow', () => {
  test('complete registration → purchase → course access', async ({ page }) => {
    // 1. Register
    await page.goto('/register');
    await page.fill('[name="firstName"]', 'Test');
    await page.fill('[name="lastName"]', 'Student');
    await page.fill('[name="email"]', `test-${Date.now()}@example.com`);
    await page.fill('[name="password"]', 'SecurePass123');
    await page.click('button[type="submit"]');
    
    // Verify email (test mode — auto-verified)
    await expect(page).toHaveURL('/dashboard');
    
    // 2. Browse courses
    await page.goto('/pricing');
    await expect(page.locator('[data-testid="package-card"]')).toHaveCount(6);
    
    // 3. Initiate checkout
    await page.click('[data-testid="package-plab1"] button');
    await expect(page).toHaveURL('/checkout');
    
    // 4. Apply coupon
    await page.fill('[name="coupon"]', 'TEST10');
    await page.click('[data-testid="apply-coupon"]');
    await expect(page.locator('[data-testid="discount"]')).toContainText('1,000');
    
    // 5. Select payment gateway (test mode)
    await page.click('[data-testid="gateway-test"]');
    await page.click('[data-testid="complete-purchase"]');
    
    // 6. Verify enrollment
    await expect(page).toHaveURL('/checkout/success');
    await page.goto('/dashboard');
    await expect(page.locator('[data-testid="enrolled-course"]')).toHaveCount.greaterThan(0);
  });
});
```

### 5.2 E2E Test Coverage Matrix

| Flow | Priority | Status |
|------|:--------:|:------:|
| Registration + email verification | P0 | Planned |
| Login + logout | P0 | Planned |
| Password reset | P0 | Planned |
| Course catalog browsing | P0 | Planned |
| Package checkout (all gateways) | P0 | Planned |
| Course video playback | P0 | Planned |
| MCQ exam taking + scoring | P0 | Planned |
| Exam result review | P1 | Planned |
| Subscription management (renew, cancel) | P1 | Planned |
| Admin: user management | P1 | Planned |
| Admin: course CRUD | P1 | Planned |
| Admin: transaction list | P1 | Planned |
| Ambassador: referral flow | P2 | Planned |
| Notification delivery | P2 | Planned |
| Certificate download | P2 | Planned |

---

## 6. Specialized Testing

### 6.1 Exam Engine Testing

The exam engine requires dedicated testing due to its complexity:

| Test Category | What it Validates |
|--------------|-------------------|
| Question randomization | Different order per attempt; all questions appear |
| Timer accuracy | Timer counts down correctly; auto-submit on expiry |
| Answer persistence | Answers saved if browser refreshes mid-exam |
| Score calculation | Correct/incorrect tallying matches expected result |
| Negative marking | Penalty applied correctly (if configured) |
| Strike-through | UI state doesn't affect scoring |
| Special characters | Apostrophes, quotes, HTML entities in questions display correctly |

### 6.2 Payment Gateway Testing

| Scenario | JazzCash | Easypaisa | Stripe |
|----------|:--------:|:---------:|:------:|
| Successful payment | Sandbox API | Sandbox API | Test mode |
| Failed payment | Simulated | Simulated | Test decline card |
| Webhook replay | Verify idempotency | Verify idempotency | Verify idempotency |
| Invalid signature | Reject webhook | Reject webhook | Reject webhook |
| Timeout/network error | Graceful handling | Graceful handling | Graceful handling |

### 6.3 Security Testing

| Test | Tool | Frequency |
|------|------|-----------|
| Dependency vulnerabilities | `npm audit` + `dotnet audit` | Every CI run |
| OWASP ZAP scan | ZAP Docker | Weekly |
| CSP validation | CSP Evaluator | On header change |
| Rate limiting verification | Custom Playwright test | Monthly |
| SQL injection | Parameterized query lint | Every CI run |
| XSS testing | OWASP ZAP | Weekly |

---

## 7. QA Workflow

### 7.1 PR Review Checklist

```markdown
## PR Quality Checklist
- [ ] Unit tests added/updated for new logic
- [ ] Integration tests added for new API endpoints
- [ ] Component tests added for new UI components
- [ ] No console.log or debugger statements
- [ ] No hardcoded secrets or credentials
- [ ] Error states handled (loading, error, empty)
- [ ] Accessibility: keyboard navigable, ARIA labels, color contrast
- [ ] Mobile responsive (tested at 375px width)
- [ ] API endpoints have authorization attributes
- [ ] Database queries use pagination for lists
```

### 7.2 Release Testing Workflow

```
Feature branch → PR → CI (lint + unit + integration) → Review → Merge to main
                                                                       │
                                                                       ▼
main → CI (full suite) → Deploy staging → E2E tests → Manual smoke test
                                                              │
                                                              ▼
                                              Tag release → Deploy production
                                                              │
                                                              ▼
                                              Post-deploy health check (5 min)
```

---

## 8. Test Data Management

```csharp
// Tests/Shared/TestData.cs
public static class TestData
{
    // Deterministic IDs for cross-test reference
    public static readonly Guid AdminUserId = Guid.Parse("00000001-0000-0000-0000-000000000001");
    public static readonly Guid StudentUserId = Guid.Parse("00000001-0000-0000-0000-000000000002");
    public static readonly Guid BasicPackageId = Guid.Parse("00000002-0000-0000-0000-000000000001");
    
    public static async Task SeedAll(FameDbContext db)
    {
        await SeedUsers(db);
        await SeedPackages(db);
        await SeedCourses(db);
        await SeedQuestions(db);
        await db.SaveChangesAsync();
    }
}
```

---

*Quality is built in, not tested in. The testing pyramid ensures fast feedback (unit tests in <30s), thorough validation (integration tests per PR), and end-to-end confidence (E2E nightly). The exam engine and payment gateways get dedicated test suites because they are the highest-risk areas.*
