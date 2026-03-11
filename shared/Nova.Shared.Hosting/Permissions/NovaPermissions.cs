namespace Nova.Shared.Hosting.Permissions;

public static class NovaPermissions
{
    public const string UserAdd = "system:user:add";
    public const string UserGet = "system:user:get";
    public const string UserList = "system:user:list";
    public const string UserDelete = "system:user:delete";

    public const string TenantAdd = "system:tenant:add";
    public const string TenantGet = "system:tenant:get";
    public const string TenantList = "system:tenant:list";
    public const string TenantSetConnection = "system:tenant:set-connection";
    public const string TenantDelete = "system:tenant:delete";

    public const string RoleAdd = "system:role:add";
    public const string RoleGet = "system:role:get";
    public const string RoleList = "system:role:list";
    public const string RoleGrantMenu = "system:role:grant-menu";

    public const string MenuAdd = "system:menu:add";
    public const string MenuList = "system:menu:list";

    public const string DictAdd = "system:dict:add";
    public const string DictGet = "system:dict:get";
    public const string DictList = "system:dict:list";
    public const string DictEdit = "system:dict:edit";
    public const string DictDelete = "system:dict:delete";

    public const string ConfigAdd = "system:config:add";
    public const string ConfigGet = "system:config:get";
    public const string ConfigList = "system:config:list";
    public const string ConfigEdit = "system:config:edit";
    public const string ConfigDelete = "system:config:delete";

    public const string NoticeAdd = "system:notice:add";
    public const string NoticeGet = "system:notice:get";
    public const string NoticeList = "system:notice:list";
    public const string NoticeEdit = "system:notice:edit";
    public const string NoticeDelete = "system:notice:delete";

    public const string MonitorLoginLogList = "monitor:loginlog:list";
    public const string MonitorOperLogList = "monitor:operlog:list";
}
