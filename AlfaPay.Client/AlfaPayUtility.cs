using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tgs.Payment.AlfaPay.Client;

public static class AlfaPayUtility
{
    public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        Converters = {
        }
    };
}
