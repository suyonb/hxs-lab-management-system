-- 为 Navicat 逆向数据库模型补齐实验室业务外键。
-- 使用 HXS_AISYSTEM 用户执行，可重复执行。
-- ENABLE NOVALIDATE：不因历史脏数据中断迁移，但会约束后续新增和修改数据。
DECLARE
    PROCEDURE ensure_fk(
        constraint_name VARCHAR2,
        table_name      VARCHAR2,
        column_name     VARCHAR2,
        parent_table    VARCHAR2,
        parent_column   VARCHAR2
    ) IS
        constraint_count NUMBER;
        child_table_count NUMBER;
        parent_table_count NUMBER;
    BEGIN
        SELECT COUNT(*) INTO constraint_count
        FROM USER_CONSTRAINTS
        WHERE CONSTRAINT_NAME = constraint_name;

        SELECT COUNT(*) INTO child_table_count
        FROM USER_TABLES
        WHERE TABLE_NAME = table_name;

        SELECT COUNT(*) INTO parent_table_count
        FROM USER_TABLES
        WHERE TABLE_NAME = parent_table;

        IF constraint_count = 0 AND child_table_count = 1 AND parent_table_count = 1 THEN
            EXECUTE IMMEDIATE
                'ALTER TABLE ' || table_name ||
                ' ADD CONSTRAINT ' || constraint_name ||
                ' FOREIGN KEY (' || column_name || ')' ||
                ' REFERENCES ' || parent_table || ' (' || parent_column || ')' ||
                ' ENABLE NOVALIDATE';
        END IF;
    END;
BEGIN
    -- 实验室基础数据
    ensure_fk('FK_LAB_MANAGER', 'HXS_LAB', 'MANAGER_ID', 'HXS_SYS_USER', 'ID');
    ensure_fk('FK_LOCATION_LAB', 'HXS_LAB_LOCATION', 'LAB_ID', 'HXS_LAB', 'ID');
    ensure_fk('FK_LOCATION_PARENT', 'HXS_LAB_LOCATION', 'PARENT_ID', 'HXS_LAB_LOCATION', 'ID');
    ensure_fk('FK_GROUP_LAB', 'HXS_LAB_GROUP', 'LAB_ID', 'HXS_LAB', 'ID');
    ensure_fk('FK_GROUP_LEADER', 'HXS_LAB_GROUP', 'LEADER_ID', 'HXS_SYS_USER', 'ID');
    ensure_fk('FK_MEMBER_GROUP', 'HXS_LAB_GROUP_MEMBER', 'GROUP_ID', 'HXS_LAB_GROUP', 'ID');
    ensure_fk('FK_MEMBER_USER', 'HXS_LAB_GROUP_MEMBER', 'USER_ID', 'HXS_SYS_USER', 'ID');
    ensure_fk('FK_DICT_ITEM_TYPE', 'HXS_SYS_DICT_ITEM', 'DICT_TYPE_ID', 'HXS_SYS_DICT_TYPE', 'ID');

    -- 仪器台账
    ensure_fk('FK_INSTRUMENT_CATEGORY', 'HXS_LAB_INSTRUMENT', 'CATEGORY_ID', 'HXS_SYS_DICT_ITEM', 'ID');
    ensure_fk('FK_INSTRUMENT_SUPPLIER', 'HXS_LAB_INSTRUMENT', 'SUPPLIER_ID', 'HXS_LAB_SUPPLIER', 'ID');
    ensure_fk('FK_INSTRUMENT_LAB', 'HXS_LAB_INSTRUMENT', 'LAB_ID', 'HXS_LAB', 'ID');
    ensure_fk('FK_INSTRUMENT_LOCATION', 'HXS_LAB_INSTRUMENT', 'LOCATION_ID', 'HXS_LAB_LOCATION', 'ID');

    -- 预约审批
    ensure_fk('FK_BOOKING_INSTRUMENT', 'HXS_LAB_BOOKING', 'INSTRUMENT_ID', 'HXS_LAB_INSTRUMENT', 'ID');
    ensure_fk('FK_BOOKING_APPLICANT', 'HXS_LAB_BOOKING', 'APPLICANT_ID', 'HXS_SYS_USER', 'ID');
    ensure_fk('FK_BOOKING_GROUP', 'HXS_LAB_BOOKING', 'GROUP_ID', 'HXS_LAB_GROUP', 'ID');
    ensure_fk('FK_BOOKING_APPROVER', 'HXS_LAB_BOOKING', 'APPROVER_ID', 'HXS_SYS_USER', 'ID');

    -- 使用和维修
    ensure_fk('FK_USAGE_INSTRUMENT', 'HXS_LAB_USAGE', 'INSTRUMENT_ID', 'HXS_LAB_INSTRUMENT', 'ID');
    ensure_fk('FK_USAGE_BOOKING', 'HXS_LAB_USAGE', 'BOOKING_ID', 'HXS_LAB_BOOKING', 'ID');
    ensure_fk('FK_USAGE_USER', 'HXS_LAB_USAGE', 'USER_ID', 'HXS_SYS_USER', 'ID');
    ensure_fk('FK_REPAIR_INSTRUMENT', 'HXS_LAB_REPAIR', 'INSTRUMENT_ID', 'HXS_LAB_INSTRUMENT', 'ID');
    ensure_fk('FK_REPAIR_REPORTER', 'HXS_LAB_REPAIR', 'REPORTER_ID', 'HXS_SYS_USER', 'ID');
    ensure_fk('FK_REPAIR_APPROVER', 'HXS_LAB_REPAIR', 'APPROVER_ID', 'HXS_SYS_USER', 'ID');
END;
/
COMMIT;

-- 验证：应返回上述实验室业务外键及其父表。
SELECT c.CONSTRAINT_NAME,
       c.TABLE_NAME,
       cc.COLUMN_NAME,
       p.TABLE_NAME AS PARENT_TABLE
FROM USER_CONSTRAINTS c
JOIN USER_CONS_COLUMNS cc ON cc.CONSTRAINT_NAME = c.CONSTRAINT_NAME
JOIN USER_CONSTRAINTS p ON p.CONSTRAINT_NAME = c.R_CONSTRAINT_NAME
WHERE c.CONSTRAINT_TYPE = 'R'
  AND (SUBSTR(c.TABLE_NAME, 1, 8) = 'HXS_LAB_' OR c.TABLE_NAME IN ('HXS_LAB', 'HXS_SYS_DICT_ITEM'))
ORDER BY c.TABLE_NAME, c.CONSTRAINT_NAME;
