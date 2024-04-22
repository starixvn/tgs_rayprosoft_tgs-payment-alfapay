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
public class DepositController(ILogger<DepositController> logger) : ControllerBase
{
    private readonly ILogger<DepositController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [Consumes("application/json")]
    [Produces(MediaTypeNames.Text.Plain)]
    [HttpPost("callback")]
    public async Task<string> DepositCallbackAsync(
       [FromServices] IGetService<GetDepositFormQuery, DepositForm?> getService,
       [FromServices] IPaymentServiceClient paymentServiceClient,
       [FromServices] IPaymentClient paymentClient,
       [FromServices] AlfaPayHasher hasher,
       [FromBody] CallbackDepositViewModel source)

    {
        _logger.LogInformation("{source}", source);

        var formId = Guid.Parse(source.OutTradeNo);

        var depositForm = await getService
            .GetAsync(new(formId), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (depositForm == null)
        {
            _logger.LogWarning("Can't found deposit form({id}).", formId);
            return "fail";
        }

        var apiToken = depositForm.ApplyInfo!.Settings.ApiToken;
        var notifyToken = depositForm.ApplyInfo!.Settings.NotifyToken;

        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(source))!;

        var sign = source.Sign;
        var hash = hasher.ComputeHash(
            apiToken,
            notifyToken,
            dict.Select(kv => new KeyValuePair<string, string?>(kv.Key, kv.Value.ToString())));

        _logger.LogInformation("Compute hash: {hash}, sign: {sign}", hash, sign);

        if (sign != hash)
        {
            _logger.LogWarning("Sign value invalid. input: {sign}, compute: {hash}", sign, hash);
            return "fail";
        }

        var exchangeRate = depositForm.ApplyInfo.Settings.ExchangeRate;

        var paymentReturn = new AlfaPayDepositReturn()
        {
            ThirdPartyOrderNo = source.TradeNo,
            OrderAmount = source.RequestAmount,
            RealAmount = decimal.Parse(source.Amount),
            FormId = formId,
            Status = source.State,
            Sign = source.Sign,
            AfterExchangeOrderAmount = source.RequestAmount / exchangeRate,
            AfterExchangeRealAmount = decimal.Parse(source.Amount) / exchangeRate,
        };

        var fsm = PaymentStateMachineFactory.CreateDepositStateMachine(depositForm.Status);

        if (paymentReturn.Status == "completed")
        {
            if (fsm.CanFire(PaymentTrigger.PaymentDepositCompleted))
            {
                await fsm.FireAsync(PaymentTrigger.PaymentDepositCompleted).ConfigureAwait(false);

                await paymentServiceClient.SubmitDepositFormStatusAsync(
                    depositForm.Id,
                    paymentReturn: JsonSerializer.Serialize(paymentReturn, AlfaPayUtility.SerializerOptions),
                    cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);

                await paymentClient.ApproveDepositAsync(
                    depositForm.Id,
                    depositForm.BrandId,
                    paymentReturn.AfterExchangeOrderAmount,
                    HttpContext.RequestAborted).ConfigureAwait(false);
            }
        }

        return "ok";
    }
}