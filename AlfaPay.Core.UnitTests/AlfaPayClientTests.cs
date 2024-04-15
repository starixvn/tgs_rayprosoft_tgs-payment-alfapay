using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using RichardSzalay.MockHttp;
using Tgs.Payment.AlfaPay;

namespace AlfaPay.Core.UnitTests;

public class AlfaPayClientTests
{
    [Fact]
    public async Task 代收接口_通道尚未配置()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://alfapay63.com");
        var signer = new AlfaPayHasher(NullLogger<AlfaPayHasher>.Instance);

        var sut = new AlfaPayClient(httpClient, signer, NullLogger<AlfaPayClient>.Instance);

        var formId = Guid.Parse("ae3e2048-6c23-4a28-987a-32f25ac587e3");
        var apiToken = "A2B0ZGwcPXFw8YB84mSrXnVVGQnwTImulmRxvXmsWxMKXaWBv2d3NbcwGxLc";

        _ = mockHttp.Expect(HttpMethod.Post, "https://alfapay63.com/api/transaction")
            .WithHeaders(new Dictionary<string, string>
            {
                { "Accept", "application/json" },
                { "Authorization", $"Bearer {apiToken}" }
            })
            .With(req => req.Content?.Headers.ContentType?.MediaType == "application/json")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "success": false,
                    "status_code": 1000,
                    "message": "Tunnel not enough",
                    "errors": ["Tunnel not enough"]
                }
                """));

        var order = new DepositOrder(
            500.00m,
            formId,
            new Uri("https://dev-payment.rpgp.cc/alfapay/deposit/callback"),
            null);

        _ = await Assert.ThrowsAsync<ApiFailException>(async () => await sut.DepositAsync(apiToken, order));
    }

    [Fact]
    public async Task 代收接口_GCash掃碼()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://alfapay63.com");
        var signer = new AlfaPayHasher(NullLogger<AlfaPayHasher>.Instance);

        var sut = new AlfaPayClient(httpClient, signer, NullLogger<AlfaPayClient>.Instance);

        var formId = Guid.Parse("1091386e-c9cc-4152-95fa-dfcf73c08026");
        var apiToken = "A2B0ZGwcPXFw8YB84mSrXnVVGQnwTImulmRxvXmsWxMKXaWBv2d3NbcwGxLc";

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
                        "trade_no": "6c46b82e-4c30-4388-924a-2a1ef111731b",
                        "out_trade_no": "1091386e-c9cc-4152-95fa-dfcf73c08026",
                        "amount": "500.00",
                        "uri": "https:\/\/payhub1255.com\/r\/dmp\/6c46b82e-4c30-4388-924a-2a1ef111731b",
                        "qrcode": "00020101021227830012com.p2pqrpay0111GXCHPHM2XXX02089996440303152170200000006560417DWQM4TK3JDO05ZDU05204601653036085406500.005802PH5907AlfaPay6008MALANDAY610412346304F268"
                    },
                    "success": true
                }
                """));

        var order = new DepositOrder(
            500.00m,
            formId,
            new Uri("https://dev-payment.rpgp.cc/alfapay/deposit/callback"),
            null);

        var actual = await sut.DepositAsync(apiToken, order);

        Assert.Equal("6c46b82e-4c30-4388-924a-2a1ef111731b", actual.ThirdPartyOrderNo);
        Assert.Equal(new Uri("https://payhub1255.com/r/dmp/6c46b82e-4c30-4388-924a-2a1ef111731b"), actual.RedirectUrl);
    }

    [Fact]
    public async Task 代收查詢接口()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://alfapay63.com");
        var signer = new AlfaPayHasher(NullLogger<AlfaPayHasher>.Instance);

        var sut = new AlfaPayClient(httpClient, signer, NullLogger<AlfaPayClient>.Instance);

        var formId = Guid.Parse("1091386e-c9cc-4152-95fa-dfcf73c08026");
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

        var actual = await sut.QueryDepositFormAsync(apiToken, formId);

        Assert.Equal("timeout", actual.State);
    }

    [Fact]
    public async Task 餘額查詢()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://alfapay63.com");
        var signer = new AlfaPayHasher(NullLogger<AlfaPayHasher>.Instance);

        var sut = new AlfaPayClient(httpClient, signer, NullLogger<AlfaPayClient>.Instance);

        var apiToken = "A2B0ZGwcPXFw8YB84mSrXnVVGQnwTImulmRxvXmsWxMKXaWBv2d3NbcwGxLc";

        _ = mockHttp.Expect(HttpMethod.Get, "https://alfapay63.com/api/balance/inquiry")
            .WithHeaders(new Dictionary<string, string>
            {
                { "Accept", "application/json" },
                { "Authorization", $"Bearer {apiToken}" }
            })
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "data": {
                        "balance": "0"
                    },
                    "success": true
                }
                """));

        var actual = await sut.QueryBalanceAsync(apiToken);

        Assert.Equal(0m, actual);
    }

    [Fact]
    public async Task 代付接口_GCash()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://alfapay63.com");
        var signer = new AlfaPayHasher(NullLogger<AlfaPayHasher>.Instance);

        var sut = new AlfaPayClient(httpClient, signer, NullLogger<AlfaPayClient>.Instance);

        var formId = Guid.Parse("b9a48264-fed8-4286-98b1-053c6e3a2274");
        var apiToken = "A2B0ZGwcPXFw8YB84mSrXnVVGQnwTImulmRxvXmsWxMKXaWBv2d3NbcwGxLc";
        var notifyToken = "DzBwQEyXd2OZZ8IFWrQCWunnmd0vZHqrKeKOaxxNe2gpS5ZSYx2KdFext74J";

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

        var order = new WithdrawOrder(
            formId,
            100.00m,
            new Uri("https://dev-payment.rpgp.cc/alfapay/withdraw/callback"),
            "GCASH",
            "9517585997",
            "測試帳號");

        var actual = await sut.WithdrawAsync(apiToken, notifyToken, order);

        Assert.Equal("42e74452-503d-4de8-ad21-aa879a3af6e9", actual.ThirdPartyOrderNo);
    }
}
