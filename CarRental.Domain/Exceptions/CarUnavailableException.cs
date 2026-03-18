namespace CarRental.Domain.Exceptions;

public class CarUnavailableException : Exception
{
    public int CarId { get; }

    public CarUnavailableException(int carId) 
        : base($"Car with ID {carId} is not available for reservation.")
    {
        CarId = carId;
    }

    public CarUnavailableException(int carId, string message) 
        : base(message)
    {
        CarId = carId;
    }

    public CarUnavailableException(int carId, string message, Exception innerException) 
        : base(message, innerException)
    {
        CarId = carId;
    }
}

