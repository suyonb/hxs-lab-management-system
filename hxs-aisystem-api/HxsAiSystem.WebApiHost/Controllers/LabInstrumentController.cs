using HxsAiSystem.Application.Auth.Authorization;
using HxsAiSystem.Application.LabInstrument;
using Microsoft.AspNetCore.Mvc;

namespace HxsAiSystem.WebApiHost.Controllers;

[ApiController]
[Route("api/lab/instruments")]
public sealed class LabInstrumentController : ControllerBase
{
    private readonly ILabInstrumentService _service;
    public LabInstrumentController(ILabInstrumentService service) => _service = service;

    /// <summary>查询仪器台账；availableOnly=true时仅返回正常启用且可预约的仪器。</summary>
    [HttpGet, PermissionAuthorize("lab:instrument:view")] public Task<List<InstrumentDto>> GetInstruments([FromQuery] bool availableOnly = false) => _service.GetInstrumentsAsync(availableOnly);
    /// <summary>新增仪器台账并校验实验室、位置及唯一编号。</summary>
    [HttpPost, PermissionAuthorize("lab:instrument:manage")] public Task<InstrumentDto> CreateInstrument(InstrumentRequest request) => _service.CreateInstrumentAsync(request);
    /// <summary>修改仪器资料、运行状态和启停状态，仪器编号保持不变。</summary>
    [HttpPut("{id:guid}"), PermissionAuthorize("lab:instrument:manage")] public async Task<IActionResult> UpdateInstrument(Guid id, InstrumentRequest request) { await _service.UpdateInstrumentAsync(id, request); return NoContent(); }

    /// <summary>查询预约记录；普通实验员仅能查看本人数据。</summary>
    [HttpGet("bookings"), PermissionAuthorize("lab:booking:view")] public Task<List<BookingDto>> GetBookings([FromQuery] bool mine = false, [FromQuery] string? status = null) => _service.GetBookingsAsync(mine, status);
    /// <summary>提交仪器预约，事务内校验仪器状态和时间冲突。</summary>
    [HttpPost("bookings"), PermissionAuthorize("lab:booking:create")] public Task<BookingDto> CreateBooking(BookingRequest request) => _service.CreateBookingAsync(request);
    /// <summary>取消本人待审核或符合条件的已通过预约。</summary>
    [HttpPost("bookings/{id:guid}/cancel"), PermissionAuthorize("lab:booking:cancel")] public async Task<IActionResult> CancelBooking(Guid id) { await _service.CancelBookingAsync(id); return NoContent(); }
    /// <summary>审批通过待审核预约，并再次校验时间冲突。</summary>
    [HttpPost("bookings/{id:guid}/approve"), PermissionAuthorize("lab:booking:approve")] public async Task<IActionResult> ApproveBooking(Guid id, ApprovalRequest request) { await _service.ApproveBookingAsync(id, request); return NoContent(); }
    /// <summary>驳回待审核预约并记录审批意见。</summary>
    [HttpPost("bookings/{id:guid}/reject"), PermissionAuthorize("lab:booking:approve")] public async Task<IActionResult> RejectBooking(Guid id, ApprovalRequest request) { await _service.RejectBookingAsync(id, request); return NoContent(); }
    /// <summary>将已通过预约标记为已完成。</summary>
    [HttpPost("bookings/{id:guid}/complete"), PermissionAuthorize("lab:booking:approve")] public async Task<IActionResult> CompleteBooking(Guid id) { await _service.CompleteBookingAsync(id); return NoContent(); }

    /// <summary>查询仪器使用记录；普通实验员仅能查看本人数据。</summary>
    [HttpGet("usages"), PermissionAuthorize("lab:usage:view")] public Task<List<UsageDto>> GetUsages([FromQuery] bool mine = false) => _service.GetUsagesAsync(mine);
    /// <summary>登记仪器实际使用信息，可关联已通过预约并自动完成预约。</summary>
    [HttpPost("usages"), PermissionAuthorize("lab:usage:create")] public Task<UsageDto> CreateUsage(UsageRequest request) => _service.CreateUsageAsync(request);

    /// <summary>查询设备报修和维修记录；普通实验员仅能查看本人数据。</summary>
    [HttpGet("repairs"), PermissionAuthorize("lab:repair:view")] public Task<List<RepairDto>> GetRepairs([FromQuery] bool mine = false, [FromQuery] string? status = null) => _service.GetRepairsAsync(mine, status);
    /// <summary>提交仪器故障报修申请。</summary>
    [HttpPost("repairs"), PermissionAuthorize("lab:repair:create")] public Task<RepairDto> CreateRepair(RepairRequest request) => _service.CreateRepairAsync(request);
    /// <summary>通过待审核报修并自动将仪器切换为维修状态。</summary>
    [HttpPost("repairs/{id:guid}/approve"), PermissionAuthorize("lab:repair:approve")] public async Task<IActionResult> ApproveRepair(Guid id, ApprovalRequest request) { await _service.ApproveRepairAsync(id, request); return NoContent(); }
    /// <summary>驳回待审核报修并记录原因。</summary>
    [HttpPost("repairs/{id:guid}/reject"), PermissionAuthorize("lab:repair:approve")] public async Task<IActionResult> RejectRepair(Guid id, ApprovalRequest request) { await _service.RejectRepairAsync(id, request); return NoContent(); }
    /// <summary>登记维修人员和处理内容，将报修转为维修中。</summary>
    [HttpPost("repairs/{id:guid}/start"), PermissionAuthorize("lab:repair:work")] public async Task<IActionResult> StartRepair(Guid id, RepairWorkRequest request) { await _service.StartRepairAsync(id, request); return NoContent(); }
    /// <summary>完成维修记录并自动恢复仪器正常状态。</summary>
    [HttpPost("repairs/{id:guid}/complete"), PermissionAuthorize("lab:repair:work")] public async Task<IActionResult> CompleteRepair(Guid id, RepairWorkRequest request) { await _service.CompleteRepairAsync(id, request); return NoContent(); }
}
