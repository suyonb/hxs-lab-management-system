namespace HxsAiSystem.Application.Auth;

public sealed class LoginResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string TokenType { get; init; } = "Bearer";
    public DateTime ExpiresAt { get; init; }
    public UserProfile User { get; init; } = new();
}
