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

public class WithdrawTests(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));

    [Fact]
    public async Task 代付回調_訂單狀態沖回()
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

        var formId = Guid.Parse("b9a48264-fed8-4286-98b1-053c6e3a2274");
        var brandId = Guid.NewGuid();
        var apiToken = "A2B0ZGwcPXFw8YB84mSrXnVVGQnwTImulmRxvXmsWxMKXaWBv2d3NbcwGxLc";
        var notifyToken = "DzBwQEyXd2OZZ8IFWrQCWunnmd0vZHqrKeKOaxxNe2gpS5ZSYx2KdFext74J";

        var settings = new AlfaPayWithdrawSettings(
            apiToken,
            notifyToken,
            "9517585997",
            "GCASH",
            "測試帳號",
            formId);
        var withdrawResponse = new AlfaPayWithdrawResponse(formId, "42e74452-503d-4de8-ad21-aa879a3af6e9");

        var applyInfo = new AlfaPayWithdrawApplyInfo(settings, withdrawResponse);

        _ = fakePaymentServiceClient.GetWithdrawFormAsync(
                Arg.Is(formId),
                Arg.Any<CancellationToken>())
            .Returns(new WithdrawFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.Approved,
                ApplyInfo = JsonSerializer.Serialize(applyInfo, AlfaPayUtility.SerializerOptions)
            });

        var request = """
            {
                "trade_no": "42e74452-503d-4de8-ad21-aa879a3af6e9",
                "amount": "100.00",
                "out_trade_no": "b9a48264-fed8-4286-98b1-053c6e3a2274",
                "state": "refund",
                "sign": "09b9da81dfe0f5a32cfa8fee283a2b69"
            }
            """;

        var content = new StringContent(request, Encoding.UTF8, new MediaTypeHeaderValue(MediaTypeNames.Application.Json));

        var response = await httpClient.PostAsync("/withdraw/callback", content);

        var actual = await response.Content.ReadAsStringAsync();

        var paymentReturn = new AlfaPayWithdrawReturn()
        {
            FormId = formId,
            Amount = 100.00m,
            Sign = "09b9da81dfe0f5a32cfa8fee283a2b69",
            ThirdPartyOrderNo = "42e74452-503d-4de8-ad21-aa879a3af6e9",
            Status = "refund",
        };

        Assert.Equal("ok", actual);
        _ = fakePaymentServiceClient.Received(1)
            .SubmitWithdrawFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Is(JsonSerializer.Serialize(paymentReturn, AlfaPayUtility.SerializerOptions)),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .RejectWithdrawAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Any<CancellationToken>());
    }
}
