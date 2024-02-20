using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Payment.AlfaPay.Client;
using Tgs.Payment.Clients;
using Xunit.Abstractions;

namespace Tgs.Payment.AlfaPay.IntegrationTests;

public class DepositTests(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));

    [Fact]
    public async Task 代收回調_訂單狀態成功()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();

        await using var web = AlfaPayTestHost.Factory
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
                .ConfigureServices(services => services
                    .AddSingleton(fakePaymentServiceClient)
                    .AddSingleton(fakePaymentClient)));

        var httpClient = web.CreateClient();

        var formId = Guid.Parse("1f194dc3-b2fe-4b1c-9f91-65c92f20a402");
        var brandId = Guid.NewGuid();
        var apiToken = "A2B0ZGwcPXFw8YB84mSrXnVVGQnwTImulmRxvXmsWxMKXaWBv2d3NbcwGxLc";
        var notifyToken = "DzBwQEyXd2OZZ8IFWrQCWunnmd0vZHqrKeKOaxxNe2gpS5ZSYx2KdFext74J";

        var request = """
            {
                "trade_no": "0a14d1d4-4740-4f4c-9173-1b40d5776910",
                "request_amount": 500,
                "amount": "500.00",
                "out_trade_no": "1f194dc3-b2fe-4b1c-9f91-65c92f20a402",
                "state": "completed",
                "sign": "d91ba7c240a3262c1995a9601c3acf78",
                "callback_url": "https://dev-payment.rpgp.cc/alfapay/deposit/callback"
            }
            """;

        var content = new StringContent(request, Encoding.UTF8, new MediaTypeHeaderValue(MediaTypeNames.Application.Json));

        var applyInfo = new AlfaPayDepositApplyInfo(
            new AlfaPayDepositSettings(
                apiToken,
                notifyToken,
                formId,
                null),
             new AlfaPayDepositResponse(
                500m,
                formId,
                new Uri("https://payhub1255.com/r/dmp/0a14d1d4-4740-4f4c-9173-1b40d5776910"),
                "0a14d1d4-4740-4f4c-9173-1b40d5776910"));

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(formId), Arg.Any<CancellationToken>())
            .Returns(new DepositFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.WaitPayment,
                Amount = 500m,
                ThirdPartyOrderNo = "0a14d1d4-4740-4f4c-9173-1b40d5776910",
                ApplyInfo = JsonSerializer.Serialize(applyInfo, AlfaPayUtility.SerializerOptions)
            });

        var response = await httpClient.PostAsync("/deposit/callback", content);

        var actual = await response.Content.ReadAsStringAsync();

        Assert.Equal("ok", actual);

        var paymentReturn = new AlfaPayDepositReturn()
        {
            Status = "completed",
            FormId = formId,
            ThirdPartyOrderNo = "0a14d1d4-4740-4f4c-9173-1b40d5776910",
            RealAmount = 500.00m,
            OrderAmount = 500m,
            Sign = "d91ba7c240a3262c1995a9601c3acf78",
        };

        _ = fakePaymentServiceClient.Received(1)
            .SubmitDepositFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Is(JsonSerializer.Serialize(paymentReturn, AlfaPayUtility.SerializerOptions)),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .ApproveDepositAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Is(paymentReturn.OrderAmount),
                Arg.Any<CancellationToken>());
    }
}
