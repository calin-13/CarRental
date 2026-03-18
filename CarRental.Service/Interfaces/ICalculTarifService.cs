using CarRental.Domain.Entities;

namespace CarRental.Service.Interfaces;

public interface ICalculTarifService
{
    Task<decimal> CalculateBaseTariffAsync(Reservation reservation);
    Task<decimal> CalculateLateFeeAsync(Reservation reservation, DateTime actualReturnDate);
    Task<decimal> CalculateTotalWithRoadTaxAsync(decimal baseAmount, string zone);
    Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency);
    Task<TariffCalculation> CalculateCompleteTariffAsync(
        Reservation reservation, 
        DateTime? actualReturnDate = null, 
        string zone = "urban",
        string targetCurrency = "RON");
}
