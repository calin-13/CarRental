using CarRental.Domain.Entities;
using CarRental.Repository.Data;
using CarRental.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Repository.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly CarRentalDbContext _context;

    public ReservationRepository(CarRentalDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Reservation>> GetAllAsync()
    {
        return await _context.Reservations
            .Include(r => r.Car)
            .Include(r => r.Client)
            .ToListAsync();
    }

    public async Task<Reservation?> GetByIdAsync(int id)
    {
        return await _context.Reservations
            .Include(r => r.Car)
            .Include(r => r.Client)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Reservation>> GetByCarIdAsync(int carId)
    {
        return await _context.Reservations
            .Include(r => r.Car)
            .Include(r => r.Client)
            .Where(r => r.CarId == carId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetActiveReservationsByCarIdAsync(int carId)
    {
        return await _context.Reservations
            .Include(r => r.Car)
            .Include(r => r.Client)
            .Where(r => r.CarId == carId && r.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetByClientIdAsync(int clientId)
    {
        return await _context.Reservations
            .Include(r => r.Car)
            .Include(r => r.Client)
            .Where(r => r.ClientId == clientId)
            .ToListAsync();
    }

    public async Task<Reservation> AddAsync(Reservation reservation)
    {
        if (reservation == null)
        {
            throw new ArgumentNullException(nameof(reservation));
        }

        await _context.Reservations.AddAsync(reservation);
        await _context.SaveChangesAsync();
        return reservation;
    }

    public async Task<Reservation> UpdateAsync(Reservation reservation)
    {
        if (reservation == null)
        {
            throw new ArgumentNullException(nameof(reservation));
        }

        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync();
        return reservation;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
        {
            return false;
        }

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Reservations.AnyAsync(r => r.Id == id);
    }

    public async Task<bool> HasOverlappingReservationsAsync(int carId, DateTime startDate, DateTime endDate, int? excludeReservationId = null)
    {
        var query = _context.Reservations
            .Where(r => r.CarId == carId 
                && r.IsActive 
                && ((r.StartDate <= startDate && r.EndDate >= startDate) 
                    || (r.StartDate <= endDate && r.EndDate >= endDate)
                    || (r.StartDate >= startDate && r.EndDate <= endDate)
                    || (r.StartDate <= startDate && r.EndDate >= endDate)));

        if (excludeReservationId.HasValue)
        {
            query = query.Where(r => r.Id != excludeReservationId.Value);
        }

        return await query.AnyAsync();
    }
}

