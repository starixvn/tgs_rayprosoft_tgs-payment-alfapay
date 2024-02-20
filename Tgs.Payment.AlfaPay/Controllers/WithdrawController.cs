using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.AlfaPay.ApplicationServices;
using Tgs.Payment.AlfaPay.Client;
using Tgs.Payment.AlfaPay.ViewModels;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.AlfaPay.Controllers;

[Route("[controller]")]
[ApiController]
public class WithdrawController(ILogger<WithdrawController> logger) : ControllerBase
{
    private readonly ILogger<WithdrawController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [Consumes("application/json")]
    [Produces(MediaTypeNames.Text.Plain)]
    [HttpPost("callback")]
    public async Task<string> WithdrawCallbackAsync(
        [FromServices] IGetService<GetWithdrawFormQuery, WithdrawForm?> getService,
        [FromServices] AlfaPayHasher hasher,
        [FromServices] IPaymentServiceClient paymentServiceClient,
        [FromServices] IPaymentClient paymentClient,
        [FromBody] CallbackWithdrawViewModel source)
    {
        _logger.LogInformation("{source}", source);

        var formId = Guid.Parse(source.OutTradeNo);

        var withdrawForm = await getService
            .GetAsync(new(formId), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (withdrawForm == null)
        {
            _logger.LogWarning("Can't found withdraw form({id}).", formId);
            return "fail";
        }

        var apiToken = withdrawForm.ApplyInfo!.Settings.ApiToken;
        var notifyToken = withdrawForm.ApplyInfo!.Settings.NotifyToken;

        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(source))!;

        var sign = source.Sign;
        var hash = hasher.ComputeHash(
            apiToken,
            notifyToken,
            dict.Where(kv => kv.Value is not null)
                .Select(kv => new KeyValuePair<string, string?>(
                kv.Key,
                kv.Value!.ToString())));

        _logger.LogInformation("Compute hash: {hash}, sign: {sign}", hash, sign);

        if (sign != hash)
        {
            _logger.LogWarning("Sign value invalid. input: {sign}, compute: {hash}", sign, hash);
            return "fail";
        }

        var paymentReturn = new AlfaPayWithdrawReturn()
        {
            FormId = formId,
            Amount = decimal.Parse(source.Amount),
            Sign = sign,
            ThirdPartyOrderNo = source.TradeNo,
            Status = source.State,
        };

        var fsm = PaymentStateMachineFactory.CreateWithdrawStateMachine(withdrawForm.Status);

        switch (paymentReturn.Status)
        {
            case "completed":
                if (fsm.CanFire(PaymentTrigger.PaymentWithdrawCompleted))
                {
                    await fsm.FireAsync(PaymentTrigger.PaymentWithdrawCompleted).ConfigureAwait(false);

                    await paymentServiceClient.SubmitWithdrawFormStatusAsync(
                        withdrawForm.Id,
                        paymentReturn: JsonSerializer.Serialize(paymentReturn, AlfaPayUtility.SerializerOptions)).ConfigureAwait(false);

                    await paymentClient
                        .PaidWithdrawAsync(
                            withdrawForm.Id,
                            withdrawForm.BrandId,
                            HttpContext.RequestAborted)
                        .ConfigureAwait(false);
                }

                break;

            case "refund":
            case "failed":
                if (fsm.CanFire(PaymentTrigger.Reject))
                {
                    await fsm.FireAsync(PaymentTrigger.Reject).ConfigureAwait(false);

                    await paymentServiceClient.SubmitWithdrawFormStatusAsync(
                        withdrawForm.Id,
                        paymentReturn: JsonSerializer.Serialize(paymentReturn, AlfaPayUtility.SerializerOptions)).ConfigureAwait(false);

                    await paymentClient
                        .RejectWithdrawAsync(withdrawForm.Id, withdrawForm.BrandId)
                        .ConfigureAwait(false);
                }

                break;
        }

        return "ok";
    }
}
