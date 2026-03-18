using CarRental.Domain.Entities;
using CarRental.Repository.Interfaces;
using CarRental.Service.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CarRental.Tests.Services;

/// Unit tests for CarService
public class CarServiceTests
{
    private readonly Mock<ICarRepository> _mockRepository;
    private readonly Mock<ILogger<CarService>> _mockLogger;
    private readonly CarService _carService;

    public CarServiceTests()
    {
        _mockRepository = new Mock<ICarRepository>();
        _mockLogger = new Mock<ILogger<CarService>>();
        _carService = new CarService(_mockRepository.Object, _mockLogger.Object);
    }

    #region GetAllCarsAsync Tests

    [Fact]
    public async Task GetAllCarsAsync_ShouldReturnAllCars_WhenCarsExist()
    {
        var expectedCars = new List<Car>
        {
            new Car { Id = 1, LicensePlate = "ABC-123", Model = "Toyota Corolla", ManufacturingYear = 2020, DailyRate = 50 },
            new Car { Id = 2, LicensePlate = "XYZ-456", Model = "Honda Civic", ManufacturingYear = 2021, DailyRate = 55 }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(expectedCars);

        var result = await _carService.GetAllCarsAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllCarsAsync_ShouldReturnEmptyList_WhenNoCarsExist()
    {
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Car>());

        var result = await _carService.GetAllCarsAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllCarsAsync_ShouldLogError_WhenExceptionOccurs()
    {
        _mockRepository.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception("Database error"));

        await Assert.ThrowsAsync<Exception>(() => _carService.GetAllCarsAsync());
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    #endregion

    #region GetCarByIdAsync Tests

    [Fact]
    public async Task GetCarByIdAsync_ShouldReturnCar_WhenCarExists()
    {
        var expectedCar = new Car { Id = 1, LicensePlate = "ABC-123", Model = "Toyota Corolla", ManufacturingYear = 2020, DailyRate = 50 };
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expectedCar);

        var result = await _carService.GetCarByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Toyota Corolla", result.Model);
        _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetCarByIdAsync_ShouldReturnNull_WhenCarDoesNotExist()
    {
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Car?)null);

        var result = await _carService.GetCarByIdAsync(999);

        Assert.Null(result);
        _mockRepository.Verify(r => r.GetByIdAsync(999), Times.Once);
    }

    [Fact]
    public async Task GetCarByIdAsync_ShouldLogError_WhenExceptionOccurs()
    {
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ThrowsAsync(new Exception("Database error"));

        await Assert.ThrowsAsync<Exception>(() => _carService.GetCarByIdAsync(1));
    }

    #endregion

    #region CreateCarAsync Validation Tests

    [Fact]
    public async Task CreateCarAsync_ShouldCreateCar_WhenValidCar()
    {
        var car = new Car
        {
            LicensePlate = "ABC-123",
            Model = "Toyota Corolla",
            ManufacturingYear = 2020,
            DailyRate = 50
        };
        var createdCar = new Car { Id = 1, LicensePlate = "ABC-123", Model = "Toyota Corolla", ManufacturingYear = 2020, DailyRate = 50 };
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Car>())).ReturnsAsync(createdCar);
        
        var result = await _carService.CreateCarAsync(car);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Car>()), Times.Once);
    }

    [Fact]
    public async Task CreateCarAsync_ShouldThrowArgumentNullException_WhenCarIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _carService.CreateCarAsync(null!));
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Car>()), Times.Never);
    }

    [Fact]
    public async Task CreateCarAsync_ShouldThrowArgumentException_WhenDailyRateIsZero()
    {
        var car = new Car { LicensePlate = "ABC-123", Model = "Toyota Corolla", ManufacturingYear = 2020, DailyRate = 0 };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _carService.CreateCarAsync(car));
        Assert.Contains("Daily rate must be greater than 0", exception.Message);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Car>()), Times.Never);
    }

    [Fact]
    public async Task CreateCarAsync_ShouldThrowArgumentException_WhenDailyRateIsNegative()
    {
        var car = new Car { LicensePlate = "ABC-123", Model = "Toyota Corolla", ManufacturingYear = 2020, DailyRate = -10 };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _carService.CreateCarAsync(car));
        Assert.Contains("Daily rate must be greater than 0", exception.Message);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Car>()), Times.Never);
    }

    [Fact]
    public async Task CreateCarAsync_ShouldThrowArgumentException_WhenManufacturingYearIsFuture()
    {
        int futureYear = DateTime.Now.Year + 1;
        var car = new Car { LicensePlate = "ABC-123", Model = "Toyota Corolla", ManufacturingYear = futureYear, DailyRate = 50 };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _carService.CreateCarAsync(car));
        Assert.Contains("Manufacturing year must be less than or equal to", exception.Message);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Car>()), Times.Never);
    }

    [Fact]
    public async Task CreateCarAsync_ShouldAcceptCurrentYear_WhenManufacturingYearIsCurrentYear()
    {
        int currentYear = DateTime.Now.Year;
        var car = new Car { LicensePlate = "ABC-123", Model = "Toyota Corolla", ManufacturingYear = currentYear, DailyRate = 50 };
        var createdCar = new Car { Id = 1, LicensePlate = "ABC-123", Model = "Toyota Corolla", ManufacturingYear = currentYear, DailyRate = 50 };
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Car>())).ReturnsAsync(createdCar);

        var result = await _carService.CreateCarAsync(car);

        Assert.NotNull(result);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Car>()), Times.Once);
    }

    [Fact]
    public async Task CreateCarAsync_ShouldThrowArgumentException_WhenModelIsNull()
    {
        var car = new Car { LicensePlate = "ABC-123", Model = null!, ManufacturingYear = 2020, DailyRate = 50 };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _carService.CreateCarAsync(car));
        Assert.Contains("Model must have at least 4 characters", exception.Message);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Car>()), Times.Never);
    }

    [Fact]
    public async Task CreateCarAsync_ShouldThrowArgumentException_WhenModelIsEmpty()
    {
        var car = new Car { LicensePlate = "ABC-123", Model = string.Empty, ManufacturingYear = 2020, DailyRate = 50 };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _carService.CreateCarAsync(car));
        Assert.Contains("Model must have at least 4 characters", exception.Message);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Car>()), Times.Never);
    }

    [Fact]
    public async Task CreateCarAsync_ShouldThrowArgumentException_WhenModelIsWhitespace()
    {
        var car = new Car { LicensePlate = "ABC-123", Model = "   ", ManufacturingYear = 2020, DailyRate = 50 };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _carService.CreateCarAsync(car));
        Assert.Contains("Model must have at least 4 characters", exception.Message);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Car>()), Times.Never);
    }

    [Fact]
    public async Task CreateCarAsync_ShouldThrowArgumentException_WhenModelHasLessThan4Characters()
    {
        var car = new Car { LicensePlate = "ABC-123", Model = "ABC", ManufacturingYear = 2020, DailyRate = 50 };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _carService.CreateCarAsync(car));
        Assert.Contains("Model must have at least 4 characters", exception.Message);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Car>()), Times.Never);
    }

    [Fact]
    public async Task CreateCarAsync_ShouldAcceptModel_WhenModelHasExactly4Characters()
    {
        var car = new Car { LicensePlate = "ABC-123", Model = "ABCD", ManufacturingYear = 2020, DailyRate = 50 };
        var createdCar = new Car { Id = 1, LicensePlate = "ABC-123", Model = "ABCD", ManufacturingYear = 2020, DailyRate = 50 };
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Car>())).ReturnsAsync(createdCar);

        var result = await _carService.CreateCarAsync(car);

        Assert.NotNull(result);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Car>()), Times.Once);
    }

    [Fact]
    public async Task CreateCarAsync_ShouldAcceptModel_WhenModelHasMoreThan4Characters()
    {
        var car = new Car { LicensePlate = "ABC-123", Model = "Toyota Corolla", ManufacturingYear = 2020, DailyRate = 50 };
        var createdCar = new Car { Id = 1, LicensePlate = "ABC-123", Model = "Toyota Corolla", ManufacturingYear = 2020, DailyRate = 50 };
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Car>())).ReturnsAsync(createdCar);

        var result = await _carService.CreateCarAsync(car);

        Assert.NotNull(result);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Car>()), Times.Once);
    }

    [Fact]
    public async Task CreateCarAsync_ShouldThrowArgumentException_WhenAllValidationsFail()
    {
        var car = new Car { LicensePlate = "ABC-123", Model = "AB", ManufacturingYear = 2030, DailyRate = -5 };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _carService.CreateCarAsync(car));
        
        Assert.Contains("Daily rate must be greater than 0", exception.Message);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Car>()), Times.Never);
    }

    [Fact]
    public async Task CreateCarAsync_ShouldLogError_WhenExceptionOccurs()
    {
        var car = new Car { LicensePlate = "ABC-123", Model = "Toyota Corolla", ManufacturingYear = 2020, DailyRate = 50 };
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Car>())).ThrowsAsync(new Exception("Database error"));

        await Assert.ThrowsAsync<Exception>(() => _carService.CreateCarAsync(car));
    }

    #endregion

    #region UpdateCarAsync Tests

    [Fact]
    public async Task UpdateCarAsync_ShouldUpdateCar_WhenValidCar()
    {
        var car = new Car { Id = 1, LicensePlate = "ABC-123", Model = "Toyota Corolla", ManufacturingYear = 2020, DailyRate = 50 };
        _mockRepository.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Car>())).ReturnsAsync(car);

        var result = await _carService.UpdateCarAsync(car);

        Assert.NotNull(result);
        _mockRepository.Verify(r => r.ExistsAsync(1), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Car>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCarAsync_ShouldThrowArgumentException_WhenCarDoesNotExist()
    {
        var car = new Car { Id = 999, LicensePlate = "ABC-123", Model = "Toyota Corolla", ManufacturingYear = 2020, DailyRate = 50 };
        _mockRepository.Setup(r => r.ExistsAsync(999)).ReturnsAsync(false);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _carService.UpdateCarAsync(car));
        Assert.Contains("does not exist", exception.Message);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Car>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCarAsync_ShouldThrowArgumentNullException_WhenCarIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _carService.UpdateCarAsync(null!));
    }

    [Fact]
    public async Task UpdateCarAsync_ShouldThrowArgumentException_WhenDailyRateIsZero()
    {
        var car = new Car { Id = 1, LicensePlate = "ABC-123", Model = "Toyota Corolla", ManufacturingYear = 2020, DailyRate = 0 };
        _mockRepository.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _carService.UpdateCarAsync(car));
        Assert.Contains("Daily rate must be greater than 0", exception.Message);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Car>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCarAsync_ShouldThrowArgumentException_WhenManufacturingYearIsFuture()
    {
        int futureYear = DateTime.Now.Year + 1;
        var car = new Car { Id = 1, LicensePlate = "ABC-123", Model = "Toyota Corolla", ManufacturingYear = futureYear, DailyRate = 50 };
        _mockRepository.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _carService.UpdateCarAsync(car));
        Assert.Contains("Manufacturing year must be less than or equal to", exception.Message);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Car>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCarAsync_ShouldThrowArgumentException_WhenModelIsInvalid()
    {
        var car = new Car { Id = 1, LicensePlate = "ABC-123", Model = "ABC", ManufacturingYear = 2020, DailyRate = 50 };
        _mockRepository.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _carService.UpdateCarAsync(car));
        Assert.Contains("Model must have at least 4 characters", exception.Message);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Car>()), Times.Never);
    }

    [Fact]
    public async Task CreateCarAsync_ShouldThrowArgumentException_WhenLicensePlateIsNull()
    {
        var car = new Car { LicensePlate = null!, Model = "Toyota Corolla", ManufacturingYear = 2020, DailyRate = 50 };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _carService.CreateCarAsync(car));
        Assert.Contains("License plate is required", exception.Message);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Car>()), Times.Never);
    }

    [Fact]
    public async Task CreateCarAsync_ShouldThrowArgumentException_WhenLicensePlateIsEmpty()
    {
        var car = new Car { LicensePlate = string.Empty, Model = "Toyota Corolla", ManufacturingYear = 2020, DailyRate = 50 };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _carService.CreateCarAsync(car));
        Assert.Contains("License plate is required", exception.Message);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Car>()), Times.Never);
    }

    [Fact]
    public async Task CreateCarAsync_ShouldThrowArgumentException_WhenLicensePlateIsWhitespace()
    {
        var car = new Car { LicensePlate = "   ", Model = "Toyota Corolla", ManufacturingYear = 2020, DailyRate = 50 };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _carService.CreateCarAsync(car));
        Assert.Contains("License plate is required", exception.Message);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Car>()), Times.Never);
    }

    #endregion

    #region DeleteCarAsync Tests

    [Fact]
    public async Task DeleteCarAsync_ShouldReturnTrue_WhenCarExists()
    {
        _mockRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _carService.DeleteCarAsync(1);

        Assert.True(result);
        _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteCarAsync_ShouldReturnFalse_WhenCarDoesNotExist()
    {
        _mockRepository.Setup(r => r.DeleteAsync(999)).ReturnsAsync(false);

        var result = await _carService.DeleteCarAsync(999);

        Assert.False(result);
        _mockRepository.Verify(r => r.DeleteAsync(999), Times.Once);
    }

    [Fact]
    public async Task DeleteCarAsync_ShouldLogError_WhenExceptionOccurs()
    {
        _mockRepository.Setup(r => r.DeleteAsync(1)).ThrowsAsync(new Exception("Database error"));

        await Assert.ThrowsAsync<Exception>(() => _carService.DeleteCarAsync(1));
    }

    #endregion
}

