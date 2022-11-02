using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.AlfaPay.Services;
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

    [HttpPost("callback")]
    [Produces("text/plain")]
    public async Task<string> DepositCallback(
       [FromServices] IGetService<GetDepositFormQuery, DepositFormInfo?> getDepositFormService,
       [FromServices] ICommandService<CheckSigningCodeCommand> checkSigningCodeService,
       [FromServices] ICommandService<SaveDepositReturnCommand> saveReturnService,
       [FromServices] ICommandService<ApproveDepositFormCommand> approveService,
       [FromBody] Dictionary<string, object> response)
    {
        var json = JsonSerializer.Serialize(response);

        _logger.LogInformation("AlfaPay Deposit Callback:{json}",json);

        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;
        _ = dict.Remove("sign");

        var depositResponse = JsonSerializer.Deserialize<CallbackDepositViewModel>(json)!;

        var depositForm = await getDepositFormService
            .GetAsync(new GetDepositFormQuery(depositResponse.OutTradeNo))
            .ConfigureAwait(false);

        var checkSigningCodeCommand = new CheckSigningCodeCommand(
            dict,
            depositResponse.Sign,
            depositForm!.ApplyInfo!.Settings.ApiToken,
            null);

        await checkSigningCodeService
            .ExecuteAsync(checkSigningCodeCommand)
            .ConfigureAwait(false);

        if (await checkSigningCodeCommand.CommandResultSource.Task.ConfigureAwait(false))
        {
            if (depositForm.Status < PaymentFlowState.WaitApprove && depositResponse.State == "completed")
            {
                var returnData = new AlfaPayDepositReturn()
                {
                    TradeNo = depositResponse.TradeNo,
                    RequestAmount = depositResponse.RequestAmount,
                    Amount = depositResponse.Amount,
                    OutTradeNo = depositResponse.OutTradeNo,
                    State = depositResponse.State,
                    Sign = depositResponse.Sign,
                    CallbackUrl = depositResponse.CallbackUrl,
                };

                await saveReturnService
                    .ExecuteAsync(new SaveDepositReturnCommand(depositForm.Id, returnData))
                    .ConfigureAwait(false);

                var jsonSerializerOptions = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                _logger.LogInformation(JsonSerializer.Serialize(returnData, jsonSerializerOptions));

                await approveService
                    .ExecuteAsync(new ApproveDepositFormCommand(
                        depositForm.Id,
                        depositForm.BrandId,
                        returnData.Amount))
                    .ConfigureAwait(false);
            }

            _logger.LogInformation("callback executed");

            return "success";
        }
        else
            throw new Exception();
    }
}
