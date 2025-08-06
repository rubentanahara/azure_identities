# 🔐 Azure Authentication Methods - Complete Guide

## 📊 Method Comparison Overview

| Aspect | Publish Profile | Federated Identity (OIDC) |
|--------|----------------|--------------------------|
| **Security Level** | Basic | Enterprise |
| **Secret Type** | Username/Password | No long-lived secrets |
| **Token Lifetime** | Until manually rotated | 15 minutes (auto-refresh) |
| **Setup Complexity** | Simple (5 minutes) | Moderate (15 minutes) |
| **GitHub Secrets** | 1 secret | 3 secrets |
| **Azure AD Integration** | No | Yes |
| **Audit Trail** | Basic App Service logs | Full Azure AD audit trail |
| **Compliance** | Basic | Enterprise-ready |
| **Cost** | Free | Free |
| **Microsoft Recommendation** | ❌ Legacy | ✅ Best Practice |

---

## 🏗️ Method 1: Publish Profile (Traditional)

### **What is Publish Profile?**
A publish profile is an XML file containing deployment credentials (username/password) that Azure App Service generates. It's the traditional, simple way to deploy applications.

### **How it Works:**
```mermaid
graph LR
    A[GitHub Actions] --> B[Publish Profile XML]
    B --> C[Username + Password]
    C --> D[Azure App Service]
    D --> E[Deployment]
```

### **📋 Setup Steps - Azure CLI:**

```bash
# 1. Create Azure Resources
az group create --name rg-app-001 --location eastus
az appservice plan create --name asp-app-001 --resource-group rg-app-001 --sku F1
az webapp create --name webapp-myapp --resource-group rg-app-001 --plan asp-app-001 --runtime "dotnet:8"

# 2. Download Publish Profile
az webapp deployment list-publishing-profiles \
  --name webapp-myapp \
  --resource-group rg-app-001 \
  --xml > publish-profile.xml

# 3. View the content to copy to GitHub Secret
cat publish-profile.xml
```

### **🖥️ Setup Steps - Azure Portal UI:**

1. **Create Resource Group:**
   - Go to Azure Portal → Resource Groups → Create
   - Name: `rg-app-001`
   - Region: `East US`
   - Click "Review + Create"

2. **Create App Service:**
   - Go to App Services → Create
   - Resource Group: `rg-app-001`
   - Name: `webapp-myapp`
   - Runtime: `.NET 8`
   - Operating System: `Windows`
   - Region: `East US`
   - Pricing Plan: `Free F1`
   - Click "Review + Create"

3. **Download Publish Profile:**
   - Go to your App Service → Overview
   - Click "Get publish profile" button
   - Save the downloaded `.PublishSettings` file
   - Open file and copy entire content

4. **Add GitHub Secret:**
   - Go to GitHub repository → Settings → Secrets and variables → Actions
   - Click "New repository secret"
   - Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
   - Value: Paste entire publish profile content
   - Click "Add secret"

### **📝 GitHub Workflow (Publish Profile):**

```yaml
name: Deploy with Publish Profile
on:
  push:
    branches: [ main ]
  workflow_dispatch:

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0'
    
    - name: Build and Deploy
      run: |
        dotnet restore
        dotnet build --configuration Release
        dotnet publish --configuration Release --output ./publish
    
    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: webapp-myapp
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

---

## 🔐 Method 2: Federated Identity (Modern/Secure)

### **What is Federated Identity?**
Federated Identity uses OpenID Connect (OIDC) to exchange GitHub's identity token for Azure access tokens. No passwords are stored anywhere - GitHub proves its identity to Azure AD using cryptographic tokens.

### **How it Works:**
```mermaid
graph LR
    A[GitHub Actions] --> B[GitHub OIDC Token]
    B --> C[Azure AD]
    C --> D[Azure Access Token]
    D --> E[Azure App Service]
    E --> F[Deployment]
    
    style C fill:#e1f5fe
    style D fill:#e8f5e8
```

### **📋 Setup Steps - Azure CLI:**

```bash
# 1. Create Azure Resources (same as Method 1)
az group create --name rg-app-001 --location eastus
az appservice plan create --name asp-app-001 --resource-group rg-app-001 --sku F1
az webapp create --name webapp-myapp --resource-group rg-app-001 --plan asp-app-001 --runtime "dotnet:8"

# 2. Get your subscription and tenant IDs
az account show --query "{subscriptionId: id, tenantId: tenantId}" -o table

# 3. Create Service Principal
az ad sp create-for-rbac \
  --name "sp-github-app" \
  --role contributor \
  --scopes /subscriptions/YOUR_SUBSCRIPTION_ID/resourceGroups/rg-app-001 \
  --json-auth

# 4. Note the clientId from the output, then create federated credential
APP_ID="YOUR_CLIENT_ID_FROM_STEP_3"

az ad app federated-credential create --id $APP_ID --parameters '{
  "name": "github-actions-main",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:YOUR_GITHUB_USERNAME/YOUR_REPO_NAME:ref:refs/heads/main",
  "description": "GitHub Actions federated credential",
  "audiences": ["api://AzureADTokenExchange"]
}'

# 5. Add these 3 values as GitHub Secrets:
# AZURE_CLIENT_ID: (clientId from step 3)
# AZURE_TENANT_ID: (tenantId from step 2)
# AZURE_SUBSCRIPTION_ID: (subscriptionId from step 2)
```

### **🖥️ Setup Steps - Azure Portal UI:**

#### **Part 1: Create App Service (same as Method 1)**

#### **Part 2: Create Service Principal and Federated Identity**

1. **Create Service Principal:**
   - Go to Azure Active Directory → App registrations → New registration
   - Name: `sp-github-app`
   - Supported account types: `Single tenant`
   - Click "Register"
   - **Copy the Application (client) ID** - you'll need this for GitHub

2. **Assign Permissions:**
   - Go to Subscriptions → Your subscription → Access control (IAM)
   - Click "Add" → "Add role assignment"
   - Role: `Contributor`
   - Assign access to: `User, group, or service principal`
   - Select: Search for `sp-github-app` and select it
   - Click "Review + assign"

3. **Create Federated Credential:**
   - Go back to Azure AD → App registrations → `sp-github-app`
   - Click "Certificates & secrets" → "Federated credentials" tab
   - Click "Add credential"
   - Federated credential scenario: `GitHub Actions deploying Azure resources`
   - Organization: `YOUR_GITHUB_USERNAME`
   - Repository: `YOUR_REPO_NAME`
   - Entity type: `Branch`
   - GitHub branch name: `main`
   - Name: `github-actions-main`
   - Click "Add"

4. **Get Required IDs:**
   - **Tenant ID**: Azure AD → Overview → Tenant ID
   - **Subscription ID**: Subscriptions → Your subscription → Subscription ID
   - **Client ID**: Already copied from step 1

5. **Add GitHub Secrets:**
   - Go to GitHub repository → Settings → Secrets and variables → Actions
   - Add these 3 secrets:
     - `AZURE_CLIENT_ID`: Application (client) ID
     - `AZURE_TENANT_ID`: Directory (tenant) ID
     - `AZURE_SUBSCRIPTION_ID`: Subscription ID

### **📝 GitHub Workflow (Federated Identity):**

```yaml
name: Deploy with Federated Identity
on:
  push:
    branches: [ main ]
  workflow_dispatch:

permissions:
  id-token: write  # Required for OIDC
  contents: read

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0'
    
    - name: Azure Login (OIDC)
      uses: azure/login@v1
      with:
        client-id: ${{ secrets.AZURE_CLIENT_ID }}
        tenant-id: ${{ secrets.AZURE_TENANT_ID }}
        subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
    
    - name: Build and Deploy
      run: |
        dotnet restore
        dotnet build --configuration Release
        dotnet publish --configuration Release --output ./publish
    
    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: webapp-myapp
        package: ./publish
    
    - name: Azure Logout
      run: az logout
```

---

## 🔍 Deep Dive: How Each Method Works

### **Publish Profile Authentication Flow:**
1. GitHub Actions reads publish profile XML from secrets
2. Extracts username/password from XML
3. Uses basic authentication to connect to Azure App Service
4. Deploys application using Web Deploy protocol
5. Connection remains authenticated for entire deployment

### **Federated Identity Authentication Flow:**
1. GitHub Actions requests OIDC token from GitHub
2. Token contains repository and workflow context
3. Azure AD validates token against federated credential
4. Azure AD issues short-lived access token (15 min)
5. GitHub Actions uses access token for Azure operations
6. Token automatically expires after deployment

---

## 🛡️ Security Comparison

### **Publish Profile Security:**
- ✅ Simple to set up
- ❌ Long-lived credentials stored in GitHub
- ❌ Manual rotation required
- ❌ Broad permissions (full app service access)
- ❌ Limited audit trail
- ❌ Credentials can be extracted if GitHub is compromised

### **Federated Identity Security:**
- ✅ No long-lived secrets in GitHub
- ✅ Automatic token rotation
- ✅ Scoped permissions (resource group level)
- ✅ Full Azure AD audit trail
- ✅ Cryptographically verified identity
- ✅ Zero Trust security model
- ✅ Cannot be compromised via GitHub secret exposure

---

## 🚀 Production Recommendations

### **When to Use Publish Profile:**
- Learning and development environments
- Quick prototypes and demos
- Simple applications with low security requirements
- When federated identity setup is too complex for the team

### **When to Use Federated Identity:**
- Production environments
- Enterprise applications
- Compliance-required environments (SOC2, ISO27001, etc.)
- Multi-environment deployments (dev/staging/prod)
- Teams with security-first culture
- Any application handling sensitive data

---

## 🔧 Troubleshooting Guide

### **Common Publish Profile Issues:**
```bash
# Issue: Invalid publish profile
# Solution: Re-download from Azure Portal
az webapp deployment list-publishing-profiles --name myapp --resource-group mygroup --xml

# Issue: Deployment fails with 401
# Solution: Check if publish profile is complete and valid
```

### **Common Federated Identity Issues:**
```bash
# Issue: AADSTS70021 - No matching federated identity
# Solution: Check repository name and branch in federated credential

# Issue: AADSTS700016 - Invalid client ID
# Solution: Verify AZURE_CLIENT_ID secret matches app registration

# Issue: Insufficient permissions
# Solution: Ensure service principal has contributor role on resource group
az role assignment list --assignee YOUR_CLIENT_ID --scope /subscriptions/YOUR_SUB/resourceGroups/YOUR_RG
```

---

## 📈 Migration Path

### **From Publish Profile to Federated Identity:**
1. Set up federated identity (keep publish profile working)
2. Test federated identity workflow in parallel
3. Once verified, disable publish profile workflow
4. Remove publish profile secret from GitHub
5. Update documentation and team processes

### **Migration Script:**
```bash
#!/bin/bash
# Disable publish profile workflow
mv .github/workflows/publish-profile.yml .github/workflows/publish-profile.yml.disabled

# Enable federated identity workflow
mv .github/workflows/federated-identity.yml.disabled .github/workflows/federated-identity.yml

# Commit changes
git add .
git commit -m "Migrate to federated identity authentication"
git push
```

---

## 🎯 Summary

Both methods achieve the same goal - deploying to Azure - but federated identity provides enterprise-grade security that's essential for production environments. The investment in setting up federated identity pays off through:

- **Enhanced security posture**
- **Simplified credential management**
- **Better compliance alignment**
- **Reduced operational overhead**
- **Future-proof authentication**

**Recommendation**: Start with publish profile for learning, migrate to federated identity for any serious application.
