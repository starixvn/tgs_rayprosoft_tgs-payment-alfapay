namespace Tgs.Payment.AlfaPay;

public record AlfaPayDepositSettings(
    string ApiToken,
    string NotifyToken,
    Guid OrderNo,
    string? PaidName);
