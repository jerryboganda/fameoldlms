# 01 — Platform Profile

## First Aid Made Easy (FAME) LMS

---

## 1. Platform Description

| Attribute | Detail |
|-----------|--------|
| **Full Name** | First Aid Made Easy (FAME) |
| **URL** | https://firstaidmadeeasy.com.pk/ |
| **Type** | Medical Exam Preparation LMS |
| **Founded By** | Dr. Hafiz Atif |
| **Tagline** | "Pakistan's First website having complete Medical lectures library in Urdu/Hindi" |
| **Core Promise** | "Pass in 1st attempt!" for major medical licensing exams |
| **Physical Address** | Mall Road, Lahore, Punjab, Pakistan (per website footer) |
| **Jurisdiction** | Rawalpindi, Pakistan (per Terms & Conditions) |
| **Support Contact** | +92 (318) 0049-742 (WhatsApp), info@firstaidmadeeasy.com.pk |
| **Copyright** | 2022 (displayed on website — outdated) |
| **Technology Partner** | PolytronX — "Business Digitalized" (credited in footer) |

---

## 2. Target Audience

### Primary Audience
- **Medical students** studying abroad (particularly in Kyrgyzstan, China, and Central Asia)
- **Medical graduates** preparing for licensing exams to practice in Pakistan, UAE, UK, USA, Australia, India
- **Demographic**: Primarily South Asian (Pakistan, India, Nepal, Bangladesh, Afghanistan)
- **Language preference**: Urdu/Hindi medium instruction

### Secondary Audience
- University-level medical institutions (UniTeacher role exists)
- Medical education collaborators (Collaboration page exists)

### User Roles (System-Defined)
| Role | Portal | Purpose |
|------|--------|---------|
| Student | Student Portal (2026 design) | Primary learner experience |
| Admin | Admin Panel (Metronic theme) | Full platform management |
| Teacher | Teacher Panel | Course and content management |
| Assistant | Admin Panel (limited) | Support operations |
| UniTeacher | Admin Panel | University instructor access |
| SuppAgent | Agent Portal | Support ticket handling |

---

## 3. Exams and Tracks Served

The platform covers 12+ major medical licensing examinations:

| Exam | Full Name | Region |
|------|-----------|--------|
| **FCPS-1** | Fellow of College of Physicians and Surgeons (Part 1) | Pakistan |
| **MD/MS-1** | Doctor of Medicine / Master of Surgery (Part 1) | Pakistan |
| **NRE-1** | National Registration Examination (Part 1) | Pakistan (PMC) |
| **NRE-2** | National Registration Examination (Part 2 – OSCE) | Pakistan (PMC) |
| **AMC-1** | Australian Medical Council Exam (Part 1) | Australia |
| **USMLE** | United States Medical Licensing Examination | USA |
| **PLAB** | Professional and Linguistic Assessments Board | UK |
| **UKMLA** | UK Medical Licensing Assessment | UK |
| **HAAD** | Health Authority Abu Dhabi | UAE |
| **MOH** | Ministry of Health | UAE / Gulf States |
| **DHA** | Dubai Health Authority | UAE |
| **NEET PG** | National Eligibility cum Entrance Test (Postgraduate) | India |
| **MRCS** | Membership of the Royal College of Surgeons | UK |

---

## 4. Product Model

### Core Product: Subscription-Based Video + MCQ Platform

| Component | Description |
|-----------|-------------|
| **Video Lectures** | ~20 medical textbooks covered via video lectures in Urdu/Hindi |
| **MCQ Bank** | 77,708+ question options, USMLE-style interface |
| **Mock Tests** | Timed exam simulations |
| **Free Trial** | Trial MCQ test available without registration |
| **Live Classes** | Zoom integration for interactive sessions |
| **Study Materials** | PDF notes, handouts, documents |
| **Certificates** | Completion certificates with public verification |
| **Progress Tracking** | Per-course and per-video watched/pending status |

### Content Library (Based on Textbooks)
1. First Aid Step-1 & Step-2 CK
2. BRS Physiology
3. Pathoma General Pathology
4. Kaplan Surgery
5. Short Snell Anatomy
6. ENT High-Yield
7. Ophthalmology Clinical
8. Obstetrics & Gynaecology (UWorld QBank, Ten Teachers)
9. FCPS Past Papers / Pearls
10. NRE Made Easy Books Vol. 1 & 2
11. AMC Made Easy Lectures
12. PMC Mock Tests
13. SK-18, 19, 20 & 21 Past Papers
14. Rafi Points
15. Double A Notes
16. Wahab Dogar Surgery (Systematic & General)
17. COVID-19 Videos
18. OSCE/NRE-2 Courses
19. AMC-1 Recalls
20. Anatomy Shelf Notes

---

## 5. Delivery Model

| Aspect | Approach |
|--------|----------|
| **Content Delivery** | On-demand video lectures via YouTube embed + internal player |
| **Assessment** | Self-paced MCQ exams + timed mock tests with instant results |
| **Live Interaction** | Scheduled Zoom classes with admin-managed meeting creation |
| **Communication** | Real-time SignalR chat (1:1 and group), support tickets |
| **Supports** | Guideline videos, FAQs, tutorials, WhatsApp support line |
| **Platform Access** | Web-based (responsive) — no native mobile app detected |
| **Device Limits** | 2 devices per account (configurable, admin-managed) |
| **Session Duration** | 6000-minute cookie timeout (~4.2 days) |

---

## 6. Access Model

### Subscription Tiers (Configuration-Observed)

| Package | Duration | Price (PKR) |
|---------|----------|-------------|
| Pack 22 | 30 days | Rs. 16,000 |
| Pack 22 | 15 days | Rs. 8,000 |
| Pack 33 | 30 days | Rs. 4,000 |
| Book Pack | 180 days | Not specified |
| AMC-1 | Variable | PKR + USD pricing |

### Access Flow
1. Student registers via multi-step wizard (7 steps)
2. Email verification (6-digit code)
3. Package selection during registration
4. Payment via JazzCash or manual transfer
5. Admin manually activates enrollment (observed workflow)
6. Student gains access to subscribed course content
7. Subscription expires based on package duration

### Payment Methods (Observed)
- **JazzCash** — 3DS payment page exists (appears sandbox/development quality)
- **Manual Transfer** — Registration request submitted, admin processes enrollment
- **Installment Plans** — Supported via admin panel

---

## 7. Platform Ecosystem Overview

### Core LMS Application
- **Framework**: ASP.NET MVC 5.2.9 / .NET Framework 4.7.2
- **ORM**: Entity Framework 6.4.4 (Database-First + Code-First hybrid, 5 contexts)
- **Database**: SQL Server (`FAME_DB`) — 66+ tables, 51 stored procedures
- **Frontend**: jQuery + Razor Views + Bootstrap 5.3 + Metronic 8 Theme
- **Real-time**: SignalR for chat and notifications
- **DI**: Autofac 6.0.0
- **Auth**: ASP.NET Identity (Cookie + OAuth — Google, Facebook)

### Companion Applications
| Application | Technology | Purpose |
|-------------|-----------|---------|
| **MCQ Portal** | Next.js 15 | Standalone MCQ practice portal (port 3005) |
| **Ambassador Program** | ASP.NET Area | Referral/commission system with separate portal |
| **Certificate System** | ASP.NET Area | Certificate issuance, management, and public verification |
| **Email Marketing** | ASP.NET Module | Campaign management, drip automations, audience segmentation |
| **Daily Report System** | ASP.NET Area | Staff daily reporting with separate auth |
| **File Manager** | ASP.NET Area | Admin and student file management |
| **Blog System** | ASP.NET Area | Content marketing blog |

### External Service Integrations
| Service | Purpose |
|---------|---------|
| Zoom SDK | Live class creation and management |
| Google OAuth | Social login |
| Facebook OAuth | Social login |
| Gmail SMTP | Transactional email delivery |
| Firebase FCM | Push notifications |
| Google Gemini AI | MCQ parsing from PDF (AI-powered import) |
| Google Vision API | OCR for CNIC document reading |
| JazzCash | Online payment processing |
| SendPK / Lifetime SMS / Mocean | SMS notifications |
| Aspose.Words | Document processing |
| EPPlus | Excel report generation |
| Rotativa | PDF generation (invoices, reports) |
| QRCoder | QR code generation |
| TinyMCE | Rich text editing |
| SweetAlert2 | User notifications |

### Background Services
- **EmailSenderService** — Processes outbound email queue
- **NotificationSenderService** — Push notification delivery
- **EmailCampaignSchedulerService** — Scheduled email campaigns

---

## 8. Platform Scale Indicators

| Metric | Observable Value | Source |
|--------|-----------------|--------|
| Registered Users | Not verified — requires DB access | — |
| MCQ Options | 77,708+ | PROJECT_SSOT.md |
| Database Tables | 66+ | EDMX model analysis |
| Stored Procedures | 51 | Database schema |
| View Files | ~301 | Production file system |
| MVC Controllers | 45 | Compiled in DLL |
| BLL Services | 63+ | Source code analysis |
| Deployment Frequency | ~15 deployments/month | DLL backup history (Feb–Mar 2026) |
| Content Coverage | ~20 medical textbooks | Landing page listing |
| Exam Tracks | 12+ | Homepage hero section |

---

*This profile is based on observable evidence from source code, production configuration, database schema scripts, and public website content. Metrics marked "Not verified" require database query access for confirmation.*
