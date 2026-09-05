using SqlSugar;

namespace HxsAiSystem.Domain.Entities;

/// <summary>系统用户。</summary>
[SugarTable("HXS_SYS_USER")]
public class AppUser
{
    [SugarColumn(IsPrimaryKey = true, ColumnName = "ID")]
    public byte[] Id { get; set; } = [];

    [SugarColumn(ColumnName = "USER_NAME", Length = 100)]
    public string UserName { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "DISPLAY_NAME", Length = 100, IsNullable = true)]
    public string? DisplayName { get; set; }

    [SugarColumn(ColumnName = "ORG_ID", IsNullable = true)]
    public byte[]? OrgId { get; set; }

    [SugarColumn(ColumnName = "PASSWORD_HASH", Length = 300)]
    public string PasswordHash { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "PHONE", Length = 30, IsNullable = true)]
    public string? Phone { get; set; }

    [SugarColumn(ColumnName = "EMAIL", Length = 100, IsNullable = true)]
    public string? Email { get; set; }

    [SugarColumn(ColumnName = "IS_ACTIVE")]
    public int IsActive { get; set; }

    [SugarColumn(ColumnName = "LAST_LOGIN_TIME", IsNullable = true)]
    public DateTime? LastLoginTime { get; set; }

    [SugarColumn(ColumnName = "FAILED_LOGIN_COUNT")]
    public int FailedLoginCount { get; set; }

    [SugarColumn(ColumnName = "LOCKED_UNTIL", IsNullable = true)]
    public DateTime? LockedUntil { get; set; }

    [SugarColumn(ColumnName = "CREATE_TIME")]
    public DateTime CreateTime { get; set; }

    [SugarColumn(ColumnName = "UPDATE_TIME")]
    public DateTime UpdateTime { get; set; }
}
