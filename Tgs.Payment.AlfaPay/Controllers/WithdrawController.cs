using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.AlfaPay.Services;
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

    [HttpPost("callback")]
    [Produces("text/plain")]
    public async Task<string> WithdrawCallback(
        [FromServices] IGetService<GetWithdrawFormQuery, Services.WithdrawFormInfo?> getWithdrawFormService,
        [FromServices] ICommandService<CheckSigningCodeCommand> checkSigningCodeService,
        [FromServices] ICommandService<SaveWithdrawReturnCommand> saveReturnService,
        [FromBody] Dictionary<string, object> response)
    {
        var json = JsonSerializer.Serialize(response);

        _logger.LogInformation("AlfaPay Withdraw Callback:{json}", json);

        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;

        var withdrawResponse = JsonSerializer.Deserialize<CallbackWithdrawViewModel>(json)!;

        var withdrawForm = await getWithdrawFormService
            .GetAsync(new GetWithdrawFormQuery(withdrawResponse.OutTradeNo))
            .ConfigureAwait(false);

        if (withdrawForm == null)
            return "Withdraw form not found.";

        var checkSigningCodeCommand = new CheckSigningCodeCommand(
            dict,
            withdrawResponse.Sign,
            withdrawForm!.ApplyInfo!.Settings.ApiToken,
            withdrawForm.ApplyInfo.Settings.NotifyToken);

        await checkSigningCodeService
            .ExecuteAsync(checkSigningCodeCommand)
            .ConfigureAwait(false);

        if (await checkSigningCodeCommand.CommandResultSource.Task.ConfigureAwait(false))
        {
            var returnData = new AlfaPayWithdrawReturn()
            {
                TradeNo = withdrawResponse.TradeNo,
                Amount = withdrawResponse.Amount,
                OutTradeNo = withdrawResponse.OutTradeNo,
                State = withdrawResponse.State,
                Sign = withdrawResponse.Sign,
                CallbackUrl = withdrawResponse.CallbackUrl
            };

            if (withdrawResponse.State == "completed")
            {
                var formStatus = withdrawForm.Status;
                if (withdrawForm.Status == PaymentFlowState.Approved)
                    formStatus = PaymentFlowState.Paid;

                var saveWithdrawCommand = new SaveWithdrawReturnCommand(
                    withdrawForm.Id,
                    formStatus,
                    returnData,
                    returnData.Amount);

                await saveReturnService
                    .ExecuteAsync(saveWithdrawCommand)
                    .ConfigureAwait(false);
            }
            else
            {
                var saveWithdrawCommand = new SaveWithdrawReturnCommand(
                    withdrawForm.Id,
                    withdrawForm.Status,
                    returnData,
                    null);

                await saveReturnService
                    .ExecuteAsync(saveWithdrawCommand)
                    .ConfigureAwait(false);
            }

            _logger.LogInformation(JsonSerializer.Serialize(returnData));

            _logger.LogInformation("callback executed");

            return "success";
        }
        else
            throw new Exception();
    }
}
