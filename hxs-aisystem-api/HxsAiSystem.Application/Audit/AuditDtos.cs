namespace HxsAiSystem.Application.Audit;

public sealed record AuditLogDto(Guid Id, Guid? UserId, string? UserName, string ModuleCode, string ActionCode,
    string? BusinessId, string RequestPath, string HttpMethod, string Result, string? IpAddress, DateTime CreateTime);
