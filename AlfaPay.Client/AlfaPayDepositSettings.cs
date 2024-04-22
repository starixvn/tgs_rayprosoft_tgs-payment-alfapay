using System.ComponentModel;

namespace Tgs.Payment.AlfaPay.Client;

public record AlfaPayDepositSettings(
    string ApiToken,
    string NotifyToken,
    Guid FormId,
    string? PaidName,
    string? BankId,
    [property: Description("匯率轉換")]
    decimal ExchangeRate);