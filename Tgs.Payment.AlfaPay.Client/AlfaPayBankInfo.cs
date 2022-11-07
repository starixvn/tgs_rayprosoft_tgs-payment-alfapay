using System.Text.Json.Serialization;

namespace Tgs.Payment.AlfaPay;

public class AlfaPayBankInfo
{
    [JsonPropertyName("phone")]
    public string AccountNumber { get; init; } = default!;

    [JsonPropertyName("username")]
    public string BankOwner { get; init; } = default!;
}
