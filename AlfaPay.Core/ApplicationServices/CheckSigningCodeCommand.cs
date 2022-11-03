namespace Tgs.Payment.AlfaPay.ApplicationServices;

public record CheckSigningCodeCommand(
    IDictionary<string, object> Data,
    string SigningCode,
    string ApiToken,
    string? NotifyToken)
{
    public TaskCompletionSource<bool> CommandResultSource { get; } = new TaskCompletionSource<bool>();
};
