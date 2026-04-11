using System.Text.Json;
using CapShop.Shared.Middleware;
using Microsoft.AspNetCore.Http;

namespace CapShop.OrderService.Services;

public class CatalogHttpClient : ICatalogHttpClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CatalogHttpClient> _logger;

    public CatalogHttpClient(HttpClient http, IHttpContextAccessor httpContextAccessor, ILogger<CatalogHttpClient> logger)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<int> GetStockAsync(Guid productId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/catalog/products/{productId}");

        if (_httpContextAccessor.HttpContext?.Items[CorrelationIdMiddleware.ItemsKey] is string correlationId)
            request.Headers.TryAddWithoutValidation(CorrelationIdMiddleware.HeaderName, correlationId);

        var response = await _http.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to get stock for product {ProductId}. HTTP {Status}", productId, (int)response.StatusCode);
            throw new InvalidOperationException($"Failed to retrieve product {productId} from catalog.");
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("stock", out var stockEl))
            throw new InvalidOperationException("Product response missing stock field.");

        return stockEl.GetInt32();
    }
}
