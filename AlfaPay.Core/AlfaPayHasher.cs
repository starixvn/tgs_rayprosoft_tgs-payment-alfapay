using System.Security.Cryptography;
using System.Text;

namespace Tgs.Payment.AlfaPay;

public class AlfaPayHasher
{
    private static readonly MD5 _Md5;

    static AlfaPayHasher()
    {
        _Md5 = MD5.Create();
    }

    public static string ComputeHash(string apiToken, string notifyToken, AlfaPayPaymentType paymentType, IDictionary<string, object> dict)
    {
        var sortDict = paymentType == AlfaPayPaymentType.Deposit
            ? dict
                .Where(x => x.Key is not "sign" and not "callback_url")
                .OrderBy(x => x.Key)
                .Select(p => p.Value is null
                    ? $"{p.Key}="
                    : $"{p.Key}={p.Value}")
            : dict
                .Where(x => x.Key != "sign")
                .OrderBy(x => x.Key)
                .Select(p => p.Key == "amount"
                    ? p.Value is null
                        ? $"{p.Key}="
                        : $"{p.Key}={decimal.Parse(p.Value.ToString()!)}"
                    : p.Value is null
                        ? $"{p.Key}="
                        : $"{p.Key}={p.Value}");
        var content = string.Join("&", sortDict);

        var plainText = $"{content}{apiToken}{notifyToken}";

        var data = Encoding.UTF8.GetBytes(plainText);

        var hash = _Md5.ComputeHash(data);

        return Convert.ToHexString(hash).ToLower();
    }
}
