using System.Text.Json;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;
using Tgs.Payment.AlfaPay.Services;

namespace Tgs.Payment.AlfaPay.Services
{
    internal class SaveDepositReturnCommandService : ICommandService<SaveDepositReturnCommand>
    {
        private readonly IPaymentServiceClient _paymentServiceClient;

        public SaveDepositReturnCommandService(IPaymentServiceClient paymentServiceClient)
        {
            _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
        }

        public async ValueTask ExecuteAsync(SaveDepositReturnCommand command, CancellationToken cancellationToken = default)
        {
            var formData = await _paymentServiceClient
                .GetDepositFormAsync(command.FormId)
                .ConfigureAwait(false);

            if (formData == null)
                return;

            await _paymentServiceClient
                .SubmitDepositFormStatusAsync(
                    command.FormId,
                    formData.Status,
                    JsonSerializer.Serialize(command.DepositReturn))
                .ConfigureAwait(false);
        }
    }
}
