using CarRental.Domain.DTOs;
using CarRental.Repository.Data;
using CarRental.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Repository.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly CarRentalDbContext _context;

    public ReportRepository(CarRentalDbContext context)
    {
        _context = context;
    }

    public async Task<List<RevenueByCategory>> GetRevenueByCarCategoryAsync()
    {
        var result = new List<RevenueByCategory>();

        using (var command = _context.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = "GetRevenueByCarCategory";
            command.CommandType = System.Data.CommandType.StoredProcedure;

            await _context.Database.OpenConnectionAsync();

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    result.Add(new RevenueByCategory
                    {
                        Category = reader.GetString(0),
                        TotalReservations = reader.GetInt32(1),
                        TotalRevenue = reader.GetDecimal(2),
                        AverageRevenue = reader.GetDecimal(3)
                    });
                }
            }
        }

        return result;
    }

    public async Task<List<TopClient>> GetTopClientsByReservationsAsync(int topCount = 10)
    {
        var result = new List<TopClient>();

        using (var command = _context.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = "GetTopClientsByReservations";
            command.CommandType = System.Data.CommandType.StoredProcedure;

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@TopCount";
            parameter.Value = topCount;
            command.Parameters.Add(parameter);

            await _context.Database.OpenConnectionAsync();

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    result.Add(new TopClient
                    {
                        Id = reader.GetInt32(0),
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        Email = reader.GetString(3),
                        TotalReservations = reader.GetInt32(4),
                        TotalSpent = reader.GetDecimal(5),
                        LastReservationDate = reader.GetDateTime(6)
                    });
                }
            }
        }

        return result;
    }
}
