using System.Text.Json.Serialization;

namespace Tgs.Payment.AlfaPay;

public class AlfaPayDepositResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("data")]
    public DepositResponseData? Data { get; init; }
}
