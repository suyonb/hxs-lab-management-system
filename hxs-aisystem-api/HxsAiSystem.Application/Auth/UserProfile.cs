namespace HxsAiSystem.Application.Auth;

public sealed class UserProfile
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
}
