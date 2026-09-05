using HxsAiSystem.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HxsAiSystem.Infrastructure.Extensions;

/// <summary>基础设施层服务注册。</summary>
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddOptions<DatabaseOptions>().BindConfiguration(DatabaseOptions.SectionName);
        return services;
    }
}
