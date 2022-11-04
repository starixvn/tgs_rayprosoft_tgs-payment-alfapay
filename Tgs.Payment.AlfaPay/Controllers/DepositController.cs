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
public class DepositController : ControllerBase
{
    private readonly ILogger<DepositController> _logger;

    public DepositController(ILogger<DepositController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Consumes("application/json")]
    [Produces(MediaTypeNames.Text.Plain)]
    [HttpPost("callback")]
    public async Task<string> DepositCallbackAsync(
       [FromServices] IGetService<GetDepositFormQuery, DepositForm?> getFormService,
       [FromServices] IPaymentServiceClient paymentServiceClient,
       [FromServices] AlfaPaySerializer alfaPaySerializer,
       [FromServices] IPaymentClient paymentClient,
       [FromServices] ICommandService<CheckSigningCodeCommand> checkSigningCodeService,
       [FromBody] Dictionary<string, object> response)
    {
        var json = JsonSerializer.Serialize(response);

        _logger.LogInformation("AlfaPay Deposit Callback:{json}", json);

        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;
        _ = dict.Remove("sign");

        var depositResponse = JsonSerializer.Deserialize<CallbackDepositViewModel>(json)!;

        var paymentReturn = new AlfaPayDepositReturn()
        {
            TradeNo = depositResponse.TradeNo,
            RequestAmount = depositResponse.RequestAmount,
            Amount = decimal.Parse(depositResponse.Amount),
            OutTradeNo = Guid.Parse(depositResponse.OutTradeNo),
            State = depositResponse.State,
            Sign = depositResponse.Sign,
            CallbackUrl = depositResponse.CallbackUrl,
        };

        if (paymentReturn.State is not "completed")
            return "fail";

        var depositForm = await getFormService
            .GetAsync(new GetDepositFormQuery(paymentReturn.OutTradeNo))
            .ConfigureAwait(false);

        _logger.LogInformation("Form: {depositForm}", depositForm);

        if (depositForm is null)
            return "fail";

        var checkSigningCodeCommand = new CheckSigningCodeCommand(
            dict,
            depositResponse.Sign,
            depositForm!.ApplyInfo!.Settings.ApiToken,
            null);

        await checkSigningCodeService
            .ExecuteAsync(checkSigningCodeCommand)
            .ConfigureAwait(false);

        if (await checkSigningCodeCommand.CommandResultSource.Task.ConfigureAwait(false) is false)
            return "fail";

        var fsm = paymentServiceClient.CreateDepositStateMachine(depositForm.Status);

        if (paymentReturn.State == "completed")
        {
            if (fsm.CanFire(PaymentTrigger.PaymentDepositCompleted))
            {
                await fsm.FireAsync(PaymentTrigger.PaymentDepositCompleted).ConfigureAwait(false);

                await paymentServiceClient.SubmitDepositFormStatusAsync(
                    depositForm.Id,
                    paymentReturn: alfaPaySerializer.Serialize(paymentReturn)).ConfigureAwait(false);

                await paymentClient.ApproveDepositAsync(
                    depositForm.Id,
                    depositForm.BrandId,
                    paymentReturn.Amount).ConfigureAwait(false);
            }
        }

        _logger.LogInformation("callback executed");

        return "ok";
    }
}
