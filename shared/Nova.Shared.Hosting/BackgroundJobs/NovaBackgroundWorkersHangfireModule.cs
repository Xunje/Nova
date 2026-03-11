using Hangfire;
using Hangfire.Server;
using Hangfire.Storage.MySql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Volo.Abp;
using Volo.Abp.BackgroundJobs.Hangfire;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DynamicProxy;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace Nova.Shared.Hosting.BackgroundJobs;

[DependsOn(typeof(AbpBackgroundJobsHangfireModule))]
public class NovaBackgroundWorkersHangfireModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddConventionalRegistrar(new NovaHangfireConventionalRegistrar());
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var hangfireConn = connectionString.Contains("AllowUserVariables", StringComparison.OrdinalIgnoreCase)
            ? connectionString
            : connectionString.TrimEnd(';') + ";AllowUserVariables=true";

        context.Services.AddHangfire(config =>
        {
            config.UseStorage(new MySqlStorage(hangfireConn, new MySqlStorageOptions
            {
                TablesPrefix = "hangfire_"
            }));
        });
    }

    public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
    {
        GlobalJobFilters.Filters.Add(context.ServiceProvider.GetRequiredService<UnitOfWorkHangfireFilter>());
    }

    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await NovaRecurringJobRegistry.RegisterRecurringJobsAsync(context.ServiceProvider, context.ServiceProvider.GetRequiredService<IConfiguration>());
    }
}

public interface INovaRecurringJob
{
    string JobId { get; }
    string CronExpression { get; }
    Task ExecuteAsync();
}

public sealed class NovaHangfireConventionalRegistrar : DefaultConventionalRegistrar
{
    protected override bool IsConventionalRegistrationDisabled(Type type)
    {
        return !typeof(INovaRecurringJob).IsAssignableFrom(type) ||
               base.IsConventionalRegistrationDisabled(type);
    }

    protected override List<Type> GetExposedServiceTypes(Type type)
    {
        return [typeof(INovaRecurringJob), type];
    }
}

public static class NovaRecurringJobRegistry
{
    private static readonly ConcurrentDictionary<string, Type> JobTypeMap = new();

    public static Task RegisterRecurringJobsAsync(IServiceProvider serviceProvider, IConfiguration configuration, params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = ResolveAssemblies(configuration);
        }

        var logger = serviceProvider.GetService<ILogger<NovaRecurringJobDispatcher>>();
        var jobTypes = assemblies
            .SelectMany(GetLoadableTypes)
            .Where(t => t.IsClass && !t.IsAbstract && typeof(INovaRecurringJob).IsAssignableFrom(t))
            .Distinct()
            .ToList();

        logger?.LogInformation("[Hangfire] 扫描程序集: {Assemblies}", string.Join(", ", assemblies.Select(a => a.GetName().Name)));

        foreach (var jobType in jobTypes)
        {
            try
            {
                var job = (INovaRecurringJob)ProxyHelper.UnProxy(serviceProvider.GetRequiredService(jobType));
                JobTypeMap[job.JobId] = jobType;

                Expression<Func<Task>> methodCall = () => serviceProvider
                    .GetRequiredService<NovaRecurringJobDispatcher>()
                    .ExecuteAsync(job.JobId);

                RecurringJob.AddOrUpdate(
                    job.JobId,
                    methodCall,
                    job.CronExpression,
                    new RecurringJobOptions
                    {
                        TimeZone = TimeZoneInfo.Local
                    });

                logger?.LogInformation("[Hangfire] 已注册定时任务: {JobId} ({Type})", job.JobId, jobType.Name);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "[Hangfire] 注册定时任务失败: {Type}", jobType.Name);
                throw;
            }
        }

        return Task.CompletedTask;
    }

    internal static Type? GetJobType(string jobId)
    {
        return JobTypeMap.TryGetValue(jobId, out var type) ? type : null;
    }

    private static Assembly[] ResolveAssemblies(IConfiguration configuration)
    {
        var configuredAssemblies = configuration
            .GetSection("Hangfire:ScanAssemblies")
            .Get<string[]>();

        if (configuredAssemblies is { Length: > 0 })
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && configuredAssemblies.Contains(a.GetName().Name, StringComparer.Ordinal))
                .ToArray();
        }

        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && a.GetName().Name?.StartsWith("Nova.", StringComparison.Ordinal) == true)
            .ToArray();
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null)!;
        }
    }
}

public class NovaRecurringJobDispatcher : ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public NovaRecurringJobDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(string jobId)
    {
        var jobType = NovaRecurringJobRegistry.GetJobType(jobId);
        if (jobType == null)
        {
            throw new InvalidOperationException($"未找到 JobId={jobId} 对应的定时任务类型");
        }

        var job = (INovaRecurringJob)_serviceProvider.GetRequiredService(jobType);
        await job.ExecuteAsync();
    }
}

public sealed class UnitOfWorkHangfireFilter : IServerFilter, ISingletonDependency
{
    private const string UnitOfWorkItemKey = "HangfireUnitOfWork";
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public UnitOfWorkHangfireFilter(IUnitOfWorkManager unitOfWorkManager)
    {
        _unitOfWorkManager = unitOfWorkManager;
    }

    public void OnPerforming(PerformingContext context)
    {
        var uow = _unitOfWorkManager.Begin();
        context.Items[UnitOfWorkItemKey] = uow;
    }

    public void OnPerformed(PerformedContext context)
    {
        AsyncHelper.RunSync(() => OnPerformedAsync(context));
    }

    private static async Task OnPerformedAsync(PerformedContext context)
    {
        if (!context.Items.TryGetValue(UnitOfWorkItemKey, out var obj) || obj is not IUnitOfWork uow)
        {
            return;
        }

        try
        {
            if (context.Exception == null && !uow.IsCompleted)
            {
                await uow.CompleteAsync();
            }
            else
            {
                await uow.RollbackAsync();
            }
        }
        finally
        {
            uow.Dispose();
        }
    }
}

public sealed class NovaHangfireAuthorizationFilter : Hangfire.Dashboard.IDashboardAsyncAuthorizationFilter, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string _requiredUserName;

    public NovaHangfireAuthorizationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _requiredUserName = "admin";
    }

    public Task<bool> AuthorizeAsync(Hangfire.Dashboard.DashboardContext context)
    {
        var currentUser = _serviceProvider.GetRequiredService<ICurrentUser>();
        return Task.FromResult(currentUser.IsAuthenticated && currentUser.UserName == _requiredUserName);
    }
}

public static class NovaHangfireApplicationBuilderExtensions
{
    public static IApplicationBuilder UseNovaHangfireDashboard(this IApplicationBuilder app, string path = "/hangfire")
    {
        var filter = app.ApplicationServices.GetRequiredService<NovaHangfireAuthorizationFilter>();
        app.UseHangfireDashboard(path, new Hangfire.DashboardOptions
        {
            AsyncAuthorization = [filter]
        });
        return app;
    }
}
