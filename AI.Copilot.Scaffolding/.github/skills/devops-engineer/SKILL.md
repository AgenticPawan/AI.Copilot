---
name: devops-engineer
description: "DevOps and infrastructure reasoning and execution skills for {{projectName}}. Use when: configuring Docker/Kubernetes, managing CI/CD pipelines with GitHub Actions, running EF Core migrations, provisioning tenants, configuring Nginx, or automating deployment tasks."
---

# DevOps Engineer Skills

Defines the reasoning and execution behavior for the `@devops-engineer` agent.

---

## Skill: Migration Management

When running database migrations, follow this reasoning chain:

### 1. Identify Target Context
- `HostDbContext` → Host database (Tenants, Features, Permissions, Roles, global entities)
- `TenantDbContext` → Tenant databases (tenant-scoped entities, per-tenant data)
- Both → Schema changes that must be applied to host AND each tenant DB

### 2. Pre-Migration Checklist
- [ ] Entity configuration complete (`IEntityTypeConfiguration<T>`)
- [ ] `HasQueryFilter(e => !e.IsDeleted)` applied
- [ ] DbSet registered on context
- [ ] Interface methods added for the entity
- [ ] No breaking changes to existing columns (or migration has data preservation)

### 3. Migration Commands
```bash
# Host database
cd src/backend
dotnet ef migrations add {Name} \
  --project {{projectName}}.Persistence \
  --startup-project {{projectName}}.Api \
  --context HostDbContext \
  --output-dir Migrations/Host

# Tenant database
dotnet ef migrations add {Name} \
  --project {{projectName}}.Persistence \
  --startup-project {{projectName}}.Api \
  --context TenantDbContext \
  --output-dir Migrations/Tenant
```

### 4. Post-Migration Verification
- Review generated `Up()` and `Down()` methods
- Verify column types, nullable settings, default values
- Check foreign key cascade behavior (prefer `DeleteBehavior.NoAction`)
- Update seeders if initial data needed

---

## Skill: Tenant Provisioning

### Provisioning Workflow
1. **Create database**: `CREATE DATABASE VS_Tenant_{Name}`
2. **Build connection string**: Server, Database, Auth, TrustCert, MARS
3. **Run tenant migrations**: Via `{{projectName}}.Migrator` or EF CLI
4. **Seed initial data**: Permissions, Roles, Admin user, Settings, Feature flags
5. **Register in host DB**: Create tenant record with AES-encrypted connection string
6. **Configure DNS**: Subdomain routing via ingress/proxy
7. **Verify**: Login → Dashboard → CRUD → Data isolation

### Connection String Security
- ALWAYS AES-encrypt before storage in host database
- NEVER log or expose connection strings in API responses
- Decrypt only at runtime in `TenantDbContext` factory
- Rotate encryption keys periodically

---

## Skill: Docker Operations

### Multi-Stage Build Pattern
```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:{{dotnetVersion}}.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:{{dotnetVersion}}.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "{{projectName}}.Api.dll"]
```

### Container Architecture
| Container | Image | Purpose |
|-----------|-------|---------|
| `api` | .NET {{dotnetVersion}} | Backend API + SignalR |
| `host-portal` | nginx | Host admin Angular SPA |
| `tenant-portal` | nginx | Tenant Angular SPA |
| `sql-server` | MSSQL | {{dbProvider}} database |
| `nginx` | nginx | Reverse proxy + WebSocket support |

### Docker Commands
```bash
docker compose -f deploy/docker/docker-compose.yml build
docker compose -f deploy/docker/docker-compose.yml up -d
docker compose -f deploy/docker/docker-compose.yml logs -f
```

---

## Skill: CI/CD Pipeline

### GitHub Actions Workflow Structure
```yaml
jobs:
  backend:
    - dotnet restore
    - dotnet build
    - dotnet test
    - dotnet publish

  frontend:
    - npm ci
    - ng lint
    - ng test --watch=false
    - ng build --configuration production

  docker:
    needs: [backend, frontend]
    - docker build (API, host-portal, tenant-portal)
    - docker push to registry
    - deploy to Kubernetes
```

### Build & Test Commands
```bash
# Backend
cd src/backend && dotnet build {{projectName}}.slnx && dotnet test {{projectName}}.Tests

# Frontend
cd src/frontend && ng build portal --configuration production
```

---

## Skill: Kubernetes Operations

### Deployment Checklist
- [ ] All migrations applied (Host + all Tenant databases)
- [ ] Seeder data updated if schema changed
- [ ] Docker images built and tagged
- [ ] Environment variables configured (JWT keys, encryption keys, CORS)
- [ ] Nginx/Ingress config updated for new routes/WebSocket paths
- [ ] Health checks passing (`/api/health`)
- [ ] Rollback plan documented
- [ ] DNS configured for new tenant subdomains
- [ ] SSL certificates valid and renewed

### Ingress Configuration
```yaml
# Wildcard subdomain routing for tenants
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: tenant-ingress
spec:
  rules:
    - host: "*.yourdomain.com"
      http:
        paths:
          - path: /api
            backend:
              service:
                name: api-service
                port:
                  number: 80
          - path: /
            backend:
              service:
                name: tenant-portal
                port:
                  number: 80
```

---

## Skill: Infrastructure Troubleshooting

### Common Issues
| Symptom | Likely Cause | Resolution |
|---------|-------------|------------|
| API returns 502 | Container crashed | Check `docker logs api`, verify health endpoint |
| Tenant portal blank | nginx misconfiguration | Check SPA fallback (`try_files $uri /index.html`) |
| SignalR disconnects | WebSocket blocked | Enable WebSocket in nginx `proxy_set_header Upgrade` |
| Migration fails | Missing startup project | Verify `--startup-project` path and context name |
| Slow queries | Missing indexes | Review EF configurations, add indexed columns |
| Connection refused | SQL Server down | Check container status, connection string, firewall |
