namespace HxsAiSystem.Application.LabOperations;
public interface ILabOperationsService
{
    Task<List<UnifiedApprovalDto>> GetApprovalsAsync(string view,string? businessType=null,string? status=null,string? keyword=null,DateTime? startTime=null,DateTime? endTime=null);
    Task<DashboardSummaryDto> GetDashboardAsync(int days=7);
    Task<(byte[] Content,string FileName)> ExportAsync(string type,string? keyword=null,string? status=null,DateTime? startTime=null,DateTime? endTime=null);
}
