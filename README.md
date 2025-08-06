# Azure Identities API

A .NET 8 Web API demonstrating two Azure identity authentication methods for GitHub Actions deployment:
1. **Publish Profile** (traditional method)
2. **Federated Identity** (modern, secure OIDC method)

## 🚀 API Endpoints

- `GET /` - Hello World message with timestamp
- `GET /health` - Health check endpoint
- `GET /swagger` - API documentation (development only)

## 🏗️ Architecture

```
Azure Identities API
├── Program.cs              # Main application entry point
├── appsettings.json        # Application configuration
├── .github/workflows/      # GitHub Actions workflows
│   ├── deploy-publish-profile.yml    # Deployment using publish profile
│   └── deploy-federated-identity.yml # Deployment using federated identity
└── azure-deployment.md    # Detailed deployment guide
```

## 🔧 Local Development

### Prerequisites
- .NET 8 SDK
- Azure CLI (for deployment)

### Running Locally
```bash
# Clone the repository
git clone <your-repo-url>
cd azure_identities

# Restore dependencies
dotnet restore

# Run the application
dotnet run

# Test the endpoints
curl http://localhost:5000/
curl http://localhost:5000/health
```

## ☁️ Azure Deployment

### Method 1: Publish Profile (Current)

1. **Create Azure Resources:**
   ```bash
   # Login to Azure
   az login

   # Create resource group
   az group create --name rg-azure-identities --location eastus

   # Create App Service Plan
   az appservice plan create --name asp-azure-identities --resource-group rg-azure-identities --sku B1 --is-linux

   # Create Web App
   az webapp create --name webapp-azure-identities-api --resource-group rg-azure-identities --plan asp-azure-identities --runtime "DOTNETCORE|8.0"
   ```

2. **Download Publish Profile:**
   ```bash
   az webapp deployment list-publishing-profiles --name webapp-azure-identities-api --resource-group rg-azure-identities --xml
   ```

3. **Configure GitHub Secrets:**
   - `AZURE_WEBAPP_PUBLISH_PROFILE`: Content of the downloaded publish profile

4. **Deploy:**
   Push to main branch or trigger workflow manually.

### Method 2: Federated Identity (Recommended)

1. **Create Service Principal:**
   ```bash
   az ad sp create-for-rbac --name "sp-azure-identities-github" --role contributor --scopes /subscriptions/{subscription-id}/resourceGroups/rg-azure-identities --json-auth
   ```

2. **Configure Federated Credentials:**
   ```bash
   APP_ID=$(az ad sp list --display-name "sp-azure-identities-github" --query "[0].appId" -o tsv)
   
   az ad app federated-credential create --id $APP_ID --parameters '{
     "name": "github-actions-federated-credential",
     "issuer": "https://token.actions.githubusercontent.com",
     "subject": "repo:YOUR_GITHUB_USERNAME/azure_identities:ref:refs/heads/main",
     "description": "GitHub Actions Federated Credential",
     "audiences": ["api://AzureADTokenExchange"]
   }'
   ```

3. **Configure GitHub Secrets:**
   - `AZURE_CLIENT_ID`: Application (client) ID
   - `AZURE_TENANT_ID`: Directory (tenant) ID
   - `AZURE_SUBSCRIPTION_ID`: Subscription ID

## 🔄 Migration Process

To migrate from Publish Profile to Federated Identity:

1. **Disable the current workflow:**
   - Rename `deploy-publish-profile.yml` to `deploy-publish-profile.yml.disabled`

2. **Enable federated identity workflow:**
   - Rename `deploy-federated-identity.yml.disabled` to `deploy-federated-identity.yml` (if disabled)

3. **Update GitHub secrets** as described in Method 2 above

4. **Remove the publish profile secret** (optional, for security)

## 🛡️ Security Benefits of Federated Identity

- **No long-lived secrets** stored in GitHub
- **Automatic token rotation** via OIDC
- **Granular permissions** with Azure RBAC
- **Audit trail** in Azure AD logs
- **Zero Trust** security model

## 🔍 Monitoring

- **Application Insights** can be added for monitoring
- **Health endpoint** at `/health` for uptime monitoring
- **Structured logging** with Serilog (can be added)

## 📚 Additional Resources

- [Azure App Service Documentation](https://docs.microsoft.com/en-us/azure/app-service/)
- [GitHub Actions for Azure](https://docs.microsoft.com/en-us/azure/developer/github/github-actions)
- [Federated Identity Credentials](https://docs.microsoft.com/en-us/azure/active-directory/develop/workload-identity-federation)# Test deployment
