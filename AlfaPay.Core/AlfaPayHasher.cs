using System.Security.Cryptography;
using System.Text;

namespace Tgs.Payment.AlfaPay
{
    public class AlfaPayHasher
    {
        private static readonly MD5 _Md5;

        static AlfaPayHasher()
        {
            _Md5 = MD5.Create();
        }

        public static string ComputeHash(string apiToken, string notifyToken, AlfaPayPaymentType paymentType, IDictionary<string, object> dict)
        {
            IEnumerable<string?> sortDict;
            if (paymentType == AlfaPayPaymentType.Deposit)
            {
                sortDict = dict
                    .Where(x => x.Key != "sign" && x.Key != "callback_url")
                    .OrderBy(x => x.Key)
                    .Select(p =>
                    {
                        return p.Value is null ? $"{p.Key}=" : $"{p.Key}={p.Value}";
                    });
            }
            else
            {
                sortDict = dict
                    .Where(x => x.Key != "sign")
                    .OrderBy(x => x.Key)
                    .Select(p =>
                    {
                        if (p.Key == "amount")
                        {
                            return p.Value is null ? $"{p.Key}=" : $"{p.Key}={decimal.Parse(p.Value.ToString()!)}";
                        }

                        return p.Value is null ? $"{p.Key}=" : $"{p.Key}={p.Value}";
                    });
            }

            var content = string.Join("&", sortDict);

            var plainText = $"{content}{apiToken}{notifyToken}";

            var data = Encoding.UTF8.GetBytes(plainText);

            var hash = _Md5.ComputeHash(data);

            return Convert.ToHexString(hash).ToLower();
        }
    }
}
