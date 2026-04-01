using CapShop.AuthService.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapShop.AuthService.Controllers;

[ApiController]
[Route("auth/internal")]
[AllowAnonymous]
public class InternalController : ControllerBase
{
    private readonly IUserRepository _users;
    private readonly IConfiguration _config;

    public InternalController(IUserRepository users, IConfiguration config)
    {
        _users = users;
        _config = config;
    }

    [HttpGet("users/{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var expectedKey = _config["InternalApi:Key"];
        if (!Request.Headers.TryGetValue("X-Internal-Key", out var receivedKey) || receivedKey != expectedKey)
            return Unauthorized(new { message = "Invalid internal key." });

        var user = await _users.FindByIdAsync(id);
        if (user is null)
            return NotFound(new { message = "User not found." });

        return Ok(new { user.Email, user.FullName });
    }
}
