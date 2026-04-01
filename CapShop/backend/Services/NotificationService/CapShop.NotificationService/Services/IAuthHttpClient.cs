namespace CapShop.NotificationService.Services;

public interface IAuthHttpClient
{
    Task<UserInfo?> GetUserAsync(Guid userId);
}

public record UserInfo(string Email, string FullName);
