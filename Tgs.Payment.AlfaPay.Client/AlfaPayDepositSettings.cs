namespace Tgs.Payment.AlfaPay;

public record AlfaPayDepositSettings(
    string ApiToken,
    Guid OrderNo,
    string? PaidName);
