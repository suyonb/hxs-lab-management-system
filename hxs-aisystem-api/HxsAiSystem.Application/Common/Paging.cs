namespace HxsAiSystem.Application.Common;

public class PageRequest
{
    private int _pageIndex = 1;
    private int _pageSize = 20;
    public int PageIndex { get => _pageIndex; set => _pageIndex = Math.Max(1, value); }
    public int PageSize { get => _pageSize; set => _pageSize = Math.Clamp(value, 1, 200); }
}

public sealed class PageResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public int PageIndex { get; init; }
    public int PageSize { get; init; }
    public long Total { get; init; }
}
