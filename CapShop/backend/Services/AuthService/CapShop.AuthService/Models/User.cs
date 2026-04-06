namespace CapShop.AuthService.Models;

public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool TwoFactorEnabled { get; set; } = false;
    public TwoFactorMethod TwoFactorMethod { get; set; } = TwoFactorMethod.None;
    public string? TotpSecret { get; set; }

    /// <summary>Relative public path e.g. /avatars/{id}.jpg when set.</summary>
    public string? ProfilePictureUrl { get; set; }
}
