using Tgs.Payment.Clients;

namespace Tgs.Payment.AlfaPay.Services;

public record WithdrawFormInfo(
      Guid Id,
      string? UserId,
      UserType UserType,
      Guid BrandId,
      Guid? BrandPaymentId,
      decimal Amount,
      PaymentFlowState Status,
      Guid BankInfoId,
      string? BankInfo,
      AlfaPayWithdrawApplyInfo? ApplyInfo,
      string? PaymentReturn = null);
