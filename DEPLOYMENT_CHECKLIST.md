# PoConnectFive - Deployment Verification Checklist

Use this checklist to verify successful deployment to Azure.

## Pre-Deployment Checklist

### Local Environment
- [ ] All code committed to Git
- [ ] Local build succeeds: `dotnet build` (0 errors, 0 warnings)
- [ ] All tests pass: `dotnet test`
- [ ] Code formatted: `dotnet format`
- [ ] Branch is `master` or `main`

### Azure Prerequisites
- [ ] `PoShared` resource group exists
- [ ] `PoSharedAppServicePlan` (F1 tier) exists in `PoShared`
- [ ] Azure CLI logged in: `az account show`
- [ ] Correct subscription selected

### GitHub Configuration
- [ ] GitHub repository variables configured:
  - [ ] `AZURE_CLIENT_ID`
  - [ ] `AZURE_TENANT_ID`
  - [ ] `AZURE_SUBSCRIPTION_ID`
  - [ ] `AZURE_ENV_NAME`
  - [ ] `AZURE_LOCATION`
- [ ] Federated credentials configured
- [ ] Workflow files exist: `.github/workflows/azure-dev.yml`

---

## Deployment Process Checklist

### Trigger Deployment
- [ ] Code pushed to `master` branch
- [ ] GitHub Actions workflow started automatically
- [ ] Workflow visible in Actions tab

### Build Job
- [ ] ✅ Checkout code
- [ ] ✅ Setup .NET 9
- [ ] ✅ Restore dependencies
- [ ] ✅ Build solution (Release configuration)
- [ ] ✅ Run tests (all passed)

### Deploy Job
- [ ] ✅ Checkout code
- [ ] ✅ Install Azure Developer CLI
- [ ] ✅ Login with federated credentials
- [ ] ✅ Provision infrastructure (or skip if exists)
- [ ] ✅ Deploy application

### Verify Job
- [ ] ✅ Wait 30 seconds for stabilization
- [ ] ✅ Health check passed (HTTP 200)
- [ ] ✅ Application root accessible (HTTP 200)
- [ ] ✅ Page title verified (contains "PoConnectFive")
- [ ] ✅ Deployment summary displayed

---

## Post-Deployment Verification

### Azure Resources Created
- [ ] Resource Group: `PoConnectFive` exists
  ```bash
  az group show --name PoConnectFive
  ```

- [ ] App Service: `PoConnectFive` exists
  ```bash
  az webapp show --name PoConnectFive --resource-group PoConnectFive
  ```
  - [ ] Status: Running
  - [ ] Using `PoSharedAppServicePlan`
  - [ ] Location: eastus2
  - [ ] .NET 9 runtime

- [ ] Application Insights: `PoConnectFive` exists
  ```bash
  az monitor app-insights component show --app PoConnectFive --resource-group PoConnectFive
  ```
  - [ ] Linked to Log Analytics workspace
  - [ ] Receiving telemetry

- [ ] Log Analytics Workspace: `PoConnectFive` exists
  ```bash
  az monitor log-analytics workspace show --workspace-name PoConnectFive --resource-group PoConnectFive
  ```
  - [ ] Retention: 30 days

- [ ] Storage Account: `poconnectfive` exists
  ```bash
  az storage account show --name poconnectfive --resource-group PoConnectFive
  ```
  - [ ] SKU: Standard_LRS
  - [ ] Tables service enabled

### Application Configuration
- [ ] App Settings configured:
  ```bash
  az webapp config appsettings list --name PoConnectFive --resource-group PoConnectFive
  ```
  - [ ] `APPLICATIONINSIGHTS_CONNECTION_STRING` set
  - [ ] `AzureTableStorage__ConnectionString` set
  - [ ] `ApplicationInsightsAgent_EXTENSION_VERSION` = `~3`

### Endpoint Health Checks

#### 1. Health Check API
```bash
curl -i https://poconnectfive.azurewebsites.net/api/health
```
- [ ] HTTP Status: 200
- [ ] Response contains: `"Status":"Healthy"`
- [ ] JSON format valid
- [ ] All checks passing:
  - [ ] Azure Table Storage
  - [ ] DNS Resolution
  - [ ] HTTP Connectivity

#### 2. Application Root
```bash
curl -i https://poconnectfive.azurewebsites.net/
```
- [ ] HTTP Status: 200
- [ ] Content-Type: text/html
- [ ] Blazor WASM loads

#### 3. Swagger UI
```bash
curl -I https://poconnectfive.azurewebsites.net/swagger
```
- [ ] HTTP Status: 200 or 301
- [ ] Swagger page loads in browser

#### 4. Diagnostics Page
```bash
curl -I https://poconnectfive.azurewebsites.net/diag
```
- [ ] HTTP Status: 200
- [ ] Diagnostics page shows component health

#### 5. Leaderboard API
```bash
curl -i https://poconnectfive.azurewebsites.net/api/leaderboard/Easy
```
- [ ] HTTP Status: 200
- [ ] JSON array returned (may be empty)

### Functional Testing

#### Game Play
- [ ] Navigate to https://poconnectfive.azurewebsites.net
- [ ] Click "Play Game"
- [ ] Start a new game (Player vs AI - Easy)
- [ ] Make moves successfully
- [ ] Complete a game (win/loss/draw)
- [ ] Victory/defeat screen appears

#### Statistics Tracking
- [ ] Game result saved to database
- [ ] Check Application Insights for `GameCompleted` event:
  ```kql
  customEvents
  | where name == "GameCompleted"
  | top 10 by timestamp desc
  ```

#### Leaderboard
- [ ] Navigate to Leaderboard page
- [ ] Select difficulty (Easy/Medium/Hard)
- [ ] Players displayed (if any)
- [ ] No errors in console

#### Settings
- [ ] Navigate to Settings
- [ ] Change AI difficulty
- [ ] Toggle sound
- [ ] Settings saved to browser storage

#### Diagnostics
- [ ] Navigate to /diag page
- [ ] All health checks show "Healthy"
- [ ] Can manually run diagnostics

### Application Insights Telemetry

#### Live Metrics
- [ ] Navigate to Application Insights → Live Metrics
- [ ] See incoming requests in real-time
- [ ] Response times displayed
- [ ] No errors shown

#### Logs Query
Run in Application Insights → Logs:

```kql
requests
| where timestamp > ago(1h)
| summarize count() by name
| order by count_ desc
```
- [ ] Requests logged
- [ ] `/api/health` visible
- [ ] `/api/leaderboard/*` visible

#### Custom Events
```kql
customEvents
| where timestamp > ago(1h)
| summarize count() by name
```
- [ ] `GameCompleted` events (if games played)
- [ ] `LeaderboardViewed` events (if leaderboard accessed)

#### Exceptions (Should be Empty)
```kql
exceptions
| where timestamp > ago(1h)
```
- [ ] No exceptions (or expected/handled exceptions only)

### Performance Checks

#### Page Load Time
- [ ] Homepage loads in < 5 seconds
- [ ] Game page loads in < 3 seconds
- [ ] Leaderboard loads in < 2 seconds

#### API Response Times
Check in Application Insights → Performance:
- [ ] `/api/health` p95 < 500ms
- [ ] `/api/leaderboard/*` p95 < 1000ms
- [ ] `/api/log/*` p95 < 200ms

#### Health Check Response
```bash
time curl -s https://poconnectfive.azurewebsites.net/api/health > /dev/null
```
- [ ] Total time < 2 seconds

### Security Checks

- [ ] HTTPS enforced (no HTTP access)
- [ ] App Service uses TLS 1.2 minimum
- [ ] Connection strings not exposed in client
- [ ] Swagger accessible (enabled for testing)
- [ ] CORS not configured (same-origin)
- [ ] No secrets in GitHub repository
- [ ] Federated credentials used (no keys in secrets)

### Logging Checks

#### App Service Logs
```bash
az webapp log tail --name PoConnectFive --resource-group PoConnectFive
```
- [ ] Logs streaming successfully
- [ ] Application started message visible
- [ ] No critical errors
- [ ] Serilog output formatted

#### Log Levels
- [ ] Information logs present
- [ ] Warnings logged appropriately
- [ ] No unexpected errors

---

## Cost Verification

### Current Month Costs
```bash
az consumption usage list \
  --start-date $(date -d "$(date +%Y-%m-01)" +%Y-%m-%d) \
  --end-date $(date +%Y-%m-%d) \
  --query "[?contains(instanceName, 'PoConnectFive')]"
```

### Expected Costs
- [ ] App Service: $0 (using shared F1 plan)
- [ ] Application Insights: < $5/month
- [ ] Log Analytics: < $2/month
- [ ] Storage Account: < $1/month
- [ ] **Total: < $8/month**

### F1 Tier Constraints
- [ ] 60 minutes/day CPU time limit
- [ ] 1 GB RAM
- [ ] No AlwaysOn (app may cold start)
- [ ] Shared compute resources

---

## Documentation Verification

- [ ] `README.md` updated with deployment instructions
- [ ] `DEPLOYMENT_GUIDE.md` created with detailed steps
- [ ] `DEPLOYMENT_CHECKLIST.md` (this file) created
- [ ] `PRD.md` includes deployment architecture
- [ ] `AGENTS.md` includes deployment instructions
- [ ] Phase 5 completion summary created

---

## Rollback Plan (If Needed)

If deployment fails or issues arise:

### Option 1: Redeploy Previous Version
```bash
# Get previous commit hash
git log --oneline -n 5

# Create rollback branch
git checkout <previous-commit-hash>
git checkout -b rollback-deployment

# Push to trigger redeployment
git push origin rollback-deployment:master
```

### Option 2: Manual Rollback via Portal
1. Azure Portal → PoConnectFive App Service
2. Deployment Center → Deployment History
3. Select previous successful deployment
4. Click "Redeploy"

### Option 3: Delete and Recreate
```bash
# Delete resource group (keeps shared plan)
az group delete --name PoConnectFive --yes

# Redeploy via GitHub Actions
git push origin master
```

---

## Final Sign-Off

### Deployment Completed By:
- **Name:** _______________
- **Date:** _______________
- **Commit Hash:** _______________
- **Deployment Duration:** _______________

### Verification Completed By:
- **Name:** _______________
- **Date:** _______________
- **Issues Found:** _______________

### Approval:
- [ ] All checks passed
- [ ] Application functional
- [ ] Performance acceptable
- [ ] Costs within budget
- [ ] Documentation updated

**Approved By:** _______________
**Date:** _______________

---

## Notes

Use this space for any deployment-specific notes or issues encountered:

```
[Add notes here]
```

---

## Next Steps After Successful Deployment

1. [ ] Monitor Application Insights for 24 hours
2. [ ] Check cost analysis after 7 days
3. [ ] Review logs for any warnings
4. [ ] Share application URL with stakeholders
5. [ ] Add custom domain (if needed)
6. [ ] Configure monitoring alerts
7. [ ] Schedule regular health checks
8. [ ] Plan for load testing (if needed)

---

**Deployment Status:** ⬜ Not Started | ⬜ In Progress | ⬜ Completed | ⬜ Failed

**Overall Health:** ⬜ Excellent | ⬜ Good | ⬜ Fair | ⬜ Poor
