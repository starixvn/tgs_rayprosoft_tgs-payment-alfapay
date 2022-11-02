using System.Text.Json.Serialization;

namespace Tgs.Payment.AlfaPay;

public class DepositOrder
{
    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("callback_url")]
    public string CallbackUrl { get; init; } = default!;

    [JsonPropertyName("out_trade_no")]
    public string OutTradeNo { get; init; } = default!;

    [JsonPropertyName("paid_name")]
    public string? PaidName { get; set; }
}
