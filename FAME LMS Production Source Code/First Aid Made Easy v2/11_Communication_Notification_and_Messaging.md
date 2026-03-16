# 11 — Communication, Notification, and Messaging System

*Email infrastructure, in-app notifications, real-time messaging, announcement system, and communication preferences.*

---

## 1. V1 → V2 Communication Improvements

| Aspect | V1 | V2 |
|--------|----|----|
| **Email** | SMTP via sendmail, inconsistent | Transactional email service (SendGrid/Resend) |
| **Templates** | Inline HTML strings | Mjml → HTML templates, version-controlled |
| **Notifications** | None | Real-time via SignalR, persisted in DB |
| **Chat** | Basic SignalR hub | Structured support chat + group discussions |
| **Announcements** | None | Multi-channel (email + notification + banner) |
| **Preferences** | None | Per-channel, per-category opt-in/out |

---

## 2. Architecture Overview

```
┌────────────────────────────────────────────────────────────┐
│                     Communication Module                    │
│                                                            │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐ │
│  │  Email        │  │ Notification │  │  Chat / Messaging│ │
│  │  Service      │  │  Service     │  │  Service         │ │
│  └──────┬───────┘  └──────┬───────┘  └────────┬─────────┘ │
│         │                 │                    │           │
│  ┌──────▼───────┐  ┌──────▼───────┐  ┌────────▼─────────┐ │
│  │  Template     │  │  SignalR Hub │  │  SignalR Hub     │ │
│  │  Engine       │  │  (notify)    │  │  (chat)          │ │
│  └──────┬───────┘  └──────────────┘  └──────────────────┘ │
│         │                                                  │
│  ┌──────▼───────┐                                         │
│  │  SendGrid /   │                                         │
│  │  Resend API   │                                         │
│  └──────────────┘                                         │
└────────────────────────────────────────────────────────────┘
```

---

## 3. Transactional Email System

### 3.1 Email Templates

| Template ID | Trigger | Subject Line Template | Variables |
|------------|---------|----------------------|-----------|
| `welcome` | Registration confirmed | Welcome to FAME, {firstName}! | firstName, loginUrl |
| `email-verify` | Registration | Verify your email address | firstName, verifyUrl, expiresIn |
| `password-reset` | Forgot password | Reset your FAME password | firstName, resetUrl, expiresIn |
| `enrollment-confirmed` | Payment completed | You're enrolled in {packageName}! | firstName, packageName, courses[], expiresAt |
| `expiry-warning-7d` | 7 days before expiry | Your subscription expires in 7 days | firstName, packageName, expiresAt, renewUrl |
| `expiry-warning-3d` | 3 days before expiry | Only 3 days left on your subscription | firstName, packageName, expiresAt, renewUrl |
| `expiry-warning-1d` | 1 day before expiry | Last day of access — renew now | firstName, packageName, renewUrl |
| `subscription-expired` | Expiry date reached | Your FAME subscription has expired | firstName, packageName, renewUrl |
| `payment-receipt` | Payment completed | Payment receipt — #{invoiceNumber} | firstName, invoiceNumber, amount, currency, date, pdfUrl |
| `exam-result` | Exam submitted | Your {examName} result: {score}% | firstName, examName, score, passed, reviewUrl |
| `certificate-issued` | Certificate generated | Congratulations! Your certificate is ready | firstName, courseName, certificateUrl |
| `ambassador-signup` | Ambassador approved | Welcome to the FAME Ambassador Program | firstName, referralCode, dashboardUrl |
| `commission-earned` | Referral purchase | You earned a commission! | firstName, amount, referralName, dashboardUrl |
| `account-locked` | Too many failed logins | Your account has been locked | firstName, unlockUrl, supportEmail |
| `support-reply` | Admin replies to ticket | Re: {ticketSubject} | firstName, ticketSubject, replyPreview, ticketUrl |

### 3.2 Email Service Implementation

```csharp
public sealed class EmailService : IEmailService
{
    private readonly IEmailProvider _provider;
    private readonly IEmailTemplateEngine _templateEngine;
    private readonly ILogger<EmailService> _logger;
    
    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        // 1. Check user preferences
        var canSend = await CheckUserPreferences(message.UserId, message.Category, ct);
        if (!canSend) return;
        
        // 2. Render template
        var rendered = await _templateEngine.RenderAsync(
            message.TemplateId, 
            message.Variables, ct);
        
        // 3. Send via provider
        var result = await _provider.SendAsync(new ProviderEmail
        {
            To = message.ToEmail,
            Subject = rendered.Subject,
            HtmlBody = rendered.HtmlBody,
            PlainTextBody = rendered.PlainTextBody,
            Tags = new[] { message.TemplateId, message.Category.ToString() },
        }, ct);
        
        // 4. Log for audit
        _logger.LogInformation(
            "Email sent: {TemplateId} to {Email} via {Provider}, MessageId: {MessageId}",
            message.TemplateId, message.ToEmail, _provider.Name, result.MessageId);
    }
}
```

### 3.3 Template Engine

```csharp
public sealed class EmailTemplateEngine : IEmailTemplateEngine
{
    // Templates stored as .mjml files → compiled to HTML at startup
    private readonly IReadOnlyDictionary<string, CompiledTemplate> _templates;
    
    public async Task<RenderedEmail> RenderAsync(
        string templateId, 
        Dictionary<string, object> variables,
        CancellationToken ct)
    {
        if (!_templates.TryGetValue(templateId, out var template))
            throw new InvalidOperationException($"Email template '{templateId}' not found");
        
        // Merge variables into template (using Scriban or similar)
        var html = template.Render(variables);
        var plainText = HtmlToPlainText(html);
        var subject = template.RenderSubject(variables);
        
        return new RenderedEmail(subject, html, plainText);
    }
}
```

---

## 4. In-App Notification System

### 4.1 Notification Data Model

```csharp
public sealed class Notification : Entity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public NotificationCategory Category { get; set; }
    public NotificationPriority Priority { get; set; }
    public string? ActionUrl { get; set; } // Deep link within the app
    public string? ImageUrl { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    public bool IsRead => ReadAt.HasValue;
}

public enum NotificationCategory
{
    Enrollment,
    Payment,
    ExamResult,
    Certificate,
    Announcement,
    Support,
    Subscription,
    System,
}

public enum NotificationPriority
{
    Low,      // Informational
    Medium,   // Action useful
    High,     // Action needed
    Urgent,   // Immediate attention
}
```

### 4.2 Real-Time Delivery via SignalR

```csharp
// Backend: Notification Hub
public sealed class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
        await base.OnConnectedAsync();
    }
}

// Backend: Sending notifications
public sealed class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hub;

    public async Task SendAsync(Notification notification, CancellationToken ct)
    {
        // 1. Persist to database
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(ct);
        
        // 2. Push real-time to connected client
        await _hub.Clients.Group($"user:{notification.UserId}")
            .SendAsync("ReceiveNotification", new
            {
                notification.Id,
                notification.Title,
                notification.Body,
                notification.Category,
                notification.Priority,
                notification.ActionUrl,
                notification.CreatedAt,
            }, ct);
    }
}
```

### 4.3 Frontend Notification Center

```typescript
// hooks/useNotifications.ts
export function useNotifications() {
  const queryClient = useQueryClient();
  const { data: connection } = useSignalR('/hubs/notifications');
  
  useEffect(() => {
    if (!connection) return;
    
    connection.on('ReceiveNotification', (notification: Notification) => {
      // Add to query cache
      queryClient.setQueryData<Notification[]>(
        ['notifications', 'unread'],
        (old = []) => [notification, ...old]
      );
      
      // Show toast for high priority
      if (notification.priority >= NotificationPriority.High) {
        toast({
          title: notification.title,
          description: notification.body,
          action: notification.actionUrl 
            ? { label: 'View', onClick: () => router.push(notification.actionUrl!) }
            : undefined
        });
      }
    });
    
    return () => connection.off('ReceiveNotification');
  }, [connection]);
  
  return useQuery({
    queryKey: ['notifications', 'unread'],
    queryFn: () => api.get<Notification[]>('/api/notifications?unreadOnly=true'),
  });
}
```

### 4.4 Notification Dropdown UI

```
┌──────────────────────────────────┐
│  Notifications (3 new)    [Mark  │
│                           all    │
│                           read]  │
│  ─────────────────────────────── │
│  ● Your PLAB-1 exam result:     │
│    82% — Passed! 🎉             │
│    2 minutes ago            [→]  │
│  ─────────────────────────────── │
│  ● Certificate ready for        │
│    Clinical Pathology            │
│    1 hour ago               [→]  │
│  ─────────────────────────────── │
│  ● Subscription expires in       │
│    3 days — Renew now            │
│    5 hours ago              [→]  │
│  ─────────────────────────────── │
│  ○ Welcome to FAME! Start your  │
│    learning journey              │
│    Yesterday                     │
│  ─────────────────────────────── │
│                                  │
│         [View All →]             │
└──────────────────────────────────┘
```

---

## 5. Announcement System

### 5.1 Announcement Types

| Type | Delivery | Use Case |
|------|----------|----------|
| **Banner** | Sticky bar at top of app | Maintenance, site-wide promotions |
| **Modal** | One-time popup on login | Major feature launches, policy changes |
| **Feed** | In notification feed | General updates, new content |
| **Email** | Bulk email | Marketing campaigns, digest |

### 5.2 Announcement Targeting

```csharp
public sealed class Announcement : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;   // Rich text (Markdown)
    public AnnouncementType Type { get; set; }
    
    // Targeting
    public List<string>? TargetRoles { get; set; }      // Null = all roles
    public List<Guid>? TargetPackageIds { get; set; }   // Null = all packages
    public List<string>? TargetExamTracks { get; set; }  // Null = all tracks
    
    // Scheduling
    public DateTimeOffset? PublishAt { get; set; }       // Null = immediate
    public DateTimeOffset? ExpiresAt { get; set; }       // Null = no expiry
    
    // Display
    public string? ActionUrl { get; set; }
    public string? ActionLabel { get; set; }
    public bool IsDismissible { get; set; } = true;
}
```

---

## 6. Chat / Messaging System

### 6.1 V2 Chat Architecture

```
┌─────────────────────────────────────────────────┐
│                  Chat Module                     │
│                                                 │
│  ┌────────────────┐  ┌───────────────────────┐  │
│  │ Support Tickets │  │ Course Discussions     │  │
│  │ (1:1 or group)  │  │ (per-course channels)  │  │
│  └────────┬───────┘  └───────────┬───────────┘  │
│           │                      │               │
│  ┌────────▼──────────────────────▼───────────┐  │
│  │            SignalR ChatHub                  │  │
│  │  - JoinRoom(roomId)                        │  │
│  │  - SendMessage(roomId, content)            │  │
│  │  - TypingIndicator(roomId)                 │  │
│  └────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────┘
```

### 6.2 Chat Data Model

```csharp
public sealed class ChatRoom : Entity
{
    public string Name { get; set; } = string.Empty;
    public ChatRoomType Type { get; set; }
    public Guid? CourseId { get; set; }               // For course discussions
    public Guid? SupportTicketId { get; set; }        // For support tickets
    public List<ChatRoomMember> Members { get; set; } = new();
    public List<ChatMessage> Messages { get; set; } = new();
}

public enum ChatRoomType
{
    SupportTicket,     // Student ↔ Support agents
    CourseDiscussion,  // All enrolled students + instructor
    DirectMessage,     // 1:1 between any two users
}

public sealed class ChatMessage : Entity
{
    public Guid ChatRoomId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; }  // Text, Image, File
    public string? AttachmentUrl { get; set; }
    public DateTimeOffset? EditedAt { get; set; }
    public bool IsDeleted { get; set; }
}
```

---

## 7. Communication Preferences

### 7.1 User Preference Model

```csharp
public sealed class CommunicationPreference : Entity
{
    public Guid UserId { get; set; }
    public NotificationCategory Category { get; set; }
    public bool EmailEnabled { get; set; } = true;
    public bool InAppEnabled { get; set; } = true;
    public bool PushEnabled { get; set; } = false;  // Future
}
```

### 7.2 Preferences UI

```
┌──────────────────────────────────────────────────────────┐
│  Communication Preferences                               │
│                                                          │
│  Category              Email    In-App                   │
│  ──────────────────────────────────────                  │
│  Enrollment updates    [✓]      [✓]                     │
│  Payment receipts      [✓]      [✓]     (Cannot disable)│
│  Exam results          [✓]      [✓]                     │
│  Certificates          [✓]      [✓]                     │
│  Subscription alerts   [✓]      [✓]                     │
│  Announcements         [✓]      [✓]                     │
│  Support replies       [✓]      [✓]     (Cannot disable)│
│  Marketing             [ ]      [✓]                     │
│                                                          │
│                      [Save Preferences]                  │
└──────────────────────────────────────────────────────────┘
```

Transactional emails (payment receipts, security alerts, support replies) cannot be disabled per CAN-SPAM/GDPR compliance.

---

## 8. Hangfire Jobs for Communication

| Job | Schedule | Purpose |
|-----|----------|---------|
| `ProcessEmailQueue` | Every 30 seconds | Sends queued emails in batches |
| `CheckExpiringSubscriptions` | Daily 6:00 AM | Sends expiry warning emails (7d, 3d, 1d) |
| `ProcessAnnouncementSchedule` | Every 5 minutes | Publishes scheduled announcements |
| `CleanOldNotifications` | Weekly Sunday 2:00 AM | Archives notifications older than 90 days |
| `SendWeeklyDigest` | Weekly Monday 9:00 AM | Sends learning progress summary email |

---

*The communication system is centralized but extensible. Every user-facing event flows through this module, ensuring consistent delivery, preference-respecting, and full audit trail. The SignalR infrastructure supports both notifications and chat with a shared connection.*
