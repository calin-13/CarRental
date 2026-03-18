using CarRental.Domain.DTOs;

namespace CarRental.Repository.Interfaces;

public interface IReportRepository
{
    Task<List<RevenueByCategory>> GetRevenueByCarCategoryAsync();
    Task<List<TopClient>> GetTopClientsByReservationsAsync(int topCount = 10);
}
