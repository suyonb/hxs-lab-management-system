namespace HxsAiSystem.Application.Auth;

public enum DataScope
{
    Self = 1,
    Laboratory = 2,
    All = 3
}

public interface IDataScopeService
{
    Task<DataScope> GetCurrentScopeAsync();
}
