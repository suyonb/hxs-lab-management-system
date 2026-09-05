# hxs-aisystem-api

与现有项目一致的分层 API 基础工程：`Domain`、`Infrastructure`、`Persistence`、`Application`、`WebApiHost`。

## 本地 Oracle 初始化

1. 以 Navicat 的 `SYSTEM` 等管理员账号执行 `database/00_create_schema.oracle.sql`，先替换脚本中的密码。
2. 用新用户 `HXS_AISYSTEM` 连接本地 Oracle（Oracle Free 默认服务名通常为 `FREEPDB1`）。
3. 执行 `database/01_init_schema.oracle.sql`。
4. 修改 `HxsAiSystem.WebApiHost/appsettings.json` 的连接字符串；真实密码建议通过 User Secrets 或环境变量管理。

## 运行

```bash
dotnet restore
dotnet run --project HxsAiSystem.WebApiHost
```

开发环境可访问 `/swagger`，健康检查为 `/health`。

## 登录

初始化脚本会创建默认管理员账号：

- 用户名：`admin`
- 密码：`Admin@123456`

登录接口：

```http
POST /api/auth/login
Content-Type: application/json

{
  "userName": "admin",
  "password": "Admin@123456"
}
```

接口会返回 Bearer Token、过期时间和当前用户信息。生产环境请修改 `Jwt:SecretKey`，并替换默认管理员密码。

## 系统管理模型

系统管理模块包含组织、用户、角色、菜单和授权关系：

- `HXS_SYS_ORG`：组织架构，公司/部门/小组树。
- `HXS_SYS_USER`：系统用户，用户通过 `ORG_ID` 归属到组织/部门。
- `HXS_SYS_ROLE`：系统角色。
- `HXS_SYS_MENU`：系统菜单和权限点，支持菜单树。
- `HXS_SYS_USER_ROLE`：用户角色关联。
- `HXS_SYS_ROLE_MENU`：角色菜单关联。

现有库可执行 `database/05_add_system_management.oracle.sql` 落地这套模型。通过 `sqlplus` 执行中文脚本时建议设置：

```bash
export NLS_LANG=AMERICAN_AMERICA.AL32UTF8
```

系统管理接口：

- `GET /api/system/orgs`：组织列表。
- `GET /api/system/orgs/tree`：组织树。
- `POST /api/system/orgs`：创建组织。
- `PUT /api/system/orgs/{id}`：修改组织。
- `DELETE /api/system/orgs/{id}`：删除组织。
- `GET /api/system/users`：用户列表，支持 `keyword` 查询。
- `POST /api/system/users`：创建用户。
- `PUT /api/system/users/{id}`：修改用户。
- `DELETE /api/system/users/{id}`：删除用户。
- `GET /api/system/users/{id}/roles`：查询用户角色。
- `PUT /api/system/users/{id}/roles`：分配用户角色。
- `GET /api/system/roles`：角色列表。
- `POST /api/system/roles`：创建角色。
- `PUT /api/system/roles/{id}`：修改角色。
- `DELETE /api/system/roles/{id}`：删除角色。
- `GET /api/system/roles/{id}/menus`：查询角色菜单。
- `PUT /api/system/roles/{id}/menus`：分配角色菜单。
- `GET /api/system/menus`：菜单列表。
- `GET /api/system/menus/tree`：菜单树。
- `POST /api/system/menus`：创建菜单。
- `PUT /api/system/menus/{id}`：修改菜单。
- `DELETE /api/system/menus/{id}`：删除菜单。
- `GET /api/auth/menus`：根据当前登录用户的角色返回可见菜单树，需要传入登录接口返回的 Bearer Token。
