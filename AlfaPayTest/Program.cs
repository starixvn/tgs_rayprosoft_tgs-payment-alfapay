using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NATS.Client.Core;
using Tgs.Payment.AlfaPay;
using Tgs.Payment.AlfaPay.Client;
using Tgs.Payment.Clients;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddPaymentClient()
    .AddKeyedSingleton("Nats",
    (sp, _) => new NatsConnection(NatsOpts.Default with
    {
        Url = "nats://localhost:4222"
    }))
    .AddPhoenixToolkitsMessageQueue()
    .AddNatsMessageQueue(natsConfig => natsConfig
        .ConfigureResolveConnection(sp => sp.GetRequiredKeyedService<NatsConnection>("Nats")))
    .AddNatsGlobPatternExchange("*");

var app = builder.Build();

await app.StartAsync().ConfigureAwait(false);

var logger = app.Services.GetRequiredService<ILogger<Program>>();

var paymentClient = app.Services.GetRequiredService<IPaymentClient>();

//代收接口(H5-網銀掃碼)
//var h5ApiToken = "A2B0ZGwcPXFw8YB84mSrXnVVGQnwTImulmRxvXmsWxMKXaWBv2d3NbcwGxLc";
//var h5NotitfyToken = "DzBwQEyXd2OZZ8IFWrQCWunnmd0vZHqrKeKOaxxNe2gpS5ZSYx2KdFext74J";
//var orderNo = Guid.NewGuid();
//var amount = 500.00m;

//var depositSettings = new AlfaPayDepositSettings(
//    h5ApiToken,
//    h5NotitfyToken,
//    orderNo,
//    PaidName: null);

//var depositReturn = await paymentClient.DepositAsync(
//    "tgs.payment.alfapay.deposit",
//    amount,
//    JsonSerializer.Serialize(depositSettings));
//logger.LogInformation($"Deposit Return: {depositReturn}");

//代收查詢
//var queryApiToken = "A2B0ZGwcPXFw8YB84mSrXnVVGQnwTImulmRxvXmsWxMKXaWBv2d3NbcwGxLc";
//var formId = Guid.Parse("1091386e-c9cc-4152-95fa-dfcf73c08026");

//var httpClient = new HttpClient
//{
//    BaseAddress = new Uri("https://alfapay63.com")
//};
//var client = new AlfaPayClient(httpClient, new AlfaPayHasher(NullLogger<AlfaPayHasher>.Instance), NullLogger<AlfaPayClient>.Instance);
//var query = await client.QueryDepositFormAsync(queryApiToken, formId);

//logger.LogInformation("{query}", query);

//餘額查詢
//var balanceApiToken = "A2B0ZGwcPXFw8YB84mSrXnVVGQnwTImulmRxvXmsWxMKXaWBv2d3NbcwGxLc";

//var httpClient = new HttpClient
//{
//    BaseAddress = new Uri("https://alfapay63.com")
//};
//var client = new AlfaPayClient(httpClient, new AlfaPayHasher(NullLogger<AlfaPayHasher>.Instance), NullLogger<AlfaPayClient>.Instance);
//var query = await client.QueryBalanceAsync(balanceApiToken);

//logger.LogInformation("{query}", query);

//代付接口
var apiToken = "A2B0ZGwcPXFw8YB84mSrXnVVGQnwTImulmRxvXmsWxMKXaWBv2d3NbcwGxLc";
var notifyToken = "DzBwQEyXd2OZZ8IFWrQCWunnmd0vZHqrKeKOaxxNe2gpS5ZSYx2KdFext74J";
var accountNumber = "9517585997";
var bankId = "GCASH";
var bankOwner = "測試帳號";
var orderNo = Guid.NewGuid();
var amount = 100M;

var withdrawSettings = new AlfaPayWithdrawSettings(
    apiToken,
    notifyToken,
    accountNumber,
    bankId,
    bankOwner,
    orderNo);

var withdrawReturn = await paymentClient.WithdrawAsync(
                "tgs.payment.alfapay.withdraw",
                amount,
                JsonSerializer.Serialize(withdrawSettings));
logger.LogInformation($"Withdraw Return: {withdrawReturn}");
