using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.Payment.AlfaPay.Client;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.AlfaPay.MessageQueue;

internal class DepositProcessor(
    IPaymentClient paymentClient,
    AlfaPayClient alfaPayClient,
    IOptions<AlfaPayOptions> optionsAccessor,
    ILogger<DepositProcessor> logger)
    : IMessageProcessor<DepositRequest, DepositResponse>
{
    private readonly AlfaPayOptions _options = optionsAccessor.Value;

    public async ValueTask<DepositResponse> HandleAsync(
        string subject,
        DepositRequest data,
        IEnumerable<MessageHeaderValue>? headerValues,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("{DepositApplyInfo}", data.ApplyInfo);

        var settings = JsonSerializer.Deserialize<AlfaPayDepositSettings>(
            data.ApplyInfo,
            AlfaPayUtility.SerializerOptions)!;

        var order = new DepositOrder(
            decimal.Parse(data.Amount) * settings.ExchangeRate,
            settings.FormId,
            new Uri($"{_options.CallbackBaseUrl}/deposit/callback"),
            settings.PaidName,
            settings.BankId);

        var depositResponse = await alfaPayClient
            .DepositAsync(settings.ApiToken, order, cancellationToken)
            .ConfigureAwait(false);

        var applyInfo = new AlfaPayDepositApplyInfo(
            settings,
            new AlfaPayDepositResponse(
                depositResponse.Amount,
                depositResponse.FormId,
                depositResponse.RedirectUrl,
                depositResponse.ThirdPartyOrderNo));

        await paymentClient.ScheduleAsync(
            string.Format(AlfaPayConsts.ScheduleRejectSubjectTemplate, _options.PaymentCode.ToLower()),
            settings.FormId,
            DateTime.UtcNow.AddMinutes(60D).ToUniversalTime(),
            cancellationToken).ConfigureAwait(false);

        return new DepositResponse
        {
            Result = JsonSerializer.Serialize(applyInfo, AlfaPayUtility.SerializerOptions)
        };
    }
}