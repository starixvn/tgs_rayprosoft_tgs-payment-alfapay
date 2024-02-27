using System.Net;
using System.Text.Json;
using Google.Protobuf;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using RichardSzalay.MockHttp;
using Tgs.Payment.AlfaPay;
using Tgs.Payment.AlfaPay.Client;
using Tgs.Payment.AlfaPay.MessageQueue;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;

namespace AlfaPay.Core.UnitTests;

public class WithdrawProcessorTests
{
    [Fact]
    public async Task 代付GCash掃碼()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://alfapay63.com");

        var fakeAlfaPayClient = new AlfaPayClient(
            httpClient,
            new AlfaPayHasher(NullLogger<AlfaPayHasher>.Instance),
            NullLogger<AlfaPayClient>.Instance);

        var fakePaymentClient = Substitute.For<IPaymentClient>();
        var options = new AlfaPayOptions
        {
            CallbackBaseUrl = new Uri("https://dev-payment.rpgp.cc/alfapay"),
            PaymentApiUrl = new Uri("https://alfapay63.com"),
            PaymentServiceBaseUrl = new Uri("http://localhost:5541")
        };

        var sut = new WithdrawProcessor(
            fakeAlfaPayClient,
            new OptionsWrapper<AlfaPayOptions>(options),
            fakePaymentClient);

        var formId = Guid.Parse("b9a48264-fed8-4286-98b1-053c6e3a2274");
        var apiToken = "A2B0ZGwcPXFw8YB84mSrXnVVGQnwTImulmRxvXmsWxMKXaWBv2d3NbcwGxLc";
        var notifyToken = "DzBwQEyXd2OZZ8IFWrQCWunnmd0vZHqrKeKOaxxNe2gpS5ZSYx2KdFext74J";

        var applySettings = new AlfaPayWithdrawSettings(
            apiToken,
            notifyToken,
            AccountNumber: "9517585997",
            BankId: "GCASH",
            BankOwner: "測試帳號",
            formId);

        var request = new WithdrawRequest
        {
            Amount = "100",
            ApplyInfo = JsonSerializer.Serialize(applySettings, AlfaPayUtility.SerializerOptions)
        };

        _ = mockHttp.Expect(HttpMethod.Post, "https://alfapay63.com/api/payment")
            .WithHeaders(new Dictionary<string, string>
            {
                { "Accept", "application/json" },
                { "Authorization", $"Bearer {apiToken}" }
            })
            .With(req => req.Content?.Headers.ContentType?.MediaType == "application/json")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "data": {
                        "trade_no": "42e74452-503d-4de8-ad21-aa879a3af6e9",
                        "out_trade_no": "b9a48264-fed8-4286-98b1-053c6e3a2274",
                        "bank_id": "GCASH",
                        "bank_owner": "\u6e2c\u8a66\u5e33\u865f",
                        "account_number": "9517585997",
                        "amount": 100,
                        "callback_url": "https:\/\/dev-payment.rpgp.cc\/alfapay\/withdraw\/callback",
                        "state": "new"
                    },
                    "success": true
                }
                """));

        // Act
        var actual = await sut.HandleAsync(
            "tgs.payment.alfapay.withdraw",
            request,
            null);
        
        var applyInfo = JsonSerializer.Deserialize<AlfaPayWithdrawApplyInfo>(actual.Result, AlfaPayUtility.SerializerOptions);

        // Assert
        _ = fakePaymentClient.Received(1)
            .SaveWithdrawFormThirdPartyOrderNoAsync(
                Arg.Is(applySettings.FormId),
                Arg.Is("42e74452-503d-4de8-ad21-aa879a3af6e9"));
        Assert.NotNull(applyInfo);
        Assert.Equal(applySettings, applyInfo.Settings);
    }
}
