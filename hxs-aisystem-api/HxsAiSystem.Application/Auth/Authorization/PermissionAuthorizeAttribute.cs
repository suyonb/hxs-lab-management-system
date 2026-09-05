using Microsoft.AspNetCore.Authorization;

namespace HxsAiSystem.Application.Auth.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class PermissionAuthorizeAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission:";
    public PermissionAuthorizeAttribute(string permissionCode) => Policy = PolicyPrefix + permissionCode;
}
