using CapShop.AuthService.Dtos;
using CapShop.AuthService.Models;
using CapShop.AuthService.Repositories;

namespace CapShop.AuthService.Services;

public class AccountService : IAccountService
{
    private readonly IUserRepository _users;
    private readonly IJwtTokenService _jwt;
    private readonly ILogger<AccountService> _logger;

    public AccountService(IUserRepository users, IJwtTokenService jwt, ILogger<AccountService> logger)
    {
        _users = users;
        _jwt = jwt;
        _logger = logger;
    }

    public async Task<string> SignupAsync(SignUpRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Email and password are required.");

        var email = request.Email.Trim().ToLowerInvariant();

        if (await _users.ExistsByEmailAsync(email))
            throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = email,
            PhoneNumber = request.PhoneNumber.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Customer
        };

        await _users.AddAsync(user);

        _logger.LogInformation("New customer registered: {Email}", email);
        return "Signup successful.";
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _users.FindByEmailAsync(email);

        if (user is null || !user.IsActive)
        {
            _logger.LogWarning("Failed login attempt for: {Email}", email);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Invalid password for: {Email}", email);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var token = _jwt.GenerateToken(user);

        _logger.LogInformation("Successful login: {Email}, Role: {Role}", email, user.Role);

        return new AuthResponse
        {
            Token = token,
            Role = user.Role.ToString()
        };
    }
}