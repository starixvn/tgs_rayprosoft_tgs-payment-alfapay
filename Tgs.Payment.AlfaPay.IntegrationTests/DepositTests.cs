using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Stateless;
using Tgs.Payment.AlfaPay.ApplicationServices;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.AlfaPay.IntegrationTests;

[TestClass]
public class DepositTests
{
    [TestMethod]
    public async Task DepositCallback()
    {
        var fakeGetDepositFormService = Substitute.For<IGetService<GetDepositFormQuery, DepositForm?>>();
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();

        await using var web = new TestAlfaPayApplication(builder => builder
            .ConfigureServices(services => services
                .AddSingleton(fakeGetDepositFormService)
                .AddSingleton(fakePaymentServiceClient)
                .AddSingleton(fakePaymentClient)));

        var httpClient = web.CreateClient();

        var depositFormId = Guid.Parse("0cb6ff09-3291-4bdf-ae98-11d0869ca5c9");
        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        var formDict = new Dictionary<string, object>
        {
            ["sign"] = "a7c9b156646556d40dd534ed3cabfe12",
            ["state"] = "completed",
            ["amount"] = "600.00",
            ["trade_no"] = "a435f350-bda2-48c3-876d-fb57dd48bd7f",
            ["out_trade_no"] = depositFormId,
            ["request_amount"] = 600,
        };

        var apiToken = "3p3FCtslWkFu9iFkAnFjOKRLWZoqAy58iaDldaCZ1M4jNmyVJA0fhkSe5IIv";
        var notifyToken = "sPknBbCd38DZWHhEZ2DyHFdyvgkDwULFl7tefGILMYwOUIaFjhqIsUps3T98";

        var sign = AlfaPayHasher.ComputeHash(apiToken, notifyToken, AlfaPayPaymentType.Deposit, formDict);

        formDict["sign"] = sign;

        var applyInfo = new AlfaPayDepositApplyInfo(
            new AlfaPayDepositSettings(apiToken, notifyToken, depositFormId, null),
            new AlfaPayDepositResponse
            {
                Success = true,
                Data = new DepositResponseData
                {
                    Uri = "https://tiancip020706.com/r/t/a435f350-bda2-48c3-876d-fb57dd48bd7f",
                    Amount = "600.00",
                    TradeNo = "a435f350-bda2-48c3-876d-fb57dd48bd7f",
                    OutTradeNo = "0cb6ff09-3291-4bdf-ae98-11d0869ca5c9",
                    QrCode = ""
                }
            });

        _ = fakeGetDepositFormService.GetAsync(Arg.Any<GetDepositFormQuery>())
            .Returns(new DepositForm(
                depositFormId,
                "4ugywbc0jwnu",
                brandId,
                Guid.Parse("eb80cab0-4c61-4399-910e-e63c995acd8f"),
                600M,
                PaymentFlowState.WaitPayment,
                applyInfo));

        var fsm = new StateMachine<PaymentFlowState, PaymentTrigger>(PaymentFlowState.WaitPayment);

        _ = fsm.Configure(PaymentFlowState.Init)
            .Permit(PaymentTrigger.DepositCreate, PaymentFlowState.WaitPayment)
            .Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

        _ = fsm.Configure(PaymentFlowState.WaitPayment)
            .Permit(PaymentTrigger.PaymentDepositCompleted, PaymentFlowState.WaitApprove)
            .Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

        _ = fsm.Configure(PaymentFlowState.WaitApprove)
            .Permit(PaymentTrigger.Approve, PaymentFlowState.Approved)
            .Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

        _ = fakePaymentServiceClient.CreateDepositStateMachine(Arg.Is(PaymentFlowState.WaitPayment))
            .Returns(fsm);

        var requestContent = new StringContent(JsonSerializer.Serialize(formDict), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("/deposit/callback", requestContent);

        var actual = await response.Content.ReadAsStringAsync();

        Assert.AreEqual("ok", actual);

        _ = fakePaymentServiceClient.Received(1)
            .SubmitDepositFormStatusAsync(
                Arg.Is(depositFormId),
                Arg.Is((PaymentFlowState?)null),
                Arg.Any<string>());
        _ = fakePaymentClient.Received(1)
            .ApproveDepositAsync(
                Arg.Is(depositFormId),
                Arg.Is(brandId),
                Arg.Is(600M));
    }
}
