namespace Tgs.Payment.AlfaPay;

public record AlfaPayClientDepositResponse(
    decimal Amount,
    Guid FormId,
    string ThirdPartyOrderNo,
    Uri RedirectUrl);
