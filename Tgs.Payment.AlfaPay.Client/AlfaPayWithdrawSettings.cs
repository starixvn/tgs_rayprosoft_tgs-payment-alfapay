namespace Tgs.Payment.AlfaPay;

public record AlfaPayWithdrawSettings(
    string ApiToken,
    string NotifyToken,
    string AccountNumber,
    string BankId,
    string BankOwner,
    Guid FormId);
