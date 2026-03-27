using CapShop.AdminService.Data;
using CapShop.AdminService.Models;

namespace CapShop.AdminService.Services;

public class AdminLogService : IAdminLogService
{
    private readonly AdminDbContext _db;

    public AdminLogService(AdminDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(Guid adminUserId, string action, string entityType, string entityId, string details)
    {
        _db.AdminLogs.Add(new AdminLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details
        });

        await _db.SaveChangesAsync();
    }
}