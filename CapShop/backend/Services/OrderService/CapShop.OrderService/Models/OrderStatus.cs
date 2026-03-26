namespace CapShop.OrderService.Models;

public enum OrderStatus
{
    PaymentPending = 0,
    Paid = 1,
    Packed = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5,
    PaymentFailed = 6
}