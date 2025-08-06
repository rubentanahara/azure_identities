# 🎯 Practical Examples - Azure Authentication Methods

## 🔧 Real-World Setup Examples

### **Example 1: E-commerce API (Production)**

**Scenario**: You're deploying a production e-commerce API that handles customer data and payments.

**Recommendation**: **Federated Identity** (Enterprise Security Required)

```bash
# Step-by-step production setup
# 1. Create resource group with proper naming
az group create \
  --name rg-ecommerce-prod-eastus \
  --location eastus \
  --tags environment=production project=ecommerce

# 2. Create App Service with production tier
az appservice plan create \
  --name asp-ecommerce-prod \
  --resource-group rg-ecommerce-prod-eastus \
  --sku S1 \
  --is-linux

# 3. Create web app with custom domain ready
az webapp create \
  --name ecommerce-api-prod \
  --resource-group rg-ecommerce-prod-eastus \
  --plan asp-ecommerce-prod \
  --runtime "DOTNETCORE|8.0"

# 4. Create service principal with minimal scope
az ad sp create-for-rbac \
  --name "sp-github-ecommerce-prod" \
  --role contributor \
  --scopes /subscriptions/YOUR_SUB_ID/resourceGroups/rg-ecommerce-prod-eastus

# 5. Create federated credential for production branch
az ad app federated-credential create --id YOUR_APP_ID --parameters '{
  "name": "github-prod-deployment",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:yourcompany/ecommerce-api:ref:refs/heads/production",
  "audiences": ["api://AzureADTokenExchange"]
}'
```

### **Example 2: Personal Blog API (Learning)**

**Scenario**: You're building a personal blog API to learn Azure deployment.

**Recommendation**: **Publish Profile** (Quick Start)

```bash
# Simple setup for learning
az group create --name rg-blog-dev --location eastus
az appservice plan create --name asp-blog --resource-group rg-blog-dev --sku F1
az webapp create --name myblog-api --resource-group rg-blog-dev --plan asp-blog --runtime "dotnet:8"

# Quick deployment credential
az webapp deployment list-publishing-profiles \
  --name myblog-api \
  --resource-group rg-blog-dev \
  --xml
```

---

## 📋 Step-by-Step CLI vs Portal Comparison

### **Scenario**: Setting up a company internal API

#### **CLI Approach (Faster for DevOps):**

```bash
# ⏱️ Total time: ~5 minutes
# 📊 Reproducible: Yes (Infrastructure as Code)
# 🎯 Best for: DevOps teams, automation

# Variables for reusability
COMPANY="acme"
ENV="dev"
LOCATION="eastus"
RG_NAME="rg-${COMPANY}-api-${ENV}"
APP_NAME="api-${COMPANY}-${ENV}"

# 1. Resource Group (30 seconds)
az group create --name $RG_NAME --location $LOCATION

# 2. App Service Plan (1 minute)
az appservice plan create \
  --name "asp-${COMPANY}-${ENV}" \
  --resource-group $RG_NAME \
  --sku B1 \
  --is-linux

# 3. Web App (1 minute)
az webapp create \
  --name $APP_NAME \
  --resource-group $RG_NAME \
  --plan "asp-${COMPANY}-${ENV}" \
  --runtime "DOTNETCORE|8.0"

# 4. Service Principal (2 minutes)
SP_OUTPUT=$(az ad sp create-for-rbac \
  --name "sp-github-${COMPANY}-${ENV}" \
  --role contributor \
  --scopes /subscriptions/$(az account show --query id -o tsv)/resourceGroups/$RG_NAME \
  --json-auth)

CLIENT_ID=$(echo $SP_OUTPUT | jq -r '.clientId')

# 5. Federated Credential (1 minute)
az ad app federated-credential create --id $CLIENT_ID --parameters '{
  "name": "github-actions-main",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:acme/internal-api:ref:refs/heads/main",
  "audiences": ["api://AzureADTokenExchange"]
}'

echo "Setup complete! Add these to GitHub Secrets:"
echo "AZURE_CLIENT_ID: $CLIENT_ID"
echo "AZURE_TENANT_ID: $(az account show --query tenantId -o tsv)"
echo "AZURE_SUBSCRIPTION_ID: $(az account show --query id -o tsv)"
```

#### **Azure Portal Approach (Better for Visual Learners):**

```
⏱️ Total time: ~15 minutes
📊 Reproducible: Manual steps (can be documented)
🎯 Best for: Learning, one-off setups, visual preference

Step 1: Create Resource Group (2 minutes)
Portal → Resource Groups → Create
├── Name: rg-acme-api-dev
├── Region: East US
└── Tags: environment=dev, project=api

Step 2: Create App Service Plan (3 minutes)
Portal → App Services → Create → Web App
├── Resource Group: rg-acme-api-dev
├── Name: asp-acme-dev
├── Runtime: .NET 8
├── Operating System: Linux
├── Region: East US
└── Pricing Plan: Basic B1

Step 3: Create Web App (3 minutes)
Continue from App Service creation
├── App Name: api-acme-dev
├── Resource Group: rg-acme-api-dev
├── App Service Plan: asp-acme-dev
└── Click "Review + Create"

Step 4: Create Service Principal (4 minutes)
Portal → Azure Active Directory → App registrations → New registration
├── Name: sp-github-acme-dev
├── Account types: Single tenant
├── Register
├── Copy Application (client) ID
└── Go to Certificates & secrets

Step 5: Assign Permissions (2 minutes)
Portal → Subscriptions → Your subscription → Access control (IAM)
├── Add role assignment
├── Role: Contributor
├── Assign to: sp-github-acme-dev
└── Save

Step 6: Create Federated Credential (1 minute)
Back to App registration → Certificates & secrets → Federated credentials
├── Add credential
├── Scenario: GitHub Actions
├── Organization: acme
├── Repository: internal-api
├── Entity: Branch
├── Branch: main
└── Add
```

---

## 🔍 Troubleshooting Real Scenarios

### **Scenario 1: Deployment Fails with "Unauthorized"**

#### **Publish Profile Method:**
```bash
# Check if publish profile is valid
curl -X POST https://YOUR_APP.scm.azurewebsites.net/api/zipdeploy \
  -u 'USERNAME_FROM_PROFILE:PASSWORD_FROM_PROFILE' \
  --data-binary @test.zip

# If fails, re-download publish profile
az webapp deployment list-publishing-profiles \
  --name YOUR_APP \
  --resource-group YOUR_RG \
  --xml > new-profile.xml
```

#### **Federated Identity Method:**
```bash
# Test Azure login manually
az login --service-principal \
  --username YOUR_CLIENT_ID \
  --tenant YOUR_TENANT_ID \
  --federated-token $(curl -H "Authorization: bearer $ACTIONS_ID_TOKEN_REQUEST_TOKEN" "$ACTIONS_ID_TOKEN_REQUEST_URL&audience=api://AzureADTokenExchange" | jq -r '.value')

# Check role assignments
az role assignment list \
  --assignee YOUR_CLIENT_ID \
  --scope /subscriptions/YOUR_SUB_ID/resourceGroups/YOUR_RG
```

### **Scenario 2: Wrong Environment Deployment**

```yaml
# Use environment-specific workflows
# .github/workflows/deploy-dev.yml
name: Deploy to Development
on:
  push:
    branches: [ develop ]
env:
  AZURE_WEBAPP_NAME: api-company-dev

# .github/workflows/deploy-prod.yml  
name: Deploy to Production
on:
  push:
    branches: [ main ]
env:
  AZURE_WEBAPP_NAME: api-company-prod
```

---

## 🎨 Advanced Configurations

### **Multi-Environment Setup with Federated Identity:**

```bash
# Create federated credentials for different environments
# Development environment
az ad app federated-credential create --id $APP_ID --parameters '{
  "name": "github-dev-deployment",
  "subject": "repo:company/api:ref:refs/heads/develop",
  "issuer": "https://token.actions.githubusercontent.com",
  "audiences": ["api://AzureADTokenExchange"]
}'

# Staging environment  
az ad app federated-credential create --id $APP_ID --parameters '{
  "name": "github-staging-deployment",
  "subject": "repo:company/api:ref:refs/heads/staging",
  "issuer": "https://token.actions.githubusercontent.com",
  "audiences": ["api://AzureADTokenExchange"]
}'

# Production environment (only from main branch)
az ad app federated-credential create --id $APP_ID --parameters '{
  "name": "github-prod-deployment",
  "subject": "repo:company/api:ref:refs/heads/main",
  "issuer": "https://token.actions.githubusercontent.com",
  "audiences": ["api://AzureADTokenExchange"]
}'
```

### **Custom Domain Setup:**

```bash
# Add custom domain (requires Standard tier or higher)
az appservice plan update \
  --name asp-company-prod \
  --resource-group rg-company-prod \
  --sku S1

# Add custom domain
az webapp config hostname add \
  --webapp-name api-company-prod \
  --resource-group rg-company-prod \
  --hostname api.company.com

# Enable SSL
az webapp config ssl bind \
  --certificate-thumbprint YOUR_CERT_THUMBPRINT \
  --ssl-type SNI \
  --name api-company-prod \
  --resource-group rg-company-prod
```

---

## 🔐 Security Best Practices

### **Publish Profile Security (If You Must Use It):**

```bash
# Rotate credentials regularly (monthly)
az webapp deployment list-publishing-profiles \
  --name YOUR_APP \
  --resource-group YOUR_RG \
  --xml > new-profile.xml

# Limit access with IP restrictions
az webapp config access-restriction add \
  --name YOUR_APP \
  --resource-group YOUR_RG \
  --rule-name "GitHub-Actions" \
  --action Allow \
  --ip-address "192.30.252.0/22" \
  --priority 100
```

### **Federated Identity Security:**

```bash
# Use least privilege (specific resource scope)
az ad sp create-for-rbac \
  --name "sp-github-app-specific" \
  --role "Website Contributor" \
  --scopes /subscriptions/SUB_ID/resourceGroups/RG_NAME/providers/Microsoft.Web/sites/APP_NAME

# Monitor service principal usage
az monitor activity-log list \
  --caller YOUR_CLIENT_ID \
  --start-time 2024-01-01 \
  --end-time 2024-01-31
```

---

## 📊 Cost Comparison

### **Free Tier Setup (Learning):**
```bash
# Publish Profile - Free
az appservice plan create --sku F1  # $0/month
# + GitHub Actions: 2000 minutes free

# Federated Identity - Free  
az appservice plan create --sku F1  # $0/month
# + Azure AD operations: Free tier
# + GitHub Actions: 2000 minutes free
```

### **Production Setup:**
```bash
# Basic Production
az appservice plan create --sku B1  # ~$13/month
# + SSL certificate: $0 (Let's Encrypt) or $69/year (standard)
# + Custom domain: $12/year (domain registrar)

# Enterprise Production
az appservice plan create --sku S1  # ~$56/month
# + Deployment slots for staging
# + Custom domains included
# + Better performance and scaling
```

This comprehensive guide gives you both theoretical understanding and practical, copy-paste commands for real-world scenarios. Whether you're learning with a simple blog or deploying enterprise applications, you now have the complete toolkit! 🚀