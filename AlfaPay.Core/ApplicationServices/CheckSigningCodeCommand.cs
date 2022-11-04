namespace Tgs.Payment.AlfaPay.ApplicationServices;

public record CheckSigningCodeCommand(
    IDictionary<string, object> Data,
    string SigningCode,
    string ApiToken,
    string NotifyToken,
    AlfaPayPaymentType PaymentType)
{
    public TaskCompletionSource<bool> CommandResultSource { get; } = new TaskCompletionSource<bool>();
};
