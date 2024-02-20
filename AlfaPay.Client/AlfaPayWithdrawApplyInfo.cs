namespace Tgs.Payment.AlfaPay.Client;

public record AlfaPayWithdrawApplyInfo(
    AlfaPayWithdrawSettings Settings,
    AlfaPayWithdrawResponse Response);
