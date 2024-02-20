using Google.Protobuf;
using Microsoft.Extensions.Options;
using Valhalla.MessageQueue;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;
using System.Text.Json;
using Tgs.Payment.AlfaPay.Client;

namespace Tgs.Payment.AlfaPay.MessageQueue;

internal class WithdrawProcessor(
    AlfaPayClient alfaPayClient,
    IOptions<AlfaPayOptions> optionsAccessor,
    IPaymentClient paymentClient) : IMessageProcessor<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>
{
    private readonly AlfaPayClient _alfaPayClient = alfaPayClient ?? throw new ArgumentNullException(nameof(alfaPayClient));
    private readonly IPaymentClient _paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
    private readonly AlfaPayOptions _options = optionsAccessor.Value;

    public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(
        string subject,
        ReadOnlyMemory<byte> data,
        IEnumerable<MessageHeaderValue>? headerValues,
        CancellationToken cancellationToken = default)
    {
        var request = WithdrawRequest.Parser.ParseFrom(data.Span);
        var settings = JsonSerializer.Deserialize<AlfaPayWithdrawSettings>(request.ApplyInfo, AlfaPayUtility.SerializerOptions)!;

        var order = new WithdrawOrder(
            settings.FormId,
            decimal.Parse(request.Amount),
            new Uri($"{_options.CallbackBaseUrl}/withdraw/callback"),
            settings.BankId,
            settings.BankOwner,
            settings.AccountNumber);

        var withdrawResponse = await _alfaPayClient
            .WithdrawAsync(settings.ApiToken, settings.NotifyToken, order, cancellationToken)
            .ConfigureAwait(false);

        var applyInfo = new AlfaPayWithdrawApplyInfo(
            settings,
            new AlfaPayWithdrawResponse(withdrawResponse.FormId, withdrawResponse.ThirdPartyOrderNo));

        await _paymentClient
            .SaveWithdrawFormThirdPartyOrderNoAsync(
                settings.FormId,
                withdrawResponse.ThirdPartyOrderNo,
                cancellationToken)
            .ConfigureAwait(false);

        var msg = new WithdrawResponse
        {
            Result = JsonSerializer.Serialize(applyInfo, AlfaPayUtility.SerializerOptions)
        };

        return msg.ToByteArray();
    }
}
