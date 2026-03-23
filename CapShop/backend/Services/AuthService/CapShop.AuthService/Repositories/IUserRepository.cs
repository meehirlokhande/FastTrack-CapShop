using CapShop.AuthService.Models;

namespace CapShop.AuthService.Repositories;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email);
    Task AddAsync(User user);
}