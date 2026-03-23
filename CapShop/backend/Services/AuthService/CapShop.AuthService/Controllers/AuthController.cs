using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CapShop.AuthService.Dtos;
using CapShop.AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapShop.AuthService.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAccountService _account;

    public AuthController(IAccountService account)
    {
        _account = account;
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { service = "AuthService", status = "Healthy" });
    }

    [HttpPost("signup")]
    [AllowAnonymous]
    public async Task<IActionResult> Signup([FromBody] SignUpRequest request)
    {
        var message = await _account.SignupAsync(request);
        return Ok(new { message });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _account.LoginAsync(request);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var name = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        var email = User.FindFirstValue(JwtRegisteredClaimNames.Email)
                    ?? User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        return Ok(new MeResponse
        {
            FullName = name,
            Email = email,
            Role = role
        });
    }
}