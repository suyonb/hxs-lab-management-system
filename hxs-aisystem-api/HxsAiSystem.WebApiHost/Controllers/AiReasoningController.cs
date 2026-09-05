using HxsAiSystem.Application.AiReasoning;
using HxsAiSystem.Application.Auth;
using HxsAiSystem.WebApiHost.Filters;
using HxsAiSystem.Application.Auth.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HxsAiSystem.WebApiHost.Controllers;

[ApiController]
[PermissionAuthorize("ai:reasoning:use")]
[Route("api/ai/conversations")]
public class AiReasoningController : ControllerBase
{
    private readonly IAiReasoningService _service;
    private readonly ICurrentUserService _currentUser;

    public AiReasoningController(IAiReasoningService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    /// <summary>查询当前用户的数据推理会话列表。</summary>
    [HttpGet]
    public Task<List<ConversationDto>> GetList() => _service.GetConversationsAsync(UserId());

    /// <summary>为当前用户创建一个新的数据推理会话。</summary>
    [HttpPost]
    public Task<ConversationDto> Create([FromBody] CreateConversationRequest request) => _service.CreateConversationAsync(UserId(), request);

    /// <summary>查询指定推理会话中的全部消息。</summary>
    [HttpGet("{id:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid id)
    {
        try { return Ok(await _service.GetMessagesAsync(UserId(), id)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>提交文本并返回结构化数据推理结果。</summary>
    [HttpPost("{id:guid}/reason")]
    public async Task<IActionResult> Reason(Guid id, [FromBody] ReasoningRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await _service.ReasonAsync(UserId(), id, request, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>删除当前用户的指定推理会话及其消息。</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try { return await _service.DeleteConversationAsync(UserId(), id) ? NoContent() : NotFound(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    private Guid UserId() => _currentUser.GetUserId()!.Value;
}
