using SqlSugar;

namespace HxsAiSystem.Domain.Entities;

[SugarTable("HXS_SYS_FILE")]
public class SysFileRecord
{
    [SugarColumn(IsPrimaryKey = true, ColumnName = "ID")]
    public byte[] Id { get; set; } = [];
    [SugarColumn(ColumnName = "BUSINESS_TYPE", Length = 50)]
    public string BusinessType { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "BUSINESS_ID", Length = 100, IsNullable = true)]
    public string? BusinessId { get; set; }
    [SugarColumn(ColumnName = "ORIGINAL_NAME", Length = 255)]
    public string OriginalName { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "STORAGE_NAME", Length = 255)]
    public string StorageName { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "FILE_PATH", Length = 500)]
    public string FilePath { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "CONTENT_TYPE", Length = 150)]
    public string ContentType { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "FILE_SIZE")]
    public long FileSize { get; set; }
    [SugarColumn(ColumnName = "UPLOADER_ID")]
    public byte[] UploaderId { get; set; } = [];
    [SugarColumn(ColumnName = "CREATE_TIME")]
    public DateTime CreateTime { get; set; }
}
