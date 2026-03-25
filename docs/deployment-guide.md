# Green Suppliers — Deployment Guide

## Pre-Deployment Checklist

### 1. Back Up WordPress (BEFORE anything else)

```bash
# SSH into the WordPress host or use hosting panel
# Export full database
mysqldump -u [user] -p [database] > greensuppliers_wp_backup_$(date +%Y%m%d).sql

# Archive wp-content (themes, plugins, uploads, media)
tar -czf greensuppliers_wp_content_$(date +%Y%m%d).tar.gz /path/to/wp-content/

# Upload both to Azure Blob Storage for safekeeping
az storage blob upload --account-name [storage] --container wordpress-backup \
  --file greensuppliers_wp_backup_*.sql --name wp-database.sql
az storage blob upload --account-name [storage] --container wordpress-backup \
  --file greensuppliers_wp_content_*.tar.gz --name wp-content.tar.gz
```

### 2. Provision Azure Resources

```bash
# Resource group
az group create --name rg-greensuppliers --location southafricanorth

# Azure SQL Database (Standard S0 — required for full-text search)
az sql server create --name sql-greensuppliers --resource-group rg-greensuppliers \
  --location southafricanorth --admin-user sqladmin --admin-password [STRONG_PASSWORD]
az sql db create --resource-group rg-greensuppliers --server sql-greensuppliers \
  --name GreenSuppliers --service-objective S0
# Allow Azure services
az sql server firewall-rule create --resource-group rg-greensuppliers \
  --server sql-greensuppliers --name AllowAzureServices \
  --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

# Azure App Service (API)
az appservice plan create --name plan-greensuppliers --resource-group rg-greensuppliers \
  --location southafricanorth --sku B1 --is-linux
az webapp create --name api-greensuppliers --resource-group rg-greensuppliers \
  --plan plan-greensuppliers --runtime "DOTNETCORE:8.0"

# Azure App Service (Worker)
az webapp create --name worker-greensuppliers --resource-group rg-greensuppliers \
  --plan plan-greensuppliers --runtime "DOTNETCORE:8.0"
# Set as "always on" for background jobs
az webapp config set --name worker-greensuppliers --resource-group rg-greensuppliers \
  --always-on true

# Azure Blob Storage
az storage account create --name stgreensuppliers --resource-group rg-greensuppliers \
  --location southafricanorth --sku Standard_LRS
az storage container create --name documents --account-name stgreensuppliers
az storage container create --name wordpress-backup --account-name stgreensuppliers

# Azure Key Vault
az keyvault create --name kv-greensuppliers --resource-group rg-greensuppliers \
  --location southafricanorth

# Application Insights
az monitor app-insights component create --app ai-greensuppliers \
  --resource-group rg-greensuppliers --location southafricanorth
```

### 3. Configure App Settings

```bash
# API App Service settings
az webapp config appsettings set --name api-greensuppliers \
  --resource-group rg-greensuppliers --settings \
  "ConnectionStrings__DefaultConnection=Server=tcp:sql-greensuppliers.database.windows.net,1433;Database=GreenSuppliers;User ID=sqladmin;Password=[PASSWORD];Encrypt=True;TrustServerCertificate=False;" \
  "Jwt__Secret=[64-CHAR-SECRET]" \
  "Jwt__Issuer=GreenSuppliers" \
  "Jwt__Audience=GreenSuppliers" \
  "Jwt__AccessTokenExpiryMinutes=60" \
  "Jwt__RefreshTokenExpiryDays=7" \
  "Cors__AllowedOrigins__0=https://www.greensuppliers.co.za" \
  "Cors__AllowedOrigins__1=https://greensuppliers.co.za" \
  "AzureBlobStorage__ConnectionString=[BLOB_CONNECTION_STRING]" \
  "AzureBlobStorage__ContainerName=documents"

# Worker App Service settings (same DB connection + email config)
az webapp config appsettings set --name worker-greensuppliers \
  --resource-group rg-greensuppliers --settings \
  "ConnectionStrings__DefaultConnection=[SAME_AS_ABOVE]"
```

### 4. Run Database Migration

```bash
# From local machine (or Azure Cloud Shell)
cd src/GreenSuppliers.Api
dotnet ef database update --connection "Server=tcp:sql-greensuppliers.database.windows.net,1433;..."
```

### 5. Enable Full-Text Search on Azure SQL

Connect to Azure SQL via SSMS or Azure Data Studio:

```sql
-- Create full-text catalog
CREATE FULLTEXT CATALOG GreenSuppliersCatalog AS DEFAULT;

-- Create full-text index on SupplierProfiles
CREATE FULLTEXT INDEX ON SupplierProfiles (TradingName, Description, ShortDescription)
  KEY INDEX PK_SupplierProfiles ON GreenSuppliersCatalog;
```

### 6. Seed Initial Data

The seed data migration runs automatically on first startup. Verify:
- 10 countries seeded
- 8 industries seeded
- Common certification types seeded
- Admin user created (email: admin@greensuppliers.co.za)

---

## Deploy Frontend to Vercel

### First-time setup:

1. **Import project on Vercel:**
   - Go to vercel.com → New Project
   - Import from GitHub repo
   - Set **Root Directory** to `web/green-suppliers-web`
   - Framework Preset: Next.js (auto-detected)

2. **Set environment variables:**
   ```
   NEXT_PUBLIC_API_URL = https://api-greensuppliers.azurewebsites.net/api/v1
   ```

3. **Configure custom domain:**
   - Add `greensuppliers.co.za` and `www.greensuppliers.co.za`
   - Vercel provides DNS records to configure

### Subsequent deploys:
Push to `main` branch → Vercel auto-deploys.

---

## DNS Cutover (greensuppliers.co.za)

**Only do this after verifying the new site works on the Vercel preview URL.**

### At your DNS provider:

```
# Point apex domain to Vercel
A     @     76.76.21.21

# Point www to Vercel
CNAME www   cname.vercel-dns.com.

# Point API subdomain to Azure App Service
CNAME api   api-greensuppliers.azurewebsites.net.
```

### Post-cutover verification:
1. Check https://www.greensuppliers.co.za loads the new site
2. Check https://api.greensuppliers.co.za/api/v1/suppliers returns JSON
3. Check Google Search Console for crawl errors
4. Monitor Application Insights for errors

---

## GitHub Secrets Required

| Secret | Description |
|--------|-------------|
| `AZURE_API_PUBLISH_PROFILE` | Download from Azure Portal → API App Service → Get Publish Profile |
| `AZURE_WORKER_PUBLISH_PROFILE` | Download from Azure Portal → Worker App Service → Get Publish Profile |

| Variable | Description |
|----------|-------------|
| `AZURE_API_APP_NAME` | `api-greensuppliers` |
| `AZURE_WORKER_APP_NAME` | `worker-greensuppliers` |

---

## Rollback Plan

If something goes wrong after DNS cutover:

1. **Revert DNS** to point back to WordPress hosting
2. WordPress backup is in Azure Blob Storage
3. New site remains on Vercel preview URL for debugging
