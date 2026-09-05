namespace HxsAiSystem.Application.LabExperiment;

public interface ILabExperimentService
{
    Task<List<ExperimentDto>> GetAsync(bool mine=false,string? keyword=null,string? status=null,DateTime? startTime=null,DateTime? endTime=null);
    Task<ExperimentDto> GetByIdAsync(Guid id);
    Task<ExperimentDto> CreateAsync(ExperimentRequest request);
    Task UpdateAsync(Guid id,ExperimentRequest request);
    Task StartAsync(Guid id);
    Task CompleteAsync(Guid id);
    Task ReopenAsync(Guid id,ExperimentReasonRequest request);
    Task ArchiveAsync(Guid id);
    Task UnarchiveAsync(Guid id,ExperimentReasonRequest request);
    Task<ExperimentRecordDto> AddRecordAsync(Guid id,ExperimentRecordRequest request);
}
