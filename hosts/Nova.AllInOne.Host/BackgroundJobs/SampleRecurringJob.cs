using Hangfire;
using Microsoft.Extensions.Logging;
using Nova.Shared.Hosting.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace Nova.AllInOne.Host.BackgroundJobs;

/// <summary>
/// 示例定时任务（参考 Yi 框架：实现 IRecurringJob 可被自动扫描注册）
/// </summary>
public class SampleRecurringJob : INovaRecurringJob, ITransientDependency
{
    private readonly ILogger<SampleRecurringJob> _logger;

    public SampleRecurringJob(ILogger<SampleRecurringJob> logger)
    {
        _logger = logger;
    }

    public string JobId => "sample-recurring";
    public string CronExpression => Cron.Minutely();

    public Task ExecuteAsync()
    {
        _logger.LogInformation("[Hangfire] SampleRecurringJob 执行于 {Time}", DateTime.UtcNow);
        return Task.CompletedTask;
    }
}
