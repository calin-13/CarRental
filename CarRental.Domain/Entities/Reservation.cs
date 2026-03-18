namespace CarRental.Domain.Entities;

public class Reservation
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public Car? Car { get; set; }
    public int ClientId { get; set; }
    public Client? Client { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalCost { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? ActualReturnDate { get; set; }
}
