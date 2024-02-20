using System.Text.Json.Serialization;

namespace Tgs.Payment.AlfaPay;

public record DepositOrder(
    decimal Amount,
    Guid FormId,
    Uri CallbackUrl,
    string? PaidName);
