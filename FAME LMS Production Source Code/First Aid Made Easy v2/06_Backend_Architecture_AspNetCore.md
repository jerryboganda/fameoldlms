# 06 — Backend Architecture (ASP.NET Core 9)

*Solution structure, modular monolith organization, CQRS-light pattern, middleware pipeline, and service layer design.*

---

## 1. Solution Structure

```
fame-v2/
├── src/
│   ├── FAME.Api/                     ← Main API host (ASP.NET Core Web API)
│   │   ├── Program.cs                ← Startup, DI, middleware pipeline
│   │   ├── appsettings.json          ← Non-secret configuration
│   │   ├── Middleware/
│   │   │   ├── ExceptionHandlerMiddleware.cs
│   │   │   ├── RequestLoggingMiddleware.cs
│   │   │   ├── CorrelationIdMiddleware.cs
│   │   │   └── RateLimitingMiddleware.cs
│   │   ├── Filters/
│   │   │   └── ValidationActionFilter.cs
│   │   └── Extensions/
│   │       ├── ServiceCollectionExtensions.cs
│   │       └── ApplicationBuilderExtensions.cs
│   │
│   ├── FAME.SharedKernel/            ← Shared types, interfaces, base classes
│   │   ├── Domain/
│   │   │   ├── Entity.cs             ← Base entity with Id
│   │   │   ├── AuditableEntity.cs    ← CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
│   │   │   ├── SoftDeletableEntity.cs ← IsDeleted, DeletedAt, DeletedBy
│   │   │   ├── IDomainEvent.cs
│   │   │   └── Result.cs             ← Result<T> pattern (no exceptions for expected failures)
│   │   ├── Interfaces/
│   │   │   ├── ICurrentUser.cs
│   │   │   ├── IDateTimeProvider.cs
│   │   │   └── IFileStorage.cs
│   │   ├── Pagination/
│   │   │   ├── PagedRequest.cs
│   │   │   └── PagedResult.cs
│   │   └── Constants/
│   │       ├── Roles.cs
│   │       └── Policies.cs
│   │
│   ├── FAME.Infrastructure/          ← Cross-cutting infrastructure
│   │   ├── Persistence/
│   │   │   ├── FameDbContext.cs       ← Single EF Core DbContext
│   │   │   ├── Configurations/       ← IEntityTypeConfiguration per entity
│   │   │   ├── Interceptors/
│   │   │   │   ├── AuditableInterceptor.cs
│   │   │   │   ├── SoftDeleteInterceptor.cs
│   │   │   │   └── MeilisearchSyncInterceptor.cs
│   │   │   └── Migrations/
│   │   ├── Identity/
│   │   │   ├── CurrentUser.cs
│   │   │   ├── JwtTokenService.cs
│   │   │   └── PermissionService.cs
│   │   ├── Storage/
│   │   │   └── S3FileStorage.cs
│   │   ├── Email/
│   │   │   └── SmtpEmailSender.cs
│   │   ├── Search/
│   │   │   └── MeilisearchService.cs
│   │   └── Caching/
│   │       └── RedisCacheService.cs
│   │
│   └── Modules/                      ← Feature modules
│       ├── FAME.Modules.Identity/
│       ├── FAME.Modules.Catalog/
│       ├── FAME.Modules.Assessment/
│       ├── FAME.Modules.Enrollment/
│       ├── FAME.Modules.Payment/
│       ├── FAME.Modules.Communication/
│       ├── FAME.Modules.Certificate/
│       ├── FAME.Modules.Ambassador/
│       ├── FAME.Modules.Content/
│       ├── FAME.Modules.Analytics/
│       └── FAME.Modules.Admin/
│
├── tests/
│   ├── FAME.UnitTests/
│   ├── FAME.IntegrationTests/
│   └── FAME.ArchitectureTests/       ← ArchUnit-style tests for module boundaries
│
├── FAME.sln
├── Directory.Build.props             ← Shared MSBuild properties
├── Directory.Packages.props          ← Central package management
└── .editorconfig
```

---

## 2. Module Internal Structure

Each module follows the same pattern:

```
FAME.Modules.Assessment/
├── AssessmentModule.cs               ← IServiceCollection extension for DI registration
├── Domain/
│   ├── Entities/
│   │   ├── Question.cs
│   │   ├── QuestionOption.cs
│   │   ├── Exam.cs
│   │   ├── ExamAttempt.cs
│   │   └── ExamResult.cs
│   ├── ValueObjects/
│   │   ├── ExamMode.cs               ← Enum: Timed, Practice, Custom
│   │   └── DifficultyLevel.cs
│   └── Events/
│       ├── ExamSubmittedEvent.cs
│       └── QuestionCreatedEvent.cs
│
├── Features/                         ← Vertical slices (CQRS-light)
│   ├── StartExam/
│   │   ├── StartExamCommand.cs       ← MediatR IRequest
│   │   ├── StartExamHandler.cs       ← MediatR IRequestHandler
│   │   ├── StartExamValidator.cs     ← FluentValidation
│   │   └── StartExamEndpoint.cs      ← Minimal API endpoint mapping
│   │
│   ├── SubmitExam/
│   │   ├── SubmitExamCommand.cs
│   │   ├── SubmitExamHandler.cs
│   │   ├── SubmitExamValidator.cs
│   │   └── SubmitExamEndpoint.cs
│   │
│   ├── GetExamResult/
│   │   ├── GetExamResultQuery.cs
│   │   ├── GetExamResultHandler.cs
│   │   └── GetExamResultEndpoint.cs
│   │
│   ├── ListQuestions/
│   │   ├── ListQuestionsQuery.cs
│   │   ├── ListQuestionsHandler.cs
│   │   └── ListQuestionsEndpoint.cs
│   │
│   ├── CreateQuestion/
│   │   ├── CreateQuestionCommand.cs
│   │   ├── CreateQuestionHandler.cs
│   │   ├── CreateQuestionValidator.cs
│   │   └── CreateQuestionEndpoint.cs
│   │
│   └── ImportQuestions/
│       ├── ImportQuestionsCommand.cs
│       ├── ImportQuestionsHandler.cs
│       └── ImportQuestionsEndpoint.cs
│
├── Infrastructure/
│   ├── QuestionConfiguration.cs      ← EF Core entity configuration
│   ├── ExamConfiguration.cs
│   └── ExamAttemptConfiguration.cs
│
└── AssessmentModule.cs
```

### 2.1 Module Registration

```csharp
// FAME.Modules.Assessment/AssessmentModule.cs
public static class AssessmentModule
{
    public static IServiceCollection AddAssessmentModule(this IServiceCollection services)
    {
        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AssessmentModule).Assembly));
        
        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(typeof(AssessmentModule).Assembly);
        
        return services;
    }

    public static WebApplication MapAssessmentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/assessment")
            .WithTags("Assessment")
            .RequireAuthorization();

        // Map all feature endpoints
        group.MapStartExamEndpoint();
        group.MapSubmitExamEndpoint();
        group.MapGetExamResultEndpoint();
        group.MapListQuestionsEndpoint();
        group.MapCreateQuestionEndpoint();
        group.MapImportQuestionsEndpoint();

        return app;
    }
}
```

### 2.2 Feature Slice Example

```csharp
// Features/SubmitExam/SubmitExamCommand.cs
public sealed record SubmitExamCommand(
    Guid AttemptId,
    Dictionary<int, string> Answers  // questionIndex → selectedOption
) : IRequest<Result<ExamResultDto>>;

// Features/SubmitExam/SubmitExamValidator.cs
public sealed class SubmitExamValidator : AbstractValidator<SubmitExamCommand>
{
    public SubmitExamValidator()
    {
        RuleFor(x => x.AttemptId).NotEmpty();
        RuleFor(x => x.Answers).NotEmpty().WithMessage("At least one answer required");
    }
}

// Features/SubmitExam/SubmitExamHandler.cs
public sealed class SubmitExamHandler : IRequestHandler<SubmitExamCommand, Result<ExamResultDto>>
{
    private readonly FameDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IPublisher _publisher;

    public SubmitExamHandler(FameDbContext db, ICurrentUser currentUser, IPublisher publisher)
    {
        _db = db;
        _currentUser = currentUser;
        _publisher = publisher;
    }

    public async Task<Result<ExamResultDto>> Handle(
        SubmitExamCommand request, CancellationToken ct)
    {
        var attempt = await _db.ExamAttempts
            .Include(a => a.Exam)
            .ThenInclude(e => e.Questions)
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId 
                && a.UserId == _currentUser.Id, ct);

        if (attempt is null)
            return Result<ExamResultDto>.NotFound("Exam attempt not found");

        if (attempt.IsSubmitted)
            return Result<ExamResultDto>.Conflict("Exam already submitted");

        // Calculate results
        var result = attempt.CalculateResult(request.Answers);
        attempt.Submit(result);

        await _db.SaveChangesAsync(ct);

        // Publish domain event for analytics, certificates, etc.
        await _publisher.Publish(new ExamSubmittedEvent(attempt.Id, result), ct);

        return Result<ExamResultDto>.Success(result.ToDto());
    }
}

// Features/SubmitExam/SubmitExamEndpoint.cs
public static class SubmitExamEndpoint
{
    public static RouteGroupBuilder MapSubmitExamEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/submit", async (
            SubmitExamCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.Match(
                success: dto => Results.Ok(dto),
                failure: error => error.ToProblemResult()
            );
        })
        .WithName("SubmitExam")
        .Produces<ExamResultDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(Policies.StudentAccess);

        return group;
    }
}
```

---

## 3. Middleware Pipeline

```csharp
// Program.cs — Middleware pipeline order matters!
var app = builder.Build();

// 1. Exception handling (outermost — catches everything)
app.UseMiddleware<ExceptionHandlerMiddleware>();

// 2. Correlation ID (adds X-Correlation-Id to every request/response)
app.UseMiddleware<CorrelationIdMiddleware>();

// 3. Request logging (structured log per request)
app.UseSerilogRequestLogging();

// 4. Security headers (added at Caddy level too, defense-in-depth)
app.UseSecurityHeaders();

// 5. HTTPS redirection
app.UseHttpsRedirection();

// 6. CORS
app.UseCors("FamePolicy");

// 7. Rate limiting
app.UseRateLimiter();

// 8. Authentication
app.UseAuthentication();

// 9. Authorization
app.UseAuthorization();

// 10. Response caching
app.UseResponseCaching();

// 11. Map module endpoints
app.MapIdentityEndpoints();
app.MapCatalogEndpoints();
app.MapAssessmentEndpoints();
app.MapEnrollmentEndpoints();
app.MapPaymentEndpoints();
app.MapCommunicationEndpoints();
app.MapCertificateEndpoints();
app.MapAmbassadorEndpoints();
app.MapContentEndpoints();
app.MapAnalyticsEndpoints();
app.MapAdminEndpoints();

// 12. Hangfire dashboard (admin only)
app.MapHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AdminAuthorizationFilter() }
});

app.Run();
```

---

## 4. Cross-Cutting Behaviors (MediatR Pipeline)

```csharp
// Pipeline: Command → Validation → Logging → Authorization → Handler

// Validation behavior
public sealed class ValidationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = (await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, ct))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0)
            throw new ValidationException(failures);

        return await next();
    }
}

// Logging behavior
public sealed class LoggingBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUser _currentUser;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation(
            "Handling {RequestName} by {UserId}", requestName, _currentUser.Id);

        var sw = Stopwatch.StartNew();
        var response = await next();
        sw.Stop();

        _logger.LogInformation(
            "Handled {RequestName} in {ElapsedMs}ms", requestName, sw.ElapsedMilliseconds);

        return response;
    }
}
```

---

## 5. Authentication & JWT Implementation

```csharp
// Infrastructure/Identity/JwtTokenService.cs
public sealed class JwtTokenService
{
    private readonly JwtSettings _settings;
    private readonly FameDbContext _db;

    public (string AccessToken, string RefreshToken) GenerateTokenPair(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.DisplayName),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var accessToken = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes), // 15 min
            signingCredentials: credentials);

        var refreshToken = GenerateRefreshToken();

        return (
            new JwtSecurityTokenHandler().WriteToken(accessToken),
            refreshToken
        );
    }
}
```

### Cookie-Based Token Delivery

```csharp
// Tokens delivered in httpOnly cookies, NOT in response body
public static void SetAuthCookies(HttpContext context, string accessToken, string refreshToken)
{
    context.Response.Cookies.Append("access_token", accessToken, new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Expires = DateTimeOffset.UtcNow.AddMinutes(15),
        Path = "/api",
    });

    context.Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Expires = DateTimeOffset.UtcNow.AddDays(7),
        Path = "/api/auth/refresh",
    });
}
```

---

## 6. Background Jobs (Hangfire)

### 6.1 Job Categories

| Job | Schedule | Module | Purpose |
|-----|---------|--------|---------|
| `SendExpiryReminders` | Daily 9:00 AM PKT | Enrollment | Email students 7/3/1 days before subscription expiry |
| `ProcessAutoRenewals` | Daily 00:01 AM PKT | Payment | Charge auto-renewal subscriptions due today |
| `SyncSearchIndex` | Every 5 min | Infrastructure | Batch-sync entities modified in last 5 min to Meilisearch |
| `GenerateDailyReport` | Daily 6:00 AM PKT | Analytics | Aggregate yesterday's metrics for admin dashboard |
| `CleanupExpiredSessions` | Hourly | Identity | Remove expired refresh tokens from database |
| `BackupDatabase` | Daily 2:00 AM PKT | Admin | `pg_dump` to S3 storage |
| `ProcessAmbassadorPayouts` | Monthly 1st | Ambassador | Calculate and queue monthly commission payouts |
| `SendScheduledEmails` | Every 1 min | Communication | Process email queue |

### 6.2 Job Registration

```csharp
// Program.cs
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(connectionString));
builder.Services.AddHangfireServer();

// After app.Run() setup:
RecurringJob.AddOrUpdate<ExpiryReminderJob>(
    "send-expiry-reminders",
    job => job.ExecuteAsync(CancellationToken.None),
    "0 4 * * *"); // 04:00 UTC = 09:00 PKT

RecurringJob.AddOrUpdate<SearchIndexSyncJob>(
    "sync-search-index",
    job => job.ExecuteAsync(CancellationToken.None),
    "*/5 * * * *"); // Every 5 minutes
```

---

## 7. Error Handling Architecture

### 7.1 Result Pattern

```csharp
// SharedKernel/Domain/Result.cs
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error? Error { get; }

    private Result(T value) { IsSuccess = true; Value = value; }
    private Result(Error error) { IsSuccess = false; Error = error; }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> NotFound(string message) => new(new Error(404, message));
    public static Result<T> BadRequest(string message) => new(new Error(400, message));
    public static Result<T> Forbidden(string message) => new(new Error(403, message));
    public static Result<T> Conflict(string message) => new(new Error(409, message));

    public IResult Match(
        Func<T, IResult> success,
        Func<Error, IResult> failure) =>
        IsSuccess ? success(Value!) : failure(Error!);
}

public sealed record Error(int StatusCode, string Message, string? Detail = null)
{
    public IResult ToProblemResult() => Results.Problem(
        statusCode: StatusCode,
        title: Message,
        detail: Detail);
}
```

### 7.2 Global Exception Handler

```csharp
// Middleware/ExceptionHandlerMiddleware.cs
public sealed class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = 400,
                Title = "Validation Error",
                Extensions = { ["errors"] = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }) }
            });
        }
        catch (Exception ex)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString();
            _logger.LogError(ex, "Unhandled exception. CorrelationId: {CorrelationId}", correlationId);

            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = 500,
                Title = "An unexpected error occurred",
                Detail = $"Reference: {correlationId}", // NO stack trace!
            });
        }
    }
}
```

---

## 8. Configuration Management

```csharp
// appsettings.json — Non-secret defaults only
{
  "ConnectionStrings": {
    "Database": "" // Set via environment variable
  },
  "Jwt": {
    "Issuer": "fame-api",
    "Audience": "fame-web",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  "Redis": {
    "InstanceName": "fame:"
  },
  "Meilisearch": {
    "Url": "http://meilisearch:7700"
  },
  "Storage": {
    "BucketName": "fame-uploads"
  },
  "Hangfire": {
    "WorkerCount": 2
  }
}

// Secrets via environment variables (Docker Compose / CI):
// DATABASE_URL=Host=postgres;Database=fame;Username=fame_app;Password=...
// JWT_SECRET=<random-256-bit-key>
// REDIS_URL=redis:6379
// MEILISEARCH_KEY=<master-key>
// S3_ACCESS_KEY=...
// S3_SECRET_KEY=...
// JAZZCASH_MERCHANT_ID=...
// JAZZCASH_PASSWORD=...
// EASYPAISA_STORE_ID=...
// STRIPE_SECRET_KEY=...
// SMTP_PASSWORD=...
```

---

## 9. Module Communication Rules

| ✅ Allowed | ❌ Forbidden |
|-----------|-------------|
| Reference entities by ID (Guid) across modules | Direct entity references across modules |
| Publish domain events (MediatR `INotification`) | Call another module's handler directly |
| Use shared interfaces from `SharedKernel` | Reference another module's project |
| Query via read-only DTOs from shared contracts | Share EF DbSet configurations |

**Example**: When an exam is submitted (Assessment module), the Certificate module listens via `ExamSubmittedEvent` to check if a certificate should be generated. Assessment never calls Certificate directly.

---

*This document defines the backend architecture. All API endpoints, database interactions, and business logic must follow the patterns described here. The modular monolith structure ensures feature isolation while keeping deployment simple.*
