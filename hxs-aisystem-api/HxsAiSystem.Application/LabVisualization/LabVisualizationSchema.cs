using SqlSugar;

namespace HxsAiSystem.Application.LabVisualization;

internal static class LabVisualizationSchema
{
    public static async Task<bool> EnsureTables(ISqlSugarClient db)
    {
        foreach (var name in new[] { "HXS_LAB_3D_BINDING", "HXS_LAB_3D_NODE", "HXS_LAB_3D_SCENE" })
        {
            var exists = await db.Ado.GetIntAsync("SELECT COUNT(*) FROM USER_TABLES WHERE TABLE_NAME = :name", new SugarParameter(":name", name));
            if (exists == 0) continue;
            var rawId = await db.Ado.GetIntAsync("SELECT COUNT(*) FROM USER_TAB_COLUMNS WHERE TABLE_NAME = :name AND COLUMN_NAME = 'ID' AND DATA_TYPE = 'RAW' AND DATA_LENGTH = 16", new SugarParameter(":name", name));
            if (rawId == 0) return false;
        }
        var tables = new[]
        {
            ("HXS_LAB_3D_SCENE", "CREATE TABLE HXS_LAB_3D_SCENE (ID RAW(16) NOT NULL, LAB_ID RAW(16) NOT NULL, SCENE_NAME VARCHAR2(100 CHAR) NOT NULL, MODEL_URL VARCHAR2(500 CHAR), MODEL_FILE_ID RAW(16), VERSION NUMBER(10) DEFAULT 1 NOT NULL, BACKGROUND_COLOR VARCHAR2(20 CHAR) NOT NULL, IS_ACTIVE NUMBER(1) NOT NULL, CREATE_TIME TIMESTAMP(6) NOT NULL, UPDATE_TIME TIMESTAMP(6) NOT NULL, CONSTRAINT PK_HXS_LAB_3D_SCENE PRIMARY KEY(ID))"),
            ("HXS_LAB_3D_NODE", "CREATE TABLE HXS_LAB_3D_NODE (ID RAW(16) NOT NULL, SCENE_ID RAW(16) NOT NULL, NODE_CODE VARCHAR2(80 CHAR) NOT NULL, NODE_NAME VARCHAR2(100 CHAR) NOT NULL, NODE_TYPE VARCHAR2(30 CHAR) NOT NULL, POSITION_X NUMBER(18,4) NOT NULL, POSITION_Y NUMBER(18,4) NOT NULL, POSITION_Z NUMBER(18,4) NOT NULL, SCALE_X NUMBER(18,4) NOT NULL, SCALE_Y NUMBER(18,4) NOT NULL, SCALE_Z NUMBER(18,4) NOT NULL, SORT_NO NUMBER(10) NOT NULL, CREATE_TIME TIMESTAMP(6) NOT NULL, UPDATE_TIME TIMESTAMP(6) NOT NULL, CONSTRAINT PK_HXS_LAB_3D_NODE PRIMARY KEY(ID), CONSTRAINT FK_3D_NODE_SCENE FOREIGN KEY(SCENE_ID) REFERENCES HXS_LAB_3D_SCENE(ID))"),
            ("HXS_LAB_3D_BINDING", "CREATE TABLE HXS_LAB_3D_BINDING (ID RAW(16) NOT NULL, NODE_ID RAW(16) NOT NULL, BUSINESS_TYPE VARCHAR2(30 CHAR) NOT NULL, BUSINESS_ID RAW(16) NOT NULL, CREATE_TIME TIMESTAMP(6) NOT NULL, UPDATE_TIME TIMESTAMP(6) NOT NULL, CONSTRAINT PK_HXS_LAB_3D_BINDING PRIMARY KEY(ID), CONSTRAINT FK_3D_BIND_NODE FOREIGN KEY(NODE_ID) REFERENCES HXS_LAB_3D_NODE(ID))")
        };
        foreach (var (name, ddl) in tables)
            if (await db.Ado.GetIntAsync("SELECT COUNT(*) FROM USER_TABLES WHERE TABLE_NAME = :name", new SugarParameter(":name", name)) == 0)
                await db.Ado.ExecuteCommandAsync(ddl);
        await EnsureColumn(db, "HXS_LAB_3D_SCENE", "MODEL_FILE_ID", "RAW(16)");
        await EnsureColumn(db, "HXS_LAB_3D_SCENE", "VERSION", "NUMBER(10) DEFAULT 1 NOT NULL");
        return true;
    }

    private static async Task EnsureColumn(ISqlSugarClient db, string table, string column, string definition)
    {
        var exists = await db.Ado.GetIntAsync(
            "SELECT COUNT(*) FROM USER_TAB_COLUMNS WHERE TABLE_NAME = :tableName AND COLUMN_NAME = :columnName",
            new SugarParameter(":tableName", table), new SugarParameter(":columnName", column));
        if (exists == 0) await db.Ado.ExecuteCommandAsync($"ALTER TABLE {table} ADD {column} {definition}");
    }
}
