using SqlSugar;
using System.Text;

namespace HxsAiSystem.Application.SystemFoundation;

/// <summary>为项目数据库表和字段补齐可用于反向建模的中文注释。</summary>
public sealed class DatabaseDocumentationInitializer : IDatabaseDocumentationInitializer
{
    private static readonly Dictionary<string, string> TableComments = new(StringComparer.OrdinalIgnoreCase)
    {
        ["HXS_APP_USER"] = "旧版系统用户表",
        ["HXS_AI_CONVERSATION"] = "AI推理会话表",
        ["HXS_AI_MESSAGE"] = "AI推理消息表",
        ["HXS_SYS_ORG"] = "系统组织架构表",
        ["HXS_SYS_USER"] = "系统用户表",
        ["HXS_SYS_ROLE"] = "系统角色表",
        ["HXS_SYS_MENU"] = "系统菜单和操作权限表",
        ["HXS_SYS_USER_ROLE"] = "用户角色关联表",
        ["HXS_SYS_ROLE_MENU"] = "角色菜单权限关联表",
        ["HXS_SYS_AUDIT_LOG"] = "系统操作审计日志表",
        ["HXS_SYS_FILE"] = "系统文件元数据表",
        ["HXS_SYS_DICT_TYPE"] = "业务字典类型表",
        ["HXS_SYS_DICT_ITEM"] = "业务字典项表",
        ["HXS_LAB"] = "实验室基础档案表",
        ["HXS_LAB_LOCATION"] = "实验室位置节点表",
        ["HXS_LAB_GROUP"] = "实验室课题组表",
        ["HXS_LAB_GROUP_MEMBER"] = "课题组成员关联表",
        ["HXS_LAB_SUPPLIER"] = "实验室供应商档案表",
        ["HXS_LAB_INSTRUMENT"] = "实验室仪器设备台账表",
        ["HXS_LAB_BOOKING"] = "仪器预约申请与审批表",
        ["HXS_LAB_USAGE"] = "仪器实际使用记录表",
        ["HXS_LAB_REPAIR"] = "仪器故障报修与维修记录表"
        ,["HXS_LAB_MATERIAL"] = "试剂耗材基础档案表"
        ,["HXS_LAB_STOCK_BATCH"] = "试剂耗材库存批次表"
        ,["HXS_LAB_STOCK_FLOW"] = "库存变动流水表"
        ,["HXS_LAB_REQUISITION"] = "物资领用申请与审批表"
        ,["HXS_LAB_REQUISITION_ITEM"] = "物资领用申请明细表"
        ,["HXS_LAB_EXPERIMENT"] = "实验任务与归档状态表"
        ,["HXS_LAB_EXPERIMENT_INSTRUMENT"] = "实验关联仪器预约表"
        ,["HXS_LAB_EXPERIMENT_MATERIAL"] = "实验关联物资领用表"
        ,["HXS_LAB_EXPERIMENT_RECORD"] = "实验过程与结果记录表"
    };

    private static readonly Dictionary<string, string> ColumnComments = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ID"]="主键ID，RAW(16)", ["PARENT_ID"]="上级记录ID", ["USER_ID"]="系统用户ID", ["ORG_ID"]="所属组织ID",
        ["ROLE_ID"]="角色ID", ["MENU_ID"]="菜单或权限ID", ["LAB_ID"]="所属实验室ID", ["GROUP_ID"]="课题组ID",
        ["INSTRUMENT_ID"]="仪器设备ID", ["BOOKING_ID"]="关联预约ID", ["DICT_TYPE_ID"]="字典类型ID", ["CATEGORY_ID"]="分类字典项ID",
        ["SUPPLIER_ID"]="供应商ID", ["LOCATION_ID"]="实验室位置ID", ["MANAGER_ID"]="实验室负责人用户ID", ["LEADER_ID"]="课题组负责人用户ID",
        ["APPLICANT_ID"]="预约申请人用户ID", ["APPROVER_ID"]="审批人用户ID", ["REPORTER_ID"]="报修人用户ID", ["UPLOADER_ID"]="文件上传人用户ID",
        ["CONVERSATION_ID"]="所属AI会话ID", ["USER_NAME"]="登录用户名", ["DISPLAY_NAME"]="用户显示名称", ["PASSWORD_HASH"]="密码哈希值",
        ["PHONE"]="联系电话", ["EMAIL"]="电子邮箱", ["LAST_LOGIN_TIME"]="最后登录时间", ["FAILED_LOGIN_COUNT"]="连续登录失败次数",
        ["LOCKED_UNTIL"]="账号锁定截止时间", ["ORG_NAME"]="组织名称", ["ORG_CODE"]="组织编码", ["ORG_TYPE"]="组织类型",
        ["ROLE_CODE"]="角色编码", ["ROLE_NAME"]="角色名称", ["MENU_CODE"]="菜单编码", ["MENU_NAME"]="菜单名称", ["MENU_TYPE"]="菜单类型：directory/page/button",
        ["ROUTE_PATH"]="前端路由路径", ["COMPONENT"]="前端组件路径", ["ICON"]="菜单图标", ["PERMISSION_CODE"]="接口操作权限编码",
        ["SORT_NO"]="显示排序号", ["IS_VISIBLE"]="是否可见：1可见，0隐藏", ["IS_ACTIVE"]="是否启用：1启用，0停用", ["DESCRIPTION"]="说明或描述",
        ["CREATE_TIME"]="创建时间", ["UPDATE_TIME"]="更新时间", ["TITLE"]="会话标题", ["ROLE"]="消息角色：user/assistant/system",
        ["CONTENT"]="消息正文", ["MESSAGE_TYPE"]="消息类型：text/reasoning", ["METADATA"]="结构化推理结果及模型元数据JSON",
        ["MODULE_CODE"]="业务模块编码", ["ACTION_CODE"]="操作编码", ["BUSINESS_ID"]="关联业务记录ID", ["REQUEST_PATH"]="HTTP请求路径",
        ["HTTP_METHOD"]="HTTP请求方法", ["BEFORE_DATA"]="操作前数据快照", ["AFTER_DATA"]="操作后数据快照", ["RESULT"]="执行结果：success/failed",
        ["IP_ADDRESS"]="客户端IP地址", ["BUSINESS_TYPE"]="文件所属业务类型", ["ORIGINAL_NAME"]="上传文件原始名称", ["STORAGE_NAME"]="服务器存储文件名",
        ["FILE_PATH"]="文件存储路径", ["CONTENT_TYPE"]="文件MIME类型", ["FILE_SIZE"]="文件大小，单位字节", ["LAB_CODE"]="实验室编码",
        ["LAB_NAME"]="实验室名称", ["LOCATION_CODE"]="位置编码", ["LOCATION_NAME"]="位置名称", ["LOCATION_TYPE"]="位置类型：building/room/area/cabinet",
        ["GROUP_CODE"]="课题组编码", ["GROUP_NAME"]="课题组名称", ["MEMBER_ROLE"]="课题组成员角色", ["SUPPLIER_CODE"]="供应商编码",
        ["SUPPLIER_NAME"]="供应商名称", ["CONTACT_NAME"]="联系人姓名", ["ADDRESS"]="联系地址", ["DICT_CODE"]="字典编码", ["DICT_NAME"]="字典名称",
        ["ITEM_VALUE"]="字典项值", ["ITEM_LABEL"]="字典项显示名称", ["INSTRUMENT_CODE"]="仪器编号", ["INSTRUMENT_NAME"]="仪器名称",
        ["MODEL"]="设备型号", ["MANUFACTURER"]="生产厂家", ["STATUS"]="业务状态", ["BOOKING_NO"]="预约单号", ["START_TIME"]="开始时间",
        ["END_TIME"]="结束时间", ["PURPOSE"]="预约用途", ["APPROVE_TIME"]="审批时间", ["APPROVE_REMARK"]="审批意见", ["CANCEL_TIME"]="取消时间",
        ["EXPERIMENT_CONTENT"]="实验内容", ["REMARK"]="备注", ["REPAIR_NO"]="报修单号", ["FAULT_DESCRIPTION"]="故障描述", ["REPAIRER"]="维修人员",
        ["REPAIR_CONTENT"]="维修处理内容", ["REPAIR_START_TIME"]="维修开始时间", ["REPAIR_END_TIME"]="维修完成时间"
        ,["MATERIAL_CODE"]="物资编码",["MATERIAL_NAME"]="物资名称",["MATERIAL_TYPE"]="物资类型：reagent/consumable",["SPECIFICATION"]="规格型号",["CAS_NO"]="化学品CAS号",["UNIT_ID"]="计量单位字典项ID",["STORAGE_LOCATION_ID"]="存放位置ID",["MIN_STOCK"]="最低库存预警值",["BATCH_NO"]="库存批次号",["PRODUCTION_DATE"]="生产日期",["EXPIRY_DATE"]="有效期",["IN_QUANTITY"]="入库数量",["AVAILABLE_QUANTITY"]="当前可用数量",["UNIT_PRICE"]="单位价格",["STOCK_IN_TIME"]="入库时间",["FLOW_NO"]="库存流水号",["FLOW_TYPE"]="流水类型：in/out/adjust",["QUANTITY"]="库存变动或关联数量",["BEFORE_QUANTITY"]="变动前数量",["AFTER_QUANTITY"]="变动后数量",["SOURCE_TYPE"]="流水业务来源",["SOURCE_ID"]="来源业务ID",["OPERATOR_ID"]="操作人用户ID",["REQUISITION_NO"]="领用申请单号",["REQUISITION_ID"]="领用申请ID",["REQUEST_QUANTITY"]="申请数量",["APPROVED_QUANTITY"]="批准数量",
        ["EXPERIMENT_ID"]="实验任务ID",["EXPERIMENT_NO"]="实验任务编号",["EXPERIMENT_NAME"]="实验名称",["OWNER_ID"]="实验负责人用户ID",["TOPIC_NAME"]="课题名称",["ARCHIVE_USER_ID"]="归档操作人用户ID",["ARCHIVE_TIME"]="归档时间",["RECORD_TYPE"]="记录类型：process/result/raw_data/reopen/unarchive",["RECORD_TIME"]="记录时间",["CREATOR_ID"]="记录创建人用户ID"
    };

    private readonly ISqlSugarClient _db;
    public DatabaseDocumentationInitializer(ISqlSugarClient db) => _db = db;

    public async Task InitializeAsync()
    {
        const string tableScope = "SUBSTR(TABLE_NAME, 1, 4) = 'HXS_'";
        var tables = await _db.Ado.SqlQueryAsync<MetadataRow>($"SELECT TABLE_NAME Name, COMMENTS Comments FROM USER_TAB_COMMENTS WHERE {tableScope}");
        var commands = new List<string>();
        foreach (var table in tables.Where(x => string.IsNullOrWhiteSpace(x.Comments)))
            commands.Add($"COMMENT ON TABLE {SafeName(table.Name)} IS '{Escape(TableComments.GetValueOrDefault(table.Name, $"{table.Name}业务数据表"))}'");

        var columns = await _db.Ado.SqlQueryAsync<ColumnMetadataRow>("SELECT c.TABLE_NAME TableName, c.COLUMN_NAME Name, cc.COMMENTS Comments FROM USER_TAB_COLUMNS c LEFT JOIN USER_COL_COMMENTS cc ON cc.TABLE_NAME=c.TABLE_NAME AND cc.COLUMN_NAME=c.COLUMN_NAME WHERE SUBSTR(c.TABLE_NAME, 1, 4) = 'HXS_'");
        foreach (var column in columns.Where(x => string.IsNullOrWhiteSpace(x.Comments)))
            commands.Add($"COMMENT ON COLUMN {SafeName(column.TableName)}.{SafeName(column.Name)} IS '{Escape(ColumnComments.GetValueOrDefault(column.Name, $"{column.Name}字段"))}'");

        if (commands.Count > 0)
        {
            var block = new StringBuilder("BEGIN\n");
            foreach (var command in commands)
                block.Append("EXECUTE IMMEDIATE '").Append(Escape(command)).Append("';\n");
            block.Append("END;");
            await _db.Ado.ExecuteCommandAsync(block.ToString());
        }

        var missingTableCount = await _db.Ado.GetIntAsync($"SELECT COUNT(*) FROM USER_TAB_COMMENTS WHERE {tableScope} AND COMMENTS IS NULL");
        var missingColumnCount = await _db.Ado.GetIntAsync("SELECT COUNT(*) FROM USER_COL_COMMENTS WHERE SUBSTR(TABLE_NAME, 1, 4) = 'HXS_' AND COMMENTS IS NULL");
        if (missingTableCount > 0 || missingColumnCount > 0)
            throw new InvalidOperationException($"数据库注释补全失败：仍有 {missingTableCount} 张表、{missingColumnCount} 个字段缺少注释。");
    }

    private static string SafeName(string value) => value.All(x => char.IsAsciiLetterOrDigit(x) || x == '_') ? value : throw new InvalidOperationException("数据库对象名称不合法。");
    private static string Escape(string value) => value.Replace("'", "''", StringComparison.Ordinal);
    private sealed class MetadataRow { public string Name { get; set; } = ""; public string? Comments { get; set; } }
    private sealed class ColumnMetadataRow { public string TableName { get; set; } = ""; public string Name { get; set; } = ""; public string? Comments { get; set; } }
}
