using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HxsAiSystem.Application.Common;
using HxsAiSystem.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SqlSugar;

namespace HxsAiSystem.Application.Auth;

public sealed class AuthService : IAuthService
{
    private readonly ISqlSugarClient _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly JwtOptions _jwtOptions;
    private readonly LoginSecurityOptions _securityOptions;

    public AuthService(ISqlSugarClient db, IPasswordHasher passwordHasher, IOptions<JwtOptions> jwtOptions, IOptions<LoginSecurityOptions> securityOptions)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtOptions = jwtOptions.Value;
        _securityOptions = securityOptions.Value;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
            return null;

        var userName = request.UserName.Trim();
        var user = await _db.Queryable<AppUser>().Where(x => x.UserName == userName).FirstAsync();

        if (user is null || user.IsActive != 1)
            return null;

        if (user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.Now)
            throw new AccountLockedException(user.LockedUntil.Value);

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= Math.Max(1, _securityOptions.MaxFailedAttempts))
            {
                user.LockedUntil = DateTime.Now.AddMinutes(Math.Max(1, _securityOptions.LockoutMinutes));
                user.FailedLoginCount = 0;
            }
            user.UpdateTime = DateTime.Now;
            await _db.Updateable(user).UpdateColumns(x => new { x.FailedLoginCount, x.LockedUntil, x.UpdateTime }).ExecuteCommandAsync();
            return null;
        }

        var userId = RawGuidConverter.ToGuid(user.Id);
        user.LastLoginTime = DateTime.Now;
        user.FailedLoginCount = 0;
        user.LockedUntil = null;
        user.UpdateTime = DateTime.Now;
        await _db.Updateable(user).ExecuteCommandAsync();

        var expiresAt = DateTime.UtcNow.AddMinutes(Math.Max(1, _jwtOptions.ExpireMinutes));
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, user.UserName)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new LoginResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt,
            User = new UserProfile
            {
                Id = userId,
                UserName = user.UserName,
                DisplayName = user.DisplayName
            }
        };
    }

}
