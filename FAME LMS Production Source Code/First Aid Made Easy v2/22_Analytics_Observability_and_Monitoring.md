# 22 — Analytics, Observability, and Monitoring

*Structured logging, distributed tracing, application metrics, business analytics, dashboards, and alerting.*

---

## 1. Observability Stack

```
Application Code
      │
      ├── Structured Logs ──────► Serilog ──────► Seq (search & dashboards)
      │
      ├── Metrics ──────────────► OpenTelemetry ──► Prometheus ──► Grafana
      │
      ├── Traces ───────────────► OpenTelemetry ──► Seq (trace viewer)
      │
      └── Business Events ──────► Analytics DB ──► Admin Dashboard
```

---

## 2. Structured Logging (Serilog + Seq)

### 2.1 Log Configuration

```csharp
// Program.cs
builder.Host.UseSerilog((context, config) =>
{
    config
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "FAME.Api")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .Enrich.WithCorrelationId()
        .WriteTo.Console(new CompactJsonFormatter())
        .WriteTo.Seq(context.Configuration["Seq:Url"]!);
});
```

### 2.2 Log Levels & Usage

| Level | When to Use | Example |
|-------|------------|---------|
| **Debug** | Detailed flow info (dev only) | Query parameters, cache hit/miss |
| **Information** | Key business events | User registered, payment completed, exam submitted |
| **Warning** | Unusual but handled situations | Rate limit approached, slow query >500ms |
| **Error** | Failures requiring investigation | Payment webhook failed, email send failed |
| **Fatal** | Application cannot continue | Database unreachable, out of memory |

### 2.3 Structured Log Examples

```csharp
// DO: Use structured logging with named properties
_logger.LogInformation(
    "Payment completed: {TransactionId} for {UserId}, Amount: {Amount} {Currency} via {Gateway}",
    transaction.Id, transaction.UserId, transaction.Amount, transaction.Currency, transaction.Gateway);

// DON'T: String interpolation (breaks structured search)
_logger.LogInformation(
    $"Payment completed: {transaction.Id} for {transaction.UserId}"); // ❌
```

### 2.4 Correlation IDs

Every request gets a unique `X-Correlation-Id` header, propagated through all logs:

```csharp
public class CorrelationIdMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
            ?? Guid.NewGuid().ToString("N");
        
        context.Response.Headers["X-Correlation-Id"] = correlationId;
        
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}
```

---

## 3. Application Metrics (OpenTelemetry + Prometheus)

### 3.1 Custom Metrics

```csharp
public static class FameMetrics
{
    private static readonly Meter Meter = new("FAME.Api");
    
    // Counters
    public static readonly Counter<long> UserRegistrations =
        Meter.CreateCounter<long>("fame.users.registrations", "count", "Total user registrations");
    
    public static readonly Counter<long> PaymentsCompleted =
        Meter.CreateCounter<long>("fame.payments.completed", "count", "Total completed payments");
    
    public static readonly Counter<long> PaymentsFailed =
        Meter.CreateCounter<long>("fame.payments.failed", "count", "Total failed payments");
    
    public static readonly Counter<long> ExamsSubmitted =
        Meter.CreateCounter<long>("fame.exams.submitted", "count", "Total exam submissions");
    
    public static readonly Counter<long> ExamsPassed =
        Meter.CreateCounter<long>("fame.exams.passed", "count", "Total exams passed");
    
    // Histograms
    public static readonly Histogram<double> ExamScores =
        Meter.CreateHistogram<double>("fame.exams.scores", "percent", "Distribution of exam scores");
    
    public static readonly Histogram<double> PaymentAmounts =
        Meter.CreateHistogram<double>("fame.payments.amounts", "PKR", "Payment amount distribution");
    
    // Gauges
    public static readonly ObservableGauge<int> ActiveEnrollments =
        Meter.CreateObservableGauge<int>("fame.enrollments.active", 
            () => GetActiveEnrollmentCount(), "count", "Currently active enrollments");
    
    public static readonly ObservableGauge<int> ConnectedWebsockets =
        Meter.CreateObservableGauge<int>("fame.websockets.connected",
            () => GetWebsocketCount(), "count", "Current WebSocket connections");
}
```

### 3.2 HTTP Metrics (Built-in)

Automatically collected by ASP.NET Core + OpenTelemetry:

| Metric | Type | Labels |
|--------|------|--------|
| `http_server_request_duration_seconds` | Histogram | method, route, status_code |
| `http_server_active_requests` | Gauge | method, route |
| `http_client_request_duration_seconds` | Histogram | method, host, status_code |

### 3.3 Database Metrics

| Metric | Source | Alert Threshold |
|--------|--------|:---------------:|
| Query duration | EF Core interceptor | P95 > 200ms |
| Connection pool utilization | Npgsql metrics | > 80% |
| Slow queries (>500ms) | Logged via interceptor | Any occurrence |
| Active connections | pg_stat_activity | > 80 |
| Database size | pg_database_size | > 80% disk |

---

## 4. Business Analytics

### 4.1 Analytics Event Model

```csharp
public sealed class AnalyticsEvent : Entity
{
    public string EventType { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; }
    public JsonDocument Properties { get; set; } = JsonDocument.Parse("{}");
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}
```

### 4.2 Tracked Business Events

| Event | Properties | Purpose |
|-------|-----------|---------|
| `user.registered` | source, examTrack | Measure registration rate by source |
| `user.login` | method | Track active usage |
| `course.viewed` | courseId, source | Measure interest |
| `checkout.started` | packageId, gateway | Measure purchase intent |
| `checkout.completed` | packageId, amount, gateway, couponCode | Revenue tracking |
| `checkout.abandoned` | packageId, gateway, step | Identify drop-off points |
| `exam.started` | examId, mode | Usage tracking |
| `exam.submitted` | examId, score, duration | Performance analytics |
| `lecture.completed` | courseId, lectureId, watchDuration | Engagement tracking |
| `certificate.issued` | courseId, userId | Completion tracking |
| `search.performed` | query, resultCount | Content gap analysis |

### 4.3 Admin Analytics Dashboard

```
┌──────────────────────────────────────────────────────────────┐
│  Analytics Dashboard                     [Last 30 days ▼]   │
│                                                              │
│  ┌─────────────┐ ┌─────────────┐ ┌──────────┐ ┌──────────┐ │
│  │ Revenue      │ │ New Students │ │ Exams    │ │ Pass Rate│ │
│  │ PKR 450,000  │ │    342      │ │  1,204   │ │   68%    │ │
│  │ ▲ 12%        │ │ ▲ 8%        │ │ ▲ 15%    │ │ ▲ 3%    │ │
│  └─────────────┘ └─────────────┘ └──────────┘ └──────────┘ │
│                                                              │
│  Revenue Trend                    Top Packages               │
│  ┌────────────────────────┐      ┌──────────────────────┐   │
│  │  ╱╲   ╱╲              │      │ PLAB-1    45%  ████░  │   │
│  │ ╱  ╲ ╱  ╲   ╱╲       │      │ AMC-1     25%  ██░░░  │   │
│  │╱    ╲    ╲ ╱  ╲      │      │ Clinical  18%  █░░░░  │   │
│  │      ╲    ╲    ╲     │      │ NRE-1     7%   ░░░░░  │   │
│  │       ╲        ╲     │      │ NRE-2     3%   ░░░░░  │   │
│  │                       │      │ Basic     2%   ░░░░░  │   │
│  └────────────────────────┘      └──────────────────────┘   │
│                                                              │
│  Student Performance               Conversion Funnel         │
│  ┌────────────────────────┐      ┌──────────────────────┐   │
│  │ Avg Score: 64%         │      │ Visits      10,430   │   │
│  │ By Track:              │      │ Registrations  342   │   │
│  │ PLAB-1:   72% ████░   │      │ Checkout       186   │   │
│  │ AMC-1:    68% ███░░   │      │ Payment        142   │   │
│  │ NRE-1:    58% ██░░░   │      │ Rate: 3.3%    ──►    │   │
│  └────────────────────────┘      └──────────────────────┘   │
└──────────────────────────────────────────────────────────────┘
```

### 4.4 Analytics Queries

```sql
-- Daily revenue (last 30 days)
SELECT DATE(paid_at) AS date, 
       SUM(amount) AS revenue,
       COUNT(*) AS transactions
FROM transactions
WHERE status = 'Completed' 
  AND paid_at >= NOW() - INTERVAL '30 days'
GROUP BY DATE(paid_at)
ORDER BY date;

-- Registration funnel
WITH funnel AS (
    SELECT 
        COUNT(*) FILTER (WHERE event_type = 'user.registered') AS registrations,
        COUNT(*) FILTER (WHERE event_type = 'checkout.started') AS checkouts,
        COUNT(*) FILTER (WHERE event_type = 'checkout.completed') AS payments
    FROM analytics_events
    WHERE timestamp >= NOW() - INTERVAL '30 days'
)
SELECT *, 
       ROUND(payments::decimal / NULLIF(registrations, 0) * 100, 1) AS conversion_rate
FROM funnel;

-- Exam performance by track
SELECT et.name AS exam_track,
       COUNT(*) AS attempts,
       ROUND(AVG(er.score), 1) AS avg_score,
       ROUND(COUNT(*) FILTER (WHERE er.passed) * 100.0 / COUNT(*), 1) AS pass_rate
FROM exam_results er
JOIN exams e ON er.exam_id = e.id
JOIN courses c ON e.course_id = c.id
JOIN exam_tracks et ON c.exam_track_id = et.id
WHERE er.submitted_at >= NOW() - INTERVAL '30 days'
GROUP BY et.name
ORDER BY attempts DESC;
```

---

## 5. Grafana Dashboards

### 5.1 Dashboard Catalog

| Dashboard | Panels | Audience |
|-----------|--------|----------|
| **API Performance** | Request rate, latency percentiles, error rate, top slow endpoints | DevOps |
| **Database** | Query time, connections, Pool usage, table sizes, slow queries | DevOps |
| **Cache (Redis)** | Hit rate, miss rate, memory, evictions, connected clients | DevOps |
| **Infrastructure** | CPU, memory, disk, network I/O, Docker container stats | DevOps |
| **Business KPIs** | Revenue, registrations, exam scores, active users | Admin |

### 5.2 Key Panels

| Panel | PromQL Query | Alert |
|-------|-------------|-------|
| Request Rate | `rate(http_server_request_duration_seconds_count[5m])` | — |
| P95 Latency | `histogram_quantile(0.95, rate(http_server_request_duration_seconds_bucket[5m]))` | > 500ms |
| Error Rate | `rate(http_server_request_duration_seconds_count{status_code=~"5.."}[5m]) / rate(http_server_request_duration_seconds_count[5m])` | > 1% |
| Revenue (24h) | Custom query via API | < expected |
| Active Sessions | `fame_websockets_connected` | — |

---

## 6. Alerting Rules

### 6.1 Infrastructure Alerts

| Alert | Condition | Severity | Channel |
|-------|-----------|:--------:|---------|
| High CPU | > 85% for 5 min | Warning | Email |
| High Memory | > 90% for 5 min | Critical | Email + SMS |
| Disk Space Low | < 15% free | Warning | Email |
| API Down | Health check fails 3× | Critical | Email + SMS |
| Database Down | pg_isready fails 3× | Critical | Email + SMS |

### 6.2 Application Alerts

| Alert | Condition | Severity | Channel |
|-------|-----------|:--------:|---------|
| High Error Rate | 5xx rate > 5% for 2 min | Critical | Email |
| Slow Responses | P95 > 1s for 5 min | Warning | Email |
| Payment Failures | > 5 failed payments in 10 min | Critical | Email + SMS |
| Login Storm | > 100 failed logins/min from IP | Critical | Auto-block + Email |
| Queue Backup | Hangfire queue > 1000 jobs | Warning | Email |

---

## 7. Log Retention & Compliance

| Data Type | Retention | Storage |
|-----------|:---------:|---------|
| Application logs | 30 days | Seq |
| Audit logs | 7 years | PostgreSQL (archived yearly) |
| Analytics events | 2 years | PostgreSQL (partitioned by month) |
| Metrics (Prometheus) | 90 days | Prometheus local storage |
| Error details | 30 days | Seq |

---

*Observability is a first-class concern in V2. Every request is traceable from frontend to database via correlation IDs. Business metrics are collected alongside technical metrics — the admin dashboard shows revenue and engagement, while Grafana shows system health. Alerting ensures issues are detected before users notice.*
