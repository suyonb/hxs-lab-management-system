namespace HxsAiSystem.Application.LabVisualization;
public interface ILabVisualizationService
{
    Task<List<Lab3dSceneDto>> GetScenesAsync();
    Task<Lab3dSceneDetailDto> GetSceneAsync(Guid id);
    Task<List<Lab3dNodeStatusDto>> GetStatusesAsync(Guid id);
    Task<List<LabSpatialLabDto>> GetSpatialLayoutAsync();
    Task<List<LabSpatialStatusDto>> GetSpatialStatusesAsync(Guid labId);
    Task<List<Lab3dSceneManageDto>> GetManageScenesAsync();
    Task<Lab3dSceneManageDto> CreateSceneAsync(Lab3dSceneRequest request);
    Task<Lab3dSceneManageDto> UpdateSceneAsync(Guid id,Lab3dSceneRequest request);
    Task DeleteSceneAsync(Guid id);
    Task<Lab3dNodeDto> CreateNodeAsync(Guid sceneId,Lab3dNodeRequest request);
    Task<Lab3dNodeDto> UpdateNodeAsync(Guid nodeId,Lab3dNodeRequest request);
    Task DeleteNodeAsync(Guid nodeId);
    Task<Lab3dNodeDto> SetBindingAsync(Guid nodeId,Lab3dBindingRequest request);
    Task RemoveBindingAsync(Guid nodeId);
    Task AttachModelAsync(Guid sceneId,Guid fileId);
    Task ActivateModelVersionAsync(Guid sceneId,Guid fileId);
    Task<Guid> GetModelFileIdAsync(Guid sceneId);
    Task<List<Lab3dModelVersionDto>> GetModelVersionsAsync(Guid sceneId);
}
public interface ILabVisualizationInitializer { Task InitializeAsync(); }
