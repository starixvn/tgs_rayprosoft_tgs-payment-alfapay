using Microsoft.Extensions.Logging;
using Tgs.MessageQueue;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;

namespace Tgs.Payment.AlfaPay.MessageQueue;

internal class DepositTimeoutHandler : IMessageHandler
{
    private readonly IPaymentServiceClient _paymentServiceClient;
    private readonly IPaymentClient _paymentClient;
    private readonly IAlfaPayClient _alfaPayClient;
    private readonly AlfaPaySerializer _alfaPaySerializer;
    private readonly ILogger<DepositTimeoutHandler> _logger;

    public DepositTimeoutHandler(
        IPaymentServiceClient paymentServiceClient,
        IPaymentClient paymentClient,
        IAlfaPayClient alfaPayClient,
        AlfaPaySerializer alfaPaySerializer,
        ILogger<DepositTimeoutHandler> logger)
    {
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
        _paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
        _alfaPayClient = alfaPayClient ?? throw new ArgumentNullException(nameof(alfaPayClient));
        _alfaPaySerializer = alfaPaySerializer ?? throw new ArgumentNullException(nameof(alfaPaySerializer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var request = ScheduleRequest.Parser.ParseFrom(data.Span);
        _logger.LogInformation(
            "Deposit Schedule Handler Receive request Form Id:{request.FormId.ToGuid()}, "
                + "Schedule Time:{request.ScheduleTime.ToDateTime():yyyy/MM/dd HH:mm:ss}",
            request.FormId.ToGuid(),
            request.ScheduleTime.ToDateTime());

        var formId = request.FormId.ToGuid();

        try
        {
            var depositForm = await _paymentServiceClient
                .GetDepositFormAsync(formId, cancellationToken)
                .ConfigureAwait(false);

            if (depositForm is null)
            {
                _logger.LogWarning("Deposit Form {request.FormId.ToGuid()} not found.", request.FormId.ToGuid());
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

            var applyInfo = _alfaPaySerializer.Deserialize<AlfaPayDepositApplyInfo>(depositForm.ApplyInfo!)!;

            var result = await _alfaPayClient.QueryDepositFormAsync(
                applyInfo.Settings.ApiToken,
                formId).ConfigureAwait(false);

            if (result?.Data.State is "timeout")
                await _paymentClient
                    .RejectDepositAsync(formId, brandId, cancellationToken)
                    .ConfigureAwait(false);
            else if (result?.Data.State is "processing")
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
