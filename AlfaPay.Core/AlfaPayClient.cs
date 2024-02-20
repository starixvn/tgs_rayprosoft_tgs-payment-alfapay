using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace Tgs.Payment.AlfaPay;

public class AlfaPayClient(
    HttpClient httpClient,
    AlfaPayHasher hasher,
    ILogger<AlfaPayClient> logger)
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly AlfaPayHasher _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
    private readonly ILogger<AlfaPayClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async ValueTask<AlfaPayClientDepositResponse> DepositAsync(
        string apiToken,
        DepositOrder order,
        CancellationToken cancellationToken = default)
    {
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

        var dict = new Dictionary<string, string?>
        {
            ["amount"] = order.Amount.ToString(),
            ["callback_url"] = order.CallbackUrl.AbsoluteUri,
            ["out_trade_no"] = order.FormId.ToString(),
        };

        if (order.PaidName is not null)
            dict["paid_name"] = order.PaidName.ToString();

        var response = await _httpClient
            .PostAsJsonAsync("/api/transaction", dict, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("AlfaPayClient Deposit response Body:{bodyJson}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Query Deposit response can't read.");

        if (node["success"]!.GetValue<bool>() != true)
            throw new ApiFailException(node["message"]!.ToString());

        var data = node["data"]!;

        return new AlfaPayClientDepositResponse(
            Amount: decimal.Parse(data["amount"]!.GetValue<string>()),
            FormId: Guid.Parse(data["out_trade_no"]!.GetValue<string>()),
            ThirdPartyOrderNo: data["trade_no"]!.GetValue<string>(),
            RedirectUrl: new Uri(data["uri"]!.GetValue<string>()));
    }

    public async ValueTask<AlfaPayClientDepositInfo> QueryDepositFormAsync(
        string apiToken,
        Guid formId,
        CancellationToken cancellationToken = default)
    {
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

        var response = await _httpClient
            .GetAsync($"/api/transaction/{formId}", cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("AlfaPayClient QueryDepositForm response Body:{bodyJson}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Query Deposit response can't read.");

        if (node["success"]!.GetValue<bool>() != true)
            throw new ApiFailException(node["message"]!.ToString());

        var data = node["data"]!;

        return new AlfaPayClientDepositInfo(data["state"]!.GetValue<string>());
    }

    public async ValueTask<AlfaPayClientWithdrawResponse> WithdrawAsync(string apiToken, string notifyToken, WithdrawOrder order, CancellationToken cancellationToken = default)
    {
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

        var dict = new Dictionary<string, string?>
        {
            ["out_trade_no"] = order.FormId.ToString(),
            ["bank_id"] = order.BankId,
            ["bank_owner"] = order.BankOwner,
            ["account_number"] = order.AccountNumber,
            ["amount"] = order.Amount.ToString("#.##"),
            ["callback_url"] = order.CallbackUrl.AbsoluteUri,
        };

        var sign = _hasher.ComputeHash(apiToken, notifyToken, dict);

        dict["sign"] = sign;

        _logger.LogInformation("AlfaPay Withdraw request Send sign:{sign}", sign);

        var response = await _httpClient
            .PostAsJsonAsync("/api/payment", dict, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("AlfaPayClient Withdraw response Body:{bodyJson}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Query Deposit response can't read.");

        if (node["success"]!.GetValue<bool>() != true)
            throw new ApiFailException(node["message"]!.ToString());

        var data = node["data"]!;

        return new AlfaPayClientWithdrawResponse(
            FormId: Guid.Parse(data["out_trade_no"]!.GetValue<string>()),
            ThirdPartyOrderNo: data["trade_no"]!.GetValue<string>(),
            Amount: data["amount"]!.GetValue<decimal>());
    }

    public async ValueTask<decimal> QueryBalanceAsync(string apiToken, CancellationToken cancellationToken = default)
    {
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

        var response = await _httpClient
            .GetAsync("/api/balance/inquiry", cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("AlfaPayClient Query Balance response Body:{bodyJson}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Query Balance response can't read.");

        if (node["success"]!.GetValue<bool>() != true)
            throw new ApiFailException(node["message"]!.ToString());

        var data = node["data"]!;

        return decimal.Parse(data["balance"]!.GetValue<string>());
    }
}
