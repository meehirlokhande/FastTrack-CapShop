using System.Security.Cryptography;
using System.Text;
using CapShop.AuthService.Dtos;
using CapShop.AuthService.Models;
using CapShop.AuthService.Repositories;
using OtpNet;

namespace CapShop.AuthService.Services;

public class TwoFactorService : ITwoFactorService
{
    private readonly IUserRepository _users;
    private readonly IOtpCodeRepository _otpCodes;
    private readonly INotificationService _notifications;
    private const int OtpExpiryMinutes = 5;

    public TwoFactorService(
        IUserRepository users,
        IOtpCodeRepository otpCodes,
        INotificationService notifications)
    {
        _users = users;
        _otpCodes = otpCodes;
        _notifications = notifications;
    }

    public async Task<TotpSetupResponse> SetupTotpAsync(Guid userId, string email)
    {
        var user = await GetUserOrThrowAsync(userId);

        var secretBytes = KeyGeneration.GenerateRandomKey(20);
        var secret = Base32Encoding.ToString(secretBytes);

        // Save the secret but don't enable 2FA yet — user must confirm first
        user.TotpSecret = secret;
        await _users.UpdateAsync(user);

        var issuer = "CapShop";
        var qrCodeUri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}" +
                        $"?secret={secret}&issuer={Uri.EscapeDataString(issuer)}&algorithm=SHA1&digits=6&period=30";

        return new TotpSetupResponse { Secret = secret, QrCodeUri = qrCodeUri };
    }

    public async Task EnableTotpAsync(Guid userId, string code)
    {
        var user = await GetUserOrThrowAsync(userId);

        if (string.IsNullOrEmpty(user.TotpSecret))
            throw new InvalidOperationException("TOTP setup not initiated. Call setup first.");

        if (!VerifyTotp(user.TotpSecret, code))
            throw new UnauthorizedAccessException("Invalid authenticator code.");

        user.TwoFactorEnabled = true;
        user.TwoFactorMethod = TwoFactorMethod.Authenticator;
        await _users.UpdateAsync(user);
    }

    public async Task SendOtpAsync(Guid userId, TwoFactorMethod method)
    {
        var user = await GetUserOrThrowAsync(userId);

        await _otpCodes.DeleteExpiredAsync(userId);

        var code = GenerateSixDigitCode();
        var hash = HashCode(code);

        await _otpCodes.AddAsync(new OtpCode
        {
            UserId = userId,
            CodeHash = hash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes)
        });

        if (method == TwoFactorMethod.Email)
            await _notifications.SendEmailOtpAsync(user.Email, code);
        else if (method == TwoFactorMethod.Sms)
            await _notifications.SendSmsOtpAsync(user.PhoneNumber, code);
        else
            throw new ArgumentException("Invalid method for OTP delivery.");
    }

    public async Task EnableOtpMethodAsync(Guid userId, string code, TwoFactorMethod method)
    {
        var user = await GetUserOrThrowAsync(userId);

        if (!await VerifyOtpCodeAsync(userId, code))
            throw new UnauthorizedAccessException("Invalid or expired code.");

        user.TwoFactorEnabled = true;
        user.TwoFactorMethod = method;
        await _users.UpdateAsync(user);
    }

    public async Task<bool> VerifyTotpCodeAsync(Guid userId, string code)
    {
        var user = await GetUserOrThrowAsync(userId);

        if (string.IsNullOrEmpty(user.TotpSecret)) return false;

        return VerifyTotp(user.TotpSecret, code);
    }

    public async Task<bool> VerifyOtpCodeAsync(Guid userId, string code)
    {
        var hash = HashCode(code);
        var otpCode = await _otpCodes.FindValidAsync(userId, hash);

        if (otpCode is null) return false;

        await _otpCodes.MarkUsedAsync(otpCode);
        return true;
    }

    public async Task DisableAsync(Guid userId)
    {
        var user = await GetUserOrThrowAsync(userId);

        user.TwoFactorEnabled = false;
        user.TwoFactorMethod = TwoFactorMethod.None;
        user.TotpSecret = null;
        await _users.UpdateAsync(user);
    }

    public async Task<TwoFactorStatusResponse> GetStatusAsync(Guid userId)
    {
        var user = await GetUserOrThrowAsync(userId);

        return new TwoFactorStatusResponse
        {
            Enabled = user.TwoFactorEnabled,
            Method = user.TwoFactorMethod.ToString()
        };
    }

    private static bool VerifyTotp(string secret, string code)
    {
        var secretBytes = Base32Encoding.ToBytes(secret);
        var totp = new Totp(secretBytes);
        return totp.VerifyTotp(code, out _, new VerificationWindow(previous: 1, future: 1));
    }

    private static string GenerateSixDigitCode()
    {
        return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
    }

    private static string HashCode(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(bytes);
    }

    private async Task<User> GetUserOrThrowAsync(Guid userId)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null) throw new InvalidOperationException("User not found.");
        return user;
    }
}
