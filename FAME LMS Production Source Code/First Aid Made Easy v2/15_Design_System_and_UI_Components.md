# 15 — Design System and UI Component Library

*Tailwind CSS 4, Radix UI primitives, CVA variants, design tokens, accessibility standards, and component catalog.*

---

## 1. Design Philosophy

| Principle | Implementation |
|-----------|----------------|
| **Medical professionalism** | Clean, white-space-heavy layouts; blue/green as primary (trust); no visual clutter |
| **Accessibility first** | WCAG 2.1 AA minimum; keyboard navigable; screen-reader tested |
| **Mobile first** | Responsive from 320px; touch targets ≥44px; bottom navigation on mobile |
| **Consistency** | One design system shared across all 4 portals (public, student, admin, ambassador) |
| **Performance** | <100KB CSS total; Tailwind purge removes unused; no CSS-in-JS runtime |

---

## 2. Design Tokens

### 2.1 Color Palette

```typescript
// design-tokens/colors.ts
export const colors = {
  // Primary — Trust, reliability (medical)
  primary: {
    50:  '#eff6ff',
    100: '#dbeafe',
    200: '#bfdbfe',
    300: '#93c5fd',
    400: '#60a5fa',
    500: '#3b82f6',  // Main brand
    600: '#2563eb',  // Buttons, links
    700: '#1d4ed8',  // Hover
    800: '#1e40af',
    900: '#1e3a8a',
    950: '#172554',
  },
  
  // Success — Pass, complete, active
  success: {
    50:  '#f0fdf4',
    500: '#22c55e',
    700: '#15803d',
  },
  
  // Danger — Fail, error, delete
  danger: {
    50:  '#fef2f2',
    500: '#ef4444',
    700: '#b91c1c',
  },
  
  // Warning — Expiry, attention
  warning: {
    50:  '#fffbeb',
    500: '#f59e0b',
    700: '#b45309',
  },
  
  // Neutral — Text, borders, backgrounds
  neutral: {
    0:   '#ffffff',
    50:  '#f9fafb',
    100: '#f3f4f6',
    200: '#e5e7eb',
    300: '#d1d5db',
    400: '#9ca3af',
    500: '#6b7280',
    600: '#4b5563',
    700: '#374151',
    800: '#1f2937',
    900: '#111827',
    950: '#030712',
  },
} as const;
```

### 2.2 Typography Scale

```typescript
// design-tokens/typography.ts
export const typography = {
  fontFamily: {
    sans: ['Inter', 'system-ui', '-apple-system', 'sans-serif'],
    mono: ['JetBrains Mono', 'Fira Code', 'monospace'],
  },
  fontSize: {
    xs:   ['0.75rem',  { lineHeight: '1rem' }],      // 12px
    sm:   ['0.875rem', { lineHeight: '1.25rem' }],    // 14px
    base: ['1rem',     { lineHeight: '1.5rem' }],     // 16px
    lg:   ['1.125rem', { lineHeight: '1.75rem' }],    // 18px
    xl:   ['1.25rem',  { lineHeight: '1.75rem' }],    // 20px
    '2xl': ['1.5rem',  { lineHeight: '2rem' }],       // 24px
    '3xl': ['1.875rem', { lineHeight: '2.25rem' }],   // 30px
    '4xl': ['2.25rem',  { lineHeight: '2.5rem' }],    // 36px
  },
} as const;
```

### 2.3 Spacing & Layout

```typescript
// design-tokens/spacing.ts
export const spacing = {
  // 4px base grid
  0:   '0px',
  0.5: '2px',
  1:   '4px',
  2:   '8px',
  3:   '12px',
  4:   '16px',
  5:   '20px',
  6:   '24px',
  8:   '32px',
  10:  '40px',
  12:  '48px',
  16:  '64px',
  20:  '80px',
  24:  '96px',
};

export const breakpoints = {
  sm:  '640px',   // Mobile landscape
  md:  '768px',   // Tablet
  lg:  '1024px',  // Desktop
  xl:  '1280px',  // Wide desktop
  '2xl': '1536px', // Ultra-wide
};

export const layout = {
  maxWidth: '1280px',        // Content area
  sidebarWidth: '280px',     // Fixed sidebar
  sidebarCollapsed: '64px',  // Collapsed sidebar
  headerHeight: '64px',
};
```

---

## 3. Tailwind CSS 4 Configuration

```css
/* tailwind.css */
@import "tailwindcss";

@theme {
  --color-primary-50: #eff6ff;
  --color-primary-100: #dbeafe;
  --color-primary-200: #bfdbfe;
  --color-primary-300: #93c5fd;
  --color-primary-400: #60a5fa;
  --color-primary-500: #3b82f6;
  --color-primary-600: #2563eb;
  --color-primary-700: #1d4ed8;
  --color-primary-800: #1e40af;
  --color-primary-900: #1e3a8a;

  --color-success-500: #22c55e;
  --color-danger-500: #ef4444;
  --color-warning-500: #f59e0b;

  --font-sans: 'Inter', system-ui, -apple-system, sans-serif;
  --font-mono: 'JetBrains Mono', 'Fira Code', monospace;

  --radius-sm: 0.375rem;
  --radius-md: 0.5rem;
  --radius-lg: 0.75rem;
  --radius-xl: 1rem;
  --radius-full: 9999px;
}
```

---

## 4. Component Library (CVA + Radix UI)

### 4.1 Button

```typescript
// components/ui/button.tsx
import { cva, type VariantProps } from 'class-variance-authority';
import { Slot } from '@radix-ui/react-slot';
import { forwardRef } from 'react';
import { cn } from '@/lib/utils';

const buttonVariants = cva(
  // Base styles
  'inline-flex items-center justify-center gap-2 rounded-md text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50',
  {
    variants: {
      variant: {
        primary:   'bg-primary-600 text-white hover:bg-primary-700 active:bg-primary-800',
        secondary: 'bg-neutral-100 text-neutral-900 hover:bg-neutral-200 active:bg-neutral-300',
        outline:   'border border-neutral-300 bg-white text-neutral-700 hover:bg-neutral-50',
        ghost:     'text-neutral-700 hover:bg-neutral-100',
        danger:    'bg-danger-500 text-white hover:bg-danger-700',
        link:      'text-primary-600 underline-offset-4 hover:underline',
      },
      size: {
        sm: 'h-8 px-3 text-xs',
        md: 'h-10 px-4 text-sm',
        lg: 'h-12 px-6 text-base',
        icon: 'h-10 w-10',
      },
    },
    defaultVariants: {
      variant: 'primary',
      size: 'md',
    },
  }
);

export interface ButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
  asChild?: boolean;
  loading?: boolean;
}

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant, size, asChild, loading, children, disabled, ...props }, ref) => {
    const Comp = asChild ? Slot : 'button';
    return (
      <Comp
        className={cn(buttonVariants({ variant, size, className }))}
        ref={ref}
        disabled={disabled || loading}
        {...props}
      >
        {loading && <Spinner className="h-4 w-4 animate-spin" />}
        {children}
      </Comp>
    );
  }
);
Button.displayName = 'Button';
```

### 4.2 Input

```typescript
// components/ui/input.tsx
import { forwardRef } from 'react';
import { cn } from '@/lib/utils';

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  error?: string;
  label?: string;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ className, error, label, id, ...props }, ref) => {
    const inputId = id || props.name;
    return (
      <div className="space-y-1.5">
        {label && (
          <label htmlFor={inputId} className="text-sm font-medium text-neutral-700">
            {label}
          </label>
        )}
        <input
          id={inputId}
          className={cn(
            'flex h-10 w-full rounded-md border border-neutral-300 bg-white px-3 py-2 text-sm',
            'placeholder:text-neutral-400',
            'focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent',
            'disabled:cursor-not-allowed disabled:opacity-50',
            error && 'border-danger-500 focus:ring-danger-500',
            className
          )}
          ref={ref}
          aria-invalid={!!error}
          aria-describedby={error ? `${inputId}-error` : undefined}
          {...props}
        />
        {error && (
          <p id={`${inputId}-error`} className="text-xs text-danger-500" role="alert">
            {error}
          </p>
        )}
      </div>
    );
  }
);
Input.displayName = 'Input';
```

### 4.3 Card

```typescript
// components/ui/card.tsx
import { cn } from '@/lib/utils';

export function Card({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      className={cn(
        'rounded-lg border border-neutral-200 bg-white shadow-sm',
        className
      )}
      {...props}
    />
  );
}

export function CardHeader({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('p-6 pb-0', className)} {...props} />;
}

export function CardTitle({ className, ...props }: React.HTMLAttributes<HTMLHeadingElement>) {
  return <h3 className={cn('text-lg font-semibold text-neutral-900', className)} {...props} />;
}

export function CardContent({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('p-6', className)} {...props} />;
}

export function CardFooter({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('flex items-center p-6 pt-0', className)} {...props} />;
}
```

### 4.4 Dialog (Modal)

```typescript
// components/ui/dialog.tsx
import * as DialogPrimitive from '@radix-ui/react-dialog';
import { X } from 'lucide-react';
import { cn } from '@/lib/utils';

export const Dialog = DialogPrimitive.Root;
export const DialogTrigger = DialogPrimitive.Trigger;

export function DialogContent({
  className,
  children,
  ...props
}: DialogPrimitive.DialogContentProps) {
  return (
    <DialogPrimitive.Portal>
      <DialogPrimitive.Overlay className="fixed inset-0 z-50 bg-black/50 data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
      <DialogPrimitive.Content
        className={cn(
          'fixed left-1/2 top-1/2 z-50 w-full max-w-lg -translate-x-1/2 -translate-y-1/2',
          'rounded-lg bg-white p-6 shadow-xl',
          'data-[state=open]:animate-in data-[state=closed]:animate-out',
          'data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0',
          'data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95',
          className
        )}
        {...props}
      >
        {children}
        <DialogPrimitive.Close className="absolute right-4 top-4 rounded-sm opacity-70 hover:opacity-100 focus:outline-none focus:ring-2 focus:ring-primary-500">
          <X className="h-4 w-4" />
          <span className="sr-only">Close</span>
        </DialogPrimitive.Close>
      </DialogPrimitive.Content>
    </DialogPrimitive.Portal>
  );
}
```

### 4.5 Badge

```typescript
// components/ui/badge.tsx
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '@/lib/utils';

const badgeVariants = cva(
  'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium',
  {
    variants: {
      variant: {
        default:  'bg-primary-100 text-primary-700',
        success:  'bg-green-100 text-green-700',
        warning:  'bg-amber-100 text-amber-700',
        danger:   'bg-red-100 text-red-700',
        neutral:  'bg-neutral-100 text-neutral-600',
      },
    },
    defaultVariants: { variant: 'default' },
  }
);

export function Badge({
  className,
  variant,
  ...props
}: React.HTMLAttributes<HTMLSpanElement> & VariantProps<typeof badgeVariants>) {
  return <span className={cn(badgeVariants({ variant }), className)} {...props} />;
}
```

---

## 5. Component Catalog

### 5.1 Primitive Components (Radix + CVA)

| Component | Radix Primitive | Variants |
|-----------|:---------------:|----------|
| Button | Slot | primary, secondary, outline, ghost, danger, link × sm, md, lg |
| Input | — | default, error state |
| Textarea | — | default, error state |
| Select | Select | default, error state |
| Checkbox | Checkbox | default, indeterminate |
| Radio Group | RadioGroup | default |
| Switch | Switch | default |
| Dialog | Dialog | default, alert |
| Dropdown Menu | DropdownMenu | default |
| Popover | Popover | default |
| Tooltip | Tooltip | default |
| Tabs | Tabs | default, underline, pill |
| Accordion | Accordion | single, multiple |
| Toast | Toast | success, error, warning, info |
| Avatar | Avatar | sm, md, lg with fallback |
| Badge | — | default, success, warning, danger, neutral |
| Separator | Separator | horizontal, vertical |
| Progress | Progress | default, success, warning |
| Skeleton | — | default |

### 5.2 Composite Components (App-Specific)

| Component | Description | Used In |
|-----------|-------------|---------|
| `CourseCard` | Thumbnail, title, track badge, progress bar, CTA | Catalog, dashboard |
| `ExamTimer` | Countdown with warning states | Exam engine |
| `QuestionCard` | MCQ option grid, strike-through, explanation | Exam engine |
| `ProgressRing` | Circular progress indicator | Course progress |
| `StatsCard` | Icon + label + big number + trend | Dashboards |
| `DataTable` | Sortable, filterable, paginated table | Admin pages |
| `EmptyState` | Illustration + message + CTA | Empty lists |
| `PricingCard` | Package details, price, features, CTA | Pricing page |
| `NotificationItem` | Icon + title + body + time + action | Notification dropdown |
| `UserAvatar` | Avatar + name + role badge | Headers, chat |

### 5.3 Layout Components

| Component | Description |
|-----------|-------------|
| `AppShell` | Sidebar + header + content area |
| `Sidebar` | Collapsible navigation with role-based items |
| `Header` | Logo, search, notifications, user menu |
| `PageHeader` | Title + breadcrumbs + actions |
| `ContentArea` | Max-width container with appropriate padding |
| `MobileNav` | Bottom tab navigation for mobile |

---

## 6. Accessibility Requirements

| Requirement | Implementation |
|-------------|----------------|
| **Keyboard navigation** | All interactive elements focusable; logical tab order; skip links |
| **Focus indicators** | Visible ring-2 ring-primary-500 on all focusable elements |
| **Color contrast** | ≥4.5:1 for text, ≥3:1 for large text and UI components |
| **Screen reader** | ARIA labels on icons, landmarks on page sections, live regions for notifications |
| **Reduced motion** | `prefers-reduced-motion` media query disables animations |
| **Form accessibility** | Labels linked to inputs, error messages in aria-describedby, required indicators |
| **Touch targets** | ≥44×44px on mobile for all interactive elements |

---

## 7. Dark Mode (Future)

Dark mode is not in MVP but the design system is prepared:
- All colors use CSS custom properties via Tailwind's `@theme`
- No hardcoded colors in component code
- `class` strategy for dark mode (toggle adds `dark` to `<html>`)
- Dark palette tokens defined but not applied in v1.0

---

*The design system is the shared language between design and engineering. Every pixel on FAME V2 comes from this system. Radix handles accessibility, CVA handles variants, Tailwind handles the styling — no CSS-in-JS runtime overhead, full type safety, and automatic dark mode readiness.*
