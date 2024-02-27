using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using RichardSzalay.MockHttp;
using Tgs.Payment.AlfaPay;
using Tgs.Payment.AlfaPay.MessageQueue;
using Tgs.Payment.Protos;

namespace AlfaPay.Core.UnitTests;

public class GetBalanceProcessorTests
{
    [Fact]
    public async Task GetBalanceProcessor_取得商戶餘額()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://alfapay63.com");

        var fakePhpGoClient = new AlfaPayClient(
            httpClient,
            new AlfaPayHasher(NullLogger<AlfaPayHasher>.Instance),
            NullLogger<AlfaPayClient>.Instance);

        var sut = new GetBalanceProcessor(fakePhpGoClient);

        var apiToken = "A2B0ZGwcPXFw8YB84mSrXnVVGQnwTImulmRxvXmsWxMKXaWBv2d3NbcwGxLc";

        var request = new PaymentBalanceRequest
        {
            BrandPaymentSettings = """
				{
					"apiToken": "A2B0ZGwcPXFw8YB84mSrXnVVGQnwTImulmRxvXmsWxMKXaWBv2d3NbcwGxLc"
				}
				"""
        };

        _ = mockHttp.Expect(HttpMethod.Get, "https://alfapay63.com/api/balance/inquiry")
            .WithHeaders(new Dictionary<string, string>
            {
                { "Accept", "application/json" },
                { "Authorization", $"Bearer {apiToken}" }
            })
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "data": {
                        "balance": "489"
                    },
                    "success": true
                }
                """));

        // Act
        var actual = await sut.HandleAsync(
            "tgs.payment.alfapay.balance",
            request,
            null);

        // Assert
        Assert.Equal(489M, actual.Balance.ToDecimal());
    }
}