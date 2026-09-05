using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace HxsAiSystem.Application.AiReasoning;

public sealed class OpenAiCompatibleClient : IAiModelClient
{
    private readonly HttpClient _httpClient;
    private readonly AiModelOptions _options;

    public OpenAiCompatibleClient(HttpClient httpClient, IOptions<AiModelOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public string ProviderName => "openai-compatible";

    public async Task<ReasoningResult> AnalyzeAsync(string content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl) || string.IsNullOrWhiteSpace(_options.ApiKey) || string.IsNullOrWhiteSpace(_options.Model))
            throw new InvalidOperationException("AI 模型配置不完整。请配置 BaseUrl、ApiKey 和 Model。");

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.BaseUrl.TrimEnd('/') + "/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Content = JsonContent.Create(new
        {
            model = _options.Model,
            response_format = new { type = "json_object" },
            temperature = 0.2,
            messages = new object[]
            {
                new { role = "system", content = "你是数据分析助手。只返回 JSON，字段必须为 summary、facts、inferences、risks、suggestions、missingInformation、confidence。不要输出思维链；仅给出结论、证据、合理推测和不确定性。数组字段均为字符串数组，confidence 为 0 到 1。" },
                new { role = "user", content }
            }
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException($"模型请求失败：{(int)response.StatusCode}");

        using var document = JsonDocument.Parse(body);
        var json = document.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        if (string.IsNullOrWhiteSpace(json)) throw new InvalidOperationException("模型未返回有效内容。");
        json = json.Trim().Trim('`');
        if (json.StartsWith("json", StringComparison.OrdinalIgnoreCase)) json = json[4..].Trim();
        return JsonSerializer.Deserialize<ReasoningResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? throw new InvalidOperationException("模型结果解析失败。");
    }
}
