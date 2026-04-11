using CapShop.Tests.Common;
using CapShop.AdminService.Dtos;
using CapShop.AdminService.Models;
using CapShop.AdminService.Repositories;
using CapShop.AdminService.Services;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace CapShop.AdminService.Tests;

[TestFixture]
public class CategoryManagementServiceTests
{
    private Mock<IAdminCategoryRepository> _repo = null!;
    private Mock<IDistributedCache> _cache = null!;
    private CategoryManagementService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _repo = new Mock<IAdminCategoryRepository>();
        _cache = new Mock<IDistributedCache>();
        _sut = new CategoryManagementService(_repo.Object, _cache.Object);
    }

    [Test]
    public async Task CreateAsync_EmptyName_Throws()
    {
        var ex = await AsyncAssert.CatchAsync(() =>
            _sut.CreateAsync(new CreateCategoryRequest { Name = "  ", Description = "d" }));
        Assert.That(ex, Is.TypeOf<ArgumentException>());
    }

    [Test]
    public async Task CreateAsync_DuplicateName_ThrowsInvalidOperation()
    {
        _repo.Setup(r => r.ExistsByNameAsync("Books")).ReturnsAsync(true);
        var ex = await AsyncAssert.CatchAsync(() =>
            _sut.CreateAsync(new CreateCategoryRequest { Name = "Books", Description = "x" }));
        Assert.That(ex, Is.TypeOf<InvalidOperationException>());
    }

    [Test]
    public async Task CreateAsync_Valid_AddsAndInvalidatesCache()
    {
        _repo.Setup(r => r.ExistsByNameAsync("New")).ReturnsAsync(false);
        _repo.Setup(r => r.AddAsync(It.IsAny<Category>()))
            .Returns(Task.CompletedTask);

        var res = await _sut.CreateAsync(new CreateCategoryRequest { Name = "New", Description = "Desc" });

        Assert.That(res.Name, Is.EqualTo("New"));
        Assert.That(res.Description, Is.EqualTo("Desc"));
        _repo.Verify(r => r.AddAsync(It.Is<Category>(c => c.Name == "New")), Times.Once);
        _cache.Verify(c => c.RemoveAsync("categories", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_NotFound_Throws()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Category?)null);
        var ex = await AsyncAssert.CatchAsync(() => _sut.DeleteAsync(99));
        Assert.That(ex, Is.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public async Task DeleteAsync_HasProducts_ThrowsInvalidOperation()
    {
        var cat = new Category { Id = 1, Name = "C", Description = "D" };
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cat);
        _repo.Setup(r => r.HasProductsAsync(1)).ReturnsAsync(true);

        var ex = await AsyncAssert.CatchAsync(() => _sut.DeleteAsync(1));
        Assert.That(ex, Is.TypeOf<InvalidOperationException>());
    }
}
