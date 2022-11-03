using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.AlfaPay.ApplicationServices;

internal class GetWithdrawFormQueryService : IGetService<GetWithdrawFormQuery, WithdrawForm?>
{
    private readonly IPaymentServiceClient _paymentServiceClient;
    private readonly AlfaPaySerializer _alfaPaySerializer;

    public GetWithdrawFormQueryService(
        IPaymentServiceClient paymentServiceClient,
        AlfaPaySerializer alfaPaySerializer)
    {
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
        _alfaPaySerializer = alfaPaySerializer ?? throw new ArgumentNullException(nameof(alfaPaySerializer));
    }

    public async ValueTask<WithdrawForm?> GetAsync(GetWithdrawFormQuery query, CancellationToken cancellationToken = default)
    {
        var formData = await _paymentServiceClient
            .GetWithdrawFormAsync(query.FormId)
            .ConfigureAwait(false);

        return formData == null
            ? null
            : new WithdrawForm(
                formData.Id,
                formData.UserId,
                formData.UserType,
                formData.BrandId,
                formData.BrandPaymentId,
                formData.Amount,
                formData.Status,
                formData.BankInfoId,
                formData.BankInfo,
                string.IsNullOrEmpty(formData.ApplyInfo)
                    ? null
                    : _alfaPaySerializer.Deserialize<AlfaPayWithdrawApplyInfo>(formData.ApplyInfo),
                formData.PaymentReturn);
    }
}
