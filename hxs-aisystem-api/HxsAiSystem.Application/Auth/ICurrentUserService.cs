namespace HxsAiSystem.Application.Auth;

public interface ICurrentUserService
{
    Guid? GetUserId();
    string? GetUserName();
    bool IsAuthenticated { get; }
}
