# 07 — UX/UI Audit

---

## 1. Design System Observations

### 1.1 FAME 2026 Design System

**File**: `Content/fame-student-2026.css` (2,138 lines)  
**Approach**: Mobile-first, CSS custom properties, Bootstrap 5.3 foundation

#### Design Tokens

| Token | Value | Role |
|-------|-------|------|
| `--fame-primary` | `#0F766E` | Clinical Teal — primary brand, trust |
| `--fame-accent` | `#EA580C` | Orange — CTAs, emphasis |
| `--fame-secondary` | `#1E293B` | Professional Slate — text, nav |
| `--fame-success` | `#16A34A` | Green — positive states |
| `--fame-warning` | `#CA8A04` | Yellow — caution |
| `--fame-danger` | `#DC2626` | Red — errors, urgency |
| `--fame-info` | `#0284C7` | Blue — informational |
| `--fame-bg-page` | `#F8FAFC` | Page background |
| `--tap-target-min` | `44px` | WCAG minimum tap target |
| `--card-radius` | `12px` | Consistent border radius |

#### Design Principles (Documented in CSS Comments)
- **Mobile-first** with 992px desktop breakpoint
- **Inter font** via Google Fonts (regular, medium, semibold, bold)
- **"NO PURPLE"** — explicitly banned in design system
- **No glassmorphism** — clean, professional aesthetic
- **`prefers-reduced-motion`** respected — animations disabled for accessibility
- **44px minimum tap target** — meets WCAG 2.5.5 AAA standard

#### Assessment
| Criterion | Rating | Notes |
|-----------|:------:|-------|
| Token-based theming | ✅ Excellent | CSS custom properties enable consistent theming |
| Mobile-first approach | ✅ Excellent | Breakpoints designed for small screens first |
| Medical-appropriate palette | ✅ Excellent | Teal conveys clinical trust; orange CTAs are distinctive |
| Documentation | ✅ Good | CSS comments explain design decisions |
| Consistency | ⚠️ Partial | Only applies to student portal — admin and public pages use different systems |

---

## 2. Visual Consistency Findings

### 2.1 Layout System Fragmentation

The platform currently uses **at least 5 different layout systems**:

| Layout | Used By | Design System | Status |
|--------|---------|---------------|--------|
| `_LayoutStudent2026.cshtml` | Student portal (2026) | FAME 2026 (Bootstrap 5.3 + Inter) | ✅ Active — primary |
| `_LayoutTeacher.cshtml` | Admin/Teacher panel | Metronic Material Design | ✅ Active |
| `_LayoutNew.cshtml` | Legacy student pages | Metronic 8 Bootstrap | ⚠️ Being phased out |
| `_LayoutFree.cshtml` | Free/public pages | Unknown | ⚠️ Legacy |
| `_Layout.cshtml` (Landing) | Landing Area pages | Custom landing theme | ✅ Active |
| No layout / standalone | TakeExam, Login, Register | Self-contained | ✅ Active |

**Finding**: Students experience **at least 3 distinct visual languages** across their journey: landing page theme → login/register (Metronic/standalone) → student portal (2026 design). This creates visual discontinuity.

### 2.2 Specific Inconsistencies

| Inconsistency | Location | Impact |
|---------------|----------|--------|
| **Landing pages use old theme** | `/Landing/Home/` | Different font, colors, spacing than student portal |
| **JazzCash payment uses old layout** | `Enrollment/JazzCash.cshtml` | Visual break during critical payment flow |
| **ContactUs uses `_LayoutNew`** | `Home/ContactUs.cshtml` | Student sees different layout when seeking help |
| **SupportOverView uses 2026 layout** | `Home/SupportOverView.cshtml` | Inconsistent with ContactUs page which links to it |
| **Login page is standalone** | `Account/Login.cshtml` | No header/footer — feels disconnected from platform |
| **Pricing page is old template** | `Home/Pricing.cshtml` | Completely different visual language — USD, Learnly branding |
| **Admin panel is Metronic** | All admin views | Intentional but creates jarring transition for admin users who also use student views |
| **Error page is minimal** | `Shared/Error.cshtml` | No branded error experience — raw layout |

---

## 3. Accessibility Findings

### 3.1 Positive Accessibility Patterns

| Pattern | Evidence | Standard |
|---------|----------|----------|
| **44px minimum tap target** | `--tap-target-min: 44px` in CSS tokens | WCAG 2.5.5 AAA |
| **`prefers-reduced-motion`** | Animations disabled for users who prefer reduced motion | WCAG 2.3.3 |
| **Semantic HTML** | Proper use of `<header>`, `<nav>`, `<main>`, `<section>` in 2026 layout | WCAG 1.3.1 |
| **Mobile viewport** | `viewport-fit=cover` with mobile theme-color | Mobile accessibility |
| **Inter font** | Highly legible sans-serif font at appropriate sizes | Readability |
| **High contrast primary** | Teal `#0F766E` on white provides adequate contrast | WCAG 1.4.3 |
| **Image alt text** | Alt attributes present on landing page images | WCAG 1.1.1 |
| **Avatar fallback** | SVG placeholder with initials when no profile photo | Visual indication |

### 3.2 Accessibility Concerns

| Concern | Impact | WCAG Reference | Severity |
|---------|--------|---------------|----------|
| **No skip navigation link** observed | Keyboard users must tab through entire sidebar | WCAG 2.4.1 | Medium |
| **Color-only status indicators** | Q-dot grid on exam result uses only color to distinguish correct/wrong/skipped | WCAG 1.4.1 | Medium |
| **No visible focus indicators** in custom components | Student sidebar links may lack focus rings | WCAG 2.4.7 | Medium |
| **Disabled double-tap zoom** | JS prevents rapid double-tap — may affect users who need zoom | WCAG 1.4.4 | Medium |
| **No lang attribute** verified on HTML element | Language not declared for screen reader localization | WCAG 3.1.1 | Low |
| **No ARIA landmarks** verified beyond semantic HTML | Custom widgets (sidebar drawer, modals) may lack ARIA roles | WCAG 4.1.2 | Medium |
| **No text resize** testing possible | CSS uses `rem` units (good) but responsive behavior at 200% not verifiable | WCAG 1.4.4 | Not Verified |

### 3.3 Accessibility Rating: 3.0 / 5
The 2026 design system includes several positive accessibility patterns (tap targets, reduced motion, semantic HTML). However, comprehensive accessibility testing requires runtime inspection with assistive technology.

---

## 4. Responsiveness Observations

### 4.1 Student Portal (2026 Layout)

**Breakpoint Strategy**: 992px mobile/desktop transition

| Viewport | Layout Behavior | Evidence |
|----------|----------------|---------|
| **Mobile (<992px)** | Single-column, sidebar as overlay drawer, hamburger menu | `_SidebarPro.cshtml` closes on link click when `< 992px` |
| **Tablet (medium)** | Sidebar overlay, content full-width | Responsive rules in CSS |
| **Desktop (≥992px)** | Fixed sidebar + main content area | `_LayoutStudent2026.cshtml` structure |

**Video Player Responsiveness** (TakeCourse.cshtml):
- Mobile: Full-width video with stacked sidebar below
- Desktop: Video + sidebar side-by-side
- Focus Mode: Sidebar hidden, video maximized

**Exam Engine Responsiveness** (TakeExam.cshtml):
- Mobile: Sidebar collapsed, full-width question area
- Desktop: Fixed sidebar with question dot grid alongside

### 4.2 Responsive Strengths
1. **Mobile-first CSS** — All styles designed for small screens first, then enhanced for desktop
2. **Sidebar drawer pattern** — Proper mobile overlay with touch-friendly toggle
3. **Video player adaptation** — Full-width on mobile, proper ratio maintained
4. **Quick Action cards** — Responsive grid on dashboard

### 4.3 Responsive Concerns

| Concern | Evidence | Severity |
|---------|----------|----------|
| **Double-tap zoom disabled** | JavaScript prevents rapid touch events with `e.preventDefault()` | Medium |
| **Landing pages may not be 2026-responsive** | Landing Area uses different layout/CSS with unknown mobile behavior | Medium |
| **Registration wizard on mobile** | 7-step wizard with multiple file uploads on small screens may be cumbersome | High |
| **Admin panel mobile support** | `_LayoutTeacher.cshtml` (Metronic) — mobile adaptability not verified | Not Verified |

---

## 5. Usability Issues

### 5.1 Form Usability

| Issue | Location | Impact |
|-------|----------|--------|
| **7-step registration wizard** | `Account/Register.cshtml` (1,139 lines) | High barrier to entry; mobile burden; potential high dropout |
| **Custom validation instead of HTML5** | Registration uses `div`-based validation (`#fieldNameVal`) instead of native HTML5 | Less accessible than native browser validation |
| **No inline validation** | Fields validated on step transition, not on blur | Delays error discovery |
| **Password strength meter** | Visual 4-bar meter — good | ✅ Helps users create strong passwords |
| **Select2 dropdowns** | Country selector uses Select2 with search | ✅ Good for long lists |

### 5.2 Navigation Friction

| Issue | Impact | Severity |
|-------|--------|----------|
| **Two About Us pages** accessible | `/Home/AboutUs` and `/Landing/Home/AboutUs` — different content/design | Medium |
| **Two Pricing pages** accessible | `/Home/Pricing` (broken template) and `/Landing/Home/OurPackages` (real) | High |
| **No breadcrumbs** in student portal | Students lose context in deep navigation | Low |
| **Sidebar active state** | Uses `ViewContext.RouteData.Values` for highlighting — may not work for all routes | Medium |

### 5.3 Error States and Feedback

| Aspect | Status | Evidence |
|--------|--------|---------|
| **SweetAlert2** for notifications | ✅ Active | Used for success/error/info messages |
| **Error page** | ❌ Poor | Misspelled "Occord", exposes stack traces |
| **Loading states** | ✅ Good | MockTests, exam data use spinners |
| **Empty states** | ✅ Good | Dashboard and Subscriptions handle no-data gracefully |
| **Form error styles** | ⚠️ Custom | Non-standard validation display |

---

## 6. Trust-Building Observations

### 6.1 Trust Signals Present

| Signal | Location | Effectiveness |
|--------|----------|---------------|
| **Dr. Hafiz Atif photo + credentials** | Landing page hero, About page | ✅ Strong — founder as credibility anchor |
| **Student testimonials** | Landing page carousel | ⚠️ Moderate — photos present but some name/image mismatches noted |
| **University names** | Testimonials mention real universities (Asian Medical Institute, Guangxi University) | ✅ Adds credibility |
| **WhatsApp support line** | Landing page, Contact section | ✅ Accessible — common in Pakistan market |
| **Certificate verification** | `/Certificate/Verify` — public URL | ✅ Strong — verifiable credentials |
| **Free Trial MCQ** | Landing page CTA | ✅ Try-before-buy builds confidence |

### 6.2 Trust Diminishers

| Issue | Impact | Severity |
|-------|--------|----------|
| **Copyright "2022"** | Signals abandoned or stale platform | Medium |
| **"0K+" social counters** | Zero followers/subscribers suggests fake or broken metrics | High |
| **"PolytronX" third-party credit** | Reduces ownership impression | Low |
| **"Keenthemes" in meta tags** | Template origin visible in page source | Low |
| **Medical term misspellings** | "Obstetrucs", "Opthamology" — undermines medical authority | High |
| **"Â" encoding artifacts** | Shows technical immaturity | Medium |
| **Broken Pricing page** | Template content with USD pricing — scam-like impression if found | High |
| **No HTTPS padlock** verification | Cannot confirm HTTPS enforcement from code analysis alone | Not Verified |

---

## 7. First-Time User Experience

### Positive Aspects
1. **Clear CTAs on landing page** — Login, Register, Free Demo, Free Trial MCQ
2. **WhatsApp link** for immediate help
3. **Trial content** available without registration
4. **Email verification** during registration adds legitimacy
5. **Password strength meter** guides secure password creation

### Concerns
1. **7-step registration** is overwhelming for a first-time visitor
2. **CNIC upload requirement** before any service delivery — may deter users unfamiliar with the platform
3. **Manual approval delay** — after completing lengthy registration, user must wait for admin activation
4. **No onboarding tutorial** — first login drops user on dashboard without guidance
5. **No welcome email content** verified — first impression after registration is unclear

---

## 8. UX/UI Ratings Summary

| Dimension | Score | Key Factor |
|-----------|:-----:|-----------|
| Design System Quality | 4/5 | Excellent token system, mobile-first, accessibility-aware |
| Visual Consistency | 2/5 | 5+ layout systems, significant fragmentation |
| Typography | 4/5 | Inter is excellent for readability; consistent sizing in 2026 |
| Color System | 4/5 | Medical-appropriate palette with purposeful accent |
| Accessibility | 3/5 | Good foundation (tap targets, reduced motion) but gaps in ARIA, focus |
| Responsiveness | 3.5/5 | Strong in student portal; unverified in admin and landing |
| Navigation | 2.5/5 | Student sidebar is clean; cross-section navigation is fragmented |
| Form Usability | 2.5/5 | Registration wizard is long; custom validation lacks accessibility |
| Error Handling UX | 2/5 | Error page is unprofessional; form errors are custom not standard |
| Trust & Confidence | 2.5/5 | Founder credibility strong; multiple trust-diminishing details |
| First-Time Experience | 2.5/5 | Free trial is great; registration barrier is high |

**Overall UX/UI Score: 3.0 / 5**

---

*UX/UI assessment is based on code analysis, CSS inspection, and structural review. Runtime usability testing with actual users would provide additional behavioral insights and is recommended as a follow-up activity.*
