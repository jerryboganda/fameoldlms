# Ambassador Portal - Project Memory Report

**Date:** February 2, 2026  
**Project:** FAME LMS (First Aid Made Easy)  
**Purpose:** Design context for Ambassador Portal feature

---

## 1. SOLUTION STRUCTURE

### Architecture Type
- **Pattern:** ASP.NET MVC 5 (Classic .NET Framework 4.x)
- **Routing:** Convention-based MVC routing with Areas
- **NOT**: ASP.NET Core, Razor Pages standalone

### Key Paths
| Component | Path |
|-----------|------|
| Solution | `First Aid Made Easy.sln` |
| Web Project | `FAME.Web/` |
| Controllers | `FAME.Web/Controllers/` |
| Views | `FAME.Web/Views/` |
| Business Logic | `FAME.Web/BLL/` |
| Interfaces | `FAME.Web/BLL/Interfaces/` |
| Data Access | `FAME.Web/DAL/` |
| Models/ViewModels | `FAME.Web/Models/` |
| Areas | `FAME.Web/Areas/` (Landing, Blogs, DailyReport, FileManager) |
| Shared Views | `FAME.Web/Views/Shared/` |
| Filters | `FAME.Web/Filters/` |

### Existing Areas Pattern
```
Areas/
├── Landing/
│   ├── Controllers/
│   ├── Views/
│   └── LandingAreaRegistration.cs
├── Blogs/
├── DailyReport/
└── FileManager/
```

---

## 2. AUTHENTICATION & AUTHORIZATION

### Identity System
- **Framework:** ASP.NET Identity 2.x with OWIN
- **User Class:** `ApplicationUser : IdentityUser` (`Models/IdentityModels.cs`)
- **DbContext:** `ApplicationDbContext : IdentityDbContext<ApplicationUser>`
- **Connection:** `DefaultConnection` in Web.config

### Custom User Extension
- `tbl_User` in `DAL/` extends user profile (Institute, Country, CNIC, MockTestType, etc.)
- Linked via `User_AspUser` → `AspNetUsers.Id`

### Roles (from `BLL/ENUM.cs`)
```csharp
public enum Roles { Student = 1, Teacher = 2, Admin = 3, Assistant = 4, SuppAgent = 5 }
```
- **No existing "Ambassador" role** → Must add

### Authorization Pattern
- `[Authorize]` attribute on controllers
- `[BrowserFilter]` custom action filter on all controllers
- Role checks via `User.IsInRole("Admin")` in views/controllers
- No claims-based or policy-based authorization

---

## 3. DATA ACCESS PATTERNS

### ORM: Entity Framework 6 (Database-First)
- **EDMX Model:** `DAL/Model1.edmx`
- **Context:** `FAMEEntities` (auto-generated)
- **Tables:** Prefixed with `tbl_` (e.g., `tbl_User`, `tbl_Package`, `tbl_EnrollmentMaster`)

### Repository Pattern
- All business logic in `BLL/` folder
- Interfaces in `BLL/Interfaces/` (e.g., `IUserRepository`, `IEnrollmentRepository`)
- Each repository instantiates `FAMEEntities` with `using` blocks
- **No Unit of Work** pattern

### Example Repository Pattern
```csharp
public class UserRepository : IUserRepository
{
    public UserVM GetProfile(string id)
    {
        using (FAMEEntities db = new FAMEEntities())
        {
            // Direct EF queries
        }
    }
}
```

---

## 4. EXISTING RELEVANT ENTITIES

### Users & Students
- `AspNetUsers` - Identity users
- `tbl_User` - Extended profile (Institute, Country, Type/UserType)

### Packages & Subscriptions
- `tbl_Package` - Subscription packages (PackageID, PackageName, PackagePrice, Duration)
- `tbl_PackageDuration` - Duration variants per package
- `tbl_PackageDetail` - Package → Course mappings

### Enrollments (Subscriptions)
- `tbl_EnrollmentMaster` - Main enrollment record
  - StudentFid, TeacherFid, Enrollment_Date, Enrollment_EndDate
  - PackageId, PackageDurationFid, CouponID
  - Enrollment_Status: Pending/Approved/Rejected/Continue/Expired
  - Enrollment_Price
- `tbl_EnrollmentDetail` - Course/Section details per enrollment

### Coupons (Partial Referral System)
- `tbl_Coupon` - Discount codes
  - CouponSecret, ExpiryDate, Type, Amount/DiscountPer
  - NoOfUses, RemUses, CreatedBy
  - ForCourse, CourseFid, SectionID
- **NOT** tied to referral tracking → Extension point

### Payments/Installments
- `tbl_Installments` - Payment schedule definitions
- `tbl_StudentInstallments` - Student payment tracking (IsPaid, InstallmentDate)

### No Existing Referral/Affiliate Tables
- Confirmed: No tables for ambassadors, referrals, commissions, or payouts

---

## 5. UI FRAMEWORK

### CSS/Design System
- **Bootstrap 4** (CDN-based)
- **Custom Theme:** Luma/Stack Dashboard theme (`Content/assets/`)
- **Icons:** Material Icons + Font Awesome
- **Perfect Scrollbar** for sidebars
- **DataTables** for grids
- **Toastr** for notifications
- **Select2** for dropdowns

### Layout Files
| Layout | Purpose |
|--------|---------|
| `_LayoutTeacher.cshtml` | Admin/Teacher panel (main backend) |
| `_LayoutStudent2026.cshtml` | Student portal |
| `_LayoutNew.cshtml` | General/public pages |
| `_LayoutAgent.cshtml` | Support agent |
| `_LayoutFree.cshtml` | Minimal layout |

### Shared Partials (Teacher/Admin)
- `_TeacherHeader.cshtml` - Top navigation
- `_TeacherAside.cshtml` - Sidebar menu (role-based sections)
- `_TeacherFooter.cshtml`
- `_TeacherStyles.cshtml` - CSS includes
- `_TeacherScripts.cshtml` - JS includes
- `_TeacherModals.cshtml` - Modal templates

### Sidebar Pattern (from `_TeacherAside.cshtml`)
```html
@if (User.IsInRole("Admin"))
{
    <div class="sidebar-heading">Section Name</div>
    <ul class="sidebar-menu">
        <li class="sidebar-menu-item">
            <a class="sidebar-menu-button" href="/Controller/Action">
                <span class="material-icons sidebar-menu-icon sidebar-menu-icon--left">icon_name</span>
                <span class="sidebar-menu-text">Menu Item</span>
            </a>
        </li>
    </ul>
}
```

---

## 6. DEPENDENCY INJECTION

### DI Container: Autofac
- Configuration: `App_Start/AutofacConfig.cs`
- Registration: `InstancePerRequest` lifecycle

### Registration Pattern
```csharp
builder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerRequest();
```

### Controller Injection
```csharp
public class AdminController : Controller
{
    private readonly IDashBoardRepository _dashBoardRepository;
    
    public AdminController(IDashBoardRepository dashBoardRepository, ...)
    {
        _dashBoardRepository = dashBoardRepository;
    }
}
```

---

## 7. CODING STANDARDS

### Naming Conventions
- Tables: `tbl_EntityName` (e.g., `tbl_Package`)
- ViewModels: `EntityNameVM` (e.g., `PackageVM`)
- Repositories: `EntityRepository : IEntityRepository`
- Controllers: `EntityController`
- FK columns: `EntityFid` or `EntityID`

### Validation
- Data Annotations on ViewModels
- Server-side validation in controllers
- Client-side via jQuery Validation

### Logging
- **Serilog** (`BLL/Logger.cs`)
- Levels: Error, Warning, Debug, Verbose, Fatal
- File-based rolling logs in `~/ErrorLog/`

### Error Handling
- Try-catch in repositories (often `throw;` to bubble up)
- No global exception middleware
- Custom error views in `Views/Shared/`

### Localization
- **Not implemented** - English only

---

## 8. INTEGRATION POINTS FOR AMBASSADOR PORTAL

### Safe Extension Strategy

1. **New Area:** Create `Areas/Ambassador/` following existing pattern
2. **New Tables:** Add via SQL scripts (match `tbl_` prefix)
3. **EDMX Update:** Regenerate from database or add partial classes
4. **New Repository:** `AmbassadorRepository : IAmbassadorRepository`
5. **DI Registration:** Add to `AutofacConfig.cs`
6. **Layout:** Use `_LayoutTeacher.cshtml` or create `_LayoutAmbassador.cshtml`
7. **Sidebar:** Add Ambassador menu section to `_TeacherAside.cshtml`
8. **Enrollment Hook:** Extend `EnrollmentRepository.Save()` for attribution

### Critical Hook Points

| Event | Location | Integration |
|-------|----------|-------------|
| User Registration | `AccountController.Register()` | Capture referral code from cookie/query |
| Enrollment Creation | `EnrollmentRepository.Save()` | Create referral conversion record |
| Payment Confirmation | Manual/Admin approval flow | Trigger commission calculation |

### Authorization Extension
```csharp
// Add to ENUM.cs
public enum Roles { ..., Ambassador = 6 }

// Add sidebar check in _TeacherAside.cshtml
@if (User.IsInRole("Ambassador"))
{
    // Ambassador menu
}
```

---

## 9. CHECKLIST FOR IMPLEMENTATION

- [ ] Create `Areas/Ambassador/` folder structure
- [ ] Add `AmbassadorAreaRegistration.cs`
- [ ] Create SQL migration script for new tables
- [ ] Update EDMX or add partial entity classes
- [ ] Create `IAmbassadorRepository` and `AmbassadorRepository`
- [ ] Create `ICommissionService` and `CommissionService`
- [ ] Create `IPayoutService` and `PayoutService`
- [ ] Register services in `AutofacConfig.cs`
- [ ] Add "Ambassador" role to seed/migration
- [ ] Create Ambassador controllers
- [ ] Create Razor views with existing layout
- [ ] Add sidebar menu items
- [ ] Hook into registration flow for referral capture
- [ ] Hook into enrollment flow for conversion tracking
- [ ] Add admin management pages
- [ ] Write unit tests

---

## 10. FILES TO MODIFY

| File | Changes |
|------|---------|
| `BLL/ENUM.cs` | Add `Ambassador = 6` to Roles enum |
| `App_Start/AutofacConfig.cs` | Register new repositories/services |
| `Views/Shared/_TeacherAside.cshtml` | Add Ambassador menu section |
| `Controllers/AccountController.cs` | Capture referral code on registration |
| `BLL/EnrollmentRepository.cs` | Hook conversion tracking on enrollment |
| `Database/` | New SQL migration script |
