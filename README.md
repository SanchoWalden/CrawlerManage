# CrawlerManage 爬虫数据管理平台

本项目是一个全栈应用，提供爬虫数据的存储、管理和可视化功能。

## 技术栈

### 后端
- .NET 9.0
- ASP.NET Core Web API
- Entity Framework Core 9.0
- SQL Server
- ASP.NET Core Identity + JWT 身份认证
- Swagger/OpenAPI

### 前端
- Vue 3 (Composition API + TypeScript)
- Vite
- Vuetify 3
- Vue Router

## 运行环境
- .NET SDK 9.0+
- SQL Server（默认连接字符串配置为本地 SQL Server，数据库：`ScraperDb`，可在 `backend/CrawlerApi/appsettings.json` 中调整）
- Node.js 18+（推荐 Node 22）

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

### 前端功能
- **用户认证**：登录/注册界面，JWT Token 持久化于 `localStorage`
- **数据管理**：
  - 数据列表展示（Vuetify VDataTable）
  - 新增爬虫记录
  - 编辑记录
  - 删除记录
  - 数据刷新
- **用户界面**：响应式设计，Material Design 风格（Vuetify）
- **路由管理**：基于 Vue Router 的页面导航和路由守卫

## 快速开始

### 1. 配置数据库
确保 SQL Server 已安装并运行，修改 `backend/CrawlerApi/appsettings.json` 中的连接字符串（如需要）。

### 2. 启动后端
```bash
cd backend/CrawlerApi
dotnet run
```
应用启动时会自动执行数据库迁移，确保数据库结构与最新代码一致。

### 3. 启动前端
```bash
cd frontend
npm install
npm run dev
```

### 4. 访问应用
- 前端：`http://localhost:5173`（默认 Vite 端口）
- 后端 API：`https://localhost:7238`
- Swagger 文档：`https://localhost:7238/swagger`

## 主要 API 端点

### 身份认证（开放接口）
- `POST /api/auth/register` - 用户注册（默认分配 `User` 角色）
- `POST /api/auth/login` - 用户登录，返回 JWT Token

### 数据管理（需要认证）
- `GET /api/scraped-items` - 获取爬虫数据列表
- `GET /api/scraped-items/{id}` - 获取单条记录
- `POST /api/scraped-items` - 创建新记录
- `PUT /api/scraped-items/{id}` - 更新记录
- `DELETE /api/scraped-items/{id}` - 删除记录

所有数据管理接口均需要在请求头中携带有效的 JWT Token：
```
Authorization: Bearer {your-token}
```

## 项目结构

```
CrawlerManage/
├── backend/
│   └── CrawlerApi/
│       ├── Controllers/         # API 控制器
│       ├── Data/                # 数据库上下文和迁移
│       ├── Models/              # 实体模型和 DTO
│       ├── Services/            # 业务服务（JWT 等）
│       ├── Options/             # 配置选项
│       ├── Constants/           # 常量定义
│       ├── appsettings.json     # 配置文件
│       └── Program.cs           # 应用入口
├── frontend/
│   ├── src/
│   │   ├── components/          # Vue 组件
│   │   ├── views/               # 页面视图
│   │   ├── services/            # API 服务和认证逻辑
│   │   ├── router/              # 路由配置
│   │   ├── types/               # TypeScript 类型定义
│   │   ├── App.vue              # 根组件
│   │   └── main.ts              # 应用入口
│   ├── package.json
│   └── vite.config.ts
└── README.md
```

## 开发说明

### 环境变量
前端通过 `VITE_API_BASE_URL` 环境变量指定后端地址，默认为 `https://localhost:7238`。

### 安全配置
- JWT Secret 配置在 `appsettings.json` 的 `Jwt:Secret` 节
- **生产环境务必替换为强密钥**
- Token 默认有效期：120 分钟（可在配置中调整）

### 扩展功能
如需添加管理员功能或更多业务逻辑，可参考以下扩展点：
- 角色管理：系统已内置 `Admin` 和 `User` 角色
- 权限控制：在控制器中使用 `[Authorize(Roles = "Admin")]`
- 前端路由守卫：在 `router/index.ts` 中扩展权限检查
