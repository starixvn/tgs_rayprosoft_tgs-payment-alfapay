using System.Text.Json;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;
using Tgs.Payment.AlfaPay.Services;

namespace Tgs.Payment.AlfaPay.Services
{
    internal class SaveWithdrawReturnCommandService : ICommandService<SaveWithdrawReturnCommand>
    {
        private readonly IPaymentServiceClient _paymentServiceClient;

        public SaveWithdrawReturnCommandService(IPaymentServiceClient paymentServiceClient)
        {
            _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
        }

        public async ValueTask ExecuteAsync(SaveWithdrawReturnCommand command, CancellationToken cancellationToken = default)
        {
            var formData = await _paymentServiceClient
                .GetWithdrawFormAsync(command.FormId)
                .ConfigureAwait(false);

            if (formData == null)
                return;

            await _paymentServiceClient
                .SubmitWithdrawFormStatusAsync(
                    formData.Id,
                    command.State,
                    JsonSerializer.Serialize(command.WithdrawReturn),
                    command.ActualAmount)
                .ConfigureAwait(false);
        }
    }
}
