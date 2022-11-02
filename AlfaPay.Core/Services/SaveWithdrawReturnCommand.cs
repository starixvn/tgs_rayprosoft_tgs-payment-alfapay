using Tgs.Payment.Clients;

namespace Tgs.Payment.AlfaPay.Services
{
    public record SaveWithdrawReturnCommand(
        Guid FormId,
        PaymentFlowState State,
        AlfaPayWithdrawReturn WithdrawReturn,
        decimal? ActualAmount);
}
