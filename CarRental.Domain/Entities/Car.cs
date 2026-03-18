namespace CarRental.Domain.Entities;

public class Car
{
    public int Id { get; set; }
    
    /// The license plate number (unique identifier for the car)
    public string LicensePlate { get; set; } = string.Empty;
    
    /// The car model (minimum 4 characters)
    public string Model { get; set; } = string.Empty;
    
    /// The manufacturing year (must be <= current year)
    public int ManufacturingYear { get; set; }
    
    /// The daily rental rate (must be > 0)
    public decimal DailyRate { get; set; }
    
    /// The availability status of the car
    public bool IsAvailable { get; set; } = true;
}

