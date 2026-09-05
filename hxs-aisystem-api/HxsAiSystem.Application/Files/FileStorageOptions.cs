namespace HxsAiSystem.Application.Files;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";
    public string RootPath { get; set; } = "App_Data/uploads";
    public long MaxFileSizeBytes { get; set; } = 20 * 1024 * 1024;
    public long MaxBusinessSizeBytes { get; set; } = 100 * 1024 * 1024;
    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".pdf", ".csv", ".xlsx", ".docx", ".txt", ".glb"];
    public string[] AllowedContentTypes { get; set; } = ["image/jpeg","image/png","application/pdf","text/csv","text/plain","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet","application/vnd.openxmlformats-officedocument.wordprocessingml.document","model/gltf-binary","application/octet-stream"];
}
