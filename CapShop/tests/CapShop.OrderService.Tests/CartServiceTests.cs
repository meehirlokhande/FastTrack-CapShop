using CapShop.Tests.Common;
using CapShop.OrderService.Dtos;
using CapShop.OrderService.Models;
using CapShop.OrderService.Repositories;
using CapShop.OrderService.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CapShop.OrderService.Tests;

[TestFixture]
public class CartServiceTests
{
    private Mock<ICartRepository> _carts = null!;
    private Mock<ICatalogHttpClient> _catalog = null!;
    private CartService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _carts = new Mock<ICartRepository>();
        _catalog = new Mock<ICatalogHttpClient>();
        _sut = new CartService(_carts.Object, _catalog.Object, NullLogger<CartService>.Instance);
    }

    [Test]
    public async Task GetCartAsync_EmptyCart_ReturnsEmptyResponse()
    {
        var uid = Guid.NewGuid();
        _carts.Setup(c => c.GetByUserIdAsync(uid)).ReturnsAsync((Cart?)null);

        var cart = await _sut.GetCartAsync(uid);

        Assert.That(cart.Items, Is.Empty);
        Assert.That(cart.Total, Is.EqualTo(0));
    }

    [Test]
    public async Task AddItemAsync_ZeroQuantity_Throws()
    {
        var ex = await AsyncAssert.CatchAsync(() =>
            _sut.AddItemAsync(Guid.NewGuid(), new AddCartItemRequest { Quantity = 0, ProductId = Guid.NewGuid() }));
        Assert.That(ex, Is.TypeOf<ArgumentException>());
    }

    [Test]
    public async Task AddItemAsync_InsufficientStock_Throws()
    {
        var pid = Guid.NewGuid();
        _catalog.Setup(c => c.GetStockAsync(pid)).ReturnsAsync(1);
        _carts.Setup(c => c.GetOrCreateAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Cart { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Items = [] });

        var ex = await AsyncAssert.CatchAsync(() =>
            _sut.AddItemAsync(Guid.NewGuid(), new AddCartItemRequest
            {
                ProductId = pid,
                Quantity = 5,
                ProductName = "X",
                Price = 1m,
                ImageUrl = ""
            }));
        Assert.That(ex, Is.TypeOf<ArgumentException>());
    }

    [Test]
    public async Task AddItemAsync_NewLine_AddsItem()
    {
        var uid = Guid.NewGuid();
        var pid = Guid.NewGuid();
        var cart = new Cart { Id = Guid.NewGuid(), UserId = uid, Items = [] };
        _carts.Setup(c => c.GetOrCreateAsync(uid)).ReturnsAsync(cart);
        _catalog.Setup(c => c.GetStockAsync(pid)).ReturnsAsync(10);

        var line = await _sut.AddItemAsync(uid, new AddCartItemRequest
        {
            ProductId = pid,
            Quantity = 2,
            ProductName = "Book",
            Price = 15m,
            ImageUrl = "/i.png"
        });

        Assert.That(line.Quantity, Is.EqualTo(2));
        Assert.That(line.Subtotal, Is.EqualTo(30m));
        _carts.Verify(c => c.AddItemAsync(It.Is<CartItem>(i => i.ProductId == pid && i.Quantity == 2)), Times.Once);
    }

    [Test]
    public async Task UpdateItemAsync_MissingItem_ThrowsKeyNotFound()
    {
        _carts.Setup(c => c.GetCartItemAsync(It.IsAny<Guid>())).ReturnsAsync((CartItem?)null);
        var ex = await AsyncAssert.CatchAsync(() =>
            _sut.UpdateItemAsync(Guid.NewGuid(), Guid.NewGuid(), new UpdateCartItemRequest { Quantity = 1 }));
        Assert.That(ex, Is.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public async Task RemoveItemAsync_MissingItem_ThrowsKeyNotFound()
    {
        _carts.Setup(c => c.GetCartItemAsync(It.IsAny<Guid>())).ReturnsAsync((CartItem?)null);
        var ex = await AsyncAssert.CatchAsync(() =>
            _sut.RemoveItemAsync(Guid.NewGuid(), Guid.NewGuid()));
        Assert.That(ex, Is.TypeOf<KeyNotFoundException>());
    }
}
