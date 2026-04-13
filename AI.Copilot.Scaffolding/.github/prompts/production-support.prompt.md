---
agent: agent
description: "Production issue triage and resolution. Use when: investigating P1-P4 incidents, debugging tenant-specific failures, checking database connectivity, or diagnosing API/frontend errors in production."
---

# Production Support: Issue Triage & Resolution

Systematic production issue investigation for {{projectName}} SaaS platform.

## Input Required
- **Severity**: `P1-Critical` (system down) | `P2-High` (major feature broken) | `P3-Medium` (degraded) | `P4-Low` (cosmetic)
- **Symptom**: Error message, behavior description, affected users/tenants
- **Tenant Affected**: `all` | `host-only` | specific tenant name
- **Timeframe**: When did it start? Intermittent or constant?

## P1 Critical - System Down

### Immediate Actions
1. **Check API health**: `curl https://{domain}/api/health`
2. **Check database connectivity**: Is SQL Server accessible?
3. **Check recent deployments**: `git log --oneline -5` - was anything deployed?
4. **Check middleware pipeline**:
   - `GlobalExceptionMiddleware` - catching unhandled exceptions?
   - `TenantResolutionMiddleware` - DNS resolution working?
   - `CorrelationIdMiddleware` - can we trace requests?

### Common P1 Causes
| Symptom | Likely Cause | Fix |
|---------|-------------|-----|
| All API calls return 500 | Database connection failure | Check connection string, SQL Server status |
| Login fails for everyone | JWT key mismatch after deploy | Verify `Jwt:SecretKey` in appsettings |
| Tenant portal blank page | Frontend build issue | Check nginx, dist folder, proxy config |
| SignalR disconnecting | WebSocket blocked by proxy | Check CORS, nginx WebSocket config |

## P2-P4 Investigation

### Backend Issues
1. **Identify the failing endpoint** from browser network tab or logs
2. **Trace through layers**: Controller → Handler → DbContext → Database
3. **Check tenant isolation**: Is the issue tenant-specific?
   - Yes → Check `TenantDbContext` query filters, tenant connection string
   - No → Check `HostDbContext`, shared services
4. **Check permissions**: Is `PermissionChecker` returning correct results?
5. **Check caching**: Is stale cache causing incorrect data?
   - Clear: `await _cacheService.RemoveByPrefixAsync("prefix:")`
   - OutputCache: `await _outputCacheStore.EvictByTagAsync("all", default)`

### Frontend Issues
1. **Console errors**: JavaScript exceptions, failed HTTP requests
2. **Network tab**: Check API response status codes and payloads
3. **Angular-specific**:
   - OnPush not detecting changes → Verify signal updates
   - Route guard blocking → Check token expiry, permission claims
   - Stale UI state → Check if signals are being reset on navigation

### Data Issues
1. **Check soft deletes**: `SELECT COUNT(*) FROM {Table} WHERE IsDeleted = 1`
2. **Check audit trail**: Query `AuditLogs` for the affected entity
3. **Check tenant data isolation**: Verify TenantId on affected records
4. **Check encryption**: Are connection strings / documents decryptable?

## Resolution Checklist
- [ ] Root cause identified
- [ ] Fix applied following existing patterns
- [ ] Regression test added
- [ ] Verified fix doesn't affect other tenants
- [ ] Cache invalidated if data was corrected
- [ ] Audit log entry for any manual data fixes
- [ ] Post-mortem notes documented
