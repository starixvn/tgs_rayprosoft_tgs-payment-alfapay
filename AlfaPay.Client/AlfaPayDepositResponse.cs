namespace Tgs.Payment.AlfaPay.Client;

public record AlfaPayDepositResponse(
    decimal Amount,
    Guid FormId,
    Uri RedirectUrl,
    string ThirdPartyOrderNo);
