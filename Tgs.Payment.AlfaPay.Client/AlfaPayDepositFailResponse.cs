using System.Text.Json.Serialization;

namespace Tgs.Payment.AlfaPay;

public class AlfaPayDepositFailResponse : AlfaPayDepositResponse
{
    [JsonPropertyName("status_code")]
    public int StatusCode { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = default!;

    [JsonPropertyName("errors")]
    public string[] Errors { get; set; } = Array.Empty<string>();
}
