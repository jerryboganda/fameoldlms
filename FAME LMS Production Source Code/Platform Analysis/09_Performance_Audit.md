# 09 — Performance Audit

---

## 1. Performance Observations

> **Note**: This audit is based on static code analysis and configuration review. No runtime benchmarking, load testing, or Lighthouse audits were conducted. Performance assessment is limited to observable patterns, resource strategies, and configuration indicators.

---

## 2. Frontend Performance Indicators

### 2.1 Resource Loading Strategy

| Aspect | Implementation | Assessment |
|--------|---------------|-----------|
| **CSS Delivery** | Bootstrap 5.3 via CDN + custom `fame-student-2026.css` with cache-busting | ⚠️ Mixed — CDN is fast but cache-busting via `DateTime.Now.Ticks` prevents browser caching |
| **Font Loading** | Google Fonts (Inter) with `preconnect` hint | ✅ Good — preconnect reduces connection latency |
| **JavaScript** | jQuery 3.6.4 + Bootstrap bundle + SweetAlert2 + SignalR client + custom scripts | ⚠️ Heavy — multiple JS libraries loaded globally |
| **Image Loading** | Direct file serving from `/Images/` directory | ⚠️ No CDN, no lazy loading observed in code, no responsive image markup |
| **Theme Assets** | Metronic 8 assets in `Content/assets/` and `Content/assetsn/` — multiple theme resource directories | ⚠️ Potentially loading unused legacy theme assets |

### 2.2 Cache-Busting Analysis

**File**: `Views/Shared/_LayoutStudent2026.cshtml`

```html
<link href="~/Content/fame-student-2026.css?v=@DateTime.Now.Ticks" rel="stylesheet" />
```

**Finding**: Cache-busting uses `DateTime.Now.Ticks` which generates a **unique URL on every page request**. This means:
- The browser **never caches** the CSS file across sessions
- Every page load requires a fresh CSS download
- CDN or proxy caching is completely ineffective

**Recommendation**: Use file content hash or build version number for cache-busting instead of timestamp ticks.

| Current Approach | Issue | Recommended Approach |
|-----------------|-------|---------------------|
| `?v=@DateTime.Now.Ticks` | New URL every request → no caching | `?v=@File.GetLastWriteTime(...).Ticks` or build version |

**Severity**: Medium — affects page load speed for returning visitors

### 2.3 Bundle Configuration

**Finding**: No evidence of ASP.NET bundling and minification configuration was observed in the production deployment. Key indicators:
- No `BundleConfig.cs` visible in production (compiled in DLL, so not verifiable)
- CSS files referenced individually in layout: `fame-student-2026.css` loaded separately
- jQuery, Bootstrap, and custom scripts appear to be loaded as separate files

**Assessment**: Without bundling, the browser makes multiple HTTP requests for CSS and JS files, increasing page load time — especially on mobile networks common in South Asia.

**Severity**: Medium

---

## 3. Media and Image Performance

### 3.1 Image Storage Analysis

**Directory**: `Images/` (production root)

| Observation | Detail | Impact |
|-------------|--------|--------|
| **1000+ files in root** | User-uploaded profile photos, documents, and content images sit flat in `/Images/` root | File system performance degradation on directory listing |
| **No subdirectory organization** for user uploads | Files named `User6381*.jpg/jpeg/png/heic/pdf` at root level | Difficult to manage, no cleanup automation |
| **Mixed file types** | JPG, JPEG, PNG, HEIC, PDF, GIF all in same directory | HEIC files may not render in all browsers |
| **No image optimization** evidence | No `.webp` versions, no responsive image markup, no `srcset` attributes observed | Larger-than-necessary image downloads |
| **Course thumbnails** | Loaded from `/Images/` + filename with fallback to default | ✅ Fallback pattern is good |
| **Profile photos** | 120x120px display but full-resolution images may be served | Potential waste of bandwidth |

**Severity**: Medium (file count) + Medium (optimization)

### 3.2 Video Delivery

| Aspect | Implementation | Assessment |
|--------|---------------|-----------|
| **Video hosting** | YouTube (external embed) | ✅ Good — offloads video bandwidth to Google CDN |
| **Player integration** | YouTube iframe embed in `TakeCourse.cshtml` | ✅ Standard approach |
| **YouTube URL handling** | Supports standard, `/live/`, and embed URLs with automatic conversion | ✅ Robust URL parsing |
| **Video watermarking** | `watermark-secure.js` loaded | ⚠️ Adds client-side processing overhead |

**Assessment**: YouTube hosting is a performance-positive decision — Google's CDN handles video delivery globally. The tradeoff is dependency on YouTube availability and terms of service compliance.

---

## 4. Dashboard and Page Load Performance

### 4.1 Student Dashboard

**File**: `Views/Student/Index.cshtml`

| Component | Data Source | Loading Pattern |
|-----------|-----------|-----------------|
| Stats Grid | Server-rendered from `StudentDashboardVM` | ✅ Single server request |
| Recent Courses | Server-rendered with image thumbnail URLs | ✅ Single request but images require additional GETs |
| Quick Actions | Static HTML | ✅ No data fetch needed |

**Assessment**: Dashboard uses server-side rendering — appropriate for initial page load. No AJAX-heavy dashboard widgets observed, which is good for perceived performance.

### 4.2 Mock Tests Page

**File**: `Views/Student/MockTests.cshtml`

| Component | Loading Pattern | Assessment |
|-----------|----------------|-----------|
| Test Types | AJAX: `loadTestTypes()` | Additional request after page load |
| Exam List | AJAX: `loadExams()` | Additional request after type selection |
| Loading State | Spinner with "Gathering available exams..." | ✅ User feedback during wait |

**Assessment**: AJAX-loaded content adds perceived delay but provides dynamic filtering. The loading spinner is a positive UX pattern.

### 4.3 Exam Result Page

**File**: `Views/Student/ExamResult.cshtml`

| Component | Loading Pattern | Assessment |
|-----------|----------------|-----------|
| Score Data | Client-side JS population (`#finalPercentage`, `#examName`) | ⚠️ Empty state risk if data not ready |
| ApexCharts | Client-side rendering | ⚠️ Library loading + rendering adds delay |
| Q-dot Grid | Client-side generation | ⚠️ Many DOM elements for large question sets |

**Assessment**: Heavy client-side rendering. For exams with 100+ questions, the Q-dot grid creates many DOM elements. Performance on low-end mobile devices may be affected.

---

## 5. Configuration-Level Performance Concerns

| Config Setting | Value | Performance Impact | Severity |
|---------------|-------|-------------------|----------|
| `debug="true"` | Enabled | Disables compilation caching, increases memory usage, slows page compilation | **High** |
| `shadowCopyBinAssemblies="false"` | Disabled | Slightly faster startup but prevents DLL updates without app pool recycle | Low |
| `maxRequestLength="1048576"` | 1 GB | Memory pressure from large uploads; IIS worker process memory consumption | Medium |
| `maxAllowedContentLength="2147483648"` | 2 GB | Potential DoS via large file uploads consuming server memory | Medium |
| Session timeout: 6000 min | ~4.2 days | Many sessions held in memory simultaneously; memory growth | Medium |
| Verbose Serilog logging | 4 levels active | Disk I/O from constant logging; daily overflow files observed | Medium |

---

## 6. Mobile Performance Considerations

### 6.1 Positive Patterns

| Pattern | Evidence | Impact |
|---------|----------|--------|
| **Mobile-first CSS** | Media queries designed small-to-large | ✅ Reduced CSS processing on mobile |
| **Google Font preconnect** | `<link rel="preconnect" href="https://fonts.googleapis.com">` | ✅ Faster font loading |
| **YouTube for video** | Video served from Google CDN | ✅ Optimized for mobile networks |
| **Bootstrap CDN** | Well-cached CDN resource | ✅ Likely already cached in browser |

### 6.2 Concerns for Mobile Networks

| Concern | Evidence | Impact |
|---------|----------|--------|
| **No service worker** | No offline capability observed | Pages fail completely without network |
| **No lazy loading** | No `loading="lazy"` on images observed in code | All images load eagerly — bandwidth waste on mobile |
| **Cache-busting prevents caching** | `DateTime.Now.Ticks` forces fresh CSS download every visit | Repeated bandwidth usage on mobile data |
| **No image compression** pipeline | No WebP, no responsive images | Large image files over slow networks |
| **Double-tap zoom disabled** | JavaScript prevents rapid touch — forces single-tap interactions | May cause interaction issues on some mobile devices |
| **1139-line registration page** | Entire page loaded at once | Large initial payload for mobile registration |

### 6.3 South Asian Network Context

The primary audience (medical students in Pakistan, Kyrgyzstan, China) often uses:
- Mobile data (3G/4G) with variable speed
- Limited data plans
- Mid-range to low-end Android devices

The lack of image optimization, aggressive caching, and offline capabilities is particularly impactful for this user base.

---

## 7. Perceived Performance Indicators

| Scenario | Perceived Speed | Evidence |
|----------|----------------|---------|
| **Initial page load** | ⚠️ Unknown | Server-side rendering helps but no data on actual TTFB |
| **Dashboard load** | ✅ Likely fast | Server-rendered, minimal AJAX |
| **Course list** | ✅ Likely fast | Server-rendered with package filtering client-side |
| **Course player** | ⚠️ YouTube dependent | Embed load time depends on YouTube response |
| **Start exam** | ⚠️ May lag | Full 1298-line standalone page with CSS/JS initialization |
| **Exam results** | ⚠️ May lag | ApexCharts initialization + Q-dot grid DOM creation |
| **Mock test list** | ⚠️ Delayed | AJAX data loading adds perceived wait |
| **Registration** | ⚠️ Slow on mobile | 1139-line page with multiple script dependencies |
| **Admin panel** | ⚠️ Unknown | 680-line sidebar + Metronic theme overhead |

---

## 8. Performance Bottleneck Risk Areas

| # | Bottleneck | Type | Priority |
|---|-----------|------|----------|
| 1 | **Cache-busting via `DateTime.Now.Ticks`** | Frontend | High — quick fix |
| 2 | **`debug="true"` in production** | Server | High — quick fix |
| 3 | **No image optimization pipeline** | Frontend/Ops | Medium |
| 4 | **1000+ flat files in `/Images/`** | Server/IO | Medium |
| 5 | **No CSS/JS bundling evidence** | Frontend | Medium |
| 6 | **Excessive upload size limits** | Server memory | Medium |
| 7 | **Verbose production logging** | Server IO | Low |
| 8 | **Long session timeout (4.2 days)** | Server memory | Medium |
| 9 | **No lazy loading for images** | Mobile | Medium |
| 10 | **No WebP/responsive images** | Bandwidth | Medium |

---

## 9. Performance Ratings

| Dimension | Score | Notes |
|-----------|:-----:|-------|
| Page Load Strategy | 2.5/5 | Cache-busting defeats caching; no bundling evidence |
| Media Performance | 3/5 | YouTube hosting good; image optimization missing |
| Mobile Performance | 2.5/5 | CSS is mobile-first but no lazy loading, offline, or optimization |
| Configuration | 2/5 | Debug mode on, excessive limits, verbose logging |
| Perceived Performance | 3/5 | Server-rendered dashboards good; AJAX pages have loading states |
| Scalability Indicators | 2.5/5 | Single server, no CDN for assets, flat file storage |

**Overall Performance Score: 3.0 / 5** (based on code-level indicators only)

---

## 10. Quick Performance Wins

| # | Action | Effort | Impact |
|---|--------|--------|--------|
| 1 | Change CSS cache-busting from `DateTime.Now.Ticks` to file hash | 15 min | High — enables browser caching |
| 2 | Set `debug="false"` in Web.config | 1 min | High — enables compilation caching |
| 3 | Add `loading="lazy"` to images in course cards and landing page | 30 min | Medium — reduces initial bandwidth |
| 4 | Reduce `maxRequestLength` to reasonable limit (e.g., 20MB) | 5 min | Medium — prevents memory abuse |
| 5 | Reduce Serilog production verbosity | 15 min | Low — reduces disk IO |
| 6 | Create `robots.txt` to block crawler access to admin paths | 15 min | Low — reduces unnecessary server load |

---

*Performance findings are derived from code-level analysis only. For definitive performance data, conduct Lighthouse audits, WebPageTest runs, and server-side APM monitoring. Consider monitoring Real User Metrics (RUM) given the target audience's network conditions.*
