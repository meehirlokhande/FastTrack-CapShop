using System.Security.Claims;
using CapShop.AuthService.Dtos;
using CapShop.AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapShop.AuthService.Controllers;

[ApiController]
[Route("auth/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private const long MaxAvatarBytes = 2 * 1024 * 1024;

    private readonly IAccountService _account;

    public ProfileController(IAccountService account)
    {
        _account = account;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var profile = await _account.GetProfileAsync(GetUserId());
        return Ok(profile);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var profile = await _account.UpdateProfileAsync(GetUserId(), request);
        return Ok(profile);
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        await _account.ChangePasswordAsync(GetUserId(), request);
        return Ok(new { message = "Password updated." });
    }

    [HttpPost("avatar")]
    [RequestSizeLimit(MaxAvatarBytes + 64 * 1024)]
    public async Task<IActionResult> UploadAvatar([FromForm] IFormFile? file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        await using var stream = file.OpenReadStream();
        var profile = await _account.SetAvatarAsync(GetUserId(), stream, file.ContentType, file.Length);
        return Ok(profile);
    }

    [HttpDelete("avatar")]
    public async Task<IActionResult> RemoveAvatar()
    {
        var profile = await _account.RemoveAvatarAsync(GetUserId());
        return Ok(profile);
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.Parse(sub!);
    }
}
