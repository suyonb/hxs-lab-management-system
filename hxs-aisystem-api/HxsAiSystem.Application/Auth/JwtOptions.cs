namespace HxsAiSystem.Application.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "hxs-aisystem-api";
    public string Audience { get; set; } = "hxs-aisystem-client";
    public string SecretKey { get; set; } = string.Empty;
    public int ExpireMinutes { get; set; } = 120;
}
