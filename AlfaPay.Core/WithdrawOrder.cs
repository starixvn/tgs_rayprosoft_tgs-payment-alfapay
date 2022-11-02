using System.Text.Json.Serialization;

namespace Tgs.Payment.AlfaPay;

public class WithdrawOrder
{
    [JsonPropertyName("out_trade_no")]
    public string OutTradeNo { get; set; } = default!;

    [JsonPropertyName("bank_id")]
    public string BankId { get; set; } = default!;

    [JsonPropertyName("bank_owner")]
    public string BankOwner { get; set; } = default!;

    [JsonPropertyName("account_number")]
    public string AccountNumber { get; set; } = default!;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("callback_url")]
    public string CallbackUrl { get; set; } = default!;

    [JsonPropertyName("sign")]
    public string? Sign { get; set; }
}
