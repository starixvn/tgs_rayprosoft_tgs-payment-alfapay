using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Tgs.Payment.AlfaPay;

internal class AlfaPayClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AlfaPayClient> _logger;

    public AlfaPayClient(
        HttpClient httpClient,
        ILogger<AlfaPayClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask<AlfaPayDepositResponse?> DepositAsync(string apiToken, DepositOrder order)
    {
        var orderJson = JsonSerializer.Serialize(order);
        var orderDict = JsonSerializer
            .Deserialize<Dictionary<string, object>>(orderJson)!;

        _logger.LogInformation("AlfaPayClient Deposit apiToken:{apiToken}, orderJson:{orderJson}", apiToken, orderJson);

        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
        var content = new StringContent(JsonSerializer.Serialize(orderDict));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var response = await _httpClient
            .PostAsync("/api/transaction", content)
            .ConfigureAwait(false);

        var bodyJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        _logger.LogInformation("AlfaPay Deposit Body:{bodyJson}", bodyJson);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<AlfaPayDepositResponse>(bodyJson);
    }

    public async ValueTask<AlfaPayWithdrawResponse?> WithdrawAsync(string apiToken, string notifyToken, WithdrawOrder order)
    {
        var orderJson = JsonSerializer.Serialize(order);
        var orderDict = JsonSerializer
            .Deserialize<Dictionary<string, object>>(orderJson)!;

        _logger.LogInformation("AlfaPayClient Withdraw apiToken:{apiToken}, orderJson:{orderJson}", apiToken, orderJson);

        var sign = AlfaPayHasher.ComputeHash(apiToken, notifyToken, AlfaPayPaymentType.Withdraw, orderDict);

        orderDict["sign"] = sign;

        var paramDict = orderDict
            .AsEnumerable()
            .Select(p => new KeyValuePair<string?, string?>(p.Key, p.Value?.ToString()));

        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
        var content = new StringContent(JsonSerializer.Serialize(orderDict));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var response = await _httpClient
            .PostAsync("/api/payment", content)
            .ConfigureAwait(false);

        var bodyJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        _logger.LogInformation("AlfaPay Withdraw Body:{bodyJson}", bodyJson);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<AlfaPayWithdrawResponse>(bodyJson);
    }
}
