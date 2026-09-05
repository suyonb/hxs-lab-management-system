# HXS 实验室管理系统

HXS 实验室管理系统是一套前后端分离的实验室业务管理平台，覆盖基础资料、仪器预约与维修、试剂耗材库存、实验记录、统一审批、数据推理和实验室 2D/3D 空间可视化。

## 项目结构

```text
hxs-aisystem-api/  .NET 9 Web API、Oracle 脚本及项目文档
hxs-aisystem-web/  Vue 3、TypeScript、Vite 管理端
```

## 已实现功能

- JWT 登录、动态菜单与按钮权限、角色数据范围和操作审计
- 实验室、位置、课题组、供应商和数据字典维护
- 仪器台账、预约审批、使用登记、报修和维修闭环
- 试剂耗材、批次库存、领用审批、库存流水和预警闭环
- 实验任务、过程记录、附件、完成和归档闭环
- 统一审批中心、首页统计、Excel 导出和 AI 数据推理会话
- 实验室 2D/3D 空间展示、业务状态联动和 GLB 场景管理

## 本地启动

后端配置说明和 Oracle 初始化步骤见 [`hxs-aisystem-api/README.md`](hxs-aisystem-api/README.md)。本地敏感配置应写入：

```text
hxs-aisystem-api/HxsAiSystem.WebApiHost/appsettings.Local.json
```

该文件不会提交到 Git。后端启动命令：

```bash
cd hxs-aisystem-api
dotnet restore
dotnet run --project HxsAiSystem.WebApiHost
```

前端启动命令：

```bash
cd hxs-aisystem-web
npm install
npm run dev
```

默认情况下，前端开发服务器将 `/api` 和 `/health` 代理到 `http://127.0.0.1:5120`。可通过 `VITE_API_TARGET` 调整目标地址。

## 在线演示

前端内置不依赖 API 和 Oracle 的演示模式，包含实验室、仪器、预约审批、库存、实验记录、数据推理和 2D/3D 空间数据。演示操作保存在当前浏览器，可在系统头部恢复初始数据。

```bash
cd hxs-aisystem-web
npm run dev:demo
```

仓库的 `docs` 目录保存在线演示构建产物。首次使用时，需要进入仓库的 `Settings > Pages`，选择 `Deploy from a branch`，分支设置为 `main`，目录设置为 `/docs`。后续更新演示版时执行 `npm run build:demo`，并将 `dist` 内容同步到根目录 `docs`。

## 文档

- 实施与交接状态：[`hxs-aisystem-api/HANDOFF.md`](hxs-aisystem-api/HANDOFF.md)
- 分阶段开发需求：[`hxs-aisystem-api/shiyanshi.md`](hxs-aisystem-api/shiyanshi.md)
- 数据模型：[`hxs-aisystem-api/docs/DATABASE_MODEL.md`](hxs-aisystem-api/docs/DATABASE_MODEL.md)
- 生产上线清单：[`hxs-aisystem-api/docs/PRODUCTION_CHECKLIST.md`](hxs-aisystem-api/docs/PRODUCTION_CHECKLIST.md)

## 安全说明

- 不要提交真实数据库连接串、JWT 密钥、生产账号或附件数据。
- 生产环境通过环境变量或密钥服务提供敏感配置。
- 正式部署前应修改初始化账号密码，并按生产上线清单完成备份恢复与权限复核。
