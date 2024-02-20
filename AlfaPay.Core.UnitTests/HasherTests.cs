using Microsoft.Extensions.Logging.Abstractions;

namespace Tgs.Payment.AlfaPay.Tests;

public class HasherTests
{
    [Fact]
    public void 代付_GCash()
    {
        var apiToken = "A2B0ZGwcPXFw8YB84mSrXnVVGQnwTImulmRxvXmsWxMKXaWBv2d3NbcwGxLc";
        var notifyToken = "DzBwQEyXd2OZZ8IFWrQCWunnmd0vZHqrKeKOaxxNe2gpS5ZSYx2KdFext74J";

        var dict = new Dictionary<string, string?>
        {
            ["account_number"] = "9517585997",
            ["amount"] = "100",
            ["bank_id"] = "GCASH",
            ["bank_owner"] = "測試帳號",
            ["callback_url"] = "https://dev-payment.rpgp.cc/alfapay/withdraw/callback",
            ["out_trade_no"] = "b9a48264-fed8-4286-98b1-053c6e3a2274",
        };
        var expected = "8b8113c6ff1c03d0d84551956d2bdea0";

        var sut = new AlfaPayHasher(NullLogger<AlfaPayHasher>.Instance);

        var actual = sut.ComputeHash(apiToken, notifyToken, dict);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 代收回調()
    {
        var apiToken = "A2B0ZGwcPXFw8YB84mSrXnVVGQnwTImulmRxvXmsWxMKXaWBv2d3NbcwGxLc";
        var notifyToken = "DzBwQEyXd2OZZ8IFWrQCWunnmd0vZHqrKeKOaxxNe2gpS5ZSYx2KdFext74J";

        var dict = new Dictionary<string, string?>
        {
            ["trade_no"] = "0a14d1d4-4740-4f4c-9173-1b40d5776910",
            ["request_amount"] = "500",
            ["amount"] = "500.00",
            ["out_trade_no"] = "1f194dc3-b2fe-4b1c-9f91-65c92f20a402",
            ["state"] = "completed",
            ["callback_url"] = "https://dev-payment.rpgp.cc/alfapay/deposit/callback",
            ["sign"] = "d91ba7c240a3262c1995a9601c3acf78",
        };

        _ = dict.Remove("callback_url");

        var expected = "d91ba7c240a3262c1995a9601c3acf78";

        var sut = new AlfaPayHasher(NullLogger<AlfaPayHasher>.Instance);

        var actual = sut.ComputeHash(apiToken, notifyToken, dict);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 代付回調()
    {
        var apiToken = "A2B0ZGwcPXFw8YB84mSrXnVVGQnwTImulmRxvXmsWxMKXaWBv2d3NbcwGxLc";
        var notifyToken = "DzBwQEyXd2OZZ8IFWrQCWunnmd0vZHqrKeKOaxxNe2gpS5ZSYx2KdFext74J";

        var dict = new Dictionary<string, string?>
        {
            ["trade_no"] = "42e74452-503d-4de8-ad21-aa879a3af6e9",
            ["amount"] = "100.00",
            ["out_trade_no"] = "b9a48264-fed8-4286-98b1-053c6e3a2274",
            ["state"] = "refund",
            ["sign"] = "09b9da81dfe0f5a32cfa8fee283a2b69",
        };

        var expected = "09b9da81dfe0f5a32cfa8fee283a2b69";

        var sut = new AlfaPayHasher(NullLogger<AlfaPayHasher>.Instance);

        var actual = sut.ComputeHash(apiToken, notifyToken, dict);

        Assert.Equal(expected, actual);
    }
}
