namespace Tgs.Payment.AlfaPay.Client;

public record AlfaPayDepositSettings(
    string ApiToken,
    string NotifyToken,
    Guid FormId,
    string? PaidName);
