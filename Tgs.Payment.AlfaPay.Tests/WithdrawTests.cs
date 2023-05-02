using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using RichardSzalay.MockHttp;
using Tgs.Payment.AlfaPay;

namespace AlfaPay.Core.UnitTests
{
    public class WithdrawTests
    {
        [Test]
        public async Task 使用直連參數代付成功()
        {
            var apiUrl = new Uri("https://tiancip020706.com");

            var url = new Uri(apiUrl, $"/api/payment");

            var apiToken直連 = "3p3FCtslWkFu9iFkAnFjOKRLWZoqAy58iaDldaCZ1M4jNmyVJA0fhkSe5IIv";

            var notifyToken直連 = "sPknBbCd38DZWHhEZ2DyHFdyvgkDwULFl7tefGILMYwOUIaFjhqIsUps3T98";

            var response = new
            {
                success = true,
                data = new
                {
                    trade_no = "0454e2e1-153b-43ff-bf5d-193cef0a0536",
                    out_trade_no = "e01f42d5-334a-4e7a-a2c4-66662c554707",
                    bank_id = "GCASH",
                    bank_owner = "王小明",
                    account_number = "9121234567",
                    amount = 100,
                    callback_url = "https://dev-payment.rpgp.cc/alfapay/withdraw/callback",
                    state = "new"
                },
            };

            var content = JsonSerializer.Serialize(response);

            var handler = new MockHttpMessageHandler();

            _ = handler.Expect(HttpMethod.Post, url.ToString())
                .With(req => req.Content?.Headers.ContentType?.MediaType == "application/json")
                .Respond(HttpStatusCode.OK, new StringContent(content, Encoding.UTF8, MediaTypeNames.Application.Json));

            var fakeHttpClient = handler.ToHttpClient();
            fakeHttpClient.BaseAddress = apiUrl;

            var sut = new AlfaPayClient(fakeHttpClient, NullLogger<AlfaPayClient>.Instance);

            var order = new WithdrawOrder
            {
                AccountNumber = "9121234567",
                Amount = 100M,
                BankId = "GCASH",
                BankOwner = "王小明",
                CallbackUrl = "https://dev-payment.rpgp.cc/alfapay/withdraw/callback",
                OutTradeNo = "e01f42d5-334a-4e7a-a2c4-66662c554707",
            };

            var actual = await sut
                .WithdrawAsync(apiToken直連, notifyToken直連, order)
                .ConfigureAwait(false);

            Assert.True(actual.Success);
        }

        [Test]
        public void 使用QR參數代付失敗()
        {
            var apiUrl = new Uri("https://tiancip020706.com");

            var url = new Uri(apiUrl, $"/api/payment");

            var apiTokenQR = "sUxtpDWiNoqUxKfqJoHCL69TS4omJRB1fBoaw4PfJT4Boncf8cLj6lYtN5DS";

            var notifyTokenQR = "1zLFGrXi6qYF91Iw7J5nWKRA5egICj2xN4fiVMcoTv2GR68PUKp9WCdNAaPM";

            var response = new
            {
                success = false,
                status_code = 422,
                message = "validation error",
                errors = new
                {
                    amount = "餘額不足"
                }
            };

            var content = JsonSerializer.Serialize(response);

            var handler = new MockHttpMessageHandler();

            _ = handler.Expect(HttpMethod.Post, url.ToString())
                .With(req => req.Content?.Headers.ContentType?.MediaType == "application/json")
                .Respond(HttpStatusCode.OK, new StringContent(content, Encoding.UTF8, MediaTypeNames.Application.Json));

            var fakeHttpClient = handler.ToHttpClient();
            fakeHttpClient.BaseAddress = apiUrl;

            var sut = new AlfaPayClient(fakeHttpClient, NullLogger<AlfaPayClient>.Instance);

            var order = new WithdrawOrder
            {
                AccountNumber = "9217729028",
                Amount = 2000M,
                BankId = "GCASH",
                BankOwner = "Richard dennison traquina",
                CallbackUrl = "https://dev-payment.rpgp.cc/alfapay/withdraw/callback",
                OutTradeNo = "31fd8805-86dc-4335-b1c1-191242e902e6",
            };

            _ = Assert.ThrowsAsync<ApiFailException>(async () => await sut
                .WithdrawAsync(apiTokenQR, notifyTokenQR, order)
                .ConfigureAwait(false));
        }

        [Test]
        public void 使用直連參數代付失敗_帳號輸入錯誤()
        {
            var apiUrl = new Uri("https://tiancip020706.com");

            var url = new Uri(apiUrl, $"/api/payment");

            var apiToken直連 = "3p3FCtslWkFu9iFkAnFjOKRLWZoqAy58iaDldaCZ1M4jNmyVJA0fhkSe5IIv";

            var notifyToken直連 = "sPknBbCd38DZWHhEZ2DyHFdyvgkDwULFl7tefGILMYwOUIaFjhqIsUps3T98";

            var response = new
            {
                success = false,
                status_code = 422,
                message = "validation error",
                errors = new
                {
                    account_number = new string[]
                    {
                        "The account number format is invalid."
                    }
                }
            };

            var content = JsonSerializer.Serialize(response);

            var handler = new MockHttpMessageHandler();

            _ = handler.Expect(HttpMethod.Post, url.ToString())
                .With(req => req.Content?.Headers.ContentType?.MediaType == "application/json")
                .Respond(HttpStatusCode.OK, new StringContent(content, Encoding.UTF8, MediaTypeNames.Application.Json));

            var fakeHttpClient = handler.ToHttpClient();
            fakeHttpClient.BaseAddress = apiUrl;

            var sut = new AlfaPayClient(fakeHttpClient, NullLogger<AlfaPayClient>.Instance);

            var order = new WithdrawOrder
            {
                AccountNumber = "0927008066",
                Amount = 100M,
                BankId = "GCASH",
                BankOwner = "cheng chong boon",
                CallbackUrl = "https://dev-payment.rpgp.cc/alfapay/withdraw/callback",
                OutTradeNo = "9d7234f0-91bc-4082-a127-32416df96eb4",
            };

            _ = Assert.ThrowsAsync<ApiFailException>(async () => await sut
                .WithdrawAsync(apiToken直連, notifyToken直連, order)
                .ConfigureAwait(false));
        }
    }
}
