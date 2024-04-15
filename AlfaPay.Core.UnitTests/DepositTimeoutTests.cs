using System.Net;
using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
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

public class DepositTimeoutTests
{
    [Fact]
    public async Task 檢查已經申請十五分鐘的單子是否已經完成()
    {
        // Arrange
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var mockHttp = new MockHttpMessageHandler();

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://alfapay63.com");

        var fakeAlfaPayClient = new AlfaPayClient(
            httpClient,
            new AlfaPayHasher(NullLogger<AlfaPayHasher>.Instance),
            NullLogger<AlfaPayClient>.Instance);

        var fakePaymentClient = Substitute.For<IPaymentClient>();

        var alfaPayOptions = new OptionsWrapper<AlfaPayOptions>(new AlfaPayOptions
        {
            PaymentCode = "AlfaPay",
            CallbackBaseUrl = new Uri("http://localhost"),
            PaymentApiUrl = new Uri("https://alfapay63.com"),
            PaymentServiceBaseUrl = new Uri("http://svc")
        });

        var sut = new DepositTimeoutHandler(
            fakePaymentServiceClient,
            fakePaymentClient,
            fakeAlfaPayClient,
            alfaPayOptions,
            NullLogger<DepositTimeoutHandler>.Instance);

        var formId = Guid.Parse("1091386e-c9cc-4152-95fa-dfcf73c08026");
        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var request = new ScheduleRequest
        {
            FormId = formId,
            Subject = "tgs.payment.alfapay.deposit.schedule.reject",
            ScheduleTime = Timestamp.FromDateTime(DateTime.UtcNow)
        };

        var applyInfo = new AlfaPayDepositApplyInfo(
            new AlfaPayDepositSettings(
                "A2B0ZGwcPXFw8YB84mSrXnVVGQnwTImulmRxvXmsWxMKXaWBv2d3NbcwGxLc",
                "DzBwQEyXd2OZZ8IFWrQCWunnmd0vZHqrKeKOaxxNe2gpS5ZSYx2KdFext74J",
                formId,
                null),
            new AlfaPayDepositResponse(
                500m,
                formId,
                new Uri("https://payhub1255.com/r/dmp/6c46b82e-4c30-4388-924a-2a1ef111731b"),
                "6c46b82e-4c30-4388-924a-2a1ef111731b"));

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(request.FormId.ToGuid()))
            .Returns(new DepositFormInfo
            {
                Id = formId,
                Amount = 500m,
                BrandId = brandId,
                ApplyInfo = JsonSerializer.Serialize(applyInfo, AlfaPayUtility.SerializerOptions)
            });

        var apiToken = "A2B0ZGwcPXFw8YB84mSrXnVVGQnwTImulmRxvXmsWxMKXaWBv2d3NbcwGxLc";

        _ = mockHttp.Expect(HttpMethod.Get, $"https://alfapay63.com/api/transaction/{formId}")
            .WithHeaders(new Dictionary<string, string>
            {
                { "Accept", "application/json" },
                { "Authorization", $"Bearer {apiToken}" }
            })
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "data": {
                        "trade_no": "6c46b82e-4c30-4388-924a-2a1ef111731b",
                        "out_trade_no": "1091386e-c9cc-4152-95fa-dfcf73c08026",
                        "amount": "500.00",
                        "request_amount": 500,
                        "state": "timeout"
                    },
                    "success": true
                }
                """));

        // Act
        await sut.HandleAsync(
            "tgs.payment.alfapay.deposit.schedule.reject",
            request,
            null);

        // Assert
        _ = fakePaymentClient
            .Received(1)
            .RejectDepositAsync(Arg.Is(formId), Arg.Is(brandId));
    }

    [Fact]
    public async Task 當AlfaPay代收接口查詢回傳狀態是processing時若GMS存款單狀態仍在等待付款時將會重新計時()
    {
        // Arrange
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var mockHttp = new MockHttpMessageHandler();

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://alfapay63.com");

        var fakeAlfaPayClient = new AlfaPayClient(
            httpClient,
            new AlfaPayHasher(NullLogger<AlfaPayHasher>.Instance),
            NullLogger<AlfaPayClient>.Instance);

        var fakePaymentClient = Substitute.For<IPaymentClient>();

        var alfaPayOptions = new OptionsWrapper<AlfaPayOptions>(new AlfaPayOptions
        {
            PaymentCode = "AlfaPay",
            CallbackBaseUrl = new Uri("http://localhost"),
            PaymentApiUrl = new Uri("https://alfapay63.com"),
            PaymentServiceBaseUrl = new Uri("http://svc")
        });

        var sut = new DepositTimeoutHandler(
            fakePaymentServiceClient,
            fakePaymentClient,
            fakeAlfaPayClient,
            alfaPayOptions,
            NullLogger<DepositTimeoutHandler>.Instance);

        var formId = Guid.Parse("1091386e-c9cc-4152-95fa-dfcf73c08026");
        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var request = new ScheduleRequest
        {
            FormId = formId,
            Subject = "tgs.payment.alfapay.deposit.schedule.reject",
            ScheduleTime = Timestamp.FromDateTime(DateTime.UtcNow)
        };

        var applyInfo = new AlfaPayDepositApplyInfo(
            new AlfaPayDepositSettings(
                "A2B0ZGwcPXFw8YB84mSrXnVVGQnwTImulmRxvXmsWxMKXaWBv2d3NbcwGxLc",
                "DzBwQEyXd2OZZ8IFWrQCWunnmd0vZHqrKeKOaxxNe2gpS5ZSYx2KdFext74J",
                formId,
                null),
            new AlfaPayDepositResponse(
                500m,
                formId,
                new Uri("https://payhub1255.com/r/dmp/6c46b82e-4c30-4388-924a-2a1ef111731b"),
                "6c46b82e-4c30-4388-924a-2a1ef111731b"));

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(request.FormId.ToGuid()))
            .Returns(new DepositFormInfo
            {
                Id = formId,
                Amount = 500m,
                BrandId = brandId,
                ApplyInfo = JsonSerializer.Serialize(applyInfo, AlfaPayUtility.SerializerOptions)
            });

        var apiToken = "A2B0ZGwcPXFw8YB84mSrXnVVGQnwTImulmRxvXmsWxMKXaWBv2d3NbcwGxLc";

        _ = mockHttp.Expect(HttpMethod.Get, $"https://alfapay63.com/api/transaction/{formId}")
            .WithHeaders(new Dictionary<string, string>
            {
                { "Accept", "application/json" },
                { "Authorization", $"Bearer {apiToken}" }
            })
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "data": {
                        "trade_no": "6c46b82e-4c30-4388-924a-2a1ef111731b",
                        "out_trade_no": "1091386e-c9cc-4152-95fa-dfcf73c08026",
                        "amount": "500.00",
                        "request_amount": 500,
                        "state": "processing"
                    },
                    "success": true
                }
                """));

        // Act
        await sut.HandleAsync(
            "tgs.payment.alfapay.deposit.schedule.reject",
            request,
            null);

        // Assert
        _ = fakePaymentClient
            .Received(1)
            .ScheduleAsync(
                Arg.Is("tgs.payment.alfapay.deposit.schedule.reject"),
                Arg.Is(request.FormId.ToGuid()),
                Arg.Any<DateTime>());
    }
}