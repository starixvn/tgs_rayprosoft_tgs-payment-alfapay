using System.Text.Json.Serialization;

namespace Tgs.Payment.AlfaPay;

internal record AlfaPayClientDepositInfo(
    [property: JsonPropertyName("success")]
    bool Success,
    [property: JsonPropertyName("data")]
    AlfaPayDepositInfoData Data);
