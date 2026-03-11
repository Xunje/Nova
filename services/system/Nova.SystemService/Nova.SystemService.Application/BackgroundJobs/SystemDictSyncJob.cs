using Hangfire;
using Microsoft.Extensions.Logging;
using Nova.Shared.Hosting.BackgroundJobs;
using Nova.SystemService.Core.IServices;
using Volo.Abp.DependencyInjection;

namespace Nova.SystemService.Application.BackgroundJobs;

public class SystemDictSyncJob : INovaRecurringJob, ITransientDependency
{
    private readonly IDictionaryDataAppService _dictionaryDataAppService;
    private readonly ILogger<SystemDictSyncJob> _logger;

    public SystemDictSyncJob(
        IDictionaryDataAppService dictionaryDataAppService,
        ILogger<SystemDictSyncJob> logger)
    {
        _dictionaryDataAppService = dictionaryDataAppService;
        _logger = logger;
    }

    public string JobId => "system-dict-sync";
    public string CronExpression => Cron.Hourly();

    public async Task ExecuteAsync()
    {
        var dictTypes = new[] { "sys_user_sex", "sys_show_hide", "sys_normal_disable" };
        foreach (var dictType in dictTypes)
        {
            try
            {
                var list = await _dictionaryDataAppService.GetByDictTypeAsync(dictType);
                _logger.LogInformation("[Hangfire] 字典 {DictType} 同步完成，共 {Count} 条", dictType, list.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Hangfire] 字典 {DictType} 同步跳过（可能不存在）", dictType);
            }
        }
    }
}
