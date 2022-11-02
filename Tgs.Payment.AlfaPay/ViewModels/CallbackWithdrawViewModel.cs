using System.Text.Json.Serialization;

namespace Tgs.Payment.AlfaPay.ViewModels;

public class CallbackWithdrawViewModel
{    
    [JsonPropertyName("trade_no")]
    public string TradeNo { get; init; } = default!;

    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }
    [JsonPropertyName("out_trade_no")]
    public string OutTradeNo { get; init; } = default!;
    
    [JsonPropertyName("state")]
    public string State { get; init; }

    [JsonPropertyName("sign")]
    public string Sign { get; init; } = default!;

    [JsonPropertyName("callback_url")]
    public string CallbackUrl { get; init; } = default!;
}