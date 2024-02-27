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

public class DepositProcessorTests
{
    [Fact]
    public async Task 儲值要求處理_GCash掃碼()
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
        var formId = Guid.Parse("1f194dc3-b2fe-4b1c-9f91-65c92f20a402");
        var apiToken = "A2B0ZGwcPXFw8YB84mSrXnVVGQnwTImulmRxvXmsWxMKXaWBv2d3NbcwGxLc";
        var notifyToken = "DzBwQEyXd2OZZ8IFWrQCWunnmd0vZHqrKeKOaxxNe2gpS5ZSYx2KdFext74J";

        var sut = new DepositProcessor(
            fakePaymentClient,
            fakeAlfaPayClient,
            new OptionsWrapper<AlfaPayOptions>(options));

        var applySettings = new AlfaPayDepositSettings(
            apiToken,
            notifyToken,
            formId,
            null);

        var request = new DepositRequest
        {
            Amount = "500",
            ApplyInfo = JsonSerializer.Serialize(applySettings, AlfaPayUtility.SerializerOptions)
        };

        _ = mockHttp.Expect(HttpMethod.Post, "https://alfapay63.com/api/transaction")
            .WithHeaders(new Dictionary<string, string>
            {
                { "Accept", "application/json" },
                { "Authorization", $"Bearer {apiToken}" }
            })
            .With(req => req.Content?.Headers.ContentType?.MediaType == "application/json")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "data": {
                        "trade_no": "0a14d1d4-4740-4f4c-9173-1b40d5776910",
                        "out_trade_no": "1f194dc3-b2fe-4b1c-9f91-65c92f20a402",
                        "amount": "500.00",
                        "uri": "https:\/\/payhub1255.com\/r\/dmp\/0a14d1d4-4740-4f4c-9173-1b40d5776910",
                        "qrcode": "00020101021227830012com.p2pqrpay0111GXCHPHM2XXX02089996440303152170200000006560417DWQM4TK3JDNXCPG2U5204601653036085406500.005802PH5907AlfaPay6008MALANDAY6104123463042985"
                    },
                    "success": true
                }
                """));

        // Act
        var actual = await sut.HandleAsync(
            "tgs.payment.alfapay.deposit",
            request,
            null);

        var applyInfo = JsonSerializer.Deserialize<AlfaPayDepositApplyInfo>(actual.Result, AlfaPayUtility.SerializerOptions);

        // Assert
        _ = fakePaymentClient.Received(1)
            .ScheduleAsync(
                Arg.Is("tgs.payment.alfapay.deposit.schedule.reject"),
                Arg.Is(applySettings.FormId),
                Arg.Any<DateTime>());
        Assert.NotNull(applyInfo);
        Assert.Equal(applySettings, applyInfo.Settings);
        Assert.Equal(
            new AlfaPayDepositResponse(
                500m,
                applySettings.FormId,
                new Uri("https://payhub1255.com/r/dmp/0a14d1d4-4740-4f4c-9173-1b40d5776910"),
                "0a14d1d4-4740-4f4c-9173-1b40d5776910"),
            applyInfo.Response);
    }
}