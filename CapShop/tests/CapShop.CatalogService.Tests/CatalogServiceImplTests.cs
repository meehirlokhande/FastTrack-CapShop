using CapShop.Tests.Common;
using System.Text;
using System.Text.Json;
using CapShop.CatalogService.Dtos;
using CapShop.CatalogService.Models;
using CapShop.CatalogService.Repositories;
using CapShop.CatalogService.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CapShop.CatalogService.Tests;

[TestFixture]
public class CatalogServiceImplTests
{
    private Mock<IProductRepository> _products = null!;
    private Mock<IDistributedCache> _cache = null!;
    private CatalogServiceImpl _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _products = new Mock<IProductRepository>();
        _cache = new Mock<IDistributedCache>();
        _sut = new CatalogServiceImpl(_products.Object, _cache.Object, NullLogger<CatalogServiceImpl>.Instance);
    }

    [Test]
    public async Task GetProductByIdAsync_NotFound_ThrowsKeyNotFound()
    {
        var id = Guid.NewGuid();
        _cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);
        _products.Setup(p => p.GetByIdAsync(id)).ReturnsAsync((Product?)null);

        var ex = await AsyncAssert.CatchAsync(() => _sut.GetProductByIdAsync(id));
        Assert.That(ex, Is.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public async Task GetProductByIdAsync_CacheMiss_LoadsFromRepositoryAndSetsCache()
    {
        var id = Guid.NewGuid();
        var category = new Category { Id = 1, Name = "Cat" };
        var product = new Product
        {
            Id = id,
            Name = "P",
            Description = "D",
            Price = 10m,
            Stock = 3,
            ImageUrl = "u",
            IsFeatured = true,
            Status = ProductStatus.Active,
            CategoryId = 1,
            Category = category
        };

        _cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);
        _products.Setup(p => p.GetByIdAsync(id)).ReturnsAsync(product);

        var dto = await _sut.GetProductByIdAsync(id);

        Assert.That(dto.Name, Is.EqualTo("P"));
        Assert.That(dto.CategoryName, Is.EqualTo("Cat"));
        _cache.Verify(c => c.SetAsync(
            It.Is<string>(k => k.Contains(id.ToString(), StringComparison.Ordinal)),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetProductByIdAsync_CacheHit_DoesNotCallRepository()
    {
        var id = Guid.NewGuid();
        var cached = new ProductResponse
        {
            Id = id,
            Name = "Cached",
            Description = "",
            Price = 1,
            Stock = 1,
            ImageUrl = "",
            IsFeatured = false,
            Status = "Active",
            CategoryName = "C"
        };
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cached));
        _cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        var dto = await _sut.GetProductByIdAsync(id);

        Assert.That(dto.Name, Is.EqualTo("Cached"));
        _products.Verify(p => p.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task GetProductsAsync_ReturnsPagedResult()
    {
        var id = Guid.NewGuid();
        var category = new Category { Id = 1, Name = "X" };
        var list = new List<Product>
        {
            new()
            {
                Id = id,
                Name = "A",
                Description = "",
                Price = 5m,
                Stock = 1,
                ImageUrl = "",
                CategoryId = 1,
                Category = category
            }
        };
        _products.Setup(p => p.GetFilteredAsync(It.IsAny<ProductListRequest>()))
            .ReturnsAsync((list, 25));

        var req = new ProductListRequest { Page = 1, PageSize = 10 };
        var page = await _sut.GetProductsAsync(req);

        Assert.That(page.TotalCount, Is.EqualTo(25));
        Assert.That(page.TotalPages, Is.EqualTo(3));
        Assert.That(page.Items, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task AdjustStockBatchAsync_ClearsProductAndFeaturedCacheKeys()
    {
        var pid = Guid.NewGuid();
        _products.Setup(p => p.AdjustStockBatchAsync(It.IsAny<IEnumerable<(Guid, int)>>()))
            .Returns(Task.CompletedTask);

        await _sut.AdjustStockBatchAsync([new StockAdjustItem(pid, -1)]);

        _cache.Verify(c => c.RemoveAsync($"product:{pid}", It.IsAny<CancellationToken>()), Times.Once);
        _cache.Verify(c => c.RemoveAsync("featured", It.IsAny<CancellationToken>()), Times.Once);
    }
}
