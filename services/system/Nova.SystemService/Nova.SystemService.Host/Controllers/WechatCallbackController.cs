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

    [HttpGet("official-account")]
    public ActionResult<string> VerifyOfficialAccount([FromQuery] WechatOfficialAccountCallbackQuery query)
    {
        return Content(_wechatAppService.VerifyOfficialAccountUrlAsync(query), "text/plain");
    }

    [HttpPost("official-account")]
    public async Task<ActionResult<string>> HandleOfficialAccount([FromQuery] WechatOfficialAccountCallbackQuery query)
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        var result = await _wechatAppService.HandleOfficialAccountMessageAsync(query, body);
        return Content(result.ResponseXml, "application/xml");
    }

    [HttpPost("pay")]
    public async Task<IActionResult> HandlePay()
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

        await _wechatAppService.HandlePayCallbackAsync(headers, body);
        return Ok(new { code = "SUCCESS", message = "成功" });
    }
}
