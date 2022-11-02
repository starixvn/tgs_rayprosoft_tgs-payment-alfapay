using System.Text.Json.Serialization;

namespace Tgs.Payment.AlfaPay;

public class AlfaPayWithdrawResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("data")]
    public WithdrawResponseData? Data { get; init; }
}
