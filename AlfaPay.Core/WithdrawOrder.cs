namespace Tgs.Payment.AlfaPay;

public record WithdrawOrder(
    Guid FormId,
    decimal Amount,
    Uri CallbackUrl,
    string BankId,
    string BankOwner,
    string AccountNumber);
