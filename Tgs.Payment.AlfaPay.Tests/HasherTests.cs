using Tgs.Payment.AlfaPay;

namespace Tgs.Payment.AlfaPay.Tests
{
    public class HasherTests
    {
        [Test]
        public void Test_Hash()
        {
            var accountNumber = "6212262000000000001";
            var amount = "500";
            var bankId = "GCASH";
            var bankOwner = "DAO VAN THANG";
            var callBackUrl = "https://www.example.com/notify";
            var outTradeNo = "2aff3561-cac7-4643-ab93-ed62cf228d70";
            var sign = "603e51a46389f7bfd0d4470e349ab5cb";

            var dict = new Dictionary<string, object>
            {
                { "account_number",accountNumber },
                { "amount",amount },
                { "bank_id",bankId},
                { "bank_owner", bankOwner},
                { "callback_url", callBackUrl},
                { "out_trade_no", outTradeNo},
                { "sign", sign}
            };

            var apiToken = "api_token";
            var notifyToken = "notify_token";

            var actual = AlfaPayHasher.ComputeHash(apiToken, notifyToken, dict);

            var expected = "603e51a46389f7bfd0d4470e349ab5cb";

            Assert.AreEqual(expected, actual);
        }
    }
}
