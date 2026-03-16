# 19 — Coding Standards and Conventions

*Naming conventions, code organization rules, formatting, documentation requirements, and enforcement tooling.*

---

## 1. General Principles

1. **Clarity over cleverness** — Write code that reads like prose. No code golf.
2. **Consistency** — Follow the conventions in this document. When in doubt, match surrounding code.
3. **Small functions** — Functions should do one thing. Target ≤30 lines.
4. **No premature abstraction** — Don't create an interface until you have 2+ implementations or need testability.
5. **Fail fast** — Validate inputs early. Return errors immediately. Don't bury validation in deep logic.
6. **No magic strings** — Use constants, enums, or configuration. String literals only in user-facing text.

---

## 2. C# / ASP.NET Core Conventions

### 2.1 Naming

| Element | Convention | Example |
|---------|-----------|---------|
| Namespace | PascalCase, match folder | `FAME.Modules.Payment.Features` |
| Class | PascalCase, noun | `CourseService`, `PaymentGateway` |
| Interface | PascalCase, `I` prefix | `IPaymentGateway`, `ICacheService` |
| Method | PascalCase, verb | `GetCourseById`, `ProcessPayment` |
| Property | PascalCase | `FirstName`, `IsActive` |
| Private field | `_camelCase` | `_logger`, `_dbContext` |
| Parameter | camelCase | `courseId`, `cancellationToken` |
| Local variable | camelCase | `enrollment`, `totalAmount` |
| Constant | PascalCase | `MaxRetries`, `DefaultPageSize` |
| Enum | PascalCase | `TransactionStatus.Completed` |
| Async method | Suffix `Async` | `GetCoursesAsync`, `SendEmailAsync` |

### 2.2 File Organization

```csharp
// One class per file. File name matches class name.
// Exception: small DTOs can be grouped.

// Order within a file:
// 1. Using directives (sorted, no unused)
// 2. Namespace (file-scoped)
// 3. Class declaration
//    a. Constants
//    b. Private fields
//    c. Constructor(s)
//    d. Public properties
//    e. Public methods
//    f. Private methods

namespace FAME.Modules.Payment.Features.ProcessCheckout;

public sealed class ProcessCheckoutHandler
{
    private const int MaxRetries = 3;
    
    private readonly FameDbContext _db;
    private readonly IPaymentGatewayFactory _gatewayFactory;
    private readonly ILogger<ProcessCheckoutHandler> _logger;
    
    public ProcessCheckoutHandler(
        FameDbContext db,
        IPaymentGatewayFactory gatewayFactory,
        ILogger<ProcessCheckoutHandler> logger)
    {
        _db = db;
        _gatewayFactory = gatewayFactory;
        _logger = logger;
    }
    
    public async Task<Result<CheckoutResponse>> Handle(
        CheckoutCommand command, CancellationToken ct)
    {
        // Implementation
    }
    
    private async Task<Transaction> CreateTransaction(/* ... */)
    {
        // Implementation
    }
}
```

### 2.3 Error Handling

```csharp
// Use Result<T> pattern — no exceptions for business logic
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public int StatusCode { get; }
}

// DO:
public async Task<Result<CourseDto>> GetCourse(Guid id, CancellationToken ct)
{
    var course = await _db.Courses.FindAsync(id, ct);
    if (course is null)
        return Result<CourseDto>.NotFound("Course not found");
    
    return Result<CourseDto>.Success(course.ToDto());
}

// DON'T:
public async Task<CourseDto> GetCourse(Guid id)
{
    var course = await _db.Courses.FindAsync(id);
    if (course is null)
        throw new NotFoundException("Course not found"); // ❌ Don't use exceptions for flow
    
    return course.ToDto();
}
```

### 2.4 Nullable Reference Types

```csharp
// Enable in all projects
<Nullable>enable</Nullable>

// DO: Express nullability explicitly
public string? MiddleName { get; set; }     // May be null
public string Email { get; set; } = "";      // Never null

// DON'T: Suppress without explanation
var name = user!.Name; // ❌ Why is this safe?
```

### 2.5 LINQ Style

```csharp
// Prefer method syntax for short chains
var active = users.Where(u => u.IsActive).ToList();

// Prefer query syntax for complex joins
var results = 
    from e in enrollments
    join p in packages on e.PackageId equals p.Id
    where e.Status == EnrollmentStatus.Active
    orderby e.CreatedAt descending
    select new { e.UserId, p.Name, e.ExpiresAt };
```

### 2.6 Cancellation Tokens

```csharp
// Always propagate CancellationToken through async chains
// Parameter name: ct (abbreviated for readability)

public async Task<Result<List<CourseDto>>> GetCourses(CancellationToken ct)
{
    var courses = await _db.Courses
        .Where(c => c.IsPublished)
        .ToListAsync(ct);  // Always pass ct
    
    return Result.Success(courses.Select(c => c.ToDto()).ToList());
}
```

---

## 3. TypeScript / Next.js Conventions

### 3.1 Naming

| Element | Convention | Example |
|---------|-----------|---------|
| File (component) | PascalCase | `CourseCard.tsx`, `ExamTimer.tsx` |
| File (utility) | camelCase | `formatDate.ts`, `apiClient.ts` |
| File (hook) | camelCase, `use` prefix | `useExamTimer.ts` |
| Component | PascalCase | `CourseCard`, `ExamTimer` |
| Function | camelCase | `formatDate`, `calculateScore` |
| Variable | camelCase | `courseList`, `isLoading` |
| Constant | SCREAMING_SNAKE | `MAX_RETRIES`, `API_BASE_URL` |
| Type/Interface | PascalCase | `CourseDto`, `ExamState` |
| Enum | PascalCase | `ExamMode.Practice` |
| CSS class (Tailwind) | kebab-case (standard) | `text-primary-600` |

### 3.2 Component Structure

```typescript
// components/CourseCard.tsx

// 1. Imports (external → internal → types → styles)
import { memo } from 'react';
import Image from 'next/image';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import { cn } from '@/lib/utils';
import type { CourseDto } from '@/types/course';

// 2. Props interface (inline for simple, extracted for complex)
interface CourseCardProps {
  course: CourseDto;
  showProgress?: boolean;
  className?: string;
}

// 3. Component (named export, not default)
export const CourseCard = memo(function CourseCard({
  course,
  showProgress = true,
  className,
}: CourseCardProps) {
  // 4. Hooks first
  const router = useRouter();
  
  // 5. Derived values
  const progressPct = Math.round(
    (course.completedLectures / course.totalLectures) * 100
  );
  
  // 6. Handlers
  const handleClick = () => {
    router.push(`/courses/${course.slug}`);
  };
  
  // 7. Render
  return (
    <Card className={cn('cursor-pointer hover:shadow-md transition-shadow', className)}>
      {/* ... */}
    </Card>
  );
});
```

### 3.3 Type Safety

```typescript
// DO: Define explicit types for API responses
interface ApiResponse<T> {
  data: T;
  meta?: { page: number; pageSize: number; total: number };
}

// DO: Use discriminated unions for state
type ExamState =
  | { status: 'loading' }
  | { status: 'ready'; questions: Question[] }
  | { status: 'in-progress'; questions: Question[]; currentIndex: number }
  | { status: 'submitted'; result: ExamResult }
  | { status: 'error'; message: string };

// DON'T: Use `any`
const data: any = await response.json(); // ❌ Never

// DON'T: Use type assertions without reason
const user = data as User; // ❌ Prefer type guards or Zod parsing
```

### 3.4 API Data Validation (Zod)

```typescript
// schemas/course.ts
import { z } from 'zod';

export const courseSchema = z.object({
  id: z.string().uuid(),
  title: z.string().min(1).max(200),
  slug: z.string().regex(/^[a-z0-9]+(?:-[a-z0-9]+)*$/),
  description: z.string().max(5000).nullable(),
  thumbnailUrl: z.string().url().nullable(),
  examTrack: z.string(),
  isPublished: z.boolean(),
  lectureCount: z.number().int().nonnegative(),
});

export type CourseDto = z.infer<typeof courseSchema>;

// Usage — validate API response at boundary
const response = await api.get('/courses');
const courses = z.array(courseSchema).parse(response.data);
```

### 3.5 Import Order

```typescript
// Enforced by ESLint (eslint-plugin-import)
// 1. React / Next.js
import { useState, useEffect } from 'react';
import Link from 'next/link';

// 2. External libraries
import { useQuery } from '@tanstack/react-query';
import { z } from 'zod';

// 3. Internal aliases (@/)
import { Button } from '@/components/ui/button';
import { apiClient } from '@/lib/api';

// 4. Relative imports
import { ExamTimer } from './ExamTimer';

// 5. Types (with `type` keyword)
import type { CourseDto } from '@/types/course';

// 6. Styles/assets (if any)
```

---

## 4. SQL / Database Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Table name | snake_case, plural | `courses`, `exam_results` |
| Column name | snake_case | `first_name`, `created_at` |
| Primary key | `id` | `id UUID PRIMARY KEY` |
| Foreign key | `{related_table}_id` | `course_id`, `user_id` |
| Boolean column | `is_` prefix | `is_active`, `is_published` |
| Timestamp column | `_at` suffix | `created_at`, `deleted_at` |
| Index name | `ix_{table}_{columns}` | `ix_courses_exam_track_id` |
| Constraint name | `ck_{table}_{rule}` | `ck_transactions_amount_positive` |

---

## 5. Git Conventions

### 5.1 Branch Naming

```
main              — Production-ready
develop           — Integration branch (optional)
feature/F-023-payment-checkout
fix/ISS-034-exam-timer-bug
hotfix/payment-webhook-signature
chore/update-dependencies
docs/api-documentation
```

### 5.2 Commit Messages (Conventional Commits)

```
<type>(<scope>): <short description>

[optional body]

[optional footer]

Types:
  feat     — New feature
  fix      — Bug fix
  refactor — Code restructure (no behavior change)
  test     — Adding or updating tests
  docs     — Documentation only
  chore    — Build, CI, tooling
  perf     — Performance improvement
  style    — Formatting (no logic change)

Examples:
  feat(payment): add JazzCash server-side integration
  fix(exam): correct score calculation for negative marking
  refactor(catalog): extract course query into dedicated service
  test(auth): add login integration tests
  chore(ci): add Playwright E2E pipeline
```

### 5.3 PR Guidelines

- Title follows commit convention: `feat(payment): add checkout API`
- Description includes: What, Why, How, Testing
- Max 400 lines changed per PR (split larger work)
- All CI checks must pass
- At least 1 approval required

---

## 6. Formatting & Linting Enforcement

### 6.1 C# (.editorconfig)

```ini
# .editorconfig
root = true

[*.cs]
indent_style = space
indent_size = 4
end_of_line = lf
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true

# Naming rules
dotnet_naming_rule.private_fields_should_be_camel_case.severity = error
dotnet_naming_rule.private_fields_should_be_camel_case.symbols = private_fields
dotnet_naming_rule.private_fields_should_be_camel_case.style = camel_case_prefix

dotnet_naming_symbols.private_fields.applicable_kinds = field
dotnet_naming_symbols.private_fields.applicable_accessibilities = private, private_protected

dotnet_naming_style.camel_case_prefix.required_prefix = _
dotnet_naming_style.camel_case_prefix.capitalization = camel_case

# Using directives
csharp_using_directive_placement = outside_namespace
dotnet_sort_system_directives_first = true

# Use file-scoped namespaces
csharp_style_namespace_declarations = file_scoped
```

### 6.2 TypeScript (ESLint + Prettier)

```javascript
// eslint.config.mjs
export default [
  {
    extends: [
      'next/core-web-vitals',
      'next/typescript',
      'plugin:@typescript-eslint/strict',
    ],
    rules: {
      // No any
      '@typescript-eslint/no-explicit-any': 'error',
      // Prefer type imports
      '@typescript-eslint/consistent-type-imports': 'error',
      // No unused variables
      '@typescript-eslint/no-unused-vars': ['error', { argsIgnorePattern: '^_' }],
      // Force named exports for components
      'import/no-default-export': 'error',
      // Consistent import order
      'import/order': ['error', {
        groups: ['builtin', 'external', 'internal', 'parent', 'sibling', 'type'],
        'newlines-between': 'always',
      }],
    },
    overrides: [
      {
        // Allow default exports for Next.js pages and layouts
        files: ['**/page.tsx', '**/layout.tsx', '**/loading.tsx', '**/error.tsx'],
        rules: { 'import/no-default-export': 'off' },
      },
    ],
  },
];
```

### 6.3 Pre-Commit Hooks (Husky + lint-staged)

```json
// package.json
{
  "lint-staged": {
    "*.{ts,tsx}": ["eslint --fix", "prettier --write"],
    "*.css": ["prettier --write"],
    "*.cs": ["dotnet format --include"]
  }
}
```

---

## 7. Documentation Requirements

| Code Element | Documentation Required |
|-------------|:-----:|
| Public API endpoints | ✅ XML doc comments + OpenAPI |
| Public classes/interfaces | ✅ XML summary |
| Non-obvious business logic | ✅ Inline comments explaining WHY |
| Complex algorithms | ✅ Comment block with approach |
| Simple CRUD operations | ❌ Self-documenting code is sufficient |
| Private helper methods | ❌ Unless complex |
| React components | ✅ Props documented via TypeScript interface |
| Custom hooks | ✅ JSDoc with usage example |

```csharp
/// <summary>
/// Calculates the exam score including negative marking adjustments.
/// </summary>
/// <param name="answers">The student's submitted answers.</param>
/// <param name="negativeMarkingFactor">
/// Penalty per wrong answer as a fraction (e.g., 0.25 = subtract 25% of mark value).
/// Set to 0 for no negative marking.
/// </param>
/// <returns>The calculated score as a percentage (0-100).</returns>
public decimal CalculateScore(
    List<SubmittedAnswer> answers, 
    decimal negativeMarkingFactor = 0)
```

---

*Coding standards exist to reduce cognitive load. When every file follows the same patterns, new code is predictable. Automated enforcement (linters, formatters, pre-commit hooks, CI checks) ensures these standards are followed without manual review burden.*
