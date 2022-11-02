using System.Text.Json.Serialization;

namespace Tgs.Payment.AlfaPay;

public class DepositResponseData
{
    [JsonPropertyName("trade_no")]
    public string TradeNo { get; init; } = default!;

    [JsonPropertyName("out_trade_no")]
    public string OutTradeNo { get; init; } = default!;

    [JsonPropertyName("amount")]
    public string Amount { get; init; } = default!;

    [JsonPropertyName("uri")]
    public string Uri { get; init; } = default!;
}
