using System.Text.Json;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Tgs.MessageQueue;
using Tgs.Payment.Protos;

namespace Tgs.Payment.AlfaPay.MessageQueue;

internal class DepositProcessor : IMessageProcessor
{
    private readonly AlfaPayClient _alfaPayClient;
    private readonly AlfaPaySerializer _alfaPaySerializer;
    private readonly AlfaPayOptions _options;

    public DepositProcessor(
        AlfaPayClient alfaPayClient,
        AlfaPaySerializer alfaPaySerializer,
        IOptions<AlfaPayOptions> optionsAccessor)
    {
        _alfaPayClient = alfaPayClient ?? throw new ArgumentNullException(nameof(alfaPayClient));
        _alfaPaySerializer = alfaPaySerializer ?? throw new ArgumentNullException(nameof(alfaPaySerializer));
        _options = optionsAccessor.Value;
    }

    public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var request = DepositRequest.Parser.ParseFrom(data.Span);
        var settings = _alfaPaySerializer.Deserialize<AlfaPayDepositSettings>(request.ApplyInfo)!;

        var order = new DepositOrder
        {
            PaidName = settings.PaidName,
            OutTradeNo = settings.OrderNo.ToString(),
            CallbackUrl = $"{_options.CallbackBaseUrl}/deposit/callback",
            Amount = decimal.Parse(request.Amount),
        };

        var depositResponse = await _alfaPayClient
            .DepositAsync(settings.ApiToken, order)
            .ConfigureAwait(false);

        if (depositResponse != null && depositResponse.Success == true)
        {
            var applyInfo = new AlfaPayDepositApplyInfo(settings, depositResponse);

            var msg = new DepositResponse
            {
                Result = JsonSerializer.Serialize(applyInfo)
            };

            return msg.ToByteArray();
        }
        else
        {
            var response = (AlfaPayDepositFailResponse)depositResponse!;

            var applyInfo = new AlfaPayDepositApplyInfo(settings, response);

            var msg = new DepositResponse
            {
                Result = JsonSerializer.Serialize(applyInfo)
            };

            return msg.ToByteArray();
        }
    }
}
