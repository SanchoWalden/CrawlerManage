# CrawlerManage 项目架构文档

## 项目概述
**CrawlerManage** 是一个全栈爬虫数据管理平台，采用前后端分离架构。

## 技术栈

### 后端技术栈
- **.NET 9.0** + ASP.NET Core Web API (Minimal API 风格)
- **Entity Framework Core 9.0** (ORM)
- **SQL Server** (数据库)
- **ASP.NET Core Identity + JWT** (身份认证与授权)
- **FluentValidation** (数据验证)
- **Swagger/OpenAPI** (API 文档)

### 前端技术栈
- **Vue 3** (Composition API + TypeScript)
- **Vite** (构建工具)
- **Vuetify 3** (Material Design UI 框架)
- **Vue Router** (路由管理)

---

## 最近优化 (2025-10-18)

### ✅ 已完成的优化

1. **安全配置增强**
   - 将敏感配置(JWT Secret, 数据库连接字符串)从 appsettings.json 移除
   - 添加 CORS 白名单配置 (`CorsOptions`)
   - 支持环境变量和 User Secrets 配置
   - 添加配置验证,防止遗漏必需配置
   - 创建 `CONFIGURATION.md` 配置指南

2. **全局错误处理**
   - 添加 `GlobalExceptionHandlerMiddleware`
   - 统一异常响应格式
   - 开发环境返回详细堆栈跟踪
   - 生产环境隐藏敏感信息

3. **数据验证**
   - 集成 FluentValidation 11.3.1
   - 为所有 DTOs 创建验证器:
     - `CreateScrapedItemRequestValidator`
     - `UpdateScrapedItemRequestValidator`
     - `RegisterRequestValidator`
     - `LoginRequestValidator`
   - 添加中文错误消息
   - URL 格式验证
   - 时间合法性验证

4. **数据库索引优化**
   - 添加多个索引以提升查询性能:
     - `IX_ScrapedItems_Url` - URL 查询
     - `IX_ScrapedItems_Source` - 来源过滤
     - `IX_ScrapedItems_CollectedAt_Desc` - 时间排序
     - `IX_ScrapedItems_CollectedAt_Source` - 复合索引(时间+来源)
     - `IX_ScrapedItems_Title` - 标题搜索
   - 生成迁移: `OptimizeScrapedItemIndexes`

---

## 主要入口

### 后端入口
- **主入口文件**: `backend/CrawlerApi/Program.cs:16`
- **启动命令**: `dotnet run --project backend/CrawlerApi/CrawlerApi.csproj`
- **默认地址**: `https://localhost:7238`
- **Swagger 文档**: `https://localhost:7238/swagger`

**启动流程**:
1. 配置服务 (DI 容器): 数据库、Identity、JWT、CORS、Swagger (`Program.cs:18-122`)
2. 应用中间件: 自动数据库迁移、角色初始化 (`Program.cs:125-138`)
3. 配置中间件管道: HTTPS、CORS、认证授权 (`Program.cs:150-153`)
4. 定义 Minimal API 端点 (`Program.cs:155-398`):
   - `/api/health` - 健康检查
   - `/api/auth/register` - 注册
   - `/api/auth/login` - 登录
   - `/api/scraped-items` - CRUD 操作 (需认证)

### 前端入口
- **主入口文件**: `frontend/src/main.ts:1`
- **启动命令**: `npm run dev`
- **默认地址**: `http://localhost:5173`

**启动流程**:
1. 创建 Vue 应用实例 (`main.ts:7`)
2. 注册 Vuetify 插件 (`main.ts:7`)
3. 注册 Vue Router (`main.ts:7`)
4. 挂载到 DOM (`main.ts:7`)

**路由配置** (`frontend/src/router/index.ts`):
- `/` → 重定向到 `/dashboard`
- `/login` - 登录页面
- `/dashboard` - 数据管理页面 (需认证)
- 路由守卫: 未认证用户自动重定向到登录页 (`router/index.ts:28-38`)

---

## 项目结构

```
CrawlerManage/
├── backend/CrawlerApi/
│   ├── Controllers/         # (未使用,采用 Minimal API)
│   ├── Data/                # 数据库上下文 + EF 迁移
│   ├── Models/              # 实体模型 + DTOs
│   ├── Services/            # JWT Token 服务
│   ├── Options/             # 配置类 (JwtOptions)
│   ├── Constants/           # 常量定义 (角色常量)
│   ├── Program.cs           # 应用主入口
│   └── appsettings.json     # 配置文件
│
├── frontend/src/
│   ├── components/          # Vue 组件
│   ├── views/               # 页面视图 (Login/Dashboard)
│   ├── services/            # API 调用 + 认证服务
│   ├── router/              # 路由配置
│   ├── types/               # TypeScript 类型定义
│   ├── plugins/             # Vuetify 插件配置
│   ├── main.ts              # 前端入口
│   └── App.vue              # 根组件
│
└── README.md                # 项目文档
```

---

## 测试方式

**当前项目未配置自动化测试框架**。没有找到测试文件或测试配置。

### 推荐的测试方式

#### 后端测试
1. **手动测试**:
   - 使用 Swagger UI: `https://localhost:7238/swagger`
   - 使用 `CrawlerApi.http` 文件 (VS Code REST Client)

2. **推荐添加测试框架**:
   - xUnit / NUnit / MSTest
   - 集成测试: WebApplicationFactory
   - 单元测试: Moq (模拟依赖)

#### 前端测试
1. **手动测试**:
   - 运行 `npm run dev` 后在浏览器中测试功能

2. **推荐添加测试框架**:
   - Vitest (单元测试)
   - Vue Test Utils (组件测试)
   - Playwright / Cypress (E2E 测试)

#### 数据库测试
- **迁移验证**: `dotnet ef migrations add TestMigration` 验证模型修改
- **迁移应用**: `dotnet ef database update` (自动在启动时执行)

---

## 核心功能模块

### 认证与授权
- **JWT 认证**: 基于 Bearer Token
- **角色系统**: `Admin`, `User` (`Constants/AppRoles`)
- **受保护的端点**: `/api/scraped-items/*` 需要认证

### 数据管理
- **爬虫数据 CRUD**: 增删改查操作
- **过滤与搜索**: 支持按标题、来源、时间范围筛选
- **分页**: 默认 20 条/页,最大 100 条/页

### 数据库管理
- **自动迁移**: 应用启动时自动执行 `Database.MigrateAsync()` (`Program.cs:128`)
- **迁移文件**: `backend/CrawlerApi/Data/Migrations/`

---

## 环境配置

### 数据库配置
- **连接字符串**: `backend/CrawlerApi/appsettings.json`
- **默认数据库**: SQL Server, `ScraperDb`

### 前端 API 配置
- **环境变量**: `VITE_API_BASE_URL` (默认 `https://localhost:7238`)

### JWT 配置
- **密钥**: `appsettings.json` → `Jwt:Secret` (生产环境需更换强密钥)
- **有效期**: 120 分钟

---

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

---

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
