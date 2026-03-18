using CarRental.Domain.Entities;
using CarRental.Domain.Exceptions;
using CarRental.Service.Interfaces;
using CarRental.Service.Services;
using Moq;
using Xunit;

namespace CarRental.Tests.Services;

public class CalculTarifServiceTests
{
    private readonly Mock<IExternalApiService> _mockExternalApi;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly CalculTarifService _service;

    public CalculTarifServiceTests()
    {
        _mockExternalApi = new Mock<IExternalApiService>();
        _mockLogger = new Mock<ILoggerService>();
        _service = new CalculTarifService(_mockExternalApi.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CalculateBaseTariffAsync_ValidReservation_ReturnsCorrectAmount()
    {
        var reservation = new Reservation
        {
            Id = 1,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(3),
            Car = new Car { DailyRate = 100m }
        };

        var result = await _service.CalculateBaseTariffAsync(reservation);

        Assert.Equal(300m, result);
    }

    [Fact]
    public async Task CalculateBaseTariffAsync_NoCar_ThrowsException()
    {
        var reservation = new Reservation
        {
            Id = 1,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(3),
            Car = null
        };

        await Assert.ThrowsAsync<InvalidTariffCalculationException>(
            () => _service.CalculateBaseTariffAsync(reservation));
    }

    [Fact]
    public async Task CalculateBaseTariffAsync_EndDateBeforeStartDate_ThrowsException()
    {
        var reservation = new Reservation
        {
            Id = 1,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(-1),
            Car = new Car { DailyRate = 100m }
        };

        await Assert.ThrowsAsync<InvalidTariffCalculationException>(
            () => _service.CalculateBaseTariffAsync(reservation));
    }

    [Fact]
    public async Task CalculateBaseTariffAsync_SameDayReservation_ReturnsMinimumOneDayRate()
    {
        var reservation = new Reservation
        {
            Id = 1,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddHours(5),
            Car = new Car { DailyRate = 100m }
        };

        var result = await _service.CalculateBaseTariffAsync(reservation);

        Assert.Equal(100m, result);
    }

    [Fact]
    public async Task CalculateLateFeeAsync_OnTimeReturn_ReturnsZero()
    {
        var reservation = new Reservation
        {
            Id = 1,
            EndDate = DateTime.Now.AddDays(1)
        };
        var actualReturnDate = DateTime.Now;

        var result = await _service.CalculateLateFeeAsync(reservation, actualReturnDate);

        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task CalculateLateFeeAsync_LateReturn_ReturnsCorrectFee()
    {
        var reservation = new Reservation
        {
            Id = 1,
            EndDate = DateTime.Now
        };
        var actualReturnDate = DateTime.Now.AddDays(3);

        var result = await _service.CalculateLateFeeAsync(reservation, actualReturnDate);

        Assert.Equal(300m, result);
    }

    [Fact]
    public async Task CalculateTotalWithRoadTaxAsync_ValidAmount_ReturnsAmountPlusTax()
    {
        _mockExternalApi.Setup(x => x.GetRoadTaxForZoneAsync("urban"))
            .ReturnsAsync(50m);

        var result = await _service.CalculateTotalWithRoadTaxAsync(1000m, "urban");

        Assert.Equal(1050m, result);
    }

    [Fact]
    public async Task CalculateTotalWithRoadTaxAsync_NegativeAmount_ThrowsException()
    {
        await Assert.ThrowsAsync<InvalidTariffCalculationException>(
            () => _service.CalculateTotalWithRoadTaxAsync(-100m, "urban"));
    }

    [Fact]
    public async Task CalculateTotalWithRoadTaxAsync_ApiFailure_UsesDefaultTax()
    {
        _mockExternalApi.Setup(x => x.GetRoadTaxForZoneAsync(It.IsAny<string>()))
            .ThrowsAsync(new ExternalApiException("API failed"));

        var result = await _service.CalculateTotalWithRoadTaxAsync(1000m, "urban");

        Assert.Equal(1025m, result);
    }

    [Fact]
    public async Task ConvertCurrencyAsync_SameCurrency_ReturnsOriginalAmount()
    {
        var result = await _service.ConvertCurrencyAsync(1000m, "RON", "RON");

        Assert.Equal(1000m, result);
    }

    [Fact]
    public async Task ConvertCurrencyAsync_DifferentCurrency_ReturnsConvertedAmount()
    {
        _mockExternalApi.Setup(x => x.GetExchangeRateAsync("RON", "EUR"))
            .ReturnsAsync(0.20m);

        var result = await _service.ConvertCurrencyAsync(1000m, "RON", "EUR");

        Assert.Equal(200m, result);
    }

    [Fact]
    public async Task ConvertCurrencyAsync_NegativeAmount_ThrowsException()
    {
        await Assert.ThrowsAsync<InvalidTariffCalculationException>(
            () => _service.ConvertCurrencyAsync(-100m, "RON", "EUR"));
    }

    [Fact]
    public async Task ConvertCurrencyAsync_ApiFailure_ThrowsException()
    {
        _mockExternalApi.Setup(x => x.GetExchangeRateAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new ExternalApiException("API failed"));

        await Assert.ThrowsAsync<InvalidTariffCalculationException>(
            () => _service.ConvertCurrencyAsync(1000m, "RON", "EUR"));
    }

    [Fact]
    public async Task CalculateCompleteTariffAsync_ValidReservation_ReturnsCompleteCalculation()
    {
        var reservation = new Reservation
        {
            Id = 1,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(5),
            Car = new Car { DailyRate = 150m }
        };

        _mockExternalApi.Setup(x => x.GetRoadTaxForZoneAsync("urban"))
            .ReturnsAsync(50m);

        var result = await _service.CalculateCompleteTariffAsync(reservation);

        Assert.Equal(1, result.ReservationId);
        Assert.Equal(750m, result.BaseRate);
        Assert.Equal(0m, result.LateFee);
        Assert.Equal(50m, result.RoadTax);
        Assert.Equal(800m, result.TotalAmount);
        Assert.Equal("RON", result.Currency);
    }

    [Fact]
    public async Task CalculateCompleteTariffAsync_WithLateFee_IncludesLateFeeInTotal()
    {
        var reservation = new Reservation
        {
            Id = 1,
            StartDate = DateTime.Now.AddDays(-5),
            EndDate = DateTime.Now.AddDays(-2),
            Car = new Car { DailyRate = 100m }
        };

        var actualReturnDate = DateTime.Now;

        _mockExternalApi.Setup(x => x.GetRoadTaxForZoneAsync("urban"))
            .ReturnsAsync(50m);

        var result = await _service.CalculateCompleteTariffAsync(reservation, actualReturnDate);

        Assert.Equal(300m, result.BaseRate);
        Assert.Equal(200m, result.LateFee);
        Assert.Equal(550m, result.TotalAmount);
    }

    [Fact]
    public async Task CalculateCompleteTariffAsync_WithCurrencyConversion_ReturnsConvertedAmount()
    {
        var reservation = new Reservation
        {
            Id = 1,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(2),
            Car = new Car { DailyRate = 100m }
        };

        _mockExternalApi.Setup(x => x.GetRoadTaxForZoneAsync("urban"))
            .ReturnsAsync(50m);
        _mockExternalApi.Setup(x => x.GetExchangeRateAsync("RON", "EUR"))
            .ReturnsAsync(0.20m);

        var result = await _service.CalculateCompleteTariffAsync(reservation, null, "urban", "EUR");

        Assert.Equal("EUR", result.Currency);
        Assert.Equal(0.20m, result.CurrencyConversionRate);
        Assert.Equal(50m, result.TotalAmount);
    }
}
