namespace Tgs.Payment.AlfaPay.Client;

public record AlfaPayDepositApplyInfo(
    AlfaPayDepositSettings Settings,
    AlfaPayDepositResponse Response);
