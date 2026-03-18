using CarRental.Domain.Entities;

namespace CarRental.Service.Interfaces;

public interface ICarService
{
    /// Gets all cars
    Task<IEnumerable<Car>> GetAllCarsAsync();
    
    /// Gets a car by ID
    Task<Car?> GetCarByIdAsync(int id);
    
    /// Creates a new car with validation
    Task<Car> CreateCarAsync(Car car);
    
    /// Updates an existing car with validation
    Task<Car> UpdateCarAsync(Car car);
    
    /// Deletes a car by ID
    Task<bool> DeleteCarAsync(int id);
}

