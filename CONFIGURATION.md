# Configuration Guide

## Environment-Specific Configuration

The application uses different configuration files for each environment:

- `appsettings.json` - Base configuration (shared across environments)
- `appsettings.Development.json` - Development environment overrides
- `appsettings.Production.json` - Production environment overrides

Configuration files are loaded in order, with later files overriding earlier ones.

---

## Development Environment

**Setting:** `ASPNETCORE_ENVIRONMENT=Development`

**Features:**
- Verbose logging (Debug level)
- Longer JWT token expiration (24 hours)
- Multiple CORS origins allowed
- Connects to localhost hardware simulator
- Swagger UI enabled
- Detailed error messages with stack traces

**Hardware Settings:**
```json
{
  "Hardware": {
    "Host": "localhost",
    "Port": 5000
  }
}
```

**JWT Settings:**
```json
{
  "JwtSettings": {
    "SecretKey": "DevSecretKeyForLocalDevelopmentOnly123456!",
    "ExpirationHours": 24
  }
}
```

**Credentials:**
- Admin: `admin` / `admin123`
- Operator: `operator` / `operator123`

---

## Production Environment

**Setting:** `ASPNETCORE_ENVIRONMENT=Production`

**Features:**
- Production logging (Warning level)
- Standard JWT token expiration (8 hours)
- Strict CORS origins
- Connects to actual hardware
- Swagger UI disabled
- Generic error messages (no stack traces)

### Required Environment Variables

Production configuration uses environment variables for sensitive data:

#### 1. JWT Secret Key (REQUIRED)

```bash
# Linux/macOS
export JWT_SECRET_KEY="your-super-secret-production-key-min-32-chars"

# Windows PowerShell
$env:JWT_SECRET_KEY="your-super-secret-production-key-min-32-chars"

# Windows CMD
set JWT_SECRET_KEY=your-super-secret-production-key-min-32-chars

# Docker
docker run -e JWT_SECRET_KEY="your-secret-key" ...
```

**Requirements:**
- Minimum 32 characters
- Use cryptographically random string
- Never commit to source control
- Rotate periodically

**Generate secure key:**
```bash
# Linux/macOS
openssl rand -base64 32

# PowerShell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
```

#### 2. Hardware Connection (Optional)

Override hardware connection settings:

```bash
export HARDWARE__HOST="192.168.1.100"
export HARDWARE__PORT="502"
```

Note: Use double underscore `__` for nested configuration.

#### 3. CORS Origins (Optional)

```bash
export CORS__ALLOWEDORIGINS__0="https://robotcontrol.example.com"
export CORS__ALLOWEDORIGINS__1="https://robotcontrol-backup.example.com"
```

---

## Configuration in Different Deployment Scenarios

### Local Development

```bash
# .NET CLI
dotnet run --environment Development

# PowerShell
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run
```

### Docker

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
ENV ASPNETCORE_ENVIRONMENT=Production
COPY . /app
WORKDIR /app
ENTRYPOINT ["dotnet", "RoboticControl.API.dll"]
```

```bash
# Docker run with environment variables
docker run \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e JWT_SECRET_KEY="your-secret-key" \
  -e HARDWARE__HOST="192.168.1.100" \
  -p 5001:5001 \
  robotcontrol-api
```

### Azure App Service

```bash
# Azure CLI
az webapp config appsettings set \
  --name robotcontrol-api \
  --resource-group myResourceGroup \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    JWT_SECRET_KEY="your-secret-key" \
    HARDWARE__HOST="192.168.1.100"
```

Or via Azure Portal:
1. Go to App Service → Configuration
2. Application Settings → New application setting
3. Add: `JWT_SECRET_KEY` = `your-secret-key`

### Kubernetes

```yaml
# secrets.yaml
apiVersion: v1
kind: Secret
metadata:
  name: robotcontrol-secrets
type: Opaque
stringData:
  jwt-secret-key: your-super-secret-production-key

---
# deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: robotcontrol-api
spec:
  template:
    spec:
      containers:
      - name: api
        image: robotcontrol-api:latest
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: JWT_SECRET_KEY
          valueFrom:
            secretKeyRef:
              name: robotcontrol-secrets
              key: jwt-secret-key
        - name: HARDWARE__HOST
          value: "192.168.1.100"
```

### systemd Service (Linux)

```ini
# /etc/systemd/system/robotcontrol-api.service
[Service]
Environment="ASPNETCORE_ENVIRONMENT=Production"
Environment="JWT_SECRET_KEY=your-secret-key"
Environment="HARDWARE__HOST=192.168.1.100"
ExecStart=/usr/bin/dotnet /opt/robotcontrol/RoboticControl.API.dll
```

---

## Configuration Priority (Lowest to Highest)

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. **Environment Variables** ← Highest priority
4. Command-line arguments

Example: If `appsettings.Production.json` sets `Hardware:Host=192.168.1.100` but environment variable `HARDWARE__HOST=192.168.1.200` is set, the app uses `192.168.1.200`.

---

## Verification

### Check Active Configuration

```csharp
// Add temporary endpoint to verify configuration (remove in production!)
app.MapGet("/api/config/verify", (IConfiguration config) => new
{
    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
    hardwareHost = config["Hardware:Host"],
    jwtSecretConfigured = !string.IsNullOrEmpty(config["JwtSettings:SecretKey"]),
    corsOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>()
});
```

### Health Check

Visit: `https://your-domain/health`

Expected response:
```json
{
  "status": "healthy",
  "timestamp": "2026-02-11T14:30:00Z"
}
```

---

## Security Best Practices

✅ **DO:**
- Use environment variables for secrets in production
- Rotate JWT secret keys periodically (every 90 days)
- Use different secrets for dev/staging/production
- Store secrets in Azure Key Vault, AWS Secrets Manager, or similar
- Limit CORS origins to specific domains
- Use HTTPS in production

❌ **DON'T:**
- Commit secrets to source control
- Use default/example secrets in production
- Share secrets via email or chat
- Reuse secrets across environments
- Use weak or short secret keys

---

## Troubleshooting

### "JWT Secret Key not configured" Error

**Cause:** `JWT_SECRET_KEY` environment variable not set in production.

**Solution:**
```bash
export JWT_SECRET_KEY="your-32-char-minimum-secret-key"
```

### CORS Errors in Production

**Cause:** Frontend URL not in `Cors:AllowedOrigins`.

**Solution:** Update `appsettings.Production.json` or set environment variable:
```bash
export CORS__ALLOWEDORIGINS__0="https://your-frontend-domain.com"
```

### Cannot Connect to Hardware

**Cause:** Wrong hardware host/port in configuration.

**Solution:** Override with environment variables:
```bash
export HARDWARE__HOST="192.168.1.100"
export HARDWARE__PORT="502"
```

---

## Example: Complete Production Setup

```bash
# 1. Generate secure JWT secret
JWT_SECRET=$(openssl rand -base64 32)

# 2. Set environment variables
export ASPNETCORE_ENVIRONMENT=Production
export JWT_SECRET_KEY="$JWT_SECRET"
export HARDWARE__HOST="192.168.1.100"
export HARDWARE__PORT="502"
export CORS__ALLOWEDORIGINS__0="https://robotcontrol.example.com"

# 3. Run application
dotnet RoboticControl.API.dll

# Application now uses:
# - Production logging (Warning level)
# - Secure JWT secret from environment
# - Real hardware connection
# - Restricted CORS
```

---

## Questions?

- Check logs in `logs/robotcontrol-*.txt`
- Verify environment: `echo $ASPNETCORE_ENVIRONMENT`
- Test configuration: Visit `/health` endpoint
- Review this guide: `CONFIGURATION.md`
