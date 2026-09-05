# 一期生产上线清单

## 1. 发布前

- 使用 `ASPNETCORE_ENVIRONMENT=Production`，通过环境变量或密钥服务提供 `Jwt__SecretKey` 和 `Database__ConnectionString`。
- JWT 密钥至少 32 字节且不得复用开发值；生产配置文件不得保存明文口令。
- 设置持久化 `FileStorage__RootPath`，确认运行账号仅拥有所需目录读写权限。
- 依次执行 `database/07_harden_auth_audit.oracle.sql` 至 `database/15_add_lab_visualization.oracle.sql`，所有脚本均应以业务 Schema 用户运行。
- 先备份数据库 Schema 和附件目录，再启动新版本 API；应用初始化器会幂等补齐菜单、角色授权和基础数据。

## 2. 启动检查

- `GET /health` 返回 200；生产环境不提供 Swagger 页面。
- 使用系统管理员、实验管理员和普通实验员各登录一次，确认菜单分别符合角色权限。
- 验证预约提交与审批、领用审批扣库、实验记录与附件、统一审批中心和首页统计。
- 验证“空间可视化 / 3D 实验室”及“3D 场景管理”，上传一个压缩后的 GLB，并验证历史版本切换和下载。
- 检查 API 日志无 500、`ORA-*`、未授权文件访问和动态组件找不到错误。

## 3. 备份与恢复

- 使用 Oracle Data Pump 定时导出 `HXS_AISYSTEM` Schema，备份任务必须有失败告警和保留周期。
- 附件目录与数据库备份采用同一时间窗口，并保留文件清单和校验值。
- 恢复演练顺序：恢复 Schema、恢复附件目录、校验附件记录路径、启动 API、执行冒烟流程。
- 发布回滚时同时回滚应用版本；迁移新增列和表保持向后兼容，不直接删除生产数据。

## 4. 运行维护

- 将 Serilog 控制台日志接入目标环境日志平台，配置访问日志、错误日志和审计日志的保留与清理策略。
- 监控 API 5xx、登录失败、Oracle 连接、附件磁盘空间、首页统计耗时和 GLB 下载耗时。
- GLB 建议使用 Draco/Meshopt 等方式离线压缩；目标终端实测首屏不超过 5 秒，桌面端目标 50 FPS 以上。
- 定期复核 `lab:3d:view`、`lab:3d:manage` 及实验室业务角色授权，离职或角色变更时及时回收权限。
