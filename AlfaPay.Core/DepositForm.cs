using Tgs.Payment.Clients;

namespace Tgs.Payment.AlfaPay;

public record DepositForm(
    Guid Id,
    string PlayerCode,
    Guid BrandId,
    Guid BrandPaymentId,
    decimal Amount,
    PaymentFlowState Status,
    AlfaPayDepositApplyInfo? ApplyInfo,
    string? PaymentReturn = null);
