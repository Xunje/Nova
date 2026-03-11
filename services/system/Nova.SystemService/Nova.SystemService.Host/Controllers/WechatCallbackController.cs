using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nova.SystemService.Core.Dtos.Wechat;
using Nova.SystemService.Core.IServices;

namespace Nova.SystemService.Host.Controllers;

[ApiController]
[AllowAnonymous]
[Route("system/wechat/callback")]
public class WechatCallbackController : ControllerBase
{
    private readonly IWechatAppService _wechatAppService;

    public WechatCallbackController(IWechatAppService wechatAppService)
    {
        _wechatAppService = wechatAppService;
    }

    /// <summary>
    /// 公众号接入校验
    /// </summary>
    /// <param name="query">微信回调查询参数</param>
    /// <returns>返回微信要求的校验串</returns>
    [HttpGet("mp")]
    public ActionResult<string> VerifyMp([FromQuery] WechatOfficialAccountCallbackQuery query)
    {
        return Content(_wechatAppService.VerifyMpAsync(query), "text/plain");
    }

    /// <summary>
    /// 公众号消息回调
    /// </summary>
    /// <param name="query">微信回调查询参数</param>
    /// <returns>公众号响应 XML</returns>
    [HttpPost("mp")]
    public async Task<ActionResult<string>> NotifyMp([FromQuery] WechatOfficialAccountCallbackQuery query)
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        var result = await _wechatAppService.NotifyMpAsync(query, body);
        return Content(result.ResponseXml, "application/xml");
    }

    /// <summary>
    /// 微信支付回调
    /// </summary>
    /// <returns>微信支付回调响应</returns>
    [HttpPost("pay")]
    public async Task<IActionResult> NotifyPay()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        var headers = new WechatPayCallbackHeaders
        {
            Timestamp = Request.Headers["Wechatpay-Timestamp"].ToString(),
            Nonce = Request.Headers["Wechatpay-Nonce"].ToString(),
            Signature = Request.Headers["Wechatpay-Signature"].ToString(),
            SerialNumber = Request.Headers["Wechatpay-Serial"].ToString()
        };

        await _wechatAppService.NotifyPayAsync(headers, body);
        return Ok(new { code = "SUCCESS", message = "成功" });
    }
}
