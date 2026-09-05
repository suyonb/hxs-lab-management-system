using HxsAiSystem.Application.Auth.Authorization;
using HxsAiSystem.Application.Files;
using HxsAiSystem.Application.LabVisualization;
using Microsoft.AspNetCore.Mvc;

namespace HxsAiSystem.WebApiHost.Controllers;

[ApiController]
[Route("api/lab/3d")]
[PermissionAuthorize("lab:3d:view")]
public sealed class LabVisualizationController(ILabVisualizationService service,IFileStorageService files):ControllerBase
{
    /// <summary>获取当前可用的实验室三维场景。</summary>
    [HttpGet("scenes")]
    public Task<List<Lab3dSceneDto>> Scenes()=>service.GetScenesAsync();

    /// <summary>获取三维场景配置、节点坐标及业务绑定。</summary>
    [HttpGet("scenes/{id:guid}")]
    public Task<Lab3dSceneDetailDto> Scene(Guid id)=>service.GetSceneAsync(id);

    /// <summary>轮询获取场景中仪器节点的最新运行状态。</summary>
    [HttpGet("scenes/{id:guid}/statuses")]
    public Task<List<Lab3dNodeStatusDto>> Statuses(Guid id)=>service.GetStatusesAsync(id);

    /// <summary>下载场景当前启用的 GLB 三维模型。</summary>
    [HttpGet("scenes/{id:guid}/model")]
    public async Task<IActionResult> Model(Guid id)
    {
        try{var fileId=await service.GetModelFileIdAsync(id);var file=await files.GetBusinessDownloadAsync(fileId,"lab-3d-model",id.ToString());return PhysicalFile(file.FullPath,file.ContentType,file.OriginalName,enableRangeProcessing:true);}
        catch(UnauthorizedAccessException){return Forbid();}
    }

    /// <summary>按现有实验室、楼栋房间位置和仪器台账生成空间展示数据。</summary>
    [HttpGet("layout")]
    public Task<List<LabSpatialLabDto>> Layout()=>service.GetSpatialLayoutAsync();

    /// <summary>按实验室轮询房间与仪器的预约、维修和运行状态。</summary>
    [HttpGet("layout/{labId:guid}/statuses")]
    public Task<List<LabSpatialStatusDto>> SpatialStatuses(Guid labId)=>service.GetSpatialStatusesAsync(labId);

    /// <summary>获取全部三维场景及节点数量，供管理页面使用。</summary>
    [HttpGet("manage/scenes"),PermissionAuthorize("lab:3d:manage")]
    public Task<List<Lab3dSceneManageDto>> ManageScenes()=>service.GetManageScenesAsync();

    /// <summary>为指定实验室新建三维场景。</summary>
    [HttpPost("manage/scenes"),PermissionAuthorize("lab:3d:manage")]
    public Task<Lab3dSceneManageDto> CreateScene(Lab3dSceneRequest request)=>service.CreateSceneAsync(request);

    /// <summary>修改三维场景名称、实验室、背景色和启用状态。</summary>
    [HttpPut("manage/scenes/{id:guid}"),PermissionAuthorize("lab:3d:manage")]
    public Task<Lab3dSceneManageDto> UpdateScene(Guid id,Lab3dSceneRequest request)=>service.UpdateSceneAsync(id,request);

    /// <summary>删除三维场景及其节点和业务绑定。</summary>
    [HttpDelete("manage/scenes/{id:guid}"),PermissionAuthorize("lab:3d:manage")]
    public Task DeleteScene(Guid id)=>service.DeleteSceneAsync(id);

    /// <summary>上传并启用场景 GLB 模型，同时递增模型版本。</summary>
    [HttpPost("manage/scenes/{id:guid}/model"),PermissionAuthorize("lab:3d:manage"),RequestSizeLimit(20*1024*1024)]
    public async Task<FileRecordDto> UploadModel(Guid id,IFormFile file,CancellationToken cancellationToken)
    {
        await using var stream=file.OpenReadStream();var result=await files.SaveAsync(stream,file.FileName,file.ContentType,file.Length,"lab-3d-model",id.ToString(),cancellationToken);await service.AttachModelAsync(id,result.Id);return result;
    }

    /// <summary>获取场景已上传的模型版本及当前启用版本。</summary>
    [HttpGet("manage/scenes/{id:guid}/models"),PermissionAuthorize("lab:3d:manage")]
    public Task<List<Lab3dModelVersionDto>> ModelVersions(Guid id)=>service.GetModelVersionsAsync(id);

    /// <summary>将历史 GLB 文件重新设为当前场景模型。</summary>
    [HttpPut("manage/scenes/{id:guid}/models/{fileId:guid}/activate"),PermissionAuthorize("lab:3d:manage")]
    public Task ActivateModelVersion(Guid id,Guid fileId)=>service.ActivateModelVersionAsync(id,fileId);

    /// <summary>在指定场景中新增空间或设备节点。</summary>
    [HttpPost("manage/scenes/{sceneId:guid}/nodes"),PermissionAuthorize("lab:3d:manage")]
    public Task<Lab3dNodeDto> CreateNode(Guid sceneId,Lab3dNodeRequest request)=>service.CreateNodeAsync(sceneId,request);

    /// <summary>修改三维节点名称、类型、坐标、缩放和排序。</summary>
    [HttpPut("manage/nodes/{id:guid}"),PermissionAuthorize("lab:3d:manage")]
    public Task<Lab3dNodeDto> UpdateNode(Guid id,Lab3dNodeRequest request)=>service.UpdateNodeAsync(id,request);

    /// <summary>删除三维节点及其业务绑定。</summary>
    [HttpDelete("manage/nodes/{id:guid}"),PermissionAuthorize("lab:3d:manage")]
    public Task DeleteNode(Guid id)=>service.DeleteNodeAsync(id);

    /// <summary>将三维节点绑定到当前实验室、位置或仪器业务数据。</summary>
    [HttpPut("manage/nodes/{id:guid}/binding"),PermissionAuthorize("lab:3d:manage")]
    public Task<Lab3dNodeDto> SetBinding(Guid id,Lab3dBindingRequest request)=>service.SetBindingAsync(id,request);

    /// <summary>解除三维节点与实验室业务数据的绑定。</summary>
    [HttpDelete("manage/nodes/{id:guid}/binding"),PermissionAuthorize("lab:3d:manage")]
    public Task RemoveBinding(Guid id)=>service.RemoveBindingAsync(id);
}
