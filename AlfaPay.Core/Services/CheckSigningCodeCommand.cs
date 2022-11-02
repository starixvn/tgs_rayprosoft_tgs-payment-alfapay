namespace Tgs.Payment.AlfaPay.Services;

public record CheckSigningCodeCommand(
    IDictionary<string, object> Data,
    string SigningCode,
    string ApiToken,
    string? NotifyToken)
{
    public TaskCompletionSource<bool> CommandResultSource { get; } = new TaskCompletionSource<bool>();
};
