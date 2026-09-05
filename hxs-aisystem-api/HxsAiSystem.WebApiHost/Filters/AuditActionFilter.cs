using System.Text.Json;
using System.Text.Json.Nodes;
using HxsAiSystem.Application.Auth;
using HxsAiSystem.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SqlSugar;

namespace HxsAiSystem.WebApiHost.Filters;

public sealed class AuditActionFilter : IAsyncActionFilter
{
    private static readonly HashSet<string> SensitiveNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "passwordHash", "accessToken", "apiKey", "secretKey", "connectionString"
    };

    private readonly ISqlSugarClient _db;
    private readonly ICurrentUserService _currentUser;

    public AuditActionFilter(ISqlSugarClient db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (HttpMethods.IsGet(context.HttpContext.Request.Method) || HttpMethods.IsHead(context.HttpContext.Request.Method))
        {
            await next();
            return;
        }

        var executed = await next();
        var descriptor = context.ActionDescriptor.RouteValues;
        descriptor.TryGetValue("controller", out var controllerName);
        descriptor.TryGetValue("action", out var actionName);
        var userId = _currentUser.GetUserId();
        var userName = _currentUser.GetUserName() ?? ExtractLoginUserName(context.ActionArguments);
        var result = executed.Exception is null && executed.HttpContext.Response.StatusCode < 400 ? "success" : "failed";
        var statusResult = executed.Result as ObjectResult;
        var row = new SysAuditLog
        {
            Id = Guid.NewGuid().ToByteArray(),
            UserId = userId?.ToByteArray(),
            UserName = userName,
            ModuleCode = controllerName ?? "unknown",
            ActionCode = actionName ?? "unknown",
            BusinessId = context.RouteData.Values.GetValueOrDefault("id")?.ToString(),
            RequestPath = context.HttpContext.Request.Path,
            HttpMethod = context.HttpContext.Request.Method,
            BeforeData = SafeSerialize(context.ActionArguments),
            AfterData = SafeSerialize(statusResult?.Value),
            Result = result,
            IpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
            CreateTime = DateTime.Now
        };
        await _db.Insertable(row).ExecuteCommandAsync();
    }

    private static string? ExtractLoginUserName(IDictionary<string, object?> arguments) =>
        arguments.Values.OfType<LoginRequest>().FirstOrDefault()?.UserName;

    private static string? SafeSerialize(object? value)
    {
        if (value is null) return null;
        try
        {
            var node = JsonSerializer.SerializeToNode(value);
            Redact(node);
            var json = node?.ToJsonString();
            return json is { Length: > 12000 } ? json[..12000] : json;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static void Redact(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            foreach (var property in obj.ToList())
            {
                if (SensitiveNames.Contains(property.Key)) obj[property.Key] = "***";
                else Redact(property.Value);
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var item in array) Redact(item);
        }
    }
}
