using Google.Protobuf;
using Microsoft.Extensions.Options;
using Valhalla.MessageQueue;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;
using System.Text.Json;
using Tgs.Payment.AlfaPay.Client;

namespace Tgs.Payment.AlfaPay.MessageQueue;

internal class DepositProcessor(
    IPaymentClient paymentClient,
    AlfaPayClient alfaPayClient,
    IOptions<AlfaPayOptions> optionsAccessor) : IMessageProcessor<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>
{
    private readonly IPaymentClient _paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
    private readonly AlfaPayClient _alfaPayClient = alfaPayClient ?? throw new ArgumentNullException(nameof(alfaPayClient));
    private readonly AlfaPayOptions _options = optionsAccessor.Value;

    public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(
        string subject,
        ReadOnlyMemory<byte> data,
        IEnumerable<MessageHeaderValue>? headerValues,
        CancellationToken cancellationToken = default)
    {
        var request = DepositRequest.Parser.ParseFrom(data.Span);
        var settings = JsonSerializer.Deserialize<AlfaPayDepositSettings>(
            request.ApplyInfo,
            AlfaPayUtility.SerializerOptions)!;

        var order = new DepositOrder(
            decimal.Parse(request.Amount),
            settings.FormId,
            new Uri($"{_options.CallbackBaseUrl}/deposit/callback"),
            settings.PaidName);

        var depositResponse = await _alfaPayClient
            .DepositAsync(settings.ApiToken, order, cancellationToken)
            .ConfigureAwait(false);

        var applyInfo = new AlfaPayDepositApplyInfo(
            settings,
            new AlfaPayDepositResponse(
                depositResponse.Amount,
                depositResponse.FormId,
                depositResponse.RedirectUrl,
                depositResponse.ThirdPartyOrderNo));

        await _paymentClient.ScheduleAsync(
            "tgs.payment.alfapay.deposit.schedule.reject",
            settings.FormId,
            DateTime.UtcNow.AddMinutes(15D).ToUniversalTime(),
            cancellationToken).ConfigureAwait(false);

        var msg = new DepositResponse
        {
            Result = JsonSerializer.Serialize(applyInfo, AlfaPayUtility.SerializerOptions)
        };

        return msg.ToByteArray();
    }
}
