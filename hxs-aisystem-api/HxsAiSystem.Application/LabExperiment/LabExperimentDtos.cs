namespace HxsAiSystem.Application.LabExperiment;

public sealed record ExperimentInstrumentDto(Guid Id,Guid InstrumentId,string? InstrumentName,Guid? BookingId,string? BookingNo);
public sealed record ExperimentMaterialDto(Guid Id,Guid MaterialId,string? MaterialName,Guid? RequisitionId,string? RequisitionNo,decimal Quantity,string? UnitName);
public sealed record ExperimentRecordDto(Guid Id,string RecordType,string Content,DateTime RecordTime,Guid CreatorId,string? CreatorName);
public sealed record ExperimentFileDto(Guid Id,string OriginalName,string ContentType,long FileSize,Guid UploaderId,DateTime CreateTime);
public sealed record ExperimentDto(Guid Id,string ExperimentNo,string ExperimentName,Guid? GroupId,string? GroupName,Guid OwnerId,string? OwnerName,string? TopicName,string Purpose,string Status,DateTime? StartTime,DateTime? EndTime,Guid? ArchiveUserId,DateTime? ArchiveTime,DateTime CreateTime,List<ExperimentInstrumentDto> Instruments,List<ExperimentMaterialDto> Materials,List<ExperimentRecordDto> Records,List<ExperimentFileDto> Files);

public sealed class ExperimentInstrumentRequest { public Guid InstrumentId { get; set; } public Guid? BookingId { get; set; } }
public sealed class ExperimentMaterialRequest { public Guid MaterialId { get; set; } public Guid? RequisitionId { get; set; } public decimal Quantity { get; set; } }
public sealed class ExperimentRequest
{
    public string ExperimentName { get; set; } = string.Empty;
    public Guid? GroupId { get; set; }
    public string? TopicName { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public List<ExperimentInstrumentRequest> Instruments { get; set; } = [];
    public List<ExperimentMaterialRequest> Materials { get; set; } = [];
}
public sealed class ExperimentRecordRequest { public string RecordType { get; set; } = "process"; public string Content { get; set; } = string.Empty; public DateTime? RecordTime { get; set; } }
public sealed class ExperimentReasonRequest { public string Reason { get; set; } = string.Empty; }
