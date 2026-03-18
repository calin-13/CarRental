namespace CarRental.Domain.DTOs;

public class TopClient
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalReservations { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime LastReservationDate { get; set; }
}
