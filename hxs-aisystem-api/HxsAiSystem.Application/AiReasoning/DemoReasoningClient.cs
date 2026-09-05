using System.Globalization;
using System.Text.RegularExpressions;

namespace HxsAiSystem.Application.AiReasoning;

public sealed partial class DemoReasoningClient : IAiModelClient
{
    public string ProviderName => "demo";

    public Task<ReasoningResult> AnalyzeAsync(string content, CancellationToken cancellationToken = default)
    {
        var sentences = content.Split(['。', '！', '？', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var metrics = MetricRegex().Matches(content).Select(x => x.Value).Distinct().Take(8).ToList();
        var hasRisk = RiskRegex().IsMatch(content);
        var facts = sentences.Take(4).Select(x => x.Length > 90 ? x[..90] + "…" : x).ToList();
        if (metrics.Count > 0) facts.Add($"识别到量化信息：{string.Join("、", metrics)}");

        var result = new ReasoningResult
        {
            Summary = BuildSummary(sentences, metrics, hasRisk),
            Facts = facts,
            Inferences = BuildInferences(content, metrics),
            Risks = hasRisk ? ["文本包含下降、异常、风险或延迟等信号，需要结合原始数据进一步核验。"] : ["当前信息较少，结论可能受上下文缺失影响。"],
            Suggestions = ["核对关键指标的统计口径和时间范围。", "补充对照组或历史同期数据后再次分析。", "对影响最大的对象进行下钻排查。"],
            MissingInformation = ["数据来源与统计周期", "可用于比较的基准数据", "可能影响结果的外部条件"],
            Confidence = Math.Clamp(0.45 + Math.Min(sentences.Length, 4) * 0.08 + Math.Min(metrics.Count, 3) * 0.06, 0.45, 0.82)
        };
        return Task.FromResult(result);
    }

    private static string BuildSummary(string[] sentences, List<string> metrics, bool hasRisk)
    {
        if (sentences.Length == 0) return "暂未识别到可分析的信息。";
        var prefix = hasRisk ? "当前信息存在值得关注的变化或风险信号。" : "已从输入中提取主要事实与可能关联。";
        return metrics.Count > 0 ? $"{prefix} 共识别到 {metrics.Count.ToString(CultureInfo.InvariantCulture)} 项量化信息。" : prefix;
    }

    private static List<string> BuildInferences(string content, List<string> metrics)
    {
        var items = new List<string>();
        if (content.Contains("增长") && (content.Contains("下降") || content.Contains("减少")))
            items.Add("不同指标方向相反，可能存在转化效率、结构变化或统计口径差异。");
        if (metrics.Count >= 2) items.Add("多个量化指标可能存在关联，但目前不能仅凭文本确认因果关系。");
        if (items.Count == 0) items.Add("现有内容能够形成初步判断，但仍需要更多上下文验证。");
        return items;
    }

    [GeneratedRegex(@"[-+]?\d+(?:\.\d+)?\s*(?:%|％|万|亿|元|人|次|天|小时)?")]
    private static partial Regex MetricRegex();

    [GeneratedRegex("下降|减少|异常|风险|延迟|失败|亏损|投诉|流失|超时")]
    private static partial Regex RiskRegex();
}
