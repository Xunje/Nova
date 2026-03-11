using Nova.AllInOne.Host;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, config) =>
        config.ReadFrom.Configuration(context.Configuration));

    await builder.AddApplicationAsync<AllInOneHostModule>();

    var app = builder.Build();

    await app.InitializeApplicationAsync();
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Nova.AllInOne.Host terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
