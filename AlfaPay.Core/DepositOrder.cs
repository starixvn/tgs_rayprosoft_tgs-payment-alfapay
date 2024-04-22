namespace Tgs.Payment.AlfaPay;

public record DepositOrder(
    decimal Amount,
    Guid FormId,
    Uri CallbackUrl,
    string? PaidName,
    string? BankId);