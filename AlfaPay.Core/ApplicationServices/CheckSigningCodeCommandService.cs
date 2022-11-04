using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;

namespace Tgs.Payment.AlfaPay.ApplicationServices;

internal class CheckSigningCodeCommandService : ICommandService<CheckSigningCodeCommand>
{
    private readonly ILogger<CheckSigningCodeCommandService> _logger;

    public CheckSigningCodeCommandService(ILogger<CheckSigningCodeCommandService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ValueTask ExecuteAsync(CheckSigningCodeCommand command, CancellationToken cancellationToken = default)
    {
        var signCode = AlfaPayHasher.ComputeHash(command.ApiToken, command.NotifyToken, command.PaymentType, command.Data);
        _logger.LogInformation("AlfaPay Callback Sign:{command.SigningCode} , ComputeHash signCode:{signCode}", command.SigningCode, signCode);

        command.CommandResultSource.SetResult(signCode == command.SigningCode);

        return ValueTask.CompletedTask;
    }
}
