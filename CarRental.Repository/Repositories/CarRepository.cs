using CarRental.Domain.Entities;
using CarRental.Repository.Data;
using CarRental.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Repository.Repositories;

public class CarRepository : ICarRepository
{
    private readonly CarRentalDbContext _context;

    public CarRepository(CarRentalDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Car>> GetAllAsync()
    {
        return await _context.Cars.ToListAsync();
    }

    public async Task<Car?> GetByIdAsync(int id)
    {
        return await _context.Cars.FindAsync(id);
    }

    public async Task<Car> AddAsync(Car car)
    {
        await _context.Cars.AddAsync(car);
        await _context.SaveChangesAsync();
        return car;
    }

    public async Task<Car> UpdateAsync(Car car)
    {
        _context.Cars.Update(car);
        await _context.SaveChangesAsync();
        return car;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var car = await _context.Cars.FindAsync(id);
        if (car == null)
        {
            return false;
        }

        _context.Cars.Remove(car);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Cars.AnyAsync(c => c.Id == id);
    }
}

