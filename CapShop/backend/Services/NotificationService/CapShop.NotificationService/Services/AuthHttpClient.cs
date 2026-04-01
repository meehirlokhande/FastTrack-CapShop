using System.Net.Http.Json;

namespace CapShop.NotificationService.Services;

public class AuthHttpClient : IAuthHttpClient
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthHttpClient> _logger;

    public AuthHttpClient(HttpClient http, IConfiguration config, ILogger<AuthHttpClient> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task<UserInfo?> GetUserAsync(Guid userId)
    {
        var internalKey = _config["InternalApi:Key"]
            ?? throw new InvalidOperationException("InternalApi:Key is missing");

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/auth/internal/users/{userId}");
        request.Headers.Add("X-Internal-Key", internalKey);

        var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("AuthService returned {Status} for user {UserId}", response.StatusCode, userId);
            return null;
        }

        return await response.Content.ReadFromJsonAsync<UserInfo>();
    }
}
