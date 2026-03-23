using CapShop.AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace CapShop.AuthService.Data;

public static class AuthDbSeeder
{
    public static async Task SeedAsync(AuthDbContext db)
    {
        await db.Database.MigrateAsync();

        const string adminEmail = "admin@capshop.com";

        if (await db.Users.AnyAsync(u => u.Email == adminEmail))
            return;

        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            FullName = "CapShop Admin",
            Email = adminEmail,
            PhoneNumber = "9999999999",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = UserRole.Admin
        });

        await db.SaveChangesAsync();
    }
}