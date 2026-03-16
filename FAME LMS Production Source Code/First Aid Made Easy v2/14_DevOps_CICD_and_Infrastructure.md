# 14 — DevOps, CI/CD, and Infrastructure

*Docker orchestration, GitHub Actions pipelines, environment management, deployment strategy, monitoring stack, and disaster recovery.*

---

## 1. Infrastructure Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                        Production VPS                                │
│  (8 vCPU, 16 GB RAM, 200 GB NVMe — Hetzner/DigitalOcean)           │
│                                                                     │
│  ┌───────────┐                                                      │
│  │   Caddy    │ :443 (TLS auto)                                     │
│  │  (reverse  │─────────┬──────────────────────────────────────┐    │
│  │   proxy)   │         │                                      │    │
│  └───────────┘         │                                      │    │
│                        ▼                                      ▼    │
│  ┌──────────────────────────┐  ┌────────────────────────────────┐  │
│  │    Next.js App           │  │    ASP.NET Core API             │  │
│  │    (Container)           │  │    (Container)                  │  │
│  │    :3000                 │  │    :5000                        │  │
│  └──────────────────────────┘  └────────────┬───────────────────┘  │
│                                              │                      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────┴───────┐             │
│  │    Redis      │  │  Meilisearch │  │  PostgreSQL   │             │
│  │    :6379      │  │    :7700     │  │    :5432      │             │
│  └──────────────┘  └──────────────┘  └──────────────┘             │
│                                                                     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐             │
│  │    Seq        │  │  Prometheus  │  │   Grafana    │             │
│  │   (Logging)   │  │  (Metrics)   │  │ (Dashboards) │             │
│  │    :5341      │  │    :9090     │  │    :3001     │             │
│  └──────────────┘  └──────────────┘  └──────────────┘             │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 2. Docker Compose Configuration

### 2.1 Production Docker Compose

```yaml
# docker-compose.prod.yml
version: "3.9"

services:
  # ------ Reverse Proxy ------
  caddy:
    image: caddy:2-alpine
    restart: unless-stopped
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./Caddyfile:/etc/caddy/Caddyfile:ro
      - caddy_data:/data
      - caddy_config:/config
    depends_on:
      - web
      - api
    networks:
      - frontend

  # ------ Frontend ------
  web:
    build:
      context: ./packages/web
      dockerfile: Dockerfile
      target: runner
    restart: unless-stopped
    environment:
      - NODE_ENV=production
      - NEXT_PUBLIC_API_URL=https://api.firstaidmadeeasy.com.pk
    expose:
      - "3000"
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:3000/api/health"]
      interval: 30s
      timeout: 5s
      retries: 3
    networks:
      - frontend

  # ------ Backend API ------
  api:
    build:
      context: .
      dockerfile: src/FAME.Api/Dockerfile
      target: final
    restart: unless-stopped
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Default=${DB_CONNECTION_STRING}
      - ConnectionStrings__Redis=${REDIS_CONNECTION_STRING}
      - Jwt__SigningKey=${JWT_SIGNING_KEY}
      - Payment__JazzCash__MerchantId=${JAZZCASH_MERCHANT_ID}
      - Payment__JazzCash__Password=${JAZZCASH_PASSWORD}
      - Payment__JazzCash__IntegritySalt=${JAZZCASH_SALT}
      - Payment__Stripe__SecretKey=${STRIPE_SECRET_KEY}
      - Payment__Stripe__WebhookSecret=${STRIPE_WEBHOOK_SECRET}
      - Email__SendGrid__ApiKey=${SENDGRID_API_KEY}
      - Storage__R2__AccessKey=${R2_ACCESS_KEY}
      - Storage__R2__SecretKey=${R2_SECRET_KEY}
    expose:
      - "5000"
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/health"]
      interval: 30s
      timeout: 5s
      retries: 3
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
    networks:
      - frontend
      - backend

  # ------ Database ------
  postgres:
    image: postgres:17-alpine
    restart: unless-stopped
    environment:
      POSTGRES_DB: fame_db
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./Database/init:/docker-entrypoint-initdb.d:ro
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER} -d fame_db"]
      interval: 10s
      timeout: 5s
      retries: 5
    expose:
      - "5432"
    networks:
      - backend

  # ------ Cache ------
  redis:
    image: redis:7-alpine
    restart: unless-stopped
    command: redis-server --requirepass ${REDIS_PASSWORD} --maxmemory 512mb --maxmemory-policy allkeys-lru
    volumes:
      - redis_data:/data
    healthcheck:
      test: ["CMD", "redis-cli", "-a", "${REDIS_PASSWORD}", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5
    expose:
      - "6379"
    networks:
      - backend

  # ------ Search ------
  meilisearch:
    image: getmeili/meilisearch:v1.10
    restart: unless-stopped
    environment:
      MEILI_MASTER_KEY: ${MEILI_MASTER_KEY}
      MEILI_ENV: production
    volumes:
      - meili_data:/meili_data
    expose:
      - "7700"
    networks:
      - backend

  # ------ Observability ------
  seq:
    image: datalust/seq:latest
    restart: unless-stopped
    environment:
      ACCEPT_EULA: "Y"
    volumes:
      - seq_data:/data
    expose:
      - "5341"
    networks:
      - backend

  prometheus:
    image: prom/prometheus:latest
    restart: unless-stopped
    volumes:
      - ./ops/prometheus.yml:/etc/prometheus/prometheus.yml:ro
      - prometheus_data:/prometheus
    expose:
      - "9090"
    networks:
      - backend

  grafana:
    image: grafana/grafana:latest
    restart: unless-stopped
    environment:
      GF_SECURITY_ADMIN_PASSWORD: ${GRAFANA_PASSWORD}
    volumes:
      - grafana_data:/var/lib/grafana
      - ./ops/grafana/dashboards:/etc/grafana/provisioning/dashboards:ro
    expose:
      - "3001"
    networks:
      - backend
      - frontend  # Accessible via Caddy

volumes:
  caddy_data:
  caddy_config:
  postgres_data:
  redis_data:
  meili_data:
  seq_data:
  prometheus_data:
  grafana_data:

networks:
  frontend:
    driver: bridge
  backend:
    driver: bridge
    internal: true  # No external access to data services
```

### 2.2 Caddyfile

```caddyfile
firstaidmadeeasy.com.pk, www.firstaidmadeeasy.com.pk {
    # Security headers
    header {
        Strict-Transport-Security "max-age=63072000; includeSubDomains; preload"
        X-Content-Type-Options "nosniff"
        X-Frame-Options "DENY"
        Referrer-Policy "strict-origin-when-cross-origin"
        -Server
    }

    # API routes → ASP.NET Core
    handle /api/* {
        reverse_proxy api:5000
    }

    # SignalR hubs
    handle /hubs/* {
        reverse_proxy api:5000
    }

    # Everything else → Next.js
    handle {
        reverse_proxy web:3000
    }
}

# API subdomain (optional)
api.firstaidmadeeasy.com.pk {
    reverse_proxy api:5000
}

# Grafana (admin only, behind auth)
grafana.firstaidmadeeasy.com.pk {
    basicauth {
        admin {$GRAFANA_BASIC_AUTH_HASH}
    }
    reverse_proxy grafana:3001
}
```

---

## 3. Dockerfiles

### 3.1 Next.js Dockerfile

```dockerfile
# packages/web/Dockerfile
FROM node:22-alpine AS base
RUN corepack enable

FROM base AS builder
WORKDIR /app
COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./
COPY packages/web/package.json ./packages/web/
RUN pnpm install --frozen-lockfile

COPY packages/web ./packages/web
RUN pnpm --filter web build

FROM base AS runner
WORKDIR /app
ENV NODE_ENV=production

RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 nextjs

COPY --from=builder --chown=nextjs:nodejs /app/packages/web/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/packages/web/.next/static ./.next/static
COPY --from=builder --chown=nextjs:nodejs /app/packages/web/public ./public

USER nextjs
EXPOSE 3000
ENV PORT=3000
CMD ["node", "server.js"]
```

### 3.2 ASP.NET Core Dockerfile

```dockerfile
# src/FAME.Api/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy csproj files for restore
COPY src/FAME.Api/FAME.Api.csproj src/FAME.Api/
COPY src/FAME.SharedKernel/FAME.SharedKernel.csproj src/FAME.SharedKernel/
COPY src/FAME.Infrastructure/FAME.Infrastructure.csproj src/FAME.Infrastructure/
COPY src/Modules/*/FAME.Modules.*.csproj src/Modules/
RUN dotnet restore src/FAME.Api/FAME.Api.csproj

# Copy everything and build
COPY src/ src/
RUN dotnet publish src/FAME.Api/FAME.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser
COPY --from=build --chown=appuser /app/publish .

USER appuser
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "FAME.Api.dll"]
```

---

## 4. CI/CD Pipeline (GitHub Actions)

### 4.1 Pipeline Overview

```
Push/PR → Lint → Test → Build → [PR: Stop] → [main: Deploy Staging] → [tag: Deploy Prod]
```

### 4.2 Main CI Workflow

```yaml
# .github/workflows/ci.yml
name: CI

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true

jobs:
  # ------ Lint & Format ------
  lint:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      
      - name: Setup Node
        uses: actions/setup-node@v4
        with:
          node-version: '22'
          cache: 'pnpm'
      
      - name: Install dependencies
        run: pnpm install --frozen-lockfile
      
      - name: Lint frontend
        run: pnpm --filter web lint
      
      - name: Format check (C#)
        run: dotnet format --verify-no-changes src/FAME.Api.sln
      
      - name: Security audit (npm)
        run: pnpm audit --audit-level=high
      
      - name: Security audit (.NET)
        run: dotnet list src/FAME.Api.sln package --vulnerable --include-transitive

  # ------ Backend Tests ------
  test-backend:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: postgres:17-alpine
        env:
          POSTGRES_DB: fame_test
          POSTGRES_USER: test
          POSTGRES_PASSWORD: test
        ports:
          - 5432:5432
        options: --health-cmd pg_isready --health-interval 10s --health-timeout 5s --health-retries 5
      redis:
        image: redis:7-alpine
        ports:
          - 6379:6379
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      
      - name: Restore
        run: dotnet restore src/FAME.Api.sln
      
      - name: Build
        run: dotnet build src/FAME.Api.sln --no-restore -c Release
      
      - name: Test
        run: dotnet test src/FAME.Api.sln --no-build -c Release --logger "trx;LogFileName=results.trx" --collect:"XPlat Code Coverage"
        env:
          ConnectionStrings__Default: "Host=localhost;Database=fame_test;Username=test;Password=test"
          ConnectionStrings__Redis: "localhost:6379"
      
      - name: Upload coverage
        uses: codecov/codecov-action@v4
        with:
          files: '**/coverage.cobertura.xml'

  # ------ Frontend Tests ------
  test-frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '22'
          cache: 'pnpm'
      
      - run: pnpm install --frozen-lockfile
      - run: pnpm --filter web test -- --coverage
      - run: pnpm --filter web build  # Verify build succeeds

  # ------ Build Docker Images ------
  build:
    needs: [lint, test-backend, test-frontend]
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    permissions:
      packages: write
    steps:
      - uses: actions/checkout@v4
      
      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v3
      
      - name: Login to GHCR
        uses: docker/login-action@v3
        with:
          registry: ghcr.io
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}
      
      - name: Build and push API
        uses: docker/build-push-action@v5
        with:
          context: .
          file: src/FAME.Api/Dockerfile
          push: true
          tags: |
            ghcr.io/fame-lms/api:${{ github.sha }}
            ghcr.io/fame-lms/api:latest
          cache-from: type=gha
          cache-to: type=gha,mode=max
      
      - name: Build and push Web
        uses: docker/build-push-action@v5
        with:
          context: .
          file: packages/web/Dockerfile
          push: true
          tags: |
            ghcr.io/fame-lms/web:${{ github.sha }}
            ghcr.io/fame-lms/web:latest
          cache-from: type=gha
          cache-to: type=gha,mode=max

  # ------ Deploy to Staging ------
  deploy-staging:
    needs: [build]
    runs-on: ubuntu-latest
    environment: staging
    steps:
      - uses: actions/checkout@v4
      
      - name: Deploy to staging
        uses: appleboy/ssh-action@v1
        with:
          host: ${{ secrets.STAGING_HOST }}
          username: deploy
          key: ${{ secrets.DEPLOY_SSH_KEY }}
          script: |
            cd /opt/fame
            docker compose -f docker-compose.staging.yml pull
            docker compose -f docker-compose.staging.yml up -d --remove-orphans
            docker compose -f docker-compose.staging.yml exec api dotnet FAME.Api.dll --migrate
            echo "Staging deployed: ${{ github.sha }}"
```

### 4.3 Production Deployment Workflow

```yaml
# .github/workflows/deploy-prod.yml
name: Deploy Production

on:
  push:
    tags:
      - 'v*'  # Triggered by version tags: v1.0.0, v1.1.0, etc.

jobs:
  deploy:
    runs-on: ubuntu-latest
    environment: production
    steps:
      - uses: actions/checkout@v4
      
      - name: Deploy to production
        uses: appleboy/ssh-action@v1
        with:
          host: ${{ secrets.PROD_HOST }}
          username: deploy
          key: ${{ secrets.DEPLOY_SSH_KEY }}
          script: |
            cd /opt/fame
            
            # Pull specific tagged images
            export TAG=${{ github.ref_name }}
            docker compose -f docker-compose.prod.yml pull
            
            # Backup database before migration
            docker compose -f docker-compose.prod.yml exec postgres \
              pg_dump -U fame fame_db > /opt/backups/pre-deploy-$(date +%Y%m%d-%H%M%S).sql
            
            # Rolling update — zero downtime
            docker compose -f docker-compose.prod.yml up -d --remove-orphans
            
            # Run migrations
            docker compose -f docker-compose.prod.yml exec api \
              dotnet FAME.Api.dll --migrate
            
            # Health check
            for i in {1..30}; do
              if curl -sf http://localhost:5000/health; then
                echo "Health check passed"
                break
              fi
              echo "Waiting for health check... ($i/30)"
              sleep 2
            done
            
            # Cleanup old images
            docker image prune -f
```

---

## 5. Environment Management

### 5.1 Environment Matrix

| Setting | Development | Staging | Production |
|---------|------------|---------|------------|
| **Domain** | localhost | staging.fame.pk | firstaidmadeeasy.com.pk |
| **HTTPS** | Self-signed | Let's Encrypt | Let's Encrypt |
| **Database** | Docker local | Shared staging DB | Production DB |
| **Redis** | Docker local | Shared staging | Production Redis |
| **Email** | Mailhog (captured) | SendGrid sandbox | SendGrid production |
| **Payment** | Sandbox/test mode | Sandbox/test mode | Live credentials |
| **Logging** | Console + Seq | Seq | Seq + Grafana alerts |
| **Error tracking** | Console | Sentry (staging) | Sentry (production) |

### 5.2 Database Migrations Strategy

```
Code-first migrations with EF Core:

1. Developer creates migration:
   dotnet ef migrations add AddCourseRating --project src/Modules/Catalog

2. Migration reviewed in PR (auto-generated SQL diff)

3. Staging: Applied automatically on deploy via --migrate flag

4. Production: Applied automatically on deploy, AFTER backup
   - Rollback migration always available:
     dotnet ef migrations revert --project src/Modules/Catalog

Rules:
- Migrations MUST be backward-compatible (no column renames, no drops without deprecation)
- Data migrations run as separate Hangfire jobs, not in schema migrations
- Large table alterations use pg_repack to avoid locks
```

---

## 6. Backup & Disaster Recovery

### 6.1 Backup Schedule

| Data | Frequency | Retention | Storage |
|------|:---------:|:---------:|---------|
| PostgreSQL full dump | Daily 2:00 AM | 30 days | Off-site S3 bucket |
| PostgreSQL WAL archives | Continuous | 7 days | Off-site S3 bucket |
| Redis RDB snapshot | Every 6 hours | 7 days | Off-site S3 bucket |
| Application config | On change | Indefinite | Git repository |
| User-uploaded files (R2) | Built-in replication | N/A | Cloudflare R2 |

### 6.2 Backup Script

```bash
#!/bin/bash
# ops/backup.sh — runs via cron
set -euo pipefail

TIMESTAMP=$(date +%Y%m%d-%H%M%S)
BACKUP_DIR="/opt/backups"
S3_BUCKET="s3://fame-backups"

# PostgreSQL dump
docker compose exec -T postgres pg_dump -U fame fame_db | gzip > \
    "${BACKUP_DIR}/fame_db_${TIMESTAMP}.sql.gz"

# Upload to S3
aws s3 cp "${BACKUP_DIR}/fame_db_${TIMESTAMP}.sql.gz" \
    "${S3_BUCKET}/postgres/fame_db_${TIMESTAMP}.sql.gz"

# Cleanup local backups older than 7 days
find "${BACKUP_DIR}" -name "*.sql.gz" -mtime +7 -delete

echo "[$(date)] Backup completed: fame_db_${TIMESTAMP}.sql.gz"
```

### 6.3 Recovery Procedures

| Scenario | RTO Target | RPO Target | Procedure |
|----------|:----------:|:----------:|-----------|
| App container crash | <1 min | 0 | Docker auto-restart |
| VPS reboot | <5 min | 0 | Docker restart policies |
| Database corruption | <30 min | <6 hours | Restore from latest backup |
| Full VPS loss | <2 hours | <24 hours | New VPS + restore from S3 backups |
| Region outage | <4 hours | <24 hours | Spin up in alternate DC |

---

## 7. Monitoring Stack

### 7.1 Prometheus Metrics

```yaml
# ops/prometheus.yml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'api'
    static_configs:
      - targets: ['api:5000']
    metrics_path: '/metrics'
  
  - job_name: 'postgres'
    static_configs:
      - targets: ['postgres-exporter:9187']
  
  - job_name: 'redis'
    static_configs:
      - targets: ['redis-exporter:9121']
  
  - job_name: 'caddy'
    static_configs:
      - targets: ['caddy:2019']
```

### 7.2 Alerting Rules

```yaml
# ops/alerts.yml
groups:
  - name: fame-alerts
    rules:
      - alert: HighErrorRate
        expr: rate(http_requests_total{status=~"5.."}[5m]) > 0.05
        for: 2m
        labels:
          severity: critical
        annotations:
          summary: "Error rate above 5%"
      
      - alert: HighLatency
        expr: histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m])) > 0.5
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "P95 latency above 500ms"
      
      - alert: DatabaseConnectionsHigh
        expr: pg_stat_activity_count > 80
        for: 2m
        labels:
          severity: warning
        annotations:
          summary: "Database connections above 80"
      
      - alert: DiskSpaceLow
        expr: node_filesystem_avail_bytes / node_filesystem_size_bytes < 0.2
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "Disk space below 20%"
```

---

## 8. Developer Environment Setup

```bash
# One-command development setup
git clone git@github.com:fame-lms/fame-v2.git
cd fame-v2

# Install dependencies
pnpm install
dotnet restore src/FAME.Api.sln

# Start infrastructure (DB, Redis, Meilisearch, Mailhog)
docker compose -f docker-compose.dev.yml up -d

# Run database migrations
dotnet ef database update --project src/FAME.Api

# Seed development data
dotnet run --project src/FAME.Api -- --seed

# Start backend (watches for changes)
dotnet watch --project src/FAME.Api

# Start frontend (in another terminal)
pnpm --filter web dev
```

---

*The DevOps pipeline ensures every commit is tested, every deployment is automated, and every environment is reproducible. Zero-downtime deployments, automated backups, and comprehensive monitoring protect the production system from day one.*
