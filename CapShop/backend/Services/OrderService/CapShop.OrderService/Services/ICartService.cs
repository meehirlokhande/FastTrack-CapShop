using CapShop.OrderService.Dtos;

namespace CapShop.OrderService.Services;

public interface ICartService
{
    Task<CartResponse> GetCartAsync(Guid userId);
    Task<CartItemResponse> AddItemAsync(Guid userId, AddCartItemRequest request);
    Task<CartItemResponse> UpdateItemAsync(Guid userId, Guid itemId, UpdateCartItemRequest request);
    Task RemoveItemAsync(Guid userId, Guid itemId);
}