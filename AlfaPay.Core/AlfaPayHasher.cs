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

        public static string ComputeHash(string apiToken, string? notifyToken, IDictionary<string, object> dict)
        {
            var content = string.Join(
                "&",
                dict.Where(x => x.Key is not "sign")
                    .OrderBy(x => x.Key)
                    .Select(p =>
                    {
                        return p.Value is null ? $"{p.Key}=" : $"{p.Key}={p.Value}";
                    }));

            var plainText = $"{content}{apiToken}{notifyToken}";

            var data = Encoding.UTF8.GetBytes(plainText);

            var hash = _Md5.ComputeHash(data);

            return Convert.ToHexString(hash).ToLower();
        }
    }
}
