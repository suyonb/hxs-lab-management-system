using HxsAiSystem.Application.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HxsAiSystem.WebApiHost.Filters;

public sealed class RequireLoginFilter : IActionFilter
{
    private readonly ICurrentUserService _currentUserService;

    public RequireLoginFilter(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!_currentUserService.GetUserId().HasValue)
            context.Result = new UnauthorizedObjectResult(new { message = "登录已失效，请重新登录。" });
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
