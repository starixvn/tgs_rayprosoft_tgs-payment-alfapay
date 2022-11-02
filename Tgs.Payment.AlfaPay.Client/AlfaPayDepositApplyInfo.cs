namespace Tgs.Payment.AlfaPay;

public record AlfaPayDepositApplyInfo(
    AlfaPayDepositSettings Settings,
    AlfaPayDepositResponse Response);
