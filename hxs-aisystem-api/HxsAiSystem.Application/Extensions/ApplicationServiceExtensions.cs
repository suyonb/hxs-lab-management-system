using HxsAiSystem.Application.Auth;
using HxsAiSystem.Application.AiReasoning;
using HxsAiSystem.Application.SystemManagement;
using HxsAiSystem.Application.Auth.Authorization;
using Microsoft.AspNetCore.Authorization;
using HxsAiSystem.Application.Audit;
using HxsAiSystem.Application.Files;
using HxsAiSystem.Application.SystemFoundation;
using HxsAiSystem.Application.LabFoundation;
using HxsAiSystem.Application.LabInstrument;
using HxsAiSystem.Application.LabInventory;
using HxsAiSystem.Application.LabExperiment;
using HxsAiSystem.Application.LabOperations;
using HxsAiSystem.Application.LabVisualization;
using Microsoft.Extensions.DependencyInjection;

namespace HxsAiSystem.Application.Extensions;

/// <summary>应用层服务注册。</summary>
public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddOptions<JwtOptions>().BindConfiguration(JwtOptions.SectionName);
        services.AddOptions<LoginSecurityOptions>().BindConfiguration(LoginSecurityOptions.SectionName);
        services.AddHttpContextAccessor();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDataScopeService, DataScopeService>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<ISystemManagementService, SystemManagementService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddOptions<FileStorageOptions>().BindConfiguration(FileStorageOptions.SectionName);
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<ISystemFoundationInitializer, SystemFoundationInitializer>();
        services.AddScoped<IDatabaseDocumentationInitializer, DatabaseDocumentationInitializer>();
        services.AddScoped<ILabFoundationInitializer, LabFoundationInitializer>();
        services.AddScoped<ILabFoundationService, LabFoundationService>();
        services.AddScoped<ILabInstrumentInitializer, LabInstrumentInitializer>();
        services.AddScoped<ILabInstrumentService, LabInstrumentService>();
        services.AddScoped<ILabInventoryInitializer, LabInventoryInitializer>();
        services.AddScoped<ILabInventoryService, LabInventoryService>();
        services.AddScoped<ILabExperimentInitializer, LabExperimentInitializer>();
        services.AddScoped<ILabExperimentService, LabExperimentService>();
        services.AddScoped<ILabOperationsInitializer, LabOperationsInitializer>();
        services.AddScoped<ILabOperationsService, LabOperationsService>();
        services.AddScoped<ILabVisualizationInitializer, LabVisualizationInitializer>();
        services.AddScoped<ILabVisualizationService, LabVisualizationService>();
        services.AddOptions<AiModelOptions>().BindConfiguration(AiModelOptions.SectionName);
        services.AddScoped<DemoReasoningClient>();
        services.AddHttpClient<OpenAiCompatibleClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AiModelOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(Math.Clamp(options.TimeoutSeconds, 10, 180));
        });
        services.AddScoped<IAiReasoningService, AiReasoningService>();
        services.AddScoped<IAiSchemaInitializer, AiSchemaInitializer>();
        return services;
    }
}
