namespace CarRental.Domain.Entities;

public class TariffCalculation
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public decimal BaseRate { get; set; }
    public decimal LateFee { get; set; }
    public decimal RoadTax { get; set; }
    public decimal CurrencyConversionRate { get; set; }
    public string Currency { get; set; } = "RON";
    public decimal TotalAmount { get; set; }
    public DateTime CalculatedAt { get; set; }
}
