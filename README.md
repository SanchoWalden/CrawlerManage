# SchoolManage 爬虫数据平台

本项目包含一个基于 .NET 9 的后端 API 与一个基于 Vite + Vue 3 + Vuetify 的前端界面，用于存储与管理爬虫采集的数据。

## 运行环境
- .NET SDK 9.0+
- SQL Server（默认连接字符串使用 `(localdb)\MSSQLLocalDB`，可在 `backend/CrawlerApi/appsettings.json` 中调整）
- Node.js 18+（已在仓库中使用 Node 22）

## 后端（`backend/CrawlerApi`）
- 启动接口: `dotnet run --project backend/CrawlerApi/CrawlerApi.csproj`
- 默认地址: `https://localhost:7238`
- 启动时会自动调用 `Database.MigrateAsync()`，确保数据库结构与最新迁移保持一致。

### 身份认证与授权
- 使用 ASP.NET Core Identity + JWT，所有 `/api/scraped-items` 路由均需 Bearer Token。
- 提供开放接口：
  - `POST /api/auth/register` 注册账号（默认授予 `User` 角色）。
  - `POST /api/auth/login` 登录并返回 `{ token, expiresAt, user }`。
- 系统启动时会自动创建 `Admin` 与 `User` 角色，如需管理员权限可手动将账号添加到 `Admin` 角色（可在后续扩展对应接口或使用数据库脚本）。
- JWT 配置位于 `appsettings.json` 的 `Jwt` 节，生产环境请替换 `Secret` 并按需调整生命周期。

### 迁移策略
1. **安装迁移工具（首次）**
   ```bash
   dotnet tool install --global dotnet-ef
   ```
2. **新增或调整实体后生成迁移**
   ```bash
   dotnet ef migrations add <MigrationName> --project backend/CrawlerApi/CrawlerApi.csproj --output-dir Data/Migrations
   ```
3. **应用迁移到数据库**
   ```bash
   dotnet ef database update --project backend/CrawlerApi/CrawlerApi.csproj
   ```
4. **注意事项**
   - 若使用不同的连接字符串，可通过 `--connection` 指定，或修改 `appsettings.json` / 环境变量。
   - 生产环境推荐在部署流水线中执行 `dotnet ef database update`，或在应用启动前自行迁移。
   - 现有迁移位于 `backend/CrawlerApi/Data/Migrations` 目录，并由 `ScraperDbContextFactory` 提供设计时上下文配置。

## 前端（`frontend`）
- 安装依赖: `npm install`
- 开发调试: `npm run dev`
- 构建产物: `npm run build`
- 通过环境变量 `VITE_API_BASE_URL` 指定后端地址（默认 `https://localhost:7238`）。

前端界面提供：
- 登录面板：输入邮箱/用户名与密码获取 JWT，登录信息持久化于 `localStorage`。
- 登录后可查看数据表格（Vuetify `VDataTable`）、刷新与新增爬虫记录（自动携带 Authorization 头）。
- 退出登录后本地令牌会被清除，随后的请求需要重新登录。

如需扩展功能，可在 `src/App.vue` 中继续完善交互逻辑或补充角色管理等模块。
