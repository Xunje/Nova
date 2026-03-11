using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Nova.Shared.Hosting.Results;

namespace Nova.Shared.Hosting.Filters;

/// <summary>
/// 全局异常过滤器
/// <para>捕获所有未处理的异常，返回统一格式的错误响应</para>
/// </summary>
public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 异常处理方法
    /// <para>记录异常日志并返回统一格式的错误响应</para>
    /// </summary>
    public void OnException(ExceptionContext context)
    {
        // 记录异常日志
        _logger.LogError(context.Exception, "Unhandled exception: {Message}", context.Exception.Message);

        // 返回统一格式的错误响应
        var result = ApiResult<object>.Fail(context.Exception.Message);
        context.Result = new ObjectResult(result) { StatusCode = 500 };
        context.ExceptionHandled = true;
    }
}
