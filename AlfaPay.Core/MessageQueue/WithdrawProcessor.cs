using System.Text.Json;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Tgs.MessageQueue;
using Tgs.Payment.Protos;
using Tgs.Payment.AlfaPay;

namespace Tgs.Payment.AlfaPay.MessageQueue;

internal class WithdrawProcessor : IMessageProcessor
{
    private readonly AlfaPayClient _alfaPayClient;
    private readonly AlfaPayOptions _options;


    public WithdrawProcessor(
        AlfaPayClient alfaPayClient,
        IOptions<AlfaPayOptions> optionsAccessor)
    {
        _alfaPayClient = alfaPayClient ?? throw new ArgumentNullException(nameof(alfaPayClient));
        _options = optionsAccessor.Value;
    }

    public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var request = WithdrawRequest.Parser.ParseFrom(data.Span);
        var settings = JsonSerializer.Deserialize<AlfaPayWithdrawSettings>(request.ApplyInfo)!;

        var order = new WithdrawOrder
        {
            AccountNumber = settings.AccountNumber,
            Amount = decimal.Parse(request.Amount),
            BankId = settings.BankId,
            BankOwner = settings.BankOwner,
            CallbackUrl = $"{_options.CallbackBaseUrl}/withdraw/callback",
            OutTradeNo = settings.OrderNo.ToString(),
        };

        var withdrawResponse = await _alfaPayClient
            .WithdrawAsync(settings.ApiToken, settings.NotifyToken, order)
            .ConfigureAwait(false);

        if (withdrawResponse != null && withdrawResponse.Success == true)
        {
            var applyInfo = new AlfaPayWithdrawApplyInfo(settings, withdrawResponse);

            var msg = new WithdrawResponse
            {
                Result = JsonSerializer.Serialize(applyInfo)
            };

            return msg.ToByteArray();
        }
        else
        {
            var response = new AlfaPayWithdrawFailResponse
            {
            };

            var applyInfo = new AlfaPayWithdrawApplyInfo(settings, response);

            var msg = new WithdrawResponse
            {
                Result = JsonSerializer.Serialize(applyInfo)
            };

            return msg.ToByteArray();
        }
    }
}
