using AlfaPayTest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

CreateHostBuilder(args).Build().Run();

static IHostBuilder CreateHostBuilder(string[] args)
    => Host.CreateDefaultBuilder(args)
        .ConfigureServices(services =>
        {
            _ = services
            .AddMessageQueue()
            .AddNatsMessageQueue(_ => { })
            .AddNatsGlobPatternExchange("*");

            _ = services
            .AddPaymentClient()
            .AddHostedService<Demo>();
        });
