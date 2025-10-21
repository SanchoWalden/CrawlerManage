# 环境配置说明

## 开发环境

开发环境的配置已在 `appsettings.Development.json` 中设置,无需额外配置。

## 生产环境

### 方法 1: 环境变量 (推荐)

设置以下环境变量:

```bash
# 数据库连接字符串
ConnectionStrings__Default="Server=your-server;Database=ScraperDb;User Id=your-user;Password=your-password;..."

# JWT 密钥 (至少 32 字符的强密钥)
Jwt__Secret="your-production-secret-key-at-least-32-characters-long"

# CORS 允许的前端域名
Cors__AllowedOrigins__0="https://your-frontend-domain.com"
Cors__AllowedOrigins__1="https://www.your-frontend-domain.com"
```

### 方法 2: appsettings.Production.json

直接编辑 `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Server=your-server;Database=ScraperDb;..."
  },
  "Jwt": {
    "Secret": "your-production-secret-key-at-least-32-characters-long"
  },
  "Cors": {
    "AllowedOrigins": ["https://your-frontend-domain.com"]
  }
}
```

**注意**: 此文件应添加到 `.gitignore`,避免敏感信息泄露。

### 方法 3: User Secrets (本地开发)

```bash
cd backend/CrawlerApi
dotnet user-secrets set "ConnectionStrings:Default" "your-connection-string"
dotnet user-secrets set "Jwt:Secret" "your-secret-key"
```

## 配置优先级

1. 环境变量 (最高优先级)
2. User Secrets (开发环境)
3. appsettings.{Environment}.json
4. appsettings.json (最低优先级)

## 安全建议

1. **JWT Secret**: 生产环境必须使用至少 32 字符的随机强密钥
2. **数据库密码**: 使用强密码,避免硬编码
3. **CORS**: 仅添加信任的前端域名,不要使用 `AllowAnyOrigin`
4. **HTTPS**: 生产环境强制使用 HTTPS
