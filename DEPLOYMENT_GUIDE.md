# PoConnectFive - Deployment Guide

This guide provides step-by-step instructions for deploying PoConnectFive to Azure App Service using GitHub Actions CI/CD with federated credentials.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Azure Resources Setup](#azure-resources-setup)
- [GitHub Configuration](#github-configuration)
- [Deployment Process](#deployment-process)
- [Verification](#verification)
- [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Tools
- Azure CLI (`az`) - [Install](https://learn.microsoft.com/cli/azure/install-azure-cli)
- Azure Developer CLI (`azd`) - [Install](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd)
- GitHub CLI (`gh`) - [Install](https://cli.github.com/)
- .NET 9 SDK - [Install](https://dotnet.microsoft.com/download/dotnet/9.0)

### Required Access
- Azure subscription with Contributor role
- GitHub repository with admin access
- Access to `PoShared` resource group (for shared App Service Plan)

---

## Azure Resources Setup

### Step 1: Create Service Principal with Federated Credentials

```bash
# Set variables
SUBSCRIPTION_ID="your-subscription-id"
APP_NAME="PoConnectFive"
REPO_OWNER="your-github-username"
REPO_NAME="PoConnectFive"

# Login to Azure
az login
az account set --subscription $SUBSCRIPTION_ID

# Create service principal
az ad sp create-for-rbac --name $APP_NAME --role contributor \
  --scopes /subscriptions/$SUBSCRIPTION_ID \
  --sdk-auth

# Note the output - you'll need:
# - clientId
# - tenantId
# - subscriptionId
```

### Step 2: Configure Federated Credentials

```bash
# Get the service principal object ID
APP_ID=$(az ad sp list --display-name $APP_NAME --query "[0].appId" -o tsv)
OBJECT_ID=$(az ad sp list --display-name $APP_NAME --query "[0].id" -o tsv)

# Create federated credential for main branch
az ad app federated-credential create \
  --id $APP_ID \
  --parameters '{
    "name": "PoConnectFiveFederatedCredential",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:'$REPO_OWNER'/'$REPO_NAME':ref:refs/heads/master",
    "audiences": ["api://AzureADTokenExchange"]
  }'
```

### Step 3: Verify PoShared Resource Group

The deployment uses a shared App Service Plan to minimize costs.

```bash
# Verify PoShared resource group exists
az group show --name PoShared

# Verify shared App Service Plan exists
az appservice plan show --name PoSharedAppServicePlan --resource-group PoShared

# Expected output:
# - Tier: Free (F1)
# - Location: eastus2
# - Status: Ready
```

---

## GitHub Configuration

### Step 1: Configure Repository Variables

Using GitHub CLI:

```bash
# Navigate to your repository
cd PoConnectFive

# Set GitHub repository variables
gh variable set AZURE_CLIENT_ID --body "<client-id-from-step-1>"
gh variable set AZURE_TENANT_ID --body "<tenant-id-from-step-1>"
gh variable set AZURE_SUBSCRIPTION_ID --body "$SUBSCRIPTION_ID"
gh variable set AZURE_ENV_NAME --body "PoConnectFive"
gh variable set AZURE_LOCATION --body "eastus2"
```

Or via GitHub Web UI:
1. Go to repository → Settings → Secrets and variables → Actions
2. Click "Variables" tab
3. Add each variable:
   - `AZURE_CLIENT_ID`
   - `AZURE_TENANT_ID`
   - `AZURE_SUBSCRIPTION_ID`
   - `AZURE_ENV_NAME` = `PoConnectFive`
   - `AZURE_LOCATION` = `eastus2`

### Step 2: Verify Workflow Files

Ensure these workflow files exist:
- `.github/workflows/azure-dev.yml` - Main deployment workflow (AZD-based)
- `.github/workflows/BuildDeploy.yml` - Alternative workflow (direct deployment)

The `azure-dev.yml` workflow is recommended as it uses Azure Developer CLI and federated credentials.

---

## Deployment Process

### Automatic Deployment (Recommended)

The GitHub Actions workflow is triggered automatically on every push to the `master` or `main` branch.

#### Workflow Steps:
1. **Build Job**:
   - Checkout code
   - Setup .NET 9
   - Restore dependencies
   - Build solution
   - Run tests

2. **Deploy Job**:
   - Checkout code
   - Install Azure Developer CLI
   - Login with federated credentials
   - Provision infrastructure (if needed)
   - Deploy application

3. **Verify Job**:
   - Wait for deployment to stabilize
   - Health check `/api/health` endpoint
   - Verify application root
   - Check page title
   - Display deployment summary

#### Trigger Deployment:

```bash
# Make a change and push to master
git add .
git commit -m "Deploy to Azure"
git push origin master
```

### Manual Deployment (Alternative)

Using Azure Developer CLI locally:

```bash
# Login to Azure
az login
azd auth login

# Set environment variables
export AZURE_ENV_NAME="PoConnectFive"
export AZURE_LOCATION="eastus2"
export AZURE_SUBSCRIPTION_ID="your-subscription-id"

# Provision infrastructure (first time only)
azd provision --no-prompt

# Deploy application
azd deploy --no-prompt
```

---

## Verification

### 1. Check GitHub Actions Workflow

1. Go to repository → Actions tab
2. Click on the latest workflow run
3. Verify all jobs completed successfully:
   - ✅ build
   - ✅ deploy
   - ✅ verify

### 2. Verify Azure Resources

```bash
# Check resource group
az group show --name PoConnectFive

# Check App Service
az webapp show --name PoConnectFive --resource-group PoConnectFive

# Check Application Insights
az monitor app-insights component show --app PoConnectFive --resource-group PoConnectFive

# Check Storage Account
az storage account show --name poconnectfive --resource-group PoConnectFive
```

### 3. Test Application Endpoints

#### Health Check
```bash
curl https://poconnectfive.azurewebsites.net/api/health
```
Expected: HTTP 200 with health status JSON

#### Application Root
```bash
curl -I https://poconnectfive.azurewebsites.net/
```
Expected: HTTP 200

#### Swagger UI
```bash
curl -I https://poconnectfive.azurewebsites.net/swagger
```
Expected: HTTP 200 or 301

#### Diagnostics Page
```bash
curl -I https://poconnectfive.azurewebsites.net/diag
```
Expected: HTTP 200

### 4. Verify Page Title

```bash
curl -s https://poconnectfive.azurewebsites.net/ | grep -o '<title>.*</title>'
```
Expected: `<title>PoConnectFive</title>`

### 5. Check Application Insights Telemetry

1. Go to Azure Portal → PoConnectFive Application Insights
2. Click "Live Metrics" - should show real-time data
3. Click "Logs" and run KQL queries from [KQL_QUERIES.md](KQL_QUERIES.md)

### 6. Test Game Functionality

1. Navigate to https://poconnectfive.azurewebsites.net
2. Click "Play Game"
3. Play a complete game
4. Check leaderboard updates
5. Verify statistics tracking

---

## Configuration

### App Service Configuration Settings

The following settings are automatically configured by the Bicep deployment:

| Setting | Value | Source |
|---------|-------|--------|
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Auto-configured | Bicep |
| `AzureTableStorage__ConnectionString` | Auto-configured | Bicep |
| `ApplicationInsightsAgent_EXTENSION_VERSION` | `~3` | Bicep |
| `ASPNETCORE_ENVIRONMENT` | `Production` | App Service Default |

### Local Development vs Production

| Aspect | Local | Production |
|--------|-------|------------|
| Storage | Azurite (`UseDevelopmentStorage=true`) | Azure Table Storage |
| App Insights | Local connection string | Azure App Insights |
| HTTPS | Optional (localhost:5001) | Required |
| Logging | Console + File | Console + App Insights |
| Swagger | Enabled | Enabled |

---

## Cost Optimization

### Resources and Costs

| Resource | Tier | Cost |
|----------|------|------|
| App Service | Free (F1) via shared plan | $0/month |
| Application Insights | Pay-as-you-go | ~$0-5/month |
| Log Analytics | Pay-as-you-go | ~$0-2/month |
| Storage Account | Standard_LRS | ~$0-1/month |
| **Total Estimated** | | **~$0-8/month** |

### Shared App Service Plan

This deployment uses the existing `PoSharedAppServicePlan` in the `PoShared` resource group:
- **No additional cost** for the App Service
- Shares F1 tier resources with other apps
- 60 minutes/day CPU time limit
- 1 GB RAM
- 1 GB storage

### Cost Monitoring

```bash
# View cost analysis
az consumption usage list \
  --start-date 2025-10-01 \
  --end-date 2025-10-31 \
  --query "[?contains(instanceName, 'PoConnectFive')]"
```

---

## Troubleshooting

### Common Issues

#### 1. Workflow Fails at "Log in with Azure"

**Symptom:** Authentication error in GitHub Actions

**Solution:**
```bash
# Verify federated credential exists
az ad app federated-credential list --id $APP_ID

# Verify repository variables
gh variable list

# Re-create federated credential if needed
az ad app federated-credential delete --id $APP_ID --federated-credential-id <credential-id>
# Then re-create following Step 2 above
```

#### 2. Health Check Fails After Deployment

**Symptom:** `/api/health` returns 503 or 500

**Solution:**
```bash
# Check App Service logs
az webapp log tail --name PoConnectFive --resource-group PoConnectFive

# Check storage connection
az webapp config connection-string list --name PoConnectFive --resource-group PoConnectFive

# Restart app service
az webapp restart --name PoConnectFive --resource-group PoConnectFive
```

#### 3. Application Insights Not Receiving Data

**Symptom:** No telemetry in Application Insights

**Solution:**
```bash
# Verify connection string
az webapp config appsettings list --name PoConnectFive --resource-group PoConnectFive \
  --query "[?name=='APPLICATIONINSIGHTS_CONNECTION_STRING']"

# Check Application Insights exists
az monitor app-insights component show --app PoConnectFive --resource-group PoConnectFive

# Wait 5-10 minutes for data to appear (ingestion delay)
```

#### 4. Storage Connection Errors

**Symptom:** "Unable to connect to Azure Table Storage"

**Solution:**
```bash
# Verify storage account exists
az storage account show --name poconnectfive --resource-group PoConnectFive

# Get connection string
az storage account show-connection-string --name poconnectfive --resource-group PoConnectFive

# Update app settings if needed
az webapp config connection-string set \
  --name PoConnectFive \
  --resource-group PoConnectFive \
  --connection-string-type Custom \
  --settings AzureTableStorage__ConnectionString="<connection-string>"
```

#### 5. F1 Tier Limitations

**Symptom:** App stops responding or slow performance

**Constraints:**
- 60 minutes/day CPU time
- 1 GB RAM
- No AlwaysOn
- Shared compute resources

**Solutions:**
- Monitor usage in Azure Portal
- Consider upgrading to B1 tier if needed ($13/month)
- Optimize code for performance

---

## Maintenance

### Updating Configuration

```bash
# Update app settings
az webapp config appsettings set \
  --name PoConnectFive \
  --resource-group PoConnectFive \
  --settings KEY=VALUE

# Restart after config changes
az webapp restart --name PoConnectFive --resource-group PoConnectFive
```

### Viewing Logs

```bash
# Stream logs in real-time
az webapp log tail --name PoConnectFive --resource-group PoConnectFive

# Download logs
az webapp log download --name PoConnectFive --resource-group PoConnectFive --log-file logs.zip
```

### Scaling (If Needed)

```bash
# Check current plan
az appservice plan show --name PoSharedAppServicePlan --resource-group PoShared

# Note: Scaling requires upgrading the entire shared plan and affects all apps using it
```

---

## Cleanup

To remove all resources:

```bash
# Delete resource group (keeps shared plan intact)
az group delete --name PoConnectFive --yes --no-wait

# Note: This does NOT delete PoShared resource group or plan
```

---

## Additional Resources

- [Azure Developer CLI Documentation](https://learn.microsoft.com/azure/developer/azure-developer-cli/)
- [GitHub Actions for Azure](https://github.com/Azure/actions)
- [Federated Credentials Setup](https://learn.microsoft.com/azure/developer/github/connect-from-azure)
- [App Service Documentation](https://learn.microsoft.com/azure/app-service/)
- [Application Insights Documentation](https://learn.microsoft.com/azure/azure-monitor/app/app-insights-overview)

---

## Support

For issues or questions:
1. Check [Troubleshooting](#troubleshooting) section above
2. Review GitHub Actions workflow logs
3. Check Azure App Service logs
4. Review Application Insights for errors
5. Open a GitHub issue in the repository
