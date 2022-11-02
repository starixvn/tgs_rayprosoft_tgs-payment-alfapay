using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Tgs.Payment.AlfaPay;
using Tgs.Payment.AlfaPay.MessageQueue;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAlfaPay(
        this IServiceCollection services,
        Action<AlfaPayOptions,
        IServiceProvider> configure)
        => services
            .RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
            .AddOptions<AlfaPayOptions>()
            .Configure(configure)
            .Services
            .AddHttpClient<AlfaPayClient>()
            .ConfigureHttpClient((sp, httpClient) =>
            {
                var options = sp.GetRequiredService<IOptions<AlfaPayOptions>>().Value;

                httpClient.BaseAddress = options.PaymentApiUrl;
            })
            .Services            
            .AddSingleton<AlfaPayHasher>()
            .AddPaymentClient()
            .AddPaymentServiceClient((options, sp) =>
            {
                var AlfaPayOptions = sp.GetRequiredService<IOptions<AlfaPayOptions>>().Value;

                options.PaymentServiceBaseUrl = AlfaPayOptions.PaymentServiceBaseUrl;

                options.JsonSerializerOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters =
                    {
                        new JsonStringEnumConverter()
                    }
                };
            })
            .AddMessageQueue()
            .AddNatsMessageQueue(config =>
            {
                _ = config
                    .ConfigQueueOptions((options, sp) =>
                    {
                        var alfaPayOptions = sp.GetRequiredService<IOptions<AlfaPayOptions>>().Value;

                        options.Url = alfaPayOptions.NatsUrl;
                    })
                    .AddProcessor<DepositProcessor>("tgs.payment.alfapay.deposit", "tgs.payment.alfapay.deposit")
                    .AddProcessor<WithdrawProcessor>("tgs.payment.alfapay.withdraw", "tgs.payment.alfapay.withdraw");
            })
            .ClearAllExchanges()
            .AddNatsGlobPatternExchange("*")
            .Services;
}
