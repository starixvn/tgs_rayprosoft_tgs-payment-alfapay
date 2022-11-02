using System.Text.Json.Serialization;

namespace Tgs.Payment.AlfaPay;

public class WithdrawResponseData
{
    [JsonPropertyName("trade_no")]
    public string TradeNo { get; init; } = default!;

    [JsonPropertyName("out_trade_no")]
    public string OutTradeNo { get; init; } = default!;

    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("bank_id")]
    public string BankId { get; init; } = default!;

    [JsonPropertyName("account_number")]
    public string AccountNumber { get; init; } = default!;

    [JsonPropertyName("bank_owner")]
    public string BankOwner { get; init; } = default!;

    [JsonPropertyName("callback_url")]
    public string CallbackUrl { get; init; } = default!;

    /// <summary>
    /// 状态码<br/>
    /// new => 新订单<br/>
    /// processing => 处理中<br/>
    /// reject => 拒绝<br/>
    /// completed => 成功<br/>
    /// failed => 失败<br/>
    /// refund => 冲回
    /// </summary>
    [JsonPropertyName("state")]
    public string State { get; init; } = default!;
}
