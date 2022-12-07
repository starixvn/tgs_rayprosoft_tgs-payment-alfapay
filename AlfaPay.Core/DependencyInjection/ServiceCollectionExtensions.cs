using System.Reflection;
using Microsoft.Extensions.Options;
using Tgs.Payment.AlfaPay;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAlfaPay(
        this IServiceCollection services,
        Action<AlfaPayOptions,
        IServiceProvider> configure,
        Func<IServiceProvider, DelegatingHandler> httpClientMessageHandlerFactory)
        => services
            .AddOptions<AlfaPayOptions>()
            .Configure(configure)
            .Services
            .AddHttpClient<IAlfaPayClient, AlfaPayClient>()
            .ConfigureHttpClient((sp, httpClient) =>
            {
                var options = sp.GetRequiredService<IOptions<AlfaPayOptions>>().Value;

                httpClient.BaseAddress = options.PaymentApiUrl;
            })
            .AddHttpMessageHandler(httpClientMessageHandlerFactory)
            .Services
            .RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
            .AddPaymentClient()
            .AddPaymentServiceClient(httpClientBuilder => httpClientBuilder
                .ConfigureHttpClient((sp, http) =>
                {
                    var skyPayOptions = sp.GetRequiredService<IOptions<AlfaPayOptions>>().Value;

                    http.BaseAddress = skyPayOptions.PaymentServiceBaseUrl;
                })
                .AddHttpMessageHandler(httpClientMessageHandlerFactory))
            .AddSingleton<AlfaPayHasher>()
            .AddSingleton<AlfaPaySerializer>();
}
