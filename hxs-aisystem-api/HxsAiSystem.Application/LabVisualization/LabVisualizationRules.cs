using System.Text.RegularExpressions;

namespace HxsAiSystem.Application.LabVisualization;

public static class LabVisualizationRules
{
    public static readonly string[] SupportedTypes=["lab","location","instrument"];

    public static void ValidateScene(Lab3dSceneRequest request)
    {
        if(request.LabId==Guid.Empty)throw new InvalidOperationException("请选择实验室。");
        if(string.IsNullOrWhiteSpace(request.SceneName)||request.SceneName.Trim().Length>100)throw new InvalidOperationException("场景名称长度应为 1 到 100 个字符。");
        if(!Regex.IsMatch(request.BackgroundColor??string.Empty,"^#[0-9a-fA-F]{6}$"))throw new InvalidOperationException("背景色必须是六位十六进制颜色。");
    }

    public static void ValidateNode(Lab3dNodeRequest request)
    {
        if(string.IsNullOrWhiteSpace(request.Code)||request.Code.Trim().Length>80)throw new InvalidOperationException("节点编码长度应为 1 到 80 个字符。");
        if(string.IsNullOrWhiteSpace(request.Name)||request.Name.Trim().Length>100)throw new InvalidOperationException("节点名称长度应为 1 到 100 个字符。");
        if(!SupportedTypes.Contains(request.Type.Trim().ToLowerInvariant()))throw new InvalidOperationException("节点类型无效。");
        if(request.ScaleX<=0||request.ScaleY<=0||request.ScaleZ<=0)throw new InvalidOperationException("节点缩放比例必须大于零。");
    }
}
