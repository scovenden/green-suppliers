namespace GreenSuppliers.Api.Models.DTOs;

public class CheckoutRequest
{
    public Guid PlanId { get; set; }
    public string BillingCycle { get; set; } = "monthly";
}
