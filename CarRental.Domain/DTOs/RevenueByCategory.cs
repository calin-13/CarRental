namespace CarRental.Domain.DTOs;

public class RevenueByCategory
{
    public string Category { get; set; } = string.Empty;
    public int TotalReservations { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageRevenue { get; set; }
}
