using Microsoft.AspNetCore.Authorization;

namespace HxsAiSystem.Application.Auth.Authorization;

public sealed record PermissionRequirement(string PermissionCode) : IAuthorizationRequirement;
