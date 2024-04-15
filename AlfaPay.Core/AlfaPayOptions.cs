namespace Tgs.Payment.AlfaPay;

public class AlfaPayOptions
{
    public required string PaymentCode { get; set; }

    public required Uri CallbackBaseUrl { get; set; }

    public required Uri PaymentApiUrl { get; set; }

    public required Uri PaymentServiceBaseUrl { get; set; }
}