using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Tgs.Payment.AlfaPay.Tests;

public class HasherTests
{
    [Test]
    public void Test_Hash_Withdraw_Sample()
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

        var actual = AlfaPayHasher.ComputeHash(apiToken, notifyToken, AlfaPayPaymentType.Withdraw, dict);

        var expected = "603e51a46389f7bfd0d4470e349ab5cb";

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Withdraw_資料中有空白_應該Trim再運算()
    {
        var accountNumber = "6212262000000000001";
        var amount = "500";
        var bankId = "GCASH";
        var bankOwner = "DAO VAN THANG ";
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

        var actual = AlfaPayHasher.ComputeHash(apiToken, notifyToken, AlfaPayPaymentType.Withdraw, dict);

        var expected = "603e51a46389f7bfd0d4470e349ab5cb";

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Test_Hash_Deposit_Callback()
    {
        var json = "{\"trade_no\":\"d835f6c0-9881-453f-ad04-d963cae3698c\",\"amount\":\"580.00\",\"request_amount\":\"580\",\"out_trade_no\":\"5993ee9d-4712-4cc6-acc4-45f575189657\",\"state\":\"completed\",\"callback_url\":\"https://example.com/notify\",\"sign\":\"1ed30abda08395adb9cacabca1d669ad\"}";
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;

        var apiToken = "3p3FCtslWkFu9iFkAnFjOKRLWZoqAy58iaDldaCZ1M4jNmyVJA0fhkSe5IIv";
        var notifyToken = "sPknBbCd38DZWHhEZ2DyHFdyvgkDwULFl7tefGILMYwOUIaFjhqIsUps3T98";

        var actual = AlfaPayHasher.ComputeHash(apiToken, notifyToken, AlfaPayPaymentType.Deposit, dict);

        var expected = "77211c6cbb8079b8cb0635ce3619b407";

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Test_Hash_Deposit_Real_Sample()
    {
        var plainText = "amount=580.00&out_trade_no=5993ee9d-4712-4cc6-acc4-45f575189657&request_amount=580&state=completed&trade_no=d835f6c0-9881-453f-ad04-d963cae3698c3p3FCtslWkFu9iFkAnFjOKRLWZoqAy58iaDldaCZ1M4jNmyVJA0fhkSe5IIvsPknBbCd38DZWHhEZ2DyHFdyvgkDwULFl7tefGILMYwOUIaFjhqIsUps3T98";

        var data = Encoding.UTF8.GetBytes(plainText);
        var hash = MD5.Create().ComputeHash(data);

        var actual = Convert.ToHexString(hash).ToLower();

        var expected = "77211c6cbb8079b8cb0635ce3619b407";
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Test_Deposit_Callback_Hash()
    {
        var json = "{\"trade_no\":\"d835f6c0-9881-453f-ad04-d963cae3698c\",\"request_amount\":580,\"amount\":\"580.00\",\"out_trade_no\":\"5993ee9d-4712-4cc6-acc4-45f575189657\",\"state\":\"completed\",\"sign\":\"77211c6cbb8079b8cb0635ce3619b407\"}";
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;

        var apiToken = "3p3FCtslWkFu9iFkAnFjOKRLWZoqAy58iaDldaCZ1M4jNmyVJA0fhkSe5IIv";
        var notifyToken = "sPknBbCd38DZWHhEZ2DyHFdyvgkDwULFl7tefGILMYwOUIaFjhqIsUps3T98";
        var actual = AlfaPayHasher.ComputeHash(apiToken, notifyToken, AlfaPayPaymentType.Deposit, dict);

        var expected = "77211c6cbb8079b8cb0635ce3619b407";
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Test_Hash_Call_Client_Withdraw_Real_Sample()
    {
        var accountNumber = "099517585997";
        var amount = "200";
        var bankId = "GCASH";
        var bankOwner = "\u738B\u5C0F\u660E";
        var callBackUrl = "https://dev-payment.rpgp.cc/alfapay/withdraw/callback";
        var outTradeNo = "cb9426c9-d6fe-4834-8ef7-9611aa94010a";
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

        var apiToken = "3p3FCtslWkFu9iFkAnFjOKRLWZoqAy58iaDldaCZ1M4jNmyVJA0fhkSe5IIv";
        var notifyToken = "sPknBbCd38DZWHhEZ2DyHFdyvgkDwULFl7tefGILMYwOUIaFjhqIsUps3T98";

        var actual = AlfaPayHasher.ComputeHash(apiToken, notifyToken, AlfaPayPaymentType.Withdraw, dict);

        var expected = "8756aa397416e8424d32ba4316bba405";

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Test_Hash_Withdraw_Callback_Real_Sample()
    {
        var tradeNo = "e93bcdee-3747-4bf6-af6f-1e505c95061e";
        var amount = "200.00";
        var outTradeNo = "9942663e-fbc6-4753-b672-d2b4b4ad807f";
        var state = "completed";
        var sign = "401fa63b599d22fb8fbee22723fdb76e";
        var dict = new Dictionary<string, object>
        {
            { "trade_no", tradeNo },
            { "amount",amount },
            { "out_trade_no", outTradeNo},
            { "state", state},
            { "sign", sign}
        };

        var apiToken = "3p3FCtslWkFu9iFkAnFjOKRLWZoqAy58iaDldaCZ1M4jNmyVJA0fhkSe5IIv";
        var notifyToken = "sPknBbCd38DZWHhEZ2DyHFdyvgkDwULFl7tefGILMYwOUIaFjhqIsUps3T98";
        var actual = AlfaPayHasher.ComputeHash(apiToken, notifyToken, AlfaPayPaymentType.Withdraw, dict);

        var expected = sign;

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Test_Withdraw_Fail()
    {
        var json = "{\"success\": false,\"status_code\": 422,\"message\": \"validation error\",\"errors\": {\"out_trade_no\": [\"out trade no 栏位为必填\"]}}";
        var actual = JsonSerializer.Deserialize<AlfaPayWithdrawFailResponse>(json);
        Assert.That(actual, Is.Not.Null);
    }
}
