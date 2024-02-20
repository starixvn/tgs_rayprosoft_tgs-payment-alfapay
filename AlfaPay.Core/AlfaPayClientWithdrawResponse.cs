namespace Tgs.Payment.AlfaPay;
public record AlfaPayClientWithdrawResponse(
    Guid FormId,
    string ThirdPartyOrderNo,
    decimal Amount);
