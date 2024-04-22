using System.Text.Json.Serialization;

namespace Tgs.Payment.AlfaPay.Client;

public record AlfaPayDepositReturn
{
    public string ThirdPartyOrderNo { get; init; } = default!;

    public decimal OrderAmount { get; init; }
    public decimal RealAmount { get; init; }
    public Guid FormId { get; init; }

    public string Status { get; init; } = default!;

    public string Sign { get; init; } = default!;
    public string CallbackUrl { get; init; } = default!;

    public decimal AfterExchangeOrderAmount { get; set; }

    public decimal AfterExchangeRealAmount { get; set; }
}