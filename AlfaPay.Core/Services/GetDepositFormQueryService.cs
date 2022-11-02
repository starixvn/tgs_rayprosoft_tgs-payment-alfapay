using System.Text.Json;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.AlfaPay.Services;

internal class GetDepositFormQueryService : IGetService<GetDepositFormQuery, DepositFormInfo?>
{
    private readonly IPaymentServiceClient _paymentServiceClient;

    public GetDepositFormQueryService(IPaymentServiceClient paymentServiceClient)
    {
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
    }

    public async ValueTask<DepositFormInfo?> GetAsync(GetDepositFormQuery query, CancellationToken cancellationToken = default)
    {
        var formId = new Guid(query.OrderNo);

        var formData = await _paymentServiceClient
            .GetDepositFormAsync(formId)
            .ConfigureAwait(false);

        return formData == null
            ? null
            : new DepositFormInfo(
                formData.Id,
                formData.PlayerCode,
                formData.BrandId,
                formData.BrandPaymentId,
                formData.Amount,
                formData.Status,
                string.IsNullOrEmpty(formData.ApplyInfo)
                    ? null
                    : JsonSerializer.Deserialize<AlfaPayDepositApplyInfo>(formData.ApplyInfo));
    }
}
