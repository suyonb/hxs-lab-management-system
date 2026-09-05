using SqlSugar;

namespace HxsAiSystem.Application.AiReasoning;

public sealed class AiSchemaInitializer : IAiSchemaInitializer
{
    private readonly ISqlSugarClient _db;

    public AiSchemaInitializer(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task InitializeAsync()
    {
        if (!await ColumnExistsAsync("HXS_AI_CONVERSATION", "USER_ID"))
            await _db.Ado.ExecuteCommandAsync("ALTER TABLE HXS_AI_CONVERSATION ADD USER_ID RAW(16)");

        if (!await ConstraintExistsAsync("FK_HXS_AI_CONV_USER"))
            await _db.Ado.ExecuteCommandAsync("ALTER TABLE HXS_AI_CONVERSATION ADD CONSTRAINT FK_HXS_AI_CONV_USER FOREIGN KEY (USER_ID) REFERENCES HXS_SYS_USER (ID)");

        if (!await IndexExistsAsync("IX_HXS_AI_CONV_USER"))
            await _db.Ado.ExecuteCommandAsync("CREATE INDEX IX_HXS_AI_CONV_USER ON HXS_AI_CONVERSATION (USER_ID, UPDATE_TIME)");

        if (!await ColumnExistsAsync("HXS_AI_MESSAGE", "MESSAGE_TYPE"))
            await _db.Ado.ExecuteCommandAsync("ALTER TABLE HXS_AI_MESSAGE ADD MESSAGE_TYPE VARCHAR2(30 CHAR) DEFAULT 'text' NOT NULL");

        if (!await ColumnExistsAsync("HXS_AI_MESSAGE", "METADATA"))
            await _db.Ado.ExecuteCommandAsync("ALTER TABLE HXS_AI_MESSAGE ADD METADATA CLOB");
    }

    private async Task<bool> ColumnExistsAsync(string tableName, string columnName)
    {
        const string sql = "SELECT COUNT(*) FROM USER_TAB_COLUMNS WHERE TABLE_NAME = :tableName AND COLUMN_NAME = :columnName";
        return await _db.Ado.GetIntAsync(sql,
            new SugarParameter(":tableName", tableName),
            new SugarParameter(":columnName", columnName)) > 0;
    }

    private async Task<bool> ConstraintExistsAsync(string name)
    {
        const string sql = "SELECT COUNT(*) FROM USER_CONSTRAINTS WHERE CONSTRAINT_NAME = :name";
        return await _db.Ado.GetIntAsync(sql, new SugarParameter(":name", name)) > 0;
    }

    private async Task<bool> IndexExistsAsync(string name)
    {
        const string sql = "SELECT COUNT(*) FROM USER_INDEXES WHERE INDEX_NAME = :name";
        return await _db.Ado.GetIntAsync(sql, new SugarParameter(":name", name)) > 0;
    }
}
