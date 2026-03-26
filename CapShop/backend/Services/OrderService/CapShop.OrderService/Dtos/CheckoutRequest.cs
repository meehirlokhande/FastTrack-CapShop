namespace CapShop.OrderService.Dtos;

public class CheckoutRequest
{
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingPincode { get; set; } = string.Empty;
}