namespace Tgs.Payment.AlfaPay.Client;

public record AlfaPayWithdrawSettings(
    string ApiToken,
    string NotifyToken,
    string AccountNumber,
    string BankId,
    string BankOwner,
    Guid FormId);
