using CarRental.Domain.Entities;
using CarRental.Domain.Exceptions;
using CarRental.Service.Interfaces;

namespace CarRental.Service.Services;

public class CalculTarifService : ICalculTarifService
{
    private readonly IExternalApiService _externalApiService;
    private readonly ILoggerService _logger;
    private const decimal LateFeePerDay = 100.0m; // 100 RON per day late

    public CalculTarifService(IExternalApiService externalApiService, ILoggerService logger)
    {
        _externalApiService = externalApiService;
        _logger = logger;
    }

    public async Task<decimal> CalculateBaseTariffAsync(Reservation reservation)
    {
        try
        {
            _logger.LogInfo($"Calculating base tariff for reservation {reservation.Id}");

            if (reservation.Car == null)
            {
                throw new InvalidTariffCalculationException("Car information is required for tariff calculation");
            }

            if (reservation.StartDate >= reservation.EndDate)
            {
                throw new InvalidTariffCalculationException("End date must be after start date");
            }

            var days = (reservation.EndDate - reservation.StartDate).Days;
            if (days <= 0)
            {
                days = 1; // Minimum 1 day rental
            }

            var baseTariff = reservation.Car.DailyRate * days;
            _logger.LogInfo($"Base tariff calculated: {baseTariff} RON for {days} days");

            return baseTariff;
        }
        catch (Exception ex) when (ex is not InvalidTariffCalculationException)
        {
            _logger.LogError("Failed to calculate base tariff", ex);
            throw new InvalidTariffCalculationException("Error calculating base tariff", ex);
        }
    }

    public async Task<decimal> CalculateLateFeeAsync(Reservation reservation, DateTime actualReturnDate)
    {
        try
        {
            _logger.LogInfo($"Calculating late fee for reservation {reservation.Id}");

            if (actualReturnDate <= reservation.EndDate)
            {
                _logger.LogInfo("No late fee - car returned on time");
                return 0;
            }

            var lateDays = (actualReturnDate - reservation.EndDate).Days;
            var lateFee = lateDays * LateFeePerDay;

            _logger.LogWarning($"Late return detected: {lateDays} days late, fee: {lateFee} RON");

            return lateFee;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to calculate late fee", ex);
            throw new InvalidTariffCalculationException("Error calculating late fee", ex);
        }
    }

    public async Task<decimal> CalculateTotalWithRoadTaxAsync(decimal baseAmount, string zone)
    {
        try
        {
            _logger.LogInfo($"Adding road tax for zone: {zone}");

            if (baseAmount < 0)
            {
                throw new InvalidTariffCalculationException("Base amount cannot be negative");
            }

            var roadTax = await _externalApiService.GetRoadTaxForZoneAsync(zone);
            var total = baseAmount + roadTax;

            _logger.LogInfo($"Total with road tax: {total} RON (base: {baseAmount}, tax: {roadTax})");

            return total;
        }
        catch (ExternalApiException ex)
        {
            _logger.LogWarning($"Failed to fetch road tax, using default: {ex.Message}");
            // Fallback to default tax if API fails
            return baseAmount + 25.0m;
        }
        catch (Exception ex) when (ex is not InvalidTariffCalculationException)
        {
            _logger.LogError("Failed to calculate total with road tax", ex);
            throw new InvalidTariffCalculationException("Error calculating total with road tax", ex);
        }
    }

    public async Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency)
    {
        try
        {
            _logger.LogInfo($"Converting {amount} {fromCurrency} to {toCurrency}");

            if (amount < 0)
            {
                throw new InvalidTariffCalculationException("Amount cannot be negative");
            }

            if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInfo("Same currency, no conversion needed");
                return amount;
            }

            var exchangeRate = await _externalApiService.GetExchangeRateAsync(fromCurrency, toCurrency);
            var convertedAmount = amount * exchangeRate;

            _logger.LogInfo($"Converted amount: {convertedAmount} {toCurrency}");

            return Math.Round(convertedAmount, 2);
        }
        catch (ExternalApiException ex)
        {
            _logger.LogError($"Currency conversion failed", ex);
            throw new InvalidTariffCalculationException($"Failed to convert currency from {fromCurrency} to {toCurrency}", ex);
        }
        catch (Exception ex) when (ex is not InvalidTariffCalculationException)
        {
            _logger.LogError("Unexpected error in currency conversion", ex);
            throw new InvalidTariffCalculationException("Error converting currency", ex);
        }
    }

    public async Task<TariffCalculation> CalculateCompleteTariffAsync(
        Reservation reservation,
        DateTime? actualReturnDate = null,
        string zone = "urban",
        string targetCurrency = "RON")
    {
        try
        {
            _logger.LogInfo($"Starting complete tariff calculation for reservation {reservation.Id}");

            // 1. Calculate base tariff
            var baseTariff = await CalculateBaseTariffAsync(reservation);

            // 2. Calculate late fee (if applicable)
            var lateFee = 0m;
            if (actualReturnDate.HasValue)
            {
                lateFee = await CalculateLateFeeAsync(reservation, actualReturnDate.Value);
            }

            // 3. Add road tax
            var subtotal = baseTariff + lateFee;
            var totalWithTax = await CalculateTotalWithRoadTaxAsync(subtotal, zone);
            var roadTax = totalWithTax - subtotal;

            // 4. Currency conversion (if needed)
            var exchangeRate = 1m;
            var finalAmount = totalWithTax;
            
            if (!targetCurrency.Equals("RON", StringComparison.OrdinalIgnoreCase))
            {
                exchangeRate = await _externalApiService.GetExchangeRateAsync("RON", targetCurrency);
                finalAmount = await ConvertCurrencyAsync(totalWithTax, "RON", targetCurrency);
            }

            var calculation = new TariffCalculation
            {
                ReservationId = reservation.Id,
                BaseRate = baseTariff,
                LateFee = lateFee,
                RoadTax = roadTax,
                CurrencyConversionRate = exchangeRate,
                Currency = targetCurrency,
                TotalAmount = finalAmount,
                CalculatedAt = DateTime.UtcNow
            };

            _logger.LogInfo($"Complete tariff calculation finished: {finalAmount} {targetCurrency}");

            return calculation;
        }
        catch (Exception ex) when (ex is not InvalidTariffCalculationException)
        {
            _logger.LogError("Failed to calculate complete tariff", ex);
            throw new InvalidTariffCalculationException("Error calculating complete tariff", ex);
        }
    }
}
