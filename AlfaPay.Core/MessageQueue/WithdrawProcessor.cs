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
    IPaymentClient paymentClient) 
    : IMessageProcessor<WithdrawRequest, WithdrawResponse>
{
    private readonly AlfaPayClient _alfaPayClient = alfaPayClient ?? throw new ArgumentNullException(nameof(alfaPayClient));
    private readonly IPaymentClient _paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
    private readonly AlfaPayOptions _options = optionsAccessor.Value;

    public async ValueTask<WithdrawResponse> HandleAsync(
        string subject,
        WithdrawRequest data,
        IEnumerable<MessageHeaderValue>? headerValues,
        CancellationToken cancellationToken = default)
    {        
        var settings = JsonSerializer.Deserialize<AlfaPayWithdrawSettings>(data.ApplyInfo, AlfaPayUtility.SerializerOptions)!;

        var order = new WithdrawOrder(
            settings.FormId,
            decimal.Parse(data.Amount),
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

        return new WithdrawResponse
        {
            Result = JsonSerializer.Serialize(applyInfo, AlfaPayUtility.SerializerOptions)
        };
    }
}
