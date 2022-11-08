using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tgs.Payment.AlfaPay;

public class AlfaPayWithdrawFailResponse : AlfaPayWithdrawResponse
{
    [JsonPropertyName("status_code")]
    public int StatusCode { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = default!;

    [JsonPropertyName("errors")]
    public JsonDocument Errors { get; init; } = default!;
}
