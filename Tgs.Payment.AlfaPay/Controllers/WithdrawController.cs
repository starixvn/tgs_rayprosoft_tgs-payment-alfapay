using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.AlfaPay.ApplicationServices;
using Tgs.Payment.AlfaPay.ViewModels;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.AlfaPay.Controllers;

[Route("[controller]")]
[ApiController]
public class WithdrawController : ControllerBase
{
    private readonly ILogger<WithdrawController> _logger;

    public WithdrawController(ILogger<WithdrawController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Consumes("application/json")]
    [Produces(MediaTypeNames.Text.Plain)]
    [HttpPost("callback")]
    public async Task<string> WithdrawCallbackAsync(
        [FromServices] IGetService<GetWithdrawFormQuery, WithdrawForm?> getWithdrawFormService,
        [FromServices] ICommandService<CheckSigningCodeCommand> checkSigningCodeService,
        [FromServices] IPaymentServiceClient paymentServiceClient,
        [FromServices] AlfaPaySerializer serializer,
        [FromServices] IPaymentClient paymentClient,
        [FromBody] Dictionary<string, object> response)
    {
        var json = JsonSerializer.Serialize(response);

        _logger.LogInformation("AlfaPay Withdraw Callback:{json}", json);

        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;

        var withdrawResponse = JsonSerializer.Deserialize<CallbackWithdrawViewModel>(json)!;

        var paymentReturn = new AlfaPayWithdrawReturn()
        {
            TradeNo = withdrawResponse.TradeNo,
            Amount = decimal.Parse(withdrawResponse.Amount),
            OutTradeNo = Guid.Parse(withdrawResponse.OutTradeNo),
            State = withdrawResponse.State,
            Sign = withdrawResponse.Sign,
            CallbackUrl = withdrawResponse.CallbackUrl
        };

        if (paymentReturn.State is not "completed")
            return "fail";

        var withdrawForm = await getWithdrawFormService
            .GetAsync(new GetWithdrawFormQuery(paymentReturn.OutTradeNo))
            .ConfigureAwait(false);

        if (withdrawForm == null)
            return "fail";

        var checkSigningCodeCommand = new CheckSigningCodeCommand(
            dict,
            withdrawResponse.Sign,
            withdrawForm!.ApplyInfo!.Settings.ApiToken,
            withdrawForm.ApplyInfo.Settings.NotifyToken,
            AlfaPayPaymentType.Withdraw);

        await checkSigningCodeService
            .ExecuteAsync(checkSigningCodeCommand)
            .ConfigureAwait(false);

        if (await checkSigningCodeCommand.CommandResultSource.Task.ConfigureAwait(false) is false)
            return "fail";

        var fsm = paymentServiceClient.CreateWithdrawStateMachine(withdrawForm.Status);

        if (paymentReturn.State == "completed")
        {
            if (fsm.CanFire(PaymentTrigger.PaymentWithdrawCompleted))
            {
                await fsm.FireAsync(PaymentTrigger.PaymentWithdrawCompleted).ConfigureAwait(false);

                await paymentServiceClient.SubmitWithdrawFormStatusAsync(
                    withdrawForm.Id,
                    paymentReturn: serializer.Serialize(paymentReturn)).ConfigureAwait(false);

                await paymentClient
                    .PaidWithdrawAsync(
                        withdrawForm.Id,
                        withdrawForm.BrandId,
                        HttpContext.RequestAborted)
                    .ConfigureAwait(false);
            }
        }

        _logger.LogInformation("callback executed");

        return "ok";
    }
}
