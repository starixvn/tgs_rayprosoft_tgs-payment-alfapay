namespace Tgs.Payment.AlfaPay;

internal interface IAlfaPayClient
{
    ValueTask<AlfaPayDepositResponse?> DepositAsync(string apiToken, DepositOrder order);

    ValueTask<AlfaPayClientDepositInfo?> QueryDepositFormAsync(string apiToken, Guid outTradeNo);

    ValueTask<string?> WithdrawAsync(string apiToken, string notifyToken, WithdrawOrder order);
}
