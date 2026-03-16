# 17 — Screenshot Index

---

## Purpose

This document serves as a **screenshot placeholder guide** for the Platform Analysis audit. Each entry identifies a specific page or feature that should be captured as visual evidence. Screenshots can be added to a `screenshots/` subdirectory within this folder and referenced by their ID.

> **Note**: This audit was conducted primarily through static code analysis and configuration review. Screenshots from live authenticated sessions would strengthen the evidence base significantly.

---

## How to Use

1. Create a `screenshots/` folder inside `Platform Analysis/`
2. Capture each screenshot as `SS-XXX_description.png`
3. Update the **Captured** column to ✅ when done

---

## Screenshot Catalog

### Public Pages

| ID | Page / Feature | URL / Route | Priority | Captured |
|----|---------------|-------------|:--------:|:--------:|
| SS-001 | Homepage hero section | `/` | High | ☐ |
| SS-002 | Homepage exam tracks grid | `/` (scroll) | High | ☐ |
| SS-003 | Homepage social counters (0K+ bug) | `/` (scroll) | Critical | ☐ |
| SS-004 | Homepage testimonial carousel | `/` (scroll) | High | ☐ |
| SS-005 | Homepage FAQ section | `/` (scroll) | Medium | ☐ |
| SS-006 | Homepage footer with copyright 2022 | `/` (scroll) | High | ☐ |
| SS-007 | Homepage footer INSTAGARM label | `/` (scroll) | High | ☐ |
| SS-008 | About page (Dr. Atif bio) | `/Home/About` | Medium | ☐ |
| SS-009 | Contact page | `/Home/Contact` | Medium | ☐ |
| SS-010 | **Broken Pricing page (USD)** | `/Home/Pricing` | Critical | ☐ |
| SS-011 | Old Packages page | `/Home/Packages` | Medium | ☐ |
| SS-012 | New OurPackages page | `/Landing/Home/OurPackages` | High | ☐ |
| SS-013 | Terms & Conditions page | `/Home/TermsAndConditions` | Medium | ☐ |
| SS-014 | Privacy Policy page | `/Home/PrivacyPolicy` | Medium | ☐ |
| SS-015 | Blog listing page | `/Blogs/` | Low | ☐ |

### Authentication

| ID | Page / Feature | URL / Route | Priority | Captured |
|----|---------------|-------------|:--------:|:--------:|
| SS-020 | Login page | `/Account/Login` | High | ☐ |
| SS-021 | Registration wizard (step 1) | `/Account/Register` | High | ☐ |
| SS-022 | Registration wizard (CNIC step) | `/Account/Register` (step) | High | ☐ |
| SS-023 | Google OAuth button | `/Account/Login` | Medium | ☐ |
| SS-024 | Forgot password page | `/Account/ForgotPassword` | Low | ☐ |

### Student Portal

| ID | Page / Feature | URL / Route | Priority | Captured |
|----|---------------|-------------|:--------:|:--------:|
| SS-030 | Student dashboard | `/Student/Index` | High | ☐ |
| SS-031 | My Courses grid | `/Student/MyCourses` | High | ☐ |
| SS-032 | Course player (standard mode) | `/Student/TakeCourse/{id}` | High | ☐ |
| SS-033 | Course player (Focus Mode) | `/Student/TakeCourse/{id}` (toggle) | High | ☐ |
| SS-034 | Mock Tests listing | `/Student/MockTests` | Medium | ☐ |
| SS-035 | **MCQ Exam full-screen** | `/Student/TakeExam/{id}` | Critical | ☐ |
| SS-036 | MCQ exam — strike-through feature | `/Student/TakeExam/{id}` | High | ☐ |
| SS-037 | MCQ exam — Lab Values modal | `/Student/TakeExam/{id}` | High | ☐ |
| SS-038 | MCQ exam — keyboard shortcuts | `/Student/TakeExam/{id}` | Medium | ☐ |
| SS-039 | Exam results page | `/Student/ExamResult/{id}` | High | ☐ |
| SS-040 | Exam results — ApexCharts | `/Student/ExamResult/{id}` | Medium | ☐ |
| SS-041 | My Subscriptions | `/Student/MySubscriptions` | Medium | ☐ |
| SS-042 | Student profile | `/Student/Profile` | Medium | ☐ |
| SS-043 | Student support/tickets | Student support views | Medium | ☐ |
| SS-044 | Favourites page | `/Student/Favourites` | Low | ☐ |
| SS-045 | Study Schedule | `/Student/StudySchedule` | Low | ☐ |
| SS-046 | Chat interface | Student chat views | Medium | ☐ |
| SS-047 | Guideline Videos | `/Student/GuidelineVideo` | Low | ☐ |
| SS-048 | Student FAQ page | Student FAQ view | Medium | ☐ |

### Admin Panel

| ID | Page / Feature | URL / Route | Priority | Captured |
|----|---------------|-------------|:--------:|:--------:|
| SS-050 | Admin dashboard | `/Admin/Index` | Medium | ☐ |
| SS-051 | Student list view | `/Admin/StudentList` | Medium | ☐ |
| SS-052 | Student detail (PII visible) | `/Admin/StudentDetail/{id}` | High | ☐ |
| SS-053 | Enrollment management | `/Enrollment/Index` | Medium | ☐ |
| SS-054 | Exam creation wizard (step 1) | `/Admin/CreateExam` | Medium | ☐ |
| SS-055 | Package management | Package admin views | Low | ☐ |
| SS-056 | Billing/Invoice | `/Billing/Index` | Low | ☐ |
| SS-057 | Invoice PDF output | `/Billing/InvoicePDF` | Low | ☐ |
| SS-058 | Report views | Report admin views | Low | ☐ |

### Payment

| ID | Page / Feature | URL / Route | Priority | Captured |
|----|---------------|-------------|:--------:|:--------:|
| SS-060 | JazzCash payment page | `/Enrollment/JazzCash` | Critical | ☐ |
| SS-061 | JazzCash response page | After JazzCash redirect | High | ☐ |

### Certificate

| ID | Page / Feature | URL / Route | Priority | Captured |
|----|---------------|-------------|:--------:|:--------:|
| SS-070 | Certificate verification page | `/Certificate/Verify/{token}` | High | ☐ |
| SS-071 | Certificate template preview | Admin certificate views | Medium | ☐ |

### Error States

| ID | Page / Feature | URL / Route | Priority | Captured |
|----|---------------|-------------|:--------:|:--------:|
| SS-080 | Error page (with stack trace visible) | `/invalid-url` or trigger error | Critical | ☐ |
| SS-081 | 404 page | Non-existent route | Medium | ☐ |

### Mobile Views

| ID | Page / Feature | URL / Route | Priority | Captured |
|----|---------------|-------------|:--------:|:--------:|
| SS-090 | Homepage mobile (375px) | `/` responsive | High | ☐ |
| SS-091 | Student dashboard mobile | `/Student/Index` responsive | High | ☐ |
| SS-092 | MCQ exam mobile | `/Student/TakeExam` responsive | High | ☐ |
| SS-093 | Navigation menu mobile | Hamburger menu expanded | Medium | ☐ |

---

## Priority Summary

| Priority | Count | Description |
|----------|:-----:|-------------|
| Critical | 5 | Must-capture: bugs, security issues, payment |
| High | 20 | Important for audit evidence |
| Medium | 17 | Supplementary context |
| Low | 7 | Optional / nice-to-have |
| **Total** | **49** | |

---

## Capture Guidelines

1. **Browser**: Chrome or Edge, default zoom (100%)
2. **Desktop resolution**: 1920×1080 or 1440×900
3. **Mobile resolution**: 375×812 (iPhone X viewport) for mobile screenshots
4. **Format**: PNG preferred (lossless)
5. **Naming**: `SS-XXX_short_description.png` (e.g., `SS-003_homepage_counters_0k.png`)
6. **Sensitive data**: Redact any real student PII (names, CNICs, emails) before including in the analysis
7. **Annotations**: Use red rectangles/arrows to highlight specific issues where applicable

---

*This index covers 49 screenshots across 8 categories. Critical screenshots should be captured first to support the most urgent findings in the security and brand audits.*
