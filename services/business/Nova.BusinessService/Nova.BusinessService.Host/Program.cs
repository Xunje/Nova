using Nova.BusinessService.Host;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, config) =>
        config.ReadFrom.Configuration(context.Configuration));

    await builder.AddApplicationAsync<BusinessServiceHostModule>();

    var app = builder.Build();

    await app.InitializeApplicationAsync();
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "BusinessService terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
