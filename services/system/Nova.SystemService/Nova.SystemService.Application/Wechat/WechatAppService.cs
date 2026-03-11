using System.Security;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Nova.SystemService.Core.Dtos.Wechat;
using Nova.SystemService.Core.Entities;
using Nova.SystemService.Core.IServices;
using Nova.SystemService.Core.Wechat;
using SKIT.FlurlHttpClient.Wechat.Api;
using SKIT.FlurlHttpClient.Wechat.Api.Models;
using SKIT.FlurlHttpClient.Wechat.TenpayV3;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Events;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Models;
using SqlSugar;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace Nova.SystemService.Application.Wechat;

public class WechatAppService : ApplicationService, IWechatAppService
{
    private readonly WechatOptions _options;
    private readonly ISqlSugarClient _db;

    public WechatAppService(IOptions<WechatOptions> options, ISqlSugarClient db)
    {
        _options = options.Value;
        _db = db;
    }

    /// <summary>
    /// 获取公众号令牌
    /// </summary>
    /// <returns>公众号 access_token</returns>
    public async Task<string> GetMpTokenAsync()
    {
        if (string.IsNullOrWhiteSpace(_options.OfficialAccount.AppId) || string.IsNullOrWhiteSpace(_options.OfficialAccount.AppSecret))
            throw new UserFriendlyException("未配置微信公众号参数");

        var client = CreateApiClient(_options.OfficialAccount.AppId, _options.OfficialAccount.AppSecret);
        var response = await client.ExecuteCgibinTokenAsync(new CgibinTokenRequest
        {
            GrantType = "client_credential"
        });

        if (!response.IsSuccessful())
            throw new UserFriendlyException($"获取公众号 access_token 失败：{response.ErrorCode}-{response.ErrorMessage}");

        return response.AccessToken;
    }

    /// <summary>
    /// 发送模板消息
    /// </summary>
    /// <param name="input">模板消息参数</param>
    /// <returns>微信消息 ID</returns>
    public async Task<long?> SendTemplateAsync(SendOfficialAccountTemplateMessageInput input)
    {
        if (string.IsNullOrWhiteSpace(_options.OfficialAccount.AppId) || string.IsNullOrWhiteSpace(_options.OfficialAccount.AppSecret))
            throw new UserFriendlyException("未配置微信公众号参数");

        var client = CreateApiClient(_options.OfficialAccount.AppId, _options.OfficialAccount.AppSecret);
        var request = new CgibinMessageTemplateSendRequest
        {
            AccessToken = input.AccessToken,
            TemplateId = input.TemplateId,
            ToUserOpenId = input.ToUserOpenId,
            Url = input.Url,
            Data = new Dictionary<string, CgibinMessageTemplateSendRequest.Types.DataItem>()
        };

        foreach (var pair in input.Data)
        {
            request.Data[pair.Key] = new CgibinMessageTemplateSendRequest.Types.DataItem
            {
                Value = pair.Value.Value
            };
        }

        var response = await client.ExecuteCgibinMessageTemplateSendAsync(request);
        if (!response.IsSuccessful())
            throw new UserFriendlyException($"发送模板消息失败：{response.ErrorCode}-{response.ErrorMessage}");

        return response.MessageId;
    }

    /// <summary>
    /// 发送客服消息
    /// </summary>
    /// <param name="input">客服文本消息参数</param>
    public async Task SendKfTextAsync(SendOfficialAccountCustomMessageInput input)
    {
        if (string.IsNullOrWhiteSpace(_options.OfficialAccount.AppId) || string.IsNullOrWhiteSpace(_options.OfficialAccount.AppSecret))
            throw new UserFriendlyException("未配置微信公众号参数");

        var client = CreateApiClient(_options.OfficialAccount.AppId, _options.OfficialAccount.AppSecret);
        var response = await client.ExecuteCgibinMessageCustomSendAsync(new CgibinMessageCustomSendRequest
        {
            AccessToken = input.AccessToken,
            ToUserOpenId = input.ToUserOpenId,
            MessageType = "text",
            MessageContentForText = new CgibinMessageCustomSendRequest.Types.TextMessage
            {
                Content = input.Content
            }
        });

        if (!response.IsSuccessful())
            throw new UserFriendlyException($"发送客服消息失败：{response.ErrorCode}-{response.ErrorMessage}");
    }

    /// <summary>
    /// JSAPI 下单
    /// </summary>
    /// <param name="input">支付下单参数</param>
    /// <returns>预支付信息与前端调起参数</returns>
    public async Task<WechatPayPrepayDto> JsapiPayAsync(CreateWechatJsapiPayInput input)
    {
        var client = CreateTenpayClient();
        var request = new CreatePayTransactionJsapiRequest
        {
            AppId = input.AppId,
            OutTradeNumber = input.OutTradeNumber,
            Description = input.Description,
            NotifyUrl = _options.Pay.NotifyUrl,
            Amount = new CreatePayTransactionJsapiRequest.Types.Amount
            {
                Total = input.Total
            },
            Payer = new CreatePayTransactionJsapiRequest.Types.Payer
            {
                OpenId = input.OpenId
            }
        };

        var response = await client.ExecuteCreatePayTransactionJsapiAsync(request);
        if (!response.IsSuccessful())
            throw new UserFriendlyException($"创建微信支付订单失败：{response.ErrorCode}-{response.ErrorMessage}");

        var order = await _db.Queryable<WechatPayOrderEntity>()
            .Where(x => !x.IsDeleted && x.OutTradeNumber == input.OutTradeNumber)
            .FirstAsync();
        if (order == null)
        {
            await _db.Insertable(new WechatPayOrderEntity
            {
                AppId = input.AppId,
                OpenId = input.OpenId,
                OutTradeNumber = input.OutTradeNumber,
                Description = input.Description,
                Total = input.Total,
                TradeState = "NOTPAY"
            }).ExecuteCommandAsync();
        }

        return new WechatPayPrepayDto
        {
            PrepayId = response.PrepayId,
            PayParameters = client.GenerateParametersForJsapiPayRequest(input.AppId, response.PrepayId)
        };
    }

    /// <summary>
    /// 处理支付回调
    /// </summary>
    /// <param name="headers">微信支付回调请求头</param>
    /// <param name="body">微信支付回调原始报文</param>
    /// <returns>回调处理结果</returns>
    public async Task<WechatPayCallbackResultDto> NotifyPayAsync(WechatPayCallbackHeaders headers, string body)
    {
        var client = CreateTenpayClient();
        if (!client.VerifyEventSignature(headers.Timestamp, headers.Nonce, body, headers.Signature, headers.SerialNumber))
            throw new SecurityException("微信支付回调验签失败");

        var webhookEvent = client.DeserializeEvent(body);
        if (!string.Equals(webhookEvent.EventType, "TRANSACTION.SUCCESS", StringComparison.OrdinalIgnoreCase))
        {
            return new WechatPayCallbackResultDto { Success = true, EventType = webhookEvent.EventType };
        }

        var resource = client.DecryptEventResource<TransactionResource>(webhookEvent);
        var order = await _db.Queryable<WechatPayOrderEntity>()
            .Where(x => !x.IsDeleted && x.OutTradeNumber == resource.OutTradeNumber)
            .FirstAsync();
        if (order != null)
        {
            order.WechatTransactionId = resource.TransactionId;
            order.TradeState = resource.TradeState;
            order.PaidTime = resource.SuccessTime.UtcDateTime;
            order.NotifyRaw = body;
            order.UpdateTime = DateTime.UtcNow;
            await _db.Updateable(order).ExecuteCommandAsync();
        }

        return new WechatPayCallbackResultDto
        {
            Success = true,
            EventType = webhookEvent.EventType,
            OutTradeNumber = resource.OutTradeNumber,
            TransactionId = resource.TransactionId
        };
    }

    /// <summary>
    /// 校验公众号回调地址
    /// </summary>
    /// <param name="query">公众号回调查询参数</param>
    /// <returns>微信要求返回的 echostr</returns>
    public string VerifyMpAsync(WechatOfficialAccountCallbackQuery query)
    {
        var client = CreateOfficialAccountClient();
        if (!client.VerifyEventSignatureForEcho(query.Timestamp, query.Nonce, query.Signature))
            throw new SecurityException("微信公众号接入校验失败");

        return query.EchoStr ?? string.Empty;
    }

    /// <summary>
    /// 处理公众号消息回调
    /// </summary>
    /// <param name="query">公众号回调查询参数</param>
    /// <param name="body">公众号回调原始报文</param>
    /// <returns>公众号回包结果</returns>
    public async Task<WechatOfficialAccountMessageResultDto> NotifyMpAsync(WechatOfficialAccountCallbackQuery query, string body)
    {
        var client = CreateOfficialAccountClient();
        if (!client.VerifyEventSignatureFromXml(body, query.Timestamp, query.Nonce, query.MsgSignature ?? query.Signature))
            throw new SecurityException("微信公众号消息验签失败");

        var xml = XDocument.Parse(body);
        var root = xml.Root ?? throw new UserFriendlyException("无效的微信消息体");
        var fromUser = root.Element("FromUserName")?.Value;
        var toUser = root.Element("ToUserName")?.Value;
        var messageType = root.Element("MsgType")?.Value;
        var content = root.Element("Content")?.Value;

        if (!string.IsNullOrWhiteSpace(fromUser) && _options.OfficialAccount.ForwardToCustomService && !string.IsNullOrWhiteSpace(content))
        {
            var accessToken = await GetMpTokenAsync();
            await SendKfTextAsync(new SendOfficialAccountCustomMessageInput
            {
                AccessToken = accessToken,
                ToUserOpenId = fromUser,
                Content = content
            });
            return new WechatOfficialAccountMessageResultDto
            {
                Success = true,
                ResponseXml = "success",
                FromUserName = fromUser,
                ToUserName = toUser,
                MessageType = messageType,
                Content = content
            };
        }

        var reply = BuildTextReplyXml(fromUser, toUser, _options.OfficialAccount.DefaultReplyMessage);
        return new WechatOfficialAccountMessageResultDto
        {
            Success = true,
            ResponseXml = reply,
            FromUserName = fromUser,
            ToUserName = toUser,
            MessageType = messageType,
            Content = content
        };
    }

    private static WechatApiClient CreateApiClient(string appId, string appSecret)
    {
        return WechatApiClientBuilder.Create(new WechatApiClientOptions
        {
            AppId = appId,
            AppSecret = appSecret
        }).Build();
    }

    private WechatApiClient CreateOfficialAccountClient()
    {
        if (string.IsNullOrWhiteSpace(_options.OfficialAccount.AppId) || string.IsNullOrWhiteSpace(_options.OfficialAccount.AppSecret))
            throw new UserFriendlyException("未配置微信公众号参数");

        return WechatApiClientBuilder.Create(new WechatApiClientOptions
        {
            AppId = _options.OfficialAccount.AppId,
            AppSecret = _options.OfficialAccount.AppSecret,
            PushToken = _options.OfficialAccount.Token,
            PushEncodingAESKey = _options.OfficialAccount.EncodingAesKey
        }).Build();
    }

    private static string BuildTextReplyXml(string? fromUser, string? toUser, string content)
    {
        var safeContent = SecurityElement.Escape(content) ?? string.Empty;
        return $"<xml><ToUserName><![CDATA[{fromUser ?? string.Empty}]]></ToUserName><FromUserName><![CDATA[{toUser ?? string.Empty}]]></FromUserName><CreateTime>{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[{safeContent}]]></Content></xml>";
    }

    private WechatTenpayClient CreateTenpayClient()
    {
        if (string.IsNullOrWhiteSpace(_options.Pay.MerchantId)
            || string.IsNullOrWhiteSpace(_options.Pay.MerchantV3Secret)
            || string.IsNullOrWhiteSpace(_options.Pay.MerchantCertificateSerialNumber)
            || string.IsNullOrWhiteSpace(_options.Pay.MerchantCertificatePrivateKey)
            || string.IsNullOrWhiteSpace(_options.Pay.PlatformCertificateSerialNumber)
            || string.IsNullOrWhiteSpace(_options.Pay.PlatformCertificatePublicKey))
        {
            throw new UserFriendlyException("未配置微信支付参数");
        }

        var options = new WechatTenpayClientOptions
        {
            MerchantId = _options.Pay.MerchantId,
            MerchantV3Secret = _options.Pay.MerchantV3Secret,
            MerchantCertificateSerialNumber = _options.Pay.MerchantCertificateSerialNumber,
            MerchantCertificatePrivateKey = _options.Pay.MerchantCertificatePrivateKey
        };
        options.PlatformAuthScheme = SKIT.FlurlHttpClient.Wechat.TenpayV3.Settings.PlatformAuthScheme.PublicKey;
        var publicKeyManager = new SKIT.FlurlHttpClient.Wechat.TenpayV3.Settings.InMemoryPublicKeyManager();
        publicKeyManager.AddEntry(new SKIT.FlurlHttpClient.Wechat.TenpayV3.Settings.PublicKeyEntry(
            "RSA",
            _options.Pay.PlatformCertificateSerialNumber,
            _options.Pay.PlatformCertificatePublicKey));
        options.PlatformPublicKeyManager = publicKeyManager;

        return new WechatTenpayClient(options);
    }
}
