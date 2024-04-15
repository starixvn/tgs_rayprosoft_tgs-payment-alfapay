using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.Payment.AlfaPay.Client;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.AlfaPay.MessageQueue;

internal class DepositTimeoutHandler(
    IPaymentServiceClient paymentServiceClient,
    IPaymentClient paymentClient,
    AlfaPayClient alfaPayClient,
    IOptions<AlfaPayOptions> optionsAccessor,
    ILogger<DepositTimeoutHandler> logger)
    : IMessageHandler<ScheduleRequest>
{
    private readonly AlfaPayOptions _options = optionsAccessor.Value;

    public async ValueTask HandleAsync(
        string subject,
        ScheduleRequest data,
        IEnumerable<MessageHeaderValue>? headerValues,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Deposit Schedule Handler Receive request Form Id:{FormId}, "
                + "Schedule Time:{ScheduleTime:yyyy/MM/dd HH:mm:ss}",
            data.FormId.ToGuid(),
            data.ScheduleTime.ToDateTime());

        var formId = data.FormId.ToGuid();

        try
        {
            var depositForm = await paymentServiceClient
                .GetDepositFormAsync(formId, cancellationToken)
                .ConfigureAwait(false);

            if (depositForm is null)
            {
                logger.LogWarning("Deposit Form {FormId} not found.", data.FormId.ToGuid());
                return;
            }

            var brandId = depositForm.BrandId;

            logger.LogInformation(
                "GetDepositFormAsync Form Id:{DepositFormId}, Brand Id:{BrandId}, Status:{DepositFormStatus}",
                depositForm.Id,
                brandId,
                depositForm.Status);

            if (depositForm.Status >= PaymentFlowState.Reject)
                return;

            var applyInfo = JsonSerializer.Deserialize<AlfaPayDepositApplyInfo>(depositForm.ApplyInfo!, AlfaPayUtility.SerializerOptions)!;

            var result = await alfaPayClient.QueryDepositFormAsync(
                applyInfo.Settings.ApiToken,
                formId,
                cancellationToken).ConfigureAwait(false);

            if (result?.State is "timeout")
                await paymentClient
                    .RejectDepositAsync(formId, brandId, cancellationToken)
                    .ConfigureAwait(false);
            else if (result?.State is "processing")
                await paymentClient.ScheduleAsync(
                    string.Format(AlfaPayConsts.ScheduleRejectSubjectTemplate, _options.PaymentCode.ToLower()),
                    formId,
                    DateTime.UtcNow.AddMinutes(10D),
                    cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Process deposit timeout error!");

            await paymentClient.ScheduleAsync(
                string.Format(AlfaPayConsts.ScheduleRejectSubjectTemplate, _options.PaymentCode.ToLower()),
                formId,
                DateTime.UtcNow.AddMinutes(10D),
                cancellationToken).ConfigureAwait(false);
        }
    }
}