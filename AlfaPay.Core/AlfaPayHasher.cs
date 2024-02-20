using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Tgs.Payment.AlfaPay;

public class AlfaPayHasher(ILogger<AlfaPayHasher> logger)
{
    private readonly ILogger<AlfaPayHasher> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public string ComputeHash(string apiToken, string notifyToken, IEnumerable<KeyValuePair<string, string?>> dict)
    {
        var content = string.Join(
            "&",
            dict.Where(kvp => string.IsNullOrWhiteSpace(kvp.Value) is false && kvp.Key != "sign")
                .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
                .Select(p => $"{p.Key}={p.Value}"));

        var fullText = $"{content}{apiToken}{notifyToken}";

        _logger.LogTrace("FullText: {fullText}", fullText);

        var data = MD5.HashData(Encoding.UTF8.GetBytes(fullText));

        return Convert.ToHexString(data).ToLower();
    }
}
