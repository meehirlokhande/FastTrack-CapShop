using System.Text;
using System.Text.Json;

namespace CapShop.PaymentService.Services;

public class OrderHttpClient : IOrderHttpClient
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<OrderHttpClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public OrderHttpClient(HttpClient http, IConfiguration config, ILogger<OrderHttpClient> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task UpdatePaymentStatusAsync(
        Guid orderId,
        Guid userId,
        string status,
        string paymentMethod,
        DateTime? paidAt)
    {
        var internalKey = _config["InternalApi:Key"]
            ?? throw new InvalidOperationException("InternalApi:Key is missing");

        var payload = new { userId, status, paymentMethod, paidAt };
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"/orders/{orderId}/payment-status");

        request.Content = content;
        request.Headers.Add("X-Internal-Key", internalKey);

        var response = await _http.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "Failed to update order {OrderId} payment status. HTTP {Status}: {Body}",
                orderId, (int)response.StatusCode, body);

            throw new InvalidOperationException(
                $"Order payment status update failed: {(int)response.StatusCode}");
        }

        _logger.LogInformation(
            "Order {OrderId} updated to status {Status}", orderId, status);
    }
}
