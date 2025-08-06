# 🚀 Azure Identities API - Deployment Setup

## ✅ Completed Steps

1. **✓ Created .NET 8 Web API** with Hello World and health endpoints
2. **✓ Initialized Git repository** with initial commit
3. **✓ Created Azure Resources:**
   - Resource Group: `rg-azure-identities-63312`
   - App Service Plan: `asp-azure-identities-63312` (Free tier, West US 2)
   - Web App: `webapp-azure-identities-api-63312`
   - Web App URL: https://webapp-azure-identities-api-63312.azurewebsites.net
4. **✓ Downloaded publish profile** (`publish-profile.xml`)
5. **✓ Updated GitHub workflow files** with correct resource names

## 🔄 Next Steps

### 1. GitHub Repository Setup

1. **Create GitHub repository:**
   ```bash
   # Create a new repository on GitHub (via web interface)
   # Repository name: azure_identities
   ```

2. **Add remote and push:**
   ```bash
   git remote add origin https://github.com/YOUR_USERNAME/azure_identities.git
   git branch -M main
   git push -u origin main
   ```

### 2. Configure GitHub Secrets (Method 1: Publish Profile)

1. **Copy publish profile content:**
   ```bash
   cat publish-profile.xml
   ```

2. **Add GitHub Secret:**
   - Go to GitHub repository → Settings → Secrets and variables → Actions
   - Click "New repository secret"
   - Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
   - Value: Paste the entire content from `publish-profile.xml`

### 3. Test Deployment

1. **Trigger deployment:**
   - Push any change to main branch OR
   - Go to Actions tab → "Deploy to Azure App Service (Publish Profile)" → "Run workflow"

2. **Verify deployment:**
   - Check: https://webapp-azure-identities-api-63312.azurewebsites.net/
   - Should return: `{"message":"Hello World from Azure Identities API!","timestamp":"...","environment":"Production","version":"1.0.0"}`

### 4. Setup Federated Identity (Method 2)

After successful publish profile deployment, migrate to federated identity:

1. **Create Service Principal:**
   ```bash
   SUBSCRIPTION_ID="55fbba2a-5b5a-4181-9c10-b9b6ccb93712"
   
   az ad sp create-for-rbac --name "sp-azure-identities-github-63312" \
     --role contributor \
     --scopes /subscriptions/$SUBSCRIPTION_ID/resourceGroups/rg-azure-identities-63312 \
     --json-auth
   ```

2. **Create Federated Credential:**
   ```bash
   APP_ID=$(az ad sp list --display-name "sp-azure-identities-github-63312" --query "[0].appId" -o tsv)
   
   az ad app federated-credential create --id $APP_ID --parameters '{
     "name": "github-actions-federated-credential",
     "issuer": "https://token.actions.githubusercontent.com",
     "subject": "repo:YOUR_GITHUB_USERNAME/azure_identities:ref:refs/heads/main",
     "description": "GitHub Actions Federated Credential",
     "audiences": ["api://AzureADTokenExchange"]
   }'
   ```

3. **Add GitHub Secrets for Federated Identity:**
   - `AZURE_CLIENT_ID`: Application (client) ID from step 1
   - `AZURE_TENANT_ID`: `52ffe4ed-773d-4c51-8976-a5045a7f3075`
   - `AZURE_SUBSCRIPTION_ID`: `55fbba2a-5b5a-4181-9c10-b9b6ccb93712`

4. **Switch workflows:**
   - Disable: `deploy-publish-profile.yml` (rename to `.disabled`)
   - Enable: `deploy-federated-identity.yml`

## 📋 Key Information

- **Azure Subscription**: 55fbba2a-5b5a-4181-9c10-b9b6ccb93712
- **Tenant ID**: 52ffe4ed-773d-4c51-8976-a5045a7f3075
- **Resource Group**: rg-azure-identities-63312
- **Web App**: webapp-azure-identities-api-63312
- **URL**: https://webapp-azure-identities-api-63312.azurewebsites.net
- **Region**: West US 2

## 🔗 Useful Commands

```bash
# Test API locally
dotnet run --urls=http://localhost:5000

# Test deployed API
curl https://webapp-azure-identities-api-63312.azurewebsites.net/
curl https://webapp-azure-identities-api-63312.azurewebsites.net/health

# View Azure resources
az group show --name rg-azure-identities-63312
az webapp show --name webapp-azure-identities-api-63312 --resource-group rg-azure-identities-63312

# Clean up resources (when done)
az group delete --name rg-azure-identities-63312 --yes --no-wait
```