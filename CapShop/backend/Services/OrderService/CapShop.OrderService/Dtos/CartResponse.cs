namespace CapShop.OrderService.Dtos;

public class CartResponse
{
    public List<CartItemResponse> Items { get; set; } = new();
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
}