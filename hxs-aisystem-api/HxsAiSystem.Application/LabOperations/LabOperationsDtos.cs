namespace HxsAiSystem.Application.LabOperations;

public sealed record UnifiedApprovalDto(string BusinessType,Guid BusinessId,string BusinessNo,Guid ApplicantId,string? ApplicantName,DateTime ApplyTime,string Summary,string Status,Guid? ApproverId,string? ApproverName,DateTime? ApproveTime,string DetailPath);
public sealed record DashboardSummaryDto(int PendingCount,int TodayBookings,int RepairingInstruments,int LowStockCount,int ExpiringCount,int ExpiredCount,int RecentExperimentCount,int ArchivedExperimentCount,List<TrendPointDto> InstrumentUsageTrend,List<TrendPointDto> MaterialConsumptionTrend);
public sealed record TrendPointDto(string Date,decimal Value);
