using Nova.Gateway.Middlewares;
using Serilog;

// 创建启动日志记录器
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // 配置Serilog日志
    builder.Host.UseSerilog((context, config) =>
        config.ReadFrom.Configuration(context.Configuration));

    // 配置YARP反向代理（从appsettings.json加载路由和集群配置）
    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
    builder.Services.AddMemoryCache();

    var app = builder.Build();

    // 启用请求日志记录
    app.UseSerilogRequestLogging();

    // 启用租户解析中间件（在路由之前执行）
    app.UseTenantResolver();

    // 配置聚合Swagger UI（展示所有微服务的API文档）
    app.UseSwaggerUI(options =>
    {
        var swaggerEndpoints = builder.Configuration
            .GetSection("SwaggerEndpoints")
            .Get<Dictionary<string, string>>() ?? new();

        foreach (var (name, url) in swaggerEndpoints)
        {
            options.SwaggerEndpoint(url, name);
        }

        options.RoutePrefix = "swagger";
    });

    // 映射反向代理路由
    app.MapReverseProxy();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Nova.Gateway terminated unexpectedly");
}
finally
{
    // 确保日志刷新
    await Log.CloseAndFlushAsync();
}
