# 21 — API Design and Endpoint Specification

*RESTful API conventions, complete endpoint catalog by module, request/response schemas, versioning, and error format.*

---

## 1. API Conventions

| Convention | Rule |
|-----------|------|
| **Base URL** | `https://api.firstaidmadeeasy.com.pk/api` or `/api` via Caddy proxy |
| **Versioning** | URL prefix: `/api/v1/...` (add v2 when breaking changes required) |
| **Naming** | Plural nouns, kebab-case: `/api/v1/exam-tracks` |
| **HTTP methods** | GET (read), POST (create), PUT (full update), PATCH (partial), DELETE |
| **Status codes** | 200 OK, 201 Created, 204 No Content, 400 Bad Request, 401, 403, 404, 409, 422, 429, 500 |
| **Request body** | JSON (`application/json`) |
| **Pagination** | Keyset: `?after={id}&pageSize=20` |
| **Filtering** | Query params: `?examTrack=plab-1&isPublished=true` |
| **Sorting** | `?sort=createdAt:desc` |
| **Search** | `?q=cardiology` (delegates to Meilisearch) |
| **Date format** | ISO 8601: `2026-03-15T10:30:00Z` |

---

## 2. Standard Response Envelope

```json
// Success (single item)
{
  "data": { ... },
  "meta": null
}

// Success (list)
{
  "data": [ ... ],
  "meta": {
    "pageSize": 20,
    "hasMore": true,
    "nextCursor": "01926a4f-..."
  }
}

// Error
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "One or more validation errors occurred.",
    "details": [
      { "field": "email", "message": "Email is required." },
      { "field": "password", "message": "Password must be at least 10 characters." }
    ]
  }
}
```

---

## 3. Endpoint Catalog by Module

### 3.1 Identity Module

| Method | Endpoint | Auth | Description |
|:------:|----------|:----:|-------------|
| POST | `/api/v1/auth/register` | Public | Register new user |
| POST | `/api/v1/auth/login` | Public | Login (returns JWT in cookie) |
| POST | `/api/v1/auth/logout` | User | Logout (clear cookies, revoke refresh) |
| POST | `/api/v1/auth/refresh` | Cookie | Refresh access token |
| POST | `/api/v1/auth/forgot-password` | Public | Send password reset email |
| POST | `/api/v1/auth/reset-password` | Public | Reset password with token |
| POST | `/api/v1/auth/verify-email` | Public | Verify email with token |
| GET | `/api/v1/users/me` | User | Get current user profile |
| PUT | `/api/v1/users/me` | User | Update current user profile |
| PUT | `/api/v1/users/me/password` | User | Change password |
| PUT | `/api/v1/users/me/avatar` | User | Upload avatar |
| GET | `/api/v1/admin/users` | Admin | List all users (paginated) |
| GET | `/api/v1/admin/users/{id}` | Admin | Get user detail |
| PUT | `/api/v1/admin/users/{id}` | Admin | Update user |
| PUT | `/api/v1/admin/users/{id}/roles` | Admin | Assign roles |
| POST | `/api/v1/admin/users/{id}/suspend` | Admin | Suspend user |
| POST | `/api/v1/admin/users/{id}/activate` | Admin | Activate user |

### 3.2 Catalog Module

| Method | Endpoint | Auth | Description |
|:------:|----------|:----:|-------------|
| GET | `/api/v1/exam-tracks` | Public | List all exam tracks |
| GET | `/api/v1/exam-tracks/{slug}` | Public | Get exam track with courses |
| GET | `/api/v1/courses` | Public | List published courses |
| GET | `/api/v1/courses/{slug}` | Public | Course detail (public info) |
| GET | `/api/v1/courses/{id}/content` | Enrolled | Full content (modules, chapters, lectures) |
| GET | `/api/v1/courses/{id}/lectures/{lectureId}` | Enrolled | Lecture detail + video URL |
| GET | `/api/v1/search?q={query}` | Public | Search courses (Meilisearch) |
| POST | `/api/v1/admin/courses` | ContentEditor | Create course |
| PUT | `/api/v1/admin/courses/{id}` | ContentEditor | Update course |
| DELETE | `/api/v1/admin/courses/{id}` | ContentEditor | Soft-delete course |
| POST | `/api/v1/admin/courses/{id}/modules` | ContentEditor | Add module |
| POST | `/api/v1/admin/courses/{id}/lectures` | ContentEditor | Add lecture |
| PUT | `/api/v1/admin/lectures/{id}` | ContentEditor | Update lecture |
| POST | `/api/v1/admin/upload/image` | ContentEditor | Upload image to S3 |
| POST | `/api/v1/admin/upload/video` | ContentEditor | Upload video to S3 |

### 3.3 Assessment Module

| Method | Endpoint | Auth | Description |
|:------:|----------|:----:|-------------|
| GET | `/api/v1/exams` | Student | List available exams |
| GET | `/api/v1/exams/{id}` | Student | Exam detail + config |
| POST | `/api/v1/exams/{id}/start` | Student | Start exam (returns questions) |
| POST | `/api/v1/exams/{id}/submit` | Student | Submit answers, get result |
| GET | `/api/v1/exams/{id}/result` | Student | Get exam result + review |
| GET | `/api/v1/exams/history` | Student | Exam attempt history |
| GET | `/api/v1/revision/daily` | Student | Get daily revision queue (SR) |
| POST | `/api/v1/revision/review` | Student | Submit revision answer |
| GET | `/api/v1/admin/questions` | AcadMgr | List questions (paginated) |
| POST | `/api/v1/admin/questions` | AcadMgr | Create question |
| PUT | `/api/v1/admin/questions/{id}` | AcadMgr | Update question |
| DELETE | `/api/v1/admin/questions/{id}` | AcadMgr | Soft-delete question |
| POST | `/api/v1/admin/questions/import` | AcadMgr | CSV bulk import |
| GET | `/api/v1/admin/exams` | AcadMgr | List all exam configs |
| POST | `/api/v1/admin/exams` | AcadMgr | Create exam config |
| PUT | `/api/v1/admin/exams/{id}` | AcadMgr | Update exam config |

### 3.4 Payment Module

| Method | Endpoint | Auth | Description |
|:------:|----------|:----:|-------------|
| GET | `/api/v1/packages` | Public | List active packages |
| GET | `/api/v1/packages/{slug}` | Public | Package detail |
| POST | `/api/v1/checkout` | Student | Initiate checkout → redirect URL |
| POST | `/api/v1/coupons/validate` | Student | Validate coupon code |
| POST | `/api/v1/webhooks/jazzcash` | System | JazzCash payment callback |
| POST | `/api/v1/webhooks/easypaisa` | System | Easypaisa payment callback |
| POST | `/api/v1/webhooks/stripe` | System | Stripe webhook |
| GET | `/api/v1/invoices` | Student | List user's invoices |
| GET | `/api/v1/invoices/{id}/pdf` | Student | Download invoice PDF |
| GET | `/api/v1/admin/transactions` | Finance | List all transactions |
| GET | `/api/v1/admin/transactions/{id}` | Finance | Transaction detail |
| POST | `/api/v1/admin/transactions/{id}/refund` | Finance | Process refund |
| GET | `/api/v1/admin/packages` | Admin | List packages (including inactive) |
| POST | `/api/v1/admin/packages` | Admin | Create package |
| PUT | `/api/v1/admin/packages/{id}` | Admin | Update package |
| GET | `/api/v1/admin/coupons` | Admin | List coupons |
| POST | `/api/v1/admin/coupons` | Admin | Create coupon |
| PUT | `/api/v1/admin/coupons/{id}` | Admin | Update coupon |

### 3.5 Enrollment Module

| Method | Endpoint | Auth | Description |
|:------:|----------|:----:|-------------|
| GET | `/api/v1/enrollments` | Student | List active enrollments |
| GET | `/api/v1/enrollments/{id}` | Student | Enrollment detail |
| POST | `/api/v1/enrollments/{id}/cancel` | Student | Cancel enrollment |
| GET | `/api/v1/progress` | Student | Overall learning progress |
| GET | `/api/v1/progress/courses/{courseId}` | Student | Course progress detail |
| POST | `/api/v1/progress/lectures/{lectureId}/complete` | Student | Mark lecture completed |
| GET | `/api/v1/admin/enrollments` | Admin | List all enrollments |
| POST | `/api/v1/admin/enrollments` | Admin | Manually create enrollment |
| PUT | `/api/v1/admin/enrollments/{id}` | Admin | Update enrollment |

### 3.6 Communication Module

| Method | Endpoint | Auth | Description |
|:------:|----------|:----:|-------------|
| GET | `/api/v1/notifications` | User | List notifications |
| PUT | `/api/v1/notifications/{id}/read` | User | Mark notification as read |
| PUT | `/api/v1/notifications/read-all` | User | Mark all as read |
| GET | `/api/v1/preferences/communication` | User | Get communication preferences |
| PUT | `/api/v1/preferences/communication` | User | Update preferences |
| GET | `/api/v1/admin/announcements` | Admin | List announcements |
| POST | `/api/v1/admin/announcements` | Admin | Create announcement |
| PUT | `/api/v1/admin/announcements/{id}` | Admin | Update announcement |
| DELETE | `/api/v1/admin/announcements/{id}` | Admin | Delete announcement |

### 3.7 Certificate Module

| Method | Endpoint | Auth | Description |
|:------:|----------|:----:|-------------|
| GET | `/api/v1/certificates` | Student | List earned certificates |
| GET | `/api/v1/certificates/{id}/pdf` | Student | Download certificate PDF |
| GET | `/api/v1/certificates/verify/{code}` | Public | Verify certificate authenticity |
| GET | `/api/v1/admin/certificates` | Admin | List all certificates |
| POST | `/api/v1/admin/certificates/issue` | Admin | Manually issue certificate |
| GET | `/api/v1/admin/certificate-templates` | Admin | List templates |
| POST | `/api/v1/admin/certificate-templates` | Admin | Create template |

### 3.8 Ambassador Module

| Method | Endpoint | Auth | Description |
|:------:|----------|:----:|-------------|
| POST | `/api/v1/ambassador/apply` | Student | Apply for ambassador role |
| GET | `/api/v1/ambassador/dashboard` | Ambassador | Dashboard stats |
| GET | `/api/v1/ambassador/referrals` | Ambassador | List referrals |
| GET | `/api/v1/ambassador/earnings` | Ambassador | Earnings & payouts |
| POST | `/api/v1/ambassador/referral-link` | Ambassador | Generate referral link |
| GET | `/api/v1/admin/ambassadors` | Admin | List all ambassadors |
| PUT | `/api/v1/admin/ambassadors/{id}/approve` | Admin | Approve application |
| PUT | `/api/v1/admin/ambassadors/{id}/reject` | Admin | Reject application |
| POST | `/api/v1/admin/ambassadors/{id}/payout` | Admin | Process payout |

### 3.9 Analytics Module

| Method | Endpoint | Auth | Description |
|:------:|----------|:----:|-------------|
| GET | `/api/v1/admin/analytics/revenue` | Admin | Revenue dashboard data |
| GET | `/api/v1/admin/analytics/users` | Admin | User growth metrics |
| GET | `/api/v1/admin/analytics/courses` | Admin | Course engagement metrics |
| GET | `/api/v1/admin/analytics/exams` | Admin | Exam performance analytics |
| GET | `/api/v1/admin/analytics/export` | Admin | Export analytics CSV |

### 3.10 System Endpoints

| Method | Endpoint | Auth | Description |
|:------:|----------|:----:|-------------|
| GET | `/health` | Public | Health check (API + DB + Redis) |
| GET | `/health/ready` | Public | Readiness check |
| GET | `/health/live` | Public | Liveness check |
| GET | `/metrics` | Internal | Prometheus metrics |

---

## 4. Endpoint Count Summary

| Module | Public | Student | Admin | System | Total |
|--------|:------:|:-------:|:-----:|:------:|:-----:|
| Identity | 7 | 4 | 6 | — | 17 |
| Catalog | 4 | 2 | 7 | — | 13 |
| Assessment | — | 7 | 7 | — | 14 |
| Payment | 2 | 3 | 9 | 3 | 17 |
| Enrollment | — | 4 | 3 | — | 7 |
| Communication | — | 4 | 4 | — | 8 |
| Certificate | 1 | 2 | 3 | — | 6 |
| Ambassador | — | 5 | 3 | — | 8 |
| Analytics | — | — | 5 | — | 5 |
| System | 3 | — | — | 1 | 4 |
| **Total** | **17** | **31** | **47** | **4** | **99** |

---

## 5. API Authentication Flow

```
Public endpoints → No auth required
                   
Student endpoints → JWT in httpOnly cookie
                    → Middleware extracts claims
                    → Policy checks role + enrollment

Admin endpoints → JWT in httpOnly cookie
                  → Middleware extracts claims
                  → Policy checks admin role

Webhook endpoints → No JWT
                    → HMAC signature verification
                    → IP allow-list (optional)
```

---

*This API specification covers all 99 endpoints across 10 modules. Each endpoint has clear authorization requirements, follows REST conventions, and returns standardized response envelopes. The API is designed to be fully consumed by the Next.js frontend.*
