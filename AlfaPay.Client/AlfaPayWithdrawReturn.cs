namespace Tgs.Payment.AlfaPay.Client;

public class AlfaPayWithdrawReturn
{
    public string ThirdPartyOrderNo { get; init; } = default!;

    public decimal Amount { get; init; }

    public Guid FormId { get; init; }

    public string Status { get; init; } = default!;

    public string Sign { get; init; } = default!;
}
