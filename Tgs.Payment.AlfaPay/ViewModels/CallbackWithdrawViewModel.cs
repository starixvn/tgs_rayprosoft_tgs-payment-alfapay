using System.Text.Json.Serialization;

namespace Tgs.Payment.AlfaPay.ViewModels;

public class CallbackWithdrawViewModel
{
    [JsonPropertyName("trade_no")]
    public string TradeNo { get; init; } = default!;

    [JsonPropertyName("amount")]
    public string Amount { get; init; } = default!;

    [JsonPropertyName("out_trade_no")]
    public string OutTradeNo { get; init; } = default!;

    [JsonPropertyName("state")]
    public string State { get; init; } = default!;

    [JsonPropertyName("sign")]
    public string Sign { get; init; } = default!;
}
