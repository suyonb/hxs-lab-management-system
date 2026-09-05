namespace HxsAiSystem.Application.Auth;

public sealed class LoginSecurityOptions
{
    public const string SectionName = "LoginSecurity";
    public int MaxFailedAttempts { get; set; } = 5;
    public int LockoutMinutes { get; set; } = 15;
}

public sealed class AccountLockedException : Exception
{
    public AccountLockedException(DateTime lockedUntil) : base($"账号已临时锁定，请在 {lockedUntil:HH:mm} 后重试。") { }
}
