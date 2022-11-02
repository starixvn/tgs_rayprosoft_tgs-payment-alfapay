namespace Tgs.Payment.AlfaPay;

public record AlfaPayWithdrawApplyInfo(
    AlfaPayWithdrawSettings Settings,
    AlfaPayWithdrawResponse Response);
