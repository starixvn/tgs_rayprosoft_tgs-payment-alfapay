using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using RichardSzalay.MockHttp;
using Tgs.Payment.AlfaPay;
using Tgs.Payment.AlfaPay.MessageQueue;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;

namespace AlfaPay.Core.UnitTests;

public class DepositTimeoutTests
{
    [Fact]
    public async Task 當AlfaPay代收接口查詢回傳狀態是timeout時若GMS存款單仍在等待付款時將會拒絕存款單()
    {
    //    var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
    //    var fakePaymentClient = Substitute.For<IPaymentClient>();

    //    var mockHttp = new MockHttpMessageHandler();

    //    var httpClient = mockHttp.ToHttpClient();
    //    httpClient.BaseAddress = new Uri("https://alfapay63.com");

    //    var fakeAlfaPayClient = new AlfaPayClient(httpClient, new AlfaPayHasher(NullLogger<AlfaPayHasher>.Instance), NullLogger<AlfaPayClient>.Instance);

    //    var fakeLogger = new NullLogger<DepositTimeoutHandler>();

    //    var sut = new DepositTimeoutHandler(fakePaymentServiceClient, fakePaymentClient, fakeAlfaPayClient, fakeLogger);

    //    var formId = Guid.NewGuid();
    //    var brandId = Guid.NewGuid();
    //    var apiToken = "apiToken";

    //    var depositFormInfo = new DepositFormInfo
    //    {
    //        Id = formId,
    //        BrandId = brandId,
    //        Status = PaymentFlowState.WaitPayment,
    //        ApplyInfo = JsonSerializer.Serialize(new AlfaPayDepositApplyInfo(
    //            new AlfaPayDepositSettings(apiToken, "notifyToken", formId, null),
    //            new AlfaPayDepositResponse()), AlfaPayUtility.SerializerOptions)
    //    };

    //    _ = fakePaymentServiceClient
    //        .GetDepositFormAsync(formId)
    //        .Returns(depositFormInfo);

    //    var alfaPayDepositInfoData = new AlfaPayDepositInfoData(
    //        Guid.Parse("5d6a00ef-798c-4f53-927c-10295e658b6a"),
    //        formId.ToString(),
    //        "1680.00",
    //        1680M,
    //        "timeout");

    //    _ = mockHttp.When($"https://alfapay63.com/api/transaction/{formId}")
    //        .Respond(new StringContent("""
				//{
				//    "data": {
				//        "trade_no": "e41840c9-a792-482f-ab4d-b0dca0489757",
				//        "out_trade_no": "0ff96c9d-d3f7-467f-90d2-9917820e89bf",
				//        "amount": 200000,
				//        "request_amount": 200000,
				//        "state": "timeout"
				//    },
				//    "success": true
				//}
				//"""));

    //    var request = new ScheduleRequest
    //    {
    //        Subject = "tgs.payment.alfapay.deposit.schedule.reject",
    //        FormId = formId,
    //        ScheduleTime = DateTime.UtcNow.ToTimestamp(),
    //    };

    //    await sut.HandleAsync(
    //        "tgs.payment.alfapay.deposit.schedule.reject",
    //        request.ToByteArray(),
    //        null);

    //    _ = fakePaymentClient.Received(1).RejectDepositAsync(formId, brandId);
    }

    [Fact]
    public async Task 當AlfaPay代收接口查詢回傳狀態是processing時若GMS存款單狀態仍在等待付款時將會重新計時()
    {
        //var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        //var fakePaymentClient = Substitute.For<IPaymentClient>();
        //var mockHttp = new MockHttpMessageHandler();

        //var httpClient = mockHttp.ToHttpClient();
        //httpClient.BaseAddress = new Uri("https://alfapay63.com");

        //var fakeAlfaPayClient = new AlfaPayClient(httpClient, new AlfaPayHasher(NullLogger<AlfaPayHasher>.Instance), NullLogger<AlfaPayClient>.Instance);

        //var fakeLogger = new NullLogger<DepositTimeoutHandler>();

        //var sut = new DepositTimeoutHandler(fakePaymentServiceClient, fakePaymentClient, fakeAlfaPayClient, fakeLogger);

        //var formId = Guid.NewGuid();
        //var brandId = Guid.NewGuid();
        //var apiToken = "apiToken";

        //var depositFormInfo = new DepositFormInfo
        //{
        //    Id = formId,
        //    BrandId = brandId,
        //    Status = PaymentFlowState.WaitPayment,
        //    ApplyInfo = JsonSerializer.Serialize(
        //        new AlfaPayDepositApplyInfo(
        //            new AlfaPayDepositSettings(apiToken, "notifyToken", formId, null),
        //            new AlfaPayDepositResponse()),
        //        AlfaPayUtility.SerializerOptions)
        //};

        //_ = fakePaymentServiceClient
        //    .GetDepositFormAsync(formId)
        //    .Returns(depositFormInfo);

        //var alfaPayDepositInfoData = new AlfaPayDepositInfoData(
        //    Guid.Parse("5d6a00ef-798c-4f53-927c-10295e658b6a"),
        //    formId.ToString(),
        //    "1680.00",
        //    1680M,
        //    "processing");

        //_ = fakeAlfaPayClient
        //    .QueryDepositFormAsync(apiToken, formId)
        //    .Returns(new AlfaPayClientDepositInfo(true, alfaPayDepositInfoData));

        //var subject = "tgs.payment.alfapay.deposit.schedule.reject";

        //var request = new ScheduleRequest
        //{
        //    Subject = subject,
        //    FormId = formId,
        //    ScheduleTime = DateTime.UtcNow.ToTimestamp(),
        //};

        //await sut.HandleAsync(
        //   "tgs.payment.alfapay.deposit.schedule.reject",
        //    request.ToByteArray(),
        //    null);

        //_ = fakePaymentClient
        //    .Received(1)
        //    .ScheduleAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<DateTime>());
    }

    [Fact]  
    public async Task 若GMS存款單狀態為Reject將不繼續執行()
    {
        //var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        //var fakePaymentClient = Substitute.For<IPaymentClient>();
        //var mockHttp = new MockHttpMessageHandler();

        //var httpClient = mockHttp.ToHttpClient();
        //httpClient.BaseAddress = new Uri("https://alfapay63.com");

        //var fakeAlfaPayClient = new AlfaPayClient(httpClient, new AlfaPayHasher(NullLogger<AlfaPayHasher>.Instance), NullLogger<AlfaPayClient>.Instance);

        //var fakeLogger = new NullLogger<DepositTimeoutHandler>();

        //var sut = new DepositTimeoutHandler(fakePaymentServiceClient, fakePaymentClient, fakeAlfaPayClient, fakeLogger);

        //var formId = Guid.NewGuid();
        //var brandId = Guid.NewGuid();
        //var apiToken = "apiToken";

        //var depositFormInfo = new DepositFormInfo
        //{
        //    Id = formId,
        //    BrandId = brandId,
        //    Status = PaymentFlowState.Reject,
        //    ApplyInfo = JsonSerializer.Serialize(
        //        new AlfaPayDepositApplyInfo(
        //            new AlfaPayDepositSettings(apiToken, "notifyToken", formId, null),
        //            new AlfaPayDepositResponse()),
        //        AlfaPayUtility.SerializerOptions)
        //};

        //_ = fakePaymentServiceClient
        //    .GetDepositFormAsync(formId)
        //    .Returns(depositFormInfo);

        //var subject = "tgs.payment.alfapay.deposit.schedule.reject";

        //var request = new ScheduleRequest
        //{
        //    Subject = subject,
        //    FormId = formId,
        //    ScheduleTime = DateTime.UtcNow.ToTimestamp(),
        //};

        //await sut.HandleAsync(
        //    "tgs.payment.alfapay.deposit.schedule.reject",
        //    request.ToByteArray(),
        //    null);

        //_ = fakePaymentClient
        //    .Received(0)
        //    .ScheduleAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<DateTime>());
    }

    [Fact]
    public async Task 若GMS存款單狀態為Approved將不繼續執行()
    {
        //var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        //var fakePaymentClient = Substitute.For<IPaymentClient>();
        //var mockHttp = new MockHttpMessageHandler();

        //var httpClient = mockHttp.ToHttpClient();
        //httpClient.BaseAddress = new Uri("https://alfapay63.com");

        //var fakeAlfaPayClient = new AlfaPayClient(httpClient, new AlfaPayHasher(NullLogger<AlfaPayHasher>.Instance), NullLogger<AlfaPayClient>.Instance);

        //var fakeLogger = new NullLogger<DepositTimeoutHandler>();

        //var sut = new DepositTimeoutHandler(fakePaymentServiceClient, fakePaymentClient, fakeAlfaPayClient, fakeLogger);

        //var formId = Guid.NewGuid();
        //var brandId = Guid.NewGuid();
        //var apiToken = "apiToken";

        //var depositFormInfo = new DepositFormInfo
        //{
        //    Id = formId,
        //    BrandId = brandId,
        //    Status = PaymentFlowState.Approved,
        //    ApplyInfo = JsonSerializer.Serialize(
        //        new AlfaPayDepositApplyInfo(
        //            new AlfaPayDepositSettings(apiToken, "notifyToken", formId, null),
        //            new AlfaPayDepositResponse()),
        //        AlfaPayUtility.SerializerOptions)
        //};

        //_ = fakePaymentServiceClient
        //    .GetDepositFormAsync(formId)
        //    .Returns(depositFormInfo);

        //var subject = "tgs.payment.alfapay.deposit.schedule.reject";

        //var request = new ScheduleRequest
        //{
        //    Subject = subject,
        //    FormId = formId,
        //    ScheduleTime = DateTime.UtcNow.ToTimestamp(),
        //};

        //await sut.HandleAsync(
        //    "tgs.payment.alfapay.deposit.schedule.reject",
        //    request.ToByteArray(),
        //    null);

        //_ = fakePaymentClient
        //    .Received(0)
        //    .ScheduleAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<DateTime>());
    }

    [Fact]
    public async Task 若GMS存款單狀態為Paid將不繼續執行()
    {
        //var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        //var fakePaymentClient = Substitute.For<IPaymentClient>();
        //var mockHttp = new MockHttpMessageHandler();

        //var httpClient = mockHttp.ToHttpClient();
        //httpClient.BaseAddress = new Uri("https://alfapay63.com");

        //var fakeAlfaPayClient = new AlfaPayClient(httpClient, new AlfaPayHasher(NullLogger<AlfaPayHasher>.Instance), NullLogger<AlfaPayClient>.Instance);
        //var fakeLogger = new NullLogger<DepositTimeoutHandler>();

        //var sut = new DepositTimeoutHandler(fakePaymentServiceClient, fakePaymentClient, fakeAlfaPayClient, fakeLogger);

        //var formId = Guid.NewGuid();
        //var brandId = Guid.NewGuid();
        //var apiToken = "apiToken";

        //var depositFormInfo = new DepositFormInfo
        //{
        //    Id = formId,
        //    BrandId = brandId,
        //    Status = PaymentFlowState.Paid,
        //    ApplyInfo = JsonSerializer.Serialize(new AlfaPayDepositApplyInfo(
        //        new AlfaPayDepositSettings(apiToken, "notifyToken", formId, null),
        //        new AlfaPayDepositResponse()), AlfaPayUtility.SerializerOptions)
        //};

        //_ = fakePaymentServiceClient
        //    .GetDepositFormAsync(formId)
        //    .Returns(depositFormInfo);

        //var subject = "tgs.payment.alfapay.deposit.schedule.reject";

        //var request = new ScheduleRequest
        //{
        //    Subject = subject,
        //    FormId = formId,
        //    ScheduleTime = DateTime.UtcNow.ToTimestamp(),
        //};

        //await sut.HandleAsync(
        //    "tgs.payment.alfapay.deposit.schedule.reject",
        //    request.ToByteArray(),
        //    null);

        //_ = fakePaymentClient
        //    .Received(0)
        //    .ScheduleAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<DateTime>());
    }
}
