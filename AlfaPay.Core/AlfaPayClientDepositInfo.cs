using System.Text.Json.Serialization;

namespace Tgs.Payment.AlfaPay;

internal class AlfaPayClientDepositInfo
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public DepositInfoData Data { get; set; } = default!;
}

public class DepositInfoData
{
    /// <summary>
    /// 平台订单号
    /// </summary>
    [JsonPropertyName("trade_no")]
    public Guid TradeNo { get; set; }

    /// <summary>
    /// 商户订单号
    /// </summary>
    [JsonPropertyName("out_trade_no")]
    public string OutTradeNo { get; set; } = default!;

    /// <summary>
    /// 实际支付金额
    /// </summary>
    [JsonPropertyName("amount")]
    public string Amount { get; set; } = default!;

    /// <summary>
    /// 请求支付金额
    /// </summary>
    [JsonPropertyName("request_amount")]
    public string RequestAmount { get; set; } = default!;

    /// <summary>
    /// 状态码<br/>
    /// new => 新订单<br/>
    /// processing => 处理中<br/>
    /// verify => 待確認<br/>
    /// reject => 拒绝<br/>
    /// completed => 成功<br/>
    /// failed => 失败<br/>
    /// refund => 冲回<br/>
    /// timeout => 逾時
    /// </summary>
    [JsonPropertyName("state")]
    public string State { get; set; } = default!;
}
