using CapShop.OrderService.Dtos;
using CapShop.OrderService.Models;
using CapShop.OrderService.Repositories;

namespace CapShop.OrderService.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _carts;
    private readonly ICatalogHttpClient _catalog;
    private readonly ILogger<CartService> _logger;

    public CartService(ICartRepository carts, ICatalogHttpClient catalog, ILogger<CartService> logger)
    {
        _carts = carts;
        _catalog = catalog;
        _logger = logger;
    }

    public async Task<CartResponse> GetCartAsync(Guid userId)
    {
        var cart = await _carts.GetByUserIdAsync(userId);

        if (cart is null || cart.Items.Count == 0)
            return new CartResponse();

        var items = cart.Items.Select(MapToCartItemResponse).ToList();

        return new CartResponse
        {
            Items = items,
            Total = items.Sum(i => i.Subtotal),
            ItemCount = items.Sum(i => i.Quantity)
        };
    }

    public async Task<CartItemResponse> AddItemAsync(Guid userId, AddCartItemRequest request)
    {
        if (request.Quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        var cart = await _carts.GetOrCreateAsync(userId);

        var existing = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);

        var totalRequested = (existing?.Quantity ?? 0) + request.Quantity;
        var availableStock = await _catalog.GetStockAsync(request.ProductId);

        if (availableStock < totalRequested)
            throw new ArgumentException($"Only {availableStock} unit(s) available for this product.");

        if (existing is not null)
        {
            existing.Quantity += request.Quantity;
            await _carts.UpdateItemAsync(existing);
            _logger.LogInformation("Updated cart item quantity for user {UserId}, product {ProductId}", userId, request.ProductId);
            return MapToCartItemResponse(existing);
        }

        var item = new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = cart.Id,
            ProductId = request.ProductId,
            ProductName = request.ProductName,
            Price = request.Price,
            ImageUrl = request.ImageUrl,
            Quantity = request.Quantity
        };

        await _carts.AddItemAsync(item);
        _logger.LogInformation("Added item to cart for user {UserId}, product {ProductId}", userId, request.ProductId);
        return MapToCartItemResponse(item);
    }

    public async Task<CartItemResponse> UpdateItemAsync(Guid userId, Guid itemId, UpdateCartItemRequest request)
    {
        if (request.Quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        var item = await _carts.GetCartItemAsync(itemId);

        if (item is null)
            throw new KeyNotFoundException("Cart item not found.");

        var availableStock = await _catalog.GetStockAsync(item.ProductId);

        if (availableStock < request.Quantity)
            throw new ArgumentException($"Only {availableStock} unit(s) available for this product.");

        item.Quantity = request.Quantity;
        await _carts.UpdateItemAsync(item);

        return MapToCartItemResponse(item);
    }

    public async Task RemoveItemAsync(Guid userId, Guid itemId)
    {
        var item = await _carts.GetCartItemAsync(itemId);

        if (item is null)
            throw new KeyNotFoundException("Cart item not found.");

        await _carts.RemoveItemAsync(item);
        _logger.LogInformation("Removed item {ItemId} from cart for user {UserId}", itemId, userId);
    }

    private static CartItemResponse MapToCartItemResponse(CartItem item)
    {
        return new CartItemResponse
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Price = item.Price,
            ImageUrl = item.ImageUrl,
            Quantity = item.Quantity,
            Subtotal = item.Price * item.Quantity
        };
    }
}