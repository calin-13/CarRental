using CarRental.Domain.Entities;

namespace CarRental.Service.Interfaces;

public interface IReservationService
{
    Task<IEnumerable<Reservation>> GetAllReservationsAsync();
    
    Task<Reservation?> GetReservationByIdAsync(int id);
    
    Task<IEnumerable<Reservation>> GetReservationsByCarIdAsync(int carId);
    
    Task<IEnumerable<Reservation>> GetReservationsByClientIdAsync(int clientId);
    
    Task<Reservation> CreateReservationAsync(Reservation reservation);
    
    Task<Reservation> UpdateReservationAsync(Reservation reservation);
    
    Task<bool> CancelReservationAsync(int id);
    
    Task<decimal> CalculateReservationCostAsync(int carId, DateTime startDate, DateTime endDate);
    
    Task<bool> IsCarAvailableForReservationAsync(int carId, DateTime startDate, DateTime endDate, int? excludeReservationId = null);
}

