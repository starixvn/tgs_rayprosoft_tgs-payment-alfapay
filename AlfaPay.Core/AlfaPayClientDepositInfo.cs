using System.Text.Json.Serialization;

namespace Tgs.Payment.AlfaPay;

internal class AlfaPayClientDepositInfo
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public DepositInfoData Data { get; set; } = default!;
}
