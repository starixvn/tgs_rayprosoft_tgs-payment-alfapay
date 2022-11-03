using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.AlfaPay.ApplicationServices;

internal class GetDepositFormQueryService : IGetService<GetDepositFormQuery, DepositForm?>
{
    private readonly IPaymentServiceClient _paymentServiceClient;
    private readonly AlfaPaySerializer _alfaPaySerializer;

    public GetDepositFormQueryService(
        IPaymentServiceClient paymentServiceClient,
        AlfaPaySerializer alfaPaySerializer)
    {
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
        _alfaPaySerializer = alfaPaySerializer;
    }

    public async ValueTask<DepositForm?> GetAsync(GetDepositFormQuery query, CancellationToken cancellationToken = default)
    {
        var formData = await _paymentServiceClient
            .GetDepositFormAsync(query.FormId, cancellationToken)
            .ConfigureAwait(false);

        return formData == null
            ? null
            : new DepositForm(
                formData.Id,
                formData.PlayerCode,
                formData.BrandId,
                formData.BrandPaymentId,
                formData.Amount,
                formData.Status,
                string.IsNullOrEmpty(formData.ApplyInfo)
                    ? null
                    : _alfaPaySerializer.Deserialize<AlfaPayDepositApplyInfo>(formData.ApplyInfo));
    }
}
