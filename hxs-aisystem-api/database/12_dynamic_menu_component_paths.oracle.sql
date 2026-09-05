-- 菜单完全动态路由：COMPONENT 保存前端 src 下真实 Vue 文件位置。
-- 使用 HXS_AISYSTEM 用户执行，可重复执行。
MERGE INTO HXS_SYS_MENU target
USING (
    SELECT 'dashboard' MENU_CODE, 'views/dashboard/DashboardView.vue' COMPONENT FROM DUAL UNION ALL
    SELECT 'ai:reasoning', 'views/ai/ReasoningView.vue' FROM DUAL UNION ALL
    SELECT 'sys:user', 'views/system/UserView.vue' FROM DUAL UNION ALL
    SELECT 'sys:role', 'views/system/RoleView.vue' FROM DUAL UNION ALL
    SELECT 'sys:menu', 'views/system/MenuView.vue' FROM DUAL UNION ALL
    SELECT 'sys:org', 'views/system/OrgView.vue' FROM DUAL UNION ALL
    SELECT 'sys:audit', 'views/system/AuditLogView.vue' FROM DUAL UNION ALL
    SELECT 'lab:labs', 'views/lab/LabView.vue' FROM DUAL UNION ALL
    SELECT 'lab:locations', 'views/lab/LocationView.vue' FROM DUAL UNION ALL
    SELECT 'lab:groups', 'views/lab/GroupView.vue' FROM DUAL UNION ALL
    SELECT 'lab:suppliers', 'views/lab/SupplierView.vue' FROM DUAL UNION ALL
    SELECT 'lab:dictionaries', 'views/lab/DictionaryView.vue' FROM DUAL UNION ALL
    SELECT 'lab:instruments', 'views/lab/InstrumentView.vue' FROM DUAL UNION ALL
    SELECT 'lab:bookings', 'views/lab/BookingView.vue' FROM DUAL UNION ALL
    SELECT 'lab:booking-approvals', 'views/lab/BookingApprovalView.vue' FROM DUAL UNION ALL
    SELECT 'lab:usages', 'views/lab/UsageView.vue' FROM DUAL UNION ALL
    SELECT 'lab:repairs', 'views/lab/RepairView.vue' FROM DUAL
) source
ON (target.MENU_CODE = source.MENU_CODE AND target.MENU_TYPE = 'page')
WHEN MATCHED THEN UPDATE SET
    target.COMPONENT = source.COMPONENT,
    target.UPDATE_TIME = SYSTIMESTAMP;

COMMIT;

SELECT MENU_CODE, ROUTE_PATH, COMPONENT
FROM HXS_SYS_MENU
WHERE MENU_TYPE = 'page'
ORDER BY SORT_NO, MENU_CODE;
