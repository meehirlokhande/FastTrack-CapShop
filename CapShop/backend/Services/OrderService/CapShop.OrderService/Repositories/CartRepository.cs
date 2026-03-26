using CapShop.OrderService.Data;
using CapShop.OrderService.Models;
using Microsoft.EntityFrameworkCore;

namespace CapShop.OrderService.Repositories;

public class CartRepository : ICartRepository
{
    private readonly OrderDbContext _db;

    public CartRepository(OrderDbContext db)
    {
        _db = db;
    }

    public async Task<Cart?> GetByUserIdAsync(Guid userId)
    {
        return await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Cart> GetOrCreateAsync(Guid userId)
    {
        var cart = await GetByUserIdAsync(userId);

        if (cart is not null)
            return cart;

        cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId
        };

        _db.Carts.Add(cart);
        await _db.SaveChangesAsync();
        return cart;
    }

    public async Task<CartItem?> GetCartItemAsync(Guid itemId)
    {
        return await _db.CartItems.FirstOrDefaultAsync(i => i.Id == itemId);
    }

    public async Task AddItemAsync(CartItem item)
    {
        _db.CartItems.Add(item);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateItemAsync(CartItem item)
    {
        _db.CartItems.Update(item);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(CartItem item)
    {
        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task ClearCartAsync(Cart cart)
    {
        _db.CartItems.RemoveRange(cart.Items);
        await _db.SaveChangesAsync();
    }
}