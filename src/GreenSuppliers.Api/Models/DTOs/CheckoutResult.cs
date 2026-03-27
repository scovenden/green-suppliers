namespace GreenSuppliers.Api.Models.DTOs;

public class CheckoutResult
{
    public Guid SubscriptionId { get; set; }
    public Guid PaymentId { get; set; }
    public string CheckoutUrl { get; set; } = string.Empty;
}
