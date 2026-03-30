using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CapShop.AuthService.Dtos;
using CapShop.AuthService.Models;
using CapShop.AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapShop.AuthService.Controllers;

[ApiController]
[Route("auth/2fa")]
[Authorize]
public class TwoFactorController : ControllerBase
{
    private readonly ITwoFactorService _twoFactor;

    public TwoFactorController(ITwoFactorService twoFactor)
    {
        _twoFactor = twoFactor;
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status()
    {
        var userId = GetUserId();
        var status = await _twoFactor.GetStatusAsync(userId);
        return Ok(status);
    }

    [HttpGet("setup/totp")]
    public async Task<IActionResult> SetupTotp()
    {
        var userId = GetUserId();
        var email = User.FindFirstValue(JwtRegisteredClaimNames.Email)
                    ?? User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

        var result = await _twoFactor.SetupTotpAsync(userId, email);
        return Ok(result);
    }

    [HttpPost("confirm/totp")]
    public async Task<IActionResult> ConfirmTotp([FromBody] ConfirmCodeRequest request)
    {
        var userId = GetUserId();
        await _twoFactor.EnableTotpAsync(userId, request.Code);
        return Ok(new { message = "Authenticator app enabled." });
    }

    [HttpPost("setup/email")]
    public async Task<IActionResult> SetupEmail()
    {
        var userId = GetUserId();
        await _twoFactor.SendOtpAsync(userId, TwoFactorMethod.Email);
        return Ok(new { message = "Verification code sent to your email." });
    }

    [HttpPost("confirm/email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmCodeRequest request)
    {
        var userId = GetUserId();
        await _twoFactor.EnableOtpMethodAsync(userId, request.Code, TwoFactorMethod.Email);
        return Ok(new { message = "Email OTP enabled." });
    }

    [HttpPost("setup/sms")]
    public async Task<IActionResult> SetupSms()
    {
        var userId = GetUserId();
        await _twoFactor.SendOtpAsync(userId, TwoFactorMethod.Sms);
        return Ok(new { message = "Verification code sent to your phone." });
    }

    [HttpPost("confirm/sms")]
    public async Task<IActionResult> ConfirmSms([FromBody] ConfirmCodeRequest request)
    {
        var userId = GetUserId();
        await _twoFactor.EnableOtpMethodAsync(userId, request.Code, TwoFactorMethod.Sms);
        return Ok(new { message = "SMS OTP enabled." });
    }

    [HttpPost("disable")]
    public async Task<IActionResult> Disable()
    {
        var userId = GetUserId();
        await _twoFactor.DisableAsync(userId);
        return Ok(new { message = "Two-factor authentication disabled." });
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? throw new UnauthorizedAccessException("User identity not found.");

        return Guid.Parse(sub);
    }
}
