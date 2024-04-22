using System.ComponentModel;

namespace Tgs.Payment.AlfaPay.Client;

public record AlfaPayWithdrawSettings(
    string ApiToken,
    string NotifyToken,
    string AccountNumber,
    string BankId,
    string BankOwner,
    Guid FormId,
    [property: Description("匯率轉換")]
    decimal ExchangeRate);
