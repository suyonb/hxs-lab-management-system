# HXS 实验室管理系统交接文档

> 最后同步：2026-08-31。本文顶部为当前有效交接状态；后续按日期排列的阶段记录用于保留实施历史。

## 当前实施状态（2026-08-31）

项目阶段 0 至阶段 6 的一期代码、数据库迁移、权限菜单和本地 Oracle 运行态验收已经完成。当前结论为 **一期 MVP 可进入目标环境部署**；生产机密钥、备份恢复、日志保留和真实业务 GLB 素材仍按 `docs/PRODUCTION_CHECKLIST.md` 在部署时执行。

| 阶段 | 状态 | 当前结论 |
| --- | --- | --- |
| 阶段 0：需求定稿 | 已完成 | 一期边界、角色权限、状态机和验收规则已确定 |
| 阶段 1：系统底座 | 已完成 | JWT、动态权限、数据范围、审计、文件服务和异常规范已落地 |
| 阶段 2：基础数据 | 已完成 | 实验室、位置、课题组、供应商和数据字典可维护 |
| 阶段 3：仪器闭环 | 已验收 | 台账、预约、审批、使用和报修维修闭环通过 |
| 阶段 4：库存闭环 | 已验收 | 入库、领用、审批扣减、流水和预警闭环通过 |
| 阶段 5：实验记录 | 已验收 | 实验任务、关联、过程记录、附件和归档闭环通过 |
| 阶段 6：上线收尾 | 已完成 | 统一审批、统计导出、动态 2D/3D、GLB 管理、权限及运行态验收通过 |

### 阶段 6 已实现

- 统一审批中心聚合仪器预约、试剂领用和设备报修，支持我的申请、待我审批和已审批查询。
- 实验室首页已展示预约、维修、库存预警、实验任务和趋势等业务统计。
- 仪器、预约、库存、库存流水、领用和实验记录支持带权限及筛选条件的 Excel 导出。
- 新增 `HXS_LAB_3D_SCENE`、`HXS_LAB_3D_NODE`、`HXS_LAB_3D_BINDING` 三张 3D 数据表及领域实体。
- 新增动态空间布局及状态接口，根据现有实验室、楼栋、楼层、房间和仪器台账生成展示数据。
- 3D 页面已支持实验室切换、楼层分层、房间号标签、进入房间、仪器查看、搜索、状态筛选、单层聚焦、视角复位、全屏和 2D 降级。
- 仪器状态、维修状态、待执行预约数量和最近预约时间已与空间节点联动。
- 2026-08-27 将 3D 页面调整为“A 空间工作台”布局：顶部控制卡片、左侧楼层与节点索引、中间场景、右侧上下文详情相互独立；2D 与 3D 共用导航和详情结构。
- 已修复全屏透明区域出现黑底、主题文字对比度不足以及深色主题下浅色场景突兀的问题；个人高级、科技专业主题使用深色 Three.js 场景材质。
- 最近一次前端 `npm run build` 通过，桌面端、移动端、2D/3D 切换和全屏主题已完成浏览器检查，未发现控制台错误。
- 新增独立迁移 `database/15_add_lab_visualization.oracle.sql`，补齐三张 3D 表、注释、唯一索引，以及场景到实验室/文件、节点到场景、绑定到节点四条外键。
- 新增“3D 场景管理”动态菜单，可维护场景、启停状态、背景色、节点坐标、缩放、排序及实验室/位置/仪器绑定。
- GLB 上传限制为管理员权限和场景归属，模型下载二次校验业务归属；支持历史模型列表、自动版本递增和历史版本重新启用。
- `lab:3d:manage` 仅授予 `admin`、`lab_admin`；`lab_user` 只保留 `lab:3d:view`。
- 修复首页趋势日期表达式在 Oracle 上触发 `ORA-03001` 的问题，真实接口复验由 500 恢复为 200。
- 2026-08-31 后端 11/11 自动化测试通过，解决方案构建 0 警告 0 错误，前端类型检查及生产构建通过。
- 浏览器验证 3D 画布 `724×566` 非空、2D/3D 切换正常、控制台无错误；场景管理接口、动态菜单和首页接口均在本地 Oracle 环境返回 200。

### 部署时执行

1. 在目标 Oracle 依次执行增量脚本至 `15_add_lab_visualization.oracle.sql`，再启动 API 幂等初始化菜单和角色授权。
2. 使用目标环境自己的 JWT 密钥、数据库连接串和附件目录，不复制本地配置。
3. 上传经过压缩的真实 GLB 素材，按目标终端补做模型大小、首屏时间和帧率基线。
4. 按 `docs/PRODUCTION_CHECKLIST.md` 配置数据库与附件备份、日志采集保留和回滚演练。

### 阶段 6 关键代码

```text
HxsAiSystem.Application/LabOperations
HxsAiSystem.Application/LabVisualization
HxsAiSystem.Domain/Entities/Lab3dEntities.cs
HxsAiSystem.WebApiHost/Controllers/LabOperationsController.cs
HxsAiSystem.WebApiHost/Controllers/LabVisualizationController.cs
hxs-aisystem-web/src/views/lab/ApprovalCenterView.vue
hxs-aisystem-web/src/views/lab/Lab3dView.vue
hxs-aisystem-web/src/views/lab/Lab3dManageView.vue
database/15_add_lab_visualization.oracle.sql
```

当前 3D 查询接口：

```http
GET /api/lab/3d/scenes
GET /api/lab/3d/scenes/{id}
GET /api/lab/3d/scenes/{id}/statuses
GET /api/lab/3d/layout
GET /api/lab/3d/layout/{labId}/statuses
GET /api/lab/3d/manage/scenes
POST /api/lab/3d/manage/scenes
PUT /api/lab/3d/manage/scenes/{id}
DELETE /api/lab/3d/manage/scenes/{id}
POST /api/lab/3d/manage/scenes/{id}/model
GET /api/lab/3d/manage/scenes/{id}/models
PUT /api/lab/3d/manage/scenes/{id}/models/{fileId}/activate
POST /api/lab/3d/manage/scenes/{id}/nodes
PUT /api/lab/3d/manage/nodes/{id}
DELETE /api/lab/3d/manage/nodes/{id}
PUT /api/lab/3d/manage/nodes/{id}/binding
DELETE /api/lab/3d/manage/nodes/{id}/binding
```

## 阶段 5：实验任务与电子实验记录（2026-08-17，开发及运行态验收完成）

- 新增实验任务、仪器预约关联、物资领用关联、过程记录四张业务表，附件复用 `HXS_SYS_FILE`。
- 新增“实验任务”菜单分组，包含“我的实验”和“实验任务查询”两个动态菜单页面。
- 实现草稿、进行中、已完成、已归档状态机，以及开始、完成、退回、归档和仅系统管理员解档。
- 实验任务可关联负责人本人已通过的仪器预约和领用申请，并校验仪器、物资及批准数量一致性。
- 实现过程、结果和原始数据说明记录；已完成禁止新增普通过程记录，归档后接口和页面只读。
- 实验附件限制扩展名、MIME、单文件及单实验总大小，使用随机存储名，并在上传下载时校验实验归属和数据范围。
- 真实 Oracle 已完成关联创建、状态流转、记录、附件、归档只读、角色越权和管理员解档验收。
- 阶段 5 验收实验编号前缀为 `EX`，实验名称为“阶段5电子实验记录验收”，便于后续识别。

## 阶段 4：试剂耗材库存闭环（2026-08-17，完整验收通过）

- 新增物资档案、批次库存、库存流水、领用申请/审批和库存预警模块。
- 新增 `HXS_LAB_MATERIAL`、`HXS_LAB_STOCK_BATCH`、`HXS_LAB_STOCK_FLOW`、`HXS_LAB_REQUISITION`、`HXS_LAB_REQUISITION_ITEM`。
- 领用审批在事务内锁定申请和可用批次，按有效期优先扣减，禁止负库存与重复审批。
- 新增 `database/13_add_lab_inventory.oracle.sql`，应用启动时由 `LabInventoryInitializer` 幂等初始化表、菜单、权限和演示数据。
- 前后端构建通过，后端测试 5/5，真实 Oracle 运行态完整验收通过。
- 已验证入库、库存调整、负库存拦截、申请、取消、批准数量调整、通过、驳回、库存不足回滚和批次出库流水。
- 已验证顺序及并发重复审批均不会重复扣库；并发双审批结果为一次成功、一次业务拒绝，且只产生一条出库流水。
- 已验证普通实验员本人数据范围及档案维护、库存调整、审批接口的越权拦截，相关请求返回 403。
- 已验证低库存、临期、过期三类预警，过期批次禁止领用，入库、调整、申请和审批操作均写入审计日志。
- 阶段 4 验收数据使用 `ACC-*` 物资/批次编码和“阶段4验收”用途，便于后续识别或清理。

## 阶段 3：仪器设备业务闭环（2026-08-16）

- 新增仪器台账、仪器预约、预约审批、使用登记、设备报修及维修处理模块。
- 新增 `HXS_LAB_INSTRUMENT`、`HXS_LAB_BOOKING`、`HXS_LAB_USAGE`、`HXS_LAB_REPAIR` 四张表，启动时幂等初始化。
- 预约提交与审批均校验时间冲突，采用 `existing.START_TIME < request.END_TIME AND existing.END_TIME > request.START_TIME` 规则。
- 已实现预约和报修状态机、本人数据范围、重复审批拦截、报修后进入维修状态及维修完成恢复正常。
- 前端新增仪器台账、仪器预约、预约审批、使用记录、设备报修五个独立菜单页面。
- 初始化两台演示仪器，管理员角色自动获得阶段 3 菜单与操作权限。
- 后端和前端生产构建均已通过，Oracle 启动初始化、菜单权限和阶段 3 业务表已完成运行态验证。
- 2026-08-16 已连接 Oracle 完成阶段 3 全流程验收：预约、审批、冲突、取消、使用、报修、维修、越权、数据范围和审计均通过。
- 验收账号：`lab_admin_accept`（实验管理员）、`lab_user_accept`（普通实验员）；初始密码均为 `Lab@123456`，正式环境使用前应修改或删除。

## 1. 项目概况

当前项目由两个并列工程组成：

```text
/Users/huanxishas/MyAIProject/
  hxs-aisystem-api   后台 API，.NET 9 Web API
  hxs-aisystem-web   前台管理端，Vue 3 + Vite + TypeScript
```

后台已经完成登录、JWT 签发、系统管理基础模型和业务接口。前台已经完成登录页、首页、系统管理页面、动态菜单、主题系统和本地状态管理。

## 2. 后台工程

后台路径：

```text
/Users/huanxishas/MyAIProject/hxs-aisystem-api
```

技术栈：

- .NET 9
- ASP.NET Core Web API
- SqlSugar
- Oracle
- Serilog
- Swagger
- JWT Token 签发

分层结构：

```text
HxsAiSystem.Domain          领域实体
HxsAiSystem.Infrastructure  基础设施配置
HxsAiSystem.Persistence     数据库连接和持久化注册
HxsAiSystem.Application     应用服务、认证、系统管理业务逻辑
HxsAiSystem.WebApiHost      Web API 启动和 Controller
database                    Oracle 初始化和迁移脚本
```

## 3. 数据库

数据库用户：

```text
HXS_AISYSTEM
```

当前连接配置在：

```text
HxsAiSystem.WebApiHost/appsettings.json
```

当前主要表：

```text
HXS_SYS_ORG          组织架构表
HXS_SYS_USER         系统用户表
HXS_SYS_ROLE         系统角色表
HXS_SYS_MENU         系统菜单和权限点表
HXS_SYS_USER_ROLE    用户角色关联表
HXS_SYS_ROLE_MENU    角色菜单关联表
HXS_AI_CONVERSATION  AI 会话表，暂未实现业务接口
HXS_AI_MESSAGE       AI 消息表，暂未实现业务接口
```

数据库脚本：

```text
database/00_create_schema.oracle.sql          创建 Oracle 用户/Schema
database/01_init_schema.oracle.sql            初始化基础表
database/02_add_login.oracle.sql              登录用户表旧版迁移
database/03_reset_admin_password.oracle.sql   重置 admin 密码
database/04_fix_chinese_comments.oracle.sql   修复中文显示名和字段注释
database/05_add_system_management.oracle.sql  系统管理模型建表和默认数据
database/06_add_dashboard_menu.oracle.sql     将首页加入菜单表并授权给 admin
```

通过 `sqlplus` 执行中文 SQL 时需要设置：

```bash
export NLS_LANG=AMERICAN_AMERICA.AL32UTF8
```

默认账号：

```text
用户名：admin
密码：Admin@123456
```

密码只保存 PBKDF2 哈希，不保存明文密码。忘记密码时执行 `database/03_reset_admin_password.oracle.sql` 重置。

## 4. 后台已实现功能

基础功能：

- `GET /health` 健康检查。
- Swagger 开发环境接口文档。
- Serilog 请求日志。
- Oracle 数据库连接。

认证功能：

- `POST /api/auth/login` 用户名密码登录。
- 登录成功返回 Bearer Token、过期时间和用户信息。
- 登录成功更新 `HXS_SYS_USER.LAST_LOGIN_TIME`。
- `GET /api/auth/menus` 根据当前用户角色返回可见菜单树。

系统管理功能：

- 组织管理：列表、树、创建、修改、删除。
- 用户管理：列表、创建、修改、删除。
- 用户角色：查询用户角色、分配用户角色。
- 角色管理：列表、创建、修改、删除。
- 角色菜单：查询角色菜单、分配角色菜单。
- 菜单管理：列表、树、创建、修改、删除。

系统管理接口当前通过轻量过滤器 `RequireLoginFilter` 校验 Bearer Token。后续建议正式接入 ASP.NET Core `JwtBearer` 中间件和权限码校验。

## 5. 后台接口清单

认证：

```http
POST /api/auth/login
GET  /api/auth/menus
```

组织：

```http
GET    /api/system/orgs
GET    /api/system/orgs/tree
POST   /api/system/orgs
PUT    /api/system/orgs/{id}
DELETE /api/system/orgs/{id}
```

用户：

```http
GET    /api/system/users?keyword=
POST   /api/system/users
PUT    /api/system/users/{id}
DELETE /api/system/users/{id}
GET    /api/system/users/{id}/roles
PUT    /api/system/users/{id}/roles
```

角色：

```http
GET    /api/system/roles
POST   /api/system/roles
PUT    /api/system/roles/{id}
DELETE /api/system/roles/{id}
GET    /api/system/roles/{id}/menus
PUT    /api/system/roles/{id}/menus
```

菜单：

```http
GET    /api/system/menus
GET    /api/system/menus/tree
POST   /api/system/menus
PUT    /api/system/menus/{id}
DELETE /api/system/menus/{id}
```

## 6. 后台关键代码

认证：

```text
HxsAiSystem.Application/Auth/AuthService.cs
HxsAiSystem.Application/Auth/PasswordHasher.cs
HxsAiSystem.Application/Auth/CurrentUserService.cs
HxsAiSystem.WebApiHost/Controllers/AuthController.cs
```

系统管理：

```text
HxsAiSystem.Application/SystemManagement/SystemManagementService.cs
HxsAiSystem.Application/SystemManagement/SystemManagementDtos.cs
HxsAiSystem.Application/SystemManagement/ISystemManagementService.cs
HxsAiSystem.WebApiHost/Controllers/SystemManagement/
```

领域实体：

```text
HxsAiSystem.Domain/Entities/AppUser.cs
HxsAiSystem.Domain/Entities/SysOrg.cs
HxsAiSystem.Domain/Entities/SysRole.cs
HxsAiSystem.Domain/Entities/SysMenu.cs
HxsAiSystem.Domain/Entities/SysUserRole.cs
HxsAiSystem.Domain/Entities/SysRoleMenu.cs
```

Oracle 的 `RAW(16)` 当前在实体里使用 `byte[]` 映射，接口层通过 `RawGuidConverter` 转成 `Guid` 返回给前端。

## 7. 前台工程

前台路径：

```text
/Users/huanxishas/MyAIProject/hxs-aisystem-web
```

技术栈：

- Vue 3
- Vite 4
- TypeScript
- Ant Design Vue
- Pinia
- Vue Router
- Axios
- Dayjs
- unplugin-auto-import
- unplugin-vue-components

选择 Vite 4 是为了兼容当前本机 Node 16。

## 8. 前台已实现功能

登录：

- 登录页。
- 默认填充 `admin / Admin@123456`。
- 登录成功保存 token 和用户信息。
- 未登录访问业务页面自动跳转登录页。

首页：

- 首页总览。
- API 状态展示。
- 组织、用户、角色、菜单数量。
- 权限链路展示。
- 主题风格切换。

系统管理：

- 组织架构页面。
- 用户管理页面。
- 用户分配角色弹窗。
- 角色管理页面。
- 角色菜单授权弹窗。
- 菜单管理页面。

菜单：

- 左侧菜单来自后台 `GET /api/auth/menus`。
- 首页已经写入 `HXS_SYS_MENU`，不是前端硬编码。
- 菜单根据用户角色授权动态展示。

主题：

- Pinia 管理主题状态。
- localStorage 持久化主题配置。
- 支持四套预设：
  - 商务简洁
  - 个人高级
  - 科技专业
  - 经典明亮
- 支持细化配置：
  - 主色
  - 强调色
  - 按钮颜色
  - 头部背景
  - 头部文字
  - 头部透明度
  - 整体圆角
  - 按钮圆角
  - 卡片透明度
  - 字体
  - 系统背景图上传

界面：

- 左侧菜单为圆角卡片式，可折叠。
- 页面头部为圆角卡片工具栏。
- 表格、卡片、弹窗、输入框、按钮统一圆角风格。
- 背景图支持本地上传，保存到 localStorage。

## 9. 前台关键代码

入口和构建：

```text
package.json
vite.config.ts
src/main.ts
src/App.vue
```

状态：

```text
src/stores/auth.ts
src/stores/theme.ts
```

路由：

```text
src/router/index.ts
```

接口：

```text
src/api/http.ts
src/api/system.ts
```

布局和样式：

```text
src/layouts/AppLayout.vue
src/styles/app.css
src/styles/theme.css
```

页面：

```text
src/views/auth/LoginView.vue
src/views/dashboard/DashboardView.vue
src/views/system/OrgView.vue
src/views/system/UserView.vue
src/views/system/RoleView.vue
src/views/system/MenuView.vue
```

## 10. 运行方式

启动后台：

```bash
cd /Users/huanxishas/MyAIProject/hxs-aisystem-api
dotnet run --project HxsAiSystem.WebApiHost
```

默认前端代理后台地址：

```text
http://127.0.0.1:5120
```

配置位置：

```text
/Users/huanxishas/MyAIProject/hxs-aisystem-web/vite.config.ts
```

启动前台：

```bash
cd /Users/huanxishas/MyAIProject/hxs-aisystem-web
npm install
npm run dev
```

构建前台：

```bash
cd /Users/huanxishas/MyAIProject/hxs-aisystem-web
npm run build
```

前台最近一次验证构建已通过。

## 11. 已验证事项

后台：

```text
dotnet build HxsAiSystem.sln：成功
登录接口：成功
GET /api/auth/menus：成功返回菜单树
系统管理接口未登录：401
系统管理接口登录后：200
```

数据库：

```text
admin 用户已创建并启用
admin 已绑定系统管理员角色
admin 已授权首页和系统管理菜单
首页菜单 dashboard 已写入 HXS_SYS_MENU
```

前台：

```text
npm install：成功
npm run build：成功
Vite dev server：可启动
```

## 12. 当前注意事项

1. 后台已接入正式 `JwtBearer` 和动态 `PERMISSION_CODE` 权限策略；阶段 6 仍需对 3D 数据范围和模型文件下载做专项越权验证。
2. 当前核心业务已经采用角色权限和 `All/Laboratory/Self` 数据范围，新增阶段 6 管理接口时必须继续执行相同规则。
3. 前端主题配置保存在 localStorage，换浏览器或清缓存会丢失。后续可做用户主题偏好表或用户配置接口。
4. 背景图当前保存为 base64 到 localStorage，适合本地偏好；大图可能占用浏览器存储。后续建议上传到后端文件服务。
5. `HXS_AI_CONVERSATION` 和 `HXS_AI_MESSAGE` 已实现数据推理会话接口和用户隔离。
6. 前端使用 Ant Design Vue，`antd` 构建 chunk 仍较大；当前已做分包和自动导入，阶段 6 性能验收时需继续观察首屏体积。
7. Oracle `RAW(16)` 与 C# `Guid` 映射采用实体 `byte[]`、接口层转换为 `Guid` 的统一方式。
8. 3D 表当前由应用启动初始化器创建，最终交付前必须补独立增量脚本并验证空库、升级库重复执行。
9. 当前 3D 主场景主要为 Three.js 程序化空间；真实 GLB 上传、版本、节点绑定和管理接口仍未闭环。

## 阶段 0、1 实施补充（2026-08-14）

- 正式接入 `JwtBearer`，不再依赖手动解析 Token 的登录过滤器。
- 动态权限策略前缀为 `Permission:`，特性为 `PermissionAuthorizeAttribute`。
- 权限由用户角色、角色菜单及 `HXS_SYS_MENU.PERMISSION_CODE` 动态计算；`admin` 角色拥有全部权限。
- 新增 `GET /api/auth/permissions`，供前端控制按钮显示。
- 新增 `IDataScopeService`，数据范围分为 `All`、`Laboratory`、`Self`。
- 新增登录失败计数和临时锁定字段。
- 新增 `HXS_SYS_AUDIT_LOG`、审计过滤器及 `GET /api/system/audit-logs`。
- 新增 `HXS_SYS_FILE` 与 `/api/files` 上传、下载基础接口。
- 新增统一异常中间件和分页模型。
- 本地敏感配置使用 `HxsAiSystem.WebApiHost/appsettings.Local.json`，该文件已加入忽略规则。
- 阶段1数据库脚本：`database/07_harden_auth_audit.oracle.sql`。
- 应用启动会通过 `SystemFoundationInitializer` 幂等补齐阶段1结构、角色和权限。
- 测试项目：`HxsAiSystem.Tests`。

## 13. 建议后续迭代

优先级较高：

- 完成 GLB 模型上传、场景版本、节点绑定和模型文件权限闭环。
- 为 3D 数据表补充独立 Oracle 增量脚本，并同步数据库注释和 Navicat 模型。
- 补齐阶段 6 数据范围、性能、资源释放、并发和安全自动化测试。
- 完成一期端到端验收与生产配置、备份恢复、日志清理方案。

前端体验：

- 主题配置保存到后台用户偏好。
- 增加页面级加载骨架和空状态。
- 增加表单校验规则和重复编码友好提示。

数据库：

- 增加审计字段：`CREATE_BY`、`UPDATE_BY`。
- 增加软删除字段：`IS_DELETED`。
- 增加数据权限模型，例如按组织范围授权。
- 增加用户多组织/多部门关系表，支持兼职部门。
# 阶段 2 补充（2026-08-15）

- 新增 `LabFoundation` 应用模块和 `/api/lab/foundation/*` 接口。
- 新增实验室、位置、课题组成员、供应商和系统字典共 7 张表。
- 启动时自动补齐结构、六类内置字典、“实验室管理 / 基础数据”菜单及管理员权限。
- 前端按 `/lab/labs`、`/lab/locations`、`/lab/groups`、`/lab/suppliers`、`/lab/dictionaries` 拆分为五个独立菜单，页面复用统一维护组件。
- 启动初始化包含 2 个实验室、5 个位置节点、2 个课题组、1 个成员和 3 个供应商的幂等演示数据。
- 数据库增量脚本：`database/08_add_lab_foundation.oracle.sql`。
