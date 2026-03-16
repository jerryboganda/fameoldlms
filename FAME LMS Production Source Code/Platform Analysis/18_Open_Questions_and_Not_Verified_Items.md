# 18 — Open Questions and Not-Verified Items

---

## Purpose

This document catalogs items that could not be fully verified during the static code/configuration audit. These represent areas where additional access, testing, or information is needed to complete the assessment.

---

## 1. Items Requiring Authenticated Access

| ID | Question | Why It Matters | Verification Method |
|----|----------|---------------|-------------------|
| OQ-01 | **What is the actual registration-to-enrollment conversion rate?** | Quantifies the revenue impact of the manual enrollment bottleneck | Analytics data or database query on registrations vs. active enrollments |
| OQ-02 | **Does the student dashboard load correctly for all roles?** | Multiple layout transitions may have left some role-specific views broken | Log in as each role (Student, Teacher, Admin, SuppAgent) and verify |
| OQ-03 | **Is the chat system (SignalR) operational in production?** | Real-time chat is a key support channel | Authenticated test with two accounts |
| OQ-04 | **Are Firebase push notifications being delivered?** | FCM is configured but delivery success is unknown | Check FCM console stats or trigger a test notification |
| OQ-05 | **Is the device management system actively enforcing limits?** | Claimed device limit control but enforcement logic is in compiled DLL | Try logging in from multiple devices as a student |
| OQ-06 | **Are email notifications actually being sent and delivered?** | Gmail SMTP is configured but deliverability, spam rates unknown | Check Gmail sent folder or use email deliverability test |
| OQ-07 | **What does the student see immediately after registration?** | 7-step wizard outcome — are they in a "pending" state or redirected? | Complete registration and observe |
| OQ-08 | **Is the installment payment system actually being used?** | Views exist but active usage is unknown | Query tbl_Installment or similar table for recent records |

---

## 2. Items Requiring Database Access

| ID | Question | Why It Matters | Query / Check |
|----|----------|---------------|--------------|
| OQ-10 | **How many active students are there currently?** | Validates the "50K+" claim on homepage | `SELECT COUNT(*) FROM AspNetUsers WHERE EmailConfirmed = 1` |
| OQ-11 | **How many exams have been taken?** | Quantifies assessment engine usage | `SELECT COUNT(*) FROM tbl_ExamAttempt` or similar |
| OQ-12 | **How many courses and video lectures exist?** | Content volume baseline | Count rows in tbl_Course, tbl_Section, tbl_Video |
| OQ-13 | **Is the coupon system table populated?** | Determines if coupons were ever active or always disabled | `SELECT COUNT(*) FROM tbl_Coupon` (if exists) |
| OQ-14 | **How many ambassador referrals have been tracked?** | Measures ambassador program adoption | `SELECT COUNT(*) FROM tbl_AmbassadorReferral` |
| OQ-15 | **What is the JazzCash transaction history?** | Determines if online payments ever processed successfully | Check JazzCash-related transaction table |
| OQ-16 | **Are there orphaned image references in the database?** | 1000+ images — some may not be referenced | Cross-reference image filenames with DB records |
| OQ-17 | **How many support tickets are open vs. resolved?** | Measures support responsiveness | Query ticket tables for status distribution |

---

## 3. Items Requiring Server/Infrastructure Access

| ID | Question | Why It Matters | Verification Method |
|----|----------|---------------|-------------------|
| OQ-20 | **Is HTTPS actually enforced via IIS?** | Web.config doesn't enforce it, but IIS binding or load balancer may | Check IIS bindings and SSL certificate status |
| OQ-21 | **What IIS security settings are configured?** | Some security hardening may be at IIS level | Review IIS configuration manager |
| OQ-22 | **Is there a WAF or DDoS protection?** | Cloudflare, Azure WAF, or similar may exist upstream | Check DNS records and hosting configuration |
| OQ-23 | **What is the server's OS and patch level?** | Affects overall security posture | `systeminfo` or Windows Update history |
| OQ-24 | **Are automatic backups running?** | Manual Backups/ folder exists but automated schedule unknown | Check Windows Task Scheduler and SQL Agent jobs |
| OQ-25 | **What is the server's available disk space?** | 1000+ images accumulating; FAME_DB.bak in repo | `Get-PSDrive C` or similar |
| OQ-26 | **Is SQL Server patched to current version?** | Affects database security | `SELECT @@VERSION` |
| OQ-27 | **Are there any other applications on this server?** | Shared hosting increases attack surface | Check IIS sites and running services |

---

## 4. Items Requiring Business/Stakeholder Input

| ID | Question | Why It Matters | Who to Ask |
|----|----------|---------------|-----------|
| OQ-30 | **What is the intended primary payment method?** | JazzCash appears sandbox; manual may be intentional | Product Owner / Dr. Atif |
| OQ-31 | **Is there a mobile app, and what is its status?** | App Store/Play Store links visible but app quality unknown | Product Owner |
| OQ-32 | **What is the target student capacity?** | Determines whether current architecture can scale | Product Owner |
| OQ-33 | **Are there plans for international expansion?** | USD pricing exists but no international payment pathway | Product Owner |
| OQ-34 | **What happened to the coupon system — was it deliberately disabled?** | Understanding intent helps prioritize re-enablement | Product Owner |
| OQ-35 | **Who manages the production server day-to-day?** | Identifies the operations bottleneck | Product Owner |
| OQ-36 | **Is there a separate staging/test environment?** | Affects deployment risk and testing strategy | Technical Team |
| OQ-37 | **What is the student onboarding SLA (expected time to access)?** | Calibrates whether manual enrollment delay is acceptable | Product Owner |
| OQ-38 | **Are the Zoom live classes actively running?** | Zoom SDK integrated but class schedule unknown | Product Owner / Teachers |
| OQ-39 | **What is the PolytronX relationship — ongoing or completed?** | Determines whether external dev support is available | Product Owner |

---

## 5. Items Requiring External Testing

| ID | Question | Why It Matters | Testing Method |
|----|----------|---------------|---------------|
| OQ-40 | **What is the actual page load time from Pakistan?** | Performance audit was code-based; real latency unknown | WebPageTest from Lahore/Karachi nodes |
| OQ-41 | **What is the Lighthouse score?** | Industry-standard performance/accessibility metric | Chrome DevTools Lighthouse audit |
| OQ-42 | **What security headers are actually returned?** | Web.config analysis may not reflect IIS overrides | securityheaders.com scan |
| OQ-43 | **Is the SSL certificate valid and complete?** | HTTPS configuration unclear from code alone | ssllabs.com/ssltest |
| OQ-44 | **What does Google Search Console show?** | Indexing issues, crawl errors, search performance | Google Search Console access |
| OQ-45 | **What does the mobile experience actually look like?** | CSS is responsive but actual rendering unknown | Real device testing (Android + iOS) |
| OQ-46 | **What email deliverability rate do notifications achieve?** | Gmail SMTP may have sender reputation issues | Email deliverability test (mail-tester.com) |
| OQ-47 | **Is the site vulnerable to common OWASP attacks?** | Static analysis suggests vulnerabilities but no penetration test done | OWASP ZAP or Burp Suite automated scan |

---

## 6. Assumptions Made During This Audit

These assumptions were necessary to complete the analysis. They should be validated:

| # | Assumption | Confidence | If Wrong |
|---|-----------|:----------:|----------|
| A-01 | JazzCash integration is not actively processing real payments | 80% | Revenue assessment would change; security risk even higher |
| A-02 | Manual enrollment is the primary pathway | 85% | Self-service checkout gap may be less critical |
| A-03 | Student count is significantly less than 50K+ | 60% | Scale concerns would increase if truly 50K+ |
| A-04 | The server runs on a single machine (no load balancer) | 75% | Performance assessment would need revision |
| A-05 | There is no WAF/CDN in front of the application | 70% | Security assessment would improve if WAF exists |
| A-06 | Dr. Atif is the primary admin and system operator | 80% | Operational bottleneck assessment would change with larger team |
| A-07 | The compiled DLLs have not been decompiled for this audit | 100% | Controller-level security verification would be possible |
| A-08 | Email marketing system is partially implemented | 75% | May be more complete than code suggests |
| A-09 | Debug mode is active in production (not just source) | 90% | If production has debug="false" via IIS override, severity reduces |
| A-10 | No CI/CD pipeline exists | 90% | If one exists outside the codebase, deployment risk reduces |

---

## 7. Recommended Next Steps

### Priority 1: Quick Verifications (< 1 hour)
1. Run `securityheaders.com` scan on `https://firstaidmadeeasy.com.pk/`
2. Run `ssllabs.com/ssltest` on the domain
3. Check Google PageSpeed Insights for real performance data
4. Verify whether debug="true" is in production Web.config
5. Test CORS headers with a simple `fetch()` from another domain

### Priority 2: Authenticated Testing (2-4 hours)
6. Log in as Student role — verify dashboard, course, exam flows
7. Log in as Admin role — verify enrollment, billing, reports
8. Test registration flow end-to-end
9. Submit a support ticket and verify the workflow
10. Attempt the JazzCash payment flow

### Priority 3: Infrastructure Review (1-2 hours)
11. Check IIS bindings (HTTP and HTTPS)
12. Check IIS security configuration
13. Verify SQL Server version and access controls
14. Check Windows Firewall rules
15. Review Windows Event Logs for application errors

---

*This document identifies 47 open questions and 10 assumptions. Addressing the Priority 1 verifications would significantly increase audit confidence with minimal effort.*
