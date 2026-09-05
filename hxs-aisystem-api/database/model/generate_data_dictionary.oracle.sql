-- SQL*Plus/SQLcl 执行：生成当前 Oracle 实例的数据字典模型文档。
SET PAGESIZE 50000
SET LINESIZE 240
SET LONG 100000
SET TRIMSPOOL ON
SET FEEDBACK OFF
SET HEADING OFF
SPOOL hxs-data-dictionary.md

PROMPT # HXS 实验室管理系统数据字典
PROMPT
PROMPT > 本文件由 Oracle 元数据生成，请勿手工维护字段清单。
PROMPT

SELECT '## ' || t.table_name || ' - ' || NVL(tc.comments, '未命名业务表') || CHR(10) || CHR(10) ||
       '| 字段 | 类型 | 可空 | 默认值 | 注释 |' || CHR(10) ||
       '| --- | --- | --- | --- | --- |' || CHR(10) ||
       LISTAGG('| `' || c.column_name || '` | ' ||
         CASE WHEN c.data_type IN ('VARCHAR2', 'CHAR') THEN c.data_type || '(' || c.char_length || ')'
              WHEN c.data_type = 'NUMBER' AND c.data_precision IS NOT NULL THEN c.data_type || '(' || c.data_precision || NVL2(c.data_scale, ',' || c.data_scale, '') || ')'
              ELSE c.data_type END || ' | ' ||
         CASE c.nullable WHEN 'Y' THEN '是' ELSE '否' END || ' | ' ||
         NVL(REPLACE(c.data_default, CHR(10), ' '), '') || ' | ' ||
         NVL(REPLACE(cc.comments, '|', '\|'), '') || ' |', CHR(10))
         WITHIN GROUP (ORDER BY c.column_id) || CHR(10)
FROM user_tables t
JOIN user_tab_columns c ON c.table_name = t.table_name
LEFT JOIN user_tab_comments tc ON tc.table_name = t.table_name
LEFT JOIN user_col_comments cc ON cc.table_name = c.table_name AND cc.column_name = c.column_name
WHERE SUBSTR(t.table_name, 1, 4) = 'HXS_'
GROUP BY t.table_name, tc.comments
ORDER BY t.table_name;

SPOOL OFF
SET HEADING ON
SET FEEDBACK ON
