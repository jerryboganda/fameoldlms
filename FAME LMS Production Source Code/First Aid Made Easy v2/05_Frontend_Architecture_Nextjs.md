# 05 — Frontend Architecture (Next.js 15)

*App Router routes, rendering strategy per route, component architecture, and exam engine rebuild spec.*

---

## 1. Next.js 15 App Router Structure

### 1.1 Route Groups

```
app/
├── (public)/                    ← Marketing / unauthenticated pages
│   ├── layout.tsx               ← Public layout (header + footer, no sidebar)
│   ├── page.tsx                 ← / (Home / Landing)
│   ├── courses/
│   │   ├── page.tsx             ← /courses (listing)
│   │   └── [track]/
│   │       ├── page.tsx         ← /courses/fcps-1 (track listing)
│   │       └── [slug]/
│   │           └── page.tsx     ← /courses/fcps-1/clinical-pathology (detail)
│   ├── pricing/
│   │   └── page.tsx             ← /pricing
│   ├── blog/
│   │   ├── page.tsx             ← /blog (listing)
│   │   └── [slug]/
│   │       └── page.tsx         ← /blog/how-to-prepare-for-fcps
│   ├── about/
│   │   └── page.tsx
│   ├── faq/
│   │   └── page.tsx
│   ├── contact/
│   │   └── page.tsx
│   ├── terms/
│   │   └── page.tsx
│   ├── privacy/
│   │   └── page.tsx
│   ├── ambassador/
│   │   └── page.tsx             ← Ambassador program landing
│   └── verify/
│       └── [certificateId]/
│           └── page.tsx         ← Certificate verification
│
├── (auth)/                      ← Authentication pages
│   ├── layout.tsx               ← Centered card layout, no nav
│   ├── login/
│   │   └── page.tsx
│   ├── register/
│   │   └── page.tsx
│   ├── forgot-password/
│   │   └── page.tsx
│   └── reset-password/
│       └── [token]/
│           └── page.tsx
│
├── (student)/                   ← Student portal (authenticated)
│   ├── layout.tsx               ← Sidebar + header layout
│   ├── dashboard/
│   │   ├── page.tsx             ← Dashboard home
│   │   ├── courses/
│   │   │   ├── page.tsx         ← My courses
│   │   │   └── [id]/
│   │   │       ├── page.tsx     ← Course player
│   │   │       └── [sectionId]/
│   │   │           └── page.tsx ← Specific section
│   │   ├── exams/
│   │   │   ├── page.tsx         ← Exam listing
│   │   │   ├── start/
│   │   │   │   └── [id]/
│   │   │   │       └── page.tsx ← Pre-exam config
│   │   │   ├── take/
│   │   │   │   └── [id]/
│   │   │   │       └── page.tsx ← Exam in progress (full-screen)
│   │   │   └── result/
│   │   │       └── [id]/
│   │   │           └── page.tsx ← Result + analytics
│   │   ├── progress/
│   │   │   └── page.tsx
│   │   ├── bookmarks/
│   │   │   └── page.tsx
│   │   ├── certificates/
│   │   │   └── page.tsx
│   │   ├── subscription/
│   │   │   ├── page.tsx
│   │   │   ├── upgrade/
│   │   │   │   └── page.tsx
│   │   │   └── invoices/
│   │   │       └── page.tsx
│   │   ├── support/
│   │   │   ├── page.tsx
│   │   │   └── [ticketId]/
│   │   │       └── page.tsx
│   │   ├── notifications/
│   │   │   └── page.tsx
│   │   └── settings/
│   │       └── page.tsx
│   └── checkout/
│       └── page.tsx             ← Payment checkout
│
├── (admin)/                     ← Admin panel (authenticated + admin role)
│   ├── layout.tsx               ← Admin sidebar layout
│   └── admin/
│       ├── page.tsx             ← Admin dashboard
│       ├── users/
│       ├── courses/
│       ├── questions/
│       ├── exams/
│       ├── enrollments/
│       ├── subscriptions/
│       ├── payments/
│       ├── coupons/
│       ├── ambassadors/
│       ├── blog/
│       ├── certificates/
│       ├── content/
│       ├── support/
│       ├── reports/
│       ├── emails/
│       ├── audit-log/
│       ├── jobs/
│       └── settings/
│
├── (ambassador)/                ← Ambassador portal
│   ├── layout.tsx
│   └── ambassador/
│       ├── page.tsx
│       ├── referrals/
│       ├── earnings/
│       ├── links/
│       └── settings/
│
├── layout.tsx                   ← Root layout (html, body, fonts, providers)
├── not-found.tsx                ← Custom 404
├── error.tsx                    ← Global error boundary
├── loading.tsx                  ← Global loading state
├── robots.ts                    ← robots.txt generation
└── sitemap.ts                   ← sitemap.xml generation
```

### 1.2 Rendering Strategy per Route

| Route Pattern | Rendering | Revalidation | Rationale |
|--------------|:---------:|:------------:|-----------|
| `/` (Home) | SSG | ISR 60s | SEO + performance, infrequent changes |
| `/courses` | SSG | ISR 300s | SEO for search, course additions ~weekly |
| `/courses/[track]` | SSG | ISR 300s | SEO, content relatively stable |
| `/courses/[track]/[slug]` | SSG | ISR 60s | SEO + detail pages, on-demand revalidation |
| `/pricing` | SSG | ISR 60s | SEO, prices change rarely, on-demand revalidation |
| `/blog/*` | SSG | ISR 3600s | SEO, new posts infrequent |
| `/about`, `/faq`, `/terms`, `/privacy` | SSG | ISR 86400s | Static content |
| `/login`, `/register`, `/forgot-password` | SSR | None | Per-request, may have redirects |
| `/dashboard/*` | SSR | None | Personalized, must be fresh |
| `/dashboard/exams/take/*` | CSR | None | Full client-side for exam interactivity |
| `/admin/*` | SSR | None | Personalized, behind auth |
| `/ambassador/*` | SSR | None | Personalized, behind auth |
| `/verify/[certificateId]` | SSR | None | Dynamic lookup, public |

---

## 2. Component Architecture

### 2.1 Component Categories

```
packages/
└── ui/                          ← Shared UI library (Turborepo package)
    └── src/
        ├── primitives/          ← Radix UI wrappers with Tailwind styling
        │   ├── Button.tsx
        │   ├── Dialog.tsx
        │   ├── DropdownMenu.tsx
        │   ├── Input.tsx
        │   ├── Select.tsx
        │   ├── Tabs.tsx
        │   ├── Toast.tsx
        │   ├── Tooltip.tsx
        │   └── ...
        │
        ├── composites/          ← Composed from primitives
        │   ├── DataTable.tsx     ← Sortable, filterable table with TanStack Table
        │   ├── FormField.tsx     ← Label + Input + Error message
        │   ├── Pagination.tsx
        │   ├── SearchInput.tsx   ← Debounced search with icon
        │   ├── FileUpload.tsx
        │   ├── RichTextEditor.tsx ← TipTap wrapper
        │   └── ...
        │
        └── layouts/             ← Page layout shells
            ├── AppShell.tsx      ← Sidebar + header + content
            ├── AuthLayout.tsx    ← Centered card
            ├── FullScreenLayout.tsx ← No chrome (exam mode)
            └── PublicLayout.tsx  ← Header + footer
```

```
apps/web/
└── src/
    ├── components/              ← App-specific components
    │   ├── courses/
    │   │   ├── CourseCard.tsx
    │   │   ├── CourseGrid.tsx
    │   │   ├── CourseDetail.tsx
    │   │   └── VideoPlayer.tsx
    │   ├── exams/
    │   │   ├── ExamEngine.tsx    ← Core exam component
    │   │   ├── QuestionCard.tsx
    │   │   ├── OptionItem.tsx
    │   │   ├── NavigationPanel.tsx
    │   │   ├── ExamTimer.tsx
    │   │   ├── LabValuesPanel.tsx
    │   │   ├── StrikeThrough.tsx
    │   │   └── ResultsView.tsx
    │   ├── dashboard/
    │   │   ├── StatsCard.tsx
    │   │   ├── ProgressChart.tsx
    │   │   ├── RecentCourses.tsx
    │   │   └── StudyStreak.tsx
    │   ├── payment/
    │   │   ├── PricingCard.tsx
    │   │   ├── CheckoutForm.tsx
    │   │   └── PaymentMethodSelector.tsx
    │   └── shared/
    │       ├── Header.tsx
    │       ├── Sidebar.tsx
    │       ├── Footer.tsx
    │       ├── Breadcrumbs.tsx
    │       └── EmptyState.tsx
    │
    ├── hooks/                   ← Custom React hooks
    │   ├── useAuth.ts
    │   ├── useExam.ts
    │   ├── useVideoPlayer.ts
    │   ├── useDebounce.ts
    │   ├── useKeyboardShortcut.ts
    │   ├── useMediaQuery.ts
    │   └── useLocalStorage.ts
    │
    ├── lib/                     ← Utility functions
    │   ├── api.ts               ← API client (fetch wrapper with auth)
    │   ├── auth.ts              ← Auth utilities (token management)
    │   ├── validators.ts        ← Zod schemas (shared with backend)
    │   ├── formatters.ts        ← Date, currency, number formatting
    │   └── constants.ts         ← App-wide constants
    │
    ├── stores/                  ← Zustand stores
    │   ├── useAuthStore.ts
    │   ├── useExamStore.ts
    │   ├── useUIStore.ts
    │   └── useNotificationStore.ts
    │
    └── queries/                 ← TanStack Query definitions
        ├── courses.ts
        ├── exams.ts
        ├── subscriptions.ts
        ├── users.ts
        └── dashboard.ts
```

### 2.2 Component Conventions

```tsx
// Example: Button primitive using Radix + CVA + Tailwind
import { Slot } from '@radix-ui/react-slot';
import { cva, type VariantProps } from 'class-variance-authority';
import { forwardRef } from 'react';
import { cn } from '@/lib/utils';

const buttonVariants = cva(
  'inline-flex items-center justify-center rounded-md text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50',
  {
    variants: {
      variant: {
        primary: 'bg-teal-700 text-white hover:bg-teal-800',
        secondary: 'bg-gray-100 text-gray-900 hover:bg-gray-200',
        destructive: 'bg-red-600 text-white hover:bg-red-700',
        ghost: 'hover:bg-gray-100',
        link: 'text-teal-700 underline-offset-4 hover:underline',
      },
      size: {
        sm: 'h-8 px-3 text-xs',
        md: 'h-10 px-4',
        lg: 'h-12 px-6 text-base',
      },
    },
    defaultVariants: { variant: 'primary', size: 'md' },
  }
);

interface ButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
  asChild?: boolean;
}

const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant, size, asChild = false, ...props }, ref) => {
    const Comp = asChild ? Slot : 'button';
    return (
      <Comp
        className={cn(buttonVariants({ variant, size, className }))}
        ref={ref}
        {...props}
      />
    );
  }
);
Button.displayName = 'Button';

export { Button, buttonVariants };
```

---

## 3. State Management Architecture

### 3.1 State Categories

| State Type | Solution | Scope | Examples |
|-----------|----------|-------|---------|
| **Server State** | TanStack Query | Per-query | Course data, user profile, exam questions, subscription info |
| **Client UI State** | Zustand | Global | Sidebar open/closed, theme, toast queue |
| **Form State** | React Hook Form | Per-form | Registration form, checkout form, settings |
| **URL State** | Next.js searchParams | Per-page | Search query, filters, pagination, sort |
| **Exam State** | Zustand (dedicated) | Per-exam | Current question, answers, flags, timer, strike-throughs |
| **Auth State** | Zustand + Cookie | Global | Current user, role, token refresh |

### 3.2 TanStack Query Pattern

```tsx
// queries/courses.ts
import { queryOptions } from '@tanstack/react-query';
import { api } from '@/lib/api';

export const courseKeys = {
  all: ['courses'] as const,
  lists: () => [...courseKeys.all, 'list'] as const,
  list: (filters: CourseFilters) => [...courseKeys.lists(), filters] as const,
  details: () => [...courseKeys.all, 'detail'] as const,
  detail: (id: string) => [...courseKeys.details(), id] as const,
};

export const courseListOptions = (filters: CourseFilters) =>
  queryOptions({
    queryKey: courseKeys.list(filters),
    queryFn: () => api.get<PaginatedResult<Course>>('/api/courses', { params: filters }),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

export const courseDetailOptions = (id: string) =>
  queryOptions({
    queryKey: courseKeys.detail(id),
    queryFn: () => api.get<CourseDetail>(`/api/courses/${id}`),
    staleTime: 60 * 1000, // 1 minute
  });
```

### 3.3 Exam Store (Zustand)

```tsx
// stores/useExamStore.ts
import { create } from 'zustand';
import { immer } from 'zustand/middleware/immer';

interface ExamState {
  // Exam data
  examId: string | null;
  questions: Question[];
  currentIndex: number;
  
  // Student answers
  answers: Record<number, string>; // questionIndex → optionKey
  flagged: Set<number>;
  strikeThrough: Record<number, Set<string>>; // questionIndex → set of struck option keys
  
  // Timer
  startTime: number | null;
  timeLimit: number | null; // seconds, null = untimed
  
  // Actions
  setAnswer: (index: number, option: string) => void;
  toggleFlag: (index: number) => void;
  toggleStrikeThrough: (index: number, option: string) => void;
  goToQuestion: (index: number) => void;
  nextQuestion: () => void;
  prevQuestion: () => void;
  
  // Computed
  answeredCount: () => number;
  flaggedCount: () => number;
  isComplete: () => boolean;
}
```

---

## 4. Exam Engine React Rebuild

### 4.1 V1 Strengths to Preserve

The V1 exam engine scored 4.0/5 — the highest score in the audit. These features **must** be preserved identically:

| Feature | V1 Implementation | V2 Implementation |
|---------|-------------------|-------------------|
| Keyboard answering (A/B/C/D) | jQuery keydown handler | `useKeyboardShortcut` hook |
| Strike-through elimination | Click-to-strike, CSS class toggle | Zustand state + conditional CSS |
| Question navigation panel | jQuery-rendered grid | React component with Zustand |
| Timer display | `setInterval` with DOM manipulation | React state with `useEffect` interval |
| Flag for review | jQuery toggle | Zustand toggle |
| Lab values panel | Separate page (link out) | Slide-out panel (React Portal) |

### 4.2 Exam Engine Component Tree

```
<ExamEngine>
  ├── <ExamTopBar>
  │   ├── <ExamTimer timeLimit={seconds} />
  │   ├── <QuestionCounter current={3} total={100} />
  │   ├── <FlagButton isFlagged={true} onClick={toggleFlag} />
  │   └── <LabValuesToggle onClick={openPanel} />
  │
  ├── <QuestionCard>
  │   ├── <QuestionText html={question.text} />
  │   └── <OptionList>
  │       ├── <OptionItem key="A" struck={false} selected={true} />
  │       ├── <OptionItem key="B" struck={true} selected={false} />
  │       ├── <OptionItem key="C" struck={false} selected={false} />
  │       └── <OptionItem key="D" struck={false} selected={false} />
  │
  ├── <ExamBottomBar>
  │   ├── <PrevButton />
  │   ├── <NavigationPanel>
  │   │   └── Grid of numbered squares (green=answered, yellow=flagged, gray=unanswered)
  │   ├── <NextButton />
  │   └── <SubmitButton />
  │
  └── <LabValuesPanel isOpen={true} onClose={closePanel}>
      ├── <SearchInput />
      └── <LabValuesList categories={labValues} />
```

### 4.3 Keyboard Shortcuts

```typescript
const EXAM_SHORTCUTS: KeyboardShortcutMap = {
  'a': () => setAnswer(currentIndex, 'A'),
  'b': () => setAnswer(currentIndex, 'B'),
  'c': () => setAnswer(currentIndex, 'C'),
  'd': () => setAnswer(currentIndex, 'D'),
  'n': () => nextQuestion(),
  'p': () => prevQuestion(),
  'f': () => toggleFlag(currentIndex),
  'l': () => toggleLabValues(),
  'ArrowRight': () => nextQuestion(),
  'ArrowLeft': () => prevQuestion(),
  '1-9': (num) => goToQuestion(num - 1), // Quick jump
};
```

---

## 5. API Client & Data Fetching

### 5.1 API Client

```typescript
// lib/api.ts
class ApiClient {
  private baseUrl: string;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  async request<T>(path: string, options?: RequestInit): Promise<T> {
    const url = `${this.baseUrl}${path}`;
    const response = await fetch(url, {
      ...options,
      credentials: 'include', // Send cookies
      headers: {
        'Content-Type': 'application/json',
        ...options?.headers,
      },
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({}));
      throw new ApiError(response.status, error);
    }

    return response.json();
  }

  get<T>(path: string, params?: Record<string, any>): Promise<T> {
    const searchParams = params ? `?${new URLSearchParams(params)}` : '';
    return this.request<T>(`${path}${searchParams}`);
  }

  post<T>(path: string, body?: unknown): Promise<T> {
    return this.request<T>(path, { method: 'POST', body: JSON.stringify(body) });
  }

  put<T>(path: string, body?: unknown): Promise<T> {
    return this.request<T>(path, { method: 'PUT', body: JSON.stringify(body) });
  }

  delete<T>(path: string): Promise<T> {
    return this.request<T>(path, { method: 'DELETE' });
  }
}

export const api = new ApiClient(process.env.NEXT_PUBLIC_API_URL || '/api');
```

### 5.2 Server Components Data Fetching

```tsx
// app/(public)/courses/page.tsx — Server Component (SSG + ISR)
import { courseListOptions } from '@/queries/courses';

export const revalidate = 300; // ISR: regenerate every 5 minutes

export default async function CoursesPage({
  searchParams,
}: {
  searchParams: { track?: string; page?: string };
}) {
  const courses = await fetch(
    `${process.env.API_URL}/api/courses?${new URLSearchParams(searchParams)}`,
    { next: { revalidate: 300 } }
  ).then(res => res.json());

  return (
    <main>
      <h1>Medical Exam Courses</h1>
      <TrackFilter current={searchParams.track} />
      <CourseGrid courses={courses.items} />
      <Pagination total={courses.total} page={Number(searchParams.page) || 1} />
    </main>
  );
}

export async function generateMetadata({ searchParams }) {
  const track = searchParams.track;
  return {
    title: track ? `${track} Courses | FAME` : 'All Courses | FAME',
    description: `Browse ${track || 'medical exam'} preparation courses.`,
  };
}
```

---

## 6. SEO Infrastructure

### 6.1 Metadata Generation

```typescript
// app/(public)/courses/[track]/[slug]/page.tsx
export async function generateMetadata({ params }): Promise<Metadata> {
  const course = await getCourse(params.track, params.slug);
  return {
    title: `${course.title} | FAME`,
    description: course.description.slice(0, 160),
    openGraph: {
      title: course.title,
      description: course.description.slice(0, 160),
      images: [{ url: course.thumbnailUrl, width: 1200, height: 630 }],
      type: 'website',
    },
  };
}
```

### 6.2 Structured Data (JSON-LD)

```tsx
// For course detail pages
<script
  type="application/ld+json"
  dangerouslySetInnerHTML={{
    __html: JSON.stringify({
      '@context': 'https://schema.org',
      '@type': 'Course',
      name: course.title,
      description: course.description,
      provider: {
        '@type': 'Organization',
        name: 'First Aid Made Easy',
      },
      instructor: {
        '@type': 'Person',
        name: 'Dr. Hafiz M. Atif Waheed',
        jobTitle: 'MBBS, FCPS',
      },
    }),
  }}
/>
```

### 6.3 Sitemap & Robots

```typescript
// app/sitemap.ts
export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const courses = await api.get<Course[]>('/api/courses/slugs');
  const posts = await api.get<BlogPost[]>('/api/blog/slugs');

  return [
    { url: 'https://firstaidmadeeasy.com.pk', lastModified: new Date(), priority: 1.0 },
    { url: 'https://firstaidmadeeasy.com.pk/courses', priority: 0.9 },
    { url: 'https://firstaidmadeeasy.com.pk/pricing', priority: 0.9 },
    ...courses.map(c => ({
      url: `https://firstaidmadeeasy.com.pk/courses/${c.track}/${c.slug}`,
      lastModified: c.updatedAt,
      priority: 0.8,
    })),
    ...posts.map(p => ({
      url: `https://firstaidmadeeasy.com.pk/blog/${p.slug}`,
      lastModified: p.updatedAt,
      priority: 0.6,
    })),
  ];
}
```

---

## 7. Performance Optimizations

| Technique | Implementation | Target |
|-----------|---------------|--------|
| **Code Splitting** | Next.js automatic per-route splitting | < 100KB initial JS per route |
| **Image Optimization** | `next/image` with WebP/AVIF, lazy loading | LCP < 1.5s |
| **Font Loading** | `next/font` with `display: swap` | No FOIT |
| **Prefetching** | `<Link>` auto-prefetch on viewport | Near-instant navigation |
| **Bundle Analysis** | `@next/bundle-analyzer` in CI | Alert on bundle > 200KB |
| **React Server Components** | Default for non-interactive components | Reduced client JS |
| **Streaming SSR** | `loading.tsx` with Suspense boundaries | Fast TTFB |
| **Static Generation** | SSG + ISR for public content | Instant loads from CDN |

---

## 8. Accessibility Requirements

| Requirement | Implementation |
|------------|---------------|
| **WCAG 2.1 AA** compliance | Radix UI primitives are accessible by default |
| **Keyboard navigation** | All interactive elements focusable, logical tab order |
| **Screen reader** | Semantic HTML, ARIA labels, live regions for dynamic content |
| **Color contrast** | 4.5:1 minimum (checked with Tailwind config) |
| **Focus indicators** | Visible focus ring on all interactive elements |
| **Motion** | `prefers-reduced-motion` respected, no autoplay animations |
| **Skip links** | "Skip to main content" link for keyboard users |
| **Error announcements** | Form errors use `aria-describedby` + live region |

---

*This document defines the complete frontend architecture. Component implementations should follow the patterns defined here. The exam engine rebuild is the highest-priority frontend feature due to the v1 exam engine's 4.0/5 rating.*
