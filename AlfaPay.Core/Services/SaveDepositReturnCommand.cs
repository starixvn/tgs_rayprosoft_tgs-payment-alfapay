namespace Tgs.Payment.AlfaPay.Services;

public record SaveDepositReturnCommand(Guid FormId, AlfaPayDepositReturn DepositReturn);
