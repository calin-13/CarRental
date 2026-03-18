using CarRental.Domain.Exceptions;
using CarRental.Service.Interfaces;
using CarRental.Service.Services;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using Xunit;

namespace CarRental.Tests.Services;

public class ExternalApiServiceTests
{
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly ExternalApiService _service;

    public ExternalApiServiceTests()
    {
        _mockLogger = new Mock<ILoggerService>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _service = new ExternalApiService(_httpClient, _mockLogger.Object);
    }

    [Fact]
    public async Task GetExchangeRateAsync_ValidCurrencies_ReturnsExchangeRate()
    {
        var jsonResponse = @"{
            ""rates"": {
                ""EUR"": 0.21,
                ""USD"": 0.23
            }
        }";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            });
        
        var result = await _service.GetExchangeRateAsync("RON", "EUR");
        
        Assert.Equal(0.21m, result);
        _mockLogger.Verify(x => x.LogInfo(It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetExchangeRateAsync_InvalidCurrency_ThrowsExternalApiException()
    {
        var jsonResponse = @"{
            ""rates"": {
                ""EUR"": 0.21
            }
        }";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            });
        
        await Assert.ThrowsAsync<ExternalApiException>(
            () => _service.GetExchangeRateAsync("RON", "INVALID"));
    }

    [Fact]
    public async Task GetExchangeRateAsync_ApiReturnsError_ThrowsExternalApiException()
    {
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });
        
        await Assert.ThrowsAsync<ExternalApiException>(
            () => _service.GetExchangeRateAsync("RON", "EUR"));
    }

    [Fact]
    public async Task GetExchangeRateAsync_NetworkError_ThrowsExternalApiException()
    {
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));
        
        await Assert.ThrowsAsync<ExternalApiException>(
            () => _service.GetExchangeRateAsync("RON", "EUR"));
        
        _mockLogger.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
    }

    [Fact]
    public async Task GetRoadTaxForZoneAsync_UrbanZone_ReturnsCorrectTax()
    {
        var result = await _service.GetRoadTaxForZoneAsync("urban");
        
        Assert.Equal(50.0m, result);
        _mockLogger.Verify(x => x.LogInfo(It.Is<string>(s => s.Contains("urban"))), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetRoadTaxForZoneAsync_SuburbanZone_ReturnsCorrectTax()
    {
        var result = await _service.GetRoadTaxForZoneAsync("suburban");
        
        Assert.Equal(30.0m, result);
    }

    [Fact]
    public async Task GetRoadTaxForZoneAsync_RuralZone_ReturnsCorrectTax()
    {
        var result = await _service.GetRoadTaxForZoneAsync("rural");
        
        Assert.Equal(20.0m, result);
    }

    [Fact]
    public async Task GetRoadTaxForZoneAsync_HighwayZone_ReturnsCorrectTax()
    {
        var result = await _service.GetRoadTaxForZoneAsync("highway");
        
        Assert.Equal(40.0m, result);
    }

    [Fact]
    public async Task GetRoadTaxForZoneAsync_UnknownZone_ReturnsDefaultTax()
    {
        var result = await _service.GetRoadTaxForZoneAsync("unknown");
        
        Assert.Equal(25.0m, result);
        _mockLogger.Verify(x => x.LogWarning(It.IsAny<string>()), Times.Once);
    }
}
