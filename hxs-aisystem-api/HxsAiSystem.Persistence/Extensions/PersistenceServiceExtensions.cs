using HxsAiSystem.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace HxsAiSystem.Persistence.Extensions;

/// <summary>持久化层服务注册。</summary>
public static class PersistenceServiceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddScoped<ISqlSugarClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
                throw new InvalidOperationException("Database:ConnectionString 未配置。");

            var dbType = Enum.TryParse<DbType>(options.DbType, true, out var parsed)
                ? parsed : DbType.Oracle;
            return new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = options.ConnectionString,
                DbType = dbType,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });
        });
        return services;
    }
}
