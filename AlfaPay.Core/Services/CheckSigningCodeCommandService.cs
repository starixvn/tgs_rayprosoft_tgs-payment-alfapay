using Tgs.ServiceUtility;

namespace Tgs.Payment.AlfaPay.Services;

internal class CheckSigningCodeCommandService : ICommandService<CheckSigningCodeCommand>
{
    public CheckSigningCodeCommandService()
    {
    }

    public ValueTask ExecuteAsync(CheckSigningCodeCommand command, CancellationToken cancellationToken = default)
    {
        var signCode = AlfaPayHasher.ComputeHash(command.ApiToken, command.NotifyToken, command.Data);

        command.CommandResultSource.SetResult(signCode == command.SigningCode);

        return ValueTask.CompletedTask;
    }
}
