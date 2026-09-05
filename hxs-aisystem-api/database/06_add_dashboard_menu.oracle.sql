-- 使用 HXS_AISYSTEM 用户连接后执行。可重复执行。
-- 如果通过 sqlplus 执行，请先确保客户端字符集为 UTF-8：
-- export NLS_LANG=AMERICAN_AMERICA.AL32UTF8

MERGE INTO HXS_SYS_MENU target
USING (
    SELECT
        'dashboard' MENU_CODE,
        '首页总览' MENU_NAME,
        'page' MENU_TYPE,
        '/' ROUTE_PATH,
        'dashboard/index' COMPONENT,
        'dashboard' ICON,
        'dashboard:view' PERMISSION_CODE,
        1 SORT_NO
    FROM DUAL
) source
ON (target.MENU_CODE = source.MENU_CODE)
WHEN MATCHED THEN UPDATE SET
    target.PARENT_ID = NULL,
    target.MENU_NAME = source.MENU_NAME,
    target.MENU_TYPE = source.MENU_TYPE,
    target.ROUTE_PATH = source.ROUTE_PATH,
    target.COMPONENT = source.COMPONENT,
    target.ICON = source.ICON,
    target.PERMISSION_CODE = source.PERMISSION_CODE,
    target.SORT_NO = source.SORT_NO,
    target.IS_VISIBLE = 1,
    target.IS_ACTIVE = 1,
    target.UPDATE_TIME = SYSTIMESTAMP
WHEN NOT MATCHED THEN INSERT (
    ID, PARENT_ID, MENU_CODE, MENU_NAME, MENU_TYPE, ROUTE_PATH, COMPONENT, ICON,
    PERMISSION_CODE, SORT_NO, IS_VISIBLE, IS_ACTIVE, CREATE_TIME, UPDATE_TIME
) VALUES (
    SYS_GUID(), NULL, source.MENU_CODE, source.MENU_NAME, source.MENU_TYPE, source.ROUTE_PATH,
    source.COMPONENT, source.ICON, source.PERMISSION_CODE, source.SORT_NO, 1, 1, SYSTIMESTAMP, SYSTIMESTAMP
);

MERGE INTO HXS_SYS_ROLE_MENU target
USING (
    SELECT r.ID ROLE_ID, m.ID MENU_ID
    FROM HXS_SYS_ROLE r
    CROSS JOIN HXS_SYS_MENU m
    WHERE r.ROLE_CODE = 'admin' AND m.MENU_CODE = 'dashboard'
) source
ON (target.ROLE_ID = source.ROLE_ID AND target.MENU_ID = source.MENU_ID)
WHEN NOT MATCHED THEN INSERT (ID, ROLE_ID, MENU_ID, CREATE_TIME)
VALUES (SYS_GUID(), source.ROLE_ID, source.MENU_ID, SYSTIMESTAMP);

COMMIT;
