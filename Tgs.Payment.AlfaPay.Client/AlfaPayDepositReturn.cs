using System.Text.Json.Serialization;

namespace Tgs.Payment.AlfaPay;

public class AlfaPayDepositReturn
{
    [JsonPropertyName("trade_no")]
    public string TradeNo { get; init; } = default!;

    [JsonPropertyName("request_amount")]
    public decimal RequestAmount { get; init; }

    /// <summary>
    /// 支付实际金额
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("out_trade_no")]
    public string OutTradeNo { get; init; } = default!;

    /// <summary>
    /// 状态码
    /// </summary>
    [JsonPropertyName("state")]
    public string State { get; init; } = default!;

    [JsonPropertyName("sign")]
    public string Sign { get; init; } = default!;

    [JsonPropertyName("callback_url")]
    public string CallbackUrl { get; init; } = default!;
}
