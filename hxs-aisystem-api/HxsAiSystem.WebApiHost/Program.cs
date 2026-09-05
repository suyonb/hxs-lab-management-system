using HxsAiSystem.Application.Auth;
using HxsAiSystem.Application.Extensions;
using HxsAiSystem.Infrastructure.Extensions;
using HxsAiSystem.Persistence.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using HxsAiSystem.WebApiHost.Filters;
using HxsAiSystem.WebApiHost.Middleware;

var builder = WebApplication.CreateBuilder(args);
var localConfigPath = Path.Combine(builder.Environment.ContentRootPath, "appsettings.Local.json");
if (!File.Exists(localConfigPath))
    localConfigPath = Path.Combine(builder.Environment.ContentRootPath, "HxsAiSystem.WebApiHost", "appsettings.Local.json");
builder.Configuration
    .AddJsonFile(localConfigPath, optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).Enrich.FromLogContext().CreateLogger();
builder.Host.UseSerilog();
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey))
    throw new InvalidOperationException("Jwt:SecretKey 未配置。");

builder.Services.AddInfrastructure();
builder.Services.AddPersistence();
builder.Services.AddApplication();
builder.Services.AddScoped<AuditActionFilter>();
builder.Services.AddControllers(options => options.Filters.AddService<AuditActionFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HXS 实验室管理系统 API",
        Version = "v1",
        Description = "系统管理、数据推理及实验室管理基础接口。"
    });
    var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "输入登录接口返回的 Bearer Token。"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        }] = Array.Empty<string>()
    });
});

var app = builder.Build();

if (args.Contains("--initialize-stage4-only", StringComparer.OrdinalIgnoreCase))
{
    using var stage4Scope = app.Services.CreateScope();
    await stage4Scope.ServiceProvider.GetRequiredService<HxsAiSystem.Application.LabInventory.ILabInventoryInitializer>().InitializeAsync();
    await stage4Scope.ServiceProvider.GetRequiredService<HxsAiSystem.Application.SystemFoundation.IDatabaseDocumentationInitializer>().InitializeAsync();
    var stage4Db = stage4Scope.ServiceProvider.GetRequiredService<SqlSugar.ISqlSugarClient>();
    var stage4Report = await stage4Db.Ado.GetDataTableAsync(@"SELECT r.ROLE_CODE, m.MENU_CODE, m.MENU_NAME
        FROM HXS_SYS_ROLE_MENU rm
        JOIN HXS_SYS_ROLE r ON r.ID=rm.ROLE_ID
        JOIN HXS_SYS_MENU m ON m.ID=rm.MENU_ID
        WHERE m.MENU_CODE IN ('lab:approval-group','lab:requisition-approvals')
        ORDER BY r.ROLE_CODE,m.MENU_CODE");
    foreach (System.Data.DataRow row in stage4Report.Rows)
        Console.WriteLine($"{row["ROLE_CODE"]}: {row["MENU_CODE"]} - {row["MENU_NAME"]}");
    Console.WriteLine("阶段4数据库、菜单和权限初始化完成。");
    return;
}

if (args.Contains("--initialize-stage5-only", StringComparer.OrdinalIgnoreCase))
{
    using var stage5Scope = app.Services.CreateScope();
    await stage5Scope.ServiceProvider.GetRequiredService<HxsAiSystem.Application.LabExperiment.ILabExperimentInitializer>().InitializeAsync();
    await stage5Scope.ServiceProvider.GetRequiredService<HxsAiSystem.Application.SystemFoundation.IDatabaseDocumentationInitializer>().InitializeAsync();
    Console.WriteLine("阶段5数据库、菜单、权限和注释初始化完成。");
    return;
}

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<HxsAiSystem.Application.SystemFoundation.ISystemFoundationInitializer>().InitializeAsync();
    await scope.ServiceProvider.GetRequiredService<HxsAiSystem.Application.LabFoundation.ILabFoundationInitializer>().InitializeAsync();
    await scope.ServiceProvider.GetRequiredService<HxsAiSystem.Application.LabInstrument.ILabInstrumentInitializer>().InitializeAsync();
    await scope.ServiceProvider.GetRequiredService<HxsAiSystem.Application.LabInventory.ILabInventoryInitializer>().InitializeAsync();
    await scope.ServiceProvider.GetRequiredService<HxsAiSystem.Application.LabExperiment.ILabExperimentInitializer>().InitializeAsync();
    await scope.ServiceProvider.GetRequiredService<HxsAiSystem.Application.LabOperations.ILabOperationsInitializer>().InitializeAsync();
    await scope.ServiceProvider.GetRequiredService<HxsAiSystem.Application.LabVisualization.ILabVisualizationInitializer>().InitializeAsync();
    await scope.ServiceProvider.GetRequiredService<HxsAiSystem.Application.AiReasoning.IAiSchemaInitializer>().InitializeAsync();
    await scope.ServiceProvider.GetRequiredService<HxsAiSystem.Application.SystemFoundation.IDatabaseDocumentationInitializer>().InitializeAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseSerilogRequestLogging();
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
