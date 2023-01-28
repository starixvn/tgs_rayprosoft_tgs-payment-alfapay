using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.Payment.AlfaPay;
using Tgs.Payment.AlfaPay.MessageQueue;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;

namespace AlfaPay.Core.UnitTests;

public class DepositTimeoutTests
{
    [Test]
    public async Task 當AlfaPay代收接口查詢回傳狀態是timeout時若GMS存款單仍在等待付款時將會拒絕存款單()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();
        var fakeAlfaPayClient = Substitute.For<IAlfaPayClient>();
        var alfaPaySerializer = new AlfaPaySerializer();
        var fakeLogger = new NullLogger<DepositTimeoutHandler>();

        var sut = new DepositTimeoutHandler(fakePaymentServiceClient, fakePaymentClient, fakeAlfaPayClient, alfaPaySerializer, fakeLogger);

        var formId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var apiToken = "apiToken";

        var depositFormInfo = new DepositFormInfo
        {
            Id = formId,
            BrandId = brandId,
            Status = PaymentFlowState.WaitPayment,
            ApplyInfo = alfaPaySerializer.Serialize(new AlfaPayDepositApplyInfo(
                new AlfaPayDepositSettings(apiToken, "notifyToken", formId, null),
                new AlfaPayDepositResponse()))
        };

        _ = fakePaymentServiceClient
            .GetDepositFormAsync(formId)
            .Returns(depositFormInfo);

        var alfaPayDepositInfoData = new AlfaPayDepositInfoData(
            Guid.Parse("5d6a00ef-798c-4f53-927c-10295e658b6a"),
            formId.ToString(),
            "1680.00",
            1680M,
            "timeout");

        _ = fakeAlfaPayClient
            .QueryDepositFormAsync(apiToken, formId)
            .Returns(new AlfaPayClientDepositInfo(true, alfaPayDepositInfoData));

        var request = new ScheduleRequest
        {
            Subject = "tgs.payment.alfapay.deposit.schedule.reject",
            FormId = formId,
            ScheduleTime = DateTime.UtcNow.ToTimestamp(),
        };

        await sut.HandleAsync(request.ToByteArray());

        _ = fakePaymentClient.Received(1).RejectDepositAsync(formId, brandId);
    }

    [Test]
    public async Task 當AlfaPay代收接口查詢回傳狀態是processing時若GMS存款單狀態仍在等待付款時將會重新計時()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();
        var fakeAlfaPayClient = Substitute.For<IAlfaPayClient>();
        var alfaPaySerializer = new AlfaPaySerializer();
        var fakeLogger = new NullLogger<DepositTimeoutHandler>();

        var sut = new DepositTimeoutHandler(fakePaymentServiceClient, fakePaymentClient, fakeAlfaPayClient, alfaPaySerializer, fakeLogger);

        var formId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var apiToken = "apiToken";

        var depositFormInfo = new DepositFormInfo
        {
            Id = formId,
            BrandId = brandId,
            Status = PaymentFlowState.WaitPayment,
            ApplyInfo = alfaPaySerializer.Serialize(new AlfaPayDepositApplyInfo(
                new AlfaPayDepositSettings(apiToken, "notifyToken", formId, null),
                new AlfaPayDepositResponse()))
        };

        _ = fakePaymentServiceClient
            .GetDepositFormAsync(formId)
            .Returns(depositFormInfo);

        var alfaPayDepositInfoData = new AlfaPayDepositInfoData(
            Guid.Parse("5d6a00ef-798c-4f53-927c-10295e658b6a"),
            formId.ToString(),
            "1680.00",
            1680M,
            "processing");

        _ = fakeAlfaPayClient
            .QueryDepositFormAsync(apiToken, formId)
            .Returns(new AlfaPayClientDepositInfo(true, alfaPayDepositInfoData));

        var subject = "tgs.payment.alfapay.deposit.schedule.reject";

        var request = new ScheduleRequest
        {
            Subject = subject,
            FormId = formId,
            ScheduleTime = DateTime.UtcNow.ToTimestamp(),
        };

        await sut.HandleAsync(request.ToByteArray());

        _ = fakePaymentClient
            .Received(1)
            .ScheduleAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<DateTime>());
    }

    [Test]
    public async Task 若GMS存款單狀態為Reject將不繼續執行()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();
        var fakeAlfaPayClient = Substitute.For<IAlfaPayClient>();
        var alfaPaySerializer = new AlfaPaySerializer();
        var fakeLogger = new NullLogger<DepositTimeoutHandler>();

        var sut = new DepositTimeoutHandler(fakePaymentServiceClient, fakePaymentClient, fakeAlfaPayClient, alfaPaySerializer, fakeLogger);

        var formId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var apiToken = "apiToken";

        var depositFormInfo = new DepositFormInfo
        {
            Id = formId,
            BrandId = brandId,
            Status = PaymentFlowState.Reject,
            ApplyInfo = alfaPaySerializer.Serialize(new AlfaPayDepositApplyInfo(
                new AlfaPayDepositSettings(apiToken, "notifyToken", formId, null),
                new AlfaPayDepositResponse()))
        };

        _ = fakePaymentServiceClient
            .GetDepositFormAsync(formId)
            .Returns(depositFormInfo);

        var subject = "tgs.payment.alfapay.deposit.schedule.reject";

        var request = new ScheduleRequest
        {
            Subject = subject,
            FormId = formId,
            ScheduleTime = DateTime.UtcNow.ToTimestamp(),
        };

        await sut.HandleAsync(request.ToByteArray());

        _ = fakePaymentClient
            .Received(0)
            .ScheduleAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<DateTime>());
    }

    [Test]
    public async Task 若GMS存款單狀態為Approved將不繼續執行()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();
        var fakeAlfaPayClient = Substitute.For<IAlfaPayClient>();
        var alfaPaySerializer = new AlfaPaySerializer();
        var fakeLogger = new NullLogger<DepositTimeoutHandler>();

        var sut = new DepositTimeoutHandler(fakePaymentServiceClient, fakePaymentClient, fakeAlfaPayClient, alfaPaySerializer, fakeLogger);

        var formId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var apiToken = "apiToken";

        var depositFormInfo = new DepositFormInfo
        {
            Id = formId,
            BrandId = brandId,
            Status = PaymentFlowState.Approved,
            ApplyInfo = alfaPaySerializer.Serialize(new AlfaPayDepositApplyInfo(
                new AlfaPayDepositSettings(apiToken, "notifyToken", formId, null),
                new AlfaPayDepositResponse()))
        };

        _ = fakePaymentServiceClient
            .GetDepositFormAsync(formId)
            .Returns(depositFormInfo);

        var subject = "tgs.payment.alfapay.deposit.schedule.reject";

        var request = new ScheduleRequest
        {
            Subject = subject,
            FormId = formId,
            ScheduleTime = DateTime.UtcNow.ToTimestamp(),
        };

        await sut.HandleAsync(request.ToByteArray());

        _ = fakePaymentClient
            .Received(0)
            .ScheduleAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<DateTime>());
    }

    [Test]
    public async Task 若GMS存款單狀態為Paid將不繼續執行()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();
        var fakeAlfaPayClient = Substitute.For<IAlfaPayClient>();
        var alfaPaySerializer = new AlfaPaySerializer();
        var fakeLogger = new NullLogger<DepositTimeoutHandler>();

        var sut = new DepositTimeoutHandler(fakePaymentServiceClient, fakePaymentClient, fakeAlfaPayClient, alfaPaySerializer, fakeLogger);

        var formId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var apiToken = "apiToken";

        var depositFormInfo = new DepositFormInfo
        {
            Id = formId,
            BrandId = brandId,
            Status = PaymentFlowState.Paid,
            ApplyInfo = alfaPaySerializer.Serialize(new AlfaPayDepositApplyInfo(
                new AlfaPayDepositSettings(apiToken, "notifyToken", formId, null),
                new AlfaPayDepositResponse()))
        };

        _ = fakePaymentServiceClient
            .GetDepositFormAsync(formId)
            .Returns(depositFormInfo);

        var subject = "tgs.payment.alfapay.deposit.schedule.reject";

        var request = new ScheduleRequest
        {
            Subject = subject,
            FormId = formId,
            ScheduleTime = DateTime.UtcNow.ToTimestamp(),
        };

        await sut.HandleAsync(request.ToByteArray());

        _ = fakePaymentClient
            .Received(0)
            .ScheduleAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<DateTime>());
    }
}
