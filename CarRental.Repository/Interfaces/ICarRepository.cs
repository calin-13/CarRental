using CarRental.Domain.Entities;

namespace CarRental.Repository.Interfaces;

public interface ICarRepository
{
    /// Gets all cars
    Task<IEnumerable<Car>> GetAllAsync();
    
    /// Gets a car by ID
    Task<Car?> GetByIdAsync(int id);
    
    /// Adds a new car
    Task<Car> AddAsync(Car car);
    
    /// Updates an existing car
    Task<Car> UpdateAsync(Car car);
    
    /// Deletes a car by ID
    Task<bool> DeleteAsync(int id);
    
    /// Checks if a car exists by ID
    Task<bool> ExistsAsync(int id);
}

