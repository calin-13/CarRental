namespace CarRental.Service.Interfaces;

public interface IExternalApiService
{
    Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency);
    Task<decimal> GetRoadTaxForZoneAsync(string zone);
}
