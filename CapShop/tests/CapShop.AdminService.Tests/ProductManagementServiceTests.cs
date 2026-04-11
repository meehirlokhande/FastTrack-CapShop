using CapShop.Tests.Common;
using CapShop.AdminService.Dtos;
using CapShop.AdminService.Models;
using CapShop.AdminService.Repositories;
using CapShop.AdminService.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CapShop.AdminService.Tests;

[TestFixture]
public class ProductManagementServiceTests
{
    private Mock<IAdminProductRepository> _products = null!;
    private Mock<IAdminLogService> _log = null!;
    private Mock<IDistributedCache> _cache = null!;
    private ProductManagementService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _products = new Mock<IAdminProductRepository>();
        _log = new Mock<IAdminLogService>();
        _cache = new Mock<IDistributedCache>();
        _sut = new ProductManagementService(
            _products.Object,
            _log.Object,
            _cache.Object,
            NullLogger<ProductManagementService>.Instance);
    }

    [Test]
    public async Task CreateProductAsync_InvalidPrice_Throws()
    {
        var req = new CreateProductRequest
        {
            Name = "N",
            Description = "D",
            Price = 0,
            Stock = 1,
            ImageUrl = "u",
            CategoryId = 1
        };
        var ex = await AsyncAssert.CatchAsync(() =>
            _sut.CreateProductAsync(Guid.NewGuid(), req));
        Assert.That(ex, Is.TypeOf<ArgumentException>());
    }

    [Test]
    public async Task CreateProductAsync_NegativeStock_Throws()
    {
        var req = new CreateProductRequest
        {
            Name = "N",
            Description = "D",
            Price = 10m,
            Stock = -1,
            ImageUrl = "u",
            CategoryId = 1
        };
        var ex = await AsyncAssert.CatchAsync(() =>
            _sut.CreateProductAsync(Guid.NewGuid(), req));
        Assert.That(ex, Is.TypeOf<ArgumentException>());
    }

    [Test]
    public async Task CreateProductAsync_UnknownCategory_ThrowsKeyNotFound()
    {
        _products.Setup(p => p.CategoryExistsAsync(5)).ReturnsAsync(false);
        var req = new CreateProductRequest
        {
            Name = "N",
            Description = "D",
            Price = 10m,
            Stock = 1,
            ImageUrl = "u",
            CategoryId = 5
        };
        var ex = await AsyncAssert.CatchAsync(() =>
            _sut.CreateProductAsync(Guid.NewGuid(), req));
        Assert.That(ex, Is.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public async Task CreateProductAsync_Valid_LogsAndReturnsProduct()
    {
        var adminId = Guid.NewGuid();
        _products.Setup(p => p.CategoryExistsAsync(1)).ReturnsAsync(true);
        Product? added = null;
        _products.Setup(p => p.AddAsync(It.IsAny<Product>()))
            .Callback<Product>(p => added = p)
            .Returns(Task.CompletedTask);
        _products.Setup(p => p.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => added is not null && added.Id == id
                ? added
                : null);

        var req = new CreateProductRequest
        {
            Name = "  Widget  ",
            Description = "  Desc  ",
            Price = 20m,
            Stock = 3,
            ImageUrl = "  /w.png  ",
            IsFeatured = false,
            CategoryId = 1
        };

        var res = await _sut.CreateProductAsync(adminId, req);

        Assert.That(res.Name, Is.EqualTo("Widget"));
        Assert.That(res.Price, Is.EqualTo(20m));
        _log.Verify(l => l.LogAsync(adminId, "Created", "Product", It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }

    [Test]
    public async Task UpdateStockAsync_NegativeStock_Throws()
    {
        var ex = await AsyncAssert.CatchAsync(() =>
            _sut.UpdateStockAsync(Guid.NewGuid(), Guid.NewGuid(), new UpdateStockRequest { Stock = -1 }));
        Assert.That(ex, Is.TypeOf<ArgumentException>());
    }
}
