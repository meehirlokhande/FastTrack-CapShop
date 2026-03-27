namespace CapShop.AdminService.Services;

public interface IAdminLogService
{
    Task LogAsync(Guid adminUserId, string action, string entityType, string entityId, string details);
}