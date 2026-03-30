using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CapShop.AuthService.Models;
using Microsoft.IdentityModel.Tokens;

namespace CapShop.AuthService.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(User user)
    {
        var jwt = _config.GetSection("Jwt");
        var key = jwt["SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey is missing");

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwt["AccessTokenMinutes"] ?? "60")),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateTempToken(Guid userId, TwoFactorMethod method)
    {
        var jwt = _config.GetSection("Jwt");
        var key = jwt["SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey is missing");

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("purpose", "2fa_pending"),
            new Claim("2fa_method", method.ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public TempTokenPayload? ValidateTempToken(string tempToken)
    {
        var jwt = _config.GetSection("Jwt");
        var key = jwt["SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey is missing");

        var handler = new JwtSecurityTokenHandler
        {
            // Prevent "sub" being remapped to ClaimTypes.NameIdentifier before we read it
            MapInboundClaims = false
        };
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = handler.ValidateToken(tempToken, validationParams, out _);

            var purpose = principal.FindFirstValue("purpose");
            if (purpose != "2fa_pending") return null;

            var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (!Guid.TryParse(sub, out var userId)) return null;

            var methodStr = principal.FindFirstValue("2fa_method");
            if (!Enum.TryParse<TwoFactorMethod>(methodStr, out var method)) return null;

            return new TempTokenPayload(userId, method);
        }
        catch
        {
            return null;
        }
    }
}