# 25 — Team Structure and Roles

*The team composition needed to build and maintain V2, with role definitions, skill requirements, and scaling recommendations.*

---

## 1. Current Reality

| Attribute | Status |
|-----------|--------|
| Team size | **1** (solo developer / founder) |
| Bus factor | **1** — critical risk (R-P02) |
| Available hours | ~40-50 hrs/week |
| Primary skills | ASP.NET MVC, SQL Server, JavaScript, system administration |
| Skills to acquire | Next.js / React, PostgreSQL, Docker, CI/CD, TypeScript |

The entire V2 project is designed around the assumption of a **single developer** building the initial system, with the ability to scale the team later. Every architectural decision reflects this constraint.

---

## 2. Solo Developer Playbook

### Phase 0-1 (Weeks 1-14): Solo Build

The founder fills all roles:

```
┌─────────────────────────────────────────────┐
│              Solo Developer                  │
│                                              │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  │
│  │ Frontend  │  │ Backend  │  │  DevOps  │  │
│  │ (Next.js) │  │ (ASP.NET)│  │ (Docker) │  │
│  └──────────┘  └──────────┘  └──────────┘  │
│                                              │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  │
│  │ Database  │  │ Testing  │  │  Design  │  │
│  │ (EF Core) │  │ (xUnit)  │  │ (Tailwind│  │
│  └──────────┘  └──────────┘  └──────────┘  │
└─────────────────────────────────────────────┘
```

**Productivity Multipliers for Solo Mode**:

1. **This Blueprint** — 38-file comprehensive spec eliminates decision paralysis
2. **Copilot / AI** — Code generation, debugging, code review
3. **Automated Testing** — CI catches regressions; no manual QA needed
4. **Docker Compose** — One-command infrastructure; no manual server setup
5. **Templates & Scaffolding** — Consistent code generation from patterns
6. **Pre-built Components** — Radix UI + Tailwind = no custom CSS from scratch

### Recommended Weekly Schedule (Solo)

| Day | Focus Area | Output |
|-----|-----------|--------|
| **Monday** | Planning + Architecture | Sprint goals, technical spikes |
| **Tuesday** | Backend development | API endpoints, business logic |
| **Wednesday** | Backend + Database | Migrations, queries, integrations |
| **Thursday** | Frontend development | Pages, components, forms |
| **Friday** | Frontend + Integration | Connect frontend → backend, E2E tests |
| **Saturday** | Testing + DevOps | Test coverage, CI/CD, deployment |
| **Sunday** | Review + Documentation | Code review (self), docs, planning |

---

## 3. First Hire Recommendations

### When to Hire

| Trigger | Role Needed | Timeline |
|---------|-------------|----------|
| MVP launch approaching | QA / Testing contractor | Phase 1 Sprint 4 |
| Post-launch support needed | Part-time support agent | Phase 2 |
| Feature velocity slowing | Junior Full-Stack Developer | Phase 2 |
| Design quality lagging | UI/UX Designer (contract) | Phase 1 Sprint 3 |

### Hire #1: Junior Full-Stack Developer (Phase 2)

| Attribute | Requirement |
|-----------|-------------|
| **Experience** | 1-2 years |
| **Must have** | TypeScript, React, C#, basic SQL |
| **Nice to have** | Next.js, ASP.NET Core, Docker |
| **Compensation** | PKR 80,000-120,000/month (remote) |
| **Role** | Feature development, bug fixes, testing |

**Onboarding plan** (leveraging this blueprint):
1. Day 1: Read files 00, 04, 06, 19 (vision, architecture, backend, coding standards)
2. Day 2: Read files 05, 07, 15 (frontend, database, design system)
3. Day 3: Clone repo, run `docker compose up`, explore codebase
4. Day 4: Fix a small bug or implement a minor feature
5. Day 5: Code review discussion, assign first real task

### Hire #2: QA / Testing Contractor (Phase 1 Sprint 4)

| Attribute | Requirement |
|-----------|-------------|
| **Experience** | 2+ years QA |
| **Must have** | Manual testing, test case writing |
| **Nice to have** | Playwright, API testing |
| **Engagement** | 2-3 week contract |
| **Role** | Pre-launch testing, regression testing, UAT |

---

## 4. Scaled Team (Phase 3+)

When the product has paying users and revenue supports it:

```
┌──────────────────────────────────────────────────────┐
│                    Founder / CTO                      │
│              Architecture, Strategy, Code Review      │
├──────────────────────┬───────────────────────────────┤
│                      │                               │
│    ┌─────────────────┤                               │
│    │                 │                               │
│    ▼                 ▼                               │
│  Full-Stack        Full-Stack      UI/UX Designer   │
│  Developer #1      Developer #2    (Contract)       │
│  (Backend-lean)    (Frontend-lean)                   │
│                                                      │
│  - API endpoints   - React pages   - Figma designs  │
│  - Business logic  - Components    - User research  │
│  - Migrations      - State mgmt   - Accessibility   │
│  - Background jobs - Performance   - Design system  │
└──────────────────────────────────────────────────────┘
```

| Role | Count | Focus |
|------|:-----:|-------|
| Founder / CTO | 1 | Architecture, code review, DevOps, strategy |
| Full-Stack (Backend-lean) | 1 | API, business logic, database |
| Full-Stack (Frontend-lean) | 1 | UI, components, performance |
| UI/UX Designer (contract) | 0.5 | Design system, user flows, branding |
| QA (contract) | 0.25 | Pre-release testing cycles |
| **Total** | **~3** | |

---

## 5. External Resources (No Hire Needed)

| Need | Solution | Cost |
|------|----------|------|
| Code review | AI-assisted (Copilot) + self-review checklist | $0-19/mo |
| Design inspiration | Metronic templates, Tailwind UI | Already licensed |
| Medical content review | Subject matter experts (existing teachers) | $0 (existing relationships) |
| Server administration | Docker abstracts most ops; Founder handles rest | $0 |
| Legal / compliance | Consult lawyer for TOS, privacy policy | One-time $200-500 |
| Accounting | Freelance accountant for payment reconciliation | PKR 10-20K/month |

---

## 6. Skill Development Plan (Solo Phase)

The founder needs to bridge specific skill gaps:

| Skill | Current Level | Target Level | Learning Path |
|-------|:------------:|:------------:|---------------|
| Next.js / React | Beginner | Intermediate | Official Next.js tutorial → build 3 pages → iterate |
| TypeScript | Beginner | Intermediate | Learn alongside React; use strict mode |
| PostgreSQL | Familiar | Competent | Use pgAdmin; learn EXPLAIN ANALYZE; JSONB queries |
| Docker | Familiar | Competent | docker-compose for dev → production deployment |
| EF Core | Familiar (EF 6) | Competent | Code-first migrations; raw SQL for complex queries |
| GitHub Actions | Beginner | Competent | Start with provided CI/CD templates from file 14 |
| Tailwind CSS | Beginner | Competent | Utility-first approach; learn alongside component building |

**Estimated ramp-up time**: 2-3 weeks (Phase 0) with focused learning + building starter projects.

---

## 7. Communication & Collaboration

### Solo Phase

- **No standups needed** — use the todo list and this blueprint as the single source of truth
- **Git commits** — Conventional Commits format serves as a work log
- **Weekly review** — 30 min self-review of progress vs roadmap (file 17)

### 2-3 Person Team

| Ceremony | Frequency | Duration | Purpose |
|----------|:---------:|:--------:|---------|
| Standup | Daily (async) | 5 min text | What I did, what I'll do, blockers |
| Sprint Planning | Bi-weekly | 30 min | Select sprint backlog from epics |
| Code Review | Every PR | 15-30 min | Quality, standards, knowledge sharing |
| Retrospective | Monthly | 30 min | Process improvement |

### Tools

| Purpose | Tool | Cost |
|---------|------|:----:|
| Code | GitHub (private repo) | Free |
| Communication | Discord or Slack | Free |
| Task tracking | GitHub Issues + Projects | Free |
| Documentation | This blueprint + README files | Free |
| Design | Figma (free tier) | Free |

---

*The team strategy is deliberately conservative: start solo, leverage AI and automation, hire only when revenue justifies it. The comprehensive documentation in this blueprint ensures that the bus factor improves over time even before the first hire, because any competent developer can onboard using these 38 files.*
