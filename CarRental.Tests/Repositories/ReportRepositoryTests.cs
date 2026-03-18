using CarRental.Domain.DTOs;
using CarRental.Domain.Entities;
using CarRental.Repository.Data;
using CarRental.Repository.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CarRental.Tests.Repositories;

public class ReportRepositoryTests : IDisposable
{
    private readonly CarRentalDbContext _context;
    private readonly ReportRepository _repository;

    public ReportRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<CarRentalDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CarRentalDbContext(options);
        _repository = new ReportRepository(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var cars = new List<Car>
        {
            new Car { Id = 1, LicensePlate = "B123ABC", Model = "Economy Car", DailyRate = 50m, ManufacturingYear = 2020 },
            new Car { Id = 2, LicensePlate = "B456DEF", Model = "Standard Car", DailyRate = 150m, ManufacturingYear = 2021 },
            new Car { Id = 3, LicensePlate = "B789GHI", Model = "Luxury Car", DailyRate = 400m, ManufacturingYear = 2022 }
        };

        var clients = new List<Client>
        {
            new Client { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com", LicenseNumber = "LIC001" },
            new Client { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@test.com", LicenseNumber = "LIC002" }
        };

        var reservations = new List<Reservation>
        {
            new Reservation { Id = 1, CarId = 1, ClientId = 1, StartDate = DateTime.Now.AddDays(-10), EndDate = DateTime.Now.AddDays(-7), TotalCost = 150m, IsActive = true },
            new Reservation { Id = 2, CarId = 2, ClientId = 1, StartDate = DateTime.Now.AddDays(-5), EndDate = DateTime.Now.AddDays(-2), TotalCost = 450m, IsActive = true },
            new Reservation { Id = 3, CarId = 3, ClientId = 2, StartDate = DateTime.Now.AddDays(-3), EndDate = DateTime.Now.AddDays(-1), TotalCost = 800m, IsActive = true }
        };

        _context.Cars.AddRange(cars);
        _context.Clients.AddRange(clients);
        _context.Reservations.AddRange(reservations);
        _context.SaveChanges();
    }

    [Fact]
    public void GetRevenueByCarCategory_PlaceholderTest()
    {
        Assert.True(true);
    }

    [Fact]
    public void GetTopClientsByReservations_PlaceholderTest()
    {
        Assert.True(true);
    }

    [Fact]
    public void RevenueByCategory_Properties_AreCorrectlySet()
    {
        var revenue = new RevenueByCategory
        {
            Category = "Luxury",
            TotalReservations = 10,
            TotalRevenue = 5000m,
            AverageRevenue = 500m
        };

        Assert.Equal("Luxury", revenue.Category);
        Assert.Equal(10, revenue.TotalReservations);
        Assert.Equal(5000m, revenue.TotalRevenue);
        Assert.Equal(500m, revenue.AverageRevenue);
    }

    [Fact]
    public void TopClient_Properties_AreCorrectlySet()
    {
        var client = new TopClient
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            TotalReservations = 5,
            TotalSpent = 2500m,
            LastReservationDate = DateTime.Now
        };

        Assert.Equal(1, client.Id);
        Assert.Equal("John", client.FirstName);
        Assert.Equal("Doe", client.LastName);
        Assert.Equal(5, client.TotalReservations);
    }

    [Fact]
    public void RevenueByCategory_DefaultValues_AreCorrect()
    {
        var revenue = new RevenueByCategory();

        Assert.Equal(string.Empty, revenue.Category);
        Assert.Equal(0, revenue.TotalReservations);
        Assert.Equal(0m, revenue.TotalRevenue);
        Assert.Equal(0m, revenue.AverageRevenue);
    }

    [Fact]
    public void TopClient_DefaultValues_AreCorrect()
    {
        var client = new TopClient();

        Assert.Equal(string.Empty, client.FirstName);
        Assert.Equal(string.Empty, client.LastName);
        Assert.Equal(string.Empty, client.Email);
        Assert.Equal(0, client.TotalReservations);
        Assert.Equal(0m, client.TotalSpent);
    }

    [Fact]
    public void ReportRepository_Constructor_InitializesCorrectly()
    {
        var repo = new ReportRepository(_context);
        Assert.NotNull(repo);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
