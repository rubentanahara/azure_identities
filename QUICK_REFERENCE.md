# 🚀 Quick Reference - Azure Authentication Methods

## ⚡ TL;DR - Which Method to Choose?

| Your Situation | Recommended Method | Setup Time |
|----------------|-------------------|------------|
| 🎓 Learning Azure | Publish Profile | 5 minutes |
| 🏢 Production app | Federated Identity | 15 minutes |
| 💰 Budget constraints | Publish Profile (F1 free) | 5 minutes |
| 🔒 Security required | Federated Identity | 15 minutes |
| 👥 Team/Enterprise | Federated Identity | 15 minutes |

---

## 🔧 Quick Setup Commands

### **Method 1: Publish Profile (Simple)**
```bash
# 1. Create resources
az group create --name rg-myapp --location eastus
az appservice plan create --name asp-myapp --resource-group rg-myapp --sku F1
az webapp create --name webapp-myapp --resource-group rg-myapp --plan asp-myapp --runtime "dotnet:8"

# 2. Get credentials
az webapp deployment list-publishing-profiles --name webapp-myapp --resource-group rg-myapp --xml

# 3. Add to GitHub as secret: AZURE_WEBAPP_PUBLISH_PROFILE
```

### **Method 2: Federated Identity (Secure)**
```bash
# 1. Create resources (same as above)
az group create --name rg-myapp --location eastus
az appservice plan create --name asp-myapp --resource-group rg-myapp --sku F1  
az webapp create --name webapp-myapp --resource-group rg-myapp --plan asp-myapp --runtime "dotnet:8"

# 2. Create service principal
az ad sp create-for-rbac --name "sp-github-myapp" --role contributor --scopes /subscriptions/$(az account show --query id -o tsv)/resourceGroups/rg-myapp --json-auth

# 3. Create federated credential (replace YOUR_GITHUB_USERNAME/YOUR_REPO)
APP_ID="OUTPUT_FROM_STEP_2"
az ad app federated-credential create --id $APP_ID --parameters '{
  "name": "github-actions-main",
  "issuer": "https://token.actions.githubusercontent.com", 
  "subject": "repo:YOUR_GITHUB_USERNAME/YOUR_REPO:ref:refs/heads/main",
  "audiences": ["api://AzureADTokenExchange"]
}'

# 4. Add to GitHub as 3 secrets:
# AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID
```

---

## 📝 GitHub Workflow Templates

### **Publish Profile Workflow:**
```yaml
name: Deploy (Publish Profile)
on: { push: { branches: [main] } }
jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    - uses: actions/setup-dotnet@v4
      with: { dotnet-version: '8.0' }
    - run: dotnet publish -c Release -o ./publish
    - uses: azure/webapps-deploy@v2
      with:
        app-name: webapp-myapp
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

### **Federated Identity Workflow:**
```yaml
name: Deploy (Federated Identity)  
on: { push: { branches: [main] } }
permissions: { id-token: write, contents: read }
jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    - uses: actions/setup-dotnet@v4
      with: { dotnet-version: '8.0' }
    - uses: azure/login@v1
      with:
        client-id: ${{ secrets.AZURE_CLIENT_ID }}
        tenant-id: ${{ secrets.AZURE_TENANT_ID }}
        subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
    - run: dotnet publish -c Release -o ./publish
    - uses: azure/webapps-deploy@v2
      with:
        app-name: webapp-myapp
        package: ./publish
```

---

## 🐛 Quick Troubleshooting

### **Common Issues & Solutions:**

| Error | Method | Solution |
|-------|--------|----------|
| `401 Unauthorized` | Both | Check credentials/secrets are correct |
| `App not found` | Both | Verify app name in workflow matches Azure |
| `Invalid publish profile` | Publish Profile | Re-download from Azure Portal |
| `AADSTS70021` | Federated Identity | Check repository name in federated credential |
| `Insufficient permissions` | Federated Identity | Verify service principal has Contributor role |

### **Quick Diagnostic Commands:**
```bash
# Test Azure login
az account show

# Check app exists
az webapp show --name webapp-myapp --resource-group rg-myapp

# Check service principal permissions
az role assignment list --assignee YOUR_CLIENT_ID
```

---

## 🔍 Azure Portal UI Paths

### **Download Publish Profile:**
```
Azure Portal → App Services → Your App → Overview → "Get publish profile"
```

### **Create Service Principal:**
```
Azure Portal → Azure Active Directory → App registrations → "New registration"
```

### **Add Federated Credential:**
```
App registration → Certificates & secrets → Federated credentials → "Add credential"
```

### **Assign Permissions:**
```
Subscriptions → Your subscription → Access control (IAM) → "Add role assignment"
```

---

## 📊 Security Comparison

| Aspect | Publish Profile | Federated Identity |
|--------|----------------|-------------------|
| **Secrets in GitHub** | ❌ Username/Password | ✅ Only IDs (no secrets) |
| **Token Lifetime** | ❌ Until rotated | ✅ 15 minutes |
| **Audit Trail** | ❌ Basic | ✅ Full Azure AD logs |
| **Setup Complexity** | ✅ Very Simple | ⚠️ Moderate |
| **Enterprise Ready** | ❌ No | ✅ Yes |

---

## 🎯 Migration Checklist

### **From Publish Profile → Federated Identity:**
- [ ] Set up federated identity (parallel to existing)
- [ ] Test federated workflow
- [ ] Disable publish profile workflow  
- [ ] Remove publish profile secret
- [ ] Update team documentation

### **Commands:**
```bash
# Disable old workflow
mv deploy-publish-profile.yml deploy-publish-profile.yml.disabled

# Test new method works
git push # Triggers federated identity workflow

# Remove old secret from GitHub UI after verification
```

---

## 🚀 Next Steps

After mastering both methods:
1. **Add Application Insights** for monitoring
2. **Set up staging slots** for blue-green deployments  
3. **Implement Infrastructure as Code** (Terraform/Bicep)
4. **Add custom domains** and SSL certificates
5. **Set up CI/CD for multiple environments**

**You now have complete mastery of Azure authentication methods!** 🏆