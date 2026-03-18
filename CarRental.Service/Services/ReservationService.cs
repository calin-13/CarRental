using CarRental.Domain.Entities;
using CarRental.Domain.Exceptions;
using CarRental.Repository.Interfaces;
using CarRental.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarRental.Service.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ICarRepository _carRepository;
    private readonly IClientRepository _clientRepository;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(
        IReservationRepository reservationRepository,
        ICarRepository carRepository,
        IClientRepository clientRepository,
        ILogger<ReservationService> logger)
    {
        _reservationRepository = reservationRepository ?? throw new ArgumentNullException(nameof(reservationRepository));
        _carRepository = carRepository ?? throw new ArgumentNullException(nameof(carRepository));
        _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Reservation>> GetAllReservationsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all reservations");
            return await _reservationRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all reservations");
            throw;
        }
    }

    public async Task<Reservation?> GetReservationByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Retrieving reservation with ID: {ReservationId}", id);
            return await _reservationRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving reservation with ID: {ReservationId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Reservation>> GetReservationsByCarIdAsync(int carId)
    {
        try
        {
            _logger.LogInformation("Retrieving reservations for car with ID: {CarId}", carId);
            return await _reservationRepository.GetByCarIdAsync(carId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving reservations for car with ID: {CarId}", carId);
            throw;
        }
    }

    public async Task<IEnumerable<Reservation>> GetReservationsByClientIdAsync(int clientId)
    {
        try
        {
            _logger.LogInformation("Retrieving reservations for client with ID: {ClientId}", clientId);
            return await _reservationRepository.GetByClientIdAsync(clientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving reservations for client with ID: {ClientId}", clientId);
            throw;
        }
    }

    public async Task<Reservation> CreateReservationAsync(Reservation reservation)
    {
        if (reservation == null)
        {
            throw new ArgumentNullException(nameof(reservation));
        }

        try
        {
            ValidateReservationDates(reservation.StartDate, reservation.EndDate);
            
            var car = await _carRepository.GetByIdAsync(reservation.CarId);
            if (car == null)
            {
                _logger.LogWarning("Attempted to create reservation for non-existent car with ID: {CarId}", reservation.CarId);
                throw new ArgumentException($"Car with ID {reservation.CarId} does not exist.");
            }

            if (!car.IsAvailable)
            {
                _logger.LogWarning("Attempted to create reservation for unavailable car with ID: {CarId}", reservation.CarId);
                throw new CarUnavailableException(reservation.CarId, $"Car with ID {reservation.CarId} is not available for reservation.");
            }

            var client = await Task.FromResult(_clientRepository.GetById(reservation.ClientId));
            if (client == null)
            {
                _logger.LogWarning("Attempted to create reservation for non-existent client with ID: {ClientId}", reservation.ClientId);
                throw new ArgumentException($"Client with ID {reservation.ClientId} does not exist.");
            }

            bool hasOverlapping = await _reservationRepository.HasOverlappingReservationsAsync(
                reservation.CarId, 
                reservation.StartDate, 
                reservation.EndDate);

            if (hasOverlapping)
            {
                _logger.LogWarning("Attempted to create overlapping reservation for car with ID: {CarId}", reservation.CarId);
                throw new CarUnavailableException(reservation.CarId, $"Car with ID {reservation.CarId} is already reserved for the selected dates.");
            }

            reservation.TotalCost = await CalculateReservationCostAsync(reservation.CarId, reservation.StartDate, reservation.EndDate);
            reservation.IsActive = true;

            _logger.LogInformation("Creating reservation for car ID: {CarId}, client ID: {ClientId}, from {StartDate} to {EndDate}", 
                reservation.CarId, reservation.ClientId, reservation.StartDate, reservation.EndDate);

            var createdReservation = await _reservationRepository.AddAsync(reservation);
            
            _logger.LogInformation("Reservation created successfully with ID: {ReservationId}", createdReservation.Id);
            
            return createdReservation;
        }
        catch (CarUnavailableException)
        {
            throw;
        }
        catch (InvalidDateException)
        {
            throw;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating reservation");
            throw;
        }
    }

    public async Task<Reservation> UpdateReservationAsync(Reservation reservation)
    {
        if (reservation == null)
        {
            throw new ArgumentNullException(nameof(reservation));
        }

        try
        {
            var existingReservation = await _reservationRepository.GetByIdAsync(reservation.Id);
            if (existingReservation == null)
            {
                _logger.LogWarning("Attempted to update non-existent reservation with ID: {ReservationId}", reservation.Id);
                throw new ArgumentException($"Reservation with ID {reservation.Id} does not exist.");
            }

            ValidateReservationDates(reservation.StartDate, reservation.EndDate);

            var car = await _carRepository.GetByIdAsync(reservation.CarId);
            if (car == null)
            {
                _logger.LogWarning("Attempted to update reservation with non-existent car ID: {CarId}", reservation.CarId);
                throw new ArgumentException($"Car with ID {reservation.CarId} does not exist.");
            }

            if (!car.IsAvailable && reservation.CarId != existingReservation.CarId)
            {
                _logger.LogWarning("Attempted to update reservation to unavailable car with ID: {CarId}", reservation.CarId);
                throw new CarUnavailableException(reservation.CarId, $"Car with ID {reservation.CarId} is not available for reservation.");
            }

            bool hasOverlapping = await _reservationRepository.HasOverlappingReservationsAsync(
                reservation.CarId, 
                reservation.StartDate, 
                reservation.EndDate,
                reservation.Id);

            if (hasOverlapping)
            {
                _logger.LogWarning("Attempted to update reservation with overlapping dates for car ID: {CarId}", reservation.CarId);
                throw new CarUnavailableException(reservation.CarId, $"Car with ID {reservation.CarId} is already reserved for the selected dates.");
            }

            reservation.TotalCost = await CalculateReservationCostAsync(reservation.CarId, reservation.StartDate, reservation.EndDate);

            _logger.LogInformation("Updating reservation with ID: {ReservationId}", reservation.Id);

            var updatedReservation = await _reservationRepository.UpdateAsync(reservation);
            
            _logger.LogInformation("Reservation with ID: {ReservationId} updated successfully", updatedReservation.Id);
            
            return updatedReservation;
        }
        catch (CarUnavailableException)
        {
            throw;
        }
        catch (InvalidDateException)
        {
            throw;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating reservation with ID: {ReservationId}", reservation.Id);
            throw;
        }
    }

    public async Task<bool> CancelReservationAsync(int id)
    {
        try
        {
            var reservation = await _reservationRepository.GetByIdAsync(id);
            if (reservation == null)
            {
                _logger.LogWarning("Attempted to cancel non-existent reservation with ID: {ReservationId}", id);
                return false;
            }

            if (!reservation.IsActive)
            {
                _logger.LogWarning("Attempted to cancel already inactive reservation with ID: {ReservationId}", id);
                return false;
            }

            reservation.IsActive = false;
            reservation.ActualReturnDate = DateTime.Now;

            _logger.LogInformation("Cancelling reservation with ID: {ReservationId}", id);

            await _reservationRepository.UpdateAsync(reservation);
            
            _logger.LogInformation("Reservation with ID: {ReservationId} cancelled successfully", id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cancelling reservation with ID: {ReservationId}", id);
            throw;
        }
    }

    public async Task<decimal> CalculateReservationCostAsync(int carId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var car = await _carRepository.GetByIdAsync(carId);
            if (car == null)
            {
                throw new ArgumentException($"Car with ID {carId} does not exist.");
            }

            if (startDate >= endDate)
            {
                throw new InvalidDateException("Start date must be before end date.");
            }

            int days = (endDate - startDate).Days;
            if (days <= 0)
            {
                days = 1;
            }

            decimal totalCost = car.DailyRate * days;

            _logger.LogInformation("Calculated cost for car ID: {CarId}, {Days} days, total: {TotalCost}", 
                carId, days, totalCost);

            return totalCost;
        }
        catch (InvalidDateException)
        {
            throw;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calculating reservation cost for car ID: {CarId}", carId);
            throw;
        }
    }

    public async Task<bool> IsCarAvailableForReservationAsync(int carId, DateTime startDate, DateTime endDate, int? excludeReservationId = null)
    {
        try
        {
            var car = await _carRepository.GetByIdAsync(carId);
            if (car == null)
            {
                return false;
            }

            if (!car.IsAvailable)
            {
                return false;
            }

            bool hasOverlapping = await _reservationRepository.HasOverlappingReservationsAsync(
                carId, 
                startDate, 
                endDate, 
                excludeReservationId);

            return !hasOverlapping;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking car availability for car ID: {CarId}", carId);
            throw;
        }
    }

    private void ValidateReservationDates(DateTime startDate, DateTime endDate)
    {
        if (startDate < DateTime.Today)
        {
            throw new InvalidDateException(
                "Start date cannot be in the past.", 
                startDate, 
                endDate);
        }

        if (endDate < startDate)
        {
            throw new InvalidDateException(
                "End date must be after start date.", 
                startDate, 
                endDate);
        }

        if (startDate == endDate)
        {
            throw new InvalidDateException(
                "Start date and end date cannot be the same.", 
                startDate, 
                endDate);
        }

        TimeSpan maxReservationPeriod = TimeSpan.FromDays(365);
        if (endDate - startDate > maxReservationPeriod)
        {
            throw new InvalidDateException(
                $"Reservation period cannot exceed {maxReservationPeriod.Days} days.", 
                startDate, 
                endDate);
        }
    }
}

