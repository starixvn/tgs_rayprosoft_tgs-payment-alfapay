namespace Tgs.Payment.AlfaPay.Services;

public record ApproveDepositFormCommand(Guid FormId, Guid BrandId, decimal? ActualAmount);
