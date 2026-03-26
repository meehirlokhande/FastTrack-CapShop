using CapShop.OrderService.Models;

namespace CapShop.OrderService.Repositories;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(Guid userId);
    Task<Cart> GetOrCreateAsync(Guid userId);
    Task<CartItem?> GetCartItemAsync(Guid itemId);
    Task AddItemAsync(CartItem item);
    Task UpdateItemAsync(CartItem item);
    Task RemoveItemAsync(CartItem item);
    Task ClearCartAsync(Cart cart);
}