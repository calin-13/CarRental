using CarRental.Domain.Entities;
using CarRental.Domain.Exceptions;
using CarRental.Repository.Interfaces;
using CarRental.Service.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CarRental.Tests.Services;

public class ReservationServiceTests
{
    private readonly Mock<IReservationRepository> _mockReservationRepository;
    private readonly Mock<ICarRepository> _mockCarRepository;
    private readonly Mock<IClientRepository> _mockClientRepository;
    private readonly Mock<ILogger<ReservationService>> _mockLogger;
    private readonly ReservationService _reservationService;

    public ReservationServiceTests()
    {
        _mockReservationRepository = new Mock<IReservationRepository>();
        _mockCarRepository = new Mock<ICarRepository>();
        _mockClientRepository = new Mock<IClientRepository>();
        _mockLogger = new Mock<ILogger<ReservationService>>();
        _reservationService = new ReservationService(
            _mockReservationRepository.Object,
            _mockCarRepository.Object,
            _mockClientRepository.Object,
            _mockLogger.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_NullReservationRepository_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ReservationService(
            null!,
            _mockCarRepository.Object,
            _mockClientRepository.Object,
            _mockLogger.Object));
    }

    [Fact]
    public void Constructor_NullCarRepository_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ReservationService(
            _mockReservationRepository.Object,
            null!,
            _mockClientRepository.Object,
            _mockLogger.Object));
    }

    [Fact]
    public void Constructor_NullClientRepository_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ReservationService(
            _mockReservationRepository.Object,
            _mockCarRepository.Object,
            null!,
            _mockLogger.Object));
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ReservationService(
            _mockReservationRepository.Object,
            _mockCarRepository.Object,
            _mockClientRepository.Object,
            null!));
    }

    #endregion

    #region GetAllReservationsAsync Tests

    [Fact]
    public async Task GetAllReservationsAsync_ShouldReturnAllReservations_WhenReservationsExist()
    {
        var expectedReservations = new List<Reservation>
        {
            new Reservation { Id = 1, CarId = 1, ClientId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(3) },
            new Reservation { Id = 2, CarId = 2, ClientId = 2, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(5) }
        };
        _mockReservationRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(expectedReservations);

        var result = await _reservationService.GetAllReservationsAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockReservationRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllReservationsAsync_ShouldReturnEmptyList_WhenNoReservationsExist()
    {
        _mockReservationRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Reservation>());

        var result = await _reservationService.GetAllReservationsAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region GetReservationByIdAsync Tests

    [Fact]
    public async Task GetReservationByIdAsync_ShouldReturnReservation_WhenReservationExists()
    {
        var expectedReservation = new Reservation { Id = 1, CarId = 1, ClientId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(3) };
        _mockReservationRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expectedReservation);

        var result = await _reservationService.GetReservationByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        _mockReservationRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetReservationByIdAsync_ShouldReturnNull_WhenReservationDoesNotExist()
    {
        _mockReservationRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Reservation?)null);

        var result = await _reservationService.GetReservationByIdAsync(999);

        Assert.Null(result);
    }

    #endregion

    #region CreateReservationAsync Tests

    [Fact]
    public async Task CreateReservationAsync_ShouldCreateReservation_WhenValidData()
    {
        var car = new Car { Id = 1, LicensePlate = "ABC-123", Model = "Toyota", ManufacturingYear = 2020, DailyRate = 50, IsAvailable = true };
        var client = new Client { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com", LicenseNumber = "123456" };
        var reservation = new Reservation { CarId = 1, ClientId = 1, StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(4) };
        var createdReservation = new Reservation { Id = 1, CarId = 1, ClientId = 1, StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(4), TotalCost = 150 };

        _mockCarRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(car);
        _mockClientRepository.Setup(r => r.GetById(1)).Returns(client);
        _mockReservationRepository.Setup(r => r.HasOverlappingReservationsAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null)).ReturnsAsync(false);
        _mockReservationRepository.Setup(r => r.AddAsync(It.IsAny<Reservation>())).ReturnsAsync(createdReservation);

        var result = await _reservationService.CreateReservationAsync(reservation);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.True(result.IsActive);
        _mockReservationRepository.Verify(r => r.AddAsync(It.IsAny<Reservation>()), Times.Once);
    }

    [Fact]
    public async Task CreateReservationAsync_ShouldThrowArgumentNullException_WhenReservationIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _reservationService.CreateReservationAsync(null!));
        _mockReservationRepository.Verify(r => r.AddAsync(It.IsAny<Reservation>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_ShouldThrowInvalidDateException_WhenStartDateIsInPast()
    {
        var reservation = new Reservation { CarId = 1, ClientId = 1, StartDate = DateTime.Today.AddDays(-1), EndDate = DateTime.Today.AddDays(3) };

        var exception = await Assert.ThrowsAsync<InvalidDateException>(() => _reservationService.CreateReservationAsync(reservation));
        Assert.Contains("Start date cannot be in the past", exception.Message);
        _mockReservationRepository.Verify(r => r.AddAsync(It.IsAny<Reservation>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_ShouldThrowInvalidDateException_WhenEndDateIsBeforeStartDate()
    {
        var reservation = new Reservation { CarId = 1, ClientId = 1, StartDate = DateTime.Today.AddDays(3), EndDate = DateTime.Today.AddDays(1) };

        var exception = await Assert.ThrowsAsync<InvalidDateException>(() => _reservationService.CreateReservationAsync(reservation));
        Assert.Contains("End date must be after start date", exception.Message);
        _mockReservationRepository.Verify(r => r.AddAsync(It.IsAny<Reservation>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_ShouldThrowInvalidDateException_WhenStartDateEqualsEndDate()
    {
        var date = DateTime.Today.AddDays(1);
        var reservation = new Reservation { CarId = 1, ClientId = 1, StartDate = date, EndDate = date };

        var exception = await Assert.ThrowsAsync<InvalidDateException>(() => _reservationService.CreateReservationAsync(reservation));
        Assert.Contains("Start date and end date cannot be the same", exception.Message);
        _mockReservationRepository.Verify(r => r.AddAsync(It.IsAny<Reservation>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_ShouldThrowArgumentException_WhenCarDoesNotExist()
    {
        var reservation = new Reservation { CarId = 999, ClientId = 1, StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(4) };
        _mockCarRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Car?)null);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _reservationService.CreateReservationAsync(reservation));
        Assert.Contains("does not exist", exception.Message);
        _mockReservationRepository.Verify(r => r.AddAsync(It.IsAny<Reservation>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_ShouldThrowCarUnavailableException_WhenCarIsNotAvailable()
    {
        var car = new Car { Id = 1, LicensePlate = "ABC-123", Model = "Toyota", ManufacturingYear = 2020, DailyRate = 50, IsAvailable = false };
        var reservation = new Reservation { CarId = 1, ClientId = 1, StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(4) };
        _mockCarRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(car);

        var exception = await Assert.ThrowsAsync<CarUnavailableException>(() => _reservationService.CreateReservationAsync(reservation));
        Assert.Equal(1, exception.CarId);
        _mockReservationRepository.Verify(r => r.AddAsync(It.IsAny<Reservation>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_ShouldThrowCarUnavailableException_WhenCarHasOverlappingReservation()
    {
        var car = new Car { Id = 1, LicensePlate = "ABC-123", Model = "Toyota", ManufacturingYear = 2020, DailyRate = 50, IsAvailable = true };
        var client = new Client { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com", LicenseNumber = "123456" };
        var reservation = new Reservation { CarId = 1, ClientId = 1, StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(4) };

        _mockCarRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(car);
        _mockClientRepository.Setup(r => r.GetById(1)).Returns(client);
        _mockReservationRepository.Setup(r => r.HasOverlappingReservationsAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null)).ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<CarUnavailableException>(() => _reservationService.CreateReservationAsync(reservation));
        Assert.Equal(1, exception.CarId);
        Assert.Contains("already reserved", exception.Message);
        _mockReservationRepository.Verify(r => r.AddAsync(It.IsAny<Reservation>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_ShouldThrowArgumentException_WhenClientDoesNotExist()
    {
        var car = new Car { Id = 1, LicensePlate = "ABC-123", Model = "Toyota", ManufacturingYear = 2020, DailyRate = 50, IsAvailable = true };
        var reservation = new Reservation { CarId = 1, ClientId = 999, StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(4) };

        _mockCarRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(car);
        _mockClientRepository.Setup(r => r.GetById(999)).Returns((Client?)null);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _reservationService.CreateReservationAsync(reservation));
        Assert.Contains("does not exist", exception.Message);
        _mockReservationRepository.Verify(r => r.AddAsync(It.IsAny<Reservation>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_ShouldCalculateTotalCost_WhenValidReservation()
    {
        var car = new Car { Id = 1, LicensePlate = "ABC-123", Model = "Toyota", ManufacturingYear = 2020, DailyRate = 50, IsAvailable = true };
        var client = new Client { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com", LicenseNumber = "123456" };
        var reservation = new Reservation { CarId = 1, ClientId = 1, StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(4) };
        var createdReservation = new Reservation { Id = 1, CarId = 1, ClientId = 1, StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(4), TotalCost = 150 };

        _mockCarRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(car);
        _mockClientRepository.Setup(r => r.GetById(1)).Returns(client);
        _mockReservationRepository.Setup(r => r.HasOverlappingReservationsAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null)).ReturnsAsync(false);
        _mockReservationRepository.Setup(r => r.AddAsync(It.IsAny<Reservation>())).ReturnsAsync(createdReservation);

        var result = await _reservationService.CreateReservationAsync(reservation);

        Assert.Equal(150, result.TotalCost);
    }

    #endregion

    #region UpdateReservationAsync Tests

    [Fact]
    public async Task UpdateReservationAsync_ShouldUpdateReservation_WhenValidData()
    {
        var existingReservation = new Reservation { Id = 1, CarId = 1, ClientId = 1, StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(4), IsActive = true };
        var car = new Car { Id = 1, LicensePlate = "ABC-123", Model = "Toyota", ManufacturingYear = 2020, DailyRate = 50, IsAvailable = true };
        var updatedReservation = new Reservation { Id = 1, CarId = 1, ClientId = 1, StartDate = DateTime.Today.AddDays(2), EndDate = DateTime.Today.AddDays(5), TotalCost = 150 };

        _mockReservationRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingReservation);
        _mockCarRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(car);
        _mockReservationRepository.Setup(r => r.HasOverlappingReservationsAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1)).ReturnsAsync(false);
        _mockReservationRepository.Setup(r => r.UpdateAsync(It.IsAny<Reservation>())).ReturnsAsync(updatedReservation);

        var result = await _reservationService.UpdateReservationAsync(updatedReservation);

        Assert.NotNull(result);
        _mockReservationRepository.Verify(r => r.UpdateAsync(It.IsAny<Reservation>()), Times.Once);
    }

    [Fact]
    public async Task UpdateReservationAsync_ShouldThrowArgumentNullException_WhenReservationIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _reservationService.UpdateReservationAsync(null!));
        _mockReservationRepository.Verify(r => r.UpdateAsync(It.IsAny<Reservation>()), Times.Never);
    }

    [Fact]
    public async Task UpdateReservationAsync_ShouldThrowArgumentException_WhenReservationDoesNotExist()
    {
        var reservation = new Reservation { Id = 999, CarId = 1, ClientId = 1, StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(4) };
        _mockReservationRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Reservation?)null);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _reservationService.UpdateReservationAsync(reservation));
        Assert.Contains("does not exist", exception.Message);
        _mockReservationRepository.Verify(r => r.UpdateAsync(It.IsAny<Reservation>()), Times.Never);
    }

    [Fact]
    public async Task UpdateReservationAsync_ShouldThrowCarUnavailableException_WhenCarHasOverlappingReservation()
    {
        var existingReservation = new Reservation { Id = 1, CarId = 1, ClientId = 1, StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(4), IsActive = true };
        var car = new Car { Id = 1, LicensePlate = "ABC-123", Model = "Toyota", ManufacturingYear = 2020, DailyRate = 50, IsAvailable = true };
        var reservation = new Reservation { Id = 1, CarId = 1, ClientId = 1, StartDate = DateTime.Today.AddDays(2), EndDate = DateTime.Today.AddDays(5) };

        _mockReservationRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingReservation);
        _mockCarRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(car);
        _mockReservationRepository.Setup(r => r.HasOverlappingReservationsAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1)).ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<CarUnavailableException>(() => _reservationService.UpdateReservationAsync(reservation));
        Assert.Equal(1, exception.CarId);
        _mockReservationRepository.Verify(r => r.UpdateAsync(It.IsAny<Reservation>()), Times.Never);
    }

    #endregion

    #region CancelReservationAsync Tests

    [Fact]
    public async Task CancelReservationAsync_ShouldCancelReservation_WhenReservationExists()
    {
        var reservation = new Reservation { Id = 1, CarId = 1, ClientId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(3), IsActive = true };
        _mockReservationRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(reservation);
        _mockReservationRepository.Setup(r => r.UpdateAsync(It.IsAny<Reservation>())).ReturnsAsync(reservation);

        var result = await _reservationService.CancelReservationAsync(1);

        Assert.True(result);
        Assert.False(reservation.IsActive);
        Assert.NotNull(reservation.ActualReturnDate);
        _mockReservationRepository.Verify(r => r.UpdateAsync(It.IsAny<Reservation>()), Times.Once);
    }

    [Fact]
    public async Task CancelReservationAsync_ShouldReturnFalse_WhenReservationDoesNotExist()
    {
        _mockReservationRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Reservation?)null);

        var result = await _reservationService.CancelReservationAsync(999);

        Assert.False(result);
        _mockReservationRepository.Verify(r => r.UpdateAsync(It.IsAny<Reservation>()), Times.Never);
    }

    [Fact]
    public async Task CancelReservationAsync_ShouldReturnFalse_WhenReservationIsAlreadyInactive()
    {
        var reservation = new Reservation { Id = 1, CarId = 1, ClientId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(3), IsActive = false };
        _mockReservationRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(reservation);

        var result = await _reservationService.CancelReservationAsync(1);

        Assert.False(result);
        _mockReservationRepository.Verify(r => r.UpdateAsync(It.IsAny<Reservation>()), Times.Never);
    }

    #endregion

    #region CalculateReservationCostAsync Tests

    [Fact]
    public async Task CalculateReservationCostAsync_ShouldCalculateCorrectCost_WhenValidDates()
    {
        var car = new Car { Id = 1, LicensePlate = "ABC-123", Model = "Toyota", ManufacturingYear = 2020, DailyRate = 50, IsAvailable = true };
        _mockCarRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(car);

        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(4);
        var result = await _reservationService.CalculateReservationCostAsync(1, startDate, endDate);

        Assert.Equal(150, result);
    }

    [Fact]
    public async Task CalculateReservationCostAsync_ShouldThrowArgumentException_WhenCarDoesNotExist()
    {
        _mockCarRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Car?)null);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _reservationService.CalculateReservationCostAsync(999, DateTime.Today.AddDays(1), DateTime.Today.AddDays(4)));
        Assert.Contains("does not exist", exception.Message);
    }

    [Fact]
    public async Task CalculateReservationCostAsync_ShouldThrowInvalidDateException_WhenStartDateIsAfterEndDate()
    {
        var car = new Car { Id = 1, LicensePlate = "ABC-123", Model = "Toyota", ManufacturingYear = 2020, DailyRate = 50, IsAvailable = true };
        _mockCarRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(car);

        var exception = await Assert.ThrowsAsync<InvalidDateException>(() => 
            _reservationService.CalculateReservationCostAsync(1, DateTime.Today.AddDays(4), DateTime.Today.AddDays(1)));
        Assert.Contains("Start date must be before end date", exception.Message);
    }

    [Fact]
    public async Task CalculateReservationCostAsync_ShouldCalculateForOneDay_WhenStartAndEndAreConsecutive()
    {
        var car = new Car { Id = 1, LicensePlate = "ABC-123", Model = "Toyota", ManufacturingYear = 2020, DailyRate = 50, IsAvailable = true };
        _mockCarRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(car);

        var startDate = DateTime.Today.AddDays(1);
        var endDate = startDate.AddDays(1);
        var result = await _reservationService.CalculateReservationCostAsync(1, startDate, endDate);

        Assert.Equal(50, result);
    }

    #endregion

    #region IsCarAvailableForReservationAsync Tests

    [Fact]
    public async Task IsCarAvailableForReservationAsync_ShouldReturnTrue_WhenCarIsAvailable()
    {
        var car = new Car { Id = 1, LicensePlate = "ABC-123", Model = "Toyota", ManufacturingYear = 2020, DailyRate = 50, IsAvailable = true };
        _mockCarRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(car);
        _mockReservationRepository.Setup(r => r.HasOverlappingReservationsAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null)).ReturnsAsync(false);

        var result = await _reservationService.IsCarAvailableForReservationAsync(1, DateTime.Today.AddDays(1), DateTime.Today.AddDays(4));

        Assert.True(result);
    }

    [Fact]
    public async Task IsCarAvailableForReservationAsync_ShouldReturnFalse_WhenCarIsNotAvailable()
    {
        var car = new Car { Id = 1, LicensePlate = "ABC-123", Model = "Toyota", ManufacturingYear = 2020, DailyRate = 50, IsAvailable = false };
        _mockCarRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(car);

        var result = await _reservationService.IsCarAvailableForReservationAsync(1, DateTime.Today.AddDays(1), DateTime.Today.AddDays(4));

        Assert.False(result);
    }

    [Fact]
    public async Task IsCarAvailableForReservationAsync_ShouldReturnFalse_WhenCarHasOverlappingReservation()
    {
        var car = new Car { Id = 1, LicensePlate = "ABC-123", Model = "Toyota", ManufacturingYear = 2020, DailyRate = 50, IsAvailable = true };
        _mockCarRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(car);
        _mockReservationRepository.Setup(r => r.HasOverlappingReservationsAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null)).ReturnsAsync(true);

        var result = await _reservationService.IsCarAvailableForReservationAsync(1, DateTime.Today.AddDays(1), DateTime.Today.AddDays(4));

        Assert.False(result);
    }

    [Fact]
    public async Task IsCarAvailableForReservationAsync_ShouldReturnFalse_WhenCarDoesNotExist()
    {
        _mockCarRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Car?)null);

        var result = await _reservationService.IsCarAvailableForReservationAsync(999, DateTime.Today.AddDays(1), DateTime.Today.AddDays(4));

        Assert.False(result);
    }

    #endregion
}

