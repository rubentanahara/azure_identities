# Azure Identities API - Deployment Guide

## Overview
This project demonstrates two Azure identity authentication methods for deployment:
1. **Publish Profile** (traditional method using credentials)
2. **Federated Identity** (modern, secure method using OIDC)

## Prerequisites
- Azure subscription
- Azure CLI installed
- GitHub repository
- .NET 8 SDK

## Step 1: Azure App Service Setup

### Create Resource Group and App Service
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

## Step 2: Publish Profile Method

### Download Publish Profile
```bash
# Download publish profile
az webapp deployment list-publishing-profiles --name webapp-azure-identities-api --resource-group rg-azure-identities --xml
```

### GitHub Secrets for Publish Profile
Add the following secret to your GitHub repository:
- `AZURE_WEBAPP_PUBLISH_PROFILE`: Content of the downloaded publish profile

## Step 3: Federated Identity Method

### Create Service Principal
```bash
# Create service principal
az ad sp create-for-rbac --name "sp-azure-identities-github" --role contributor --scopes /subscriptions/{subscription-id}/resourceGroups/rg-azure-identities --json-auth
```

### Configure Federated Credentials
```bash
# Get application ID
APP_ID=$(az ad sp list --display-name "sp-azure-identities-github" --query "[0].appId" -o tsv)

# Create federated credential
az ad app federated-credential create --id $APP_ID --parameters '{
  "name": "github-actions-federated-credential",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:YOUR_GITHUB_USERNAME/azure_identities:ref:refs/heads/main",
  "description": "GitHub Actions Federated Credential",
  "audiences": ["api://AzureADTokenExchange"]
}'
```

### GitHub Secrets for Federated Identity
Add the following secrets to your GitHub repository:
- `AZURE_CLIENT_ID`: Application (client) ID
- `AZURE_TENANT_ID`: Directory (tenant) ID
- `AZURE_SUBSCRIPTION_ID`: Subscription ID

## Deployment Commands
The GitHub Actions workflows will handle the deployment automatically.