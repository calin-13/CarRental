using CarRental.Domain.Entities;
using CarRental.Repository.Interfaces;
using CarRental.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarRental.Service.Services;

public class CarService : ICarService
{
    private readonly ICarRepository _carRepository;
    private readonly ILogger<CarService> _logger;

    public CarService(ICarRepository carRepository, ILogger<CarService> logger)
    {
        _carRepository = carRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Car>> GetAllCarsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all cars");
            return await _carRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all cars");
            throw;
        }
    }

    public async Task<Car?> GetCarByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Retrieving car with ID: {CarId}", id);
            return await _carRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving car with ID: {CarId}", id);
            throw;
        }
    }

    public async Task<Car> CreateCarAsync(Car car)
    {
        try
        {
            ValidateCar(car);
            
            _logger.LogInformation("Creating new car with model: {Model}", car.Model);
            
            var createdCar = await _carRepository.AddAsync(car);
            _logger.LogInformation("Car created successfully with ID: {CarId}", createdCar.Id);
            
            return createdCar;
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogWarning(ex, "Validation failed: car is null");
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation failed while creating car");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating car");
            throw;
        }
    }

    public async Task<Car> UpdateCarAsync(Car car)
    {
        try
        {
            ValidateCar(car);
            
            _logger.LogInformation("Updating car with ID: {CarId}", car.Id);
            
            if (!await _carRepository.ExistsAsync(car.Id))
            {
                _logger.LogWarning("Car with ID: {CarId} not found for update", car.Id);
                throw new ArgumentException($"Car with ID {car.Id} does not exist.");
            }
            
            var updatedCar = await _carRepository.UpdateAsync(car);
            _logger.LogInformation("Car with ID: {CarId} updated successfully", updatedCar.Id);
            
            return updatedCar;
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogWarning(ex, "Validation failed: car is null");
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation failed while updating car with ID: {CarId}", car?.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating car with ID: {CarId}", car?.Id);
            throw;
        }
    }

    public async Task<bool> DeleteCarAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting car with ID: {CarId}", id);
            
            var result = await _carRepository.DeleteAsync(id);
            
            if (result)
            {
                _logger.LogInformation("Car with ID: {CarId} deleted successfully", id);
            }
            else
            {
                _logger.LogWarning("Car with ID: {CarId} not found for deletion", id);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting car with ID: {CarId}", id);
            throw;
        }
    }

    /// Validates car entity according to business rules:
    /// - LicensePlate must not be empty
    /// - DailyRate must be > 0
    /// - ManufacturingYear must be <= current year
    /// - Model must have at least 4 characters
    private void ValidateCar(Car car)
    {
        if (car == null)
        {
            throw new ArgumentNullException(nameof(car), "Car cannot be null.");
        }

        // Validation: LicensePlate must not be empty
        if (string.IsNullOrWhiteSpace(car.LicensePlate))
        {
            throw new ArgumentException("License plate is required.", nameof(car));
        }

        // Validation: DailyRate > 0
        if (car.DailyRate <= 0)
        {
            throw new ArgumentException("Daily rate must be greater than 0.", nameof(car));
        }

        // Validation: ManufacturingYear <= CurrentYear
        int currentYear = DateTime.Now.Year;
        if (car.ManufacturingYear > currentYear)
        {
            throw new ArgumentException($"Manufacturing year must be less than or equal to {currentYear}.", nameof(car));
        }

        // Validation: Model length >= 4 characters
        if (string.IsNullOrWhiteSpace(car.Model) || car.Model.Length < 4)
        {
            throw new ArgumentException("Model must have at least 4 characters.", nameof(car));
        }
    }
}

