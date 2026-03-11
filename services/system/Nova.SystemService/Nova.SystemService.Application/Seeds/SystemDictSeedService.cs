using Nova.SystemService.Core.Entities;
using SqlSugar;
using Volo.Abp.DependencyInjection;

namespace Nova.SystemService.Application.Seeds;

/// <summary>
/// 系统字典种子服务
/// </summary>
public class SystemDictSeedService : ITransientDependency
{
    private readonly ISqlSugarClient _db;

    public SystemDictSeedService(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task SeedAsync()
    {
        await EnsureDictionaryDataAsync();
    }

    private async Task EnsureDictionaryDataAsync()
    {
        if (!_db.DbMaintenance.IsAnyTable("sys_dict_type", false))
            return;

        var dictTypes = new[] { ("sys_user_sex", "用户性别", "用户性别列表"), ("sys_show_hide", "菜单状态", "显示/隐藏"), ("sys_normal_disable", "系统开关", "正常/停用") };
        foreach (var (dictType, dictName, remark) in dictTypes)
        {
            var exists = await _db.Queryable<DictionaryTypeEntity>().AnyAsync(x => x.DictType == dictType);
            if (exists) continue;

            await _db.Insertable(new DictionaryTypeEntity
            {
                DictType = dictType,
                DictName = dictName,
                Remark = remark,
                Status = true,
                OrderNum = 100,
                TenantId = null,
                CreateTime = DateTime.UtcNow
            }).ExecuteCommandAsync();
        }

        if (!_db.DbMaintenance.IsAnyTable("sys_dict_data", false))
            return;

        var dictData = new[]
        {
            ("sys_user_sex", "男", "Male", 100), ("sys_user_sex", "女", "Woman", 99), ("sys_user_sex", "未知", "Unknown", 98),
            ("sys_show_hide", "显示", "true", 100), ("sys_show_hide", "隐藏", "false", 99),
            ("sys_normal_disable", "正常", "true", 100), ("sys_normal_disable", "停用", "false", 99)
        };
        foreach (var (dictType, label, value, order) in dictData)
        {
            var exists = await _db.Queryable<DictionaryDataEntity>().AnyAsync(x => x.DictType == dictType && x.DictValue == value);
            if (exists) continue;

            await _db.Insertable(new DictionaryDataEntity
            {
                DictType = dictType,
                DictLabel = label,
                DictValue = value,
                OrderNum = order,
                Status = true,
                TenantId = null,
                CreateTime = DateTime.UtcNow
            }).ExecuteCommandAsync();
        }
    }
}
