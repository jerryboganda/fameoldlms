# 16 — Data Migration Strategy (V1 → V2)

*Complete plan for migrating from SQL Server + ASP.NET Identity to PostgreSQL, including user accounts, content, progress, and payments.*

---

## 1. Migration Scope

### 1.1 What Migrates

| V1 Table Group | Records (Est.) | V2 Target | Priority |
|----------------|:--------------:|-----------|:--------:|
| AspNetUsers + AspNetRoles | ~20,000 | Identity module | P0 |
| AspNetUserRoles | ~20,000 | Identity module | P0 |
| Courses + related content | ~500 | Catalog module | P0 |
| MCQ Questions + Options | 20,000+ Q / 77,708+ opts | Assessment module | P0 |
| ExamResults | ~50,000 | Assessment module | P0 |
| Subscriptions / Payments | ~15,000 | Payment module | P0 |
| Enrollments / Access | ~20,000 | Enrollment module | P0 |
| Student Progress | ~100,000 | Enrollment module | P1 |
| Blog posts | ~200 | Content module | P1 |
| Chat messages | ~50,000 | Communication module | P2 |
| Images / Files | ~10 GB | S3-compatible storage | P0 |

### 1.2 What Does NOT Migrate

- Session data (ephemeral)
- Temporary upload files
- Cache data
- Old error logs
- Orphaned records (no parent FK)

---

## 2. Migration Architecture

```
┌─────────────────┐     ┌─────────────────┐     ┌──────────────────┐
│   SQL Server     │     │  ETL Pipeline    │     │   PostgreSQL 17   │
│   (V1 FAME_DB)  │────►│  (C# Console)   │────►│   (V2 fame_db)   │
│                  │     │                  │     │                  │
│  96+ tables      │     │  Transform:      │     │  36 entities     │
│  int PKs         │     │  - Map IDs       │     │  Guid v7 PKs     │
│  ASP.NET         │     │  - Hash passwords│     │  EF Core 9       │
│  Identity v2     │     │  - Normalize     │     │                  │
└─────────────────┘     │  - Validate      │     └──────────────────┘
                        └─────────────────┘
                               │
                        ┌──────┴──────┐
                        │  ID Mapping  │
                        │  Table       │
                        │  (int→Guid)  │
                        └─────────────┘
```

---

## 3. ID Mapping Strategy

V1 uses `int` auto-increment primary keys. V2 uses `Guid v7` (time-sortable). The migration maintains a mapping table:

```csharp
// Migration/Models/IdMapping.cs
public sealed class IdMapping
{
    public string EntityType { get; set; } = string.Empty;  // "User", "Course", etc.
    public int V1Id { get; set; }
    public Guid V2Id { get; set; }
    public DateTimeOffset MigratedAt { get; set; }
}

// Usage during migration
public Guid GetOrCreateV2Id(string entityType, int v1Id)
{
    var existing = _mappings.FirstOrDefault(m => 
        m.EntityType == entityType && m.V1Id == v1Id);
    
    if (existing is not null)
        return existing.V2Id;
    
    var newId = Guid.CreateVersion7();
    _mappings.Add(new IdMapping
    {
        EntityType = entityType,
        V1Id = v1Id,
        V2Id = newId,
        MigratedAt = DateTimeOffset.UtcNow,
    });
    
    return newId;
}
```

---

## 4. User Account Migration

### 4.1 Password Migration (Dual-Hash)

V1 uses ASP.NET Identity v2 password hashing (PBKDF2-HMAC-SHA256). V2 uses Argon2id. Strategy:

```csharp
// Phase 1: Import with V1 hash preserved
public sealed class MigratedUser
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string? V1PasswordHash { get; set; }    // Old format, encrypted at rest
    public string? V2PasswordHash { get; set; }    // New Argon2id format (null initially)
    public bool PasswordMigrated { get; set; }     // false initially
}

// Phase 2: On first login, verify with V1 hash, then upgrade
public async Task<LoginResult> Login(string email, string password)
{
    var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
    if (user is null) return LoginResult.Failed();
    
    if (!user.PasswordMigrated && user.V1PasswordHash is not null)
    {
        // Verify using V1 ASP.NET Identity v2 hasher
        var v1Result = _v1Hasher.VerifyHashedPassword(user, user.V1PasswordHash, password);
        if (v1Result == PasswordVerificationResult.Success)
        {
            // Upgrade to Argon2id
            user.V2PasswordHash = _argon2Hasher.HashPassword(password);
            user.V1PasswordHash = null;  // Remove old hash
            user.PasswordMigrated = true;
            await _db.SaveChangesAsync();
            
            return LoginResult.Success(user);
        }
        return LoginResult.Failed();
    }
    
    // V2 password — normal verification
    if (_argon2Hasher.VerifyPassword(user.V2PasswordHash!, password))
        return LoginResult.Success(user);
    
    return LoginResult.Failed();
}
```

### 4.2 User Data Mapping

```
V1 AspNetUsers              V2 Users
────────────────            ──────────────
Id (int)            ──→     Id (Guid v7)
Email               ──→     Email
UserName            ──→     Email (use email as username)
PasswordHash        ──→     V1PasswordHash
FirstName           ──→     FirstName
LastName            ──→     LastName
PhoneNumber         ──→     PhoneNumber
EmailConfirmed      ──→     EmailVerifiedAt (DateTime if true, null if false)
LockoutEnabled      ──→     IsActive (inverted)
CreatedDate         ──→     CreatedAt
ProfileImage        ──→     AvatarUrl (upload to S3, update path)

V1 AspNetUserRoles          V2 UserRoles
────────────────            ──────────────
UserId + RoleId     ──→     UserId (mapped) + Role (enum)

V1 Role Mapping:
  Admin       → SuperAdmin (first admin) or Admin
  Teacher     → Instructor
  Assistant   → ContentEditor
  UniTeacher  → Instructor
  SuppAgent   → SupportAgent
  Student     → Student
```

---

## 5. Content Migration

### 5.1 Course Structure Mapping

```
V1 Courses                   V2 Courses
────────────                 ──────────────
Course_Id (int)      ──→     Id (Guid v7)
Course_Name          ──→     Title
Course_Desc          ──→     Description
Course_Pic           ──→     ThumbnailUrl (re-upload to S3)
ExamTrack_Id         ──→     ExamTrackId (mapped)
IsActive             ──→     IsPublished
SortOrder            ──→     DisplayOrder
                     NEW     Slug (generated from title)
                     NEW     DurationMinutes (summed from lectures)
```

### 5.2 MCQ Migration

```csharp
// MCQ migration — preserve question/option integrity
public async Task MigrateQuestions(SqlConnection v1Db, FameDbContext v2Db)
{
    var v1Questions = await v1Db.QueryAsync<V1Question>(
        "SELECT * FROM tbl_Question WHERE IsDeleted = 0");
    
    foreach (var v1Q in v1Questions)
    {
        var v2CourseId = GetOrCreateV2Id("Course", v1Q.Course_Id);
        
        var question = new Question
        {
            Id = GetOrCreateV2Id("Question", v1Q.Question_Id),
            CourseId = v2CourseId,
            Text = CleanHtml(v1Q.Question_Text),
            Explanation = CleanHtml(v1Q.Explanation),
            Difficulty = MapDifficulty(v1Q.Difficulty_Level),
            IsActive = v1Q.IsActive,
        };
        
        // Migrate options
        var v1Options = await v1Db.QueryAsync<V1Option>(
            "SELECT * FROM tbl_Option WHERE Question_Id = @Id", 
            new { Id = v1Q.Question_Id });
        
        foreach (var v1Opt in v1Options)
        {
            question.Options.Add(new QuestionOption
            {
                Id = GetOrCreateV2Id("Option", v1Opt.Option_Id),
                Text = CleanHtml(v1Opt.Option_Text),
                IsCorrect = v1Opt.IsCorrect,
                DisplayOrder = v1Opt.SortOrder,
            });
        }
        
        v2Db.Questions.Add(question);
    }
    
    await v2Db.SaveChangesAsync();
    _logger.LogInformation("Migrated {Count} questions", v1Questions.Count());
}
```

---

## 6. File / Image Migration

```csharp
// Migrate all images from local filesystem to S3-compatible storage
public async Task MigrateFiles()
{
    var sourceDir = @"C:\firstaidmadeeasy.com.pk\Images";
    var files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
    
    var migrated = 0;
    var skipped = 0;
    
    foreach (var localPath in files)
    {
        var relativePath = Path.GetRelativePath(sourceDir, localPath);
        var s3Key = $"images/{relativePath.Replace('\\', '/')}";
        
        // Skip system files
        if (IsProtectedFile(relativePath))
        {
            skipped++;
            continue;
        }
        
        await using var stream = File.OpenRead(localPath);
        var contentType = GetContentType(localPath);
        
        await _s3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = "fame-assets",
            Key = s3Key,
            InputStream = stream,
            ContentType = contentType,
            CannedACL = S3CannedACL.PublicRead,
        });
        
        migrated++;
        
        if (migrated % 100 == 0)
            _logger.LogInformation("Migrated {Count} files...", migrated);
    }
    
    _logger.LogInformation("File migration complete: {Migrated} migrated, {Skipped} skipped",
        migrated, skipped);
}
```

---

## 7. Migration Execution Plan

### 7.1 Pre-Migration (T-7 days)

```
1. ☐ Deploy V2 to staging environment
2. ☐ Run migration against V1 backup (not production)
3. ☐ Validate migrated data
    - User count matches
    - Course count matches
    - Question/option count matches
    - Payment records match
    - Random sample of 100 users: login works
4. ☐ Performance test with migrated data
5. ☐ Fix any migration issues found
6. ☐ Run migration again on fresh V1 backup
7. ☐ Final validation
```

### 7.2 Migration Day (T-0)

```
Timeline (Saturday night, low traffic):

22:00  1. ☐ Enable maintenance mode on V1
       2. ☐ Take final V1 database backup
       3. ☐ Take final V1 file system backup
       
22:15  4. ☐ Run ETL pipeline against fresh V1 backup
       5. ☐ Monitor migration progress (est. 30-60 min)
       
23:15  6. ☐ Run validation suite
       7. ☐ Verify user count, content count, payment count
       
23:30  8. ☐ Migrate files to S3 (parallel upload, est. 20 min for 10GB)
       
23:50  9. ☐ Update DNS to point to V2
       10. ☐ Deploy V2 with production config
       
00:00  11. ☐ Smoke test all critical flows:
            - Student login (V1 password)
            - Course catalog loads
            - Exam taking works
            - Payment checkout loads
            - Admin login works
            
00:15  12. ☐ Disable maintenance mode → V2 is live
       13. ☐ Monitor error rates for 2 hours
       
02:00  14. ☐ All clear — migration complete
```

### 7.3 Rollback Plan

If critical issues found during migration:

```
1. Revert DNS to V1
2. Disable V2
3. V1 is still running with pre-migration data
4. Any transactions during V2 window: manually reconcile
```

Keep V1 running (read-only) for 30 days post-migration as fallback.

---

## 8. Data Validation Queries

```sql
-- Post-migration validation queries (run against V2 PostgreSQL)

-- User count should match V1 (minus orphaned/test accounts)
SELECT 'Users' AS entity, COUNT(*) AS v2_count FROM users;

-- Role distribution
SELECT role, COUNT(*) FROM user_roles GROUP BY role ORDER BY count DESC;

-- Course count
SELECT 'Courses' AS entity, COUNT(*) AS v2_count FROM courses WHERE NOT is_deleted;

-- Question count
SELECT 'Questions' AS entity, COUNT(*) AS v2_count FROM questions WHERE NOT is_deleted;

-- Option count (should be ~4× questions)
SELECT 'Options' AS entity, COUNT(*) AS v2_count FROM question_options;

-- Payment records
SELECT 'Transactions' AS entity, COUNT(*) AS v2_count FROM transactions;

-- Verify no broken foreign keys
SELECT 'Orphaned enrollments' AS check_name,
       COUNT(*) AS count
FROM enrollments e
LEFT JOIN users u ON e.user_id = u.id
WHERE u.id IS NULL;

-- Verify password migration readiness
SELECT 
    COUNT(*) FILTER (WHERE v1_password_hash IS NOT NULL) AS pending_migration,
    COUNT(*) FILTER (WHERE v2_password_hash IS NOT NULL) AS already_migrated,
    COUNT(*) AS total
FROM users;
```

---

## 9. Post-Migration Monitoring

| Metric | First 24h Target | Action if Breached |
|--------|:----------------:|---------------------|
| Login success rate | >95% | Check password migration, review V1 hash format edge cases |
| Error rate (5xx) | <1% | Check logs in Seq, identify failing queries |
| Page load time | <3s P95 | Check database query performance, missing indexes |
| Payment success | Same as V1 average | Verify gateway integration, check webhook URLs |
| User reports | <5 support tickets about data issues | Triage individually |

---

*Migration is the riskiest phase of the V2 transition. The dual-hash password strategy ensures zero password resets. The ETL pipeline is idempotent — it can be run multiple times safely. The rollback plan keeps V1 available for 30 days.*
