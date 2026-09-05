-- 补齐当前用户下所有 HXS_ 表及字段注释。可重复执行，不覆盖已有注释。
DECLARE
    FUNCTION quote_text(value VARCHAR2) RETURN VARCHAR2 IS
    BEGIN
        RETURN REPLACE(value, '''', '''''');
    END;

    FUNCTION table_label(name VARCHAR2) RETURN VARCHAR2 IS
    BEGIN
        RETURN CASE name
            WHEN 'HXS_APP_USER' THEN '旧版系统用户表'
            WHEN 'HXS_AI_CONVERSATION' THEN 'AI推理会话表'
            WHEN 'HXS_AI_MESSAGE' THEN 'AI推理消息表'
            WHEN 'HXS_SYS_ORG' THEN '系统组织架构表'
            WHEN 'HXS_SYS_USER' THEN '系统用户表'
            WHEN 'HXS_SYS_ROLE' THEN '系统角色表'
            WHEN 'HXS_SYS_MENU' THEN '系统菜单和操作权限表'
            WHEN 'HXS_SYS_USER_ROLE' THEN '用户角色关联表'
            WHEN 'HXS_SYS_ROLE_MENU' THEN '角色菜单权限关联表'
            WHEN 'HXS_SYS_AUDIT_LOG' THEN '系统操作审计日志表'
            WHEN 'HXS_SYS_FILE' THEN '系统文件元数据表'
            WHEN 'HXS_SYS_DICT_TYPE' THEN '业务字典类型表'
            WHEN 'HXS_SYS_DICT_ITEM' THEN '业务字典项表'
            WHEN 'HXS_LAB' THEN '实验室基础档案表'
            WHEN 'HXS_LAB_LOCATION' THEN '实验室位置节点表'
            WHEN 'HXS_LAB_GROUP' THEN '实验室课题组表'
            WHEN 'HXS_LAB_GROUP_MEMBER' THEN '课题组成员关联表'
            WHEN 'HXS_LAB_SUPPLIER' THEN '实验室供应商档案表'
            WHEN 'HXS_LAB_INSTRUMENT' THEN '实验室仪器设备台账表'
            WHEN 'HXS_LAB_BOOKING' THEN '仪器预约申请与审批表'
            WHEN 'HXS_LAB_USAGE' THEN '仪器实际使用记录表'
            WHEN 'HXS_LAB_REPAIR' THEN '仪器故障报修与维修记录表'
            ELSE name || '业务数据表'
        END;
    END;

    FUNCTION column_label(name VARCHAR2) RETURN VARCHAR2 IS
    BEGIN
        RETURN CASE name
            WHEN 'ID' THEN '主键ID，RAW(16)'
            WHEN 'PARENT_ID' THEN '上级记录ID'
            WHEN 'USER_ID' THEN '系统用户ID'
            WHEN 'ORG_ID' THEN '所属组织ID'
            WHEN 'ROLE_ID' THEN '角色ID'
            WHEN 'MENU_ID' THEN '菜单或权限ID'
            WHEN 'LAB_ID' THEN '所属实验室ID'
            WHEN 'GROUP_ID' THEN '课题组ID'
            WHEN 'INSTRUMENT_ID' THEN '仪器设备ID'
            WHEN 'BOOKING_ID' THEN '关联预约ID'
            WHEN 'CREATE_TIME' THEN '创建时间'
            WHEN 'UPDATE_TIME' THEN '更新时间'
            WHEN 'IS_ACTIVE' THEN '是否启用：1启用，0停用'
            WHEN 'STATUS' THEN '业务状态'
            WHEN 'DESCRIPTION' THEN '说明或描述'
            WHEN 'REMARK' THEN '备注'
            WHEN 'START_TIME' THEN '开始时间'
            WHEN 'END_TIME' THEN '结束时间'
            ELSE name || '字段'
        END;
    END;
BEGIN
    FOR item IN (
        SELECT table_name
        FROM user_tab_comments
        WHERE SUBSTR(table_name, 1, 4) = 'HXS_' AND comments IS NULL
    ) LOOP
        EXECUTE IMMEDIATE 'COMMENT ON TABLE ' || item.table_name ||
            ' IS ''' || quote_text(table_label(item.table_name)) || '''';
    END LOOP;

    FOR item IN (
        SELECT table_name, column_name
        FROM user_col_comments
        WHERE SUBSTR(table_name, 1, 4) = 'HXS_' AND comments IS NULL
    ) LOOP
        EXECUTE IMMEDIATE 'COMMENT ON COLUMN ' || item.table_name || '.' || item.column_name ||
            ' IS ''' || quote_text(column_label(item.column_name)) || '''';
    END LOOP;
END;
/
COMMIT;

-- 验证结果应均为 0。
SELECT COUNT(*) AS UNCOMMENTED_TABLES
FROM USER_TAB_COMMENTS
WHERE SUBSTR(TABLE_NAME, 1, 4) = 'HXS_' AND COMMENTS IS NULL;

SELECT COUNT(*) AS UNCOMMENTED_COLUMNS
FROM USER_COL_COMMENTS
WHERE SUBSTR(TABLE_NAME, 1, 4) = 'HXS_' AND COMMENTS IS NULL;
