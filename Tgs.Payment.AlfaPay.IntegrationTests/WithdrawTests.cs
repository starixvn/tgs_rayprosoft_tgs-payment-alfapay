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
public class WithdrawTests
{
    [TestMethod]
    public async Task WithdrawCallback()
    {
        var fakeGetWithdrawFormQueryService = Substitute.For<IGetService<GetWithdrawFormQuery, WithdrawForm?>>();
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();

        await using var web = new TestAlfaPayApplication(builder => builder
            .ConfigureServices(services => services
                .AddSingleton(fakeGetWithdrawFormQueryService)
                .AddSingleton(fakePaymentServiceClient)
                .AddSingleton(fakePaymentClient)));

        var httpClient = web.CreateClient();

        var apiToken = "3p3FCtslWkFu9iFkAnFjOKRLWZoqAy58iaDldaCZ1M4jNmyVJA0fhkSe5IIv";
        var notifyToken = "sPknBbCd38DZWHhEZ2DyHFdyvgkDwULFl7tefGILMYwOUIaFjhqIsUps3T98";

        var withdrawFormId = Guid.Parse("309c8a04-232b-472e-93eb-c4364906b85b");
        var formDict = new Dictionary<string, object>
        {
            ["trade_no"] = "23a749c3-2d09-403c-95b7-21994a7942f1",
            ["amount"] = "200.00",
            ["out_trade_no"] = withdrawFormId.ToString(),
            ["state"] = "completed",
            ["sign"] = "16aab275570247895de8f84e8502425b"
        };

        var sign = AlfaPayHasher.ComputeHash(apiToken, notifyToken, AlfaPayPaymentType.Withdraw, formDict);

        formDict["sign"] = sign;

        var applyInfo = new AlfaPayWithdrawApplyInfo(
            new AlfaPayWithdrawSettings(
                apiToken,
                notifyToken,
                "099517585997",
                "GCASH",
                "王小明",
                withdrawFormId),
            new AlfaPayWithdrawResponse
            {
                Success = true,
                Data = new WithdrawResponseData
                {
                    AccountNumber = "099517585997",
                    Amount = 200M,
                    BankId = "GCASH",
                    BankOwner = "\u738b\u5c0f\u660e",
                    OutTradeNo = "309c8a04-232b-472e-93eb-c4364906b85b",
                    CallbackUrl = "https://dev-payment.rpgp.cc/alfapay/withdraw/callback",
                    State = "completed",
                    TradeNo = "23a749c3-2d09-403c-95b7-21994a7942f1"
                }
            });

        _ = fakeGetWithdrawFormQueryService.GetAsync(Arg.Any<GetWithdrawFormQuery>())
            .Returns(new WithdrawForm(
                withdrawFormId,
                "4udxu8ivnr24",
                UserType.Player,
                Guid.NewGuid(),
                Guid.NewGuid(),
                200M,
                PaymentFlowState.Approved,
                Guid.NewGuid(),
                null,
                applyInfo));

        var fsm = new StateMachine<PaymentFlowState, PaymentTrigger>(PaymentFlowState.Approved);

        _ = fsm.Configure(PaymentFlowState.Init)
            .Permit(PaymentTrigger.WithdrawCreate, PaymentFlowState.WaitApprove)
            .Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

        _ = fsm.Configure(PaymentFlowState.WaitApprove)
            .Permit(PaymentTrigger.Approve, PaymentFlowState.Approved)
            .Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

        _ = fsm.Configure(PaymentFlowState.Approved)
            .Permit(PaymentTrigger.PaymentWithdrawCompleted, PaymentFlowState.Paid)
            .Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

        _ = fakePaymentServiceClient.CreateWithdrawStateMachine(Arg.Is(PaymentFlowState.Approved))
            .Returns(fsm);

        var requestContent = new StringContent(JsonSerializer.Serialize(formDict), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("/withdraw/callback", requestContent);

        var actual = await response.Content.ReadAsStringAsync();

        Assert.AreEqual("ok", actual);

        _ = fakePaymentServiceClient.Received(1)
            .SubmitWithdrawFormStatusAsync(
                Arg.Is(withdrawFormId),
                Arg.Is(default(PaymentFlowState?)),
                Arg.Any<string?>());
        _ = fakePaymentClient.Received(1)
            .PaidWithdrawAsync(
                Arg.Is(withdrawFormId),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }
}
