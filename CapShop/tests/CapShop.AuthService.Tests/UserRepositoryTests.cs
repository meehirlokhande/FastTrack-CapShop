using CapShop.AuthService.Data;
using CapShop.AuthService.Models;
using CapShop.AuthService.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CapShop.AuthService.Tests;

[TestFixture]
public class UserRepositoryTests
{
    private AuthDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AuthDbContext(options);
    }

    [Test]
    public async Task ExistsByEmailAsync_ReturnsFalseWhenEmpty()
    {
        await using var db = CreateContext();
        var repo = new UserRepository(db);

        var exists = await repo.ExistsByEmailAsync("any@test.com");

        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task AddAndFindByEmail_RoundTrips()
    {
        await using var db = CreateContext();
        var repo = new UserRepository(db);
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "T",
            Email = "t@test.com",
            PhoneNumber = "1",
            PasswordHash = "hash",
            Role = UserRole.Customer
        };

        await repo.AddAsync(user);
        var found = await repo.FindByEmailAsync("t@test.com");

        Assert.That(found, Is.Not.Null);
        Assert.That(found!.Id, Is.EqualTo(user.Id));
    }

    [Test]
    public async Task UpdateAsync_PersistsChanges()
    {
        await using var db = CreateContext();
        var repo = new UserRepository(db);
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Old",
            Email = "u@test.com",
            PhoneNumber = "1",
            PasswordHash = "h",
            Role = UserRole.Customer
        };
        await repo.AddAsync(user);

        user.FullName = "New";
        await repo.UpdateAsync(user);

        var found = await repo.FindByIdAsync(user.Id);
        Assert.That(found!.FullName, Is.EqualTo("New"));
    }
}
