using CarRental.Domain.Entities;

namespace CarRental.Repository.Interfaces;

public interface IReservationRepository
{
    Task<IEnumerable<Reservation>> GetAllAsync();
    
    Task<Reservation?> GetByIdAsync(int id);
    
    Task<IEnumerable<Reservation>> GetByCarIdAsync(int carId);
    
    Task<IEnumerable<Reservation>> GetActiveReservationsByCarIdAsync(int carId);
    
    Task<IEnumerable<Reservation>> GetByClientIdAsync(int clientId);
    
    Task<Reservation> AddAsync(Reservation reservation);
    
    Task<Reservation> UpdateAsync(Reservation reservation);
    
    Task<bool> DeleteAsync(int id);
    
    Task<bool> ExistsAsync(int id);
    
    Task<bool> HasOverlappingReservationsAsync(int carId, DateTime startDate, DateTime endDate, int? excludeReservationId = null);
}

