using System.Reflection;
using Tgs.Payment.AlfaPay;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAlfaPay(
        this IServiceCollection services,
        Action<AlfaPayOptions, IServiceProvider> configureOptions,
        Action<IHttpClientBuilder> ConfigureAiClient)
    {
        ConfigureAiClient?.Invoke(services.AddHttpClient<AlfaPayClient>());

        return services
            .AddOptions<AlfaPayOptions>()
            .Configure(configureOptions)
            .Services
            .RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
            .AddSingleton<AlfaPayHasher>();
    }
}
