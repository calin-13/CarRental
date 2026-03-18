using CarRental.Domain.Exceptions;
using CarRental.Service.Interfaces;
using System.Text.Json;

namespace CarRental.Service.Services;

public class ExternalApiService : IExternalApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILoggerService _logger;

    public ExternalApiService(HttpClient httpClient, ILoggerService logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
    {
        try
        {
            _logger.LogInfo($"Fetching exchange rate from {fromCurrency} to {toCurrency}");
            
            // Using exchangerate-api.com (free tier)
            var url = $"https://api.exchangerate-api.com/v4/latest/{fromCurrency}";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new ExternalApiException($"API returned status code: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            
            if (jsonDoc.RootElement.TryGetProperty("rates", out var rates) &&
                rates.TryGetProperty(toCurrency, out var rate))
            {
                var exchangeRate = rate.GetDecimal();
                _logger.LogInfo($"Exchange rate {fromCurrency}/{toCurrency}: {exchangeRate}");
                return exchangeRate;
            }

            throw new ExternalApiException($"Currency {toCurrency} not found in API response");
        }
        catch (Exception ex) when (ex is not ExternalApiException)
        {
            _logger.LogError($"Failed to fetch exchange rate", ex);
            throw new ExternalApiException("Failed to fetch exchange rate from external API", ex);
        }
    }

    public async Task<decimal> GetRoadTaxForZoneAsync(string zone)
    {
        try
        {
            _logger.LogInfo($"Fetching road tax for zone: {zone}");
            
            // Simulare API pentru taxe rutiere pe zone
            // În realitate ar fi un API real de taxe rutiere
            await Task.Delay(100); // Simulate API call
            
            var roadTaxes = new Dictionary<string, decimal>
            {
                { "urban", 50.0m },
                { "suburban", 30.0m },
                { "rural", 20.0m },
                { "highway", 40.0m }
            };

            if (roadTaxes.TryGetValue(zone.ToLower(), out var tax))
            {
                _logger.LogInfo($"Road tax for zone {zone}: {tax} RON");
                return tax;
            }

            _logger.LogWarning($"Unknown zone: {zone}, using default tax");
            return 25.0m; // Default tax
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to fetch road tax", ex);
            throw new ExternalApiException("Failed to fetch road tax from external API", ex);
        }
    }
}
