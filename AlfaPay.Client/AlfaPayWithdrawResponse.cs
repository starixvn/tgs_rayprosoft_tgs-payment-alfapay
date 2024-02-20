namespace Tgs.Payment.AlfaPay.Client;

public record AlfaPayWithdrawResponse(
    Guid FormId,
    string ThirdPartyOrderNo);
