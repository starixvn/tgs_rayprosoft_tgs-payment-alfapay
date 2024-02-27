using Microsoft.Extensions.Logging;
using Valhalla.MessageQueue;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;
using System.Text.Json;
using Tgs.Payment.AlfaPay.Client;

namespace Tgs.Payment.AlfaPay.MessageQueue;

internal class DepositTimeoutHandler(
    IPaymentServiceClient paymentServiceClient,
    IPaymentClient paymentClient,
    AlfaPayClient alfaPayClient,
    ILogger<DepositTimeoutHandler> logger)
    : IMessageHandler<ScheduleRequest>
{
    private readonly IPaymentServiceClient _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
    private readonly IPaymentClient _paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
    private readonly AlfaPayClient _alfaPayClient = alfaPayClient ?? throw new ArgumentNullException(nameof(alfaPayClient));
    private readonly ILogger<DepositTimeoutHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async ValueTask HandleAsync(
        string subject,
        ScheduleRequest data,
        IEnumerable<MessageHeaderValue>? headerValues,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Deposit Schedule Handler Receive request Form Id:{request.FormId.ToGuid()}, "
                + "Schedule Time:{request.ScheduleTime.ToDateTime():yyyy/MM/dd HH:mm:ss}",
            data.FormId.ToGuid(),
            data.ScheduleTime.ToDateTime());

        var formId = data.FormId.ToGuid();

        try
        {
            var depositForm = await _paymentServiceClient
                .GetDepositFormAsync(formId, cancellationToken)
                .ConfigureAwait(false);

            if (depositForm is null)
            {
                _logger.LogWarning("Deposit Form {request.FormId.ToGuid()} not found.", data.FormId.ToGuid());
                return;
            }

            var brandId = depositForm.BrandId;

            _logger.LogInformation(
                "GetDepositFormAsync Form Id:{depositForm.Id}, Brand Id:{brandId}, Status:{depositForm.Status}",
                depositForm.Id,
                brandId,
                depositForm.Status);

            if (depositForm.Status >= PaymentFlowState.Reject)
                return;

            var applyInfo = JsonSerializer.Deserialize<AlfaPayDepositApplyInfo>(depositForm.ApplyInfo!, AlfaPayUtility.SerializerOptions)!;

            var result = await _alfaPayClient.QueryDepositFormAsync(
                applyInfo.Settings.ApiToken,
                formId,
                cancellationToken).ConfigureAwait(false);

            if (result?.State is "timeout")
                await _paymentClient
                    .RejectDepositAsync(formId, brandId, cancellationToken)
                    .ConfigureAwait(false);
            else if (result?.State is "processing")
                await _paymentClient.ScheduleAsync(
                    "tgs.payment.alfapay.deposit.schedule.reject",
                    formId,
                    DateTime.UtcNow.AddMinutes(10D),
                    cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Process deposit timeout error!");

            await _paymentClient.ScheduleAsync(
                "tgs.payment.alfapay.deposit.schedule.reject",
                formId,
                DateTime.UtcNow.AddMinutes(10D),
                cancellationToken).ConfigureAwait(false);
        }
    }
}