using Nova.SystemService.Host;
using Serilog;

// 创建启动日志记录器
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // 配置Serilog日志（从appsettings.json读取配置）
    builder.Host.UseSerilog((context, config) =>
        config.ReadFrom.Configuration(context.Configuration));

    // 初始化ABP应用模块
    await builder.AddApplicationAsync<SystemServiceHostModule>();

    var app = builder.Build();

    // 初始化并启动应用
    await app.InitializeApplicationAsync();
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "SystemService terminated unexpectedly");
}
finally
{
    // 确保日志刷新
    await Log.CloseAndFlushAsync();
}
