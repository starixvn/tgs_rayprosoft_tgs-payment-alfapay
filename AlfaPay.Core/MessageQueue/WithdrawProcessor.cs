using System.Text.Json;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Tgs.MessageQueue;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;

namespace Tgs.Payment.AlfaPay.MessageQueue;

internal class WithdrawProcessor : IMessageProcessor
{
    private readonly IAlfaPayClient _alfaPayClient;
    private readonly AlfaPaySerializer _alfaPaySerializer;
    private readonly IPaymentClient _paymentClient;
    private readonly AlfaPayOptions _options;

    public WithdrawProcessor(
        IAlfaPayClient alfaPayClient,
        AlfaPaySerializer alfaPaySerializer,
        IOptions<AlfaPayOptions> optionsAccessor,
        IPaymentClient paymentClient)
    {
        _alfaPayClient = alfaPayClient ?? throw new ArgumentNullException(nameof(alfaPayClient));
        _alfaPaySerializer = alfaPaySerializer ?? throw new ArgumentNullException(nameof(alfaPaySerializer));
        _paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
        _options = optionsAccessor.Value;
    }

    public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var request = WithdrawRequest.Parser.ParseFrom(data.Span);
        var settings = _alfaPaySerializer.Deserialize<AlfaPayWithdrawSettings>(request.ApplyInfo)!;

        var order = new WithdrawOrder
        {
            AccountNumber = settings.AccountNumber,
            Amount = decimal.Parse(request.Amount),
            BankId = settings.BankId,
            BankOwner = settings.BankOwner,
            CallbackUrl = $"{_options.CallbackBaseUrl}/withdraw/callback",
            OutTradeNo = settings.FormId.ToString(),
        };

        var response = await _alfaPayClient
            .WithdrawAsync(settings.ApiToken, settings.NotifyToken, order)
            .ConfigureAwait(false);

        var jsonDocument = JsonDocument.Parse(response!);

        _ = jsonDocument.RootElement.TryGetProperty("success", out var jsonSuccess);

        var success = jsonSuccess.GetBoolean();

        if (response != null && success == true)
        {
            var withdrawResponse = JsonSerializer.Deserialize<AlfaPayWithdrawResponse>(response)!;

            var applyInfo = new AlfaPayWithdrawApplyInfo(settings, withdrawResponse);

            var thirdPartyOrderNo = applyInfo.Response.Data!.TradeNo;

            await _paymentClient
            .SaveWithdrawFormThirdPartyOrderNoAsync(
                settings.FormId,
                thirdPartyOrderNo,
                cancellationToken)
            .ConfigureAwait(false);

            var msg = new WithdrawResponse
            {
                Result = _alfaPaySerializer.Serialize(applyInfo)
            };

            return msg.ToByteArray();
        }
        else
        {
            var withdrawResponse = JsonSerializer.Deserialize<AlfaPayWithdrawFailResponse>(response!)!;

            var applyInfo = new AlfaPayWithdrawApplyInfo(settings, withdrawResponse);

            var msg = new WithdrawResponse
            {
                Result = _alfaPaySerializer.Serialize(applyInfo)
            };

            return msg.ToByteArray();
        }
    }
}
